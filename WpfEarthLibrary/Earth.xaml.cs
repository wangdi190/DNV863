using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Xna.Framework;

namespace WpfEarthLibrary
{
    //******************************************************************************************
    // 注：
    // 1. 模型增删性质的操作，应用objmanager中的updatemodel来刷新，属性变化自行更新，但isshowlabel除外，因为这个属性涉及的是文字模型的增删，所以应用updatemodel来处理
    //
    //
    //******************************************************************************************



    /// <summary>
    /// Earth.xaml 的交互逻辑
    /// </summary>
    public partial class Earth : UserControl
    {
        //public static System.Windows.Shapes.Rectangle recdebug;
        ///<summary>对象管理器</summary>
        public pObjectManager objManager;
        ///<summary>地图管理器</summary>
        public EarthManager earthManager;
        ///<summary>光源管理器</summary>
        public LightManager lightManager;
        ///<summary>色彩管理器</summary>
        public ColorManager colorManager;
        ///<summary>坐标系管理器</summary>
        public CoordinateManager coordinateManager;
        ///<summary>图例管理器</summary>
        public LegendManager legendManager;
        ///<summary>配置管理</summary>
        public Config config;
        ///<summary>相机</summary>
        public Camera camera;
        ///<summary>全局变量</summary>
        internal Global global = new Global();

        internal int earthkey;// 多实例处理用的key

        public Earth()
        {
            config = new Config(this);
            InitializeComponent();
            objManager = new pObjectManager(this);
            earthManager = new EarthManager(this);
            colorManager = new ColorManager();
            coordinateManager = new CoordinateManager(this);
            legendManager = new LegendManager(this);
            lightManager = new LightManager(this);

            earthkey = this.GetHashCode();

            //recdebug = debug;

            // Set up the initial state for the D3DImage.
            HRESULT.Check(D3DManager.SetSize(earthkey, 800, 600));
            HRESULT.Check(D3DManager.SetAlpha(earthkey, true));
            HRESULT.Check(D3DManager.SetNumDesiredSamples(earthkey, 4));

            // 
            // Optional: Subscribing to the IsFrontBufferAvailableChanged event.
            //
            // If you don't render every frame (e.g. you only render in 
            // reaction to a button click), you should subscribe to the
            // IsFrontBufferAvailableChanged event to be notified when rendered content 
            // is no longer being displayed. This event also notifies you when 
            // the D3DImage is capable of being displayed again. 

            // For example, in the button click case, if you don't render again when 
            // the IsFrontBufferAvailable property is set to true, your 
            // D3DImage won't display anything until the next button click.
            //
            // Because this application renders every frame, there is no need to
            // handle the IsFrontBufferAvailableChanged event.
            // 
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);

            //
            // Optional: Multi-adapter optimization
            //
            // The surface is created initially on a particular adapter.
            // If the WPF window is dragged to another adapter, WPF
            // ensures that the D3DImage still shows up on the new
            // adapter. 
            //
            // This process is slow on Windows XP.
            //
            // Performance is better on Vista with a 9Ex device. It's only 
            // slow when the D3DImage crosses a video-card boundary.
            //
            // To work around this issue, you can move your surface when
            // the D3DImage is displayed on another adapter. To
            // determine when that is the case, transform a point on the
            // D3DImage into screen space and find out which adapter
            // contains that screen space point.
            //
            // When your D3DImage straddles two adapters, nothing  
            // can be done, because one will be updating slowly.
            //
            _adapterTimer = new DispatcherTimer();
            _adapterTimer.Tick += new EventHandler(AdapterTimer_Tick);
            _adapterTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _adapterTimer.Start();

            //
            // Optional: Surface resizing
            //
            // The D3DImage is scaled when WPF renders it at a size 
            // different from the natural size of the surface. If the
            // D3DImage is scaled up significantly, image quality 
            // degrades. 
            // 
            // To avoid this, you can either create a very large
            // texture initially, or you can create new surfaces as
            // the size changes. Below is a very simple example of
            // how to do the latter.
            //
            // By creating a timer at Render priority, you are guaranteed
            // that new surfaces are created while the element
            // is still being arranged. A 200 ms interval gives
            // a good balance between image quality and performance.
            // You must be careful not to create new surfaces too 
            // frequently. Frequently allocating a new surface may 
            // fragment or exhaust video memory. This issue is more 
            // significant on XDDM than it is on WDDM, because WDDM 
            // can page out video memory.
            //
            // Another approach is deriving from the Image class, 
            // participating in layout by overriding the ArrangeOverride method, and
            // updating size in the overriden method. Performance will degrade
            // if you resize too frequently.
            //
            // Blurry D3DImages can still occur due to subpixel 
            // alignments. 
            //


            ////使用深度测试，下方应注释，以固定backbuffer大小
            _sizeTimer = new DispatcherTimer(DispatcherPriority.Render);
            _sizeTimer.Tick += new EventHandler(SizeTimer_Tick);
            _sizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            _sizeTimer.Start();


            Application.Current.MainWindow.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
            tmrFreeView.Tick += new EventHandler(tmrFreeView_Tick);
        }

        ~Earth()
        {
            if (Application.Current != null)
            {
                try
                {
                    Application.Current.MainWindow.KeyDown -= new KeyEventHandler(MainWindow_KeyDown);
                }
                catch (Exception)
                {


                }

            }
            D3DManager.Destroy(earthkey);
        }

