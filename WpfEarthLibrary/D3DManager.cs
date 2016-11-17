using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Permissions;


namespace WpfEarthLibrary
{
    public enum ETileReadMode { 内置瓦片服务, 自定义文件瓦片,自定义Web瓦片}  //google类瓦片是指我们自带的瓦片服务
    public enum EMapType { 卫星, 道路, 地形, 无 }
    public enum EGeometryType { 立方体, 柱体, 球体 };
    public enum EAniType { 无动画, 闪烁, 绘制, 擦除, 潮流动画, 渐变, 缩放, 旋转 };

    public enum EInputCoordinate { WGS84球面坐标, 平面坐标 }
    public enum ESceneMode { 地球, 局部平面 }

    ///<summary>地图模式禁止Y轴旋转确保文字向上适用于地理图；轨迹球模式可自由旋转适用于呈现模型物体；平面模式相当于可Z轴旋转地图模式</summary>
    public enum EOperateMode { 地图模式, 轨迹球模式, 自由视角, 平面模式 }

    ///<summary>对象可见性检查模式</summary>
    public enum ECheckMode {视锥体检查,经纬检查}

    internal enum EModelType { 地图, 相机, 光源, 折线, 图元, 几何体, 区域, 等值图, 文字, 潮流 ,自定义模型};
    internal enum EPropertyType { 类型, 颜色, 大小, 长度, 宽度, 高度, 偏移, 方向, 纹理, 材质, 动画, 内容, 可见性, 参数, 地址, 路径, 路径2, 进度, 位置 ,角度, 模式};

    public enum EDrawMode { 纯色模式, 纹理模式, 线框模式 };

    public enum EModifyStatus {未修改, 新增, 修改, 删除 }

    ///<summary>经纬坐标结构</summary>
    public struct GeoPoint
    {
        public GeoPoint(double latitude, double longitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
        ///<summary>经度</summary>
        public double Longitude;
        ///<summary>纬度</summary>
        public double Latitude;
    }


    ////===== 折线参数结构 =====
    [StructLayout(LayoutKind.Sequential)]
    struct STRUCT_Line
    {
        public bool isReceivePick;
        public int pickFlagId;
        public int deepOrd;
        public float thickness;
        public D3DMATERIAL9 material;
        public uint arrowColor;
        public float arrowSize;
        public bool isInverse;
        public STRUCT_Ani aniDraw;
        public STRUCT_Ani aniFlow;
        public STRUCT_Ani aniTwinkle;
        public VECTOR3D axis;
        public float angle;
        public int radCount;
    };
    ////===== 区域参数结构 =====
    [StructLayout(LayoutKind.Sequential)]
    struct STRUCT_Area
    {
        public bool isReceivePick;
        public int pickFlagId;
        public int deepOrd;
        public D3DMATERIAL9 material;
        public STRUCT_Ani aniShow;
        public VECTOR3D axis;
        public float angle;
    };
    ////===== 图元参数结构 =====
    [StructLayout(LayoutKind.Sequential)]
    struct STRUCT_Symbol
    {
        public bool isReceivePick;
        public int pickFlagId;
        public int deepOrd;
        public int rootid;
        public int parentid;
        public int texturekey;
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
        public bool isH;
        public D3DMATERIAL9 material;
        public bool isUseColor;
        public STRUCT_Ani aniTwinkle;
        public STRUCT_Ani aniShow;
        public STRUCT_Ani aniScale;
        public bool isUseXModel;
        public int XMKey;
        public float XMScaleAddition;
        public VECTOR3D axis;
        public float angle;
    };
    ////===== 自定义模型参数结构 =====
    [StructLayout(LayoutKind.Sequential)]
    struct STRUCT_Custom
    {
        public bool isReceivePick;
        public int pickFlagId;
        public int deepOrd;
        public int rootid;
        public int parentid;
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
        public D3DMATERIAL9 material;
        public VECTOR3D axis;
        public float angle;
        public int texturekey;
        public int drawMode;
    };

