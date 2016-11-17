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
using System.Text.RegularExpressions;

namespace DataLayer.WCF
{
    /// <summary>
    /// UCUserInfo.xaml 的交互逻辑
    /// </summary>
    public partial class UCUserInfo : UserControl
    {
        public UCUserInfo()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            WcfDataService.UserType ui = DataProvider.wcf.ReadSelfInfomation();
            txtAlias.Text = ui.alias;
            txtCreateDate.Text = ui.createDate;
            txtEmail.Text = ui.email;
            txtLastActivityDate.Text = ui.lastActivityDate;
            txtLastLockDate.Text = ui.lastLockDate;
            txtLastLoginDate.Text = ui.lastLoginDate;
            txtPassword.Password = ui.password;
            txtStatus.Text = ui.isLock ? "加锁" : "正常";
            txtUserName.Text = ui.username;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            string pattern;
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                pattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
                if (!Regex.IsMatch(txtEmail.Text, pattern))
                {
                    info.Text = "EMail地址格式不正确！";
                    return;
                }
            }
            //校验口令强度
            if (DataProvider.passwordStrong== EPasswordStrong.中)
            {
                if(txtPassword.Password.Length < 6 ||txtPassword.Password.Length >18) 
                {
                    info.Text="口令要求6-18位长度，同时包含字母和数字。";
                    return;
                }
                int count=0;
                pattern=@"[0-9]+";
                if (Regex.IsMatch(txtPassword.Password,pattern))
                    count++;
                pattern=@"[a-zA-Z]+";
                if (Regex.IsMatch(txtPassword.Password,pattern))
                    count++;
                
                if (count<2)
                {
                    info.Text="口令要求6-18位长度，同时包含字母和数字。";
                    return;
                }
            }
            else if (DataProvider.passwordStrong== EPasswordStrong.强)
            {
                 if(txtPassword.Password.Length < 6 ||txtPassword.Password.Length >18) 
                {
                    info.Text="口令要求至少8-18长度，同时包含大小写字母、数字和特殊字符。";
                    return;
                }
                int count=0;
                pattern=@"[0-9]+";
                if (Regex.IsMatch(txtPassword.Password,pattern))
                    count++;
                pattern=@"[a-z]+";
                if (Regex.IsMatch(txtPassword.Password,pattern))
                    count++;
                pattern=@"[A-Z]+";
                if (Regex.IsMatch(txtPassword.Password,pattern))
                    count++;
                pattern=@"[~!@#$%^&*()_+]+";
                if (Regex.IsMatch(txtPassword.Password,pattern))
                    count++;
                
                if (count<4)
                {
                    info.Text="口令要求至少8-18长度，同时包含大小写字母、数字和特殊字符。";
                    return;
                }

            }

            //更新
            DataProvider.wcf.UpdateSelfInfomation(txtAlias.Text, txtPassword.Password, txtEmail.Text);
            info.Text = "用户信息已更新！";

        }
    }
}
