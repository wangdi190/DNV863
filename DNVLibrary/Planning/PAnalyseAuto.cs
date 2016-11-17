using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Planning
{
    class PAnalyseAuto:AppBase
    {
        public PAnalyseAuto(UCDNV863 Root)
            : base(Root)
        {
            panel = new PAnalyseAutoPanel(Root);
            grdPanel.Children.Add(panel);
        }

        internal override void load()
        {
            if (panel != null) (panel as BaseIPanel).load();
            root.earth.camera.adjustCameraAngle(0);
            root.earth.camera.adjustCameraDistance(0.38f);
            root.earth.colorManager.isEnabled = false;  //关闭色彩管理
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "规划_配网自动化";
        }

        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();
            root.earth.colorManager.isEnabled = true;  //打开色彩管理
            root.earth.refreshColor();//恢复色彩
        }
    }
}
