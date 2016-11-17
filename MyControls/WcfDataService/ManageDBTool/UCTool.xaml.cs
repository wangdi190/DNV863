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

namespace ManageDBTool
{
    /// <summary>
    /// UCTool.xaml 的交互逻辑
    /// </summary>
    public partial class UCTool : UserControl
    {
        public UCTool()
        {
            InitializeComponent();
        }

        viewmodel vm = new viewmodel();
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            vm.dtApplication = DBOperate.getDataTable("select * from applications");


            grdMain.DataContext = vm;
        }

        private void btnNewApp_Click(object sender, RoutedEventArgs e)
        {
            string sql;
            string name =txtAppName.Text;
            if (name.Length < 2)
            {
                txtInfo.Text = "应用程序名称长度不小于2";
                return;
            }
            //不允许应用重名
            sql = string.Format("select applicationname from applications where applicationname='{0}'", name);
            DataTable dt = DBOperate.getDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                txtInfo.Text = "同名应用程序已存在，请更改应用名称!";
                return;
            }

            sql = string.Format("insert applications (applicationname) values ('{0}')", name);
            if (DBOperate.executeCommand(sql))
            {
                vm.refreshApplication();
                txtInfo.Text = string.Format("应用程序{0}已添加。", name);
                txtAppName.Text = "";
            }

        }

        private void btnEditApp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDelApp_Click(object sender, RoutedEventArgs e)
        {
            if (lstApplication.SelectedItem == null) return;
            if (MessageBox.Show("删除后不可恢复，确定要删除吗？", "删除确认", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;

            string appid = vm.applicationid;
            lstApplication.SelectedIndex = -1;
            //先删除两个关系
            string sql = string.Format("delete UsersInRoles where RoleID in (select RoleID from Roles where ApplicationID='{0}')", appid);
            if (DBOperate.executeCommand(sql))
                vm.refreshRoleUser();
            sql = string.Format("delete ModelsInRoles where RoleID in (select RoleID from Roles where ApplicationID='{0}')", appid);
            if (DBOperate.executeCommand(sql))
                vm.refreshRoleModel();
            //再删除其余相关表
            sql = string.Format("delete connections where ApplicationID='{0}'", appid);
            if (DBOperate.executeCommand(sql))
                vm.refreshConn();
            sql = string.Format("delete models where ApplicationID='{0}'", appid);
            if (DBOperate.executeCommand(sql))
                vm.refreshModel();
            sql = string.Format("delete users where ApplicationID='{0}'", appid);
            if (DBOperate.executeCommand(sql))
                vm.refreshUser();
            sql = string.Format("delete roles where ApplicationID='{0}'", appid);
            if (DBOperate.executeCommand(sql))
                vm.refreshRole();
            //最后本表
            sql = string.Format("delete applications where ApplicationID='{0}'", appid);
            if (DBOperate.executeCommand(sql))
                vm.refreshApplication();

        }

        private void btnNewRole_Click(object sender, RoutedEventArgs e)
        {
            string sql;
            string name = txtRoleName.Text;
            if (name.Length < 2 || string.IsNullOrWhiteSpace(vm.applicationid))
            {
                txtInfo.Text = "角色名称长度不小于2";
                return;
            }
            //同一程序中，不允许角色重名
            sql = string.Format("select rolename from roles where applicationid='{0}' and rolename='{1}'", vm.applicationid, name);
            DataTable dt = DBOperate.getDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                txtInfo.Text = "角色名已存在，请更改!";
                return;
            }

            sql = string.Format("insert roles (applicationid,rolename) values ('{0}','{1}')", vm.applicationid, name);
            if (DBOperate.executeCommand(sql))
            {
                vm.refreshRole();
                txtInfo.Text = string.Format("角色{0}已添加。", name);
                txtRoleName.Text = "";
            }
        }

        private void btnEditRole_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDelRole_Click(object sender, RoutedEventArgs e)
        {
            if (lstRole.SelectedItem == null || string.IsNullOrWhiteSpace(vm.applicationid)) return;
            if (MessageBox.Show("删除后不可恢复，确定要删除吗？", "删除确认", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;

            string roleid = (lstRole.SelectedItem as DataRowView).Row.Field<System.Guid>("roleid").ToString();
            //先删除角色用户关系
            string sql = string.Format("delete usersinroles where roleid='{0}'", roleid);
            if (DBOperate.executeCommand(sql))
                vm.refreshRoleUser();
            //先删除角色模块关系
            sql = string.Format("delete modelsinroles where roleid='{0}'", roleid);
            if (DBOperate.executeCommand(sql))
                vm.refreshRoleModel();
            //再删除角色本身
            sql = string.Format("delete roles where roleid='{0}'", roleid);
            if (DBOperate.executeCommand(sql))
                vm.refreshRole();
        }

        private void btnNewUser_Click(object sender, RoutedEventArgs e)
        {
            string sql;
            string username = txtUserName.Text;
            string password = txtPassword.Text;
            if (username.Length < 5 || password.Length < 6 || string.IsNullOrWhiteSpace(vm.applicationid))
            {
                txtInfo.Text = "用户名长度不小于5，口令长度不小于6";
                return;
            }
            password = DES.EncryptString(password, DES.theKey);
            //全范围不允许用户重名，这是为了防止不同应用程序之间的用户冒名替代
            sql = string.Format("select username from users where username='{0}'", username);
            DataTable dt = DBOperate.getDataTable(sql);
            if (dt.Rows.Count>0)
            {
                txtInfo.Text = "用户名已存在，请更改用户名";
                return;
            }
            sql = string.Format("insert users (applicationid,username,password) values ('{0}','{1}','{2}')", vm.applicationid, username, password);
            if (DBOperate.executeCommand(sql))
            {
                vm.refreshUser();
                txtInfo.Text =string.Format("用户{0}已添加。",username);
                txtUserName.Text = txtPassword.Text = "";
            }
        }

        private void btnEditUser_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDelUser_Click(object sender, RoutedEventArgs e)
        {
            if (lstUser.SelectedItem == null || string.IsNullOrWhiteSpace(vm.applicationid)) return;
            if (MessageBox.Show("删除后不可恢复，确定要删除吗？", "删除确认", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;

            string userid = (lstUser.SelectedItem as DataRowView).Row.Field<System.Guid>("userid").ToString();
            //先删除角色用户关系
            string sql = string.Format("delete usersinroles where userid='{0}'", userid);
            if (DBOperate.executeCommand(sql))
                vm.refreshRoleUser();
            //再删除用户本身
            sql = string.Format("delete users where userid='{0}'", userid);
            if (DBOperate.executeCommand(sql))
                vm.refreshUser();
        }

        private void btnNewModel_Click(object sender, RoutedEventArgs e)
        {
            string sql;
            string name = txtModelName.Text;
            if ( name.Length < 2 || string.IsNullOrWhiteSpace(vm.applicationid))
            {
                txtInfo.Text = "模块名称长度不小于2";
                return;
            }
            //应用程序范围内，不允许模块名重复
            sql = string.Format("select modelname from models where modelname='{0}' and applicationid='{1}'", name,vm.applicationid);
            DataTable dt = DBOperate.getDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                txtInfo.Text = "应用程序内已存在同名模块！";
                return;
            }
            sql = string.Format("insert models (applicationid,modelname) values ('{0}','{1}')", vm.applicationid, name);
            if (DBOperate.executeCommand(sql))
            {
                vm.refreshModel();
                txtInfo.Text = string.Format("模块{0}已添加。", name);
                txtModelName.Text = "";
            }

        }

        private void btnEditModel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDelModel_Click(object sender, RoutedEventArgs e)
        {
            if (lstModel.SelectedItem == null || string.IsNullOrWhiteSpace(vm.applicationid)) return;
            if (MessageBox.Show("删除后不可恢复，确定要删除吗？", "删除确认", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;

            string modelid = (lstModel.SelectedItem as DataRowView).Row.Field<System.Guid>("modelid").ToString();
            //先删除角色模块关系
            string sql = string.Format("delete modelsinroles where modelid='{0}'", modelid);
            if (DBOperate.executeCommand(sql))
                vm.refreshRoleModel();
            //再删除模块本身
            sql = string.Format("delete models where modelid='{0}'", modelid);
            if (DBOperate.executeCommand(sql))
                vm.refreshModel();
        }

        private void btnNewDBS_Click(object sender, RoutedEventArgs e)
        {
            string sql;
            string group = txtDBSName.Text;
            string name = txtConnName.Text;
            if (group.Length<2 || name.Length < 2 || string.IsNullOrWhiteSpace(vm.applicationid))
            {
                txtInfo.Text = "数据源名称和连接名称长度不小于2";
                return;
            }
            //同一程序中，不允许连接重名
            sql = string.Format("select connname from connections where applicationid='{0}' and connname='{1}'", vm.applicationid, name);
            DataTable dt = DBOperate.getDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                txtInfo.Text = "应用程序中已存在同名连接，请更改!";
                return;
            }

            sql = string.Format("insert connections (applicationid,connname,datasourcename) values ('{0}','{1}','{2}')", vm.applicationid, name,group);
            if (DBOperate.executeCommand(sql))
            {
                vm.refreshConn();
                txtInfo.Text = string.Format("数据源{0}={1}已添加。",group ,name);
                txtDBSName.Text=txtConnName.Text = "";
            }
        }

        private void btnEditDBS_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDelDBS_Click(object sender, RoutedEventArgs e)
        {
            if (lstConn.SelectedItem == null || string.IsNullOrWhiteSpace(vm.applicationid)) return; if (MessageBox.Show("删除后不可恢复，确定要删除吗？", "删除确认", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;

            string connid = (lstConn.SelectedItem as DataRowView).Row.Field<System.Guid>("connid").ToString();
            string info = (lstConn.SelectedItem as DataRowView).Row.Field<string>("datasourcename")+"——>"+(lstConn.SelectedItem as DataRowView).Row.Field<string>("connname");
            string sql = string.Format("delete connections where applicationid='{0}' and connid='{1}'", vm.applicationid, connid);
            if (DBOperate.executeCommand(sql))
            {
                vm.refreshConn();
                txtInfo.Text = string.Format("选定数据源{0}已删除。",info);
            }

        }

        private void btnAddRoleUserRelation_Click(object sender, RoutedEventArgs e)
        {
            if (lstRole.SelectedItem == null || lstUser.SelectedItem==null || string.IsNullOrWhiteSpace(vm.applicationid)) return;
            string roleid = (lstRole.SelectedItem as DataRowView).Row.Field<System.Guid>("roleid").ToString();
            string rolename = (lstRole.SelectedItem as DataRowView).Row.Field<string>("rolename");
            string userid = (lstUser.SelectedItem as DataRowView).Row.Field<System.Guid>("userid").ToString();
            string username = (lstUser.SelectedItem as DataRowView).Row.Field<string>("username");
            string sql = string.Format("insert usersinroles (roleid,userid) values ('{0}','{1}')",roleid,userid);
            if (DBOperate.executeCommand(sql))
            {
                vm.refreshRoleUser();
                txtInfo.Text = string.Format("授权用户{0}拥有{1}角色权限。", username, rolename);
            }
        }

        private void btnDelRoleUserRelation_Click(object sender, RoutedEventArgs e)
        {
            if (lstRoleUser.SelectedItem == null) return;
            if (MessageBox.Show("删除后不可恢复，确定要删除吗？", "删除确认", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;

            string roleid = (lstRoleUser.SelectedItem as DataRowView).Row.Field<System.Guid>("roleid").ToString();
            string userid = (lstRoleUser.SelectedItem as DataRowView).Row.Field<System.Guid>("userid").ToString();
            string rolename = (lstRoleUser.SelectedItem as DataRowView).Row.Field<string>("rolename");
            string username = (lstRoleUser.SelectedItem as DataRowView).Row.Field<string>("username");
            string sql = string.Format("delete usersinroles where roleid='{0}' and userid='{1}'", roleid, userid);
            if (DBOperate.executeCommand(sql))
            {
                vm.refreshRoleUser();
                txtInfo.Text = string.Format("已取消用户{0}的{1}角色授权。", username, rolename);
            }

        }

        private void btnAddRoleModelRelation_Click(object sender, RoutedEventArgs e)
        {
            if (lstRole.SelectedItem == null || lstModel.SelectedItem == null || string.IsNullOrWhiteSpace(vm.applicationid)) return;
            string roleid = (lstRole.SelectedItem as DataRowView).Row.Field<System.Guid>("roleid").ToString();
            string rolename = (lstRole.SelectedItem as DataRowView).Row.Field<string>("rolename");
            string modelid = (lstModel.SelectedItem as DataRowView).Row.Field<System.Guid>("modelid").ToString();
            string modelname = (lstModel.SelectedItem as DataRowView).Row.Field<string>("modelname");
            string sql = string.Format("insert modelsinroles (roleid,modelid) values ('{0}','{1}')", roleid, modelid);
            if (DBOperate.executeCommand(sql))
            {
                vm.refreshRoleModel();
                txtInfo.Text = string.Format("授权角色{0}拥有{1}模块权限。", rolename, modelname);
            }
        }

        private void btnDelRoleModelRelation_Click(object sender, RoutedEventArgs e)
        {
            if (lstRoleModel.SelectedItem == null) return;
            if (MessageBox.Show("删除后不可恢复，确定要删除吗？", "删除确认", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;

            string roleid = (lstRoleModel.SelectedItem as DataRowView).Row.Field<System.Guid>("roleid").ToString();
            string modelid = (lstRoleModel.SelectedItem as DataRowView).Row.Field<System.Guid>("modelid").ToString();
            string rolename = (lstRoleModel.SelectedItem as DataRowView).Row.Field<string>("rolename");
            string modelname = (lstRoleModel.SelectedItem as DataRowView).Row.Field<string>("modelname");
            string sql = string.Format("delete modelsinroles where roleid='{0}' and modelid='{1}'", roleid, modelid);
            if (DBOperate.executeCommand(sql))
            {
                vm.refreshRoleModel();
                txtInfo.Text = string.Format("已取消角色{0}对{1}的使用授权。", rolename, modelname);
            }


        }

        private void btnAddModelConnRelation_Click(object sender, RoutedEventArgs e)
        {
            if (lstConn.SelectedItem == null || lstModel.SelectedItem == null || string.IsNullOrWhiteSpace(vm.applicationid)) return;
            string connid = (lstConn.SelectedItem as DataRowView).Row.Field<System.Guid>("connid").ToString();
            string connname = (lstConn.SelectedItem as DataRowView).Row.Field<string>("datasourcename");
            string modelid = (lstModel.SelectedItem as DataRowView).Row.Field<System.Guid>("modelid").ToString();
            string modelname = (lstModel.SelectedItem as DataRowView).Row.Field<string>("modelname");
            string sql = string.Format("insert connectionsinmodels (connid,modelid) values ('{0}','{1}')", connid, modelid);
            if (DBOperate.executeCommand(sql))
            {
                vm.refreshModelConn();
                txtInfo.Text = string.Format("授权模块{0}拥有使用{1}权限。", modelname,connname);
            }
        }

        private void btnDelModelConnRelation_Click(object sender, RoutedEventArgs e)
        {
            if (lstModelConn.SelectedItem == null) return;
            if (MessageBox.Show("删除后不可恢复，确定要删除吗？", "删除确认", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;

            string connid = (lstModelConn.SelectedItem as DataRowView).Row.Field<System.Guid>("connid").ToString();
            string modelid = (lstModelConn.SelectedItem as DataRowView).Row.Field<System.Guid>("modelid").ToString();
            string connname = (lstModelConn.SelectedItem as DataRowView).Row.Field<string>("connname");
            string modelname = (lstModelConn.SelectedItem as DataRowView).Row.Field<string>("modelname");
            string sql = string.Format("delete connectionsinmodels where connid='{0}' and modelid='{1}'", connid, modelid);
            if (DBOperate.executeCommand(sql))
            {
                vm.refreshModelConn();
                txtInfo.Text = string.Format("已取消模块{0}对{1}的使用权限。", modelname, connname);
            }
        }

        private void rdor_Checked(object sender, RoutedEventArgs e)
        {
            if (lstModelConn.SelectedItem == null) return;
            int saveselectindex = lstModelConn.SelectedIndex;
            string connid = (lstModelConn.SelectedItem as DataRowView).Row.Field<System.Guid>("connid").ToString();
            string modelid = (lstModelConn.SelectedItem as DataRowView).Row.Field<System.Guid>("modelid").ToString();
            string sql = string.Format("update connectionsinmodels set isreadonly=1 where connid='{0}' and modelid='{1}'", connid, modelid);
            if (DBOperate.executeCommand(sql))
                vm.refreshModelConn();
            lstModelConn.SelectedIndex=saveselectindex;

        }

        private void rdow_Checked(object sender, RoutedEventArgs e)
        {
            if (lstModelConn.SelectedItem == null) return;
            int saveselectindex = lstModelConn.SelectedIndex;
            string connid = (lstModelConn.SelectedItem as DataRowView).Row.Field<System.Guid>("connid").ToString();
            string modelid = (lstModelConn.SelectedItem as DataRowView).Row.Field<System.Guid>("modelid").ToString();
            string sql = string.Format("update connectionsinmodels set isreadonly=0 where connid='{0}' and modelid='{1}'", connid, modelid);
            if (DBOperate.executeCommand(sql))
                vm.refreshModelConn();
            lstModelConn.SelectedIndex = saveselectindex;

        }

        private void lstModelConn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstModelConn.SelectedItem == null) return;

            bool isreadonly = (lstModelConn.SelectedItem as DataRowView).Row.Field<bool>("isreadonly");
            rdor.IsChecked = isreadonly;
            rdow.IsChecked = !isreadonly;
        }
    }
}
