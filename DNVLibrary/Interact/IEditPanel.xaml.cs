using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DNVLibrary.Interact
{
    /// <summary>
    /// iEditPanel.xaml 的交互逻辑
    /// </summary>
    public partial class IEditPanel : UserControl
    {
        public IEditPanel(UCDNV863 Root)
        {
            root = Root;
            InitializeComponent();
        }

        UCDNV863 root;
        Random rd = new Random();

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            initSceneData();
        }

        SceneDatas scenedata = new SceneDatas();
        ///<summary>初始化场景数据</summary>
        void initSceneData()
        {
            //模拟数据
            for (int i = 0; i < 40; i++)
            {
                SceneDataItem tmp = new SceneDataItem(scenedata);
                tmp.num = i + 1;
                tmp.idx1.value = rd.NextDouble();
                tmp.idx2.value = 0.9 + 0.23 * rd.NextDouble();
                tmp.idx3.value = rd.NextDouble() * 3300;
                tmp.idx4.value = rd.NextDouble();
                tmp.idx5.value = rd.NextDouble();
                tmp.idx6.value = rd.NextDouble();
                scenedata.items.Add(tmp);
            }
            scenedata.init();
            foreach (var item in scenedata.items)
            {
                scenepanel.Children.Add(item.button);
                item.button.MouseDown += new MouseButtonEventHandler(button_MouseDown);
            }

        }
        int curSceneNum = 0;
        void button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SceneButton btn = sender as SceneButton;
            if (curSceneNum > 0)
            {
                scenedata.items[curSceneNum - 1].button.Background = Brushes.Black;
                scenedata.items[curSceneNum - 1].button.txtNum.Foreground = new SolidColorBrush(Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF));
            }

            btn.Background = Brushes.AliceBlue;
            btn.txtNum.Foreground = new SolidColorBrush(Color.FromArgb(0x7F, 0x99, 0x32, 0xCC));

            curSceneNum = btn.data.num;

            refreshData();

        }

        ///<summary>读取数据并刷新界面</summary>
        void refreshData()
        {

            //zh注
            root.distnet.showFlow();
            root.distnet.showLoadCol();
            root.distnet.showVLContour();

        }

        bool isEditing = true;
        private void btnChangeSceneEdit_Click(object sender, RoutedEventArgs e)
        {
            isEditing = !isEditing;
            if (isEditing)  //进入编辑状态
            {
                gaugePanel.Visibility = System.Windows.Visibility.Collapsed;
                root.distnet.clearFlow();
                root.distnet.clearLoadCol();
                root.distnet.clearVLContour();
                root.distnet.scene.UpdateModel(); 
                root.distnet.isEditMode = true;
                btnChangeSceneEdit.Content = "计算典型场景";
            }
            else  //进入场景查看状态
            {
                gaugePanel.Visibility = System.Windows.Visibility.Visible;
                root.distnet.isEditMode = false;
                btnChangeSceneEdit.Content = "返回网架编辑";
            }
        }
    }
}
