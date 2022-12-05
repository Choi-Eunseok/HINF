using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace HINF
{
    /// <summary>
    /// SearchStudent.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SearchStudent : Window
    {
        private ObservableCollection<StudentData> _stdList;
        public ObservableCollection<StudentData> StdList
        {
            get { return _stdList; }
            set
            {
                _stdList = value;
                _eleList.Clear();
                foreach (var item in _stdList)
                {
                    WrapPanel panel = GetWrapPanel(item.ToString());
                    panel.Tag = item;
                    _eleList.Add(panel);
                    
                }
            }
        }

        private ObservableCollection<UIElement> _eleList = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> EleList
        {
            get { return _eleList; }
        }

        public SearchStudent()
        {
            InitializeComponent();
        }

        private void Search(object sender, TextChangedEventArgs e)
        {
            if(Int32.TryParse(txtLeaseM.Text, out int minMileage) == false)
            {
                txtLeaseM.Text = String.Empty;
                return;
            }

            if (String.IsNullOrEmpty(txtLeaseM.Text) == false)
            {
                _eleList.Clear();
                foreach (var item in _stdList)
                {
                    if (item.Mileage >= minMileage)
                    {
                        WrapPanel panel = GetWrapPanel(item.ToString());
                        panel.Tag = item;
                        _eleList.Add(panel);
                    }
                }
                return;
            }
            else
            {
                _eleList.Clear();
                foreach (var item in _stdList)
                {
                    WrapPanel panel = GetWrapPanel(item.ToString());
                    panel.Tag = item;
                    _eleList.Add(panel);
                }
                return;
            }
        }

        private WrapPanel GetWrapPanel(string labelText)
        {
            WrapPanel panel = new WrapPanel();

            CheckBox box = new CheckBox();
            box.VerticalAlignment = VerticalAlignment.Center;

            Label label = new Label();
            label.Content = labelText;

            panel.Children.Add(box);
            panel.Children.Add(label);

            return panel;
        }

        private void btnDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (Int32.TryParse(txtDeMileage.Text, out int decrease) == false) return;

            foreach(var item in _eleList)
            {
                WrapPanel panel = item as WrapPanel;
                CheckBox box = panel.Children[0] as CheckBox;

                if(box.IsChecked == true)
                {
                    foreach(var std in _stdList)
                    {
                        if(std == panel.Tag as StudentData)
                        {
                            int realDec = decrease;
                            if(decrease > std.Mileage)
                            {
                                realDec = std.Mileage;
                            }

                            if(realDec == 0)
                            {
                                continue;
                            }

                            std.DecreaseMileage(realDec);
                            Log.LogText(MainWindow.LogFilePath, $"{std.Name}({std.ClassNum})의 마일리지를 {realDec}만큼 하락시켰습니다.", DateTime.Now);
                            
                        }
                    }
                }
            }
            Sync();
        }

        /// <summary>
        /// _eleList를 _stdList에 맞춥니다.
        /// </summary>
        private void Sync()
        {
            _eleList.Clear();

            foreach (var item in _stdList)
            {
                WrapPanel panel = GetWrapPanel(item.ToString());
                panel.Tag = item;
                _eleList.Add(panel);

            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            
            DialogResult = true;
        }
    }
}
