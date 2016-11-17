using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Text;

namespace MyClassLibrary.Share3D
{
    #region 三维空间中扩展的基本对象定义
    /// <summary>
    /// 点法结构，可用于描述平面和线
    /// </summary>
    public struct DotNormal
    {
        public DotNormal(Point3D pPoint,Vector3D pNormal)
        {
            point = pPoint; normal = pNormal; normal.Normalize();
        }
        public Point3D point;
        public Vector3D normal;

        public void setNormalAndPoint(Vector3D pNormal, Point3D pPoint)
        {
            point = pPoint; normal = pNormal; normal.Normalize();
        }
    }
 
    #endregion

    /// <summary>
    /// 3D数学运算helper
    /// </summary>
    public static class Math3DHelper
    {
        /// <summary>
        /// 求面与线的交点
        /// </summary>
        /// <param name="flat">平面</param>
        /// <param name="line">线</param>
        /// <returns>返回相交点，若为null表示平行，或线在面上</returns>
        public static Nullable<Point3D> getFlatXLine(DotNormal flat,DotNormal line)
        {
            Nullable<Point3D> result = null;
            /*
                设    Pt = P1 + t(P2 ‐P1)；现在求取参数t的大小；
                因为Pt也是平面上的顶点，则矢量Pt Pon与法向量的点乘为0，可列方程：
                             (Pt ‐Pon) • N = 0；
                             (P1 ‐Pon)• N + t(P2 ‐P1)• N = 0；
                             则 t = ‐(P1 ‐Pon)• N /(P2 ‐P1)• N；
            */
            double t;
            line.normal.Normalize();
            Point3D p2 = line.point;
            Point3D p1 = p2 + line.normal;
            if (Vector3D.DotProduct((p1 - flat.point), flat.normal) != 0 && Vector3D.DotProduct((p2 - p1), flat.normal) != 0) //不平行，不在面上
            {
                t = -Vector3D.DotProduct((p1 - flat.point), flat.normal) / Vector3D.DotProduct((p2 - p1), flat.normal);
                result = p1 + t * (p2 - p1);
            }
            return result;
        }

        /// <summary>
        /// 求面面相交的线
        /// </summary>
        /// <param name="flat1">平面一</param>
        /// <param name="flat2">平面二</param>
        /// <returns>返回相交线，若为null表示平行或重合</returns>
        public static DotNormal? getFlatXFlat(DotNormal flat1, DotNormal flat2)
        {
            Vector3D dir = Vector3D.CrossProduct(flat1.normal, flat2.normal);
            if (dir == new Vector3D(0, 0, 0)) return null;
            //通过求两个平面中某个面A法向量和步骤一求得的方向向量的叉积，可以求得该直线在这个平面A内的法线向量L；
            //然后，求以平面原点为起点、以法线向量L为方向的射线与另一个平面B的交点，此交点就是直线上的一个点。
            Vector3D vinflat = Vector3D.CrossProduct(dir, flat1.normal);
            Point3D? px = getFlatXLine(flat2, new DotNormal(flat1.point, vinflat));
            if (px == null)
                return null;

            return new DotNormal((Point3D)px, dir);
        }

        /// <summary>
        /// 计算三面相交点
        /// </summary>
        /// <param name="flat1"></param>
        /// <param name="flat2"></param>
        /// <param name="flat3"></param>
        /// <returns></returns>
        public static Point3D? get3FlatCrossPoint(DotNormal flat1, DotNormal flat2, DotNormal flat3)
        {
            DotNormal? cline = getFlatXFlat(flat1, flat2);
            if (cline == null) return null;

            return getFlatXLine(flat3, (DotNormal)cline);
        }
    }
}