        #region ===== 自由视角处理, 以及其它热键 =====
        System.Windows.Threading.DispatcherTimer tmrFreeView = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(20) };
        //bool isKeyPressed;  
        ///<summary>键盘事件，用于：操作游戏方式变换视角</summary>
        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (!tmrFreeView.IsEnabled && camera.operateMode == EOperateMode.自由视角 && (e.Key == Key.W || e.Key == Key.S || e.Key == Key.A || e.Key == Key.D))
            {
                tmrFreeView.Start();
            }
            else if (e.Key == Key.F12)  //编辑状态
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    config.isEditMode = !config.isEditMode;
                    MyBaseControls.StatusBarTool.StatusBarTool.reportInfo.showInfo(config.isEditMode ? "编辑状态" : "浏览状态", 0);
                }
            }


        }
        void tmrFreeView_Tick(object sender, EventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.W))
            {
                camera.cameraPosition = camera.cameraPosition + camera.cameraDirection * camera.moveDistance;
                camera.updateD3DCamera();
            }
            else if (Keyboard.IsKeyDown(Key.S))
            {
                camera.cameraPosition = camera.cameraPosition - camera.cameraDirection * camera.moveDistance;
                camera.updateD3DCamera();
            }
            else if (Keyboard.IsKeyDown(Key.A))
            {
                Vector3 axis = Vector3.Cross(camera.cameraDirection, camera.cameraUp);
                axis.Normalize();
                camera.cameraPosition = camera.cameraPosition - axis * camera.moveDistance / 2;
                camera.updateD3DCamera();
            }
            else if (Keyboard.IsKeyDown(Key.D))
            {
                Vector3 axis = Vector3.Cross(camera.cameraDirection, camera.cameraUp);
                axis.Normalize();
                camera.cameraPosition = camera.cameraPosition + axis * camera.moveDistance / 2;
                camera.updateD3DCamera();
            }
            else
                tmrFreeView.Stop();
        }
        #endregion

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            tmrToolbox.Interval = TimeSpan.FromMilliseconds(1000);

            initMinimap();
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ////启用深度测试，初始设置大小
            //uint actualWidth = (uint)grdMain.ActualWidth;//(uint)imgelt.ActualWidth;
            //uint actualHeight = (uint)grdMain.ActualHeight;//(uint)imgelt.ActualHeight;
            //if ((actualWidth > 0 && actualHeight > 0) &&
            //    (actualWidth != (uint)d3dimg.Width || actualHeight != (uint)d3dimg.Height))
            //{
            //    HRESULT.Check(D3DManager.SetSize(earthkey, actualWidth, actualHeight));
            //}



            tmrToolbox.Tick += new EventHandler(tmrToolbox_Tick);
            tmrToolbox.Start();

            tooltiptimer.Tick += new EventHandler(tooltiptimer_Tick);
            tooltiptimer.Interval = TimeSpan.FromMilliseconds(config.tooltipMoveDelay);
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            tmrToolbox.Tick -= new EventHandler(tmrToolbox_Tick);
            tooltiptimer.Tick -= new EventHandler(tooltiptimer_Tick);
        }

        #region ===================  tooltips 相关 ==============================
        //------------------------------------------------------------------------------------------
        // 说明：mouse移动不停的进行拾取测试将严重影响性能，所以只在mouse停下一段时间后才进行一次拾取测试，若单击双击，可直接调用拾取方法
        //------------------------------------------------------------------------------------------
        internal System.Windows.Threading.DispatcherTimer tooltiptimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Point mouseposition;
        void tooltiptimer_Tick(object sender, EventArgs e)
        {
            //测试拾取
            WpfEarthLibrary.PowerBasicObject obj = objManager.pick(mouseposition);  // 拾取方法，返回拾取到的对象
            if (obj != null && (obj is pData || obj is pPowerLine || obj is pSymbolObject || obj is pArea))
            {
                if (obj.tooltipMoveContent != null)
                {
                    if (!string.IsNullOrWhiteSpace(obj.tooltipMoveTemplate))
                        tooltipcontent.ContentTemplate = (DataTemplate)this.FindResource(obj.tooltipMoveTemplate);
                    else
                        tooltipcontent.ContentTemplate = (DataTemplate)Resources["TooltipNormalTemplate"];
                    tooltipcontent.Content = obj.tooltipMoveContent;
                    double ToolTipOffset = 0;
                    Tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
                    Tooltip.PlacementTarget = grdMain;
                    Tooltip.HorizontalOffset = mouseposition.X + ToolTipOffset + 1;
                    Tooltip.VerticalOffset = mouseposition.Y + ToolTipOffset;
                    Tooltip.IsOpen = true;
                }
            }
            else
                Tooltip.IsOpen = false;
            tooltiptimer.Stop();
        }



        #endregion


        #region ========== 公开属性、方法、事件 ==========
        public Grid gridAddition { get { return grdAddition; } }


        internal void setDoubleClick(bool isEnable)
        {
            if (isEnable)
                this.MouseDoubleClick += new MouseButtonEventHandler(MainWindow_MouseDoubleClick);
            else
                this.MouseDoubleClick -= new MouseButtonEventHandler(MainWindow_MouseDoubleClick);
        }
        ///<summary>借用窗体实现双击</summary>
        void MainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (config.doubleClickPickEnable)
            {
                PowerBasicObject obj = objManager.pick(prevMouseState);  // 拾取方法，返回拾取到的对象
                RaiseDoubleClickPickedEvent(new PickEventArgs(obj));
            }
        }

        internal void setOperateMode()
        {
            prevMouseState = Mouse.GetPosition(grdMain);
            if (camera.operateMode == EOperateMode.自由视角)
            {
                isFreeEnable = false;
                SetCursorPos((int)global.ScreenWidth / 2, (int)global.ScreenHeight / 2);
            }
        }



        ///<summary>可视范围变化事件，global的isDynShow动态显示为真时有效</summary>
        public event EventHandler VisualRangeChanged;
        protected virtual void RaiseVisualRangeChangedEvent()
        {
            if (VisualRangeChanged != null)
                VisualRangeChanged(this, null);
        }

        ///<summary>拾取事件，pickEnable为真时有效，左键点击拾取到对象时发生</summary>
        public event EventHandler<PickEventArgs> Picked;
        protected virtual void RaisePickedEvent(PickEventArgs pickEventArgs)
        {
            if (Picked != null)
                Picked(this, pickEventArgs);
        }
        ///<summary>双击拾取事件，doubleClickPickEnable为真时有效，左键双击时发生</summary>
        public event EventHandler<PickEventArgs> DoubleClickPicked;
        protected virtual void RaiseDoubleClickPickedEvent(PickEventArgs pickEventArgs)
        {
            if (DoubleClickPicked != null)
                DoubleClickPicked(this, pickEventArgs);
        }
        public class PickEventArgs : EventArgs
        {
            public PickEventArgs(PowerBasicObject PickedObject)
            {
                _pickedObject = PickedObject;
            }
            private PowerBasicObject _pickedObject;
            public PowerBasicObject pickedObject
            {
                get { return _pickedObject; }
            }
        }






        ///<summary>更新模型</summary>
        public void UpdateModel()
        {
            if (IsVisible)
                objManager.updateModel();
        }


        ///<summary>使用色彩管理器情况下，刷新色彩</summary>
        public void refreshColor()
        {
            if (colorManager.isEnabled)
                foreach (pLayer layer in objManager.zLayers.Values)
                {
                    foreach (PowerBasicObject pobj in layer.pModels.Values)
                    {
                        if (pobj is pPowerLine)
                        {
                            pPowerLine obj = pobj as pPowerLine;
                            if (obj.isColorManaged)
                                obj.color = colorManager.getColor(earthManager.mapType, ColorManager.ECObjectType.折线, obj.objTypeString, obj.objStatusString);
                        }
                        else if (pobj is pSymbolObject)
                        {
                            pSymbolObject obj = pobj as pSymbolObject;
                            if (obj.isColorManaged)
                                obj.color = colorManager.getColor(earthManager.mapType, ColorManager.ECObjectType.图元, obj.objTypeString, obj.objStatusString);
                        }
                        else if (pobj is pArea)
                        {
                            pArea obj = pobj as pArea;
                            if (obj.isColorManaged)
                                obj.color = colorManager.getColor(earthManager.mapType, ColorManager.ECObjectType.区域, obj.objTypeString, obj.objStatusString);
                        }

                    }
                }
        }

        ///<summary>控件左侧的工具栏容器，可供用户附加工具按钮</summary>
        public StackPanel toolBox { get { return toolbox; } }

        ///<summary>返回点击测试对象, 注：只有允许接受拾取的对象会被测试</summary>
        public PowerBasicObject hitTest(System.Windows.Point hitPoint)
        {
            return objManager.pick(prevMouseState);
        }
        ///<summary>返回指定标志的点击测试对象, 注：只有允许接受拾取的对象会被测试</summary>
        public PowerBasicObject hitTest(System.Windows.Point hitPoint, string flag)
        {
            return objManager.pickByFlag(prevMouseState, flag);
        }


        #endregion


        #region ========== 控制调用d3d  ==========
        void AdapterTimer_Tick(object sender, EventArgs e)
        {
            if (!IsVisible) return;

            POINT p = new POINT(imgelt.PointToScreen(new System.Windows.Point(0, 0)));

            HRESULT.Check(D3DManager.SetAdapter(earthkey, p));
        }

        void SizeTimer_Tick(object sender, EventArgs e)
        {
            // The following code does not account for RenderTransforms.
            // To handle that case, you must transform up to the root and 
            // check the size there.

            // Given that the D3DImage is at 96.0 DPI, its Width and Height 
            // properties will always be integers. ActualWidth/Height 
            // may not be integers, so they are cast to integers. 

            uint actualWidth = (uint)grdMain.ActualWidth;//(uint)imgelt.ActualWidth;
            uint actualHeight = (uint)grdMain.ActualHeight;//(uint)imgelt.ActualHeight;
            if ((actualWidth > 0 && actualHeight > 0) &&
                (actualWidth != (uint)d3dimg.Width || actualHeight != (uint)d3dimg.Height))
            {
                HRESULT.Check(D3DManager.SetSize(earthkey, actualWidth, actualHeight));
            }
        }
        bool isinited = false;
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;

            // It's possible for Rendering to call back twice in the same frame 
            // so only render when we haven't already rendered in this frame.
            if (d3dimg.IsFrontBufferAvailable && _lastRender != args.RenderingTime)
            {



                IntPtr pSurface = IntPtr.Zero;
                HRESULT.Check(D3DManager.GetBackBufferNoRef(earthkey, out pSurface));
                if (pSurface != IntPtr.Zero)
                {
                    d3dimg.Lock();
                    // Repeatedly calling SetBackBuffer with the same IntPtr is 
                    // a no-op. There is no performance penalty.
                    d3dimg.SetBackBuffer(D3DResourceType.IDirect3DSurface9, pSurface);
                    if (!isinited) //是否已初始化相机
                    {
                        camera.updateD3DCamera();
                        earthManager.updateEarthPara(); //初始设置地理显示参数
                        objManager.updateSymbolTexture();//初始加载公用符号材质
                        objManager.updateGeomeries();//初始加载公用几何体
                        objManager.updateXModel();//初始加载xmodel

                        objManager.updateModel(); //初始加载模型
                        //lightManager = new LightManager(this); //初始化光源，因可能用到相机位置来确定光源位置，所以放在此处初始化
                        lightManager.applyLights();

                        isinited = true;
                        global.isUpdate = true;
                    }
                    if (earthManager.mapType != EMapType.无 && camera.operateMode== EOperateMode.地图模式)
                        earthManager.updateEarth();
                    //global.isUpdate = false;


                    HRESULT.Check(D3DManager.Render(earthkey));


                    d3dimg.AddDirtyRect(new Int32Rect(0, 0, d3dimg.PixelWidth, d3dimg.PixelHeight));
                    d3dimg.Unlock();

                    _lastRender = args.RenderingTime;
                }
            }
        }

        DispatcherTimer _sizeTimer;
        DispatcherTimer _adapterTimer;
        TimeSpan _lastRender;

        private void grdMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            global.ScreenWidth = (int)grdMain.ActualWidth;
            global.ScreenHeight = (int)grdMain.ActualHeight;
        }

        #endregion

        #region ========== 编辑模式相关 ==========
        ///<summary>清除编辑目标</summary>
        public void clearEditObject()
        {
            editObject = null;
        }

        PowerBasicObject editObject;
        PowerBasicObject pickObject;

        ///<summary>编辑模式选择对象事件，左键点击拾取到对象时发生</summary>
        public event EventHandler<PickEventArgs> EditObjectSelected;
        protected virtual void RaiseEditObjectSelectedEvent(PickEventArgs pickEventArgs)
        {
            if (EditObjectSelected != null)
                EditObjectSelected(this, pickEventArgs);
        }

        ///<summary>编辑模式选择对象事件，左键点击拾取到对象时发生</summary>
        public event EventHandler<PickEventArgs> EditObjectMove;
        protected virtual void RaiseEditObjectMoveEvent(PickEventArgs pickEventArgs)
        {
            if (EditObjectMove != null)
                EditObjectMove(this, pickEventArgs);
        }

        #endregion

        #region ========== 相机操作 ==========


        ///<summary>相机缩放</summary>
        private void grdMain_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool isupdate = false;
            float distanceground = (earthManager.earthpara.SceneMode == ESceneMode.地球) ? camera.cameraPosition.Length() - Para.Radius : camera.cameraPosition.Z;
            if (e.Delta > 0)
            {
                if (distanceground < camera.MaxGroundDistance || camera.operateMode!= EOperateMode.地图模式)
                {
                    camera.cameraPosition -= camera.cameraDirection * distanceground * 0.1f;
                    isupdate = true;
                }
            }
            else
                if (distanceground > camera.MinGroundDistance || camera.operateMode != EOperateMode.地图模式)
                {
                    camera.cameraPosition += camera.cameraDirection * distanceground * 0.1f;
                    isupdate = true;
                }
            if (isupdate)
            {
                camera.calCameraByDirection();
                camera.updateD3DCamera(true, 200);
                global.isUpdate = true;

                if (config.isDynShow && camera.operateMode == EOperateMode.地图模式)
                {
                    RaiseVisualRangeChangedEvent();
                    if (!config.isDynLoad)
                        UpdateModel();
                }
                if (objManager.dynLineWidthEnable && camera.operateMode == EOperateMode.地图模式) objManager.refreshDynLineWidth();
                if (config.isShowDebugInfo) showdebuginfo();

                if (minimap != null && minimap.Visibility == System.Windows.Visibility.Visible)
                    minimap.refreshPoly();

            }

        }





        private void grdMain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            prevMouseState = clickdownMouseState = e.GetPosition(sender as Grid);

            if (config.isEditMode) //编辑模式
            {
                pickObject = objManager.pick(prevMouseState);
                //if (editObject == null && !(pickObject is pData)) //pdata不接受编辑
                if (!(pickObject is pData)) //注：将修改编辑模式，新位置将直接存储，并增加被编辑属性和可编辑两个属性
                {
                    editObject = pickObject;
                    //if (editObject != null)
                    //    RaiseEditObjectSelectedEvent(new PickEventArgs(editObject)); //引发编辑对象被选取事件
                }

                if (pickObject != null && !(pickObject is pData))
                    RaiseEditObjectSelectedEvent(new PickEventArgs(pickObject));
            }

        }
        private void grdMain_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point curpoint = e.GetPosition(sender as Grid);
            if (config.isEditMode) //编辑模式
            {

            }
            else  //正常模式
            {
                if ((clickdownMouseState - curpoint).Length < 3)
                {
                    Tooltip.IsOpen = false;
                    //  tooltip click
                    if (config.tooltipClickEnable || config.pickEnable)
                    {
                        PowerBasicObject obj = objManager.pick(prevMouseState);  // 拾取方法，返回拾取到的对象

                        if (config.pickEnable)//拾取
                        {
                            RaisePickedEvent(new PickEventArgs(obj));
                        }
                        if (config.tooltipClickEnable) //左键tooltips
                        {
                            if (obj != null && (obj is pData || obj is pPowerLine || obj is pSymbolObject || obj is pArea))
                            {
                                if (obj.tooltipClickContent != null)
                                {
                                    mouseposition = e.GetPosition(grdMain);

                                    if (tooltiptimer.IsEnabled)
                                        tooltiptimer.Stop();

                                    if (!string.IsNullOrWhiteSpace(obj.tooltipClickTemplate))
                                        tooltipcontent.ContentTemplate = (DataTemplate)this.FindResource(obj.tooltipClickTemplate);
                                    else
                                        tooltipcontent.ContentTemplate = (DataTemplate)Resources["TooltipNormalTemplate"];
                                    tooltipcontent.Content = obj.tooltipClickContent;
                                    double ToolTipOffset = 0;
                                    Tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
                                    Tooltip.PlacementTarget = grdMain;
                                    Tooltip.HorizontalOffset = mouseposition.X + ToolTipOffset - 1;
                                    Tooltip.VerticalOffset = mouseposition.Y + ToolTipOffset - 1;
                                    Tooltip.IsOpen = true;
                                }
                            }
                            else
                                Tooltip.IsOpen = false;
                        }
                    }
                }


            }


            if (readyupdate)
            {
                global.isUpdate = true;
                if (config.isDynShow)
                {
                    if (config.isDynLoad)
                        RaiseVisualRangeChangedEvent();
                    else
                        UpdateModel();
                }
                if (config.isShowDebugInfo) showdebuginfo();
            }
            readyupdate = false;


        }


        private void grdMain_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            prevMouseState = clickdownMouseState = e.GetPosition(sender as Grid);


        }


        private void grdMain_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

            System.Windows.Point curpoint = e.GetPosition(sender as Grid);

            if (!config.isEditMode)
            {
                if ((clickdownMouseState - curpoint).Length < 3)
                {
                    //  tooltip click
                    if (config.tooltipRightClickEnable)
                    {
                        WpfEarthLibrary.PowerBasicObject obj = objManager.pick(prevMouseState);  // 拾取方法，返回拾取到的对象
                        if (obj != null && (obj is pData || obj is pPowerLine || obj is pSymbolObject || obj is pArea))
                        {
                            if (obj.tooltipRightClickContent != null)
                            {
                                mouseposition = e.GetPosition(grdMain);

                                if (tooltiptimer.IsEnabled)
                                    tooltiptimer.Stop();

                                if (!string.IsNullOrWhiteSpace(obj.tooltipRightClickTemplate))
                                    tooltipcontent.ContentTemplate = (DataTemplate)this.FindResource(obj.tooltipRightClickTemplate);
                                else
                                    tooltipcontent.ContentTemplate = (DataTemplate)Resources["TooltipNormalTemplate"];
                                tooltipcontent.Content = obj.tooltipRightClickContent;
                                double ToolTipOffset = 0;
                                Tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
                                Tooltip.PlacementTarget = grdMain;
                                Tooltip.HorizontalOffset = mouseposition.X + ToolTipOffset - 1;
                                Tooltip.VerticalOffset = mouseposition.Y + ToolTipOffset - 1;
                                Tooltip.IsOpen = true;
                            }
                        }
                        else
                            Tooltip.IsOpen = false;
                    }
                }
            }
        }



        internal bool readyupdate = false;
        System.Windows.Point prevMouseState, clickdownMouseState;

        [DllImport("user32.dll")]
        internal static extern int SetCursorPos(int x, int y);
        bool isFreeEnable;  //自由视角生效性切换用

        ///<summary>相机移动</summary>
        private void grdMain_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point curpoint = e.GetPosition(sender as Grid);

            if (config.isEditMode)  //编辑模式
            {
                if (e.LeftButton == MouseButtonState.Pressed && editObject != null && (editObject == pickObject || pickObject is EDDot))
                {
                    if (pickObject is pSymbolObject) //所有类型对象的位置编辑均转换为psymbolobject来操作
                    {
                        pSymbolObject tmpobj = pickObject as pSymbolObject;
                        Vector3 vec = new Vector3(tmpobj.VecLocation.x, tmpobj.VecLocation.y, tmpobj.VecLocation.z);

                        Vector3? orgpoint, newpoint;
                        if (Keyboard.IsKeyDown(Key.LeftShift))  //按下左shift，垂直面移动   zhh注：由于地球模式下未实现相应函数，有潜在bug
                        {
                            float distanceY = vec.Y;
                            orgpoint = Helpler.GetProjectPoint3D2(new Vector2((float)prevMouseState.X, (float)prevMouseState.Y), camera, global.ScreenWidth, global.ScreenHeight, distanceY, earthManager.earthpara);
                            newpoint = Helpler.GetProjectPoint3D2(new Vector2((float)curpoint.X, (float)curpoint.Y), camera, global.ScreenWidth, global.ScreenHeight, distanceY, earthManager.earthpara);
                        }
                        else  //水平面移动
                        {
                            float distanceZ = vec.Z;
                            orgpoint = Helpler.GetProjectPoint3D(new Vector2((float)prevMouseState.X, (float)prevMouseState.Y), camera, global.ScreenWidth, global.ScreenHeight, distanceZ, earthManager.earthpara);
                            newpoint = Helpler.GetProjectPoint3D(new Vector2((float)curpoint.X, (float)curpoint.Y), camera, global.ScreenWidth, global.ScreenHeight, distanceZ, earthManager.earthpara);
                        }
                        if (orgpoint != null && newpoint != null && orgpoint != newpoint)
                        {
                            Microsoft.Xna.Framework.Matrix matrix = MapHelper.getMatrixP2P((Vector3)newpoint, (Vector3)orgpoint, earthManager.earthpara);
                            matrix = Microsoft.Xna.Framework.Matrix.Invert(matrix);
                            if (matrix != Microsoft.Xna.Framework.Matrix.Identity)
                            {
                                vec = Vector3.Transform(vec, matrix);
                                tmpobj.VecLocation = new VECTOR3D(vec.X, vec.Y, vec.Z);

                                tmpobj.sendChangedLocation();
                                prevMouseState = curpoint;


                                RaiseEditObjectMoveEvent(new PickEventArgs(pickObject));

                                updateStatusBarCoordinate(e.GetPosition(grdMain));
                            }
                        }

                    }
                }

            }

            if (!config.isEditMode || (config.isEditMode && pickObject == null)) //正常模式 或 编辑模式无编辑对象时
            {

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (camera.operateMode == EOperateMode.地图模式)
                    {
                        #region 地图模式
                        if (curpoint.X > camera.XRotationScale * global.ScreenWidth) //在屏幕的右边1/10内，则为X轴旋转
                        {
                            float tmp = (float)(prevMouseState.Y - curpoint.Y) / global.ScreenHeight * MathHelper.PiOver4;
                            if (earthManager.earthpara.SceneMode == ESceneMode.地球)
                            {
                                System.Windows.Media.Media3D.Vector3D v1 = new System.Windows.Media.Media3D.Vector3D(camera.cameraPosition.X, camera.cameraPosition.Y, camera.cameraPosition.Z);
                                System.Windows.Media.Media3D.Vector3D v2 = new System.Windows.Media.Media3D.Vector3D(camera.cameraDirection.X, camera.cameraDirection.Y, camera.cameraDirection.Z);
                                double angle = System.Windows.Media.Media3D.Vector3D.AngleBetween(v1, v2) + tmp / MathHelper.Pi * 180;
                                if (angle < 180 && angle > 120)
                                {
                                    camera.cameraDirection = Vector3.Transform(camera.cameraDirection, Microsoft.Xna.Framework.Matrix.CreateFromAxisAngle(Vector3.Cross(camera.cameraUp, camera.cameraDirection), tmp));
                                    camera.calCameraByDirection();

                                    camera.updateD3DCamera();
                                    readyupdate = true;
                                    prevMouseState = curpoint;
                                }
                            }
                            else
                            {
                                System.Windows.Media.Media3D.Vector3D v1 = new System.Windows.Media.Media3D.Vector3D(0, 0, 1);
                                System.Windows.Media.Media3D.Vector3D v2 = new System.Windows.Media.Media3D.Vector3D(camera.cameraDirection.X, camera.cameraDirection.Y, camera.cameraDirection.Z);
                                double angle = System.Windows.Media.Media3D.Vector3D.AngleBetween(v1, v2) + tmp / MathHelper.Pi * 180;
                                if (angle < 180 && angle > 120)
                                {
                                    camera.cameraDirection = Vector3.Transform(camera.cameraDirection, Microsoft.Xna.Framework.Matrix.CreateFromAxisAngle(new Vector3(-1, 0, 0), tmp));
                                    camera.calCameraByDirection();

                                    camera.updateD3DCamera();
                                    readyupdate = true;
                                    prevMouseState = curpoint;
                                }

                            }
                        }
                        else  // 移动
                        {
                            Vector3? orgpoint = Helpler.GetProjectPoint3D(new Vector2((float)prevMouseState.X, (float)prevMouseState.Y), camera, global.ScreenWidth, global.ScreenHeight, 0, earthManager.earthpara);
                            Vector3? newpoint = Helpler.GetProjectPoint3D(new Vector2((float)curpoint.X, (float)curpoint.Y), camera, global.ScreenWidth, global.ScreenHeight, 0, earthManager.earthpara);
                            if (orgpoint != null && newpoint != null && orgpoint != newpoint)
                            {
                                Microsoft.Xna.Framework.Matrix matrix = MapHelper.getMatrixP2P((Vector3)newpoint, (Vector3)orgpoint, earthManager.earthpara);

                                if (matrix != Microsoft.Xna.Framework.Matrix.Identity)
                                {
                                    if (earthManager.earthpara.SceneMode == ESceneMode.地球)
                                    {
                                        camera.cameraPosition = Vector3.Transform(camera.cameraPosition, matrix);
                                        camera.cameraDirection = Vector3.Transform(camera.cameraDirection, matrix);
                                        camera.cameraUp = Vector3.Transform(camera.cameraUp, matrix);
                                    }
                                    else
                                    {
                                        camera.cameraPosition = Vector3.Transform(camera.cameraPosition, matrix);
                                        camera.cameraLookat = Vector3.Transform(camera.cameraLookat, matrix);
                                        camera.cameraDirection = camera.cameraLookat - camera.cameraPosition;
                                    }
                                    camera.calCameraByDirection();

                                    camera.updateD3DCamera();

                                    readyupdate = true;
                                    prevMouseState = curpoint;

                                    if (minimap != null && minimap.Visibility == System.Windows.Visibility.Visible)
                                        minimap.refreshPoly();
                                }
                            }

                        }
                        #endregion
                    }
                    else if (camera.operateMode == EOperateMode.轨迹球模式)
                    {
                        #region 轨迹球模式

                        double anglex = -180.0 * (curpoint.X - prevMouseState.X) / this.ActualWidth;
                        System.Windows.Media.Media3D.Vector3D axisx = System.Windows.Media.Media3D.Vector3D.CrossProduct(new System.Windows.Media.Media3D.Vector3D(camera.cameraDirection.X, camera.cameraDirection.Y, camera.cameraDirection.Z), new System.Windows.Media.Media3D.Vector3D(camera.cameraUp.X, camera.cameraUp.Y, camera.cameraUp.Z));
                        axisx = System.Windows.Media.Media3D.Vector3D.CrossProduct(axisx, new System.Windows.Media.Media3D.Vector3D(camera.cameraDirection.X, camera.cameraDirection.Y, camera.cameraDirection.Z));
                        axisx.Normalize();
                        System.Windows.Media.Media3D.AxisAngleRotation3D rotax = new System.Windows.Media.Media3D.AxisAngleRotation3D(axisx, anglex);
                        System.Windows.Media.Media3D.RotateTransform3D transformx = new System.Windows.Media.Media3D.RotateTransform3D(rotax, camera.traceBallCenter);

                        double angley = -180.0 * (curpoint.Y - prevMouseState.Y) / this.ActualHeight;
                        System.Windows.Media.Media3D.Vector3D axisy = System.Windows.Media.Media3D.Vector3D.CrossProduct(new System.Windows.Media.Media3D.Vector3D(camera.cameraDirection.X, camera.cameraDirection.Y, camera.cameraDirection.Z), new System.Windows.Media.Media3D.Vector3D(camera.cameraUp.X, camera.cameraUp.Y, camera.cameraUp.Z));
                        axisy.Normalize();
                        System.Windows.Media.Media3D.AxisAngleRotation3D rotay = new System.Windows.Media.Media3D.AxisAngleRotation3D(axisy, angley);
                        System.Windows.Media.Media3D.RotateTransform3D transformy = new System.Windows.Media.Media3D.RotateTransform3D(rotay, camera.traceBallCenter);


                        System.Windows.Media.Media3D.Point3D cameralocation = new System.Windows.Media.Media3D.Point3D(camera.cameraPosition.X, camera.cameraPosition.Y, camera.cameraPosition.Z);
                        cameralocation = transformx.Transform(cameralocation);
                        cameralocation = transformy.Transform(cameralocation);
                        camera.cameraPosition = new Vector3((float)cameralocation.X, (float)cameralocation.Y, (float)cameralocation.Z);

                        camera.cameraLookat = new Vector3((float)camera.traceBallCenter.X, (float)camera.traceBallCenter.Y, (float)camera.traceBallCenter.Z);
                        camera.cameraDirection = camera.cameraLookat - camera.cameraPosition;
                        camera.cameraDirection.Normalize();

                        camera.cameraUp = new Vector3((float)axisx.X, (float)axisx.Y, (float)axisx.Z);


                        camera.calCameraByDirection();
                        camera.updateD3DCamera();

                        prevMouseState = curpoint;


                        #endregion
                    }
                    else if (camera.operateMode == EOperateMode.平面模式)
                    {
                        #region 平面模式

                        

                        double anglex = -180.0 * (curpoint.X - prevMouseState.X) / this.ActualWidth;
                        System.Windows.Media.Media3D.Vector3D axisx = new System.Windows.Media.Media3D.Vector3D(0, 0, 1);
                        System.Windows.Media.Media3D.AxisAngleRotation3D rotax = new System.Windows.Media.Media3D.AxisAngleRotation3D(axisx, anglex);
                        System.Windows.Media.Media3D.RotateTransform3D transformx = new System.Windows.Media.Media3D.RotateTransform3D(rotax, camera.traceBallCenter);
                        System.Windows.Media.Media3D.Point3D cameralocation = new System.Windows.Media.Media3D.Point3D(camera.cameraPosition.X, camera.cameraPosition.Y, camera.cameraPosition.Z);
                        cameralocation = transformx.Transform(cameralocation);

                        double tmp = System.Windows.Media.Media3D.Vector3D.AngleBetween(new System.Windows.Media.Media3D.Vector3D(camera.cameraDirection.X, camera.cameraDirection.Y, camera.cameraDirection.Z), new System.Windows.Media.Media3D.Vector3D(0, 0, -1));
                        double angley = -180.0 * (curpoint.Y - prevMouseState.Y) / this.ActualHeight;
                        if ((tmp > 5 && angley < 0) || (tmp < 95 && angley > 0))
                        {
                            System.Windows.Media.Media3D.Vector3D axisy = System.Windows.Media.Media3D.Vector3D.CrossProduct(new System.Windows.Media.Media3D.Vector3D(camera.cameraDirection.X, camera.cameraDirection.Y, camera.cameraDirection.Z), new System.Windows.Media.Media3D.Vector3D(camera.cameraUp.X, camera.cameraUp.Y, camera.cameraUp.Z));
                            axisy.Normalize();
                            System.Windows.Media.Media3D.AxisAngleRotation3D rotay = new System.Windows.Media.Media3D.AxisAngleRotation3D(axisy, angley);
                            System.Windows.Media.Media3D.RotateTransform3D transformy = new System.Windows.Media.Media3D.RotateTransform3D(rotay, camera.traceBallCenter);
                            cameralocation = transformy.Transform(cameralocation);
                        }

                        
                        camera.cameraPosition = new Vector3((float)cameralocation.X, (float)cameralocation.Y, (float)cameralocation.Z);

                        camera.cameraLookat = new Vector3((float)camera.traceBallCenter.X, (float)camera.traceBallCenter.Y, (float)camera.traceBallCenter.Z);
                        camera.cameraDirection = camera.cameraLookat - camera.cameraPosition;
                        camera.cameraDirection.Normalize();

                        camera.cameraUp = new Vector3((float)axisx.X, (float)axisx.Y, (float)axisx.Z);


                        camera.calCameraByDirection();
                        camera.updateD3DCamera();

                        prevMouseState = curpoint;


                        #endregion
                    }


                    Tooltip.IsOpen = false;
                }
                else //tooltip move
                {
                    if (camera.operateMode == EOperateMode.自由视角)
                    {
                        #region 自由视角模式  改变相机面向方向

                        if (isFreeEnable)
                        {
                            isFreeEnable = false;
                            float anglex = (float)((curpoint - prevMouseState).X / global.ScreenWidth * Math.PI / 2);
                            float angley = (float)((curpoint - prevMouseState).Y / global.ScreenHeight * Math.PI / 2);

                            Microsoft.Xna.Framework.Matrix matrx = Microsoft.Xna.Framework.Matrix.CreateFromAxisAngle(camera.cameraUp, -anglex / 2);
                            Vector3 vecdir = Vector3.Transform(camera.cameraDirection, matrx);
                            Vector3 vecaxis = Vector3.Cross(vecdir, camera.cameraUp);
                            Microsoft.Xna.Framework.Matrix matry = Microsoft.Xna.Framework.Matrix.CreateFromAxisAngle(vecaxis, -angley / 2);
                            vecdir = Vector3.Transform(vecdir, matry);
                            vecdir.Normalize();

                            camera.cameraDirection = vecdir;
                            //global.camera.cameraUp = Vector3.Cross(vecaxis,vecdir);
                            camera.cameraUp = Vector3.Cross(vecdir, Vector3.Cross(new Vector3(0, 0, 1), vecdir));
                            camera.cameraLookat = camera.cameraPosition + vecdir;
                            camera.updateD3DCamera();
                            SetCursorPos((int)global.ScreenWidth / 2, (int)global.ScreenHeight / 2);
                        }
                        else
                        {
                            prevMouseState = Mouse.GetPosition(grdMain);
                            isFreeEnable = true;
                        }

                        #endregion

                    }

                    if (config.tooltipMoveEnable)
                    {
                        Tooltip.IsOpen = false;
                        mouseposition = e.GetPosition(grdMain);
                        if (tooltiptimer.IsEnabled)
                            tooltiptimer.Stop();
                        tooltiptimer.Start();
                    }

                    updateStatusBarCoordinate(e.GetPosition(grdMain));
                }

            }



        }


        ///<summary>更新状态栏坐标信息</summary>
        void updateStatusBarCoordinate(System.Windows.Point curpoint)
        {
            if (config.isShowCoordinate && MyBaseControls.StatusBarTool.StatusBarTool.isEnable)
            {
                System.Windows.Point? crd = camera.getOuterCoordinate((float)curpoint.X, (float)curpoint.Y);
                if (crd == null)
                    MyBaseControls.StatusBarTool.StatusBarTool.realTimeInfo.showInfo("");
                else
                    MyBaseControls.StatusBarTool.StatusBarTool.realTimeInfo.showInfo(crd.ToString());
            }
        }

        ///<summary>显示调试信息的方法</summary>
        void showdebuginfo()
        {
            string s0 = "相机方向地心夹角：{1:f3}；相机离地高度：{2:f3}；相机位置：{0}；";
            string info0 = string.Format(s0, camera.cameraPosition, camera.curCameraAngle, camera.curCameraDistanceToGround);

            string s1 = "经度近端：{0:f5}-{1:f5}，远端：{2:f5}-{3:f5}；纬度：{4:f5}-{5:f5}；";
            string info1="";
            if (camera.operateMode== EOperateMode.地图模式)
            info1= string.Format(s1, camera.curCamearaViewRange.nearLongitudeStart, camera.curCamearaViewRange.nearLongitudeEnd,
                camera.curCamearaViewRange.farLongitudeStart, camera.curCamearaViewRange.farLongitudeEnd,
                camera.curCamearaViewRange.latitudeStart, camera.curCamearaViewRange.latitudeEnd);

            string s2 = "屏幕中心：{0}；";
            System.Windows.Point? wjtmp = camera.getScreenCenter();
            string info2 = "";
            if (wjtmp != null)
                info2 = string.Format(s2, (System.Windows.Point)wjtmp);

            string s3 = "   对象数：{0}/{1}；";
            string info3 = "";
            try
            {
                int objcount = objManager.getObjList().Count();
                int objall = objManager.getAllObjList().Count();
                info3 = string.Format(s3, objcount, objall);
            }
            catch (Exception)
            {
            }


            string info4 = "";
            if (earthManager.calStatus == EarthManager.ECalStatus.空闲)
                info4 = string.Format("瓦片数：{0}；", earthManager.datas.blockcount);


            //txtDebug.Text = info1 + info2 + info4 + "  " + global.maxlayertileinfo;
            MyBaseControls.StatusBarTool.StatusBarTool.debugInfo.showInfo(info3 + info0 + info1 + info2 + info4 + "  " + global.maxlayertileinfo);
        }








        #endregion


        #region ========== 内置工具栏 ==========
        System.Windows.Media.Animation.DoubleAnimation aniexpand = new System.Windows.Media.Animation.DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
        bool isexpand = true;
        DispatcherTimer tmrToolbox = new DispatcherTimer();

        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isexpand && e.LeftButton == MouseButtonState.Released)
                showControlButtonPanel();
        }
        void showControlButtonPanel()
        {
            aniexpand.To = 0;
            paneltransform.BeginAnimation(TranslateTransform.XProperty, aniexpand);
            isexpand = true;
        }

        private void panel_MouseLeave(object sender, MouseEventArgs e)
        {
            hideControlButtonPanel();
        }
        void hideControlButtonPanel()
        {
            if (isexpand)
            {
                aniexpand.To = -40;
                paneltransform.BeginAnimation(TranslateTransform.XProperty, aniexpand);
                isexpand = !isexpand;
            }
        }
        void tmrToolbox_Tick(object sender, EventArgs e)
        {
            hideControlButtonPanel();
            tmrToolbox.Stop();
        }



        private void btnSat_Click(object sender, RoutedEventArgs e)
        {
            earthManager.mapType = EMapType.卫星;
        }

        private void btnRoad_Click(object sender, RoutedEventArgs e)
        {
            earthManager.mapType = EMapType.道路;
        }

        private void btnTerrain_Click(object sender, RoutedEventArgs e)
        {
            earthManager.mapType = EMapType.地形;
        }

        private void btnNone_Click(object sender, RoutedEventArgs e)
        {
            earthManager.mapType = EMapType.无;
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            camera.adjustCameraAngle(45);
        }

        private void btnDonw_Click(object sender, RoutedEventArgs e)
        {
            camera.adjustCameraAngle(0);
        }

        private void btnLight_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDark_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnJW_Click(object sender, RoutedEventArgs e)
        {

        }

        Tools.LightSetTool lightsettool;
        private void btnLightSet_Click(object sender, RoutedEventArgs e)  //光源设置
        {
                if (lightsettool == null)
                {
                    lightsettool = new Tools.LightSetTool() { VerticalAlignment = System.Windows.VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, 20), lightset = lightManager.lightset };
                    panelTool.Children.Add(lightsettool);
                }
                else
                    lightsettool.Visibility = lightsettool.Visibility == System.Windows.Visibility.Visible ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        #endregion


        #region ========== 导航地图 ==========
        internal minimap.minimap minimap;

        minimap.winSetupMinimap winminmapsetup;
        private void btnMiniMap_Click(object sender, RoutedEventArgs e)
        {
            if (winminmapsetup != null && winminmapsetup.IsVisible)
            {
                minimap.mapdata.save();
                winminmapsetup.Close();
                winminmapsetup.earth = null;
                winminmapsetup = null;
            }
            else
            {
                winminmapsetup = null;
                winminmapsetup = new minimap.winSetupMinimap();
                winminmapsetup.earth = this;
                winminmapsetup.ShowDialog();
            }
        }

        internal void showMinimap()
        {
            if (minimap == null)
                minimap = new minimap.minimap(this);
            if (grdminimap.Children.Count == 0)
                grdminimap.Children.Add(minimap);
            minimap.Visibility = System.Windows.Visibility.Visible;
        }
        internal void hideMinimap()
        {
            if (grdminimap.Children.Count > 0)
                minimap.Visibility = System.Windows.Visibility.Collapsed;
        }

        void initMinimap()
        {
            if (!config.enableMinimap) return;
            if (minimap == null)
                minimap = new minimap.minimap(this);
            if (grdminimap.Children.Count == 0)
                grdminimap.Children.Add(minimap);

        }

        #endregion




    }




}

