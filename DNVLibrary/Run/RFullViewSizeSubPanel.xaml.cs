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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Charts;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
namespace DNVLibrary.Run
{
    /// <summary>
    /// Scales_BDZ.xaml 的交互逻辑
    /// </summary>
    public partial class RFullViewSizeSubPanel : UserControl
    {
        Grid frame;
        public RFullViewSizeSubPanel(ChartHitInfo info)
        {
            pHitInfo = info;
            InitializeComponent();
            InitPara();
        }
        const int clickDelta = 200;
        bool isLeftMouseButtonReleased = true;
        int mouseDownTime;
        ChartHitInfo pHitInfo;
        void chart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDownTime = e.Timestamp;
            isLeftMouseButtonReleased = false;
        }

        bool IsClick(int mouseUpTime)
        {
            return mouseUpTime - mouseDownTime < clickDelta;
        }

        void chart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChartControl myChart = sender as ChartControl;
            isLeftMouseButtonReleased = true;
            if (!IsClick(e.Timestamp))
                return;
            ChartHitInfo hitInfo = myChart.CalcHitInfo(e.GetPosition(myChart));
            if (hitInfo == null || hitInfo.SeriesPoint == null)
                return;
            double distance = PieSeries.GetExplodedDistance(hitInfo.SeriesPoint);
            AnimationTimeline animation = distance > 0 ? (AnimationTimeline)TryFindResource("CollapseAnimation") : (AnimationTimeline)TryFindResource("ExplodeAnimation");
            Storyboard storyBoard = new Storyboard();
            storyBoard.Children.Add(animation);
            Storyboard.SetTarget(animation, hitInfo.SeriesPoint);
            Storyboard.SetTargetProperty(animation, new PropertyPath(PieSeries.ExplodedDistanceProperty));
            storyBoard.Begin();
        }

        void chart_QueryChartCursor(object sender, QueryChartCursorEventArgs e)
        {
            ChartControl myChart = sender as ChartControl;
            ChartHitInfo hitInfo = myChart.CalcHitInfo(e.Position);
            if (hitInfo != null && hitInfo.SeriesPoint != null && isLeftMouseButtonReleased)
                e.Cursor = Cursors.Hand;
        }

        private void InitPara()
        {
            ChartHitInfo hitInfo = pHitInfo;
            Series serie = hitInfo.Series;
            SeriesPoint sPoint = hitInfo.SeriesPoint;
            SeriesCollection seriesColl = hitInfo.Diagram.Series;
            double sum1=0,sum2=0,val=0;
            string year=sPoint.Argument+"年";
            string titleName=null;
            List<string> adds = new List<string>();
            foreach (Series series in seriesColl)
            {
                if (series.GetType() == serie.GetType())
                {
                    SeriesPointCollection pointColl = series.Points;
                    foreach (SeriesPoint p in pointColl)
                    {
                        if (p.Argument == sPoint.Argument)
                        {
                            SeriesPoint newPoint = new SeriesPoint();
                            newPoint.Argument = series.DisplayName;
                            newPoint.Value = p.Value;
                            pie1.Points.Add(newPoint);
                            sum1 += p.Value;
                        }
                    }
                }
                else
                {
                    SeriesPointCollection pointColl = series.Points;
                    foreach (SeriesPoint p in pointColl)
                    {
                        if (p.Argument == sPoint.Argument &&!adds.Contains(p.Series.DisplayName))
                        {
                            adds.Add(p.Series.DisplayName);
                            SeriesPoint newPoint = new SeriesPoint();
                            newPoint.Argument = series.DisplayName;
                            newPoint.Value = p.Value;
                            pie2.Points.Add(newPoint);
                            if (string.Compare(serie.Name.Split('_')[0], p.Series.Name.Split('_')[0]) == 0)
                            {
                                val = p.Value;
                                titleName = p.Series.DisplayName;
                            }
                            sum2 += p.Value;
                        }
                    }
                }
            }
            
            double ratio1 = sPoint.Value / sum1;
            double ratio2 = val / sum2;
            string displayName1=year+hitInfo.Series.DisplayName+"占比："+string.Format("{0:P0}",ratio1);
            string displayName2 = year + titleName + "占比：" + string.Format("{0:P0}", ratio2);
            title1.Content = displayName1;
            title2.Content = displayName2;
        }

        
        private void close_Clicked(object sender, EventArgs e)
        {
            Popup myPanel = uc.Parent as Popup;
            myPanel.Child = null;
            myPanel.IsOpen = false;
            myPanel = null; 
        }
    }
}
