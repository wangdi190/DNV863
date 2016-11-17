using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using WpfEarthLibrary;
using System.Windows.Threading;
using DistNetLibrary;
using cfg = MyBaseControls.ConfigTool;


namespace DNVLibrary
{
    /// <summary>
    /// UCDNV863.xaml 的交互逻辑
    /// </summary>
    public partial class UCDNV863 : UserControl
    {
        public enum EDistnet { 亦庄15, 亦庄16, 亦庄new, 厦门 }

        public static EDistnet EDISTNET = EDistnet.亦庄15;


        public UCDNV863()
        {
            ContextSensitiveHelp.HelpProvider.SetKeyword(System.Windows.Application.Current.MainWindow, "配网规划");
            InitializeComponent();
        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            initMainMenu();  //初始化主菜单
            toolboxtimer.Tick += new EventHandler(toolboxtimer_Tick);
            tmrTime.Tick += new EventHandler(tmrTime_Tick);

            checkModelPermission();//检查模块权限
        }

        ///<summary>初始化状态栏</summary>
        void initStatusbar()
        {
            grdStatus.Children.Add(MyBaseControls.StatusBarTool.StatusBarTool.statusBar);
            MyBaseControls.StatusBarTool.StatusBarTool.realTimeInfo.width = 280;
            MyBaseControls.StatusBarTool.StatusBarTool.realTimeInfo.iconselect = MyBaseControls.StatusBarTool.EIcon.坐标;
            MyBaseControls.StatusBarTool.StatusBarTool.realTimeInfo.isVisible = true;
            MyBaseControls.StatusBarTool.StatusBarTool.reportInfo.isVisible = true;
            MyBaseControls.StatusBarTool.StatusBarTool.reportInfo.iconselect = MyBaseControls.StatusBarTool.EIcon.信息;

            MyBaseControls.StatusBarTool.StatusBarTool.statusInfo.isVisible = true;
            //MyBaseControls.StatusBarTool.StatusBarTool.statusInfo.calStatus.status = MyBaseControls.StatusBarTool.CalStatus.EStatus.无计算;

            MyBaseControls.StatusBarTool.StatusBarTool.debugInfo.isVisible = true;
            MyBaseControls.StatusBarTool.StatusBarTool.debugInfo.width=420;

            MyBaseControls.StatusBarTool.StatusBarTool.isEnable = true;


            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.isVisible = true;
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.iconselect = MyBaseControls.StatusBarTool.EIcon.提示;

            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.loadFromXml();
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "主菜单";




        }

        #region ===== 配置相关 =====

        ///<summary>初始化earth控件的工具栏</summary>
        void initEarthToolbox()
        {
            MyBaseControls.ConfigTool.Config.isMemory = true;

            Button btnconfig = new Button() { Width = 32, Height = 32, Background = Brushes.Transparent, Margin = new Thickness(0, 5, 0, 0), ToolTip = "配置" };
            btnconfig.Content = new Image() { Source = new BitmapImage(new Uri("/DNVLibrary;component/Images/config.png", UriKind.RelativeOrAbsolute)) };
            btnconfig.Click += new RoutedEventHandler(btnconfig_Click);
            earth.toolBox.Children.Add(btnconfig);
        }
        void btnconfig_Click(object sender, RoutedEventArgs e)
        {
            MyBaseControls.ConfigTool.winUserEdit win = new MyBaseControls.ConfigTool.winUserEdit();
            win.refreshObject = refreshconfigobject;  //重要，将刷新方法，委托给配置窗体，配置窗体才能调用刷新方法
            win.ShowDialog();
        }
        void refreshconfigobject(string cfgkey)
        {
            Dictionary<string, object> objdict = distnet.getAllObjDictAsObject();

            MyBaseControls.ConfigTool.Config.refreshObjects(objdict, cfgkey);
        }

        #endregion


        #region 时钟显示
        void tmrTime_Tick(object sender, EventArgs e)
        {
            time7Segment.Text = string.Format("{0:H:mm:ss}", DateTime.Now);
        }
        System.Windows.Threading.DispatcherTimer tmrTime = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
        public void ShowClock(bool isShow)
        {
            if (isShow)
            {
                Clock.Visibility = System.Windows.Visibility.Visible;
                tmrTime.Start();
            }
            else
            {
                Clock.Visibility = System.Windows.Visibility.Collapsed;
                tmrTime.Stop();
            }
        }
        #endregion

        internal Earth earth;
        internal DistNet distnet;


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            if (distnet == null)  //初始化主控件，应放在Loaded事件中（因为需要取得图面大小）
            {
                //  步骤1. 初始化设置各项参数
                initDistnet();

                //  步骤2. （可选）使用distnet.scene.objManager.AddSymbol添加额外的图元资源，
                //       disnet缺省的图元资源可通过DistNetLibrary.ESymbol.toString()取得，
                //       disnet缺省的几何体资源可通过DistNetLibrary.EGeometry.toString()取得
                //


                grdMap.Children.Add(distnet.scene);

                //DataGenerator.earth = earth;  // 注：以后去掉，赋模拟数据生成器引用对象
                //  步骤3. 载入配网图形
                switch (UCDNV863.EDISTNET)
                {
                    case EDistnet.亦庄15:
                        distnet.dbdesc = new Dictionary<string, DistNetLibrary.Edit.DBDesc>();
                        distnet.dbdesc.Add("基础数据", DistNetLibrary.Edit.DBDesc.ReadFromXml(".\\xml\\DBDesc.xml"));  //添加1.5版亦庄数据描述
                        loadGrid(); //载入旧亦庄配网
                        break;
                    case EDistnet.亦庄16:
                        distnet.dbdesc = new Dictionary<string, DistNetLibrary.Edit.DBDesc>();
                        distnet.dbdesc.Add("基础数据", DistNetLibrary.Edit.DBDesc.ReadFromXml(".\\xml\\DBDescYZ.xml"));  //添加1.6版亦庄数据描述
                        loadYZ();
                        break;
                    case EDistnet.亦庄new:
                        distnet.dbdesc = new Dictionary<string, DistNetLibrary.Edit.DBDesc>();
                        distnet.dbdesc.Add("基础数据", DistNetLibrary.Edit.DBDesc.ReadFromXml(".\\xml\\DBDescnewYZ.xml"));  //添加新版亦庄数据描述
                        loadNewYZ();
                        break;
                    case EDistnet.厦门:
                        loadXM();
                        break;
                }



                //  步骤3. 初始定位及其它可能需要的初始化设置
                //distnet.scene.camera.initCamera(511716.352216916, 288755.818023908, 0.41);//定位相机初始位置
                //distnet.scene.camera.initCamera(116.5178240834, 39.7599991899178, 0.41);//定位相机初始位置
                switch (UCDNV863.EDISTNET)
                {
                    case EDistnet.亦庄15:  //旧亦庄数据已转换为经纬坐标
                        distnet.scene.camera.initCamera(116.5178240834, 39.7599991899178, 0.41);//定位相机初始位置
                        break;
                    case EDistnet.亦庄16:  //新亦庄未转换，使用内置转换
                        distnet.scene.camera.initCamera(511716.352216916, 288755.818023908, 0.41);//定位相机初始位置;
                        break;
                    case EDistnet.亦庄new:  //新亦庄未转换，使用内置转换
                        distnet.scene.camera.initCamera(513367.785096645,288252.1618277, 0.41);//定位相机初始位置;513367.785096645,288252.1618277
                        break;
                    case EDistnet.厦门:
                        distnet.scene.camera.initCamera(111.029059, 21.469713, 0.41);//定位相机初始位置
                        break;
                }

                //distnet.scene.refreshColor();
                distnet.scene.camera.XRotationScale = 0.75;  //设置mouse位置大于屏幕宽度的此值时，不再是移动，而为X轴旋转

                distnet.scene.config.isShowDebugInfo = true;  //设置是否显示调试信息

                distnet.scene.legendManager.legends["设备图元图例"].fold();
                //closePanel();
            }
            initStatusbar();
        }

        ///<summary>初始化设置</summary>
        void initDistnet()
        {
            distnet = new DistNet();
            earth = distnet.scene;
            Planning._Global.distnet = distnet;  //规划模块引用
            //光源处理
            earth.config.enableLightSetPanel = true;
            earth.lightManager.lightset = WpfEarthLibrary.Tools.LightSet.ReadFromXml(string.Format("{0}{1}",AppDomain.CurrentDomain.BaseDirectory,"xml\\LightSet.xml"));
            if (earth.lightManager.lightset==null)
            {
                earth.lightManager.lightset = new WpfEarthLibrary.Tools.LightSet(true);
                earth.lightManager.lightset.xmlfile = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "xml\\LightSet.xml");
            }

            


            //初始化四参数转换
            //geohelper.m_DX = 4118585.75733497;
            //geohelper.m_DY = 38942245.9657318;
            //geohelper.m_Scale = 1.00006825902658;
            //geohelper.m_RotationAngle = 1.57811204215682;

            earth.earthManager.earthpara.isDepthStencil = false;

            //设置地方坐标与北京54的转换
            earth.coordinateManager.paraX = 4118585.75733497 + 100;//4118585.75733497;
            earth.coordinateManager.paraY = 38942245.9657318 + 530;//38942245.9657318;
            earth.coordinateManager.paraScale = 1.00006825902658;
            earth.coordinateManager.paraRotation = 1.57811204215682;
            earth.coordinateManager.paraYMirror = -1;
            //设置北京54与经纬坐标的转换
            earth.coordinateManager.gisCenter = null;
            earth.coordinateManager.gisBandtype = EBandType.三度带;
            earth.coordinateManager.gisCoordinate = ECoordinate.北京54;

            //打开坐标转换
            earth.coordinateManager.EnableTransformToGIS = true;
            if (UCDNV863.EDISTNET == EDistnet.亦庄15)
            {
                earth.coordinateManager.Enable = false;
                earth.coordinateManager.isXAsLong = false;  //重要：是否外部坐标是以X为经度
            }
            else if(UCDNV863.EDISTNET== EDistnet.亦庄16)
            {
                earth.coordinateManager.Enable = true;
                earth.coordinateManager.isXAsLong = true;  //重要：是否外部坐标是以X为经度
            }
            else if (UCDNV863.EDISTNET == EDistnet.亦庄new)
            {
                earth.coordinateManager.Enable = true;
                earth.coordinateManager.isXAsLong = true;  //重要：是否外部坐标是以X为经度
            }
            else if (UCDNV863.EDISTNET == EDistnet.厦门)
            {
                earth.coordinateManager.Enable = false;
                earth.coordinateManager.isXAsLong = false;  //重要：是否外部坐标是以X为经度
            }
            //设置光源
            //earth.lightManager.AmbientLight = Colors.White;

            //打开动态线宽
            earth.objManager.dynLineWidthEnable = true;
            earth.objManager.dynLineWidthDefaultDistance = 0.2f; //一般最初可设置为初始时的相机距离，看效果再调整
            earth.objManager.dynLineWidthMin = distnet.UnitMeasure * 0.2f;
            earth.objManager.dynLineWidthMax = distnet.UnitMeasure * 5f;
            earth.objManager.ObjectCheckMode = ECheckMode.视锥体检查;
            earth.objManager.isCheckPoints = true;
            if (UCDNV863.EDISTNET == EDistnet.亦庄15)
                earth.objManager.checkPointsMinLength = 0.000001;
            else if (UCDNV863.EDISTNET== EDistnet.亦庄16)
                earth.objManager.checkPointsMinLength = 0.01;
            else if (UCDNV863.EDISTNET == EDistnet.亦庄new)
                earth.objManager.checkPointsMinLength = 0.01;
            else if (UCDNV863.EDISTNET == EDistnet.厦门)
            {
                earth.objManager.isCheckPoints = true;
                earth.objManager.checkPointsMinLength = 0.0000001;
            }
            distnet.scene.colorManager.isEnabled = false;  //设置是否使用色彩管理
            distnet.scene.earthManager.earthpara.ArrowSpan = 0.05f;

            distnet.scene.config.isDynShow = true;    //设置是否使用动态呈现（此示例为全载入，动态呈现）

            distnet.UnitMeasure = 0.0001f; //设置场景的基础度量单元值

            //earth.tooltipMoveEnable = true;  //设置mouse move形式的tooltip是否生效
            distnet.scene.config.tooltipRightClickEnable = true; //设置mouse right click形式的tooltip生效以显示台账（右键单击显示台账）
            //distnet.scene.tooltipClickEnable = true;

            //设置地图瓦片模式，应在调用calPlaneModePara前设置, 若为自定义瓦片必须使用局部平面场景模式
            earth.earthManager.earthpara.tileReadMode = ETileReadMode.内置瓦片服务;

            //earth.earthManager.earthpara.isShowOverlay = false;

            distnet.scene.earthManager.earthpara.SceneMode = ESceneMode.局部平面;   //设置场景模式
            distnet.scene.earthManager.earthpara.InputCoordinate = EInputCoordinate.WGS84球面坐标; //设置图形对象输入坐标系，其它坐标系暂未支持
            if (UCDNV863.EDISTNET == EDistnet.亦庄15 || UCDNV863.EDISTNET == EDistnet.亦庄16 || UCDNV863.EDISTNET== EDistnet.亦庄new)
            {
                distnet.scene.earthManager.earthpara.StartLocation = new GeoPoint(38.75000, 115.48590);  //设置局部平面场景的起始坐标 , 注：若为自定义瓦片，它应是瓦片0层0行0列所代表的经纬
                distnet.scene.earthManager.earthpara.EndLocation = new GeoPoint(40.77232, 117.54060); //设置局部平面场景的终止坐标
                distnet.scene.earthManager.earthpara.calPlaneModePara();  //刷新计算内部参数
            }
            else if (UCDNV863.EDISTNET== EDistnet.厦门)
            {
                distnet.scene.earthManager.earthpara.StartLocation = new GeoPoint(20.469713, 100.029059);  //设置局部平面场景的起始坐标 , 注：若为自定义瓦片，它应是瓦片0层0行0列所代表的经纬
                distnet.scene.earthManager.earthpara.EndLocation = new GeoPoint(22.469713, 112.029059); //设置局部平面场景的终止坐标
                distnet.scene.earthManager.earthpara.calPlaneModePara();  //刷新计算内部参数
            }

            //相机设置
            distnet.scene.camera.Near = 0.001f; //设置相机近平面距离，用户自行调整
            //distnet.scene.camera.Far = 3.0f;  //设置相机远平面距离，用户自行调整
            distnet.scene.camera.Far = 1.0f;  //设置相机远平面距离，用户自行调整

            distnet.scene.camera.MinGroundDistance = 0.01f;
            //distnet.scene.camera.MaxGroundDistance = 2.9f;
            distnet.scene.camera.MaxGroundDistance = 0.9f;


            //下为若使用坐标调整的设置
            //earth.coordinateManager.orgJD = earth.earthManager.earthpara.StartLocation.Longitude; //设置缩放原点
            //earth.coordinateManager.orgWD = earth.earthManager.earthpara.StartLocation.Latitude;
            //earth.coordinateManager.scaleJD = 1;  //设置X方向缩放系数
            //earth.coordinateManager.offsetJD = 0; //设置X方向偏移
            //earth.coordinateManager.update();
            //earth.coordinateManager.Enable = false;  //设置坐标调整是否生效


            //distnet.isEditMode = true;
            //if (distnet.isEditMode)
            //    distnet.editpanel.Margin = new Thickness(0, 80, 0, 0);

            initEarthToolbox();

            distnet.scene.config.tooltipRightClickEnable = true;

