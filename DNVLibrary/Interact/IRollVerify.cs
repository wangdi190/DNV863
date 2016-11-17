using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfEarthLibrary;

namespace DNVLibrary.Interact
{
   
    class IRollVerify : BaseMain
    {
        public IRollVerify(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {

        }


        protected override void load()  //进入时装载数据
        {

            root.distnet.scene.camera.adjustCameraAngle(0);

            if (IRollVerifyController.ucpanel == null)
                IRollVerifyController.ucpanel = new IRollVerifyPanel(root);
            root.grdContent.Children.Add(IRollVerifyController.ucpanel);

            //保存视觉
            //IEnumerable<PowerBasicObject> allobjs = root.distnet.getAllObjList();
            //foreach (var item in allobjs)
            //    root.distnet.scene.objManager.saveVisionProperty(item);

            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "互动_滚动校验";

        }
        protected override void unload()  //退出时卸载数据
        {

            if (root.grdContent.Children.Contains(IRollVerifyController.ucpanel))
                root.grdContent.Children.Remove(IRollVerifyController.ucpanel);

            root.distnet.clearFlow();
            root.distnet.clearLoadCol();
            root.distnet.clearVLContour();

            //恢复视觉
            //root.distnet.scene.objManager.restoreVisionProperty();


        }
    }
}
