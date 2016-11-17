using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace ManageDBTool
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DBOperate.conn = ManageDBTool.Properties.Settings.Default.conn;
            string dbtype = ManageDBTool.Properties.Settings.Default.dbtype.ToLower();
            if (dbtype == "mssql")
                DBOperate.databaseType = EDataBaseType.MsSql;
            else if (dbtype == "mysql")
                DBOperate.databaseType = EDataBaseType.MySql;
        }
    }
}
