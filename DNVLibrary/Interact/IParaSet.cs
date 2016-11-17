using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Interact
{
    class IParaSet : BaseMain
    {
        public IParaSet(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {

            zButton btn;
            btn = new zButton() { text = "分类负荷特性" };
            btn.Click += new System.Windows.RoutedEventHandler(btnClassifcation_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "统计负荷特性" };
            btn.Click += new System.Windows.RoutedEventHandler(btnSummary_Click);
            toolboxSub.Children.Add(btn);
        }

        AppBase app;
        //分类负荷特性
        void btnClassifcation_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Interact.IParaSetClassification)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Interact.IParaSetClassification))  //不为本身，执行创建
                {
                    app = new Interact.IParaSetClassification(root);
                    app.begin();
                }
                else
                    (app as Interact.IParaSetClassification).show();
            }
            else
                if (app is Interact.IParaSetClassification)
                    (app as Interact.IParaSetClassification).hide();
        }
        //统计负荷特性
        void btnSummary_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Interact.IParaSetSummary)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Interact.IParaSetSummary))  //不为本身，执行创建
                {
                    app = new Interact.IParaSetSummary(root);
                    app.begin();
                }
                else
                    (app as Interact.IParaSetSummary).show();
            }
            else
                if (app is Interact.IParaSetSummary)
                    (app as Interact.IParaSetSummary).hide();
        }
       


        protected override void load()  //进入时装载数据
        {
       

            
        }
        protected override void unload()  //退出时卸载数据
        {



        }
    }
}
