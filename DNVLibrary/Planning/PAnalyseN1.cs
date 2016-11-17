using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Planning
{

    class PAnalyseN1 : AppBase
    {
        public PAnalyseN1(UCDNV863 Root)
            : base(Root)
        {
            panel = new PAnalyseN1Panel(Root);
            grdPanel.Children.Add(panel);
        }


        internal override void load()
        {
            if (panel != null) (panel as BaseIPanel).load();
            root.earth.camera.adjustCameraAngle(50);
            root.earth.camera.adjustCameraDistance(0.5f);
            root.earth.colorManager.isEnabled = false;
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "规划_配网N-1";

        }

        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();
            root.earth.colorManager.isEnabled = true;

            
        }
    }

}
