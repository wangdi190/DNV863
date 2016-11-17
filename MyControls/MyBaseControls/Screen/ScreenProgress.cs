using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace MyBaseControls.Screen
{
    public static class ScreenProgress
    {
        static ScreenProgress()
        {
         
        }

        ///<summary>设置为false时，关闭进度窗体</summary>
        public static bool isActive;

        ///<summary>进度值，当show方法的isIndeterminate参数为false时有效</summary>
        public static double progress = 0;

        ///<summary>进度窗体标题</summary>
        public static string title = "载入进度";

        ///<summary>进度文字</summary>
        public static string info = "";


        ///<summary>显示进度窗体, isIndeterminate参数为true时显示为载入动画, 为false时显示为progress指定进度的进度条</summary>
        public static void show(bool isIndeterminate=true)
        {
            try
            {
                isActive = true;
                Thread th = new Thread(new ThreadStart(() =>
                {
                    var win = new WinProgress();
                    win.bar.IsIndeterminate = isIndeterminate;
                    win.Show();
                    win.active();
                    System.Windows.Threading.Dispatcher.Run();

                }));

                th.SetApartmentState(ApartmentState.STA);
                th.IsBackground = true;

                th.Start();
            }
            catch (Exception e)
            {

            }
            
        }

        ///<summary>隐藏进度窗体</summary>
        public static void hide()
        {
            isActive = false;
        }


    }
}
