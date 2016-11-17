using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using WpfEarthLibrary;
using DistNetLibrary;

namespace DNVLibrary.Run
{
    class RRealTime : BaseMain
    {
        public RRealTime(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {


            // 工具栏初始化

            zButton btn;
            btn = new zButton() { text = "显示潮流", group = "1" };
            btn.Click += new System.Windows.RoutedEventHandler(btnFlow_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "变压器负载", group = "1" };
            btn.Click += new System.Windows.RoutedEventHandler(btnLoadColumn_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "电压等值图", group = "1" };
            btn.Click += new System.Windows.RoutedEventHandler(btnVLContour_Click);
            toolboxSub.Children.Add(btn);
            btn = new zButton() { text = "停电区域", group = "1", Margin = new System.Windows.Thickness(0, 0, 10, 0) };
            btn.Click += new System.Windows.RoutedEventHandler(btnCutArea_Click);
            toolboxSub.Children.Add(btn);

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
        //变压器负载
        void btnLoadColumn_Click(object sender, System.Windows.RoutedEventArgs e)
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
        //电压等值图
        void btnVLContour_Click(object sender, System.Windows.RoutedEventArgs e)
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


        //停电范围
        void btnCutArea_Click(object sender, System.Windows.RoutedEventArgs e)
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
            (app as RRunFlow).ShowCutArea(btn.isChecked);
        }





        protected override void load()  //进入时装载数据
        {
            root.distnet.scene.config.pickEnable = true;
            root.distnet.scene.Picked += new EventHandler<Earth.PickEventArgs>(scene_Picked);

            DataGenerator.initRunData(root.distnet);
            DataGenerator.StartGenData(root.distnet);

            root.distnet.scene.config.tooltipMoveEnable = true;
            root.earth.objManager.isUseXModel = false;

            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "运行_实时";
        }
        protected override void unload()  //退出时卸载数据
        {
            if (app != null) app.end();

            root.distnet.scene.config.pickEnable = false;
            root.distnet.scene.Picked -= new EventHandler<Earth.PickEventArgs>(scene_Picked);

            DataGenerator.StopGenData();

            root.distnet.scene.config.tooltipMoveEnable = false;
            root.earth.objManager.isUseXModel = true;

        }



        #region 点击对象相关

        //拾取到对象，处理特别显示，包括供电范围与电源追溯
        void scene_Picked(object sender, Earth.PickEventArgs e)
        {
            root.distnet.clearTraceSupplyRange();
            if (e.pickedObject == null) return;
            if (e.pickedObject is DNDistTransformer || e.pickedObject is DNSwitchHouse)  //电源追溯
            {
                root.distnet.showSourceTrace(e.pickedObject);
                //List<TopoFindResult> items = root.distnet.findByMaxLength(e.pickedObject.id, new List<EObjectType>() { EObjectType.两卷主变, EObjectType.三卷主变 }, 200); //测试按长度搜
                ////List<TopoFindResult> items = root.distnet.findByMinSurplusLoad(e.pickedObject.id, new List<EObjectType>() { EObjectType.两卷主变, EObjectType.三卷主变 }, -10);//测度按负荷余量搜
                
                ////查看测试结果
                //int idx = 0;
                //foreach (var itm in items)
                //{
                //    idx++;
                //    Color c=Colors.White;
                //    if (idx == 1)
                //        c = Colors.OrangeRed;
                //    else if (idx == 2)
                //        c = Colors.Purple;
                //    else if (idx == 3)
                //        c = Colors.Orchid;
                //    else if (idx == 4)
                //        c = Colors.GreenYellow;

                //    foreach (string id in itm.path)
                //    {
                //        PowerBasicObject item = root.distnet.findObj(id);
                //        if (item is pSymbolObject)
                //        {
                //            (item as pSymbolObject).color = c;
                //            (item as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.闪烁);
                //        }
                //        else if (item is pPowerLine)
                //        {
                //            (item as pPowerLine).lineColor = c;
                //            (item as pPowerLine).AnimationBegin(pPowerLine.EAnimationType.闪烁);
                //        }
                //    }
                //}


            }
            else if (e.pickedObject is DNMainTransformer || e.pickedObject is DNSubStation) //供电范围
            {
                root.distnet.showSupplyRange(e.pickedObject);
            }

        }




        #endregion
    }



}
