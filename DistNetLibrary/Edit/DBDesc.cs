using System;
using System.Reflection;
using System.Data;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MyClassLibrary;
using WpfEarthLibrary;
using System.Xml.Serialization;
using System.Windows.Media;

namespace DistNetLibrary.Edit
{
    public enum ETopoType { 基础关联, 基础从属, 扩展关联 }

    [Serializable]
    public class DBDesc
    {
        ///<summary>保存数据库描述的xml文件名，缺省为DBDesc.xml</summary>
        public string xmlFileName { get; set; }

        public DBDesc()
        {
            SQLS = new BindingList<SQL>();
            exTopo = new EXTOPO();
        }

        public static void SaveToXml(DBDesc dbdesc)
        {
            XmlHelper.saveToXml(dbdesc.xmlFileName, dbdesc);
        }
        public static DBDesc ReadFromXml(string xmlFileName)
        {
            DBDesc dbdesc = (DBDesc)XmlHelper.readFromXml(xmlFileName, typeof(DBDesc));
            if (dbdesc == null) return null; //dbdesc = new DBDesc();
            dbdesc.xmlFileName = xmlFileName;
            if (string.IsNullOrWhiteSpace(dbdesc.datasourceName))
                dbdesc.datasourceName = DataLayer.DataProvider.curDataSourceName;
            dbdesc.setParent();

            return dbdesc;
        }

        ///<summary>数据描述使用的数据源名称</summary>
        public string datasourceName { get; set; }

        ///<summary>供用户调用的数据描述字典</summary>
        [XmlIgnore]
        public Dictionary<string, SQL> DictSQLS { get { return SQLS.ToDictionary(p => p.key); } }

        ///<summary>数据库描述列表，用户请使用DictSQLS字典</summary>
        public BindingList<SQL> SQLS { get; set; }


        ///<summary>全局的扩展拓扑数据描述</summary>
        public EXTOPO exTopo { get; set; }

        ///<summary>全局的拓扑字段数据解析模式</summary>
        public EParseTopoMode parseTopoMode { get; set; }

        ///<summary>注释</summary>
        public string note { get; set; }

        ///<summary>编辑工具所使用</summary>
        [XmlIgnore]
        public SQL selectedSQL { get; set; }

        internal void setParent()
        {
            foreach (var item in SQLS)
            {
                item.dbdesc = this;
            }
        }




    }

    [Serializable]
    public class EXTOPO : MyClassLibrary.MVVM.NotificationObject
    {
        public EXTOPO()
        {
            topoTableRelation = new TableRelation();
            topoexpanddesces = new BindingList<PropertyDesc>();
        }

        //--- 全局扩展拓扑数据表，独立的补充拓扑关系数据表
        public TableRelation topoTableRelation { get; set; }
        public BindingList<PropertyDesc> topoexpanddesces { get; set; }

        private string _topoExSelect;
        public string topoExSelect
        {
            get { return _topoExSelect; }
            set { _topoExSelect = value; RaisePropertyChanged(() => topoExSelect); }
        }
        private string _topoExSelectAll;
        public string topoExSelectAll
        {
            get { return _topoExSelectAll; }
            set { _topoExSelectAll = value; RaisePropertyChanged(() => topoExSelectAll); }
        }
        private string _topoExInsert;
        public string topoExInsert
        {
            get { return _topoExInsert; }
            set { _topoExInsert = value; RaisePropertyChanged(() => topoExInsert); }
        }
        private string _topoExDelete;
        public string topoExDelete
        {
            get { return _topoExDelete; }
            set { _topoExDelete = value; RaisePropertyChanged(() => topoExDelete); }
        }

    }

    [Serializable]
    public class SQL : MyClassLibrary.MVVM.NotificationObject
    {
        public SQL()
        {
            keypdesces = new BindingList<PropertyDesc>();
            anctdesces = new BindingList<PropertyDesc>();
            rundatadesces = new BindingList<PropertyDesc>();
            planningdesces = new BindingList<PropertyDesc>();
            toposubordinatedesces = new BindingList<PropertyDesc>();
            toporelationdesces = new BindingList<PropertyDesc>();

            acntTableRelation = new TableRelation();
            rundataTableRelation = new TableRelation();
            planningTableRelation = new TableRelation();

            acntInsert = new BindingList<string>();
            acntUpdate = new BindingList<string>();
            acntDelete = new BindingList<string>();

            simRunDataFields = new BindingList<FieldDesc>();
            simPlanningFields = new BindingList<FieldDesc>();

            acntfreedesces = new BindingList<PropertyDesc>();
            //topoReInsert = new BindingList<string>();
            //topoReUpdate = new BindingList<string>();
            //topoReDelete = new BindingList<string>();
            //topoSuInsert = new BindingList<string>();
            //topoSuUpdate = new BindingList<string>();
            //topoSuDelete = new BindingList<string>();
        }

        internal DBDesc dbdesc;

        public string key { get; set; }


        public string DNObjTypeName { get; set; }
        public string DNObjTypeFullName { get; set; }

        #region ========== 数据描述 ==========
        //===== 台账相关
        public TableRelation acntTableRelation { get; set; }

