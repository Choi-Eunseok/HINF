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
    /// RemoveStudent.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RemoveStudent : Window
    {
        private ObservableCollection<StudentData> _stdList = new ObservableCollection<StudentData>();
        public ObservableCollection<StudentData> StdList
        {
            get { return _stdList; }
            set
            {
                _stdList = value;
                foreach (var std in _stdList)
                {
                    WrapPanel panel = new WrapPanel();

                    CheckBox box = new CheckBox();
                    box.VerticalAlignment = VerticalAlignment.Center;

                    Label label = new Label();
                    label.Content = std.ToString();

                    panel.Children.Add(box);
                    panel.Children.Add(label);

                    _eleList.Add(panel);
                }
            }
        }

        private ObservableCollection<UIElement> _eleList = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> EleList
        {
            get { return _eleList; }
        }

        public RemoveStudent()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<StudentData> newData = new ObservableCollection<StudentData>();

            for(int i = 0; i < _eleList.Count; i++)
            {
                WrapPanel panel = _eleList[i] as WrapPanel;
                CheckBox box = panel.Children[0] as CheckBox;

                if (box.IsChecked != true)
                {
                    newData.Add(StdList[i]);
                }
            }
            StdList = newData;
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
