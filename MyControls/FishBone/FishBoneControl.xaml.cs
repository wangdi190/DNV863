using System;
using System.Data;
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

namespace FishBone
{
    /// <summary>
    /// FishBoneControl.xaml 的交互逻辑
    /// </summary>
    public partial class FishBoneControl : UserControl
    {
        public FishBoneControl()
        {
            InitializeComponent();
        }


        bool isInit;
        Fish fish;

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            fish = new Fish(this);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isInit)
            {
                isInit = true;
               
                fish.para.fishLen = this.ActualWidth;
                fish.para.fishHeight = this.ActualHeight;

                fish.cal();
                fish.draw();
                grdMain.Children.Add(fish.canv);
                
            }
        }

        ///<summary>强制重绘，变更数据源后应调用, 调用条件，本控件的IsLoaded为真</summary>
        public void reDraw()
        {
            if (this.IsLoaded)
            {
                fish.para.fishLen = this.ActualWidth;
                fish.para.fishHeight = this.ActualHeight;

                fish.cal();
                fish.draw();
            }
        }
        
        private DataTable _dataSource;
        public DataTable dataSource
        {
            get { return _dataSource; }
            set
            {
                _dataSource = value; 

                string oldsort2,sort2;
                oldsort2 = null;

                fish.bone.bones.Clear();
                SubBone bone = null;
                
                foreach (DataRow dr in value.Rows)
                {
                    sort2 = dr.Field<string>("sort2");
                    if (sort2 != oldsort2)  //创建新枝
                    {
                        bone= fish.bone.addBone(dr.Field<string>("id"), dr.Field<string>("indexname"), dr.Field<string>("sort1"), dr.Field<string>("sort2"));

                        //if (value.AsEnumerable().Where(p => p.Field<string>("sort2") == sort2).Count() <= 1) //只有一个子代，直接为叶
                        //{
                        //    bone.isLeaf = true;
                        //}
                        //else  //加为第一子叶
                        {
                            addleaf(bone, dr);
                        }
                        oldsort2 = sort2;
                    }
                    else  //添加其它叶
                    {
                        addleaf(bone, dr);
                    }

                }




            }
        }
      
        void addleaf(SubBone bone, DataRow dr)
        {
            //select id,sort1,sort2,ord,indexname, definition,IMPORTANT,format,VALUE,UNIT,VALUENOTE,REFER1,REFER2,REFERTYPE,refernote from d_index where SORT0='863' order by sort2ord,IMPORTANT

            SubBone subbone;
            subbone = bone.addBone(dr.Field<string>("id"), dr.Field<string>("indexname"), dr.Field<string>("sort1"), dr.Field<string>("sort2"));

            double.TryParse(dr["value"].ToString(), out subbone.value);
            double.TryParse(dr["refer1"].ToString(), out subbone.ref1);
            double.TryParse(dr["refer2"].ToString(), out subbone.ref2);
            subbone.format = dr["format"].ToString();
            subbone.unit = dr["unit"].ToString();
            subbone.reftype = dr.Field<string>("refertype");
            subbone.definition = dr.Field<string>("definition");
            subbone.refnote = dr.Field<string>("refernote");

        }

     

    }
}
