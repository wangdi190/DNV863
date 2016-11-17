using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DNVLibrary.Run
{
    class RHistory : BaseMain
    {
        public RHistory(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {

            toolboxSub.Children.Add(new TextBlock() { Text = "时间段：" ,Foreground=Brushes.Cyan, VerticalAlignment= System.Windows.VerticalAlignment.Bottom, Margin=new System.Windows.Thickness(20,0,0,0), FontSize=16});
            dateStart = new DevExpress.Xpf.Editors.DateEdit() { DateTime = DateTime.Now.AddDays(-1), Height = 24, VerticalAlignment = System.Windows.VerticalAlignment.Bottom };
            toolboxSub.Children.Add(dateStart);
            toolboxSub.Children.Add(new TextBlock() { Text = "――――", Foreground = Brushes.Cyan, VerticalAlignment = System.Windows.VerticalAlignment.Bottom, Margin=new System.Windows.Thickness(4,0,4,0) });
            dateEnd = new DevExpress.Xpf.Editors.DateEdit() { DateTime = DateTime.Now, Height=24, Margin=new System.Windows.Thickness(0,0,60,0), VerticalAlignment= System.Windows.VerticalAlignment.Bottom };
            toolboxSub.Children.Add(dateEnd);

            dateStart.EditValueChanged += new DevExpress.Xpf.Editors.EditValueChangedEventHandler(dateStart_EditValueChanged);
            dateEnd.EditValueChanged += new DevExpress.Xpf.Editors.EditValueChangedEventHandler(dateEnd_EditValueChanged);

            // 工具栏初始化
            zButton btn;
            btn = new zButton() { text = "运行重演", group = "1" };
            btn.Click += new System.Windows.RoutedEventHandler(btnReplay_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "统计分析", group = "2", IsEnabled = false };
            btn.Click += new System.Windows.RoutedEventHandler(btnStatistics_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "指标评价", group = "3" };
            btn.Click += new System.Windows.RoutedEventHandler(btnIndexEvaluate_Click);
            toolboxSub.Children.Add(btn);
        }
        DevExpress.Xpf.Editors.DateEdit dateStart;
        DevExpress.Xpf.Editors.DateEdit dateEnd;
        void dateStart_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            if (app is Run.RHistoryReplay)
                (app as RHistoryReplay).curcontrol.startDate = dateStart.DateTime.Date;
        }
        void dateEnd_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            if (app is Run.RHistoryReplay)
                (app as RHistoryReplay).curcontrol.endDate = dateEnd.DateTime.Date.AddDays(1);
            
        }



        AppBase app;
        //运行重演
        void btnReplay_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Run.RHistoryReplay)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Run.RHistoryReplay))  //不为本身，执行创建
                {
                    app = new Run.RHistoryReplay(root);
                    app.begin();
                }
            }
            (app as RHistoryReplay).curcontrol.startDate = dateStart.DateTime.Date;
            (app as RHistoryReplay).curcontrol.endDate = dateEnd.DateTime.Date.AddDays(1);
            (app as RHistoryReplay).show(btn.isChecked);
        }
        //统计分析
        void btnStatistics_Click(object sender, System.Windows.RoutedEventArgs e)
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
        //指标评价
        void btnIndexEvaluate_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Run.RHistoryIndexFish)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Run.RHistoryIndexFish))  //不为本身，执行创建
                {
                    app = new Run.RHistoryIndexFish(root);
                    app.begin();
                }
            }
            (app as RHistoryIndexFish).show(btn.isChecked);
        }






        protected override void load()  //进入时装载数据
        {
            root.earth.camera.adjustCameraAngle(50);

            DataGenerator.initRunData(root.distnet);
            DataGenerator.StartGenData(root.distnet);

        }
        protected override void unload()  //退出时卸载数据
        {
            if (app != null) app.end();

            dateStart.EditValueChanged -= new DevExpress.Xpf.Editors.EditValueChangedEventHandler(dateStart_EditValueChanged);
            dateEnd.EditValueChanged -= new DevExpress.Xpf.Editors.EditValueChangedEventHandler(dateEnd_EditValueChanged);

            DataGenerator.StopGenData();
        }

    }
}
