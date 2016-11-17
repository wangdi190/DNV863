using System;
using System.Windows;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace DNVLibrary.Interact
{
    internal class IEditInstanceViewModel : MyClassLibrary.MVVM.NotificationObject
    {
        ///<summary>根</summary>
        public Item root { get; set; }


        private Item _curItem;
        ///<summary>当前项</summary>
        public Item curItem
        {
            get { return _curItem; }
            set { _curItem = value; RaisePropertyChanged(() => curItem); }
        }

        ///<summary>删除项集合，仅原在数据库中有效</summary>
        internal List<Item> deleteItems = new List<Item>();


        ///<summary>从数据库初始化树</summary>
        internal void initTree()
        {
            root = new Item();
            root.id = -1;
            string sql = string.Format("select * from all_instance");
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(sql);
            root.addChild(dt);

            dt = null;
        }

    }

    public class Item : MyClassLibrary.MVVM.NotificationObject
    {
        public Item()
        {
            subitems = new ObservableCollection<Item>();
        }


        ///<summary>根据树构成方式不同而不同</summary>
        public int id { get; set; }
        ///<summary>根据树构成方式不同而不同</summary>
        public int? pid { get; set; }
        ///<summary>实例名</summary>
        public string instanceName { get; set; }
        ///<summary>实例说明</summary>
        public string instanceNote { get; set; }
        ///<summary>规划年</summary>
        public int planningYear { get; set; }
        ///<summary>实例是否作为方案</summary>
        public bool isProject { get; set; }

        ///<summary>是否已在数据库中</summary>
        internal bool isDataBase { get; set; }
        internal Item parentItem { get; set; }
                
        public ObservableCollection<Item> subitems { get; set; }

        ///<summary>包含自身的所有项</summary>
        public List<Item> allitems
        {
            get
            {
                List<Item> tmp = (from e0 in subitems from e1 in e0.allitems select e1).ToList();
                if (parentItem!=null) //排除根
                    tmp.Add(this);
                return tmp;
            }
        }

        public int maxID { get { return subitems.Count == 0 ? id : Math.Max(id, subitems.Max(p => p.maxID)); } }


        ///<summary>创建所属子节点</summary>
        internal void addChild(DataTable dt)
        {
            var tmp = dt.AsEnumerable().Where(p => id < 0 ? p["parentid"] is DBNull : p.getInt("parentid") == id);
            foreach (var e0 in tmp)
            {
                Item item = new Item()
                {
                    id = e0.getInt("id"),
                    pid = null,
                    instanceName = e0.getString("instancename"),
                    instanceNote = e0.getString("note"),
                    planningYear = e0.getInt("year"),
                    isProject = e0.getBool("isproject"),
                    isDataBase = true,
                    parentItem = this,
                };
                subitems.Add(item);
                item.addChild(dt);
            }

        }

        ///<summary>从树和主列表中删除自身和子项, 删除的数据库中的项进入删除集合</summary>
        internal void delSelf(IEditInstanceViewModel viewModel)
        {
            while (subitems.Count > 0)
                subitems[0].delSelf(viewModel);

            if (isDataBase)
                viewModel.deleteItems.Add(this);
            parentItem.subitems.Remove(this);
        }

        ///<summary>查找节点</summary>
        internal Item find(int id)
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