        ///<summary>无过滤条件的Select语句，适用于初始批量创建对象, 用户还可以自行在此基础上附加where过滤</summary>
        private string _acntSelect;
        public string acntSelect
        {
            get { return _acntSelect; }
            set { _acntSelect = value; RaisePropertyChanged(() => acntSelect); }
        }
        private string _acntSelectAll;
        public string acntSelectAll
        {
            get { return _acntSelectAll; }
            set { _acntSelectAll = value; RaisePropertyChanged(() => acntSelectAll); }
        }
        private string _acntSelectAllID;
        public string acntSelectAllID
        {
            get { return _acntSelectAllID; }
            set { _acntSelectAllID = value; RaisePropertyChanged(() => acntSelectAllID); }
        }
        private BindingList<string> _acntInsert;
        public BindingList<string> acntInsert
        {
            get { return _acntInsert; }
            set { _acntInsert = value; RaisePropertyChanged(() => acntInsert); }
        }
        private BindingList<string> _acntUpdate;
        public BindingList<string> acntUpdate
        {
            get { return _acntUpdate; }
            set { _acntUpdate = value; RaisePropertyChanged(() => acntUpdate); }
        }
        private BindingList<string> _acntDelete;
        public BindingList<string> acntDelete
        {
            get { return _acntDelete; }
            set { _acntDelete = value; RaisePropertyChanged(() => acntDelete); }
        }

        public string acntTypeName { get; set; }
        public string acntTypeFullName { get; set; }

        public BindingList<PropertyDesc> keypdesces { get; set; }  //关键字段描述

        //public SerializableDictionary<string, AcntPropertyDesc> dicAnct = new SerializableDictionary<string, AcntPropertyDesc>();
        public BindingList<PropertyDesc> anctdesces { get; set; }  //台账描述

        public BindingList<PropertyDesc> acntfreedesces { get; set; }  //自由字段描述
        //===== 运行相关
        public TableRelation rundataTableRelation { get; set; }
        public string rundataFilterFieldName { get; set; }

        ///<summary>无过滤条件的Select语句，适用于批量载入对象实时运行数据, 用户还可以自行在此基础上附加where过滤</summary>
        public string rundataSelectAll { get; set; }

        private string _rundataSelect;
        public string rundataSelect
        {
            get { return _rundataSelect; }
            set { _rundataSelect = value; RaisePropertyChanged(() => rundataSelect); }
        }
        public string rundataTestSQL { get; set; }  //测试用sql, 用于取得第一条记录的ID

        public string rundataTypeName { get; set; }
        public string rundataTypeFullName { get; set; }
        public BindingList<PropertyDesc> rundatadesces { get; set; }

        // 模拟
        private string _rundataSimAll;
        ///<summary>运行数据模拟语句</summary>
        public string rundataSimAll
        {
            get { return _rundataSimAll; }
            set { _rundataSimAll = value; RaisePropertyChanged(() => rundataSimAll); }
        }

        [XmlIgnore]
        internal DataTable simRunDataDataTable { get; set; }

        private BindingList<FieldDesc> _simRunDataFields;
        [XmlIgnore]
        public BindingList<FieldDesc> simRunDataFields
        {
            get { return _simRunDataFields; }
            set { _simRunDataFields = value; RaisePropertyChanged(() => simRunDataFields); }
        }


        //===== 规划相关
        public TableRelation planningTableRelation { get; set; }
        public string planningFilterFieldName { get; set; }

        ///<summary>无过滤条件的Select语句，适用于批量载入对象规划模拟运行数据, 用户还可以自行在此基础上附加where过滤</summary>
        private string _planningSelectAll;
        public string planningSelectAll
        {
            get { return _planningSelectAll; }
            set { _planningSelectAll = value; RaisePropertyChanged(() => planningSelectAll); }
        }


        private string _planningSelect;
        public string planningSelect
        {
            get { return _planningSelect; }
            set { _planningSelect = value; RaisePropertyChanged(() => planningSelect); }
        }
        public string planningTestSQL { get; set; }  //测试用sql, 用于取得第一条记录的ID

        public BindingList<PropertyDesc> planningdesces { get; set; }
        // 模拟
        private string _planningSimAll;
        ///<summary>规划运行数据模拟语句</summary>
        public string planningSimAll
        {
            get { return _planningSimAll; }
            set { _planningSimAll = value; RaisePropertyChanged(() => planningSimAll); }
        }

        [XmlIgnore]
        internal DataTable simPlanningDataTable { get; set; }

        private BindingList<FieldDesc> _simPlanningFields;
        [XmlIgnore]
        public BindingList<FieldDesc> simPlanningFields
        {
            get { return _simPlanningFields; }
            set { _simPlanningFields = value; RaisePropertyChanged(() => simPlanningFields); }
        }



        //===== 拓扑相关, 注：基础拓扑并入台账关键数据，仅扩展拓扑单独处理
        private PropertyDesc _topoBelontToFacility;
        public PropertyDesc topoBelontToFacility
        {
            get { return _topoBelontToFacility; }
            set { _topoBelontToFacility = value; RaisePropertyChanged(() => topoBelontToFacility); }
        }
        private PropertyDesc _topoBelongToEquipment;
        public PropertyDesc topoBelongToEquipment
        {
            get { return _topoBelongToEquipment; }
            set { _topoBelongToEquipment = value; RaisePropertyChanged(() => topoBelongToEquipment); }
        }


        ////--- 基础拓扑关联，主表中
        //public string topoReSelectAll { get; set; }

        //private string _topoReSelect;
        //public string topoReSelect
        //{
        //    get { return _topoReSelect; }
        //    set { _topoReSelect = value; RaisePropertyChanged(() => topoReSelect); }
        //}
        //private BindingList<string> _topoReInsert;
        //public BindingList<string> topoReInsert
        //{
        //    get { return _topoReInsert; }
        //    set { _topoReInsert = value; RaisePropertyChanged(() => topoReInsert); }
        //}

