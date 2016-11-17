using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;
using DistNetLibrary;

namespace DNVLibrary.Run
{
    ///<summary>模拟数据生成器 </summary>
    internal static class DataGenerator
    {
        static System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(10000) };
        static DataGenerator()
        {
            tmr.Tick += new EventHandler(tmr_Tick);
        }

        #region 初始化配网对象的运行数据
        static bool isRunDataInit;
        ///<summary>初始化运行数据类</summary>
        public static void initRunData(DistNet dn, bool isForce=false)
        {
            if (!isRunDataInit || isForce)
            {
                //变电设施类
                foreach (DNTransformFacilityBase item in dn.getAllObjListByCategory(EObjectCategory.变电设施类))
                {
                    item.createRunData();
                    item.tooltipMoveTemplate = "RunDataSubsationTemplate";
                    item.tooltipMoveContent = item.busiRunData;
                }
                //变压器类
                foreach (DNTransformerBase item in dn.getAllObjListByCategory(EObjectCategory.变压器类))
                {
                    item.createRunData();
                    item.tooltipMoveTemplate = "RunDataTransformerTemplate";
                    item.tooltipMoveContent = item.busiRunData;
                }
                //导线类
                foreach (DNACLineBase item in dn.getAllObjListByCategory(EObjectCategory.导线类))
                {
                    item.createRunData();
                    if (item is DNACLine)  //仅输电线路tooltip
                    {
                        item.tooltipMoveTemplate = "RunDataACLineTemplate";
                        item.tooltipMoveContent = item.busiRunData;
                    }
                }
                //开关类
                foreach (PowerBasicObject item in dn.getAllObjListByCategory(EObjectCategory.开关类))
                    item.createRunData();
                //节点
                foreach (DNNode item in dn.getAllObjListByObjType(EObjectType.节点))
                    item.createRunData();
                //电厂
                foreach (DNPlantBase item in dn.getAllObjListByCategory(EObjectCategory.电厂设施类))
                    item.createRunData();

            }
            tmr.Tag = dn;
            isRunDataInit = true;
        }

        #endregion

        public static System.ComponentModel.BindingList<EventData> events = new System.ComponentModel.BindingList<EventData>();

        static Random rd = new Random();

        public delegate void UpdateData();
        public static UpdateData UpdateLineData { get; set; }
        public static UpdateData UpdateStationData { get; set; }
        public static UpdateData UpdateSwitchData { get; set; }
        public static UpdateData UpdatePlantData { get; set; }

        public static void clearAll()
        {
            tmr.Stop();
            UpdateLineData = UpdateStationData = UpdateSwitchData=UpdatePlantData = null;
            tmr.Tag = null;
            events.Clear();
        }

        ///<summary>产生实时数据</summary>
        static void tmr_Tick(object sender, EventArgs e)
        {
            genline((sender as System.Windows.Threading.DispatcherTimer).Tag as DistNet);
            if (UpdateLineData != null) UpdateLineData();
            genswitch((sender as System.Windows.Threading.DispatcherTimer).Tag as DistNet);
            if (UpdateSwitchData != null) UpdateSwitchData();
            gentranform((sender as System.Windows.Threading.DispatcherTimer).Tag as DistNet);
            if (UpdateStationData != null) UpdateStationData();
            genplant((sender as System.Windows.Threading.DispatcherTimer).Tag as DistNet);
            if (UpdatePlantData != null) UpdatePlantData();

        }

        ///<summary>产生开关数据</summary>
        static void genswitch(DistNet dn)
        {
            //暂取常态

            foreach (PowerBasicObject item in dn.getAllObjListByCategory(EObjectCategory.开关类))
            {
                RunDataSwitchBase rundata = item.busiRunData as RunDataSwitchBase;
                AcntSwitchBase acnt = item.busiAccount as AcntSwitchBase;
                rundata.isClose = acnt.switchStatus == ESwitchStatus.闭合;
            }

        }

