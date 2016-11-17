using System;
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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfEarthLibrary;

namespace DNVLibrary.Planning
{
    /// <summary>
    /// PAllTimeLine.xaml 的交互逻辑
    /// </summary>
    public partial class PAllTimeLine : UserControl
    {
        public PAllTimeLine(PAllPanel Parent)
        {
            parent = Parent;
            InitializeComponent();
        }

        PAllPanel parent;

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            StartDate = new DateTime(parent.startYear, 1, 1);
            EndDate = new DateTime(parent.endYear, 12, 31);

            initAni();
            sb.CurrentTimeInvalidated += new EventHandler(sb_CurrentTimeInvalidated);
            sb.Completed += new EventHandler(sb_Completed);
            //sb.Begin(this, true);
            //sb.Pause(this);

        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            rebuildTimeLine();
            selYear = _selYear;
        }



        void rebuildTimeLine()
        {
            if (parent.endYear <= parent.startYear) return;
            Brush txtBrush = Brushes.Yellow;

            
            

            //重建时间线
            int startYear = parent.startYear;
            int endYear = parent.endYear;
            trc.TickFrequency = 1.0 / (endYear - startYear);
            grdYears.Children.Clear();
            int step = (int)Math.Ceiling((parent.endYear - parent.startYear + 1) / 10.0);
            if (step > 2 && step < 5)
                step = 5;
            else if (step > 5)
                step = 10;
            grdYears.Children.Add(new TextBlock() { Text = startYear.ToString(), Margin = new Thickness(8, 0, 0, 0), Foreground = txtBrush });
            grdYears.Children.Add(new TextBlock() { Text = endYear.ToString(), Margin = new Thickness(10 + (endYear - startYear) * (trc.ActualWidth - 12) / (endYear - startYear), 0, 0, 0), Foreground = txtBrush });
            for (int i = startYear + 1; i < endYear; i++)
            {
                if (i % step == 0)  //年份文字
                    grdYears.Children.Add(new TextBlock() { Text = i.ToString(), Margin = new Thickness(10 + (i - startYear) * (trc.ActualWidth - 12) / (endYear - startYear), 0, 0, 0), Foreground = txtBrush });
                //if (i % 10 == 0)  //十年刻度
                //    grdline.Children.Add(new Line() { X1 = 0, Y1 = 5, X2 = 0, Y2 = 10, StrokeThickness = 1, Stroke = linBrush, Margin = new Thickness(20 + (i - startYear) * trc.ActualWidth / (endYear - startYear), 0, 0, 0) });
                //else
                //    //if (i % 5 == 0)  //五年刻度
                //    grdline.Children.Add(new Line() { X1 = 0, Y1 = 7, X2 = 0, Y2 = 10, StrokeThickness = 1, Stroke = linBrush, Margin = new Thickness(20 + (i - startYear) * trc.ActualWidth / (endYear - startYear), 0, 0, 0) });
            }
            //重建方案标志
            grdTri.Children.Clear();
            foreach (var item in parent.years)
            {
                for (int i = item.Value.projects.Count - 1; i > 0; i--)
                {
                    Path path2 = new Path() { Data = (Geometry)this.FindResource("geoTri"), StrokeThickness = 1, Stroke = Brushes.Cyan, Fill = Brushes.Transparent, Margin = new Thickness(13 + 6 + i * 2 + (item.Key - startYear) * (trc.ActualWidth - 12) / (endYear - startYear), 0, 0, 0), Cursor = Cursors.Hand, Tag = item.Key };
                    grdTri.Children.Add(path2);
                }

                Path path = new Path() { Data = (Geometry)this.FindResource("geoTri"), StrokeThickness = 1, Stroke = Brushes.Cyan, Fill = Brushes.Transparent, Margin = new Thickness(13 + 6 + (item.Key - startYear) * (trc.ActualWidth - 12) / (endYear - startYear), 0, 0, 0), Cursor = Cursors.Hand, Tag = item.Key };
                path.MouseLeftButtonDown += new MouseButtonEventHandler(path_MouseLeftButtonDown);
                grdTri.Children.Add(path);
                item.Value.path = path;

            }


        }

