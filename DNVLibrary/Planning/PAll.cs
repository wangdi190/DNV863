using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Text;

namespace DNVLibrary.Planning
{
    class PAll : BaseMain
    {
        public PAll(UCDNV863 parent, string AppName, Brush icon)
            : base(parent, AppName, icon)
        {


            //// 工具栏初始化
            //for (int i = 2015; i < 2026; i++)
            //    cmbYear.Items.Add(i);
            //cmbYear.SelectedValue = 2020;
            //toolboxSub.Children.Add(cmbYear);
            //cmbYear.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(cmbYear_SelectionChanged);

            //zButton btn;
            //btn = new zButton() { text = "配网自动化程度" };
            //btn.Click += new System.Windows.RoutedEventHandler(btnAuto_Click);
            //toolboxSub.Children.Add(btn);
            //btn = new zButton() { text = "可靠性分析" };
            //btn.Click += new System.Windows.RoutedEventHandler(btnReliability_Click);
            //toolboxSub.Children.Add(btn);
            //btn = new zButton() { text = "N-1分析" };
            //btn.Click += new System.Windows.RoutedEventHandler(btnN1_Click);
            //toolboxSub.Children.Add(btn);
            //btn = new zButton() { text = "主动性分析" };
            //btn.Click += new System.Windows.RoutedEventHandler(btnInitiative_Click);
            //toolboxSub.Children.Add(btn);
            //btn = new zButton() { text = "方案对比" };
            //btn.Click += new System.Windows.RoutedEventHandler(btnCompare_Click);
            //toolboxSub.Children.Add(btn);

        }








        protected override void load()  //进入时装载数据
        {
            //Run.DataGenerator.initRunData(root.distnet);
            

            //对象数：1755/2118；相机方向地心夹角：59.912；相机离地高度：0.444；相机位置：{X:13.71358 Y:13.53778 Z:0.2224692}；经度近端：116.50000-116.53560，远端：116.41080-116.62480；纬度：39.74936-39.85075；屏幕中心：39.7638139215075,116.517803631564；瓦片数：131；  15-26986-12432
            root.earth.camera.aniLookGeo(116.517803631564, 39.7638139215075, 0.48,0);
            root.earth.camera.adjustCameraAngle(55);

            root.grdContent.Children.Add(new PAllPanel(root.distnet));
            root.earth.legendManager.legends["设备图元图例"].isShow = false;

            root.earth.config.tooltipMoveEnable = true;
        }
        protected override void unload()  //退出时卸载数据
        {
            root.grdContent.Children.Clear();

        }

    }
}
