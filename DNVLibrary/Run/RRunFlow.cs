using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;

namespace DNVLibrary.Run
{
    class RRunFlow:AppBase
    {
        public RRunFlow(UCDNV863 Root)
            : base(Root)
        {
            panel = new RRunFlowPanel(root);  //动画需求，不能放入load中
            grdPanel.Children.Add(panel);
        }


        internal override void load()
        {
            if (panel != null) (panel as BaseIPanel).load();
            root.earth.camera.adjustCameraAngle(50);
            root.earth.camera.aniLook(116.518061542732, 39.7599258160558, 1.335, 0);
            root.earth.camera.adjustCameraDistance(0.4f);
        }

        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();

        }

        ///<summary>显示或隐藏潮流</summary>
        internal void ShowFlow(bool isshow)
        {
            (panel as RRunFlowPanel).isShowFlow = isshow;
            (panel as RRunFlowPanel).refresh();
        }
        ///<summary>显示或隐藏节点负载</summary>
        internal void ShowLoad(bool isshow)
        {
            (panel as RRunFlowPanel).isShowLoadColumn = isshow;
            (panel as RRunFlowPanel).refresh();
        }
        ///<summary>显示或隐藏电压等值图</summary>
        internal void ShowVL(bool isshow)
        {
            (panel as RRunFlowPanel).isShowVLContour = isshow;
            (panel as RRunFlowPanel).refresh();
        }

        internal void ShowSectionColor(bool isshow)
        {
            (panel as RRunFlowPanel).isShowSectionColor = isshow;
            (panel as RRunFlowPanel).isShowLoadColor = !isshow;
            (panel as RRunFlowPanel).isShowVLColor = !isshow;
            (panel as RRunFlowPanel).refresh();
        }
        internal void ShowLoadColor(bool isshow)
        {
            (panel as RRunFlowPanel).isShowSectionColor = !isshow;
            (panel as RRunFlowPanel).isShowLoadColor = isshow;
            (panel as RRunFlowPanel).isShowVLColor = !isshow;
            (panel as RRunFlowPanel).refresh();
        }
        internal void ShowVLColor(bool isshow)
        {
            (panel as RRunFlowPanel).isShowSectionColor = !isshow;
            (panel as RRunFlowPanel).isShowLoadColor = !isshow;
            (panel as RRunFlowPanel).isShowVLColor = isshow;
            (panel as RRunFlowPanel).refresh();
        }
        internal void ShowCutArea(bool isshow)
        {
            (panel as RRunFlowPanel).isShowCutArea = isshow;
            (panel as RRunFlowPanel).refresh();
        }



    }
}
