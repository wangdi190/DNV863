using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Interact
{
    class ITimesApp : AppBase
    {
        public ITimesApp(UCDNV863 Root)
            : base(Root)
        {
            if (ITimesController.ucpanel == null) ITimesController.ucpanel = new ITimesPanel(root);
            root.grdContent.Children.Add(ITimesController.ucpanel);
        }

        internal override void load()
        {
            if (panel != null) (panel as BaseIPanel).load();
            root.earth.camera.adjustCameraAngle(0);
            root.earth.camera.adjustCameraDistance(0.38f);
            root.earth.colorManager.isEnabled = false;  //关闭色彩管理
        }

        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();
            root.earth.colorManager.isEnabled = true;  //打开色彩管理
            root.earth.refreshColor();//恢复色彩

            if (root.grdContent.Children.Contains(ITimesController.ucpanel))
                root.grdContent.Children.Remove(ITimesController.ucpanel);
        }
    }
}
