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

namespace WcfDataServiceTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        WcfDataServiceTest.ServiceReference1.DataServiceClient wcfdata = new ServiceReference1.DataServiceClient();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //wcfdata.ClientCredentials.UserName.UserName = "admin863";
            //wcfdata.ClientCredentials.UserName.Password = "admin123";

            ////wcfdata.ClientCredentials.UserName.UserName = "test123";
            ////wcfdata.ClientCredentials.UserName.Password = "123456";


            ////wcfdata.UpdateSelfInfomation("管理员A", "admin123", "bbbaaa@ddd.com");
            ////ServiceReference1.UserType tmp= wcfdata.ReadSelfInfomation();
            ////DataTable dt = wcfdata.GetDataTable("select top 10 * from d_index","基础数据源1.5");
            //string ss= wcfdata.test();

            //ss = wcfdata.test();

            //txtinfo.Text = ss.ToString();
        }

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(wcfdata.ClientCredentials.UserName.UserName))
            {
                wcfdata.ClientCredentials.UserName.UserName = "admin863";
                wcfdata.ClientCredentials.UserName.Password = "admin123";
            }
            DateTime d = DateTime.Now;
            string ss = wcfdata.test();
            double dd = (DateTime.Now - d).TotalMilliseconds;
            txtinfo.Text = ss.ToString();
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(wcfdata.ClientCredentials.UserName.UserName))
            {
                wcfdata.ClientCredentials.UserName.UserName = "admin863";
                wcfdata.ClientCredentials.UserName.Password = "admin123";
            }
            //string ss = wcfdata.test();
            //txtinfo.Text = ss.ToString();
            DateTime sd = DateTime.Now;

            DataTable dt = wcfdata.GetDataTable("select * from t_sb_znyc_fhkg", "基础数据源1.6");
            dt = wcfdata.GetDataTable("select * from t_sb_zwyc_dld", "基础数据源1.6");
            dt = wcfdata.GetDataTable("select * from t_sb_zwyc_wlg", "基础数据源1.6");
            dt = wcfdata.GetDataTable("select * from t_tx_znyc_dlq", "基础数据源1.6");
            dt = wcfdata.GetDataTable("select * from t_tx_znyc_glkg", "基础数据源1.6");
            dt = wcfdata.GetDataTable("select * from t_tx_znyc_znljx", "基础数据源1.6");    

            double ccc = (DateTime.Now - sd).TotalMilliseconds;
        }

     
    }
}
