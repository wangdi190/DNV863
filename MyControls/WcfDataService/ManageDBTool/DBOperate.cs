using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Web;
using MySql.Data.MySqlClient;


namespace ManageDBTool
{
    public enum EDataBaseType { 未知, MsSql, MySql }

    public static class DBOperate
    {
        public static string conn;
        public static EDataBaseType databaseType;

        internal static DataTable getDataTable(string sql)
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
            catch (Exception e)
            {
                
            }
            return null;
        }

        ///<summary>对每个可尝试连接均执行</summary>
        internal static bool executeCommand(string executeString)
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
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
