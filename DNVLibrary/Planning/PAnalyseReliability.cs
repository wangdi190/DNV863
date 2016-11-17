using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Planning
{
    class PAnalyseReliability:AppBase
    {
        public PAnalyseReliability(UCDNV863 Root)
            : base(Root)
        {
            panel = new PAnalyseReliabilityPanel(Root);
            grdPanel.Children.Add(panel);
        }

        internal override void load()
        {
            if (panel != null) (panel as BaseIPanel).load();
            root.earth.camera.adjustCameraAngle(0);
            root.earth.camera.adjustCameraDistance(0.4f);
            root.earth.colorManager.isEnabled = false;  //关闭色彩管理
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "规划_配网可靠性";
        }

        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();
            root.earth.colorManager.isEnabled = true;  //打开色彩管理
            root.earth.refreshColor();//恢复色彩
        }
    }
}
