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

namespace IDXKPI
{
    /// <summary>
    /// UCIDXKPI.xaml 的交互逻辑
    /// </summary>
    public partial class UCIDXKPI : UserControl
    {
        public UCIDXKPI()
        {
            InitializeComponent();
        }


        ///<summary>是否百分比的指标除以100，用以转换以0-100数表示百分比的情形</summary>
        public bool isPerDiv100 { get; set; }

        private DataTable _idxDataSource;
        ///<summary>指数数据源，显示指数形式</summary>
        public DataTable idxDataSource
        {
            get { return _idxDataSource; }
            set { _idxDataSource = value; initidx(); }
        }




        #region 指数相关

        double percdiv = 1;

        //List<SortIDX> sortidxes = new List<SortIDX>();
        //List<SortIDX> sortgood = new List<SortIDX>();
        //List<SortIDX> sortbad = new List<SortIDX>();
        IDXViewModel idxvm = new IDXViewModel();

        void initidx()
        {
            if (isPerDiv100)
                percdiv = 100;

            //读取数据
            //select cast(ID as nvarchar(100)) id,sort, sort1,sort2,ord,indexname, definition,IMPORTANT,format,VALUE,UNIT,VALUENOTE,REFER1,REFER2,REFERTYPE,refernote,necessary from d_index where SORT0='863' order by ord,IMPORTANT
            SortIDX sortidx;
            IndexData index;

            IEnumerable<string> sorts = idxDataSource.AsEnumerable().GroupBy(p => p.Field<string>("sort")).Select(p => p.Key);
            foreach (string sort in sorts)
            {
                sortidx = new SortIDX() { name = sort + "指数", idx = 100 };
                idxvm.sortgoods.Add(sortidx);
                sortidx = new SortIDX() { name = sort + "指数", idx = 60 };
                idxvm.sortbads.Add(sortidx);
                sortidx = new SortIDX() { name = sort + "指数" };
                idxvm.sortidxes.Add(sortidx);



                IEnumerable<DataRow> drs = idxDataSource.AsEnumerable().Where(p => p.Field<string>("sort") == sort);
                foreach (DataRow dr in drs)
                {
                    if (dr["value"] is DBNull || dr["REFER1"] is DBNull || dr["IMPORTANT"] is DBNull || dr["REFERTYPE"] is DBNull) continue;

                    index = new IndexData(sortidx)
                    {
                        id = dr.Field<string>("id"),
                        name = dr.Field<string>("indexname"),
                        define = dr.Field<string>("definition"),
                        format = dr.Field<string>("format"),
                        reftype = dr.Field<string>("REFERTYPE"),
                        unit = dr.Field<string>("UNIT"),
                        refnote = dr.Field<string>("refernote"),
                        necessary = dr.Field<bool>("necessary"),
                        value = double.Parse(dr["value"].ToString()),
                        ref1 = double.Parse(dr["REFER1"].ToString()),
                        important = int.Parse(dr["important"].ToString())
                    };
                    if (!(dr["REFER2"] is DBNull))
                        index.ref2 = double.Parse(dr["REFER2"].ToString());
                    if (index.unit == "%")
                    {
                        index.value = index.value / percdiv;
                        index.ref1 = index.ref1 / percdiv;
                        index.ref2 = index.ref2 / percdiv;
                    }

                    sortidx.indexes.Add(index);
                }

            }

            //计算指数
            foreach (SortIDX item in idxvm.sortidxes)
            {
                item.calidx();
            }

            //radgood.DataSource = sortgood;
            //radbad.DataSource = sortbad;
            //radidx.DataSource = sortidxes;


            grdMain.DataContext = idxvm;

            idxvm.selectSort = idxvm.sortidxes[0];
        }


        #endregion

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lstIndex.SelectedIndex = -1;
        }