    ////===== 数据对象参数结构 =====
    [StructLayout(LayoutKind.Sequential)]
    struct STRUCT_PolyCol
    {
        public bool isReceivePick;
        public int pickFlagId;
        public int deepOrd;
        public int rootid;
        public int parentid;
        public D3DMATERIAL9 material;
        public float sizex;
        public float sizey;
        public float height;
        //public float startAngle;
        //public int span;
        public int geokey;
        public STRUCT_Ani aniScale;
        public STRUCT_Ani aniRotation;
        public VECTOR3D axis;
        public float angle;
    };

    ////===== 公用几何体参数结构 =====
    [StructLayout(LayoutKind.Sequential)]
    struct STRUCT_Geometry
    {
        public int geoType;
        public float pf1;
        public float pf2;
        public float pf3;
        public int pi1;
        public int pi2;
        public STRUCT_Ani aniScale;
    };
    ////===== 地图设置参数结构 =====
    [StructLayout(LayoutKind.Sequential)]
    public struct STRUCT_EarthPara
    {
        public EMapType mapType;
        public bool isShowOverlay;
        public uint background;

        public ETileReadMode tileReadMode; //瓦片读取模式，缺省自用web方式，文件读取方式
        //public int tileFileOffsetLI; //瓦片偏移-层序
        //public int tileFileOffsetXI; //瓦片偏移-X经度方向序号
        //public int tileFileOffsetYI; //瓦片偏移-Y纬度方向序号
        public bool isCover; //是否路径方式覆盖在web方式瓦片之上，用以测试调整偏移



        ///<summary>场景模式，因为精度关系，若有极小的图形对象则应使用局面平面才能正常绘制极小的图形</summary>
        public ESceneMode SceneMode;

        ///<summary>外部输入采用的坐标系</summary>
        public EInputCoordinate InputCoordinate;

        ///<summary>局部平面场景下的起始坐标</summary>
        public GeoPoint StartLocation;
        ///<summary>局部平面场景下的结束坐标</summary>
        public GeoPoint EndLocation;

        ///<summary>局部平面场景下的有效起始层数</summary>
        internal int StartLayer;
        internal int StartIdxX;
        internal int StartIdxY;
        internal int EndIdxX;
        internal int EndIdxY;
        public double UnitLatLen;
        public double UnitLongLen;
        ///<summary>对瓦片长宽比的修正系数</summary>
        public double AdjustAspect;
        ///<summary>潮流箭头间隔，缺省0.1</summary>
        public double ArrowSpan;

        ///<summary>是否启用深度测试</summary>
        public bool isDepthStencil;

