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
using System.Text.RegularExpressions;

namespace DataLayer.WCF
{
    /// <summary>
    /// UCManageUser.xaml 的交互逻辑
    /// </summary>
    public partial class UCManageUser : UserControl
    {
        public UCManageUser()
        {
            InitializeComponent();
        }

        System.Collections.ObjectModel.ObservableCollection<RoleData> roles = new System.Collections.ObjectModel.ObservableCollection<RoleData>();
        System.Collections.ObjectModel.ObservableCollection<UserData> users = new System.Collections.ObjectModel.ObservableCollection<UserData>();
        string appid;

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            appid = DataProvider.wcf.GetApplicationID();

            refreshUsers();

            string sql;
            DataTable dt;
            sql = string.Format("select RoleID,RoleName from roles where applicationid='{0}'", appid);
            dt = DataProvider.wcf.GetManageData(sql);
            foreach (DataRow dr in dt.Rows)
                roles.Add(new RoleData() { roleID = dr[0].ToString(), roleName = dr[1].ToString() });
            //lstRole.ItemsSource = roles;
        }

        void refreshUsers()
        {
            string sql;
            DataTable dt;
            users.Clear();
            sql = string.Format("select t1.UserID,t1.UserName,t1.Alias,t1.IsLocked,(select COUNT(*) from UsersInRoles t2 where t2.UserID=t1.UserID and t2.RoleID in (select RoleID from Roles where RoleName='管理员')) as isAdmin from users t1 where ApplicationID='{0}'", appid);
            dt = DataProvider.wcf.GetManageData(sql);
            foreach (DataRow dr in dt.Rows)
            {
                UserData ud = new UserData() { userID = dr[0].ToString(), userName = dr[1].ToString(), alias = dr[2].ToString(), isLock = (bool)dr[3], isAdmin = dr.Field<int>(4) > 0 };
                ud.brushAdmin = ud.isAdmin ? (Brush)FindResource("ibAdmin") : Brushes.Transparent;
                ud.brushLock = ud.isLock ? (Brush)FindResource("ibLock") : Brushes.Transparent;
                users.Add(ud);
            }
            lstUser.ItemsSource = users;

        }


