using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Security.Permissions;
using System.Security.Principal;
using System.IdentityModel.Policy;
using System.IdentityModel.Claims;


namespace WcfDataService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IService1”。
    [ServiceContract(SessionMode=SessionMode.Required)]
    public interface IDataService
    {
        ///<summary>测试</summary>
        [OperationContract]
        string test(); //不能删除，用于调用以验证用户


        #region ====== 数据库操作类 ======
        [OperationContract]
        DataTable GetDataTable(string sql, string datasourcename);

        [OperationContract]
        bool ExecuteCommand(string sql, string datasourcename);

        #endregion




        #region ===== 管理类 =====
        [OperationContract]
        UserType ReadSelfInfomation();

        [OperationContract]
        bool UpdateSelfInfomation(string Alias, string Password, string Email);

        [OperationContract]
        DataTable GetManageData(string sql);

        [OperationContract]
        string GetApplicationID();

        [OperationContract]
        bool UpdateUserPassword(string userid,string password);

        [OperationContract]
        bool AddNewUser(string username, string password);

        [OperationContract]
        bool ExecuteManageCommand(string sql);

        [OperationContract]
        bool CheckModelPermissionn(string ModelName);  //检查指定模块名的运行许可

        [OperationContract]
        LogInfoType ReadLogInfomation();  //获得日志统计信息

        [OperationContract]
        DataTable ReadLogs(DateTime start,DateTime end, string logtype);  //获得日志统计信息

        #endregion

    }

    ///<summary>用户信息数据协定</summary>
    [DataContract]
    public class UserType
    {
        [DataMember]
        public string username { get; set; }

        [DataMember]
        public string alias { get; set; }

        [DataMember]
        public string password { get; set; }

        [DataMember]
        public bool isLock { get; set; }

        [DataMember]
        public string email { get; set; }

        [DataMember]
        public string createDate { get; set; }
        [DataMember]
        public string lastLoginDate { get; set; }
        [DataMember]
        public string lastLockDate { get; set; }
        [DataMember]
        public string lastActivityDate { get; set; }

        [DataMember]
        public List<string> roles { get; set; }

    }

    ///<summary>日志信息数据协定</summary>
    public class LogInfoType
    {
        public LogInfoType()
        {
            types = new List<string>();
            configs = new List<KeyValuePair<string, string>>();
        }

        [DataMember]
        public string startDate { get; set; }
        [DataMember]
        public string endDate { get; set; }
        [DataMember]
        public int count { get; set; }

        [DataMember]
        public List<string> types { get; set; }

        [DataMember]
        public List<KeyValuePair<string,string>> configs { get; set; }

    }
  

}
