using System;
using System.Windows.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Windows;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;


namespace DataLayer
{
    #region 直接读写数据库
    ///<summary>连接串类</summary>
    public class ConnInfo
    {
        /// <summary>
        /// 简单连接描述，只含连接串和ping测试IP
        /// </summary>
        /// <param name="connstring">连接串</param>
        public ConnInfo(string connstring, string IP, EDataBaseType DatabaseType)
        {
            name = "连接";
            conn = connstring;
            ip = IP;
            databaseType = DatabaseType;
            datasourceName = "基础数据源";
        }

        /// <summary>
        /// 连接描述, 含数据源名称和连接串，数据源名称允许数据层扩展为同时操作多个数据来源（注意：镜像数据库或同一数据库的wcf读取模式，应使用同一数据源名称）
        /// </summary>
        /// <param name="DatasourceName">数据源名称</param>
        /// <param name="connstring">连接串</param>
        public ConnInfo(string DatasourceName, string connstring, string IP, EDataBaseType DatabaseType)
        {
            name = "连接";
            conn = connstring;
            ip = IP;
            databaseType = DatabaseType;
            datasourceName = DatasourceName;
        }

        ///<summary>连接的名称</summary>
        public string name { get; set; }
        ///<summary>连接字串</summary>
        public string conn { get; set; }

        ///<summary>数据源名称（缺省为基础数据源），以便数据层扩展为可以同时操作多个数据源（镜像类数据库应使用相同的数据源名称）</summary>
        public string datasourceName { get; set; }

        ///<summary>数据库类型</summary>
        public EDataBaseType databaseType { get; set; }

        public string ip { get; set; }

        ///<summary>是否尝试连接</summary>
        internal bool istry = true;
    }


    public static class DirectDBAccessor
    {
        static DirectDBAccessor()
        {

            //connectionString = "Data Source=(local);Initial Catalog=DMSDB;Integrated Security=True";
            //connectionString = "Data Source=10.218.230.15;Initial Catalog=DMSDB;User ID=ch;Password=root.2011";

            //connections.Add(new ConnInfo("Data Source=192.168.1.234;Initial Catalog=DMSDB;User ID=sa;Password=123456;Connection Timeout=3"));

        }


        ///<summary>尝试连接前是否先Ping IP，以避免长时间等待，若服务器防火墙禁止Ping，则必须将此属性设置为False。</summary>
        public static bool isFirstTestIP { get; set; }


        public static List<ConnInfo> connections = new List<ConnInfo>();

        public static int connectionCount { get { return connections.Count; } }

        ///<summary>按所有可尝试连接依次读，取到数据后即终止并返回数据，不通的连接将置istry为false，下次不再尝试从该连接读取</summary>
        internal static DataTable readDataBase(string queryString, bool isShowMessage, string datasourceName)
        {

            DataSet dataset = new DataSet();
            string einfo = "";
            foreach (ConnInfo ci in connections.Where(p => p.datasourceName == datasourceName))
            {
                if (ci.istry && testIP(ci.ip))
                {
                    ci.istry = testIP(ci.ip); //若ip不通，不再使用此连接
                    try
                    {
                        if (ci.databaseType == EDataBaseType.MsSql)
                        {
                            using (SqlConnection connection = new SqlConnection(ci.conn))
                            {
                                SqlDataAdapter adapter = new SqlDataAdapter();
                                adapter.SelectCommand = new SqlCommand(queryString, connection);
                                adapter.Fill(dataset);
                            }
                            return dataset.Tables[0];
                        }
                        else if (ci.databaseType == EDataBaseType.MySql)
                        {
                            using (MySqlConnection connection = new MySqlConnection(ci.conn))
                            {
                                MySqlDataAdapter adapter = new MySqlDataAdapter();
                                adapter.SelectCommand = new MySqlCommand(queryString, connection);
                                adapter.Fill(dataset);
                            }
                            return dataset.Tables[0];
                        }

                    }
                    catch (Exception e)
                    {
                        einfo = e.Message;
                        //ci.istry = false;
                    }
                }
                //else
                //    ci.istry = false;
            }
            if (isShowMessage)
            {
                System.Windows.MessageBox.Show("不能联接数据库或SQL语句错误，程序将关闭！" + queryString, "错误信息");
                System.Windows.MessageBox.Show(einfo, "错误信息");
                Environment.Exit(0);
            }

            return null;
        }

