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

namespace DNVLibrary.Planning
{
    /// <summary>
    /// UCGauge.xaml 的交互逻辑
    /// </summary>
    public partial class PAllOneGauge : UserControl
    {
        public enum EGaugeMode { 无, 中, 小, 可编辑 }

        public PAllOneGauge()
        {
            InitializeComponent();
        }

        EGaugeMode _gaugeMode;
        ///<summary>注释</summary>
        public EGaugeMode gaugeMode
        {
            get { return _gaugeMode; }
            set
            {
                _gaugeMode = value;
                //switch (value)
                //{
                //    case EGaugeMode.中:
                //        scalelayer.ScaleLayerTemplate = (ControlTemplate)this.FindResource("NormalTemplate");
                //        gauge.Margin = new Thickness(-20, -20,-20,-20);
                //        gauge.IsHitTestVisible = false;
                //        break;
                //    case EGaugeMode.小:
                //        scalelayer.ScaleLayerTemplate = (ControlTemplate)this.FindResource("SmallTemplate");
                //        gauge.Margin = new Thickness(-20, -40,-20,-40);
                //        gauge.IsHitTestVisible = false;
                //        break;
                //    case EGaugeMode.可编辑:
                //        scalelayer.ScaleLayerTemplate = (ControlTemplate)this.FindResource("EditTemplate");
                //        gauge.Margin = new Thickness(-20, -40,-20,-40);
                //        gauge.IsHitTestVisible = true;
                //        break;
                //    default:
                //        scalelayer.ScaleLayerTemplate = null;
                //        gauge.IsHitTestVisible = false;
                //        break;
                //}
            }
        }


    }
}
