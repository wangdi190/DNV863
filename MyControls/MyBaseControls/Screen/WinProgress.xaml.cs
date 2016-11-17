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

namespace MyBaseControls.Screen
{
    /// <summary>
    /// WinProgress.xaml 的交互逻辑
    /// </summary>
    public partial class WinProgress : Window 
    {
        public WinProgress()
        {
            InitializeComponent();
            tmr.Tick += new EventHandler(tmr_Tick);
        }

        void tmr_Tick(object sender, EventArgs e)
        {
            if (!ScreenProgress.isActive)
            {
                tmr.Stop();
                this.Close();
            }
            Title = ScreenProgress.title;
            txt.Text = ScreenProgress.info;
            bar.Value = ScreenProgress.progress;
        }


        System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer(){Interval=TimeSpan.FromMilliseconds(100)};

      
        public void active()
        {
            tmr.Start();
        }

    }
}