        private void lstUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstUser.SelectedItem == null)
            {
                lstRole.ItemsSource = null;
                txtAlias.Text = txtCreateDate.Text = txtEmail.Text = txtLastActivityDate.Text = txtLastLockDate.Text = txtLastLoginDate.Text = txtPassword.Password = txtStatus.Text = txtUserName.Text = "";
                btnDel.IsEnabled = btnLock.IsEnabled = btnUpdate.IsEnabled = btnUpdateRole.IsEnabled = false;
                return;
            }

            btnDel.IsEnabled = btnLock.IsEnabled = btnUpdate.IsEnabled = btnUpdateRole.IsEnabled = true;
            string sql;
            DataTable dt;
            string uid = (lstUser.SelectedItem as UserData).userID;
            sql = string.Format("select * from users where userid='{0}'", uid);
            dt = DataProvider.wcf.GetManageData(sql);
            txtAlias.Text = dt.Rows[0]["Alias"].ToString();
            txtCreateDate.Text = dt.Rows[0]["CreateDate"].ToString();
            txtEmail.Text = dt.Rows[0]["Email"].ToString();
            txtLastActivityDate.Text = dt.Rows[0]["LastActivityDate"].ToString();
            txtLastLockDate.Text = dt.Rows[0]["LastLockDate"].ToString();
            txtLastLoginDate.Text = dt.Rows[0]["LastLoginDate"].ToString();
            txtPassword.Password = "";
            txtStatus.Text = (bool)dt.Rows[0]["isLocked"] ? "加锁" : "正常";
            btnLock.Content = (bool)dt.Rows[0]["isLocked"] ? "解锁" : "加锁";
            txtUserName.Text = dt.Rows[0]["UserName"].ToString();



            foreach (var item in roles)
                item.isInclude = false;
            sql = string.Format("select * from usersinroles where userid='{0}'", uid);
            dt = DataProvider.wcf.GetManageData(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string tmproleid = dr["RoleID"].ToString();
                roles.First(p => p.roleID == tmproleid).isInclude = true;
            }
            lstRole.ItemsSource = roles;
        }



        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            WinNewUser win = new WinNewUser();
            if ((bool)win.ShowDialog())
            {
                string newusername = win.txtUserName.Text;
                refreshUsers();
                UserData newuser = users.First(p => p.userName == newusername);
                lstUser.SelectedItem = newuser;
            }

        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("删除后不可恢复，确定要删除吗？", "删除确认", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            string uid = (lstUser.SelectedItem as UserData).userID;
            UserData curuser = lstUser.SelectedItem as UserData;
            lstUser.SelectedItem = null;

            string sql = string.Format("delete usersinroles where userid='{0}'", uid);
            DataProvider.wcf.ExecuteManageCommand(sql);
            sql = string.Format("delete users where userid='{0}'", uid);
            DataProvider.wcf.ExecuteManageCommand(sql);

            users.Remove(curuser);
            info.Text = string.Format("用户{0}已删除!",curuser.userName);
        }

        private void btnLock_Click(object sender, RoutedEventArgs e)
        {
            bool curIsLocked = btnLock.Content == "解锁";
            string uid = (lstUser.SelectedItem as UserData).userID;
            string sql = string.Format("update users set islocked={1},lastlockdate='{2}' where userid='{0}'", uid, curIsLocked ? 0 : 1, DateTime.Now);
            DataProvider.wcf.ExecuteManageCommand(sql);
            curIsLocked = !curIsLocked;
            txtStatus.Text = curIsLocked ? "加锁" : "正常";
            btnLock.Content = curIsLocked ? "解锁" : "加锁";

            (lstUser.SelectedItem as UserData).isLock = curIsLocked;
            (lstUser.SelectedItem as UserData).brushLock = curIsLocked ? (Brush)FindResource("ibLock") : Brushes.Transparent;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
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

            //更新
            string uid = (lstUser.SelectedItem as UserData).userID;
            DataProvider.wcf.UpdateUserPassword(uid, txtPassword.Password);

            info.Text = "用户口令已更新！";
        }

        private void btnUpdateRole_Click(object sender, RoutedEventArgs e)
        {
            string uid = (lstUser.SelectedItem as UserData).userID;
            string sql = string.Format("delete usersinroles where userid='{0}'", uid);
            DataProvider.wcf.ExecuteManageCommand(sql);
            (lstUser.SelectedItem as UserData).isAdmin = false;
            foreach (var item in roles.Where(p => p.isInclude))
            {
                sql = string.Format("insert usersinroles (userid,roleid) values ('{0}','{1}')", uid, item.roleID);
                DataProvider.wcf.ExecuteManageCommand(sql);
                if (item.roleName == "管理员")
                    (lstUser.SelectedItem as UserData).isAdmin = true;
            }

            (lstUser.SelectedItem as UserData).brushAdmin = (lstUser.SelectedItem as UserData).isAdmin ? (Brush)FindResource("ibAdmin") : Brushes.Transparent;
            info.Text = "用户角色信息已更新!";
        }

    }

    class ModelData : MyClassLibrary.MVVM.NotificationObject
    {
        public string modelID { get; set; }
        public string modelName { get; set; }


        private bool _isInclude;
        public bool isInclude
        {
            get { return _isInclude; }
            set { _isInclude = value; RaisePropertyChanged(() => isInclude); }
        }

    }

    class RoleData : MyClassLibrary.MVVM.NotificationObject
    {
        public string roleID { get; set; }
        public string roleName { get; set; }

        
        private bool _isInclude;
        public bool isInclude
        {
            get { return _isInclude; }
            set { _isInclude = value; RaisePropertyChanged(() => isInclude); }
        }

        private Brush _brushAdmin;
        public Brush brushAdmin
        {
            get { return _brushAdmin; }
            set { _brushAdmin = value; RaisePropertyChanged(() => brushAdmin); }
        }

    }

    class UserData: MyClassLibrary.MVVM.NotificationObject
    {
        public string userID { get; set; }
        public string userName { get; set; }
        public string alias { get; set; }
        public bool isAdmin { get; set; }
        public bool isLock { get; set; }

        
        private Brush _brushAdmin;
        public Brush brushAdmin
        {
            get { return _brushAdmin; }
            set { _brushAdmin = value; RaisePropertyChanged(() => brushAdmin); }
        }

        
        private Brush _brushLock;
        public Brush brushLock
        {
            get { return _brushLock; }
            set { _brushLock = value; RaisePropertyChanged(() => brushLock); }
        }
    }
}
