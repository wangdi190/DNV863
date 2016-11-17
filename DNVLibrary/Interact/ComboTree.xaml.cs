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

namespace DNVLibrary.Interact
{
    /// <summary>
    /// ComboTree.xaml 的交互逻辑
    /// </summary>
    public partial class ComboTree : UserControl
    {
        public ComboTree()
        {
            InitializeComponent();
        }
        System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200) };
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            tmr.Tick += new EventHandler(tmr_Tick);
        }

        void tmr_Tick(object sender, EventArgs e)
        {
            PopupTest.IsOpen = false;
            tmr.Stop();
        }


        private void Tree1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = tree.SelectedItem;
            if (tree.SelectedItem == null)
            {
                header.Text = "";
                Header.ToolTip = "";
                return;
            }
            string display,tooltip;
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                display = tree.SelectedItem.ToString();
            }
            else
            {
                Type type = tree.SelectedItem.GetType(); //获取类型
                System.Reflection.PropertyInfo propertyInfo = type.GetProperty(DisplayName); //获取指定名称的属性
                display = propertyInfo.GetValue(tree.SelectedItem, null).ToString(); //获取属性值
            }
            header.Text = display;

            if (string.IsNullOrWhiteSpace(ToolTipName))
            {
                tooltip = null;
            }
            else
            {
                Type type = tree.SelectedItem.GetType(); //获取类型
                System.Reflection.PropertyInfo propertyInfo = type.GetProperty(ToolTipName); //获取指定名称的属性
                tooltip = propertyInfo.GetValue(tree.SelectedItem, null).ToString(); //获取属性值
            }
            Header.ToolTip =tooltip;
            //var trvItem = trv.SelectedItem as TreeViewItem;
            ////if (trvItem.Items.Count != 0) return;
            //header.Text = trvItem.Header.ToString();
            
            //PopupTest.IsOpen = false;
            tmr.Start();
        }


        private void header_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PopupTest.IsOpen)
            {
                PopupTest.IsOpen = false;
            }
            else
            {
                PopupTest.Placement = System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
                PopupTest.VerticalOffset = header.ActualHeight;// header.Height;
                PopupTest.StaysOpen = true;
                PopupTest.Height = tree.Height;
                PopupTest.Width = this.ActualWidth; //header.Width;
                PopupTest.IsOpen = true;
            }
        }


        private IEnumerable<object> _ItemsSource;
        ///<summary>项目源</summary>
        public IEnumerable<object> ItemsSource
        {
            get { return _ItemsSource; }
            set
            {
                _ItemsSource = value;
                tree.ItemsSource = value;
            }
        }


        private DataTemplate _ItemTemplate;
        ///<summary>树模板</summary>
        public DataTemplate ItemTemplate
        {
            get { return _ItemTemplate; }
            set
            {
                _ItemTemplate = value;
                tree.ItemTemplate = value;
            }
        }


        private string _DisplayName;
        ///<summary>显示属性名</summary>
        public string DisplayName
        {
            get { return _DisplayName; }
            set { _DisplayName = value; }
        }

        private string _ToolTipName;
        ///<summary>Tooltip显示属性名</summary>
        public string ToolTipName
        {
            get { return _ToolTipName; }
            set { _ToolTipName = value; }
        }

        
        private object _SelectedItem;
        ///<summary>选择的项目</summary>
        public object SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                _SelectedItem = value; 
                RaiseSelectItemChangedEvent();
            }
        }

        
        ///<summary>树项目选择改变事件</summary>
        public event EventHandler SelectItemChanged;
        protected virtual void RaiseSelectItemChangedEvent()
        {
            if (SelectItemChanged != null)
                SelectItemChanged(this, null);
        }

     
      








    }
}
