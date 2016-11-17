using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Planning
{
    class PEvaluteIndex:AppBase
    {
        public PEvaluteIndex(UCDNV863 Root)
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

            curcontrol = new MyControlLibrary.Controls3D.Index3D.UserControl1();
            root.grdContent.Children.Add(curcontrol);
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "规划_指标_12面体";

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