            initLegend();
            initXModel();
            MyBaseControls.LogTool.Log.addLog(string.Format("已完成平台基础参数设置({0})",this), MyBaseControls.LogTool.ELogType.记录);
        }

        ///<summary>初始化图例</summary>
        void initLegend()
        {
            earth.legendManager.isShow = true;
            BrushLegend legend = earth.legendManager.createBrushLegend("设备图元图例");
            legend.header = " 设备设施 ";
            legend.isShow = true;
            legend.panel.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            legend.panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            legend.panel.Margin = new Thickness(5, 0, 0, 30);
            legend.panel.Background = new SolidColorBrush(Color.FromArgb(0xCC, 0x00, 0x00, 0x00));
            legend.headerBackground = Brushes.Black;
            legend.headerForeground = Brushes.Aqua;
            legend.headerBorderBrush = new SolidColorBrush(Colors.White);
            foreach (var item in earth.objManager.zSymbols.Values.Where(p => p.sort == "设备图元" || p.sort == "设施图元"))
            {
                legend.addItem(item.brush, item.id, Brushes.White, 24);
            }

            

        }

        void initXModel()
        {
            pXModel xm;
            //earth.objManager.AddXModel("transformer", ".\\x\\928house.x"); //载入3D模型，成熟后，直接并入DistNetLibrary
            //xm = earth.objManager.zXModels["transformer"];
            //xm.rotationAxis = new VECTOR3D(1, 0, 0);
            //xm.rotationAngle = (float)(Math.PI / 2);

            //earth.objManager.AddXModel("pv", ".\\x\\taiyangneng.x"); //载入3D模型，成熟后，直接并入DistNetLibrary
            //xm = earth.objManager.zXModels["pv"];
            //xm.rotationAxis = new VECTOR3D(1, 0, 0);
            //xm.rotationAngle = (float)(Math.PI / 2);

            earth.objManager.AddXModel("testxmodel", "x\\928house.x"); //载入3D模型，成熟后，直接并入DistNetLibrary
            earth.objManager.zXModels["testxmodel"].rotationAxis = new VECTOR3D(1, 0, 0);
            earth.objManager.zXModels["testxmodel"].rotationAngle = (float)(Math.PI / 2);


            //earth.objManager.AddSphereResource("球体", (float)(Math.PI * 2), 32, 32);

            //earth.objManager.AddBoxResource("立方体", 1, 1, 1);




        }

        #region ======================= 基础网架载入 ========================
        //------------------------------------------------------------------------------------------
        // 说明：从数据库中载入北京主城区配网数据为示例，经纬度坐标
        //------------------------------------------------------------------------------------------


        Random rd = new Random();

        protected DataTable dtsymbol, dtsymbolitem, dttext, dtlayer, dtstyle, dtshareobject; //dtsymbol为symbol和symbolitem的合集
        protected DataTable dtproject, dtallproject, dtdata;
        protected DataTable dtdictype, dtdicproperty, dtobject, dtproperty;
        protected DataRow curprj = null;
        int curprjkey;

        #region ----- 新亦庄载入 -----
        void loadNewYZ()
        {
            genCarSymbolBrush();

            //DateTime dtmp1 = DateTime.Now;

            string allfilter = "";
            string planninglayer = "规划层";

            //载入了所有实例所有对象
            loadBDZH3(allfilter,planninglayer);  //变电站
            loadXL3(allfilter, planninglayer);
            loadZBYQ3(allfilter, planninglayer);
            loadKBZH3(allfilter, planninglayer);
            loadPDS3(allfilter, planninglayer);
            loadPDBYQ3(allfilter, planninglayer);
            loadMX3(allfilter, planninglayer);
            loadDLQ3(allfilter, planninglayer);
            loadLJX3(allfilter, planninglayer);
            loadNode3(allfilter, planninglayer);
            loadLJD3(allfilter, planninglayer);
            loadCHX3(allfilter, planninglayer);

            loadRegionGrid3(allfilter);

            distnet.buildEquipmentFacilityRelationByLocation(50, 50, 50); //用图形来建立设施设备从属关系

            distnet.buildTopoRelation();  //建立拓扑双向联接
            
                


            IEnumerable<PowerBasicObject> tmpppp= distnet.getAllObjList();
            IEnumerable<PowerBasicObject> tmpppp2=tmpppp.Where(p =>p.busiTopo==null || (p.busiTopo as TopoData).relationObjs.Count == 0);

            



            global.clearNoLoadObject();
            List<WpfEarthLibrary.pLayer> layers = new List<WpfEarthLibrary.pLayer>() { distnet.scene.objManager.zLayers["规划层"], distnet.scene.objManager.zLayers["网格"] };
            global.showInstanceObject(layers, 1);
        }



        void loadBDZH3(string filter,string layername)//变电站
        {

            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["变电站"].batchCreateDNObjects(distnet,filter,layername); //注：方法还可附加过滤

            foreach (DNSubStation obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.symbolid = "变电站规划";//ESymbol.中压变电站.ToString();  //材质Key
                //obj.color = Colors.Lime;
                obj.isH = true;
                obj.visualMinDistance = visualdistance;
                //obj.scaleX = obj.scaleY = obj.scaleZ = distnet.UnitMeasure * 90;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;


                (obj as pSymbolObject).XModelKey = "testxmodel";
                (obj as pSymbolObject).XMScaleAddition = 0.0050;
                (obj as pSymbolObject).isUseXModel = true;


                cfg.Config.setValue(obj, obj.id, () => obj.color, "中压变电站.颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleX, "中压变电站.大小");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleY, "中压变电站.大小");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleZ, "中压变电站.大小");

                //===补充全局静态实例对象管理数据
                global.objects[obj.id].obj = obj;
            }
        }

        void loadXL3(string filter, string layername)//导线段
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["线路"].batchCreateDNObjects(distnet, filter, layername); //注：方法还可附加过滤
            foreach (DNACLine obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                //obj.lineColor = Colors.Yellow;
                //obj.arrowColor = Colors.Blue;
                obj.isFlow = false;
                //obj.arrowSize = distnet.UnitMeasure * 5 * 1.2f;
                //obj.thickness = distnet.UnitMeasure * 5;
                //obj.defaultArrowSize = distnet.UnitMeasure * 5 * 1.2f;
                //obj.defaultThickness = distnet.UnitMeasure * 5;
                obj.dynLineWidthEnable = true;
                //obj.visualMaxDistance = visualdistance;


                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;


                cfg.Config.setValue(obj, obj.id, () => obj.color, "中压输电线路.颜色.正常");
                cfg.Config.setValue(obj, obj.id, () => obj.thickness, "中压输电线路.线宽");
                cfg.Config.setValue(obj, obj.id, () => obj.defaultThickness, "中压输电线路.线宽");
                cfg.Config.setValue(obj, obj.id, () => obj.arrowColor, "中压输电线路.箭头颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.arrowSize, "中压输电线路.箭头大小");
                cfg.Config.setValue(obj, obj.id, () => obj.defaultArrowSize, "中压输电线路.箭头大小");

                //===补充全局静态实例对象管理数据
                global.objects[obj.id].obj = obj;
                obj.thisDesc.Flags.Add("线路",true);
            }
        }

        void loadLJX3(string filter, string layername)//连接线
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["连接线"].batchCreateDNObjects(distnet, filter, layername); //注：方法还可附加过滤
            foreach (DNACLine obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                //obj.lineColor = Colors.Yellow;
                //obj.arrowColor = Colors.Blue;
                obj.isFlow = false;
                //obj.arrowSize = distnet.UnitMeasure * 5 * 1.2f;
                //obj.thickness = distnet.UnitMeasure * 5;
                //obj.defaultArrowSize = distnet.UnitMeasure * 5 * 1.2f;
                //obj.defaultThickness = distnet.UnitMeasure * 5;
                obj.dynLineWidthEnable = true;
                obj.visualMaxDistance = visualdistance;


                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;


                cfg.Config.setValue(obj, obj.id, () => obj.color, "中压输电线路.颜色.正常");
                cfg.Config.setValue(obj, obj.id, () => obj.thickness, "中压输电线路.线宽");
                cfg.Config.setValue(obj, obj.id, () => obj.defaultThickness, "中压输电线路.线宽");
                cfg.Config.setValue(obj, obj.id, () => obj.arrowColor, "中压输电线路.箭头颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.arrowSize, "中压输电线路.箭头大小");
                cfg.Config.setValue(obj, obj.id, () => obj.defaultArrowSize, "中压输电线路.箭头大小");

                //===补充全局静态实例对象管理数据
                global.objects[obj.id].obj = obj;

            }
        }

        void loadZBYQ3(string filter, string layername)//主变压器
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["主变2卷"].batchCreateDNObjects(distnet, filter, layername); //注：方法还可附加过滤
            foreach (DNMainTransformer2W obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0025f;
                obj.color = System.Windows.Media.Colors.Lime;
                obj.visualMaxDistance = visualdistance;
                //===补充全局静态实例对象管理数据
                global.objects[obj.id].obj = obj;
                obj.thisDesc.Flags.Add("主变", true);

            }
        }

        void loadKBZH3(string filter, string layername)//开闭站
        {

            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["开关站"].batchCreateDNObjects(distnet, filter, layername); //注：方法还可附加过滤
            foreach (DNSwitchStation obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color =Color.FromRgb(0x4B, 0x4B, 0xFF);// Colors.Blue;
                obj.isH = true;
                obj.visualMinDistance = visualdistance;
                //obj.visualMaxDistance = visualdistance2;
                //obj.scaleX = obj.scaleY = obj.scaleZ = 0.004f;
                //cfg.Config.setValue(obj, obj.id,()=>obj.color , "中压开关站.颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleX, "中压开关站.大小");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleY, "中压开关站.大小");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleZ, "中压开关站.大小");

                obj.createTopoData();

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                //===补充全局静态实例对象管理数据
                global.objects[obj.id].obj = obj;

            }
        }

        void loadPDS3(string filter, string layername)//配电室
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["配电室"].batchCreateDNObjects(distnet, filter, layername); //注：方法还可附加过滤
            foreach (DNSwitchHouse obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                //obj.color = Colors.Blue;
                obj.isH = true;
                obj.visualMinDistance = visualdistance;
                //obj.visualMaxDistance = visualdistance2;
                //obj.scaleX = obj.scaleY = obj.scaleZ = 0.004f;
                cfg.Config.setValue(obj, obj.id, () => obj.color, "中压配电室.颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleX, "中压配电室.大小");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleY, "中压配电室.大小");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleZ, "中压配电室.大小");

                obj.createTopoData();
                

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                //===补充全局静态实例对象管理数据
                global.objects[obj.id].obj = obj;

            }
        }

        void loadPDBYQ3(string filter, string layername)//配电变压器
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["配变"].batchCreateDNObjects(distnet, filter, layername); //注：方法还可附加过滤
            foreach (DNDistTransformer obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0006f;
                obj.color = System.Windows.Media.Colors.Lime;
                obj.visualMaxDistance = visualdistance;
                //===补充全局静态实例对象管理数据
                global.objects[obj.id].obj = obj;
                obj.thisDesc.Flags.Add("配变", true);
            }
        }

        void loadMX3(string filter, string layername) //母线
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["母线"].batchCreateDNObjects(distnet, filter, layername); //注：方法还可附加过滤
            foreach (DNBusBar obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.isFlow = false;
                obj.dynLineWidthEnable = false;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                cfg.Config.setValue(obj, obj.id, () => obj.color, "母线.颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.thickness, "母线.线宽");
                obj.visualMaxDistance = visualdistance;
                //===补充全局静态实例对象管理数据
                global.objects[obj.id].obj = obj;

            }
        }

        void loadDLQ3(string filter, string layername)//断路器
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["断路器"].batchCreateDNObjects(distnet, filter, layername); //注：方法还可附加过滤
            foreach (DNBreaker obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0006f;
                obj.color = System.Windows.Media.Colors.Red;
                obj.visualMaxDistance = visualdistance;
                //===补充全局静态实例对象管理数据
                global.objects[obj.id].obj = obj;

            }
        }

        void loadNode3(string filter, string layername) //节点
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["节点"].batchCreateDNObjects(distnet, filter, layername); //注：方法还可附加过滤
            foreach (DNNode obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.White;
                obj.isH = true;
                obj.visualMaxDistance = visualdistance;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0002f;
                //obj.logicVisibility = false; 
                //===补充全局静态实例对象管理数据
                global.objects[obj.id].obj = obj;

            }
        }
        void loadLJD3(string filter, string layername) //连接点
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["连接点"].batchCreateDNObjects(distnet, filter, layername); //注：方法还可附加过滤
            foreach (DNConnectivityNode obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Lime;
                obj.isH = true;
                obj.visualMaxDistance = visualdistance;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0002f;
                //obj.logicVisibility = false;
                //===补充全局静态实例对象管理数据
                global.objects[obj.id].obj = obj;

            }
        }
        void loadCHX3(string filter, string layername) //变电站出线   
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["变电站出线"].batchCreateDNObjects(distnet, filter, layername); //注：方法还可附加过滤
            foreach (DNSubstationOutline obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Fuchsia;
                obj.isH = true;
                obj.visualMaxDistance = visualdistance;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0002f;
                //===补充全局静态实例对象管理数据
                global.objects[obj.id].obj = obj;

            }
        }


        void loadRegionGrid3(string filter)  //网格区域
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["网格"].batchCreateDNObjects(distnet, filter, "网格"); //注：方法还可附加过滤
            distnet.scene.objManager.zLayers["网格"].logicVisibility = false;
            foreach (DNGridArea obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                //===补充全局静态实例对象管理数据
                global.objects[obj.id].obj = obj;
            }

        }