        private void imgHelp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WinHelp winhelp = new WinHelp();
            winhelp.ShowDialog();
        }



    }


    internal class IDXViewModel : MyClassLibrary.MVVM.DependencyNotificationObject
    {


        private List<SortIDX> _sortidxes = new List<SortIDX>();
        public List<SortIDX> sortidxes
        {
            get { return _sortidxes; }
            set { _sortidxes = value; }
        }

        private List<SortIDX> _sortgoods = new List<SortIDX>();
        public List<SortIDX> sortgoods
        {
            get { return _sortgoods; }
            set { _sortgoods = value; }
        }
        private List<SortIDX> _sortbads = new List<SortIDX>();
        public List<SortIDX> sortbads
        {
            get { return _sortbads; }
            set { _sortbads = value; }
        }




        private SortIDX _selectSort;
        public SortIDX selectSort
        {
            get { return _selectSort; }
            set { _selectSort = value; RaisePropertyChanged(() => selectSort); }
        }



    }



    ///<summary>分类指数</summary>
    internal class SortIDX:MyClassLibrary.MVVM.DependencyNotificationObject
    {
        public SortIDX()
        {
            indexes = new List<IndexData>();
        }

        public string name { get; set; }
        public double idx { get; set; } //指数
        public string idxstring { get { return "：" + idx.ToString("f1"); } }

        public List<IndexData> indexes { get; set; }


        private IndexData _selectIndex;
        public IndexData selectIndex
        {
            get { return _selectIndex; }
            set
            {
                _selectIndex = value; RaisePropertyChanged(() => selectIndex); foreach (IndexData item in indexes)
                {
                    item.updateSelected();
                }
            }
        }
        
        public Visibility isShowSelected
        {
            get { return selectIndex == null ? Visibility.Collapsed : Visibility.Visible; }
        }


        public void calidx()
        {
            int allimportent = 0;
            foreach (IndexData item in indexes)
            {
                item.calidx();
                allimportent += item.important;
            }

            double tmp = 0;
            foreach (IndexData item in indexes)
            {
                tmp += item.idx * item.important / allimportent;
            }

            int badnecessary = indexes.Where(p => p.necessary && p.idx < 60).Count();
            if (badnecessary>0)
            {
                tmp = tmp * 60 / 100;
            }


            idx = tmp;
        }
    }

    ///<summary>指标数据</summary>
    internal class IndexData:MyClassLibrary.MVVM.FrameworkNotificationObject
    {
        public IndexData(SortIDX Parent)
        {
            parent = Parent;
        }
        public SortIDX parent;

        public static Color normalcolor = Colors.ForestGreen;
        public static Color warningcolor = Colors.LightCoral;

        public string id { get; set; }
        public string name { get; set; }
        public string define { get; set; }
        public double value { get; set; }
        public string reftype { get; set; }
        public double ref1 { get; set; }
        public double ref2 { get; set; }
        public int important { get; set; }
        public bool necessary { get; set; }
        public string refnote { get; set; }
        public string format { get; set; }
        public string unit { get; set; }

        public double idx { get; set; } //指数

        //viewmodel用
        public string idxstring { get { return string.Format("({0:f0})", idx); } }
        public Color color1 { get; set; }
        public Color color2 { get; set; }
        public Color color3 { get; set; }
        public double width1 { get; set; }
        public double width2 { get; set; }
        public double width3 { get; set; }
        public double offset { get; set; }
        public Brush txtcolor
        {
            get
            {
                return idx < 60 ? Brushes.Pink : Brushes.LightBlue;
            }
        }
        public string valuestring
        {
            get
            {
                if (unit == "%")
                    return value.ToString("p1");
                else
                    return string.Format("{0}{1}", value.ToString(format), unit);
            }
        }
        public string refstring
        {
            get
            {
                string tmp = "";
                string tmpformat = unit == "%" ? "p1" : format;
                string tmpunit = unit == "%" ? "" : unit;

                switch (reftype)
                {
                    case ">":
                        tmp = string.Format("{0}{1}{2}", reftype, ref1.ToString(tmpformat), tmpunit);
                        break;
                    case "<":
                        tmp = string.Format("{0}{1}{2}", reftype, ref1.ToString(tmpformat), tmpunit);
                        break;
                    case "<>":
                        tmp = string.Format(">{0}{2}且<{1}{2}", ref1.ToString(tmpformat), ref2.ToString(tmpformat), tmpunit);
                        break;
                    case "><":
                        tmp = string.Format("<{0}{2}或>{1}{2}", ref1.ToString(tmpformat), ref2.ToString(tmpformat), tmpunit);
                        break;
                }

                return tmp;
            }
        }

        public Visibility isShowDetail
        {
            get { return this.Equals(parent.selectIndex) ? Visibility.Visible : Visibility.Collapsed; }
        }

        internal void updateSelected()
        {
            RaisePropertyChanged(() => isShowDetail);

            //height = this.Equals(parent.selectIndex) ? 68 : 0;
            ani.To = this.Equals(parent.selectIndex) ? 68 : 0;
            this.BeginAnimation(IndexData.heightProperty, ani);

        }
        System.Windows.Media.Animation.DoubleAnimation ani = new System.Windows.Media.Animation.DoubleAnimation() { Duration=TimeSpan.FromSeconds(0.3)};


        
        public double height
        {
            get { return (double)GetValue(heightProperty); }
            set { SetValue(heightProperty, value); }
        }
        public static readonly DependencyProperty heightProperty =
            DependencyProperty.Register("height", typeof(double), typeof(IndexData), new UIPropertyMetadata(0.0, new PropertyChangedCallback(OnheightChanged)));
        private static void OnheightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IndexData Sender = (IndexData)d;

        }
      

     


        internal void calidx()
        {
            //if (!hasidx) return;
            double normalvalue, bestvalue, worstvalue;

            switch (reftype)
            {
                case ">":
                    normalvalue = ref1;
                    bestvalue = (unit == "%") ? 1 : normalvalue / 0.6; //百分比类100%，数字类以normal为60计算100的值
                    if (value >= normalvalue)
                        idx = 60 + (value - normalvalue) / (bestvalue - normalvalue) * 40;
                    else
                        idx = value / normalvalue * 60;

                    color1 = IndexData.warningcolor;
                    color2 = IndexData.normalcolor;
                    color3 = Colors.Transparent;
                    width1 = (unit == "%") ? ref1 * 200 : 100;
                    width2 = (unit == "%") ? (1 - ref1) * 200 : 100;
                    width3 = 0;
                    offset = (unit == "%") ? value * 200 : value / 2 / ref1 * 200;

                    break;
                case "<":
                    normalvalue = ref1;
                    worstvalue = (unit == "%") ? 1 : normalvalue / 0.6; //百分比类100%，数字类以normal为60计算100的值
                    if (value <= normalvalue)
                        idx = 60 + (normalvalue - value) / normalvalue * 40;
                    else
                        idx = (worstvalue - value) / (worstvalue - normalvalue) * 60;

                    color1 = IndexData.normalcolor;
                    color2 = IndexData.warningcolor;
                    color3 = Colors.Transparent;
                    width1 = (unit == "%") ? ref1 * 200 : 100;
                    width2 = (unit == "%") ? (1 - ref1) * 200 : 100;
                    width3 = 0;
                    offset = (unit == "%") ? value * 200 : value / 2 / ref1 * 200;

                    break;
                case "<>":
                    bestvalue = (ref1 + ref2) / 2;
                    if (ref1 <= value && value <= ref2)
                        idx = 60 + (1 - Math.Abs(value - bestvalue) / (bestvalue - ref1)) * 40;
                    else if (value < ref1)
                    {
                        worstvalue = ref1 - (bestvalue - ref1) * 6 / 4;
                        idx = ((value - worstvalue) / (ref1 - worstvalue)) * 60;
                    }
                    else if (value > ref2)
                    {
                        worstvalue = ref2 + (bestvalue - ref1) * 6 / 4;
                        idx = ((worstvalue - value) / (worstvalue - ref2)) * 60;
                    }

                    color1 = IndexData.warningcolor;
                    color2 = IndexData.normalcolor;
                    color3 = IndexData.warningcolor;
                    width1 = (unit == "%") ? ref1 * 200 : 70;
                    width2 = (unit == "%") ? (ref2 - ref1) * 200 : 60;
                    width3 = (unit == "%") ? (1 - ref2) * 200 : 70;
                    offset = (unit == "%") ? value * 200 : 70 + (value - ref1) / (ref2 - ref1) * 60;

                    break;
                case "><":
                    worstvalue = (ref1 + ref2) / 2;
                    if (ref1 < value && value < ref2) //不满足
                        idx = Math.Abs(value - worstvalue) / (worstvalue - ref1) * 60;
                    else if (value < ref1)
                    {
                        bestvalue = ref1 - (worstvalue - ref1) * 6 / 4;
                        idx = 60 + ((ref1 - value) / (ref1 - bestvalue)) * 40;
                    }
                    else if (value > ref2)
                    {
                        bestvalue = ref2 + (worstvalue - ref1) * 6 / 4;
                        idx = 60 + ((value - ref2) / (bestvalue - ref2)) * 40;
                    }

                    color1 = IndexData.normalcolor;
                    color2 = IndexData.warningcolor;
                    color3 = IndexData.normalcolor;
                    width1 = (unit == "%") ? ref1 * 200 : 70;
                    width2 = (unit == "%") ? (ref2 - ref1) * 200 : 60;
                    width3 = (unit == "%") ? (1 - ref2) * 200 : 70;
                    offset = (unit == "%") ? value * 200 : 70 + (value - ref1) / (ref2 - ref1) * 60;

                    break;
            }

            if (idx < 0) idx = 0;
            if (idx > 100) idx = 100;
            if (offset < 0) offset = 0;
            if (offset > 200) offset = 200;
        }

    }

}