        //private BindingList<string> _topoReUpdate;
        //public BindingList<string> topoReUpdate
        //{
        //    get { return _topoReUpdate; }
        //    set { _topoReUpdate = value; RaisePropertyChanged(() => topoReUpdate); }
        //}
        //private BindingList<string> _topoReDelete;
        //public BindingList<string> topoReDelete
        //{
        //    get { return _topoReDelete; }
        //    set { _topoReDelete = value; RaisePropertyChanged(() => topoReDelete); }
        //}

        ////--- 基础拓扑从属，主表中
        //public string topoSuSelectAll { get; set; }

        //private string _topoSuSelect;
        //public string topoSuSelect
        //{
        //    get { return _topoSuSelect; }
        //    set { _topoSuSelect = value; RaisePropertyChanged(() => topoSuSelect); }
        //}
        //private BindingList<string> _topoSuInsert;
        //public BindingList<string> topoSuInsert
        //{
        //    get { return _topoSuInsert; }
        //    set { _topoSuInsert = value; RaisePropertyChanged(() => topoSuInsert); }
        //}

        //private BindingList<string> _topoSuUpdate;
        //public BindingList<string> topoSuUpdate
        //{
        //    get { return _topoSuUpdate; }
        //    set { _topoSuUpdate = value; RaisePropertyChanged(() => topoSuUpdate); }
        //}
        //private BindingList<string> _topoSuDelete;
        //public BindingList<string> topoSuDelete
        //{
        //    get { return _topoSuDelete; }
        //    set { _topoSuDelete = value; RaisePropertyChanged(() => topoSuDelete); }
        //}




        public BindingList<PropertyDesc> toporelationdesces { get; set; }
        public BindingList<PropertyDesc> toposubordinatedesces { get; set; }


        #endregion

        #region ========== 操作方法 ==========

        #region ----- 对象批量创建 -----
        ///<summary>
        ///根据dbopkey取得的数据库描述定义，批量创建对象载入到distnet中，同时读入台账数据和拓扑数据。
        ///以对象的大类名为层名
        ///若同层同ID对象已存在，则忽略
        ///addwhere为附加到selectAll的附加过滤语句
        ///layername为创建对象时可指定层名称，若为空则以对象所属大类为类名称
        ///返回创建对象列表，供附加其它设置
        ///</summary>
        public List<PowerBasicObject> batchCreateDNObjects(DistNet distnet, string addwhere = "", string layername = null)
        {
            MyBaseControls.Screen.ScreenProgress.info = string.Format("载入{0}...", this.key);

            DataLayer.DataProvider.curDataSourceName = dbdesc.datasourceName;

            List<PowerBasicObject> result = new List<PowerBasicObject>();

            PowerBasicObject obj;
            pLayer player;
            string s = acntSelectAll + " " + addwhere;
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(s);
            if (dt.Rows.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(layername))
                {
                    pLayer tmplayer = new pLayer(null);
                    obj = createDNObject(tmplayer);
                    player = distnet.addLayer((obj.busiDesc as DescData).objCategory.ToString());
                }
                else
                    player = distnet.addLayer(layername);

                //批量创建对象
                foreach (DataRow dr in dt.Rows)
                {

                    //string zid = dr.getString(this.acntTableRelation.mainTable.keyFieldName);
                    string zid = dr.getString(this.keypdesces.FirstOrDefault(p => p.propertyname == "ID").fieldname);
                    if (string.IsNullOrWhiteSpace(zid))
                    {
                        MyBaseControls.StatusBarTool.StatusBarTool.reportInfo.showInfo("存在设备ID为空的对象，这些对象未被创建，请检查数据来源！", 30);
                        MyBaseControls.LogTool.Log.addLog(string.Format("{1}存在设备ID为空的对象，这些对象未被创建。({0})", this, this.key), MyBaseControls.LogTool.ELogType.告警);
                        continue;  //若设备id为空，不创建对象
                    }

                    bool isExist = false;
                    foreach (pLayer lay in player.parent.zLayers.Values)
                    {
                        isExist = lay.pModels.ContainsKey(zid);
                        if (isExist)
                        {
                            MyBaseControls.StatusBarTool.StatusBarTool.reportInfo.showInfo("存在相同ID的对象，只创建第一个对象，请检查数据来源！", 30);
                            MyBaseControls.LogTool.Log.addLog(string.Format("{1}存在相同ID的对象，只创建第一个对象，请检查数据来源。({0})", this, this.key), MyBaseControls.LogTool.ELogType.告警);
                            break;
                        }
                    }

                    if (!isExist)  //没有同ID对象，才创建
                    {
                        obj = createDNObject(player);
                        obj.DBOPKey = key;
                        loadKeyAcnt(dr, obj);
                        player.AddObject(obj);
                        result.Add(obj);
                    }

                }

                loadExTopo(result); //载入扩展拓扑


            }

            return result;
        }

