using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNVLibrary.Interact
{
    internal static class ITimesController
    {
        public enum EStep { 起始, 时序推演中,推演出错, 推演完成 }

        public static ITimesTop uctop;  //顶部控件
        public static ITimesPanel ucpanel;  //主控件

        public static string taskid;

        public static EStep step = EStep.起始;

        ///<summary>根据step刷新显示</summary>
        public static void refresh()
        {
            switch (step)
            {
                case EStep.起始:
                    ucpanel.Visibility = System.Windows.Visibility.Collapsed;
                    ucpanel.clearData();
                    break;
                case EStep.推演出错:
                    ucpanel.Visibility = System.Windows.Visibility.Collapsed;
                    ucpanel.clearData();
                    break;
                case EStep.时序推演中:
                    ucpanel.Visibility = System.Windows.Visibility.Collapsed;
                    ucpanel.clearData();
                    break;
                case EStep.推演完成:
                    ucpanel.Visibility = System.Windows.Visibility.Visible;
                    ucpanel.readalldata();
                    break;
                default:
                    break;
            }
        }

    }
}
