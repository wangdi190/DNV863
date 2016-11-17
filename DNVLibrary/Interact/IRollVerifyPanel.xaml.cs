using System;
using System.Data;
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

namespace DNVLibrary.Interact
{
    /// <summary>
    /// IRollVerifyPanel.xaml 的交互逻辑
    /// </summary>
    public partial class IRollVerifyPanel : UserControl
    {
        public IRollVerifyPanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
            tmr.Tick += new EventHandler(tmr_Tick);
        }
        int timecount = 0;
        void tmr_Tick(object sender, EventArgs e)
        {
            //读计算状态
            timecount++;
            if (timecount > 60)   //若计算完成
            {
                tmr.Stop();
                bar.Value = 100;

                //若本页面可见，设置状态栏为无计算，若本页面不可见，设置状态栏为计算完成。
                statusbartask.status = this.IsVisible ? MyBaseControls.StatusBarTool.CalStatus.EStatus.无计算 : MyBaseControls.StatusBarTool.CalStatus.EStatus.计算完成;
                pnlList.Visibility = System.Windows.Visibility.Visible;
                readCalData();
            }

            bar.Value = 100.0 * timecount / 60;
        }

        UCDNV863 root;
        Random rd = new Random();

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            initdata();
        }
        SceneDataItem idxRun = new SceneDataItem(null);
        SceneDataItem idxPlanning = new SceneDataItem(null);
        SceneDataItem idxAdjust = new SceneDataItem(null);

        List<RadarDataItem> dat1;
        void initdata()
        {
            //读取规划指标
            string sql = "";
            string sim = "select top 1 iv1e1 as id, fmin0max1 as idx1, fmin0.9max1.1 as idx2, fmin3000max4000 as idx3, fmin0.1max0.3 as idx4, fmin0.9max1 as idx5, fmin0max1 as idx6, imin0max400 as hours";
            DataTable dt = DataLayer.DataProvider.getDataTable(sql, sim, DataLayer.EReadMode.模拟).Value;
            idxPlanning.idx1.value = dt.Rows[0].getDouble("idx1");
            idxPlanning.idx2.value = dt.Rows[0].getDouble("idx2");
            idxPlanning.idx3.value = dt.Rows[0].getDouble("idx3");
            idxPlanning.idx4.value = dt.Rows[0].getDouble("idx4");
            idxPlanning.idx5.value = dt.Rows[0].getDouble("idx5");
            idxPlanning.idx6.value = dt.Rows[0].getDouble("idx6");
            //读取运行指标
            dt = DataLayer.DataProvider.getDataTable(sql, sim, DataLayer.EReadMode.模拟).Value;
            idxRun.idx1.value = dt.Rows[0].getDouble("idx1");
            idxRun.idx2.value = dt.Rows[0].getDouble("idx2");
            idxRun.idx3.value = dt.Rows[0].getDouble("idx3");
            idxRun.idx4.value = dt.Rows[0].getDouble("idx4");
            idxRun.idx5.value = dt.Rows[0].getDouble("idx5");
            idxRun.idx6.value = dt.Rows[0].getDouble("idx6");


            dat1 = new List<RadarDataItem>();
            dat1.Add(new RadarDataItem() { argu = idxPlanning.idx1.name, sort = "1.规划", maxvalue = idxPlanning.idx1.max, value = idxPlanning.idx1.value, format = idxPlanning.idx1.labformat });
            dat1.Add(new RadarDataItem() { argu = idxPlanning.idx2.name, sort = "1.规划", maxvalue = idxPlanning.idx2.max, value = idxPlanning.idx2.value, format = idxPlanning.idx2.labformat });
            dat1.Add(new RadarDataItem() { argu = idxPlanning.idx3.name, sort = "1.规划", maxvalue = idxPlanning.idx3.max, value = idxPlanning.idx3.value, format = idxPlanning.idx3.labformat });
            dat1.Add(new RadarDataItem() { argu = idxPlanning.idx4.name, sort = "1.规划", maxvalue = idxPlanning.idx4.max, value = idxPlanning.idx4.value, format = idxPlanning.idx4.labformat });
            dat1.Add(new RadarDataItem() { argu = idxPlanning.idx5.name, sort = "1.规划", maxvalue = idxPlanning.idx5.max, value = idxPlanning.idx5.value, format = idxPlanning.idx5.labformat });
            dat1.Add(new RadarDataItem() { argu = idxPlanning.idx6.name, sort = "1.规划", maxvalue = idxPlanning.idx6.max, value = idxPlanning.idx6.value, format = idxPlanning.idx6.labformat });
            dat1.Add(new RadarDataItem() { argu = idxRun.idx1.name, sort = "2.运行", maxvalue = idxRun.idx1.max, value = idxRun.idx1.value, format = idxRun.idx1.labformat });
            dat1.Add(new RadarDataItem() { argu = idxRun.idx2.name, sort = "2.运行", maxvalue = idxRun.idx2.max, value = idxRun.idx2.value, format = idxRun.idx2.labformat });
            dat1.Add(new RadarDataItem() { argu = idxRun.idx3.name, sort = "2.运行", maxvalue = idxRun.idx3.max, value = idxRun.idx3.value, format = idxRun.idx3.labformat });
            dat1.Add(new RadarDataItem() { argu = idxRun.idx4.name, sort = "2.运行", maxvalue = idxRun.idx4.max, value = idxRun.idx4.value, format = idxRun.idx4.labformat });
            dat1.Add(new RadarDataItem() { argu = idxRun.idx5.name, sort = "2.运行", maxvalue = idxRun.idx5.max, value = idxRun.idx5.value, format = idxRun.idx5.labformat });
            dat1.Add(new RadarDataItem() { argu = idxRun.idx6.name, sort = "2.运行", maxvalue = idxRun.idx6.max, value = idxRun.idx6.value, format = idxRun.idx6.labformat });

            cht1.pens.Add(new Pen() { Brush = Brushes.Lime, Thickness = 1 });
            cht1.pens.Add(new Pen() { Brush = Brushes.Orange, Thickness = 1 });
            cht1.pens.Add(new Pen() { Brush = Brushes.Lime, Thickness = 1, DashStyle= DashStyles.Dash });
            cht1.title.Text = "指标对比雷达图";

            cht1.dataSource = dat1;



        }

        MyBaseControls.StatusBarTool.CalTask statusbartask;


        System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };

        private void btnCal_Click(object sender, RoutedEventArgs e)
        {
            clear();
            bar.Value = 0;

            statusbartask = MyBaseControls.StatusBarTool.StatusBarTool.statusInfo.calStatus.addCalTask("滚动校验", null);
            statusbartask.status = MyBaseControls.StatusBarTool.CalStatus.EStatus.计算中;

            //===提交服务申请
            timecount = 0;
            tmr.Start(); //持续读计算状态





        }
        void readCalData()
        {

            //读取调整后指标
            string sql = "";
            string sim = "select top 1 iv1e1 as id, fmin0max1 as idx1, fmin0.9max1.1 as idx2, fmin3000max4000 as idx3, fmin0max0.4 as idx4, fmin0max1 as idx5, fmin0max1 as idx6, imin0max400 as hours";

            idxAdjust.idx1.value = idxPlanning.idx1.value + (idxRun.idx1.value - idxPlanning.idx1.value) * rd.NextDouble();
            idxAdjust.idx2.value = idxPlanning.idx2.value + (idxRun.idx2.value - idxPlanning.idx2.value) * rd.NextDouble();
            idxAdjust.idx3.value = idxPlanning.idx3.value + (idxRun.idx3.value - idxPlanning.idx3.value) * rd.NextDouble();
            idxAdjust.idx4.value = idxPlanning.idx4.value + (idxRun.idx4.value - idxPlanning.idx4.value) * rd.NextDouble();
            idxAdjust.idx5.value = idxPlanning.idx5.value + (idxRun.idx5.value - idxPlanning.idx5.value) * rd.NextDouble();
            idxAdjust.idx6.value = idxPlanning.idx6.value + (idxRun.idx6.value - idxPlanning.idx6.value) * rd.NextDouble();


            //添补调整后之数据
            dat1.RemoveAll(p => p.sort == "3.调整规划");
            dat1.Add(new RadarDataItem() { argu = idxAdjust.idx1.name, sort = "3.调整规划", maxvalue = idxAdjust.idx1.max, value = idxAdjust.idx1.value, format = idxAdjust.idx1.labformat });
            dat1.Add(new RadarDataItem() { argu = idxAdjust.idx2.name, sort = "3.调整规划", maxvalue = idxAdjust.idx2.max, value = idxAdjust.idx2.value, format = idxAdjust.idx2.labformat });
            dat1.Add(new RadarDataItem() { argu = idxAdjust.idx3.name, sort = "3.调整规划", maxvalue = idxAdjust.idx3.max, value = idxAdjust.idx3.value, format = idxAdjust.idx3.labformat });
            dat1.Add(new RadarDataItem() { argu = idxAdjust.idx4.name, sort = "3.调整规划", maxvalue = idxAdjust.idx4.max, value = idxAdjust.idx4.value, format = idxAdjust.idx4.labformat });
            dat1.Add(new RadarDataItem() { argu = idxAdjust.idx5.name, sort = "3.调整规划", maxvalue = idxAdjust.idx5.max, value = idxAdjust.idx5.value, format = idxAdjust.idx5.labformat });
            dat1.Add(new RadarDataItem() { argu = idxAdjust.idx6.name, sort = "3.调整规划", maxvalue = idxAdjust.idx6.max, value = idxAdjust.idx6.value, format = idxAdjust.idx6.labformat });

            cht1.build();


            //模拟调整对象
            IEnumerable<PowerBasicObject> allobjs = root.distnet.getAllObjList().Where(p => !(p is pArea));
            foreach (var item in allobjs)
                item.color = Colors.Aqua;

            adjDatas = new List<ObjAdjustData>();
            for (int i = 0; i < 7; i++)
            {
                PowerBasicObject obj = allobjs.ElementAt(rd.Next(allobjs.Count()));
                EAdjustType atype = (EAdjustType)(1 + rd.Next(3));
                adjDatas.Add(new ObjAdjustData() { adjustType = atype, objName = obj.name, objID = obj.id, obj = obj });

                switch (atype)
                {
                    case EAdjustType.未知:
                        obj.color = Colors.White;
                        break;
                    case EAdjustType.新增:
                        obj.color = Colors.Lime;
                        break;
                    case EAdjustType.改造:
                        obj.color = Colors.Yellow;
                        break;
                    case EAdjustType.退运:
                        obj.color = Colors.Red;
                        break;
                }
                if (obj is pPowerLine)
                    (obj as pPowerLine).AnimationBegin(pPowerLine.EAnimationType.闪烁);
                else if (obj is pSymbolObject)
                    (obj as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.闪烁);

            }
            lstAdjust.ItemsSource = adjDatas;

        }
        List<ObjAdjustData> adjDatas;

        void clear()
        {
            dat1.RemoveAll(p => p.sort == "3.调整规划");
            cht1.build();

            if (adjDatas!=null)
            foreach (var item in adjDatas)
            {
                item.obj.color = Colors.Aqua;
                if (item.obj is pPowerLine)
                    (item.obj as pPowerLine).AnimationStop(pPowerLine.EAnimationType.闪烁);
                else if (item.obj is pSymbolObject)
                    (item.obj as pSymbolObject).AnimationStop(pSymbolObject.EAnimationType.闪烁);
            }

            lstAdjust.ItemsSource = null;

        }

        private void lstAdjust_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstAdjust.SelectedItem != null)
            {
                ObjAdjustData item = (ObjAdjustData)lstAdjust.SelectedItem;
                root.distnet.scene.camera.aniLook(item.obj.VecLocation);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!tmr.IsEnabled && statusbartask != null)
                statusbartask.status = MyBaseControls.StatusBarTool.CalStatus.EStatus.无计算;

        }

    }





}
