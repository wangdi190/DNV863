using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WpfEarthLibrary
{
    /// <summary>
    /// 坐标系转换类: 用于处理自定义瓦片的情况
    /// 使用方法：
    /// 1. 使用工具，
    /// </summary>
    public class CoordinateManager
    {
        public CoordinateManager(Earth pearth)
        {
            earth = pearth;
            //scaleJD = scaleWD = 1;
            paraYMirror = 1;
        }

        Earth earth;

        ///<summary>是否启用平面坐标与经纬坐标的转换, 需提前将平面坐标转换为北京54或西安80</summary>
        public bool EnableTransformToGIS { get; set; }

        ///<summary>平面坐标四参数转换之X偏移参数</summary>
        public double paraX { get; set; }
        ///<summary>平面坐标四参数转换之Y偏移参数</summary>
        public double paraY { get; set; }
        ///<summary>平面坐标四参数转换之缩放参数</summary>
        public double paraScale { get; set; }
        ///<summary>平面坐标四参数转换之旋转参数</summary>
        public double paraRotation { get; set; }
        ///<summary>原始Y坐标是否镜像处理，镜像参数:-1, 不处理1</summary>
        public double paraYMirror { get; set; }
        ///<summary>在原始坐标中，是否x对应经度，缺省为false, 即缺省的原始经纬度坐标，x对应纬度非经度，此属性始终有效，不受Enable限制</summary>
        public bool isXAsLong { get; set; }

        ///<summary>GIS坐标转换参数，中央子午线经度。若为null，Y应为8位带带号数字，否则Y应为6位数字</summary>
        public double? gisCenter { get; set; }
        ///<summary>GIS坐标转换参数，原始坐标为三度带或六度带</summary>
        public EBandType gisBandtype { get; set; }
        ///<summary>GIS坐标转换参数，原始坐标为西安80或北京54</summary>
        public ECoordinate gisCoordinate { get; set; }


        ////=============== 下为之前处理平面坐标，以后替换删除
        ///<summary>是否启用坐标转换</summary>
        public bool Enable { get; set; }

        ///////<summary>缩放原点经度</summary>
        //public double orgJD { get; set; }
        ///////<summary>缩放原点纬度</summary>
        //public double orgWD { get; set; }

        /////<summary>经度缩放系数</summary>
        //public double scaleJD { get; set; }
        /////<summary>纬度缩放系数</summary>
        //public double scaleWD { get; set; }

        /////<summary>缩放前偏移经度</summary>
        //public double offsetJD { get; set; }
        /////<summary>缩放前偏移纬度</summary>
        //public double offsetWD { get; set; }

        //Matrix matrix ;
        //Matrix invert;
        /////<summary>设置完转换参数后调用以便生效</summary>
        //public void update()
        //{
        //    matrix = Matrix.Identity;
        //    matrix.Translate(offsetWD, offsetJD);
        //    //matrix.ScaleAt(scaleWD, scaleJD, orgWD, orgJD);
        //    matrix.ScaleAt(scaleWD, scaleJD, orgWD, orgJD);

        //    invert = matrix;
        //    invert.Invert();
        //}


        ///<summary>转换为内部坐标系, 若EnableTransformToGIS为true,转换为经纬坐标(x纬y经)</summary>
        public Point transToInner(Point outPoint)
        {
            loadPara();
            Point inp = geohelper.TransformToD(new Point(outPoint.X, paraYMirror*outPoint.Y));
            if (EnableTransformToGIS)
                inp = geohelper.Plane2Geo(inp,gisCenter, gisBandtype,gisCoordinate);

            return inp;

            //return matrix.Transform(truePoint);
        }


        ///<summary>转换为外部坐标系, x纬y经</summary>
        public Point transToOuter(Point inPoint)
        {
            loadPara();
            Point op=inPoint;
            if (EnableTransformToGIS)
                op = geohelper.geo2Plane(inPoint,gisCenter,gisBandtype,gisCoordinate);
            op = geohelper.TransformToS(op);
            op = new Point(op.X, paraYMirror * op.Y);
            return op;
            //return invert.Transform(VirtualPoint);
        }

        ///<summary>转换为外部坐标系, jwh</summary>
        public Point3D transToOuter(Point3D jwg)
        {
            Point pnt=new Point(jwg.Y,jwg.X);
            pnt=transToOuter(pnt);
            return new Point3D(pnt.X, pnt.Y, jwg.Z);
            //return new Point3D(pnt.Y, pnt.X, jwg.Z);
        }

        void loadPara()
        {
            geohelper.m_DX = paraX;
            geohelper.m_DY = paraY;
            geohelper.m_Scale = paraScale;
            geohelper.m_RotationAngle = paraRotation;
        }


        ///<summary>获取指定经纬高的点在三维场景中的坐标</summary>
        public VECTOR3D getScene3DCoordinate(double jd, double wd, double hd)
        {
            return MapHelper.JWHToPoint(jd, wd, hd, earth.earthManager.earthpara);
        }
    }
}
