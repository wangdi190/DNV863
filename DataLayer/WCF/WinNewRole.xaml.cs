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
using System.Windows.Shapes;

namespace DataLayer.WCF
{
    /// <summary>
    /// WinNewRole.xaml 的交互逻辑
    /// </summary>
    public partial class WinNewRole : Window
    {
        public WinNewRole()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtRoleName.Focus();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (txtRoleName.Text.Length < 2)
            {
                info.Text = "角色名称长度不小于2！";
                return;
            }

            string apiid = DataProvider.wcf.GetApplicationID();
            string sql = string.Format("select rolename from roles where rolename='{0}' and applicationid='{1}'", txtRoleName.Text, apiid);
            DataTable dt = DataProvider.wcf.GetManageData(sql);
            if (dt.Rows.Count > 0)
            {
                info.Text = "同名角色已存在，请更改角色名称！";
                return;
            }

            sql = string.Format("insert roles (applicationid,rolename) values ('{0}','{1}')", apiid, txtRoleName.Text);
            DataProvider.wcf.ExecuteManageCommand(sql);

            DialogResult = true;
            this.Close();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();

        }
    }
}