        ///<summary>产生变电数据</summary>
        static void gentranform(DistNet dn)
        {
            foreach (DNTransformFacilityBase item in dn.getAllObjListByCategory(EObjectCategory.变电设施类))
            {
                RunDataTransformFacilityBase rundata = item.busiRunData as RunDataTransformFacilityBase;
                AcntTransformFacilityBase acnt = item.busiAccount as AcntTransformFacilityBase;
                rundata.activePower = acnt.cap * (0.4 + 0.5 * rd.NextDouble());
                //rundata.powerFactor = 0.8 + 0.2 * rd.NextDouble();
                //rundata.apparentPower = rundata.activePower / rundata.powerFactor;
                rundata.reactivePower = Math.Sqrt(Math.Pow(rundata.apparentPower, 2) - Math.Pow(rundata.activePower, 2));
                //rundata.rateOfLoad = rundata.apparentPower / acnt.cap;
                //rundata.HVoltPUV = 0.85 + 0.3 * rd.NextDouble();
                rundata.HVL = acnt.hnvl * rundata.HVoltPUV;

                if (rundata.lstApparentPower.Count==0) //初始模拟50条
                {
                    for (int i = 50; i > 0; i--)
                    {
                        rundata.lstApparentPower.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate=DateTime.Now.AddMinutes(i),value=acnt.cap * (0.4 + 0.5 * rd.NextDouble())});
                    }
                }
                //rundata.addApparentPower(rundata.apparentPower);
                rundata.refresh();
            }

            foreach (DNTransformerBase item in dn.getAllObjListByCategory(EObjectCategory.变压器类))
            {
                RunDataTransformerBase rundata = item.busiRunData as RunDataTransformerBase;
                AcntTransformBase acnt = item.busiAccount as AcntTransformBase;
                rundata.activePower = acnt.cap * (0.4 + 0.5 * rd.NextDouble());
                //rundata.powerFactor = 0.8 + 0.2 * rd.NextDouble();
                //rundata.apparentPower = rundata.activePower / rundata.powerFactor;
                rundata.reactivePower = Math.Sqrt(Math.Pow(rundata.apparentPower, 2) - Math.Pow(rundata.activePower, 2));
                //rundata.rateOfLoad = rundata.apparentPower / acnt.cap;
                //rundata.HVoltPUV = 0.85 + 0.3 * rd.NextDouble();
                rundata.HVL = acnt.hnvl * rundata.HVoltPUV;

                if (rundata.lstApparentPower.Count == 0) //初始模拟50条
                {
                    for (int i = 50; i > 0; i--)
                    {
                        rundata.lstApparentPower.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = DateTime.Now.AddMinutes(i), value = acnt.cap * (0.4 + 0.5 * rd.NextDouble()) });
                    }
                }
                //rundata.addApparentPower(rundata.apparentPower);
                rundata.refresh();

            }

            foreach (var item in dn.getAllObjListByObjType(EObjectType.节点))
            {
                RunDataNode rundata = item.busiRunData as RunDataNode;
                rundata.vl = 110;
                //rundata.voltPUV = 0.9 + 0.2 * rd.NextDouble();
                rundata.volt = rundata.voltPUV * rundata.vl;
            }

        }

        ///<summary>产生线路数据</summary>
        static void genline(DistNet dn)
        {
            foreach (DNACLineBase item in dn.getAllObjListByObjType(EObjectType.输电线路))
            {
                RunDataACLineBase rundata = item.busiRunData as RunDataACLineBase;
                AcntACLineBase acnt = item.busiAccount as AcntACLineBase;
                if (acnt.cap == 0) acnt.cap = rd.Next(50);
                rundata.activePower = acnt.cap * (0.4 + 0.5 * rd.NextDouble());
                //rundata.powerFactor = 0.8 + 0.2 * rd.NextDouble();
                //rundata.apparentPower = rundata.activePower / rundata.powerFactor;
                rundata.reactivePower = Math.Sqrt(Math.Pow(rundata.apparentPower, 2) - Math.Pow(rundata.activePower, 2));
                //rundata.rateOfLoad = rundata.apparentPower / acnt.cap;

                if (rd.NextDouble() < 0.01)
                {
                    if (events.Count > 50)
                        events.Remove(events.Last());
                    events.Insert(0, new EventData() { eObjID = item.id, startTime = DateTime.Now, eType = (EventData.EEventType)rd.Next(2), eTitle = item.name + "事件." , eContent=DateTime.Now.ToShortDateString()+item.name+"事件描述......"});

                }



                if (item is DNACLine)
                {
                    if (rundata.lstApparentPower.Count == 0) //初始模拟50条
                    {
                        for (int i = 50; i > 0; i--)
                        {
                            rundata.lstApparentPower.Add(new MyClassLibrary.DevShare.ChartDataPoint() { argudate = DateTime.Now.AddMinutes(i), value = acnt.cap * (0.4 + 0.5 * rd.NextDouble()) });
                        }
                    }
                    //rundata.addApparentPower(rundata.apparentPower);
                    rundata.refresh();
                }

            }
        }

        static void genplant(DistNet dn)
        {
            foreach (DNPlantBase item in dn.getAllObjListByCategory(EObjectCategory.电厂设施类))
            {
                RunDataPlantBase rundata = item.busiRunData as RunDataPlantBase;
                AcntPlantBase acnt = item.busiAccount as AcntPlantBase;
                rundata.rateOfLoad = 0.8 + 0.2 * rd.NextDouble();
                rundata.power = rundata.rateOfLoad * acnt.cap;
            }

        }



        public static void StartGenData(DistNet dn)
        {
            genline(dn); gentranform(dn); genswitch(dn); genplant(dn);
            tmr.Start();
        }


        public static void StopGenData()
        {
            tmr.Stop();
        }


    }
}
