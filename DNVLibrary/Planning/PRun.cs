using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using WpfEarthLibrary;
using DistNetLibrary;
using System.Data;

namespace DNVLibrary.Planning
{
    class PRun : BaseMain
    {
        public PRun(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {
            object container = VisualTreeHelper.GetParent(_Global.InstanceSelector);
            if (container != null)
                (container as System.Windows.Controls.StackPanel).Children.Remove(_Global.InstanceSelector);
            toolboxSub.Children.Add(_Global.InstanceSelector);

            // 工具栏初始化
            //for (int i = 2015; i < 2026; i++)
            //    cmbYear.Items.Add(i);
            //cmbYear.SelectedValue = 2020;
            //toolboxSub.Children.Add(cmbYear);
            //cmbYear.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(cmbYear_SelectionChanged);

            zButton btn;
            btn = new zButton() { text = "潮流", group = "1" };
            btn.Click += new System.Windows.RoutedEventHandler(btnFlow_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "负载率", group = "1" };
            btn.Click += new System.Windows.RoutedEventHandler(btnLoad_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "电压", group = "1", Margin = new System.Windows.Thickness(0, 0, 10, 0) };
            btn.Click += new System.Windows.RoutedEventHandler(btnVL_Click);
            toolboxSub.Children.Add(btn);

            //btn = new zButton() { text = "断面潮流", group = "5" };
            //btn.Click += new System.Windows.RoutedEventHandler(btnSection_Click);
            //toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "电源追溯", group = "2" };
            btn.Click += new System.Windows.RoutedEventHandler(btnTrace_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "供电范围", group = "3" };
            btn.Click += new System.Windows.RoutedEventHandler(btnRange_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "负荷等值图", group = "6" };
            btn.Click += new System.Windows.RoutedEventHandler(btnLoadContour_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "区块负荷预测", group = "4" };
            btn.Click += new System.Windows.RoutedEventHandler(btnLoadForcast_Click);
            toolboxSub.Children.Add(btn);
        }

   
        //void cmbYear_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //{
        //    if (app != null)
        //        UCDNV863.CmdPlanningDateChanged.Execute(this, app.panel);
        //}
        //System.Windows.Controls.ComboBox cmbYear = new System.Windows.Controls.ComboBox() { VerticalContentAlignment = System.Windows.VerticalAlignment.Center }; //规划年选择
        //internal int PlanningYear { get { return (int)cmbYear.SelectedValue; } }


        AppBase app;
        //潮流
        void btnFlow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunFlow)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunFlow))  //不为本身，执行创建
                {
                    app = new Planning.PRunFlow(root);
                    app.begin();
                }
            }
            (app as PRunFlow).ShowFlow(btn.isChecked);
        }
        //负载
        void btnLoad_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunFlow)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunFlow))  //不为本身，执行创建
                {
                    app = new Planning.PRunFlow(root);
                    app.begin();
                }
            }
            (app as PRunFlow).ShowLoad(btn.isChecked);
        }
        //电压
        void btnVL_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunFlow)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunFlow))  //不为本身，执行创建
                {
                    app = new Planning.PRunFlow(root);
                    app.begin();
                }
            }
            (app as PRunFlow).ShowVL(btn.isChecked);
        }
        //负荷等值图
        void btnLoadContour_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunLoadContour)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunLoadContour))  //不为本身，执行创建
                {
                    app = new Planning.PRunLoadContour(root);
                    app.begin();
                }

            }
            (app as Planning.PRunLoadContour).showhide(btn.isChecked);
        }
        //断面
        void btnSection_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunSection)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunSection))  //不为本身，执行创建
                {
                    app = new Planning.PRunSection(root);
                    app.begin();
                }
            }

        }

        //区块负荷预测
        void btnLoadForcast_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunLoadForcast)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunLoadForcast))  //不为本身，执行创建
                {
                    app = new Planning.PRunLoadForcast(root);
                    app.begin();
                }
            }
            else
            {
                if (app!=null && (app is Planning.PRunLoadForcast))
                {
                    app.end();
                    app = null;
                }
            }

        }
        //供电范围
        void btnRange_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunSupplyRange)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunSupplyRange))  //不为本身，执行创建
                {
                    app = new Planning.PRunSupplyRange(root);
                    app.begin();
                }
            }
        }
        //电源追溯
        void btnTrace_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunRetrospect)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunRetrospect))  //不为本身，执行创建
                {
                    app = new Planning.PRunRetrospect(root);
                    app.begin();
                }
            }
        }





        protected override void load()  //进入时装载数据
        {
            //状态栏提示添加
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.push();
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "规划_模拟运行";


            loaddata();

            _Global.InstanceSelector.cmbTree.SelectItemChanged += new EventHandler(cmbTree_SelectItemChanged);

        }

        void cmbTree_SelectItemChanged(object sender, EventArgs e)
        {
            DataProvider.PlanningRunDataRead(root.distnet, _Global.curInstanceID);
            //更新显示
            if (app != null)
                UCDNV863.CmdPlanningDateChanged.Execute(this, app.panel);

        }
        protected override void unload()  //退出时卸载数据
        {
            if (app != null) app.end();
            //状态栏提示恢复
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.pop();

            _Global.InstanceSelector.cmbTree.SelectItemChanged -= new EventHandler(cmbTree_SelectItemChanged);
        }


        Random rd = new Random();
        void loaddata()
        {
            if (UCDNV863.EDISTNET == UCDNV863.EDistnet.亦庄16)
            {
                #region ----- gis16数据模拟或载入 -----
                //逐项填写运行数据
                //载入潮流
                List<PowerBasicObject> objs = root.distnet.dbdesc["基础数据"].DictSQLS["导线段"].batchLoadRunData(root.distnet, false);
                foreach (var item in objs)
                {
                    DNLineSeg obj = item as DNLineSeg;
                    obj.isInverse = obj.thisRunData.activePower < 0; //校验方向
                    obj.tooltipMoveTemplate = "PlanningLineTemplate";
                    obj.tooltipMoveContent = obj.busiRunData;
                }
                //载入变电站
                objs = root.distnet.dbdesc["基础数据"].DictSQLS["变电站"].batchLoadRunData(root.distnet, false);
                foreach (var obj in objs)
                {
                    obj.tooltipMoveTemplate = "PlanningSubstationTemplate";
                    obj.tooltipMoveContent = obj.busiRunData;
                }
                //载入主变压器
                objs = root.distnet.dbdesc["基础数据"].DictSQLS["主变压器"].batchLoadRunData(root.distnet, false);
                foreach (var obj in objs)
                {
                    obj.tooltipMoveTemplate = "PlanningSubstationTemplate";
                    obj.tooltipMoveContent = obj.busiRunData;
                }

                #endregion
            }
            else if (UCDNV863.EDISTNET == UCDNV863.EDistnet.亦庄new)
            {
                DataProvider.PlanningRunDataRead(root.distnet,_Global.curInstanceID);

                #region ----- 新亦庄数据模拟或载入 -----
                ////逐项填写运行数据
                ////载入潮流
                //List<PowerBasicObject> objs = root.distnet.dbdesc["基础数据"].DictSQLS["线路"].batchLoadRunData(root.distnet, false);
                //foreach (var item in objs)
                //{
                //    DNACLine obj = item as DNACLine;
                //    obj.isInverse = obj.thisRunData.activePower < 0; //校验方向
                //    obj.tooltipMoveTemplate = "PlanningLineTemplate";
                //    obj.tooltipMoveContent = obj.busiRunData;
                //}
                ////载入变电站
                //objs = root.distnet.dbdesc["基础数据"].DictSQLS["变电站"].batchLoadRunData(root.distnet, false);
                //foreach (var obj in objs)
                //{
                //    obj.tooltipMoveTemplate = "PlanningSubstationTemplate";
                //    obj.tooltipMoveContent = obj.busiRunData;
                //}
                ////载入主变压器
                //objs = root.distnet.dbdesc["基础数据"].DictSQLS["主变2卷"].batchLoadRunData(root.distnet, false);
                //foreach (var obj in objs)
                //{
                //    obj.tooltipMoveTemplate = "PlanningSubstationTemplate";
                //    obj.tooltipMoveContent = obj.busiRunData;
                //}

                #endregion
            }
            else
            {

                #region ------ gis15数据模拟域载入 ------

                //逐项填写运行数据
                //载入潮流
                string id; PowerBasicObject obj;
                DataTable dt = DataLayer.DataProvider.getDataTableFromSQL("select f_id id,f_mch name,f_begyg yg,f_begwg wg,f_begyg/1000 fzl from MCR_ACFLOW_BRANCH union select f_id id,f_mch name,F_SHDYG yg,F_SHDWG wg,f_fzl fzl from HCR_ACFLOW_BRANCH");
                Dictionary<string, PowerBasicObject> objs;
                if (root.earth.objManager.zLayers.ContainsKey(EObjectCategory.导线类.ToString()))
                {
                    //objs = root.earth.objManager.zLayers[EObjectCategory.导线类.ToString()].pModels;
                    objs = root.distnet.getAllObjDictByCategory(EObjectCategory.导线类);
                    foreach (DataRow dr in dt.Rows)
                    {
                        id = dr["id"].ToString();
                        if (objs.TryGetValue(id, out obj))
                        {
                            if (obj.busiRunData == null) obj.createRunData();
                            RunDataACLineBase rdata = obj.busiRunData as RunDataACLineBase;
                            //rdata.rateOfLoad = dr.getDouble("fzl");
                            rdata.activePower = dr.getDouble("yg");
                            rdata.reactivePower = dr.getDouble("wg");
                            (obj as pPowerLine).isInverse = rdata.activePower < 0; //校验方向

                            obj.tooltipMoveTemplate = "PlanningLineTemplate";
                            obj.tooltipMoveContent = obj.busiRunData;
                        }

                    }
                }

                //载入变电站
                dt = DataLayer.DataProvider.getDataTableFromSQL("select f_id id, f_mch name,f_ygfh yg,f_wgfh wg, f_fzl fzl, f_cos fcos, null ryd from MCR_ACFLOW_SS union select f_id id,F_MCH name, F_YGFH yg, F_WGFH wg,f_fzl fzl,f_cos fcos,f_ryd ryd from HCR_ACFLOW_SSLR");
                objs = root.earth.objManager.zLayers[EObjectCategory.变电设施类.ToString()].pModels; // root.earth.objManager.getAllObjDictBelongtoCategory("变电站");
                foreach (DataRow dr in dt.Rows)
                {
                    id = dr["id"].ToString();
                    if (objs.TryGetValue(id, out obj))
                    {
                        if (obj.busiRunData == null) obj.createRunData();
                        RunDataSubstation rdata = obj.busiRunData as RunDataSubstation;
                        rdata.activePower = dr.getDouble("yg");
                        rdata.reactivePower = dr.getDouble("wg");
                        //rdata.rateOfLoad = dr.getDouble("fzl") / 100;
                        //rdata.powerFactor = dr.getDouble("fcos");
                        rdata.redundancy = dr.getDouble("ryd");
                        rdata.HVL = 90 + rd.Next(40); //zh 注模拟
                        //rdata.HVoltPUV = rdata.HVL / 110;

                        obj.tooltipMoveTemplate = "PlanningSubstationTemplate";
                        obj.tooltipMoveContent = obj.busiRunData;
                    }
                }
                //zh注，强制模拟配电室
                foreach (var item in root.distnet.getAllObjListByObjType(EObjectType.配电室))
                {
                    if (item.busiRunData == null) item.createRunData();
                    RunDataTransformFacilityBase rdata = item.busiRunData as RunDataTransformFacilityBase;
                    //rdata.rateOfLoad = (30.0 + rd.Next(60)) / 100;
                    rdata.HVL = 8 + rd.Next(4);
                    //rdata.HVoltPUV = rdata.HVL / 10;
                }

                //载入变压器
                string sql = @"
                select f_id id, f_mch name, f_fh yg,f_wgfh wg,f_fzl fzl,f_glysh fcos,f_gycyxdy hvl,f_zsh allloss,f_shzgl ap,f_tgs closs,f_ts iloss,f_bs tloss,f_bsl tlr from HCR_ACFLOW_MTTV
                union 
                select f_id id, f_mch name, f_fh yg,f_wgfh wg,f_fzl fzl,f_glysh fcos,f_gycyxdy hvl,f_zsh allloss,f_shzgl ap,f_tgs closs,f_ts iloss,f_bs tloss,f_bsl tlr from HCR_ACFLOW_MTTHV
                union 
                select f_id id, f_mch name, f_ygfh yg,f_wgfh wg,f_fzl fzl,f_cos fcos,0 hvl,f_zws allloss,sqrt(f_ygfh*f_ygfh+f_wgfh*f_wgfh) ap,f_pbcus closs,f_pbfes iloss,f_pbsh tloss,f_pbsl tlr from MCR_ACFLOW_MT
                ";
                dt = DataLayer.DataProvider.getDataTableFromSQL(sql);
                if (root.earth.objManager.zLayers.ContainsKey(EObjectCategory.变压器类.ToString()))
                {
                    objs = root.earth.objManager.zLayers[EObjectCategory.变压器类.ToString()].pModels;// root.earth.objManager.getAllObjDictBelongtoCategory("变压器");
                    foreach (DataRow dr in dt.Rows)
                    {
                        id = dr["id"].ToString();
                        if (objs.TryGetValue(id, out obj))
                        {
                            RunDataTransformerBase rundata = new RunDataTransformerBase(obj)
                            {
                                activePower = double.Parse(dr["yg"].ToString()),
                                reactivePower = double.Parse(dr["wg"].ToString()),
                                //rateOfLoad = double.Parse(dr["fzl"].ToString()) / 100,
                                //powerFactor = double.Parse(dr["fcos"].ToString()),
                                HVL = double.Parse(dr["hvl"].ToString()),
                                allLoss = double.Parse(dr["allloss"].ToString()),
                                //apparentPower = double.Parse(dr["ap"].ToString()),
                                copperLoss = double.Parse(dr["closs"].ToString()),
                                ironLoss = double.Parse(dr["iloss"].ToString()),
                                transformLoss = double.Parse(dr["tloss"].ToString()),
                                transformLossRate = double.Parse(dr["tlr"].ToString()),
                            };

                            obj.busiRunData = rundata;
                            obj.tooltipMoveTemplate = "PlanningTransformerTemplate";
                            obj.tooltipMoveContent = obj.busiRunData;
                        }
                    }
                }


                //======= 载入节点数据
                dt = DataLayer.DataProvider.getDataTableFromSQL("select * from MCR_ACFLOW_node");
                //objs = root.earth.objManager.getAllObjDictBelongtoCategory("节点");
                objs = root.distnet.getAllObjDictByObjType(EObjectType.节点);
                foreach (DataRow dr in dt.Rows)
                {
                    id = dr["f_id"].ToString();
                    if (objs.TryGetValue(id, out obj))
                    {
                        RunDataNode rundata = new RunDataNode(obj)
                        {
                            activePower = dr.getDouble("f_yg"),
                            reactivePower = dr.getDouble("f_wg"),
                            volt = dr.getDouble("f_dy"),
                            //voltPUV = dr.getDouble("f_dy_by"),
                        };
                        obj.busiRunData = rundata;
                    }

                }



                ////载入3卷变压器
                //dt = DataLayer.DataProvider.getDataTableFromSQL("select * from HCR_ACFLOW_MTTHV");
                //objs = root.earth.objManager.getAllObjDictBelongtoCategory("3卷变压器");
                //foreach (DataRow dr in dt.Rows)
                //{
                //    id = dr["f_id"].ToString();
                //    if (objs.TryGetValue(id, out obj))
                //    {
                //        RunDataTransformer3P rundata = new RunDataTransformer3P()
                //        {
                //            name = dr["F_MCH"].ToString(),
                //            activePower = double.Parse(dr["F_FH"].ToString()),
                //            reactivePower = double.Parse(dr["F_WGFH"].ToString()),
                //            rateOfLoad = double.Parse(dr["F_FZL"].ToString()) / 100,
                //            powerFactor = double.Parse(dr["F_GLYSH"].ToString()),
                //            HVL = double.Parse(dr["F_gycyxdy"].ToString()),
                //            allLoss = double.Parse(dr["F_zsh"].ToString()),
                //            apparentPower = double.Parse(dr["F_shzgl"].ToString()),
                //            copperLoss = double.Parse(dr["F_tgs"].ToString()),
                //            ironLoss = double.Parse(dr["F_ts"].ToString()),
                //            transformLoss = double.Parse(dr["F_bs"].ToString()),
                //            transformLossRate = double.Parse(dr["F_bsl"].ToString()),
                //        };

                //        obj.busiRunData = rundata;
                //        obj.tooltipMoveTemplate = "PlanningTransformer3Template";
                //        obj.tooltipMoveContent = obj.busiRunData;
                //    }
                //}

                #endregion
            }
           
        }

    }
}
