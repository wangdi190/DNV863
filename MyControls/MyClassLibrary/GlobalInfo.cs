using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClassLibrary
{
    public static class GlobalInfo
    {
        public static string FtpIP { get; set; }
        public static string FtpUser { get; set; }
        public static string FtpPassword { get; set; }
        public static string FtpEncodeString { get; set; }

        internal static Encoding FtpEncode
        {
            get
            {
                switch (FtpEncodeString.ToLower())
                {
                    case "utf8":
                        return Encoding.UTF8;
                    default:
                        return Encoding.Default;
                }

            }
        }



        public static string InfoBaseUrl { get; set; }
        public static string ResourceUrl { get; set; }

    }



}
