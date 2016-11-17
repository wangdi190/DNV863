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
using DistNetLibrary;

namespace DNVLibrary.Run
{

    /// <summary>
    /// PFlowPanel.xaml 的交互逻辑
    /// </summary>
    public partial class RRunFlowPanel : UserControl, BaseIPanel
    {
        public RRunFlowPanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
        }

        PanelData paneldata;
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            paneldata = new PanelData(root);
            grdMain.DataContext = paneldata;
            paneldata.start();

            DataGenerator.UpdateLineData = updatelinedata;
            DataGenerator.UpdateStationData = updatestationdata;
            DataGenerator.UpdateSwitchData = updateswitchdata;


            //
            if (!System.Windows.Application.Current.Resources.Contains("imgup"))
            {
                Application.Current.Resources.Add("imgup", this.Resources["imgup"]);
                Application.Current.Resources.Add("imgmid", this.Resources["imgmid"]);
                Application.Current.Resources.Add("imgdown", this.Resources["imgdown"]);
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            (panel.Children[0] as RRunInfoPanel).status = RRunInfoPanel.EStatus.最大化;
        }


        internal bool isShowFlow, isShowLoadColumn, isShowVLContour, isShowSectionColor, isShowLoadColor, isShowVLColor, isShowCutArea;

        bool isLoadColumnInited, isVLContourInited, isCutAreaInited; //用于判断是否已实始化
        Color saveStationColor, saveLineColor;



        public void load()
        {

            root.earth.config.tooltipMoveEnable = true;



        }

        internal void refresh()
        {
            updatelinedata();
            updatestationdata();
            root.earth.UpdateModel();


        }

        #region ===== 右边面板相关 =====



        #endregion


        void updateswitchdata()
        {

        }

        void updatelinedata()
        {
            if (isShowFlow)
                root.distnet.showFlow();
            else
                root.distnet.clearFlow();


            //RunDataACLine data;
            //foreach (DNACLine lin in root.distnet.getAllObjListByObjType(EObjectType.输电线路))
            //{
            //    data = lin.busiRunData as RunDataACLine;
            //    lin.isFlow = isShowFlow;
            //    if (isShowFlow)
            //    {
            //        lin.arrowSize = (float)(0.0006 * (1 + data.rateOfLoad));
            //        lin.lineColor = MyClassLibrary.Share2D.MediaHelper.getColorBetween(data.rateOfLoad, Colors.Blue, Colors.Red);
            //    }
            //    else
            //    {
            //        lin.arrowSize = 0.0006f;
            //        lin.lineColor = Colors.Yellow;
            //    }
            //}
        }
        void updatestationdata()
        {
            bool isupdatemodel = false;

            //变压负载柱
            if (isShowLoadColumn)
                root.distnet.showLoadCol();
            else
                root.distnet.clearLoadCol();



            //电压等高线
            if (isShowVLContour)
                root.distnet.showVLContour();
            else
                root.distnet.hideVLContour();

            //----- 停电
            if (isShowCutArea)
            {
                IEnumerable<PowerBasicObject> objs = root.distnet.getAllObjListByCategory( EObjectCategory.开关类);
                PowerBasicObject obj=objs.ElementAt(rd.Next(objs.Count()));
                root.distnet.showSupplyRange(obj, Color.FromArgb(80,20,20,20));
            }
            else
            {
                root.distnet.clearTraceSupplyRange();
            }



            //if (isShowCutArea && !isCutAreaInited)
            //    initCutArea();

            //if (isShowCutArea && cutAreaLayer.pModels.Count > 0)
            //{
            //    cutAreaLayer.pModels.Clear();
            //    isupdatemodel = true;
            //}

            ////随机取变压器生成停电区域

            //if (isShowCutArea && rd.NextDouble()<0.2)
            //{
            //    IEnumerable<PowerBasicObject> objs = root.distnet.getAllObjListByObjType(EObjectType.配变);
            //    DNDistTransformer obj=objs.ElementAt(rd.Next(objs.Count())) as DNDistTransformer;
            //    List<EObjectType> findtypes = new List<EObjectType>() { EObjectType.两卷主变, EObjectType.三卷主变 };
            //    List<Dictionary<string, PowerBasicObject>> tmp =root.distnet.getTrace(obj, findtypes, null, true);

            //    if (tmp.Count > 0 && tmp[0].Count>0)
            //    {
            //        rangegen.drawObjects.Clear();
            //        rangegen.rad = 100;
            //        rangegen.brush = new SolidColorBrush(Color.FromArgb(0x37, 0x34, 0x34, 0x34));
            //        foreach (string lid in tmp[0].Keys)
            //        {
            //            PowerBasicObject lin = root.earth.objManager.find(lid);
            //            if (lin is pPowerLine)
            //                rangegen.drawObjects.Add(new RangeGenerator.StruDrawObjDesc((lin as pPowerLine).strPoints, 0));
            //        }
            //        rangegen.layerCount = 10;
            //        rangegen.GenRangeBrush();

            //        //创建图形对象
            //        pRange = new pContour(cutAreaLayer) { id = obj.id + "范围图" };
            //        pRange.setRange(rangegen.minx, rangegen.maxx, rangegen.miny, rangegen.maxy);
            //        pRange.brush = rangegen.RangeBrush;



            //        cutAreaLayer.AddObject(pRange.id, pRange);
            //        isupdatemodel = true;

            //        if (cutAreaLayer != null)
            //            cutAreaLayer.isShow = true;
            //    }
            //}
            //else
            //{
            //    if (cutAreaLayer != null)
            //        cutAreaLayer.isShow = false;
            //}

       
            //if (isupdatemodel)
            //    root.earth.UpdateModel();

        }

        RangeGenerator rangegen = new RangeGenerator(); //范围生成对象
        pContour pRange;


        public void unload()
        {
            DataGenerator.UpdateLineData = DataGenerator.UpdateStationData = DataGenerator.UpdateSwitchData = null;

            root.earth.config.tooltipMoveEnable = false;
            root.distnet.clearVLContour(); 
            root.distnet.clearFlow();
            root.distnet.clearLoadCol();

            root.distnet.clearTraceSupplyRange();
            //if (root.earth.objManager.zLayers.Keys.Contains("停电图层"))
            //{
            //    root.earth.objManager.zLayers.Remove("停电图层");
            //    cutAreaLayer.pModels.Clear();
            //}
        }


        Random rd = new Random();
        UCDNV863 root;
        pLayer cutAreaLayer;
        void initCutArea()
        {
            if (!root.earth.objManager.zLayers.TryGetValue("停电图层", out cutAreaLayer))
                cutAreaLayer = root.earth.objManager.AddLayer("停电图层", "停电图层", "停电图层");
        }


        private void RRunInfoPanel_OnClickHeader(object sender, EventArgs e)
        {
            RRunInfoPanel pan = sender as RRunInfoPanel;
            pan.status = RRunInfoPanel.EStatus.最大化;
        }


    }






}
