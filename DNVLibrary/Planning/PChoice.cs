using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DNVLibrary.Planning
{
    class PChoice : BaseMain
    {
        public PChoice(UCDNV863 parent, string AppName, Brush icon)
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
            btn = new zButton() { text = "变电站选址定容", IsEnabled=false,  };
            btn.Click += new System.Windows.RoutedEventHandler(btnTransform_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "充电桩选址定容"};
            btn.Click += new System.Windows.RoutedEventHandler(btnCharge_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "储能装置选址定容", IsEnabled=false};
            btn.Click += new System.Windows.RoutedEventHandler(btnStore_Click);
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
        //变电站
        void btnTransform_Click(object sender, System.Windows.RoutedEventArgs e)
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
        //充电桩
        void btnCharge_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Planning.PChoiceCar)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Planning.PChoiceCar))  //不为本身，执行创建
                {
                    app = new Planning.PChoiceCar(root);
                    app.begin();
                }
            }
        }
        //储能
        void btnStore_Click(object sender, System.Windows.RoutedEventArgs e)
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
