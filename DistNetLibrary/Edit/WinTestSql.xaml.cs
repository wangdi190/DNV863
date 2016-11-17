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
using System.Windows.Shapes;

namespace DistNetLibrary.Edit
{
    /// <summary>
    /// WinTestSql.xaml 的交互逻辑
    /// </summary>
    public partial class WinTestSql : Window
    {
        public WinTestSql(SQL Sql)
        {
            sql = Sql;

            InitializeComponent();
        }

        Random rd = new Random();
        string keyPara;
        string timePara;
        internal SQL sql;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            infos = new System.ComponentModel.BindingList<CInfo>();
            lstinfo.ItemsSource = infos;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            test();

        }

        System.ComponentModel.BindingList<CInfo> infos { get; set; }
        void test()
        {
            DataTable dt;
            string tmpid;
            string tmpfielter="";
            string s = "";
            //================== 生成查询参数
            keyPara = "999999999";

            #region ===== 测试台账语句 =====
            infos.Add(new CInfo() { info = "★开始测试台账SQL语句" });
            infos.Add(new CInfo() { info = "　☆开始测试台账Insert语句" });
            if (sql.acntInsert.Count == 0)
            {
                infos.Add(new CInfo() { info = "　　☆无台账Insert语句，放弃后续台账Update、Select、Delete语句测试。", brush = Brushes.Red });
            }
            else
            {
                foreach (string ss in sql.acntInsert)
                {
                    s = ss;
                    foreach (var item in sql.keypdesces)
                    {
                        TableDesc td = sql.acntTableRelation.tables.FirstOrDefault(p => p.tableName == item.tablename);
                        if (item.fieldname == td.keyFieldName) //主键字段
                            s = s.Replace(string.Format("※{0}※", item.propertyname), keyPara);
                        else
                            s = s.Replace(string.Format("※{0}※", item.propertyname), simValue(item.fieldtypename));
                    }
                    foreach (var item in sql.anctdesces)
                        s = s.Replace(string.Format("※{0}※", item.propertyname), simValue(item.fieldtypename));
                    if (DataLayer.DataProvider.ExecuteSQL(s, false))
                        infos.Add(new CInfo() { info = "　　台账Insert语句测试成功。", sql = s });
                    else
                        infos.Add(new CInfo() { info = "　　台账Insert语句测试失败。", sql = s, brush = Brushes.Red });
                }
                //-----
                infos.Add(new CInfo() { info = "　☆开始测试台账Update语句" });
                foreach (string ss in sql.acntUpdate)
                {
                    s = ss;
                    foreach (var item in sql.keypdesces)
                        if (item.propertyname=="ID" || item.propertyname=="ID2")
                            s = s.Replace(string.Format("※{0}※", item.propertyname), keyPara);
                        else
                            s = s.Replace(string.Format("※{0}※", item.propertyname), simValue(item.fieldtypename));
                    foreach (var item in sql.anctdesces)
                        s = s.Replace(string.Format("※{0}※", item.propertyname), simValue(item.fieldtypename));
                    s = string.Format(s, keyPara);
                    if (DataLayer.DataProvider.ExecuteSQL(s, false))
                        infos.Add(new CInfo() { info = "　　台账Update语句测试成功。", sql = s });
                    else
                        infos.Add(new CInfo() { info = "　　台账Update语句测试失败。", sql = s, brush = Brushes.Red });
                }
                //-----
                infos.Add(new CInfo() { info = "　☆开始测试台账Select语句" });
                s = string.Format(sql.acntSelect, keyPara);
                dt = DataLayer.DataProvider.getDataTableFromSQL(s, false);
                if (dt != null)
                    infos.Add(new CInfo() { info = "　　台账Select语句测试成功，查询记录" + dt.Rows.Count + "条。", sql = s });
                else
                    infos.Add(new CInfo() { info = "　　台账Select语句测试失败。", sql = s, brush = Brushes.Red });
                //-----
                infos.Add(new CInfo() { info = "　☆开始测试台账Delete语句" });
                foreach (string ss in sql.acntDelete)
                {
                    s = string.Format(ss, keyPara);
                    if (DataLayer.DataProvider.ExecuteSQL(s, false))
                        infos.Add(new CInfo() { info = "　　台账Delete语句测试成功。", sql = s });
                    else
                        infos.Add(new CInfo() { info = "　　台账Delete语句测试失败。", sql = s, brush = Brushes.Red });
                }


                //再次清理insert产生的记录
                foreach (var tab in sql.acntTableRelation.tables)
                {
                    s = string.Format("delete {0} where {1}='{2}'", tab.tableName, tab.keyFieldName, keyPara);
                    if (DataLayer.DataProvider.ExecuteSQL(s, false))
                        infos.Add(new CInfo() { info = "　　已清理台账Insert语句测试产生的记录。", sql = s });
                    else
                        infos.Add(new CInfo() { info = "　　清理台账测试Insert产生的记录失败，请手动清理数据库。", sql = s, brush = Brushes.Red });
                }
            }
            #endregion

            #region ===== 测试实时运行和规划运行语句 =====
            if (!string.IsNullOrWhiteSpace(sql.rundataSelect))
            {
                infos.Add(new CInfo() { info = "★开始测试实时运行数据Select语句" });
                dt = DataLayer.DataProvider.getDataTableFromSQL(sql.rundataTestSQL, false);
                if (dt != null && dt.Rows.Count > 0)
                {
                    tmpid = dt.Rows[0][0].ToString();
                    s = string.Format(sql.rundataSelect, tmpid);
                    dt = DataLayer.DataProvider.getDataTableFromSQL(s, false);
                    if (dt != null)
                        infos.Add(new CInfo() { info = "　　实时运行Select语句测试成功，查询记录" + dt.Rows.Count + "条。", sql = s });
                    else
                        infos.Add(new CInfo() { info = "　　实时运行Select语句测试失败。", sql = s, brush = Brushes.Red });
                }
                else
                    infos.Add(new CInfo() { info = "　　可能因主表无数据，无法测试实时运行Select语句。", brush = new SolidColorBrush(Color.FromRgb(0xFF, 0x99, 0x00)) });
            }
            else
                infos.Add(new CInfo() { info = "☆无实时运行数据描述，跳过实时运行数据测试。" });

            //===============================
            if (!string.IsNullOrWhiteSpace(sql.planningSelect))
            {
                infos.Add(new CInfo() { info = "★开始测试规划模拟运行数据Select语句" });
                dt = DataLayer.DataProvider.getDataTableFromSQL(sql.planningTestSQL, false);
                if (dt != null && dt.Rows.Count > 0)
                {
                    tmpid = dt.Rows[0][0].ToString();
                    s = string.Format(sql.planningSelect, tmpid);
                    dt = DataLayer.DataProvider.getDataTableFromSQL(s, false);
                    if (dt != null)
                        infos.Add(new CInfo() { info = "　　规划模拟运行Select语句测试成功，查询记录" + dt.Rows.Count + "条。", sql = s });
                    else
                        infos.Add(new CInfo() { info = "　　规划模拟运行Select语句测试失败。", sql = s, brush = Brushes.Red });
                }
                else
                    infos.Add(new CInfo() { info = "　　可能因主表无数据，无法测试规划模拟运行Select语句。", brush = new SolidColorBrush(Color.FromRgb(0xFF, 0x99, 0x00)) });
            }
            else
                infos.Add(new CInfo() { info = "☆无规划模拟运行数据描述，跳过规划模拟运行数据测试。" });
            #endregion

            //#region ===== 测试拓扑基础关联语句 =====
            //infos.Add(new CInfo() { info = "★开始测试拓扑数据基础关联SQL语句" });
            //infos.Add(new CInfo() { info = "　☆开始拓扑基础关联Insert语句" });
            //if (sql.topoReInsert.Count == 0)
            //{
            //    infos.Add(new CInfo() { info = "　　☆无拓扑基础关联Insert语句，放弃后续Update、Select、Delete语句测试。", brush = Brushes.Red });
            //}
            //else
            //{
            //    foreach (string ss in sql.topoReInsert)
            //    {
            //        s = ss;
            //        foreach (var item in sql.toporelationdesces)
            //            s = s.Replace(string.Format("※{0}※", item.tablename + "." + item.fieldname), simValue(item.fieldtypename));
            //        s = string.Format(s, keyPara);
            //        if (DataLayer.DataProvider.ExecuteSQL(s, false))
            //            infos.Add(new CInfo() { info = "　　拓扑基础关联Insert语句测试成功。", sql = s });
            //        else
            //            infos.Add(new CInfo() { info = "　　拓扑基础关联Insert语句测试失败。", sql = s, brush = Brushes.Red });
            //    }
            //    //-----
            //    infos.Add(new CInfo() { info = "　☆开始测试拓扑基础关联Update语句" });
            //    foreach (string ss in sql.topoReUpdate)
            //    {
            //        s = ss;
            //        foreach (var item in sql.toporelationdesces)
            //            s = s.Replace(string.Format("※{0}※", item.tablename + "." + item.fieldname), simValue(item.fieldtypename));
            //        s = string.Format(s, keyPara);
            //        if (DataLayer.DataProvider.ExecuteSQL(s, false))
            //            infos.Add(new CInfo() { info = "　　拓扑基础关联Update语句测试成功。", sql = s });
            //        else
            //            infos.Add(new CInfo() { info = "　　拓扑基础关联Update语句测试失败。", sql = s, brush = Brushes.Red });
            //    }
            //    //-----
            //    infos.Add(new CInfo() { info = "　☆开始测试拓扑基础关联Select语句" });
            //    s = string.Format(sql.topoReSelect, keyPara);
            //    dt = DataLayer.DataProvider.getDataTableFromSQL(s, false);
            //    if (dt != null)
            //        infos.Add(new CInfo() { info = "　　拓扑基础关联Select语句测试成功，查询记录" + dt.Rows.Count + "条。", sql = s });
            //    else
            //        infos.Add(new CInfo() { info = "　　拓扑基础关联Select语句测试失败。", sql = s, brush = Brushes.Red });
            //    //-----
            //    infos.Add(new CInfo() { info = "　☆开始测试拓扑基础关联Delete语句" });
            //    foreach (string ss in sql.topoReDelete)
            //    {
            //        s = string.Format(ss, keyPara);
            //        if (DataLayer.DataProvider.ExecuteSQL(s, false))
            //            infos.Add(new CInfo() { info = "　　拓扑基础关联Delete语句测试成功。", sql = s });
            //        else
            //            infos.Add(new CInfo() { info = "　　拓扑基础关联Delete语句测试失败，请手动清理数据库。", sql = s, brush = Brushes.Red });
            //    }
            //}
            //#endregion

            //#region ===== 测试拓扑基础从属语句 =====
            //infos.Add(new CInfo() { info = "★开始测试拓扑数据基础从属SQL语句" });
            //infos.Add(new CInfo() { info = "　☆开始拓扑基础从属Insert语句" });
            //if (sql.topoSuInsert.Count == 0)
            //{
            //    infos.Add(new CInfo() { info = "　　☆无拓扑基础从属Insert语句，放弃后续Update、Select、Delete语句测试。", brush = Brushes.Red });
            //}
            //else
            //{
            //    foreach (string ss in sql.topoSuInsert)
            //    {
            //        s = ss;
            //        foreach (var item in sql.toposubordinatedesces)
            //            s = s.Replace(string.Format("※{0}※", item.tablename + "." + item.fieldname), simValue(item.fieldtypename));
            //        s = string.Format(s, keyPara);
            //        if (DataLayer.DataProvider.ExecuteSQL(s, false))
            //            infos.Add(new CInfo() { info = "　　拓扑基础从属Insert语句测试成功。", sql = s });
            //        else
            //            infos.Add(new CInfo() { info = "　　拓扑基础从属Insert语句测试失败。", sql = s, brush = Brushes.Red });
            //    }
            //    //-----
            //    infos.Add(new CInfo() { info = "　☆开始测试拓扑基础从属Update语句" });
            //    foreach (string ss in sql.topoSuUpdate)
            //    {
            //        s = ss;
            //        foreach (var item in sql.toposubordinatedesces)
            //            s = s.Replace(string.Format("※{0}※", item.tablename + "." + item.fieldname), simValue(item.fieldtypename));
            //        s = string.Format(s, keyPara);
            //        if (DataLayer.DataProvider.ExecuteSQL(s, false))
            //            infos.Add(new CInfo() { info = "　　拓扑基础从属Update语句测试成功。", sql = s });
            //        else
            //            infos.Add(new CInfo() { info = "　　拓扑基础从属Update语句测试失败。", sql = s, brush = Brushes.Red });
            //    }
            //    //-----
            //    infos.Add(new CInfo() { info = "　☆开始测试拓扑基础从属Select语句" });
            //    s = string.Format(sql.topoSuSelect, keyPara);
            //    dt = DataLayer.DataProvider.getDataTableFromSQL(s, false);
            //    if (dt != null)
            //        infos.Add(new CInfo() { info = "　　拓扑基础从属Select语句测试成功，查询记录" + dt.Rows.Count + "条。", sql = s });
            //    else
            //        infos.Add(new CInfo() { info = "　　拓扑基础从属Select语句测试失败。", sql = s, brush = Brushes.Red });
            //    //-----
            //    infos.Add(new CInfo() { info = "　☆开始测试拓扑基础从属Delete语句" });
            //    foreach (string ss in sql.topoSuDelete)
            //    {
            //        s = string.Format(ss, keyPara);
            //        if (DataLayer.DataProvider.ExecuteSQL(s, false))
            //            infos.Add(new CInfo() { info = "　　拓扑基础从属Delete语句测试成功。", sql = s });
            //        else
            //            infos.Add(new CInfo() { info = "　　拓扑基础从属Delete语句测试失败，请手动清理数据库。", sql = s, brush = Brushes.Red });
            //    }

            //}
            //#endregion

            #region ===== 测试拓扑扩展关联语句 =====
            infos.Add(new CInfo() { info = "★开始测试拓扑数据扩展关联SQL语句" });
            infos.Add(new CInfo() { info = "　☆开始拓扑扩展关联Insert语句" });
            string keyPara2 = "888888888";
            if (string.IsNullOrWhiteSpace(sql.dbdesc.exTopo.topoExInsert))
            {
                infos.Add(new CInfo() { info = "　　☆无拓扑扩展关联Insert语句，放弃后续Select、Delete语句测试。", brush = Brushes.Red });
            }
            else
            {
                s = string.Format(sql.dbdesc.exTopo.topoExInsert, keyPara, keyPara2);
                if (DataLayer.DataProvider.ExecuteSQL(s, false))
                    infos.Add(new CInfo() { info = "　　拓扑扩展关联Insert语句测试成功。", sql = s });
                else
                    infos.Add(new CInfo() { info = "　　拓扑扩展关联Insert语句测试失败。", sql = s, brush = Brushes.Red });
                //-----
                infos.Add(new CInfo() { info = "　☆开始测试拓扑扩展关联Select语句" });
                s = string.Format(sql.dbdesc.exTopo.topoExSelect, keyPara);
                dt = DataLayer.DataProvider.getDataTableFromSQL(s, false);
                if (dt != null)
                    infos.Add(new CInfo() { info = "　　拓扑扩展关联Select语句测试成功，查询记录" + dt.Rows.Count + "条。", sql = s });
                else
                    infos.Add(new CInfo() { info = "　　拓扑扩展关联Select语句测试失败。", sql = s, brush = Brushes.Red });
                //-----
                infos.Add(new CInfo() { info = "　☆开始测试拓扑扩展关联Delete语句" });
                s = string.Format(sql.dbdesc.exTopo.topoExDelete, keyPara, keyPara2);
                if (DataLayer.DataProvider.ExecuteSQL(s, false))
                    infos.Add(new CInfo() { info = "　　拓扑扩展关联Delete语句测试成功。", sql = s });
                else
                    infos.Add(new CInfo() { info = "　　拓扑扩展关联Delete语句测试失败，请手动清理数据库。", sql = s, brush = Brushes.Red });

            }
            #endregion


            infos.Add(new CInfo() { info = "===== 测试完毕 =====" });
        }


        string simValue(string typename)
        {
            switch (typename)
            {
                case "float":
                    return (10.0f * rd.NextDouble()).ToString();
                case "real":
                    return (10.0f * rd.NextDouble()).ToString();
                case "nvarchar":
                    return "AAA" + rd.Next(10);
                case "varchar":
                    return "bbb" + rd.Next(10);
                case "datetime":
                    return "1999-01-02";
                case "datetime2":
                    return "2000-01-02 1:2:3";
                case "smallint":
                    return rd.Next(10).ToString();
                case "int":
                    return rd.Next(20).ToString();
                case "decimal":
                    return rd.Next(30).ToString();
                case "bit":
                    return "0";

                default:
                    return "";
            }


            return "";
        }

    }




    class CInfo
    {
        public CInfo()
        {
            brush = Brushes.Black;
            isShowBox = Visibility.Collapsed;
        }

        public string info { get; set; }

        private string _sql;
        public string sql
        {
            get { return _sql; }
            set { _sql = value; isShowBox = Visibility.Visible; }
        }

        public Brush brush { get; set; }
        public Visibility isShowBox { get; set; }
    }

}
