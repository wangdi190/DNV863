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

namespace DNVLibrary.Planning
{
    /// <summary>
    /// PAllYearPorjects.xaml 的交互逻辑
    /// </summary>
    public partial class PAllYearPorjects : UserControl
    {
        public PAllYearPorjects(YearData Year)
        {
            year = Year;
            InitializeComponent();
            buildview();
        }
        YearData year;

        List<PAllPrj> prjviews = new List<PAllPrj>();

        void buildview()
        {
            Color[] colors = new Color[] { Colors.Red, Colors.Cyan, Colors.Yellow, Colors.Lime, Colors.Blue, Colors.Purple};

            int idx = 0;
            foreach (var item in year.projects)
            {
                item.color = colors[idx % colors.Count()];
                PAllPrj prj = new PAllPrj() { name = item.name, note = item.note, isSelected = idx == 0, idx=idx,color=item.color, Tag=item};
                if (idx == 0) selprj = prj;
                prj.projectChanged += new EventHandler(prj_projectChanged);
                prjviews.Add( prj);

                grdMain.Children.Add(prj);
                idx++;
            }

        }

        public Visibility colorVisibility
        {
            set
            {
                foreach (var item in prjviews)
                {
                    item.rect.Visibility = value;
                }

            }
        }


        public PAllPrj selprj { get; set; }

        void prj_projectChanged(object sender, EventArgs e)
        {
            selprj = sender as PAllPrj;
            int count = prjviews.Count;
            int move = selprj.idx;
            foreach (var item in prjviews)
            {
                item.isSelected = item == selprj;
                int tmp = item.idx - move;
                if (tmp < 0) tmp += count;
                item.idx = tmp;
            }
            RaiseprojectChangedEvent();
        }

        
        public event EventHandler projectChanged;
        protected virtual void RaiseprojectChangedEvent()
        {
            if (projectChanged != null)
                projectChanged(this, null);
        }


        //System.Windows.Media.Animation.DoubleAnimation ani = new System.Windows.Media.Animation.DoubleAnimation() { Duration=TimeSpan.FromSeconds(0.4), AutoReverse=true, FillBehavior= System.Windows.Media.Animation.FillBehavior.Stop};
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //ani.From = 0;
            //ani.To = 25;
            //eff.BeginAnimation(System.Windows.Media.Effects.DropShadowEffect.BlurRadiusProperty, ani);
        }
      

    }
}
