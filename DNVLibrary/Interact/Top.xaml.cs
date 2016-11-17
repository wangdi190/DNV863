using System;
using System.Data;
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
    /// Top.xaml 的交互逻辑
    /// </summary>
    public partial class Top : UserControl
    {
        public Top()
        {
            InitializeComponent();
        }

        internal IEditInstanceViewModel instanceviewmodel;

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            instanceviewmodel = new IEditInstanceViewModel();
            instanceviewmodel.initTree();

        
            //
            cmbTree.ItemsSource = instanceviewmodel.root.subitems;
            
            
        }

        private void btnPrjManage_Click(object sender, RoutedEventArgs e)
        {
            IEditInstanceManagerWindow win = new IEditInstanceManagerWindow();
            win.viewModel = instanceviewmodel;
            win.ShowDialog();
            win = null;
        }

        ///<summary>设置当前实例，不可用待完成</summary>
        public void setCurInstance(int instanceid)
        {
            //Item item = instanceviewmodel.root.find(instanceid);
            //object obj= cmbTree.tree.ItemContainerGenerator.ContainerFromItem(item);
        }


    }

 
}
