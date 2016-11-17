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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfEarthLibrary;
using DistNetLibrary;

namespace DNVLibrary.Planning
{
    /// <summary>
    /// PEvolvePanel.xaml 的交互逻辑
    /// </summary>
    public partial class PTimesEvolvePanel : UserControl, BaseIPanel
    {
        public PTimesEvolvePanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
        }

        Random rd = new Random();
        UCDNV863 root;
        PTimesEvolveTimeline timeline;
        public void load()
        {
            root.distnet.scene.colorManager.isEnabled = true;
            root.earth.camera.adjustCameraDistance(0.12f);

            timeline = new PTimesEvolveTimeline(){Margin=new Thickness(0,0,0,30)};
            root.grdContent.Children.Add(timeline);
            timeline.btnPause.Click += new RoutedEventHandler(btnPause_Click);
            timeline.btnPlay.Click += new RoutedEventHandler(btnPlay_Click);
            timeline.btnPlus.Click += new RoutedEventHandler(btnPlus_Click);
            timeline.btnReduce.Click += new RoutedEventHandler(btnReduce_Click);
            timeline.trc.EditValueChanged += new DevExpress.Xpf.Editors.EditValueChangedEventHandler(trc_EditValueChanged);


            initData();
            initAni();
            sb.CurrentTimeInvalidated += new EventHandler(sb_CurrentTimeInvalidated);
            sb.Completed += new EventHandler(sb_Completed);
            sb.Begin(this, true);
            sb.Pause(this);
        }

        #region 变量
        DateTime StartDate, EndDate;

        List<EventInfo> eventInfoes = new List<EventInfo>();

        #endregion


