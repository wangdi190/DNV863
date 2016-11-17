using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;

namespace DistNetLibrary
{
    public class DescData
    {
        public DescData()
        {
            Flags = new Dictionary<string, bool>();
        }

        internal EObjectType _objType;
        ///<summary>对象类型</summary>
        public EObjectType objType { get { return _objType; } }

        internal EObjectScope _objScope;
        ///<summary>对象范畴</summary>
        public EObjectScope objScope { get { return _objScope; } }

        internal EObjectCategory _objCategory;
        ///<summary>对象种类</summary>
        public EObjectCategory objCategory { get { return _objCategory; } }

        ///<summary>对象标志集合，可以创建对象时赋任意字串</summary>
        public Dictionary<string,bool> Flags { get; set; }

        internal bool _isFacility;
        ///<summary>是否是设施</summary>
        public bool isFacility { get { return _isFacility; } }

        internal bool _isEquipment;
        ///<summary>是否是设备</summary>
        public bool isEquipment { get { return _isEquipment; } }

        internal System.Windows.Media.Brush _icon=(System.Windows.Media.Brush)System.Windows.Application.Current.FindResource("通用对象");
        ///<summary>图标brush</summary>
        public System.Windows.Media.Brush icon { get { return _icon; } }

            

    }
}
