using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfEarthLibrary
{
    internal class Global
    {
        internal int ScreenWidth = 1280;
        internal int ScreenHeight = 1024;
        ///<summary>是否刷新地图</summary>
        internal bool isUpdate = false;
        internal int maxlayer = 0; //使用到的最大层
        internal string maxlayertileinfo;  //使用到的最大层的块信息
    }
}
