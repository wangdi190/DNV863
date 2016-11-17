using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;

namespace DNVLibrary.Planning
{
    class PRunFlow:AppBase
    {
        public PRunFlow(UCDNV863 Root)
            : base(Root)
        {
            panel = new PRunFlowPanel(root);  //动画需求，不能放入load中
            grdPanel.Children.Add(panel);
        }

        internal override void load()
        {

            if (panel != null) (panel as BaseIPanel).load();
            root.earth.camera.adjustCameraAngle(50);
            root.earth.camera.adjustCameraDistance(0.36f);
            root.earth.objManager.isUseXModel = false;

            //状态栏提示的程序域保存
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.push();
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "规划_模拟运行_潮流";
        }

        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();
            root.earth.objManager.isUseXModel = true;
            //状态栏提示恢复
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.pop();

        }

        ///<summary>显示或隐藏潮流</summary>
        internal void ShowFlow(bool isshow)
        {
            (panel as PRunFlowPanel).ShowFlow(isshow);
        }
        ///<summary>显示或隐藏节点负载</summary>
        internal void ShowLoad(bool isshow)
        {
            (panel as PRunFlowPanel).ShowLoad(isshow);
        }
        ///<summary>显示或隐藏电压等值图</summary>
        internal void ShowVL(bool isshow)
        {
            (panel as PRunFlowPanel).ShowVL(isshow);
        }




      

    }
}
