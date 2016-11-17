using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DNVLibrary.Planning
{
    class PTimes : BaseMain
    {
        public PTimes(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {

            object container = VisualTreeHelper.GetParent(_Global.InstanceSelector);
            if (container != null)
                (container as System.Windows.Controls.StackPanel).Children.Remove(_Global.InstanceSelector);
            toolboxSub.Children.Add(_Global.InstanceSelector);

            //// 工具栏初始化
            //for (int i = 2015; i < 2026; i++)
            //    cmbYear.Items.Add(i);
            //cmbYear.SelectedValue = 2020;
            //toolboxSub.Children.Add(cmbYear);
            //cmbYear.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(cmbYear_SelectionChanged);

            zButton btn;
            btn = new zButton() { text = "配网规划演进",group="1" };
            btn.Click += new System.Windows.RoutedEventHandler(btnEvolve_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "配网历史变迁", group = "2", IsEnabled=false };
            btn.Click += new System.Windows.RoutedEventHandler(btnHistory_Click);
            toolboxSub.Children.Add(btn);
            //btn = new zButton() { text = "用电负荷变化", group = "3", IsEnabled=false };
            //btn.Click += new System.Windows.RoutedEventHandler(btnLoadChange_Click);
            //toolboxSub.Children.Add(btn);
        }

        //void cmbYear_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //{
        //    if (app != null)
        //        UCDNV863.CmdPlanningDateChanged.Execute(this, app.panel);
        //}
        //System.Windows.Controls.ComboBox cmbYear = new System.Windows.Controls.ComboBox() { VerticalContentAlignment = System.Windows.VerticalAlignment.Center }; //规划年选择
        //internal int PlanningYear { get { return (int)cmbYear.SelectedValue; } }


        AppBase app;
        //规划演进
        void btnEvolve_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PTimesEvolve)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PTimesEvolve))  //不为本身，执行创建
                {
                    app = new Planning.PTimesEvolve(root);
                    app.begin();
                }
            }
        }
        //历史变迁
        void btnHistory_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunFlow)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunFlow))  //不为本身，执行创建
                {
                    //app = new Planning.PFlow(root);
                    //app.begin();
                }
            }
        }
        //负荷变化
        void btnLoadChange_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PRunFlow)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PRunFlow))  //不为本身，执行创建
                {
                    //app = new Planning.PFlow(root);
                    //app.begin();
                }
            }
        }
     





        protected override void load()  //进入时装载数据
        {

        }
        protected override void unload()  //退出时卸载数据
        {
            if (app != null) app.end();

        }

    }
}


