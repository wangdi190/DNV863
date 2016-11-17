using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Interact
{

    class ITimes : BaseMain
    {
        public ITimes(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {
          

       
        }
        AppBase app;

        protected override void load()  //进入时装载数据
        {
            if (ITimesController.uctop == null) ITimesController.uctop = new ITimesTop();
            toolboxSub.Children.Add(ITimesController.uctop);

            if (app == null) app = new ITimesApp(root);
            app.begin();

            DNVLibrary.Run.DataGenerator.initRunData(root.distnet);
            DNVLibrary.Run.DataGenerator.StartGenData(root.distnet);

            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "互动_时序推演";
        }
        protected override void unload()  //退出时卸载数据
        {
            toolboxSub.Children.Clear();
            if (app != null) app.end();

            root.distnet.clearFlow();
            root.distnet.clearLoadCol();
            root.distnet.clearVLContour();

            DNVLibrary.Run.DataGenerator.StopGenData();

        }
    }
}
