using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using WpfEarthLibrary;

namespace DistNetLibrary
{
    public enum ESwitchStatus { 未知 = -1, 断开 = 0, 闭合 = 1 }

    ///<summary>线路长度接口</summary>
    public interface ILength
    {
        double len { get; set; }
    }
    ///<summary>容量接口</summary>
    public interface ICapcity
    {
        double cap { get; set; }
    }

    #region ===== 基类定义 =====
    ///<summary>台账数据基类</summary>
    public abstract class AcntDataBase
    {
        public AcntDataBase()
        {
            additionInfoes = new List<AdditionInfo>();
        }
        //internal PowerBasicObject parent;


        public enum EPlanningProp { 未知 = -1, 已有 = 0, 新建 = 1, 扩建 = 2, 改造 = 3 }
        public enum EStructureMode { 未知 = -1, 户内 = 0, 户外 = 1, 半户内 = 2, 地下 = 3, 半地下 = 4 };
        public enum EBuildingMode { 未知 = -1, 城区 = 0, 县区 = 1 }
        public enum ESupplyAreaType { 未知 = -1, A加 = 0, A = 1, B = 2, C = 3, D = 4, E = 5 }
        public enum ESubStationType { 未知 = -1, 配电站 = 0, 送电站 = 1 }
        public enum EAssetsProp { 未知 = -1, 电力公司 = 0, 用户 = 1 }
        public enum ESwitchType { 未知 = -1, 断路器 = 0, 负荷开关 = 1, 隔离开关 = 2, 熔断器 = 3 }
        public enum EPDAssemble { 未知 = -1, 箱变 = 0, 柱上变 = 1, 配电室 = 2 }
        public enum EPVType { 未知 = -1, 屋顶光伏 = 0, 墙面光伏 = 1 }


        [Category("标志"), DisplayName("ID")]
        public string id { get; set; }
        [Category("标志"), DisplayName("名称")]
        public string name { get; set; }


        [Browsable(false)]
        public List<AdditionInfo> additionInfoes { get; set; }

    }

    ///<summary>附加灵活信息类</summary>
    public class AdditionInfo
    {
        [DisplayName("名称")]
        public string name { get; set; }
        [DisplayName("值")]
        public string value { get; set; }
    }

    #endregion

    #region ===== 配电类 =====
    ///<summary>配电基类</summary>
    public abstract class AcntDistBase : AcntDataBase
    {
        [Category("标志"), DisplayName("电压等级(KV)")]
        public double vl { get; set; }
        [Category("标志"), DisplayName("投运情况")]
        public EPlanningProp planningProp { get; set; }


        [Category("管理"), DisplayName("投运日期")]
        public DateTime runDate { get; set; }
        [Category("管理"), DisplayName("退运日期")]
        public DateTime retireDate { get; set; }
        [Category("管理"), DisplayName("所属责任区")]
        public string responsibilityArea { get; set; }
        [Category("管理"), DisplayName("运行编号")]
        public string runNum { get; set; }
        [Category("管理"), DisplayName("运维单位")]
        public string OAM { get; set; }
    }

    ///<summary>配电设施基类</summary>
    public abstract class AcntFacilityBase : AcntDistBase
    {
    }

    ///<summary>配电设备基类</summary>
    public abstract class AcntEqupmentBase : AcntDistBase
    {
        [Category("管理"), DisplayName("型号")]
        public string model { get; set; }
        [Category("管理"), DisplayName("出厂日期")]
        public DateTime productDate { get; set; }
        [Category("管理"), DisplayName("出厂编号")]
        public string serialNum { get; set; }
        [Category("管理"), DisplayName("资产编号")]
        public string assetsNum { get; set; }
        [Category("管理"), DisplayName("资产单位")]
        public string assetsUnit { get; set; }

    }


    #region ---------- 配电设施类 ----------

