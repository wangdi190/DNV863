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
using WpfEarthLibrary;

namespace DNVLibrary.Planning
{
    /// <summary>
    /// PSupplyRangePanel.xaml 的交互逻辑
    /// </summary>
    public partial class PRunSupplyRangePanel : UserControl, BaseIPanel
    {
        public PRunSupplyRangePanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
        }
        UCDNV863 root;

        public void load()
        {
            Run.DataGenerator.initRunData(root.distnet);
            Run.DataGenerator.StartGenData(root.distnet);

            lstStation.MouseDoubleClick += new MouseButtonEventHandler(lstStation_MouseDoubleClick);

            //读取变电站列表

            lstStation.ItemsSource = root.distnet.getAllObjListByObjType(DistNetLibrary.EObjectType.变电站);// root.earth.objManager.getObjList("变电站").Where(p => (p as pSymbolObject).relationID.Count > 0);

        }

        //List<pPowerLine> anilines = new List<pPowerLine>();
        //pSymbolObject selobj;
        //pData additionObj;

        //RangeGenerator rangegen=new RangeGenerator(); //范围生成对象
        //pContour pRange;
        void lstStation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            root.distnet.clearTraceSupplyRange();
            DistNetLibrary.DNSubStation pds = lstStation.SelectedItem as DistNetLibrary.DNSubStation;
            root.distnet.showSupplyRange(pds);
            propObj.SelectedObject = pds.busiAccount;
            root.earth.camera.aniLook(pds.VecLocation);


            //恢复之前
            //foreach (pPowerLine lin in anilines)
            //{
            //    //lin.lineColor = Colors.Cyan;
            //    lin.objStatus = pPowerLine.ECStatus._正常;
            //    lin.thickness = 0.002f;
            //    lin.AnimationStop(pPowerLine.EAnimationType.闪烁);
            //}
            //anilines.Clear();
            //if (selobj != null) selobj.submodels.Clear();
            ////设置当前
            //selobj = lstStation.SelectedItem as pSymbolObject;
            //propObj.SelectedObject = selobj.busiAccount;
            //root.earth.camera.aniLook(selobj.VecLocation);

            //foreach (string lid in selobj.relationID)
            //{
            //    pPowerLine lin = (pPowerLine)root.earth.objManager.find(lid);
            //    anilines.Add(lin);
            //    //lin.lineColor = Colors.Red;
            //    lin.objStatus = pPowerLine.ECStatus.选择;
            //    lin.thickness = 0.004f;
            //    lin.AnimationBegin(pPowerLine.EAnimationType.闪烁);
            //}

            //additionObj = new pData(selobj.parent) { id = selobj.id + "范围", location = selobj.location, valueScale = 0.1f, radScale = 0.05f };
            //additionObj.datas.Add(new Data() { id = selobj.id + "数据", value = 1.2, argu = selobj.name, color = Color.FromArgb(0xC3, 0xFF, 0x00, 0x00), geokey = "倒锥体" });
            //additionObj.datas.Add(new Data() { id = selobj.id + "数据2", value = 1.2, argu = selobj.name, color = Color.FromArgb(0xC3, 0xFF, 0x00, 0x00), geokey = "正锥体" });
            //selobj.AddSubObject("sf", additionObj);
            //additionObj.aniRotation.isDoAni = true;


            ////生成供电范围
            //if (selobj.relationID.Count > 0)
            //{
            //    rangegen.drawObjects.Clear();
            //    rangegen.rad = 100;
            //    rangegen.brush = new SolidColorBrush(Color.FromArgb(0x37, 0xFF, 0xFF, 0xFF));
            //    foreach (string lid in selobj.relationID)
            //    {
            //        pPowerLine lin = (pPowerLine)root.earth.objManager.find(lid);
            //        rangegen.drawObjects.Add(new RangeGenerator.StruDrawObjDesc(lin.strPoints, 0));
            //    }
            //    rangegen.layerCount = 10;
            //    rangegen.GenRangeBrush();
            //    pLayer containerLayer;
            //    if (!root.earth.objManager.zLayers.TryGetValue("范围图层", out containerLayer))
            //    {
            //        containerLayer = root.earth.objManager.AddLayer("范围图层", "范围图层", "范围图层");
            //        containerLayer.deepOrder = -1;
            //    }
            //    //创建图形对象
            //    containerLayer.pModels.Clear();
            //    pRange = new pContour(containerLayer) { id = selobj.id + "范围图" };
            //    pRange.setRange(rangegen.minx, rangegen.maxx, rangegen.miny, rangegen.maxy);
            //    pRange.brush = rangegen.RangeBrush;
            //    containerLayer.AddObject("等值线", pRange);
            //}



            //root.earth.UpdateModel();
        }

        public void unload()
        {
            //lstStation.MouseDoubleClick -= new MouseButtonEventHandler(lstStation_MouseDoubleClick);
            ////恢复外观
            //foreach (pPowerLine lin in anilines)
            //{
            //    lin.lineColor = Colors.Cyan;
            //    lin.thickness = 0.002f;
            //    lin.AnimationStop(pPowerLine.EAnimationType.闪烁);
            //}
            //anilines.Clear();

            ////移除对象
            //if (selobj != null) selobj.submodels.Clear();
            ////移除范围对象
            //if (root.earth.objManager.zLayers.Keys.Contains("范围图层")) root.earth.objManager.zLayers.Remove("范围图层");

            ////清除生成器对象
            //rangegen.drawObjects.Clear();
            //rangegen = null;


            //root.earth.UpdateModel();

            lstStation.MouseDoubleClick -= new MouseButtonEventHandler(lstStation_MouseDoubleClick);
            Run.DataGenerator.StopGenData();
            Run.DataGenerator.clearAll();


            root.distnet.clearTraceSupplyRange();

        }
    }
}
