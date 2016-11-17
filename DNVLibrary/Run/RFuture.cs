using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DNVLibrary.Run
{
    class RFuture : BaseMain
    {
        public RFuture(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {


            zButton btn;
            // 工具栏初始化
            cmbSelect.Items.Add("5分钟后");
            cmbSelect.Items.Add("30分钟后");
            cmbSelect.Items.Add("60分钟后");

            toolboxSub.Children.Add(cmbSelect);
            cmbSelect.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(cmbSelect_SelectionChanged);


            btn = new zButton() { text = "预测 ",Foreground=new SolidColorBrush(Colors.Blue), isNormalButton=true,group = "0", Margin=new System.Windows.Thickness(3,0,30,0), IsEnabled=false};
            btn.Click += new System.Windows.RoutedEventHandler(btnCal_Click);
            toolboxSub.Children.Add(btn);
            //显示部分
            btn = new zButton() { text = "预测潮流", group = "1", IsEnabled = false };
            btn.Click += new System.Windows.RoutedEventHandler(btnFlow_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "负荷转移", group = "1", IsEnabled = false };
            btn.Click += new System.Windows.RoutedEventHandler(btnLoadTransfer_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "电压无功", group = "1", IsEnabled = false };
            btn.Click += new System.Windows.RoutedEventHandler(btnVLRP_Click);
            toolboxSub.Children.Add(btn);
        }

        void cmbSelect_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            
        }
        System.Windows.Controls.ComboBox cmbSelect = new System.Windows.Controls.ComboBox() { VerticalContentAlignment = System.Windows.VerticalAlignment.Center, Width=80 }; //规划年选择
        internal int PlanningYear { get { return (int)cmbSelect.SelectedValue; } }


     

        //计算
        void btnCal_Click(object sender, System.Windows.RoutedEventArgs e)
        {
          
        }

        AppBase app;
        //潮流
        void btnFlow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Run.RRunFlow)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Run.RRunFlow))  //不为本身，执行创建
                {
                    app = new Run.RRunFlow(root);
                    app.begin();
                }
            }
            (app as RRunFlow).ShowFlow(btn.isChecked);
        }
        //负荷转移
        void btnLoadTransfer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Run.RRunFlow)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Run.RRunFlow))  //不为本身，执行创建
                {
                    app = new Run.RRunFlow(root);
                    app.begin();
                }
            }
            (app as RRunFlow).ShowLoad(btn.isChecked);
        }
        //电压无功
        void btnVLRP_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Run.RRunFlow)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Run.RRunFlow))  //不为本身，执行创建
                {
                    app = new Run.RRunFlow(root);
                    app.begin();
                }
            }
            (app as RRunFlow).ShowVL(btn.isChecked);
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