    ///<summary>变电设施运行数据基类</summary>
    public abstract class AcntTransformFacilityBase : AcntFacilityBase, ICapcity
    {
        [Category("技术参数"), DisplayName("额定容量(MW)")]
        public double cap { get; set; }
        [Category("技术参数"), DisplayName("高压侧电压等级（KV）")]
        public double hvl { get; set; }
        [Category("技术参数"), DisplayName("高压侧额定电压（KV）")]
        public double hnvl { get; set; }
        [Category("技术参数"), DisplayName("低压侧电压等级（KV）")]
        public double lvl { get; set; }
        [Category("技术参数"), DisplayName("低压侧额定电压（KV）")]
        public double lnvl { get; set; }


    }

    ///<summary>变电站台账</summary>
    public class AcntSubstation : AcntTransformFacilityBase
    {

        [Category("技术参数"), DisplayName("电压序列")]
        public string vlSerial { get; set; }
        [Category("技术参数"), DisplayName("出线数")]
        public int outlineCount { get; set; }
        [Category("技术参数"), DisplayName("出线间隔数")]
        public int outlineSpanCount { get; set; }
        [Category("技术参数"), DisplayName("高压侧母线接线形式")]
        public string HBusbarLinkMode { get; set; }
        [Category("技术参数"), DisplayName("中压侧母线接线形式")]
        public string MBusbarLinkMode { get; set; }
        [Category("技术参数"), DisplayName("低压侧母线接线形式")]
        public string LBusbarLinkMode { get; set; }
        [Category("技术参数"), DisplayName("变电容量构成")]
        public string capComposition { get; set; }
        [Category("标志"), DisplayName("无功容量(MVar)")]
        public double recap { get; set; }
        [Category("标志"), DisplayName("无功配置")]
        public string recapComposition { get; set; }


        [Category("技术参数"), DisplayName("经济容量(MW)")]
        public double economicCap { get; set; }
        //[Category("技术参数"), DisplayName("现状年最高负荷(MW)")]
        //public double maxload { get; set; }
        //[Category("技术参数"), DisplayName("是否综合自动化")]
        //public bool isAuto { get; set; }
        //[Category("技术参数"), DisplayName("高压侧间隔")]
        //public int hSpanCount { get; set; }
        //[Category("技术参数"), DisplayName("高压侧已用间隔")]
        //public int hUseSpanCount { get; set; }
        //[Category("技术参数"), DisplayName("中压侧间隔")]
        //public int mSpanCount { get; set; }
        //[Category("技术参数"), DisplayName("中压侧已用间隔")]
        //public int mUseSpanCount { get; set; }
        //[Category("技术参数"), DisplayName("低压侧间隔")]
        //public int lSpanCount { get; set; }
        //[Category("技术参数"), DisplayName("低压侧已用间隔")]
        //public int lUseSpanCount { get; set; }

        //[Category("其它"), DisplayName("是否公用")]
        //public bool isPublice { get; set; }
        //[Category("其它"), DisplayName("是否无人值守")]
        //public bool isUnmanned { get; set; }
        [Category("其它"), DisplayName("结构形式")]
        public EStructureMode structureMode { get; set; }
        [Category("其它"), DisplayName("建设形式")]
        public EBuildingMode buildingMode { get; set; }


    }

    ///<summary>配电室台账</summary>
    public class AcntSwitchHouse : AcntTransformFacilityBase
    {

        //[Category("技术参数"), DisplayName("型号")]
        //public string model { get; set; }
        //[Category("技术参数"), DisplayName("型号名称")]
        //public string modelName { get; set; }
        //[Category("技术参数"), DisplayName("是否接配变")]
        //public bool isLinkDT { get; set; }
        //[Category("技术参数"), DisplayName("是否是开闭器")]
        //public bool isSwitch { get; set; }
        //[Category("技术参数"), DisplayName("是否箱式变")]
        //public bool isBoxT { get; set; }


    }

    ///<summary>开闭站台账</summary>
    public class AcntSwitchStation : AcntTransformFacilityBase
    {
        [Category("标志"), DisplayName("所属变电站")]
        public string belongto { get; set; }

