using System;
using System.Windows;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using xna = Microsoft.Xna.Framework;



namespace WpfEarthLibrary
{

    ///<summary>========== 基类 ==========</summary>
    public abstract class PowerBasicObject : MyClassLibrary.MVVM.DependencyNotificationObject
    {

        public PowerBasicObject(pLayer Parent) { parent = Parent; }

        internal virtual void initGraphObject() { }

        public pLayer parent;

        ///<summary>包含的数据对象</summary>
        public Dictionary<string, PowerBasicObject> submodels = new Dictionary<string, PowerBasicObject>();

        ///<summary>添加数据对象</summary>
        public void AddSubObject(string Key, pData Object)
        {
            if (!submodels.Keys.Contains(Key))
            {
                submodels.Add(Key, Object);
                Object.refObject = this;
            }
        }

        string _id;
        ///<summary>唯一设备ID</summary>
        public string id
        {
            get { return _id; }
            set
            {
                _id = value;
                hashcode = value.GetHashCode();
                labelhashcode = (value + "标签").GetHashCode();
            }
        }

        ///<summary>辅助ID，用于辅助保存未使用设备ID做为ID的数据表，如pms2.0中，图形表使用了单独的ID</summary>
        public string id2 { get; set; }

        ///<summary>哈希码，作为D3D中对象ID</summary>
        internal int hashcode;

        ///<summary>标签哈希码，作为D3D中标签对象ID</summary>
        internal int labelhashcode;

        ///<summary>名称</summary>
        public string name { get; set; }

        ///<summary>修改状态</summary>
        public EModifyStatus modifyStatus { get; set; }

        private bool _isReceivePick = true;
        ///<summary>是否接受拾取, 缺省为True接受</summary>
        public bool isReceivePick
        {
            get { return _isReceivePick; }
            set { _isReceivePick = value; }
        }


        private string _pickFlag;
        ///<summary>拾取标志，调用限定性的拾取方法可限制只拾取该标志对象</summary>
        public string pickFlag
        {
            get { return _pickFlag; }
            set { _pickFlag = value; }
        }
        internal int pickFlagId { get { return string.IsNullOrWhiteSpace(_pickFlag) ? 0 : _pickFlag.GetHashCode(); } }

        ///<summary>对象在三D空间的位置</summary>
        public VECTOR3D VecLocation;

        ///<summary>原始坐标中心点</summary>
        public Point center;

        ///<summary>原始坐标表示的范围，若为经纬度，x表示纬度, y表示经度</summary>
        public System.Windows.Rect bounds;

        ///<summary>对象在三D空间内部的bounding</summary>
        internal abstract xna.BoundingBox boundingBox { get; }


        ///<summary>动态载入时使用，是否已处理</summary>
        public bool isHandled;

        ///<summary>此模型是否显示，由logicVisibility与rangeVisibility共同决定</summary>
        public bool isShowObject
        {
            get { return logicVisibility && (rangeVisibility || !parent.parent.earth.config.isDynShow); }
        }


        private bool _logicVisibility = true;
        ///<summary>业务逻辑可见性，实际呈现的必要条件之一，缺省true</summary>
        public bool logicVisibility
        {
            get { return _logicVisibility; }
            set { _logicVisibility = value; }
        }

        private bool _rangeVisibility = true;
        ///<summary>范围可见性，动态显示模式下实际呈现的必要条件之一，缺省true</summary>
        public bool rangeVisibility
        {
            get { return _rangeVisibility; }
            set { _rangeVisibility = value; }
        }




        private bool _isShowSubObject = true;
        ///<summary>是否显示此模型的子对象</summary>
        public bool isShowSubObject
        {
            get { return _isShowSubObject; }
            set { _isShowSubObject = value; }
        }


        private VECTOR3D _axis = new VECTOR3D(0, 0, 1);
        ///<summary>初始旋转轴</summary>
        public VECTOR3D axis
        {
            get { return _axis; }
            set { _axis = value; }
        }

