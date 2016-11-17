using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using MyClassLibrary.Share2D;


namespace MyClassLibrary.Share3D
{
    #region 辅助定义
    public enum EModelType { none, 常规园柱, 平滑园柱, 常规方柱, 平滑方柱, 圆锥, 标签, 堆叠园柱, 堆叠方柱, 堆叠园锥, 饼 }  // 单体模型
    public enum EModelLinkMode { 无, 置于上, 置于下, 置于左, 置于右, 置于前, 置于后 }
    public enum EValueType { X, Y, Z, SizeX, SizeY, SizeZ, Color } //值对应3D特征的类型
    public enum ELabelDirection { 垂直, 水平 }
    /// <summary>
    /// 颜色图例结构，名称->颜色
    /// </summary>
    //public struct StruLegendColorToName
    //{
    //    public StruLegendColorToName(string name, Brush brush)
    //    {
    //        Lname = name; Lbrush = brush; Lvalue = 0.00001;
    //    }
    //    public StruLegendColorToName(string name, Brush brush, double value)
    //    {
    //        Lname = name; Lbrush = brush; Lvalue = value;
    //    }
    //    public string Lname;
    //    public Brush Lbrush;
    //    public double Lvalue; //不明确意义的附加值，可用于设定组合柱的高度等
    //}
    #endregion


    public static class myModelBuilder
    {
        /// <summary>
        /// 创建模型
        /// </summary>
        /// <param name="mtype">模型类型</param>
        /// <param name="LstGroupLabel">组合模型的名称</param>
        /// <returns></returns>
        public static ModelBase CreateModel(ModelSerial serial, EModelType mtype, Material mat, Dictionary<string, Brush> LstGroupLabel)
        {
            serial.Material = mat;
            serial.LstGroupLabel = LstGroupLabel; //组模型，置构成组的成员的标签名称
            serial.modelType = mtype;
            return serial.addNewModel();
        }
    }


    #region 场景类
    /// <summary>
    /// 场景: 
    /// 1.Limit3D定义3D限定参数，用于值转换为3D度量。 XYZ：限定空间最大长宽高；SizeXYZ：限定模型ModelBase的最大长宽高
    /// 2.addNewSerial增加系列
    /// </summary>
    public class Scene3D : DependencyObject
    {
        /// <summary>3D限定参数，用于值转换为3D度量：XYZ：限定空间最大长宽高；SizeXYZ：限定模型ModelBase的最大长宽高</summary>
        public Rect3D Limit3D = new Rect3D(15, 10, 10, 1, 2, 1);  //内容场景的区域范围


        #region 场景全局控制属性
        ///<summary>是否显示模型顶部标签</summary>
        public bool isShowLabel { get; set; }

        ///<summary>值变化时是否动画改变模型</summary>
        public bool isAniModel { get; set; }

        ///<summary>是否显示现实模型</summary>
        public bool isShowRealModel { get; set; }

        #endregion



        //public double baseScale = 1; //全局比例
        public static DependencyProperty BaseScaleProperty = DependencyProperty.Register("BaseScale", typeof(double), typeof(Scene3D), new PropertyMetadata(1.0, new PropertyChangedCallback(OnBaseScaleChanged)));
        public double BaseScale
        {
            get { return (double)GetValue(BaseScaleProperty); }
            set
            {
                SetValue(BaseScaleProperty, value);
            }
        }
        internal static void OnBaseScaleChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            Scene3D scene = sender as Scene3D;
            //m.changeBaseScale((double)e.NewValue, (double)e.OldValue, e.Property);
            foreach (ModelSerial s in scene.serials)
            {
                foreach (ModelBase m in s.models)
                {
                    if (m.model != null)
                    {
                        Matrix3D matrix = (m.model.Transform as MatrixTransform3D).Matrix;
                        double scale = (double)e.NewValue / (double)e.OldValue;
                        matrix.ScaleAtPrepend(new Vector3D(scale, scale, scale), m.controlPoint);
                        (m.model.Transform as MatrixTransform3D).Matrix = matrix;
                    }
                }
            }
        }



        //系列
        public List<ModelSerial> serials = new List<ModelSerial>();

        /// <summary>
        /// 添加系列
        /// </summary>
        /// <returns>返回添加的系列</returns>
        public ModelSerial addNewSerial()
        {
            ModelSerial s = new ModelSerial(this);
            s.isShowLabel = isShowLabel;
            serials.Add(s);
            return s;
        }

        #region 公开方法
        public List<ModelBase> findModel(string id)
        {
            List<ModelBase> result = new List<ModelBase>();
            foreach (ModelSerial e1 in serials)
            {
                foreach (ModelBase e2 in e1.models)
                {
                    if (e2.id == id)
                    {
                        result.Add(e2);
                        break;
                    }
                }

            }
            return result;
        }



