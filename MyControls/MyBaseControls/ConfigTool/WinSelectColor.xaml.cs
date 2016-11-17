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
using DevExpress.Xpf.Editors;


namespace MyBaseControls.ConfigTool
{
    /// <summary>
    /// WinSelectColor.xaml 的交互逻辑
    /// </summary>
    public partial class WinSelectColor : Window
    {
        public WinSelectColor()
        {
            InitializeComponent();
            
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            colorEdit1.Palettes.Remove(colorEdit1.Palettes["Standard Colors"]);
            colorEdit1.Palettes.Remove(colorEdit1.Palettes["Theme Colors"]);
            colorEdit1.Palettes.Clear();
            colorEdit1.Palettes.Add(CustomPalette.CreateGradientPalette("标准色", PredefinedColorCollections.Standard));
            //colorEdit1.Palettes.Add(
            //    new CustomPalette("Custom RGB Colors",
            //    new List<Color>() {
            //        Color.FromRgb(150, 18, 30),
            //        Color.FromRgb(20, 40, 20),
            //        Color.FromRgb(88, 73, 29) }));
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            isCancel = true;
        }
        public bool isCancel { get; set; }
        
        private Color _color;
        public Color color
        {
            get { return _color; }
            set { _color = value; }
        }
      

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            isCancel = false;
            color = colorEdit1.Color;
            this.Hide();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            isCancel = true;
            this.Hide();
        }

     


    }
}
