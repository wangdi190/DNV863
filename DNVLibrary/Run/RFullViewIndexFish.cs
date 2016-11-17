using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Run
{
  

    class RFullViewIndexFish : AppBase
    {
        public RFullViewIndexFish(UCDNV863 Root)
            : base(Root)
        {
            //panel = new PRunFlowPanel(root);  //动画需求，不能放入load中
            //grdPanel.Children.Add(panel);
        }

        internal void show(bool isshow)
        {
            if (isshow)
            {
                root.grdMap.Opacity = 0.2;
                curcontrol.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                root.grdMap.Opacity = 1;
                curcontrol.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        FishBone.FishBoneControl curcontrol;
        internal override void load()
        {
            

            if (root.grdContent.Children.Contains(curcontrol))
                root.grdContent.Children.Remove(curcontrol);

            curcontrol = new FishBone.FishBoneControl(){Margin=new System.Windows.Thickness(10,60,10,10)};
            curcontrol.dataSource = DataLayer.DataProvider.getDataTableFromSQL("select cast(ID as nvarchar(100)) id, sort1,sort2,ord,indexname, definition,IMPORTANT,format,VALUE,UNIT,VALUENOTE,REFER1,REFER2,REFERTYPE,refernote from d_index where SORT0='863' and isPeriod=0 order by ord,IMPORTANT");
            root.grdContent.Children.Add(curcontrol);
        }



        internal override void unload()
        {
            if (panel != null) (panel as BaseIPanel).unload();

            root.grdMap.Opacity = 1;
            if (root.grdContent.Children.Contains(curcontrol))
                root.grdContent.Children.Remove(curcontrol);
      
            curcontrol=null;
        }


    }
}
