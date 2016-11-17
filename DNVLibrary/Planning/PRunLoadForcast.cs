using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Planning
{
    class PRunLoadForcast:AppBase
    {
        public PRunLoadForcast(UCDNV863 Root)
            : base(Root)
        {
            panel = new PRunLoadForcastPanel(Root);
            grdPanel.Children.Add(panel);
        }

        internal override void load()
        {
            if (panel != null) (panel as BaseIPanel).load();
            root.earth.camera.adjustCameraAngle(0);
            //root.earth.camera.adjustCameraDistance(2.4f);
            //状态栏提示的程序域保存
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.push();
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "规划_模拟运行_区块负荷预测";

        }

        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();

            //状态栏提示恢复
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.pop();
        }
    }
}
