using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using MyClassLibrary;
using System.Xml.Serialization;

namespace MyBaseControls.ConfigTool
{

    [Serializable]
    public class ConfigData
    {
        public enum ESort { 对象名, 属性名, 状态, 标志 };
        public enum EValueType { 字符串, 颜色, 整型, 数字, 布尔 }

        public ConfigData()
        {
            items = new SerializableDictionary<string, Item>();
            sort1 = ESort.对象名;
            sort2 = ESort.属性名;
            sort3 = ESort.状态;
            sort4 = ESort.标志;
        }

        public SerializableDictionary<string, Item> items { get; set; }

        public ESort sort1 { get; set; }
        public ESort sort2 { get; set; }
        public ESort sort3 { get; set; }
        public ESort sort4 { get; set; }


        #region 树相关
        [NonSerialized]
        internal Item root = new Item("", "");

        ///<summary>构建树，</summary>
        internal void buildTree(bool isDeveloperEdit)
        {
            //清除内部增加，且无值的节点
            List<string> tmp = items.Values.Where(p => p.isInAddTree && string.IsNullOrWhiteSpace(p.value)).Select(p => p.key).ToList();
            foreach (string key in tmp)
            {
                items.Remove(key);
            }
            //重置状态
            foreach (var item in items.Values)
            {
                item.subitems.Clear();//清空子节点
                item.id = buildID(item); //重建ID
            }
            //重构树
            if (root == null)
                root = new Item("", "", "", "");
            else
                root.subitems.Clear();

            List<string> keys = items.Select(p => p.Key).ToList();
            foreach (var key in keys)
            {
                if (isDeveloperEdit || items[key].isUserEditable)  //开发者编辑或用户可编辑
                    items[key].addInParent(this);
            }

        }



        #endregion

        internal string buildID(Item item)
        {
            string s = "";
            ESort[] sorts = new ESort[] { sort1, sort2, sort3, sort4 };
            for (int i = 0; i < sorts.Count(); i++)
            {
                switch (sorts[i])
                {
                    case ESort.对象名:
                        if (!string.IsNullOrWhiteSpace(item.obj)) s += (i == 0 ? "" : ".") + item.obj;
                        break;
                    case ESort.属性名:
                        if (!string.IsNullOrWhiteSpace(item.property)) s += (i == 0 ? "" : ".") + item.property;
                        break;
                    case ESort.状态:
                        if (!string.IsNullOrWhiteSpace(item.status)) s += (i == 0 ? "" : ".") + item.status;
                        break;
                    case ESort.标志:
                        if (!string.IsNullOrWhiteSpace(item.flag)) s += (i == 0 ? "" : ".") + item.flag;
                        break;
                }
            }
            return s;
        }

    }


    public class Item : MyClassLibrary.MVVM.NotificationObject
    {
        public Item()
        {
            subitems = new ObservableCollection<Item>();
        }
        public Item(string Obj, string Property, string Status = "", string Flag = "")
        {
            subitems = new ObservableCollection<Item>();
            obj = Obj; property = Property; status = Status; flag = Flag;
        }

        ///<summary>必填项，要设置的对象</summary>
        public string obj { get; set; }
        ///<summary>必填项，要设置的属性</summary>
        public string property { get; set; }
        ///<summary>选填项，状态，可以此区分同一对象在不同状态下的不同外观</summary>
        public string status { get; set; }
        ///<summary>选填项，标志，可以此区分同一对象在不同场景或应用下的不同外观，如不同的地图背景下</summary>
        public string flag { get; set; }
        ///<summary>必填项，值</summary>
        public string value { get; set; }
        ///<summary>选填项，说明</summary>
        public string note { get; set; }
        ///<summary>值类型，缺省字符串型</summary>
        public ConfigData.EValueType valueType { get; set; }
        ///<summary>用户是否可编辑，缺省否</summary>
        public bool isUserEditable { get; set; }

        ///<summary>根据构成键的四个字段组合的键值</summary>
        public string key { get { return Item.buildKey(obj, property, status, flag); } }

        ///<summary>是否是内部添加的节点</summary>
        public bool isInAddTree { get; set; }
        ///<summary>静态函数，根据给定参数构建Key</summary>
        public static string buildKey(string sobj, string sproperty, string sstatus, string sflag)
        {
            string s = "";
            if (!string.IsNullOrWhiteSpace(sobj)) s += sobj;
            if (!string.IsNullOrWhiteSpace(sproperty)) s += (string.IsNullOrWhiteSpace(s) ? "" : ".") + sproperty;
            if (!string.IsNullOrWhiteSpace(sstatus)) s += (string.IsNullOrWhiteSpace(s) ? "" : ".") + sstatus;
            if (!string.IsNullOrWhiteSpace(sflag)) s += (string.IsNullOrWhiteSpace(s) ? "" : ".") + sflag;
            return s;
        }

        ///<summary>根据树构成方式不同而不同</summary>
        public string id { get; set; }
        ///<summary>根据树构成方式不同而不同</summary>
        public string pid { get { return id.Contains('.') ? id.Substring(0, id.LastIndexOf('.')) : ""; } }
        ///<summary>根据树构成方式不同而不同</summary>
        public string displayName { get { return id.Contains('.') ? id.Substring(id.LastIndexOf('.') + 1) : id; } }

