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

namespace MyControlLibrary.Controls3D.Index3D
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {


            IndexMain uc = new IndexMain();
            string sql = @"select * from d_index where SORT0='时段'";
            uc.DataSource = DataLayer.DataProvider.getDataTableFromSQL(sql).DefaultView;
            uc.controlTitle = "本年指标";
            grdmain.Children.Add(uc);


        }
    }
}
