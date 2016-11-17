using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;


namespace WpfEarthLibrary
{
    public enum EBandType { 三度带, 六度带 }
    public enum ECoordinate { WGS84, 北京54, 西安80 }

    /////<summary>经纬坐标结构</summary>
    //struct GeoPoint
    //{
    //    public GeoPoint(double latitude,double longitude)
    //    {
    //        Latitude = latitude;
    //        Longitude = longitude;
    //    }
    //    ///<summary>纬度</summary>
    //    public double Latitude;
    //    ///<summary>经度</summary>
    //    public double Longitude;
    //}

    public static class geohelper
    {
        static geohelper()
        {
            m_DX = 4118585.75733497;
            m_DY = 38942245.9657318;
            m_Scale = 1.00006825902658;
            m_RotationAngle = 1.57811204215682;

        }


        internal static Vector refpoint = new Vector(496981.265668, 299028.441571);
        internal static double scale = 1;

        ///<summary>原点平面坐标转经纬坐标，原点参考点由geohelper的refpoint指定, 此专用函数假定平面图为北京地方坐标转经纬度坐标</summary>
        internal static Point planeToGeo(string planepoint)
        {
            Point pt = Point.Parse(planepoint);

            Point tp = geohelper.TransformToD(new Point(refpoint.X + pt.X*scale , -(refpoint.Y - pt.Y*scale )));
            return geohelper.Plane2Geo(tp);
        }


        static void getpara(ECoordinate coordinate, out double a, out double b)
        {
            switch (coordinate)
            {
                case ECoordinate.WGS84:
                    a = 6378137.0;
                    b = 6356752.3142;
                    break;
                case ECoordinate.北京54:
                    a = 6378245.0;
                    b = 6356863.0187730473;
                    break;
                case ECoordinate.西安80:
                    a = 6378140.0;
                    b = 6356755.2881575287;
                    break;
                default:
                    a = 6378245.0;
                    b = 6356863.0187730473;
                    break;
            }
        }

