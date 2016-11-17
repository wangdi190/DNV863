using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using System.Windows.Media.Media3D;
using MyClassLibrary.Share3D;


namespace DNV863
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("基础数据源1.5",DNV863.Properties.Settings.Default.conn, DNV863.Properties.Settings.Default.ip, DataLayer.EDataBaseType.MsSql));
            WpfEarthLibrary.Config.MapIP = DNV863.Properties.Settings.Default.mapurl;
            WpfEarthLibrary.Config.MapPath = DNV863.Properties.Settings.Default.mappath;
            WpfEarthLibrary.Config.MapPath2 = DNV863.Properties.Settings.Default.mappath2;

            MyBaseControls.LogTool.Log.addLog(String.Format("已设置地图瓦片来源{0}({1})", WpfEarthLibrary.Config.MapIP, this), MyBaseControls.LogTool.ELogType.记录);

            //======== 不同的配网进行必要的设置 =================
            DataLayer.DataProvider.dataStatus = DataLayer.EDataStatus.模拟; 
            DataLayer.DirectDBAccessor.connections.Clear();
            switch (DNVLibrary.UCDNV863.EDISTNET)
            {
                case DNVLibrary.UCDNV863.EDistnet.亦庄15:
                    //DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("Data Source=192.168.0.203;Initial Catalog=nmsplan;User ID=sa;Password=123456", "192.168.0.203"));
                    DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("基础数据源1.5", "Data Source=(local);Initial Catalog=nmsplan;User ID=sa;Password=123456", "localhost", DataLayer.EDataBaseType.MsSql));
                    DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("互动数据源", "server=192.168.0.203;user id=root;database=cal_data;Password=123456", "192.168.0.203", DataLayer.EDataBaseType.MySql));
                    //DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo(DNV863.Properties.Settings.Default.conn, DNV863.Properties.Settings.Default.ip));
                    break;
                case DNVLibrary.UCDNV863.EDistnet.亦庄16:
                    DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("基础数据源1.6", "server=localhost;user id=root;database=gis16;Password=123456", "localhost", DataLayer.EDataBaseType.MySql));
                    //DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("临时数据源", "server=localhost;user id=root;database=yzdistrict;Password=123456", "localhost", DataLayer.EDataBaseType.MySql));
                    break;
                case DNVLibrary.UCDNV863.EDistnet.亦庄new:
                    DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("基础数据源", "Data Source=(local);Initial Catalog=distnetdb;User ID=sa;Password=123456", "localhost", DataLayer.EDataBaseType.MsSql));
                    DataLayer.DataProvider.dataStatus = DataLayer.EDataStatus.数据库;
                    break;
                case DNVLibrary.UCDNV863.EDistnet.厦门:
                    DataLayer.DirectDBAccessor.connections.Add(new DataLayer.ConnInfo("临时数据源", "server=localhost;user id=root;database=yzdistrict;Password=123456", "localhost", DataLayer.EDataBaseType.MySql));
                    break;
                default:
                    break;
            }


            MyBaseControls.LogTool.Log.addLog(String.Format("数据库配置为使用{0}({1})", DataLayer.EDataBaseType.MySql, this), MyBaseControls.LogTool.ELogType.记录);

            

            //========== Theme ==========
            DevExpress.Xpf.Core.Theme theme = new DevExpress.Xpf.Core.Theme("Theme863Black", "DevExpress.Xpf.Themes.Theme863Black.v14.1");
            theme.AssemblyName = "DevExpress.Xpf.Themes.Theme863Black.v14.1";
            DevExpress.Xpf.Core.Theme.RegisterTheme(theme);

            theme = new DevExpress.Xpf.Core.Theme("Theme863Black2", "DevExpress.Xpf.Themes.Theme863Black2.v14.1");
            theme.AssemblyName = "DevExpress.Xpf.Themes.Theme863Black2.v14.1";
            DevExpress.Xpf.Core.Theme.RegisterTheme(theme);

            //loadMeshResource();
        }


        /// <summary>
        /// 装载公用的mesh资源,此类资源是程序化生成的
        /// </summary>
        private void loadMeshResource()
        {
            MeshGeometry3D mesh = Model3DHelper.genCylinder3DMesh();
            mesh.Freeze();
            this.Resources.Add("meshCyl", mesh);
            mesh = Model3DHelper.genCylinder3DTopMesh();
            mesh.Freeze();
            this.Resources.Add("meshCylTop", mesh);
            mesh = Model3DHelper.genCube3DMesh();
            mesh.Freeze();
            this.Resources.Add("meshCube", mesh);
            mesh = Model3DHelper.genTextVPlaneMesh();
            mesh.Freeze();
            this.Resources.Add("meshText", mesh);
            mesh = Model3DHelper.genTextHPlaneMesh();
            mesh.Freeze();
            this.Resources.Add("meshTextH", mesh);
            mesh = Model3DHelper.genSphereMesh(0, 1);
            mesh.Freeze();
            this.Resources.Add("meshSphere", mesh);
            mesh = Model3DHelper.genCone3DMesh(new Point3D(0, 1, 0), new Point3D(0, -1, 0), 0, 1);
            mesh.Freeze();
            this.Resources.Add("meshCone", mesh);
            mesh = Model3DHelper.genBoxTopMesh();
            mesh.Freeze();
            this.Resources.Add("meshBoxTop", mesh);
            mesh = Model3DHelper.genBoxMidMesh();
            mesh.Freeze();
            this.Resources.Add("meshBoxMid", mesh);
            mesh = Model3DHelper.genCylTopMesh();
            mesh.Freeze();
            this.Resources.Add("meshSCylTop", mesh);
            mesh = Model3DHelper.genCylMidMesh();
            mesh.Freeze();
            this.Resources.Add("meshSCylMid", mesh);

            mesh = Model3DHelper.genPlantMesh();
            mesh.Freeze();
            this.Resources.Add("meshHD", mesh);
            mesh = Model3DHelper.genPlantWindMesh();
            mesh.Freeze();
            this.Resources.Add("meshFD", mesh);
        }
    }
}
