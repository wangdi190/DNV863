using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.ServiceModel;

namespace WcfDataService
{
    public enum EDataBaseType { 未知, MsSql, MySql }

    ///<summary>连接串类</summary>
    public class ConnInfo
    {
        /// <summary>
        /// 连接描述, 含数据源名称和连接串，数据源名称允许数据层扩展为同时操作多个数据来源（注意：镜像数据库或同一数据库的wcf读取模式，应使用同一数据源名称）
        /// </summary>
        /// <param name="DatasourceName">数据源名称</param>
        /// <param name="connstring">连接串</param>
        public ConnInfo(string DatasourceName, string connstring, EDataBaseType DatabaseType, string connName)
        {
            name = connName;
            conn = connstring;
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

        ///<summary>是否尝试连接</summary>
        internal bool istry = true;

        internal DataTable getDataTable(string sql)
        {
            DataSet dataset = new DataSet();
            try
            {
                if (databaseType == EDataBaseType.MsSql)
                {
                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        adapter.SelectCommand = new SqlCommand(sql, connection);
                        adapter.Fill(dataset);
                    }
                    return dataset.Tables[0];
                }
                else if (databaseType == EDataBaseType.MySql)
                {
                    using (MySqlConnection connection = new MySqlConnection(conn))
                    {
                        MySqlDataAdapter adapter = new MySqlDataAdapter();
                        adapter.SelectCommand = new MySqlCommand(sql, connection);
                        adapter.Fill(dataset);
                    }
                    return dataset.Tables[0];
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        internal bool executeCommand(string executeString)
        {
            try
            {
                if (databaseType == EDataBaseType.MsSql)
                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        SqlCommand command = new SqlCommand(executeString, connection);
                        command.Connection.Open();
                        command.ExecuteNonQuery();
                    }
                else if (databaseType == EDataBaseType.MySql)
                {
                    using (MySqlConnection connection = new MySqlConnection(conn))
                    {
                        MySqlCommand command = new MySqlCommand(executeString, connection);
                        command.Connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }



    public static class dbhelper
    {
        public static EDataBaseType getDBType(string strDBType)
        {
            string lower = strDBType.ToLower();
            string[] ss = Enum.GetNames(typeof(EDataBaseType));
            for (int i = 0; i < ss.Count(); i++)
            {
                if (ss[i].ToLower() == lower)
                    return (EDataBaseType)i;
            }
            return EDataBaseType.未知;
        }

    }



}