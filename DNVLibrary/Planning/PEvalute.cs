using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DNVLibrary.Planning
{
    class PEvalute : BaseMain
    {
        public PEvalute(UCDNV863 parent, string AppName, Brush icon)
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
            btn = new zButton() { text = "指标体系" };
            btn.Click += new System.Windows.RoutedEventHandler(btnIndex_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "指标鱼骨图" };
            btn.Click += new System.Windows.RoutedEventHandler(btnIndexFish_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "KPI关键指标" };
            btn.Click += new System.Windows.RoutedEventHandler(btnKPI_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "评价指数" };
            btn.Click += new System.Windows.RoutedEventHandler(btnIDX_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "财务评价指标" };
            btn.Click += new System.Windows.RoutedEventHandler(btnEconomy_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "敏感性分析" };
            btn.Click += new System.Windows.RoutedEventHandler(btnPrice_Click);
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
        //指标体系
        void btnIndex_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PEvaluteIndex)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PEvaluteIndex))  //不为本身，执行创建
                {
                    app = new Planning.PEvaluteIndex(root);
                    app.begin();
                }
            }
        }
        //指标鱼骨图
        void btnIndexFish_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PEvaluteIndexFish)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PEvaluteIndexFish))  //不为本身，执行创建
                {
                    app = new Planning.PEvaluteIndexFish(root);
                    app.begin();
                }
            }
        }
        //KPI
        void btnKPI_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PEvaluteKPI)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PEvaluteKPI))  //不为本身，执行创建
                {
                    app = new Planning.PEvaluteKPI(root);
                    app.begin();
                }
            }
        }
        //IDX指数评价
        void btnIDX_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PEvaluteIDX)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PEvaluteIDX))  //不为本身，执行创建
                {
                    app = new Planning.PEvaluteIDX(root);
                    app.begin();
                }
            }
        }
        //财务评价指标
        void btnEconomy_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PEvaluteEconomy)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PEvaluteEconomy))  //不为本身，执行创建
                {
                    app = new Planning.PEvaluteEconomy(root);
                    app.begin();
                }
            }
        }
        //敏感性分析
        void btnPrice_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PEvaluteSensitive)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PEvaluteSensitive))  //不为本身，执行创建
                {
                    app = new Planning.PEvaluteSensitive(root);
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


