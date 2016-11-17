using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Run
{
    class RFullViewGridArea : AppBase
    {
        public RFullViewGridArea(UCDNV863 Root)
            : base(Root)
        {
            //panel = new PRunFlowPanel(root);  //动画需求，不能放入load中
            //grdPanel.Children.Add(panel);
        }

        internal void show(bool isshow)
        {
            WpfEarthLibrary.pLayer arealayer = root.earth.objManager.zLayers[DistNetLibrary.EObjectCategory.区域类.ToString()];
            arealayer.logicVisibility = isshow;
            root.earth.UpdateModel();

            //if (isshow)
            //    curcontrol.Visibility = System.Windows.Visibility.Visible;
            //else
            //    curcontrol.Visibility = System.Windows.Visibility.Collapsed;

            root.earth.legendManager.isShow = isshow;

        }

         System.Windows.Controls.StackPanel curcontrol;
        internal override void load()
        {
            //curcontrol = new System.Windows.Controls.StackPanel() { HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = System.Windows.VerticalAlignment.Center, Margin = new System.Windows.Thickness(30, 0, 0, 0), IsHitTestVisible=false , };
            //WpfEarthLibrary.pLayer arealayer = root.earth.objManager.zLayers[DistNetLibrary.EObjectCategory.区域类.ToString()];
            //var tmp = from e1 in arealayer.pModels.Values
            //          group e1 by (e1.busiAccount as DistNetLibrary.AcntGridArea).useType into g
            //          select new
            //          {
            //              usetype = g.Key,
            //              color = (g.First() as DistNetLibrary.DNGridArea).color,
            //          };

            //foreach (var item in tmp)
            //{
            //    System.Windows.Controls.StackPanel sp = new System.Windows.Controls.StackPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal };
            //    //sp.Effect = new System.Windows.Media.Effects.DropShadowEffect(){ShadowDepth=0};
            //    System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle() { Width = 10, Height = 10, Fill = new SolidColorBrush(item.color), StrokeThickness = 1, Stroke = Brushes.White };
            //    System.Windows.Controls.TextBlock txt = new System.Windows.Controls.TextBlock() {FontSize=14, Text = item.usetype, Foreground = Brushes.Aqua ,Margin=new System.Windows.Thickness(3,0,0,0)};
            //    sp.Children.Add(rect);
            //    sp.Children.Add(txt);
            //    curcontrol.Children.Add(sp);
            //}

            //curcontrol.Visibility = System.Windows.Visibility.Collapsed;
            //root.grdContent.Children.Add(curcontrol);

            root.earth.legendManager.isShow = true;
            WpfEarthLibrary.BrushLegend legend = root.earth.legendManager.createBrushLegend("区块图例");

            legend.isShow = true;
            legend.panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            legend.panel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            legend.panel.Margin = new System.Windows.Thickness(10, 250, 0, 0);
            legend.header = "区域类型";
            WpfEarthLibrary.pLayer arealayer = root.earth.objManager.zLayers[DistNetLibrary.EObjectCategory.区域类.ToString()];
            var tmp = from e1 in arealayer.pModels.Values
                      group e1 by (e1.busiAccount as DistNetLibrary.AcntGridArea).useType into g
                      select new
                      {
                          usetype = g.Key,
                          color = (g.First() as DistNetLibrary.DNGridArea).color,
                      };

            foreach (var item in tmp)
            {
                legend.addItem(new SolidColorBrush(item.color), item.usetype);
            }
        }



        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();

            root.grdMap.Opacity = 1;
            //if (root.grdContent.Children.Contains(curcontrol))
            //    root.grdContent.Children.Remove(curcontrol);

            //curcontrol = null;
            root.earth.legendManager.deleteLegend("区块图例");


            WpfEarthLibrary.pLayer arealayer = root.earth.objManager.zLayers[DistNetLibrary.EObjectCategory.区域类.ToString()];
            arealayer.logicVisibility = false;
            root.earth.UpdateModel();
        }

    }
}
