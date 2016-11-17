using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Interact
{
   
    class IInvert : BaseMain
    {
        public IInvert(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {

        }


        protected override void load()  //进入时装载数据
        {
            if (IInvertController.uctop == null)
            {
                IInvertController.uctop = new Top();
                IInvertController.uctop.cmbTree.SelectItemChanged += new EventHandler(cmbTree_SelectItemChanged);
            }
            toolboxSub.Children.Add(IInvertController.uctop);

            root.distnet.scene.camera.adjustCameraAngle(0);

            if (IInvertController.ucpanel == null)
            {
                IInvertController.ucpanel = new IInvertPanel(root);
                IInvertController.ucpanel.changeProject();
            }

            root.grdContent.Children.Add(IInvertController.ucpanel);

            DNVLibrary.Run.DataGenerator.initRunData(root.distnet);
            DNVLibrary.Run.DataGenerator.StartGenData(root.distnet);

            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "互动_指标反演";

        }
  ///<summary>改变方案</summary>
        void cmbTree_SelectItemChanged(object sender, EventArgs e)
        {
            IInvertController.ucpanel.changeProject();
        }

      
     
        protected override void unload()  //退出时卸载数据
        {

            if (root.grdContent.Children.Contains(IInvertController.ucpanel))
                root.grdContent.Children.Remove(IInvertController.ucpanel);
            toolboxSub.Children.Clear();

            root.distnet.clearFlow();
            root.distnet.clearLoadCol();
            root.distnet.clearVLContour();


            DNVLibrary.Run.DataGenerator.StopGenData();
        }
    }
}
