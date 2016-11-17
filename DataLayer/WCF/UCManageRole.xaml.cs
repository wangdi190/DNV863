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

namespace DataLayer.WCF
{
    /// <summary>
    /// UCManageRole.xaml 的交互逻辑
    /// </summary>
    public partial class UCManageRole : UserControl
    {
        public UCManageRole()
        {
            InitializeComponent();
        }

        System.Collections.ObjectModel.ObservableCollection<RoleData> roles = new System.Collections.ObjectModel.ObservableCollection<RoleData>();
        System.Collections.ObjectModel.ObservableCollection<ModelData> models = new System.Collections.ObjectModel.ObservableCollection<ModelData>();
        string appid;

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            appid = DataProvider.wcf.GetApplicationID();

            refreshRoles();

            string sql;
            DataTable dt;
            sql = string.Format("select ModelID,ModelName from models where applicationid='{0}'", appid);
            dt = DataProvider.wcf.GetManageData(sql);
            foreach (DataRow dr in dt.Rows)
                models.Add(new ModelData() { modelID = dr[0].ToString(), modelName = dr[1].ToString() });

        }
        void refreshRoles()
        {
            string sql;
            DataTable dt;
            roles.Clear();
            sql = string.Format("select RoleID,RoleName from roles where ApplicationID='{0}'", appid);
            dt = DataProvider.wcf.GetManageData(sql);
            foreach (DataRow dr in dt.Rows)
            {
                RoleData rd = new RoleData() { roleID = dr[0].ToString(), roleName = dr[1].ToString()};
                rd.brushAdmin = rd.roleName=="管理员" ? (Brush)FindResource("ibAdmin") : Brushes.Transparent;
                roles.Add(rd);
            }
            lstRole.ItemsSource = roles;
        }

        private void lstRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstRole.SelectedItem == null)
            {
                lstModel.ItemsSource = null;
                btnDel.IsEnabled = btnUpdateModel.IsEnabled = false;
                return;
            }

            btnDel.IsEnabled = (lstRole.SelectedItem as RoleData).roleName != "管理员";
            btnUpdateModel.IsEnabled = true;
            string sql;
            DataTable dt;
            string rid = (lstRole.SelectedItem as RoleData).roleID;

            foreach (var item in models)
                item.isInclude = false;
            sql = string.Format("select * from modelsinroles where roleid='{0}'", rid);
            dt = DataProvider.wcf.GetManageData(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string tmpmodelid = dr["ModelID"].ToString();
                models.First(p => p.modelID == tmpmodelid).isInclude = true;
            }
            lstModel.ItemsSource = models;
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            WinNewRole win = new WinNewRole();
            if ((bool)win.ShowDialog())
            {
                string newrolename = win.txtRoleName.Text;
                refreshRoles();
                RoleData newrole = roles.First(p => p.roleName == newrolename);
                lstRole.SelectedItem = newrole;
            }

        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("删除后不可恢复，确定要删除吗？", "删除确认", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            string rid = (lstRole.SelectedItem as RoleData).roleID;
            RoleData currole = lstRole.SelectedItem as RoleData;
            lstRole.SelectedItem = null;

            string sql = string.Format("delete modelsinroles where roleid='{0}'", rid); //删除此角色的模块关联
            DataProvider.wcf.ExecuteManageCommand(sql);
            sql = string.Format("delete usersinroles where roleid='{0}'", rid); //删除此角色的用户关联
            DataProvider.wcf.ExecuteManageCommand(sql);

            sql = string.Format("delete roles where roleid='{0}'", rid);  //删除角色本身
            DataProvider.wcf.ExecuteManageCommand(sql);

            roles.Remove(currole);
            info.Text = string.Format("角色{0}已删除!", currole.roleName);
        }

      
        private void btnUpdateModel_Click(object sender, RoutedEventArgs e)
        {
            string rid = (lstRole.SelectedItem as RoleData).roleID;
            string sql = string.Format("delete modelsinroles where roleid='{0}'", rid);
            DataProvider.wcf.ExecuteManageCommand(sql);
            foreach (var item in models.Where(p => p.isInclude))
            {
                sql = string.Format("insert modelsinroles (modelid,roleid) values ('{0}','{1}')", item.modelID, rid);
                DataProvider.wcf.ExecuteManageCommand(sql);
            }

            info.Text = "角色的模块权限信息已更新!";
        }

      

    }


}
