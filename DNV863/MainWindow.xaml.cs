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

namespace DNV863
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            
            //grdMain.Children.Add(new MyControlLibrary.Controls3D.CurveSurface3D.UserControl1());

            //坐标转换，数据库更新
            //tempWinTransCood863 win = new tempWinTransCood863(); win.ShowDialog();

            //DataLayer.DataProvider.isAuthorize = true;
            //DataLayer.DataProvider.Login();
            //grdMain.Children.Add(new DNVLibrary.UCDNV863());


            //grdMain.Children.Add(new WpfEarthLibrary.Tools.CalCoordinateTransformPara());


            //DataLayer.WCF.WinLogin win = new DataLayer.WCF.WinLogin(); win.ShowDialog();


            //grdMain.Children.Add(new MyControlLibrary.Controls3D.CurveSurface3D.UserControl1());
            //grdMain.Children.Add(new IDXKPI.UserControl1());
            //grdMain.Children.Add(new FishBone.UserControl1());
            //grdMain.Children.Add(new UCSample1());   //地球示例
            //grdMain.Children.Add(new UCSample2());   //平面示例
            //grdMain.Children.Add(new UCSample3());     //多实例示例
            //grdMain.Children.Add(new UCSample4());  //测试动态显示
            //grdMain.Children.Add(new UCSample5());  //测试动态载入

        }

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            DataLayer.DirectDBAccessor.connections.Clear();
            DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("Data Source=(local);Initial Catalog=sdnvdb;User ID=sa;Password=123456", "localhost", DataLayer.EDataBaseType.MsSql));
            hidebtn();
            grdMain.Children.Add(new UCSample1());   //地球示例
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            hidebtn();
            grdMain.Children.Add(new UCSample2());   //平面示例
        }

        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            DataLayer.DirectDBAccessor.connections.Clear();
            DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("Data Source=(local);Initial Catalog=sdnvdb;User ID=sa;Password=123456", "localhost", DataLayer.EDataBaseType.MsSql));
            hidebtn();
            grdMain.Children.Add(new UCSample3());     //多实例示例
        }

        private void btn4_Click(object sender, RoutedEventArgs e)
        {
            DataLayer.DirectDBAccessor.connections.Clear();
            DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("Data Source=(local);Initial Catalog=sdnvdb;User ID=sa;Password=123456", "localhost", DataLayer.EDataBaseType.MsSql));
            hidebtn();
            grdMain.Children.Add(new UCSample4());  //测试动态显示
        }

        private void btn5_Click(object sender, RoutedEventArgs e)
        {
            DataLayer.DirectDBAccessor.connections.Clear();
            DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("Data Source=(local);Initial Catalog=sdnvdb;User ID=sa;Password=123456", "localhost", DataLayer.EDataBaseType.MsSql));
            hidebtn();
            grdMain.Children.Add(new UCSample5());  //测试动态载入
        }

        private void btn863_Click(object sender, RoutedEventArgs e)
        {
            DNVLibrary.UCDNV863.EDISTNET = DNVLibrary.UCDNV863.EDistnet.亦庄15;
            DataLayer.DirectDBAccessor.connections.Clear();
            DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("基础数据源1.5","Data Source=(local);Initial Catalog=nmsplan;User ID=sa;Password=123456", "localhost", DataLayer.EDataBaseType.MsSql));
            DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("互动数据源", "server=192.168.0.203;user id=root;database=cal_data;Password=123456", "192.168.0.203", DataLayer.EDataBaseType.MySql));


            hidebtn();
            grdMain.Children.Add(new DNVLibrary.UCDNV863());
        }

        private void btn863gis16_Click(object sender, RoutedEventArgs e)
        {
            DNVLibrary.UCDNV863.EDISTNET = DNVLibrary.UCDNV863.EDistnet.亦庄16;
            DataLayer.DirectDBAccessor.connections.Clear();
            DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("基础数据源1.6","server=localhost;user id=root;database=gis16;Password=123456", "localhost", DataLayer.EDataBaseType.MySql));

            hidebtn();
            grdMain.Children.Add(new DNVLibrary.UCDNV863());
        }

        private void btn863new_Click(object sender, RoutedEventArgs e)
        {
            DNVLibrary.UCDNV863.EDISTNET = DNVLibrary.UCDNV863.EDistnet.亦庄new;
            DataLayer.DirectDBAccessor.connections.Clear();
            DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("基础数据源", "Data Source=(local);Initial Catalog=distnetdb;User ID=sa;Password=123456", "localhost", DataLayer.EDataBaseType.MsSql));
            DataLayer.DataProvider.dataStatus = DataLayer.EDataStatus.数据库;

            hidebtn();
            grdMain.Children.Add(new DNVLibrary.UCDNV863());

        }


        void hidebtn()
        {
            grdbtn.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btnTool_Click(object sender, RoutedEventArgs e)
        {
            hidebtn();

            grdMain.Children.Clear();
            UserControl uc = null;
            uc = new MyBaseControls.ConfigTool.UCDeveloperEdit();
            grdMain.Children.Add(uc);


        }

        private void btnTooldb15_Click(object sender, RoutedEventArgs e)
        {
            hidebtn();
            DNVLibrary.UCDNV863.EDISTNET = DNVLibrary.UCDNV863.EDistnet.亦庄15;
            DataLayer.DirectDBAccessor.connections.Clear();
            DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("基础数据源1.5","Data Source=(local);Initial Catalog=nmsplan;User ID=sa;Password=123456", "localhost", DataLayer.EDataBaseType.MsSql));
            DataLayer.DataProvider.curDataSourceName = "基础数据源1.5";
            DistNetLibrary.Edit.WinDBDescTool win = new DistNetLibrary.Edit.WinDBDescTool(".\\xml\\DBDesc.xml"); win.ShowDialog();
        }

        private void btnTooldb16_Click(object sender, RoutedEventArgs e)
        {
            hidebtn();
            DNVLibrary.UCDNV863.EDISTNET = DNVLibrary.UCDNV863.EDistnet.亦庄16;
            DataLayer.DirectDBAccessor.connections.Clear();
            //DataLayer.DataProvider.databaseType = DataLayer.EDataBaseType.MySql;
            DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("基础数据源1.6","server=localhost;user id=root;database=gis16;Password=123456", "localhost", DataLayer.EDataBaseType.MySql));
            DataLayer.DataProvider.curDataSourceName = "基础数据源1.6";
            DistNetLibrary.Edit.WinDBDescTool win = new DistNetLibrary.Edit.WinDBDescTool(".\\xml\\DBDescYZ.xml"); win.ShowDialog();
        }

        private void btnTooldbnew_Click(object sender, RoutedEventArgs e)
        {
            hidebtn();
            DNVLibrary.UCDNV863.EDISTNET = DNVLibrary.UCDNV863.EDistnet.亦庄new;
            DataLayer.DirectDBAccessor.connections.Clear();
            //DataLayer.DataProvider.databaseType = DataLayer.EDataBaseType.MySql;
            DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("基础数据源", "Data Source=(local);Initial Catalog=distnetdb;User ID=sa;Password=123456", "localhost", DataLayer.EDataBaseType.MsSql));
            DataLayer.DataProvider.curDataSourceName = "基础数据源";
            DistNetLibrary.Edit.WinDBDescTool win = new DistNetLibrary.Edit.WinDBDescTool(".\\xml\\DBDescNewYZ.xml"); win.ShowDialog();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //grdMain.Children.Add(new DNVLibrary.UCDNV863());

        }


    }
}
