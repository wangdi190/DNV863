using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DNVLibrary.Planning
{
    class PAnalyse : BaseMain
    {
        public PAnalyse(UCDNV863 parent, string AppName, Brush icon)
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
            btn = new zButton() { text = "配网自动化程度" };
            btn.Click += new System.Windows.RoutedEventHandler(btnAuto_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "可靠性分析"};
            btn.Click += new System.Windows.RoutedEventHandler(btnReliability_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "N-1分析" };
            btn.Click += new System.Windows.RoutedEventHandler(btnN1_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "主动性分析" };
            btn.Click += new System.Windows.RoutedEventHandler(btnInitiative_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "方案对比"};
            btn.Click += new System.Windows.RoutedEventHandler(btnCompare_Click);
            toolboxSub.Children.Add(btn);

        }

        //void cmbYear_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //{
        //    if (app != null)
        //        UCDNV863.CmdPlanningDateChanged.Execute(this, app.panel);
        //}
        //System.Windows.Controls.ComboBox cmbYear = new System.Windows.Controls.ComboBox() { VerticalContentAlignment = System.Windows.VerticalAlignment.Center }; //规划年选择
        //internal int PlanningYear { get { return (int)cmbYear.SelectedValue; } }


        AppBase app;
        //自动化
        void btnAuto_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PAnalyseAuto)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PAnalyseAuto))  //不为本身，执行创建
                {
                    app = new Planning.PAnalyseAuto(root);
                    app.begin();
                }
            }
        }
        //可靠性
        void btnReliability_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PAnalyseReliability)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PAnalyseReliability))  //不为本身，执行创建
                {
                    app = new Planning.PAnalyseReliability(root);
                    app.begin();
                }
            }
        }
        void btnN1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PAnalyseN1)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PAnalyseN1))  //不为本身，执行创建
                {
                    app = new Planning.PAnalyseN1(root);
                    app.begin();
                }
            }
        }
        //主动性分析
        void btnInitiative_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PAnalyseInitiative)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PAnalyseInitiative))  //不为本身，执行创建
                {
                    app = new Planning.PAnalyseInitiative(root);
                    app.begin();
                }
            }
        }
        //方案对比
        void btnCompare_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PAnalyseCompare)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PAnalyseCompare))  //不为本身，执行创建
                {
                    app = new Planning.PAnalyseCompare(root);
                    app.begin();
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


