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
using System.Windows.Shapes;

namespace WpfEarthLibrary.minimap
{
    /// <summary>
    /// winSetupMinimap.xaml 的交互逻辑
    /// </summary>
    public partial class winSetupMinimap : Window
    {
        public winSetupMinimap()
        {
            InitializeComponent();
        }

        internal Earth earth;


        private void btnSnap_Click(object sender, RoutedEventArgs e)
        {
            if (earth.camera.curCameraAngle < 1)
            {

                MyClassLibrary.IOHelper.SaveToImage(earth.imgelt, "minimap.png", 0.25);
                if (earth.minimap == null) earth.minimap = new minimap(earth);
                var range = earth.camera.curCamearaViewRange;
                earth.minimap.mapdata.lefttop = new GeoPoint(range.latitudeEnd, range.farLongitudeStart);
                earth.minimap.mapdata.leftbottom = new GeoPoint(range.latitudeStart, range.nearLongitudeStart);
                earth.minimap.mapdata.righttop = new GeoPoint(range.latitudeEnd, range.farLongitudeEnd);
                earth.minimap.mapdata.rightbottom = new GeoPoint(range.latitudeStart, range.nearLongitudeEnd);

                earth.minimap.mapdata.offsetX = earth.minimap.tranTranslate.X;
                earth.minimap.mapdata.offsetY = earth.minimap.tranTranslate.Y;
                earth.minimap.mapdata.sclae = earth.minimap.tranScale.ScaleX;
                earth.minimap.mapdata.isshow = earth.minimap.Visibility == System.Windows.Visibility.Visible;

                earth.minimap.mapdata.save();
            }
            else
                MessageBox.Show("请将地图调整到俯视角度再快照！");
        }

        private void sdrScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (earth != null)
                earth.minimap.tranScale.ScaleX = earth.minimap.tranScale.ScaleY = earth.minimap.mapdata.sclae = sdrScale.Value;
        }

        private void chkShow_Checked(object sender, RoutedEventArgs e)
        {
            earth.minimap.mapdata.isshow = true;
            earth.showMinimap();
        }

        private void chkShow_Unchecked(object sender, RoutedEventArgs e)
        {
            earth.minimap.mapdata.isshow = false;
            earth.hideMinimap();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            earth.minimap.mapdata.save();
            earth = null;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.chkShow.IsChecked = earth.minimap.Visibility == System.Windows.Visibility.Visible;
            sdrScale.Value = earth.minimap.tranScale.ScaleX;
        }

     


    }
}
