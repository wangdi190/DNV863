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

namespace ToolsShell
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        UserControl uc;
        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            grdContent.Children.Clear();
            uc = null;
            uc = new MyBaseControls.ConfigTool.UCDeveloperEdit();
            grdContent.Children.Add(uc);
        }

        private void btnDBDesc_Click(object sender, RoutedEventArgs e)
        {
            grdContent.Children.Clear();
            uc = null;
            DistNetLibrary.Edit.WinDBDescTool win = new DistNetLibrary.Edit.WinDBDescTool(".\\xml\\DBDescYZ.xml"); win.ShowDialog();

        }
    }
}
