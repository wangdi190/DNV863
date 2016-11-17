using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;

namespace MyClassLibrary.Share2D
{
    public static class MyGeometryHelper
    {
        #region bezier曲线计算
        //  计算bezier曲线的两个控制点
        public static void GetCurveControlPoints(Point[] knots, out Point[] firstControlPoints, out Point[] secondControlPoints)
        {

            if (knots == null)
                throw new ArgumentNullException("knots");
            int n = knots.Length - 1;
            if (n < 1)
                throw new ArgumentException("At least two knot points required", "knots");
            if (n == 1)
            { // Special case: Bezier curve should be a straight line.
                firstControlPoints = new Point[1];
                // 3P1 = 2P0 + P3
                firstControlPoints[0].X = (2 * knots[0].X + knots[1].X) / 3;
                firstControlPoints[0].Y = (2 * knots[0].Y + knots[1].Y) / 3;

                secondControlPoints = new Point[1];
                // P2 = 2P1 – P0
                secondControlPoints[0].X = 2 * firstControlPoints[0].X - knots[0].X;
                secondControlPoints[0].Y = 2 * firstControlPoints[0].Y - knots[0].Y;
                return;
            }

            // Calculate first Bezier control points
            // Right hand side vector
            double[] rhs = new double[n];

            // Set right hand side X values
            for (int i = 1; i < n - 1; ++i)
                rhs[i] = 4 * knots[i].X + 2 * knots[i + 1].X;
            rhs[0] = knots[0].X + 2 * knots[1].X;
            rhs[n - 1] = (8 * knots[n - 1].X + knots[n].X) / 2.0;
            // Get first control points X-values
            double[] x = GetFirstControlPoints(rhs);

            // Set right hand side Y values
            for (int i = 1; i < n - 1; ++i)
                rhs[i] = 4 * knots[i].Y + 2 * knots[i + 1].Y;
            rhs[0] = knots[0].Y + 2 * knots[1].Y;
            rhs[n - 1] = (8 * knots[n - 1].Y + knots[n].Y) / 2.0;
            // Get first control points Y-values
            double[] y = GetFirstControlPoints(rhs);

            // Fill output arrays.
            firstControlPoints = new Point[n];
            secondControlPoints = new Point[n];
            for (int i = 0; i < n; ++i)
            {
                // First control point
                firstControlPoints[i] = new Point(x[i], y[i]);
                // Second control point
                if (i < n - 1)
                    secondControlPoints[i] = new Point(2 * knots[i + 1].X - x[i + 1], 2 * knots[i + 1].Y - y[i + 1]);
                else
                    secondControlPoints[i] = new Point((knots[n].X + x[n - 1]) / 2, (knots[n].Y + y[n - 1]) / 2);
            }
        }

        /// <summary>
        /// Solves a tridiagonal system for one of coordinates (x or y) of first Bezier control points.
        /// </summary>
        /// <param name="rhs">Right hand side vector.</param>
        /// <returns>Solution vector.</returns>
        private static double[] GetFirstControlPoints(double[] rhs)
        {
            int n = rhs.Length;
            double[] x = new double[n]; // Solution vector.
            double[] tmp = new double[n]; // Temp workspace.

            double b = 2.0;
            x[0] = rhs[0] / b;
            for (int i = 1; i < n; i++) // Decomposition and forward substitution.
            {
                tmp[i] = 1 / b;
                b = (i < n - 1 ? 4.0 : 3.5) - tmp[i];
                x[i] = (rhs[i] - x[i - 1]) / b;
            }
            for (int i = 1; i < n; i++)
                x[n - i - 1] -= tmp[n - i] * x[n - i]; // Backsubstitution.

            return x;
        }

        // 根据cp 0-4, 计算插值点
        public static Point PointOnBezier(Point[] cp, double t)
        {
            double ax, bx, cx;
            double ay, by, cy;
            double tSquared, tCubed;
            Point result = new Point();

            /*計算多項式係數*/

            cx = 3.0 * (cp[1].X - cp[0].X);
            bx = 3.0 * (cp[2].X - cp[1].X) - cx;
            ax = cp[3].X - cp[0].X - cx - bx;

            cy = 3.0 * (cp[1].Y - cp[0].Y);
            by = 3.0 * (cp[2].Y - cp[1].Y) - cy;
            ay = cp[3].Y - cp[0].Y - cy - by;

            /*計算位於參數值t的曲線點*/

            tSquared = t * t;
            tCubed = tSquared * t;

            result.X = (ax * tCubed) + (bx * tSquared) + (cx * t) + cp[0].X;
            result.Y = (ay * tCubed) + (by * tSquared) + (cy * t) + cp[0].Y;

            return result;
        }

        #endregion bezier曲线计算


        #region 中心点计算
        ///<summary>获得多边形中心点</summary>
        public static Point getCenterFromPolygon(PointCollection edgePoints)
        {
                   double area = 0;
                for (int i = 0; i < edgePoints.Count - 1; i++)
                {
                    area += -edgePoints[i].X * edgePoints[i + 1].Y + edgePoints[i + 1].X * edgePoints[i].Y;
                }
                area = area / 2;

                double x = 0, y = 0;
                for (int i = 0; i < edgePoints.Count - 1; i++)
                {
                    x += (edgePoints[i].X + edgePoints[i + 1].X) * (-edgePoints[i].X * edgePoints[i + 1].Y + edgePoints[i + 1].X * edgePoints[i].Y);
                    y += (edgePoints[i].Y + edgePoints[i + 1].Y) * (-edgePoints[i].X * edgePoints[i + 1].Y + edgePoints[i + 1].X * edgePoints[i].Y);
                }
                return new Point(x / 6 / area, y / 6 /area);
        }

        ///<summary>获得线段中心点</summary>
        public static Point getCenterFromPolyLine(PointCollection edgePoints)
        {
            

            double length = 0;
            for (int i = 0; i < edgePoints.Count - 1; i++)
            {
                length += (edgePoints[i+1]-edgePoints[i]).Length;
            }
            

            double len=0;
            for (int i = 0; i < edgePoints.Count - 1; i++)
            {
                if (len < length / 2 && len + (edgePoints[i + 1] - edgePoints[i]).Length > length / 2)
                {
                    double divlen = length / 2 - len;
                    Vector vec = (edgePoints[i + 1] - edgePoints[i]);
                    vec.Normalize();
                    return edgePoints[i] + vec * divlen;
                }
                else
                {
                    len += (edgePoints[i + 1] - edgePoints[i]).Length;
                }
            }
            return new Point();
        }
        #endregion
    }
}
