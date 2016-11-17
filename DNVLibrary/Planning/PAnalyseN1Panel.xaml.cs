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
    /// PAnalyseN1Panel.xaml 的交互逻辑
    /// </summary>
    public partial class PAnalyseN1Panel : UserControl, BaseIPanel
    {
        public PAnalyseN1Panel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
        }
        UCDNV863 root;
        Random rd = new Random();
        List<PowerBasicObject> noobjs=new List<PowerBasicObject>();

        public void load()
        {
            noobjs.Clear();
            foreach (pLayer layer in root.earth.objManager.zLayers.Values)
            {
                foreach (PowerBasicObject obj in layer.pModels.Values)
                {
                    //if (obj.busiData.busiSort == "线路")
                    if (obj is DistNetLibrary.DNACLine)
                    {
                        if (rd.NextDouble()<0.1)
                        {
                            noobjs.Add(obj);

                        }
                    }
                }
            }


            
            foreach (pPowerLine lin in noobjs)
            {
                savethickness = lin.thickness;
                savecolor = lin.color;
                lin.color = Colors.Red;
                lin.thickness = 0.0008f;
                lin.AnimationBegin(pPowerLine.EAnimationType.闪烁);

                lin.LabelColor = Colors.Yellow;
                lin.LabelSizeX = lin.LabelSizeY = 0.18f;
                lin.Label = lin.name;
                lin.isShowLabel = true;//初始化时放最后(暂时)

                pData additionObj = new pData(lin.parent) { id = lin.id + "标志", location = lin.location, valueScale = 0.01f, radScale = 0.0025f };
                additionObj.datas.Add(new Data() { id = lin.id + "数据", value = 1, argu = lin.name, color = Color.FromArgb(0xC3, 0xFF, 0x00, 0x00), geokey = "倒锥体" });
                lin.AddSubObject("sf", additionObj);
                additionObj.aniRotation.isDoAni = true;
            }

            root.earth.UpdateModel();

            lstObject.ItemsSource = noobjs;



            //===附加表格
            grid = new PShareGrid { Width = root.grdContent.ActualWidth - this.Width , Height = 210, Margin = new System.Windows.Thickness(0, 0, 0, 24), VerticalAlignment = System.Windows.VerticalAlignment.Bottom, HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
            root.grdContent.Children.Add(grid);

        }
        
        float savethickness=0.0001f;
        Color savecolor = Colors.AliceBlue;

        public void unload()
        {
            foreach (pPowerLine lin in noobjs)
            {
                lin.isShowLabel = false;
                lin.submodels.Clear();
                lin.thickness = savethickness;
                lin.color = savecolor;
                lin.AnimationStop(pPowerLine.EAnimationType.闪烁);
            }

            noobjs.Clear();
            root.earth.UpdateModel();


            root.grdContent.Children.Remove(grid);
        }


        private void lstObject_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //设置当前
             pPowerLine selobj =lstObject.SelectedItem as pPowerLine;


            propObj.SelectedObject = selobj.busiAccount;
            root.earth.camera.aniLook(selobj.VecLocation);
        }

        #region ===== 结论表格相关 =====
        PShareGrid grid;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button).Content.ToString();
            grid.ButtonClick(tag);
            uncheckall();
            if (grid.curtag != null)
                (sender as MyClassLibrary.MyButton).IsChecked = true;

        }

        void uncheckall()
        {
            foreach (MyClassLibrary.MyButton item in panelbutton.Children)
            {
                item.IsChecked = false;
            }
        }

        #endregion
    }
}
