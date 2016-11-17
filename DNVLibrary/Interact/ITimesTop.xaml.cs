using System;
using System.Data;
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

namespace DNVLibrary.Interact
{
    /// <summary>
    /// ITimesTop.xaml 的交互逻辑
    /// </summary>
    public partial class ITimesTop : UserControl
    {
        public ITimesTop()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            tmr.Tick += new EventHandler(tmr_Tick);
        }

        int timecount = 0;
        void tmr_Tick(object sender, EventArgs e)  
        {
            //读计算状态
            DataLayer.DataProvider.curDataSourceName = "互动数据源";
            string sql = string.Format("select tStatus,tError,tProgress from cal_task where tid='{0}'", ITimesController.taskid);
            DataTable dt = DataLayer.DataProvider.getDataTableFromSQL(sql);

            int calstatus = dt.Rows[0].getInt("tStatus");
            if (calstatus == 0) //等待
            {
                info.Text = string.Format("{0}的时序推演计算加入计算队列等待。", prjname);
                bar.Value = 0;
            }
            else if (calstatus == 1) //计算中
            {
                info.Text = string.Format("{0}的时序推演正在计算中...", prjname);
                bar.Value = dt.Rows[0].getDouble("tProgress") * 100;
            }
            else if (calstatus == 2) //出错
            {
                string calerror = dt.Rows[0].getString("tError");
                info.Text = string.Format("{0}的时序推演出错，{1}", prjname, calerror);
                bar.Value = 0;
                tmr.Stop();
                ITimesController.step = ITimesController.EStep.推演出错;
                ITimesController.refresh();
                //若本页面可见，设置状态栏为无计算，若本页面不可见，设置状态栏为计算完成。
                statusbartask.status = this.IsVisible ? MyBaseControls.StatusBarTool.CalStatus.EStatus.无计算 : MyBaseControls.StatusBarTool.CalStatus.EStatus.计算完成;
            }
            else if (calstatus == 3)  //结束
            {
                info.Text = string.Format("{0}的时序推演完成。", prjname);
                bar.Value = 100;
                tmr.Stop();
                ITimesController.step = ITimesController.EStep.推演完成;
                ITimesController.refresh();
                //若本页面可见，设置状态栏为无计算，若本页面不可见，设置状态栏为计算完成。
                statusbartask.status = this.IsVisible ? MyBaseControls.StatusBarTool.CalStatus.EStatus.无计算 : MyBaseControls.StatusBarTool.CalStatus.EStatus.计算完成;

            }

            timecount++;
            if (timecount > 180)  //timeout  超时
            {
                info.Text = string.Format("{0}的时序推演超时，请检查计算服务程序。", prjname);
                bar.Value = 0;
                tmr.Stop();
                ITimesController.step = ITimesController.EStep.推演出错;
                ITimesController.refresh();
                //若本页面可见，设置状态栏为无计算，若本页面不可见，设置状态栏为计算完成。
                statusbartask.status = this.IsVisible ? MyBaseControls.StatusBarTool.CalStatus.EStatus.无计算 : MyBaseControls.StatusBarTool.CalStatus.EStatus.计算完成;
            }



            //timecount++;
            //if (timecount > 60)   //若计算完成
            //{
            //    tmr.Stop();
            //    bar.Value = 100;
            //    info.Text = string.Format("{0}的时序推演计算完成。", prjname);
            //    ITimesController.step = ITimesController.EStep.推演完成;
            //    ITimesController.refresh();
            //}
            //bar.Value = 100.0 * timecount / 60;
        }

        System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.5) };


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!tmr.IsEnabled && statusbartask!=null)
                statusbartask.status = MyBaseControls.StatusBarTool.CalStatus.EStatus.无计算 ;
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        MyBaseControls.StatusBarTool.CalTask statusbartask;
        int prjid;
        string prjname;
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            if (prj.cmbTree.SelectedItem == null) return;

            prjid = (prj.cmbTree.SelectedItem as Item).id;
            prjname = (prj.cmbTree.SelectedItem as Item).instanceName;
            ITimesController.step = ITimesController.EStep.时序推演中;
            info.Text = string.Format("正在进行{0}的时序推演计算，请稍候...", prjname);
            bar.Value = 0;

            statusbartask = MyBaseControls.StatusBarTool.StatusBarTool.statusInfo.calStatus.addCalTask("时序推演", null);
            statusbartask.status = MyBaseControls.StatusBarTool.CalStatus.EStatus.计算中;

            //===提交计算服务申请
            DataLayer.DataProvider.curDataSourceName = "互动数据源";
            ITimesController.taskid = MyClassLibrary.helper.getGUID();
            string sql = string.Format("insert cal_para (tID,paraname,paravalue,pnote) values ('{0}','{1}','{2}','{3}')", ITimesController.taskid, "prjid", prjid, "方案id");
            DataLayer.DataProvider.ExecuteSQL(sql);
            sql = string.Format("insert cal_para (tID,paraname,paravalue,pnote) values ('{0}','{1}','{2}','{3}')", ITimesController.taskid, "adjust", spnAdjust.Value / 100, "负荷预测调整百分比");
            DataLayer.DataProvider.ExecuteSQL(sql);
            sql = string.Format("insert cal_para (tID,paraname,paravalue,pnote) values ('{0}','{1}','{2}','{3}')", ITimesController.taskid, "startTime", "2020010101", "开始时间");
            DataLayer.DataProvider.ExecuteSQL(sql);
            sql = string.Format("insert cal_para (tID,paraname,paravalue,pnote) values ('{0}','{1}','{2}','{3}')", ITimesController.taskid, "endTime", "2020010223", "结束时间");
            DataLayer.DataProvider.ExecuteSQL(sql);
            sql = string.Format("insert cal_task (tID,cid,tStatus,tNote,tRequestTime,tProgress,tPeriod) values ('{0}',1,0,'{1}','{2}',0,'{3}')", ITimesController.taskid, "时序推演", DateTime.Now, DateTime.Now.AddDays(1));
            DataLayer.DataProvider.ExecuteSQL(sql);

            timecount = 0;
            ITimesController.refresh();
            tmr.Start(); //持续读计算状态
        }




    }
}
