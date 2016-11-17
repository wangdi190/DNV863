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

namespace IDXKPI
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UCIDXKPI uc = new UCIDXKPI();
            uc.isPerDiv100 = true;
            uc.idxDataSource = DataLayer.DataProvider.getDataTableFromSQL("select cast(ID as nvarchar(100)) id,sort, sort1,sort2,ord,indexname, definition,IMPORTANT,format,VALUE,UNIT,VALUENOTE,REFER1,REFER2,REFERTYPE,refernote,necessary from d_index where SORT0='863' order by ord,IMPORTANT");
            grdMain.Children.Add(uc);
        }
    }
}
