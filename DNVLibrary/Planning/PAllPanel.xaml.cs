using System;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DNVLibrary.Planning
{
    /// <summary>
    /// PAllPanel.xaml 的交互逻辑
    /// </summary>
    public partial class PAllPanel : UserControl
    {
        public PAllPanel(DistNetLibrary.DistNet Distnet)
        {
            distnet = Distnet;
            InitializeComponent();
        }

        internal DistNetLibrary.DistNet distnet;

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            //simData();
            loadData();

            timeline = new PAllTimeLine(this) { VerticalAlignment = System.Windows.VerticalAlignment.Bottom, Margin = new Thickness(250, 0, 250, 20) };
            timeline.selYearChanged += new EventHandler(timeline_selYearChanged);
            timeline.projectChanged += new EventHandler(timeline_projectChanged);
            timeline.PlayBegin += new EventHandler(timeline_PlayBegin);
            grdMain.Children.Add(timeline);
            timeline.selYear = startYear;

            gauge.btnMoreIndex.Click += new RoutedEventHandler(btnMoreIndex_Click);
            gauge.lstObjects.MouseDoubleClick += new MouseButtonEventHandler(lstObjects_MouseDoubleClick);
        }

        void lstObjects_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WpfEarthLibrary.PowerBasicObject obj = gauge.lstObjects.SelectedItem as WpfEarthLibrary.PowerBasicObject;
            distnet.scene.camera.aniLook(obj.VecLocation, 5);

        }


        void timeline_PlayBegin(object sender, EventArgs e)
        {
            chkFlow.IsChecked = chkLoad.IsChecked = chkVL.IsChecked = chkNP1.IsChecked = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        ///<summary>年选中事件</summary>
        void timeline_selYearChanged(object sender, EventArgs e)
        {
            grdPrjview.Children.Clear();
            grdPrjview.Children.Add(years[timeline.selYear].view);

            pnlCompare.Visibility = years[timeline.selYear].projects.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

            if (aniidx.To == 1)  //年变化处理指标对比
            {
                if (years[timeline.selYear].projects.Count < 2)
                    hideindexcompare();
                else
                    showindexcompare();

            }
        }
        void timeline_projectChanged(object sender, EventArgs e)
        {
            oldprj = curprj;
            curprj = years[timeline.selYear].view.selprj.Tag as ProjectData;
            prjchange();
        }

        ProjectData oldprj, curprj;
        YearData curyear { get { return years[timeline.selYear]; } }

        PAllTimeLine timeline;

        Random rd = new Random();
        internal int startYear;
        internal int endYear;
        internal Dictionary<int, YearData> years = new Dictionary<int, YearData>();


        void loadData()
        {

            startYear = global.instances.Min(p => p.year);
            endYear = global.instances.Max(p => p.year);

            //重构所有实例信息，载入指标数据
            foreach (InstanceData inst in global.instances)
            {
                YearData yeardata = null;
                if (!years.TryGetValue(inst.year, out yeardata))
                {
                    yeardata = new YearData() { year = inst.year };
                    years.Add(inst.year, yeardata);
                }
                ProjectData pd = new ProjectData() { prjid = inst.instanceID, year = inst.year, name = inst.instanceName, note = inst.note };
                loadindex(pd);  //载入指标

                yeardata.projects.Add(pd);
            }
            gauge.nowPrj = years[years.Min(p => p.Key)].projects[0];  //初始化仪表为现状年

            //----- 创建方案集合视图
            foreach (var item in years.Values)
            {
                PAllYearPorjects prjview = new PAllYearPorjects(item) { Margin = new Thickness(20, 0, 0, 50) };
                item.view = prjview;
                prjview.projectChanged += new EventHandler(prjview_projectChanged);
            }

            // 填写方案增量设备引用
            foreach (var yeardata in years)
            {
                if (yeardata.Value.year == startYear) continue;
                foreach (var prj in yeardata.Value.projects)
                {
                    prj.addobjs = global.getParentAdditionObjects(prj.prjid).Values.ToList();

                    foreach (var item in prj.addobjs)  //若无增量设备的生效日期，则强制赋于为当年7.1日
                    {
                        if ((item.busiAccount as DistNetLibrary.AcntDistBase).runDate.Year < prj.year)
                            (item.busiAccount as DistNetLibrary.AcntDistBase).runDate = new DateTime(prj.year, 7, 1);
                    }
                }
            }

        }

        void simData()
        {

            startYear = 2014;
            endYear = 2015;

            int id = 1;
            YearData year = new YearData() { year = startYear };
            ProjectData pd;
            pd = new ProjectData() { prjid = id, year = year.year, name = " 配网现状", note = "配网现状情况。" };
            simindex(pd, (1.0 * year.year - startYear) / (endYear - startYear));
            year.projects.Add(pd);
            years.Add(startYear, year);
            gauge.nowPrj = pd;  //填写现状数据
            id++;
            year = new YearData() { year = endYear };
            pd = new ProjectData() { prjid = id, year = year.year, name = " 规划方案" + id, note = string.Format(" 规划方案{0}。", id) };
            simindex(pd, (1.0 * year.year - startYear) / (endYear - startYear));
            year.projects.Add(pd);
            years.Add(endYear, year);
            //for (int i = 0; i < 10 + rd.Next(7); i++)
            //{
            //    id++;
            //    int tmp = startYear + rd.Next(endYear - startYear);
            //    if (tmp == startYear) continue;
            //    if (!years.TryGetValue(tmp, out year))
            //    {
            //        year = new YearData() { year = tmp };
            //        years.Add(tmp, year);
            //    }
            //    pd = new ProjectData() { prjid = id, year = year.year, name = " 规划方案" + id, note = string.Format(" 规划方案{0}。", id) };
            //    simindex(pd, (1.0 * year.year - startYear) / (endYear - startYear));
            //    year.projects.Add(pd);

            //}
            id++;

            //----- 创建方案集合视图


            foreach (var item in years.Values)
            {
                PAllYearPorjects prjview = new PAllYearPorjects(item) { Margin = new Thickness(20, 0, 0, 50) };
                item.view = prjview;
                prjview.projectChanged += new EventHandler(prjview_projectChanged);
            }

            //----- 模拟投退运,仅设施、主配变、连接线路
            IEnumerable<WpfEarthLibrary.PowerBasicObject> tmpobjs;
            IEnumerable<WpfEarthLibrary.PowerBasicObject> objs = distnet.getAllObjListByCategory(DistNetLibrary.EObjectCategory.变电设施类);
            tmpobjs = distnet.getAllObjListByCategory(DistNetLibrary.EObjectCategory.开关设施类);
            objs = objs.Union(tmpobjs);
            objs = distnet.getAllObjListByCategory(DistNetLibrary.EObjectCategory.导线类);
            objs = objs.Union(tmpobjs);
            tmpobjs = distnet.getAllObjListByCategory(DistNetLibrary.EObjectCategory.变压器类);
            objs = objs.Union(tmpobjs);
            List<WpfEarthLibrary.PowerBasicObject> allobjs = objs.ToList();

            var allprjs = from e0 in years.Values
                          from e1 in e0.projects
                          select e1;
            foreach (ProjectData prj in allprjs.OrderBy(p => p.year))
            {
                if (prj.year == startYear) continue;  //跳过现状年
                for (int i = 0; i < 2 + rd.Next(3); i++)
                {
                    WpfEarthLibrary.PowerBasicObject obj = allobjs[rd.Next(allobjs.Count)];
                    if (obj.busiAccount is DistNetLibrary.AcntDistBase)
                    {
                        (obj.busiAccount as DistNetLibrary.AcntDistBase).runDate = new DateTime(prj.year, 1 + rd.Next(12), 1);  //模拟增加投运日期
                        prj.addobjs.Add(obj);  //加该设备为方案增量设备
                        obj.logicVisibility = false;  //初始设定该增量设备不可见
                    }
                }
            }



        }

        ///<summary>读取核心指标</summary>
        void loadindex(ProjectData pd)
        {
            //注：从指标表中读取数据，并改写预定的指标
            string sql = string.Format("select t1.id,t1.indexname,t1.unit,t1.format,t1.minvalue,t1.maxvalue,t1.note,t2.Value from Dic_Index t1,Anal_Index t2 where Category='求实' and t2.InstanceID={0} and t1.ID=t2.IndexID", pd.prjid);
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string indexname = dr.Field<string>("indexname");
                IndexBase idx = null;
                if (indexname == "供电可靠率")  //改写idx1核心指标
                {
                    idx = pd.idx2;
                    idx.gaugeformat = "{0:p3}";
                }
                else if (indexname == "110kV容载比")  //改写idx2核心指标
                {
                    idx = pd.idx1;
                }
                else if (indexname == "清洁能源渗透率")  //改写idx3核心指标
                {
                    idx = pd.idx3;
                }
                if (idx != null)
                {
                    idx.format = dr.Field<string>("format");
                    idx.unit = dr.Field<string>("unit");
                    idx.note = dr.Field<string>("note");
                    idx.name = indexname;
                    idx.min = dr.getDouble("minvalue");
                    idx.max = dr.getDouble("maxvalue");
                    idx.value = dr.getDouble("value");
                }
            }
            //填充计算其它指标
            var tmp1 = global.getObjDictByFlag(pd.prjid, "主变").Select(p => p.Value.busiAccount as DistNetLibrary.AcntTransformBase);
            pd.idxes.indexes["主变台数"].value = tmp1.Count();
            pd.idxes.indexes["主变容量（kW）"].value = tmp1.Sum(p => p.cap);
            var tmp2 = global.getObjDictByFlag(pd.prjid, "DG").Select(p => p.Value.busiAccount as DistNetLibrary.AcntPlantBase);
            pd.idxes.indexes["DG容量（kW）"].value = tmp2.Sum(p => p.cap);
            var tmp3 = global.getObjDictByFlag(pd.prjid, "线路").Select(p => p.Value.busiAccount as DistNetLibrary.AcntACLine);
            pd.idxes.indexes["线路长度（km）"].value = tmp3.Sum(p => p.len);
            var tmp4 = global.getObjDictByFlag(pd.prjid, "配变").Select(p => p.Value.busiAccount as DistNetLibrary.AcntTransformBase);
            pd.idxes.indexes["配变台数"].value = tmp4.Count();
            pd.idxes.indexes["配变容量（kW）"].value = tmp4.Sum(p => p.cap);

        }

        void simindex(ProjectData pd, double xs)
        {
            pd.idx1.value = 1.5 + rd.NextDouble();
            pd.idx2.value = 0.1 + 0.2 * rd.NextDouble();
            pd.idx3.value = 0.99 + 0.0099 * rd.NextDouble();

            foreach (var item in pd.idxes.indexes)
            {
                item.Value.simData(xs * (0.95 + 0.1 * rd.NextDouble()));
            }
        }


        void prjview_projectChanged(object sender, EventArgs e)
        {
            oldprj = curprj;
            curprj = (sender as PAllYearPorjects).selprj.Tag as ProjectData;
            prjchange();
        }

        ///<summary>方案改变</summary>
        void prjchange()
        {
            txtPrjName.Text = String.Format("{0}（{1}年）", curprj.name, curprj.year);
            gauge.prjdata = curprj;

            List<WpfEarthLibrary.pLayer> layers = new List<WpfEarthLibrary.pLayer>() { distnet.scene.objManager.zLayers["规划层"], distnet.scene.objManager.zLayers["网格"] };
            global.showInstanceObject(layers, curprj.prjid);

            loadRundata();

            ////===== 设备变化
            //var allprjs = from e0 in years.Values
            //              from e1 in e0.projects.Where(p => p.year > curprj.year || (p.year == curprj.year && p != curprj))
            //              select e1;
            //foreach (var e0 in allprjs)  //大于选定方案规划年的，以及同规划年的其它方案的增量设备隐藏 注：暂没实现退运和改造
            //{
            //    foreach (var e1 in e0.addobjs)
            //    {
            //        e1.logicVisibility = false;
            //    }
            //}

            var allprjs = from e0 in years.Values
                          from e1 in e0.projects.Where(p => p.year < curprj.year || p == curprj)
                          select e1;
            foreach (var e0 in allprjs)  //小于选定方案规划年的，本方案直接显示  注：暂没实现退运和改造
            {
                foreach (var e1 in e0.addobjs)
                {
                    e1.color = Colors.Cyan;
                    e1.logicVisibility = true;
                }
            }
            distnet.scene.UpdateModel();

            foreach (var e1 in curprj.addobjs)   //本方案，闪烁
            {
                e1.Progress = 1;
                if (e1 is WpfEarthLibrary.pPowerLine)
                {
                    e1.color = Colors.Red;
                    (e1 as WpfEarthLibrary.pPowerLine).aniTwinkle.doCount = 20;
                    (e1 as WpfEarthLibrary.pPowerLine).AnimationBegin(WpfEarthLibrary.pPowerLine.EAnimationType.闪烁);
                }
                else if (e1 is WpfEarthLibrary.pSymbolObject)
                {
                    e1.color = Colors.Red;
                    (e1 as WpfEarthLibrary.pSymbolObject).aniTwinkle.doCount = 20;
                    (e1 as WpfEarthLibrary.pSymbolObject).AnimationBegin(WpfEarthLibrary.pSymbolObject.EAnimationType.闪烁);
                }
            }




            //===== 运行显示
            //Run.DataGenerator.StartGenData(distnet);
            //Run.DataGenerator.StopGenData();
            refreshScreen();

        }

        ///<summary>载入当前实例运行数据</summary>
        void loadRundata()
        {
            //重新载入运行数据, 指标数据已一次性全载入
            //载入潮流数据
            DataProvider.PlanningRunDataRead(distnet, curprj.prjid);
            //载入网格数据
            List<WpfEarthLibrary.PowerBasicObject> objs = distnet.dbdesc["基础数据"].DictSQLS["网格"].batchLoadRunData(distnet, false, curprj.prjid);
            if (objs.Count > 0)
            {
                double maxLoadDensity = objs.Max(p => (p.busiRunData as DistNetLibrary.RunDataGridArea).loadDensity);
                foreach (var obj in objs)
                {
                    obj.tooltipMoveTemplate = "PlanningGridAreaTemplate";
                    obj.tooltipMoveContent = obj.busiRunData;
                    obj.color = MyClassLibrary.Share2D.MediaHelper.getColorBetweenRedBlue((obj.busiRunData as DistNetLibrary.RunDataGridArea).loadDensity / maxLoadDensity);
                }
            }
            //载入可靠性数据




        }

        void refreshScreen()
        {
            if ((bool)chkFlow.IsChecked)
                distnet.showFlow(null, null, false, true);
            if ((bool)chkLoad.IsChecked)
                distnet.showLoadCol();
            if ((bool)chkVL.IsChecked)
                distnet.showVLContour();
        }

        private void chkFlow_Checked(object sender, RoutedEventArgs e)
        {
            distnet.showFlow(null, null, false, true);
        }

        private void chkFlow_Unchecked(object sender, RoutedEventArgs e)
        {
            distnet.clearFlow();
        }

        private void chkLoad_Checked(object sender, RoutedEventArgs e)
        {
            distnet.showLoadCol();
        }

        private void chkLoad_Unchecked(object sender, RoutedEventArgs e)
        {
            distnet.clearLoadCol();
        }

        private void chkVL_Checked(object sender, RoutedEventArgs e)
        {
            distnet.showVLContour();
        }

        private void chkVL_Unchecked(object sender, RoutedEventArgs e)
        {
            distnet.clearVLContour();
        }

        private void chkNP1_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chkNP1_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void chkForecast_Checked(object sender, RoutedEventArgs e)
        {
            distnet.scene.objManager.zLayers["网格"].logicVisibility = true;
            distnet.scene.UpdateModel();

        }

        private void chkForecast_Unchecked(object sender, RoutedEventArgs e)
        {
            distnet.scene.objManager.zLayers["网格"].logicVisibility = false;
            distnet.scene.UpdateModel();

        }

        private void chkAuto_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void chkAuto_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chkReliability_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chkReliability_Unchecked(object sender, RoutedEventArgs e)
        {

        }


        #region ===== 指标鱼骨图相关 =====
        FishBone.FishBoneControl fish;
        System.Windows.Media.Animation.DoubleAnimation anifish = new System.Windows.Media.Animation.DoubleAnimation();
        System.Windows.Media.Animation.DoubleAnimation anidistnet = new System.Windows.Media.Animation.DoubleAnimation();
        bool isFishInited;
        void initfish()
        {

            if (isFishInited) return;
            isFishInited = true;
            fish = new FishBone.FishBoneControl();
            fish.Margin = new Thickness(0, 50, 0, 0);
            

            anifish.Duration = anidistnet.Duration = TimeSpan.FromSeconds(0.5);
            fish.Opacity = 0;
            grdAdd.Children.Add(fish);
            hidefish();
            fish.IsHitTestVisible = false;
        }

        void btnMoreIndex_Click(object sender, RoutedEventArgs e)
        {
            initfish();
            if (anifish.To != 1)
                showfish();
            else
                hidefish();
        }

        int savefishinstanceid = -1;
        void showfish()
        {
            if (savefishinstanceid != curprj.prjid)
            {
                savefishinstanceid = curprj.prjid;
                string sql = string.Format("select cast(ID as nvarchar(100)) id, sort1,sort2,ord,indexname, definition,IMPORTANT,format,t2.VALUE,UNIT,VALUENOTE,REFER1,REFER2,REFERTYPE,refernote from  Dic_Index t1,Anal_Index t2 where Category='求实' and t2.InstanceID={0} and t1.ID=t2.IndexID order by ord,IMPORTANT", curprj.prjid);
                fish.dataSource = DataLayer.DataProvider.getDataTableFromSQL(sql);
                fish.reDraw();
            }

            anifish.To = 1;
            fish.BeginAnimation(UserControl.OpacityProperty, anifish);
            anidistnet.To = 0.4;
            distnet.scene.BeginAnimation(UserControl.OpacityProperty, anidistnet);
            fish.IsHitTestVisible = true;
        }
        void hidefish()
        {
            anifish.To = 0;
            fish.BeginAnimation(UserControl.OpacityProperty, anifish);
            anidistnet.To = 1;
            distnet.scene.BeginAnimation(UserControl.OpacityProperty, anidistnet);
            fish.IsHitTestVisible = false;
        }



        #endregion

        #region ===== 指标对比表格 =====
        System.Windows.Media.Animation.DoubleAnimation aniidx = new System.Windows.Media.Animation.DoubleAnimation() { Duration = TimeSpan.FromSeconds(0.5) };

        private void btnCompareIndex_Click(object sender, RoutedEventArgs e)
        {
            if (aniidx.To != 1)
                showindexcompare();
            else
                hideindexcompare();


        }

        void showindexcompare()
        {
            //创建表
            DataTable dt = new DataTable();
            DataColumn dc;
            dc = new DataColumn("分类", typeof(string)); dt.Columns.Add(dc);
            dc = new DataColumn("指标", typeof(string)); dt.Columns.Add(dc);
            YearData yd = years[timeline.selYear];
            foreach (var item in yd.projects)
            {
                dc = new DataColumn(item.name, typeof(string)); dt.Columns.Add(dc);
            }

            string sql = string.Format("select cast(ID as nvarchar(100)) id, sort1,sort2,ord,indexname, definition,IMPORTANT,format,UNIT,VALUENOTE,REFER1,REFER2,REFERTYPE,refernote from  Dic_Index t1 where Category='求实' order by ord,IMPORTANT");

            DataTable dtidx = DataLayer.DataProvider.getDataTableFromSQL(sql);
            //创建树的枝记录
            DataRow adddr;
            var tmp = from e0 in dtidx.AsEnumerable() group e0 by e0["sort2"];
            foreach (var ee in tmp)
            {
                adddr = dt.NewRow();
                adddr[1] = ee.Key;
                dt.Rows.Add(adddr);
            }

            //======读指标
            //构建读指标值语句
            string filter = "";
            int tmpi = 0;
            foreach (var item in yd.projects)
            {
                if (tmpi == 0)
                    filter += string.Format("instanceid={0}", item.prjid);
                else
                    filter += string.Format(" or instanceid={0}", item.prjid);
                tmpi++;
            }
            sql = string.Format("select * from anal_index where flag='规划指标评估报表' and ({0})", filter);
            DataTable dtvalue = DataLayer.DataProvider.getDataTableFromSQL(sql);


            foreach (DataRow dr in dtidx.Rows)
            {
                adddr = dt.NewRow();
                adddr[0] = dr["sort2"].ToString();
                if (dr["format"].ToString().Contains("p"))
                    adddr[1] = String.Format("{0}（%）", dr["indexname"]);
                else
                    adddr[1] = String.Format("{0}（{1}）", dr["indexname"], dr["unit"]);
                if (adddr[0].ToString() == adddr[1].ToString())
                    adddr[1] = dr["indexname"].ToString() + "指标";
                int idx = 2;
                foreach (var item in yd.projects)
                {
                    DataRow vdr = dtvalue.AsEnumerable().FirstOrDefault(p => p.getInt("instanceid") == item.prjid && p.getInt("indexid") == dr.getInt("id"));
                    double value = vdr == null ? 0 : vdr.getDouble("value");

                    adddr[idx] = String.Format("{0}{1}", value.ToString(dr["format"].ToString()), dr["unit"]);
                    idx++;
                }
                dt.Rows.Add(adddr);
            }
            gridIndexCompare.ItemsSource = null;
            gridIndexCompare.ItemsSource = dt;

            aniidx.To = 1;
            gridIndexCompare.BeginAnimation(Grid.OpacityProperty, aniidx);
            anidistnet.To = 0.4;
            distnet.scene.BeginAnimation(UserControl.OpacityProperty, anidistnet);
            gridIndexCompare.IsHitTestVisible = true;
        }
        void hideindexcompare()
        {
            aniidx.To = 0;
            gridIndexCompare.BeginAnimation(UserControl.OpacityProperty, aniidx);
            anidistnet.To = 1;
            distnet.scene.BeginAnimation(UserControl.OpacityProperty, anidistnet);
            gridIndexCompare.IsHitTestVisible = false;
        }
        private void gridIndexCompare_AutoGeneratingColumn(object sender, DevExpress.Xpf.Grid.AutoGeneratingColumnEventArgs e)
        {
            e.Column.EditSettings = new DevExpress.Xpf.Editors.Settings.TextEditSettings();
            e.Column.ActualEditSettings.HorizontalContentAlignment = DevExpress.Xpf.Editors.Settings.EditSettingsHorizontalAlignment.Center;
        }


        #endregion

        bool isGridCompare;
        private void btnCompareDistnet_Click(object sender, RoutedEventArgs e)
        {
            if (isGridCompare)
            {
                distnet.scene.objManager.restoreVisionProperty();

                curyear.view.colorVisibility = System.Windows.Visibility.Collapsed;
            }
            else
            {

                //===== 保存状态
                distnet.scene.objManager.clearVisionProperty();
                distnet.scene.objManager.saveVisionProperty();
                //===== 设置所有设备
                IEnumerable<WpfEarthLibrary.PowerBasicObject> objs = distnet.scene.objManager.getAllObjList();
                foreach (var item in objs)
                {
                    item.color = Color.FromArgb(80, 255, 255, 255);
                }

                //===== 设备变化
                var allprjs = from e0 in years.Values
                              from e1 in e0.projects.Where(p => p.year > curprj.year)
                              select e1;
                foreach (var e0 in allprjs)  //大于选定方案规划年的，以及同规划年的其它方案的增量设备隐藏 注：暂没实现退运和改造
                {
                    foreach (var e1 in e0.addobjs)
                    {
                        e1.logicVisibility = false;
                    }
                }

                allprjs = from e0 in years.Values
                          from e1 in e0.projects.Where(p => p.year == curprj.year)
                          select e1;
                foreach (var e0 in allprjs)  //本年各方案
                {

                    foreach (var e1 in e0.addobjs)
                    {
                        e1.color = e0.color;
                        e1.logicVisibility = true;

                        if (e1 is WpfEarthLibrary.pPowerLine)
                        {
                            (e1 as WpfEarthLibrary.pPowerLine).aniTwinkle.doCount = 0;
                            (e1 as WpfEarthLibrary.pPowerLine).aniTwinkle.duration = 500;
                            (e1 as WpfEarthLibrary.pPowerLine).AnimationBegin(WpfEarthLibrary.pPowerLine.EAnimationType.闪烁);
                        }
                        else if (e1 is WpfEarthLibrary.pSymbolObject)
                        {
                            (e1 as WpfEarthLibrary.pSymbolObject).aniTwinkle.doCount = 0;
                            (e1 as WpfEarthLibrary.pSymbolObject).aniTwinkle.duration = 500;
                            (e1 as WpfEarthLibrary.pSymbolObject).AnimationBegin(WpfEarthLibrary.pSymbolObject.EAnimationType.闪烁);
                        }
                    }
                }
                distnet.scene.UpdateModel();

                curyear.view.colorVisibility = System.Windows.Visibility.Visible;

                //===== 运行显示
                Run.DataGenerator.StartGenData(distnet);
                Run.DataGenerator.StopGenData();
                refreshScreen();


            }
            isGridCompare = !isGridCompare;
        }



    }



}
