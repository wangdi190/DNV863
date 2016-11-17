using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Planning
{
    class PEvaluteIDX : AppBase
    {
        public PEvaluteIDX(UCDNV863 Root)
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
            curcontrol = new IDXKPI.UserControl1();
            curcontrol.Margin = new System.Windows.Thickness(0, 80, 0, 0);
            root.grdContent.Children.Add(curcontrol);

            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "规划_指标_评价指数";
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
