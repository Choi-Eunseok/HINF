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
    /// RegStudent.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RegStudent : Window
    {
        public List<StudentData> AddedStudents { get; private set; }

        public RegStudent()
        {
            InitializeComponent();
        }

        private ObservableCollection<StudentData> _stdList = new ObservableCollection<StudentData>();
        public ObservableCollection<StudentData> StdList
        {
            get { return _stdList; }
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            if(!(String.IsNullOrEmpty(txtName.Text) || String.IsNullOrEmpty(txtNum.Text)))
            {
                _stdList.Add(new StudentData(txtName.Text, txtNum.Text));
            }
            txtName.Text = txtNum.Text = String.Empty;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _stdList = null;
            DialogResult = false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if(_stdList.Count == 0)
            {
                _stdList = null;
                DialogResult = false;
                return;
            }
            else
            {
                DialogResult = true;
            }
        }
    }
}
