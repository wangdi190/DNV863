using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Planning
{
    class PAnalyseCompare : AppBase
    {
        public PAnalyseCompare(UCDNV863 Root)
            : base(Root)
        {
            panel = new PAnalyseComparePanel(Root);
            grdPanel.Children.Add(panel);
        }

        internal override void load()
        {
            if (panel != null) (panel as BaseIPanel).load();
            root.earth.camera.adjustCameraAngle(0);
            root.earth.camera.adjustCameraDistance(0.38f);
            root.earth.colorManager.isEnabled = false;  //关闭色彩管理
            MyBaseControls.StatusBarTool.StatusBarTool.tipsInfo.curDomain = "规划_方案对比";
        }

        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();
            root.earth.colorManager.isEnabled = true;  //打开色彩管理
            root.earth.refreshColor();//恢复色彩
        }
    }


    public enum EItemType { 经济性, 规模, 供电质量, 主动性 }
    public enum EMaxMin { 中, 最大, 最小 }
    class CompareData
    {
        ///<summary>maxidx和minidx: 最大最小值的方案序号，-1表示不设置最大值图标</summary>
        public CompareData(EItemType itemType, string Title, string value1, string value2, string value3, int maxidx=-1, int minidx=-1)
        {
            iconvisibles = new List<System.Windows.Visibility>() { System.Windows.Visibility.Collapsed, System.Windows.Visibility.Collapsed, System.Windows.Visibility.Collapsed };
            iconbrushes = new List<System.Windows.Media.Brush>() { null, null, null };

            itemtype = itemType;
            title = Title;
            values = new List<string>();
            values.Add(value1);
            values.Add(value2);
            values.Add(value3);

            setmaxmin(maxidx, minidx);
        }

        ///<summary>对比项标题</summary>
        public string title { get; set; }

        public List<string> values { get; set; }
        public List<System.Windows.Visibility> iconvisibles { get; set; }
        public List<System.Windows.Media.Brush> iconbrushes { get; set; }


        private EItemType _itemtype;
        public EItemType itemtype
        {
            get { return _itemtype; }
            set
            {
                _itemtype = value;
                switch (_itemtype)
                {
                    case EItemType.经济性:
                        backgroundbrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x5A, 0xFF, 0xE5, 0x85));
                        break;
                    case EItemType.规模:
                        backgroundbrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x7E, 0x99, 0x99, 0xFF));
                        break;
                    case EItemType.供电质量:
                        backgroundbrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x7F, 0x87, 0xCE, 0xFA));
                        break;
                    case EItemType.主动性:
                        backgroundbrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x5A, 0x60, 0xFF, 0x60));
                        break;
                    default:
                        backgroundbrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                        break;
                }
            }
        }
        public System.Windows.Media.Brush backgroundbrush { get; set; }


        ///<summary>指定最大最小序号，以0开始</summary>
        void setmaxmin(int maxidx, int minidx)
        {
            

            for (int i = 0; i < iconvisibles.Count; i++)
                iconvisibles[i] = System.Windows.Visibility.Collapsed;

            if (maxidx > -1)
            {
                iconvisibles[maxidx] = System.Windows.Visibility.Visible;
                iconbrushes[maxidx] = (System.Windows.Media.ImageBrush)System.Windows.Application.Current.FindResource("imgup");
            }
            if (minidx > -1)
            {
                iconvisibles[minidx] = System.Windows.Visibility.Visible;
                iconbrushes[minidx] = (System.Windows.Media.ImageBrush)System.Windows.Application.Current.FindResource("imgdown");
            }
        }

        

    }

}
