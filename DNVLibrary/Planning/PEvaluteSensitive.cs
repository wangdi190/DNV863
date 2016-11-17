using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Planning
{
    class PEvaluteSensitive : AppBase
    {
        public PEvaluteSensitive(UCDNV863 Root)
            : base(Root)
        {
            //panel = new PRunFlowPanel(root);  //动画需求，不能放入load中
            //grdPanel.Children.Add(panel);
        }

        System.Windows.Controls.UserControl curcontrol;
        internal override void load()
        {
            root.grdMap.Opacity = 0.2;

            if (root.grdContent.Children.Contains(curcontrol))
                root.grdContent.Children.Remove(curcontrol);
            curcontrol = new PEvaluteSensitiveMain();
            curcontrol.Margin = new System.Windows.Thickness(20, 100, 20, 20);
            root.grdContent.Children.Add(curcontrol);
        }


        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();

            root.grdMap.Opacity = 1;
            if (root.grdContent.Children.Contains(curcontrol))
                root.grdContent.Children.Remove(curcontrol);
        }


    }


}
