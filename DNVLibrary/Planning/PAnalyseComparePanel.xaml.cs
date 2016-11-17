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
    /// PAnalyseComparePanel.xaml 的交互逻辑
    /// </summary>
    public partial class PAnalyseComparePanel : UserControl, BaseIPanel
    {
        public PAnalyseComparePanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
        }
        UCDNV863 root;
        System.Collections.ObjectModel.ObservableCollection<CompareData> datas = new System.Collections.ObjectModel.ObservableCollection<CompareData>();

        Random rd = new Random();

        ///<summary>设置指标</summary>
        void setIndex()
        {

            datas.Add(new CompareData(EItemType.经济性,"投资总额(万元)",  "235.5", "25.5",  "23.5" ,0,2));

            datas.Add(new CompareData(EItemType.规模, "新增线路(KM)",  "325.5",  "215.5",  "1024.5",2,1));
            datas.Add(new CompareData( EItemType.规模, "新增变压器(台)", "5",  "5",  "0" ));
            datas.Add(new CompareData( EItemType.规模, "新增变电容量(MW)", "25.5", "15.2",  "24.1" ,0,1));

            datas.Add(new CompareData( EItemType.供电质量, "可靠性(%)",  "99.9999%", "99.9925%", "99.9850%",0,2 ));
            datas.Add(new CompareData( EItemType.供电质量, "减少停电时间(h)", "586", "925",  "580" ));

            datas.Add(new CompareData(EItemType.主动性, "DG渗率透(%)", "16%", "21%", "8%",1,2));

        }

        List<WpfEarthLibrary.PowerBasicObject> objs1 = new List<WpfEarthLibrary.PowerBasicObject>();
        List<WpfEarthLibrary.PowerBasicObject> objs2 = new List<WpfEarthLibrary.PowerBasicObject>();
        List<WpfEarthLibrary.PowerBasicObject> objs3 = new List<WpfEarthLibrary.PowerBasicObject>();
        ///<summary>设置对象</summary>
        void setObject()
        {
            IEnumerable<WpfEarthLibrary.PowerBasicObject> objs = root.distnet.getAllObjList();
            foreach (var obj in objs)
            {
                Color c;
                if (rd.NextDouble()<0.05)
                {
                    c = Colors.Red;
                    objs1.Add(obj);
                }
                else if (rd.NextDouble()<0.10) 
                {
                    c = Colors.Blue;
                    objs2.Add(obj);
                }
                else if (rd.NextDouble()<0.15) 
                {
                    c = Colors.Lime;
                    objs3.Add(obj);
                }
                else
                {
                    c = Colors.Yellow;
                }
                root.distnet.scene.objManager.saveVisionProperty(obj);
                if (obj is WpfEarthLibrary.pSymbolObject)
                {
                    root.distnet.scene.objManager.saveVisionProperty(obj);
                    (obj as WpfEarthLibrary.pSymbolObject).color = c;
                }
                else if (obj is WpfEarthLibrary.pPowerLine)
                {
                    root.distnet.scene.objManager.saveVisionProperty(obj);
                    (obj as WpfEarthLibrary.pPowerLine).color = c;
                }


            }
        }




        public void load()
        {
            lstProjects.ItemsSource = datas;
            setIndex();
            setObject();

        }

        public void unload()
        {
            root.distnet.scene.objManager.restoreVisionProperty();
        }

        int cursel = 0;
        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            foreach (var obj in objs2)
                obj.logicVisibility = cursel==1;
            foreach (var obj in objs3)
                obj.logicVisibility = cursel==1;
            foreach (var obj in objs1)
                obj.logicVisibility = true;
            cursel = cursel == 1 ? 0 : 1;
            btn2.Background = Brushes.Transparent;
            btn3.Background = Brushes.Transparent;
            btn1.Background = cursel == 1 ? Brushes.Black : Brushes.Transparent;
            root.distnet.scene.UpdateModel();
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            foreach (var obj in objs1)
                obj.logicVisibility = cursel == 2;
            foreach (var obj in objs3)
                obj.logicVisibility = cursel == 2;
            foreach (var obj in objs2)
                obj.logicVisibility = true;
            cursel = cursel == 2 ? 0 : 2;
            btn1.Background = Brushes.Transparent;
            btn3.Background = Brushes.Transparent;
            btn2.Background = cursel == 2 ? Brushes.Black: Brushes.Transparent;
            root.distnet.scene.UpdateModel();
        }

        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            foreach (var obj in objs1)
                obj.logicVisibility = cursel == 3;
            foreach (var obj in objs2)
                obj.logicVisibility = cursel == 3;
            foreach (var obj in objs3)
                obj.logicVisibility = true;
            cursel = cursel == 3 ? 0 : 3;
            btn1.Background = Brushes.Transparent;
            btn2.Background = Brushes.Transparent;
            btn3.Background = cursel == 3 ? Brushes.Black : Brushes.Transparent;
            root.distnet.scene.UpdateModel();
        }

        private void btnSelectPrj1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSelectPrj2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSelectPrj3_Click(object sender, RoutedEventArgs e)
        {

        }

     

    }
}
