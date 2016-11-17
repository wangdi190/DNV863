using System;
using System.ComponentModel;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistNetLibrary.Edit
{
        public enum EParseTopoMode { 无解析需要, 北京经研院格式, 逗号分隔字串}

    class WinDBDescToolViewModel
    {
        public WinDBDescToolViewModel(string xmlFileName)
        {
            dbdesc = DBDesc.ReadFromXml(xmlFileName);
            if (dbdesc!=null)  //读取到数据库描述，设置数据源
            {
                if (string.IsNullOrWhiteSpace(dbdesc.datasourceName))
                    dbdesc.datasourceName = DataLayer.DataProvider.curDataSourceName;
                else
                    DataLayer.DataProvider.curDataSourceName = dbdesc.datasourceName;
            }

            tables = new List<TableDesc>();
            //获取数据库信息
            string sql="";
            if (DataLayer.DataProvider.databaseType == DataLayer.EDataBaseType.MsSql)
                sql = @"
select t1.name,t2.value cname from 
(select * from sysobjects t1 where xtype='U') t1 left join
(select * from sys.extended_properties   where minor_id=0 and name='MS_Description') t2 on t1.id=t2.major_id order by name
";
            else if (DataLayer.DataProvider.databaseType== DataLayer.EDataBaseType.MySql)
                sql = "SELECT TABLE_NAME as name,Table_COMMENT as cname FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = (select database() limit 1) and table_type='base table'";

            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string tname = dr.Field<string>("name");
                string cname=dr.Field<string>("cname");
                TableDesc tabledesc = new TableDesc() { tableName = tname, tableCName=cname};
                tables.Add(tabledesc);
                //SELECT * FROM sys.extended_properties WHERE major_id = OBJECT_ID ('mem_server' )
                if (DataLayer.DataProvider.databaseType == DataLayer.EDataBaseType.MsSql)
                    sql = @"
SELECT syscolumns.name,systypes.name ftype,t3.value cname 
FROM syscolumns join systypes on syscolumns.xusertype = systypes.xusertype 
	left join sys.extended_properties t3 on syscolumns.id=t3.major_id and syscolumns.colorder=t3.minor_id
WHERE syscolumns.id = object_id('{0}') order by name
";
                else if (DataLayer.DataProvider.databaseType == DataLayer.EDataBaseType.MySql)
                    sql = @"select column_name as name,data_type as ftype,column_comment as cname from information_schema.columns where table_schema=(select database() limit 1) and table_name='{0}'";

                DataTable dt2 = DataLayer.DataProvider.getDataTableFromSQL(String.Format(sql, tname));
                foreach (DataRow dr2 in dt2.Rows)
                {
                    FieldDesc fd = new FieldDesc() { fieldName = dr2.Field<string>("name"), fieldCName = dr2.Field<string>("cname"), fieldTypeName = dr2.Field<string>("ftype") };
                    if (fd.fieldCName == null) fd.fieldCName = fd.fieldName;
                    tabledesc.fields.Add(fd);
                }
            }

            //

            //获取配网类型
            objtypedesces = new List<TypeDesc>();
            objtypedesces.Add(new TypeDesc("变电站", typeof(DNSubStation), typeof(AcntSubstation), typeof(RunDataSubstation)));
            objtypedesces.Add(new TypeDesc("变电站出线", typeof(DNSubstationOutline), typeof(AcntSubstationOutline), typeof(RunDataSubstationOutline)));
            objtypedesces.Add(new TypeDesc("开关站", typeof(DNSwitchStation), typeof(AcntSwitchStation), typeof(RunDataSwitchStation)));
            objtypedesces.Add(new TypeDesc("配电室", typeof(DNSwitchHouse), typeof(AcntSwitchHouse), typeof(RunDataSwitchHouse)));
            objtypedesces.Add(new TypeDesc("主变压器", typeof(DNMainTransformer), typeof(AcntMainTransformer), typeof(RunDataMainTransformer)));
            objtypedesces.Add(new TypeDesc("两卷主变", typeof(DNMainTransformer2W), typeof(AcntMainTransformer2W), typeof(RunDataMainTransformer2W)));
            objtypedesces.Add(new TypeDesc("三卷主变", typeof(DNMainTransformer3W), typeof(AcntMainTransformer3W), typeof(RunDataMainTransformer3W)));
            objtypedesces.Add(new TypeDesc("配变", typeof(DNDistTransformer), typeof(AcntDistTransformer), typeof(RunDataDistTransformer)));
            objtypedesces.Add(new TypeDesc("柱上变", typeof(DNColumnTransformer), typeof(AcntColumnTransformer), typeof(RunDataColumnTransformer)));
            objtypedesces.Add(new TypeDesc("用户变", typeof(DNCustomerTransformer), typeof(AcntCustomerTransformer), typeof(RunDataCustomerTransformer)));
            objtypedesces.Add(new TypeDesc("节点", typeof(DNNode), typeof(AcntNode), typeof(RunDataNode)));
            objtypedesces.Add(new TypeDesc("隔离开关", typeof(DNSwitch), typeof(AcntSwitch), typeof(RunDataSwitch)));
            objtypedesces.Add(new TypeDesc("负荷开关", typeof(DNLoadSwitch), typeof(AcntLoadSwitch), typeof(RunDataLoadSwitch)));
            objtypedesces.Add(new TypeDesc("断路器", typeof(DNBreaker), typeof(AcntBreaker), typeof(RunDataBreaker)));
            objtypedesces.Add(new TypeDesc("熔断器", typeof(DNFuse), typeof(AcntFuse), typeof(RunDataFuse)));
            objtypedesces.Add(new TypeDesc("输电线路", typeof(DNACLine), typeof(AcntACLine), typeof(RunDataACLine)));
            objtypedesces.Add(new TypeDesc("导线段", typeof(DNLineSeg), typeof(AcntLineSeg), typeof(RunDataLineSeg)));
            objtypedesces.Add(new TypeDesc("电缆段", typeof(DNCableSeg), typeof(AcntCableSeg), typeof(RunDataCableSeg)));
            objtypedesces.Add(new TypeDesc("母线", typeof(DNBusBar), typeof(AcntBusBar), typeof(RunDataBusBar)));
            objtypedesces.Add(new TypeDesc("母联开关", typeof(DNBusBarSwitch), typeof(AcntBusBarSwitch), typeof(RunDataBusBarSwitch)));
            objtypedesces.Add(new TypeDesc("连接线", typeof(DNConnectivityLine), typeof(AcntConnectivityLine), typeof(RunDataConnectivityLine)));
            objtypedesces.Add(new TypeDesc("连接点", typeof(DNConnectivityNode), typeof(AcntConnectivityNode), typeof(RunDataConnectivityNode)));
            objtypedesces.Add(new TypeDesc("光伏发电", typeof(DNPVPlant), typeof(AcntPVPlant), typeof(RunDataPVPlant)));
            objtypedesces.Add(new TypeDesc("风力发电", typeof(DNWindPlant), typeof(AcntWindPlant), typeof(RunDataWindPlant)));
            objtypedesces.Add(new TypeDesc("网格区域", typeof(DNGridArea), typeof(AcntGridArea), typeof(RunDataGridArea)));
            objtypedesces.Add(new TypeDesc("分界室", typeof(DNDividingRoom), typeof(AcntDividing), typeof(RunDataDividing)));
            objtypedesces.Add(new TypeDesc("分界箱", typeof(DNDividingBox), typeof(AcntDividing), typeof(RunDataDividing)));
            objtypedesces.Add(new TypeDesc("直线杆塔", typeof(DNIntermediateSupport), typeof(AcntIntermediateSupport), typeof(RunDataIntermediateSupport)));
            objtypedesces.Add(new TypeDesc("耐张杆塔", typeof(DNStrainSupport), typeof(AcntStrainSupport), typeof(RunDataStrainSupport)));


      

            //==========载入数据操作定义

            if (dbdesc == null)
            {
                dbdesc = new DBDesc();
                dbdesc.xmlFileName = xmlFileName;
            }
            else
            {
                //更新数据库定义
                TableDesc oldtd, newtd;
                foreach (var itemsql in dbdesc.SQLS)
                {
                    //更新主表
                    if (itemsql.acntTableRelation.mainTable != null)
                    {
                        oldtd = itemsql.acntTableRelation.mainTable;
                        newtd = tables.FirstOrDefault(p => p.tableName == itemsql.acntTableRelation.mainTable.tableName).Clone();
                        if (newtd != null)
                        {
                            newtd.filter = oldtd.filter;
                            newtd.isMainTable = oldtd.isMainTable;
                            newtd.keyFieldName = oldtd.keyFieldName;
                        }
                        itemsql.acntTableRelation.mainTable = newtd;
                    }
                    //更新表集
                    BindingList<TableDesc> newlist = new BindingList<TableDesc>();
                    foreach (var itemtable in itemsql.acntTableRelation.tables)
                    {
                        if (itemtable.isMainTable && itemtable.tableName == itemsql.acntTableRelation.mainTable.tableName)
                        {
                            newlist.Add(itemsql.acntTableRelation.mainTable);
                        }
                        else
                        {
                            oldtd = itemtable;
                            newtd = tables.FirstOrDefault(p => p.tableName == itemtable.tableName).Clone();
                            if (newtd != null)
                            {
                                newtd.filter = oldtd.filter;
                                newtd.isMainTable = false;// oldtd.isMainTable;
                                newtd.keyFieldName = oldtd.keyFieldName;
                                newlist.Add(newtd);
                            }
                        }
                    }
                    itemsql.acntTableRelation.tables = newlist;
                }
            }

        }

        //数据库描述保存类
        public DBDesc dbdesc { get; set; }
        

        ///<summary>所有数据表列表</summary>
        public List<TableDesc> tables { get; set; }



        ///<summary>所有配网对象类型列表</summary>
        public List<TypeDesc> objtypedesces { get; set; }
        //List<Type> objtypes = new List<Type>();



        #region ===== 全局设置 =====
        public List<KeyValuePair<int, string>> topoparsemodes
        {
            get
            {
                List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>();
                for (int i = 0; i < Enum.GetNames(typeof(EParseTopoMode)).Count(); i++)
                {
                    result.Add(new KeyValuePair<int, string>(i, ((EParseTopoMode)i).ToString()));
                }
                return result;
            }
        }

        #endregion



        /////<summary>所有台账类型列表</summary>
        //public List<TypeDesc> acnttypedesces { get; set; }
        //List<Type> acnttypes = new List<Type>();


        /////<summary>所有运行类型列表</summary>
        //public List<TypeDesc> rundatatypedesces { get; set; }
        //List<Type> rundatatypes = new List<Type>();
    }


    public class TypeDesc
    {
        public TypeDesc(string objName,Type objType, Type acntType,Type rundataType)
        {
            objname = string.Format("{0}({1})", objName , objType.Name);

            objtype = objType;
            objtypename = objType.Name;
            objtypefullname = objType.FullName;

            acnttype = acntType;
            acnttypename = acntType.Name;
            acnttypefullname = acntType.FullName;

            rundatatype = rundataType;
            rundatatypename = rundataType.Name;
            rundatatypefullname = rundataType.FullName;

        }
        public string objname { get; set; }

        public Type objtype;
        public string objtypename {get;set;}
        public string objtypefullname { get; set; }

        public Type acnttype;
        public string acnttypename { get; set; }
        public string acnttypefullname { get; set; }

        public Type rundatatype;
        public string rundatatypename { get; set; }
        public string rundatatypefullname { get; set; }


        //object create()
        //{
        //    return System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(type.FullName);
        //}

    }

}
