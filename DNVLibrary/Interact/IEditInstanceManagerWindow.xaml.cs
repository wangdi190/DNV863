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

namespace DNVLibrary.Interact
{
    /// <summary>
    /// IEditInstanceManagerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class IEditInstanceManagerWindow : Window
    {
        public IEditInstanceManagerWindow()
        {
            InitializeComponent();
        }


        private IEditInstanceViewModel _viewModel;
        ///<summary>实例的viewmodel</summary>
        internal IEditInstanceViewModel viewModel
        {
            get { return _viewModel; }
            set { _viewModel = value; grdMain.DataContext = value; }
        }


        private void Window_Initialized(object sender, EventArgs e)
        {
        }

        ///<summary>选择项改变</summary>
        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            viewModel.curItem = (Item)tree.SelectedItem;

        }

        ///<summary>新建实例</summary>
        private void btnNewInstance_Click(object sender, RoutedEventArgs e)
        {
            int newid = viewModel.root.maxID + 1;
            Item newitem = new Item()
            {
                id = newid,
                instanceName = "新实例",
                isProject = false,
            };
            newitem.instanceNote = viewModel.curItem == null ? "新的基础实例" : string.Format("基于{0}的派生实例", viewModel.curItem.instanceName);
            newitem.planningYear = viewModel.curItem == null ? 2016 : viewModel.curItem.planningYear;
            if (viewModel.curItem == null)
            {
                newitem.pid = null;
                newitem.parentItem = viewModel.root;
                viewModel.root.subitems.Add(newitem);
            }
            else
            {
                newitem.pid = viewModel.curItem.id;
                newitem.parentItem = viewModel.curItem;
                viewModel.curItem.subitems.Add(newitem);
            }


        }
        ///<summary>删除实例枝</summary>
        private void btnDelInstance_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.curItem == null) return;

            viewModel.curItem.delSelf(viewModel);
        }
        ///<summary>保存到数据库</summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            bool isdelete = true;
            if (viewModel.deleteItems.Count > 0)
            {
                MessageBoxResult mbr = MessageBox.Show("操作将从数据库中真正删除实例对象及其相关数据，且不可恢复，\r\n请选择【确定】（删除）或【取消】（取消删除）？", "删除确认！", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning);
                isdelete = (mbr == MessageBoxResult.OK);
            }
            if (isdelete)  //执行操作
            {
                string sql;
                int count = 0;
                //删除操作
                if (viewModel.deleteItems.Count > 0)
                {
                    // 1. 删除rundata和anal表中的将删除实例的记录
                    sql = string.Format("select * from Dic_ObjectType");
                    DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(sql);
                    List<string> datatables = new List<string>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        //if (!(dr["AccountTableName"] is DBNull))
                        //    datatables.Add(dr["AccountTableName"].ToString());
                        if (!(dr["RundataTableName"] is DBNull))
                            datatables.Add(dr["RundataTableName"].ToString());
                        if (!(dr["AnalyseDataTableName"] is DBNull))
                            datatables.Add(dr["AnalyseDataTableName"].ToString());
                    }
                    List<string> sqls = new List<string>();
                    foreach (string datatable in datatables)
                    {
                        count = 0;
                        sql = string.Format("delete from {0} ", datatable);
                        foreach (var item in viewModel.deleteItems)
                        {
                            sql += (count == 0 ? " where " : " or ") + string.Format("instanceid={0}", item.id);
                            count++;
                        }
                        sqls.Add(sql);
                    }
                    DataLayer.DataProvider.bacthExecute(sqls);
                    sqls.Clear();
                    //2.删除分析表中的将删除实例的记录
                    List<string> analtables = new List<string>() { "Anal_DestNetObject", "Anal_DistNet", "Anal_Index", "Anal_Result", "Anal_SomeObject" };//分析表名列表

                    foreach (string datatable in analtables)
                    {
                        count = 0;
                        sql = string.Format("delete from {0} ", datatable);
                        foreach (var item in viewModel.deleteItems)
                        {
                            sql += (count == 0 ? " where " : " or ") + string.Format("instanceid={0}", item.id);
                            count++;
                        }
                        sqls.Add(sql);
                    }
                    DataLayer.DataProvider.bacthExecute(sqls);
                    sqls.Clear();
                    //3.删除分类对象表中对象
                    string subsql = "select id from all_object ";  //对象id子查询
                    count = 0;
                    foreach (var item in viewModel.deleteItems)
                    {
                        subsql += (count == 0 ? " where " : " or ") + string.Format("instanceid={0}", item.id);
                        count++;
                    }
                    datatables.Clear();
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (!(dr["AccountTableName"] is DBNull))
                            datatables.Add(dr["AccountTableName"].ToString());
                    }
                    foreach (string datatable in datatables)
                    {
                        count = 0;
                        sql = string.Format("delete from {0} where id in ({1})", datatable, subsql);
                        sqls.Add(sql);
                    }
                    DataLayer.DataProvider.bacthExecute(sqls);
                    sqls.Clear();
                    //4. 删除all_object表中对象
                    sql = string.Format("delete from all_object ");
                    count = 0;
                    foreach (var item in viewModel.deleteItems)
                    {
                        sql += (count == 0 ? " where " : " or ") + string.Format("instanceid={0}", item.id);
                        count++;
                    }
                    DataLayer.DataProvider.ExecuteSQL(sql);
                    //5.最后删除实例
                    sql = string.Format("delete from all_instance ");
                    count = 0;
                    foreach (var item in viewModel.deleteItems)
                    {
                        sql += (count == 0 ? " where " : " or ") + string.Format("id={0}", item.id);
                        count++;
                    }
                    DataLayer.DataProvider.ExecuteSQL(sql);

                    viewModel.deleteItems.Clear();
                }
                


                //修改和新增操作
                foreach (var item in viewModel.root.allitems)
                {
                    if (item.isDataBase)  //更新操作
                    {
                        sql = string.Format("update all_instance set instancename='{1}', note='{2}' ,year={3} , isproject={4} where id={0}", item.id, item.instanceName, item.instanceNote, item.planningYear, item.isProject ? 1 : 0);
                        DataLayer.DataProvider.ExecuteSQL(sql);
                    }
                    else  //新增操作
                    {
                        sql = string.Format("insert all_instance (id,parentid,instancename,note,year,isproject) values ({0},{1},'{2}','{3}',{4},{5})", item.id, item.pid == null ? "null" : item.pid.ToString(), item.instanceName, item.instanceNote, item.planningYear, item.isProject ? 1 : 0);
                        DataLayer.DataProvider.ExecuteSQL(sql);
                        item.isDataBase = true;
                    }
                }

                MessageBox.Show("数据库更新完毕！");
            }




        }
        ///<summary>退出</summary>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

     
    }
}
