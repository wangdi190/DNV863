using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using System.Windows;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace WpfEarthLibrary
{
    public class Helpler
    {
        ///<summary>获取3D转2D屏幕坐标</summary>
        public static Vector2 GetProjectPoint2D(Vector3 vec, Matrix viewMatrix, Matrix projMatrix, int Width, int Height)
        {
            Matrix mat = Matrix.Identity * viewMatrix * projMatrix;

            Vector4 v4 = Vector4.Transform(vec, mat);

            return new Vector2((int)((v4.X / v4.W + 1) * (Width / 2)), (int)((1 - v4.Y / v4.W) * (Height / 2)));
        }
        ///<summary>获取2D转3D水平表面的坐标（地球模式下的地平面或平面模式下的z垂直平面）dixtanceZ为距地面距离</summary>
        public static Vector3? GetProjectPoint3D(Vector2 ptCursor, Camera camera, int Width, int Height, float distanceZ, STRUCT_EarthPara earthpara)
        {
            camera.calCameraByDirection();
            Plane plane = new Plane(new Vector3(0, 0, -1), distanceZ);  //指平面模式下的地平面
            return GetProjectPoint3D(ptCursor, camera.view, camera.projection, Width, Height, plane, earthpara);
        }
        ///<summary>
        ///获取2D转3D垂直表面的坐标（地球模式下垂直地面与纬线方向的平面(未实现) 或 平面模式下的y垂直平面）
        ///distanceY为重直平面距原点的距离
        ///</summary>
        public static Vector3? GetProjectPoint3D2(Vector2 ptCursor, Camera camera, int Width, int Height, float distanceY , STRUCT_EarthPara earthpara)
        {
            camera.calCameraByDirection();
            Plane plane = new Plane(new Vector3(0, -1, 0), distanceY);  //指平面模式下，距原点为distance的与y轴垂直的平面
            //zhh注:地球模式下暂无需求，未实现垂直面的取点
            return GetProjectPoint3D(ptCursor, camera.view, camera.projection, Width, Height, plane, earthpara);
            
        }

        public static Vector3? GetProjectPoint3D(Vector2 ptCursor, Matrix view, Matrix projection, int Width, int Height, Plane plane, STRUCT_EarthPara earthpara)
        {
            //计算世界观察矩阵的逆矩阵
            Matrix m = Matrix.Invert(view);
            //计算拾取射线的方向与原点
            Vector3 vTemp;
            vTemp.X = (((2.0f * ptCursor.X) / Width) - 1) / projection.M11;
            vTemp.Y = -(((2.0f * ptCursor.Y) / Height) - 1) / projection.M22;
            vTemp.Z = -1.0f; //zh注：右手坐标系，取-1

            Vector3 vPickRayDir, vPickRayOrig;
            vPickRayDir.X = vTemp.X * m.M11 + vTemp.Y * m.M21 + vTemp.Z * m.M31;
            vPickRayDir.Y = vTemp.X * m.M12 + vTemp.Y * m.M22 + vTemp.Z * m.M32;
            vPickRayDir.Z = vTemp.X * m.M13 + vTemp.Y * m.M23 + vTemp.Z * m.M33;

            vPickRayOrig.X = m.M41;
            vPickRayOrig.Y = m.M42;
            vPickRayOrig.Z = m.M43;


            vPickRayDir.Normalize();


            Ray ray = new Ray(vPickRayOrig, vPickRayDir);

            if (earthpara.SceneMode == ESceneMode.地球) //zhh注:地球模式下暂无需求，未实现垂直面的取点
            {
                BoundingSphere sphere = new BoundingSphere(new Vector3(0, 0, 0), Para.Radius);
                return GetIntersect(ray, sphere);
            }
            else
            {
                //Plane plane = new Plane(new Vector3(0, 0, 1), 0);

                return GetIntersect(ray, plane);
            }

        }

        ///<summary>获取屏幕点的射线</summary>
        internal static Ray GetScreenRay(Vector2 ptCursor, Matrix view, Matrix projection, int Width, int Height)
        {
            //计算世界观察矩阵的逆矩阵
            Matrix m = Matrix.Invert(view);
            //计算拾取射线的方向与原点
            Vector3 vTemp;
            vTemp.X =(((2.0f * ptCursor.X) / Width) - 1) / projection.M11;
            vTemp.Y = -(((2.0f * ptCursor.Y) / Height) - 1) / projection.M22;
            vTemp.Z = -1.0f; //zh注：右手坐标系，取-1

            Vector3 vPickRayDir, vPickRayOrig;
            vPickRayDir.X = vTemp.X * m.M11 + vTemp.Y * m.M21 + vTemp.Z * m.M31;
            vPickRayDir.Y = vTemp.X * m.M12 + vTemp.Y * m.M22 + vTemp.Z * m.M32;
            vPickRayDir.Z = vTemp.X * m.M13 + vTemp.Y * m.M23 + vTemp.Z * m.M33;

            vPickRayOrig.X = m.M41;
            vPickRayOrig.Y = m.M42;
            vPickRayOrig.Z = m.M43;


            vPickRayDir.Normalize();
            return new Ray(vPickRayOrig, vPickRayDir);
        }


        ///<summary>获取三平面交点</summary>
        public static Vector3? GetIntersect(Plane p1, Plane p2, Plane p3)
        {
            float denom=Vector3.Dot(Vector3.Cross(p1.Normal,p2.Normal),p3.Normal);
            if (denom==0)
                return null;
            return ( -(p1.D *Vector3.Cross(p2.Normal, p3.Normal)) - (p2.D * Vector3.Cross(p3.Normal, p1.Normal)) -(p3.D *Vector3.Cross(p1.Normal, p2.Normal)) )/denom;
            
        }

        ///<summary>获取射线与球面交点</summary>
        public static Vector3? GetIntersect(Ray ray, BoundingSphere sphere)
        {
            float? dis= ray.Intersects(sphere);
            if (dis != null)
            {
                Vector3 dirunit = ray.Direction;
                dirunit.Normalize();
                return ray.Position + dirunit * dis;
            }
            else
                return null;
        }
        ///<summary>获取射线与平面交点</summary>
        public static Vector3? GetIntersect(Ray ray, Plane plane)
        {
            float? dis = ray.Intersects(plane);
            if (dis != null)
            {
                Vector3 dirunit = ray.Direction;
                dirunit.Normalize();
                return ray.Position + dirunit * dis;
            }
            else
                return null;
        }


        ///<summary>获得块的经纬度范围</summary>
        public static Rect GetTileJW(int layer, int idxx, int idxy)
        {
                int ycount = (int)Math.Pow(2, layer);
                double angle = MapHelper.DegToRad(360.0 / ycount);
                double tileLength = 2.0 * Math.PI / Math.Pow(2, layer); //2*pi/power(2,layer)  假定半径为1, 平面地图坐标系下该层划分块的长和宽度
                double xStart = (idxx * angle - Math.PI) / Math.PI * 180;//  指定索引号块的起始经度
                double xEnd = ((idxx + 1) * angle - Math.PI) / Math.PI * 180;//  指定索引号块的起始经度
                double yStart = (Math.Atan(Math.Exp(Math.PI - (idxy + 1) * tileLength)) * 2 - Math.PI / 2) / Math.PI * 180; //指定索引号块的结束纬度
                double yEnd = (Math.Atan(Math.Exp(Math.PI - idxy * tileLength)) * 2 - Math.PI / 2) / Math.PI * 180; //atan(exp(PI-idxy*2*pi/power(2,layer)))*2-pi/2  指定索引号块的起始纬度
                return new Rect(xStart, yStart, xEnd - xStart, yEnd - yStart);
        }


        public static float GetAngleBetween(Vector3 v1, Vector3 v2)
        {
            return (float)Math.Acos(Vector3.Dot(v1, v2) / v1.Length() / v2.Length());
        }

        public static uint ColorToUInt(System.Windows.Media.Color color)
        {
            return (0x1000000) * (uint)color.A + 0x10000 * (uint)color.R + 0x100 * (uint)color.G + (uint)color.B;
        }


        public static string getGUID()
        {
            System.Guid guid = new Guid();
            guid = Guid.NewGuid();
            string str = guid.ToString();
            return str;
        }


        public static VECTOR3D vecWpfToD3D(System.Windows.Media.Media3D.Vector3D vec)
        {
            return new VECTOR3D(vec.X, vec.Y, vec.Z);
        }

        public static System.Windows.Media.Media3D.Vector3D vecD3DToWpf(VECTOR3D vec)
        {
            return new System.Windows.Media.Media3D.Vector3D(vec.x, vec.y, vec.z);
        }




        #region 数学方法
        /// <summary>
        /// 求指定点与指定射线方向的线段与球的交点，假定球心始终为0,0,0
        /// </summary>
        /// <param name="cp">线的点</param>
        /// <param name="dir">射线方向</param>
        /// <returns></returns>
        public static System.Windows.Media.Media3D.Point3D? getCrossBall(System.Windows.Media.Media3D.Point3D cp, System.Windows.Media.Media3D.Vector3D dir, double rad)
        {
            dir.Normalize();
            //B=2(xd(x0-xc)+yd(y0-yc)+zd(z0-zc))
            //C=(x0-xc)^2+(y0-yc)^2+(z0-zc)^2-sr^2
            //B=2*(xd*x0+yd*y0+zd*z0)
            //C=x0^2+y0^2+z0^2-sr^2
            double B = 2 * (dir.X * cp.X + dir.Y * cp.Y + dir.Z * cp.Z);
            double C = cp.X * cp.X + cp.Y * cp.Y + cp.Z * cp.Z - rad * rad;
            double root = B * B - 4 * C;
            if (root > 0)
            {
                double t0 = (-B - Math.Pow(root, 0.5)) / 2;
                double t1 = (-B + Math.Pow(root, 0.5)) / 2;
                double t;
                if (t0 > 0 && t1 > 0)
                    t = Math.Min(t0, t1);
                else if (t0 > 0 || t1 > 0)
                    t = Math.Max(t0, t1);
                else
                    return null; //射线方向反向

                return new System.Windows.Media.Media3D.Point3D(cp.X + t * dir.X, cp.Y + t * dir.Y, cp.Z + t * dir.Z);
            }
            return null;
        }

        ///<summary>经纬高转换为坐标点</summary>
        public static System.Windows.Media.Media3D.Point3D JWHToPoint(System.Windows.Media.Media3D.Point3D jwh)
        {
            double x, y, z, r, r2;
            r = Para.Radius + jwh.Z;
            y = r * Math.Sin(jwh.Y * Math.PI / 180);
            r2 = r * Math.Cos(jwh.Y * Math.PI / 180);
            x = r2 * Math.Cos((-jwh.X + 180) * Math.PI / 180);
            z = r2 * Math.Sin((-jwh.X + 180) * Math.PI / 180);
            return new System.Windows.Media.Media3D.Point3D(x, y, z);
        }
        ///<summary>经纬高转换为坐标点</summary>
        public static Vector3 JWHToPoint(double j, double w, double h, STRUCT_EarthPara earthpara)
        {
            if (earthpara.SceneMode == ESceneMode.地球)
            {
                double x, y, z, r, r2;
                r = Para.Radius + h;
                y = r * Math.Sin(w * Math.PI / 180);
                r2 = r * Math.Cos(w * Math.PI / 180);
                x = r2 * Math.Cos((-j + 180) * Math.PI / 180);
                z = r2 * Math.Sin((-j + 180) * Math.PI / 180);
                return new Vector3((float)x, (float)y, (float)z);
            }
            else 
            {
                double x, y, z;
                x = (j - earthpara.StartLocation.Longitude) * earthpara.UnitLongLen;
                y=(w-earthpara.StartLocation.Latitude)*earthpara.UnitLatLen;
                z = h;
                return new Vector3((float)x, (float)y, (float)z);
            }
        }


        ///<summary>坐标点转换为经纬高</summary>
        public static System.Windows.Media.Media3D.Point3D PointToJWH(System.Windows.Media.Media3D.Point3D point, STRUCT_EarthPara earthpara)
        {
            if (earthpara.SceneMode == ESceneMode.地球)
            {
                double j, w, h, r, r2;
                r = ((System.Windows.Media.Media3D.Vector3D)point).Length;
                h = r - Para.Radius;
                w = Math.Asin(point.Y / r) * 180 / Math.PI;

                r2 = r * Math.Cos(w * Math.PI / 180);
                j = -Math.Acos(point.X / r2) * 180 / Math.PI + 180;
                if (point.Z < 0) j = 360.0 - j;

                return new System.Windows.Media.Media3D.Point3D(j, w, h);
            }
            else
            {
                double j, w,h;
                j = point.X / earthpara.UnitLongLen + earthpara.StartLocation.Longitude;
                w = point.Y / earthpara.UnitLatLen + earthpara.StartLocation.Latitude;
                h = point.Z;
                return new System.Windows.Media.Media3D.Point3D(j, w, h);

            }
        }


        #endregion


        internal static D3DCOLORVALUE getD3DColor(System.Windows.Media.Color color)
        {
            D3DCOLORVALUE dc = new D3DCOLORVALUE();
            dc.a = (float)color.A / 255;
            dc.r = (float)color.R / 255;
            dc.g = (float)color.G / 255;
            dc.b = (float)color.B / 255;
            return dc;
        }

    }




  
}
