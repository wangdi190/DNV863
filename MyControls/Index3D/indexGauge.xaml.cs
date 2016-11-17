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
using DevExpress.Xpf.Gauges;

namespace MyControlLibrary.Controls3D.Index3D
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class indexGauge : ContentControl
    {
        public indexGauge()
        {
            InitializeComponent();
        }
        public indexGauge(GaugeInfo gi)
        {
            InitializeComponent();
            _gi = gi;
            initGauge();
        }

        GaugeInfo _gi;
        public GaugeInfo gaugeInfo
        {
            set
            {
                _gi = value;
                initGauge();
            }
        }
        private void initGauge()
        {
            gscale.StartValue = _gi.startValue;
            gscale.EndValue = _gi.endValue;

            labelOption.FormatString = _gi.labelFormat;

            marker.Value = _gi.value;
            needle.Value = _gi.value;
            range0.StartValue =new RangeValue(_gi.range[0, 0]);
            range0.EndValue = new RangeValue(_gi.range[0, 1]);
            range1.StartValue = new RangeValue(_gi.range[1, 0]);
            range1.EndValue = new RangeValue(_gi.range[1, 1]);
            range2.StartValue = new RangeValue(_gi.range[2, 0]);
            range2.EndValue = new RangeValue(_gi.range[2, 1]);

            (range0.Presentation as DefaultArcScaleRangePresentation).Fill = _gi.rangeBrush[0];
            (range1.Presentation as DefaultArcScaleRangePresentation).Fill = _gi.rangeBrush[1];
            (range2.Presentation as DefaultArcScaleRangePresentation).Fill = _gi.rangeBrush[2];
        }
    
    
    }
}
