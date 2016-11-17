using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DNVLibrary.Manage
{
    class MLog : BaseMain
    {
        public MLog(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {

        }


        protected override void load()  //进入时装载数据
        {

            root.grdContent.Children.Add(new DataLayer.WCF.UCLog());

            root.grdMap.Visibility = System.Windows.Visibility.Collapsed;

            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "管理_日志";

        }
        protected override void unload()  //退出时卸载数据
        {
            root.grdContent.Children.Clear();

            root.grdMap.Visibility = System.Windows.Visibility.Visible;
        }
    }

}