        #region ----- 关键属性、台账、基础拓扑的操作 -----
        ///<summary>
        ///从数据库载入关键属性、台账数据和基础拓扑，依据dr提供的数据载入。
        ///适用在初始批量创建对象:
        ///1. 以acntSelectAll语句获取的DataTable
        ///2. 以DataTable中的DataRow(dr)创建对象(obj)，并填入基本信息id, dbopkey
        ///3. 调用本方法填充关键属性、台账和基础拓扑
        ///</summary>
        public void loadKeyAcnt(DataRow dr, PowerBasicObject obj)
        {
            //=====处理关键属性
            //obj.id = dr.getString(this.acntTableRelation.mainTable.keyFieldName);
            obj.id = dr.getString(this.keypdesces.FirstOrDefault(p => p.propertyname == "ID").fieldname);
            if (this.keypdesces.FirstOrDefault(p => p.propertyname == "ID2") != null)
                obj.id2 = dr.getString(this.keypdesces.FirstOrDefault(p => p.propertyname == "ID2").fieldname);

            double x, y; x = y = 0;
            string ps = "";
            foreach (PropertyDesc item in keypdesces)
            {
                if (item.propertyname == "name")
                    obj.name = dr.getString(item.fieldname);
                else if (item.propertyname == "X")
                {
                    x = dr.getDouble(item.fieldname);
                    ps = (new System.Windows.Point(x, y)).ToString();
                }
                else if (item.propertyname == "Y")
                {
                    y = dr.getDouble(item.fieldname);
                    ps = (new System.Windows.Point(x, y)).ToString();
                }
                else if (item.propertyname == "points")
                {
                    ps = dr.getString(item.fieldname);
                    if (ps.IndexOf(" ")<ps.IndexOf(",")) //转换为标准格式
                    {
                        ps = ps.Replace(',', '_');
                        ps=ps.Replace(' ',',');
                        ps = ps.Replace('_', ' ');
                        ps=ps.Replace(";","");
                    }
                    else if (!ps.Contains(','))
                    {
                        ps = ps.Replace(' ', ',');
                    }
                }
                else if (item.propertyname == "shape")
                    ps = analShape(dr.getString(item.fieldname));

            }

            if (obj is pDotObject)
                (obj as pDotObject).location = ps;
            else if (obj is pPowerLine)
                (obj as pPowerLine).strPoints = ps;
            else if (obj is pArea)
                (obj as pArea).strPoints = ps;
            //=====处理台账
            if (obj.busiAccount == null)
                obj.busiAccount = createAcnt();
            else
                if (obj.busiAccount.GetType().FullName != acntTypeFullName)
                {
                    System.Windows.MessageBox.Show("已有台账类型与描述不一致, 强制使用描述类型。" + key);
                    obj.busiAccount = createAcnt();
                }

            foreach (DataColumn item in dr.Table.Columns)
            {
                PropertyDesc pd = anctdesces.FirstOrDefault(p => p.fieldname != null && p.fieldname.ToLower() == item.ColumnName.ToLower());
                if (pd != null)
                {
                    setPropertyValue(obj.busiAccount, dr, pd, DataLayer.EReadMode.数据库读取);
                }
                //补充信息
                pd = acntfreedesces.FirstOrDefault(p => p.fieldname != null && p.fieldname.ToLower() == item.ColumnName.ToLower());
                if (pd != null)
                {
                    (obj.busiAccount as AcntDataBase).additionInfoes.Add(new AdditionInfo() { name = pd.propertycname, value = dr[pd.fieldname].ToString() });
                }
            }

            //拓扑相关
            if (toporelationdesces.Count > 0 || toposubordinatedesces.Count > 0 || topoBelontToFacility != null || topoBelongToEquipment != null)
            {
                if (obj.busiTopo == null) obj.busiTopo = new TopoData(obj);
                foreach (DataColumn item in dr.Table.Columns)
                {
                    PropertyDesc pd = toporelationdesces.FirstOrDefault(p => p.fieldname != null && p.fieldname.ToLower() == item.ColumnName.ToLower());
                    if (pd != null) //添加基础关联
                    {
                        if (dbdesc.parseTopoMode == EParseTopoMode.北京经研院格式)
                        {
                            List<string> ss = parseTopoByBJJYY(dr[item].ToString());
                            foreach (string sid in ss)
                                (obj.busiTopo as TopoData).relationObjs.Add(new TopoObjDesc() { id = sid, table = pd.tablename, field = pd.fieldname });
                        }
                        else if (dbdesc.parseTopoMode== EParseTopoMode.逗号分隔字串)
                        {
                            if (!(dr[item] is DBNull))
                            {
                                string[] ss = dr[item].ToString().Split(',');
                                foreach (string sid in ss)
                                    (obj.busiTopo as TopoData).relationObjs.Add(new TopoObjDesc() { id = sid, table = pd.tablename, field = pd.fieldname });
                            }
                        }
                        else  //直接字段值为关联对象ID
                            (obj.busiTopo as TopoData).relationObjs.Add(new TopoObjDesc() { id = dr[item].ToString(), table = pd.tablename, field = pd.fieldname });
                    }
                    pd = toposubordinatedesces.FirstOrDefault(p => p.fieldname != null && p.fieldname.ToLower() == item.ColumnName.ToLower());
                    if (pd != null) //添加基础从属
                    {
                        (obj.busiTopo as TopoData).subordinateObjs.Add(new TopoObjDesc() { id = dr[item].ToString(), table = pd.tablename, field = pd.fieldname });
                    }
                    if (topoBelontToFacility != null && topoBelontToFacility.fieldname.ToLower() == item.ColumnName.ToLower())  //填写直接所属设施
                        (obj.busiTopo as TopoData).belontToFacilityID = new TopoObjDesc() { id = dr[item].ToString(), table = topoBelontToFacility.tablename, field = topoBelontToFacility.fieldname };
                    if (topoBelongToEquipment != null && topoBelongToEquipment.fieldname.ToLower() == item.ColumnName.ToLower())  //填写直接所属设备（等效设备）
                        (obj.busiTopo as TopoData).belongToEquipmentID = new TopoObjDesc() { id = dr[item].ToString(), table = topoBelongToEquipment.tablename, field = topoBelongToEquipment.fieldname };
                }
            }
        }

