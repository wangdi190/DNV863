using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Planning
{
    class PChoiceCar:AppBase
    {
        public PChoiceCar(UCDNV863 Root)
            : base(Root)
        {
            panel = new PChoiceCarPanel(Root);
            grdPanel.Children.Add(panel);
        }

        internal override void load()
        {
            if (panel != null) (panel as BaseIPanel).load();
            root.earth.camera.adjustCameraAngle(0);
           // root.earth.camera.adjustCameraDistance(20);
           MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "通用提示";
        }

        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();


        }
    }
}
