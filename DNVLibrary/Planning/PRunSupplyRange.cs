using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Planning
{
    class PRunSupplyRange:AppBase
    {
        public PRunSupplyRange(UCDNV863 Root)
            : base(Root)
        {
            panel = new PRunSupplyRangePanel(Root);
            grdPanel.Children.Add(panel);
        }

        internal override void load()
        {
            if (panel != null) (panel as BaseIPanel).load();
            root.earth.camera.adjustCameraAngle(45);
            root.earth.camera.adjustCameraDistance(0.36f);
            //状态栏提示的程序域保存
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.push();
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "规划_模拟运行_供电范围";
        }

        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();
            //状态栏提示恢复
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.pop();

        }
    }
}