        [Category("技术参数"), DisplayName("型号")]
        public string model { get; set; }
        [Category("技术参数"), DisplayName("型号名称")]
        public string modelName { get; set; }
        [Category("技术参数"), DisplayName("电压等级(KV)")]
        public double vl { get; set; }
        [Category("技术参数"), DisplayName("进线数")]
        public int inLineCount { get; set; }
        [Category("技术参数"), DisplayName("出线数")]
        public int outLineCount { get; set; }
        [Category("技术参数"), DisplayName("出线间隔数")]
        public int outLineSpanCount { get; set; }
        [Category("技术参数"), DisplayName("剩余出线间隔数")]
        public int restOutLineSpanCount { get; set; }
        [Category("技术参数"), DisplayName("单条母线出线数")]
        public int BusbarOutLineCount { get; set; }
        [Category("技术参数"), DisplayName("是否接配变")]
        public bool isLinkDT { get; set; }


        [Category("其它"), DisplayName("是否公用")]
        public bool isPublice { get; set; }


    }
    #endregion



    #region ---------- 变压器类 ----------
    ///<summary>变压器台账基类</summary>
    public class AcntTransformBase : AcntEqupmentBase, ICapcity
    {

        [Category("标志"), DisplayName("额定容量(MW)")]
        public double cap { get; set; }
        [Category("技术参数"), DisplayName("高压侧电压等级（KV）")]
        public double hvl { get; set; }
        [Category("技术参数"), DisplayName("高压侧额定电压（KV）")]
        public double hnvl { get; set; }
        [Category("技术参数"), DisplayName("低压侧电压等级（KV）")]
        public double lvl { get; set; }
        [Category("技术参数"), DisplayName("低压侧额定电压（KV）")]
        public double lnvl { get; set; }

    }

    ///<summary>主变压器台账</summary>
    public class AcntMainTransformer : AcntTransformBase
    {
        [Category("标志"), DisplayName("所属变电站")]
        public string belongto { get; set; }


        [Category("其它"), DisplayName("是否有载调压")]
        public bool isAdjustVL { get; set; }
    }

    ///<summary>2卷变压器台账</summary>
    public class AcntMainTransformer2W : AcntTransformBase
    {

        [Category("技术参数"), DisplayName("型号")]
        public string model { get; set; }
        [Category("技术参数"), DisplayName("无功容量（MVar）")]
        public double rcap { get; set; }
        [Category("技术参数"), DisplayName("无功配置")]
        public string reactivepowerconfig { get; set; }
        [Category("技术参数"), DisplayName("电抗器（MVar）")]
        public double reactance { get; set; }
        [Category("技术参数"), DisplayName("短路电压（%）")]
        public double shortvl { get; set; }
        [Category("技术参数"), DisplayName("短路损耗（KW）")]
        public double shortloss { get; set; }
        [Category("技术参数"), DisplayName("空载电流（%）")]
        public double idlingcurrent { get; set; }
        [Category("技术参数"), DisplayName("空载损耗（KW）")]
        public double idlingloss { get; set; }



    }

    ///<summary>3卷变压器台账</summary>
    public class AcntMainTransformer3W : AcntTransformBase
    {
        [Category("技术参数"), DisplayName("型号")]
        public string model { get; set; }
        [Category("技术参数"), DisplayName("无功容量（MVar）")]
        public double rcap { get; set; }
        [Category("技术参数"), DisplayName("无功配置")]
        public string reactivepowerconfig { get; set; }
        [Category("技术参数"), DisplayName("电抗器（MVar）")]
        public double reactance { get; set; }
        [Category("技术参数"), DisplayName("中压侧电压等级（KV）")]
        public double mvl { get; set; }
        [Category("技术参数"), DisplayName("中压侧额定电压（KV）")]
        public double mnvl { get; set; }
        [Category("技术参数"), DisplayName("高低压侧短路电压（%）")]
        public double shortvlhl { get; set; }
        [Category("技术参数"), DisplayName("高低压侧短路损耗（KW）")]
        public double shortlosshl { get; set; }
        [Category("技术参数"), DisplayName("高中压侧短路电压（%）")]
        public double shortvlhm { get; set; }
        [Category("技术参数"), DisplayName("高中压侧短路损耗（KW）")]
        public double shortlosshm { get; set; }
        [Category("技术参数"), DisplayName("中低压侧短路电压（%）")]
        public double shortvlml { get; set; }
        [Category("技术参数"), DisplayName("中低压侧短路损耗（KW）")]
        public double shortlossml { get; set; }
        [Category("技术参数"), DisplayName("空载电流（%）")]
        public double idlingcurrent { get; set; }
        [Category("技术参数"), DisplayName("空载损耗（KW）")]
        public double idlingloss { get; set; }


    }

