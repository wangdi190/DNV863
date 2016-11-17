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
using System.Text.RegularExpressions;

namespace DataLayer.WCF
{
    /// <summary>
    /// WinNewUser.xaml 的交互逻辑
    /// </summary>
    public partial class WinNewUser : Window
    {
        public WinNewUser()
        {
            InitializeComponent();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (txtUserName.Text.Length<3)
            {
                info.Text = "用户名长度不小于3！";
                return;
            }
            string pattern;
            //校验口令强度
            if (DataProvider.passwordStrong == EPasswordStrong.中)
            {
                if (txtPassword.Password.Length < 6 || txtPassword.Password.Length > 18)
                {
                    info.Text = "口令要求6-18位长度，同时包含字母和数字。";
                    return;
                }
                int count = 0;
                pattern = @"[0-9]+";
                if (Regex.IsMatch(txtPassword.Password, pattern))
                    count++;
                pattern = @"[a-zA-Z]+";
                if (Regex.IsMatch(txtPassword.Password, pattern))
                    count++;

                if (count < 2)
                {
                    info.Text = "口令要求6-18位长度，同时包含字母和数字。";
                    return;
                }
            }
            else if (DataProvider.passwordStrong == EPasswordStrong.强)
            {
                if (txtPassword.Password.Length < 6 || txtPassword.Password.Length > 18)
                {
                    info.Text = "口令要求至少8-18长度，同时包含大小写字母、数字和特殊字符。";
                    return;
                }
                int count = 0;
                pattern = @"[0-9]+";
                if (Regex.IsMatch(txtPassword.Password, pattern))
                    count++;
                pattern = @"[a-z]+";
                if (Regex.IsMatch(txtPassword.Password, pattern))
                    count++;
                pattern = @"[A-Z]+";
                if (Regex.IsMatch(txtPassword.Password, pattern))
                    count++;
                pattern = @"[~!@#$%^&*()_+]+";
                if (Regex.IsMatch(txtPassword.Password, pattern))
                    count++;

                if (count < 4)
                {
                    info.Text = "口令要求至少8-18长度，同时包含大小写字母、数字和特殊字符。";
                    return;
                }

            }

            string sql =string.Format("select username from users where username='{0}'",txtUserName.Text);
            DataTable dt = DataProvider.wcf.GetManageData(sql);
            if (dt.Rows.Count>0)
            {
                info.Text = "同名用户已存在，请更改用户名！";
                return;
            }

            DataProvider.wcf.AddNewUser(txtUserName.Text, txtPassword.Password);

            DialogResult = true;
            this.Close();


        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUserName.Focus();
        }

    }
}