        ///<summary>对每个可尝试连接均执行</summary>
        internal static bool executeCommand(string executeString, bool isShowMessage, string datasourceName)
        {
            string einfo = "";
            bool isAllClose = true;
            foreach (ConnInfo ci in connections.Where(p => p.datasourceName == datasourceName))
            {
                if (ci.istry && testIP(ci.ip))
                {
                    ci.istry = testIP(ci.ip); //若ip不通，不再使用此连接
                    try
                    {
                        if (ci.databaseType == EDataBaseType.MsSql)
                        using (SqlConnection connection = new SqlConnection(ci.conn))
                        {
                            SqlCommand command = new SqlCommand(executeString, connection);
                            command.Connection.Open();
                            command.ExecuteNonQuery();
                        }
                        else if (ci.databaseType == EDataBaseType.MySql)
                        {
                            using (MySqlConnection connection = new MySqlConnection(ci.conn))
                            {
                                MySqlCommand command = new MySqlCommand(executeString, connection);
                                command.Connection.Open();
                                command.ExecuteNonQuery();
                            }
                        }

                        isAllClose = false;
                    }
                    catch (Exception e)
                    {
                        einfo = e.Message;
                        //ci.istry = false;
                    }
                }
                //else
                //    ci.istry = false;
            }
            if (isAllClose)
            {
                if (isShowMessage)
                {
                    System.Windows.MessageBox.Show("不能联接数据库或SQL语句错误，程序将关闭！" + executeString, "错误信息");
                    System.Windows.MessageBox.Show(einfo, "错误信息");
                    Environment.Exit(0);
                }
                return false;
            }
            return true;
        }


        public static bool testConn(int connidx)
        {
            if (connidx < connectionCount && testIP(connections[connidx].conn))
            {
                string connectionString = connections[connidx].conn;

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }


        static bool testIP(string strIP)
        {
            if (isFirstTestIP)
            {
                Ping pingSender = new Ping();
                PingOptions options = new PingOptions();
                options.DontFragment = true;
                string data = "t";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 3000;
                PingReply reply = pingSender.Send(strIP, timeout, buffer, options);

                return (reply.Status == IPStatus.Success);
            }
            else
                return true;




        }

        ///<summary>批量执行，事务方式</summary>
        internal static void bacthExecute(List<string> sqls, string datasourceName)
        {
            string einfo = "";
            bool isAllClose = true;
            foreach (ConnInfo ci in connections.Where(p => p.datasourceName == datasourceName))
            {
                if (ci.istry && testIP(ci.ip))
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ci.conn))
                        {
                            connection.Open();
                            SqlCommand command = connection.CreateCommand();
                            SqlTransaction transaction;
                            transaction = connection.BeginTransaction("batchTransaction");
                            command.Connection = connection;
                            command.Transaction = transaction;

                            string tmp;
                            try
                            {
                                foreach (string item in sqls)
                                {
                                    command.CommandText = item;
                                    tmp = item;
                                    command.ExecuteNonQuery();
                                }
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    transaction.Rollback();
                                }
                                catch (Exception ex2)
                                {
                                }
                                System.Windows.MessageBox.Show(ex.Message);
                            }
                        }
                        isAllClose = false;
                    }
                    catch (Exception e)
                    {
                        einfo = e.Message;
                        ci.istry = false;
                    }
                else
                    ci.istry = false;
            }
            if (isAllClose)
            {
                System.Windows.MessageBox.Show("不能联接数据库或SQL语句错误，程序将关闭！", "错误信息");
                System.Windows.MessageBox.Show(einfo, "错误信息");
                Environment.Exit(0);
            }


        }






    }


    #endregion

}
