using DevExpress.Xpf.Charts;
using DevExpress.Xpf.Editors;
using MyClassLibrary.DevShare;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LoadInput
{
    /// <summary>
    /// LoadClassification.xaml 的交互逻辑
    /// </summary>
    public partial class LoadClassification : UserControl
    {
        public LoadClassification()
        {
            InitializeComponent();
        }

        LoadClassData data = new LoadClassData();
        HitInfo hit = new HitInfo();
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            grdMain.DataContext = data;
            data.start();
        }

        private void ChartControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChartControl chart = (sender as ChartControl);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point pt = e.GetPosition(this);
                ChartHitInfo hitInfo = (sender as ChartControl).CalcHitInfo(e.GetPosition(chart));
                if (hitInfo.SeriesPoint != null)
                {
                    hit.nPos = (hitInfo.SeriesPoint.Tag as MyChartDataPoint).nPos;
                    hit.dValue = hitInfo.SeriesPoint.Value;
                    hit.species = (hitInfo.SeriesPoint.Tag as MyChartDataPoint).species;

                    //(modifyPopup.Child as Slider).Value = 0;
                    //(modifyPopup.Child as TrackBarEdit).Value = 0;
                    trackbar.Value = 0;
                    modifyPopup.HorizontalOffset = pt.X+5;
                    modifyPopup.VerticalOffset = pt.Y+50;
                    modifyPopup.IsOpen = true;
                }
                else
                {
                    modifyPopup.IsOpen = false;
                }
            }
            
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(modifyPopup.IsOpen)
            {
                data.modify(hit.species, hit.nPos, hit.dValue+e.NewValue);
            }
        }

        private void TrackBarEdit_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            if (modifyPopup.IsOpen)
            {
                data.modify(hit.species, hit.nPos, hit.dValue + (double)e.NewValue);
            }
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility != System.Windows.Visibility.Visible) modifyPopup.IsOpen = false;
        }
    }
}
