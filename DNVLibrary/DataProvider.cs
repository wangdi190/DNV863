using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;
using DistNetLibrary;


namespace DNVLibrary
{
    /// <summary>
    /// 新的统一数据提供器静态类
    /// </summary>
    internal static class DataProvider
    {
        static DataProvider()
        {
            tmrreal.Tick += new EventHandler(tmrreal_Tick);
        }


        #region =========== 公用 ==========
        public delegate void DataUpdated();

        ///<summary>清理所有对象的运行数据，模板，内容</summary>
        static void clearAll(DistNet dn)
        {
            foreach (var layer in dn.scene.objManager.zLayers.Values)
            {
                foreach (var item in layer.pModels.Values)
                {
                    item.busiRunData =item.tooltipMoveContent =item.tooltipMoveTemplate = null;
                }
            }

        }

        #endregion


        #region  ========== 实时数据 ==========
        static System.Windows.Threading.DispatcherTimer tmrreal = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };
        public static System.ComponentModel.BindingList<EventData> events = new System.ComponentModel.BindingList<EventData>();
        
        private static int _realDataReadInterval=10;
        ///<summary>实时数据读取间隔，单位秒，缺省10秒</summary>
        public static int RealDataReadInterval
        {
            get { return _realDataReadInterval; }
            set { _realDataReadInterval = value; tmrreal.Interval = TimeSpan.FromSeconds(value); }
        }

        ///<summary>实时数据更新后，委托执行方法</summary>
        public static DataUpdated RealDataUpdated { get; set; }

        ///<summary>初始化实时运行数据类并设置相应tooltipmove模板</summary>
        static void initRealRunData(DistNet dn)
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

            tmrreal.Tag = dn;
        }


        ///<summary>方法将创建rundata对象设置模板，并开始计时读取实时运行数据</summary>
        public static void RealDataReadStart(DistNet dn)
        {
            initRealRunData(dn);

            readrealdata();
            tmrreal.Start();
        }

        ///<summary>停止读取实时运行数据, 并清理相关对象</summary>
        public static void RealDataReadStop(DistNet dn)
        {
            tmrreal.Stop();
            clearRealData(dn);
        }
        ///<summary>清理实时运行数据，包括停止计时器，清除事件列表, 清理模板和内容指向</summary>
        static void clearRealData(DistNet dn)
        {
            tmrreal.Stop();
            clearAll(dn);
            tmrreal.Tag = null;
            events.Clear();
        }



        static void tmrreal_Tick(object sender, EventArgs e)
        {
            readrealdata();
        }

        static void readrealdata()
        {



            if (RealDataUpdated != null)
                RealDataUpdated();
        }

        


        #endregion



        #region  ========== 规划模拟运行数据 ==========

        ///<summary>实时数据更新后，委托执行方法</summary>
        public static DataUpdated PlanningDataUpdated { get; set; }


        ///<summary>初始化规划模拟运行数据类并设置相应tooltipmove模板</summary>
        static void initPlanningRunData(DistNet dn)
        {
            //变电设施类
            foreach (DNTransformFacilityBase item in dn.getAllObjListByCategory(EObjectCategory.变电设施类))
            {
                item.createRunData();
                item.tooltipMoveTemplate = "PlanningSubstationTemplate";
                item.tooltipMoveContent = item.busiRunData;
            }
            //变压器类
            foreach (DNTransformerBase item in dn.getAllObjListByCategory(EObjectCategory.变压器类))
            {
                item.createRunData();
                item.tooltipMoveTemplate = "PlanningSubstationTemplate";
                item.tooltipMoveContent = item.busiRunData;
            }
            //导线类
            foreach (DNACLineBase item in dn.getAllObjListByCategory(EObjectCategory.导线类))
            {
                item.createRunData();
                if (item is DNACLine)  //仅输电线路tooltip
                {
                    item.tooltipMoveTemplate = "PlanningLineTemplate";
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


        ///<summary>开始读取规划运行数据</summary>
        public static void PlanningRunDataRead(DistNet dn, int instanceID)
        {
            initPlanningRunData(dn);

            //逐项填写运行数据
            //载入潮流

            

            List<PowerBasicObject> objs = dn.dbdesc["基础数据"].DictSQLS["线路"].batchLoadRunData(dn, false, instanceID);
            foreach (var item in objs)
            {
                DNACLine obj = item as DNACLine;
                obj.isInverse = obj.thisRunData.activePower < 0; //校验方向
            }
            //载入变电站
            objs = dn.dbdesc["基础数据"].DictSQLS["变电站"].batchLoadRunData(dn, false,instanceID);
            //载入配电室            //注：配电无功率数据
            objs = dn.dbdesc["基础数据"].DictSQLS["配电室"].batchLoadRunData(dn, false, instanceID);
            //载入主变压器
            objs = dn.dbdesc["基础数据"].DictSQLS["主变2卷"].batchLoadRunData(dn, false,instanceID);
            //载入配变压器
            objs = dn.dbdesc["基础数据"].DictSQLS["配变"].batchLoadRunData(dn, false, instanceID);
            //断路器状态
            objs = dn.dbdesc["基础数据"].DictSQLS["断路器"].batchLoadRunData(dn, false, instanceID);
         
 
            //网格数据
            objs = dn.dbdesc["基础数据"].DictSQLS["网格"].batchLoadRunData(dn, false, instanceID);

        }

        ///<summary>清理rundata对象、模板、内容</summary>
        public static void PlanningRunDataClear(DistNet dn)
        {
            clearAll(dn);
        }

        #endregion

        #region  ========== 其它分析运行数据 ==========


        #endregion

    }
}
