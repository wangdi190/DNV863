using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;


namespace DistNetLibrary
{
    public enum EObjectScope { 无, 发电侧, 输配电侧, 用电侧 }
    public enum EObjectType { 无, 变电站, 开关站, 箱式站, 配电室, 两卷主变, 三卷主变, 配变,柱上变,用户变, 输电线路, 导线段, 馈线, 变电站出线, 分支箱,电缆,电缆段, 光缆, 光缆站点, 环网柜, 隔离开关,负荷开关, 连接点, 连接线, 母线, 母联, 节点, 行政区域, 区域, 网格, 风电厂, 光伏电厂, 生物质能电厂, 储能装置, 客户, 充电桩站, 断路器, 分界箱,分界室,熔断器,直线杆塔,耐张杆塔 }
    public enum EObjectCategory { 无, 电厂设施类, 变电设施类, 开关设施类, 变压器类, 导线类, 母线类, 开关类, 杆塔类, 虚拟类, 用户类, 区域类, 其它类 }
    public enum EObjectFlag {无,主变,配变,线路,DG}
   
    #region ========== 设施类 ==========
    ///<summary>变电设施基类</summary>
    public abstract class DNTransformFacilityBase:pSymbolObject
    {
        public DNTransformFacilityBase(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._isFacility = true;
            thisDesc._objCategory = EObjectCategory.变电设施类;
            thisDesc._objScope = EObjectScope.输配电侧;
        }

        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public void createTopoData() { busiTopo = new TopoData(this); }
    }
    ///<summary>变电站</summary>
    public class DNSubStation : DNTransformFacilityBase 
    {
        public DNSubStation(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.变电站;
            symbolid = ESymbol.变电站运行.ToString();
            thisDesc._icon =(System.Windows.Media.Brush)System.Windows.Application.Current.FindResource("通用对象");
        }

      

