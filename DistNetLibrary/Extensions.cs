using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using WpfEarthLibrary;
using System.Windows.Threading;
using WpfEarthLibrary;

namespace DistNetLibrary
{
    public static class Extensions
    {

        #region ========== 扩展增加对象方法 ==========
        ///<summary>返回指定字段的双精度数据，转换错误返回double.NaN</summary>
        public static double getDouble(this DataRow dr, string fieldname)
        {
            double result;
            if (double.TryParse(dr[fieldname].ToString(), out result))
                return result;
            else
                return double.NaN;
        }

        ///<summary>返回指定字段的整型数据，转换错误返回0，适用于一般情况</summary>
        public static int getInt(this DataRow dr, string fieldname)
        {
            int result;
            if (int.TryParse(dr[fieldname].ToString(), out result))
                return result;
            else
                return 0;
        }

        ///<summary>返回指定字段的布尔数据，转换错误返回false</summary>
        public static bool getBool(this DataRow dr, string fieldname)
        {
            bool result;
            if (bool.TryParse(dr[fieldname].ToString(), out result))
                return result;
            else
                return false;
        }


        ///<summary>返回指定字段的整型数据，转换错误返回-1，适用于-1表示不确定的枚举情况</summary>
        public static int getIntN1(this DataRow dr, string fieldname)
        {
            int result;
            if (int.TryParse(dr[fieldname].ToString(), out result))
                return result;
            else
                return -1;
        }

        ///<summary>返回指定字段的日期型数据，转换错误返回1900.1.1</summary>
        public static DateTime getDatetime(this DataRow dr, string fieldname)
        {
            DateTime result;
            if (DateTime.TryParse(dr[fieldname].ToString(), out result))
                return result;
            else
                return new DateTime(1900, 1, 1);
        }

        ///<summary>返回指定字段的字符串型数据</summary>
        public static string getString(this DataRow dr, string fieldname)
        {
            return dr[fieldname].ToString();
        }


        #endregion


    }
}
