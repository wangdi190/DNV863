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
    /// PReliabilityPanel.xaml 的交互逻辑
    /// </summary>
    public partial class PAnalyseReliabilityPanel : UserControl, BaseIPanel
    {
        public PAnalyseReliabilityPanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
        }
        UCDNV863 root;

        Random rd = new Random();
        public void load()
        {
            lstObj.MouseDoubleClick += new MouseButtonEventHandler(lstObj_MouseDoubleClick);

            Color[] colors = new Color[4];
            colors[0] = Colors.Red;
            colors[1] = Colors.Orange;
            colors[2] = Colors.Green;
            colors[3] = Colors.Blue;
            //载入
            int tmp;
            double value;
            foreach (PowerBasicObject obj in root.earth.objManager.getObjList().Where(p =>!(p is DistNetLibrary.DNGridArea) && !string.IsNullOrWhiteSpace(p.name)))
            {
                double tmpd = rd.NextDouble();
                if (tmpd < 0.1)
                { value = 0.99 + 0.009 * rd.NextDouble(); tmp = 0; }
                else if (tmpd < 0.2)
                { value = 0.999 + 0.0009 * rd.NextDouble(); tmp = 1; }
                else if (tmpd < 0.4)
                { value = 0.9999 + 0.00009 * rd.NextDouble(); tmp = 2; }
                else
                { value = 0.99999 + 0.000009 * rd.NextDouble(); tmp = 3; }

                if (obj is pPowerLine)
                    (obj as pPowerLine).color = colors[tmp];
                else if (obj is pSymbolObject)
                    (obj as pSymbolObject).color = colors[tmp];

                if (obj.busiData == null) obj.busiData = new busiBase(obj);
                obj.busiData.busiCurValue = value;
                obj.busiData.busiColor1 = colors[tmp];
                obj.busiData.busiFlag = tmp;
                obj.busiData.busiStr1 = string.Format("({0:p4})", value);
            }
            //对象列表
            lstObj.ItemsSource = root.earth.objManager.getObjList().Where(p => !(p is DistNetLibrary.DNGridArea) && !string.IsNullOrWhiteSpace(p.name)).OrderBy(p => p.busiData.busiCurValue).Take(100);

            //饼图
            var piedata = from e0 in root.earth.objManager.getObjList().Where(p => !(p is DistNetLibrary.DNGridArea) && !string.IsNullOrWhiteSpace(p.name))
                          group e0 by e0.busiData.busiFlag into g
                          orderby g.Key
                          select new
                          {
                              argu = g.Key,
                              value = g.Count()
                          };
            cht.DataSource = piedata;


        }

        PowerBasicObject selobj;
        void lstObj_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //恢复旧的
            if (selobj!=null)
            {
                if (lstObj.SelectedItem is pPowerLine)
                    (lstObj.SelectedItem as pPowerLine).AnimationStop(pPowerLine.EAnimationType.闪烁);
                else if (lstObj.SelectedItem is pSymbolObject)
                    (lstObj.SelectedItem as pSymbolObject).AnimationStop(pSymbolObject.EAnimationType.闪烁);

            }


            propObj.SelectedObject = (lstObj.SelectedItem as PowerBasicObject).busiAccount;
            root.earth.camera.aniLook((lstObj.SelectedItem as PowerBasicObject).VecLocation);
            if (lstObj.SelectedItem is pPowerLine)
                (lstObj.SelectedItem as pPowerLine).AnimationBegin(pPowerLine.EAnimationType.闪烁);
            else if (lstObj.SelectedItem is pSymbolObject)
                (lstObj.SelectedItem as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.闪烁);

            selobj = lstObj.SelectedItem as PowerBasicObject;

        }

        public void unload()
        {
            lstObj.MouseDoubleClick -= new MouseButtonEventHandler(lstObj_MouseDoubleClick);
            if (selobj != null)
            {
                if (lstObj.SelectedItem is pPowerLine)
                    (lstObj.SelectedItem as pPowerLine).AnimationStop(pPowerLine.EAnimationType.闪烁);
                else if (lstObj.SelectedItem is pSymbolObject)
                    (lstObj.SelectedItem as pSymbolObject).AnimationStop(pSymbolObject.EAnimationType.闪烁);

            }

        }

    }



}