        ///<summary>计算局部平面场景模式下的相关参数，设置相关参数后调用</summary>
        public void calPlaneModePara()
        {
            if (tileReadMode == ETileReadMode.内置瓦片服务 || tileReadMode== ETileReadMode.自定义Web瓦片 )
            {
                //计算最合适的对应层，再加2层作为起始层
                double minjd = StartLocation.Longitude;
                double minwd = StartLocation.Latitude;
                double maxjd = EndLocation.Longitude;
                double maxwd = EndLocation.Latitude;
                double spanjd = maxjd - minjd;
                double spanwd = maxwd - minwd;
                int ycount; double angle, tileLength, xStart, xEnd, yStart, yEnd;
                int si = 0; double cz = double.PositiveInfinity;
                for (int i = 0; i < 18; i++)
                {
                    int idxx = 0; int idxy = 0;
                    ycount = (int)Math.Pow(2, i);
                    angle = MapHelper.DegToRad(360.0 / ycount);
                    tileLength = 2.0 * Math.PI / Math.Pow(2, i); //2*pi/power(2,layer)  假定半径为1, 平面地图坐标系下该层划分块的长和宽度
                    xStart = (idxx * angle - Math.PI) / Math.PI * 180;//  指定索引号块的起始经度
                    xEnd = ((idxx + 1) * angle - Math.PI) / Math.PI * 180;//  指定索引号块的起始经度
                    yStart = (Math.Atan(Math.Exp(Math.PI - (idxy + 1) * tileLength)) * 2 - Math.PI / 2) / Math.PI * 180; //指定索引号块的结束纬度
                    yEnd = (Math.Atan(Math.Exp(Math.PI - idxy * tileLength)) * 2 - Math.PI / 2) / Math.PI * 180; //atan(exp(PI-idxy*2*pi/power(2,layer)))*2-pi/2  指定索引号块的起始纬度    
                    double tmp = Math.Abs(xEnd - xStart - spanjd) + Math.Abs(yStart - yEnd - spanwd);
                    if (tmp < cz) { cz = tmp; si = i; }
                }
                StartLayer = si + 2;

                //计算合适的瓦片序号
                ycount = (int)Math.Pow(2, StartLayer);
                angle = MapHelper.DegToRad(360.0 / ycount);
                StartIdxX = (int)((minjd / 180 * Math.PI + Math.PI) / angle);
                tileLength = 2.0 * Math.PI / Math.Pow(2, StartLayer); //2*pi/power(2,layer)  假定半径为1, 平面地图坐标系下该层划分块的长和宽度
                StartIdxY = (int)((Math.PI - Math.Log(Math.Tan((minwd / 180 * Math.PI + Math.PI / 2) / 2))) / tileLength);

                EndIdxX = (int)((maxjd / 180 * Math.PI + Math.PI) / angle);
                tileLength = 2.0 * Math.PI / Math.Pow(2, StartLayer); //2*pi/power(2,layer)  假定半径为1, 平面地图坐标系下该层划分块的长和宽度
                EndIdxY = (int)((Math.PI - Math.Log(Math.Tan((maxwd / 180 * Math.PI + Math.PI / 2) / 2))) / tileLength);
                //计算新的准确的起止经纬
                System.Windows.Rect tmprect = Helpler.GetTileJW(StartLayer, StartIdxX, StartIdxY);
                StartLocation = new GeoPoint(tmprect.Bottom, tmprect.Left);
                tmprect = Helpler.GetTileJW(StartLayer, EndIdxX, EndIdxY);
                EndLocation = new GeoPoint(tmprect.Top, tmprect.Right);

                //以12，0，赤道为基准计算单位转换坐标
                System.Windows.Rect rect12 = Helpler.GetTileJW(12, 0, (int)Math.Pow(2, 11));
                UnitLongLen = 1 / rect12.Width;
                UnitLatLen = 1 / rect12.Height * AdjustAspect;
            }
            else  //自定义瓦片
            {
                StartLayer = 0;
                StartIdxX = 0;
                StartIdxY = 0;
                EndIdxX = 1;
                EndIdxY = 1;
                UnitLatLen = 100 / (EndLocation.Latitude - StartLocation.Latitude);
                UnitLongLen = 100 / (EndLocation.Longitude - StartLocation.Longitude);
            }

        }

        ///<summary>计算获得指定层的块宽度</summary>
        internal double getTileWidth(int layer)
        {
            return Math.Pow(0.5, layer - 12); //以12层0,0为标准1宽
        }
        ///<summary>计算获得指定层和指定纬度序号的块高度</summary>
        internal double getTileWidth(int layer, int idxy)
        {
            System.Windows.Rect rect12 = Helpler.GetTileJW(12, 0, 0);
            System.Windows.Rect rect = Helpler.GetTileJW(layer, 0, idxy);
            return rect.Height / rect12.Height;

        }


        public void setBackground(System.Windows.Media.Color color)
        {
            background = Helpler.ColorToUInt(color);
        }

    };

    ////===== 公用动画参数结构 =====
    [StructLayout(LayoutKind.Sequential)]
    public struct STRUCT_Ani
    {
        public EAniType aniType;
        ///<summary>是否播放动画</summary>
        public bool isDoAni;
        ///<summary>动画周期时长，毫秒</summary>
        public int duration;
        ///<summary>循环次数，0表示无限, -1可使自动产生的动画效果无效</summary>
        public int doCount;
        ///<summary>完成后是否反转播放</summary>
        public bool isReverse;  

        //以下为运行时数据，为与d3d那边保持一致
        int doneCount; //已执行次数
        uint startTick; //动画开始计时
    };