        ///<summary>解析北京经研院的CONNECTION关联拓扑字段</summary>
        List<string> parseTopoByBJJYY(string s)
        {
            //2;23580694:<1397383_36000000:36000000:(513626.330492,287090.556251019):-1>;23454695:<1397367_36000000:36000000:(513625.610492,287090.556250981):-1>
            List<string> list = new List<string>();

            Regex rgx = new Regex(";(\\w*|\\d*|-):<.[^<>]*>", RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(s);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                    list.Add(match.Groups[1].Value);
            }
            return list;
        }


        ///<summary>解析shape形式的坐标描述串</summary>
        string analShape(string shape)
        {
            //例2:2:0(539411.543202,287283.9925172764);(539411.543202,287283.99251);(539411.543202,287283.99250272365)
            //表示: 
            //以":"分隔,  第一位2表示类型, 1-点设备, 2-线设备, 3-多边形设备
            //第二位2表示维度, 2 - 两维, 3-三维
            //第三位 表示SRID, 
            //第四位,表示坐标点, 以括号表示一个点的坐标,上面的结构表示三个点的坐标
            string s = shape.Substring(shape.IndexOf('('));
            s = s.Replace("(", "").Replace(")", "").Replace(";", " ");
            return s;
        }

        ///<summary>从数据库载入关键属性和台账数据, 使用指定的ID，适用于id修改后的恢复或强制载入其它对象的台账</summary>
        public void loadKeyAcnt(string id, PowerBasicObject obj)
        {
            DataLayer.DataProvider.curDataSourceName = dbdesc.datasourceName;

            string s = string.Format(acntSelect, id);
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(s);
            if (dt.Rows.Count > 1) System.Windows.MessageBox.Show("读取到两条或以上记录，请检查数据库描述。" + key);
            DataRow dr = dt.Rows[0];
            loadKeyAcnt(dr, obj);
        }
        ///<summary>从数据库载入关键属性和台账数据，使用obj本身的id载入台账</summary>
        public void loadKeyAcnt(PowerBasicObject obj)
        {
            loadKeyAcnt(obj.id, obj);
        }

        ///<summary>保存关键属性和台账数据到数据库, id为where定位使用的旧值，新值通过obj.id获取，若无id记录则新增</summary>
        public void saveKeyAcnt(int instanceid, string oldid, PowerBasicObject obj)
        {
            DataLayer.DataProvider.curDataSourceName = dbdesc.datasourceName;

            string s = string.Format(acntSelect, oldid);
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(s);
            if (dt.Rows.Count == 0)   //新增
            {
                foreach (string ss in acntInsert)
                {
                    s = ss;
                    //处理关键属性
                    foreach (var item in keypdesces)
                    {
                        if (item.propertyname=="InstanceID")
                            s = s.Replace(string.Format("※{0}※", item.propertyname), instanceid.ToString());
                        else if (item.propertyname == "ID")
                            s = s.Replace(string.Format("※{0}※", item.propertyname), obj.id);
                        else if (item.propertyname == "name")
                            s = s.Replace(string.Format("※{0}※", item.propertyname), obj.name);
                        else if (item.propertyname == "points")
                        {
                            if (obj is pPowerLine)
                                s = s.Replace(string.Format("※{0}※", item.propertyname), (obj as pPowerLine).strPoints);
                            else if (obj is pArea)
                                s = s.Replace(string.Format("※{0}※", item.propertyname), (obj as pArea).strPoints);
                        }
                        else if (item.propertyname == "X")
                            s = s.Replace(string.Format("※{0}※", item.propertyname), (obj as pDotObject).center.X.ToString());
                        else if (item.propertyname == "Y")
                            s = s.Replace(string.Format("※{0}※", item.propertyname), (obj as pDotObject).center.Y.ToString());
                    }
                    //处理台账属性
                    foreach (var item in anctdesces)
                    {
                        s = s.Replace(string.Format("※{0}※", item.propertyname), getPropertyValue(obj.busiAccount, item));
                    }
                    DataLayer.DataProvider.ExecuteSQL(s);
                }
            }
            else  //更改
            {
                foreach (string ss in acntUpdate)
                {
                    s = ss;
                    //处理关键属性
                    foreach (var item in keypdesces)
                    {
                        if (item.propertyname == "ID")
                            s = s.Replace(string.Format("※{0}※", item.propertyname), obj.id);
                        else if (item.propertyname == "name")
                            s = s.Replace(string.Format("※{0}※", item.propertyname), obj.name);
                        else if (item.propertyname == "points")
                        {
                            if (obj is pPowerLine)
                                s = s.Replace(string.Format("※{0}※", item.propertyname), (obj as pPowerLine).strPoints);
                            else if (obj is pArea)
                                s = s.Replace(string.Format("※{0}※", item.propertyname), (obj as pArea).strPoints);
                        }
                        else if (item.propertyname == "X")
                            s = s.Replace(string.Format("※{0}※", item.propertyname), (obj as pDotObject).center.X.ToString());
                        else if (item.propertyname == "Y")
                            s = s.Replace(string.Format("※{0}※", item.propertyname), (obj as pDotObject).center.Y.ToString());
                    }
                    //处理台账属性
                    foreach (var item in anctdesces)
                    {
                        s = s.Replace(string.Format("※{0}※", item.propertyname), getPropertyValue(obj.busiAccount, item));
                    }

                    s = string.Format(s, oldid); //where语句之id

                    DataLayer.DataProvider.ExecuteSQL(s);
                }
            }

        }

        ///<summary>在数据库中删除关键属性和台账记录</summary>
        public void delKeyAcnt(string id)
        {
            DataLayer.DataProvider.curDataSourceName = dbdesc.datasourceName;

            foreach (string item in acntDelete)
            {
                string s = string.Format(item, id);
                DataLayer.DataProvider.ExecuteSQL(s);
            }
        }
        #endregion




