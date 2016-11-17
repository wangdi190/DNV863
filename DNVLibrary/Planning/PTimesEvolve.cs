using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Planning
{
    class PTimesEvolve:AppBase
    {

        public PTimesEvolve(UCDNV863 Root)
            : base(Root)
        {
            panel = new PTimesEvolvePanel(Root);
            grdPanel.Children.Add(panel);
        }

        internal override void load()
        {
            if (panel != null) (panel as BaseIPanel).load();
            root.earth.camera.adjustCameraAngle(0);
           // root.earth.camera.adjustCameraDistance(20);
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "规划_演进";
        }

        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();


        }
    }
}
