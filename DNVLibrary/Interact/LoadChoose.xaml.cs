using DevExpress.Xpf.Gauges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace LoadInput.MyControl
{
    /// <summary>
    /// LoadChoose.xaml 的交互逻辑
    /// </summary>
    public partial class LoadChoose : UserControl
    {
        public LoadChoose(LoadSummary _parent)
        {
            InitializeComponent();
            parent = _parent;
        }

        public LoadSummary parent;

        private ElecSpecies _species;
        public ElecSpecies species
        {
            get { return _species; }
            set { _species = value; }
        }

        private double _weight=0.5;
        public double weight
        {
            get { return _weight; }
            set { _weight = value; }
        }

        private bool _isEnable;
        public bool isEnable
        {
            get { return _isEnable; }
            set { _isEnable = value; }
        }

        ArcScaleNeedle selectedNeedle = null;
        const int toolTipOffset = 15;


        void SensitivityNeedle_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            parent.Update(species, e.NewValue);
        }

        void CircularGaugeControl_MouseMove(object sender, MouseEventArgs e)
        {
            CircularGaugeControl gauge = (CircularGaugeControl)sender;
            ArcScaleNeedle currentSelectedNeedle = selectedNeedle != null ? selectedNeedle : gauge.CalcHitInfo(e.GetPosition(gauge)).Needle;
            if (currentSelectedNeedle != null)
                ShowTooltip(currentSelectedNeedle, this, e.GetPosition(this));
            else
                HideTooltip();
        }

        void CircularGaugeControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CircularGaugeControl gauge = sender as CircularGaugeControl;
            if (gauge != null)
            {
                CircularGaugeHitInfo hitInfo = gauge.CalcHitInfo(e.GetPosition(gauge));
                if (hitInfo.InNeedle)
                    selectedNeedle = hitInfo.Needle;
            }
        }

        void CircularGaugeControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            selectedNeedle = null;
            HideTooltip();
        }

        void CircularGaugeControl_MouseLeave(object sender, MouseEventArgs e)
        {
            selectedNeedle = null;
            HideTooltip();
        }

        void HideTooltip()
        {
            ttContent.Text = "";
            needleTooltip.IsOpen = false;
            Cursor = Cursors.Arrow;
        }

        void ShowTooltip(ArcScaleNeedle needle, UIElement placementTarget, Point position)
        {
            ttContent.Text = string.Format("Value = {0:F2}", needle.Value);
            needleTooltip.Placement = PlacementMode.RelativePoint;
            needleTooltip.PlacementTarget = placementTarget;

            needleTooltip.HorizontalOffset = position.X + toolTipOffset;
            needleTooltip.VerticalOffset = position.Y + toolTipOffset;
            needleTooltip.IsOpen = true;
            Cursor = Cursors.Hand;
        }

        private void gauge_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            gauge.Opacity=(bool)e.NewValue ? 1.0 : 0.5;
            label.Opacity = (bool)e.NewValue ? 1.0 : 0.5;
        }

        
    }
}
