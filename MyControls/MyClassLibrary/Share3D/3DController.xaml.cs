using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyClassLibrary.Share3D
{
    /// <summary>
    /// 3D操作控制，支持鼠标和触摸，适用于地面形式场景
    /// 1.组件覆盖于Viewport3D之上。
    /// 2.调用程序必须初始化mainViewport3D,mainModel
    /// </summary>
    public partial class Controller3D : UserControl
    {
        public Controller3D()
        {
            InitializeComponent();
        }

        public bool _isUnloadDispose = true;
        public bool isUnloadDispose { get { return _isUnloadDispose; } set { _isUnloadDispose = value; if (controlpanel == null) controlpanel.isUnloadDispose = value; } }


        private bool _isTranslucent = true;
        [CategoryAttribute("外观"), Description("面板是否半透明")]
        ///<summary>面板是否半透明</summary>
        public bool isTranslucent
        {
            get { return _isTranslucent; }
            set { _isTranslucent = value; stackPanel.Opacity = value ? 0.3 : 1; }
        }

        ControlPanel3D controlpanel;
        private bool _isShowControlPanel = false;
        [CategoryAttribute("外观"), Description("是否显示控制部件")]
        public bool isShowControlPanel
        {
            get { return _isShowControlPanel; }
            set
            {
                _isShowControlPanel = value;
                if (value)
                {
                    controlpanel = new ControlPanel3D() { Width = 80, Height = 320 };
                    controlpanel.zoomEvent += new ControlPanel3D.zoomEventHandle(controlpanel_zoomEvent);
                    controlpanel.moveratationEvent += new ControlPanel3D.moveratationEventHandle(controlpanel_moveratationEvent);
                    stackPanel.Children.Add(controlpanel);
                }


                //controlpanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed; 
            }
        }


        public StackPanel PanelDyn { get { return stackPanelDyn; } }


        ///<summary>是否按隧道事件方式处理多点触摸事件
        ///注：由于系统未提供Manipulation类的隧道事件，故通过此属性和相关公开的多点触摸方法，模拟解决此需求
        ///当此属性为True时，控制器将不执行多点触摸处理，改由外部调用程序通过调用公开的多点触摸方法来和外部其它处理方法来执行多点触摸行为
        ///</summary>
        public bool isPreviewManipulation { get; set; }


        private double _XRotationScale = 0.8;
        ///<summary>超过此值为按X轴旋转</summary>
        public double XRotationScale
        {
            get { return _XRotationScale; }
            set { _XRotationScale = value; }
        }


        ///<summary>要操控的Viewport3D控件</summary>
        public Viewport3D mainViewport3D;
        ///<summary>主可视模型，若主要模型无附加变换，可为空</summary>
        public ModelVisual3D mainModel;
        ///<summary>mainViewport3D是否是map控件，缺省为false。true:直接替换camera的transform, false:只替换transform的matrix</summary>
        public bool isMap;


        private MatrixTransform3D cameraTransform
        { get { return (MatrixTransform3D)mainViewport3D.Camera.Transform; } }


        #region 公开事件
        public event EventHandler CameraChanged;
        protected virtual void RaiseCameraChangedEvent()
        {
            if (CameraChanged != null)
                CameraChanged(this, null);
        }

        public delegate void DoubleClickEventHandle(Object sender, MouseButtonEventArgs e);
        public event DoubleClickEventHandle DoubleClick;

        #endregion


        #region 交互控制
        private string _controlStatus = "";  //鼠标操作状态，用于确定移动或X旋转或Y旋转
        private Point _oldPoint, orgPoint;
        private Nullable<Point3D> _pcenter;
        private Nullable<Point3D> _pcenterScale;  //触摸缩放操作中心点
        private Nullable<Point3D> _pcenterRotation;  //触摸旋转操作中心点
        private double _distcenter;
        Point3D panelCenter = new Point3D(0, 0, 0); //控制板操作状态的中心点


        /// <summary>
        /// 获取屏幕点与地平面Y=0的交点
        /// </summary>
        /// <param name="p">viewport3D上的屏幕点</param>
        private void getCrossPoint3D(Point p)
        {
            Matrix3D matrix2, matrixc;
            Vector3D v1, v2;
            Vector3D raxis; double rangle;
            //v1 = new Vector3D(0, 0, -1);
            PerspectiveCamera camera = mainViewport3D.Camera as PerspectiveCamera;
            matrixc = (camera.Transform as MatrixTransform3D).Matrix;

            Point3D pline = camera.Position;
            pline = matrixc.Transform(pline);
            Vector3D vline = camera.LookDirection;
            vline = matrixc.Transform(vline);
            double angx = Math.Atan((p.X - mainViewport3D.ActualWidth / 2) / (mainViewport3D.ActualWidth / 2) * Math.Tan(camera.FieldOfView / 2 / 180 * Math.PI));
            double angy = Math.Atan((-p.Y + mainViewport3D.ActualHeight / 2) / (mainViewport3D.ActualWidth / 2) * Math.Tan(camera.FieldOfView / 2 / 180 * Math.PI));
            v1 = new Vector3D(0, 0, -1);
            v2 = new Vector3D(Math.Tan(angx), Math.Tan(angy), -1);
            raxis = Vector3D.CrossProduct(v1, v2);
            raxis.Normalize();
            raxis = matrixc.Transform(raxis);
            raxis.Normalize();
            rangle = Vector3D.AngleBetween(v1, v2);
            if (rangle != 0)
            {
                matrix2 = new Matrix3D();
                matrix2.Rotate(new Quaternion(raxis, rangle));
                vline = matrix2.Transform(vline);
                vline.Normalize();
            }
            _pcenter = Model3DHelper.calFlatXLine(new Vector3D(0, 1, 0), new Point3D(10000, 0, 10000), vline, pline);

        }

        //private void getCrossPoint3D(Point p2d)
        //{
        //    PointHitTestParameters pointparams = new PointHitTestParameters(p2d);
        //    VisualTreeHelper.HitTest(mainViewport3D, null, HTResult, pointparams);
        //}
        //private HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        //{
        //    RayHitTestResult rayResult = rawresult as RayHitTestResult;

        //    if (rayResult != null)
        //    {

        //        if (rayResult.ModelHit.Equals(groundGeometryModel))
        //        {
        //            //_pcenter = rayResult.PointHit;
        //            _pcenter = groundTransformGroup.Transform(rayResult.PointHit);
        //            _distcenter = rayResult.DistanceToRayOrigin;
        //            return HitTestResultBehavior.Stop;
        //        }
        //    }
        //    return HitTestResultBehavior.Continue;
        //}

        #region 鼠标操作事件
        private void grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _pcenter = null;
            getCrossPoint3D(e.GetPosition(mainViewport3D));
            scaleOperate(e.Delta > 0 ? 1.05 : 0.95, _pcenter);

            if (controlpanel != null)
                controlpanel.zoomValue = controlpanel.zoomValue * (e.Delta > 0 ? 0.95 : 1.05);

            e.Handled = true;
        }

        /// <summary>
        /// 显示细节回调函数
        /// </summary>
        /// <param name="rayMeshGeometryTestResult">射线测试结果</param>
        /// <param name="mousePoint">点击的屏幕坐标点</param>
        /// <returns>是否命中模型显示了要细节</returns>
        public delegate bool ShowDetail(RayMeshGeometry3DHitTestResult rayMeshGeometryTestResult, Point mousePoint);
        public ShowDetail showDetail { get; set; }

        const int clickDelta = 200;
        int mouseDownTime;
        int mouseDownCount;
        private void grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Timestamp - mouseDownTime < clickDelta && e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released) //双击
            {
                _controlStatus = "none";
                Cursor = Cursors.Arrow;
                if (DoubleClick != null)
                    DoubleClick(this, e);
            }
            else
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    Cursor = Cursors.SizeAll;
                else if (e.RightButton == MouseButtonState.Pressed)
                    Cursor = Cursors.Hand;

                //=======  注：点击，确定操作类型
                if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released)
                {
                    if (e.GetPosition(mainViewport3D).X > XRotationScale * mainViewport3D.ActualWidth)
                        _controlStatus = "rotationX";
                    else
                        _controlStatus = "move";
                }
                else if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Pressed)
                {
                    _controlStatus = "rotationY";
                }

            }
            mouseDownTime = e.Timestamp;



            //if (UCDetail.Visibility == Visibility.Visible)
            //    UCDetail.closeDetail();

            _oldPoint = e.GetPosition(mainViewport3D);
            orgPoint = _oldPoint;
        }

        private void grid_MouseLeave(object sender, MouseEventArgs e)
        {
            _controlStatus = "";
            _pcenter = null;
            Cursor = null;
        }

        private void grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _controlStatus = "";
            _pcenter = null;
            Cursor = null;

            Point newPoint = e.GetPosition(mainViewport3D);
            if (showDetail != null && (newPoint - orgPoint).Length < 5)
            {
                HitTestResult result = VisualTreeHelper.HitTest(mainViewport3D, newPoint);
                if (result is RayMeshGeometry3DHitTestResult)
                {
                    RayMeshGeometry3DHitTestResult ray3DResult = (RayMeshGeometry3DHitTestResult)result;
                    showDetail(ray3DResult, newPoint);
                }
            }

        }

        private void grid_MouseMove(object sender, MouseEventArgs e)
        {
            Point newPoint = e.GetPosition(mainViewport3D);
            _pcenter = null;

            if (_controlStatus == "move")
            {
                Point3D? porg, pobj;
                getCrossPoint3D(_oldPoint);
                porg = _pcenter;
                getCrossPoint3D(newPoint);
                pobj = _pcenter;

                if (porg != null && pobj != null)
                {
                    Vector3D v3 = (Vector3D)(porg - pobj);
                    traslateOperate(v3);
                }
            }
            else if (_controlStatus == "rotationX")
            {
                getCrossPoint3D(new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2));
                if (_pcenter != null)
                {
                    Point mp = new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2);
                    Vector vecold = _oldPoint - mp;
                    Vector vecnew = newPoint - mp;
                    double dev = Vector.AngleBetween(vecold, vecnew);
                    rotateXOperate(dev, _pcenter);
                }
            }
            else if (_controlStatus == "rotationY")
            {
                getCrossPoint3D(new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2));
                if (_pcenter != null)
                {
                    Point mp;
                    if (mainModel == null)
                        mp = (mainViewport3D.Children[0]).TransformToAncestor(mainViewport3D).Transform((Point3D)_pcenter);
                    else
                        mp = mainModel.TransformToAncestor(mainViewport3D).Transform((Point3D)_pcenter);
                    Vector vecold = _oldPoint - mp;
                    Vector vecnew = newPoint - mp;
                    double dev = Vector.AngleBetween(vecold, vecnew);
                    rotateYOperate(dev, _pcenter);
                }
            }
            else
            {
                if (showTooltips != null)
                {
                    HitTestResult result = VisualTreeHelper.HitTest(mainViewport3D, newPoint);
                    if (result is RayMeshGeometry3DHitTestResult)
                    {
                        RayMeshGeometry3DHitTestResult ray3DResult = (RayMeshGeometry3DHitTestResult)result;
                        showTooltips(ray3DResult, newPoint);
                    }
                }
            }
            _oldPoint = newPoint;
        }
        /// <summary>
        /// 显示Tooltips回调函数
        /// </summary>
        /// <param name="rayMeshGeometryTestResult">射线测试结果</param>
        /// <param name="mousePoint">鼠标移动的屏幕坐标点</param>
        /// <returns>是否命中模型显示了Tooltips</returns>
        public delegate bool ShowTooltips(RayMeshGeometry3DHitTestResult rayMeshGeometryTestResult, Point mousePoint);
        public ShowTooltips showTooltips { get; set; }

        #endregion



        #region 控制板事件
        private void controlpanel_zoomEvent(object sender, EventArgs e)
        {
            if (mainViewport3D == null) return;
            if (_pcenter == null)
                getCrossPoint3D(new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2));
            if (_pcenter == null)
                _pcenter = panelCenter;

            scaleOperate(controlpanel.zoomScale, _pcenter);
        }

        private void controlpanel_moveratationEvent(object sender, EventArgs e)
        {
            if (mainViewport3D == null) return;
            Vector vec;
            switch (controlpanel.panelStatus)
            {
                case ControlPanel3D.EPanelStatus.move:

                    Point3D? porg, pobj;
                    getCrossPoint3D(new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2));
                    porg = _pcenter;
                    getCrossPoint3D(new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2) + new Vector(controlpanel.moveVec.X, -controlpanel.moveVec.Y) * 5);
                    pobj = _pcenter;

                    if (porg != null && pobj != null)
                    {
                        Vector3D v3 = (Vector3D)(porg - pobj);
                        traslateOperate(v3);
                    }

                    break;
                case ControlPanel3D.EPanelStatus.rotation:
                    getCrossPoint3D(new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2));
                    if (_pcenter == null)
                        _pcenter = panelCenter;
                    vec = controlpanel.rotationVec;

                    vec.Normalize();

                    if (Math.Abs(vec.X) > 0.2)
                    {
                        rotateYOperate(-vec.X * 0.5, _pcenter);
                    }
                    if (Math.Abs(vec.Y) > 0.2)
                    {
                        rotateXOperate(-vec.Y * 0.5, _pcenter);
                    }
                    break;
            }

        }
        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isTranslucent)
            {
                DoubleAnimation da = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200));
                (sender as StackPanel).BeginAnimation(StackPanel.OpacityProperty, da);
            }
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isTranslucent)
            {
                DoubleAnimation da = new DoubleAnimation(0.3, TimeSpan.FromMilliseconds(200));
                (sender as StackPanel).BeginAnimation(StackPanel.OpacityProperty, da);
            }
        }
        #endregion
        #region 操作方法
        public void scaleOperate(double dev, Point3D? pcenter)
        {
            if (pcenter == null) return;
            Matrix3D matrix = cameraTransform.Matrix;
            matrix.ScaleAt(new Vector3D(dev, dev, dev), (Point3D)pcenter);
            if (isMap)
                mainViewport3D.Camera.Transform = new MatrixTransform3D(matrix);
            else
                cameraTransform.Matrix = matrix;
            RaiseCameraChangedEvent();
        }

        public void traslateOperate(Vector3D vec)
        {
            Matrix3D matrix = cameraTransform.Matrix;
            matrix.Translate(vec);
            if (isMap)
                mainViewport3D.Camera.Transform = new MatrixTransform3D(matrix);
            else
                cameraTransform.Matrix = matrix;
            RaiseCameraChangedEvent();

        }
        public void rotateXOperate(double dev, Point3D? pcenter)
        {
            if (pcenter == null) return;
            Matrix3D matrix = cameraTransform.Matrix;
            matrix.RotateAt(new Quaternion(matrix.Transform(new Vector3D(1, 0, 0)), -dev), (Point3D)pcenter);
            if (isMap)
                mainViewport3D.Camera.Transform = new MatrixTransform3D(matrix);
            else
                cameraTransform.Matrix = matrix;
            RaiseCameraChangedEvent();

        }
        public void rotateYOperate(double dev, Point3D? pcenter)
        {
            if (pcenter == null) return;
            Matrix3D matrix = cameraTransform.Matrix;
            matrix.RotateAt(new Quaternion(new Vector3D(0, 1, 0), dev), (Point3D)pcenter);
            if (isMap)
                mainViewport3D.Camera.Transform = new MatrixTransform3D(matrix);
            else
                cameraTransform.Matrix = matrix;
            RaiseCameraChangedEvent();

        }
        #endregion

        #region 多点触摸
        Point startpoint, savepoint;
        int starttime, savetime;
        int clicknum = 0;
        Rect controlbouns;
        private void grid_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            ManipulationStartingMothod(sender, e);
        }

        public void ManipulationStartingMothod(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = mainViewport3D;
            //单点操作设置
            e.IsSingleTouchEnabled = true;
            Point pc = new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2);
            //pc=mainViewport3D.TranslatePoint(pc,grdMain);
            e.Pivot = new ManipulationPivot(pc, 200);

            e.Handled = true;

        }


        private void grid_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (!isPreviewManipulation)
            {
                ManipulationStartedMothod(sender, e);
                e.Handled = true;
            }
        }
        public void ManipulationStartedMothod(object sender, ManipulationStartedEventArgs e)
        {
            //存最初触点
            startpoint = e.ManipulationOrigin;
            starttime = e.Timestamp;
            getCrossPoint3D(startpoint);
            _pcenterScale = _pcenter;
            Point pc = new Point(mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2);
            getCrossPoint3D(pc);
            _pcenterRotation = _pcenter;
            //存最初判断移动或旋转的矩形区域
            Rect rect0 = new Rect(0, 0, mainViewport3D.ActualWidth, mainViewport3D.ActualHeight);
            Rect rect1 = new Rect(mainViewport3D.ActualWidth / 4, mainViewport3D.ActualHeight / 4, mainViewport3D.ActualWidth / 2, mainViewport3D.ActualHeight / 2);
            //Rect rect2 = get3DRect();//modelMain.TransformToAncestor(mainViewport3D).TransformBounds(modelMain.Content.Bounds);
            ////中央2/4区域与模型投影的交集，若高宽小于1/4，取模型与全屏的交集，若无，取全屏
            //Rect rect3 = Rect.Inflate(rect2, -rect2.Width / 4, -rect2.Height / 4);
            //if (rect3.Width > rect1.Width)
            //    rect3.Inflate(rect1.Width / 2 - rect3.Width / 2, 0);
            //if (rect3.Height > rect1.Height)
            //    rect3.Inflate(0, rect1.Height / 2 - rect3.Height / 2);
            //if (rect3.Left > rect1.Right)
            //    rect3.Location = new Point(Math.Min(rect2.Left, rect1.Right), rect3.Top);
            //if (rect3.Top > rect1.Bottom)
            //    rect3.Location = new Point(rect3.Left, Math.Min(rect2.Top, rect1.Bottom));
            //if (rect3.Right < rect1.Left)
            //    rect3.Location = new Point(Math.Max(rect2.Right, rect1.Left) - rect2.Width, rect3.Top);
            //if (rect3.Bottom < rect1.Top)
            //    rect3.Location = new Point(rect3.Left, Math.Max(rect2.Bottom, rect1.Top) - rect3.Height);
            controlbouns = rect1;// rect3;
            e.Handled = true;
        }
        //private Rect get3DRect()  //获得3D模型的2D投影rect
        //{
        //    Rect rect = mainModel.TransformToAncestor(mainViewport3D).TransformBounds(groundGeometryModel.Bounds);
        //    //rect.Union(mainModel.TransformToAncestor(mainViewport3D).TransformBounds(mgAll.Bounds));
        //    return rect;
        //}


        private void grid_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {

            e.TranslationBehavior.DesiredDeceleration = 5.0 * 96.0 / (1000.0 * 1000.0);
            e.RotationBehavior.DesiredDeceleration = 360.0 / (1000.0 * 3000.0);
            //e.Handled = true;




        }

        private void grid_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (!isPreviewManipulation)
            {
                ManipulationDeltaMothod(sender, e);
                e.Handled = true;
            }
        }
        public void ManipulationDeltaMothod(object sender, ManipulationDeltaEventArgs e)
        {
            //===== 缩放
            double dev;
            dev = 1.0 / e.DeltaManipulation.Scale.X;
            //_pcenter = null;
            //calPCenter(startpoint);
            scaleOperate(dev, _pcenterScale);

            if (controlbouns.Contains(startpoint))
            {

                //===== 移动
                Point3D? porg, pobj;
                getCrossPoint3D(e.ManipulationOrigin);
                porg = _pcenter;
                getCrossPoint3D(e.ManipulationOrigin + e.DeltaManipulation.Translation);
                pobj = _pcenter;
                if (porg != null && pobj != null)
                {
                    Vector3D v3 = (Vector3D)(porg - pobj);
                    traslateOperate(v3);
                }
            }
            else
            {
                //===== 旋转
                dev = e.DeltaManipulation.Rotation;
                //x旋转
                if (Math.Abs(e.DeltaManipulation.Translation.X) < Math.Abs(e.DeltaManipulation.Translation.Y))
                {
                    rotateXOperate(dev, _pcenterRotation);
                }   //y旋转
                else
                {
                    rotateYOperate(dev, _pcenterRotation);
                }
            }
            e.Handled = true;
        }

        private void grid_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (!isPreviewManipulation)
            {
                ManipulationCompletedMothod(sender, e);
                e.Handled = true;
            }
        }
        public void ManipulationCompletedMothod(object sender, ManipulationCompletedEventArgs e)
        {
            //通过位置和时间判断为操作或是单击
            if (e.Timestamp - starttime < 1800000 && e.TotalManipulation.Translation.Length < 50)
            {
                if (clicknum == 0)
                    clicknum++;
                else
                    if (e.Timestamp - savetime < 20000000 && (startpoint - savepoint).Length < 30)
                        clicknum++;
                    else
                        clicknum = 0;

                savepoint = startpoint;
                savetime = e.Timestamp;
            }
            else
                clicknum = 0;
            if (clicknum == 2) //双击, 显示细节
            {
                if (showDetail != null)
                {
                    HitTestResult result = VisualTreeHelper.HitTest(mainViewport3D, startpoint);
                    if (result is RayMeshGeometry3DHitTestResult)
                    {
                        RayMeshGeometry3DHitTestResult ray3DResult = (RayMeshGeometry3DHitTestResult)result;
                        showDetail(ray3DResult, startpoint);
                    }
                    clicknum = 0;
                }
            }
            e.Handled = true;
        }
        #endregion

        #endregion 交互控制

        /// <summary>
        /// 在控制面板下方，附加其它操作控件
        /// </summary>
        /// <param name="control"></param>
        public void addControl(UIElement control)
        {
            stackPanel.Children.Add(control);
        }

        ///<summary>清除所有固定控件</summary>
        public void ClearaddControl()
        {
            stackPanel.Children.Clear();
        }


        ///<summary>附加可移除控件</summary>
        public void addDynControl(UIElement control)
        {
            stackPanelDyn.Children.Add(control);
        }

        ///<summary>清除所有可移除控件</summary>
        public void clearDynControl()
        {
            stackPanelDyn.Children.Clear();
        }



    }
}