        #endregion

        #region ----- 实时和规划模拟运行数据的操作 -----
        ///<summary>
        ///根据dbopkey取得的数据库描述定义，批量载入运行数据。
        ///isRealRun参数：ture载入实时运行数据，false载入规划模拟运行数据
        ///返回设置了运行数据的对象列表
        ///</summary>
        public List<PowerBasicObject> batchLoadRunData(DistNet distnet, bool isRealRun, int instanceid=0)
        {
            DataLayer.DataProvider.curDataSourceName = dbdesc.datasourceName;

            List<PowerBasicObject> result = new List<PowerBasicObject>();
            string sql = isRealRun ? this.rundataSelectAll : string.Format(this.planningSelectAll,instanceid);
            
            string sim = isRealRun ? this.rundataSimAll : this.planningSimAll;
            //若为模拟数据，需预填对象id
            KeyValuePair<DataLayer.EReadMode, DataTable> kvp;
            if (DataLayer.DataProvider.dataStatus == DataLayer.EDataStatus.模拟 && (simRunDataDataTable == null || simPlanningDataTable == null))
            {
                DataTable dtids = DataLayer.DataProvider.getDataTableFromSQL(acntSelectAllID, false);
                if (dtids == null || dtids.Rows.Count == 0)
                    System.Windows.MessageBox.Show("模拟对象序列来源于台账页面的acntSelectAllID语句，但查询为空导致模拟失败，请检查！");
                List<string> ids = dtids.AsEnumerable().Select(p => p[0].ToString()).ToList();
                kvp = DataLayer.DataProvider.getDataTable(null, sim, ids, DataLayer.EReadMode.模拟);
            }
            else
                kvp = DataLayer.DataProvider.getDataTable(sql, sim, isRealRun ? this.simRunDataDataTable : this.simPlanningDataTable);
            if (kvp.Key == DataLayer.EReadMode.模拟)
            {
                if (isRealRun)
                    this.simRunDataDataTable = kvp.Value;
                else
                    this.simPlanningDataTable = kvp.Value;
            }
            foreach (DataRow dr in kvp.Value.Rows)
            {
                string id;
                if (kvp.Key == DataLayer.EReadMode.数据库读取)
                    id = dr[(isRealRun ? rundataTableRelation : planningTableRelation).mainTable.keyFieldName].ToString();
                else
                    id = dr[0].ToString(); //模拟数据固定为第一字段为ID字段
                PowerBasicObject obj = distnet.findObj(id);
                if (obj != null)
                {
                    loadRundata(dr, obj, kvp.Key, isRealRun);
                    result.Add(obj);
                }
            }
            return result;
        }

        ///<summary>
        ///从载入实时运行数据, 适用于批量处理载入。
        ///1. 使用rundataSelectAll获取DataTable
        ///2. 调用程序应根据dr中的id字段（rundataTableRelation.mainTable.keyFieldName指明了id字段名）查找对象obj
        ///3. 调用本方法填充运行数据
        ///</summary>
        void loadRundata(DataRow dr, PowerBasicObject obj, DataLayer.EReadMode readmode, bool isRealRun)
        {
            if (obj.busiRunData == null)
                obj.busiRunData = createRundata(obj);
            else
                if (obj.busiRunData.GetType().FullName != rundataTypeFullName)
                {
                    System.Windows.MessageBox.Show("已有运行数据类型与描述不一致, 强制使用描述类型。" + key);
                    obj.busiRunData = createRundata(obj);
                }

            foreach (DataColumn item in dr.Table.Columns)
            {
                PropertyDesc pd;
                if (readmode == DataLayer.EReadMode.数据库读取)
                    pd = (isRealRun ? rundatadesces : planningdesces).FirstOrDefault(p => p.fieldname != null && p.fieldname.ToLower() == item.ColumnName.ToLower());
                else
                    pd = (isRealRun ? rundatadesces : planningdesces).FirstOrDefault(p => p.simFieldName != null && p.simFieldName.ToLower() == item.ColumnName.ToLower());
                if (pd != null)
                {
                    setPropertyValue(obj.busiRunData, dr, pd, readmode);
                }
            }
        }

        ///<summary>从数据库载入运行数据，单对象载入</summary>
        public void loadRundata(PowerBasicObject obj, bool isRealRun)
        {
            DataLayer.DataProvider.curDataSourceName = dbdesc.datasourceName;

            string s = string.Format(rundataSelect, obj.id);
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(s);
            if (dt.Rows.Count > 1) System.Windows.MessageBox.Show("读取到两条或以上记录，请检查数据库描述。" + key);
            DataRow dr = dt.Rows[0];
            loadRundata(dr, obj, DataLayer.EReadMode.数据库读取, isRealRun);
        }




        #endregion

        #region ----- 拓扑数据的操作 -----
        ///<summary>对给定的对象列表，载入扩展拓扑</summary>
        public void loadExTopo(List<PowerBasicObject> result)
        {
            DataLayer.DataProvider.curDataSourceName = dbdesc.datasourceName;

            //处理扩展拓扑
            DataTable dtex;
            if (!string.IsNullOrWhiteSpace(dbdesc.exTopo.topoExSelectAll))
            {
                string sqlex = dbdesc.exTopo.topoExSelectAll + string.Format(" where {1} in ({0}) or {2} in ({0})", acntSelectAllID, dbdesc.exTopo.topoexpanddesces[0].fieldname, dbdesc.exTopo.topoexpanddesces[1].fieldname);
                dtex = DataLayer.DataProvider.getDataTableFromSQL(sqlex);
                foreach (DataRow dr in dtex.Rows)
                {
                    string id1 = dr[0].ToString();
                    string id2 = dr[1].ToString();
                    PowerBasicObject findobj = result.FirstOrDefault(p => p.id == id1);
                    if (findobj != null)
                        if ((findobj.busiTopo as TopoData).relationObjs.FirstOrDefault(p => p.id == id2) == null)
                            (findobj.busiTopo as TopoData).relationObjs.Add(new TopoObjDesc() { id = id2, isExpand = true });
                    findobj = result.FirstOrDefault(p => p.id == id2);
                    if (findobj != null)
                        if ((findobj.busiTopo as TopoData).relationObjs.FirstOrDefault(p => p.id == id1) == null)
                            (findobj.busiTopo as TopoData).relationObjs.Add(new TopoObjDesc() { id = id1, isExpand = true });
                }

            }
        }


