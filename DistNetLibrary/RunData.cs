using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using WpfEarthLibrary;

namespace DistNetLibrary
{
    ///<summary>负荷接口</summary>
    public interface ILoad 
    {
        double apparentPower { get;}
    }

    ///<summary>事件数据</summary>
        public class EventData
        {
            public enum EEventType {计划事件, 告警事件, 故障事件 }

            ///<summary>事件对象ID</summary>
            public string eObjID { get; set; }
            
            ///<summary>事件类型</summary>
            public EEventType eType {get;set;}
            ///<summary>事件标题</summary>
            public string eTitle { get; set; }
            ///<summary>事件内容</summary>
            public string eContent {get;set;}
            ///<summary>事件开始时间</summary>
            public DateTime? startTime {get;set;}
            ///<summary>事件结束时间</summary>
            public DateTime? endTime {get;set;}

            public string strStartTime { get { return startTime == null ? "" : ((DateTime)startTime).ToString("dd HH:mm"); } }
            public Brush icon {get {
                return (Brush)System.Windows.Application.Current.TryFindResource(eType.ToString());
            }}
        }



    ///<summary>运行数据基类</summary>
    public abstract class RunDataBase:MyClassLibrary.MVVM.NotificationObject
    {
        public RunDataBase(PowerBasicObject Parent)
        {
            parent = Parent;
        }

        internal PowerBasicObject parent;

        //注：只有有DisplayName的属性，才能使用数据库描述定义来存取数据
        //[DisplayName("名称")]
        public string name { get { return parent.name; } }

        public List<EventData> events = new List<EventData>();
    }

    ///<summary>含功率数据的运行数据基类</summary>
    public abstract class RunDataPowerBase :RunDataBase, ILoad
    {
        public RunDataPowerBase(PowerBasicObject Parent)
            : base(Parent)
        {
            lstApparentPower = new System.ComponentModel.BindingList<MyClassLibrary.DevShare.ChartDataPoint>();
        }


        [DisplayName("有功功率")]
        ///<summary>有功功率</summary>
        public double activePower { get; set; }
        [DisplayName("无功功率")]
        ///<summary>无功功率</summary>
        public double reactivePower { get; set; }

        //[DisplayName("视在功率")]
        ///<summary>视在功率=(有功^2+无功^2)^0.5</summary>
        public double apparentPower { get { return Math.Pow((activePower * activePower + reactivePower * reactivePower), 0.5); } }
        //[DisplayName("负载率")]
        ///<summary>负载率</summary>
        public double rateOfLoad { get { return (parent.busiAccount is ICapcity) ? apparentPower / (parent.busiAccount as ICapcity).cap : 0; } }
        ///<summary>功率因素=有功功率/视在功率</summary>
        public double powerFactor { get { return apparentPower == 0 ? 0 : activePower / apparentPower; } }
        ///<summary>视在功率曲线</summary>
        public System.ComponentModel.BindingList<MyClassLibrary.DevShare.ChartDataPoint> lstApparentPower { get; set; }



        public Color rateOfLoadColor1 { get { return rateOfLoad > 0.8 ? Color.FromRgb(0xFF, 0x8A, 0x8A) : (rateOfLoad < 0.2 ? Color.FromRgb(0x81, 0xFF, 0x81) : Color.FromRgb(0x8E, 0x8E, 0xFF)); } }
        public Color rateOfLoadColor2 { get { return rateOfLoad > 0.8 ? Color.FromRgb(0xFF, 0x60, 0x60) : (rateOfLoad < 0.2 ? Color.FromRgb(0x00, 0xB8, 0x00) : Color.FromRgb(0x4F, 0x4F, 0xFF)); } }
        public Color rateOfLoadColor3 { get { return rateOfLoad > 0.8 ? Color.FromRgb(0x99, 0x00, 0x00) : (rateOfLoad < 0.2 ? Color.FromRgb(0x00, 0x42, 0x00) : Color.FromRgb(0x00, 0x00, 0x66)); } }
        public Brush rateOfLoadBrush
        {
            get
            {
                LinearGradientBrush brush = new LinearGradientBrush() { EndPoint = new System.Windows.Point(0, 1) };
                brush.GradientStops.Add(new GradientStop() { Color = Colors.Gainsboro, Offset = 0 });
                brush.GradientStops.Add(new GradientStop() { Color = rateOfLoadColor1, Offset = 0.5 });
                brush.GradientStops.Add(new GradientStop() { Color = rateOfLoadColor2, Offset = 0.5 });
                brush.GradientStops.Add(new GradientStop() { Color = rateOfLoadColor3, Offset = 1 });
                return brush;
            }
        }
        public string apparentPowerInfo { get { return string.Format("{0:f3}MW", apparentPower); } }
        public string activePowerInfo { get { return string.Format("{0:f3}MW", activePower); } }
        public string reactivePowerInfo { get { return string.Format("{0:f3}MVar", reactivePower); } }
        public string rateOfLoadInfo { get { return string.Format("{0:p1}", rateOfLoad); } }

