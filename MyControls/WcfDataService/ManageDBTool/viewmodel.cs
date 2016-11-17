using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;

namespace ManageDBTool
{
    class viewmodel:NotificationObject
    {

        internal string applicationid;

        #region ===== 刷新表 =====
        internal void refreshApplication()
        {
            dtApplication = DBOperate.getDataTable(string.Format("select * from applications"));
            RaisePropertyChanged(() => dtApplication);
        }
        internal void refreshRole()
        {
            dtRole = DBOperate.getDataTable(string.Format("select * from roles where applicationid='{0}' order by rolename", applicationid));
            RaisePropertyChanged(() => dtRole);
        }
        internal void refreshUser()
        {
            dtUser = DBOperate.getDataTable(string.Format("select * from users where applicationid='{0}' order by username", applicationid));
            RaisePropertyChanged(() => dtUser);
        }
        internal void refreshModel()
        {
            dtModel = DBOperate.getDataTable(string.Format("select * from models where applicationid='{0}' order by modelname", applicationid));
            RaisePropertyChanged(() => dtModel);
        }
        internal void refreshConn()
        {
            dtConn = DBOperate.getDataTable(string.Format("select * from connections where applicationid='{0}' order by connname", applicationid));
            RaisePropertyChanged(() => dtConn);
        }
        internal void refreshRoleUser()
        {
            dtRoleUser = DBOperate.getDataTable(string.Format("select t1.*,t2.RoleName,t3.UserName from UsersInRoles t1, Roles t2, Users t3 where t1.RoleID=t2.RoleID and t1.UserID=t3.UserID and t2.ApplicationID='{0}' order by t3.username", applicationid));
            RaisePropertyChanged(() => dtRoleUser);
        }
        internal void refreshRoleModel()
        {
            dtRoleModel = DBOperate.getDataTable(string.Format("select t1.*,t2.RoleName,t3.ModelName from ModelsInRoles t1, Roles t2, Models t3 where t1.RoleID=t2.RoleID and t1.ModelID=t3.ModelID and t2.ApplicationID='{0}' order by t2.rolename", applicationid));
            RaisePropertyChanged(() => dtRoleModel);
        }
        internal void refreshModelConn()
        {
            dtModelConn = DBOperate.getDataTable(string.Format("select t1.*, case when t1.isreadonly=0 then '读写' else '只读' end as flag ,t2.ConnName,t2.DatasourceName,t3.ModelName from ConnectionsInModels t1, Connections t2, Models t3 where t1.ConnID=t2.ConnID and t1.ModelID=t3.ModelID and t2.ApplicationID='{0}' order by t3.ModelName", applicationid));
            RaisePropertyChanged(() => dtModelConn);
        }

        #endregion

        private DataRowView _ApplicationSel;
        public DataRowView ApplicationSel
        {
            get { return _ApplicationSel; }
            set {
                _ApplicationSel = value;
                applicationid=value.Row.Field<System.Guid>("applicationid").ToString();
                refreshRole();
                refreshUser();
                refreshModel();
                refreshConn();
                refreshRoleUser();
                refreshRoleModel();
                refreshModelConn();
            }
        }

        public DataTable dtApplication { get; set; }
        public DataTable dtRole { get; set; }
        public DataTable dtUser { get; set; }
        public DataTable dtModel { get; set; }
        public DataTable dtConn { get; set; }
        public DataTable dtRoleUser { get; set; }
        public DataTable dtRoleModel { get; set; }
        public DataTable dtModelConn { get; set; }
    }

    public class NotificationObject : INotifyPropertyChanged
    {
        protected void RaisePropertyChanged<T>(Expression<Func<T>> action)
        {
            var propertyName = GetPropertyName(action);
            RaisePropertyChanged(propertyName);
        }

        private static string GetPropertyName<T>(Expression<Func<T>> action)
        {
            var expression = (MemberExpression)action.Body;
            var propertyName = expression.Member.Name;
            return propertyName;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