        #endregion






        //---------------------------------------------------------------------------------------------------
        ///<summary>创建配网对象</summary>
        PowerBasicObject createDNObject(pLayer player)
        {
            Object[] parameters = new Object[1]; // 定义构造函数需要的参数，所有参数都必须为Object
            parameters[0] = player;
            return (PowerBasicObject)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(DNObjTypeFullName, false,
               System.Reflection.BindingFlags.Default, null, parameters, null, null);

        }

        ///<summary>创建台账对象</summary>
        AcntDataBase createAcnt()
        {
            return (AcntDataBase)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(acntTypeFullName);

        }
        ///<summary>创建运行对象</summary>
        RunDataBase createRundata(PowerBasicObject parent)
        {
            Object[] parameters = new Object[1]; // 定义构造函数需要的参数，所有参数都必须为Object
            parameters[0] = parent;
            return (RunDataBase)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(rundataTypeFullName, false,
               System.Reflection.BindingFlags.Default, null, parameters, null, null);
        }
        ///<summary>创建拓扑对象</summary>
        TopoData createTopo(PowerBasicObject parent)
        {
            return new TopoData(parent);
        }

        ///<summary>设置属性值</summary>
        void setPropertyValue(object obj, DataRow dr, PropertyDesc pd, DataLayer.EReadMode readmode)
        {
            //反射设置属性值
            PropertyInfo pi = obj.GetType().GetProperty(pd.propertyname);
            string fieldtypename = readmode == DataLayer.EReadMode.模拟 ? pd.simtypename : pd.fieldtypename;
            string fieldname = readmode == DataLayer.EReadMode.模拟 ? pd.simFieldName : pd.fieldname;
            switch (pd.propertyTypeName)
            {
                case "Float":
                    pi.SetValue(obj, getValueFromDB<float>(dr, fieldtypename, fieldname), null);
                    break;
                case "Double":
                    pi.SetValue(obj, getValueFromDB<double>(dr, fieldtypename, fieldname), null);
                    break;
                case "Int32":
                    pi.SetValue(obj, getValueFromDB<int>(dr, fieldtypename, fieldname), null);
                    break;
                case "String":
                    pi.SetValue(obj, getValueFromDB<string>(dr, fieldtypename, fieldname), null);
                    break;
                case "Boolean":
                    pi.SetValue(obj, getValueFromDB<bool>(dr, fieldtypename, fieldname), null);
                    break;
                case "DateTime":
                    pi.SetValue(obj, getValueFromDB<DateTime>(dr, fieldtypename, fieldname), null);
                    break;
                case "Color":
                    pi.SetValue(obj, getValueFromDB<Color>(dr, fieldtypename, fieldname), null);
                    break;
                default:  //zh注：枚举的处理，还要检查
                    pi.SetValue(obj, getValueFromDB<int>(dr, fieldtypename, fieldname), null);
                    break;
            }


        }

        ///<summary>获取属性值</summary>
        string getPropertyValue(object obj, PropertyDesc pd)
        {
            //反射设置属性值
            PropertyInfo pi = obj.GetType().GetProperty(pd.propertyname);
            object value = pi.GetValue(obj, null);
            if (value == null)
                return "";
            else if (value is Boolean)
                return (Boolean)value ? "1" : "0";
            else if (value is Enum)  //枚举类型
            {
                return Convert.ToInt32(value).ToString();
            }
            return value.ToString();
        }

        ///<summary>从数据库读取值</summary>
        T getValueFromDB<T>(DataRow dr, string fieldtypename, string fieldname)
        {
            switch (fieldtypename)
            {
                case "System.Double":
                    return (T)(object)dr.getDouble(fieldname);
                case "System.String":
                    return (T)(object)dr.getString(fieldname);
                case "System.DateTime":
                    return (T)(object)dr.getDatetime(fieldname);
                case "System.Int32":
                    return (T)(object)dr.getInt(fieldname);

                case "float":
                    if (typeof(T) == typeof(string))
                        return (T)(object)dr.getDouble(fieldname).ToString();
                    else
                        return (T)(object)dr.getDouble(fieldname);
                case "real":
                    if (typeof(T) == typeof(int))
                        return (T)(object)(int)dr.getDouble(fieldname);
                    else if (typeof(T) == typeof(string))
                        return (T)(object)dr.getDouble(fieldname).ToString();
                    else
                        return (T)(object)dr.getDouble(fieldname);
                case "decimal":
                    if (typeof(T) == typeof(string))
                        return (T)(object)dr.getDouble(fieldname).ToString();
                    else
                        return (T)(object)dr.getDouble(fieldname);
                case "nvarchar":
                    if (typeof(T)==typeof(Color))
                        return (T)(new MyColorConverter()).ConvertFrom(dr.getString(fieldname)); 
                    else
                        return (T)(object)dr.getString(fieldname);
                case "varchar":
                    if (typeof(T) == typeof(Color))
                        return (T)(new MyColorConverter()).ConvertFrom(dr.getString(fieldname));
                    else
                        return (T)(object)dr.getString(fieldname).TrimEnd();
                case "date":
                    return (T)(object)dr.getDatetime(fieldname);
                case "datetime":
                    return (T)(object)dr.getDatetime(fieldname);
                case "datetime2":
                    return (T)(object)dr.getDatetime(fieldname);
                case "smallint":
                    if (typeof(T) == typeof(Boolean))
                        return (T)(object)(dr.getIntN1(fieldname) != 0);
                    else
                        return (T)(object)dr.getIntN1(fieldname);
                case "int":
                    if (typeof(T) == typeof(Boolean))
                        return (T)(object)(dr.getIntN1(fieldname) != 0);
                    else
                        //return (T)(object)dr.getIntN1(pd.fieldname);
                        return (T)Convert.ChangeType(dr.getIntN1(fieldname), typeof(T));
                case "bit":  //bit认为是bool型
                    return (T)(object)(dr.getBool(fieldname));

                default:
                    return default(T);
            }


            
        }



