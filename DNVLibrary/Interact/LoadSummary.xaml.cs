using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LoadInput.MyControl;

namespace LoadInput
{
    /// <summary>
    /// LoadSummary.xaml 的交互逻辑
    /// </summary>
    public partial class LoadSummary : UserControl
    {
        public LoadSummary()
        {
            InitializeComponent();
        }

        LoadSummaryData data = new LoadSummaryData();
        ElecSpecies curSpecies;
        private void grdMain_Loaded(object sender, RoutedEventArgs e)
        {

            grdMain.DataContext = data;
            data.start();

            LoadChoose gy = new LoadChoose(this) { DataContext = data.gyInfo, species = ElecSpecies.工业, Margin=new Thickness(10) };
            LoadChoose sy = new LoadChoose(this) { DataContext = data.syInfo, species = ElecSpecies.商业, Margin = new Thickness(10) };
            LoadChoose jm = new LoadChoose(this) { DataContext = data.jmInfo, species = ElecSpecies.居民, Margin = new Thickness(10) };
            gy.MouseDoubleClick += LoadChoose_MouseDoubleClick;
            sy.MouseDoubleClick += LoadChoose_MouseDoubleClick;
            jm.MouseDoubleClick += LoadChoose_MouseDoubleClick;

            gy.MouseEnter += LoadChoose_MouseEnter;
            sy.MouseEnter += LoadChoose_MouseEnter;
            jm.MouseEnter += LoadChoose_MouseEnter;

            panel.Children.Add(gy);
            panel.Children.Add(sy);
            panel.Children.Add(jm);
        }

        private void LoadChoose_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ElecSpecies spec = (sender as LoadChoose).species;

            if (spec == ElecSpecies.工业)
            {

                data.gyInfo.isEnable = !data.gyInfo.isEnable;
                if (data.gyInfo.isEnable == false)
                {
                    data.syInfo.isEnable = !data.gyInfo.isEnable;
                    data.jmInfo.isEnable = !data.gyInfo.isEnable;
                }
            }
            else if (spec == ElecSpecies.商业)
            {
                data.syInfo.isEnable = !data.syInfo.isEnable;
                if (data.syInfo.isEnable == false)
                {
                    data.gyInfo.isEnable = !data.syInfo.isEnable;
                    data.jmInfo.isEnable = !data.syInfo.isEnable;
                }

            }
            else if (spec == ElecSpecies.居民)
            {
                data.jmInfo.isEnable = !data.jmInfo.isEnable;
                if (data.jmInfo.isEnable == false)
                {
                    data.syInfo.isEnable = !data.jmInfo.isEnable;
                    data.gyInfo.isEnable = !data.jmInfo.isEnable;
                }
            }
        }

        private void LoadChoose_MouseEnter(object sender, MouseEventArgs e)
        {
            curSpecies = (sender as LoadChoose).species;
        }

        public void Update(ElecSpecies _species, double value)
        {
            double gy = data.gyInfo.weight;
            double sy = data.syInfo.weight;
            double jm = data.jmInfo.weight;


            if (_species == ElecSpecies.工业 && _species == curSpecies)
            {
                if (data.syInfo.isEnable && data.jmInfo.isEnable)
                    sy = jm = (1 - gy) / 2;


                else if (!data.syInfo.isEnable)
                {
                    gy = (gy > 1 - sy) ? 1 - sy : gy;
                    jm = 1 - gy - sy;
                }

                else if (!data.jmInfo.isEnable)
                {
                    gy = (gy > 1 - jm) ? 1 - jm : gy;
                    sy = 1 - gy - jm;
                }
            }

            else if (_species == ElecSpecies.商业 && _species == curSpecies)
            {
                if (data.gyInfo.isEnable && data.jmInfo.isEnable)
                    gy = jm = (1 - sy) / 2;


                else if (!data.gyInfo.isEnable)
                {
                    sy = (sy > 1 - gy) ? 1 - gy : sy;
                    jm = 1 - sy - gy;
                }

                else if (!data.jmInfo.isEnable)
                {
                    sy = (sy > 1 - jm) ? 1 - jm : sy;
                    gy = 1 - sy - jm;
                }
            }

            else if (_species == ElecSpecies.居民 && _species == curSpecies)
            {
                if (data.gyInfo.isEnable && data.syInfo.isEnable)
                    gy = sy = (1 - jm) / 2;


                else if (!data.gyInfo.isEnable)
                {
                    jm = (jm > 1 - gy) ? 1 - gy : jm;
                    sy = 1 - jm - gy;
                }

                else if (!data.syInfo.isEnable)
                {
                    jm = (jm > 1 - sy) ? 1 - sy : jm;
                    gy = 1 - jm - sy;
                }
            }

            data.gyInfo.weight = gy;
            data.syInfo.weight = sy;
            data.jmInfo.weight = jm;
        }
    }
}
