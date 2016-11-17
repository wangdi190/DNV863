using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IdentityModel;
using System.IdentityModel.Selectors;
using System.Configuration;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.ServiceModel.Activation;

namespace WcfDataService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“Service1”。
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class DataService : IDataService
    {


        //***** 保存在内存中的静态管理数据字典，以避免频繁查询数据库
        internal static Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();
        //***** 保存在内存中的应用程序管理项，以避免频繁查询数据库和共享管理数据
        internal static Dictionary<string, AppData> apps = new Dictionary<string, AppData>();
        //***** 是否启用全局安全日志
        internal static bool? isGlobalSafeLog;

        public DataService()
        {
            DataTable dt;
            string sql;
            //通过全局唯一用户名传递

            #region ----- 初始化可用数据源 -----
            if (connManage == null)
            {
                foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
                {
                    string[] ss = item.Name.Split('_');
                    if (ss[0] == "管理数据源")
                    {
                        EDataBaseType dbtype = dbhelper.getDBType(ss[1]);
                        connManage = new ConnInfo(ss[0], item.ConnectionString, dbtype, ss[2]);
                    }
                }
            }


            applicationid = DataService.users[OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name].appID;
            if (conns == null)
            {
                conns = new List<ConnInfo>();
                sql = string.Format("select connname,datasourcename,databasetype,connectionstring from connections where applicationid='{0}'", applicationid);
                dt = connManage.getDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    string connstring = dr["connectionstring"].ToString();
                    string datasourcename = dr["datasourcename"].ToString();
                    EDataBaseType dbtype = (EDataBaseType)int.Parse(dr["databasetype"].ToString());
                    string connname = dr["connname"].ToString();
                    if (!string.IsNullOrWhiteSpace(connstring) && !string.IsNullOrWhiteSpace(datasourcename))
                        conns.Add(new ConnInfo(datasourcename, connstring, dbtype, connname));
                }
            }
            #endregion



        }
        List<ConnInfo> conns;
        ConnInfo connManage;
        string applicationid;


        #region ====== 数据操作类 ======
        public DataTable GetDataTable(string sql, string datasourcename)
        {
            UserInfo userinfo = DataService.users[OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name];
            if (!userinfo.isManager && !userinfo.conns.ContainsKey(datasourcename)) //也可用声明来授权
            {
                if (apps[applicationid].isLog && apps[applicationid].isLogSafe)  //日志
                {
                    string logcontent = string.Format("用户{0}试图非法读取{1}，也可能是模块未正确设置数据源读权限或无该数据源。", userinfo.userName, datasourcename);
                    string logsql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", applicationid, DateTime.Now, "安全", logcontent);
                    connManage.executeCommand(logsql);
                }

                throw new FaultException(string.Format("无{0}或无读取权限!", datasourcename));
            }

            foreach (ConnInfo conn in conns.Where(p => p.datasourceName == datasourcename))
            {
                try
                {
                    DataTable dt = conn.getDataTable(sql);
                    if (dt != null)
                    {
                        if (apps[applicationid].isLog && apps[applicationid].isLogRead)  //日志
                        {
                            string logcontent = string.Format("用户{0}读取了{1}。", userinfo.userName, datasourcename);
                            string logsql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", applicationid, DateTime.Now, "读取", logcontent);
                            connManage.executeCommand(logsql);
                        }
                        return dt;
                    }
                }
                catch (Exception e)
                {
                    if (apps[applicationid].isLog && apps[applicationid].isLogError)  //日志
                    {
                        string logcontent = e.Message;
                        string logsql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", applicationid, DateTime.Now, "错误", logcontent);
                        connManage.executeCommand(logsql);
                    }
                    throw;
                }
            }


            return null;
        }

        public bool ExecuteCommand(string sql, string datasourcename)
        {
            UserInfo userinfo = DataService.users[OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name];
            if (!userinfo.isManager && (!userinfo.conns.ContainsKey(datasourcename) || userinfo.conns[datasourcename]))
            {
                if (apps[applicationid].isLog && apps[applicationid].isLogSafe)  //日志
                {
                    string logcontent = string.Format("用户{0}试图非法执行{1}的命令，也可能是模块未正确设置数据源写权限或无该数据源。", userinfo.userName, datasourcename);
                    string logsql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", applicationid, DateTime.Now, "安全", logcontent);
                    connManage.executeCommand(logsql);
                }

                throw new FaultException(string.Format("无{0}或无执行权限!", datasourcename));
            }

            foreach (ConnInfo conn in conns.Where(p => p.datasourceName == datasourcename))
            {
                try
                {
                    bool isSuccess = conn.executeCommand(sql);
                    if (isSuccess)
                    {
                        if (apps[applicationid].isLog && apps[applicationid].isLogExecute)  //日志
                        {
                            string logcontent = string.Format("用户{0}对{1}执行了命令({2})。", userinfo.userName, datasourcename, sql.Replace("'", "''"));
                            string logsql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", applicationid, DateTime.Now, "执行", logcontent);
                            connManage.executeCommand(logsql);
                        }
                        return true;
                    }
                }
                catch (Exception e)
                {
                    if (apps[applicationid].isLog && apps[applicationid].isLogError)  //日志
                    {
                        string logcontent = e.Message;
                        string logsql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", applicationid, DateTime.Now, "错误", logcontent);
                        connManage.executeCommand(logsql);
                    }
                    throw;
                }
            }


            return false;
        }
        #endregion


        #region ====== 管理类 ======
        public UserType ReadSelfInfomation()
        {
            string userid = DataService.users[OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name].userID;

            string sql = string.Format("select username,alias,password,email,islocked,createdate,lastlogindate,lastlockdate,lastactivitydate from users where userid='{0}'", userid);
            DataTable dt = connManage.getDataTable(sql);
            DataRow dr = dt.Rows[0];
            UserType info = new UserType()
            {
                alias = dr["alias"].ToString(),
                createDate = dr["createdate"].ToString(),
                email = dr["email"].ToString(),
                isLock = (bool)dr["islocked"],
                lastActivityDate = dr["lastactivitydate"].ToString(),
                lastLockDate = dr["lastlockdate"].ToString(),
                lastLoginDate = dr["lastlogindate"].ToString(),
                password = DES.DecryptString(dr["password"].ToString(), DES.theKey),
                username = dr["username"].ToString()
            };
            sql = string.Format("select t1.rolename from roles t1,usersinroles t2 where t1.roleid=t2.roleid and t2.userid='{0}'", userid);
            dt = connManage.getDataTable(sql);
            info.roles = new List<string>();
            foreach (DataRow ddr in dt.Rows)
                info.roles.Add(ddr[0].ToString());
            return info;
        }

        public bool UpdateSelfInfomation(string Alias, string Password, string Email)
        {
            string userid = DataService.users[OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name].userID;
            string sql = string.Format("update users set alias='{1}',password='{2}',email='{3}' where userid='{0}'", userid, Alias, DES.EncryptString(Password, DES.theKey), Email);
            if (apps[applicationid].isLog && apps[applicationid].isLogManage)  //日志
            {
                string logcontent = string.Format("用户{0}更新了自已的信息。", OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name);
                string logsql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", applicationid, DateTime.Now, "管理", logcontent);
                connManage.executeCommand(logsql);
            }
            return connManage.executeCommand(sql);
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "管理员")]
        public DataTable GetManageData(string sql)
        {
            return connManage.getDataTable(sql);
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "管理员")]
        public bool ExecuteManageCommand(string sql)
        {
            if (apps[applicationid].isLog && apps[applicationid].isLogManage)  //日志
            {
                string logcontent = string.Format("管理员{0}执行了管理命令({1})。", OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name, sql.Replace("'", "''"));
                string logsql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", applicationid, DateTime.Now, "管理", logcontent);
                connManage.executeCommand(logsql);
            }

            return connManage.executeCommand(sql);
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "管理员")]
        public string GetApplicationID()
        {
            return DataService.users[OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name].appID;
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "管理员")]
        public bool UpdateUserPassword(string userid, string password)
        {
            if (apps[applicationid].isLog && apps[applicationid].isLogManage)  //日志
            {
                string logcontent = string.Format("管理员{0}更新用户ID为{1}的口令。", OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name, userid);
                string logsql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", applicationid, DateTime.Now, "管理", logcontent);
                connManage.executeCommand(logsql);
            }

            string sql = string.Format("update users set password='{1}' where userid='{0}'", userid, DES.EncryptString(password, DES.theKey));
            return connManage.executeCommand(sql);
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "管理员")]
        public bool AddNewUser(string username, string password)
        {
            if (apps[applicationid].isLog && apps[applicationid].isLogManage)  //日志
            {
                string logcontent = string.Format("管理员{0}添加了名为{1}的用户。", OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name, username);
                string logsql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", applicationid, DateTime.Now, "管理", logcontent);
                connManage.executeCommand(logsql);
            }
            string appid = DataService.users[OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name].appID;
            string sql = string.Format("insert users (applicationid, username,password) values ('{0}','{1}','{2}')", appid, username, DES.EncryptString(password, DES.theKey));
            return connManage.executeCommand(sql);
        }

        ///<summary>检查并返回指定模块名的模块是否允许运行</summary>
        public bool CheckModelPermissionn(string ModelName)
        {
            UserInfo userinfo = DataService.users[OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name];
            if (userinfo.isManager) return true;
            return userinfo.models.Contains(ModelName);
        }
        #endregion

        #region ----- 日志类 -----
        [PrincipalPermission(SecurityAction.Demand, Role = "管理员")]
        public LogInfoType ReadLogInfomation()  //获得日志统计信息
        {
            LogInfoType loginfo = new LogInfoType();
            string sql = string.Format("select count(*) count,max(logdate) maxdate,min(logdate) mindate from log where applicationid='{0}'", applicationid);
            DataTable dt = connManage.getDataTable(sql);
            loginfo.count = int.Parse(dt.Rows[0][0].ToString());
            loginfo.startDate = dt.Rows[0][2].ToString();
            loginfo.endDate = dt.Rows[0][1].ToString();
            //日志类型
            sql = "select logtype from log group by logtype";
            dt = connManage.getDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                loginfo.types.Add(dr[0].ToString());
            }
            //日志配置
            sql = string.Format("select configkey,configvalue from config where (applicationid='{0}' or configkey='全局安全日志是否启用') and configsort='日志' order by configorder", applicationid);
            dt = connManage.getDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                loginfo.configs.Add(new KeyValuePair<string, string>(dr[0].ToString(), dr[1].ToString()));
            }

            return loginfo;
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "管理员")]
        public DataTable ReadLogs(DateTime start, DateTime end, string logtype)  //获得日志统计信息
        {
            string sql;
            if (!string.IsNullOrWhiteSpace(logtype))
                if (logtype == "全局安全")
                    sql = string.Format("select top 500 logdate,logtype,logcontent from log where logtype='全局安全' and logdate>='{1}' and logdate<='{2}' order by logdate desc", applicationid, start, end);
                else
                    sql = string.Format("select top 500 logdate,logtype,logcontent from log where applicationid='{0}' and logdate>='{1}' and logdate<='{2}' and logtype='{3}' order by logdate desc", applicationid, start, end, logtype);
            else
                sql = string.Format("select top 500 logdate,logtype,logcontent from log where (applicationid='{0}' or logtype='全局安全') and logdate>='{1}' and logdate<='{2}' order by logdate desc", applicationid, start, end);
            DataTable dt = connManage.getDataTable(sql);

            return dt;

        }

        #endregion

        ///<summary>测试</summary>
        public string test()
        {
            return "OK";
        }

    }

    ///<summary>自定义认证类</summary>
    public class CustomUserNameValidator : UserNamePasswordValidator
    {
        ConnInfo _conn;
        ConnInfo conn
        {
            get
            {
                if (_conn == null)
                {
                    foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
                    {
                        string[] ss = item.Name.Split('_');
                        if (ss[0] == "管理数据源")
                        {
                            EDataBaseType dbtype = dbhelper.getDBType(ss[1]);
                            _conn = new ConnInfo(ss[0], item.ConnectionString, dbtype, ss[2]);
                        }
                    }
                }
                return _conn;
            }
        }


        public override void Validate(string userName, string password)
        {
            if (null == userName || null == password || null == conn)
            {
                throw new ArgumentNullException();
            }

            if (DataService.isGlobalSafeLog == null)  //初始化全局安全性配置
            {
                string tmpsql = string.Format("select configvalue from config where configkey='全局安全日志是否启用'");
                DataTable tmpdt = conn.getDataTable(tmpsql);
                if (tmpdt.Rows.Count > 0 && tmpdt.Rows[0][0].ToString() == "true")
                    DataService.isGlobalSafeLog = true;
                else
                    DataService.isGlobalSafeLog = false;
            }

            string sql = string.Format("select islocked,failedpasswordattemptcount,userid from users t3 where username='{0}'", userName);
            DataTable dt = conn.getDataTable(sql);
            int attemptcount;
            string uid;
            if (dt == null)
            {
                throw new FaultException("服务器端验证故障!");
            }
            else if (dt.Rows.Count == 0)
            {
                if ((bool)DataService.isGlobalSafeLog)  //日志全局安全性
                {
                    string ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                    //System.ServiceModel.Channels.MessageProperties properties = OperationContext.Current.IncomingMessageProperties;
                    //RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                    //string logcontent = string.Format("有客户端从IP:{0}试图以不存在的用户名({1})登录系统。", endpoint.Address, userName);
                    string logcontent = string.Format("有客户端从IP:{0}试图以不存在的用户名({1})登录系统。", ip, userName);
                    logcontent = logcontent.Length > 255 ? logcontent.Substring(0, 255) : logcontent;
                    string tmpsql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", Guid.NewGuid(), DateTime.Now, "全局安全", logcontent);
                    conn.executeCommand(tmpsql);
                }
                throw new FaultException("用户名不存在!");
            }
            else
            {
                bool islocked = (bool)dt.Rows[0][0];
                attemptcount = int.Parse(dt.Rows[0][1].ToString());
                uid = dt.Rows[0][2].ToString();
                if (islocked)
                {
                    if ((bool)DataService.isGlobalSafeLog)  //日志全局安全性
                    {
                        string ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                        //System.ServiceModel.Channels.MessageProperties properties = OperationContext.Current.IncomingMessageProperties;
                        //RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                        //string logcontent = string.Format("有客户端从IP:{0}试图以被加锁的用户名({1})登录系统。", endpoint.Address, userName);
                        string logcontent = string.Format("有客户端从IP:{0}试图以被加锁的用户名({1})登录系统。", ip, userName);
                        logcontent = logcontent.Length > 255 ? logcontent.Substring(0, 255) : logcontent;
                        string tmpsql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", Guid.NewGuid(), DateTime.Now, "全局安全", logcontent);
                        conn.executeCommand(tmpsql);
                    }

                    throw new FaultException("该账号已被加锁，请联系系统管理员解除!");
                }
            }


            sql = string.Format("select applicationid,userid from users t3 where username='{0}' and password='{1}' and islocked=0", userName, DES.EncryptString(password, DES.theKey));
            dt = conn.getDataTable(sql);
            if (dt.Rows.Count == 0)  // 错误
            {
                if ((bool)DataService.isGlobalSafeLog)  //日志全局安全性
                {
                    string ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                    //System.ServiceModel.Channels.MessageProperties properties = OperationContext.Current.IncomingMessageProperties;
                    //RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                    //string logcontent = string.Format("有客户端从IP:{0}试图以用户名({1})和错误口令登录系统。", endpoint.Address, userName);
                    string logcontent = string.Format("有客户端从IP:{0}试图以用户名({1})和错误口令登录系统。", ip, userName);
                    logcontent = logcontent.Length > 255 ? logcontent.Substring(0, 255) : logcontent;
                    string tmpsql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", Guid.NewGuid(), DateTime.Now, "全局安全", logcontent);
                    conn.executeCommand(tmpsql);
                }
                if (attemptcount + 1 > 8) //尝试次数大于8次，加锁
                {
                    sql = string.Format("update users set FailedPasswordAttemptCount=0,islocked=1,lastlockdate='{1}' where userid='{0}'", uid, DateTime.Now);
                    conn.executeCommand(sql);
                    throw new FaultException("该账号尝试登录超过8次，已被加锁，请联系系统管理员解除!");
                }
                else
                {
                    sql = string.Format("update users set FailedPasswordAttemptCount={0} where userid='{1}'", attemptcount + 1, uid);
                    conn.executeCommand(sql);
                    throw new FaultException(string.Format("口令错误，该账号尝试登录{0}次，超过8次将被加锁无法使用!", attemptcount + 1));
                }
            }
            else
            {
                sql = string.Format("update users set FailedPasswordAttemptCount=0, lastlogindate='{0}' where userid='{1}'", DateTime.Now, uid);
                conn.executeCommand(sql);

                #region ----- 认证后，处理保存静态变量中的用户数据 -----
                string appid = dt.Rows[0][0].ToString();
                UserInfo userinfo;
                if (!DataService.users.TryGetValue(userName, out userinfo))  //以用户名为键
                {
                    userinfo = new UserInfo() { userID = uid, appID = appid, userName = userName };
                    DataService.users.Add(userName, userinfo);
                    //角色信息
                    sql = @"select t2.RoleName  from Users t1,Roles t2,UsersInRoles r where t1.UserName='{0}' and t1.UserID=r.UserID and t2.RoleID=r.RoleID";
                    sql = string.Format(sql, userName);
                    dt = conn.getDataTable(sql);
                    foreach (DataRow item in dt.Rows)
                        userinfo.roles.Add(item[0].ToString());
                    //模块信息
                    sql = @"select t2.ModelName  from Roles t1,Models t2,ModelsInRoles r where t1.RoleID in
                             (select t2.RoleID  from Users t1,Roles t2,UsersInRoles r where t1.UserName='{0}' and t1.UserID=r.UserID and t2.RoleID=r.RoleID) 
                                and t1.RoleID=r.RoleID and t2.ModelID=r.ModelID";
                    sql = string.Format(sql, userName);
                    dt = conn.getDataTable(sql);
                    foreach (DataRow item in dt.Rows)
                        userinfo.models.Add(item[0].ToString());
                    //数据源信息
                    sql = @"select t2.DatasourceName,r.isReadOnly from Models t1,Connections t2,ConnectionsInModels r where t1.ModelID in
                                (select t2.ModelID  from Roles t1,Models t2,ModelsInRoles r where t1.RoleID in
	                                (select t2.RoleID  from Users t1,Roles t2,UsersInRoles r where t1.UserName='{0}' and t1.UserID=r.UserID and t2.RoleID=r.RoleID) 
		                        and t1.RoleID=r.RoleID and t2.ModelID=r.ModelID) 
                           and t1.ModelID=r.ModelID and t2.ConnID=r.ConnID";
                    sql = string.Format(sql, userName);
                    dt = conn.getDataTable(sql);
                    foreach (DataRow item in dt.Rows)
                    {
                        string dsname = item[0].ToString();
                        bool isreadonly = (bool)item[1];
                        if (!userinfo.conns.ContainsKey(dsname))
                            userinfo.conns.Add(dsname, isreadonly);
                        else
                        {
                            if (userinfo.conns[dsname] && !isreadonly) //多个角色权限取最大权限
                            {
                                userinfo.conns[dsname] = isreadonly;
                            }
                        }
                    }

                }
                #endregion
                #region ----- 应用程序的管理 -----
                AppData appdata;
                if (!DataService.apps.TryGetValue(appid, out appdata)) //若静态变量中无此应用程序项，则添加
                {
                    appdata = new AppData();
                    DataService.apps.Add(appid, appdata);
                    sql = string.Format("select configkey,configvalue from config where applicationid='{0}' and configsort='日志'", appid);
                    dt = conn.getDataTable(sql);
                    foreach (DataRow dr in dt.Rows)  //填充配置项，若无或无效，则使用缺省值
                    {
                        if (dr[0].ToString() == "是否启用日志" && dr[1].ToString() == "true")
                            appdata.isLog = true;
                        if (dr[0].ToString() == "日志有效期限")
                            int.TryParse(dr[1].ToString(), out appdata.logValidDays);
                        if (dr[0].ToString() == "日志是否记录读操作" && dr[1].ToString() == "true")
                            appdata.isLogRead = true;
                        if (dr[0].ToString() == "日志是否记录执行操作" && dr[1].ToString() == "true")
                            appdata.isLogExecute = true;
                        if (dr[0].ToString() == "日志是否记录管理操作" && dr[1].ToString() == "true")
                            appdata.isLogManage = true;
                        if (dr[0].ToString() == "日志是否记录安全信息" && dr[1].ToString() == "true")
                            appdata.isLogSafe = true;
                        if (dr[0].ToString() == "日志是否记录错误信息" && dr[1].ToString() == "true")
                            appdata.isLogError = true;
                    }
                }
                //若启用了日志，且程序有效期内未清除过或清除时间超过24小时, 则清除过期日志
                if (appdata.isLog && (appdata.lastClearLogDate == null || (DateTime.Now - (DateTime)appdata.lastClearLogDate).TotalHours > 24))
                {
                    sql = string.Format("delete log where (applicationid='{0}' or logtype='全局安全') and logdate<'{1}'", appid, DateTime.Now.AddDays(-appdata.logValidDays));
                    conn.executeCommand(sql);
                    appdata.lastClearLogDate = DateTime.Now;

                    //sql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", appid, DateTime.Now, "系统", "系统执行了一次过期日志清理。");
                    //conn.executeCommand(sql);
                }

                #endregion

                if (appdata.isLog && appdata.isLogSafe)
                {
                    string ip = System.Web.HttpContext.Current.Request.UserHostAddress;
                    sql = string.Format("insert log (applicationid,logdate,logtype,logcontent) values ('{0}','{1}','{2}','{3}')", appid, DateTime.Now, "安全", "用户" + userName + "从"+ip+"登录了系统。");
                    conn.executeCommand(sql);
                }
            }

        }
    }



    ///<summary>自定义授权管理类</summary>
    public class SimpleServiceAuthorizationManager : ServiceAuthorizationManager
    {
        ConnInfo _conn;
        ConnInfo conn
        {
            get
            {
                if (_conn == null)
                {
                    foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
                    {
                        string[] ss = item.Name.Split('_');
                        if (ss[0] == "管理数据源")
                        {
                            EDataBaseType dbtype = dbhelper.getDBType(ss[1]);
                            _conn = new ConnInfo(ss[0], item.ConnectionString, dbtype, ss[2]);
                        }
                    }
                }
                return _conn;
            }
        }

        protected override bool CheckAccessCore(OperationContext operationContext)
        {

            string userName = operationContext.ServiceSecurityContext.PrimaryIdentity.Name;


            //每次读数据库可能导致性能问题，采用静态内存保存
            //operationContext.ServiceSecurityContext.AuthorizationContext.Properties["Principal"] = GetPrincipal(userName);
            operationContext.ServiceSecurityContext.AuthorizationContext.Properties["Principal"] =
                new GenericPrincipal(new GenericIdentity(userName), DataService.users[userName].arrayRoles); ;
            return true;
        }
        private IPrincipal GetPrincipal(string userName)
        {
            GenericIdentity identity = new GenericIdentity(userName);
            string sql = string.Format("select t2.RoleName from Users t1,Roles t2,UsersInRoles t3 where t1.UserName='{0}' and t1.UserID=t3.UserID and t2.RoleID=t3.RoleID", userName);
            DataTable dt = conn.getDataTable(sql);
            string[] roles = new string[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
                roles[i] = dt.Rows[i][0].ToString();
            return new GenericPrincipal(identity, roles);
        }


    }





    internal class UserInfo
    {
        public UserInfo()
        {
            roles = new List<string>();
            models = new List<string>();
            conns = new Dictionary<string, bool>();
        }

        public string appID { get; set; }

        public string userID { get; set; }

        public string userName { get; set; }

        public List<string> roles { get; set; }

        public List<string> models { get; set; }

        public Dictionary<string, bool> conns { get; set; }  //数据源名称, 只读

        bool? _isManager;
        public bool isManager { get { if (_isManager == null) _isManager = roles.Contains("管理员"); return (bool)_isManager; } }

        string[] _arrayRoles;
        public string[] arrayRoles
        {
            get
            {
                if (_arrayRoles == null)
                {
                    _arrayRoles = new string[roles.Count];
                    for (int i = 0; i < roles.Count; i++)
                    {
                        _arrayRoles[i] = roles[i];
                    }
                }
                return _arrayRoles;
            }
        }
    }


    ///<summary>应用程序运行数据</summary>
    internal class AppData
    {
        ///<summary>是否启用日志</summary>
        internal bool isLog = false;
        ///<summary>日志有效期限（日）</summary>
        internal int logValidDays = 10;
        ///<summary>上次清理日志时间</summary>
        internal DateTime? lastClearLogDate = null;
        ///<summary>是否记录读数据源操作</summary>
        internal bool isLogRead = false;
        ///<summary>是否记录数据库执行命令操作</summary>
        internal bool isLogExecute = true;
        ///<summary>是否记录管理数据库执行的操作</summary>
        internal bool isLogManage = true;
        ///<summary>是否记录安全信息</summary>
        internal bool isLogSafe = true;
        ///<summary>是否记录错误信息</summary>
        internal bool isLogError = true;
    }

}
