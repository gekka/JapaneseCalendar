//MICROSOFT LIMITED PUBLIC LICENCE

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

namespace WpfApplication1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            CultureInfo culture = new CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            culture.DateTimeFormat.Calendar = new JapaneseCalendar();
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            InitializeComponent();
        }
    }

    class CalendarEx : System.Windows.Controls.Calendar
    {
        public CalendarEx()
        {
            this.Loaded += CalendarEx_Loaded;
        }


        void CalendarEx_Loaded(object sender, RoutedEventArgs e)
        {
            SetToHeaderTemplate(this);
        }
        private static void SetToHeaderTemplate(System.Windows.Controls.Calendar target)
        {
            if (target.Template != null)
            {
                var item = (System.Windows.Controls.Primitives.CalendarItem)target.Template.FindName("PART_CalendarItem", target);
                if (item == null)
                {
                    return;
                }

                var header = (Button)item.Template.FindName("PART_HeaderButton", item);
                Binding bnd = new Binding("HeaderTemplate");
                bnd.Source = target;
                header.SetBinding(ContentControl.ContentTemplateProperty, bnd);
            }
            else
            {
                target.Loaded += (s, e) =>
                    {
                        SetToHeaderTemplate(target);
                    };
            }
        }

        public static DataTemplate GetHeaderTemplate(DependencyObject obj)
        {
            return (DataTemplate)obj.GetValue(HeaderTemplateProperty);
        }

        public static void SetHeaderTemplate(DependencyObject obj, DataTemplate value)
        {
            obj.SetValue(HeaderTemplateProperty, value);
        }

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.RegisterAttached
            ("HeaderTemplate"
            , typeof(DataTemplate)
            , typeof(System.Windows.Controls.Calendar)
            , new PropertyMetadata(null, HeaderTemplatePropertyChangedCallback));

        private static void HeaderTemplatePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetToHeaderTemplate((System.Windows.Controls.Calendar)d);
        }
    }

    /// <summary>カレンダーに年が西暦の文字列で入ってくるので、それを和暦文字に変換するコンバーター</summary>
    class YearConverter : IValueConverter
    {
        static string[] MonthString = { "睦月", "如月", "弥生", "卯月", "皐月", "水無月", "文月", "葉月", "長月", "神無月", "霜月", "師走" };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            string s = value.ToString().Trim();
            if (s.Length == 0)
            {
                return string.Empty;
            }
            int i;
            if (!int.TryParse(s.Substring(0, 1), out i))
            {
                return s;
            }


            var ss = s.Split('-');
            DateTime t1;
            DateTime t2;
            string retval;
            string YEARMONTH_PATTERN = System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.YearMonthPattern;
            string YEAR_PATTERN = "ggyy";
            int y;
            if (!int.TryParse(ss[0], out y))
            {
                if (DateTime.TryParse(ss[0], culture, DateTimeStyles.None, out t1) || DateTime.TryParse(ss[0], System.Threading.Thread.CurrentThread.CurrentUICulture, DateTimeStyles.None, out t1))
                {
                    bool isHeader = parameter == null ? false : (parameter.ToString().ToUpper() == "H");
                    if (isHeader)
                    {
                        retval = t1.ToString(YEARMONTH_PATTERN);
                    }
                    else
                    {
                        retval = t1.ToString(YEAR_PATTERN, System.Threading.Thread.CurrentThread.CurrentUICulture);
                    }
                }
                else
                {
                    //英語の月名とか
                    return value;
                }
            }
            else if (y <= 12)
            {
                //12以下は月表示とみなす
                bool is陰暦の月 = parameter == null ? false : (parameter.ToString().ToUpper() == "K");
                if (is陰暦の月)
                {
                    retval = MonthString[y - 1];
                }
                else
                {
                    retval = value.ToString();
                }
            }
            else
            {
                try
                {
                    t1 = new DateTime(int.Parse(ss[0]), 1, 1);
                    retval = t1.ToString(YEAR_PATTERN, System.Threading.Thread.CurrentThread.CurrentUICulture);
                    if (ss.Length >= 2)
                    {
                        t2 = new DateTime(int.Parse(ss[1]), 1, 1);
                        retval += "-" + t2.ToString(YEAR_PATTERN, System.Threading.Thread.CurrentThread.CurrentUICulture);
                    }
                }
                catch
                {
                    return value;
                }
            }
            return retval;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    class Tool
    {
        public static T FindChildInTemplate<T>(Control d) where T : FrameworkElement
        {
            return FindChildInTemplate<T>(d, d);
        }
        public static T FindChildInTemplate<T>(DependencyObject d, Control parent) where T : FrameworkElement
        {
            int count = VisualTreeHelper.GetChildrenCount(d);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(d, i);
                T t = child as T;
                if (t != null)
                {
                    if (t.TemplatedParent == parent)
                    {
                        return t;
                    }
                }

                t = FindChildInTemplate<T>(child, parent);
                if (t != null)
                {
                    return t;
                }

            }
            return null;
        }
    }
}