        ///<summary>加视在功率到曲线，应仅在实时数据中使用</summary>
        public void addApparentPower()
        {
            //apparentPower = ap;
            if (lstApparentPower.Count > 0 && DateTime.Now.Minute != ((DateTime)lstApparentPower.Last().argudate).Minute) //按分钟加
            {
                if (lstApparentPower.Count > 60)
                    lstApparentPower.Remove(lstApparentPower.First());
                lstApparentPower.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = DateTime.Now, value = apparentPower });
            }
        }

        public void refresh()
        {
            RaisePropertyChanged(() => apparentPowerInfo);
            RaisePropertyChanged(() => activePowerInfo);
            RaisePropertyChanged(() => reactivePowerInfo);
            RaisePropertyChanged(() => rateOfLoadInfo);
            RaisePropertyChanged(() => rateOfLoadBrush);
            RaisePropertyChanged(() => rateOfLoad);
        }

    }



    #region ---------- 潮流线路类 -----------
    ///<summary>潮流导线基类</summary>
    public abstract class RunDataACLineBase : RunDataPowerBase
    {
        public RunDataACLineBase(PowerBasicObject Parent)
            : base(Parent)
        {
           
        }

     
    }
    ///<summary>线路运行数据</summary>
    public class RunDataACLine : RunDataACLineBase
    {
        public RunDataACLine(PowerBasicObject Parent)
            : base(Parent)
        {
        }

    }

    ///<summary>导线段</summary>
    public class RunDataLineSeg : RunDataACLineBase
    {
        public RunDataLineSeg(PowerBasicObject Parent)
            : base(Parent)
        {
        }

    }
    ///<summary>电缆段</summary>
    public class RunDataCableSeg : RunDataACLineBase
    {
        public RunDataCableSeg(PowerBasicObject Parent)
            : base(Parent)
        {
        }

    }
    ///<summary>馈线</summary>
    public class RunDataFeeder : RunDataACLineBase
    {
        public RunDataFeeder(PowerBasicObject Parent)
            : base(Parent)
        {
        }

    }
    ///<summary>连接线</summary>
    public class RunDataConnectivityLine : RunDataACLineBase
    {
        public RunDataConnectivityLine(PowerBasicObject Parent)
            : base(Parent)
        {
        }


    }
    #endregion
    ///<summary>连接点</summary>
    public class RunDataConnectivityNode : RunDataBase
    {
        public RunDataConnectivityNode(PowerBasicObject Parent)
            : base(Parent)
        {
        }


    }

    #region ---------- 变电设施类 ----------

    ///<summary>变电设施运行数据基类</summary>
    public class RunDataTransformFacilityBase : RunDataPowerBase
    {
        public RunDataTransformFacilityBase(PowerBasicObject Parent)
            : base(Parent)
        {
            
        }

        [DisplayName("冗余度")]
        ///<summary>冗余度</summary>
        public double redundancy { get; set; }
        [DisplayName("高压侧运行电压")]
        ///<summary>高压侧运行电压</summary>
        public double HVL { get; set; }
        [DisplayName("中压侧运行电压")]
        ///<summary>中压侧运行电压</summary>
        public double MVL { get; set; }
        [DisplayName("低压侧运行电压")]
        ///<summary>低压侧运行电压</summary>
        public double LVL { get; set; }
        //[DisplayName("电压标幺值")]
        ///<summary>电压标幺值</summary>
        public double HVoltPUV { get { return (parent.busiAccount is AcntDistBase) ? HVL / (parent.busiAccount as AcntDistBase).vl : 1; } }
        
        public string HVLInfo { get { return string.Format("{0:f1}KV", HVL); } }
        

        public void refresh()
        {
            base.refresh();
            RaisePropertyChanged(() => HVLInfo);
        }
    }
       ///<summary>变电站运行数据</summary>
    public class RunDataSubstation : RunDataTransformFacilityBase
    {
        public RunDataSubstation(PowerBasicObject Parent)
            : base(Parent)
        {
        }
    }

    ///<summary>变电站出线运行数据</summary>
    public class RunDataSubstationOutline : RunDataBase
    {
        public RunDataSubstationOutline(PowerBasicObject Parent)
            : base(Parent)
        {
        }
    }


    ///<summary>配电室运行数据</summary>
    public class RunDataSwitchHouse : RunDataTransformFacilityBase
    {
        public RunDataSwitchHouse(PowerBasicObject Parent)
            : base(Parent)
        {
        }

    }
    #endregion

    ///<summary>开关站运行数据</summary>
    public class RunDataSwitchStation : RunDataBase
    {
        public RunDataSwitchStation(PowerBasicObject Parent)
            : base(Parent)
        {
        }

    }


    #region ----------- 变压器类 -----------
    ///<summary>变压器运行数据基类</summary>
    public class RunDataTransformerBase : RunDataPowerBase
    {
        public RunDataTransformerBase(PowerBasicObject Parent)
            : base(Parent)
        {

        }

        [DisplayName("高压侧运行电压")]
        ///<summary>高压侧运行电压</summary>
        public double HVL { get; set; }
        [DisplayName("低压侧运行电压")]
        ///<summary>低压侧运行电压</summary>
        public double LVL { get; set; }
        //[DisplayName("电压标幺值")]
        ///<summary>电压标幺值</summary>
        public double HVoltPUV { get { return (parent.busiAccount is AcntDistBase) ? HVL / (parent.busiAccount as AcntDistBase).vl : 1; } }
        [DisplayName("变损")]
        ///<summary>变损</summary>
        public double transformLoss { get; set; }
        [DisplayName("变损率")]
        ///<summary>变损率</summary>
        public double transformLossRate { get; set; }
        [DisplayName("铜损")]
        ///<summary>铜损</summary>
        public double copperLoss { get; set; }
        [DisplayName("铁损")]
        ///<summary>铁损</summary>
        public double ironLoss { get; set; }
        [DisplayName("总损耗")]
        ///<summary>总损耗</summary>
        public double allLoss { get; set; }


        public string HVLInfo { get { return string.Format("{0:f1}KV", HVL); } }
        public string transformLossInfo { get { return string.Format("{0:f3}MW", transformLoss); } }
        public string transformLossRateInfo { get { return string.Format("{0:p1}", transformLossRate); } }

        public void refresh()
        {
            base.refresh();
            RaisePropertyChanged(() => HVLInfo);
            RaisePropertyChanged(() => transformLossInfo);
            RaisePropertyChanged(() => transformLossRateInfo);
        }

    }

    ///<summary>主变压器运行数据</summary>
    public class RunDataMainTransformer : RunDataTransformerBase
    {
        public RunDataMainTransformer(PowerBasicObject Parent)
            : base(Parent)
        {
        }


    }


    ///<summary>2卷变压器运行数据</summary>
    public class RunDataMainTransformer2W : RunDataTransformerBase
    {
        public RunDataMainTransformer2W(PowerBasicObject Parent)
            : base(Parent)
        {
        }


    }

    ///<summary>3卷变压器运行数据</summary>
    public class RunDataMainTransformer3W : RunDataTransformerBase
    {
        public RunDataMainTransformer3W(PowerBasicObject Parent)
            : base(Parent)
        {
        }


    }

    ///<summary>配变运行数据</summary>
    public class RunDataDistTransformer : RunDataTransformerBase
    {
        public RunDataDistTransformer(PowerBasicObject Parent)
            : base(Parent)
        {
        }


    }

    ///<summary>柱上变压器运行数据</summary>
    public class RunDataColumnTransformer : RunDataTransformerBase
    {
        public RunDataColumnTransformer(PowerBasicObject Parent)
            : base(Parent)
        {
        }


    }
    ///<summary>用户变压器运行数据</summary>
    public class RunDataCustomerTransformer : RunDataTransformerBase
    {
        public RunDataCustomerTransformer(PowerBasicObject Parent)
            : base(Parent)
        {
        }


    }

    #endregion

    ///<summary>节点数据</summary>
    public class RunDataNode : RunDataBase
    {
        public RunDataNode(PowerBasicObject Parent)
            : base(Parent)
        {
        }
        [DisplayName("有功功率")]
        ///<summary>有功功率</summary>
        public double activePower { get; set; }
        [DisplayName("无功功率")]
        ///<summary>无功功率</summary>
        public double reactivePower { get; set; }
        [DisplayName("电压等级")]
        ///<summary>电压等级</summary>
        public double vl { get; set; }
        [DisplayName("节点电压")]
        ///<summary>节点电压</summary>
        public double volt { get; set; }
        ///<summary>电压标幺值</summary>
        public double voltPUV { get { return volt / vl; } }

        public Color voltPUVColor { get { return voltPUV > 1.05 ? Color.FromRgb(0xFF, 0x8A, 0x8A) : (voltPUV > 0.95 ? Color.FromRgb(0x81, 0xFF, 0x81) : Color.FromRgb(0x8E, 0x8E, 0xFF)); } }
        public Brush voltPUVBrush
        {
            get
            {
                SolidColorBrush brush = new SolidColorBrush(voltPUVColor);
                return brush;
            }
        }

    }


    #region ---------- 开关类 ----------

    ///<summary>开关运行数据基类</summary>
    public class RunDataSwitchBase : RunDataBase
    {
        public RunDataSwitchBase(PowerBasicObject Parent)
            : base(Parent)
        {
        }
        [DisplayName("是否闭合")]
        ///<summary>是否闭合</summary>
        public virtual bool isClose { get; set; }
    }

    ///<summary>隔离开关运行数据</summary>
    public class RunDataSwitch : RunDataSwitchBase
    {
        public RunDataSwitch(PowerBasicObject Parent)
            : base(Parent)
        {
        }

    }

    ///<summary>负荷开关运行数据</summary>
    public class RunDataLoadSwitch : RunDataSwitchBase
    {
        public RunDataLoadSwitch(PowerBasicObject Parent)
            : base(Parent)
        {
        }

    }


    ///<summary>断路器运行数据</summary>
    public class RunDataBreaker : RunDataSwitchBase
    {
        public RunDataBreaker(PowerBasicObject Parent)
            : base(Parent)
        {
        }

        public override bool isClose
        {
            get
            {
                return base.isClose;
            }
            set
            {
                base.isClose = value;
                (parent as DNBreaker).symbolid = value ? ESymbol.断路器合.ToString() : ESymbol.断路器分.ToString();
            }
        }

    }
    ///<summary>熔断器运行数据</summary>
    public class RunDataFuse : RunDataSwitchBase
    {
        public RunDataFuse(PowerBasicObject Parent)
            : base(Parent)
        {
        }

    }

    ///<summary>母联</summary>
    public class RunDataBusBarSwitch : RunDataSwitchBase
    {
        public RunDataBusBarSwitch(PowerBasicObject Parent)
            : base(Parent)
        {
        }


    }
    #endregion



    ///<summary>母线</summary>
    public class RunDataBusBar : RunDataBase
    {
        public RunDataBusBar(PowerBasicObject Parent)
            : base(Parent)
        {
        }


    }

  




    ///<summary>储能装置</summary>
    public class RunDataStoredEnergy : RunDataBase
    {
        public RunDataStoredEnergy(PowerBasicObject Parent)
            : base(Parent)
        {
        }


    }

    ///<summary>分界室箱</summary>
    public class RunDataDividing : RunDataBase
    {
        public RunDataDividing(PowerBasicObject Parent)
            : base(Parent)
        {
        }


    }

    ///<summary>杆塔基类</summary>
    public class RunDataSupportBase: RunDataBase
    {
        public RunDataSupportBase(PowerBasicObject Parent)
            : base(Parent)
        {
        }
    }
    ///<summary>直线杆塔</summary>
    public class RunDataIntermediateSupport: RunDataSupportBase
    {
        public RunDataIntermediateSupport(PowerBasicObject Parent)
            : base(Parent)
        {
        }
    }
    ///<summary>耐张杆塔</summary>
    public class RunDataStrainSupport : RunDataSupportBase
    {
        public RunDataStrainSupport(PowerBasicObject Parent)
            : base(Parent)
        {
        }
    }


    #region 区域类
    ///<summary>行政区域</summary>
    public class RunDataRegion : RunDataBase
    {
        public RunDataRegion(PowerBasicObject Parent)
            : base(Parent)
        {
        }

        [DisplayName("负荷")]
        ///<summary>负荷</summary>
        public double load { get; set; }



    }

    ///<summary>区域</summary>
    public class RunDataArea : RunDataBase
    {
        public RunDataArea(PowerBasicObject Parent)
            : base(Parent)
        {
        }

        [DisplayName("负荷")]
        ///<summary>负荷</summary>
        public double load { get; set; }
    }

    ///<summary>网络区域</summary>
    public class RunDataGridArea : RunDataBase
    {
        public RunDataGridArea(PowerBasicObject Parent)
            : base(Parent)
        {
        }

        [DisplayName("负荷")]
        ///<summary>负荷</summary>
        public double load { get; set; }

        ///<summary>负荷密度</summary>
        public double loadDensity { get { return load/(parent.busiAccount as AcntAreaBase).area; } }
    }
    #endregion


    #region 发电侧
    public class RunDataPlantBase : RunDataBase
    {
        public RunDataPlantBase(PowerBasicObject Parent)
            : base(Parent)
        {
        }

        ///<summary>有功出力</summary>
        public double power { get; set; }
        ///<summary>负载率</summary>
        public double rateOfLoad { get; set; }

        public Color rateOfLoadColor1 { get { return rateOfLoad > 0.8 ? Color.FromRgb(0xFF, 0x8A, 0x8A) : (rateOfLoad < 0.2 ? Color.FromRgb(0x81, 0xFF, 0x81) : Color.FromRgb(0x8E, 0x8E, 0xFF)); } }
        public Color rateOfLoadColor2 { get { return rateOfLoad > 0.8 ? Color.FromRgb(0xFF, 0x60, 0x60) : (rateOfLoad < 0.2 ? Color.FromRgb(0x00, 0xB8, 0x00) : Color.FromRgb(0x4F, 0x4F, 0xFF)); } }
        public Color rateOfLoadColor3 { get { return rateOfLoad > 0.8 ? Color.FromRgb(0x99, 0x00, 0x00) : (rateOfLoad < 0.2 ? Color.FromRgb(0x00, 0x42, 0x00) : Color.FromRgb(0x00, 0x00, 0x66)); } }
        public Brush rateOfLoadBrush
        {
            get
            {
                LinearGradientBrush brush = new LinearGradientBrush() { EndPoint = new System.Windows.Point(0, 1) };
                brush.GradientStops.Add(new GradientStop() { Color = Colors.Gainsboro, Offset = 0 });
                brush.GradientStops.Add(new GradientStop() { Color = rateOfLoadColor1, Offset = 0.5 });
                brush.GradientStops.Add(new GradientStop() { Color = rateOfLoadColor2, Offset = 0.5 });
                brush.GradientStops.Add(new GradientStop() { Color = rateOfLoadColor3, Offset = 1 });
                return brush;
            }
        }
    }

    ///<summary>风电厂</summary>
    public class RunDataWindPlant : RunDataPlantBase
    {
        public RunDataWindPlant(PowerBasicObject Parent)
            : base(Parent)
        {
        }
    }

    ///<summary>光伏电厂</summary>
    public class RunDataPVPlant : RunDataPlantBase
    {
        public RunDataPVPlant(PowerBasicObject Parent)
            : base(Parent)
        {
        }

    }
    ///<summary>生物质能电厂</summary>
    public class RunDataBiomassPlant : RunDataPlantBase
    {
        public RunDataBiomassPlant(PowerBasicObject Parent)
            : base(Parent)
        {
        }

    }

    #endregion
    #region 用电侧
    ///<summary>客户台账</summary>
    public class RunDataCustomer : RunDataBase
    {
        public RunDataCustomer(PowerBasicObject Parent)
            : base(Parent)
        {
        }

    }
    ///<summary>充电桩站台账</summary>
    public class RunDataRechange : RunDataBase
    {
        public RunDataRechange(PowerBasicObject Parent)
            : base(Parent)
        {
        }

    }


    #endregion


}