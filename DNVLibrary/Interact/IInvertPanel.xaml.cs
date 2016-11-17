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
    /// IInvertPanel.xaml 的交互逻辑
    /// </summary>
    public partial class IInvertPanel : UserControl
    {
        public IInvertPanel(UCDNV863 Root)
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

                readCalData();
            }

            bar.Value = 100.0 * timecount / 60;
        }

        Random rd = new Random();
        string prjname;
        UCDNV863 root;
        SceneDatas scenedata = new SceneDatas();
        SceneDataItem prjindex = new SceneDataItem(null);  //方案指标

        MyBaseControls.StatusBarTool.CalTask statusbartask;


        System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };
        private void btnInvertCal_Click(object sender, RoutedEventArgs e)
        {

            prjname =(IInvertController.uctop.cmbTree.SelectedItem as Item).instanceName;
            bar.Value = 0;

            statusbartask = MyBaseControls.StatusBarTool.StatusBarTool.statusInfo.calStatus.addCalTask("指标反演", null);
            statusbartask.status = MyBaseControls.StatusBarTool.CalStatus.EStatus.计算中;

            //===提交服务申请
            timecount = 0;
            tmr.Start(); //持续读计算状态
            

        }

        ///<summary>改变方案</summary>
        internal void changeProject()
        {
            if (IInvertController.uctop.cmbTree.SelectedItem == null) return;

            //清除之前效果
            root.distnet.clearFlow();
            root.distnet.clearLoadCol();
            root.distnet.clearVLContour();



            prjname = (IInvertController.uctop.cmbTree.SelectedItem as Item).instanceName;
            //方案指标
            string sql = "";
            string sim = "select top 1 iv1e1 as id, fmin0max1 as idx1, fmin0.9max1.1 as idx2, fmin3000max4000 as idx3, fmin0max0.4 as idx4, fmin0max1 as idx5, fmin0max1 as idx6, imin0max400 as hours";
            DataTable dt = DataLayer.DataProvider.getDataTable(sql, sim, DataLayer.EReadMode.模拟).Value;

            prjindex.idx1.value = dt.Rows[0].getDouble("idx1");
            prjindex.idx2.value = dt.Rows[0].getDouble("idx2");
            prjindex.idx3.value = dt.Rows[0].getDouble("idx3");
            prjindex.idx4.value = dt.Rows[0].getDouble("idx4");
            prjindex.idx5.value = dt.Rows[0].getDouble("idx5");
            prjindex.idx6.value = dt.Rows[0].getDouble("idx6");

            grdMain.DataContext = prjindex;
            pnlScene.Visibility = System.Windows.Visibility.Collapsed;
            pnlList.Visibility = System.Windows.Visibility.Collapsed;

        }

        ///<summary>读取场景统计数据</summary>
        void readSceneData()
        {
            //40场景
            string sql = "";
            string sim = "select top 40 iv1e1 as id, fmin0max1 as idx1, fmin0.9max1.1 as idx2, fmin0max4000 as idx3, fmin0max0.4 as idx4, fmin0max1 as idx5, fmin0max1 as idx6, imin0max400 as hours";
            DataTable dt = DataLayer.DataProvider.getDataTable(sql, sim, DataLayer.EReadMode.模拟).Value;

            foreach (DataRow dr in dt.Rows)
            {
                SceneDataItem tmp = new SceneDataItem(scenedata);
                tmp.num = dr.getInt("id");
                tmp.hours = dr.getInt("hours");
                tmp.idx1.value = dr.getDouble("idx1");
                tmp.idx2.value = dr.getDouble("idx2");
                tmp.idx3.value = dr.getDouble("idx3");
                tmp.idx4.value = dr.getDouble("idx4");
                tmp.idx5.value = dr.getDouble("idx5");
                tmp.idx6.value = dr.getDouble("idx6");

                scenedata.items.Add(tmp);

            }
            scenedata.init();
            foreach (var item in scenedata.items)
            {
                scenepanel.Children.Add(item.button);
                item.button.MouseDown += new MouseButtonEventHandler(button_MouseDown);
            }


        }
        void readCalData()
        {
            pnlScene.Visibility = System.Windows.Visibility.Visible;
            pnlList.Visibility = System.Windows.Visibility.Visible;

            readSceneData();

            //模拟调整对象
            IEnumerable<PowerBasicObject> allobjs = root.distnet.getAllObjList().Where(p => !(p is pArea));
            foreach (var item in allobjs)
                item.color = Colors.Aqua;

            List<ObjAdjustData> adjDatas = new List<ObjAdjustData>();
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

        int curSceneNum = 0;
        void button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SceneButton btn = sender as SceneButton;
            if (curSceneNum > 0)
            {
                scenedata.items[curSceneNum - 1].button.Background = Brushes.Black;
                scenedata.items[curSceneNum - 1].button.txtNum.Foreground = new SolidColorBrush(Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF));
            }

            btn.Background = Brushes.AliceBlue;
            btn.txtNum.Foreground = new SolidColorBrush(Color.FromArgb(0x7F, 0x99, 0x32, 0xCC));

            curSceneNum = btn.data.num;

            refreshData();

            e.Handled = true;
        }

        ///<summary>读取数据并刷新界面</summary>
        void refreshData()
        {
            grdMain.DataContext = scenedata.items[curSceneNum - 1];

            //zh注
            root.distnet.showFlow();
            root.distnet.showLoadCol();
            root.distnet.showVLContour();

        }



        private void lstAdjust_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ObjAdjustData item = (ObjAdjustData)lstAdjust.SelectedItem;
            root.distnet.scene.camera.aniLook(item.obj.VecLocation);
        }

        ///<summary>场景排序</summary>
        private void btnord1_Click(object sender, RoutedEventArgs e)
        {
            scenepanel.Children.Clear();
            foreach (var item in scenedata.items.OrderByDescending(p => p.hours))
                scenepanel.Children.Add(item.button);
        }

        private void btnord2_Click(object sender, RoutedEventArgs e)
        {
            scenepanel.Children.Clear();
            foreach (var item in scenedata.items.OrderByDescending(p => p.idx3.value))
                scenepanel.Children.Add(item.button);

        }

        private void btnord3_Click(object sender, RoutedEventArgs e)
        {
            scenepanel.Children.Clear();
            foreach (var item in scenedata.items.OrderByDescending(p => p.idx4.value))
                scenepanel.Children.Add(item.button);

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!tmr.IsEnabled && statusbartask != null)
                statusbartask.status = MyBaseControls.StatusBarTool.CalStatus.EStatus.无计算;

        }

    }
}
