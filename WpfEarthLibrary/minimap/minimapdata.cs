using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;

namespace WpfEarthLibrary.minimap
{
    [Serializable]
    public class minimapdata
    {
        //保存地图的坐标信息
        public GeoPoint lefttop { get; set; }
        public GeoPoint leftbottom { get; set; }
        public GeoPoint righttop { get; set; }
        public GeoPoint rightbottom { get; set; }
    

        //运行时参数
        public double offsetX { get; set; }
        public double offsetY { get; set; }
        public double sclae { get; set; }
        public bool isshow { get; set; }



        public void save()
        {
            MyClassLibrary.XmlHelper.saveToXml(".\\xml\\minimap.xml", this);
        }

    }

}
