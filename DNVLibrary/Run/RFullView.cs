using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DNVLibrary.Run
{
    class RFullView : BaseMain
    {
        public RFullView(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {


            // 工具栏初始化
            zButton btn;
            btn = new zButton() { text = "网格区域", group = "0" };
            btn.Click += new System.Windows.RoutedEventHandler(btnArea_Click);
            toolboxSub.Children.Add(btn);
            btnArea = btn;

            btn = new zButton() { text = "技术指标", group = "1" };
            btn.Click += new System.Windows.RoutedEventHandler(btnIndex_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "网架规模", group = "2" };
            btn.Click += new System.Windows.RoutedEventHandler(btnSize_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "生产统计", group = "3" };
            btn.Click += new System.Windows.RoutedEventHandler(btnStat_Click);
            toolboxSub.Children.Add(btn);
        }
        zButton btnArea;

        ///<summary>是否显示网格区域</summary>
        void btnArea_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Run.RFullViewGridArea)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Run.RFullViewGridArea))  //不为本身，执行创建
                {
                    app = new Run.RFullViewGridArea(root);
                    app.begin();
                }
            }
            (app as RFullViewGridArea).show(btn.isChecked);
            if (btn.isChecked)
                panel.Visibility = System.Windows.Visibility.Visible;
        }

        void btnIndex_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;

            if (btn.isChecked)
                panel.Visibility = System.Windows.Visibility.Hidden;
            else
                panel.Visibility = System.Windows.Visibility.Visible;

            if (btn.isChecked)
            {
                if (app != null && !(app is Run.RFullViewIndexFish)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Run.RFullViewIndexFish))  //不为本身，执行创建
                {
                    app = new Run.RFullViewIndexFish(root);
                    app.begin();
                }
            }
            (app as RFullViewIndexFish).show(btn.isChecked);
          

        }
        void btnSize_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Run.RFullViewSize)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Run.RFullViewSize))  //不为本身，执行创建
                {
                    app = new Run.RFullViewSize(root);
                    app.begin();
                }
            }
            (app as RFullViewSize).show(btn.isChecked);
            if (btn.isChecked)
                panel.Visibility = System.Windows.Visibility.Hidden;
            else
                panel.Visibility = System.Windows.Visibility.Visible;


        }
        void btnStat_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            zButton btn = sender as zButton;
            if (btn.isChecked)
            {
                if (app != null && !(app is Run.RFullViewProduce)) app.end();  //不为空，不为本身，执行结束操作

                if (!(app is Run.RFullViewProduce))  //不为本身，执行创建
                {
                    app = new Run.RFullViewProduce(root);
                    app.begin();
                }
            }
            (app as RFullViewProduce).show(btn.isChecked);
            if (btn.isChecked)
                panel.Visibility = System.Windows.Visibility.Hidden;
            else
                panel.Visibility = System.Windows.Visibility.Visible;

        }


        AppBase app;





        RFullViewStatPanel panel;
        protected override void load()  //进入时装载数据
        {
            root.earth.camera.adjustCameraAngle(50);
            root.earth.camera.aniLook(116.530393753392, 39.7493343184097, 1.335, 0);
            root.earth.camera.adjustCameraDistance(1.4f);

            //供电区域

            //网架：天然存在

            //实时潮流

            //统计数据
            RFullViewStatData.initDatas(root.distnet);
            panel = new RFullViewStatPanel();
            root.grdContent.Children.Add(panel);

            //静态鱼骨图


            root.ShowClock(true);

            

            DataGenerator.initRunData(root.distnet);
            DataGenerator.StartGenData(root.distnet);


            btnArea.isChecked = true;
            btnArea_Click(btnArea, null);

            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "运行_全景";
        }
        protected override void unload()  //退出时卸载数据
        {
            if (app != null) app.end();

            if (panel != null && root.grdContent.Children.Contains(panel))
                root.grdContent.Children.Remove(panel);

            root.ShowClock(false);

            //显示区域
            WpfEarthLibrary.pLayer arealayer = root.earth.objManager.zLayers[DistNetLibrary.EObjectCategory.区域类.ToString()];
            arealayer.logicVisibility = false;
            root.earth.UpdateModel();

            DataGenerator.StopGenData();
        }

    }
}
