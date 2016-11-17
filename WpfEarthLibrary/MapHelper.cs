using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace WpfEarthLibrary
{
    public enum EMapElementType { 区域, 线路, 电厂, 变电站, 河流, 水库 };
    public enum EEffect { 无, 模糊, 阴影 }
    public enum EAttachLocation { 右上竖排, 右下竖排, 左下横排, 左上竖排, 右下横排, 正下方, 正右方 }
    public enum EAttachType { 自定义标注, 数据图表, 有效对象列表 }
    public enum ETooltipType { 无, 名称, 附加信息, 值, 组合信息 }
    public enum ECurMode { 无, 仅类别, 仅数据名, 类和数据名 }

    public enum EPushpinModel { 缺省, 圆环, 图标, 水滴 }
    public enum EDataBehaviour { 无, 大于等于0隐藏, 小于等于0隐藏, 填充色, 填充色蓝红, 文字框_自定义类型, 图表_自定义类型, 图标_点类型, 生成标注, 生成图钉, 色谱, 生成文字, 半径范围 }
    public enum EPowerModelType { 无, 风电, 火电, 水电, 光伏, 变压器, 开关站, 杆塔 }


    static class MapHelper
    {
        //缓存mesh和model
        //internal static Dictionary<string, MeshGeometry3D> meshs = new Dictionary<string, MeshGeometry3D>();
        internal static Dictionary<string, EarthData> models = new Dictionary<string, EarthData>();

        internal static EarthData curdata; //debug
        public static int gencount = 0;
        public static Random rd = new Random();

        #region 全局变量
        /////<summary>是否允许显示3D地形</summary>
        //public static bool AllowTerrain = false;

        /////<summary>表示可以支持3D地形的经续度范围</summary>
        //public static Rect TerrainRange = new Rect(95, 25, 15, 10);

        /////<summary>相机与屏幕中心点地面的夹角</summary>
        //public static double? groundAngleDegree = 90;

        /////<summary>在计算地形时有效，用以保证显示地形的可见块在同一层级</summary>
        //public static double maxTerrainLayer;

        //static EMapType _mapType = EMapType.卫星;
        //public static EMapType mapType
        //{
        //    get { return _mapType; }
        //    set
        //    {
        //        _mapType = value;
        //        switch (value)
        //        {
        //            case EMapType.卫星:
        //                tilePath = EarthPara.MapUrl + "?tilepath=\\googlemaps\\satellite\\{0}\\{1}\\{2}.jpg";
        //                break;
        //            case EMapType.地形:
        //                tilePath = EarthPara.MapUrl + "?tilepath=\\googlemaps\\terrain\\{0}\\{1}\\{2}.jpg";
        //                break;
        //            case EMapType.道路:
        //                tilePath = EarthPara.MapUrl + "?tilepath=\\googlemaps\\roadmap\\{0}\\{1}\\{2}.png";
        //                break;
        //        }
        //    }
        //}

        public static bool isShowOverlay = true;
        #endregion

        //public static string tilePath = "D:\\Map\\wx\\layer_{0}\\{1}-{2}.jpg";
        //public static string tilePath = "D:\\Map\\qn\\bingmaps\\satellite\\{0}\\{1}\\{2}.jpg";
        //public static string tilePath = EarthPara.MapUrl + "?tilepath=\\googlemaps\\satellite\\{0}\\{1}\\{2}.jpg";//
        //static string overlayPath = EarthPara.MapUrl + "?tilepath=\\googlemaps\\overlay\\{0}\\{1}\\{2}.png";//
        //public static string tilePath = "D:\\Map\\qn\\googlemaps\\merge\\{0}\\{1}\\{2}.jpg";


        ///// <summary>
        ///// 生成指定层级，指定索引号的google地图墨卡托投影mesh块
        ///// </summary>
        ///// <param name="layer">层</param>
        ///// <param name="idxY">纬度索引号，层的竖直基本面数为 2的layer次方，横向经度基本面数为2的layer次方</param>
        ///// <returns></returns>
        //public static void genGoogleMeshPlane(int layer, int idxX, int idxY, out VertexPositionNormalTexture[] verts,out int[] idxes)
        //{

        //    int Div = calDivFromLayer(layer);

        //    int ycount = (int)Math.Pow(2, layer);
        //    double angle = DegToRad(360f / ycount);
        //    double tileLength = (float)(MathHelper.TwoPi / Math.Pow(2, layer)); //2*pi/power(2,layer)  假定半径为1, 平面地图坐标系下该层划分块的长和宽度
        //    double xStart = idxX * angle - MathHelper.Pi;//  指定索引号块的起始经度
        //    double yStart = (float)(Math.Atan(Math.Exp(Math.PI - idxY * tileLength)) * 2 - Math.PI / 2); //atan(exp(PI-idxy*2*pi/power(2,layer)))*2-pi/2  指定索引号块的起始纬度
        //    double yEnd = (float)(Math.Atan(Math.Exp(Math.PI - (idxY + 1) * tileLength)) * 2 - Math.PI / 2); //指定索引号块的结束纬度
        //    double angleDiv = angle / Div;
        //    //MeshGeometry3D mesh = new MeshGeometry3D();

        //    float ys =(float)(Para.Radius * Math.Sin(yStart));
        //    float ye =(float)(Para.Radius * Math.Sin(yEnd));

        //    verts = new VertexPositionNormalTexture[(Div + 1) * (Div + 1)];

        //    int idx = 0;
        //    for (int yi = 0; yi <= Div; yi++)
        //    {
        //        double yangle = Math.Atan(Math.Exp(Math.PI - (idxY + (double)yi / Div) * tileLength)) * 2 - Math.PI / 2;
        //        double ynew = Para.Radius * Math.Sin(yangle);
        //        for (int ti = 0; ti <= Div; ti++)
        //        {
        //            double t = -angleDiv * ti - xStart + Math.PI;
        //            //mesh.Positions.Add(SphereGetPosition(t, ynew));
        //            //mesh.Normals.Add(SphereGetNormal(t, ynew));
        //            ////mesh.TextureCoordinates.Add(SphereGetTextureCoordinate(-t, y)); //注，要重写材质座标，主要是Y
        //            //mesh.TextureCoordinates.Add(SphereGetTextureCoordinate(ti, Div, ynew, ys, ye));
        //            Vector3 v3= SphereGetPosition((float)t, (float)ynew);
        //            verts[idx] = new VertexPositionNormalTexture(v3,v3 , SphereGetTextureCoordinate(ti, Div, (float)ynew, (float)ys, (float)ye));
        //            idx++;
        //        }
        //    }

        //    idxes = new int[Div * Div * 6];
        //    idx = 0;
        //    for (int yi = 0; yi < Div; yi++)
        //    {
        //        for (int ti = 0; ti < Div; ti++)
        //        {
        //            int x0 = ti;
        //            int x1 = (ti + 1);
        //            int y0 = yi * (Div + 1);
        //            int y1 = (yi + 1) * (Div + 1);

        //            idxes[idx] = x0 + y0;
        //            idxes[idx+1] = x0 + y1;
        //            idxes[idx+2] = x1 + y0;
        //            idxes[idx+3] = x1 + y0;
        //            idxes[idx+4] = x0 + y1;
        //            idxes[idx+5] = x1 + y1;
        //            idx += 6;

        //            //mesh.TriangleIndices.Add(x0 + y0);
        //            //mesh.TriangleIndices.Add(x0 + y1);
        //            //mesh.TriangleIndices.Add(x1 + y0);
        //            //mesh.TriangleIndices.Add(x1 + y0);
        //            //mesh.TriangleIndices.Add(x0 + y1);
        //            //mesh.TriangleIndices.Add(x1 + y1);
        //        }
        //    }
        //}

        /// <summary>
        /// 获取指定层级，指定索引号的瓦片mesh四角点的normal
        /// </summary>
        /// <param name="layer">层</param>
        /// <param name="idxY">纬度索引号，层的竖直基本面数为 2的layer次方，横向经度基本面数为2的layer次方</param>
        /// <returns>返回数组含四角点normal</returns>
        public static Vector3[] getMesh4Normal(int layer, int idxX, int idxY)
        {
            Vector3[] normals = new Vector3[4];

            int Div = calDivFromLayer(layer);

            int ycount = (int)Math.Pow(2, layer);
            double angle = DegToRad(360f / ycount);
            double tileLength = (float)(MathHelper.TwoPi / Math.Pow(2, layer)); //2*pi/power(2,layer)  假定半径为1, 平面地图坐标系下该层划分块的长和宽度
            double xStart = idxX * angle - MathHelper.Pi;//  指定索引号块的起始经度
            double yStart = (float)(Math.Atan(Math.Exp(Math.PI - idxY * tileLength)) * 2 - Math.PI / 2); //atan(exp(PI-idxy*2*pi/power(2,layer)))*2-pi/2  指定索引号块的起始纬度
            double yEnd = (float)(Math.Atan(Math.Exp(Math.PI - (idxY + 1) * tileLength)) * 2 - Math.PI / 2); //指定索引号块的结束纬度
            double angleDiv = angle / Div;
          
            int idx = 0;
            for (int yi = 0; yi <= Div; yi=yi+Div)
            {
                double yangle = Math.Atan(Math.Exp(Math.PI - (idxY + (double)yi / Div) * tileLength)) * 2 - Math.PI / 2;
                double ynew = Para.Radius * Math.Sin(yangle);
                for (int ti = 0; ti <= Div; ti=ti+Div)
                {
                    double t = -angleDiv * ti - xStart + Math.PI;
                    Vector3 v3 = SphereGetPosition((float)t, (float)ynew);
                    normals[idx] = v3;
                    idx++;
                }
            }

            return normals;
        }

        /// <summary>
        /// 获取指定层级，指定索引号的瓦片mesh四角点的normal
        /// </summary>
        /// <param name="layer">层</param>
        /// <param name="idxY">纬度索引号，层的竖直基本面数为 2的layer次方，横向经度基本面数为2的layer次方</param>
        /// <returns>返回数组含四角点normal</returns>
        public static Vector3[] getMeshAllNormal(int layer, int idxX, int idxY)
        {

            int Div = calDivFromLayer(layer);

            Vector3[] normals = new Vector3[(Div + 1) * (Div + 1)];


            int ycount = (int)Math.Pow(2, layer);
            double angle = DegToRad(360f / ycount);
            double tileLength = (float)(MathHelper.TwoPi / Math.Pow(2, layer)); //2*pi/power(2,layer)  假定半径为1, 平面地图坐标系下该层划分块的长和宽度
            double xStart = idxX * angle - MathHelper.Pi;//  指定索引号块的起始经度
            double yStart = (float)(Math.Atan(Math.Exp(Math.PI - idxY * tileLength)) * 2 - Math.PI / 2); //atan(exp(PI-idxy*2*pi/power(2,layer)))*2-pi/2  指定索引号块的起始纬度
            double yEnd = (float)(Math.Atan(Math.Exp(Math.PI - (idxY + 1) * tileLength)) * 2 - Math.PI / 2); //指定索引号块的结束纬度
            double angleDiv = angle / Div;

            int idx = 0;
            for (int yi = 0; yi <= Div; yi = yi + 1)
            {
                double yangle = Math.Atan(Math.Exp(Math.PI - (idxY + (double)yi / Div) * tileLength)) * 2 - Math.PI / 2;
                double ynew = Para.Radius * Math.Sin(yangle);
                for (int ti = 0; ti <= Div; ti = ti + 1)
                {
                    double t = -angleDiv * ti - xStart + Math.PI;
                    Vector3 v3 = SphereGetPosition((float)t, (float)ynew);
                    normals[idx] = v3;
                    idx++;
                }
            }

            return normals;
        }


        /// <summary>
        /// 计算分割数，层越大，表明弧面越平，所需三角面越小
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        static int calDivFromLayer(int layer) //计算分割数
        {
            int tmp = (int)(32.0 / (layer + 1));
            return tmp < 1 ? 1 : tmp;
        }

        internal static double DegToRad(double degrees)
        {
            return (degrees / 180) * MathHelper.Pi;
        }

        static Vector3 SphereGetPosition(float t, float y)
        {
            double r = Math.Sqrt(Para.Radius * Para.Radius - y * y);
            double x = r * Math.Cos(t);
            double z = r * Math.Sin(t);

            return new Vector3((float)x, (float)y, (float)z);
        }


        static Vector2 SphereGetTextureCoordinate(int ti, int div, float y, float y0, float y1)
        {
            Matrix TYtoUV = Matrix.CreateScale(1.0f / div, 1.0f / (y1 - y0), 1.0f);
            Vector3 p = new Vector3((float)ti, (float)(y - y0), 1);
            p =Vector3.Transform(p,TYtoUV);
            return new Vector2(p.X, p.Y>1?1:p.Y);
        }

        public static string getTileImageURL(int layer, int idxX, int idxY)
        {
            string s = @"http://localhost:8080/img2.aspx?tilepath=\googlemaps\satellite\{0}\{1}\{2}.jpg";
            s = string.Format(s, layer, idxX, idxY);
            return s;
        }

        ///<summary>获得tile的屏幕rect</summary>
        public static Rect GetTileRect(EarthData node, Earth earth)
        {
            Vector2[] v2s = new Vector2[node.normals.Length];
            for (int i = 0; i < node.normals.Length; i++)
            {
                v2s[i] = Helpler.GetProjectPoint2D(node.normals[i],earth.camera.view,earth.camera.projection,earth.global.ScreenWidth,earth.global.ScreenHeight);
            }

            double minx=v2s.Min(p=>p.X);double miny=v2s.Min(p=>p.Y);
            double maxx=v2s.Max(p=>p.X);double maxy=v2s.Max(p=>p.Y);

            return new Rect(minx, miny, maxx - minx, maxy - miny);

        }

        ///<summary>获得tile的屏幕2D点</summary>
        public static Vector2[] GetTileCorner(EarthData node,Earth earth)
        {
            Vector2[] v2s = new Vector2[node.normals.Length];
            for (int i = 0; i < node.normals.Length; i++)
            {
                v2s[i] = Helpler.GetProjectPoint2D(node.normals[i], earth.camera.view, earth.camera.projection,earth.global.ScreenWidth,earth.global.ScreenHeight);
            }
                      
            return v2s;

        }


        ///<summary>经纬高转换为坐标点</summary>
        public static VECTOR3D JWHToPoint(double JD, double WD, double GD, STRUCT_EarthPara earthpara)
        {
            if (earthpara.SceneMode == ESceneMode.地球)
            {
                double x, y, z, r, r2;
                r = Para.Radius + GD;
                y = r * Math.Sin(WD * Math.PI / 180);
                r2 = r * Math.Cos(WD * Math.PI / 180);
                x = r2 * Math.Cos((-JD + 180) * Math.PI / 180);
                z = r2 * Math.Sin((-JD + 180) * Math.PI / 180);
                return new VECTOR3D((float)x, (float)y, (float)z);
            }
            else
            {
                double x, y, z;
                x = (JD - earthpara.StartLocation.Longitude) * earthpara.UnitLongLen;
                y = (WD - earthpara.StartLocation.Latitude) * earthpara.UnitLatLen;
                z = GD;
                return new VECTOR3D((float)x, (float)y, (float)z);
            }
        }


        ///<summary>坐标点转换为经纬高</summary>
        public static Vector3 PointToJWH(Vector3 point)
        {
            double j, w, h, r, r2;
            r = point.Length();
            h = r - Para.Radius;
            w = Math.Asin(point.Y / r) * 180 / Math.PI;

            r2 = r * Math.Cos(w * Math.PI / 180);
            j = -Math.Acos(point.X / r2) * 180 / Math.PI + 180;
            if (point.Z < 0) j = 360.0 - j;

            return new Vector3((float)j, (float)w, (float)h);
        }



        ///<summary>得到从一点到另一点的保持up为北的变换矩阵</summary>
        public static Matrix getMatrixP2P(Vector3 pstart,Vector3 pend, STRUCT_EarthPara earthpara)
        {

            Matrix matrix = Matrix.Identity;

            if (earthpara.SceneMode == ESceneMode.地球)
            {
                double angle1 = Math.Atan(pstart.X / pstart.Z);
                double angle2 = Math.Atan(pend.X / pend.Z);
                if (angle2 != angle1)
                    matrix = Matrix.CreateRotationY((float)(angle2 - angle1));

                Vector3 pmid = Vector3.Transform(pstart, matrix);
                if (pmid != pend)
                {
                    Vector3 axis = Vector3.Cross(pmid, pend);
                    axis.Normalize();

                    System.Windows.Media.Media3D.Vector3D v1 = new System.Windows.Media.Media3D.Vector3D(pmid.X, pmid.Y, pmid.Z);
                    System.Windows.Media.Media3D.Vector3D v2 = new System.Windows.Media.Media3D.Vector3D(pend.X, pend.Y, pend.Z);
                    float angle = (float)(System.Windows.Media.Media3D.Vector3D.AngleBetween(v1, v2) / 180.0 * Math.PI);
                    matrix *= Matrix.CreateFromAxisAngle(axis, angle);
                }
            }
            else
            {
                matrix = Matrix.CreateTranslation(pend - pstart);
            }
            return matrix;
        }


        public static string TileXYToQuadKey(int levelOfDetail, int tileX, int tileY)
        {
            StringBuilder quadKey = new StringBuilder();
            for (int i = levelOfDetail; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);
                if ((tileX & mask) != 0)
                {
                    digit++;
                }
                if ((tileY & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKey.Append(digit);
            }
            return quadKey.ToString();
        }

        
    }
}