        /// <summary>
        /// 平面坐标转经纬坐标
        /// 1.若平面坐标point的Y为6位，需指定center中央子午线经度
        /// </summary>
        /// <param name="point">平面从标</param>
        /// <param name="center">中央子午线经度，若为null，Y为8位带带号数字，否则Y应为6位数字</param>
        /// <param name="bandtype"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public static Point Plane2Geo(Point point, double? center = null, EBandType bandtype = EBandType.三度带, ECoordinate coordinate = ECoordinate.北京54)
        {
            double t_f;
            double Eta_f;
            double B_f;
            double N_f;
            double M_f;

            double B0;
            double K0, K2, K4, K6;
            double dh;
            double y = point.Y;
            double x = point.X;

            if (center == null)
            {
                dh = (int)(y / 1000000);  //带号
                y = (y % 1000000) - 500000;
            }
            else
            {
                dh = 0;
                y = y - 500000;
            }

            double a, b, e1, e2;  //椭球参数
            getpara(coordinate, out a, out b);

            e1 = Math.Sqrt(a * a - b * b) / a;
            e2 = Math.Sqrt(a * a - b * b) / b;

            double A0;
            A0 = 1.0 + 3.0 / 4 * e1 * e1 + 45.0 / 64 * Math.Pow(e1, 4) + 350.0 / 512 * Math.Pow(e1, 6) + 11025.0 / 16384 * Math.Pow(e1, 8);

            B0 = x / (a * (1 - e1 * e1) * A0);
            K0 = 1.0 / 2 * (3.0 / 4 * e1 * e1 + 45.0 / 64 * Math.Pow(e1, 4) + 350.0 / 512 * Math.Pow(e1, 6) + 11025.0 / 16384 * Math.Pow(e1, 8));
            K2 = -1.0 / 3 * (63.0 / 64 * Math.Pow(e1, 4) + 1108.0 / 512 * Math.Pow(e1, 6) + 58239.0 / 16384 * Math.Pow(e1, 8));
            K4 = 1.0 / 3 * (604.0 / 512 * Math.Pow(e1, 6) + 68484.0 / 16384 * Math.Pow(e1, 8));
            K6 = -1.0 / 3 * (26328.0 / 16384 * Math.Pow(e1, 8));

            B_f = B0 + Math.Sin(2 * B0) * (K0 + Math.Sin(B0) * Math.Sin(B0) * (K4 + K6 * Math.Sin(B0) * Math.Sin(B0)));
            t_f = Math.Tan(B_f);
            Eta_f = e2 * Math.Cos(B_f);
            N_f = a / Math.Sqrt(1 - e1 * e1 * Math.Sin(B_f) * Math.Sin(B_f));
            M_f = N_f / (1 + e2 * e2 * Math.Cos(B_f) * Math.Cos(B_f));

            double B;
            B = B_f - t_f / (2 * M_f * N_f) * y * y +
                t_f / (24 * M_f * Math.Pow(N_f, 3)) * (5 + 3 * t_f * t_f + Eta_f * Eta_f - 9 * Eta_f * Eta_f * t_f * t_f) * Math.Pow(y, 4) -
                t_f / (720 * M_f * Math.Pow(N_f, 5)) * (61 + 90 * t_f * t_f + 45 * Math.Pow(t_f, 4)) * Math.Pow(y, 6);

            double l;
            l = 1.0 / (N_f * Math.Cos(B_f)) * y -
                1.0 / (6 * Math.Pow(N_f, 3) * Math.Cos(B_f)) * (1 + 2 * t_f * t_f + Eta_f * Eta_f) * Math.Pow(y, 3) +
                1.0 / (120 * Math.Pow(N_f, 5) * Math.Cos(B_f)) * (5 + 28 * t_f * t_f + 24 * Math.Pow(t_f, 4) + 6 * Eta_f * Eta_f + 8 * Eta_f * Eta_f * t_f * t_f) * Math.Pow(y, 5);

            //将B转化为度分秒的形式
            double dDegB;
            dDegB = B * 180 / Math.PI;

            double dDegL;

            dDegL = l * 180 / Math.PI;

            if (center == null)
            {
                if (bandtype == EBandType.三度带)
                    center = dh * 3;
                else if (bandtype == EBandType.六度带)
                    center = dh * 6 - 3;
            }


            return new Point(dDegB, dDegL + (double)center);
        }

