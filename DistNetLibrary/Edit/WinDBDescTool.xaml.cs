using System;
using System.Data;
using System.ComponentModel;
using System.Reflection;
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
    /// WinDBDescTool.xaml 的交互逻辑
    /// </summary>
    public partial class WinDBDescTool : Window
    {
        public WinDBDescTool(string XmlFileName)
        {
            xmlFileName = XmlFileName;
            InitializeComponent();

        }

        string xmlFileName;

        WinDBDescToolViewModel viewmodel;

        private void Window_Initialized(object sender, EventArgs e)
        {
            viewmodel = new WinDBDescToolViewModel(xmlFileName);
            grdMain.DataContext = viewmodel;
            this.Title = "数据库描述设置工具——" + xmlFileName;
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(newname.Text) && !viewmodel.dbdesc.SQLS.Select(p => p.key).Contains(newname.Text))
            {
                SQL sql = new SQL() { key = newname.Text };
                sql.dbdesc = viewmodel.dbdesc;
                viewmodel.dbdesc.SQLS.Add(sql);
                viewmodel.dbdesc.selectedSQL = sql;
                lstsqls.SelectedItem = sql;
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (viewmodel.dbdesc.selectedSQL != null)
            {
                MessageBoxResult mbr = MessageBox.Show("是否删除该定义，【确定】（删除）或【取消】（取消删除）？", "删除确认！", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning);
                if (mbr == MessageBoxResult.OK)
                {
                    viewmodel.dbdesc.SQLS.Remove(viewmodel.dbdesc.selectedSQL);

                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DBDesc.SaveToXml(viewmodel.dbdesc);
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {

        }
        ///<summary>实例ID选取</summary>
        private void btnInstanceID_Click(object sender, RoutedEventArgs e)
        {
            if (lstFields.SelectedItem == null) return;
            PropertyDesc keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "InstanceID");
            if (keyProperty == null)
            {
                keyProperty = new PropertyDesc() { propertyname = "InstanceID", propertycname = "实例ID" };
                viewmodel.dbdesc.selectedSQL.keypdesces.Add(keyProperty);
            }
            keyProperty.fieldname = (lstFields.SelectedItem as FieldDesc).fieldName;
            keyProperty.fieldcname = (lstFields.SelectedItem as FieldDesc).fieldCName;
            keyProperty.fieldtypename = (lstFields.SelectedItem as FieldDesc).fieldTypeName;
            keyProperty.tablename = (lstTables.SelectedItem as TableDesc).tableName;

        }
        ///<summary>主ID选取</summary>
        private void btnID_Click(object sender, RoutedEventArgs e)
        {
            if (lstFields.SelectedItem == null) return;
            PropertyDesc keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "ID");
            if (keyProperty == null)
            {
                keyProperty = new PropertyDesc() { propertyname = "ID", propertycname = "主ID" };
                viewmodel.dbdesc.selectedSQL.keypdesces.Add(keyProperty);
            }
            keyProperty.fieldname = (lstFields.SelectedItem as FieldDesc).fieldName;
            keyProperty.fieldcname = (lstFields.SelectedItem as FieldDesc).fieldCName;
            keyProperty.fieldtypename = (lstFields.SelectedItem as FieldDesc).fieldTypeName;
            keyProperty.tablename = (lstTables.SelectedItem as TableDesc).tableName;
        }
        private void btnID2_Click(object sender, RoutedEventArgs e)
        {
            if (lstFields.SelectedItem == null) return;
            PropertyDesc keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "ID");
            if (keyProperty != null)
            {
                if (keyProperty.fieldname == (lstFields.SelectedItem as FieldDesc).fieldName)
                {
                    System.Windows.MessageBox.Show("与主ID一致，无须设置。");
                    return;
                }
            }

            keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "ID2");
            if (keyProperty == null)
            {
                keyProperty = new PropertyDesc() { propertyname = "ID2", propertycname = "次ID" };
                viewmodel.dbdesc.selectedSQL.keypdesces.Add(keyProperty);
            }
            keyProperty.fieldname = (lstFields.SelectedItem as FieldDesc).fieldName;
            keyProperty.fieldcname = (lstFields.SelectedItem as FieldDesc).fieldCName;
            keyProperty.fieldtypename = (lstFields.SelectedItem as FieldDesc).fieldTypeName;
            keyProperty.tablename = (lstTables.SelectedItem as TableDesc).tableName;
        }


        private void btnName_Click(object sender, RoutedEventArgs e)
        {
            if (lstFields.SelectedItem == null) return;
            PropertyDesc keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "name");
            if (keyProperty == null)
            {
                keyProperty = new PropertyDesc() { propertyname = "name", propertycname = "名称属性" };
                viewmodel.dbdesc.selectedSQL.keypdesces.Add(keyProperty);
            }
            keyProperty.fieldname = (lstFields.SelectedItem as FieldDesc).fieldName;
            keyProperty.fieldcname = (lstFields.SelectedItem as FieldDesc).fieldCName;
            keyProperty.fieldtypename = (lstFields.SelectedItem as FieldDesc).fieldTypeName;
            keyProperty.tablename = (lstTables.SelectedItem as TableDesc).tableName;
        }
        private void btnX_Click(object sender, RoutedEventArgs e)
        {
            if (lstFields.SelectedItem == null) return;
            PropertyDesc keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "X");
            if (keyProperty == null)
            {
                keyProperty = new PropertyDesc() { propertyname = "X", propertycname = "X属性" };
                viewmodel.dbdesc.selectedSQL.keypdesces.Add(keyProperty);
            }
            keyProperty.fieldname = (lstFields.SelectedItem as FieldDesc).fieldName;
            keyProperty.fieldcname = (lstFields.SelectedItem as FieldDesc).fieldCName;
            keyProperty.fieldtypename = (lstFields.SelectedItem as FieldDesc).fieldTypeName;
            keyProperty.tablename = (lstTables.SelectedItem as TableDesc).tableName;

            keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "points");
            if (keyProperty != null)
                viewmodel.dbdesc.selectedSQL.keypdesces.Remove(keyProperty);
            keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "shape");
            if (keyProperty != null)
                viewmodel.dbdesc.selectedSQL.keypdesces.Remove(keyProperty);

        }

        private void btnY_Click(object sender, RoutedEventArgs e)
        {
            if (lstFields.SelectedItem == null) return;
            PropertyDesc keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "Y");
            if (keyProperty == null)
            {
                keyProperty = new PropertyDesc() { propertyname = "Y", propertycname = "Y属性" };
                viewmodel.dbdesc.selectedSQL.keypdesces.Add(keyProperty);
            }
            keyProperty.fieldname = (lstFields.SelectedItem as FieldDesc).fieldName;
            keyProperty.fieldcname = (lstFields.SelectedItem as FieldDesc).fieldCName;
            keyProperty.fieldtypename = (lstFields.SelectedItem as FieldDesc).fieldTypeName;
            keyProperty.tablename = (lstTables.SelectedItem as TableDesc).tableName;

            keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "points");
            if (keyProperty != null)
                viewmodel.dbdesc.selectedSQL.keypdesces.Remove(keyProperty);
            keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "shape");
            if (keyProperty != null)
                viewmodel.dbdesc.selectedSQL.keypdesces.Remove(keyProperty);

        }

        private void btnPS_Click(object sender, RoutedEventArgs e)
        {
            if (lstFields.SelectedItem == null) return;
            PropertyDesc keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "points");
            if (keyProperty == null)
            {
                keyProperty = new PropertyDesc() { propertyname = "points", propertycname = "点集属性" };
                viewmodel.dbdesc.selectedSQL.keypdesces.Add(keyProperty);
            }
            keyProperty.fieldname = (lstFields.SelectedItem as FieldDesc).fieldName;
            keyProperty.fieldcname = (lstFields.SelectedItem as FieldDesc).fieldCName;
            keyProperty.fieldtypename = (lstFields.SelectedItem as FieldDesc).fieldTypeName;
            keyProperty.tablename = (lstTables.SelectedItem as TableDesc).tableName;

            keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "X");
            if (keyProperty != null)
                viewmodel.dbdesc.selectedSQL.keypdesces.Remove(keyProperty);
            keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "Y");
            if (keyProperty != null)
                viewmodel.dbdesc.selectedSQL.keypdesces.Remove(keyProperty);
            keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "shape");
            if (keyProperty != null)
                viewmodel.dbdesc.selectedSQL.keypdesces.Remove(keyProperty);

        }
        private void btnShape_Click(object sender, RoutedEventArgs e)
        {
            if (lstFields.SelectedItem == null) return;
            PropertyDesc keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "shape");
            if (keyProperty == null)
            {
                keyProperty = new PropertyDesc() { propertyname = "shape", propertycname = "shape属性" };
                viewmodel.dbdesc.selectedSQL.keypdesces.Add(keyProperty);
            }
            keyProperty.fieldname = (lstFields.SelectedItem as FieldDesc).fieldName;
            keyProperty.fieldcname = (lstFields.SelectedItem as FieldDesc).fieldCName;
            keyProperty.fieldtypename = (lstFields.SelectedItem as FieldDesc).fieldTypeName;
            keyProperty.tablename = (lstTables.SelectedItem as TableDesc).tableName;

            keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "X");
            if (keyProperty != null)
                viewmodel.dbdesc.selectedSQL.keypdesces.Remove(keyProperty);
            keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "Y");
            if (keyProperty != null)
                viewmodel.dbdesc.selectedSQL.keypdesces.Remove(keyProperty);
            keyProperty = viewmodel.dbdesc.selectedSQL.keypdesces.FirstOrDefault(p => p.propertyname == "points");
            if (keyProperty != null)
                viewmodel.dbdesc.selectedSQL.keypdesces.Remove(keyProperty);

        }

        ///<summary>关联台账属性与字段</summary>
        private void btnRelation_Click(object sender, RoutedEventArgs e)
        {
            if (lstFields.SelectedItem == null || lstproperties.SelectedItem == null) return;
            PropertyDesc keyProperty = lstproperties.SelectedItem as PropertyDesc;
            keyProperty.fieldname = (lstFields.SelectedItem as FieldDesc).fieldName;
            keyProperty.fieldcname = (lstFields.SelectedItem as FieldDesc).fieldCName;
            keyProperty.fieldtypename = (lstFields.SelectedItem as FieldDesc).fieldTypeName;
            keyProperty.tablename = (lstTables.SelectedItem as TableDesc).tableName;

        }
        ///<summary>解除台账属性与字段的关联</summary>
        private void btnDelRelation_Click(object sender, RoutedEventArgs e)
        {
            if (lstproperties.SelectedItem == null) return;
            PropertyDesc keyProperty = lstproperties.SelectedItem as PropertyDesc;
            keyProperty.fieldname = keyProperty.fieldcname = keyProperty.fieldtypename = keyProperty.tablename = "";
        }
        ///<summary>添加补充台账信息字段</summary>
        private void btnRelationFree_Click(object sender, RoutedEventArgs e)
        {
            if (lstFields.SelectedItem == null) return;

            FieldDesc fd = lstFields.SelectedItem as FieldDesc;
            if (viewmodel.dbdesc.selectedSQL.acntfreedesces.FirstOrDefault(p => p.fieldname == fd.fieldName) == null)
                viewmodel.dbdesc.selectedSQL.acntfreedesces.Add(new PropertyDesc() { tablename = (lstTables.SelectedItem as TableDesc).tableName, fieldname = fd.fieldName, fieldcname = fd.fieldCName, fieldtypename = fd.fieldTypeName, propertycname = string.IsNullOrWhiteSpace(fd.fieldCName) ? fd.fieldName : fd.fieldCName });

        }
        ///<summary>删除补充台账信息字段</summary>
        private void btnDelRelationFree_Click(object sender, RoutedEventArgs e)
        {
            if (lstpropertiesfree.SelectedItem == null) return;
            viewmodel.dbdesc.selectedSQL.acntfreedesces.Remove((PropertyDesc)lstpropertiesfree.SelectedItem);

        }



        ///<summary>选取与定义相关联的对象类型</summary>
        private void btnSelectObjType_Click(object sender, RoutedEventArgs e)
        {
            if (cmbObjType.SelectedItem == null) return;

            PropertyDesc newpd;
            List<string> proplist = new List<string>();
            TypeDesc ztype = cmbObjType.SelectedItem as TypeDesc;
            viewmodel.dbdesc.selectedSQL.DNObjTypeName = ztype.objtypename;
            viewmodel.dbdesc.selectedSQL.DNObjTypeFullName = ztype.objtypefullname;

            //台账
            viewmodel.dbdesc.selectedSQL.acntTypeName = ztype.acnttypename;
            viewmodel.dbdesc.selectedSQL.acntTypeFullName = ztype.acnttypefullname;
            foreach (var item in ztype.acnttype.GetProperties())
            {
                proplist.Add(item.Name);
                if (item.Name == "additionInfoes") continue;
                newpd = viewmodel.dbdesc.selectedSQL.anctdesces.FirstOrDefault(p => p.propertyname == item.Name);
                if (newpd == null)  //查找若无则新建
                {
                    newpd = new PropertyDesc();
                    viewmodel.dbdesc.selectedSQL.anctdesces.Add(newpd);
                }
                newpd.propertyname = item.Name;
                newpd.propertyTypeName = item.PropertyType.Name;
                newpd.propertyTypeFullName = item.PropertyType.FullName;
                newpd.propertycname = (Attribute.GetCustomAttribute(item, typeof(DisplayNameAttribute), false) as DisplayNameAttribute).DisplayName;
            }
            int count = 0;
            while (count < viewmodel.dbdesc.selectedSQL.anctdesces.Count)  //删除已作废台账属性关联项
            {
                if (proplist.Contains(viewmodel.dbdesc.selectedSQL.anctdesces[count].propertyname))
                    count++;
                else
                    viewmodel.dbdesc.selectedSQL.anctdesces.RemoveAt(count);
            }

            //实时运行和规划运行
            viewmodel.dbdesc.selectedSQL.rundataTypeName = ztype.rundatatypename;
            viewmodel.dbdesc.selectedSQL.rundataTypeFullName = ztype.rundatatypefullname;
            proplist.Clear();
            foreach (var item in ztype.rundatatype.GetProperties())
            {
                if (Attribute.GetCustomAttribute(item, typeof(DisplayNameAttribute), false) != null)
                {
                    proplist.Add(item.Name);
                    // 实时运行数据
                    newpd = viewmodel.dbdesc.selectedSQL.rundatadesces.FirstOrDefault(p => p.propertyname == item.Name);
                    if (newpd == null)  //查找若无则新建
                    {
                        newpd = new PropertyDesc();
                        viewmodel.dbdesc.selectedSQL.rundatadesces.Add(newpd);
                    }
                    newpd.propertyname = item.Name;
                    newpd.propertyTypeName = item.PropertyType.Name;
                    newpd.propertyTypeFullName = item.PropertyType.FullName;
                    newpd.propertycname = (Attribute.GetCustomAttribute(item, typeof(DisplayNameAttribute), false) as DisplayNameAttribute).DisplayName;

                    //规划运行数据
                    newpd = viewmodel.dbdesc.selectedSQL.planningdesces.FirstOrDefault(p => p.propertyname == item.Name);
                    if (newpd == null)  //查找若无则新建
                    {
                        newpd = new PropertyDesc();
                        viewmodel.dbdesc.selectedSQL.planningdesces.Add(newpd);
                    }
                    newpd.propertyname = item.Name;
                    newpd.propertyTypeName = item.PropertyType.Name;
                    newpd.propertyTypeFullName = item.PropertyType.FullName;
                    newpd.propertycname = (Attribute.GetCustomAttribute(item, typeof(DisplayNameAttribute), false) as DisplayNameAttribute).DisplayName;
                }
            }

            count = 0;
            while (count < viewmodel.dbdesc.selectedSQL.rundatadesces.Count)  //删除已作废实时运行属性关联项
            {
                if (proplist.Contains(viewmodel.dbdesc.selectedSQL.rundatadesces[count].propertyname))
                    count++;
                else
                    viewmodel.dbdesc.selectedSQL.rundatadesces.RemoveAt(count);
            }
            count = 0;
            while (count < viewmodel.dbdesc.selectedSQL.planningdesces.Count)  //删除已作废规划运行属性关联项
            {
                if (proplist.Contains(viewmodel.dbdesc.selectedSQL.planningdesces[count].propertyname))
                    count++;
                else
                    viewmodel.dbdesc.selectedSQL.planningdesces.RemoveAt(count);
            }


        }


        ///<summary>设置台账数据所用类</summary>
        private void btnSelectAcntType_Click(object sender, RoutedEventArgs e)
        {
            //if (cmbAcntType.SelectedItem == null) return;
            //TypeDesc acnttype = cmbAcntType.SelectedItem as TypeDesc;
            //viewmodel.dbdesc.selectedSQL.anctdesces.Clear();
            //viewmodel.dbdesc.selectedSQL.acntTypeName = acnttype.typename;
            //viewmodel.dbdesc.selectedSQL.acntTypeFullName = acnttype.typefullname;
            //foreach (var item in acnttype.type.GetProperties())
            //{
            //    viewmodel.dbdesc.selectedSQL.anctdesces.Add(new PropertyDesc()
            //    {
            //        propertyname = item.Name,
            //        propertyTypeName = item.PropertyType.Name,
            //        propertyTypeFullName = item.PropertyType.FullName,
            //        propertycname = (Attribute.GetCustomAttribute(item, typeof(DisplayNameAttribute), false) as DisplayNameAttribute).DisplayName
            //    });
            //}

        }

        ///<summary>设置运行数据所用类，同时也设置了规划所用模拟运行的数据所用的类</summary>
        private void btnSelectRundataType_Click(object sender, RoutedEventArgs e)
        {
            //if (cmbRundataType.SelectedItem == null) return;
            //TypeDesc rundatatype = cmbRundataType.SelectedItem as TypeDesc;
            //viewmodel.dbdesc.selectedSQL.rundatadesces.Clear();
            //foreach (var item in rundatatype.type.GetProperties())
            //{
            //    if (Attribute.GetCustomAttribute(item, typeof(DisplayNameAttribute), false) != null)
            //        viewmodel.dbdesc.selectedSQL.rundatadesces.Add(new PropertyDesc()
            //        {
            //            propertyname = item.Name,
            //            propertyTypeName = item.PropertyType.Name,
            //            propertyTypeFullName = item.PropertyType.FullName,
            //            propertycname = (Attribute.GetCustomAttribute(item, typeof(DisplayNameAttribute), false) as DisplayNameAttribute).DisplayName
            //        });
            //}
            ////---下为规划
            //viewmodel.dbdesc.selectedSQL.planningdesces.Clear();
            //foreach (var item in rundatatype.type.GetProperties())
            //{
            //    if (Attribute.GetCustomAttribute(item, typeof(DisplayNameAttribute), false) != null)
            //        viewmodel.dbdesc.selectedSQL.planningdesces.Add(new PropertyDesc()
            //        {
            //            propertyname = item.Name,
            //            propertyTypeName = item.PropertyType.Name,
            //            propertyTypeFullName = item.PropertyType.FullName,
            //            propertycname = (Attribute.GetCustomAttribute(item, typeof(DisplayNameAttribute), false) as DisplayNameAttribute).DisplayName
            //        });
            //}
        }

        ///<summary>关联运行属性与字段</summary>
        private void btnRundataRelation_Click(object sender, RoutedEventArgs e)
        {
            if (tabRundataFields.SelectedIndex == 0) //数据库字段关联
            {
                if (lstRundataFields.SelectedItem == null || lstrundataproperties.SelectedItem == null) return;
                PropertyDesc keyProperty = lstrundataproperties.SelectedItem as PropertyDesc;
                keyProperty.fieldname = (lstRundataFields.SelectedItem as FieldDesc).fieldName;
                keyProperty.fieldcname = (lstRundataFields.SelectedItem as FieldDesc).fieldCName;
                keyProperty.fieldtypename = (lstRundataFields.SelectedItem as FieldDesc).fieldTypeName;
                keyProperty.tablename = (lstRundataTables.SelectedItem as TableDesc).tableName;
            }
            else //模拟数据字段关联
            {
                if (lstRundataSimFields.SelectedItem == null || lstrundataproperties.SelectedItem == null) return;
                PropertyDesc keyProperty = lstrundataproperties.SelectedItem as PropertyDesc;
                keyProperty.simFieldName = (lstRundataSimFields.SelectedItem as FieldDesc).fieldName;
                keyProperty.simtypename = (lstPlanningSimFields.SelectedItem as FieldDesc).fieldTypeName;

            }
        }
        ///<summary>解除运行属性与字段的关联</summary>
        private void btnDelRundataRelation_Click(object sender, RoutedEventArgs e)
        {
            if (lstrundataproperties.SelectedItem == null) return;
            PropertyDesc keyProperty = lstrundataproperties.SelectedItem as PropertyDesc;
            keyProperty.fieldname = keyProperty.fieldcname = keyProperty.fieldtypename = keyProperty.tablename = keyProperty.simFieldName = "";
        }

        ///<summary>指定或取消直接所属设施</summary>
        private void btnPointFacility_Click(object sender, RoutedEventArgs e)
        {
            if (viewmodel.dbdesc.selectedSQL.topoBelontToFacility == null)
            {
                viewmodel.dbdesc.selectedSQL.topoBelontToFacility = new PropertyDesc()
                {
                    tablename = (lstTables.SelectedItem as TableDesc).tableName,
                    fieldname = (lstFields.SelectedItem as FieldDesc).fieldName,
                    fieldcname = (lstFields.SelectedItem as FieldDesc).fieldCName,
                    fieldtypename = (lstFields.SelectedItem as FieldDesc).fieldTypeName
                };
            }
            else
                viewmodel.dbdesc.selectedSQL.topoBelontToFacility = null;
        }
        ///<summary>指定或取消直接所属设备</summary>
        private void btnPointEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (viewmodel.dbdesc.selectedSQL.topoBelongToEquipment == null)
            {
                viewmodel.dbdesc.selectedSQL.topoBelongToEquipment = new PropertyDesc()
                {
                    tablename = (lstTables.SelectedItem as TableDesc).tableName,
                    fieldname = (lstFields.SelectedItem as FieldDesc).fieldName,
                    fieldcname = (lstFields.SelectedItem as FieldDesc).fieldCName,
                    fieldtypename = (lstFields.SelectedItem as FieldDesc).fieldTypeName
                };
            }
            else
                viewmodel.dbdesc.selectedSQL.topoBelongToEquipment = null;

        }


        private void btnTopoRelation_Click(object sender, RoutedEventArgs e)
        {
            if (lstFields.SelectedItem == null) return;
            PropertyDesc keyProperty = new PropertyDesc();
            keyProperty.fieldname = (lstFields.SelectedItem as FieldDesc).fieldName;
            keyProperty.fieldcname = (lstFields.SelectedItem as FieldDesc).fieldCName;
            keyProperty.fieldtypename = (lstFields.SelectedItem as FieldDesc).fieldTypeName;
            keyProperty.tablename = (lstTables.SelectedItem as TableDesc).tableName;
            viewmodel.dbdesc.selectedSQL.toporelationdesces.Add(keyProperty);

        }

        private void btntopoSuntain_Click(object sender, RoutedEventArgs e)
        {
            if (lstFields.SelectedItem == null) return;
            PropertyDesc keyProperty = new PropertyDesc();
            keyProperty.fieldname = (lstFields.SelectedItem as FieldDesc).fieldName;
            keyProperty.fieldcname = (lstFields.SelectedItem as FieldDesc).fieldCName;
            keyProperty.fieldtypename = (lstFields.SelectedItem as FieldDesc).fieldTypeName;
            keyProperty.tablename = (lstTables.SelectedItem as TableDesc).tableName;
            viewmodel.dbdesc.selectedSQL.toposubordinatedesces.Add(keyProperty);

        }

        private void lstsqls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewmodel.dbdesc.selectedSQL == null)
            {
                cmbObjType.SelectedItem = null;
            }
            else
            {
                cmbObjType.SelectedItem = viewmodel.objtypedesces.FirstOrDefault(p => p.objtypename == viewmodel.dbdesc.selectedSQL.DNObjTypeName);


            }
        }


        ///<summary>打开设置关键数据和台账数据所用数据表关系的窗口</summary>
        private void btnSetTableRelation_Click(object sender, RoutedEventArgs e)
        {
            WinTableRelationSetup win = new WinTableRelationSetup(viewmodel.tables, viewmodel.dbdesc.selectedSQL.acntTableRelation, WinTableRelationSetup.ETableRelationType.台账);
            win.ShowDialog();
        }

        ///<summary>打开设置运行数据所用数据表关系的窗口</summary>
        private void btnSetRundataTableRelation_Click(object sender, RoutedEventArgs e)
        {
            WinTableRelationSetup win = new WinTableRelationSetup(viewmodel.tables, viewmodel.dbdesc.selectedSQL.rundataTableRelation, WinTableRelationSetup.ETableRelationType.运行);
            win.ShowDialog();
        }

        ///<summary>打开设置扩展拓扑数据所用数据表关系的窗口</summary>
        private void btnSetTopoTableRelation_Click(object sender, RoutedEventArgs e)
        {
            WinTableRelationSetup win = new WinTableRelationSetup(viewmodel.tables, viewmodel.dbdesc.exTopo.topoTableRelation, WinTableRelationSetup.ETableRelationType.拓扑);
            win.ShowDialog();
        }

        ///<summary>打开设置规划数据所用数据表关系的窗口</summary>
        private void btnSetPlanningTableRelation_Click(object sender, RoutedEventArgs e)
        {
            WinTableRelationSetup win = new WinTableRelationSetup(viewmodel.tables, viewmodel.dbdesc.selectedSQL.planningTableRelation, WinTableRelationSetup.ETableRelationType.规划);
            win.ShowDialog();
        }

        ///<summary>生成关键属性、台账、基础拓扑相关sql语句</summary>
        private void btnGenSql_Click(object sender, RoutedEventArgs e)
        {
            TableDesc td;
            List<string> tablefieldlist = new List<string>();
            string strfields, strfrom, strwhere, strvalues;
            //========== select ==========
            SQL sql = viewmodel.dbdesc.selectedSQL;
            if (sql.acntTableRelation.mainTable == null) return;
            if (sql.keypdesces.Count == 0)
            {
                MessageBox.Show("请填写关键属性！");
                return;
            }
            //fields
            strfields = strfrom = strwhere = strvalues = "";
            foreach (var item in sql.keypdesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname)))  // 关键属性
            {
                tablefieldlist.Add(item.tablename + item.fieldname);
                strfields += string.Format(",{0}.{1}", item.tablename, item.fieldname);
            }
            foreach (var item in sql.anctdesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname)))  // 台账
                if (!tablefieldlist.Contains(item.tablename + item.fieldname))
                {
                    tablefieldlist.Add(item.tablename + item.fieldname);
                    strfields += string.Format(",{0}.{1}", item.tablename, item.fieldname);
                }
            foreach (var item in sql.acntfreedesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname)))  // 补充信息
                if (!tablefieldlist.Contains(item.tablename + item.fieldname))
                {
                    tablefieldlist.Add(item.tablename + item.fieldname);
                    strfields += string.Format(",{0}.{1}", item.tablename, item.fieldname);
                }
            foreach (var item in sql.toporelationdesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname)))  // 基础关联拓扑
                if (!tablefieldlist.Contains(item.tablename + item.fieldname))
                {
                    tablefieldlist.Add(item.tablename + item.fieldname);
                    strfields += string.Format(",{0}.{1}", item.tablename, item.fieldname);
                }
            foreach (var item in sql.toposubordinatedesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname)))  // 基础从属拓扑
                if (!tablefieldlist.Contains(item.tablename + item.fieldname))
                {
                    tablefieldlist.Add(item.tablename + item.fieldname);
                    strfields += string.Format(",{0}.{1}", item.tablename, item.fieldname);
                }
            if (sql.topoBelontToFacility != null)//直接所属设施
                if (!tablefieldlist.Contains(sql.topoBelontToFacility.tablename + sql.topoBelontToFacility.fieldname))
                {
                    tablefieldlist.Add(sql.topoBelontToFacility.tablename + sql.topoBelontToFacility.fieldname);
                    strfields += string.Format(",{0}.{1}", sql.topoBelontToFacility.tablename, sql.topoBelontToFacility.fieldname);
                }
            if (sql.topoBelongToEquipment != null)//直接所属设备
                if (!tablefieldlist.Contains(sql.topoBelongToEquipment.tablename + sql.topoBelongToEquipment.fieldname))
                    strfields += string.Format(",{0}.{1}", sql.topoBelongToEquipment.tablename, sql.topoBelongToEquipment.fieldname);

            strfields = strfields.Substring(1);
            //from
            strfrom = sql.acntTableRelation.mainTable.tableName;
            foreach (var item in sql.acntTableRelation.tableKeyPairs)
            {
                strfrom += String.Format(" left join {0} on {1}.{2}={0}.{3}", item.subTableName, item.mainTableName, item.mainKeyFieldName, item.subKeyFieldName);
            }
            //where
            strwhere = String.Format("{0}.{1}='{{0}}'", sql.acntTableRelation.mainTable.tableName, sql.acntTableRelation.mainTable.keyFieldName);
            sql.acntSelect = string.Format("select {0} from {1} where {2}", strfields, strfrom, strwhere);
            if (!string.IsNullOrWhiteSpace(sql.acntTableRelation.mainTable.filter)) //附加过滤
            {
                sql.acntSelectAll = string.Format("select {0} from {1} where {2}", strfields, strfrom, sql.acntTableRelation.mainTable.filter);
                sql.acntSelectAllID = string.Format("select {0}.{1} from {2} where {3}", sql.acntTableRelation.mainTable.tableName, sql.acntTableRelation.mainTable.keyFieldName, strfrom, sql.acntTableRelation.mainTable.filter);
            }
            else
            {
                sql.acntSelectAll = string.Format("select {0} from {1}", strfields, strfrom);
                sql.acntSelectAllID = string.Format("select {0}.{1} from {2}", sql.acntTableRelation.mainTable.tableName, sql.acntTableRelation.mainTable.keyFieldName, strfrom);
            }
            //========== insert ==========
            sql.acntInsert.Clear();
            //List<string> tablenames = sql.acntTableRelation.tableKeyPairs.Where(p => p.mainKeyFieldName == sql.acntTableRelation.mainTable.keyFieldName).GroupBy(p => p.subTableName).Select(p => p.Key).ToList();//原为仅主表关联字段为主键的才进行insert操作
            List<string> tablenames = sql.acntTableRelation.tables.Where(p => p.isMainTable || p.tableName == sql.acntTableRelation.mainTable.tableName).Select(p => p.tableName).ToList();//改为有主表属性的表
            //tablenames.Insert(0, sql.acntTableRelation.mainTable.tableName);
            foreach (string tablename in tablenames)
            {
                tablefieldlist.Clear();
                strfields = strfrom = strwhere = strvalues = "";
                foreach (var item in sql.keypdesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname) && p.tablename == tablename)) //关键属性
                {
                    tablefieldlist.Add(item.fieldname);
                    strfields += string.Format(",{0}", item.fieldname);
                    strvalues += string.Format(",'※{0}※'", item.propertyname);
                }
                foreach (var item in sql.anctdesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname) && p.tablename == tablename))  //台账
                {
                    if (!tablefieldlist.Contains(item.fieldname))
                    {
                        tablefieldlist.Add(item.fieldname);
                        strfields += string.Format(",{0}", item.fieldname);
                        strvalues += string.Format(",'※{0}※'", item.propertyname);
                    }
                }
                foreach (var item in sql.toporelationdesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname) && p.tablename == tablename))  //基础关联拓扑
                {
                    if (!tablefieldlist.Contains(item.fieldname))
                    {
                        tablefieldlist.Add(item.fieldname);
                        strfields += string.Format(",{0}", item.fieldname);
                        strvalues += string.Format(",'※{0}※'", item.fieldname);
                    }
                }
                foreach (var item in sql.toposubordinatedesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname) && p.tablename == tablename))  //基础从属拓扑
                {
                    if (!tablefieldlist.Contains(item.fieldname))
                    {
                        tablefieldlist.Add(item.fieldname);
                        strfields += string.Format(",{0}", item.fieldname);
                        strvalues += string.Format(",'※{0}※'", item.fieldname);
                    }
                }
                if (sql.topoBelontToFacility != null)//直接所属设施
                    if (!tablefieldlist.Contains(sql.topoBelontToFacility.fieldname))
                    {
                        tablefieldlist.Add(sql.topoBelontToFacility.fieldname);
                        strfields += string.Format(",{0}", sql.topoBelontToFacility.fieldname);
                        strvalues += string.Format(",'※{0}※'", sql.topoBelontToFacility.fieldname);
                    }
                if (sql.topoBelongToEquipment != null)//直接所属设备
                    if (!tablefieldlist.Contains(sql.topoBelongToEquipment.fieldname))
                    {
                        strfields += string.Format(",{0}", sql.topoBelongToEquipment.fieldname);
                        strvalues += string.Format(",'※{0}※'", sql.topoBelongToEquipment.fieldname);
                    }

                if (!string.IsNullOrWhiteSpace(strfields)) //未使用到的表，不加入sql列表
                {
                    if (tablename != sql.acntTableRelation.mainTable.tableName) //非主表，加子表主键字段
                    {
                        td = sql.acntTableRelation.tables.FirstOrDefault(p => p.tableName == tablename);
                        strfields += string.Format(",{0}", td.keyFieldName);
                        strvalues += string.Format(",'※{0}※'", sql.anctdesces.Union(sql.keypdesces).FirstOrDefault(p => p.tablename == sql.acntTableRelation.mainTable.tableName && p.fieldname == sql.acntTableRelation.mainTable.keyFieldName).propertyname);
                    }
                    strfields = strfields.Substring(1);
                    strvalues = strvalues.Substring(1);
                    sql.acntInsert.Add(string.Format("insert {0} ({1}) values ({2})", tablename, strfields, strvalues));
                }
            }
            //========== update ==========
            sql.acntUpdate.Clear();
            //tablenames = sql.acntTableRelation.tableKeyPairs.Where(p => p.mainKeyFieldName == sql.acntTableRelation.mainTable.keyFieldName).GroupBy(p => p.subTableName).Select(p => p.Key).ToList();//仅主表关联字段为主键的才进行insert操作
            //tablenames.Insert(0, sql.acntTableRelation.mainTable.tableName);
            foreach (string tablename in tablenames)
            {
                tablefieldlist.Clear();
                strfields = strfrom = strwhere = strvalues = "";
                foreach (var item in sql.keypdesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname) && p.tablename == tablename)) //关键属性
                {
                    tablefieldlist.Add(item.fieldname);
                    strvalues += string.Format(",{0}='※{1}※'", item.fieldname, item.propertyname);
                }
                foreach (var item in sql.anctdesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname) && p.tablename == tablename))  //台账
                {
                    if (!tablefieldlist.Contains(item.fieldname))
                    {
                        tablefieldlist.Add(item.fieldname);
                        strvalues += string.Format(",{0}='※{1}※'", item.fieldname, item.propertyname);
                    }
                }
                foreach (var item in sql.toporelationdesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname) && p.tablename == tablename))  //基础关联拓扑
                {
                    if (!tablefieldlist.Contains(item.fieldname))
                    {
                        tablefieldlist.Add(item.fieldname);
                        strvalues += string.Format(",{0}='※{1}※'", item.fieldname, item.fieldname);
                    }
                }
                foreach (var item in sql.toposubordinatedesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname) && p.tablename == tablename))  //基础从属拓扑
                {
                    if (!tablefieldlist.Contains(item.fieldname))
                    {
                        tablefieldlist.Add(item.fieldname);
                        strvalues += string.Format(",{0}='※{1}※'", item.fieldname, item.fieldname);
                    }
                }
                if (sql.topoBelontToFacility != null) //直接所属设施
                    if (!tablefieldlist.Contains(sql.topoBelontToFacility.fieldname))
                    {
                        tablefieldlist.Add(sql.topoBelontToFacility.fieldname);
                        strvalues += string.Format(",{0}='※{1}※'", sql.topoBelontToFacility.fieldname, sql.topoBelontToFacility.fieldname);
                    }
                if (sql.topoBelongToEquipment != null)//直接所属设备
                    if (!tablefieldlist.Contains(sql.topoBelongToEquipment.fieldname))
                        strvalues += string.Format(",{0}='※{1}※'", sql.topoBelongToEquipment.fieldname, sql.topoBelongToEquipment.fieldname);
                if (!string.IsNullOrWhiteSpace(strvalues)) //未使用到的表，不加入sql列表
                {
                    if (tablename != sql.acntTableRelation.mainTable.tableName) //非主表，加子表主键字段
                    {
                        td = sql.acntTableRelation.tables.FirstOrDefault(p => p.tableName == tablename);
                        strvalues += string.Format(",{0}='※{1}※'", td.keyFieldName, sql.anctdesces.Union(sql.keypdesces).FirstOrDefault(p => p.tablename == sql.acntTableRelation.mainTable.tableName && p.fieldname == sql.acntTableRelation.mainTable.keyFieldName).propertyname);
                    }
                    strvalues = strvalues.Substring(1);
                    td = sql.acntTableRelation.tables.FirstOrDefault(p => p.tableName == tablename);
                    sql.acntUpdate.Add(string.Format("update {0} set {1} where {2}='{{0}}'", tablename, strvalues, td.keyFieldName));
                }

            }

            //========== delete ==========
            sql.acntDelete.Clear();
            //tablenames = sql.acntTableRelation.tableKeyPairs.Where(p => p.mainKeyFieldName == sql.acntTableRelation.mainTable.keyFieldName).GroupBy(p => p.subTableName).Select(p => p.Key).ToList();//仅主表关联字段为主键的才进行insert操作
            //tablenames.Insert(0, sql.acntTableRelation.mainTable.tableName);
            foreach (string tablename in tablenames)
            {
                td = sql.acntTableRelation.tables.FirstOrDefault(p => p.tableName == tablename);
                sql.acntDelete.Add(string.Format("delete {0} where {1}='{{0}}'", tablename, td.keyFieldName));
            }

        }

        ///<summary>生成运行数据所用SQL语句</summary>
        private void btnGenRundataSql_Click(object sender, RoutedEventArgs e)
        {
            string strfields, strfrom, strwhere;
            //========== select ==========
            SQL sql = viewmodel.dbdesc.selectedSQL;
            if (sql.rundataTableRelation.mainTable == null) return;
            //fields
            strfields = strfrom = strwhere = "";
            foreach (var item in sql.rundatadesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname)))
                strfields += string.Format(",{0}.{1}", item.tablename, item.fieldname);
            strfields = strfields.Substring(1);
            //from
            strfrom = sql.rundataTableRelation.mainTable.tableName;
            foreach (var item in sql.rundataTableRelation.tableKeyPairs)
            {
                strfrom += String.Format(" left join {0} on {1}.{2}={0}.{3}", item.subTableName, item.mainTableName, item.mainKeyFieldName, item.subKeyFieldName);
            }
            //where
            strwhere = String.Format("{0}.{1}='{{0}}'", sql.rundataTableRelation.mainTable.tableName, sql.rundataTableRelation.mainTable.keyFieldName);
            sql.rundataSelect = string.Format("select {0} from {1} where {2}", strfields, strfrom, strwhere);

            string tmpadd = String.Format("{0}.{1},", sql.rundataTableRelation.mainTable.tableName, sql.rundataTableRelation.mainTable.keyFieldName);
            if (!string.IsNullOrWhiteSpace(sql.rundataTableRelation.mainTable.filter)) //附加过滤
                sql.rundataSelectAll = string.Format("select {0} from {1} where {2}", tmpadd + strfields, strfrom, sql.rundataTableRelation.mainTable.filter);
            else
                sql.rundataSelectAll = string.Format("select {0} from {1}", tmpadd + strfields, strfrom);

            if (string.IsNullOrWhiteSpace(sql.rundataTableRelation.mainTable.filter))
                sql.rundataTestSQL = string.Format("select top 1 {1} from {0}", sql.rundataTableRelation.mainTable.tableName, sql.rundataTableRelation.mainTable.keyFieldName);
            else
                sql.rundataTestSQL = string.Format("select top 1 {1} from {0} where {2}", sql.rundataTableRelation.mainTable.tableName, sql.rundataTableRelation.mainTable.keyFieldName, sql.rundataTableRelation.mainTable.filter);


        }

        ///<summary>生成规划数据所用SQL语句</summary>
        private void btnGenPlanningSql_Click(object sender, RoutedEventArgs e)
        {
            string strfields, strfrom, strwhere;
            //========== select ==========
            SQL sql = viewmodel.dbdesc.selectedSQL;
            if (sql.planningTableRelation.mainTable == null) return;
            //fields
            strfields = strfrom = strwhere = "";
            foreach (var item in sql.planningdesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname)))
                strfields += string.Format(",{0}.{1}", item.tablename, item.fieldname);
            strfields = strfields.Substring(1);
            //from
            strfrom = sql.planningTableRelation.mainTable.tableName;
            foreach (var item in sql.planningTableRelation.tableKeyPairs)
            {
                strfrom += String.Format(" left join {0} on {1}.{2}={0}.{3}", item.subTableName, item.mainTableName, item.mainKeyFieldName, item.subKeyFieldName);
            }
            //where
            strwhere = String.Format("{0}.{1}='{{1}}'", sql.planningTableRelation.mainTable.tableName, sql.planningTableRelation.mainTable.keyFieldName);
            strwhere = string.Format("{0} and ", sql.planningTableRelation.mainTable.filter)+strwhere;
            sql.planningSelect = string.Format("select {0} from {1} where {2}", strfields, strfrom, strwhere);

            string tmpadd = String.Format("{0}.{1},", sql.planningTableRelation.mainTable.tableName, sql.planningTableRelation.mainTable.keyFieldName);
            if (!string.IsNullOrWhiteSpace(sql.planningTableRelation.mainTable.filter)) //附加过滤
                sql.planningSelectAll = string.Format("select {0} from {1} where {2}", tmpadd + strfields, strfrom, sql.planningTableRelation.mainTable.filter);
            else
                sql.planningSelectAll = string.Format("select {0} from {1}", tmpadd + strfields, strfrom);

            if (string.IsNullOrWhiteSpace(sql.planningTableRelation.mainTable.filter))
                sql.planningTestSQL = string.Format("select top 1 {1} from {0}", sql.planningTableRelation.mainTable.tableName, sql.planningTableRelation.mainTable.keyFieldName);
            else
                sql.planningTestSQL = string.Format("select top 1 {1} from {0} where {2}", sql.planningTableRelation.mainTable.tableName, sql.planningTableRelation.mainTable.keyFieldName, sql.planningTableRelation.mainTable.filter);

        }

        ///<summary>生成拓扑数据所用SQL语句</summary>
        private void btnGenTopoSql_Click(object sender, RoutedEventArgs e)//zh注：考虑拓扑数据结构与数据库之间的关系有两种形式，一是行式，即主表某些特定字段指明特定关联关系，二是补充扩展关系，需另建单独数据表，建立两两关系对
        {
            //// ********** 基础关联 **********
            //TableDesc td;
            //string strfields, strfrom, strwhere, strvalues;
            ////========== select ==========
            //SQL sql = viewmodel.dbdesc.selectedSQL;
            //if (sql.topoTableRelation.mainTable != null && sql.toporelationdesces.Count > 0)
            //{
            //    //fields
            //    strfields = strfrom = strwhere = strvalues = "";
            //    foreach (var item in sql.toporelationdesces)
            //        strfields += string.Format(",{0}.{1}", item.tablename, item.fieldname);
            //    strfields = strfields.Substring(1);
            //    //from
            //    strfrom = sql.topoTableRelation.mainTable.tableName;
            //    foreach (var item in sql.topoTableRelation.tableKeyPairs)
            //    {
            //        strfrom += String.Format(" left join {0} on {1}.{2}={0}.{3}", item.subTableName, item.mainTableName, item.mainKeyFieldName, item.subKeyFieldName);
            //    }
            //    //where
            //    strwhere = String.Format("{0}.{1}='{{0}}'", sql.topoTableRelation.mainTable.tableName, sql.topoTableRelation.mainTable.keyFieldName);
            //    sql.topoReSelect = string.Format("select {0} from {1} where {2}", strfields, strfrom, strwhere);
            //    sql.topoReSelectAll = string.Format("select {0} from {1}", strfields, strfrom);
            //    //========== insert ========== 仅用于测试
            //    sql.topoReInsert.Clear();
            //    List<string> tablenames = sql.topoTableRelation.tableKeyPairs.Where(p => p.mainKeyFieldName == sql.topoTableRelation.mainTable.keyFieldName).GroupBy(p => p.subTableName).Select(p => p.Key).ToList();//仅主表关联字段为主键的才进行insert操作
            //    tablenames.Insert(0, sql.topoTableRelation.mainTable.tableName);
            //    foreach (string tablename in tablenames)
            //    {
            //        strfields = strfrom = strwhere = strvalues = "";
            //        foreach (var item in sql.toporelationdesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname) && p.tablename == tablename))
            //        {
            //            strfields += string.Format(",{0}", item.fieldname);
            //            strvalues += string.Format(",'※{0}※'", item.tablename + "." + item.fieldname);
            //        }
            //        //加主键字段
            //        {
            //            td = sql.topoTableRelation.tables.FirstOrDefault(p => p.tableName == tablename);
            //            strfields += string.Format(",{0}", td.keyFieldName);
            //            strvalues += string.Format(",'{{0}}'"); //主键值参数
            //        }

            //        strfields = strfields.Substring(1);
            //        strvalues = strvalues.Substring(1);
            //        sql.topoReInsert.Add(string.Format("insert {0} ({1}) values ({2})", tablename, strfields, strvalues));
            //    }
            //    //========== update ==========
            //    sql.topoReUpdate.Clear();
            //    foreach (string tablename in tablenames)
            //    {
            //        strfields = strfrom = strwhere = strvalues = "";
            //        foreach (var item in sql.toporelationdesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname) && p.tablename == tablename))
            //        {
            //            strvalues += string.Format(",{0}='※{1}※'", item.fieldname, item.tablename + "." + item.fieldname);
            //        }
            //        strvalues = strvalues.Substring(1);
            //        td = sql.topoTableRelation.tables.FirstOrDefault(p => p.tableName == tablename);
            //        sql.topoReUpdate.Add(string.Format("update {0} set {1} where {2}='{{0}}'", tablename, strvalues, td.keyFieldName));
            //    }

            //    //========== delete ========== 仅用于测试
            //    sql.topoReDelete.Clear();
            //    foreach (string tablename in tablenames)
            //    {
            //        td = sql.topoTableRelation.tables.FirstOrDefault(p => p.tableName == tablename);
            //        sql.topoReDelete.Add(string.Format("delete {0} where {1}='{{0}}'", tablename, td.keyFieldName));
            //    }
            //}
            ////********** 基础从属 **********
            ////========== select ==========
            //if (sql.topoTableRelation.mainTable != null && sql.toposubordinatedesces.Count > 0)
            //{
            //    //fields
            //    strfields = strfrom = strwhere = strvalues = "";
            //    foreach (var item in sql.toposubordinatedesces)
            //        strfields += string.Format(",{0}.{1}", item.tablename, item.fieldname);
            //    strfields = strfields.Substring(1);
            //    //from
            //    strfrom = sql.topoTableRelation.mainTable.tableName;
            //    foreach (var item in sql.topoTableRelation.tableKeyPairs)
            //    {
            //        strfrom += String.Format(" left join {0} on {1}.{2}={0}.{3}", item.subTableName, item.mainTableName, item.mainKeyFieldName, item.subKeyFieldName);
            //    }
            //    //where
            //    strwhere = String.Format("{0}.{1}='{{0}}'", sql.topoTableRelation.mainTable.tableName, sql.topoTableRelation.mainTable.keyFieldName);
            //    sql.topoSuSelect = string.Format("select {0} from {1} where {2}", strfields, strfrom, strwhere);
            //    sql.topoSuSelectAll = string.Format("select {0} from {1}", strfields, strfrom);
            //    //========== insert ========== 仅用于测试
            //    sql.topoSuInsert.Clear();
            //    List<string> tablenames = sql.topoTableRelation.tableKeyPairs.Where(p => p.mainKeyFieldName == sql.topoTableRelation.mainTable.keyFieldName).GroupBy(p => p.subTableName).Select(p => p.Key).ToList();//仅主表关联字段为主键的才进行insert操作
            //    tablenames.Insert(0, sql.topoTableRelation.mainTable.tableName);
            //    foreach (string tablename in tablenames)
            //    {
            //        strfields = strfrom = strwhere = strvalues = "";
            //        foreach (var item in sql.toposubordinatedesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname) && p.tablename == tablename))
            //        {
            //            strfields += string.Format(",{0}", item.fieldname);
            //            strvalues += string.Format(",'※{0}※'", item.tablename + "." + item.fieldname);
            //        }
            //        //加主键字段
            //        {
            //            td = sql.topoTableRelation.tables.FirstOrDefault(p => p.tableName == tablename);
            //            strfields += string.Format(",{0}", td.keyFieldName);
            //            strvalues += string.Format(",'{{0}}'"); //主键值参数
            //        }

            //        strfields = strfields.Substring(1);
            //        strvalues = strvalues.Substring(1);
            //        sql.topoSuInsert.Add(string.Format("insert {0} ({1}) values ({2})", tablename, strfields, strvalues));
            //    }
            //    //========== update ==========
            //    sql.topoSuUpdate.Clear();
            //    foreach (string tablename in tablenames)
            //    {
            //        strfields = strfrom = strwhere = strvalues = "";
            //        foreach (var item in sql.toposubordinatedesces.Where(p => !string.IsNullOrWhiteSpace(p.fieldname) && p.tablename == tablename))
            //        {
            //            strvalues += string.Format(",{0}='※{1}※'", item.fieldname, item.tablename + "." + item.fieldname);
            //        }
            //        strvalues = strvalues.Substring(1);
            //        td = sql.topoTableRelation.tables.FirstOrDefault(p => p.tableName == tablename);
            //        sql.topoSuUpdate.Add(string.Format("update {0} set {1} where {2}='{{0}}'", tablename, strvalues, td.keyFieldName));
            //    }

            //    //========== delete ========== 仅用于测试
            //    sql.topoSuDelete.Clear();
            //    foreach (string tablename in tablenames)
            //    {
            //        td = sql.topoTableRelation.tables.FirstOrDefault(p => p.tableName == tablename);
            //        sql.topoSuDelete.Add(string.Format("delete {0} where {1}='{{0}}'", tablename, td.keyFieldName));
            //    }
            //}
            //********** 扩展关联，扩展关联为多记录形式，与上述单记录不同 **********
            //========== select ==========
            if (viewmodel.dbdesc.exTopo.topoTableRelation.mainTable != null && viewmodel.dbdesc.exTopo.topoexpanddesces.Count > 1)
            {
                string field1, field2, extable;
                field1 = viewmodel.dbdesc.exTopo.topoexpanddesces[0].fieldname;
                field2 = viewmodel.dbdesc.exTopo.topoexpanddesces[1].fieldname;
                extable = viewmodel.dbdesc.exTopo.topoexpanddesces[0].tablename;
                string exsql = "select {2} rid from {0} where {1}='{{0}}' union select {1} rid from {0} where {2}='{{0}}'";
                viewmodel.dbdesc.exTopo.topoExSelect = string.Format(exsql, extable, field1, field2);
                viewmodel.dbdesc.exTopo.topoExSelectAll = string.Format("select {1} id1,{2} id2 from {0}", extable, field1, field2);
                //========== insert ==========
                exsql = "insert {0} ({1},{2}) values ('{{0}}','{{1}}')";
                viewmodel.dbdesc.exTopo.topoExInsert = string.Format(exsql, extable, field1, field2);
                //========== delete ==========
                exsql = "delete {0} where ({1}='{{0}}' and {2}='{{1}}') or ({1}='{{1}}' and {2}='{{0}}')";
                viewmodel.dbdesc.exTopo.topoExDelete = string.Format(exsql, extable, field1, field2);
            }

        }

        ///<summary>打开sql测试窗体进行各项sql语句测试</summary>
        private void btnTestSql_Click(object sender, RoutedEventArgs e)
        {
            WinTestSql win = new WinTestSql(viewmodel.dbdesc.selectedSQL);
            win.ShowDialog();
        }

        ///<summary>规划之运行数据关联</summary>
        private void btnPlanningRelation_Click(object sender, RoutedEventArgs e)
        {
            if (tabPlanningFields.SelectedIndex == 0) //数据库字段关联
            {
                if (lstPlanningFields.SelectedItem == null || lstplanningproperties.SelectedItem == null) return;
                PropertyDesc keyProperty = lstplanningproperties.SelectedItem as PropertyDesc;
                keyProperty.fieldname = (lstPlanningFields.SelectedItem as FieldDesc).fieldName;
                keyProperty.fieldcname = (lstPlanningFields.SelectedItem as FieldDesc).fieldCName;
                keyProperty.fieldtypename = (lstPlanningFields.SelectedItem as FieldDesc).fieldTypeName;
                keyProperty.tablename = (lstPlanningTables.SelectedItem as TableDesc).tableName;
            }
            else //模拟数据字段关联
            {
                if (lstPlanningSimFields.SelectedItem == null || lstplanningproperties.SelectedItem == null) return;
                PropertyDesc keyProperty = lstplanningproperties.SelectedItem as PropertyDesc;
                keyProperty.simFieldName = (lstPlanningSimFields.SelectedItem as FieldDesc).fieldName;
                keyProperty.simtypename = (lstPlanningSimFields.SelectedItem as FieldDesc).fieldTypeName;
            }

        }
        ///<summary>解除规划之运行数据关联</summary>
        private void btnDelPlanningRelation_Click(object sender, RoutedEventArgs e)
        {
            if (lstplanningproperties.SelectedItem == null) return;
            PropertyDesc keyProperty = lstplanningproperties.SelectedItem as PropertyDesc;
            keyProperty.fieldname = keyProperty.fieldcname = keyProperty.fieldtypename = keyProperty.tablename = keyProperty.simFieldName = "";
        }


        ///<summary>设定扩展关联拓扑数据表描述</summary>
        private void btnTopoEx_Click(object sender, RoutedEventArgs e)
        {
            if (lstTopoTables.SelectedItem == null) return;
            viewmodel.dbdesc.exTopo.topoexpanddesces.Clear();
            PropertyDesc keyProperty;
            keyProperty = new PropertyDesc();
            keyProperty.fieldname = (lstTopoFields.Items[0] as FieldDesc).fieldName;
            keyProperty.fieldcname = (lstTopoFields.Items[0] as FieldDesc).fieldCName;
            keyProperty.fieldtypename = (lstTopoFields.Items[0] as FieldDesc).fieldTypeName;
            keyProperty.tablename = (lstTopoTables.SelectedItem as TableDesc).tableName;
            viewmodel.dbdesc.exTopo.topoexpanddesces.Add(keyProperty);
            keyProperty = new PropertyDesc();
            keyProperty.fieldname = (lstTopoFields.Items[1] as FieldDesc).fieldName;
            keyProperty.fieldcname = (lstTopoFields.Items[1] as FieldDesc).fieldCName;
            keyProperty.fieldtypename = (lstTopoFields.Items[1] as FieldDesc).fieldTypeName;
            keyProperty.tablename = (lstTopoTables.SelectedItem as TableDesc).tableName;
            viewmodel.dbdesc.exTopo.topoexpanddesces.Add(keyProperty);
        }

        ///<summary>删除基础关联拓扑数据库描述</summary>
        private void btnDelTopoRelation_Click(object sender, RoutedEventArgs e)
        {
            if (lstTopoRelations.SelectedItem == null) return;
            viewmodel.dbdesc.selectedSQL.toporelationdesces.Remove(lstTopoRelations.SelectedItem as PropertyDesc);
        }

        ///<summary>删除基础从属拓扑数据库描述</summary>
        private void btnDeltopoSuntain_Click(object sender, RoutedEventArgs e)
        {
            if (lsttopoSubordinate.SelectedItem == null) return;
            viewmodel.dbdesc.selectedSQL.toposubordinatedesces.Remove(lsttopoSubordinate.SelectedItem as PropertyDesc);
        }

        #region ===== 模拟相关 =====

        ///<summary>尝试生成实时运行数据模拟数据表</summary>
        private void btnTestRundataSim_Click(object sender, RoutedEventArgs e)
        {
            string sim = viewmodel.dbdesc.selectedSQL.rundataSimAll;
            DataTable simDT = null;
            try
            {
                DataTable dtids = DataLayer.DataProvider.getDataTableFromSQL(viewmodel.dbdesc.selectedSQL.acntSelectAllID, false);
                if (dtids == null || dtids.Rows.Count == 0)
                    MessageBox.Show("模拟对象序列来源于台账页面的acntSelectAllID语句，但查询为空导致模拟失败，请检查！");
                List<string> ids = dtids.AsEnumerable().Select(p => p[0].ToString()).ToList();
                simDT = DataLayer.DataProvider.getDataTable(null, sim, ids, DataLayer.EReadMode.模拟).Value;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }

            WinShowDataTable win = new WinShowDataTable(simDT);
            win.ShowDialog();
            //重新填充模拟字段
            viewmodel.dbdesc.selectedSQL.simRunDataFields.Clear();
            foreach (DataColumn dc in simDT.Columns)
                viewmodel.dbdesc.selectedSQL.simRunDataFields.Add(new FieldDesc() { fieldName = dc.ColumnName, fieldTypeName = dc.DataType.ToString() });

        }
        ///<summary>尝试生成规划运行数据模拟数据表</summary>
        private void btnTestPlanningSim_Click(object sender, RoutedEventArgs e)
        {
            string sim = viewmodel.dbdesc.selectedSQL.planningSimAll;
            DataTable simDT = null;
            try
            {
                DataTable dtids = DataLayer.DataProvider.getDataTableFromSQL(viewmodel.dbdesc.selectedSQL.acntSelectAllID, false);
                if (dtids == null || dtids.Rows.Count == 0)
                    MessageBox.Show("模拟对象序列来源于台账页面的acntSelectAllID语句，但查询为空导致模拟失败，请检查！");
                List<string> ids = dtids.AsEnumerable().Select(p => p[0].ToString()).ToList();
                simDT = DataLayer.DataProvider.getDataTable(null, sim, ids, DataLayer.EReadMode.模拟).Value;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }

            WinShowDataTable win = new WinShowDataTable(simDT);
            win.ShowDialog();
            //重新填充模拟字段
            viewmodel.dbdesc.selectedSQL.simPlanningFields.Clear();
            foreach (DataColumn dc in simDT.Columns)
                viewmodel.dbdesc.selectedSQL.simPlanningFields.Add(new FieldDesc() { fieldName = dc.ColumnName, fieldTypeName = dc.DataType.ToString() });

        }


        private void btnSimReadme_Click(object sender, RoutedEventArgs e)
        {
            WinSimNote winsimnote = new WinSimNote();
            winsimnote.Show();
        }




        #endregion













    }
}