        #endregion

    }

    [Serializable]
    public class PropertyDesc : MyClassLibrary.MVVM.NotificationObject
    {
        
        private string _propertyname;
        public string propertyname
        {
            get { return _propertyname; }
            set { _propertyname = value; RaisePropertyChanged(() => propertyname); }
        }
        private string _propertycname;
        public string propertycname
        {
            get { return _propertycname; }
            set { _propertycname = value; RaisePropertyChanged(() => propertycname); }
        }
        private string _propertyTypeName;
        public string propertyTypeName
        {
            get { return _propertyTypeName; }
            set { _propertyTypeName = value; RaisePropertyChanged(() => propertyTypeName); }
        }
        private string _propertyTypeFullName;
        public string propertyTypeFullName
        {
            get { return _propertyTypeFullName; }
            set { _propertyTypeFullName = value; RaisePropertyChanged(() => propertyTypeFullName); }
        }




        private string _tablename;
        public string tablename
        {
            get { return _tablename; }
            set { _tablename = value; RaisePropertyChanged(() => tablename); }
        }
        private string _fieldname;
        public string fieldname
        {
            get { return _fieldname; }
            set { _fieldname = value; RaisePropertyChanged(() => fieldname); }
        }
        private string _fieldcname;
        public string fieldcname
        {
            get { return _fieldcname; }
            set { _fieldcname = value; RaisePropertyChanged(() => fieldcname); }
        }

        private string _fieldtypename;
        public string fieldtypename
        {
            get { return _fieldtypename; }
            set { _fieldtypename = value; RaisePropertyChanged(() => fieldtypename); }
        }

        private string _simtypename;
        public string simtypename
        {
            get { return _simtypename; }
            set { _simtypename = value; RaisePropertyChanged(() => simtypename); }
        }

        private string _simFieldName;
        public string simFieldName
        {
            get { return _simFieldName; }
            set { _simFieldName = value; RaisePropertyChanged(() => simFieldName); }
        }



        //[XmlIgnore]
        //public string properyinfo { get { return string.Format("{0}({1})[2]", propertycname, propertyname, propertyTypeName); } }

    }


    ///<summary>描述所用的表及表关系</summary>
    public class TableRelation : MyClassLibrary.MVVM.NotificationObject
    {
        public TableRelation()
        {
            tables = new BindingList<TableDesc>();
            tableKeyPairs = new BindingList<TableKeyPair>();
        }

        private TableDesc _mainTable;
        public TableDesc mainTable
        {
            get { return _mainTable; }
            set { _mainTable = value; RaisePropertyChanged(() => mainTable); }
        }

        public BindingList<TableDesc> tables { get; set; }

        public BindingList<TableKeyPair> tableKeyPairs { get; set; }
    }

    public class TableDesc : MyClassLibrary.MVVM.NotificationObject
    {
        public TableDesc()
        {
            fields = new BindingList<FieldDesc>();
        }

        private bool _isMainTable;
        ///<summary>是否是主表</summary>
        public bool isMainTable
        {
            get { return _isMainTable; }
            set { _isMainTable = value; RaisePropertyChanged(() => isMainTable); }
        }


        private string _tableName;
        public string tableName
        {
            get { return _tableName; }
            set { _tableName = value; RaisePropertyChanged(() => tableName); }
        }
        public string tableCName { get; set; }


        private string _keyFieldName;
        public string keyFieldName
        {
            get { return _keyFieldName; }
            set { _keyFieldName = value; RaisePropertyChanged(() => keyFieldName); }
        }


        private string _filter;
        public string filter
        {
            get { return _filter; }
            set { _filter = value; RaisePropertyChanged(() => filter); }
        }



        public BindingList<FieldDesc> fields { get; set; }

        ///<summary>克隆对象实例，但其中的fields仍为引用</summary>
        public TableDesc Clone()
        {
            TableDesc newinstance = new TableDesc()
            {
                tableName = this.tableName,
                isMainTable = this.isMainTable,
                filter = this.filter,
                keyFieldName = this.keyFieldName,
                tableCName = this.tableCName,
                fields = this.fields
            };
            return newinstance;
        }
    }
    public class FieldDesc
    {
        public string fieldName { get; set; }
        public string fieldCName { get; set; }
        public string fieldTypeName { get; set; }
    }


    public class TableKeyPair
    {
        public string mainTableName { get; set; }
        public string mainKeyFieldName { get; set; }
        public string subTableName { get; set; }
        public string subKeyFieldName { get; set; }
    }
}