        private float _angle = 0;
        ///<summary>初始旋转角，弧度单位</summary>
        public float angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                if (this is pSymbolObject)
                    updateProperty(value, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.角度, hashcode, hashcode);
                else if (this is pCustomObject)
                    updateProperty(value, parent.parent.earth.earthkey, EModelType.自定义模型, EPropertyType.角度, hashcode, hashcode);

            }
        }



        ///<summary>更新基本类型的属性</summary>
        internal void updateProperty(object value, int earthkey, EModelType modeltype, EPropertyType propertytype, int rootid, int id)
        {

            IntPtr ipPara = IntPtr.Zero;
            if (propertytype == EPropertyType.颜色) //色彩属性，转为DWORD类型
            {
                uint tmp = Helpler.ColorToUInt((Color)value);
                ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(tmp));
                Marshal.StructureToPtr(tmp, ipPara, false);
            }
            else
            {
                ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(value));
                Marshal.StructureToPtr(value, ipPara, false);
            }
            D3DManager.ChangeProperty(earthkey, (int)modeltype, (int)propertytype, rootid, id, ipPara, 1, IntPtr.Zero, 0);
            Marshal.FreeCoTaskMem(ipPara);

        }




        //===========================  业务数据扩展  ==============================
        ///<summary>基础业务数据，向前兼容</summary>
        public busiBase busiData { get; set; }

        ///<summary>扩展业务对象描述数据，一般为静态数据</summary>
        public object busiDesc { get; set; }
        ///<summary>扩展业务台账数据，一般为静态数据</summary>
        public object busiAccount { get; set; }
        ///<summary>扩展业务运行数据，一般为动态数据</summary>
        public object busiRunData { get; set; }
        ///<summary>扩展业务对象拓扑数据</summary>
        public object busiTopo { get; set; }

        ///<summary>创建台账数据busiAccount，若派生类未实现，将不会创建</summary>
        public virtual void createAcntData() { }
        ///<summary>创建运行数据busiRundata，若派生类未实现，将不会创建</summary>
        public virtual void createRunData() { }

        ///<summary>用于存储上层应用的数据库操作的键值，仅起存储和指示功能</summary>
        public string DBOPKey { get; set; }

        #region ====================== 通用可视属性 =======================
        private Color _color;
        ///<summary>模型颜色，通过自动设置为材质的环境色和漫反射色生效，更改后会自动调用材质刷新，更复杂的如反射效果请直接设置material属性</summary>
        public Color color
        {
            get { return _color; }
            set
            {
                _color = value;

                material.Ambient = material.Diffuse = value;
                refreshMaterial();


            }
        }



        private CMaterial _material = new CMaterial();
        ///<summary>模型材质</summary>
        public CMaterial material
        {
            get { return _material; }
            set
            {
                _material = value;
            }
        }

        ///<summary>刷新材质，当更改材质内部参数后需调用此方法才可生效</summary>
        public void refreshMaterial()
        {
            if (parent != null && parent.parent != null)
            {
                if (this is pSymbolObject)
                    updateProperty(material.materialSturPara, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.材质, hashcode, hashcode);
                else if (this is pPowerLine)
                    updateProperty(material.materialSturPara, parent.parent.earth.earthkey, EModelType.折线, EPropertyType.材质, hashcode, hashcode);
                else if (this is pArea)
                    updateProperty(material.materialSturPara, parent.parent.earth.earthkey, EModelType.区域, EPropertyType.材质, hashcode, hashcode);
                else if (this is pText)
                    updateProperty(material.materialSturPara, parent.parent.earth.earthkey, EModelType.文字, EPropertyType.材质, hashcode, hashcode);
                else if (this is pCustomObject)
                    updateProperty(material.materialSturPara, parent.parent.earth.earthkey, EModelType.自定义模型, EPropertyType.材质, hashcode, hashcode);
            }
        }



        #endregion

        #region ======================== 标签相关 ====================================

        private string _Label;
        ///<summary>标签，注：isShowLabel应最后设置，之前设置的标签属性才能生效</summary>
        public string Label
        {
            get { return _Label; }
            set
            {
                _Label = value;
                //if (LabelObject == null)
                //    CreateLabelObject();
                //LabelObject.text = value;
            }
        }


        private bool _isShowLabel;
        ///<summary>是否显示文字标签</summary>
        public bool isShowLabel
        {
            get { return _isShowLabel; }
            set
            {
                _isShowLabel = value;
                if (value)
                {
                    if (!string.IsNullOrWhiteSpace(Label))
                    {
                        if (LabelObject == null)
                            CreateLabelObject();
                        LabelObject.text = Label;
                        submodels.Add("内置标签", LabelObject);
                    }
                    else
                    {
                        if (submodels.Keys.Contains("内置标签"))
                            submodels.Remove("内置标签");
                    }
                }
                else
                {
                    if (submodels.Keys.Contains("内置标签"))
                        submodels.Remove("内置标签");
                }
            }
        }


        ///<summary>文字面板的纹理键</summary>
        public string LabelPanelKey { get; set; }


        private float _LabelSizeX = 1;
        ///<summary>标签X缩放系数</summary>
        public float LabelSizeX
        {
            get { return _LabelSizeX; }
            set { _LabelSizeX = value; }
        }
        private float _LabelSizeY = 1;
        ///<summary>标签X缩放系数</summary>
        public float LabelSizeY
        {
            get { return _LabelSizeY; }
            set { _LabelSizeY = value; }
        }

        private Color _LabelColor = Colors.Red;
        ///<summary>标签文字色</summary>
        public Color LabelColor
        {
            get { return _LabelColor; }
            set { _LabelColor = value; }
        }



        pText LabelObject;

        void CreateLabelObject()
        {
            LabelObject = new pText(parent) { id = id + "内置标签", VecLocation = VecLocation, isH = true, textureid = LabelPanelKey, scaleX = LabelSizeX, scaleY = LabelSizeY, color = LabelColor };
            //进行位置调整
            if (this is pSymbolObject)
            {
                Vector3D vec3 = new Vector3D(VecLocation.x, VecLocation.y, VecLocation.z);
                Vector3D axis = Vector3D.CrossProduct(vec3, new Vector3D(0, 1, 0));

                //暂限定为单行
                double height = 3.0 * 0.0075f * LabelObject.scaleY;
                double angle = -Math.Atan(height / Para.Radius) * 2;
                RotateTransform3D rotate = new RotateTransform3D(new AxisAngleRotation3D(axis, angle * 180 / Math.PI));
                Vector3D vvv = rotate.Transform(vec3);
                LabelObject.VecLocation = new VECTOR3D((float)vvv.X, (float)vvv.Y, (float)vvv.Z);
            }
        }

        #endregion

        #region  ===================== 绘制深度相关 =======================================
        ///<summary>辅助深度绘制用的纬度</summary>
        public double latitude;

        ///<summary>离地高度, 需在设置location前设置，location设置的同时计算内部坐标</summary>
        public double groundHeight;

        ///<summary>类型排序号，地面类和几何体类, 限0-20, 缺省为0</summary>
        public int typeOrd = 0;



        private int _deepOrd = 0;
        ///<summary>深度序, 按从小到大顺序绘制</summary>
        public int deepOrd
        {
            get
            {
                //2147483648
                _deepOrd = typeOrd * 100000000 + (int)(groundHeight * 10000000) + (int)((90 - latitude) * 10000);
                //_deepOrd = -_deepOrd;
                return _deepOrd;
            }
        }

        #endregion

        #region 可见性相关
        ///<summary>可视最大距离，缺省100，即相机远端距离，全可见</summary>
        public double visualMaxDistance = 100;
        ///<summary>可视最小距离，缺省0</summary>
        public double visualMinDistance = 0;

        ///<summary>检查可见性, curRange: 相机可见经纬范围, curCameraDistance：相机离地距离</summary>
        internal abstract void checkVisiualization();
        #endregion


        public double Progress //控制绘制进度
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(PowerBasicObject), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnProgressChanged)));
        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PowerBasicObject sh = (PowerBasicObject)d;
            sh.onchangeprogress();
        }
        protected virtual void onchangeprogress()
        {
        }


        #region ========== tooltip相关 ==========

        ///<summary>mouse move形态的模板名</summary>
        public string tooltipMoveTemplate { get; set; }
        ///<summary>mouse move形态的内容</summary>
        public object tooltipMoveContent { get; set; }

        ///<summary>mouse click形态的模板名</summary>
        public string tooltipClickTemplate { get; set; }
        ///<summary>mouse click形态的内容</summary>
        public object tooltipClickContent { get; set; }

        ///<summary>mouse right click形态的模板名</summary>
        public string tooltipRightClickTemplate { get; set; }
        ///<summary>mouse right click形态的内容</summary>
        public object tooltipRightClickContent { get; set; }
        #endregion


        ///<summary>虚函数，重新计算源位置并向D3D提交改变位置</summary>
        public virtual void sendChangedLocation() { }
        ///<summary>从系统内部3D坐标反向计算源坐标</summary>
        protected virtual void backCalLocation() { }

        ///<summary>强制再次向D3D提交刷新位置</summary>
        public virtual void refreshLocation() { }
    }

    ///<summary>========== 点类对象基类 ==========</summary>
    public class pDotObject : PowerBasicObject
    {
        public pDotObject(pLayer Parent)
            : base(Parent)
        {

        }


        #region 坐标数据

        private string _planeLocation;
        ///<summary>平面模式坐标，拟取消，由坐标系指定控制</summary>
        public string planeLocation
        {
            get { return _planeLocation; }
            set
            {
                _planeLocation = value;
                System.Windows.Point geopnt = geohelper.planeToGeo(value);
                location = geopnt.ToString(); //转置经纬度坐标
            }
        }


        protected string _location;
        ///<summary>输入坐标</summary>
        public string location
        {
            get
            {
                return _location;
            }
            set
            {
                _location = value;

                // 注：对于用线段表示的点对象，转换取其中心点
                string[] ss = value.Split(' ');
                if (ss.Count() > 1)
                {
                    double minx, maxx, miny, maxy;
                    minx = miny = double.PositiveInfinity;
                    maxx = maxy = double.NegativeInfinity;
                    for (int i = 0; i < ss.Length; i++)
                    {
                        Point tmp = System.Windows.Point.Parse(ss[i]);
                        minx = Math.Min(minx, tmp.X);
                        miny = Math.Min(miny, tmp.Y);
                        maxx = Math.Max(maxx, tmp.X);
                        maxy = Math.Max(maxy, tmp.Y);
                    }
                    center = new Point((minx + maxx) / 2, (miny + maxy) / 2);
                }
                else
                    center = System.Windows.Point.Parse(value);
                bounds = new System.Windows.Rect(center.X, center.Y, 0, 0);
                System.Windows.Point gp = center;
                if (parent.parent.earth.coordinateManager.Enable) gp = parent.parent.earth.coordinateManager.transToInner(center); //若激活了坐标转换，转换为内部坐标
                VecLocation = MapHelper.JWHToPoint(gp.Y, gp.X, groundHeight, parent.parent.earth.earthManager.earthpara);
                this.latitude = parent.parent.earth.coordinateManager.isXAsLong ? center.Y : center.X;
            }
        }

        internal override xna.BoundingBox boundingBox
        {
            get { return new xna.BoundingBox(new xna.Vector3(VecLocation.x, VecLocation.y, VecLocation.z), new xna.Vector3(VecLocation.x, VecLocation.y, VecLocation.z)); }
        }

        #endregion




        internal override void checkVisiualization()
        {
            if (parent.parent.earth.objManager.ObjectCheckMode == ECheckMode.经纬检查)
            {
                Camera.ViewRange curRange = parent.parent.earth.camera.curCamearaViewRange;
                float curCameraDistance = parent.parent.earth.camera.curCameraDistanceToGround;

                if (curCameraDistance > visualMinDistance && curCameraDistance < visualMaxDistance)
                {
                    //判断条件二：经纬范围
                    bool isIntersect = false;
                    foreach (System.Windows.Rect range in curRange.rangesOuter)
                    {
                        if (range.Contains(center))
                        {
                            isIntersect = true;
                            break;
                        }
                    }
                    rangeVisibility = isIntersect;
                }
                else
                    rangeVisibility = false;
            }
            else if (parent.parent.earth.objManager.ObjectCheckMode == ECheckMode.视锥体检查)
            {
                float curCameraDistance = parent.parent.earth.camera.curCameraDistanceToGround;
                if (curCameraDistance > visualMinDistance && curCameraDistance < visualMaxDistance)
                    rangeVisibility = parent.parent.earth.camera.cameraFrustum.Intersects(boundingBox);
                else
                    rangeVisibility = false;
            }
        }



        ///<summary>从VecLocation计算新的源位置，并向D3D提交改变位置 </summary>
        public override void sendChangedLocation()
        {
            backCalLocation();
            refreshLocation();
        }

        public override void refreshLocation()
        {
            updateProperty(VecLocation, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.位置, hashcode, hashcode);
        }


        ///<summary>从VecLocation回存Location</summary>
        protected override void backCalLocation()
        {
            System.Windows.Media.Media3D.Point3D p3d = new Point3D(VecLocation.x, VecLocation.y, VecLocation.z);
            System.Windows.Media.Media3D.Point3D jwh = Helpler.PointToJWH(p3d, parent.parent.earth.earthManager.earthpara);
            System.Windows.Point pnt = new System.Windows.Point(jwh.Y, jwh.X);
            if (parent.parent.earth.coordinateManager.Enable) pnt = parent.parent.earth.coordinateManager.transToOuter(pnt);  //若启用了坐标转换，转换为外部坐标
            _location = pnt.ToString();  //源输入坐标
            groundHeight = jwh.Z;  //源之离地高度
            center = pnt; //可视性检查属性
        }

    }

    ///<summary>========== 范围类对象基类 ==========</summary>
    public class pRangeObject : PowerBasicObject
    {
        public pRangeObject(pLayer Parent)
            : base(Parent)
        {

        }


        #region 坐标数据

        ///<summary>区域类定点坐标，只供其它对象引用</summary>
        public string location { get; set; }

        private string _planeStrPoints;
        ///<summary>平面模式坐标, 拟取消, 由模式控制</summary>
        public string planeStrPoints
        {
            get { return _planeStrPoints; }
            set
            {
                _planeStrPoints = value;

                string[] ss = value.Split(' ');
                //List<Point> geos = new List<Point>(); ;// new System.Windows.Point[ss.Length];
                PointCollection geos = new PointCollection();
                System.Windows.Point geopnt = new System.Windows.Point();
                for (int i = 0; i < ss.Length; i++)
                {
                    geopnt = geohelper.planeToGeo(ss[i]);
                    geos.Add(geopnt); //geos[i] = geopnt;
                }
                latitude = geopnt.X; //记录最后一点的续度，以做深度顺序 

                //geoPoints = geos;
                strPoints = geos.ToString();
            }
        }

        protected string _strPoints;
        ///<summary>经纬度坐标</summary>
        public virtual string strPoints
        {
            get
            {
                return _strPoints;
            }
            set
            {
                _strPoints = value;
                calPoints();
                calCenterBound();
                PointCollection transedpoins = calTransPoints();
                calVecPoints(transedpoins);
            }

        }

        ///<summary>若为3D立体折线，需在设置strPoints前设置各点的离地高度</summary>
        public string strGroundHeights { get; set; }

        private PointCollection _points;
        ///<summary>原始坐标点集</summary>
        public PointCollection points
        {
            get { return _points; }
            set { _points = value; }
        }

        ///<summary>从字串到点集，生成points</summary>
        protected void calPoints()
        {
            string[] ss = strPoints.Split(' ');
            points = new PointCollection();
            for (int i = 0; i < ss.Length; i++)
                points.Add(System.Windows.Point.Parse(ss[i]));

            //处理不合格点和多余点
            if (parent.parent.isCheckPoints && points.Count > 2)
            {
                int tmppntidx = 1;
                while (points.Count > tmppntidx)
                {
                    if (points.Count == 2)  //只余两个点时，强制退出，避免线路只有一个点
                        break;
                    if ((points[tmppntidx] - points[tmppntidx - 1]).Length < parent.parent.checkPointsMinLength)
                        points.RemoveAt(tmppntidx);
                    else if (tmppntidx < points.Count - 1 && Math.Abs(Vector.AngleBetween(points[tmppntidx - 1] - points[tmppntidx], points[tmppntidx + 1] - points[tmppntidx])) > 178)
                        points.RemoveAt(tmppntidx);
                    else
                        tmppntidx++;
                }

            }


        }

        ///<summary>根据points, 返回转换后坐标点集</summary>
        protected PointCollection calTransPoints()
        {
            if (parent.parent.earth.coordinateManager.Enable)
            {
                PointCollection geos = new PointCollection();
                foreach (Point pnt in points)
                {
                    Point tpnt = parent.parent.earth.coordinateManager.transToInner(pnt); //若激活了坐标转换，转换为内部坐标
                    geos.Add(tpnt);
                }
                return geos;
            }
            else
                return points;

        }

        ///<summary>计算中心点范围等</summary>
        protected void calCenterBound()
        {
            latitude = parent.parent.earth.coordinateManager.isXAsLong ? points.Last().Y : points.Last().X; //记录最后一点的续度，以做深度顺序 
            //计算定点坐标
            double alllen = 0;
            double len = 0;
            for (int i = 0; i < points.Count - 1; i++)
                alllen += (points[i + 1] - points[i]).Length;
            for (int i = 0; i < points.Count - 1; i++)
            {
                double tmp = (points[i + 1] - points[i]).Length;
                if ((len + tmp) / alllen > 0.5)
                {
                    double distance = alllen / 2 - len;
                    System.Windows.Vector vecdir = (points[i + 1] - points[i]);
                    vecdir.Normalize();
                    center = points[i] + distance * vecdir;
                    location = center.ToString();
                    break;
                }
                else
                    len += tmp;
            }
            //计算范围
            bounds = new System.Windows.Rect(new System.Windows.Point(points.Min(p => p.X), points.Min(p => p.Y)), new System.Windows.Point(points.Max(p => p.X), points.Max(p => p.Y)));

            System.Windows.Point jwcenter = center;
            if (parent.parent.earth.coordinateManager.Enable) jwcenter = parent.parent.earth.coordinateManager.transToInner(jwcenter); //若激活了坐标转换，转换为内部坐标
            VecLocation = MapHelper.JWHToPoint(jwcenter.Y, jwcenter.X, Para.LineHeight, parent.parent.earth.earthManager.earthpara);

        }

        ///<summary>转换为三维空间坐标</summary>
        protected void calVecPoints(PointCollection transedPoints)
        {
            List<double> pcgh = null;//高度点集 
            if (!String.IsNullOrWhiteSpace(strGroundHeights))
            {
                pcgh = new List<double>();
                string[] ss = strGroundHeights.Split(',');
                for (int i = 0; i < ss.Length; i++)
                    pcgh.Add(double.Parse(ss[i]));
            }

            VecPoints = new List<VECTOR3D>();// new VECTOR3D[_geoPoints.Length];
            for (int i = 0; i < transedPoints.Count; i++)
            {
                //VecPoints[i] = MapHelper.JWHToPoint(_geoPoints[i].Y, _geoPoints[i].X, groundHeight, parent.parent.earth.earthManager.earthpara);
                double gh;
                if (pcgh == null || pcgh.Count < i) //无高度点集，则认为是高度是同为groundHeight的折线
                    gh = groundHeight;
                else
                    gh = pcgh[i];
                VecPoints.Add(MapHelper.JWHToPoint(transedPoints[i].Y, transedPoints[i].X, gh, parent.parent.earth.earthManager.earthpara));
            }
        }


        ///<summary>对象在三D坐标中的点集</summary>
        public List<VECTOR3D> VecPoints;

        xna.BoundingBox? _boundingBox = null;
        internal override xna.BoundingBox boundingBox
        {
            get
            {
                if (_boundingBox == null)
                    _boundingBox = new xna.BoundingBox(new xna.Vector3(VecPoints.Min(p => p.x), VecPoints.Min(p => p.y), VecPoints.Min(p => p.z)), new xna.Vector3(VecPoints.Max(p => p.x), VecPoints.Max(p => p.y), VecPoints.Max(p => p.z)));
                return (xna.BoundingBox)_boundingBox;
            }
        }
        #endregion


        internal override void checkVisiualization()
        {
            if (parent.parent.earth.objManager.ObjectCheckMode == ECheckMode.经纬检查)
            {
                Camera.ViewRange curRange = parent.parent.earth.camera.curCamearaViewRange;
                float curCameraDistance = parent.parent.earth.camera.curCameraDistanceToGround;
                if (curCameraDistance > visualMinDistance && curCameraDistance < visualMaxDistance)
                {
                    //判断条件二：经纬范围
                    bool isIntersect = false;
                    foreach (System.Windows.Rect range in curRange.rangesOuter)
                    {
                        if (bounds.IntersectsWith(range))
                        {
                            isIntersect = true;
                            break;
                        }
                    }
                    rangeVisibility = isIntersect;
                }
                else
                    rangeVisibility = false;
            }
            else if (parent.parent.earth.objManager.ObjectCheckMode == ECheckMode.视锥体检查)
            {
                float curCameraDistance = parent.parent.earth.camera.curCameraDistanceToGround;
                if (curCameraDistance > visualMinDistance && curCameraDistance < visualMaxDistance)
                    rangeVisibility = parent.parent.earth.camera.cameraFrustum.Intersects(boundingBox);
                else
                    rangeVisibility = false;
            }


        }



        ///<summary>从VecLocation回存Location</summary>
        protected override void backCalLocation()
        {
            points.Clear();
            string strheight = "";
            for (int i = 0; i < VecPoints.Count(); i++)
            {
                System.Windows.Media.Media3D.Point3D p3d = new Point3D(VecPoints[i].x, VecPoints[i].y, VecPoints[i].z);
                System.Windows.Media.Media3D.Point3D jwh = Helpler.PointToJWH(p3d, parent.parent.earth.earthManager.earthpara);
                System.Windows.Point pnt = new System.Windows.Point(jwh.Y, jwh.X);
                if (parent.parent.earth.coordinateManager.Enable) pnt = parent.parent.earth.coordinateManager.transToOuter(pnt);  //若启用了坐标转换，转换为外部坐标
                points.Add(pnt);
                strheight += (i == 0 ? "" : ",") + jwh.Z.ToString();
            }
            _strPoints = points.ToString();  //源输入坐标
            strGroundHeights = strheight; //高度点集
            calCenterBound();


        }


    }

    ///<summary>========== 线路 ==========</summary>
    public class pPowerLine : pRangeObject
    {
        /// <summary>
        /// 线路对象
        /// </summary>
        /// <param name="Parent">所属的层</param>
        public pPowerLine(pLayer Parent)
            : base(Parent)
        {
            groundHeight = Para.LineHeight;
        }


        #region 呈现控制

        #region ----- 色彩管理相关 -----
        public enum ECType { _通用, 高压, 中压, 低压, 连接线, 引线 }
        public enum ECStatus { _正常, 选择, 过载, 轻载, 检修, 故障, 停电, 测试, 建设, 规划, 退运, 自定1, 自定2, 自定3, 自定4, 自定5 }
        public bool isColorManaged = true;

        ECType _objType = ECType._通用;
        ///<summary>对象类型，静态</summary>
        public ECType objType
        {
            get { return _objType; }
            set { _objType = value; objTypeString = value.ToString(); }
        }
        private string _objTypeString = ECType._通用.ToString();
        ///<summary>对象类型字串，静态</summary>
        public string objTypeString
        {
            get { return _objTypeString; }
            set { _objTypeString = value; }
        }

        private ECStatus _objStatus = ECStatus._正常;

        ///<summary>对象状态，动态控制色彩</summary>
        public ECStatus objStatus
        {
            get { return _objStatus; }
            set
            {
                _objStatus = value;
                objStatusString = value.ToString();
            }
        }

        private string _objStatusString = ECStatus._正常.ToString();
        ///<summary>对象状态</summary>
        public string objStatusString
        {
            get { return _objStatusString; }
            set
            {
                if (parent.parent.earth.colorManager.isEnabled && isColorManaged && _objStatusString != value)
                {
                    color = parent.parent.earth.colorManager.getColor(parent.parent.earth.earthManager.mapType, ColorManager.ECObjectType.折线, objTypeString, value);
                }
                _objStatusString = value;

            }
        }
        #endregion


        //public string styleid;

        private Color _arrowColor = Colors.Blue;
        ///<summary>箭头颜色</summary>
        public Color arrowColor
        {
            get { return _arrowColor; }
            set
            {
                _arrowColor = value;
                //updateProperty(value, parent.parent.earth.earthkey, EModelType.潮流, EPropertyType.颜色, hashcode, hashcode);
            }
        }

        private float _arrowSize = 0.01f;
        ///<summary>箭头大小</summary>
        public float arrowSize
        {
            get { return _arrowSize; }
            set
            {
                _arrowSize = value;
                updateProperty(value, parent.parent.earth.earthkey, EModelType.潮流, EPropertyType.大小, hashcode, hashcode);
            }
        }




        private bool _isFlow;
        ///<summary>是否显示潮流箭头</summary>
        public bool isFlow
        {
            get { return _isFlow; }
            set
            {
                _isFlow = value;


                aniFlow.isDoAni = value;
                updateProperty(aniFlow, parent.parent.earth.earthkey, EModelType.潮流, EPropertyType.动画, hashcode, hashcode);
            }
        }

        private bool _isInverse;
        ///<summary>潮流是否反向</summary>
        public bool isInverse
        {
            get { return _isInverse; }
            set
            {
                _isInverse = value;
                updateProperty(value, parent.parent.earth.earthkey, EModelType.潮流, EPropertyType.方向, hashcode, hashcode);
            }
        }


        private float _thickness = 0.002f;
        ///<summary>线宽</summary>
        public float thickness
        {
            get { return _thickness; }
            set
            {
                _thickness = value;
                if (isShowObject)
                    updateProperty(value, parent.parent.earth.earthkey, EModelType.折线, EPropertyType.宽度, hashcode, hashcode);
            }
        }


        private int _radCount = 4;
        ///<summary>管线断面分割数，缺省4</summary>
        public int radCount
        {
            get { return _radCount; }
            set { _radCount = value; }
        }


        ///<summary>是否使用动态线宽</summary>
        public bool dynLineWidthEnable { get; set; }

        ///<summary>缺省线宽，使用动态线宽时必须进行设置</summary>
        public float defaultThickness { get; set; }

        ///<summary>缺省箭头大小，使用动态线宽时必须进行设置</summary>
        public float defaultArrowSize { get; set; }

        #endregion

        #region 动画相关
        public enum EAnimationType { 绘制, 擦除, 闪烁 };

        ///<summary>提交开始执行一个动画，动画参数由相应的AniDraw, AniTwinkle结构决定(绘制和擦除不相容，相互覆盖)</summary>
        public void AnimationBegin(EAnimationType anitype)
        {
            switch (anitype)
            {
                case EAnimationType.绘制:
                    aniDraw.isDoAni = true;
                    aniDraw.aniType = EAniType.绘制;
                    updateProperty(aniDraw, parent.parent.earth.earthkey, EModelType.折线, EPropertyType.动画, hashcode, hashcode);
                    aniDraw.isDoAni = false;
                    break;
                case EAnimationType.擦除:
                    aniDraw.isDoAni = true;
                    aniDraw.aniType = EAniType.擦除;
                    updateProperty(aniDraw, parent.parent.earth.earthkey, EModelType.折线, EPropertyType.动画, hashcode, hashcode);
                    aniDraw.isDoAni = false;
                    break;
                case EAnimationType.闪烁:
                    aniTwinkle.isDoAni = true;
                    updateProperty(aniTwinkle, parent.parent.earth.earthkey, EModelType.折线, EPropertyType.动画, hashcode, hashcode);
                    aniTwinkle.isDoAni = false;
                    break;
            }

        }
        ///<summary>提交停止一个动画</summary>
        public void AnimationStop(EAnimationType anitype)
        {
            switch (anitype)
            {
                case EAnimationType.绘制:
                    aniDraw.isDoAni = false;
                    aniDraw.aniType = EAniType.绘制;
                    updateProperty(aniDraw, parent.parent.earth.earthkey, EModelType.折线, EPropertyType.动画, hashcode, hashcode);
                    break;
                case EAnimationType.擦除:
                    aniDraw.isDoAni = false;
                    aniDraw.aniType = EAniType.擦除;
                    updateProperty(aniDraw, parent.parent.earth.earthkey, EModelType.折线, EPropertyType.动画, hashcode, hashcode);
                    break;
                case EAnimationType.闪烁:
                    aniTwinkle.isDoAni = false;
                    updateProperty(aniTwinkle, parent.parent.earth.earthkey, EModelType.折线, EPropertyType.动画, hashcode, hashcode);
                    break;
            }

        }
        ///<summary>提交动画参数，可用于控制动画播放、停止和刷新动画的参数</summary>
        public void SendAniPara(STRUCT_Ani aniPara)
        {
            updateProperty(aniPara, parent.parent.earth.earthkey, EModelType.折线, EPropertyType.动画, hashcode, hashcode);
        }

        ///<summary>控制绘制和擦除动画的参数</summary>
        public STRUCT_Ani aniDraw = new STRUCT_Ani() { aniType = EAniType.绘制, duration = 5000, doCount = 1 };

        ///<summary>控制潮流动画的参数</summary>
        public STRUCT_Ani aniFlow = new STRUCT_Ani() { aniType = EAniType.潮流动画, duration = 10000 };

        ///<summary>控制闪烁动画的参数</summary>
        public STRUCT_Ani aniTwinkle = new STRUCT_Ani() { aniType = EAniType.闪烁, duration = 200, doCount = 0, isReverse = true };

        #endregion


        ///<summary>部份绘制的进度</summary>
        protected override void onchangeprogress()
        {
            base.onchangeprogress();

            updateProperty((float)Progress, parent.parent.earth.earthkey, EModelType.折线, EPropertyType.进度, hashcode, hashcode);
        }


        ///<summary>从VecLocation计算新的源位置，并向D3D提交改变位置 </summary>
        public override void sendChangedLocation()
        {
            backCalLocation();
            refreshLocation();
        }

        public override void refreshLocation()
        {
            int count = VecPoints.Count;
            IntPtr ipData = Marshal.AllocCoTaskMem(Marshal.SizeOf(VecPoints[0]) * count);  //传递点序列结构数组指针
            for (int i = 0; i < count; i++)
            {
                Marshal.StructureToPtr(VecPoints[i], (IntPtr)(ipData.ToInt32() + i * Marshal.SizeOf(VecPoints[i])), false);
            }
            D3DManager.ChangeProperty(parent.parent.earth.earthkey, (int)EModelType.折线, (int)EPropertyType.位置, hashcode, hashcode, ipData, count, IntPtr.Zero, 0);
            Marshal.FreeCoTaskMem(ipData);

        }

    }

    ///<summary>图元对象</summary>
    public class pSymbolObject : pDotObject
    {
        /// <summary>
        /// 厂站图元对象
        /// </summary>
        /// <param name="Parent">所属的层</param>
        public pSymbolObject(pLayer Parent)
            : base(Parent)
        {
            groundHeight = Para.SymbolHeight;
        }
        ///<summary>关联对象ID</summary>
        public List<string> relationID = new List<string>();



        #region 呈现控制

        #region ----- 色彩管理相关 -----
        public enum ECType { _通用, 变电高压, 变电中压, 变电低压, 开关高压, 开关中压, 开关低压, 杆塔 }
        public enum ECStatus { _正常, 选择, 过载, 轻载, 检修, 故障, 停电, 测试, 建设, 规划, 退运, 闭合, 断开, 自定1, 自定2, 自定3, 自定4, 自定5 }
        public bool isColorManaged = true;

        ECType _objType = ECType._通用;
        ///<summary>对象类型，静态</summary>
        public ECType objType
        {
            get { return _objType; }
            set { _objType = value; objTypeString = value.ToString(); }
        }


        private string _objTypeString = ECType._通用.ToString();
        ///<summary>对象类型字串，静态</summary>
        public string objTypeString
        {
            get { return _objTypeString; }
            set { _objTypeString = value; }
        }

        private ECStatus _objStatus = ECStatus._正常;
        ///<summary>对象状态，动态控制色彩</summary>
        public ECStatus objStatus
        {
            get { return _objStatus; }
            set
            {
                _objStatus = value;
                objStatusString = value.ToString();
            }
        }

        private string _objStatusString = ECStatus._正常.ToString();
        ///<summary>对象状态</summary>
        public string objStatusString
        {
            get { return _objStatusString; }
            set
            {
                if (parent.parent.earth.colorManager.isEnabled && isColorManaged && _objStatusString != value)
                {
                    color = parent.parent.earth.colorManager.getColor(parent.parent.earth.earthManager.mapType, ColorManager.ECObjectType.图元, objTypeString, value);
                }
                _objStatusString = value;

            }
        }

        #endregion



        string _symbolid;
        public string symbolid
        {
            get { return _symbolid; }
            set
            {
                _symbolid = value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (parent.parent != null)
                        updateProperty(value.GetHashCode(), parent.parent.earth.earthkey, EModelType.图元, EPropertyType.内容, hashcode, hashcode);
                }
            }
        }

        ///<summary>brush转换为纹理的大小，缺省64</summary>
        public int brushsize = 64;

        Brush _brush;
        public Brush brush
        {
            get { return _brush; }
            set
            {
                _brush = value;
                if (value != null)
                {
                    sendD3D();
                }
            }
        }

        ///<summary>发送新的纹理更新到d3d</summary>
        void sendD3D()
        {
            if (parent == null || parent.parent == null) return;  //若还没初始化，不发送
            //纹理数据
            int size = brushsize;
            Rectangle rec = new System.Windows.Shapes.Rectangle();
            rec.Fill = _brush;
            rec.Width = rec.Height = size;
            rec.Measure(new System.Windows.Size(size, size));
            rec.Arrange(new System.Windows.Rect(0, 0, size, size));


            RenderTargetBitmap renderTarget = new RenderTargetBitmap(size, size, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            renderTarget.Render(rec);
            renderTarget.Freeze();

            byte[] bitmaps;
            int tcount;
            IntPtr ipTexture;
            using (MemoryStream ms = new MemoryStream())
            {
                PngBitmapEncoder encode = new PngBitmapEncoder();
                encode.Frames.Add(BitmapFrame.Create(renderTarget));
                encode.Save(ms);
                bitmaps = ms.GetBuffer();

                tcount = bitmaps.Length;
                ipTexture = Marshal.AllocCoTaskMem(Marshal.SizeOf(bitmaps[0]) * tcount);  //传递纹理数据数组指针
                for (int i = 0; i < tcount; i++)
                {
                    Marshal.StructureToPtr(bitmaps[i], (IntPtr)(ipTexture.ToInt32() + i * Marshal.SizeOf(bitmaps[i])), false);
                }
            }

            D3DManager.ChangeProperty(parent.parent.earth.earthkey, (int)EModelType.图元, (int)EPropertyType.纹理, hashcode, hashcode, ipTexture, tcount, IntPtr.Zero, 0);
            Marshal.FreeCoTaskMem(ipTexture);

        }


        //private Color _color = Colors.Gray;
        //public Color color
        //{
        //    get { return _color; }
        //    set
        //    {
        //        _color = value;
        //        updateProperty(value, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.颜色, hashcode, hashcode);
        //    }
        //}


        public bool isUseColor = true;

        private float _scaleX = 0.025f;
        ///<summary>图形的尺寸系数</summary>
        public float scaleX
        {
            get { return _scaleX; }
            set { _scaleX = value; if (parent != null && parent.parent != null) updateProperty(value, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.长度, hashcode, hashcode); }
        }

        private float _scaleY = 0.025f;
        ///<summary>图形的尺寸系数</summary>
        public float scaleY
        {
            get { return _scaleY; }
            set { _scaleY = value; if (parent != null && parent.parent != null) updateProperty(value, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.宽度, hashcode, hashcode); }
        }

        private float _scaleZ = 0.025f;
        ///<summary>图形的尺寸系数</summary>
        public float scaleZ
        {
            get { return _scaleZ; }
            set { _scaleZ = value; if (parent != null && parent.parent != null) updateProperty(value, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.高度, hashcode, hashcode); }
        }

        private bool _isH = true;
        ///<summary>是否水平放置</summary>
        public bool isH
        {
            get { return _isH; }
            set { _isH = value; }
        }



        private bool _isUseXModel;
        ///<summary>是否使用3D模型</summary>
        public bool isUseXModel
        {
            get { return _isUseXModel; }
            set
            {
                _isUseXModel = value;
                if (parent != null && parent.parent != null)
                    updateProperty(value, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.类型, hashcode, hashcode);
            }
        }


        private string _XModelKey;
        ///<summary>3D模型字典中的键值</summary>
        public string XModelKey
        {
            get { return _XModelKey; }
            set { _XModelKey = value; }
        }

        private double _XMScaleAddition = 1;
        ///<summary>3D模型时，ScaleX Y X的附加比例</summary>
        public double XMScaleAddition
        {
            get { return _XMScaleAddition; }
            set { _XMScaleAddition = value; }
        }



        #endregion

        #region 动画相关
        public enum EAnimationType { 闪烁, 缩放 };
        ///<summary>提交开始执行一个动画</summary>
        public void AnimationBegin(EAnimationType anitype)
        {
            switch (anitype)
            {
                case EAnimationType.闪烁:
                    aniTwinkle.isDoAni = true;
                    updateProperty(aniTwinkle, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.动画, hashcode, hashcode);
                    aniTwinkle.isDoAni = false;
                    break;
                case EAnimationType.缩放:
                    aniScale.isDoAni = true;
                    updateProperty(aniScale, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.动画, hashcode, hashcode);
                    aniScale.isDoAni = false;
                    break;
            }

        }
        ///<summary>提交停止一个动画</summary>
        public void AnimationStop(EAnimationType anitype)
        {
            switch (anitype)
            {
                case EAnimationType.闪烁:
                    aniTwinkle.isDoAni = false;
                    updateProperty(aniTwinkle, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.动画, hashcode, hashcode);
                    break;
                case EAnimationType.缩放:
                    aniScale.isDoAni = false;
                    updateProperty(aniScale, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.动画, hashcode, hashcode);
                    break;
            }

        }
        ///<summary>提交动画参数，可用于控制动画播放、停止和刷新动画的参数</summary>
        public void SendAniPara(STRUCT_Ani aniPara)
        {
            updateProperty(aniPara, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.动画, hashcode, hashcode);
        }

        ///<summary>控制闪烁动画的参数</summary>
        public STRUCT_Ani aniTwinkle = new STRUCT_Ani() { aniType = EAniType.闪烁, duration = 200, doCount = 0, isReverse = true };

        ///<summary>控制显示动画的参数</summary>
        public STRUCT_Ani aniShow = new STRUCT_Ani() { aniType = EAniType.渐变, duration = 1000, doCount = 1 };

        ///<summary>控制缩放动画的参数</summary>
        public STRUCT_Ani aniScale = new STRUCT_Ani() { aniType = EAniType.缩放, duration = 2000, doCount = 0 };

        #endregion


        ///<summary>部份绘制的进度</summary>
        protected override void onchangeprogress()
        {
            base.onchangeprogress();

            updateProperty((float)Progress, parent.parent.earth.earthkey, EModelType.图元, EPropertyType.进度, hashcode, hashcode);
        }




    }

    ///<summary>文字对象</summary>
    public class pText : pDotObject
    {
        /// <summary>
        /// 文字对象
        /// </summary>
        /// <param name="Parent">所属的层</param>
        public pText(pLayer Parent)
            : base(Parent)
        {
            groundHeight = Para.TextHeight;
        }



        #region 呈现控制
        public string text = "";

        //private Color _color = Colors.Red;
        /////<summary>文字色</summary>
        //public Color color
        //{
        //    get { return _color; }
        //    set { _color = value; }
        //}


        private float _scaleX = 1f;
        ///<summary>文字在X方向的缩放系数</summary>
        public float scaleX
        {
            get { return _scaleX; }
            set { _scaleX = value; }
        }

        private float _scaleY = 1;
        ///<summary>文字在Y方向的缩放系数</summary>
        public float scaleY
        {
            get { return _scaleY; }
            set { _scaleY = value; }
        }


        private bool _isH;
        ///<summary>是否水平放置</summary>
        public bool isH
        {
            get { return _isH; }
            set
            {
                _isH = value;
                typeOrd = value ? 0 : 1;
            }
        }

        ///<summary>设置后，可使用预存在材质库的材质为文字背景</summary>
        public string textureid;
        #endregion


        public STRUCT_Ani aniTwinkle = new STRUCT_Ani() { aniType = EAniType.闪烁, duration = 200, doCount = 0, isReverse = true };

    }


    ///<summary>区域</summary>
    public class pArea : pRangeObject
    {
        public pArea(pLayer Parent)
            : base(Parent)
        {
            groundHeight = Para.AreaHeight;
        }

        #region 坐标数据



        public override string strPoints
        {
            get
            {
                return base.strPoints;
            }
            set
            {
                base.strPoints = value;

                buildMesh();
            }
        }



        internal ushort[] indexes;


        ///<summary>创建mesh</summary>
        void buildMesh()
        {
            //VECTOR3D AreaCenter;
            bool isClockwise;
            PointCollection SimplePoints = new PointCollection();//临时使用点集

            isClockwise = calClockWise(points);

            //生成经纬简化序列
            float minlen = 0;// 0.0008f; //最小距离，小于的被忽略
            System.Windows.Point pold, pnew;
            pold = points[0];
            SimplePoints.Add(pold);
            for (int i = 1; i < VecPoints.Count; i++)
            {
                pnew = points[i];
                if ((pnew - pold).Length > minlen)
                {
                    if (isClockwise)
                        SimplePoints.Add(pnew);
                    else
                        SimplePoints.Insert(0, pnew);
                    pold = pnew;
                }

            }
            if (SimplePoints[0] == SimplePoints.Last())
                SimplePoints.Remove(SimplePoints.Last());
            // simplepoints为新的点集
            //重写str与geopoint，以便序号与vecpoints一致
            points = SimplePoints;
            _strPoints = points.ToString();
            calCenterBound();

            //重新转换点
            PointCollection transedpoint = calTransPoints();

            //转换为3D点
            calVecPoints(transedpoint);


            bool isClockwiseNew = calClockWise(transedpoint);
            if (isClockwise != isClockwiseNew) //重建转换后点的顺序方向若不一致，则反转点序
            {
                PointCollection newtransedpoint = new PointCollection();
                for (int i = 0; i < transedpoint.Count; i++)
                {
                        newtransedpoint.Insert(0, transedpoint[i]);
                }
                transedpoint = newtransedpoint;
            }


            //===================================================== 
            // 计算顶点
            //=====================================================

            List<ushort> pidx = new List<ushort>();
            for (ushort i = 0; i < transedpoint.Count; i++)
            {
                pidx.Add(i);
            }

            List<ushort> tri = new List<ushort>();
            int tuidx = 0, tmp = 0;
            System.Windows.Point p1, p2, p3;
            System.Windows.Vector v1, v2;
            int calcount = 0;
            while (pidx.Count > 2)
            {
                if (calcount > 10000)
                    break;
                calcount++;
                p1 = transedpoint[pidx[tuidx]];
                p2 = transedpoint[pidx[tuidx + 1]];
                p3 = transedpoint[pidx[tuidx + 2]];
                v1 = (p1 - p2);
                v2 = p3 - p2;
                double ang = Vector.AngleBetween(v1, v2);
                if (ang <= 0)
                {
                    tuidx = tuidx + 1;
                }
                else
                {
                    bool iscontain = false;

                    for (int i = 0; i < pidx.Count; i++)
                    {
                        System.Windows.Point testpoint = transedpoint[pidx[i]];
                        if (testpoint != p1 && testpoint != p2 && testpoint != p3)
                        {
                            System.Windows.Vector vv1 = testpoint - p1, vv2 = testpoint - p2, vv3 = testpoint - p3;
                            double anglesum = Math.Abs(System.Windows.Vector.AngleBetween(vv2, vv1)) + Math.Abs(System.Windows.Vector.AngleBetween(vv1, vv3)) + Math.Abs(System.Windows.Vector.AngleBetween(vv3, vv2));
                            if (Math.Abs(anglesum - 360) < 0.01)
                            {
                                iscontain = true;
                                break;
                            }
                        }
                    }

                    if (iscontain)
                        tuidx++;
                    else
                    {
                        tri.Add(pidx[tuidx + 0]);
                        tri.Add(pidx[tuidx + 1]);
                        tri.Add(pidx[tuidx + 2]);
                        //tri.Add(pidx[tuidx + 2]);
                        //tri.Add(pidx[tuidx + 1]);
                        //tri.Add(pidx[tuidx]);
                        pidx.RemoveAt(tuidx + 1);
                        tuidx = tuidx + 1;
                        tmp++;
                    }
                }
                if (tuidx + 2 > pidx.Count - 1)
                {
                    tuidx = 0;
                }

            }


            indexes = new ushort[tri.Count];
            for (int i = 0; i < tri.Count; i++)
            {
                indexes[i] = tri[i];
            }

            string s = "";
            for (int i = 0; i < indexes.Length; i++)
            {
                //s += points[i].x.ToString()+","+points[i].y.ToString()+","+points[i].z.ToString()+" ";
                s += indexes[i].ToString() + " ";
            }



        }


        bool calClockWise(PointCollection p)
        {
            int n = p.Count;

            int i, j, k;

            int count = 0;

            double z;



            if (n < 3)

                return true;



            for (i = 0; i < n; i++)
            {

                j = (i + 1) % n;

                k = (i + 2) % n;

                z = (p[j].X - p[i].X) * (p[k].Y - p[j].Y);

                z -= (p[j].Y - p[i].Y) * (p[k].X - p[j].X);

                if (z < 0)

                    count--;

                else if (z > 0)

                    count++;

            }

            if (count > 0)

                return false;

            else if (count < 0)

                return true;

            else

                return false;

        }


        #endregion


        #region 呈现控制
        #region ----- 色彩管理相关 -----
        public enum ECType { _通用, 供电区域, 行政区域 }
        public enum ECStatus { _正常, 选择, 供电, 断电, 自定1, 自定2, 自定3, 自定4, 自定5 }
        public bool isColorManaged = true;

        ECType _objType = ECType._通用;
        ///<summary>对象类型，静态</summary>
        public ECType objType
        {
            get { return _objType; }
            set { _objType = value; objTypeString = value.ToString(); }
        }
        private string _objTypeString = ECType._通用.ToString();
        ///<summary>对象类型字串，静态</summary>
        public string objTypeString
        {
            get { return _objTypeString; }
            set { _objTypeString = value; }
        }

        private ECStatus _objStatus = ECStatus._正常;
        ///<summary>对象状态，动态控制色彩</summary>
        public ECStatus objStatus
        {
            get { return _objStatus; }
            set
            {
                _objStatus = value;
                objStatusString = value.ToString();
            }
        }

        private string _objStatusString = ECStatus._正常.ToString();
        ///<summary>对象状态</summary>
        public string objStatusString
        {
            get { return _objStatusString; }
            set
            {
                if (parent.parent.earth.colorManager.isEnabled && isColorManaged && _objStatusString != value)
                {
                    color = parent.parent.earth.colorManager.getColor(parent.parent.earth.earthManager.mapType, ColorManager.ECObjectType.区域, objTypeString, value);
                }
                _objStatusString = value;

            }
        }
        #endregion



        //private Color _areaColor;
        /////<summary>区域颜色</summary>
        //public Color areaColor
        //{
        //    get { return _areaColor; }
        //    set
        //    {
        //        _areaColor = value;
        //        updateProperty(value, parent.parent.earth.earthkey, EModelType.区域, EPropertyType.颜色, hashcode, hashcode);
        //    }
        //}

        #endregion


        public STRUCT_Ani aniShow = new STRUCT_Ani() { aniType = EAniType.渐变, duration = 1000, doCount = 1 };



        ///<summary>从VecLocation计算新的源位置，并向D3D提交改变位置 </summary>
        public override void sendChangedLocation()
        {
            backCalLocation();
            PointCollection transedpoins = calTransPoints();
            calVecPoints(transedpoins);
            buildMesh();

            refreshLocation();

        }

        public override void refreshLocation()
        {
            //mesh数据   
            int count = VecPoints.Count;
            IntPtr ipData = Marshal.AllocCoTaskMem(Marshal.SizeOf(VecPoints[0]) * count);  //传递点序列结构数组指针
            for (int i = 0; i < count; i++)
            {
                Marshal.StructureToPtr(VecPoints[i], (IntPtr)(ipData.ToInt32() + i * Marshal.SizeOf(VecPoints[i])), false);
            }
            //索引数据
            int idxcount = indexes.Length;
            IntPtr ipIndex = Marshal.AllocCoTaskMem(Marshal.SizeOf(indexes[0]) * idxcount);  //传递点序列结构数组指针
            for (int i = 0; i < idxcount; i++)
            {
                Marshal.StructureToPtr(indexes[i], (IntPtr)(ipIndex.ToInt32() + i * Marshal.SizeOf(indexes[i])), false);
            }
            D3DManager.ChangeProperty(parent.parent.earth.earthkey, (int)EModelType.区域, (int)EPropertyType.位置, hashcode, hashcode, ipData, count, ipIndex, idxcount);
            Marshal.FreeCoTaskMem(ipData);
            Marshal.FreeCoTaskMem(ipIndex);

        }

    }


    ///<summary>等值线区域</summary>
    public class pContour : pRangeObject
    {
        public pContour(pLayer Parent)
            : base(Parent)
        {
            groundHeight = Para.AreaHeight;
            isReceivePick = false;
        }
        #region 基本数据
        ///<summary>设置平面坐标</summary>
        public void setPlaneRange(double MinX, double MaxX, double MinY, double MaxY)
        {
            System.Windows.Point gmin = geohelper.planeToGeo(new System.Windows.Point(MinX, MaxY).ToString());
            System.Windows.Point gmax = geohelper.planeToGeo(new System.Windows.Point(MaxX, MinY).ToString());
            setRange(gmin.Y, gmax.Y, gmin.X, gmax.X);

        }

        public void setRange(double MinJD, double MaxJD, double MinWD, double MaxWD)
        {
            if (parent.parent.earth.coordinateManager.isXAsLong)
            {
                center = new Point((MinJD + MaxJD) / 2, (MinWD + MaxWD) / 2);
                bounds = new Rect(MinJD, MinWD, MaxJD - MinJD, MaxWD - MinWD);
            }
            else
            {
                center = new Point((MinWD + MaxWD) / 2, (MinJD + MaxJD) / 2);
                bounds = new Rect(MinWD, MinJD, MaxWD - MinWD, MaxJD - MinJD);
            }

            if (parent.parent.earth.coordinateManager.Enable)
            {
                System.Windows.Point tp, vp;
                tp = (parent.parent.earth.coordinateManager.isXAsLong) ? new Point(MinJD, MinWD) : new Point(MinWD, MinJD);
                vp = parent.parent.earth.coordinateManager.transToInner(tp); //若激活了坐标转换，转换为内部坐标
                MinWD = vp.X; MinJD = vp.Y;
                tp = (parent.parent.earth.coordinateManager.isXAsLong) ? new Point(MaxJD, MaxWD) : new Point(MaxWD, MaxJD);
                vp = parent.parent.earth.coordinateManager.transToInner(tp); //若激活了坐标转换，转换为内部坐标
                MaxWD = vp.X; MaxJD = vp.Y;
            }

            points = new VECTOR3D[4];

            points[0] = MapHelper.JWHToPoint((float)MaxJD, (float)MaxWD, groundHeight, parent.parent.earth.earthManager.earthpara);
            points[1] = MapHelper.JWHToPoint((float)MinJD, (float)MaxWD, groundHeight, parent.parent.earth.earthManager.earthpara);
            points[2] = MapHelper.JWHToPoint((float)MaxJD, (float)MinWD, groundHeight, parent.parent.earth.earthManager.earthpara);
            points[3] = MapHelper.JWHToPoint((float)MinJD, (float)MinWD, groundHeight, parent.parent.earth.earthManager.earthpara);

            VecPoints = new List<VECTOR3D>();
            VecPoints.Add(points[0]);
            VecPoints.Add(points[1]);
            VecPoints.Add(points[2]);
            VecPoints.Add(points[3]);
        }

        internal VECTOR3D[] points;

        #endregion


        #region 呈现控制


        private Brush _brush;
        ///<summary>等值线画刷</summary>
        public Brush brush
        {
            get { return _brush; }
            set
            {
                _brush = value;

                sendD3D();
            }
        }


        ///<summary>发送新的纹理更新到d3d</summary>
        void sendD3D()
        {
            //纹理数据
            int size = 1024;
            Rectangle rec = new System.Windows.Shapes.Rectangle();
            rec.Fill = _brush;
            rec.Width = rec.Height = size;
            rec.Measure(new System.Windows.Size(size, size));
            rec.Arrange(new System.Windows.Rect(0, 0, size, size));


            RenderTargetBitmap renderTarget = new RenderTargetBitmap(size, size, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            renderTarget.Render(rec);
            renderTarget.Freeze();

            byte[] bitmaps;
            int tcount;
            IntPtr ipTexture;
            using (MemoryStream ms = new MemoryStream())
            {
                PngBitmapEncoder encode = new PngBitmapEncoder();
                encode.Frames.Add(BitmapFrame.Create(renderTarget));
                encode.Save(ms);
                bitmaps = ms.GetBuffer();

                tcount = bitmaps.Length;
                ipTexture = Marshal.AllocCoTaskMem(Marshal.SizeOf(bitmaps[0]) * tcount);  //传递纹理数据数组指针
                for (int i = 0; i < tcount; i++)
                {
                    Marshal.StructureToPtr(bitmaps[i], (IntPtr)(ipTexture.ToInt32() + i * Marshal.SizeOf(bitmaps[i])), false);
                }
            }

            D3DManager.ChangeProperty(parent.parent.earth.earthkey, (int)EModelType.等值图, (int)EPropertyType.纹理, id.GetHashCode(), 0, ipTexture, tcount, IntPtr.Zero, 0);
            Marshal.FreeCoTaskMem(ipTexture);

        }


        #endregion


        public STRUCT_Ani aniShow = new STRUCT_Ani() { aniType = EAniType.渐变, duration = 1000, doCount = 1 };


    }


    ///<summary>可视化数据对象</summary>
    public class pData : pDotObject
    {
        public pData(pLayer Parent)
            : base(Parent)
        {
            datas.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(datas_CollectionChanged);
            typeOrd = 1;
            isReceivePick = false;
        }
        ~pData()
        {
        }

        public enum EShowType { 几何体, 文字 }

        ///<summary>对应的图形对象</summary>
        public PowerBasicObject refObject;

        #region 业务数据相关


        ///<summary>数据对象集合</summary>
        public ObservableCollection<Data> datas = new ObservableCollection<Data>();

        ///<summary>处理集合更改</summary>
        void datas_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (Data item in e.NewItems)
                {
                    item.ValueChange += new EventHandler(item_ValueChange);
                }
            }
        }

        ///<summary>处理值更改</summary>
        void item_ValueChange(object sender, EventArgs e)
        {
            int earthkey = refObject == null ? parent.parent.earth.earthkey : refObject.parent.parent.earth.earthkey;
            Data zd = sender as Data;
            if (showType == EShowType.几何体)
            {
                float fv = (float)(zd.CurValue * valueScale);
                IntPtr ipPara = Marshal.AllocCoTaskMem(Marshal.SizeOf(fv));
                Marshal.StructureToPtr(fv, ipPara, false);
                D3DManager.ChangeProperty(earthkey, (int)EModelType.几何体, (int)EPropertyType.高度, datas[0].hashcode, zd.hashcode, ipPara, 1, IntPtr.Zero, 0);
                Marshal.FreeCoTaskMem(ipPara);

                updateProperty(zd.material.materialSturPara, earthkey, EModelType.几何体, EPropertyType.材质, datas[0].hashcode, zd.hashcode);

                if (isShowLabel)//标签文字
                {
                    string oldlable = dataLabel;
                    if (isAutoDataLabel)
                        dataLabel = null;
                    if (dataLabel != oldlable)
                    {
                        IntPtr ipLabel = Marshal.StringToCoTaskMemUni(dataLabel);
                        D3DManager.ChangeProperty(earthkey, (int)EModelType.文字, (int)EPropertyType.内容, datas[0].hashcode, labelhashcode, ipLabel, 0, IntPtr.Zero, 0);
                        Marshal.FreeCoTaskMem(ipLabel);
                    }
                }
            }
            else
            {
                string oldlable = dataLabel;
                if (isAutoDataLabel)
                    dataLabel = null;
                if (dataLabel != oldlable)
                {
                    IntPtr ipLabel = Marshal.StringToCoTaskMemUni(dataLabel);
                    D3DManager.ChangeProperty(earthkey, (int)EModelType.文字, (int)EPropertyType.内容, labelhashcode, labelhashcode, ipLabel, 0, IntPtr.Zero, 0);
                    Marshal.FreeCoTaskMem(ipLabel);
                }
            }

        }


        bool isAutoDataLabel = true;
        string _dataLabel = null;
        ///<summary>
        ///可以手动设置标签内容，若不设置，标签根据数据项定义自动生成。‘|’符号可换行。
        ///设置为null可恢复自动标签。
        ///若手动标签，当数据更改后需自行重新设置标签内容
        ///</summary>
        public string dataLabel
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_dataLabel))
                {
                    string s = "";
                    string sym = "";
                    for (int i = datas.Count - 1; i >= 0; i--)
                    {
                        if (datas[i].isShowLable)
                        {
                            s += sym + datas[i].label;
                            sym = "|";
                        }
                    }
                    isAutoDataLabel = true;
                    _dataLabel = s;
                }
                return _dataLabel;
            }
            set
            {
                _dataLabel = value;
                isAutoDataLabel = false;
                if (value == null)
                    isAutoDataLabel = true;
            }
        }

        private Color _dataLabelColor = Colors.Yellow;
        ///<summary>数据标签文字颜色</summary>
        public Color dataLabelColor
        {
            get { return _dataLabelColor; }
            set { _dataLabelColor = value; }
        }


        #endregion




        #region 呈现控制

        ///<summary>值转换为高度的系数，公式为：高度=值*valuescale. (几何体有效)</summary>
        public double valueScale = 1;

        ///<summary>底部宽度.(几何体有效)</summary>
        public double radScale = 0.01;

        private bool _isShowLabel = false;
        ///<summary>是否显示标签，建议只显示重要对象数据的标签，标签过多影响美观和性能.(几何体有效)</summary>
        public bool isShowLabel
        {
            get { return _isShowLabel; }
            set
            {
                _isShowLabel = value;

            }
        }


        ///<summary>数据显示形式</summary>
        public EShowType showType = EShowType.几何体;

        ///<summary>文字是否水平放置</summary>
        public bool isH = false;

        ///<summary>PData 文字X方向缩放系数，缺省1</summary>
        public float labelScaleX = 1;

        ///<summary>PData 文字Y方向缩放系数，缺省1</summary>
        public float labelScaleY = 1;



        #endregion




        #region 动画相关
        public enum EAnimationType { 缩放, 旋转 };
        ///<summary>提交开始执行一个动画</summary>
        public void AnimationBegin(EAnimationType anitype)
        {
            switch (anitype)
            {
                case EAnimationType.旋转:
                    aniRotation.isDoAni = true;
                    updateProperty(aniRotation, parent.parent.earth.earthkey, EModelType.几何体, EPropertyType.动画, hashcode, hashcode);
                    break;
                case EAnimationType.缩放:
                    aniScale.isDoAni = true;
                    updateProperty(aniScale, parent.parent.earth.earthkey, EModelType.几何体, EPropertyType.动画, hashcode, hashcode);
                    break;
            }

        }
        ///<summary>提交停止一个动画</summary>
        public void AnimationStop(EAnimationType anitype)
        {
            switch (anitype)
            {
                case EAnimationType.旋转:
                    aniRotation.isDoAni = false;
                    updateProperty(aniRotation, parent.parent.earth.earthkey, EModelType.几何体, EPropertyType.动画, hashcode, hashcode);
                    break;
                case EAnimationType.缩放:
                    aniScale.isDoAni = false;
                    updateProperty(aniScale, parent.parent.earth.earthkey, EModelType.几何体, EPropertyType.动画, hashcode, hashcode);
                    break;
            }

        }
        ///<summary>提交动画参数，可用于控制动画播放、停止和刷新动画的参数</summary>
        public void SendAniPara(STRUCT_Ani aniPara)
        {
            updateProperty(aniPara, parent.parent.earth.earthkey, EModelType.几何体, EPropertyType.动画, hashcode, hashcode);
        }

        ///<summary>控制旋转动画的参数</summary>
        public STRUCT_Ani aniRotation = new STRUCT_Ani() { aniType = EAniType.旋转, duration = 10000, doCount = 0 };

        ///<summary>控制缩放动画的参数</summary>
        public STRUCT_Ani aniScale = new STRUCT_Ani() { aniType = EAniType.缩放, duration = 1000, doCount = 1 };

        #endregion
    }



    public enum EPowerModel3DType { 无, 火电, 水电, 光伏, 风电, 变电站, 开关站, 杆塔 }
    ///<summary>3D模型</summary>
    public class pModel3D : pDotObject
    {
        public pModel3D(pLayer Parent)
            : base(Parent)
        {
            typeOrd = 1;
        }


        #region 基本数据


        private EPowerModel3DType _Model3DType;
        ///<summary>3D模型的类型</summary>
        public EPowerModel3DType Model3DType
        {
            get { return _Model3DType; }
            set { _Model3DType = value; }
        }




        #endregion

        #region 业务数据



        #endregion

        #region 呈现控制

        ///<summary>位置</summary>
        public string location { get; set; }

        public double modelScale = 1;


        #endregion


        #region XNA
        //internal override XnaEarth.BasicModel model
        //{
        //    get
        //    {
        //        if (_model == null)
        //        {
        //            if (Model3DType == EPowerModel3DType.风电)
        //                _model = new XnaEarth.Addon.ModelWind() {locationJW=location, heightScale=(float)modelScale, radScale=(float)modelScale };

        //        }

        //        return _model;
        //    }
        //}


        #endregion

    }

    ///<summary>自定义模型</summary>
    public class pCustomObject : pDotObject
    {
        /// <summary>
        /// 自定义模型对象
        /// </summary>
        /// <param name="Parent">所属的层</param>
        public pCustomObject(pLayer Parent)
            : base(Parent)
        {
            groundHeight = 0;
            isReceivePick = false;
        }



        #region 呈现控制

        ///<summary>对象在三D坐标中的顶点集</summary>
        public List<VECTOR3D> VecVertices;
        ///<summary>对象在三D坐标中的法线</summary>
        public List<VECTOR3D> VecNormals;
        ///<summary>对象的三角点序</summary>
        public List<ushort> VecIndexes;
        ///<summary>对象顶点uv坐标集</summary>
        public List<VECTOR2D> uvs;
        ///<summary>对象的纹理文件名</summary>
        public string texture;


        ///<summary>发送新的材质更新到d3d</summary>
        void sendD3D()
        {

        }

        string _symbolid;
        public string symbolid
        {
            get { return _symbolid; }
            set
            {
                _symbolid = value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (parent.parent != null)
                        updateProperty(value.GetHashCode(), parent.parent.earth.earthkey, EModelType.自定义模型, EPropertyType.内容, hashcode, hashcode);
                }
            }
        }



        private EDrawMode _drawMode = EDrawMode.纯色模式;
        ///<summary>注释</summary>
        public EDrawMode drawMode
        {
            get { return _drawMode; }
            set { _drawMode = value; updateProperty((int)value, parent.parent.earth.earthkey, EModelType.自定义模型, EPropertyType.模式, hashcode, hashcode); }
        }


        private float _scaleX = 1f;
        ///<summary>图形的尺寸系数</summary>
        public float scaleX
        {
            get { return _scaleX; }
            set { _scaleX = value; updateProperty(value, parent.parent.earth.earthkey, EModelType.自定义模型, EPropertyType.长度, hashcode, hashcode); }
        }

        private float _scaleY = 1f;
        ///<summary>图形的尺寸系数</summary>
        public float scaleY
        {
            get { return _scaleY; }
            set { _scaleY = value; updateProperty(value, parent.parent.earth.earthkey, EModelType.自定义模型, EPropertyType.宽度, hashcode, hashcode); }
        }

        private float _scaleZ = 1f;
        ///<summary>图形的尺寸系数</summary>
        public float scaleZ
        {
            get { return _scaleZ; }
            set { _scaleZ = value; updateProperty(value, parent.parent.earth.earthkey, EModelType.自定义模型, EPropertyType.高度, hashcode, hashcode); }
        }


        #endregion





    }




    #region 编辑辅助对象
    ///<summary>线段编辑辅助对象</summary>
    public class EDDot : pSymbolObject
    {
        public EDDot(pLayer layer)
            : base(layer)
        {
            //symbolid = "杆塔_小圆圈";
            color = System.Windows.Media.Colors.Magenta;
            isUseXModel = true;
            XModelKey = "球体";
            XMScaleAddition = 0.0005;
        }

        ///<summary>线段控制点索引号</summary>
        public int order { get; set; }
    }
    #endregion

}
