using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;

namespace DNVLibrary.Interact
{
    internal enum EAdjustType {未知,新增,改造,退运}

    ///<summary>对象调整数据</summary>
    internal class ObjAdjustData
    {
        public EAdjustType adjustType { get; set; }

        public string objID { get; set; }

        public string objName { get; set; }

        ///<summary>调整类型文字</summary>
        public string typeInfo
        {
            get
            {
                switch (adjustType)
                {
                    case EAdjustType.未知:
                        return "未";
                    case EAdjustType.新增:
                        return "新";
                    case EAdjustType.改造:
                        return "改";
                    case EAdjustType.退运:
                        return "退";
                }
                return "";

            }
        }
        ///<summary>调整类型画刷</summary>
        public SolidColorBrush typeBrush
        {
            get
            {
                switch (adjustType)
                {
                    case EAdjustType.未知:
                        return Brushes.White;
                    case EAdjustType.新增:
                        return Brushes.Lime;
                    case EAdjustType.改造:
                        return Brushes.Yellow;
                    case EAdjustType.退运:
                        return Brushes.Red;
                }
                return Brushes.White;

            }
        }

        public PowerBasicObject obj;
    }
}
