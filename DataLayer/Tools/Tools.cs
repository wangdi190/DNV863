using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataLayer.Tools
{
    ///<summary>更好的办法是在类库中加extensions扩展类，对类加扩展方法</summary>
    public static class Tools
    {
        ///<summary>返回双精度数据，转换错误返回double.NaN</summary>
        public static double getDouble(DataRow dr, string fieldname)
        {
            double result;
            if (double.TryParse(dr[fieldname].ToString(), out result))
                return result;
            else
                return double.NaN;
        }

        ///<summary>返回整型数据，转换错误返回0，适用于一般情况</summary>
        public static int getInt(DataRow dr, string fieldname)
        {
            int result;
            if (int.TryParse(dr[fieldname].ToString(), out result))
                return result;
            else
                return 0;
        }


        ///<summary>返回整型数据，转换错误返回-1，适用于-1表示不确定的枚举情况</summary>
        public static int getIntN1(DataRow dr, string fieldname)
        {
            int result;
            if (int.TryParse(dr[fieldname].ToString(), out result))
                return result;
            else
                return -1;
        }

        ///<summary>返回日期型数据，转换错误返回1900.1.1</summary>
        public static DateTime getDatetime(DataRow dr, string fieldname)
        {
            DateTime result;
            if (DateTime.TryParse(dr[fieldname].ToString(), out result))
                return result;
            else
                return new DateTime(1900,1,1);
        }
    }
}