    ////===== 相机参数结构 =====
    [StructLayout(LayoutKind.Sequential)]
    internal struct STRUCT_Camera
    {
        public VECTOR3D pos;
        public VECTOR3D lookat;
        public VECTOR3D direction;
        public VECTOR3D up;
        public float fieldofview;
        public float near;
        public float far;
    };
    ////===== 光源参数结构 =====
    [StructLayout(LayoutKind.Sequential)]
    internal struct STRUCT_Light
    {
        public bool isEnable;
        public D3DLIGHT9 light;
    }
    ////===== D3D光源结构 =====
    [StructLayout(LayoutKind.Sequential)]
    internal struct D3DLIGHT9
    {
        public int Type;            /* Type of light source */
        public D3DCOLORVALUE Diffuse;         /* Diffuse color of light */
        public D3DCOLORVALUE Specular;        /* Specular color of light */
        public D3DCOLORVALUE Ambient;         /* Ambient color of light */
        public VECTOR3D Position;         /* Position in world space */
        public VECTOR3D Direction;        /* Direction in world space */
        public float Range;            /* Cutoff range */
        public float Falloff;          /* Falloff */
        public float Attenuation0;     /* Constant attenuation */
        public float Attenuation1;     /* Linear attenuation */
        public float Attenuation2;     /* Quadratic attenuation */
        public float Theta;            /* Inner angle of spotlight cone */
        public float Phi;              /* Outer angle of spotlight cone */
    }
    ////===== D3D材质结构 =====
    [StructLayout(LayoutKind.Sequential)]
    public struct D3DMATERIAL9
    {
        public D3DCOLORVALUE Diffuse;        /* Diffuse color RGBA */
        public D3DCOLORVALUE Ambient;        /* Ambient color RGB */
        public D3DCOLORVALUE Specular;       /* Specular 'shininess' */
        public D3DCOLORVALUE Emissive;       /* Emissive color RGB */
        public float Power;          /* Sharpness if specular highlight */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D3DCOLORVALUE
    {
        public D3DCOLORVALUE(System.Windows.Media.Color color)
        {
            a = (float)color.A / 255;
            r = (float)color.R / 255;
            g = (float)color.G / 255;
            b = (float)color.B / 255;
        }

        public float r;
        public float g;
        public float b;
        public float a;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct POINT
    {
        public POINT(System.Windows.Point p)
        {
            x = (int)p.X;
            y = (int)p.Y;
        }

        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VECTOR2D
    {
        public VECTOR2D(float X, float Y)
        {
            x = X;
            y = Y;
        }
        public VECTOR2D(double X, double Y)
        {
            x = (float)X;
            y = (float)Y;
        }

        public float x;
        public float y;

        public override string ToString()
        {
            return string.Format("{0},{1}", x, y);
        }
        public static VECTOR2D Parse(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return new VECTOR2D(0, 0);
            System.Windows.Point p2 = System.Windows.Point.Parse(str);
            return new VECTOR2D(p2.X, p2.Y);
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct VECTOR3D
    {
        public VECTOR3D(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }
        public VECTOR3D(double X, double Y, double Z)
        {
            x = (float)X;
            y = (float)Y;
            z = (float)Z;
        }
        public VECTOR3D(System.Windows.Media.Media3D.Point3D p3d)
        {
            x = (float)p3d.X;
            y = (float)p3d.Y;
            z = (float)p3d.Z;
        }
        public VECTOR3D(System.Windows.Media.Media3D.Vector3D v3d)
        {
            x = (float)v3d.X;
            y = (float)v3d.Y;
            z = (float)v3d.Z;
        }


        public float x;
        public float y;
        public float z;

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", x, y, z);
        }
        public static VECTOR3D Parse(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return new VECTOR3D(0,0,1);
            System.Windows.Media.Media3D.Point3D p3 = System.Windows.Media.Media3D.Point3D.Parse(str);
            return new VECTOR3D(p3.X, p3.Y, p3.Z);
        }

        public VECTOR3D normalize()
        {
            System.Windows.Media.Media3D.Vector3D v = new System.Windows.Media.Media3D.Vector3D(x, y, z);
            v.Normalize();
            return new VECTOR3D((float)v.X,(float)v.Y,(float)v.Z);
        }
    }


    internal static class D3DManager
    {

        [DllImport("D3DEarth.dll")]
        internal static extern int GetBackBufferNoRef(int ekey, out IntPtr pSurface);

        [DllImport("D3DEarth.dll")]
        internal static extern int SetSize(int ekey, uint width, uint height);

        [DllImport("D3DEarth.dll")]
        internal static extern int SetAlpha(int ekey, bool useAlpha);

        [DllImport("D3DEarth.dll")]
        internal static extern int SetNumDesiredSamples(int ekey, uint numSamples);


        [DllImport("D3DEarth.dll")]
        internal static extern int SetAdapter(int ekey, POINT screenSpacePoint);

        [DllImport("D3DEarth.dll")]
        internal static extern int Render(int ekey);

        [DllImport("D3DEarth.dll")]
        internal static extern void Destroy(int ekey);


        [DllImport("D3DEarth.dll")]
        internal static extern int ChangeCameraPara(int ekey, IntPtr para, bool isAni, int duration);

        [DllImport("D3DEarth.dll")]
        internal static extern void ChangeLightPara(int ekey, int lightNum, IntPtr pPara);
        [DllImport("D3DEarth.dll")]
        internal static extern void ChangeAmbientLight(int ekey, uint color);

        //===== 传输瓦片数据控制
        [DllImport("D3DEarth.dll")]
        internal static extern int BeginTransfer(int ekey);

        [DllImport("D3DEarth.dll")]
        internal static extern void EndTransfer(int ekey);

        [DllImport("D3DEarth.dll")]
        internal static extern int AddMapTile(int ekey, int id, int layer, int idxx, int idxy, bool isShowTerrain, int terrainSpan, IntPtr pHigh);


        //===== 传输模型控制
        [DllImport("D3DEarth.dll")]
        internal static extern int BeginTransferModel(int ekey);

        [DllImport("D3DEarth.dll")]
        internal static extern void EndTransferModel(int ekey);


        [DllImport("D3DEarth.dll")]
        internal static extern void AddModel(int ekey, int modeltype, int id, IntPtr para, IntPtr pmesh, int mcount, IntPtr ptexture, int tcount); // modeltype 0: 折线, 1: 图元符号

        [DllImport("D3DEarth.dll")]
        internal static extern void AddCustomModel(int ekey, int id, IntPtr para, IntPtr plocation, IntPtr pvertices, IntPtr pnormal, int vcount, IntPtr pindex, int icount, IntPtr puv, int uvcount, IntPtr ptexture); 


        //添加公共材质
        [DllImport("D3DEarth.dll")]
        internal static extern void AddTexture(int ekey, int id, IntPtr data, int vectorcount);
        //添加公共材质
        [DllImport("D3DEarth.dll")]
        internal static extern void AddTextureFromFile(int ekey, int id, IntPtr file);

        //添加公共XModel
        [DllImport("D3DEarth.dll")]
        internal static extern void AddXModel(int ekey, int id, IntPtr file, IntPtr axis, float angle);

        //添加公共custom模型为XModel
        [DllImport("D3DEarth.dll")]
        internal static extern void AddCustomAsXModel(int ekey, int modelkey, IntPtr pvertices, IntPtr pnormal, int vcount, IntPtr pindex, int icount, IntPtr puv, int uvcount, IntPtr ptexture, IntPtr axis, float angle);


        //添加公共几何体
        [DllImport("D3DEarth.dll")]
        internal static extern void AddGeometry(int ekey, int geokey, IntPtr pPara);

        //更改对象属性值
        [DllImport("D3DEarth.dll")]
        internal static extern void ChangeProperty(int ekey, int modeltype, int propertytype, int id, int subid, IntPtr para, int count, IntPtr para2, int count2);

        //拾取模型
        [DllImport("D3DEarth.dll")]
        internal static extern int PickModel(int ekey, POINT screenPoint);

        //拾取模型
        [DllImport("D3DEarth.dll")]
        internal static extern int PickFlagModel(int ekey, POINT screenPoint, int flagid);

        //获取三维空间点转换到屏幕的点
        [DllImport("D3DEarth.dll")]
        internal static extern POINT TransformD3DToScreen(int ekey, VECTOR3D d3dpoint);

     
    }

    internal static class HRESULT
    {
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void Check(int hr)
        {
            Marshal.ThrowExceptionForHR(hr);
        }
    }
}
