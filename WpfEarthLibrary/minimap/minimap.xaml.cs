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

namespace WpfEarthLibrary.minimap
{
    /// <summary>
    /// minimap.xaml 的交互逻辑
    /// </summary>
    public partial class minimap : UserControl
    {
        public minimap(Earth pearth)
        {
            earth = pearth;
            InitializeComponent();
        }
        Earth earth;

        internal minimapdata mapdata = new minimapdata();

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            //载入小地图坐标数据
            mapdata = (minimapdata)MyClassLibrary.XmlHelper.readFromXml(".\\xml\\minimap.xml", typeof(minimapdata));
            //载入地图
            System.IO.FileStream pFileStream = new System.IO.FileStream(Environment.CurrentDirectory + "\\minimap.png", System.IO.FileMode.Open, System.IO.FileAccess.Read); 
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = pFileStream;
            bi.CacheOption = BitmapCacheOption.OnLoad; 
            bi.EndInit();
            bi.Freeze();
            pFileStream.Close(); pFileStream.Dispose();

            brdMap.Background = new ImageBrush(bi);
            //根据地图比例设置控件大小
            this.Width = 300 + 1;
            this.Height = 300d / bi.Width * bi.Height + 1;
            //控制地图位置
            tranScale.ScaleX = tranScale.ScaleY = mapdata.sclae;
            tranTranslate.X = mapdata.offsetX;
            tranTranslate.Y = mapdata.offsetY;
            this.Visibility = mapdata.isshow ? Visibility.Visible : Visibility.Collapsed;
        }


        Point prevPoint;
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Grid pobj = (Grid)VisualTreeHelper.GetParent(this);
                Point curPoint = e.GetPosition(pobj);
                Vector vec = curPoint - prevPoint;
                if (vec.Length > 0)
                {
                    tranTranslate.X += vec.X;
                    tranTranslate.Y += vec.Y;
                    mapdata.offsetX = tranTranslate.X;
                    mapdata.offsetY = tranTranslate.Y;
                    prevPoint = curPoint;
                }
            }
            e.Handled = true;
        }

        private void UserControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Grid pobj = (Grid)VisualTreeHelper.GetParent(this);
            prevPoint = e.GetPosition(pobj);
        }

        private void UserControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            mapdata.save();
        }

        internal void refreshPoly()
        {
            double x, y;
            //leftop
            x = (earth.camera.curCamearaViewRange.farLongitudeStart - mapdata.lefttop.Longitude) / (mapdata.righttop.Longitude - mapdata.lefttop.Longitude) * brdMap.ActualWidth;
            y = (mapdata.lefttop.Latitude - earth.camera.curCamearaViewRange.latitudeEnd) / (mapdata.lefttop.Latitude - mapdata.leftbottom.Latitude) * brdMap.ActualHeight;
            poly.Points[0] = new Point(x, y);
            //righttop
            x = (earth.camera.curCamearaViewRange.farLongitudeEnd - mapdata.lefttop.Longitude) / (mapdata.righttop.Longitude - mapdata.lefttop.Longitude) * brdMap.ActualWidth;
            poly.Points[1] = new Point(x, y);
            //rightbottom
            x = (earth.camera.curCamearaViewRange.nearLongitudeEnd - mapdata.leftbottom.Longitude) / (mapdata.rightbottom.Longitude - mapdata.leftbottom.Longitude) * brdMap.ActualWidth;
            y = (mapdata.lefttop.Latitude - earth.camera.curCamearaViewRange.latitudeStart) / (mapdata.lefttop.Latitude - mapdata.leftbottom.Latitude) * brdMap.ActualHeight;
            poly.Points[2] = new Point(x, y);
            //leftbottom
            x = (earth.camera.curCamearaViewRange.nearLongitudeStart - mapdata.leftbottom.Longitude) / (mapdata.rightbottom.Longitude - mapdata.leftbottom.Longitude) * brdMap.ActualWidth;
            poly.Points[3] = new Point(x, y);





        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Visibility== System.Windows.Visibility.Visible)
                refreshPoly();
        }



        private void poly_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            prevPoint = e.GetPosition(grdContent);
            Cursor = Cursors.Hand;
        }
        private void poly_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Arrow;
            earth.UpdateModel();
        }

        ///<summary>小地图中的移动</summary>
        private void poly_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                //地图移动
                Point curPoint = e.GetPosition(grdContent);
                Vector vec = curPoint - prevPoint;
                if (vec.Length > 0)
                {
                    double offsetLong = vec.X / brdMap.ActualWidth * (mapdata.righttop.Longitude - mapdata.lefttop.Longitude);
                    double offsetLat = -vec.Y / brdMap.ActualHeight * (mapdata.righttop.Latitude - mapdata.rightbottom.Latitude);

                    Point cp = (Point)earth.camera.getScreenCenterGeo();
                    double gd = earth.camera.curCameraDistanceToGround;
                    cp.Offset(offsetLat, offsetLong);
                    earth.camera.aniLookGeo(cp.Y, cp.X, gd, 0);

                    prevPoint = curPoint;
                    refreshPoly();
                }

                
            }
            e.Handled = true;
        }

       


    }
}