        /// <summary>
        /// 经纬转平面坐标
        /// 1.若参数center为null，则按三或六度带自动确定中央子午线经度，返回结果的Y为8位，前两位为带号
        /// 2.若参数center指定中央子午线经度，返回结果Y为6位
        /// </summary>
        /// <param name="geopoint">经纬度坐标</param>
        /// <param name="center">中央子午度经度</param>
        /// <param name="bandtype"></param>
        /// <param name="coordinate"></param>
        /// <returns>若center=null, Y为8位，前两位为带号, 否则为6位</returns>
        public static Point geo2Plane(Point geopoint, double? center = null, EBandType bandtype = EBandType.三度带, ECoordinate coordinate = ECoordinate.北京54)
        {
            double N;
            double t;
            double Eta;
            double X;
            double A0, A2, A4, A6, A8;
            double RadB;

            double Rou;
            Rou = 180 * 3600 / Math.PI;

            double longitude = geopoint.Y;
            double latitude = geopoint.X;

            double a, b, e1, e2;  //椭球参数
            getpara(coordinate, out a, out b);

            e1 = Math.Sqrt(a * a - b * b) / a;
            e2 = Math.Sqrt(a * a - b * b) / b;

            double l;
            double L0;
            int n;

            if (center != null)  //指定中央经线
            {
                L0 = (int)center;
                n = 0;
            }
            else
            {
                if (bandtype == EBandType.三度带)
                {
                    n = (int)((longitude + 1.5) / 3);
                    L0 = 3 * n;
                }
                else
                {
                    n = (int)(longitude / 6) + 1;
                    L0 = 6 * n - 3;
                }

            }

            //L0 = 111;
            double L;
            L = longitude;
            l = (L - L0) * 3600;

            RadB = (latitude) * Math.PI / 180;

            N = a / Math.Sqrt(1 - e1 * e1 * Math.Sin(RadB) * Math.Sin(RadB));
            t = Math.Tan(RadB);
            Eta = e2 * Math.Cos(RadB);

            A0 = 1.0 + 3.0 / 4 * e1 * e1 + 45.0 / 64 * Math.Pow(e1, 4) + 350.0 / 512 * Math.Pow(e1, 6) + 11025.0 / 16384 * Math.Pow(e1, 8);
            A2 = -1.0 / 2 * (3.0 / 4 * e1 * e1 + 60.0 / 64 * Math.Pow(e1, 4) + 525.0 / 512 * Math.Pow(e1, 6) + 17640.0 / 16384 * Math.Pow(e1, 8));
            A4 = 1.0 / 4 * (15.0 / 64 * Math.Pow(e1, 4) + 210.0 / 512 * Math.Pow(e1, 6) + 8820.0 / 16384 * Math.Pow(e1, 8));
            A6 = -1.0 / 6 * (35.0 / 512 * Math.Pow(e1, 6) + 2520.0 / 16384 * Math.Pow(e1, 8));
            A8 = 1.0 / 8 * (315.0 / 16384 * Math.Pow(e1, 8));
            X = a * (1 - e1 * e1) * (A0 * RadB + A2 * Math.Sin(2 * RadB) + A4 * Math.Sin(4 * RadB) + A6 * Math.Sin(6 * RadB) + A8 * Math.Sin(8 * RadB));

            //计算平面横轴
            double plane_X = X + N / (2 * Rou * Rou) * Math.Sin(RadB) * Math.Cos(RadB) * l * l +
                N / (24 * Math.Pow(Rou, 4)) * Math.Sin(RadB) * Math.Pow(Math.Cos(RadB), 3) * (5 - t * t + 9 * Eta * Eta + 4 * Math.Pow(Eta, 4)) * Math.Pow(l, 4) +
                N / (720 * Math.Pow(Rou, 6)) * Math.Sin(RadB) * Math.Pow(Math.Cos(RadB), 5) * (61 - 58 * t * t + Math.Pow(t, 4)) * Math.Pow(l, 6);

            //计算平面纵轴
            double plane_Y = N / Rou * Math.Cos(RadB) * l +
                N / (6 * Math.Pow(Rou, 3)) * Math.Pow(Math.Cos(RadB), 3) * (1 - t * t + Eta * Eta) * Math.Pow(l, 3) +
                N / (120 * Math.Pow(Rou, 5)) * Math.Pow(Math.Cos(RadB), 5) * (5 - 18 * t * t + Math.Pow(t, 4) + 14 * Eta * Eta - 58 * Eta * Eta * t * t) * Math.Pow(l, 5);

            double py;
            if (center == null)
                py = double.Parse(n.ToString() + (plane_Y + 500000).ToString());
            else
                py = plane_Y + 500000;

            return new Point(plane_X, py);
        }

        #region 坐标四参数变换