        #endregion
    }
    #endregion


    #region 模型系列类
    /// <summary>
    /// 模型系列
    /// 1.须指定模型类型、对齐方式、材质方式
    /// 2.可定义系列的各可视化特性（位置，大小，材质）对应的最大和起始数值（缺省为0）
    /// 3.用addNewModel添加模型
    /// 4.材质可以有固定模式、值模式，值模式可能需设定颜色属性和变化模式属性
    /// 5.组合类型类，需先设置组合的类型和子模型数目
    /// 6.模型标签机制：由系列的LstLabel列表控制要显示的对应3D特征的值的标签，若为空，不显示标签
    /// </summary>
    public class ModelSerial
    {
        public ModelSerial(Scene3D Scene)
        {
            scene = Scene;
        }
        public Scene3D scene;

        //===== 系列模型共用的模型特征映射的数据最大值和起始值
        public double minXValue = 0;
        public double maxXValue = 1;
        public double minYValue = 0;
        public double maxYValue = 1;
        public double minZValue = 0;
        public double maxZValue = 1;
        public double minSizeXValue = 0;
        public double maxSizeXValue = 1;
        public double minSizeYValue = 0;
        public double maxSizeYValue = 5;
        public double minSizeZValue = 0;
        public double maxSizeZValue = 1;
        public double minColorValue = 0;
        public double maxColorValue = 1;
        //===== 度量变换比例
        public double calscaleX { get { return (maxXValue - minXValue) / scene.Limit3D.X; } }
        public double calscaleY { get { return (maxYValue - minYValue) / scene.Limit3D.Y; } }
        public double calscaleZ { get { return (maxZValue - minZValue) / scene.Limit3D.Z; } }
        public double calscaleSizeX { get { return (maxSizeXValue - minSizeXValue) / scene.Limit3D.SizeX; } }
        public double calscaleSizeY { get { return (maxSizeYValue - minSizeYValue) / scene.Limit3D.SizeY; } }
        public double calscaleSizeZ { get { return (maxSizeZValue - minSizeZValue) / scene.Limit3D.SizeZ; } }

        //=====标签相关字段
        public string descX = "";
        public string descY = "";
        public string descZ = "";
        public string descSizeX = "";
        public string descSizeY = "";
        public string descSizeZ = "";
        public string descColor = "";
        public string LabelTemplate = "{SizeY}";  //标签模板
        public bool isShowLabel = false; //是否显示标签
        public ELabelDirection LabelDirection = ELabelDirection.垂直;
        public double LabelHeight = 0.05;
        public Brush LabelColor = new SolidColorBrush(Color.FromRgb(0x3F, 0x6B, 0xFF));
        public bool isGlow = false;
        ///<summary>单体模型，标签要显示的内容</summary>
        public List<EValueType> LstLabel = new List<EValueType>();
        ///<summary>组模型，置构成组的成员的标签名称和初始画刷 </summary>
        public Dictionary<string, Brush> LstGroupLabel = new Dictionary<string, Brush>();
        //=====系列特性字段
        public string SerialName = "";  //系列名称

        //=====系列模型参数字段
        //与groupModelType互斥，表示系列模型为单体模型
        EModelType _ModelType = EModelType.常规园柱;
        //EGroupModelType _GroupModelType = EGroupModelType.none;
        public EModelType modelType
        {
            get { return _ModelType; }
            set { _ModelType = value; }// _GroupModelType = EGroupModelType.none; }
        }
        //public EGroupModelType groupModelType
        //{
        //    get { return _GroupModelType; }
        //    set { _GroupModelType = value; _ModelType = EModelType.none; }
        //}
        public int groupModelCount { get { return LstGroupLabel.Count; } }
        //public EModelAlign mAlign;
        public Vector3D mAlign = new Vector3D(0.5, 0, 0.5);
        public EMaterialValueToColorMode matColorMode = EMaterialValueToColorMode.纯色; //变化色的模式
        public Color color = Colors.Blue; //变化色相或透明度时的原色
        public Material Material = My3DHelper.getSolidMaterial(EMaterailColor.红, EMaterialColorDeep.浅色);  //指定的材质

        //===== 系列模型列表
        public List<ModelBase> models = new List<ModelBase>();

        public Model3DGroup mgModel = new Model3DGroup();
        public Model3DGroup mgLabel = new Model3DGroup();
        public Model3DGroup mgRealModel = new Model3DGroup();

        //===== 公开方法
        public ModelBase addNewModel()
        {
            ModelBase model = CreateModel();
            models.Add(model);
            return model;
        }
        internal ModelBase CreateModel()
        {
            ModelBase model = null;
            switch (modelType)
            {
                case EModelType.常规园柱:
                    model = new MCylinder(this);
                    break;
                case EModelType.平滑园柱:
                    model = new MSmoothCylinder(this);
                    break;
                case EModelType.常规方柱:
                    model = new MCube(this);
                    break;
                case EModelType.平滑方柱:
                    model = new MSmoothCube(this);
                    break;
                case EModelType.圆锥:
                    model = new MCone(this);
                    break;
                case EModelType.堆叠园柱:
                    model = new MGroupCylinder(this);
                    break;
                case EModelType.堆叠方柱:
                    model = new MGroupCube(this);
                    break;
                case EModelType.堆叠园锥:
                    model = new MGroupCone(this);
                    break;
                case EModelType.饼:
                    model = new MGroupPie(this);
                    break;
            }

            if (matColorMode == EMaterialValueToColorMode.纯色)
            {
                model.Material = this.Material;
            }
            return model;
        }


    }
    #endregion


    #region 模型类
    /// <summary>
    /// 模型基类
    /// 1.x y z sizeX sizeY sizeZ colorValue 可对应实际数据，与系列设定的极值和场景设定的Limit转换为3D度量
    /// 2.x3d...sizeZ3D直接按3D度量
    /// 3.设置LinkModel（先）和LinkMode（后）连接模型和连接模式，可创建堆叠效果。
    /// </summary>
    public abstract class ModelBase : FrameworkElement
    {
        public ModelBase()
        {
            //    serial = myModelBuilder.serial;
            //    mAlign = serial.mAlign;
            //    Material = serial.Material; 
        }
        public ModelBase(ModelSerial Serial)
        {
            serial = Serial;
            mAlign = serial.mAlign;
            Material = serial.Material;
            //bind(object src, DependencyObject obj, DependencyProperty srcp, DependencyProperty objp, Type converttype, object convertpara, BindingMode bindmode)
            OperateHelper.bind(this, ModelBase.XProperty, this, ModelBase.X3DProperty, typeof(ValueTo3DConverter), serial.calscaleX, BindingMode.TwoWay);
            OperateHelper.bind(this, ModelBase.YProperty, this, ModelBase.Y3DProperty, typeof(ValueTo3DConverter), serial.calscaleY, BindingMode.TwoWay);
            OperateHelper.bind(this, ModelBase.ZProperty, this, ModelBase.Z3DProperty, typeof(ValueTo3DConverter), serial.calscaleZ, BindingMode.TwoWay);
            OperateHelper.bind(this, ModelBase.SizeXProperty, this, ModelBase.SizeX3DProperty, typeof(ValueTo3DConverter), serial.calscaleSizeX, BindingMode.TwoWay);
            OperateHelper.bind(this, ModelBase.SizeYProperty, this, ModelBase.SizeY3DProperty, typeof(ValueTo3DConverter), serial.calscaleSizeY, BindingMode.TwoWay);
            OperateHelper.bind(this, ModelBase.SizeZProperty, this, ModelBase.SizeZ3DProperty, typeof(ValueTo3DConverter), serial.calscaleSizeZ, BindingMode.TwoWay);
        }
        //===== 变换
        public ModelSerial serial;
        public Transform3D transform; //附加的变换
        public Vector3D translate; //初始的位移
        public Vector3D mAlign;
        //===== 内部使用字段和属性
        protected MatrixTransform3D allTransform = new MatrixTransform3D(); //模型的变换
        Point3D? _controlPoint = null;
        internal Point3D controlPoint  //原始控制点
        {
            get
            {
                if (_controlPoint == null)
                {
                    Point3D p = new Point3D();
                    p.X = model.Bounds.X + model.Bounds.SizeX * mAlign.X;
                    p.Y = model.Bounds.Y + model.Bounds.SizeY * mAlign.Y;
                    p.Z = model.Bounds.Z + model.Bounds.SizeZ * mAlign.Z;
                    _controlPoint = p;
                }
                return (Point3D)_controlPoint;
            }
        }
        protected bool isSingle;
        //===== 为model提供附加属性
        public static readonly DependencyProperty TagProperty = DependencyProperty.Register("Tag", typeof(object), typeof(ModelBase));
        #region 位置属性
        //===== *位置X
        public static DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(ModelBase), new PropertyMetadata(0.0));
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }
        public static DependencyProperty X3DProperty = DependencyProperty.Register("X3D", typeof(double), typeof(ModelBase), new PropertyMetadata(0.0, new PropertyChangedCallback(OnLocationChanged)));
        public double X3D
        {
            get { return (double)GetValue(X3DProperty); }
            set { SetValue(X3DProperty, value); }
        }
        //===== *位置Y
        public static DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(ModelBase), new PropertyMetadata(0.0));
        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }
        public static DependencyProperty Y3DProperty = DependencyProperty.Register("Y3D", typeof(double), typeof(ModelBase), new PropertyMetadata(0.0, new PropertyChangedCallback(OnLocationChanged)));
        public double Y3D
        {
            get { return (double)GetValue(Y3DProperty); }
            set { SetValue(Y3DProperty, value); }
        }
        //===== *位置Z
        public static DependencyProperty ZProperty = DependencyProperty.Register("Z", typeof(double), typeof(ModelBase), new PropertyMetadata(0.0));
        public double Z
        {
            get { return (double)GetValue(ZProperty); }
            set { SetValue(ZProperty, value); }
        }
        public static DependencyProperty Z3DProperty = DependencyProperty.Register("Z3D", typeof(double), typeof(ModelBase), new PropertyMetadata(0.0, new PropertyChangedCallback(OnLocationChanged)));
        public double Z3D
        {
            get { return (double)GetValue(Z3DProperty); }
            set { SetValue(Z3DProperty, value); }
        }
        internal static void OnLocationChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            ModelBase m = sender as ModelBase;
            Vector3D vec = new Vector3D(0, 0, 0);
            Matrix3D matrix = m.allTransform.Matrix;
            switch (e.Property.Name)
            {
                case "X3D":
                    //vec.X = (double)e.NewValue - (double)e.OldValue;
                    //matrix.Translate(vec);
                    matrix.OffsetX = (double)e.NewValue;
                    break;
                case "Y3D":
                    //vec.Y = (double)e.NewValue - (double)e.OldValue;
                    //matrix.Translate(vec);
                    matrix.OffsetY = (double)e.NewValue;
                    break;
                case "Z3D":
                    //vec.Z = (double)e.NewValue - (double)e.OldValue;
                    //matrix.Translate(vec);
                    matrix.OffsetZ = (double)e.NewValue;
                    break;
            }
            m.allTransform.Matrix = matrix;
        }
        #endregion
        #region 大小属性
        //===== *X方向长度
        public static DependencyProperty SizeXProperty = DependencyProperty.Register("SizeX", typeof(double), typeof(ModelBase), new PropertyMetadata(-1.0));
        public double SizeX
        {
            get { return (double)GetValue(SizeXProperty); }
            set { SetValue(SizeXProperty, value); }
        }
        public static DependencyProperty SizeX3DProperty = DependencyProperty.Register("SizeX3D", typeof(double), typeof(ModelBase), new PropertyMetadata(-1.0, new PropertyChangedCallback(OnSizeChanged)));
        public double SizeX3D
        {
            get { return (double)GetValue(SizeX3DProperty); }
            set { SetValue(SizeX3DProperty, value); }
        }
        //===== *Y方向长度
        public static DependencyProperty SizeYProperty = DependencyProperty.Register("SizeY", typeof(double), typeof(ModelBase), new PropertyMetadata(-1.0));
        public double SizeY
        {
            get { return (double)GetValue(SizeYProperty); }
            set
            {
                if (value == 0)
                {
                    if (serial.scene.isAniModel)
                        aniSetSizeY(0.000001);
                    SetValue(SizeYProperty, 0.000001);
                }
                else
                {
                    if (serial.scene.isAniModel)
                        aniSetSizeY(Math.Abs(value));
                    SetValue(SizeYProperty, Math.Abs(value));
                }

            }
        }
        public static DependencyProperty SizeY3DProperty = DependencyProperty.Register("SizeY3D", typeof(double), typeof(ModelBase), new PropertyMetadata(-1.0, new PropertyChangedCallback(OnSizeChanged)));
        public double SizeY3D
        {
            get { return (double)GetValue(SizeY3DProperty); }
            set { SetValue(SizeY3DProperty, value); }
        }
        //===== *Z方向长度
        public static DependencyProperty SizeZProperty = DependencyProperty.Register("SizeZ", typeof(double), typeof(ModelBase), new PropertyMetadata(-1.0));
        public double SizeZ
        {
            get { return (double)GetValue(SizeZProperty); }
            set { SetValue(SizeZProperty, value); }
        }
        public static DependencyProperty SizeZ3DProperty = DependencyProperty.Register("SizeZ3D", typeof(double), typeof(ModelBase), new PropertyMetadata(-1.0, new PropertyChangedCallback(OnSizeChanged)));
        public double SizeZ3D
        {
            get { return (double)GetValue(SizeZ3DProperty); }
            set { SetValue(SizeZ3DProperty, value); }
        }
        internal static void OnSizeChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            ModelBase m = sender as ModelBase;
            m.changeSize((double)e.NewValue, (double)e.OldValue, e.Property);
        }
        protected internal virtual void changeSize(double newValue, double oldValue, DependencyProperty dp)
        {
            if (model != null)
            {
                //if (model.Bounds.SizeX == 0 || model.Bounds.SizeY == 0 || model.Bounds.SizeZ == 0) //若长宽高之一为0，按新值重新生成matrix
                //{
                //    Matrix3D matrix = Matrix3D.Identity;
                //    Vector3D vec;
                //    vec = new Vector3D(SizeX3D > 0 ? SizeX3D : 1, SizeY3D > 0 ? SizeY3D : 1, SizeZ3D > 0 ? SizeZ3D : 1);
                //    matrix.ScaleAtPrepend(vec, controlPoint);
                //    vec = new Vector3D(X3D, Y3D, Z3D);
                //    matrix.Translate(vec);
                //    allTransform.Matrix = matrix;
                //}
                //else
                {
                    Vector3D vec = new Vector3D(1, 1, 1);
                    switch (dp.Name)
                    {
                        case "SizeX3D":
                            if (oldValue <= 0)
                                vec.X = newValue;
                            else
                                vec.X = newValue / oldValue;
                            break;
                        case "SizeY3D":
                            if (oldValue <= 0)
                                vec.Y = newValue;
                            else
                                vec.Y = newValue / oldValue;

                            break;
                        case "SizeZ3D":
                            if (oldValue <= 0)
                                vec.Z = newValue;
                            else
                                vec.Z = newValue / oldValue;
                            break;
                    }
                    Matrix3D matrix = allTransform.Matrix;
                    matrix.ScaleAtPrepend(vec, controlPoint);
                    allTransform.Matrix = matrix;
                }
            }
        }
        #endregion
        #region 材质属性
        //直接赋材质
        public static DependencyProperty MaterialProperty = DependencyProperty.Register("Material", typeof(Material), typeof(ModelBase), new PropertyMetadata(null, new PropertyChangedCallback(OnMaterialChanged)));
        public Material Material
        {
            get { return (Material)GetValue(MaterialProperty); }
            set { SetValue(MaterialProperty, value); }
        }
        internal static void OnMaterialChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            ModelBase m = sender as ModelBase;
            if (m.model is Model3DGroup)
            {
                foreach (GeometryModel3D one in (m.model as Model3DGroup).Children)
                {
                    one.Material = m.Material;
                }
            }
            else if (m.model is GeometryModel3D)
            {
                (m.model as GeometryModel3D).Material = m.Material;
            }
        }
        //值计算材质
        //===== *材质颜色属性
        public static DependencyProperty ColorValueProperty = DependencyProperty.Register("ColorValue", typeof(double), typeof(ModelBase), new PropertyMetadata(-1.0, new PropertyChangedCallback(OnColorValueChanged)));
        public double ColorValue
        {
            get { return (double)GetValue(ColorValueProperty); }
            set { SetValue(ColorValueProperty, value); }
        }
        internal static void OnColorValueChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            ModelBase m = sender as ModelBase;
            m.Material = My3DHelper.getValueColorMaterial(m.serial.minColorValue, m.serial.maxColorValue, m.ColorValue, m.serial.matColorMode, m.serial.color);
        }
        #endregion
        #region 连接属性
        public static DependencyProperty LinkModeProperty = DependencyProperty.Register("LinkMode", typeof(EModelLinkMode), typeof(ModelBase), new PropertyMetadata(EModelLinkMode.无, new PropertyChangedCallback(OnLinkModeChanged)));
        public EModelLinkMode LinkMode
        {
            get { return (EModelLinkMode)GetValue(LinkModeProperty); }
            set { SetValue(LinkModeProperty, value); }
        }
        internal static void OnLinkModeChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            ModelBase m = sender as ModelBase;
            EModelLinkMode linkmode = (EModelLinkMode)e.NewValue;
            if (linkmode == EModelLinkMode.置于上 || linkmode == EModelLinkMode.置于下)
            {
                OperateHelper.bind(m.LinkModel, ModelBase.X3DProperty, m, ModelBase.X3DProperty);
                OperateHelper.bind(m.LinkModel, ModelBase.Z3DProperty, m, ModelBase.Z3DProperty);
                m.bindlink(m.LinkModel, ModelBase.Y3DProperty, ModelBase.SizeY3DProperty, m, ModelBase.Y3DProperty, linkmode);
            }
            else if (linkmode == EModelLinkMode.置于左 || linkmode == EModelLinkMode.置于右)
            {
                OperateHelper.bind(m.LinkModel, ModelBase.Y3DProperty, m, ModelBase.Y3DProperty);
                OperateHelper.bind(m.LinkModel, ModelBase.Z3DProperty, m, ModelBase.Z3DProperty);
                m.bindlink(m.LinkModel, ModelBase.X3DProperty, ModelBase.SizeX3DProperty, m, ModelBase.X3DProperty, linkmode);
            }
            else if (linkmode == EModelLinkMode.置于前 || linkmode == EModelLinkMode.置于后)
            {
                OperateHelper.bind(m.LinkModel, ModelBase.Y3DProperty, m, ModelBase.Y3DProperty);
                OperateHelper.bind(m.LinkModel, ModelBase.X3DProperty, m, ModelBase.X3DProperty);
                m.bindlink(m.LinkModel, ModelBase.Z3DProperty, ModelBase.SizeZ3DProperty, m, ModelBase.Z3DProperty, linkmode);
            }
        }
        internal void clearLinkBinding()
        {
            BindingOperations.ClearBinding(this, ModelBase.X3DProperty);
            BindingOperations.ClearBinding(this, ModelBase.Y3DProperty);
            BindingOperations.ClearBinding(this, ModelBase.Z3DProperty);
        }

        private void bindlink(object src, DependencyProperty srcLocationP, DependencyProperty srcSizeP, DependencyObject obj, DependencyProperty objp, EModelLinkMode linkmode)
        {
            MultiBinding mbind = new MultiBinding();
            mbind.Converter = new LinkConverter();
            List<object> paras = new List<object>();
            paras.Add(linkmode);
            paras.Add(src);
            paras.Add(obj);
            mbind.ConverterParameter = paras;
            mbind.Mode = BindingMode.OneWay;

            Binding bind;
            bind = new Binding();
            bind.Source = src;
            bind.Path = new PropertyPath(srcLocationP);
            bind.Mode = BindingMode.OneWay;
            mbind.Bindings.Add(bind);

            bind = new Binding();
            bind.Source = src;
            bind.Path = new PropertyPath(srcSizeP);
            bind.Mode = BindingMode.OneWay;
            mbind.Bindings.Add(bind);

            BindingOperations.SetBinding(obj, objp, mbind);
        }

        public ModelBase LinkModel = null;
        #endregion

        #region 描述属性
        public string id { get; set; }
        public string name { get; set; }
        public string sort { get; set; }
        public double mainValue { get; set; }

    
        #endregion

        //===== 可视属性字段
        public Model3D model;
        public Model3D label
        {
            get
            {
                if (this is MSingleBase)
                    return (this as MSingleBase).LabelModel.model;
                else if (this is GroupModelBase)
                    return (this as GroupModelBase).LabelModel.model;
                else
                    return null;
            }
        }

        //===== 供模型初始化时调用的方法
        protected void normalizeModel() //规范化模型为bound长宽高均为1, 并对齐
        {
            if (model != null)
            {
                //缩放到`1
                Vector3D vec = new Vector3D(1 / model.Bounds.SizeX, 1 / model.Bounds.SizeY, 1 / model.Bounds.SizeZ);
                Matrix3D matrix = allTransform.Matrix;
                matrix.ScaleAtPrepend(vec, controlPoint);
                allTransform.Matrix = matrix;
                doAlign();
            }
        }
        protected void doAlign()   //初始化模型，控制点对齐到原点，并记录下原始位移 
        {
            if (model != null)
            {
                Vector3D vec = new Vector3D(-(model.Bounds.X + model.Bounds.SizeX * mAlign.X), -(model.Bounds.Y + model.Bounds.SizeY * mAlign.Y), -(model.Bounds.Z + model.Bounds.SizeZ * mAlign.Z));
                Matrix3D matrix = allTransform.Matrix;
                matrix.Translate(vec);
                allTransform.Matrix = matrix;
            }
        }

        internal Rect3D initBound;//模型变换前的初始Bound

        //===== 私有方法
        /// <summary>
        /// 强制设置尺寸位置
        /// </summary>
        /// <param name="newvalue">强制设置的3D度量值</param>
        /// <param name="point">变换控制点</param>
        void ForceSet(EValueType flag, double newvalue, Point3D point)
        {
            Matrix3D matrix = allTransform.Matrix;
            Vector3D vec;
            switch (flag)
            {
                case EValueType.X:
                    vec = new Vector3D(-point.X + newvalue, 0, 0);
                    matrix.Translate(vec);
                    break;
                case EValueType.Y:
                    vec = new Vector3D(0, -point.Y + newvalue, 0);
                    matrix.Translate(vec);
                    break;
                case EValueType.Z:
                    vec = new Vector3D(0, 0, -point.Z + newvalue);
                    matrix.Translate(vec);
                    break;
                case EValueType.SizeX:
                    vec = new Vector3D(newvalue / model.Bounds.SizeX, 1, 1);
                    matrix.ScaleAt(vec, point);
                    break;
                case EValueType.SizeY:
                    vec = new Vector3D(1, newvalue / model.Bounds.SizeY, 1);
                    matrix.ScaleAt(vec, point);
                    break;
                case EValueType.SizeZ:
                    vec = new Vector3D(1, 1, newvalue / model.Bounds.SizeZ);
                    matrix.ScaleAt(vec, point);
                    break;
            }

            allTransform.Matrix = matrix;
        }

        //===== 公开方法
        public void Rotate(Vector3D axisOfRotation, double angleInDegrees)
        {
            Matrix3D matrix = allTransform.Matrix;
            Quaternion q = new Quaternion(axisOfRotation, angleInDegrees);
            matrix.RotateAt(q, controlPoint);
            allTransform.Matrix = matrix;
        }
        public void RotatePrepend(Vector3D axisOfRotation, double angleInDegrees)
        {
            Matrix3D matrix = allTransform.Matrix;
            Quaternion q = new Quaternion(axisOfRotation, angleInDegrees);
            matrix.RotateAtPrepend(q, controlPoint);
            allTransform.Matrix = matrix;
        }

        DoubleAnimation aniSizeY = new DoubleAnimation(1, TimeSpan.FromMilliseconds(350), FillBehavior.HoldEnd);
        public void aniSetSizeY(double value)
        {
            aniSizeY.To = value;
            this.BeginAnimation(ModelBase.SizeYProperty, aniSizeY);
        }


        Material saveMaterial;
        public void setHighLight()
        {
            MaterialGroup mg = (Material as MaterialGroup).Clone();
            //EmissiveMaterial emmat = new EmissiveMaterial((mg.Children[0] as DiffuseMaterial).Brush);
            EmissiveMaterial emmat = new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(0x60, 0xFF, 0xFF, 0xFF)));
            mg.Children.Add(emmat);
            saveMaterial = Material;
            Material = mg;
        }

        public void setNormal()
        {
            Material = saveMaterial;
        }



    }



    #region 标签模型
    /// <summary>
    /// 直接创建标签: LabelDirection指定垂直(缺省)或水平, LabelHeight确定大小
    /// LabelText设置后生成模型
    /// </summary>
    public class MLabel : ModelBase
    {
        //public MLabel()
        //{
        //    isSingle = true;
        //}
        public MLabel(ModelSerial Serial)
            : base(Serial)
        {
            isSingle = true;
            LabelDirection = serial.LabelDirection;
            LabelHeight = serial.LabelHeight;
            LabelColor = serial.LabelColor;
            isGlow = serial.isGlow;
            mAlign = new Vector3D(0.5, 0, 0.5);
        }
        //标签参数

        public ELabelDirection LabelDirection = ELabelDirection.垂直;
        public double LabelHeight = 0.001;
        public Brush LabelColor;
        public bool isGlow = false;
        //标签文本
        public static DependencyProperty LabelTextProperty = DependencyProperty.Register("LabelText", typeof(String), typeof(MLabel), new PropertyMetadata("", new PropertyChangedCallback(OnLabelTextChanged)));
        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }
        internal static void OnLabelTextChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            MLabel m = sender as MLabel;
            if ((string)e.OldValue == "") //创建
            {
                string meshname = m.LabelDirection == ELabelDirection.垂直 ? "meshText" : "meshTextH";
                MeshGeometry3D meshlabel = (MeshGeometry3D)Application.Current.FindResource(meshname);
                int rownums = MyFunction.StringContainsSubstringNums(m.LabelText, "\r\n");
                double panelheight = m.LabelHeight * (rownums + 1);
                double whscale = 1;
                Material mat = Model3DHelper.genTextMaterial(Brushes.Transparent, m.isGlow, m.LabelColor, 36, m.LabelText, out whscale);
                //m.Material = mat;
                double panelwidth = panelheight * whscale;
                m.model = new GeometryModel3D(meshlabel, mat);
                m.initBound = m.model.Bounds;
                //MyClassLibrary.OperateHelper.bind(m, "Material", m.model, GeometryModel3D.MaterialProperty);
                m.doAlign();

                if (m.LabelDirection == ELabelDirection.垂直)
                {
                    m.SizeY3D = panelheight;
                    m.SizeX3D = panelwidth;
                }
                else
                {
                    m.SizeZ3D = panelheight;
                    m.SizeX3D = panelwidth;
                }
                m.model.Transform = m.allTransform;

            }
            else //更改
            {
                int rownums = MyFunction.StringContainsSubstringNums(m.LabelText, "\r\n");
                double panelheight = m.LabelHeight * (rownums + 1);
                double whscale = 1;

                m.Material = Model3DHelper.genTextMaterial(Brushes.Transparent, m.isGlow, m.LabelColor, 36, m.LabelText, out whscale);
                (m.model as GeometryModel3D).Material = m.Material;
                double panelwidth = panelheight * whscale;
                if (m.LabelDirection == ELabelDirection.垂直)
                {
                    m.SizeY3D = panelheight;
                    m.SizeX3D = panelwidth;
                }
                else
                {
                    m.SizeZ3D = panelheight;
                    m.SizeX3D = panelwidth;
                }
            }
        }


    }

    #endregion

    #region 单体模型类
    /// <summary>
    /// 单体模型基类
    /// </summary>
    public abstract class MSingleBase : ModelBase
    {
        public MSingleBase(ModelSerial Serial)
            : base(Serial)
        {
            isSingle = true;
        }
        //===== *模型类型
        public static DependencyProperty ModelTypeProperty = DependencyProperty.Register("ModelType", typeof(EModelType), typeof(MSingleBase), new PropertyMetadata(EModelType.none, new PropertyChangedCallback(OnModelTypeChanged)));
        public EModelType ModelType
        {
            get { return (EModelType)GetValue(ModelTypeProperty); }
            set { SetValue(ModelTypeProperty, value); }
        }
        internal static void OnModelTypeChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            MSingleBase m = sender as MSingleBase;
            m.changeModelType();
        }
        //===== 重载方法
        protected abstract void changeModelType();
        protected virtual void afterBuildModel() //建模之后的处理
        {
            CreateLabel();  //若serial的LstLabel有值，则创建标签
        }
        //===== 标签处理
        public MLabel LabelModel;
        protected void CreateLabel()
        {
            if (serial.LstLabel.Count > 0 && serial.isShowLabel)
            {
                //创建标签模型
                LabelModel = new MLabel(serial);
                LabelModel.mAlign = new Vector3D(0.5, 0, 0.5); //EModelAlign.底对齐;
                //实现3D特征到ModelLabel属性的绑定
                MultiBinding mbind = new MultiBinding();
                mbind.Converter = new LabelConverter();
                mbind.ConverterParameter = this;
                mbind.Mode = BindingMode.OneWay;

                Binding bind;
                foreach (EValueType one in serial.LstLabel)
                {
                    bind = new Binding();
                    bind.Source = this;
                    switch (one)
                    {
                        case EValueType.X:
                            bind.Path = new PropertyPath(ModelBase.XProperty);
                            break;
                        case EValueType.Y:
                            bind.Path = new PropertyPath(ModelBase.YProperty);
                            break;
                        case EValueType.Z:
                            bind.Path = new PropertyPath(ModelBase.ZProperty);
                            break;
                        case EValueType.SizeX:
                            bind.Path = new PropertyPath(ModelBase.SizeXProperty);
                            break;
                        case EValueType.SizeY:
                            bind.Path = new PropertyPath(ModelBase.SizeYProperty);
                            break;
                        case EValueType.SizeZ:
                            bind.Path = new PropertyPath(ModelBase.SizeZProperty);
                            break;
                        case EValueType.Color:
                            bind.Path = new PropertyPath(ModelBase.ColorValueProperty);
                            break;
                    }
                    bind.Mode = BindingMode.OneWay;
                    mbind.Bindings.Add(bind);
                }
                BindingOperations.SetBinding(LabelModel, MLabel.LabelTextProperty, mbind);
                //设置标签和主体的连接
                LabelModel.LinkModel = this;
                LabelModel.LinkMode = EModelLinkMode.置于上;
            }
        }


    }
    /// <summary>
    /// 常规园柱
    /// </summary>
    public class MCylinder : MSingleBase
    {
        public MCylinder(ModelSerial Serial)
            : base(Serial)
        {
            ModelType = EModelType.常规园柱;
            afterBuildModel();
        }
        protected override void changeModelType()
        {
            //throw new NotImplementedException();
            MeshGeometry3D mesh = (MeshGeometry3D)Application.Current.FindResource("meshCyl");
            model = new GeometryModel3D(mesh, Material);
            initBound = model.Bounds;
            normalizeModel();
            model.SetValue(ModelBase.TagProperty, this);
            model.Transform = allTransform;
        }
    }
    /// <summary>
    /// 常规方柱
    /// </summary>
    public class MCube : MSingleBase
    {
        public MCube(ModelSerial Serial)
            : base(Serial)
        {
            ModelType = EModelType.常规方柱;
            afterBuildModel();
        }
        protected override void changeModelType()
        {
            //throw new NotImplementedException();
            MeshGeometry3D mesh = (MeshGeometry3D)Application.Current.FindResource("meshCube");
            model = new GeometryModel3D(mesh, Material);
            initBound = model.Bounds;
            normalizeModel();
            model.SetValue(ModelBase.TagProperty, this);
            model.Transform = allTransform;
        }
    }
    /// <summary>
    /// 平滑边圆柱
    /// </summary>
    public class MSmoothCylinder : MSingleBase
    {
        public MSmoothCylinder(ModelSerial Serial)
            : base(Serial)
        {
            ModelType = EModelType.平滑园柱;
            afterBuildModel();
        }
        protected override void changeModelType()
        {
            //throw new NotImplementedException();
            MeshGeometry3D meshtop = (MeshGeometry3D)Application.Current.FindResource("meshSCylTop");
            MeshGeometry3D meshmid = (MeshGeometry3D)Application.Current.FindResource("meshSCylMid");
            model = new Model3DGroup();
            (model as Model3DGroup).Children.Add(new GeometryModel3D(meshtop, Material));
            (model as Model3DGroup).Children.Add(new GeometryModel3D(meshmid, Material));
            initBound = model.Bounds;
            normalizeModel();
            model.SetValue(ModelBase.TagProperty, this);
            model.Transform = allTransform;
        }
    }
    /// <summary>
    /// 平滑边方柱
    /// </summary>
    public class MSmoothCube : MSingleBase
    {
        public MSmoothCube(ModelSerial Serial)
            : base(Serial)
        {
            ModelType = EModelType.平滑方柱;
            afterBuildModel();
        }
        protected override void changeModelType()
        {
            //throw new NotImplementedException();
            MeshGeometry3D meshtop = (MeshGeometry3D)Application.Current.FindResource("meshBoxTop");
            MeshGeometry3D meshmid = (MeshGeometry3D)Application.Current.FindResource("meshBoxMid");
            model = new Model3DGroup();
            (model as Model3DGroup).Children.Add(new GeometryModel3D(meshtop, Material));
            (model as Model3DGroup).Children.Add(new GeometryModel3D(meshmid, Material));
            initBound = model.Bounds;
            normalizeModel();
            model.SetValue(ModelBase.TagProperty, this);
            model.Transform = allTransform;
        }
    }
    /// <summary>
    /// 园锥
    /// </summary>
    public class MCone : MSingleBase
    {
        public MCone(ModelSerial Serial)
            : base(Serial)
        {
            ModelType = EModelType.圆锥;
            afterBuildModel();
        }
        protected override void changeModelType()
        {
            MeshGeometry3D mesh = (MeshGeometry3D)Application.Current.FindResource("meshCone");
            model = new GeometryModel3D(mesh, Material);
            initBound = model.Bounds;
            normalizeModel();
            model.SetValue(ModelBase.TagProperty, this);
            model.Transform = allTransform;
        }
    }
    #endregion
    #endregion
    #region 组合模型类
    /// <summary>
    /// 组合模型基类
    /// </summary>
    public abstract class GroupModelBase : ModelBase
    {
        public GroupModelBase(ModelSerial Serial)
            : base(Serial)
        {
            mAlign = new Vector3D(0.5, 0, 0.5);
            //for (int i = 0; i < serial.groupModelCount; i++)
            //    Items.Add(new GroupValueItem(serial.LstGroupLabel[i].Lvalue, this));
            isSingle = false;

        }

        public Dictionary<string, GroupValueItem> Items = new Dictionary<string, GroupValueItem>(); //组合类的值列表
        public Dictionary<string, ModelBase> models = new Dictionary<string, ModelBase>();
        public MLabel LabelModel;
        public int dataCount { get; set; }
        //===== 统计属性
        public static DependencyProperty GroupSumProperty = DependencyProperty.Register("GroupSum", typeof(double), typeof(GroupModelBase), new PropertyMetadata(double.NegativeInfinity));
        public double GroupSum
        {
            get { return (double)GetValue(GroupSumProperty); }
            set { SetValue(GroupSumProperty, value); }
        }
        public static DependencyProperty GroupModelTypeProperty = DependencyProperty.Register("GroupModelType", typeof(EModelType), typeof(GroupModelBase), new PropertyMetadata(EModelType.none, new PropertyChangedCallback(OnGroupModelTypeChanged)));
        public EModelType GroupModelType
        {
            get { return (EModelType)GetValue(GroupModelTypeProperty); }
            set { SetValue(GroupModelTypeProperty, value); }
        }
        internal static void OnGroupModelTypeChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            GroupModelBase m = sender as GroupModelBase;
            m.OnChangeGroupModelType();
        }
        //============ 需重写方法
        protected abstract void OnChangeGroupModelType();
        internal abstract void OnValueChange();
        internal virtual void createSubModel(string dataname) { }  //创建指定的子模型
        public abstract void setBindValue(object src, DependencyProperty srcp, string dataname);
        protected virtual void afterBuildModel()
        {
            //CreateLabel();
        }   //===== 标签处理
        protected virtual void CreateLabel()
        {
            if (Items.Count == dataCount && serial.isShowLabel && LabelModel == null)
            {

                //创建标签模型
                LabelModel = new MLabel(serial);
                LabelModel.mAlign = new Vector3D(0.5, 0, 0.5);//EModelAlign.底对齐;
                ////实现3D特征到ModelLabel属性的绑定
                //MultiBinding mbind = new MultiBinding();
                //mbind.Converter = new GroupLabelConverter();
                //mbind.ConverterParameter = this;
                //mbind.Mode = BindingMode.OneWay;

                //Binding bind;
                ////for (int i = 0; i < serial.LstGroupLabel.Count; i++)
                //foreach (GroupValueItem item in Items.Values)
                //{
                //    bind = new Binding();
                //    bind.Source = item;//Items[i];
                //    bind.Path = new PropertyPath(GroupValueItem.ValueProperty);
                //    bind.Mode = BindingMode.OneWay;
                //    mbind.Bindings.Add(bind);
                //}
                //BindingOperations.SetBinding(LabelModel, MLabel.LabelTextProperty, mbind);

                LabelModel.LabelText = genLabelText();

                //设置标签和主体的连接
                LabelModel.LinkModel = models.Last().Value;
                LabelModel.LinkMode = EModelLinkMode.置于上;
            }
            else if (serial.isShowLabel && LabelModel != null)
            {
                LabelModel.LabelText = genLabelText();
            }
        }

        protected string genLabelText()
        {
            string result = "";
            string cname = "", svalue = "";
            int i = 0;
            foreach (KeyValuePair<string, GroupValueItem> item in Items)
            {
                cname = item.Key;
                svalue = item.Value.Value.ToString("f0");
                if (i > 0)
                    result += "\r\n";
                result += (cname + "：" + svalue);
                i++;
            }
            return result;
        }
        //============ 公开方法
        public void setBindColorValue(object src, DependencyProperty srcp, string dataname)
        {
            OperateHelper.bind(src, srcp, models[dataname], ModelBase.ColorValueProperty);
        }
        public void setMateral(Material mat, string dataname)
        {
            if (models.Count>0)
            models[dataname].Material = mat;
        }

        public void setValue(string dataname, double value)
        {
            double newvalue = value == 0 ? 0.000001 : Math.Abs(value);
            if (Items.Keys.FirstOrDefault(p => p == dataname) == null) //创建
            {
                GroupValueItem item = new GroupValueItem(newvalue, this);
                Items.Add(dataname, item);

                if (models.Keys.FirstOrDefault(p => p == dataname) == null)
                {
                    createSubModel(dataname);
                }
                CreateLabel();
            }
            else  //更新
            {
                Items[dataname].Value = newvalue;
                if (serial.scene.isAniModel)
                    models[dataname].aniSetSizeY(newvalue);
                if (dataname == Items.Last().Key)  //设置最后一个值时，更新label
                    CreateLabel();
            }
        }

    }
    public class GroupValueItem : DependencyObject
    {
        public GroupValueItem(double value, GroupModelBase groupmodel)
        {
            //idx = index;
            Value = value;
            GroupModel = groupmodel;
        }

        public GroupModelBase GroupModel;

        //===== *值属性
        public static DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(GroupValueItem), new PropertyMetadata(0.0, new PropertyChangedCallback(OnValueChanged)));
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        internal static void OnValueChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            GroupValueItem m = sender as GroupValueItem;
            if (m.GroupModel != null)
                m.GroupModel.OnValueChange();
        }
        //====
        //public int idx 
        //{
        //    get
        //    {
        //        int idx = 0;
        //        foreach (GroupValueItem one in GroupModel.Items)
        //        {
        //            if (this.Equals(one))
        //                break;
        //            idx++;
        //        }
        //        return idx;
        //    }
        //}
    }


    /// <summary>
    /// 堆叠园柱
    /// </summary>
    public class MGroupCylinder : GroupModelBase
    {
        public MGroupCylinder(ModelSerial Serial)
            : base(Serial)
        {
            GroupModelType = EModelType.堆叠园柱;
            afterBuildModel();

        }
        protected override void OnChangeGroupModelType()
        {
            // value对应sizeY
            if (model == null)
                model = new Model3DGroup();

            //MCylinder m, oldm = null;
            //int i = 0;
            //foreach (KeyValuePair<string, GroupValueItem> item in Items)
            //{
            //    m = new MCylinder(serial);
            //    if (i == 0)
            //    {
            //        OperateHelper.bind(this, ModelBase.X3DProperty, m, ModelBase.X3DProperty);
            //        OperateHelper.bind(this, ModelBase.Y3DProperty, m, ModelBase.Y3DProperty);
            //        OperateHelper.bind(this, ModelBase.Z3DProperty, m, ModelBase.Z3DProperty);
            //    }
            //    OperateHelper.bind(this, ModelBase.SizeX3DProperty, m, ModelBase.SizeX3DProperty);
            //    OperateHelper.bind(this, ModelBase.SizeZ3DProperty, m, ModelBase.SizeZ3DProperty);

            //    MaterialGroup mat = new MaterialGroup();
            //    mat.Children.Add(new DiffuseMaterial(serial.LstGroupLabel[item.Key]));
            //    mat.Children.Add(new SpecularMaterial(Brushes.Silver, 80));
            //    m.Material = mat;
            //    models.Add(item.Key, m);
            //    (model as Model3DGroup).Children.Add(m.model);
            //    OperateHelper.bind(Items[item.Key], GroupValueItem.ValueProperty, models[item.Key], ModelBase.SizeYProperty);
            //    if (i > 0)  //子模型通过连接实现堆叠
            //    {
            //        m.LinkModel = oldm;
            //        m.LinkMode = EModelLinkMode.置于上;
            //    }
            //    oldm = m;
            //    i++;
            //}

            model.SetValue(ModelBase.TagProperty, this);

        }
        public override void setBindValue(object src, DependencyProperty srcp, string dataname)
        {
            OperateHelper.bind(src, srcp, Items[dataname], GroupValueItem.ValueProperty);
        }
        internal override void OnValueChange()
        {
            GroupSum = Items.Sum(p => p.Value.Value);
        }
        internal override void createSubModel(string dataname)
        {
            MCylinder m;
            m = new MCylinder(serial);
            m.model.SetValue(ModelBase.TagProperty, this);
            if (models.Count == 0)
            {
                OperateHelper.bind(this, ModelBase.X3DProperty, m, ModelBase.X3DProperty);
                OperateHelper.bind(this, ModelBase.Y3DProperty, m, ModelBase.Y3DProperty);
                OperateHelper.bind(this, ModelBase.Z3DProperty, m, ModelBase.Z3DProperty);
            }
            OperateHelper.bind(this, ModelBase.SizeX3DProperty, m, ModelBase.SizeX3DProperty);
            OperateHelper.bind(this, ModelBase.SizeZ3DProperty, m, ModelBase.SizeZ3DProperty);

            MaterialGroup mat = new MaterialGroup();
            mat.Children.Add(new DiffuseMaterial(serial.LstGroupLabel[dataname]));
            mat.Children.Add(new SpecularMaterial(Brushes.Silver, 80));
            m.Material = mat;
            if (models.Count > 0)  //子模型通过连接实现堆叠
            {
                m.LinkModel = models.Last().Value;
                m.LinkMode = EModelLinkMode.置于上;
            }
            models.Add(dataname, m);
            (model as Model3DGroup).Children.Add(m.model);
            OperateHelper.bind(Items[dataname], GroupValueItem.ValueProperty, models[dataname], ModelBase.SizeYProperty);

        }
    }
    /// <summary>
    /// 堆叠方柱
    /// </summary>
    public class MGroupCube : GroupModelBase
    {
        public MGroupCube(ModelSerial Serial)
            : base(Serial)
        {

            GroupModelType = EModelType.堆叠方柱;
            afterBuildModel();

        }
        protected override void OnChangeGroupModelType()
        {
            // value对应sizeY
            model = new Model3DGroup();

            MCube m, oldm = null;
            int i = 0;
            foreach (KeyValuePair<string, GroupValueItem> item in Items)
            {
                m = new MCube(serial);
                if (i == 0)
                {
                    OperateHelper.bind(this, ModelBase.X3DProperty, m, ModelBase.X3DProperty);
                    OperateHelper.bind(this, ModelBase.Y3DProperty, m, ModelBase.Y3DProperty);
                    OperateHelper.bind(this, ModelBase.Z3DProperty, m, ModelBase.Z3DProperty);
                }
                OperateHelper.bind(this, ModelBase.SizeX3DProperty, m, ModelBase.SizeX3DProperty);
                OperateHelper.bind(this, ModelBase.SizeZ3DProperty, m, ModelBase.SizeZ3DProperty);

                MaterialGroup mat = new MaterialGroup();
                mat.Children.Add(new DiffuseMaterial(serial.LstGroupLabel[item.Key]));
                mat.Children.Add(new SpecularMaterial(Brushes.Silver, 80));
                m.Material = mat;
                models.Add(item.Key, m);
                (model as Model3DGroup).Children.Add(m.model);
                OperateHelper.bind(Items[item.Key], GroupValueItem.ValueProperty, models[item.Key], ModelBase.SizeYProperty);
                if (i > 0)  //子模型通过连接实现堆叠
                {
                    m.LinkModel = oldm;
                    m.LinkMode = EModelLinkMode.置于上;
                }
                oldm = m;
                i++;
            }

            model.SetValue(ModelBase.TagProperty, this);

        }
        public override void setBindValue(object src, DependencyProperty srcp, string dataname)
        {
            OperateHelper.bind(src, srcp, Items[dataname], GroupValueItem.ValueProperty);
        }
        internal override void OnValueChange()
        {
            GroupSum = Items.Sum(p => p.Value.Value);
        }

    }
    /// <summary>
    /// 堆叠圆锥
    /// </summary>
    public class MGroupCone : GroupModelBase
    {
        public MGroupCone(ModelSerial Serial)
            : base(Serial)
        {
            GroupModelType = EModelType.堆叠园锥;
            afterBuildModel();
        }
        protected override void OnChangeGroupModelType()
        {
            // value对应sizeY
            model = new Model3DGroup();
            int i = 0;
            //for (int i = 0; i < Items.Count; i++)
            foreach (KeyValuePair<string, GroupValueItem> item in Items)
            {
                MConePart m = new MConePart(serial, genMesh(i));
                MaterialGroup mat = new MaterialGroup();
                mat.Children.Add(new DiffuseMaterial(serial.LstGroupLabel[item.Key]));
                mat.Children.Add(new SpecularMaterial(Brushes.Silver, 80));
                m.Material = mat;
                models.Add(item.Key, m);
                (model as Model3DGroup).Children.Add(m.model);
                i++;
            }
            model.SetValue(ModelBase.TagProperty, this);
            model.Transform = allTransform;
        }
        public override void setBindValue(object src, DependencyProperty srcp, string dataname)
        {
            OperateHelper.bind(src, srcp, Items[dataname], GroupValueItem.ValueProperty);
        }
        internal override void OnValueChange()
        {
            //for (int i = 0; i < Items.Count; i++)
            int i = 0;
            foreach (KeyValuePair<string, GroupValueItem> item in Items)
            {
                (models[item.Key] as MPartBase).ModifyMesh(genMesh(i));
                i++;
            }
            GroupSum = Items.Sum(p => p.Value.Value);
        }

        private MeshGeometry3D genMesh(int idx)  //生成园锥片mesh
        {
            MeshGeometry3D mesh;
            double sumvalue = Items.Sum(p => p.Value.Value);
            double maxRadius = 1;
            Point3D pTop, pButtom;
            double tRadius, bRadius;
            pButtom = new Point3D(0, Items.Take(idx).Sum(p => p.Value.Value) / serial.maxSizeYValue * serial.scene.Limit3D.SizeY, 0);
            pTop = new Point3D(0, Items.Take(idx + 1).Sum(p => p.Value.Value) / serial.maxSizeYValue * serial.scene.Limit3D.SizeY, 0);
            bRadius = maxRadius * Items.Skip(idx).Sum(p => p.Value.Value) / sumvalue;
            tRadius = maxRadius * Items.Skip(idx + 1).Sum(p => p.Value.Value) / sumvalue;
            mesh = Model3DHelper.genCone3DMesh(pTop, pButtom, tRadius, bRadius);
            return mesh;
        }
    }
    /// <summary>
    /// 饼
    /// </summary>
    public class MGroupPie : GroupModelBase
    {
        public MGroupPie(ModelSerial Serial)
            : base(Serial)
        {
            GroupModelType = EModelType.饼;
            afterBuildModel();
        }
        protected override void OnChangeGroupModelType()
        {
            model = new Model3DGroup();
            int i = 0;
            foreach (KeyValuePair<string, GroupValueItem> item in Items)
            //for (int i = 0; i < Items.Count; i++)
            {
                MPiePart m = new MPiePart(serial, genMesh(i));
                models.Add(item.Key, m);
                (model as Model3DGroup).Children.Add(m.model);
                i++;
            }
            model.SetValue(ModelBase.TagProperty, this);
            normalizeModel();
            model.Transform = allTransform;
        }
        public override void setBindValue(object src, DependencyProperty srcp, string dataname)
        {
            OperateHelper.bind(src, srcp, Items[dataname], GroupValueItem.ValueProperty);
        }

        internal override void OnValueChange()
        {
            //for (int i = 0; i < Items.Count; i++)
            int i = 0;
            foreach (KeyValuePair<string, GroupValueItem> item in Items)
            {
                (models[item.Key] as MPartBase).ModifyMesh(genMesh(i));
                i++;
            }
            GroupSum = Items.Sum(p => p.Value.Value);
        }

        private MeshGeometry3D genMesh(int idx)  //生成饼片mesh
        {
            MeshGeometry3D mesh;
            double sumvalue = Items.Sum(p => p.Value.Value);
            double startAngle, endAngle;
            startAngle = 360.0 * Items.Take(idx).Sum(p => p.Value.Value) / sumvalue;
            endAngle = 360.0 * Items.Take(idx + 1).Sum(p => p.Value.Value) / sumvalue;
            mesh = Model3DHelper.genPieSlice(0.5, 0, 2, startAngle, endAngle, 10, Model3DHelper.EPieType.闭合矩形面);
            return mesh;
        }

    }


    #region 组合模型的片类
    /// <summary>
    /// 片基类
    /// </summary>
    public abstract class MPartBase : ModelBase
    {
        protected abstract void CreatModel(MeshGeometry3D mesh);
        internal abstract void ModifyMesh(MeshGeometry3D mesh);
    }

    /// <summary>
    /// 园锥片
    /// </summary>
    public class MConePart : MPartBase
    {
        public MConePart(ModelSerial Serial, MeshGeometry3D mesh)
        {
            serial = Serial;
            this.Material = serial.Material;
            CreatModel(mesh);
        }

        protected override void CreatModel(MeshGeometry3D mesh)
        {
            model = new GeometryModel3D(mesh, Material);
        }
        internal override void ModifyMesh(MeshGeometry3D mesh)
        {
            (model as GeometryModel3D).Geometry = mesh;
        }
    }

    /// <summary>
    /// 饼片
    /// </summary>
    public class MPiePart : MPartBase
    {
        public MPiePart(ModelSerial Serial, MeshGeometry3D mesh)
        {
            serial = Serial;
            this.Material = serial.Material;
            CreatModel(mesh);
        }

        protected override void CreatModel(MeshGeometry3D mesh)
        {
            model = new GeometryModel3D(mesh, Material);
        }
        internal override void ModifyMesh(MeshGeometry3D mesh)
        {
            (model as GeometryModel3D).Geometry = mesh;
        }
    }

    #endregion
    #endregion
    #region 转换器
    /// <summary>
    /// 位置连接值转换器，结果实际从参数中计算得到
    /// </summary>
    public class LinkConverter : IMultiValueConverter
    {
        public object Convert(object[] o, Type type, object parameter, CultureInfo culture)
        {
            double result = 0;
            List<object> paras = (List<object>)parameter;
            EModelLinkMode linkmode = (EModelLinkMode)paras[0];
            Rect3D bound = (paras[1] as ModelBase).model.Bounds;
            Rect3D selfbound = (paras[2] as ModelBase).model.Bounds;
            Rect3D initbound = (paras[2] as ModelBase).initBound;
            Vector3D selfAlign = (paras[2] as ModelBase).mAlign;
            double py = -(initbound.Y + selfAlign.Y * initbound.SizeY) * selfbound.SizeY / initbound.SizeY;
            switch (linkmode)
            {
                case EModelLinkMode.置于上:
                    result = bound.Y + bound.SizeY + py;
                    break;
                case EModelLinkMode.置于下:
                    result = bound.Y;
                    break;
                case EModelLinkMode.置于左:
                    result = bound.X;
                    break;
                case EModelLinkMode.置于右:
                    result = bound.X + bound.SizeX;
                    break;
                case EModelLinkMode.置于前:
                    result = bound.Z + bound.SizeZ;
                    break;
                case EModelLinkMode.置于后:
                    result = bound.Z;
                    break;
            }




            return result;
        }
        public object[] ConvertBack(object o, Type[] type, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// 位置连接值转换器2，结果实际从参数中计算得到
    /// </summary>
    public class LinkConverter2 : IValueConverter
    {
        public object Convert(object value, Type type, object parameter, CultureInfo culture)
        {
            double result = 0;
            List<object> paras = (List<object>)parameter;
            string axis = paras[0].ToString();
            Rect3D bound = (paras[1] as ModelBase).model.Bounds;
            switch (axis)
            {
                case "x":
                    result = bound.X;
                    break;
                case "y":
                    result = bound.Y;
                    break;
                case "z":
                    result = bound.Z;
                    break;
            }
            return result;

        }
        public object ConvertBack(object value, Type type, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    /// <summary>
    /// XYZ sizeXYZ(值) 与 3D 度量转换
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class ValueTo3DConverter : IValueConverter
    {
        public object Convert(object value, Type type, object parameter, CultureInfo culture)
        {
            //value为值, 返回3D空间度量, parameter为换算比例
            double v = (double)value; ;
            double scale = (double)parameter;
            return v / scale; ;
        }
        public object ConvertBack(object value, Type type, object parameter, CultureInfo culture)
        {
            double v = (double)value; ;
            double scale = (double)parameter;
            return v * scale; ;
        }
    }

    /// <summary>
    /// Label多值字串转换器，结果实际从参数中计算得到
    /// </summary>
    public class LabelConverter : IMultiValueConverter
    {
        public object Convert(object[] o, Type type, object parameter, CultureInfo culture)
        {
            string result = "";
            MSingleBase m = (MSingleBase)parameter;
            string cname = "", svalue = "";

            for (int i = 0; i < m.serial.LstLabel.Count; i++)
            {
                string txt = m.serial.LabelTemplate;
                if (!string.IsNullOrWhiteSpace(m.name))
                    txt = txt.Replace("{Name}", m.name);
                txt = txt.Replace("{X}", m.X.ToString("f0"));
                txt = txt.Replace("{Y}", m.Y.ToString("f0"));
                txt = txt.Replace("{Z}", m.Z.ToString("f0"));
                txt = txt.Replace("{SizeX}", m.SizeX.ToString("f0"));
                txt = txt.Replace("{SizeY}", m.SizeY.ToString("f0"));
                txt = txt.Replace("{SizeZ}", m.SizeZ.ToString("f0"));
                txt = txt.Replace("{Color}", m.ColorValue.ToString("f0"));
                txt = txt.Replace("{MainValue}", m.mainValue.ToString("f0"));

                if (m.serial.LstLabel.Count > 1)
                {
                    if (i > 0)
                        result += "\r\n";
                    result += txt;
                }
                else
                    result = txt;

                //switch (m.serial.LstLabel[i])
                //{
                //    case EValueType.X:
                //        cname = m.serial.descX;
                //        svalue = m.X.ToString("f0");
                //        break;
                //    case EValueType.Y:
                //        cname = m.serial.descY;
                //        svalue = m.Y.ToString("f0");
                //        break;
                //    case EValueType.Z:
                //        cname = m.serial.descZ;
                //        svalue = m.Z.ToString("f0");
                //        break;
                //    case EValueType.SizeX:
                //        cname = m.serial.descSizeX;
                //        svalue = m.SizeX.ToString("f0");
                //        break;
                //    case EValueType.SizeY:
                //        cname = m.serial.descSizeY;
                //        svalue = m.SizeY.ToString("f0");
                //        break;
                //    case EValueType.SizeZ:
                //        cname = m.serial.descSizeZ;
                //        svalue = m.SizeZ.ToString("f0");
                //        break;
                //    case EValueType.Color:
                //        cname = m.serial.descColor;
                //        svalue = m.ColorValue.ToString("f0");
                //        break;
                //}
                //if (m.serial.LstLabel.Count > 1)
                //{
                //    if (i > 0)
                //        result += "\r\n";
                //    result += cname + svalue;
                //}
                //else
                //{
                //    result = svalue;
                //}
            }


            return result;
        }
        public object[] ConvertBack(object o, Type[] type, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// 组合模型Label多值字串转换器，结果实际从参数中计算得到
    /// </summary>
    public class GroupLabelConverter : IMultiValueConverter
    {
        public object Convert(object[] o, Type type, object parameter, CultureInfo culture)
        {
            string result = "";
            GroupModelBase m = (GroupModelBase)parameter;
            string cname = "", svalue = "";
            //for (int i = 0; i < m.Items.Count; i++)
            int i = 0;
            foreach (KeyValuePair<string, GroupValueItem> item in m.Items)
            {
                cname = item.Key; //m.serial.LstGroupLabel[m.Items[i].idx].Lname;
                svalue = item.Value.Value.ToString("f0");
                if (i > 0)
                    result += "\r\n";
                result += (cname + "：" + svalue);
                i++;
            }


            return result;
        }
        public object[] ConvertBack(object o, Type[] type, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    #endregion
}
