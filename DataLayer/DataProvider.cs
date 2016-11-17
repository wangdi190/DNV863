using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataLayer
{
    public enum EDataStatus { 模拟, WCF, 数据库}
    public enum EDataBaseType { MsSql, MySql }
    public enum EReadMode {自动,数据库读取,模拟}
    public enum EPasswordStrong {弱,中,强}

    public static class DataProvider
    {
        static DataProvider()
        {
            readSentenceFromXml();
            dataStatus = EDataStatus.数据库;
        }

        internal static EPasswordStrong passwordStrong = EPasswordStrong.弱;

        static bool _isAuthorize;
        ///<summary>是否进行身份认证和授权，若进行认证，statStatus数据状态自动切换为WCF, 反之若dataStatus设置为WCF，则isAuthorize为真</summary>
        public static bool isAuthorize
        {
            get { return _isAuthorize; }
            set 
            {
                _isAuthorize = value;
                if (_isAuthorize)
                    _dataStatus = EDataStatus.WCF;
                else
                    _dataStatus=  EDataStatus.数据库;
            }
        }

        static EDataStatus _dataStatus;
        ///<summary>采用什么数据来源, 若需进行认证，请设置为wcf</summary>
        public static EDataStatus dataStatus
        {
            get { return _dataStatus; }
            set 
            {
                _dataStatus = value;
                if (_dataStatus== EDataStatus.WCF)
                    _isAuthorize=true;
                else
                    _isAuthorize=false;
            }
        }

        ///<summary>获取当前数据连接的数据库类型</summary>
        public static EDataBaseType databaseType { get { return DirectDBAccessor.connections.FirstOrDefault(p => p.datasourceName == curDataSourceName).databaseType; } }
        ///<summary>设置或获得当前使用的数据源（当系统同时使用多种数据来源时，用于切换数据连接）</summary>
        public static string curDataSourceName = "基础数据源";
        ///<summary>当语句从xml文件存取时，语句是否加密</summary>
        public static bool isEncryptXml = false;

        internal static WcfDataService.DataServiceClient wcf;
        ///<summary>wcf提供验证的登录界面</summary>
        public static void Login() 
        {
            if (wcf == null)
            {
                WCF.WinLogin login = new WCF.WinLogin();
                if (login.ShowDialog() == false)
                {
                        System.Windows.MessageBox.Show("登录失败，退出程序！");
                        Environment.Exit(0);
                }
            }
        }

        ///<summary>返回用户是否有模块运行权限</summary>
        public static bool checkModelPermissionn(string ModelName)
        {
            return wcf.CheckModelPermissionn(ModelName);
        }

        #region ===== 新增可使用数据模拟的方法 =====
        ///<summary>综合readmode参数(优先)和dataStatus静态变量，返回应采用的读取模式</summary>
        static EReadMode getReadMode(EReadMode readmode)
        {
            EReadMode rm;
            if (readmode != EReadMode.自动)
                rm = readmode;
            else
            {
                if (dataStatus == EDataStatus.模拟)
                    rm = EReadMode.模拟;
                else
                    rm = EReadMode.数据库读取;
            }
            return rm;
        }

        ///<summary>返回数据表，依据dataStatus的值从数据库用sql语句读取或用sim语句模拟。优先判断readmode指定的读取模式，readmode为自动的情况下，由静态变量dataStatus决定读取模式</summary>
        public static KeyValuePair<EReadMode, DataTable> getDataTable(string sql, string sim, EReadMode readmode= EReadMode.自动,bool isShowMessage=true)
        {
            if (getReadMode(readmode)== EReadMode.数据库读取)
                return new KeyValuePair<EReadMode, DataTable>(EReadMode.数据库读取, getDataTableFromSQL(sql, isShowMessage));
            else
                return new KeyValuePair<EReadMode,DataTable>( EReadMode.模拟, Simulate.simData(sim));
        }
        ///<summary>返回数据表，依据dataStatus的值从数据库用sql语句读取或用sim语句模拟, 模拟时采用整型ids列表为ID </summary>
        public static KeyValuePair<EReadMode, DataTable> getDataTable(string sql, string sim, List<int> ids, EReadMode readmode = EReadMode.自动,bool isShowMessage=true)
        {
            if (getReadMode(readmode) == EReadMode.数据库读取)
                return new KeyValuePair<EReadMode, DataTable>(EReadMode.数据库读取, getDataTableFromSQL(sql, isShowMessage));
            else
                return new KeyValuePair<EReadMode, DataTable>(EReadMode.模拟, Simulate.simData(sim,ids));
        }
        ///<summary>返回数据表，依据dataStatus的值从数据库用sql语句读取或用sim语句模拟，模拟时采用字符串型ids列表为ID </summary>
        public static KeyValuePair<EReadMode, DataTable> getDataTable(string sql, string sim, List<string> ids, EReadMode readmode = EReadMode.自动, bool isShowMessage = true)
        {
            if (getReadMode(readmode) == EReadMode.数据库读取)
                return new KeyValuePair<EReadMode, DataTable>(EReadMode.数据库读取, getDataTableFromSQL(sql, isShowMessage));
            else
                return new KeyValuePair<EReadMode, DataTable>(EReadMode.模拟, Simulate.simData(sim,ids));
        }
        ///<summary>返回数据表，依据dataStatus的值从数据库用sql语句读取或用sim语句模拟，模拟时以在参数oldDataTable的数据基础上模拟 </summary>
        public static KeyValuePair<EReadMode, DataTable> getDataTable(string sql, string sim, DataTable oldDataTable, EReadMode readmode = EReadMode.自动, bool isShowMessage = true)
        {
            if (getReadMode(readmode) == EReadMode.数据库读取)
                return new KeyValuePair<EReadMode, DataTable>(EReadMode.数据库读取, getDataTableFromSQL(sql, isShowMessage));
            else
                return new KeyValuePair<EReadMode, DataTable>(EReadMode.模拟, Simulate.simData(sim,oldDataTable));

        }

        //----------------------------- xml相关 ----------------------------------
        static SentenceCollection sentenceCollection;
        ///<summary>从xml文件读取语句</summary>
        static void readSentenceFromXml()
        {
            string filename = ".\\Xml\\DataSentence.xml";
            if (System.IO.File.Exists(filename))
                sentenceCollection = (SentenceCollection)MyClassLibrary.XmlHelper.readFromXml(filename, typeof(SentenceCollection));
        }

        ///<summary>返回数据表，依据dataStatus的值，用指定键值代表的sql语句或sim语句来读取或模拟数据 </summary>
        public static KeyValuePair<EReadMode, DataTable> getDataTableByKey(string key, EReadMode readmode = EReadMode.自动,bool isShowMessage=true)
        {
            SqlSimDesc desc;
            if (sentenceCollection.sentences.TryGetValue(key, out desc))
                return getDataTable(desc.sql, desc.sim, readmode, isShowMessage);
            else
                System.Windows.MessageBox.Show(String.Format("未找到指定键值{0}的语句！", key));
            return new KeyValuePair<EReadMode,DataTable>();
        }
        ///<summary>返回数据表，依据dataStatus的值，用指定键值代表的sql语句或sim语句来读取或模拟数据, 模拟时采用整型ids列表为ID </summary>
        public static KeyValuePair<EReadMode, DataTable> getDataTableByKey(string key, List<int> ids, EReadMode readmode = EReadMode.自动, bool isShowMessage = true)
        {
            SqlSimDesc desc;
            if (sentenceCollection.sentences.TryGetValue(key, out desc))
                return getDataTable(desc.sql, desc.sim, ids, readmode,isShowMessage);
            else
                System.Windows.MessageBox.Show(String.Format("未找到指定键值{0}的语句！", key));
            return new KeyValuePair<EReadMode, DataTable>();
        }
        ///<summary>返回数据表，依据dataStatus的值，用指定键值代表的sql语句或sim语句来读取或模拟数据，模拟时采用字符串型ids列表为ID </summary>
        public static KeyValuePair<EReadMode, DataTable> getDataTableByKey(string key, List<string> ids, EReadMode readmode = EReadMode.自动, bool isShowMessage = true)
        {
            SqlSimDesc desc;
            if (sentenceCollection.sentences.TryGetValue(key, out desc))
                return getDataTable(desc.sql, desc.sim, ids, readmode, isShowMessage);
            else
                System.Windows.MessageBox.Show(String.Format("未找到指定键值{0}的语句！", key));
            return new KeyValuePair<EReadMode, DataTable>();

        }
        ///<summary>返回数据表，依据dataStatus的值，用指定键值代表的sql语句或sim语句来读取或模拟数据，模拟时以在参数oldDataTable的数据基础上模拟 </summary>
        public static KeyValuePair<EReadMode, DataTable> getDataTableByKey(string key, DataTable oldDataTable, EReadMode readmode = EReadMode.自动, bool isShowMessage = true)
        {
            SqlSimDesc desc;
            if (sentenceCollection.sentences.TryGetValue(key, out desc))
                return getDataTable(desc.sql, desc.sim, oldDataTable, readmode, isShowMessage);
            else
                System.Windows.MessageBox.Show(String.Format("未找到指定键值{0}的语句！", key));
            return new KeyValuePair<EReadMode, DataTable>();
        }
        #endregion


        /// <summary>
        /// 按给定的SQL语句查询数据，查询将依次尝试所有连接，直至获取数据，不通的连接在程序运行期间将不再尝试
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="isShowMessage">出错时，是否弹出错误信息窗体</param>
        /// <param name="groupNum">分组号，仅同时多种数据源时需给出此参数</param>
        /// <returns>返回DataTable</returns>
        public static DataTable getDataTableFromSQL(string sql, bool isShowMessage = true)
        {
            if (string.IsNullOrWhiteSpace(sql)) return null;

            DataTable dt = null;
            if (dataStatus == EDataStatus.WCF)
            {
                try
                {
                    if (wcf == null)
                    {
                        WCF.WinLogin login = new WCF.WinLogin();
                        if (login.ShowDialog() == false)
                        {
                            if (isShowMessage)
                            {
                                System.Windows.MessageBox.Show("WCF数据服务失败，退出程序！");
                                Environment.Exit(0);
                            }
                        }
                    }
                    dt = wcf.GetDataTable(sql, curDataSourceName);
                }
                catch (Exception ee)
                {
                    try
                    {
                        WCF.WinLogin login = new WCF.WinLogin();
                        if (login.ShowDialog() == false)
                        {
                            if (isShowMessage)
                            {
                                System.Windows.MessageBox.Show("WCF数据服务失败，退出程序！"+ee.Message);
                                Environment.Exit(0);
                            }
                        }
                        dt = wcf.GetDataTable(sql, curDataSourceName);
                    }
                    catch (Exception eee)
                    {
                        if (isShowMessage)
                        {
                            System.Windows.MessageBox.Show("WCF数据服务失败，退出程序！"+eee.Message);
                            Environment.Exit(0);
                        }
                    }
                }
            }
            else
                dt = DirectDBAccessor.readDataBase(sql, isShowMessage, curDataSourceName);
            return dt;
        }

        /// <summary>
        /// 执行给定的SQL语句，程序尝试对所有连接执行该SQL语句，不通的连接在程序运行期间不再尝试。
        /// </summary>
        /// <param name="sql">SQL语句</param>
        public static bool ExecuteSQL(string sql, bool isShowMessage = true)
        {
            if (dataStatus != EDataStatus.WCF)
                return DirectDBAccessor.executeCommand(sql, isShowMessage, curDataSourceName);
            else
            {

                try
                {
                    if (wcf == null)
                    {
                        WCF.WinLogin login = new WCF.WinLogin();
                        if (login.ShowDialog() == false)
                        {
                            if (isShowMessage)
                            {
                                System.Windows.MessageBox.Show("WCF数据服务连接失败，退出程序！");
                                Environment.Exit(0);
                            }
                        }
                    }
                    return wcf.ExecuteCommand(sql,curDataSourceName);
                }
                catch
                {
                    try
                    {
                        WCF.WinLogin login = new WCF.WinLogin();
                        if (login.ShowDialog() == false)
                        {
                            if (isShowMessage)
                            {
                                System.Windows.MessageBox.Show("WCF数据服务连接失败，退出程序！");
                                Environment.Exit(0);
                            }
                        }
                        return wcf.ExecuteCommand(sql, curDataSourceName);
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show("WCF数据服务连接失败，退出程序！");
                        Environment.Exit(0);
                    }
                    return false;
                }


            }
        }

        /// <summary>
        /// 批量执行
        /// </summary>
        /// <param name="sqls">sql语句列表</param>
        public static void bacthExecute(List<string> sqls)
        {
            if (dataStatus == EDataStatus.数据库)
                DirectDBAccessor.bacthExecute(sqls, curDataSourceName);
            else
            {

            }
        }

     


        /// <summary>
        /// 测试数据连接通断
        /// </summary>
        /// <param name="idx">连接序号，0为第一个连接</param>
        /// <returns>True 通; False 断</returns>
        public static bool TestConnection(int idx = 0)
        {
            if (dataStatus == EDataStatus.数据库)
                return DirectDBAccessor.testConn(idx);
            else
            {
                bool result = false;
                try
                {
                    if (wcf == null)
                    {
                        WCF.WinLogin login = new WCF.WinLogin();
                        if (login.ShowDialog() == false)
                        {
                                System.Windows.MessageBox.Show("WCF数据服务连接失败，退出程序！");
                                Environment.Exit(0);
                        }
                    }
                    result = true;
                }
                catch
                { }
                return result;
            }
        }

        ///<summary>所有的连接数</summary>
        public static int ConnectionCount
        {
            get
            {
                if (dataStatus == EDataStatus.数据库)
                    return DirectDBAccessor.connectionCount;
                else
                    return 1;
            }
        }

        ///<summary>当前可用的连接</summary>
        public static List<ConnInfo> CanUseConnections
        {
            get
            {
                if (dataStatus == EDataStatus.数据库)
                    return DirectDBAccessor.connections.Where(p => p.istry).ToList();
                else
                    return null;
            }
        }


        //private static Random rd = new Random();
        //#region 模拟数据提供
        //public static DataTable simGetDataTable(string sql, int simcount, string timespan, double min, double max)
        //{
        //    string[] simstr1 = new string[5] { "甲", "乙", "丙", "丁", "戊" };
        //    string[] simstr2 = new string[5] { "AA", "BB", "CC", "DD", "EE" };
        //    string[] simstr;
        //    if (rd.NextDouble() > 0.5)
        //        simstr = simstr1;
        //    else
        //        simstr = simstr2;

        //    List<FieldDef> fd;
        //    DataTable dt = simGetEmptyDTFromSQL(sql, out fd);

        //    for (int i = 0; i < simcount; i++)
        //    {
        //        DataRow dr = dt.NewRow();
        //        for (int j = 0; j < fd.Count; j++)
        //        {
        //            if (fd[j].fieldType == typeof(DateTime))
        //            {
        //                switch (timespan)
        //                {
        //                    case "分":
        //                        dr[j] = (new DateTime(DateTime.Now.Year - 1, 1, 1)).AddMinutes(5);
        //                        break;
        //                    case "时":
        //                        dr[j] = (new DateTime(DateTime.Now.Year - 1, 1, 1)).AddHours(i);
        //                        break;
        //                    case "日":
        //                        dr[j] = (new DateTime(DateTime.Now.Year - 1, 1, 1)).AddDays(i);
        //                        break;
        //                    case "月":
        //                        dr[j] = (new DateTime(DateTime.Now.Year - 1, 1, 1)).AddMonths(i);
        //                        break;
        //                    default:
        //                        dr[j] = (new DateTime(DateTime.Now.Year - 5, 1, 1)).AddYears(i);
        //                        break;
        //                }
        //            }
        //            else if (fd[j].fieldType == typeof(double))
        //            {
        //                dr[j] = min + (max - min) * rd.NextDouble();
        //            }
        //            else
        //            {
        //                dr[j] = simstr[rd.Next(simstr.Count())];
        //            }


        //        }
        //        dt.Rows.Add(dr);
        //    }

        //    return dt;
        //}

        //private static DataTable simGetEmptyDTFromSQL(string sql, out List<FieldDef> fd)
        //{
        //    fd = new List<FieldDef>();
        //    string s = sql.ToLower();
        //    s = s.Substring(0, s.IndexOf(" from"));
        //    s = s.Replace("select ", "");
        //    string[] fs = s.Split(',');
        //    foreach (string item in fs)
        //    {
        //        string ss;
        //        if (item.IndexOf(" as ") > -1)
        //            ss = item.Substring(item.IndexOf(" as ") + 4);
        //        else
        //            ss = item;

        //        char pfx = ss[0];
        //        Type type;
        //        switch (pfx)
        //        {
        //            case 'd':
        //                type = typeof(DateTime);
        //                break;
        //            case 'n':
        //                type = typeof(Double);
        //                break;
        //            default:
        //                type = typeof(string);
        //                break;
        //        }

        //        fd.Add(new FieldDef(ss, type));
        //    }

        //    return simGenEmptyDT(fd);
        //}

        //private static DataTable simGenEmptyDT(List<FieldDef> lstfd)
        //{
        //    DataTable dt = new DataTable();
        //    foreach (FieldDef item in lstfd)
        //    {
        //        dt.Columns.Add(new DataColumn(item.fieldName, item.fieldType));
        //    }
        //    //dt.Columns.Add(new DataColumn("zSort0", typeof(string)));   //层0   
        //    return dt;
        //}


        //#endregion
    }

    internal struct FieldDef
    {
        public FieldDef(string fn, Type ft)
        {
            fieldName = fn;
            fieldType = ft;
        }
        public string fieldName;
        public Type fieldType;
    }



}