    ///<summary>配电变压器台账</summary>
    public class AcntDistTransformer : AcntTransformBase
    {
        [Category("技术参数"), DisplayName("型号")]
        public string model { get; set; }
        [Category("技术参数"), DisplayName("系列")]
        public string series { get; set; }
        [Category("技术参数"), DisplayName("无功容量（MVar）")]
        public double rcap { get; set; }
        [Category("技术参数"), DisplayName("电压等级（KV）")]
        public double vl { get; set; }
        [Category("技术参数"), DisplayName("高压侧额定电压（KV）")]
        public double hnvl { get; set; }
        [Category("技术参数"), DisplayName("低压侧额定电压（KV）")]
        public double lnvl { get; set; }
        [Category("技术参数"), DisplayName("短路电压（%）")]
        public double shortvl { get; set; }
        [Category("技术参数"), DisplayName("短路损耗（KW）")]
        public double shortloss { get; set; }
        [Category("技术参数"), DisplayName("空载电流（%）")]
        public double idlingcurrent { get; set; }
        [Category("技术参数"), DisplayName("空载损耗（KW）")]
        public double idlingloss { get; set; }
        [Category("技术参数"), DisplayName("是否标准")]
        public bool isNormal { get; set; }
        [Category("技术参数"), DisplayName("是否非晶合金")]
        public bool isNoJHJ { get; set; }
        [Category("技术参数"), DisplayName("是否高损变")]
        public bool isHighLoss { get; set; }


        [Category("其它"), DisplayName("安装形式")]
        public EPDAssemble assemble { get; set; }
        [Category("其它"), DisplayName("是否公变")]
        public bool isPublic { get; set; }
        [Category("其它"), DisplayName("用户数")]
        public int customerCount { get; set; }


    }

    ///<summary>柱上变压器台账</summary>
    public class AcntColumnTransformer : AcntTransformBase
    {

    }

    ///<summary>用户变压器台账</summary>
    public class AcntCustomerTransformer : AcntTransformBase
    {

    }

    #endregion



    #region ---------- 开关类 ----------
    ///<summary>开关台账数据基类</summary>
    public class AcntSwitchBase : AcntEqupmentBase
    {
        [Category("技术参数"), DisplayName("常态")]
        public ESwitchStatus switchStatus { get; set; }

    }

    ///<summary>隔离开关台账</summary>
    public class AcntSwitch : AcntSwitchBase
    {

        [Category("技术参数"), DisplayName("是否标准")]
        public bool isNormal { get; set; }
        [Category("技术参数"), DisplayName("是否绝缘")]
        public bool isInsulation { get; set; }

        [Category("技术参数"), DisplayName("功能类型")]
        public ESwitchType switchType { get; set; }
        [Category("技术参数"), DisplayName("介质类型")]
        public double mediaType { get; set; }



    }

    ///<summary>负荷开关台账</summary>
    public class AcntLoadSwitch : AcntSwitchBase
    {

        [Category("技术参数"), DisplayName("是否标准")]
        public bool isNormal { get; set; }
        [Category("技术参数"), DisplayName("是否绝缘")]
        public bool isInsulation { get; set; }
        [Category("技术参数"), DisplayName("是否老旧")]
        public bool isOld { get; set; }

        [Category("技术参数"), DisplayName("功能类型")]
        public ESwitchType switchType { get; set; }
        [Category("技术参数"), DisplayName("介质类型")]
        public double mediaType { get; set; }



    }

