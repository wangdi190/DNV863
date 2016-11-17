using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Run
{

    class RFullViewSize : AppBase
    {
        public RFullViewSize(UCDNV863 Root)
            : base(Root)
        {
            //panel = new PRunFlowPanel(root);  //动画需求，不能放入load中
            //grdPanel.Children.Add(panel);
        }

        internal void show(bool isshow)
        {
            if (isshow)
            {
                root.grdMap.Opacity = 0.2;
                curcontrol.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                root.grdMap.Opacity = 1;
                curcontrol.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        RFullViewSizePanel curcontrol;
        internal override void load()
        {


            if (root.grdContent.Children.Contains(curcontrol))
                root.grdContent.Children.Remove(curcontrol);

            curcontrol = new RFullViewSizePanel() { Margin = new System.Windows.Thickness(20, 100, 20, 20) };
            root.grdContent.Children.Add(curcontrol);
        }



        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();

            root.grdMap.Opacity = 1;
            if (root.grdContent.Children.Contains(curcontrol))
                root.grdContent.Children.Remove(curcontrol);

            curcontrol = null;
        }

    }
}
