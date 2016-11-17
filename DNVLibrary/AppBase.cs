using System;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary
{
    abstract class AppBase
    {
        public AppBase(UCDNV863 Root)
        {
            root = Root;

            //初始化信息板容器
            grdPanel.RenderTransform = transformpanel;
            transformpanel.X = 300;
            root.grdInfo.Children.Add(grdPanel);

        }
        protected UCDNV863 root;

        UserControl _panel;
        internal UserControl panel
        {
            get { return _panel; }
            set
            {
                _panel = value;
                _panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                if (double.IsNaN(value.Width))
                    _panel.Width = 300;
                transformpanel.X = _panel.Width;
            }
        }

        internal abstract void load();
        internal abstract void unload();


        public void begin()
        {
            if (grdPanel.Children.Count != 0)
            {
                load();
                anipanel.Completed += new EventHandler(aniBegin_Completed);


                anipanel.To = 0;
                transformpanel.BeginAnimation(TranslateTransform.XProperty, anipanel);
            }
            else
            {
                load();
            }
        }

        public void end()
        {
            if (grdPanel.Children.Count != 0)
            {
                unload();
                anipanel.Completed += new EventHandler(aniEnd_Completed);
                //anipanel.To = 300;

                anipanel.To = panel.Width;

                transformpanel.BeginAnimation(TranslateTransform.XProperty, anipanel);
            }
            else
                unload();
        }


        void aniBegin_Completed(object sender, EventArgs e)
        {
            //load();
            anipanel.Completed -= aniBegin_Completed;
        }
        void aniEnd_Completed(object sender, EventArgs e)
        {
            root.grdInfo.Children.Remove(grdPanel);
            anipanel.Completed -= aniEnd_Completed;
            //unload();
        }



        #region ========== 信息面板相关 ==============
        protected Grid grdPanel = new Grid(); //信息面板容器
        DoubleAnimation anipanel = new DoubleAnimation() { Duration = TimeSpan.FromMilliseconds(500) };
        protected TranslateTransform transformpanel = new TranslateTransform();

        #endregion
    }
}