    ///<summary>断路器台账</summary>
    public class AcntBreaker : AcntSwitchBase
    {
        [Category("技术参数"), DisplayName("额定电流(A)")]
        public double ratedCurrent { get; set; }
        [Category("技术参数"), DisplayName("额定频率(HZ)")]
        public double ratedFrequency { get; set; }


    }

    ///<summary>熔断器台账</summary>
    public class AcntFuse : AcntSwitchBase
    {
        [Category("技术参数"), DisplayName("型号")]
        public string model { get; set; }
        [Category("技术参数"), DisplayName("电压等级(KV)")]
        public double vl { get; set; }
        [Category("技术参数"), DisplayName("额定电流(A)")]
        public double ratedCurrent { get; set; }




    }

    ///<summary>母联</summary>
    public class AcntBusBarSwitch : AcntSwitchBase
    {
        [Category("标志"), DisplayName("所属变电站")]
        public string belongto { get; set; }


    }

    #endregion

    #region ---------- 导线类 ----------
    ///<summary>导线类运行数据基类</summary>
    public class AcntACLineBase : AcntEqupmentBase, ILength, ICapcity
    {

        [Category("标志"), DisplayName("额定容量(MW)")]
        public double cap { get; set; }
        [Category("技术参数"), DisplayName("线路长度(KM)")]
        public double len { get; set; }

    }

    ///<summary>线路</summary>
    public class AcntACLine : AcntACLineBase
    {

        [Category("标志"), DisplayName("首端")]
        public string begininfo { get; set; }
        [Category("标志"), DisplayName("末端")]
        public string endinfo { get; set; }

        [Category("技术参数"), DisplayName("电阻(Ω)")]
        public double resistance { get; set; }
        [Category("技术参数"), DisplayName("电抗(Ω)")]
        public double reactance { get; set; }
        [Category("技术参数"), DisplayName("电纳(μS)")]
        public double susceptance { get; set; }
        [Category("技术参数"), DisplayName("电导(μS)")]
        public double conductance { get; set; }
        [Category("技术参数"), DisplayName("标准最大热稳电流(A)")]
        public double normalmaxcurrent { get; set; }

    }

    ///<summary>导线段</summary>
    public class AcntLineSeg : AcntACLineBase
    {

    }
    ///<summary>电缆段</summary>
    public class AcntCableSeg : AcntACLineBase
    {

    }
    ///<summary>馈线</summary>
    public class AcntFeeder : AcntACLineBase
    {
    }
    ///<summary>连接线台账</summary>
    public class AcntConnectivityLine : AcntACLineBase
    {


    }


    #endregion

    #region ----- 其它 -----
    ///<summary>连接点台账</summary>
    public class AcntConnectivityNode : AcntDistBase
    {


    }
    ///<summary>节点台账</summary>
    public class AcntNode : AcntDistBase
    {


    }




    ///<summary>变电站出线台账</summary>
    public class AcntSubstationOutline : AcntDistBase
    {


    }



    ///<summary>母线台账</summary>
    public class AcntBusBar : AcntDistBase
    {


    }


    ///<summary>储能装置</summary>
    public class AcntStoredEnergy : AcntEqupmentBase
    {


    }


    ///<summary>分界室箱</summary>
    public class AcntDividing : AcntEqupmentBase
    {

        [Category("技术参数"), DisplayName("额定电流(A)")]
        public double ratedCurrent { get; set; }

    }


    ///<summary>杆塔基类</summary>
    public class AcntSupportBase : AcntEqupmentBase
    {

    }
    ///<summary>直线杆塔</summary>
    public class AcntIntermediateSupport : AcntSupportBase
    {

    }
    ///<summary>耐张杆塔</summary>
    public class AcntStrainSupport : AcntSupportBase
    {

    }




    #endregion

    #endregion

    #region 区域类
    ///<summary>区域类基类</summary>
    public abstract class AcntAreaBase:AcntDataBase
    {
        [Category("标志"), DisplayName("面积")]
        public double area { get; set; }
    }

    ///<summary>行政区域</summary>
    public class AcntRegion : AcntAreaBase
    {


    }

