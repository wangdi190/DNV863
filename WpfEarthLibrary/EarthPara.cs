using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfEarthLibrary
{
    //public static class EarthPara
    //{
        
    //    private static string _MapUrl="http://localhost:8080/img2.aspx";
    //    public static string MapUrl
    //    {
    //        get { return _MapUrl; }
    //        set { _MapUrl= value; }
    //    }
      
    //}


    public static class Para
    {

        internal static float scalepara = 1f;// 0.001;  //需要缩小单位，以确保正常显示，估计和double最大值有关

        internal static float Radius = 6378.137f * scalepara;

        public static float LineHeight = 0.000045f * scalepara;
        public static float ArrowHeight = 0.000055f * scalepara;
        public static float AreaHeight = 0.00004f * scalepara;
        public static float SymbolHeight = 0.00005f * scalepara;
        public static float TextHeight = 0.000060f * scalepara;

        internal static bool debug = false;

        
    }
}