        pLayer elayer;
        void initData()
        {
            StartDate = new DateTime(2015, 1, 1);
            EndDate = new DateTime(2025, 1, 1);

            IEnumerable<PowerBasicObject> objs= root.distnet.getAllObjListByObjType(EObjectType.配电室).Union(root.distnet.getAllObjListByObjType(EObjectType.输电线路));
            foreach (var obj in objs)
            {
                if (rd.NextDouble() > 0.2) continue;

                evolveDatas ed = new evolveDatas();
                if (rd.NextDouble() > 0.2)
                {
                    ed.datas.Add(new evolveData() { evoType = EevoType.新建, zDate = StartDate.AddDays((EndDate - StartDate).TotalDays * rd.NextDouble()*0.8), });
                    if (obj is DNACLine)
                        (obj as DNACLine).objStatus = pPowerLine.ECStatus.轻载;
                    else if (obj is DNSwitchHouse)
                        (obj as DNSwitchHouse).objStatus = pSymbolObject.ECStatus.轻载;
                    obj.Progress = 0;
                }
                //else if (rd.NextDouble() > 0.3)
                //{
                //    ed.datas.Add(new evolveData() { evoType = EevoType.改建, zDate = StartDate.AddDays((EndDate - StartDate).TotalDays * rd.NextDouble()*0.8), });
                //    if (obj is DNACLine)
                //        (obj as DNACLine).objStatus = pPowerLine.ECStatus.过载;

                //    else if (obj is DNSwitchHouse)
                //        (obj as DNSwitchHouse).objStatus = pSymbolObject.ECStatus.过载;
                //}
                else
                {
                    ed.datas.Add(new evolveData() { evoType = EevoType.退运, zDate = StartDate.AddDays((EndDate - StartDate).TotalDays * rd.NextDouble()*0.8), });
                    if (obj is DNACLine)
                        (obj as DNACLine).objStatus = pPowerLine.ECStatus.退运;

                    else if (obj is DNSwitchHouse)
                        (obj as DNSwitchHouse).objStatus = pSymbolObject.ECStatus.退运;
                }
                if (obj.busiData == null) obj.busiData = new busiBase(obj);
                obj.busiData.busiAddition = ed;

            }



            //int curprjkey = 13;
            //DataTable dtallproject = DataLayer.DataProvider.getDataTableFromSQL("select * from map_project");
            //DataTable dtproject = DataLayer.DataProvider.getDataTableFromSQL(string.Format("select * from map_project where id={0}", curprjkey));
            //DataRow curprj = dtproject.Rows[0];
            //string sqlwhere = PrjHelper.genProjectsExpress(curprjkey, dtallproject);
            //string sqlwhere2 = PrjHelper.genProjectsExpress(curprjkey, dtallproject, "t1.");
            //DataTable dtobject = DataLayer.DataProvider.getDataTableFromSQL(string.Format("select t1.*,t2.layer from map_object t1 join map_svg_layer t2 on t1.layerid=t2.id where points is not null and {0}", sqlwhere2));


            //foreach (DataRow item in dtobject.AsEnumerable().OrderBy(p => p.Field<int>("prjid")))
            //{
            //    if (item.Field<DateTime?>("builddate") != null || item.Field<DateTime?>("rebuilddate") != null || item.Field<DateTime?>("outagedate") != null)
            //    {
            //        string id = item.Field<string>("id");
            //        PowerBasicObject obj = root.earth.objManager.find(id);
            //        if (obj != null)
            //        {
            //            evolveDatas ed = new evolveDatas();

            //            if (item.Field<DateTime?>("builddate") != null)
            //            {
            //                ed.datas.Add(new evolveData() { evoType = EevoType.新建, zDate = item.Field<DateTime>("builddate") });
            //                if (obj is pPowerLine)
            //                {
            //                    (obj as pPowerLine).objStatus = pPowerLine.ECStatus.轻载;
            //                }
            //                else if (obj is pSymbolObject)
            //                {
            //                    (obj as pSymbolObject).objStatus = pSymbolObject.ECStatus.轻载;
            //                }
            //                obj.Progress = 0;
            //            }
            //            else if (item.Field<DateTime?>("rebuilddate") != null)
            //            {
            //                ed.datas.Add(new evolveData() { evoType = EevoType.改建, zDate = item.Field<DateTime>("rebuilddate"), FromId = item.Field<string>("fromid") });
            //                if (obj is pPowerLine)
            //                {
            //                    (obj as pPowerLine).objStatus = pPowerLine.ECStatus.过载;
            //                }
            //                else if (obj is pSymbolObject)
            //                {
            //                    (obj as pSymbolObject).objStatus = pSymbolObject.ECStatus.过载;
            //                }
            //            }
            //            else
            //            {
            //                ed.datas.Add(new evolveData() { evoType = EevoType.退运, zDate = item.Field<DateTime>("outagedate") });
            //                if (obj is pPowerLine)
            //                {
            //                    (obj as pPowerLine).objStatus = pPowerLine.ECStatus.退运;
            //                }
            //                else if (obj is pSymbolObject)
            //                {
            //                    (obj as pSymbolObject).objStatus = pSymbolObject.ECStatus.退运;
            //                }
            //            }
            //            obj.busiData.busiAddition = ed;
            //        }
            //    }

            //}


        }

