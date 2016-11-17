using System;
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
    /// WinTableRelationSetup.xaml 的交互逻辑
    /// </summary>
    public partial class WinTableRelationSetup : Window
    {
        public enum ETableRelationType {台账,运行,拓扑,规划}
        public WinTableRelationSetup(List<TableDesc> Tables, TableRelation trelation, ETableRelationType relationtype)
        {
            tablerelationtype = relationtype;
            viewmodel = new WinTableRelationSetupViewModel(Tables,trelation);
            InitializeComponent();
            this.Title = "选取设置"+relationtype.ToString()+"所用数据表";
        }

        ETableRelationType tablerelationtype;

        WinTableRelationSetupViewModel viewmodel;

        private void Window_Initialized(object sender, EventArgs e)
        {
            grdMain.DataContext = viewmodel;
        }

        private void btnSetMainTable_Click(object sender, RoutedEventArgs e)
        {
            viewmodel.tableRelation.mainTable = lstAllTables.SelectedItem as TableDesc;
            viewmodel.tableRelation.mainTable.isMainTable = true;
            viewmodel.tableRelation.tables.Add(lstAllTables.SelectedItem as TableDesc);
        }

        private void btnAddSubTable_Click(object sender, RoutedEventArgs e)
        {
            viewmodel.tableRelation.tables.Add(lstAllTables.SelectedItem as TableDesc);
        }
    
        private void chkAsMainTable_Checked(object sender, RoutedEventArgs e)
        {
            if (lstSubTables.SelectedItem!=null)
                (lstSubTables.SelectedItem as TableDesc).isMainTable = true;
        }

        private void chkAsMainTable_Unchecked(object sender, RoutedEventArgs e)
        {
            if (lstSubTables.SelectedItem != null)
                (lstSubTables.SelectedItem as TableDesc).isMainTable = false;
        }


        private void btnDelSubTable_Click(object sender, RoutedEventArgs e)
        {
            viewmodel.tableRelation.tables.Remove(lstSubTables.SelectedItem as TableDesc);
        }

        private void btnSetMainKey_Click(object sender, RoutedEventArgs e)
        {
            viewmodel.tableRelation.mainTable.keyFieldName = (lstMainFields.SelectedItem as FieldDesc).fieldName;

            TableDesc td= viewmodel.tableRelation.tables.FirstOrDefault(p => p.tableName == viewmodel.tableRelation.mainTable.tableName);
            td.keyFieldName = viewmodel.tableRelation.mainTable.keyFieldName;

        }
      

        private void btnLink_Click(object sender, RoutedEventArgs e)
        {
            viewmodel.tableRelation.tableKeyPairs.Add(new TableKeyPair() 
            {
                mainTableName = viewmodel.tableRelation.mainTable.tableName,
                mainKeyFieldName=(lstMainFields.SelectedItem as FieldDesc).fieldName,
                subTableName=(lstSubTables.SelectedItem as TableDesc).tableName,
                subKeyFieldName=(lstSubFields.SelectedItem as FieldDesc).fieldName
            });
            (lstSubTables.SelectedItem as TableDesc).keyFieldName = (lstSubFields.SelectedItem as FieldDesc).fieldName;
        }

        private void btnDelLink_Click(object sender, RoutedEventArgs e)
        {
            viewmodel.tableRelation.tableKeyPairs.Remove(lstPair.SelectedItem as TableKeyPair);
        }

     
      



  

    }
}