        void path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selYear = (int)(sender as Path).Tag;

        }



        private int _selYear = 0;
        ///<summary>注释</summary>
        public int selYear
        {
            get { return _selYear; }
            set
            {
                if (IsLoaded)
                {
                    YearData oldyear, newyear;
                    if (!parent.years.TryGetValue(value, out newyear))
                        return;

                    if (parent.years.TryGetValue(_selYear, out oldyear))
                        oldyear.path.Fill = Brushes.Transparent;

                    newyear.path.Fill = new SolidColorBrush(Colors.OrangeRed);

                    txt.Text = value + "年";
                    txtbrd.Margin = new Thickness(-3 + (value - parent.startYear) * (trc.ActualWidth-12) / (parent.endYear - parent.startYear), 3, 0, 0);
                    trc.Value = (1.0 * value - parent.startYear) / (parent.endYear - parent.startYear);
                    txtbrd.IsHitTestVisible = value != parent.startYear;
                    txtbrd.Cursor = value == parent.startYear ? Cursors.Arrow : Cursors.Hand;


                    _selYear = value;
                    RaiseselYearChangedEvent();
                    RaiseprojectChangedEvent();
                }
                else
                    _selYear = value;
            }
        }


        public event EventHandler projectChanged;
        protected virtual void RaiseprojectChangedEvent()
        {
            if (projectChanged != null)
                projectChanged(this, null);
        }


        public event EventHandler selYearChanged;
        protected virtual void RaiseselYearChangedEvent()
        {
            if (selYearChanged != null)
                selYearChanged(this, null);
        }

        System.Windows.Media.Animation.DoubleAnimation ani = new System.Windows.Media.Animation.DoubleAnimation() { Duration = TimeSpan.FromSeconds(0.5) };
        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            ani.To = 0.7;
            brd.BeginAnimation(Border.OpacityProperty, ani);
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            ani.To = 0.001;
            brd.BeginAnimation(Border.OpacityProperty, ani);
        }


        #region ===== 演进相关 =====
        bool isPlayMode;
        DateTime StartDate, EndDate;
        List<EventInfo> eventInfoes = new List<EventInfo>();

        public event EventHandler PlayBegin;
        protected virtual void RaisePlayBeginEvent()
        {
            if (PlayBegin != null)
                PlayBegin(this, null);
        }


        ///<summary>进入演进模式</summary>
        private void txtbrd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sb.Children.Count == 0) return;

            sb.Begin(this, true);
            sb.Pause(this);

            isPlayMode = true;
            RaisePlayBeginEvent();  //事件通知主面板停止潮流等显示
            pnlPlay.Visibility = System.Windows.Visibility.Visible;
            trc.IsEnabled = true;
            //===== 隐藏所有增量设备，回归现状
            var allprjs = from e0 in parent.years.Values
                          from e1 in e0.projects
                          select e1;
            foreach (var e0 in allprjs)
            {
                foreach (var e1 in e0.addobjs)
                {
                    e1.Progress = 0;
                }
            }
            parent.distnet.scene.UpdateModel();
            trc.Value = 0;

            txt.Text = parent.startYear + "年";
            txtbrd.Margin = new Thickness(-3 , 3, 0, 0);


        }



        void trc_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            if (isPlayMode)
                if (sb.GetIsPaused(this) || sb.GetCurrentState(this) == ClockState.Filling)
                {
                    double allts = (((TimeSpan)sb.Children.Last().BeginTime + (sb.Children.Last() as ParallelTimeline).Children[0].Duration)).TimeSpan.TotalMilliseconds;
                    TimeSpan ts = TimeSpan.FromMilliseconds(allts * trc.Value);
                    //timeline.txtDate.Text = StartDate.AddDays(ts.TotalMilliseconds / unitspan).ToString("yyyy年MM月dd日");
                    txt.Text = StartDate.AddDays(ts.TotalMilliseconds / unitspan).ToString("yyyy.MM");

                    resetAniStatus(StartDate.AddDays(ts.TotalMilliseconds / unitspan));
                    sb.Seek(this, ts, TimeSeekOrigin.BeginTime);
                }
        }

        double rad = 1;
        void btnReduce_Click(object sender, RoutedEventArgs e)
        {
            rad = rad / 2;
            sb.SetSpeedRatio(this, rad);
            txtRad.Text = "速度：" + rad.ToString("f1");
        }

        void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            rad = rad * 2;
            sb.SetSpeedRatio(this, rad);
            txtRad.Text = "速度：" + rad.ToString("f1");
        }

        void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            sb.Stop(this);
            resetAniStatus(StartDate);
            sb.Begin(this, true);
            btnPause.Content = "暂停";
        }

        void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (sb.GetIsPaused(this))
            {
                sb.Resume(this);
                btnPause.Content = "暂停";
            }
            else
            {
                sb.Pause(this);
                btnPause.Content = "继续";
            }
        }

        void sb_CurrentTimeInvalidated(object sender, EventArgs e)
        {
            if (isPlayMode)
            {
                Clock storyboardClock = (Clock)sender;
                if (storyboardClock.CurrentTime == null)
                {
                    trc.Value = 0;
                    return;
                }
                //timeline.txtDate.Text = StartDate.AddDays((((TimeSpan)storyboardClock.CurrentTime).TotalMilliseconds / unitspan)).ToString("yyyy年MM月dd日");
                txt.Text = StartDate.AddDays((((TimeSpan)storyboardClock.CurrentTime).TotalMilliseconds / unitspan)).ToString("yyyy.MM");
                trc.Value = (double)storyboardClock.CurrentProgress;
                txtbrd.Margin = new Thickness(-3 + trc.Value * (trc.ActualWidth - 12), 3, 0, 0);
            }
        }

        void sb_Completed(object sender, EventArgs e)
        {
            sb.Pause(this);
        }

        Storyboard sb = new Storyboard() { FillBehavior = FillBehavior.HoldEnd };
        const int unitspan = 30;  //日时间片

        ///<summary>根据方案树动态生成(注：暂没实现)</summary>
        void initAni()
        {
            txtRad.Text = "速度：" + sb.SpeedRatio.ToString("f1");

            //var l = from e1 in root.earth.objManager.getObjList().Where(p => p.busiData != null && p.busiData.busiAddition != null)
            //        from e2 in (e1.busiData.busiAddition as evolveDatas).datas
            //        select new { d = e2.zDate };

            //if (l.Count() == 0) return;

            //StartDate = (DateTime)l.Min(p => p.d);
            //EndDate = (DateTime)l.Max(p => p.d);

            //var l2 = from e1 in root.earth.objManager.getObjList().Where(p => p.busiData != null && p.busiData.busiAddition != null)
            //         from e2 in (e1.busiData.busiAddition as evolveDatas).datas
            //         select new
            //         {
            //             argu = e2.evoType.ToString(),
            //             date = e2.zDate,
            //             fromid = e2.FromId,
            //             etype = e2.evoType,
            //             obj = e1
            //         };
            var allprjs = from e0 in parent.years.Values
                          from e1 in e0.projects
                          select e1;

            var l2 = from e1 in allprjs
                     from e2 in e1.addobjs
                     select new
                     {
                         argu = "新增",
                         date = (e2.busiAccount as DistNetLibrary.AcntDistBase).runDate,
                         fromid = "",
                         etype = EevoType.新建,
                         prjdate=new DateTime(e1.year,1,1),
                         obj = e2
                     };
            int idx = 0;
            foreach (var item in l2.Where(p=>p.date>=p.prjdate).OrderBy(p => p.date)) //处理边上列表
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
                    parent.distnet.scene.camera.aniLook(ei.obj.VecLocation, 0);
                    parent.distnet.scene.UpdateModel();
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

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            sb.Stop(this);
            sb.Remove(this);
            isPlayMode = false;
            trc.IsEnabled = false;
            pnlPlay.Visibility = System.Windows.Visibility.Hidden;
            selYear = selYear;
            
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

                //item.obj.logicVisibility = item.eventDate < curdate;
            }
        }

        #endregion

    }


}
