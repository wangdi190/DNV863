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
    /// UCEdit.xaml 的交互逻辑
    /// </summary>
    public partial class UCDeveloperEdit : UserControl
    {
        public UCDeveloperEdit()
        {
            InitializeComponent();
        }

        ConfigData data;
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            data = Config.cfgData;
            if (data != null)
            {
                data.buildTree(true);
                tree.ItemsSource = data.root.subitems;

                //初始化重构树的控件
                for (int i = 0; i < 4; i++)
                {
                    cmbsort1.Items.Add((ConfigData.ESort)i);
                }
                cmbsort1.SelectedItem = data.sort1;
                cmbsort2.SelectedItem = data.sort2;
                cmbsort3.SelectedItem = data.sort3;
                //初始化值类型
                for (int i = 0; i < Enum.GetNames(typeof(ConfigData.EValueType)).Count(); i++)
                {
                    cmbValueType.Items.Add((ConfigData.EValueType)i);
                }
            }
        }


        private void btnDel_Click(object sender, RoutedEventArgs e)
        {

            if (tree.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("是否确定要删除选中项及其子项？删除后不可恢复。", "删除", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Item delitem = tree.SelectedItem as Item;
                    delitem.delSelf(data);
                    
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(txtObj.Text) && string.IsNullOrWhiteSpace(txtProperty.Text)) return;
            if (data == null)
            {
                data = new ConfigData(); ;
                tree.ItemsSource = data.root.subitems;
            }
            string key = Item.buildKey(txtObj.Text, txtProperty.Text, txtStatus.Text, txtFlag.Text);
            Item item;
            if (data.items.ContainsKey(key))
                item = data.items[key];
            else
            {
                item = new Item(txtObj.Text, txtProperty.Text, txtStatus.Text, txtFlag.Text);
                item.addInParent(data);
            }

            item.note = txtNote.Text;
            item.valueType = (ConfigData.EValueType)cmbValueType.SelectedItem;
            item.isUserEditable = (bool)chkUserEditable.IsChecked;
            item.value = txtValue.Text;
            item.refreshDispaly();
        }

        private void btnSaveXml_Click(object sender, RoutedEventArgs e)
        {
            Config.save();
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            key.Text = Item.buildKey(txtObj.Text, txtProperty.Text, txtStatus.Text, txtFlag.Text);
        }

        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Item it = (Item)e.NewValue;
            if (it != null)
            {
                txtObj.Text = it.obj;
                txtProperty.Text = it.property;
                txtStatus.Text = it.status;
                txtFlag.Text = it.flag;
                txtValue.Text = it.value;
                txtNote.Text = it.note;
                cmbValueType.SelectedItem = it.valueType;
                chkUserEditable.IsChecked = it.isUserEditable;
                key.Text = it.key;

                if (it.valueType == ConfigData.EValueType.布尔)
                    chkBoolValue.IsChecked = (bool)Convert.ToBoolean(it.value);
            }
        }

        private void btnCopyText_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetDataObject(key.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("复制失败！");

            }

        }

        private void btnRebuildTree_Click(object sender, RoutedEventArgs e)
        {
            data.sort1 =(ConfigData.ESort)cmbsort1.SelectedItem;
            data.sort2 = (ConfigData.ESort)cmbsort2.SelectedItem;
            data.sort3 = (ConfigData.ESort)cmbsort3.SelectedItem;
            data.sort4 = (ConfigData.ESort)cmbsort4.SelectedItem;
            data.buildTree(true);
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
            if (cmbsort2.SelectedIndex>-1)
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
            if (winColor==null) winColor = new WinSelectColor();
            winColor.ShowDialog();
            if (!winColor.isCancel)
            {
                txtValue.Text = winColor.color.ToString();
                rectColor.Fill = new SolidColorBrush(winColor.color);
            }
        }

        private void cmbValueType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbValueType.SelectedIndex < 0)
                return;
            ConfigData.EValueType vtype = (ConfigData.EValueType)cmbValueType.SelectedItem;
            foreach (FrameworkElement item in grdInputButton.Children)
                item.Visibility = System.Windows.Visibility.Collapsed;
            if (vtype == ConfigData.EValueType.颜色)
            {
                btnSelColor.Visibility = System.Windows.Visibility.Visible;
                Color color = Colors.Transparent;
                try
                {
                    color = (Color)ColorConverter.ConvertFromString(txtValue.Text);
                }
                catch (Exception)
                {
                }
                rectColor.Fill = new SolidColorBrush(color);
            }
            else if (vtype == ConfigData.EValueType.布尔)
                chkBoolValue.Visibility = System.Windows.Visibility.Visible;


            //正则表达式
            txtValue.IsEnabled = true;
            switch (vtype)
            {
                case ConfigData.EValueType.字符串:
                    txtValue.MaskType = DevExpress.Xpf.Editors.MaskType.None;
                    break;
                case ConfigData.EValueType.颜色:
                    txtValue.MaskType = DevExpress.Xpf.Editors.MaskType.None;
                    txtValue.IsEnabled = false;
                    break;
                case ConfigData.EValueType.整型:
                    txtValue.MaskType = DevExpress.Xpf.Editors.MaskType.RegEx;
                    txtValue.Mask=@"-?[1-9]\d*";
                    break;
                case ConfigData.EValueType.数字:
                    txtValue.MaskType = DevExpress.Xpf.Editors.MaskType.RegEx;
                    txtValue.Mask=@"-?([1-9]\d*\.\d*|0\.\d*[1-9]\d*|0?\.0+|0)";
                    break;
                case ConfigData.EValueType.布尔:
                    txtValue.MaskType = DevExpress.Xpf.Editors.MaskType.None;
                    txtValue.IsEnabled = false;
                    break;
                default:
                    txtValue.MaskType = DevExpress.Xpf.Editors.MaskType.None;
                    break;
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

        private void chkBoolValue_Checked(object sender, RoutedEventArgs e)
        {
            txtValue.Text = "true";
        }

        private void chkBoolValue_Unchecked(object sender, RoutedEventArgs e)
        {
            txtValue.Text = "false";
        }
    }
}
