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
using System.Windows.Shapes;

namespace DataLayer.WCF
{
    /// <summary>
    /// WinLogin.xaml 的交互逻辑
    /// </summary>
    public partial class WinLogin : Window
    {
        public WinLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            login();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

     
        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                login();
        }

        void login()
        {
            DataProvider.wcf = new WcfDataService.DataServiceClient();
            DataProvider.wcf.ClientCredentials.UserName.UserName = txtUserName.Text;
            DataProvider.wcf.ClientCredentials.UserName.Password = txtPassword.Password;
            try
            {
                DataProvider.wcf.test();
                this.DialogResult = true;
            }
            catch (Exception ei)
            {
                info.Foreground = Brushes.Red;
                if (ei.InnerException == null)
                    info.Text = ei.Message;
                else
                    info.Text = ei.InnerException.Message;
                txtPassword.Password = "";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUserName.Focus();
        }
    }
}
