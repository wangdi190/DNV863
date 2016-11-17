using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Text;
using System.Xml.Serialization;

namespace MyClassLibrary.DevShare
{
    ///<summary>通用列表呈现项数据类</summary>
    public class VListItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string note { get; set; }
        public double value { get; set; }
        public double value2 { get; set; }
        public string format { get; set; }

        public Brush icon { get; set; }
        public string strvalue { get { return string.Format(format, value, value2); } }
        public Color color { get; set; }
        public Color color2 { get; set; }
        public Color color3 { get; set; }
        public Brush brush { get; set; }
    }
}