        ///<summary>返回类型明确的台账数据</summary>
        public AcntSubstation thisAcntData { get { return busiAccount as AcntSubstation; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataSubstation thisRunData { get { return busiRunData as RunDataSubstation; } }


        public override void createAcntData() { busiAccount = new AcntSubstation(); }
        public override void createRunData() { busiRunData = new RunDataSubstation(this); }
    }
    ///<summary>配电室</summary>
    public class DNSwitchHouse : DNTransformFacilityBase
    {
        public DNSwitchHouse(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.配电室;
            symbolid = ESymbol.配电室.ToString();
        }


        ///<summary>返回类型明确的台账数据</summary>
        public AcntSwitchHouse thisAcntData { get { return busiAccount as AcntSwitchHouse; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataSwitchHouse thisRunData { get { return busiRunData as RunDataSwitchHouse; } }



        public override void createAcntData() { busiAccount = new AcntSwitchHouse(); }
        public override void createRunData() { busiRunData = new RunDataSwitchHouse(this); }
    }

   


    ///<summary>开关站</summary>
    public class DNSwitchStation : pSymbolObject 
    {
        public DNSwitchStation(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._isFacility = true;
            thisDesc._objType = EObjectType.开关站;
            thisDesc._objCategory = EObjectCategory.开关设施类;
            thisDesc._objScope = EObjectScope.输配电侧;
            symbolid = ESymbol.开关站运行.ToString();
        }


        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntSwitchStation thisAcntData { get { return busiAccount as AcntSwitchStation; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataSwitchStation thisRunData { get { return busiRunData as RunDataSwitchStation; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntSwitchStation(); }
        public override void createRunData() { busiRunData = new RunDataSwitchStation(this); }
        public void createTopoData() { busiTopo = new TopoData(this); }

    }

    ///<summary>箱式站</summary>
    public class DNBoxStation 
    { }


    ///<summary>分界室</summary>
    public class DNDividingRoom : pSymbolObject
    {
        public DNDividingRoom(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._isFacility = true;
            thisDesc._objType = EObjectType.分界室;
            thisDesc._objCategory = EObjectCategory.其它类;
            thisDesc._objScope = EObjectScope.输配电侧;
            symbolid = ESymbol.分界室.ToString();
        }


        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntDividing thisAcntData { get { return busiAccount as AcntDividing; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataDividing thisRunData { get { return busiRunData as RunDataDividing; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntDividing(); }
        public override void createRunData() { busiRunData = new RunDataDividing(this); }
        public void createTopoData() { busiTopo = new TopoData(this); }

    }
    ///<summary>分界箱</summary>
    public class DNDividingBox : pSymbolObject
    {
        public DNDividingBox(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._isFacility = true;
            thisDesc._objType = EObjectType.分界箱;
            thisDesc._objCategory = EObjectCategory.其它类;
            thisDesc._objScope = EObjectScope.输配电侧;
            symbolid = ESymbol.分界箱.ToString();
        }


        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntDividing thisAcntData { get { return busiAccount as AcntDividing; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataDividing thisRunData { get { return busiRunData as RunDataDividing; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntDividing(); }
        public override void createRunData() { busiRunData = new RunDataDividing(this); }
        public void createTopoData() { busiTopo = new TopoData(this); }

    }


    #endregion


    #region ========== 设备类 ==========

    #region ----- 变压器类 -----
    ///<summary>变压器基类</summary>
    public abstract class DNTransformerBase : pSymbolObject
    {
        public DNTransformerBase(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._isEquipment = true;
            thisDesc._objCategory = EObjectCategory.变压器类;
            thisDesc._objScope = EObjectScope.输配电侧;

            symbolid = ESymbol.双绕组变压器.ToString();
            isH = true;

        }
        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }

        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        ///<summary>创建对象拓扑数据</summary>
        public void createTopoData() { busiTopo = new TopoData(this); }

    }

    ///<summary>主变基类</summary>
    public abstract class DNMainTransformerBase : DNTransformerBase
    {
        public DNMainTransformerBase(pLayer layer)
            : base(layer)
        {
        }

    }

    ///<summary>主变压器</summary>
    public class DNMainTransformer : DNMainTransformerBase
    {
        public DNMainTransformer(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.三卷主变;
            symbolid = ESymbol.三绕组变压器.ToString();
            isH = true;
        }

        ///<summary>返回类型明确的台账数据</summary>
        public AcntMainTransformer thisAcntData { get { return busiAccount as AcntMainTransformer; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataMainTransformer thisRunData { get { return busiRunData as RunDataMainTransformer; } }

        public override void createAcntData() { busiAccount = new AcntMainTransformer(); }
        public override void createRunData() { busiRunData = new RunDataMainTransformer(this); }


    }


    ///<summary>2卷主变</summary>
    public class DNMainTransformer2W : DNMainTransformerBase
    {
        public DNMainTransformer2W(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.两卷主变;
            symbolid = ESymbol.双绕组变压器.ToString();
            isH = true;
        }

        ///<summary>返回类型明确的台账数据</summary>
        public AcntMainTransformer2W thisAcntData { get { return busiAccount as AcntMainTransformer2W; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataMainTransformer2W thisRunData { get { return busiRunData as RunDataMainTransformer2W; } }

        public override void createAcntData() { busiAccount = new AcntMainTransformer2W(); }
        public override void createRunData() { busiRunData = new RunDataMainTransformer2W(this); }
            

    }
    ///<summary>3卷主变</summary>
    public class DNMainTransformer3W : DNMainTransformerBase
    {
        public DNMainTransformer3W(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.三卷主变;
            symbolid = ESymbol.三绕组变压器.ToString();
            isH = true;
        }

        ///<summary>返回类型明确的台账数据</summary>
        public AcntMainTransformer3W thisAcntData { get { return busiAccount as AcntMainTransformer3W; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataMainTransformer3W thisRunData { get { return busiRunData as RunDataMainTransformer3W; } }

        public override void createAcntData() { busiAccount = new AcntMainTransformer3W(); }
        public override void createRunData() { busiRunData = new RunDataMainTransformer3W(this); }
    

    }



    ///<summary>配变</summary>
    public class DNDistTransformer : DNTransformerBase
    {
        public DNDistTransformer(pLayer layer)
            : base(layer)
        {
           thisDesc._objType = EObjectType.配变;
           symbolid = ESymbol.配变.ToString();
        }

        ///<summary>返回类型明确的台账数据</summary>
        public AcntDistTransformer thisAcntData { get { return busiAccount as AcntDistTransformer; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataDistTransformer thisRunData { get { return busiRunData as RunDataDistTransformer; } }

        public override void createAcntData() { busiAccount = new AcntDistTransformer(); }
        public override void createRunData() { busiRunData = new RunDataDistTransformer(this); }

    }

    ///<summary>柱上变</summary>
    public class DNColumnTransformer : DNTransformerBase
    {
        public DNColumnTransformer(pLayer layer)
            : base(layer)
        {

            thisDesc._objType = EObjectType.柱上变;
            //symbolid = ESymbol.配变.ToString();
        }

        ///<summary>返回类型明确的台账数据</summary>
        public AcntColumnTransformer thisAcntData { get { return busiAccount as AcntColumnTransformer; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataColumnTransformer thisRunData { get { return busiRunData as RunDataColumnTransformer; } }

        public override void createAcntData() { busiAccount = new AcntColumnTransformer(); }
        public override void createRunData() { busiRunData = new RunDataColumnTransformer(this); }

    }

    ///<summary>用户变</summary>
    public class DNCustomerTransformer : DNTransformerBase
    {
        public DNCustomerTransformer(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.用户变;
            //symbolid = ESymbol.配变.ToString();
        }

        ///<summary>返回类型明确的台账数据</summary>
        public AcntCustomerTransformer thisAcntData { get { return busiAccount as AcntCustomerTransformer; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataCustomerTransformer thisRunData { get { return busiRunData as RunDataCustomerTransformer; } }

        public override void createAcntData() { busiAccount = new AcntCustomerTransformer(); }
        public override void createRunData() { busiRunData = new RunDataCustomerTransformer(this); }

    }
    #endregion

    #region ----- 导线类 -----

    ///<summary>导线基类</summary>
    public abstract class DNACLineBase : pPowerLine
    {
        public DNACLineBase(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._isEquipment = true;
            thisDesc._objCategory = EObjectCategory.导线类;
            thisDesc._objScope = EObjectScope.输配电侧;

        }

        ///<summary>首端连接对象ID</summary>
        public string fromID { get; set; }
        ///<summary>末端连接对象ID</summary>
        public string toID { get; set; }


        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public void createTopoData() { busiTopo = new TopoData(this); }

    }

    ///<summary>输电线路</summary>
    public class DNACLine : DNACLineBase
    {
        public DNACLine(pLayer layer)
            : base(layer)
        {
            thisDesc._isFacility = true;
            thisDesc._objType = EObjectType.输电线路;
        }


        ///<summary>返回类型明确的台账数据</summary>
        public AcntACLine thisAcntData { get { return busiAccount as AcntACLine; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataACLine thisRunData { get { return busiRunData as RunDataACLine; } }

        public override void createAcntData() { busiAccount = new AcntACLine(); }
        public override void createRunData() { busiRunData = new RunDataACLine(this); }
    }

    ///<summary>导线段</summary>
    public class DNLineSeg : DNACLineBase
    {
        public DNLineSeg(pLayer layer)
            : base(layer)
        {
            thisDesc._isFacility = true;
            thisDesc._objType = EObjectType.导线段;
        }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntLineSeg thisAcntData { get { return busiAccount as AcntLineSeg; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataLineSeg thisRunData { get { return busiRunData as RunDataLineSeg; } }

        public override void createAcntData() { busiAccount = new AcntLineSeg(); }
        public override void createRunData() { busiRunData = new RunDataLineSeg(this); }
    }


    ///<summary>电缆段</summary>
    public class DNCableSeg : DNACLineBase
    {
        public DNCableSeg(pLayer layer)
            : base(layer)
        {
            thisDesc._isFacility = true;
            thisDesc._objType = EObjectType.电缆段;
        }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntCableSeg thisAcntData { get { return busiAccount as AcntCableSeg; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataCableSeg thisRunData { get { return busiRunData as RunDataCableSeg; } }

        public override void createAcntData() { busiAccount = new AcntCableSeg(); }
        public override void createRunData() { busiRunData = new RunDataCableSeg(this); }
    }

    ///<summary>馈线</summary>
    public class DNFeeder : DNACLineBase
    {
        public DNFeeder(pLayer layer)
            : base(layer)
        {
            thisDesc._isFacility = true;
            thisDesc._objType = EObjectType.馈线;

        }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntFeeder thisAcntData { get { return busiAccount as AcntFeeder; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataFeeder thisRunData { get { return busiRunData as RunDataFeeder; } }

        public override void createAcntData() { busiAccount = new AcntFeeder(); }
        public override void createRunData() { busiRunData = new RunDataFeeder(this); }

    }

    ///<summary>连接线</summary>
    public class DNConnectivityLine : DNACLineBase
    {
        public DNConnectivityLine(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.连接线;
        }

        ///<summary>返回类型明确的台账数据</summary>
        public AcntConnectivityLine thisAcntData { get { return busiAccount as AcntConnectivityLine; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataConnectivityLine thisRunData { get { return busiRunData as RunDataConnectivityLine; } }

        public override void createAcntData() { busiAccount = new AcntConnectivityLine(); }
        public override void createRunData() { busiRunData = new RunDataConnectivityLine(this); }
    }

    #endregion

    ///<summary>变电站出线</summary>
    public class DNSubstationOutline : pSymbolObject
    {
        public DNSubstationOutline(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._isEquipment = true;
            thisDesc._isFacility = true;
            thisDesc._objType = EObjectType.变电站出线;
            thisDesc._objCategory = EObjectCategory.其它类;
            thisDesc._objScope = EObjectScope.输配电侧;
            symbolid = ESymbol.小圆圈.ToString();
        }
        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntSubstationOutline thisAcntData { get { return busiAccount as AcntSubstationOutline; } }
        ///<summary>无运行数据，返回null</summary>
        public RunDataBase thisRunData { get { return null; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntSubstationOutline(); }
        ///<summary>方法对于出线无效，不考虑运行数据</summary>
        public override void createRunData() { busiRunData = null ; }
        public void createTopoData() { busiTopo = new TopoData(this); }


    }

    ///<summary>分支箱</summary>
    public class DNCableBranchBox 
    { }

    ///<summary>光缆</summary>
    public class DNFiber 
    { }

    ///<summary>光缆站点</summary>
    public class DNFiberNode 
    { }

    ///<summary>环网柜</summary>
    public class DNRingMainUnit 
    { }


    ///<summary>开关基类</summary>
    public class DNSwitchBase : pSymbolObject
    {
        public DNSwitchBase(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._isEquipment = true;
            thisDesc._objCategory = EObjectCategory.开关类;
            thisDesc._objScope = EObjectScope.输配电侧;
        }
        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public void createTopoData() { busiTopo = new TopoData(this); }

    }


    ///<summary>隔离开关</summary>
    public class DNSwitch : DNSwitchBase
    {
        public DNSwitch(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.隔离开关;
            symbolid = ESymbol.隔离开关合.ToString();
        }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntSwitch thisAcntData { get { return busiAccount as AcntSwitch; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataSwitch thisRunData { get { return busiRunData as RunDataSwitch; } }

        public override void createAcntData() { busiAccount = new AcntSwitch(); }
        public override void createRunData() { busiRunData = new RunDataSwitch(this); }

    }
    ///<summary>负荷开关</summary>
    public class DNLoadSwitch : DNSwitchBase
    {
        public DNLoadSwitch(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.负荷开关;
            symbolid = ESymbol.负荷开关合.ToString();
        }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntLoadSwitch thisAcntData { get { return busiAccount as AcntLoadSwitch; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataLoadSwitch thisRunData { get { return busiRunData as RunDataLoadSwitch; } }

        public override void createAcntData() { busiAccount = new AcntLoadSwitch(); }
        public override void createRunData() { busiRunData = new RunDataLoadSwitch(this); }

    }
    ///<summary>断路器</summary>
    public class DNBreaker : DNSwitchBase
    {
        public DNBreaker(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.断路器;
            symbolid = ESymbol.断路器合.ToString();
        }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntBreaker thisAcntData { get { return busiAccount as AcntBreaker; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataBreaker thisRunData { get { return busiRunData as RunDataBreaker; } }

        public override void createAcntData() { busiAccount = new AcntBreaker(); }
        public override void createRunData() { busiRunData = new RunDataBreaker(this); }


    }

    ///<summary>熔断器</summary>
    public class DNFuse : DNSwitchBase
    {
        public DNFuse(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.熔断器;
            symbolid = ESymbol.熔断器.ToString();
        }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntFuse thisAcntData { get { return busiAccount as AcntFuse; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataFuse thisRunData { get { return busiRunData as RunDataFuse; } }

        public override void createAcntData() { busiAccount = new AcntFuse(); }
        public override void createRunData() { busiRunData = new RunDataFuse(this); }

    }

    ///<summary>杆塔基类</summary>
    public class DNSupportBase:pSymbolObject
    {
        public DNSupportBase(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._isEquipment = true;
            thisDesc._objCategory = EObjectCategory.杆塔类;
            thisDesc._objScope = EObjectScope.输配电侧;
        }

        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public void createTopoData() { busiTopo = new TopoData(this); }

    }
    ///<summary>直线杆塔</summary>
    public class DNIntermediateSupport : DNSupportBase
    {
        public DNIntermediateSupport(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.直线杆塔;
            symbolid = ESymbol.运行杆塔直线型.ToString();
        }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntIntermediateSupport thisAcntData { get { return busiAccount as AcntIntermediateSupport; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataIntermediateSupport thisRunData { get { return busiRunData as RunDataIntermediateSupport; } }

        public override void createAcntData() { busiAccount = new AcntIntermediateSupport(); }
        public override void createRunData() { busiRunData = new RunDataIntermediateSupport(this); }

    }
    ///<summary>耐张杆塔</summary>
    public class DNStrainSupport : DNSupportBase
    {
        public DNStrainSupport(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.耐张杆塔;
            symbolid = ESymbol.运行杆塔耐张型.ToString();
        }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntStrainSupport thisAcntData { get { return busiAccount as AcntStrainSupport; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataStrainSupport thisRunData { get { return busiRunData as RunDataStrainSupport; } }

        public override void createAcntData() { busiAccount = new AcntStrainSupport(); }
        public override void createRunData() { busiRunData = new RunDataStrainSupport(this); }

    }


    ///<summary>连接点</summary>
    public class DNConnectivityNode : pSymbolObject
    {
        public DNConnectivityNode(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._isEquipment = true;
            thisDesc._objType = EObjectType.连接点;
            thisDesc._objCategory = EObjectCategory.其它类;
            thisDesc._objScope = EObjectScope.输配电侧;
            symbolid = ESymbol.小圆圈.ToString();
        }

        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public void createTopoData() { busiTopo = new TopoData(this); }

    }

    ///<summary>母线</summary>
    public class DNBusBar : pPowerLine
    {
        public DNBusBar(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData() ;
            thisDesc._isEquipment = true;
            thisDesc._objType = EObjectType.母线;
            thisDesc._objCategory = EObjectCategory.母线类;
            thisDesc._objScope = EObjectScope.输配电侧;

        }

        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntBusBar thisAcntData { get { return busiAccount as AcntBusBar; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataBusBar thisRunData { get { return busiRunData as RunDataBusBar; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntBusBar(); }
        public override void createRunData() { busiRunData = new RunDataBusBar(this); }
        public void createTopoData() { busiTopo = new TopoData(this); }

    }

    ///<summary>母联</summary>
    public class DNBusBarSwitch : pPowerLine
    {
        public DNBusBarSwitch(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._isEquipment = true;
            thisDesc._objType = EObjectType.母联;
            thisDesc._objCategory = EObjectCategory.开关类; 
            thisDesc._objScope = EObjectScope.输配电侧;

        }

        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntBusBarSwitch thisAcntData { get { return busiAccount as AcntBusBarSwitch; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataBusBarSwitch thisRunData { get { return busiRunData as RunDataBusBarSwitch; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntBusBarSwitch(); }
        public override void createRunData() { busiRunData = new RunDataBusBarSwitch(this); }
        public void createTopoData() { busiTopo = new TopoData(this); }
    }


    ///<summary>节点</summary>
    public class DNNode : pSymbolObject
    {
        public DNNode(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._isEquipment = true;
            thisDesc._objType = EObjectType.节点;
            thisDesc._objCategory = EObjectCategory.其它类;
            thisDesc._objScope = EObjectScope.输配电侧;
            symbolid = ESymbol.小圆圈.ToString();
        }

        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntNode thisAcntData { get { return busiAccount as AcntNode; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataNode thisRunData { get { return busiRunData as RunDataNode; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntNode(); }
        public override void createRunData() { busiRunData = new RunDataNode(this); }
        public void createTopoData() { busiTopo = new TopoData(this); }


    }


        ///<summary>储能装置</summary>
    public class DNStoredEnergy : pSymbolObject
    {
        public DNStoredEnergy(pLayer layer)
            : base(layer)
        {
            busiDesc=new DescData();
            thisDesc._objType = EObjectType.储能装置;
        }

        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntStoredEnergy thisAcntData { get { return busiAccount as AcntStoredEnergy; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataStoredEnergy thisRunData { get { return busiRunData as RunDataStoredEnergy; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntStoredEnergy(); }
        public override void createRunData() { busiRunData = new RunDataStoredEnergy(this); }
        public void createTopoData() { busiTopo = new TopoData(this); }
    }

    #endregion

    #region ========== 区域类 ==========
    ///<summary>行政区域</summary>
    public class DNRegion : pArea
    {
        public DNRegion(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._objType = EObjectType.行政区域;
            thisDesc._objCategory = EObjectCategory.区域类;

        }

        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntRegion thisAcntData { get { return busiAccount as AcntRegion; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataRegion thisRunData { get { return busiRunData as RunDataRegion; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntRegion(); }
        public override void createRunData() { busiRunData = new RunDataRegion(this); }
        public void createTopoData() { busiTopo = new TopoData(this); }

    }



    ///<summary>区域</summary>
    public class DNArea : pArea
    {
        public DNArea(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._objType = EObjectType.区域;
            thisDesc._objCategory = EObjectCategory.区域类;
        }

        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntArea thisAcntData { get { return busiAccount as AcntArea; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataArea thisRunData { get { return busiRunData as RunDataArea; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntArea(); }
        public override void createRunData() { busiRunData = new RunDataArea(this); }
        public void createTopoData() { busiTopo = new TopoData(this); }


    }

    ///<summary>网格，即最小区域单位</summary>
    public class DNGridArea : pArea
    {
        public DNGridArea(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._objType = EObjectType.网格;
            thisDesc._objCategory = EObjectCategory.区域类;

        }
        ///<summary>用地类型着色</summary>
        public System.Windows.Media.Color typeColor { get; set; }


        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntGridArea thisAcntData { get { return busiAccount as AcntGridArea; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataGridArea thisRunData { get { return busiRunData as RunDataGridArea; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntGridArea(); }
        public override void createRunData() { busiRunData = new RunDataGridArea(this); }
        public void createTopoData() { busiTopo = new TopoData(this); }

    }

    #endregion

    #region ========== 发电侧 ==========
    ///<summary>发电厂基类</summary>
    public abstract class DNPlantBase : pSymbolObject 
    {
        public DNPlantBase(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._isFacility = true;
            thisDesc._objCategory = EObjectCategory.电厂设施类;
            thisDesc._objScope = EObjectScope.发电侧;
        }

        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public void createTopoData() { busiTopo = new TopoData(this); }

    }

    ///<summary>风电</summary>
    public class DNWindPlant : DNPlantBase
    {
        public DNWindPlant(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.风电厂;
            symbolid = ESymbol.风力发电运行.ToString();
        }

        ///<summary>返回类型明确的台账数据</summary>
        public AcntWindPlant thisAcntData { get { return busiAccount as AcntWindPlant; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataWindPlant thisRunData { get { return busiRunData as RunDataWindPlant; } }

        public override void createAcntData() { busiAccount = new AcntWindPlant(); }
        public override void createRunData() { busiRunData = new RunDataWindPlant(this); }

    }

    ///<summary>光伏</summary>
    public class DNPVPlant : DNPlantBase
    {
        public DNPVPlant(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.光伏电厂;
            symbolid = ESymbol.光伏发电运行.ToString();
        }

        ///<summary>返回类型明确的台账数据</summary>
        public AcntPVPlant thisAcntData { get { return busiAccount as AcntPVPlant; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataPVPlant thisRunData { get { return busiRunData as RunDataPVPlant; } }

        public override void createAcntData() { busiAccount = new AcntPVPlant(); }
        public override void createRunData() { busiRunData = new RunDataPVPlant(this); }
    }

    ///<summary>生物质能</summary>
    public class DNBiomassPlant : DNPlantBase
    {
        public DNBiomassPlant(pLayer layer)
            : base(layer)
        {
            thisDesc._objType = EObjectType.生物质能电厂;
        }

        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntBiomassPlant thisAcntData { get { return busiAccount as AcntBiomassPlant; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataBiomassPlant thisRunData { get { return busiRunData as RunDataBiomassPlant; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntBiomassPlant(); }
        public override void createRunData() { busiRunData = new RunDataBiomassPlant(this); }
        public void createTopoData() { busiTopo = new TopoData(this); }
    }



    #endregion


    #region ========== 用电侧 ==========

    ///<summary>客户</summary>
    public class DNCustomer : pSymbolObject
    {
        public DNCustomer(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._objType = EObjectType.客户;
        }

        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntCustomer thisAcntData { get { return busiAccount as AcntCustomer; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataCustomer thisRunData { get { return busiRunData as RunDataCustomer; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntCustomer(); }
        public override void createRunData() { busiRunData = new RunDataCustomer(this); }
        public void createTopoData() { busiTopo = new TopoData(this); }
    }


    ///<summary>充电桩</summary>
    public class DNRechange : pSymbolObject
    {
        public DNRechange(pLayer layer)
            : base(layer)
        {
            busiDesc = new DescData();
            thisDesc._objType = EObjectType.充电桩站;
        }

        ///<summary>返回对象描述数据</summary>
        public DescData thisDesc { get { return busiDesc as DescData; } }
        ///<summary>返回类型明确的台账数据</summary>
        public AcntRechange thisAcntData { get { return busiAccount as AcntRechange; } }
        ///<summary>返回类型明确的运行数据</summary>
        public RunDataRechange thisRunData { get { return busiRunData as RunDataRechange; } }
        ///<summary>返回对象拓扑数据</summary>
        public TopoData thisTopoData { get { return busiTopo as TopoData; } }

        public override void createAcntData() { busiAccount = new AcntRechange(); }
        public override void createRunData() { busiRunData = new RunDataRechange(this); }
        public void createTopoData() { busiTopo = new TopoData(this); }
    }





    #endregion


}