        ///<summary>计算四参数之旋转参数</summary>
        static double GetRotationPara(Point fromPoint1, Point toPoint1, Point fromPoint2, Point toPoint2)
        {
            //double a = (toPoint2.Y - toPoint1.Y) * (fromPoint2.X - fromPoint1.X) - (toPoint2.X - toPoint1.X) * (fromPoint2.Y - fromPoint1.Y);
            //double b = (toPoint2.X - toPoint1.X) * (fromPoint2.X - fromPoint1.X) + (toPoint2.Y - toPoint1.Y) * (fromPoint2.Y - fromPoint1.Y);
            //if (Math.Abs(b) > 0)
            //    return Math.Tan(a / b);
            //else
            //    return Math.Tan(0);

            return Vector.AngleBetween((fromPoint2 - fromPoint1), (toPoint2 - toPoint1)) * Math.PI / 180;
        }
        ///<summary>计算四参数之缩放参数</summary>
        static double GetScale(Point fromPoint1, Point toPoint1, Point fromPoint2, Point toPoint2, double rotation)
        {
            double a = toPoint2.X - toPoint1.X;
            double b = (fromPoint2.X - fromPoint1.X) * Math.Cos(rotation) - (fromPoint2.Y - fromPoint1.Y) * Math.Sin(rotation);
            if (Math.Abs(b) > 0)
                return a / b;
            else
                return 0;
        }
        ///<summary>计算四参数之X位移参数</summary>
        static double GetXTranslation(Point fromPoint1, Point toPoint1, double rotation, double scale)
        {
            return (toPoint1.X - scale * (fromPoint1.X * Math.Cos(rotation) - fromPoint1.Y * Math.Sin(rotation)));
        }
        ///<summary>计算四参数之Y位移参数</summary>
        static double GetYTranslation(Point fromPoint1, Point toPoint1, double rotation, double scale)
        {
            return (toPoint1.Y - scale * (fromPoint1.X * Math.Sin(rotation) + fromPoint1.Y * Math.Cos(rotation)));
        }

        public static double m_Scale, m_RotationAngle, m_DX, m_DY;

        public static void calTransformPara(Point fromPoint1, Point toPoint1, Point fromPoint2, Point toPoint2)
        {
            m_RotationAngle = GetRotationPara(fromPoint1, toPoint1, fromPoint2, toPoint2);
            m_Scale= GetScale(fromPoint1, toPoint1, fromPoint2, toPoint2, m_RotationAngle); //zhh注：考虑以后改写为五参数，分离scale为scalex和scaley，代码还没改写，暂用同一个
            m_DX = GetXTranslation(fromPoint1, toPoint1, m_RotationAngle, m_Scale);
            m_DY = GetYTranslation(fromPoint1, toPoint1, m_RotationAngle, m_Scale);
        }

        ///<summary>转换为目标坐标系</summary>
        public static Point TransformToD(Point sPoint)
        {
            //********************************************
            //说明：采用相似变换模型（四参数变换模型）
            // X= ax - by + c
            // Y= bx + ay + d
            //*********************************************

            double A = m_Scale * Math.Cos(m_RotationAngle);

            double B = m_Scale * Math.Sin(m_RotationAngle);

            Point tPoint = new Point();

            tPoint.X = A * sPoint.X - B * sPoint.Y + m_DX;
            tPoint.Y = B * sPoint.X + A * sPoint.Y + m_DY;


            //tPoint.X = A * sPoint.X + B * sPoint.Y + m_DX;
            //tPoint.Y = -B * sPoint.X + A * sPoint.Y + m_DY;


            return tPoint;

        }

        ///<summary>转换为源坐标系坐标</summary>
        public static Point TransformToS(Point dPoint)
        {

            double A = m_Scale * Math.Cos(m_RotationAngle);

            double B = m_Scale * Math.Sin(m_RotationAngle);

            Point tPoint = new Point();

            tPoint.Y = dPoint.Y / A / (1 + B * B / A / A) - B * dPoint.X / A / A / (1 + B * B / A / A) + B * m_DX / A / A / (1 + B * B / A / A) - m_DY / A / (1 + B * B / A / A);
            tPoint.X = dPoint.X / A + B * tPoint.Y / A - m_DX / A;

            //tPoint.Y = dPoint.Y / A / (1 + B * B / A / A) + B * dPoint.X / A / A / (1 + B * B / A / A) - B * m_DX / A / A / (1 + B * B / A / A) - m_DY / A / (1 + B * B / A / A);
            //tPoint.X = dPoint.X / A - B * tPoint.Y / A - m_DX / A;

            return tPoint;

        }
        #endregion
    }
}
