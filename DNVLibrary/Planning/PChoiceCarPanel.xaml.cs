using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using WpfEarthLibrary;

namespace DNVLibrary.Planning
{
    /// <summary>
    /// PCarPanel.xaml 的交互逻辑
    /// </summary>
    public partial class PChoiceCarPanel : UserControl, BaseIPanel
    {
        public PChoiceCarPanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
        }

        Random rd = new Random();
        UCDNV863 root;
        public void load()
        {
            lstObj.MouseDoubleClick += new MouseButtonEventHandler(lstObj_MouseDoubleClick);
            init();

        }
        public void unload()
        {
            lstObj.MouseDoubleClick -= new MouseButtonEventHandler(lstObj_MouseDoubleClick);
            carlayer.pModels.Clear();
            rangelayer.pModels.Clear();
            root.earth.objManager.zLayers.Remove("carlayer");
            root.earth.objManager.zLayers.Remove("carrangelayer");
            root.earth.UpdateModel();
        }

        pLayer carlayer;
        pLayer rangelayer;
        List<string> symbolkeys;
        void init()
        {
            

            symbolkeys = root.earth.objManager.zSymbols.Values.Where(p => p.sort == "car").Select(p => p.id).ToList();
            int symcount = symbolkeys.Count;

            //模拟站桩
            rangelayer = root.earth.objManager.AddLayer("carrangelayer", "carrangelayer", "carrangelayer");
            carlayer = root.earth.objManager.AddLayer("carlayer", "carlayer", "carlayer");
            Rect range = root.earth.objManager.getBounds();
            for (int i = 0; i < 30; i++)
            {
                int tmpi = rd.Next(symcount);
                Point loca = new Point(range.X + range.Width * rd.NextDouble(), range.Y + range.Height * rd.NextDouble());
                pSymbolObject nobj = new pSymbolObject(carlayer)
                {
                    id = "car" + i,
                    name = "充电站" + i,
                    symbolid = symbolkeys[tmpi],
                    location = loca.ToString(),
                    isH = true,
                    scaleX = 0.0025f,
                    scaleY = 0.0025f,
                    color = Color.FromRgb(0xFF, 0x66, 0x00)
                };
                if (nobj.busiData == null) nobj.busiData = new busiBase(nobj);
                nobj.busiData.busiValue1 = (5 + rd.Next(6)); //覆盖半径
                nobj.busiData.busiStr1 = symbolkeys[tmpi];//名称
                nobj.busiData.busiDatetime = DateTime.Now.AddDays(rd.Next(2000));//投运时间
                nobj.busiData.busiSort = "充电桩";
                nobj.busiData.busiBrush = root.earth.objManager.zSymbols[nobj.symbolid].brush;
                carlayer.AddObject(nobj.id, nobj);
                //台账
                AccountCar acc = new AccountCar()
                {
                    id = nobj.id,
                    name = nobj.name,
                    address = "A区B街",
                    cap = 100 * (1 + rd.Next(10)),
                    frhour = 3 + rd.Next(10),
                    geo = nobj.location,
                    nums = 1 + rd.Next(5),
                    ptype = nobj.busiData.busiStr1,
                    rad = 2 + rd.Next(10),
                    rcmode = (AccountCar.Ertype)rd.Next(3),
                    rdate = DateTime.Now.AddDays(100 + rd.Next(8000)).ToString("yyyy年MM月"),
                    region = "某区",
                    rmode = (AccountCar.Ermode)rd.Next(4),
                    vl = 380
                };
                nobj.busiAccount = acc;



                //范围
                pSymbolObject robj = new pSymbolObject(rangelayer)
                {
                    id = i + "carrange",
                    symbolid = "充电桩覆盖范围",
                    location = loca.ToString(),
                    isH = true,
                    isUseColor = false
                };
                robj.scaleX = robj.scaleY =(float)( nobj.busiData.busiValue1 / 100);
                if (robj.busiData == null) robj.busiData = new busiBase(robj);
                robj.busiData.busiSort = "覆盖范围";

                rangelayer.AddObject(robj.id, robj);


                

            }
            root.earth.UpdateModel();


            //初始化图例
            lstType.ItemsSource = root.earth.objManager.zSymbols.Values.Where(p => p.sort == "car");
            //分类占比
            cht.DataSource = from e0 in root.earth.objManager.getObjList("充电桩")
                             group e0 by e0.busiData.busiStr1 into g
                             select new
                             {
                                 argu = g.Key,
                                 value = g.Count()
                             };
            lstObj.ItemsSource = root.earth.objManager.getObjList("充电桩");

        }

        void lstObj_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            root.earth.camera.aniLook((lstObj.SelectedItem as PowerBasicObject).VecLocation);
            propObj.SelectedObject = (lstObj.SelectedItem as PowerBasicObject).busiAccount;
        }



    }



    internal class AccountCar
    {
        public enum Ertype { 普通充电, 快速充电, 电池更换 };
        public enum Ermode { 恒流充电, 阶段充电, 恒压充电, 快速充电 };


        [Category("标志"), DisplayName("ID")]
        public string id { get; set; }
        [Category("标志"), DisplayName("名称")]
        public string name { get; set; }

        [Category("地理"), DisplayName("坐标")]
        public string geo { get; set; }
        [Category("地理"), DisplayName("区域")]
        public string region { get; set; }
        [Category("地理"), DisplayName("位置")]
        public string address { get; set; }

        [Category("技术参数"), DisplayName("布点类型")]
        public string ptype { get; set; }
        [Category("技术参数"), DisplayName("充换方式")]
        public Ertype rcmode { get; set; }
        [Category("技术参数"), DisplayName("服务半径KM")]
        public double rad { get; set; }
        [Category("技术参数"), DisplayName("充电设备数")]
        public int nums { get; set; }
        [Category("技术参数"), DisplayName("平均满充时间h")]
        public int frhour { get; set; }
        [Category("技术参数"), DisplayName("充电方法")]
        public Ermode rmode { get; set; }
        [Category("技术参数"), DisplayName("电压等级")]
        public int vl { get; set; }
        [Category("技术参数"), DisplayName("单路配电容量")]
        public int cap { get; set; }

        [Category("其它"), DisplayName("投运时间")]
        public string rdate { get; set; }




    }
}