    ///<summary>区域</summary>
    public class AcntArea : AcntAreaBase
    {
        [Category("标志"), DisplayName("所属区域")]
        public string belongto { get; set; }
        [Category("标志"), DisplayName("容积率")]
        public double rjl { get; set; }


        ///<summary>所属上级区域编号</summary>
        [Browsable(false)]
        public string belongtoBH { get; set; }
    }

    ///<summary>小区区域台账</summary>
    public class AcntSmallArea : AcntAreaBase
    {

        [Category("标志"), DisplayName("编号")]
        public string BH { get; set; }

        ///<summary>所属供电中区编号</summary>
        [Browsable(false)]
        public string belongMidAreaBH { get; set; }
    }

    ///<summary>网格区域台账</summary>
    public class AcntGridArea : AcntAreaBase
    {

        [Category("标志"), DisplayName("编号")]
        public string BH { get; set; }

        [Category("参数"), DisplayName("规划时间")]
        public DateTime planningDate { get; set; }
        [Category("参数"), DisplayName("建设程度")]
        public string buildInfo { get; set; }
        [Category("参数"), DisplayName("控高")]
        public double KG { get; set; }
        [Category("参数"), DisplayName("容积率")]
        public double rjl { get; set; }
        [Category("参数"), DisplayName("用地类别")]
        public string useType { get; set; }
        [Category("参数"), DisplayName("分类着色")]
        public Color areaColor { get; set; }

    }


    #endregion


    #region 发电侧
    ///<summary>发电厂基类</summary>
    public class AcntPlantBase : AcntDataBase
    {
        [Category("标志"), DisplayName("电压等级(KV)")]
        public double vl { get; set; }
        [Category("标志"), DisplayName("投运情况")]
        public EPlanningProp planningProp { get; set; }


        [Category("管理"), DisplayName("投运日期")]
        public DateTime runDate { get; set; }
        [Category("管理"), DisplayName("退运日期")]
        public DateTime retireDate { get; set; }


        [Category("技术参数"), DisplayName("装机容量(MW)")]
        public double cap { get; set; }


    }


    ///<summary>风电厂</summary>
    public class AcntWindPlant : AcntPlantBase
    {
        [Category("技术参数"), DisplayName("海拔高度")]
        public double height { get; set; }
        [Category("技术参数"), DisplayName("平均风速（m/s）")]
        public double windspeed { get; set; }
        [Category("技术参数"), DisplayName("平均气温(℃)")]
        public double temp { get; set; }
        [Category("技术参数"), DisplayName("风能年均可利用小时数")]
        public double useHours { get; set; }
        [Category("技术参数"), DisplayName("风能利用率")]
        public double utilization { get; set; }

    }

    ///<summary>光伏电厂</summary>
    public class AcntPVPlant : AcntPlantBase
    {
        [Category("技术参数"), DisplayName("单位面积水平年太阳能吸收量（kWh/㎡）")]
        public double UnitAreaPV { get; set; }
        [Category("技术参数"), DisplayName("日照百分率")]
        public double sunshine { get; set; }
        [Category("技术参数"), DisplayName("太阳能利用率")]
        public double utilization { get; set; }

        [Category("技术参数"), DisplayName("光伏电源类型")]
        public EPVType pvType { get; set; }

    }
    ///<summary>生物质能电厂</summary>
    public class AcntBiomassPlant : AcntPlantBase
    {


    }

    #endregion

    #region 用电侧

    ///<summary>用户类基类</summary>
    public abstract class AcntUserBase : AcntDataBase
    {
        [Category("标志"), DisplayName("电压等级(KV)")]
        public double vl { get; set; }
        [Category("标志"), DisplayName("投运情况")]
        public EPlanningProp planningProp { get; set; }


        [Category("管理"), DisplayName("投运日期")]
        public DateTime runDate { get; set; }
        [Category("管理"), DisplayName("退运日期")]
        public DateTime retireDate { get; set; }

    }

    ///<summary>客户台账</summary>
    public class AcntCustomer : AcntUserBase
    {

    }
    ///<summary>充电桩站台账</summary>
    public class AcntRechange : AcntUserBase
    {


    }


    #endregion








}
