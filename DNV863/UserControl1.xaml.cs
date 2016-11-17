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

namespace DNV863
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();

        }
        Random rd = new Random();

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            double minjd, maxjd, minwd, maxwd;
            minjd = 116.32850; maxjd = 116.45070;
            minwd = 39.84914; maxwd = 39.95997;

            string s = "insert into map_object (id,prjid,objname,layerid,shapetype,points,symbolid,reflongitude,reflatitude,class) values ('{0}',{1},'{2}','{3}','{4}','{5}','{6}',{7},{8},'{9}')";
            

            List<string> sqls=new List<string>();
            for (int i = 0; i < 50000; i++)
            {
                string id="t57"+i;
                int prjid=11;
                string objname="模拟对象"+i;
                string layerid="96ab1608-48db-40e8-b84e-d607728e92f3";
                string shapetype="dot";
                string symbolid="#WindTurbine";
                double reflongitude=minjd+(maxjd-minjd)*rd.NextDouble();
                double reflatitude=minwd+(maxwd-minwd)*rd.NextDouble();
                string points=string.Format("{0},{1}",reflatitude,reflongitude);


                string sql = string.Format(s, id, prjid, objname, layerid, shapetype, points, symbolid, reflongitude, reflatitude,"test");
                sqls.Add(sql);


                if (i%10==0)
                {
                    DataLayer.DataProvider.bacthExecute(sqls);
                    sqls.Clear();
                    info.Text = "已插入" + i;
                    MyClassLibrary.helper.refreshScreen();
                }



            }
            
            


        }
    }
}
