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

namespace FishBone
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        FishBoneControl uc = new FishBoneControl();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            uc.Margin = new Thickness(0, 50, 0, 0);
            uc.dataSource = DataLayer.DataProvider.getDataTableFromSQL("select cast(ID as nvarchar(100)) id, sort1,sort2,ord,indexname, definition,IMPORTANT,format,VALUE,UNIT,VALUENOTE,REFER1,REFER2,REFERTYPE,refernote from d_index where SORT0='863' order by ord,IMPORTANT");
            grdMain.Children.Add(uc);
        }


    }
}