        void trc_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            if (sb.GetIsPaused(this) || sb.GetCurrentState(this) == ClockState.Filling)
            {
                double allts = (((TimeSpan)sb.Children.Last().BeginTime + (sb.Children.Last() as ParallelTimeline).Children[0].Duration)).TimeSpan.TotalMilliseconds;
                TimeSpan ts = TimeSpan.FromMilliseconds(allts * timeline.trc.Value);
                timeline.txtDate.Text = StartDate.AddDays(ts.TotalMilliseconds / unitspan).ToString("yyyy年MM月dd日");
                resetAniStatus(StartDate.AddDays(ts.TotalMilliseconds / unitspan));
                sb.Seek(this, ts, TimeSeekOrigin.BeginTime);
            }
        }

        double rad = 1;
        void btnReduce_Click(object sender, RoutedEventArgs e)
        {
            rad = rad / 2;
            sb.SetSpeedRatio(this, rad);
            timeline.txtRad.Text = "速度：" + rad.ToString("f1");
        }

        void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            rad = rad * 2;
            sb.SetSpeedRatio(this, rad);
            timeline.txtRad.Text = "速度：" + rad.ToString("f1");
        }

        void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            lstInfo.SelectedIndex = -1;
            resetAniStatus(StartDate);
            sb.Begin(this, true);
            timeline.btnPause.Content = "暂停";
        }

        void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (sb.GetIsPaused(this))
            {
                sb.Resume(this);
                timeline.btnPause.Content = "暂停";
            }
            else
            {
                sb.Pause(this);
                timeline.btnPause.Content = "继续";
                lstInfo.SelectedIndex = -1;
            }
        }

        private void lstInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstInfo.SelectedIndex < 0) return;
            EventInfo ei = lstInfo.SelectedItem as EventInfo;
            root.earth.camera.aniLook(ei.obj.VecLocation);
        }
        void sb_CurrentTimeInvalidated(object sender, EventArgs e)
        {
            Clock storyboardClock = (Clock)sender;
            if (storyboardClock.CurrentTime == null)
            {
                timeline.trc.Value = 0;
                return;
            }
            timeline.txtDate.Text = StartDate.AddDays((((TimeSpan)storyboardClock.CurrentTime).TotalMilliseconds / unitspan)).ToString("yyyy年MM月dd日");
            timeline.trc.Value = (double)storyboardClock.CurrentProgress;
        }

        void sb_Completed(object sender, EventArgs e)
        {
            sb.Pause(this);
        }
        #region 动画相关
        Storyboard sb = new Storyboard() { FillBehavior = FillBehavior.HoldEnd };
        const int unitspan = 30;  //日时间片


        void initAni()
        {
            timeline.txtRad.Text = "速度：" + sb.SpeedRatio.ToString("f1");

            var l = from e1 in root.earth.objManager.getObjList().Where(p =>p.busiData!=null && p.busiData.busiAddition != null)
                    from e2 in (e1.busiData.busiAddition as evolveDatas).datas
                    select new { d = e2.zDate };

            if (l.Count() == 0) return;

            StartDate = (DateTime)l.Min(p => p.d);
            EndDate = (DateTime)l.Max(p => p.d);

            var l2 = from e1 in root.earth.objManager.getObjList().Where(p =>p.busiData!=null && p.busiData.busiAddition != null)
                     from e2 in (e1.busiData.busiAddition as evolveDatas).datas
                     select new
                     {
                         argu = e2.evoType.ToString(),
                         date = e2.zDate,
                         fromid = e2.FromId,
                         etype = e2.evoType,
                         obj = e1
                     };
            int idx = 0;
            foreach (var item in l2.OrderBy(p => p.date)) //处理边上列表
            {
                EventInfo ei = new EventInfo()
                {
                    idx = idx,
                    background = Brushes.Transparent,
                    eventType = item.etype,
                    visibility = Visibility.Collapsed,
                    objName = item.obj.name,
                    //center = item.obj.center,
                    eventDate = (DateTime)item.date,
                    fromid = item.fromid,
                    obj = item.obj,
                    info = "其它信息"
                };
                eventInfoes.Add(ei);
                idx++;
            }
            lstInfo.ItemsSource = eventInfoes;

            ParallelTimeline ptl;
            int starttick;
            foreach (EventInfo item in eventInfoes)
            {


                starttick = ((int)((DateTime)item.eventDate - StartDate).TotalDays) * unitspan;

                ptl = new ParallelTimeline() { BeginTime = new TimeSpan(0, 0, 0, 0, starttick) };
                sb.Children.Add(ptl);


                switch (item.eventType)
                {
                    case EevoType.新建:
                        if (item.obj is pSymbolObject)
                        {

                            DoubleAnimation ani = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(6 * 30 * unitspan), FillBehavior.HoldEnd);
                            Storyboard.SetTarget(ani, item.obj);
                            Storyboard.SetTargetProperty(ani, new PropertyPath(PowerBasicObject.ProgressProperty));
                            ptl.Children.Add(ani);
                            ptl.CurrentStateInvalidated += new EventHandler(ptl_CurrentStateInvalidated);
                            ptl.SetValue(ToolTipService.ToolTipProperty, item.idx);



                        }
                        else if (item.obj is pPowerLine)
                        {
                            DoubleAnimation ani = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(6 * 30 * unitspan), FillBehavior.HoldEnd);
                            Storyboard.SetTarget(ani, item.obj);
                            Storyboard.SetTargetProperty(ani, new PropertyPath(PowerBasicObject.ProgressProperty));
                            ptl.Children.Add(ani);
                            ptl.CurrentStateInvalidated += new EventHandler(ptl_CurrentStateInvalidated);
                            ptl.SetValue(ToolTipService.ToolTipProperty, item.idx);

                            //(item.obj as zMapPolyline).aniType = MyControlLibrary.DevMap.EAniType.绘制;
                        }


                        break;
                    case EevoType.改建: //改建失效
                        if (item.obj is pSymbolObject)
                        {
                        }
                        else if (item.obj is pPowerLine)
                        {

                        }
                        break;
                    case EevoType.退运:  //退运
                        if (item.obj is pSymbolObject)
                        {
                            DoubleAnimation ani = new DoubleAnimation(1, 0.5, TimeSpan.FromMilliseconds(6 * 30 * unitspan), FillBehavior.HoldEnd);
                            Storyboard.SetTarget(ani, item.obj);
                            Storyboard.SetTargetProperty(ani, new PropertyPath(PowerBasicObject.ProgressProperty));
                            ptl.Children.Add(ani);
                            ptl.CurrentStateInvalidated += new EventHandler(ptl_CurrentStateInvalidated);
                            ptl.SetValue(ToolTipService.ToolTipProperty, item.idx);
                        }
                        else if (item.obj is pPowerLine)
                        {
                            DoubleAnimation ani = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(6 * 30 * unitspan), FillBehavior.HoldEnd);
                            Storyboard.SetTarget(ani, item.obj);
                            Storyboard.SetTargetProperty(ani, new PropertyPath(PowerBasicObject.ProgressProperty));
                            ptl.Children.Add(ani);
                            ptl.CurrentStateInvalidated += new EventHandler(ptl_CurrentStateInvalidated);
                            ptl.SetValue(ToolTipService.ToolTipProperty, item.idx);

                            //(item.obj as zMapPolyline).aniType = MyControlLibrary.DevMap.EAniType.擦除;

                        }

                        break;
                }



            }


        }

        ///<summary>子时间线状态改变</summary>
        void ptl_CurrentStateInvalidated(object sender, EventArgs e)
        {
            Clock clock = sender as Clock;
            ParallelTimeline tl = clock.Timeline as ParallelTimeline;

            int idx = (int)tl.GetValue(ToolTipService.ToolTipProperty);
            EventInfo ei = eventInfoes.FirstOrDefault(p => p.idx == idx);

            if (clock.CurrentState == ClockState.Active && sb.GetCurrentState(this) == ClockState.Active)
            {
                //if (!sb.GetIsPaused(this))
                {

                    //ucmap.BaseMap.zMapControl.CenterPoint = MapHelper.getOffsetPoint(ei.center, new Point(175, 0), ucmap.BaseMap.zMapControl.Layers[0]);
                    root.earth.camera.aniLook(ei.obj.VecLocation,0);
                    root.earth.UpdateModel();
                    //infopanel.Visible = true;
                    //infopanel.Location = ei.center;
                    //infopanel.Content = ei;
                }
                ei.visibility = Visibility.Visible;
                ei.background = new SolidColorBrush(Colors.Chocolate);
            }
            else if (clock.CurrentState == ClockState.Filling)
            {
                ei.visibility = Visibility.Visible;
                ei.background = Brushes.Transparent;
            }
            else
            {
                ei.visibility = Visibility.Collapsed;
                ei.background = Brushes.Transparent;
            }



        }


        ///<summary>设置对象呈现状态</summary>
        void resetAniStatus(DateTime curdate)
        {
            foreach (EventInfo item in eventInfoes)
            {
                //if (item.obj is zMapPolyline)
                //{
                //    if (item.eventType == "builddate")
                //        item.obj.isVisual = item.eventDate < curdate;

                //    if (item.eventDate > curdate && item.eventType == "builddate")
                //        item.obj.Progress = 0;
                //}
                //else if (item.obj is zMapRectangle)
                //{
                //    if (item.eventDate > curdate && item.eventType == "builddate")
                //        item.obj.Progress = 0;
                //}

                item.visibility = item.eventDate < curdate ? Visibility.Visible : Visibility.Collapsed;

            }
        }


        #endregion



        public void unload()
        {
            sb.Stop();
            root.distnet.scene.colorManager.isEnabled = false;

            if (root.grdContent.Children.Contains(timeline))
                root.grdContent.Children.Remove(timeline);
            timeline.btnPause.Click -= new RoutedEventHandler(btnPause_Click);
            timeline.btnPlay.Click -= new RoutedEventHandler(btnPlay_Click);
            timeline.btnPlus.Click -= new RoutedEventHandler(btnPlus_Click);
            timeline.btnReduce.Click -= new RoutedEventHandler(btnReduce_Click);
            timeline.trc.EditValueChanged -= new DevExpress.Xpf.Editors.EditValueChangedEventHandler(trc_EditValueChanged);
            timeline = null;
            sb.CurrentTimeInvalidated -= new EventHandler(sb_CurrentTimeInvalidated);
            sb.Completed -= new EventHandler(sb_Completed);

            foreach (PowerBasicObject obj in root.earth.objManager.getObjList().Where(p =>p.busiData!=null && p.busiData.busiAddition != null))
            {
                if (obj is pPowerLine)
                    (obj as pPowerLine).objStatus = pPowerLine.ECStatus._正常;
                else if (obj is pSymbolObject)
                    (obj as pSymbolObject).objStatus = pSymbolObject.ECStatus._正常;

            }
            root.earth.objManager.zLayers.Remove("演进层");

            foreach (PowerBasicObject obj in root.earth.objManager.getObjList())
                if (obj.busiData!=null)
                    obj.busiData.busiAddition = null;

            root.earth.UpdateModel();
        }



    }




    #region 事件信息类
    internal class EventInfo : MyClassLibrary.MVVM.NotificationObject
    {
        public int idx { get; set; }
        public EevoType eventType { get; set; }
        public DateTime eventDate { get; set; }
        public string objName { get; set; }
        //public GeoPoint center { get; set; }
        public string info { get; set; }
        public string fromid { get; set; }

        private SolidColorBrush _background;
        public SolidColorBrush background
        {
            get { return _background; }
            set { _background = value; RaisePropertyChanged(() => background); }
        }


        private Visibility _visibility;
        public Visibility visibility
        {
            get { return _visibility; }
            set { _visibility = value; RaisePropertyChanged(() => visibility); }
        }

        public PowerBasicObject obj { get; set; }

        public string strType
        {
            get
            {
                return eventType.ToString();
            }
        }
        public string strDate
        {
            get
            {
                return eventDate.ToString("yyyy-MM：");
            }
        }
    }
    #endregion

    class evolveDatas
    {
        public evolveDatas()
        {
            datas = new List<evolveData>();
        }
        public List<evolveData> datas { get; set; }
    }

    class evolveData
    {
        ///<summary>类型</summary>
        public EevoType evoType { get; set; }

        ///<summary>日期</summary>
        public DateTime zDate { get; set; }

        ///<summary>改建前ID</summary>
        public string FromId { get; set; }
    }

    enum EevoType { 新建, 改建, 退运 }
}
