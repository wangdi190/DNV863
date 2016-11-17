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
using System.Windows.Shapes;

namespace MyBaseControls.ConfigTool
{
    /// <summary>
    /// winUserEdit.xaml 的交互逻辑
    /// </summary>
    public partial class winUserEdit : Window
    {
        public winUserEdit()
        {
            InitializeComponent();
        }

        ///<summary>刷新对象的委托</summary>
        public UCUserEdit.RefreshObject refreshObject { get { return ucedit.refreshObject; } set { ucedit.refreshObject = value; } }

    }
}