#endregion

        #region ----- 亦庄1.6载入 -----
        ///<summary>装载新亦庄</summary>
        void loadYZ()
        {

            MyBaseControls.Screen.ScreenProgress.show();

            MyBaseControls.LogTool.Log.addLog(string.Format("开始载入对象({0})", this), MyBaseControls.LogTool.ELogType.记录);

            genCarSymbolBrush();
       
            //DateTime dtmp1 = DateTime.Now;

            loadBDZH2();  //变电站
            loadKBZH2();  //开闭站
            loadFJS2();    //分界室
            loadFJX2();    //分界箱
            loadPDS2();  //配电室
            loadDXD2();  //导线段
            /////////////loadDXD2Simple();// 代码合并线段后的线路    效果不理想，暂不使用
            loadDLD2();  //电缆段
            loadZSB2();  //柱上变
            loadZBYQ2();  //主变压器
            loadPDBYQ2(); //配变
            loadDLQ2();   //断路器
            loadRDQ2(); //熔断器
            loadFHKG2();   //负荷开关
            loadGLKG2();   //隔离开关
            loadZSFHKG2();   //柱上负荷开关
            loadZSGLKG2();   //柱上隔离开关
            loadMX2(); //母线
            loadZNLJX2(); //站内连接线
            loadZWLJX2();//站外连接线
            loadZWCLJX2();//站外超连接线
            loadZNDL2(); //站内电缆
            
            loadZXGT2();//直线杆塔
            //loadNZGT2();//耐张杆塔  耐张与直线大量重合？？？


            //double tmptmp = (DateTime.Now - dtmp1).TotalMilliseconds;

            //loadtemplinenode();

            distnet.buildTopoRelation();//生成拓扑
            distnet.buildEquipmentFacilityRelationByLocation(20.001, 20.001, 50.0004);//数据库中无从属关系，根据图形位置推断

            MyBaseControls.LogTool.Log.addLog(string.Format("结束载入对象({0})", this), MyBaseControls.LogTool.ELogType.记录);

            MyBaseControls.Screen.ScreenProgress.hide();
        }


        void loadtemplinenode() //临时测试学生处理线路和节点用
        {
            pLayer lay= distnet.addLayer("templayer");

            string sql = "select * from line";
            DataLayer.DataProvider.curDataSourceName = "临时数据源";
            DataTable dt = DataLayer.DataProvider.getDataTable(sql, "", DataLayer.EReadMode.数据库读取).Value;
            foreach (DataRow dr in dt.Rows)
            {
                string sss=dr["f_lj"].ToString();
                if (string.IsNullOrWhiteSpace(sss)) continue;
                sss=sss.Replace(';',' ');
                PointCollection pc = PointCollection.Parse(sss);
                if (pc.Count < 2) continue;


                DNACLine obj = new DNACLine(lay)
                {
                    id= dr["f_id"].ToString(),
                    color=Colors.Purple,
                    strPoints=sss,
                    thickness=0.0002f
                };
                obj.createAcntData();
                obj.thisAcntData.id=obj.id;
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                lay.AddObject(obj);
            }

        }


        void loadBDZH2()//变电站
        {

            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["变电站"].batchCreateDNObjects(distnet); //注：方法还可附加过滤

            foreach (DNSubStation obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.symbolid = "变电站规划";//ESymbol.中压变电站.ToString();  //材质Key
                //obj.color = Colors.Lime;
                obj.isH = true;
                obj.visualMinDistance = visualdistance;
                //obj.scaleX = obj.scaleY = obj.scaleZ = distnet.UnitMeasure * 90;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;


                (obj as pSymbolObject).XModelKey = "testxmodel";
                (obj as pSymbolObject).XMScaleAddition = 0.0050;
                (obj as pSymbolObject).isUseXModel = false;


                cfg.Config.setValue(obj, obj.id, () => obj.color, "中压变电站.颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleX, "中压变电站.大小");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleY, "中压变电站.大小");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleZ, "中压变电站.大小");
            }
        }
        void loadKBZH2()//开闭站
        {

            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["开闭站"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNSwitchStation obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Blue;
                obj.isH = true;
                obj.visualMinDistance = visualdistance;
                //obj.visualMaxDistance = visualdistance2;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.002f;
                obj.createTopoData();

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

            }
        }
        void loadFJS2()//分界室
        {

            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["分界室"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNDividingRoom obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Blue;
                obj.isH = true;
                obj.visualMinDistance = visualdistance;
                //obj.visualMaxDistance = visualdistance2;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.002f;
                obj.createTopoData();

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

            }
        }
        void loadFJX2()//分界箱
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["分界箱"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNDividingBox obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Blue;
                obj.isH = true;
                obj.visualMinDistance = visualdistance;
                //obj.visualMaxDistance = visualdistance2;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.002f;
                obj.createTopoData();

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

            }
        }
        void loadPDS2()//配电室
        {

            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["配电室"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNSwitchHouse obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Blue;
                obj.isH = true;
                obj.visualMinDistance = visualdistance;
                //obj.visualMaxDistance = visualdistance2;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.002f;
                obj.createTopoData();

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

            }
        }
        void loadDXD2()//导线段
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["导线段"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNLineSeg obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                //obj.lineColor = Colors.Yellow;
                //obj.arrowColor = Colors.Blue;
                obj.isFlow = false;
                //obj.arrowSize = distnet.UnitMeasure * 5 * 1.2f;
                //obj.thickness = distnet.UnitMeasure * 5;
                //obj.defaultArrowSize = distnet.UnitMeasure * 5 * 1.2f;
                //obj.defaultThickness = distnet.UnitMeasure * 5;
                obj.dynLineWidthEnable = true;
                //obj.visualMaxDistance = visualdistance;


                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;


                cfg.Config.setValue(obj, obj.id, () => obj.color, "中压输电线路.颜色.正常");
                cfg.Config.setValue(obj, obj.id, () => obj.thickness, "中压输电线路.线宽");
                cfg.Config.setValue(obj, obj.id, () => obj.defaultThickness, "中压输电线路.线宽");
                cfg.Config.setValue(obj, obj.id, () => obj.arrowColor, "中压输电线路.箭头颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.arrowSize, "中压输电线路.箭头大小");
                cfg.Config.setValue(obj, obj.id, () => obj.defaultArrowSize, "中压输电线路.箭头大小");


            }
        }
        void loadDXD2Simple()  //导线段代码连接合并, 注：仅有合并图形
        {
            string sql;
            DataTable dt;
            sql = "select * from T_TX_ZWYC_XL";
            dt = DataLayer.DataProvider.getDataTableFromSQL(sql);
            if (dt.Rows.Count > 0)
            {
                MessageBoxResult mbr = System.Windows.MessageBox.Show("线路表中，已有数据存在，选择<是>将覆盖坐标字段，选择<否>退出。", "", MessageBoxButton.YesNo);
                if (mbr == MessageBoxResult.No) return;
            }

            sql = "select ssxl from t_tx_zwyc_dxd group by ssxl";
            DataTable dtxlname = DataLayer.DataProvider.getDataTableFromSQL(sql);
            foreach (DataRow dr in dtxlname.Rows)
            {
                string xlid = dr[0].ToString();
                sql = String.Format("select shape from t_tx_zwyc_dxd where ssxl='{0}'", xlid);
                DataTable dtxd = DataLayer.DataProvider.getDataTableFromSQL(sql);
                List<xddata> xds = new List<xddata>();
                foreach (DataRow xddr in dtxd.Rows)
                {
                    string shape = xddr[0].ToString();

                    Match match = Regex.Match(shape, @"(\((\d+\.?\d*,\d+\.?\d*)\);\((\d+\.?\d*,\d+\.?\d*)\))");//查找select标识符
                    if (match.Success)
                        xds.Add(new xddata() { p1 = Point.Parse(match.Groups[2].Value), p2 = Point.Parse(match.Groups[3].Value) });
                    else
                        System.Windows.MessageBox.Show(shape + "未能正确解析");
                }

                //===== 以第一条线段开始延展
                PointCollection pc = new PointCollection();
                xds[0].isSelected = true;
                pc.Add(xds[0].p1);
                pc.Add(xds[0].p2);
                Point findpnt = xds[0].p2;  //向后添加
                xddata nextxd = xds.Where(p => !p.isSelected).FirstOrDefault(p => p.p1 == findpnt || p.p2 == findpnt);
                while (nextxd != null)
                {
                    findpnt = nextxd.p1 == findpnt ? nextxd.p2 : nextxd.p1;
                    pc.Add(findpnt);
                    nextxd.isSelected = true;
                    nextxd = xds.Where(p => !p.isSelected).FirstOrDefault(p => p.p1 == findpnt || p.p2 == findpnt);
                }
                findpnt = xds[0].p1;  //向前添加
                nextxd = xds.Where(p => !p.isSelected).FirstOrDefault(p => p.p1 == findpnt || p.p2 == findpnt);
                while (nextxd != null)
                {
                    findpnt = nextxd.p1 == findpnt ? nextxd.p2 : nextxd.p1;
                    pc.Insert(0, findpnt);
                    nextxd.isSelected = true;
                    nextxd = xds.Where(p => !p.isSelected).FirstOrDefault(p => p.p1 == findpnt || p.p2 == findpnt);
                }

                //=====处理离散的点
                IEnumerable<xddata> noselxds = xds.Where(p => !p.isSelected);
                while (noselxds.Count() > 0)
                {
                    //  System.Windows.MessageBox.Show(xlid+"有离散的点");
                    Point fp = pc.First();
                    Point lp = pc.Last();

                    xddata fxd, lxd;
                    double minf, minl;
                    minf = minl = double.PositiveInfinity;
                    Point fsamepnt, lsamepnt, fsamepnt2, lsamepnt2;
                    fxd = lxd = null;
                    fsamepnt = fsamepnt2 = lsamepnt = lsamepnt2 = new Point();
                    foreach (xddata nxd in noselxds)
                    {
                        if ((fp - nxd.p1).Length < minf) { minf = (fp - nxd.p1).Length; fxd = nxd; fsamepnt = nxd.p1; fsamepnt2 = nxd.p2; };
                        if ((fp - nxd.p2).Length < minf) { minf = (fp - nxd.p2).Length; fxd = nxd; fsamepnt = nxd.p2; fsamepnt2 = nxd.p1; };

                        if ((lp - nxd.p1).Length < minl) { minl = (lp - nxd.p1).Length; lxd = nxd; lsamepnt = nxd.p1; lsamepnt2 = nxd.p2; };
                        if ((lp - nxd.p2).Length < minl) { minl = (lp - nxd.p2).Length; lxd = nxd; lsamepnt = nxd.p2; lsamepnt2 = nxd.p1; };
                    }


                    if (minf < minl)  //一次延长前点或后点
                    {
                        if (minf > 1)
                            pc.Insert(0, fsamepnt);
                        pc.Insert(0, fsamepnt2);
                        fxd.isSelected = true;
                    }
                    else
                    {
                        if (minl > 1)
                            pc.Add(lsamepnt);
                        pc.Add(lsamepnt2);
                        lxd.isSelected = true;
                    }

                    noselxds = xds.Where(p => !p.isSelected);
                }

                //====== 去除直线上的点
                PointCollection newpc = new PointCollection();
                newpc.Add(pc[0]);
                for (int i = 1; i < pc.Count - 1; i++)
                {
                    if (Math.Abs(Vector.AngleBetween(pc[i - 1] - pc[i], pc[i + 1] - pc[i])) < 175)
                        newpc.Add(pc[i]);
                }



                //建立对象
                pLayer layer = distnet.addLayer("线路");
                DNACLine obj = new DNACLine(layer);
                obj.id = xlid;
                obj.strPoints = newpc.ToString();
                obj.isFlow = false;
                obj.dynLineWidthEnable = true;
                obj.visualMaxDistance = visualdistance;


                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;


                cfg.Config.setValue(obj, obj.id, () => obj.color, "中压输电线路.颜色.正常");
                cfg.Config.setValue(obj, obj.id, () => obj.thickness, "中压输电线路.线宽");
                cfg.Config.setValue(obj, obj.id, () => obj.defaultThickness, "中压输电线路.线宽");
                cfg.Config.setValue(obj, obj.id, () => obj.arrowColor, "中压输电线路.箭头颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.arrowSize, "中压输电线路.箭头大小");
                cfg.Config.setValue(obj, obj.id, () => obj.defaultArrowSize, "中压输电线路.箭头大小");
                layer.AddObject(obj);

            }
        }
        class xddata  //临时代码合并线段所用
        {
            public string oid;
            public Point p1;
            public Point p2;
            public bool isSelected;
        }
        void loadDLD2()//电缆段
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["电缆段"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNCableSeg obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Color.FromRgb(0xFF, 0x99, 0x00);
                //obj.arrowColor = Colors.Blue;
                obj.isFlow = false;
                //obj.arrowSize = distnet.UnitMeasure * 5 * 1.2f;
                //obj.thickness = distnet.UnitMeasure * 5;
                //obj.defaultArrowSize = distnet.UnitMeasure * 5 * 1.2f;
                //obj.defaultThickness = distnet.UnitMeasure * 5;
                obj.dynLineWidthEnable = true;
                //obj.visualMaxDistance = visualdistance;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                //Config.setValue(obj, obj.id, () => obj.lineColor, "中压输电线路.颜色.正常");
                cfg.Config.setValue(obj, obj.id, () => obj.thickness, "中压输电线路.线宽");
                cfg.Config.setValue(obj, obj.id, () => obj.defaultThickness, "中压输电线路.线宽");
                cfg.Config.setValue(obj, obj.id, () => obj.arrowColor, "中压输电线路.箭头颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.arrowSize, "中压输电线路.箭头大小");
                cfg.Config.setValue(obj, obj.id, () => obj.defaultArrowSize, "中压输电线路.箭头大小");


                //if (obj.id != "43360998-70F1-4EE6-BD27-5CAA9BB89385-38295")
                //    obj.logicVisibility = false;

            }
        }
        void loadZSB2()//柱上变
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["柱上变"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNColumnTransformer obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.001f;
                obj.color = System.Windows.Media.Colors.Lime;
                obj.visualMaxDistance = visualdistance;

            }
        }
        void loadZBYQ2()//主变压器
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["主变压器"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNMainTransformer obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.001f;
                obj.color = System.Windows.Media.Colors.Lime;
                obj.visualMaxDistance = visualdistance;

            }
        }
        void loadPDBYQ2()//配电变压器
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["配电变压器"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNDistTransformer obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0002f;
                obj.color = System.Windows.Media.Colors.Lime;
                obj.visualMaxDistance = visualdistance;

            }
        }
        void loadDLQ2()//断路器
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["断路器"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNBreaker obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0001f;
                obj.color = System.Windows.Media.Colors.Red;
                obj.visualMaxDistance = visualdistance;

            }
        }
        void loadRDQ2()//熔断器
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["熔断器"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNFuse obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0001f;
                obj.color = System.Windows.Media.Colors.Red;
                obj.visualMaxDistance = visualdistance;

            }
        }
        void loadFHKG2()//负荷开关
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["负荷开关"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNLoadSwitch obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0001f;
                obj.color = System.Windows.Media.Colors.Red;
                obj.visualMaxDistance = visualdistance;
            }
        }
        void loadGLKG2()//隔离开关
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["隔离开关"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNSwitch obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0001f;
                obj.color = System.Windows.Media.Colors.Red;
                obj.visualMaxDistance = visualdistance;
            }
        }
        void loadZSFHKG2()//柱上负荷开关
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["柱上负荷开关"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNLoadSwitch obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0001f;
                obj.color = Color.FromRgb(0xFF, 0x66, 0x00);
                obj.visualMaxDistance = visualdistance;
            }
        }
        void loadZSGLKG2()//柱上隔离开关
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["柱上隔离开关"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNSwitch obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0001f;
                obj.color = Color.FromRgb(0xFF, 0x66, 0x00);
                obj.visualMaxDistance = visualdistance;
            }
        }

        void loadMX2() //母线
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["母线"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNBusBar obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.isFlow = false;
                obj.dynLineWidthEnable = true;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                cfg.Config.setValue(obj, obj.id, () => obj.color, "母线.颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.thickness, "母线.线宽");
                obj.visualMaxDistance = visualdistance;

            }
        }
        void loadZNLJX2() //站内连接线
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["站内连接线"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNConnectivityLine obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.isFlow = false;
                obj.dynLineWidthEnable = false;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                cfg.Config.setValue(obj, obj.id, () => obj.color, "站内连接线.颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.thickness, "站内连接线.线宽");
                obj.visualMaxDistance = visualdistance;

            }
        }
        void loadZWLJX2() //站外连接线
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["站外连接线"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNConnectivityLine obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.isFlow = false;
                obj.dynLineWidthEnable = false;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                cfg.Config.setValue(obj, obj.id, () => obj.color, "站内连接线.颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.thickness, "站内连接线.线宽");
                obj.visualMaxDistance = visualdistance;

            }
        }
        void loadZWCLJX2() //站外超连接线
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["站外超连接线"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNConnectivityLine obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.isFlow = false;
                obj.dynLineWidthEnable = false;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                cfg.Config.setValue(obj, obj.id, () => obj.color, "站内连接线.颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.thickness, "站内连接线.线宽");
                obj.visualMaxDistance = visualdistance;

            }
        }

        void loadZNDL2() //站内电缆
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["站内电缆"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNCableSeg obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.isFlow = false;
                obj.dynLineWidthEnable = false;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                obj.color = Color.FromRgb(0xFF, 0x99, 0x00);
                //cfg.Config.setValue(obj, obj.id, () => obj.color, "站内连接线.颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.thickness, "站内连接线.线宽");
                obj.visualMaxDistance = visualdistance;

            }
        }
        void loadZXGT2()//直线杆塔
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["直线杆塔"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNIntermediateSupport obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0001f;
                obj.color = Colors.Aqua;
                obj.visualMaxDistance = visualdistance;
            }
        }
        void loadNZGT2()//耐张杆塔
        {
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["耐张杆塔"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNStrainSupport obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0001f;
                obj.color = Colors.White;
                obj.visualMaxDistance = visualdistance;
            }
        }



        #endregion

        double xdiv = 530, ydiv = 100; //x y 偏移，地方坐标附加的偏移

        ///<summary>设施与设备的分界距离</summary>
        internal double visualdistance = 0.1;
        ///<summary>主要设施与次要设施的分界距离</summary>
        double visualdistance2 = 0.3;

        #region ----- gis1.5义庄载入 -----


        ///<summary>装载主程序</summary>
        void loadGrid()
        {

            genCarSymbolBrush();


            //earth.objManager.AddXModel("testxmodel", ".\\x\\928house.x"); //载入3D模型，成熟后，直接并入DistNetLibrary
            //earth.objManager.zXModels["testxmodel"].rotationAxis = new VECTOR3D(1, 0, 0);
            //earth.objManager.zXModels["testxmodel"].rotationAngle = (float)Math.PI / 2;

            earth.objManager.AddXModel("testxmodel", ".\\shengwuzhinengdianzhan.x"); //载入3D模型，成熟后，直接并入DistNetLibrary
            earth.objManager.zXModels["testxmodel"].rotationAxis = new VECTOR3D(1, 0, 0);
            earth.objManager.zXModels["testxmodel"].rotationAngle = (float)Math.PI/2 ;

            //DateTime dstart = DateTime.Now;

            //高压
            //loadGYBDZH();
            //loadGY2ZHB();
            //loadGY3ZHB();
            //loadGYXlXD();
            //loadGYMX();
            //loadGYML();
            //loadGYLJX();
            ////loadLJD();  //连接点
            //adjustGyLineDir();  //暂不用，当前方向与判断方向是一致的

            //中压
            loadZYBDZH();  //高压变电站中，含有中压且id和名称都不同，暂时不要, 
            loadZY2ZHB();
            loadZY3ZHB();
            loadZYXlXD();
            loadZYMX();
            loadZYML();
            loadZYLJX();
            loadZYLJD();  //连接点, 不显示出来，但需分析拓扑使用
            loadBDZHCHX();  //变电站出线
            loadKBZH();
            loadKG();
            loadPDSH();
            loadPDBYQ();
            loadPDMX();

            loadNode();//节点 
            //loadXQArea();
            loadGridArea();

            loadPlant();



            //double tmpppp = (DateTime.Now - dstart).TotalMilliseconds;

            distnet.buildTopoRelation();//生成拓扑
            distnet.buildEquipmentFacilityRelationByLocation(0.001, 0.001, 0.0004);//数据库中无从属关系，根据图形位置推断

            //PowerBasicObject obj = distnet.scene.objManager.find("_0005_201506150141321003292");
            //var tmp = distnet.getTrace(obj, EObjectType.两卷主变, EObjectType.三卷主变);

            
        }






        ///<summary>载入高压变电站</summary>
        void loadGYBDZH()
        {
            string layername = EObjectCategory.变电设施类.ToString();
            pLayer containerLayer = distnet.addLayer(layername);
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from h_substation");

            foreach (DataRow dr in dt.Rows)
            {
                Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_ex") + xdiv, -(dr.Field<double>("f_ey") + ydiv)));
                Point gp = geohelper.Plane2Geo(tp);
                //===== 创建对象
                DNSubStation obj = new DNSubStation(containerLayer)
                {
                    id = dr["f_id"].ToString(),
                    name = dr["f_mch"].ToString(),
                    symbolid = ESymbol.变电站运行.ToString(),  //材质Key
                    color = Colors.Lime,
                    isH = true,
                    location = gp.ToString(),
                    visualMinDistance = visualdistance,
                };
                containerLayer.AddObject(obj);
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.009f;
                obj.busiData.busiCategory = "变电站";
                obj.busiData.busiSort = "高压";
                //===== 创建台账
                obj.busiAccount = new AcntSubstation()
                {
                    id = obj.id,
                    name = obj.name,
                    vl = dr.getDouble("F_DYDJ"),
                    cap = dr.getDouble("f_rl"),
                    buildingMode = (AcntDataBase.EBuildingMode)dr.getIntN1("f_jshxs"),
                    capComposition = dr.getString("f_zhbrl"),
                    structureMode = (AcntDataBase.EStructureMode)dr.getIntN1("f_jgxs"),
                    economicCap = dr.getDouble("F_JJRL"),
                    planningProp = (AcntDataBase.EPlanningProp)dr.getIntN1("f_ghxzh"),
                    runDate = dr.getDatetime("f_tyrq"),
                    retireDate = dr.getDatetime("f_tynf"),
                };
                //===== 设置台账为clicktooltip内容
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
                //===== 创建拓扑数据
                obj.createTopoData();



                ////测试.x模型
                //(obj as pSymbolObject).XModelKey = "testxmodel";
                //(obj as pSymbolObject).XMScaleAddition = 0.0100;
                //(obj as pSymbolObject).isUseXModel = true;

            }

        }
        ///<summary>载入中压变电站</summary>
        void loadZYBDZH()
        {
            #region 原手动方式，注释留存参考
            //string layername = EObjectCategory.变电设施类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);

            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from m_substation");

            //foreach (DataRow dr in dt.Rows)
            //{
            //    //Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_ex") + xdiv, -(dr.Field<double>("f_ey") + ydiv)));
            //    //Point gp = geohelper.Plane2Geo(tp);
            //    Point gp = new Point(dr.Field<double>("f_gx"), (dr.Field<double>("f_gy")));

            //    DNSubStation obj = new DNSubStation(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        symbolid = ESymbol.中压变电站.ToString(),  //材质Key
            //        color = Colors.Lime,
            //        isH = true,
            //        location = gp.ToString(),
            //        visualMinDistance = visualdistance,
            //        DBOPKey = "中压变电站",
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.scaleX = obj.scaleY = obj.scaleZ = distnet.UnitMeasure * 90;
            //    obj.busiData.busiCategory = "变电站";
            //    obj.busiData.busiSort = "中压";

            //    obj.busiAccount = new AcntSubstation()
            //    {
            //        id = obj.id,
            //        name = obj.name,
            //        etype = (AcntDataBase.ESubStationType)DataLayer.Tools.getIntN1(dr, "F_BDZHLX"),
            //        vl = dr.getDouble("f_dydj"),
            //        cap = dr.getDouble("F_RL"),
            //        //buildingMode = (DNVLibrary.AcntDataBase.EBuildingMode)dr.Field<short>("f_jshxs"),
            //        capComposition = dr.getString("f_zhbrl"),
            //        //structureMode = (DNVLibrary.AcntDataBase.EStructureMode)dr.Field<short>("f_jgxs"),
            //        //economicCap = dr.Field<double>("F_JJRL"),
            //        //hSpanCount = dr.Field<int>("F_gycjg"),
            //        //hUseSpanCount dr.Field<int>("F_gycyyjg"),
            //        //mSpanCount = dr.Field<int>("F_zhycjg"),
            //        //mUseSpanCount = dr.Field<int>("F_zhycyyjg"),
            //        //lSpanCount = dr.Field<int>("F_dycjg"),
            //        //lUseSpanCount = dr.Field<int>("F_dycyyjg"),
            //        //isAuto = dr.Field<short>("F_zhzdhshl") == 1,
            //        //isOptimize = dr.Field<short>("F_shfcjyh") == 1,
            //        //isPublice = dr.Field<short>("F_shfgy") == 1,
            //        //isUnmanned = dr.Field<short>("F_wrzhbshl") == 1,
            //        //maxload = dr.Field<double>("F_maxfh"),
            //        planningProp = (AcntDataBase.EPlanningProp)dr.getIntN1("f_ghxzh"),
            //        runDate = dr.getDatetime("f_tyrq"),
            //        retireDate = dr.getDatetime("f_tynf"),
            //        supplyAreaType = (AcntDataBase.ESupplyAreaType)dr.getIntN1("f_gdqhtypeid"),
            //    };
            //    //clicktooltip
            //    obj.tooltipRightClickTemplate = "AcntTemplate";
            //    obj.tooltipRightClickContent = obj.busiAccount;

            //    //===== 创建拓扑数据
            //    obj.createTopoData();



            //    ////测试.x模型
            //    (obj as pSymbolObject).XModelKey = "testxmodel";
            //    (obj as pSymbolObject).XMScaleAddition = 0.0100;
            //    (obj as pSymbolObject).isUseXModel = true;

            //}

            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["中压变电站"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNSubStation obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.symbolid = ESymbol.变电站运行.ToString();  //材质Key
                //obj.color = Colors.Lime;
                obj.isH = true;
                obj.visualMinDistance = visualdistance;
                obj.scaleX = obj.scaleY = obj.scaleZ = distnet.UnitMeasure * 900;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                (obj as pSymbolObject).XModelKey = "testxmodel";
                (obj as pSymbolObject).XMScaleAddition = 0.0100;
                //(obj as pSymbolObject).XMScaleAddition = 0.0002;

                (obj as pSymbolObject).isUseXModel = true;


                cfg.Config.setValue(obj, obj.id, () => obj.color, "中压变电站.颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleX, "中压变电站.大小");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleY, "中压变电站.大小");
                cfg.Config.setValue(obj, obj.id, () => obj.scaleZ, "中压变电站.大小");



            }


            #endregion


        }

        ///<summary>载入高压2卷主变</summary>
        void loadGY2ZHB()
        {
            string layername = EObjectCategory.变压器类.ToString();
            pLayer containerLayer = distnet.addLayer(layername);

            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from h_maintransformer_twovolume t1 left JOIN he_maintransformer_twovolume_para t2 on t1.F_TYPE=t2.F_TYPE");
            foreach (DataRow dr in dt.Rows)
            {
                Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_ex") + xdiv, -(dr.Field<double>("f_ey") + ydiv)));
                Point gp = geohelper.Plane2Geo(tp);
                DNMainTransformer2W obj = new DNMainTransformer2W(containerLayer)
                {
                    id = dr["f_id"].ToString(),
                    name = dr["f_bdzhmch"].ToString() + dr["f_mch"].ToString(),
                    //symbolid = ESymbol.两卷变压器.ToString(),  //材质Key
                    color = Colors.Lime,
                    isH = true,
                    location = gp.ToString(),
                    visualMaxDistance = visualdistance,
                };
                containerLayer.AddObject(obj);
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.002f;
                obj.busiData.busiCategory = "变压器";
                obj.busiData.busiSort = "高压";


                obj.busiAccount = new AcntMainTransformer2W()
                {
                    id = obj.id,
                    name = obj.name,
                    cap = dr.getDouble("F_RL"),
                    planningProp = (AcntDataBase.EPlanningProp)dr.getIntN1("f_ghxzh"),
                    runDate = dr.getDatetime("f_tyrq"),
                    retireDate = dr.getDatetime("f_tynf"),
                    hvl = dr.getDouble("F_DYDJ"),
                    hnvl = dr.getDouble("F_HEDDY"),
                    lvl = dr.getDouble("F_LDYDJ"),
                    lnvl = dr.getDouble("F_LEDDY"),
                    model = dr.getString("f_type"),
                    rcap = dr.getDouble("F_WGRL"),
                    reactivepowerconfig = dr.getString("F_WGPZH"),
                    reactance = dr.getDouble("F_DKQ"),
                    idlingcurrent = dr.getDouble("F_kzdl"),
                    idlingloss = dr.getDouble("F_kzsh"),
                    shortvl = dr.getDouble("F_dldy"),
                    shortloss = dr.getDouble("F_dlsh"),
                };
                //clicktooltip
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                //----- 拓扑数据
                obj.createTopoData();
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_hmxid") }); //value的false表示未经分析处理
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_lmxid") });
            }

        }
        ///<summary>载入中压2卷主变</summary>
        void loadZY2ZHB()
        {
            #region 载入方式一：手动方式，全手动读取和设置各项数据，无需数据描述支持，这是最灵活的方式，但代码量较大
            //string layername = EObjectCategory.变压器类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);

            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from m_maintransformer_twovolume t1 left JOIN me_maintransformer_twovolume_para t2 on t1.F_TYPE=t2.F_TYPE");
            //foreach (DataRow dr in dt.Rows)
            //{
            //    //Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_ex") + xdiv, -(dr.Field<double>("f_ey") + ydiv)));
            //    //Point gp = geohelper.Plane2Geo(tp);
            //    Point gp = new Point(dr.Field<double>("f_gx"), (dr.Field<double>("f_gy")));
            //    DNMainTransformer2W obj = new DNMainTransformer2W(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_bdzhmch"].ToString() + dr["f_mch"].ToString(),
            //        color = Colors.Lime,
            //        location = gp.ToString(),
            //        visualMaxDistance = visualdistance,
            //        DBOPKey = "中压2卷变压器",  //需提供正常的DB操作键，才可以支持编辑功能
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.scaleX = obj.scaleY = obj.scaleZ = 0.002;
            //    obj.busiData.busiCategory = "变压器";
            //    obj.busiData.busiSort = "中压";


            //    obj.busiAccount = new AcntMainTransformer2W()
            //    {
            //        id = obj.id,
            //        name = obj.name,
            //        cap = dr.getDouble("F_RL"),
            //        planningProp = (AcntDataBase.EPlanningProp)dr.getIntN1("f_ghxzh"),
            //        runDate = dr.getDatetime("f_tyrq"),
            //        retireDate = dr.getDatetime("f_tynf"),
            //        belongto = dr.getString("F_BDZHMCH"),
            //        hvl = dr.getDouble("F_DYDJ"),
            //        hnvl = dr.getDouble("F_HEDDY"),
            //        lvl = dr.getDouble("F_LDYDJ"),
            //        lnvl = dr.getDouble("F_LEDDY"),
            //        model = dr["f_type"].ToString(),
            //        rcap = dr.getDouble("F_WGRL"),
            //        reactivepowerconfig = dr.getString("F_WGPZH"),
            //        reactance = double.NaN,// double.Parse(dr["F_DKQ"].ToString()),
            //        isAdjustVL = false,// dr.Field<short>("F_SHFYZTY") == 1,
            //        price = dr.getDouble("F_price"),
            //        idlingcurrent = dr.getDouble("F_kzdl"),
            //        idlingloss = dr.getDouble("F_kzsh"),
            //        shortvl = dr.getDouble("F_dldy"),
            //        shortloss = dr.getDouble("F_dlsh"),
            //    };
            //    //clicktooltip
            //    obj.tooltipRightClickTemplate = "AcntTemplate";
            //    obj.tooltipRightClickContent = obj.busiAccount;

            //    //----- 拓扑数据
            //    obj.createTopoData();
            //    obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_hmxid") });
            //    obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_lmxid") });
            //}
            #endregion

            #region 载入方式二：半手动方式，手动读取数据库，手动创建对象，调用方法填充台账、关键数据和基础拓扑，介于方式一和方式三之间

            #endregion

            #region 载入方式三：自动方式，自动读取数据库，自动创建对象，自动填充台账、关键数据和基础拓扑，自动填充扩展拓扑，手动填写可视属性，这是最快捷的方式，但灵活性较低
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["中压2卷变压器"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNMainTransformer2W obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Lime;
                obj.visualMaxDistance = visualdistance;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.002f;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
            }


            #endregion


        }

        ///<summary>载入高压3卷主变</summary>
        void loadGY3ZHB()
        {
            string layername = EObjectCategory.变压器类.ToString();
            pLayer containerLayer = distnet.addLayer(layername);
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from h_maintransformer_threevolume t1 left JOIN he_maintransformer_threevolume_para t2 on t1.F_TYPE=t2.F_TYPE");
            foreach (DataRow dr in dt.Rows)
            {
                Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_ex") + xdiv, -(dr.Field<double>("f_ey") + ydiv)));
                Point gp = geohelper.Plane2Geo(tp);
                DNMainTransformer3W obj = new DNMainTransformer3W(containerLayer)
                {
                    id = dr["f_id"].ToString(),
                    name = dr["f_bdzhmch"].ToString() + dr["f_mch"].ToString(),
                    symbolid = "3卷变压器",  //材质Key
                    color = Colors.Lime,
                    isH = true,
                    location = gp.ToString(),
                    visualMaxDistance = visualdistance,
                };
                containerLayer.AddObject(obj);
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.002f;

                obj.busiAccount = new AcntMainTransformer3W()
                {
                    id = obj.id,
                    name = obj.name,
                    cap = dr.getDouble("F_RL"),
                    planningProp = (AcntDataBase.EPlanningProp)dr.getIntN1("f_ghxzh"),
                    runDate = dr.getDatetime("f_tyrq"),
                    retireDate = dr.getDatetime("f_tynf"),
                    hvl = dr.getDouble("F_DYDJ"),
                    hnvl = dr.getDouble("F_HEDDY"),
                    mvl = dr.getDouble("F_MDYDJ"),
                    mnvl = dr.getDouble("F_MEDDY"),
                    lvl = dr.getDouble("F_LDYDJ"),
                    lnvl = dr.getDouble("F_LEDDY"),
                    model = dr["f_type"].ToString(),
                    rcap = dr.getDouble("F_WGRL"),
                    reactivepowerconfig = dr["F_WGPZH"].ToString(),
                    reactance = dr.getDouble("F_DKQ"),
                    idlingcurrent = dr.getDouble("F_kzdl"),
                    idlingloss = dr.getDouble("F_kzsh"),
                    shortvlhl = dr.getDouble("F_dldy_hl"),
                    shortlosshl = dr.getDouble("F_dlsh_hl"),
                    shortvlhm = dr.getDouble("F_dldy_hm"),
                    shortlosshm = dr.getDouble("F_dlsh_hm"),
                    shortvlml = dr.getDouble("F_dldy_ml"),
                    shortlossml = dr.getDouble("F_dlsh_ml"),
                };
                //clicktooltip
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                //----- 拓扑数据
                obj.createTopoData();
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_hmxid") });
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_mmxid") });
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_lmxid") });
            }

        }
        ///<summary>载入中压3卷主变</summary>
        void loadZY3ZHB()
        {
            string layername = EObjectCategory.变压器类.ToString();
            pLayer containerLayer = distnet.addLayer(layername);
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from m_maintransformer_threevolume t1 left JOIN me_maintransformer_threevolume_para t2 on t1.F_TYPE=t2.F_TYPE");
            foreach (DataRow dr in dt.Rows)
            {
                //Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_ex") + xdiv, -(dr.Field<double>("f_ey") + ydiv)));
                //Point gp = geohelper.Plane2Geo(tp);
                Point gp = new Point(dr.Field<double>("f_gx"), (dr.Field<double>("f_gy")));
                DNMainTransformer3W obj = new DNMainTransformer3W(containerLayer)
                {
                    id = dr["f_id"].ToString(),
                    name = dr["f_bdzhmch"].ToString() + dr["f_mch"].ToString(),
                    color = Colors.Lime,
                    isH = true,
                    location = gp.ToString(),
                    visualMaxDistance = visualdistance,
                };
                containerLayer.AddObject(obj);
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.002f;

                obj.busiAccount = new AcntMainTransformer3W()
                {
                    id = obj.id,
                    name = obj.name,
                    cap = dr.getDouble("F_RL"),
                    planningProp = (AcntDataBase.EPlanningProp)dr.getIntN1("f_ghxzh"),
                    runDate = dr.getDatetime("f_tyrq"),
                    retireDate = dr.getDatetime("f_tynf"),
                    hvl = dr.getDouble("F_DYDJ"),
                    hnvl = dr.getDouble("F_HEDDY"),
                    mvl = dr.getDouble("F_MDYDJ"),
                    mnvl = dr.getDouble("F_MEDDY"),
                    lvl = dr.getDouble("F_LDYDJ"),
                    lnvl = dr.getDouble("F_LEDDY"),
                    model = dr["f_type"].ToString(),
                    rcap = dr.getDouble("F_WGRL"),
                    reactivepowerconfig = dr["F_WGPZH"].ToString(),
                    reactance = dr.getDouble("F_DKQ"),
                    idlingcurrent = dr.getDouble("F_kzdl"),
                    idlingloss = dr.getDouble("F_kzsh"),
                    shortvlhl = dr.getDouble("F_dldy_hl"),
                    shortlosshl = dr.getDouble("F_dlsh_hl"),
                    shortvlhm = dr.getDouble("F_dldy_hm"),
                    shortlosshm = dr.getDouble("F_dlsh_hm"),
                    shortvlml = dr.getDouble("F_dldy_ml"),
                    shortlossml = dr.getDouble("F_dlsh_ml"),

                };
                //clicktooltip
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                //----- 拓扑数据
                obj.createTopoData();
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_hmxid") });
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_mmxid") });
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_lmxid") });


            }

        }

        ///<summary>载入变电站出线，点</summary>
        void loadBDZHCHX()
        {
            #region 原手动方式，注释留存参考
            //string layername = EObjectCategory.其它类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);
            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from M_SubstationOutline");
            //foreach (DataRow dr in dt.Rows)
            //{
            //    //Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_gx") + xdiv, -(dr.Field<double>("f_gy") + ydiv)));
            //    //Point gp = geohelper.Plane2Geo(tp);
            //    Point gp = new Point(dr.Field<double>("f_gx"), (dr.Field<double>("f_gy")));
            //    DNSubstationOutline obj = new DNSubstationOutline(containerLayer)
            //     {
            //         id = dr["f_id"].ToString(),
            //         name = dr["f_mch"].ToString(),
            //         symbolid = ESymbol.杆塔_小圆圈.ToString(),  //材质Key
            //         color = Colors.Blue,
            //         isH = true,
            //         location = gp.ToString(),
            //         visualMaxDistance = visualdistance,
            //     };
            //    containerLayer.AddObject(obj);
            //    obj.scaleX = obj.scaleY = obj.scaleZ = 0.00015;
            //    obj.busiData.busiSort = "中压";


            //    //----- 拓扑数据
            //    obj.createTopoData();
            //    obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_mxid") });
            //}


            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["中压变电站出线"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNSubstationOutline obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.symbolid = ESymbol.小圆圈.ToString();  //材质Key
                obj.color = Colors.Blue;
                obj.isH = true;
                obj.visualMaxDistance = visualdistance;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.00015f;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

            }


            #endregion

        }

        ///<summary>载入开闭站</summary>
        void loadKBZH()
        {
            #region 原手动方式，注释留存参考
            //string layername = EObjectCategory.开关设施类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);

            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from M_SwitchingStation t1 left join me_pdstation_para t2 on t1.f_type=t2.F_TYPE");
            //foreach (DataRow dr in dt.Rows)
            //{
            //    //Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_gx") + xdiv, -(dr.Field<double>("f_gy") + ydiv)));
            //    //Point gp = geohelper.Plane2Geo(tp);
            //    Point gp = new Point(dr.Field<double>("f_gx"), (dr.Field<double>("f_gy")));
            //    DNSwitchStation obj = new DNSwitchStation(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        color = Colors.Blue,
            //        isH = true,
            //        location = gp.ToString(),
            //        visualMinDistance = visualdistance,
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.scaleX = obj.scaleY = obj.scaleZ = 0.004;

            //    obj.busiData.busiSort = "中压";

            //    obj.busiAccount = new AcntSwitchStation()
            //    {
            //        id = obj.id,
            //        name = obj.name,
            //        belongto = dr.getString("f_bdzhmch"),
            //        BusbarOutLineCount = dr.getInt("f_mxchxsh"),
            //        inLineCount = dr.getInt("f_jx"),
            //        isLinkDT = dr.getInt("f_shfjpb") == 1,
            //        isPublice = dr.getInt("f_shfgy") == 1,
            //        model = dr.getString("f_type"),
            //        modelName = dr.getString("f_lxmch"),
            //        outLineCount = dr.getInt("f_chx"),
            //        outLineSpanCount = dr.getInt("f_chxjg"),
            //        planningProp = (AcntDataBase.EPlanningProp)dr.getIntN1("f_ghxzh"),
            //        price = dr.getDouble("f_price"),
            //        restOutLineSpanCount = dr.getInt("f_shychxjg"),
            //        retireDate = dr.getDatetime("f_tynf"),
            //        runDate = dr.getDatetime("f_tyrq"),
            //        supplyAreaType = (AcntDataBase.ESupplyAreaType)dr.getIntN1("f_gdqhtypeid"),
            //        vl = dr.getDouble("f_dydj"),
            //        assetsProp = (AcntDataBase.EAssetsProp)dr.getIntN1("f_zcxz")

            //    };
            //    //clicktooltip
            //    obj.tooltipRightClickTemplate = "AcntTemplate";
            //    obj.tooltipRightClickContent = obj.busiAccount;


            //    //----- 拓扑数据
            //    obj.createTopoData();

            //}



            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["开闭站"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNSwitchStation obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Blue;
                obj.isH = true;
                obj.visualMinDistance = visualdistance;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.004f;
                obj.createTopoData();

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

            }


            #endregion
        }

        ///<summary>载入开关</summary>
        void loadKG()
        {
            #region 原手动方式，注释留存参考
            //string layername = EObjectCategory.开关类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);

            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from M_Switch t1 left join me_switchtype t2 on t1.f_type=t2.F_TYPE");
            //foreach (DataRow dr in dt.Rows)
            //{
            //    //Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_gx") + xdiv, -(dr.Field<double>("f_gy") + ydiv)));
            //    //Point gp = geohelper.Plane2Geo(tp);
            //    Point gp = new Point(dr.Field<double>("f_gx"), (dr.Field<double>("f_gy")));
            //    DNSwitch obj = new DNSwitch(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        color = Colors.Blue,
            //        isH = true,
            //        location = gp.ToString(),
            //        visualMaxDistance = visualdistance,
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.scaleX = obj.scaleY = obj.scaleZ = 0.0002;

            //    obj.busiData.busiSort = "中压";

            //    obj.busiAccount = new AcntSwitch()
            //    {
            //        id = obj.id,
            //        name = obj.name,
            //        belongto = dr.getString("f_bdzhmch"),
            //        isPublice = dr.getInt("f_shfgy") == 1,
            //        model = dr.getString("f_type"),
            //        planningProp = (AcntDataBase.EPlanningProp)dr.getIntN1("f_ghxzh"),
            //        price = dr.getDouble("f_price"),
            //        retireDate = dr.getDatetime("f_tynf"),
            //        runDate = dr.getDatetime("f_tyrq"),
            //        vl = dr.getDouble("f_dydj"),
            //        isInsulation = dr.getInt("f_shfjy") == 1,
            //        isNormal = dr.getInt("f_shfbzh") == 1,
            //        isOld = dr.getInt("f_shflj") == 1,
            //        mediaType = dr.getDouble("f_jzlx"),
            //        switchStatus = (ESwitchStatus)dr.getIntN1("f_zht"),
            //        switchType = (AcntDataBase.ESwitchType)dr.getIntN1("f_gnlx")
            //    };
            //    //clicktooltip
            //    obj.tooltipRightClickTemplate = "AcntTemplate";
            //    obj.tooltipRightClickContent = obj.busiAccount;

            //    //----- 拓扑数据
            //    obj.createTopoData();
            //    obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_begid") });
            //    obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_endid") });

            //    //zh注：所属的需检查
            //    //obj.thisTopoData.belongToObject = dr.getString("f_zhbid");
            //    //obj.thisTopoData.belongToFacility = dr.getString("f_bdzhid");
            //}



            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["中压开关"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNSwitch obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Blue;
                obj.isH = true;
                obj.visualMaxDistance = visualdistance;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0002f;


                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

            }


            #endregion

        }

        ///<summary>载入高压线路线段</summary>
        void loadGYXlXD()
        {
            string layername = EObjectCategory.导线类.ToString();
            pLayer containerLayer = distnet.addLayer(layername);

            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select t1.*,t2.F_BDZHMCH bstation,t2.F_MCH bbus,t3.F_BDZHMCH estation,t3.F_MCH ebus from h_line t1 left join h_busbar t2 on t1.F_BEGID=t2.F_ID  left join h_busbar t3 on t1.F_ENDID=t3.F_ID");
            foreach (DataRow dr in dt.Rows)
            {
                string strpnts = dr.Field<string>("F_EZBCH");
                string[] ss = strpnts.Split(',');
                PointCollection pc = new PointCollection();

                for (int i = 0; i < ss.Count(); i++)
                {
                    Point tmp = Point.Parse(ss[i].Replace(' ', ','));
                    Point tp = geohelper.TransformToD(new Point(tmp.X + xdiv, -(tmp.Y + ydiv)));
                    Point gp = geohelper.Plane2Geo(tp);
                    pc.Add(gp);

                }
                PointCollection newpc = checkLinePoints(pc, 0.0001);


                DNACLine obj = new DNACLine(containerLayer)
                {
                    id = dr["f_id"].ToString(),
                    name = dr["f_mch"].ToString(),
                    strPoints = newpc.ToString(),
                    color = Colors.Yellow,
                    arrowColor = Colors.Blue,
                    isFlow = false,
                    arrowSize = 0.0006f,
                    thickness = 0.0005f,
                    fromID = dr["f_begid"].ToString(),
                    toID = dr["f_endid"].ToString(),
                };
                containerLayer.AddObject(obj);

                obj.busiAccount = new AcntACLine()
                {
                    id = dr["f_id"].ToString(),
                    name = dr["f_mch"].ToString(),
                    begininfo = dr["bstation"].ToString() + dr["bbus"].ToString(),
                    endinfo = dr["estation"].ToString() + dr["ebus"].ToString(),
                    vl = double.Parse(dr["F_DYDJ"].ToString()),
                    len = double.Parse(dr["F_ALLLEN"].ToString()),

                    conductance = double.Parse(dr["F_G"].ToString()),
                    reactance = double.Parse(dr["F_X"].ToString()),
                    resistance = double.Parse(dr["F_R"].ToString()),
                    susceptance = double.Parse(dr["F_B"].ToString()),
                    normalmaxcurrent = double.Parse(dr["F_TPMAXTHERM"].ToString()),

                    planningProp = (AcntDataBase.EPlanningProp)dr.Field<short>("f_ghxzh"),
                    runDate = dr.Field<DateTime>("f_tyrq"),
                    retireDate = dr.Field<DateTime>("f_tynf"),

                };
                //clicktooltip
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                obj.busiData.busiCategory = "潮流线路";
                obj.busiData.busiSort = "高压";


                //----- 拓扑数据
                obj.createTopoData();
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_begid") });
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_endid") });


            }

        }
        ///<summary>载入中压线路线段</summary>
        void loadZYXlXD()
        {
            #region 原手动方式，注释留存参考
            //string dbopkey = "中压输电线路";

            //string layername = EObjectCategory.导线类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);

            ////DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select t1.*,t2.F_BDZHMCH bstation,t2.F_MCH bbus,t3.F_BDZHMCH estation,t3.F_MCH ebus from m_line t1 left join m_busbar t2 on t1.F_BEGID=t2.F_ID  left join h_busbar t3 on t1.F_ENDID=t3.F_ID");
            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from m_line");

            //foreach (DataRow dr in dt.Rows)
            //{
            //    string strpnts = dr.Field<string>("F_GZBCH");
            //    //string[] ss = strpnts.Split(',');
            //    string[] ss = strpnts.Split(' ');
            //    PointCollection pc = new PointCollection();

            //    for (int i = 0; i < ss.Count(); i++)
            //    {
            //        //Point tmp = Point.Parse(ss[i].Replace(' ', ','));
            //        Point tmp = Point.Parse(ss[i]);
            //        //Point tp = geohelper.TransformToD(new Point(tmp.X + xdiv, -(tmp.Y + ydiv)));
            //        //Point gp = geohelper.Plane2Geo(tp);
            //        //pc.Add(gp);
            //        pc.Add(tmp);

            //    }
            //    PointCollection newpc = checkLinePoints(pc, 0.0001);
            //    //PointCollection newpc = checkLinePoints(pc, 1);


            //    DNACLine obj = new DNACLine(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        strPoints = newpc.ToString(),
            //        lineColor = Colors.Yellow,
            //        arrowColor = Colors.Blue,
            //        isFlow = false,
            //        arrowSize = distnet.UnitMeasure * 1.2f,
            //        thickness = distnet.UnitMeasure,
            //        defaultArrowSize = distnet.UnitMeasure * 1.2f,
            //        defaultThickness = distnet.UnitMeasure,
            //        dynLineWidthEnable = true,
            //        fromID = dr["f_begid"].ToString(),
            //        toID = dr["f_endid"].ToString(),
            //        DBOPKey = dbopkey,
            //    };
            //    containerLayer.AddObject(obj);

            //    obj.busiAccount = new AcntACLine()
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        //begininfo = dr["bstation"].ToString() + dr["bbus"].ToString(),
            //        //endinfo = dr["estation"].ToString() + dr["ebus"].ToString(),
            //        begininfo = dr.getString("f_begmch"),
            //        endinfo = dr.getString("f_endmch"),
            //        vl = double.Parse(dr["F_DYDJ"].ToString()),
            //        len = double.Parse(dr["F_ALLLEN"].ToString()),
            //        branchtype = dr["F_LTYPE"].ToString(),
            //        branchlen = double.Parse(dr["F_LEN"].ToString()),
            //        branch2type = dr["F_LTYPE2"].ToString(),
            //        branch2len = double.Parse(dr["F_LEN2"].ToString()),
            //        branch3type = dr["F_LTYPE3"].ToString(),
            //        branch3len = double.Parse(dr["F_LEN3"].ToString()),

            //        conductance = double.Parse(dr["F_G"].ToString()),
            //        reactance = double.Parse(dr["F_X"].ToString()),
            //        resistance = double.Parse(dr["F_R"].ToString()),
            //        susceptance = double.Parse(dr["F_B"].ToString()),
            //        normalmaxcurrent = double.NaN,// double.Parse(dr["F_TPMAXTHERM"].ToString()),

            //        cap = 20 + rd.Next(50),

            //        planningProp = (AcntDataBase.EPlanningProp)dr.Field<short>("f_ghxzh"),
            //        runDate = dr.Field<DateTime>("f_tyrq"),
            //        retireDate = dr.Field<DateTime>("f_tynf"),
            //        supplyAreaType = AcntDataBase.ESupplyAreaType.未知,// (DNVLibrary.AcntDataBase.ESupplyAreaType)dr.Field<short>("f_gdqhtypeid"),

            //        isOptimize = false,// dr.Field<short>("F_shfcjyh") == 1,
            //        isCable = dr.getBool("F_shfdl"),
            //        isUnderground = dr.getBool("F_shfdx"),
            //        isWorking = dr.getBool("F_shfyx"),
            //    };
            //    //clicktooltip
            //    obj.tooltipRightClickTemplate = "AcntTemplate";
            //    obj.tooltipRightClickContent = obj.busiAccount;

            //    obj.busiData.busiCategory = "潮流线路";
            //    obj.busiData.busiSort = "中压";

            //    //----- 拓扑数据
            //    obj.createTopoData();
            //    obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_begid") });
            //    obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_endid") });


            //}


            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["中压输电线路"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNACLine obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                //obj.lineColor = Colors.Yellow;
                obj.arrowColor = Colors.Blue;
                obj.isFlow = false;
                //obj.arrowSize = distnet.UnitMeasure * 5 * 1.2f;
                //obj.thickness = distnet.UnitMeasure * 5;
                //obj.defaultArrowSize = distnet.UnitMeasure * 5 * 1.2f;
                //obj.defaultThickness = distnet.UnitMeasure * 5;
                obj.dynLineWidthEnable = true;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;


                cfg.Config.setValue(obj, obj.id, () => obj.color, "中压输电线路.颜色.正常");
                cfg.Config.setValue(obj, obj.id, () => obj.thickness, "中压输电线路.线宽");
                cfg.Config.setValue(obj, obj.id, () => obj.defaultThickness, "中压输电线路.线宽");
                cfg.Config.setValue(obj, obj.id, () => obj.arrowColor, "中压输电线路.箭头颜色");
                cfg.Config.setValue(obj, obj.id, () => obj.arrowSize, "中压输电线路.箭头大小");
                cfg.Config.setValue(obj, obj.id, () => obj.defaultArrowSize, "中压输电线路.箭头大小");
            }



            #endregion
        }

        ///<summary>载入高压母线</summary>
        void loadGYMX()
        {
            string layername = EObjectCategory.母线类.ToString();
            pLayer containerLayer = distnet.addLayer(layername);

            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from h_busbar");
            foreach (DataRow dr in dt.Rows)
            {
                string strpnts = dr.Field<string>("F_EZBCH");
                string[] ss = strpnts.Split(',');
                PointCollection pc = new PointCollection();
                for (int i = 0; i < ss.Count(); i++)
                {
                    Point tmp = Point.Parse(ss[i].Replace(' ', ','));
                    Point tp = geohelper.TransformToD(new Point(tmp.X + xdiv, -(tmp.Y + ydiv)));
                    Point gp = geohelper.Plane2Geo(tp);
                    pc.Add(gp);

                }

                DNBusBar obj = new DNBusBar(containerLayer)
                {
                    id = dr["f_id"].ToString(),
                    name = dr["f_mch"].ToString(),
                    strPoints = pc.ToString(),
                    color = Color.FromRgb(0xFF, 0x01, 0x00),
                    arrowColor = Colors.Blue,
                    isFlow = false,
                    thickness = 0.0002f,
                    visualMaxDistance = visualdistance
                };
                containerLayer.AddObject(obj);
                obj.busiData.busiSort = "高压";

                //----- 拓扑数据
                obj.createTopoData();

            }

        }
        ///<summary>载入中压母线</summary>
        void loadZYMX()
        {
            #region 原手动方式，注释留存参考
            //string layername = EObjectCategory.母线类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);

            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from m_busbar");
            //foreach (DataRow dr in dt.Rows)
            //{
            //    string strpnts = dr.Field<string>("F_GZBCH");
            //    string[] ss = strpnts.Split(' ');
            //    PointCollection pc = new PointCollection();
            //    for (int i = 0; i < ss.Count(); i++)
            //    {
            //        Point tmp = Point.Parse(ss[i]);
            //        //Point tp = geohelper.TransformToD(new Point(tmp.X + xdiv, -(tmp.Y + ydiv)));
            //        //Point gp = geohelper.Plane2Geo(tp);
            //        //pc.Add(gp);
            //        pc.Add(tmp);

            //    }

            //    DNBusBar obj = new DNBusBar(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        strPoints = pc.ToString(),
            //        lineColor = Color.FromRgb(0xFF, 0x01, 0x00),
            //        arrowColor = Colors.Blue,
            //        isFlow = false,
            //        thickness = 0.0002f,
            //        visualMaxDistance = visualdistance
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.busiData.busiSort = "中压";


            //    //----- 拓扑数据
            //    obj.createTopoData();
            //    obj.thisTopoData.belongToEquipmentID = new TopoObjDesc() { id = dr.getString("F_ZHBID") }; //母线所属主变，静态搜索必填，若无此数据，可通过图形位置来推断
            //}



            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["中压母线"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNBusBar obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Color.FromRgb(0xFF, 0x01, 0x00);
                obj.arrowColor = Colors.Blue;
                obj.isFlow = false;
                obj.thickness = distnet.UnitMeasure * 2;// 0.0002f;
                obj.visualMaxDistance = visualdistance;

            }
            #endregion
        }

        ///<summary>载入高压母联</summary>
        void loadGYML()
        {
            string layername = EObjectCategory.开关类.ToString();
            pLayer containerLayer = distnet.addLayer(layername);

            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from h_busbarswitch");
            foreach (DataRow dr in dt.Rows)
            {

                string strpnts = dr.Field<string>("F_EZBCH");
                string[] ss = strpnts.Split(',');
                PointCollection pc = new PointCollection();
                for (int i = 0; i < ss.Count(); i++)
                {
                    Point tmp = Point.Parse(ss[i].Replace(' ', ','));
                    Point tp = geohelper.TransformToD(new Point(tmp.X + xdiv, -(tmp.Y + ydiv)));
                    Point gp = geohelper.Plane2Geo(tp);
                    pc.Add(gp);
                }

                DNBusBarSwitch obj = new DNBusBarSwitch(containerLayer)
                {
                    id = dr["f_id"].ToString(),
                    name = dr["f_mch"].ToString(),
                    strPoints = pc.ToString(),
                    color = Colors.Lime,
                    arrowColor = Colors.Blue,
                    isFlow = false,
                    thickness = 0.0001f,//*0.01f,
                    visualMaxDistance = visualdistance
                };
                containerLayer.AddObject(obj);
                obj.busiData.busiSort = "高压";

                //----- 拓扑数据
                obj.createTopoData();
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_begid") });
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_endid") });

            }

        }
        ///<summary>载入中压母联</summary>
        void loadZYML()
        {
            #region 原手动方式，注释留存参考
            //string layername = EObjectCategory.开关类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);
            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from m_busbarswitch");
            //foreach (DataRow dr in dt.Rows)
            //{

            //    string strpnts = dr.Field<string>("F_GZBCH");
            //    string[] ss = strpnts.Split(' ');
            //    PointCollection pc = new PointCollection();
            //    for (int i = 0; i < ss.Count(); i++)
            //    {
            //        Point tmp = Point.Parse(ss[i]);
            //        //Point tp = geohelper.TransformToD(new Point(tmp.X + xdiv, -(tmp.Y + ydiv)));
            //        //Point gp = geohelper.Plane2Geo(tp);
            //        //pc.Add(gp);
            //        pc.Add(tmp);
            //    }

            //    DNBusBarSwitch obj = new DNBusBarSwitch(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        strPoints = pc.ToString(),
            //        lineColor = Colors.Lime,
            //        arrowColor = Colors.Blue,
            //        isFlow = false,
            //        thickness = 0.0001f,//*0.01f,
            //        visualMaxDistance = visualdistance
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.busiData.busiSort = "中压";

            //    obj.busiAccount = new AcntBusBarSwitch()
            //    {
            //        id = obj.id,
            //        name = obj.name,
            //        belongto = dr.getString("f_bdzhmch"),
            //        planningProp = (AcntDataBase.EPlanningProp)dr.getIntN1("f_ghxzh"),
            //        retireDate = dr.getDatetime("f_tynf"),
            //        runDate = dr.getDatetime("f_tyrq"),
            //        vl = dr.getDouble("f_dydj"),
            //        mediaType = dr.getDouble("f_jzlx"),
            //        switchStatus = (ESwitchStatus)dr.getIntN1("f_zht"),
            //        switchType = (AcntDataBase.ESwitchType)dr.getIntN1("f_gnlx")
            //    };
            //    //clicktooltip
            //    obj.tooltipRightClickTemplate = "AcntTemplate";
            //    obj.tooltipRightClickContent = obj.busiAccount;
            //    //----- 拓扑数据
            //    obj.createTopoData();
            //    obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_begid") });
            //    obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_endid") });

            //}


            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["中压母联"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNBusBarSwitch obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Lime;
                obj.arrowColor = Colors.Blue;
                obj.isFlow = false;
                obj.thickness = distnet.UnitMeasure;
                obj.visualMaxDistance = visualdistance;

                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
            }
            #endregion
        }

        ///<summary>载入高压连接线</summary>
        void loadGYLJX()
        {
            string layername = EObjectCategory.导线类.ToString();
            pLayer containerLayer = distnet.addLayer(layername);
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from H_ConnectivityLine");
            foreach (DataRow dr in dt.Rows)
            {
                string strpnts = dr.Field<string>("F_EZBCH");
                string[] ss = strpnts.Split(',');
                PointCollection pc = new PointCollection();
                for (int i = 0; i < ss.Count(); i++)
                {
                    Point tmp = Point.Parse(ss[i].Replace(' ', ','));
                    Point tp = geohelper.TransformToD(new Point(tmp.X + xdiv, -(tmp.Y + ydiv)));
                    Point gp = geohelper.Plane2Geo(tp);
                    pc.Add(gp);

                }

                DNConnectivityLine obj = new DNConnectivityLine(containerLayer)
                {
                    id = dr["f_id"].ToString(),
                    name = dr["f_mch"].ToString(),
                    strPoints = pc.ToString(),
                    color = Colors.Aqua,
                    arrowColor = Colors.Blue,
                    arrowSize = 0.0002f,
                    isFlow = false,
                    thickness = 0.0001f,
                    visualMaxDistance = visualdistance
                };
                containerLayer.AddObject(obj);
                obj.busiData.busiCategory = "潮流线路";
                obj.busiData.busiSort = "高压";

                //----- 拓扑数据
                obj.createTopoData();
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_begid") });
                obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_endid") });

            }

        }
        ///<summary>载入中压连接线</summary>
        void loadZYLJX()
        {
            #region 原手动方式，注释留存参考
            //string layername = EObjectCategory.导线类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);
            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from M_ConnectivityLine");
            //foreach (DataRow dr in dt.Rows)
            //{
            //    string strpnts = dr.Field<string>("F_GZBCH");
            //    string[] ss = strpnts.Split(' ');
            //    PointCollection pc = new PointCollection();
            //    for (int i = 0; i < ss.Count(); i++)
            //    {
            //        Point tmp = Point.Parse(ss[i]);
            //        //Point tp = geohelper.TransformToD(new Point(tmp.X + xdiv, -(tmp.Y + ydiv)));
            //        //Point gp = geohelper.Plane2Geo(tp);
            //        //pc.Add(gp);
            //        pc.Add(tmp);
            //    }
            //    PointCollection newpc = checkLinePoints(pc, 0.0001);

            //    DNConnectivityLine obj = new DNConnectivityLine(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        strPoints = newpc.ToString(),
            //        lineColor = Colors.Aqua,
            //        arrowColor = Colors.Blue,
            //        arrowSize = distnet.UnitMeasure * 1.2f,
            //        isFlow = false,
            //        thickness = distnet.UnitMeasure,
            //        visualMaxDistance = visualdistance
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.busiData.busiCategory = "潮流线路";
            //    obj.busiData.busiSort = "中压";

            //    //----- 拓扑数据
            //    obj.createTopoData();
            //    obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_begid") });
            //    obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_endid") });

            //}


            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["中压连接线"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNConnectivityLine obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {

                obj.color = Colors.Aqua;
                obj.arrowColor = Colors.Blue;
                obj.arrowSize = distnet.UnitMeasure * 1.2f;
                obj.isFlow = false;
                obj.thickness = distnet.UnitMeasure;
                obj.visualMaxDistance = visualdistance;


                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
            }
            #endregion
        }


        ///<summary>校正高压线路方向</summary>
        void ValidGyLineDir()
        {
            PowerBasicObject obj;
            pLayer mxlayer = earth.objManager.zLayers["高压母线层"];
            foreach (DNACLineBase item in earth.objManager.zLayers["高压线路线段层"].pModels.Values)
            {
                //特定fromid为母线，比较起始母线位置与线段两端距离
                if (mxlayer.pModels.TryGetValue(item.fromID, out obj))
                    item.isInverse = (obj.center - item.points.First()).Length > (obj.center - item.points.Last()).Length;

            }

        }



        ///<summary>载入高压连接点</summary>
        void loadGYLJD()
        {
            string layername = "隐藏图层";
            pLayer containerLayer = distnet.addLayer(layername);
            containerLayer.logicVisibility = false;

            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from H_ConnectivityNode");
            foreach (DataRow dr in dt.Rows)
            {
                Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_ex") + xdiv, -(dr.Field<double>("f_ey") + ydiv)));
                Point gp = geohelper.Plane2Geo(tp);
                DNConnectivityNode obj = new DNConnectivityNode(containerLayer)
                {
                    id = dr["f_id"].ToString(),
                    name = dr["f_mch"].ToString(),
                    color = Colors.Blue,
                    isH = true,
                    location = gp.ToString(),
                    visualMaxDistance = visualdistance,
                };
                containerLayer.AddObject(obj);
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0002f;
                obj.busiData.busiSort = "高压";

                //----- 拓扑数据
                obj.createTopoData();

            }

        }
        ///<summary>载入中压连接点</summary>
        void loadZYLJD()
        {
            #region 原手动方式，注释留存参考
            //string layername = "隐藏图层";
            //pLayer containerLayer = distnet.addLayer(layername);
            //containerLayer.isShow = false;

            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from M_ConnectivityNode");
            //foreach (DataRow dr in dt.Rows)
            //{
            //    //Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_ex") + xdiv, -(dr.Field<double>("f_ey") + ydiv)));
            //    //Point gp = geohelper.Plane2Geo(tp);
            //    Point gp = new Point(dr.Field<double>("f_gx"), (dr.Field<double>("f_gy")));
            //    DNConnectivityNode obj = new DNConnectivityNode(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        color = Colors.Blue,
            //        isH = true,
            //        location = gp.ToString(),
            //        visualMaxDistance = visualdistance,
            //        logicVisibility = false,
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.scaleX = obj.scaleY = obj.scaleZ = 0.0002;
            //    obj.busiData.busiSort = "中压";

            //    //----- 拓扑数据
            //    obj.createTopoData();

            //}



            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["中压连接点"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNConnectivityNode obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Blue;
                obj.isH = true;
                obj.visualMaxDistance = visualdistance;
                //obj.logicVisibility = false;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0002f;
            }
            #endregion
        }

        ///<summary>载入配电室</summary>
        void loadPDSH()
        {
            #region 原手动方式，注释留存参考
            //string dbopkey = "配电室";


            //string layername = EObjectCategory.变电设施类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);
            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from M_PowerSwitchRoom t1 left join me_pdstation_para t2 on t1.f_type=t2.f_type");
            //foreach (DataRow dr in dt.Rows)
            //{
            //    //Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_gx") + xdiv, -(dr.Field<double>("f_gy") + ydiv)));
            //    //Point gp = geohelper.Plane2Geo(tp);
            //    Point gp = new Point(dr.Field<double>("f_gx"), (dr.Field<double>("f_gy")));
            //    DNSwitchHouse obj = new DNSwitchHouse(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        color = Colors.Blue,
            //        isH = true,
            //        location = gp.ToString(),
            //        visualMinDistance = visualdistance,
            //        DBOPKey = dbopkey,
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.scaleX = obj.scaleY = obj.scaleZ = 0.005;
            //    obj.busiData.busiSort = "中压";

            //    obj.busiAccount = new AcntSwitchHouse()
            //    {
            //        id = obj.id,
            //        name = obj.name,
            //        belongto = dr.getString("f_bdzhmch"),
            //        isLinkDT = dr.getInt("f_shfjpb") == 1,
            //        model = dr.getString("f_type"),
            //        modelName = dr.getString("f_lxmch"),
            //        planningProp = (AcntDataBase.EPlanningProp)dr.getIntN1("f_ghxzh"),
            //        price = dr.getDouble("f_price"),
            //        retireDate = dr.getDatetime("f_tynf"),
            //        runDate = dr.getDatetime("f_tyrq"),
            //        supplyAreaType = (AcntDataBase.ESupplyAreaType)dr.getIntN1("f_gdqhtypeid"),
            //        vl = dr.getDouble("f_dydj"),
            //        assetsProp = (AcntDataBase.EAssetsProp)dr.getIntN1("f_zcxz"),
            //        isBoxT = dr.getInt("f_shfxshb") == 1,
            //        isSwitch = dr.getInt("f_shfkbq") == 1

            //    };
            //    containerLayer.AddObject(obj);
            //    //clicktooltip
            //    obj.tooltipRightClickTemplate = "AcntTemplate";
            //    obj.tooltipRightClickContent = obj.busiAccount;

            //    //----- 拓扑数据
            //    obj.createTopoData();

            //}




            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["配电室"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNSwitchHouse obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Blue;
                obj.isH = true;
                obj.visualMinDistance = visualdistance;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.005f;
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
            }
            #endregion
        }

        ///<summary>载入配电变压器</summary>
        void loadPDBYQ()
        {
            #region 原手动方式，注释留存参考
            //string layername = EObjectCategory.变压器类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);
            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from M_PDPowerTransformer t1 left join me_pdpowertransformer_para t2 on t1.f_type=t2.f_type");
            //foreach (DataRow dr in dt.Rows)
            //{
            //    //Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_gx") + xdiv, -(dr.Field<double>("f_gy") + ydiv)));
            //    //Point gp = geohelper.Plane2Geo(tp);
            //    Point gp = new Point(dr.Field<double>("f_gx"), (dr.Field<double>("f_gy")));
            //    DNDistTransformer obj = new DNDistTransformer(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        color = Colors.Blue,
            //        isH = true,
            //        location = gp.ToString(),
            //        visualMaxDistance = visualdistance,
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.scaleX = obj.scaleY = obj.scaleZ = 0.001;
            //    obj.busiData.busiCategory = "变压器";
            //    obj.busiData.busiSort = "中压";


            //    obj.busiAccount = new AcntDistTransformer()
            //    {
            //        id = obj.id,
            //        name = obj.name,
            //        belongto = dr.getString("f_bdzhmch"),
            //        model = dr.getString("f_type"),
            //        series = dr.getString("f_sery"),
            //        planningProp = (AcntDataBase.EPlanningProp)dr.getIntN1("f_ghxzh"),
            //        price = dr.getDouble("f_price"),
            //        retireDate = dr.getDatetime("f_tynf"),
            //        runDate = dr.getDatetime("f_tyrq"),
            //        assetsProp = (AcntDataBase.EAssetsProp)dr.getIntN1("f_zcxz"),
            //        cap = dr.getDouble("f_rl"),
            //        customerCount = dr.getInt("f_yhsh"),
            //        hnvl = dr.getDouble("f_heddy"),
            //        idlingcurrent = dr.getDouble("f_kzdl"),
            //        idlingloss = dr.getDouble("f_kzsh"),
            //        isHighLoss = dr.getInt("f_shfgsb") == 1,
            //        isNoJHJ = dr.getInt("f_shffjhj") == 1,
            //        isNormal = dr.getInt("f_shfbzh") == 1,
            //        isOld = dr.getInt("f_shflj") == 1,
            //        isPublic = dr.getInt("f_shfgb") == 1,
            //        lnvl = dr.getDouble("f_leddy"),
            //        vl = dr.getDouble("f_dydj"),
            //        rcap = dr.getDouble("f_wgrl"),
            //        shortloss = dr.getDouble("f_dlsh"),
            //        shortvl = dr.getDouble("f_dldy"),
            //        assemble = (AcntDataBase.EPDAssemble)dr.getIntN1("f_anzhxsh"),
            //    };
            //    //clicktooltip
            //    obj.tooltipRightClickTemplate = "AcntTemplate";
            //    obj.tooltipRightClickContent = obj.busiAccount;

            //    //----- 拓扑数据
            //    obj.createTopoData();
            //    obj.thisTopoData.relationObjs.Add(new TopoObjDesc() { id = dr.getString("f_ljdid") });

            //}




            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["配电变压器"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNDistTransformer obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Blue;
                obj.isH = true;
                obj.visualMaxDistance = visualdistance;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.001f;
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
            }
            #endregion
        }

        ///<summary>载入配电母线</summary>
        void loadPDMX()
        {
            #region 原手动方式，注释留存参考
            //string layername = EObjectCategory.母线类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);
            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from M_PDBusBar");
            //foreach (DataRow dr in dt.Rows)
            //{
            //    string strpnts = dr.Field<string>("F_GZBCH");
            //    string[] ss = strpnts.Split(' ');
            //    PointCollection pc = new PointCollection();
            //    for (int i = 0; i < ss.Count(); i++)
            //    {
            //        Point tmp = Point.Parse(ss[i]);
            //        //Point tp = geohelper.TransformToD(new Point(tmp.X + xdiv, -(tmp.Y + ydiv)));
            //        //Point gp = geohelper.Plane2Geo(tp);
            //        //pc.Add(gp);
            //        pc.Add(tmp);

            //    }

            //    DNBusBar obj = new DNBusBar(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        strPoints = pc.ToString(),
            //        lineColor = Color.FromRgb(0xFF, 0x01, 0x00),
            //        arrowColor = Colors.Blue,
            //        isFlow = false,
            //        thickness = 0.0002f,
            //        visualMaxDistance = visualdistance
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.busiData.busiSort = "中压";

            //    //----- 拓扑数据
            //    obj.createTopoData();
            //}


            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["配电母线"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNBusBar obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Color.FromRgb(0xFF, 0x01, 0x00);
                obj.arrowColor = Colors.Blue;
                obj.isFlow = false;
                obj.thickness = 0.0002f;
                obj.visualMaxDistance = visualdistance;

            }
            #endregion
        }


        ///<summary>载入中压节点</summary>
        void loadNode()
        {
            #region 原手动方式，注释留存参考
            //string layername = EObjectCategory.其它类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);
            ////containerLayer.isShow = false;
            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from M_Node");
            //foreach (DataRow dr in dt.Rows)
            //{
            //    //Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_gx") + xdiv, -(dr.Field<double>("f_gy") + ydiv)));
            //    //Point gp = geohelper.Plane2Geo(tp);
            //    Point gp = new Point(dr.Field<double>("f_gx"), (dr.Field<double>("f_gy")));
            //    DNNode obj = new DNNode(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        color = Colors.White,
            //        isH = true,
            //        location = gp.ToString(),
            //        //visualMaxDistance = 0,
            //        //isShow=false,
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.scaleX = obj.scaleY = obj.scaleZ = 0.0002;

            //    obj.busiAccount = new AcntNode()
            //    {
            //        id = obj.id,
            //        name = obj.name,
            //        vl = dr.getDouble("f_dydj"),

            //        busbarID = dr.getString("f_mxid"),
            //        outlineID = dr.getString("f_chxid"),
            //        substationID = dr.getString("f_bdzhid"),
            //        transformerID = dr.getString("f_zhbid"),
            //    };

            //    obj.busiData.busiCategory = "节点";
            //    obj.busiData.busiSort = "中压";

            //    //----- 拓扑数据
            //    obj.createTopoData();

            //}






            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["中压节点"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNNode obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.White;
                obj.isH = true;

                obj.scaleX = obj.scaleY = obj.scaleZ = 0.0002f;
            }
            #endregion
        }

        ///<summary>载入小区区域</summary>
        void loadXQArea()
        {
            string layername = EObjectCategory.区域类.ToString();
            pLayer containerLayer = distnet.addLayer(layername);
            //containerLayer.isShow = false;
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from H_Area");
            foreach (DataRow dr in dt.Rows)
            {
                string strpnts = dr.Field<string>("F_GZBCH");
                string[] ss = strpnts.Split(' ');
                PointCollection pc = new PointCollection();
                for (int i = 0; i < ss.Count(); i++)
                {
                    Point tmp = Point.Parse(ss[i]);
                    //Point tp = geohelper.TransformToD(new Point(tmp.X + xdiv, -(tmp.Y + ydiv)));
                    //Point gp = geohelper.Plane2Geo(tp);
                    //pc.Add(gp);
                    pc.Add(tmp);

                }

                DNArea obj = new DNArea(containerLayer)
                {
                    id = dr["f_id"].ToString(),
                    name = dr["f_mch"].ToString(),
                    strPoints = pc.ToString(),
                    color = Colors.LemonChiffon

                };
                containerLayer.AddObject(obj);
                obj.busiAccount = new AcntSmallArea()
                {
                    id = obj.id,
                    name = obj.name,
                    BH = dr.getString("f_bh"),
                    area = dr.getDouble("f_area"),
                    belongMidAreaBH = dr.getString("f_dqbh"),
                };
                //clicktooltip
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;


                //----- 拓扑数据
                obj.createTopoData();

            }


        }
        ///<summary>载入网格区域</summary>
        void loadGridArea()
        {
            string dbopkey = "网格区域";

            string layername = EObjectCategory.区域类.ToString();
            pLayer containerLayer = distnet.addLayer(layername);
            containerLayer.logicVisibility = false;
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from H_Grid t1 join d_gridareatype t2 on t1.F_TYPE=t2.gtype");
            foreach (DataRow dr in dt.Rows)
            {

                string strpnts = dr.Field<string>("F_GZBCH");
                string[] ss = strpnts.Split(' ');
                PointCollection pc = new PointCollection();
                for (int i = 0; i < ss.Count(); i++)
                {
                    Point tmp = Point.Parse(ss[i]);
                    //Point tp = geohelper.TransformToD(new Point(tmp.X + xdiv, -(tmp.Y + ydiv)));
                    //Point gp = geohelper.Plane2Geo(tp);
                    //pc.Add(gp);
                    pc.Add(tmp);

                }

                Color c = (Color)MyClassLibrary.MyColorConverter.Default.ConvertFrom(dr.getString("color"));

                DNGridArea obj = new DNGridArea(containerLayer)
                {
                    id = dr["f_id"].ToString(),
                    name = dr["f_mch"].ToString(),
                    strPoints = pc.ToString(),
                    color = c,
                    typeColor = c,
                    DBOPKey = dbopkey,
                };


                containerLayer.AddObject(obj);
                obj.busiAccount = new AcntGridArea()
                {
                    id = obj.id,
                    name = obj.name,
                    BH = dr.getString("f_bh"),
                    area = dr.getDouble("f_area"),
                    rjl = dr.getDouble("f_rjl"),
                    buildInfo = dr.getString("f_jshchd"),
                    planningDate = dr.getDatetime("f_ghshj"),
                    useType = dr.getString("f_type"),
                };
                //clicktooltip
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                //----- 拓扑数据
                obj.createTopoData();
            }


        }

        ///<summary>载入发电侧</summary>
        void loadPlant()
        {
            #region 原手动方式，注释留存参考
            //string layername = EObjectCategory.电厂设施类.ToString();
            //pLayer containerLayer = distnet.addLayer(layername);
            ////===== 光伏
            //string dbopkey = "发电侧_光伏";

            //DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select * from s_pvsource");
            //foreach (DataRow dr in dt.Rows)
            //{
            //    //Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_gx") + xdiv, -(dr.Field<double>("f_gy") + ydiv)));
            //    //Point gp = geohelper.Plane2Geo(tp);
            //    Point gp = new Point(dr.Field<double>("f_gx"), (dr.Field<double>("f_gy")));
            //    DNPVPlant obj = new DNPVPlant(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        color = Colors.Lime,
            //        isH = true,
            //        location = gp.ToString(),
            //        DBOPKey = dbopkey,
            //        //visualMinDistance = visualdistance,
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.scaleX = obj.scaleY = obj.scaleZ = 0.005;
            //    obj.busiData.busiSort = "发电侧";

            //    obj.busiAccount = new AcntPVPlant()
            //    {
            //        id = obj.id,
            //        name = obj.name,
            //        cap = dr.getDouble("f_rl"),
            //        pvType = (AcntDataBase.EPVType)dr.getIntN1("f_gftype"),
            //        utilization = dr.getDouble("f_lyl"),
            //    };
            //    containerLayer.AddObject(obj);
            //    //clicktooltip
            //    obj.tooltipRightClickTemplate = "AcntTemplate";
            //    obj.tooltipRightClickContent = obj.busiAccount;

            //    //----- 拓扑数据
            //    //obj.createTopoData();
            //}



            #endregion

            #region 数据库描述自动载入方式
            List<PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["发电侧_光伏"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNPVPlant obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Lime;
                obj.isH = true;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.005f;
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
            }
            #endregion



            //====== 风机
            #region 原手动方式，注释留存参考
            //dbopkey = "发电侧_风电";

            //dt = DataLayer.DataProvider.getDataTableFromSQL("select * from s_windsource");
            //foreach (DataRow dr in dt.Rows)
            //{
            //    //Point tp = geohelper.TransformToD(new Point(dr.Field<double>("f_gx") + xdiv, -(dr.Field<double>("f_gy") + ydiv)));
            //    //Point gp = geohelper.Plane2Geo(tp);
            //    Point gp = new Point(dr.Field<double>("f_gx"), (dr.Field<double>("f_gy")));
            //    DNWindPlant obj = new DNWindPlant(containerLayer)
            //    {
            //        id = dr["f_id"].ToString(),
            //        name = dr["f_mch"].ToString(),
            //        color = Colors.Lime,
            //        isH = true,
            //        location = gp.ToString(),
            //        DBOPKey = dbopkey,
            //        //visualMinDistance = visualdistance,
            //    };
            //    containerLayer.AddObject(obj);
            //    obj.scaleX = obj.scaleY = obj.scaleZ = 0.005;
            //    obj.busiData.busiSort = "发电侧";

            //    obj.busiAccount = new AcntWindPlant()
            //    {
            //        id = obj.id,
            //        name = obj.name,
            //        cap = dr.getDouble("f_rl"),
            //        utilization = dr.getDouble("f_lyl"),
            //    };
            //    containerLayer.AddObject(obj);
            //    //clicktooltip
            //    obj.tooltipRightClickTemplate = "AcntTemplate";
            //    obj.tooltipRightClickContent = obj.busiAccount;

            //    //----- 拓扑数据
            //    //obj.createTopoData();
            //}

            #endregion

            #region 数据库描述自动载入方式
            objs = distnet.dbdesc["基础数据"].DictSQLS["发电侧_风电"].batchCreateDNObjects(distnet); //注：方法还可附加过滤
            foreach (DNWindPlant obj in objs)  //手动补充必要的其它信息，以后如有必要，这些信息可以合并到配置工具中
            {
                obj.color = Colors.Lime;
                obj.isH = true;
                obj.scaleX = obj.scaleY = obj.scaleZ = 0.005f;
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;
            }
            #endregion

        }


        #endregion


        #region ----- 厦门载入 -----
        void loadXM()
        {
            DataLayer.DataProvider.curDataSourceName = "临时数据源";
            string sql = "select * from mmline";
            pLayer lay = distnet.addLayer("xmlayer");
            DataTable dt = DataLayer.DataProvider.getDataTable(sql, "", DataLayer.EReadMode.数据库读取).Value;
            foreach (DataRow dr in dt.Rows)
            {
                DNACLine obj = new DNACLine(lay) 
                {
                    id=dr["id"].ToString(),
                    name=dr["name"].ToString(),
                    strPoints=dr["points"].ToString(),
                    color= Colors.Red,
                    thickness=0.001f,
                };
                obj.createAcntData();
                obj.thisAcntData.id = obj.id;
                obj.thisAcntData.name = obj.name;
                obj.tooltipRightClickTemplate = "AcntTemplate";
                obj.tooltipRightClickContent = obj.busiAccount;

                lay.AddObject(obj);
            }

        }

        #endregion

        /// <summary>
        /// 线段的点距检查，以排除不合理的点
        /// </summary>
        /// <param name="pc">原始点集</param>
        /// <param name="checklen">检测点距，一般可高为线宽度</param>
        /// <returns>返回新的点集</returns>
        PointCollection checkLinePoints(PointCollection pc, double checklen)
        {
            PointCollection newpc = new PointCollection();
            newpc.Add(pc[0]);
            for (int i = 1; i < pc.Count - 1; i++)
            {
                if ((pc[i] - pc[i - 1]).Length > checklen)
                {
                    if (i < pc.Count - 1)
                    {
                        Vector v1 = pc[i] - pc[i - 1]; v1.Normalize();
                        Vector v2 = pc[i + 1] - pc[i]; v2.Normalize();
                        if (v1 != v2)
                            newpc.Add(pc[i]);
                    }
                    else
                        newpc.Add(pc[i]);
                }
            }
            newpc.Add(pc[pc.Count - 1]);
            return newpc;
        }


        ///<summary>生成图元Brush，纹理使用</summary>
        void genSymbolBrush(Brush defaultbrush, string hz)
        {
            Brush brush;



            foreach (IGrouping<string, DataRow> g in dtsymbol.AsEnumerable().GroupBy(p => p.Field<string>("svgsymbolid")))
            {
                string sid = g.Key;
                string name = g.First().Field<string>("name");

                DrawingGroup drawgroup = new DrawingGroup();
                GeometryDrawing aDrawing;


                double sizeX = 10;
                double sizeY = 10;
                int idx = 0;
                foreach (DataRow item in dtsymbol.AsEnumerable().Where(p => p.Field<string>("svgsymbolid") == sid))
                {
                    if (idx == 0)//获取尺寸
                    {
                        Rect tmp = Rect.Parse(item["viewbox"].ToString());
                        sizeX = tmp.Width;
                        sizeY = tmp.Height;
                    }


                    string shapetype = item["shapetype"].ToString();
                    string data = item["data"].ToString();
                    if (shapetype == "circle")
                    {
                        Regex regex = new Regex("(\\d*.?\\d*,\\d*.?\\d*)\\|(\\d*.?\\d*)", RegexOptions.Multiline);
                        Match m = regex.Match(data);
                        if (m.Success)
                        {
                            Point pc = Point.Parse(m.Groups[1].Value);
                            double r = double.Parse(m.Groups[2].Value);

                            EllipseGeometry geo = new EllipseGeometry(pc, r, r);
                            aDrawing = new GeometryDrawing();
                            aDrawing.Geometry = geo;

                            Pen pen = new Pen();
                            double thickness = double.Parse(item["width"].ToString());
                            thickness = thickness < 2 ? 2 : thickness;
                            pen.Thickness = thickness;
                            brush = defaultbrush;//强制缺省用黄色//brush = anaBrush(item["fill"].ToString());
                            pen.Brush = brush;
                            aDrawing.Pen = pen;
                            drawgroup.Children.Add(aDrawing);
                        }
                    }
                    else if (shapetype == "path")
                    {
                        Geometry geo = PathGeometry.Parse(data);
                        aDrawing = new GeometryDrawing();
                        aDrawing.Geometry = geo;

                        brush = defaultbrush;//强制缺省用黄色//brush = anaBrush(item["fill"].ToString());
                        aDrawing.Brush = brush;
                        Pen pen = new Pen();
                        pen.Thickness = double.Parse(item["width"].ToString());
                        brush = defaultbrush;
                        pen.Brush = brush;
                        aDrawing.Pen = pen;
                        drawgroup.Children.Add(aDrawing);
                    }
                    idx++;
                }

                DrawingBrush myDrawingBrush = new DrawingBrush();
                myDrawingBrush.Drawing = drawgroup;


                ////生成材质文件代码
                //Rectangle rect = new Rectangle() { Width = 256, Height = 256, Fill = myDrawingBrush };
                //rect.Measure(new System.Windows.Size(256, 256));
                //rect.Arrange(new Rect(0, 0, 256, 256));
                //System.IO.FileStream fs = new System.IO.FileStream("d:\\"+name+".png",System.IO.FileMode.Create);
                //RenderTargetBitmap bmp = new RenderTargetBitmap(256,256,96,96,PixelFormats.Pbgra32);
                //bmp.Render(rect);
                //BitmapEncoder encoder = new PngBitmapEncoder();
                //encoder.Frames.Add(BitmapFrame.Create(bmp));
                //encoder.Save(fs);
                //fs.Close();
                //fs.Dispose();





                pSymbol sym = new pSymbol() { id = sid + hz, sizeX = sizeX, sizeY = sizeY, brush = myDrawingBrush, name = name };
                //可选以文件生成材质, 否则以brush生成材质
                //if (sid == "SubstationEntityDisH")
                //    sym.texturefile = "SubstationEntityDisH.dds";
                //if (sid == "SwitchStationOpen")
                //    sym.texturefile = "SwitchStationOpen.dds";
                //if (sid == "Pole")
                //    sym.texturefile = "Pole.dds";

                earth.objManager.zSymbols.Add(sid + hz, sym);


            }

        }


        ///<summary>生成电动汽车所用图元, 模拟</summary>
        void genCarSymbolBrush()
        {
            GeometryDrawing aDrawing;
            Geometry geo;
            Pen pen;
            DrawingBrush myDrawingBrush;
            string name;
            Brush brush = new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x00));

            geo = new RectangleGeometry() { Rect = new Rect(0, 40, 100, 60) };
            pen = new Pen() { Thickness = 1, Brush = brush };
            aDrawing = new GeometryDrawing() { Geometry = geo, Brush = brush, Pen = pen };
            myDrawingBrush = new DrawingBrush();
            myDrawingBrush.Drawing = aDrawing;
            name = "高校、科技园区规划充电桩布点";
            earth.objManager.AddSymbol("car", name, myDrawingBrush, 64, 64);


            geo = new EllipseGeometry(new Point(50, 50), 50, 50);
            pen = new Pen() { Thickness = 1, Brush = brush };
            aDrawing = new GeometryDrawing() { Geometry = geo, Brush = brush, Pen = pen };
            myDrawingBrush = new DrawingBrush();
            myDrawingBrush.Drawing = aDrawing;
            name = "P+R停车场规划充电桩布点";
            earth.objManager.AddSymbol("car", name, myDrawingBrush, 64, 64);


            geo = PathGeometry.Parse("M 50.000000 0.000000 L 0 100 L 100 100 Z");
            pen = new Pen() { Thickness = 1, Brush = brush };
            aDrawing = new GeometryDrawing() { Geometry = geo, Brush = brush, Pen = pen };
            myDrawingBrush = new DrawingBrush();
            myDrawingBrush.Drawing = aDrawing;
            name = "社会停车场规划充电桩布点";
            earth.objManager.AddSymbol("car", name, myDrawingBrush, 64, 64);


            geo = new RectangleGeometry() { Rect = new Rect(0, 40, 100, 60) };
            pen = new Pen() { Thickness = 20, Brush = brush };
            aDrawing = new GeometryDrawing() { Geometry = geo, Brush = Brushes.Transparent, Pen = pen };
            myDrawingBrush = new DrawingBrush();
            myDrawingBrush.Drawing = aDrawing;
            name = "高速公路服务区规划充电桩布点";
            earth.objManager.AddSymbol("car", name, myDrawingBrush, 64, 64);


            geo = new EllipseGeometry(new Point(50, 50), 50, 50);
            pen = new Pen() { Thickness = 20, Brush = brush };
            aDrawing = new GeometryDrawing() { Geometry = geo, Brush = Brushes.Transparent, Pen = pen };
            myDrawingBrush = new DrawingBrush();
            myDrawingBrush.Drawing = aDrawing;
            name = "加油站规划充电桩布点";
            earth.objManager.AddSymbol("car", name, myDrawingBrush, 64, 64);


            geo = PathGeometry.Parse("M49.999998,0.5 L61.68472,38.315565 L99.499996,38.314635 L68.906276,61.68501 L80.592679,99.5 L49.999998,76.128121 L19.407317,99.5 L31.093721,61.68501 L0.49999989,38.314635 L38.315276,38.315565 z");
            pen = new Pen() { Thickness = 1, Brush = brush };
            aDrawing = new GeometryDrawing() { Geometry = geo, Brush = brush, Pen = pen };
            myDrawingBrush = new DrawingBrush();
            myDrawingBrush.Drawing = aDrawing;
            name = "电网营业厅规划充电桩布点";
            earth.objManager.AddSymbol("car", name, myDrawingBrush, 64, 64);


            geo = PathGeometry.Parse("M50.000001,0.5 L99.500002,25.25 L99.500002,74.75 L50.000001,99.5 L0.50000004,74.75 L0.50000004,25.25 z");
            pen = new Pen() { Thickness = 1, Brush = brush };
            aDrawing = new GeometryDrawing() { Geometry = geo, Brush = brush, Pen = pen };
            myDrawingBrush = new DrawingBrush();
            myDrawingBrush.Drawing = aDrawing;
            name = "4S店规划充电桩布点";
            earth.objManager.AddSymbol("car", name, myDrawingBrush, 64, 64);

            geo = new EllipseGeometry(new Point(50, 50), 50, 50);
            pen = new Pen() { Thickness = 0, Brush = Brushes.Blue };
            aDrawing = new GeometryDrawing() { Geometry = geo, Brush = new SolidColorBrush(Color.FromArgb(0x88, 0x87, 0xCE, 0xFA)), Pen = pen };
            myDrawingBrush = new DrawingBrush();
            myDrawingBrush.Drawing = aDrawing;
            name = "充电桩覆盖范围";
            earth.objManager.AddSymbol("carRange", name, myDrawingBrush, 64, 64);

        }


        #endregion



        #region ==================== 规划相关 =================================
        public static RoutedCommand CmdPlanningDateChanged = new RoutedCommand();

        #endregion



        #region 载入代码

        string curModelName, showModelName;
        BaseMain curapp, showapp;
        zMenuButton curBtn;
        double curopacity;
        System.Windows.Threading.DispatcherTimer toolboxtimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(300) };
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //使用动画，发生内存泄漏现象，原因还没查到，暂不用动画
            zMenuButton btn = sender as zMenuButton;
            if (curModelName != (sender as Button).Tag.ToString())
            {
                if (curapp != null) curapp.end();
                if (curBtn != null) curBtn.isSelected = false;

                if (showapp != null)
                {
                    curBtn = btn;
                    curBtn.isSelected = true;
                    curModelName = showModelName;
                    curapp = showapp;
                    curapp.show(curopacity, 1);
                    curapp.toolbox.IsHitTestVisible = true;
                    curapp.begin();
                    closePanel();
                }
            }




        }
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            zMenuButton btn = sender as zMenuButton;
            StackPanel panel = btn.Content as StackPanel;
            ((TextBlock)panel.Children[1]).Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x66, 0x00));

            //curopacity = curapp == null ? 1 : 0.7;
            curopacity = 1;
            if (btn.Tag.ToString() != curModelName && btn.Tag.ToString() != showModelName)
            {
                showModelName = btn.Tag.ToString();
                showapp = getNewApp(showModelName, btn);
                if (showapp != null)
                {
                    showapp.show(0, curopacity);
                    showapp.toolbox.IsHitTestVisible = false;
                }

                if (curapp != null)
                {
                    if (toolboxtimer.IsEnabled)
                    {
                        toolboxtimer.Stop();
                    }
                    else
                    {
                        curapp.hide(1, 0);
                        curapp.toolbox.IsHitTestVisible = false;
                    }
                }
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            StackPanel panel = btn.Content as StackPanel;
            ((TextBlock)panel.Children[1]).Foreground = Brushes.White;

            if (showModelName != curModelName)
            {
                if (showapp != null) showapp.hide(curopacity, 0);
                showModelName = null;
                showapp = null;
            }

            if (curapp != null && btn.Tag.ToString() != curModelName)
            {
                toolboxtimer.Start();
            }


        }
        void toolboxtimer_Tick(object sender, EventArgs e) //延时显示现工具栏
        {
            curapp.show(0, 1);
            curapp.toolbox.IsHitTestVisible = true;
            showapp = curapp;
            showModelName = curModelName;
            toolboxtimer.Stop();
        }

        ///<summary>验证模式下，检查模块授权</summary>
        void checkModelPermission()
        {
            //非验证模式下，或没有权限，隐藏模块
            if (!DataLayer.DataProvider.isAuthorize)
                mbtnUserInfo.Visibility = System.Windows.Visibility.Collapsed;
            if (!DataLayer.DataProvider.isAuthorize || (DataLayer.DataProvider.isAuthorize && !DataLayer.DataProvider.checkModelPermissionn("管理用户模块")))
                mbtnUserManage.Visibility = System.Windows.Visibility.Collapsed;
            if (!DataLayer.DataProvider.isAuthorize || (DataLayer.DataProvider.isAuthorize && !DataLayer.DataProvider.checkModelPermissionn("管理角色模块")))
                mbtnRoleManage.Visibility = System.Windows.Visibility.Collapsed;
            if (!DataLayer.DataProvider.isAuthorize || (DataLayer.DataProvider.isAuthorize && !DataLayer.DataProvider.checkModelPermissionn("管理角色模块")))
                mbtnLog.Visibility = System.Windows.Visibility.Collapsed;

        }

        BaseMain getNewApp(string appname, zMenuButton btn)
        {
            switch (appname)
            {
                case "规划_规划概览":
                    return new Planning.PAll(this, btn.head, btn.icon);
                case "规划_模拟运行":
                    return new Planning.PRun(this, btn.head, btn.icon);
                case "规划_选址定容":
                    return new Planning.PChoice(this, btn.head, btn.icon);
                case "规划_时序演进":
                    return new Planning.PTimes(this, btn.head, btn.icon);
                case "规划_分析评估":
                    return new Planning.PAnalyse(this, btn.head, btn.icon);
                case "规划_指标评价":
                    return new Planning.PEvalute(this, btn.head, btn.icon);
                case "运行_全景":
                    return new Run.RFullView(this, btn.head, btn.icon);
                case "运行_实时态":
                    return new Run.RRealTime(this, btn.head, btn.icon);
                case "运行_未来态":
                    return new Run.RFuture(this, btn.head, btn.icon);
                case "运行_历史态":
                    return new Run.RHistory(this, btn.head, btn.icon);
                case "互动_参数设置":
                    return new Interact.IParaSet(this, btn.head, btn.icon);
                case "互动_时序推演":
                    return new Interact.ITimes(this, btn.head, btn.icon);
                case "互动_网架编辑":
                    return new Interact.IEdit(this, btn.head, btn.icon);
                case "互动_指标反演":
                    return new Interact.IInvert(this, btn.head, btn.icon);
                case "互动_滚动校验":
                    return new Interact.IRollVerify(this, btn.head, btn.icon);
                case "管理_用户信息":
                    return new Manage.MUserInfo(this, btn.head, btn.icon);
                case "管理_用户管理":
                    return new Manage.MUserManage(this, btn.head, btn.icon);
                case "管理_角色管理":
                    return new Manage.MRoleManage(this, btn.head, btn.icon);
                case "管理_系统日志":
                    return new Manage.MLog(this, btn.head, btn.icon);
                case "管理_视觉配置":
                    return new Manage.MVisualConfig(this, btn.head, btn.icon);
                //case "管理_系统帮助":
                //    return new Manage.MHelp(this, btn.head, btn.icon);
            }
            return null;
        }

        #endregion


        #region 导航代码
        Vector[] locations = new Vector[7];
        Image[] imgs = new Image[4];
        DoubleAnimationUsingKeyFrames[] aniTranslatesX = new DoubleAnimationUsingKeyFrames[4];
        DoubleAnimationUsingKeyFrames[] aniTranslatesY = new DoubleAnimationUsingKeyFrames[4];
        DoubleAnimationUsingKeyFrames[] aniScales = new DoubleAnimationUsingKeyFrames[4];
        DoubleAnimation aniSubPanelX = new DoubleAnimation(); //面板内部切换
        DoubleAnimation aniPanelX = new DoubleAnimation();
        DoubleAnimation aniPanelY = new DoubleAnimation();
        TranslateTransform[] translates = new TranslateTransform[4];
        ScaleTransform[] scales = new ScaleTransform[4];
        int curimgidx = 0;
        bool isopen = true;  //菜单是否打开可见的
        private void initMainMenu()
        {
            locations[0] = new Vector(-192, 0);
            locations[1] = new Vector(-128, 0);
            locations[2] = new Vector(-64, 0);
            locations[3] = new Vector(0, 0);
            locations[4] = new Vector(0, 64);
            locations[5] = new Vector(0, 128);
            locations[6] = new Vector(0, 192);
            imgs[0] = imgPlanning;
            imgs[1] = imgRun;
            imgs[2] = imgInteraction;
            imgs[3] = imgmanager;
            for (int i = 0; i < 4; i++)
            {
                aniTranslatesX[i] = new DoubleAnimationUsingKeyFrames();
                aniTranslatesY[i] = new DoubleAnimationUsingKeyFrames();
                aniScales[i] = new DoubleAnimationUsingKeyFrames();

                scales[i] = (ScaleTransform)(imgs[i].RenderTransform as TransformGroup).Children[0];
                translates[i] = (TranslateTransform)(imgs[i].RenderTransform as TransformGroup).Children[1];
            }

            aniSubPanelX.Completed += new EventHandler(aniSubPanelX_Completed);
        }

        void aniSubPanelX_Completed(object sender, EventArgs e)
        {
            grdmainpanel.IsHitTestVisible = true;
        }

        private void ani(int devidx)
        {
            int timespan = 500;
            // 四个图标动画
            for (int i = 0; i < 4; i++)
            {
                aniTranslatesX[i].KeyFrames.Clear();
                aniTranslatesY[i].KeyFrames.Clear();
                aniScales[i].KeyFrames.Clear();
                int locationidx = (i - curimgidx) + 3;
                for (int j = 1; j <= Math.Abs(devidx); j++)
                {
                    int sign = Math.Sign(devidx);
                    aniTranslatesX[i].KeyFrames.Add(new LinearDoubleKeyFrame(locations[locationidx + sign * j].X, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(timespan * j))));
                    aniTranslatesY[i].KeyFrames.Add(new LinearDoubleKeyFrame(locations[locationidx + sign * j].Y, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(timespan * j))));
                    aniScales[i].KeyFrames.Add(new LinearDoubleKeyFrame((locationidx + sign * j) == 3 ? 0.5 : 0.35, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(timespan * j))));
                }
                translates[i].BeginAnimation(TranslateTransform.XProperty, aniTranslatesX[i]);
                translates[i].BeginAnimation(TranslateTransform.YProperty, aniTranslatesY[i]);
                scales[i].BeginAnimation(ScaleTransform.ScaleXProperty, aniScales[i]);
                scales[i].BeginAnimation(ScaleTransform.ScaleYProperty, aniScales[i]);
            }
            //面板动画
            aniSubPanelX.To = -1.0 * (curimgidx - devidx) * grdmainpanel.ActualWidth;
            aniSubPanelX.Duration = new Duration(TimeSpan.FromMilliseconds(timespan * Math.Abs(devidx)));
            grdmainpanel.IsHitTestVisible = false;
            subPanelTranslate.BeginAnimation(TranslateTransform.XProperty, aniSubPanelX);
        }

        private void img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            int selimgidx = int.Parse(img.Tag.ToString());
            if (selimgidx != curimgidx)
            {
                ani(curimgidx - selimgidx);
                curimgidx = selimgidx;
            }
            else
            {
                if (isopen)
                    closePanel();
                else
                    openPanel();
            }
        }

        private void closePanel()
        {
            int timespan = 500;
            aniPanelX.To = 0;
            aniPanelX.Duration = new Duration(TimeSpan.FromMilliseconds(timespan));
            aniPanelX.Completed += new EventHandler(aniPanelXOpen_Completed);
            grdMenu.IsHitTestVisible = false;
            panelScale.BeginAnimation(ScaleTransform.ScaleXProperty, aniPanelX);
            panelScale.BeginAnimation(ScaleTransform.ScaleYProperty, aniPanelX);

            for (int i = 0; i < 4; i++)
            {
                if (i != curimgidx)
                {
                    Panel.SetZIndex(imgs[i], 500);
                    aniTranslatesX[i].KeyFrames.Clear();
                    aniTranslatesY[i].KeyFrames.Clear();
                    aniTranslatesX[i].KeyFrames.Add(new LinearDoubleKeyFrame(locations[3].X, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(timespan))));
                    aniTranslatesY[i].KeyFrames.Add(new LinearDoubleKeyFrame(locations[3].Y, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(timespan))));
                    translates[i].BeginAnimation(TranslateTransform.XProperty, aniTranslatesX[i]);
                    translates[i].BeginAnimation(TranslateTransform.YProperty, aniTranslatesY[i]);
                }
                else
                {
                    Panel.SetZIndex(imgs[i], 600);
                    aniScales[i].KeyFrames.Clear();
                    aniScales[i].KeyFrames.Add(new LinearDoubleKeyFrame(0.35, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(timespan))));
                    scales[i].BeginAnimation(ScaleTransform.ScaleXProperty, aniScales[i]);
                    scales[i].BeginAnimation(ScaleTransform.ScaleYProperty, aniScales[i]);

                }
            }
            isopen = false;
        }

        void aniPanelXOpen_Completed(object sender, EventArgs e)
        {
            grdMenu.IsHitTestVisible = true;
            aniPanelX.Completed -= new EventHandler(aniPanelXOpen_Completed);
        }
        void aniPanelXClose_Completed(object sender, EventArgs e)
        {
            grdMenu.IsHitTestVisible = true;
            aniPanelX.Completed -= new EventHandler(aniPanelXClose_Completed);
        }
        private void openPanel()
        {
            int timespan = 500;
            aniPanelX.To = 1;
            aniPanelX.Duration = new Duration(TimeSpan.FromMilliseconds(timespan));
            aniPanelX.Completed += new EventHandler(aniPanelXClose_Completed);
            grdMenu.IsHitTestVisible = false;
            panelScale.BeginAnimation(ScaleTransform.ScaleXProperty, aniPanelX);
            panelScale.BeginAnimation(ScaleTransform.ScaleYProperty, aniPanelX);

            for (int i = 0; i < 4; i++)
            {
                if (i != curimgidx)
                {
                    aniTranslatesX[i].KeyFrames.Clear();
                    aniTranslatesY[i].KeyFrames.Clear();
                    aniTranslatesX[i].KeyFrames.Add(new LinearDoubleKeyFrame(locations[3 + i - curimgidx].X, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(timespan))));
                    aniTranslatesY[i].KeyFrames.Add(new LinearDoubleKeyFrame(locations[3 + i - curimgidx].Y, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(timespan))));
                    translates[i].BeginAnimation(TranslateTransform.XProperty, aniTranslatesX[i]);
                    translates[i].BeginAnimation(TranslateTransform.YProperty, aniTranslatesY[i]);
                }
                else
                {
                    aniScales[i].KeyFrames.Clear();
                    aniScales[i].KeyFrames.Add(new LinearDoubleKeyFrame(0.5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(timespan))));
                    scales[i].BeginAnimation(ScaleTransform.ScaleXProperty, aniScales[i]);
                    scales[i].BeginAnimation(ScaleTransform.ScaleYProperty, aniScales[i]);
                }
            }
            isopen = true;
        }


        #endregion

     



    }
}
