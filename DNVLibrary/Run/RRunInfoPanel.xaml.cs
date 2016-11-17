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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DNVLibrary.Run
{
    /// <summary>
    /// RRunInfoPanel.xaml 的交互逻辑
    /// </summary>
    public partial class RRunInfoPanel : UserControl
    {
        public RRunInfoPanel()
        {
            InitializeComponent();
         
        }

        


        ///<summary>click header事件</summary>
        public event EventHandler OnClickHeader;
        protected virtual void RaiseClickHeaderEvent()
        {
            if (OnClickHeader != null)
                OnClickHeader(this, null);
        }

        public enum EStatus { 普通, 最大化 }

        
        private EStatus _status;
        public EStatus status
        {
            get { return _status; }
            set 
            {
                _status = value;
                double allheight = 0;
                foreach (RRunInfoPanel item in (this.Parent as StackPanel).Children)
                    allheight += item.normalHeight;

                foreach (RRunInfoPanel item in (this.Parent as StackPanel).Children)
                {
                    if (this.Equals(item))
                        item.aniTo((this.Parent as StackPanel).ActualHeight - (allheight - item.normalHeight));
                    else
                        item.aniTo(item.normalHeight);
                }
            }
        }

        
        private double _normalHeight=60;
        ///<summary>注释</summary>
        public double normalHeight
        {
            get { return _normalHeight; }
            set { _normalHeight = value; }
        }

        public string header
        {
            get { return head.Text; }
            set { head.Text = value; }
        }

        public UIElementCollection content
        {
            get { return grdContent.Children; }
        }

        DoubleAnimation ani = new DoubleAnimation() { Duration = TimeSpan.FromSeconds(0.6) };

        internal void aniTo(double toheight)
        {
            ani.To = toheight;
            this.BeginAnimation(UserControl.HeightProperty, ani);
        }

        private void headpanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RaiseClickHeaderEvent();
        }

      


    }
}