        ///<summary>呈现的值</summary>
        public string displayValue { get { return string.IsNullOrWhiteSpace(value) ? "" : "：" + value; } }
        ///<summary>呈现的颜色</summary>
        public SolidColorBrush displayColor
        {
            get
            {
                Color color = Colors.Transparent;
                try
                {
                    color = (Color)ColorConverter.ConvertFromString(value);
                }
                catch (Exception)
                {
                }
                return new SolidColorBrush(color);
            }
        }
        ///<summary>是否显示颜色</summary>
        public Visibility colorVisibility { get { return valueType == ConfigData.EValueType.颜色 ? Visibility.Visible : Visibility.Collapsed; } }
        ///<summary>是否显示布尔checkbox</summary>
        public Visibility boolVisibility { get { return valueType == ConfigData.EValueType.布尔 ? Visibility.Visible : Visibility.Collapsed; } }
        ///<summary>值文本框是否可编辑</summary>
        public bool isTextValueEnable { get { return valueType == ConfigData.EValueType.数字 || valueType == ConfigData.EValueType.整型 || valueType == ConfigData.EValueType.字符串; } }
        ///<summary>值文本框的mask</summary>
        public string TextValueMask
        {
            get
            {
                if (valueType == ConfigData.EValueType.整型)
                    return @"-?[1-9]\d*";
                else if (valueType == ConfigData.EValueType.数字)
                    return @"-?([1-9]\d*\.\d*|0\.\d*[1-9]\d*|0?\.0+|0)";
                else
                    return "";
            }
        }
        ///<summary>值文本框的masktype</summary>
        public DevExpress.Xpf.Editors.MaskType textValueMaskType { get { return (valueType == ConfigData.EValueType.数字 || valueType == ConfigData.EValueType.整型) ? DevExpress.Xpf.Editors.MaskType.RegEx : DevExpress.Xpf.Editors.MaskType.None; } }


        //----- 下方为useredit使用
        [XmlIgnore]
        public bool isEditing { get; set; }
        public Visibility editVisibleity { get { return isEditing ? Visibility.Visible : Visibility.Collapsed; } }
        //-----

        internal void refreshDispaly()
        {
            RaisePropertyChanged(() => displayName);
            RaisePropertyChanged(() => displayValue);
            RaisePropertyChanged(() => displayColor);
            RaisePropertyChanged(() => colorVisibility);
            RaisePropertyChanged(() => editVisibleity);


        }

        [System.Xml.Serialization.XmlIgnore]
        public ObservableCollection<Item> subitems { get; set; }


        ///<summary>在root中查找并添加进父节点</summary>
        internal void addInParent(ConfigData data)
        {
            if (!data.items.ContainsKey(key))
                data.items.Add(key, this); //加入列表

            id = data.buildID(this); //构建id
            Item pi = data.items.Values.FirstOrDefault(p => p.id == pid);//data.root.find(pid);

            if (pi == null)
            {
                if (id.Contains('.'))
                {
                    string[] ss = id.Split('.');
                    string obj, prop, stat, flg;

                    obj = prop = stat = flg = "";
                    int idx = getindex(data, ConfigData.ESort.对象名);
                    if (idx < ss.Count() - 1) obj = ss[idx];
                    idx = getindex(data, ConfigData.ESort.属性名);
                    if (idx < ss.Count() - 1) prop = ss[idx];
                    idx = getindex(data, ConfigData.ESort.状态);
                    if (idx < ss.Count() - 1) stat = ss[idx];
                    idx = getindex(data, ConfigData.ESort.标志);
                    if (idx < ss.Count() - 1) flg = ss[idx];

                    //for (int i = 0; i < ss.Count()-1; i++)
                    //{
                    //    if (i == 0) obj = ss[i];
                    //    else if (i == 1) prop = ss[i];
                    //    else if (i == 2) stat = ss[i];
                    //    else flg = ss[i];
                    //}
                    pi = new Item(obj, prop, stat, flg);
                    pi.isInAddTree = true;
                    pi.subitems.Add(this);
                    pi.addInParent(data);
                }
                else
                {
                    data.root.subitems.Add(this);
                }
            }
            else
            {
                pi.subitems.Add(this);
            }
        }
        int getindex(ConfigData data, ConfigData.ESort sort)
        {
            if (data.sort1 == sort)
                return 0;
            else if (data.sort2 == sort)
                return 1;
            else if (data.sort3 == sort)
                return 2;
            else
                return 3;
        }

        ///<summary>删除子项，同时将删除在主列表中的项</summary>
        void delSubitem(ConfigData data)
        {
            //删除子项
            foreach (var item in subitems)
            {
                item.delSubitem(data);
                data.items.Remove(item.key);
            }
            subitems.Clear();

        }
        ///<summary>从树和主列表中删除自身和子项</summary>
        internal void delSelf(ConfigData data)
        {
            delSubitem(data);
            Item parentitem = data.items.Values.FirstOrDefault(p => p.id == pid);
            if (parentitem == null && data.root.subitems.Contains(this))
                data.root.subitems.Remove(this);
            else
                parentitem.subitems.Remove(this);
            data.items.Remove(key);
        }

        ///<summary>查找节点</summary>
        internal Item find(string id)
        {
            Item fi = subitems.FirstOrDefault(p => p.id == id);
            if (fi != null)
                return fi;
            else
            {
                foreach (var e in subitems)
                {
                    fi = e.find(id);
                    if (fi != null)
                        return fi;
                }
                return null;
            }
        }


    }





    class TreeViewLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TreeViewItem item = (TreeViewItem)value;
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);
            return ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
}
