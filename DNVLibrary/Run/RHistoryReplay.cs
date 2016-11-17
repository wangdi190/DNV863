using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Run
{
    class RHistoryReplay: AppBase
    {
        public RHistoryReplay(UCDNV863 Root)
            : base(Root)
        {
            
        }

        internal void show(bool isshow)
        {
            if (isshow)
            {
                curcontrol.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                curcontrol.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        internal RHistoryReplayPanel curcontrol;
        internal override void load()
        {


            if (root.grdContent.Children.Contains(curcontrol))
                root.grdContent.Children.Remove(curcontrol);

            curcontrol = new RHistoryReplayPanel(root);

            //zh注：取指标数据前，应先计算指标

            root.grdContent.Children.Add(curcontrol);

            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "运行_历史重演";
        }



        internal override void unload()
        {
            if (curcontrol != null) (curcontrol as BaseIPanel).unload();

            if (root.grdContent.Children.Contains(curcontrol))
                root.grdContent.Children.Remove(curcontrol);
            DataGenerator.StopGenData();
            curcontrol = null;
        }



    }
}
