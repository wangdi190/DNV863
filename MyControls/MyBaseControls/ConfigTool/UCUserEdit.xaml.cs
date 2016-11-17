using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyClassLibrary;


namespace MyBaseControls.ConfigTool
{
    /// <summary>
    /// UCUserEdit.xaml 的交互逻辑
    /// </summary>
    public partial class UCUserEdit : UserControl
    {
        public UCUserEdit()
        {
            InitializeComponent();
        }

        ConfigData data;
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            data = Config.cfgData;
            if (data != null)
            {
                data.buildTree(false);
                tree.ItemsSource = data.root.subitems;

                //初始化重构树的控件
                for (int i = 0; i < 4; i++)
                {
                    cmbsort1.Items.Add((ConfigData.ESort)i);
                }
                cmbsort1.SelectedItem = data.sort1;
                cmbsort2.SelectedItem = data.sort2;
                cmbsort3.SelectedItem = data.sort3;

            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            btnApply.Visibility = Config.isMemory ? Visibility.Visible : Visibility.Collapsed;

            CollapseAll();

        }
        // 全部收缩
        private void CollapseAll()
        {
            foreach (var item in tree.Items)
            {
                DependencyObject dObject = tree.ItemContainerGenerator.ContainerFromItem(item);
                CollapseTreeviewItems(((TreeViewItem)dObject));
            }
        }
        void CollapseTreeviewItems(TreeViewItem Item)
        {
            Item.IsExpanded = false;
            foreach (var item in Item.Items)
            {
                DependencyObject dObject = tree.ItemContainerGenerator.ContainerFromItem(item);
                if (dObject != null)
                {
                    ((TreeViewItem)dObject).IsExpanded = false;
                    if (((TreeViewItem)dObject).HasItems) { CollapseTreeviewItems(((TreeViewItem)dObject)); }
                }
            }
        }

        private void btnSaveXml_Click(object sender, RoutedEventArgs e)
        {
            Config.save();
        }

        Item selitem;
        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (selitem != null)
            {
                selitem.isEditing = false;
                selitem.refreshDispaly();
            }
            Item it = (Item)e.NewValue;
            if (it != null)
            {
                it.isEditing = true;
                it.refreshDispaly();
                selitem = it;
            }
        }

        private void btnRebuildTree_Click(object sender, RoutedEventArgs e)
        {
            data.sort1 = (ConfigData.ESort)cmbsort1.SelectedItem;
            data.sort2 = (ConfigData.ESort)cmbsort2.SelectedItem;
            data.sort3 = (ConfigData.ESort)cmbsort3.SelectedItem;
            data.sort4 = (ConfigData.ESort)cmbsort4.SelectedItem;
            data.buildTree(false);
        }

        private void cmbsort1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbsort2.Items.Clear();
            for (int i = 0; i < 4; i++)
            {
                if ((ConfigData.ESort)i != (ConfigData.ESort)cmbsort1.SelectedItem)
                    cmbsort2.Items.Add((ConfigData.ESort)i);
            }
            validcmbsort();
        }


        private void cmbsort2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbsort3.Items.Clear();
            if (cmbsort2.SelectedIndex > -1)
                for (int i = 0; i < 4; i++)
                {
                    if ((ConfigData.ESort)i != (ConfigData.ESort)cmbsort1.SelectedItem && (ConfigData.ESort)i != (ConfigData.ESort)cmbsort2.SelectedItem)
                        cmbsort3.Items.Add((ConfigData.ESort)i);
                }
            validcmbsort();
        }

        private void cmbsort3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbsort4.Items.Clear();
            if (cmbsort3.SelectedIndex > -1)
            {
                for (int i = 0; i < 4; i++)
                {
                    if ((ConfigData.ESort)i != (ConfigData.ESort)cmbsort1.SelectedItem && (ConfigData.ESort)i != (ConfigData.ESort)cmbsort2.SelectedItem && (ConfigData.ESort)i != (ConfigData.ESort)cmbsort3.SelectedItem)
                        cmbsort4.Items.Add((ConfigData.ESort)i);
                }
                cmbsort4.SelectedIndex = 0;
            }
            validcmbsort();
        }

        void validcmbsort()
        {
            btnRebuildTree.IsEnabled = cmbsort1.SelectedIndex > -1 && cmbsort2.SelectedIndex > -1 && cmbsort3.SelectedIndex > -1 && cmbsort4.SelectedIndex > -1;
        }

        WinSelectColor winColor;
        private void btnSelColor_Click(object sender, RoutedEventArgs e)
        {
            if (winColor == null)
            {
                winColor = new WinSelectColor();

            }
            winColor.ShowDialog();
            if (!winColor.isCancel)
            {
                selitem.value = winColor.color.ToString();
                selitem.refreshDispaly();
            }

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (winColor != null)
            {
                winColor.Close();
                winColor = null;
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (refreshObject != null && selitem != null)
                refreshObject(selitem.key);
        }


        public delegate void RefreshObject(string cfgkey);
        ///<summary>委托刷新</summary>
        public RefreshObject refreshObject { get; set; }

        

    }
}
