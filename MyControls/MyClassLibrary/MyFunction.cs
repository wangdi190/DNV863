using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.CodeDom.Compiler;
using System.Reflection;


namespace MyClassLibrary
{
    public static class MyFunction
    {
        /// <summary>
        /// 获取指定日期所在月的天数
        /// </summary>
        /// <param name="d">指定的日期</param>
        /// <returns></returns>
        public static int getMonthDays(DateTime d)
        {
            DateTime d1, d2;
            d1 = new DateTime(d.Year, d.Month, 1);
            d2 = d1.AddMonths(1);
            return (d2 - d1).Days;
        }
        /// <summary>
        /// 获取指定日期所在年的天数
        /// </summary>
        /// <param name="d">指定的日期</param>
        /// <returns></returns>
        public static int getYearDays(DateTime d)
        {
            DateTime d1, d2;
            d1 = new DateTime(d.Year, 1, 1);
            d2 = d1.AddYears(1);
            return (d2 - d1).Days;
        }/// <summary>
        /// 获取指定日0-24点，与一个时间范围交集的时间
        /// </summary>
        /// <param name="dstart">时间范围开始时点</param>
        /// <param name="dend">时间范围结束时点</param>
        /// <param name="theday">指定日</param>
        /// <returns>返回重合的时间，以天数表示</returns>
        public static double getCrossTime(DateTime dstart, DateTime dend, DateTime theday)
        {
            DateTime db = new DateTime(theday.Year, theday.Month, theday.Day, 0, 0, 0, 0);
            DateTime de = db.AddDays(1);

            DateTime d1 = db > dstart ? db : dstart;
            DateTime d2 = de < dend ? de : dend;
            if (d2 < d1)
                return 0;
            else
                return (d2 - d1).TotalDays;
        }

        /// <summary>
        /// 获取两个日期之间包含的月数
        /// </summary>
        /// <param name="d1">起始日期</param>
        /// <param name="d2">结束日期</param>
        /// <returns>月数</returns>
        public static int GetMonthsBetweenDate(DateTime d1, DateTime d2)
        {
            return (d2.Year - d1.Year) * 12 + (d2.Month - d1.Month) + 1;
        }

        public static bool isSameYear(DateTime d1, DateTime d2)
        {
            return d1.Year == d2.Year;
        }
        public static bool isSameMonth(DateTime d1, DateTime d2)
        {
            return d1.Year == d2.Year && d1.Month == d2.Month;
        }
        public static bool isSameDay(DateTime d1, DateTime d2)
        {
            return d1.ToShortDateString() == d2.ToShortDateString();
        }
        /// <summary>
        /// 判断句子中是否含有中文
        /// </summary>
        /// <param >字符串</param>
        public static bool WordsIScn(string words)
        {
            string TmmP;
            for (int i = 0; i < words.Length; i++)
            {
                TmmP = words.Substring(i, 1);
                byte[] sarr = System.Text.Encoding.GetEncoding("gb2312").GetBytes(TmmP);
                if (sarr.Length == 2)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 返回一个字串包含另一字串的次数
        /// </summary>
        /// <param name="s">主字串</param>
        /// <param name="sub">被包含的字串</param>
        /// <returns></returns>
        public static int StringContainsSubstringNums(string s, string sub)
        {
            int n = 0;
            if (sub.Length > 0)
            {
                string newstring = s.Replace(sub, "");
                n = (s.Length - newstring.Length) / sub.Length;
            }
            return n;
        }


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


        public static double simMonthData(int m)
        {
            //y = 2.65E-06x6 + 9E-06x5 - 0.0017x4 + 0.018x3 - 0.0409x2 - 0.0075x + 0.4786 
            return 2.65E-06 * Math.Pow(m, 6) + 9E-06 * Math.Pow(m, 5) - 0.0017 * Math.Pow(m, 4) + 0.018 * Math.Pow(m, 3) - 0.0409 * Math.Pow(m, 2) - 0.0075 * m + 0.4786;
        }

        public static double simMonthData(int month,int day)
        {
            //y = 2.65E-06x6 + 9E-06x5 - 0.0017x4 + 0.018x3 - 0.0409x2 - 0.0075x + 0.4786 
            double m = month + day / 30.5;
            return 2.65E-06 * Math.Pow(m, 6) + 9E-06 * Math.Pow(m, 5) - 0.0017 * Math.Pow(m, 4) + 0.018 * Math.Pow(m, 3) - 0.0409 * Math.Pow(m, 2) - 0.0075 * m + 0.4786;
        }

        public static double simHourData(int h)
        {
            //y = -4E-07x6 + 2E-05x5 - 0.0002x4 - 0.0026x3 + 0.0456x2 - 0.1582x + 0.5721
            //return -4E-07 * Math.Pow(h, 6) + 2E-05 * Math.Pow(h, 5) - 0.0002 * Math.Pow(h, 4) - 0.0026 * Math.Pow(h, 3) + 0.0456 * Math.Pow(h, 2) - 0.1582 * h + 0.5721;
            double[] hv = new double[24];
            hv[0] = 0.55;
            hv[1] = 0.5;
            hv[2] = 0.45;
            hv[3] = 0.4;
            hv[4] = 0.38;
            hv[5] = 0.4;
            hv[6] = 0.55;
            hv[7] = 0.7;
            hv[8] = 0.8;
            hv[9] = 0.85;
            hv[10] = 0.85;
            hv[11] = 0.78;
            hv[12] = 0.7;
            hv[13] = 0.68;
            hv[14] = 0.74;
            hv[15] = 0.75;
            hv[16] = 0.74;
            hv[17] = 0.8;
            hv[18] = 0.88;
            hv[19] = 0.96;
            hv[20] = 1;
            hv[21] = 0.92;
            hv[22] = 0.8;
            hv[23] = 0.65;
            return hv[h];
        }
    }

    #region 动态求值
    /// <summary>
    /// 动态求值
    /// </summary>
    public class Evaluator
    {
        /// <summary>
        /// 计算结果,如果表达式出错则抛出异常
        /// </summary>
        /// <param name="statement">表达式,如"1+2+3+4"</param>
        /// <returns>结果</returns>
        public static object Eval(string statement)
        {
            return _evaluatorType.InvokeMember(
                        "Eval",
                        BindingFlags.InvokeMethod,
                        null,
                        _evaluator,
                        new object[] { statement }
                     );
        }
        public static double EvalToDouble(string statement)
        {
            string result = (string)_evaluatorType.InvokeMember(
                        "Eval",
                        BindingFlags.InvokeMethod,
                        null,
                        _evaluator,
                        new object[] { statement }
                     );
            return double.Parse(result);

        }
        /// <summary>
        /// 
        /// </summary>
        static Evaluator()
        {
            //构造JScript的编译驱动代码
            CodeDomProvider provider = CodeDomProvider.CreateProvider("JScript");

            CompilerParameters parameters;
            parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;

            CompilerResults results;
            results = provider.CompileAssemblyFromSource(parameters, _jscriptSource);

            Assembly assembly = results.CompiledAssembly;
            _evaluatorType = assembly.GetType("Evaluator");

            _evaluator = Activator.CreateInstance(_evaluatorType);
        }


        private static object _evaluator = null;
        private static Type _evaluatorType = null;
        /// <summary>
        /// JScript代码
        /// </summary>
        private static readonly string _jscriptSource =

           @"class Evaluator
               {
                   public function Eval(expr : String) : String 
                   { 
                      return eval(expr); 
                   }
               }";
    }
    #endregion




}
