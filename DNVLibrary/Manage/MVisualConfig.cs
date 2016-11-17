using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DNVLibrary.Manage
{
    class MVisualConfig : BaseMain
    {
        public MVisualConfig(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {

        }


        protected override void load()  //进入时装载数据
        {

            MyBaseControls.ConfigTool.UCUserEdit ui = new MyBaseControls.ConfigTool.UCUserEdit();
            root.grdContent.Children.Add(ui);
            ui.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            ui.Padding = new System.Windows.Thickness(0, 200, 0, 200);
            ui.refreshObject = refreshconfigobject;  //重要，将刷新方法，委托给配置窗体，配置窗体才能调用刷新方法
           
            root.grdMap.Visibility = System.Windows.Visibility.Collapsed;

        }
        void refreshconfigobject(string cfgkey)
        {
            Dictionary<string, object> objdict = root.distnet.getAllObjDictAsObject();
            MyBaseControls.ConfigTool.Config.refreshObjects(objdict, cfgkey);
        }


        protected override void unload()  //退出时卸载数据
        {
            root.grdContent.Children.Clear();

            root.grdMap.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
