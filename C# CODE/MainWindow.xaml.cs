using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
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
using System.Windows.Navigation;
using System.IO;
using System.Threading;
using System.Reflection;
using Microsoft.Win32;

namespace HINF
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort m_sp1;

        private object myLock = new object();

        public static int BaudRate { get; } = 9600;
        public static string BaseDirectory { get; private set; }
        public static string StudentsFilePath { get; private set; }
        public static string LogFilePath { get; private set; }

        private Queue<ProcessData> _inputQueue = new Queue<ProcessData>();

        Thread thread;


        private ObservableCollection<StudentData> _stdList = new ObservableCollection<StudentData>();
        public ObservableCollection<StudentData> StdList
        {
            get { return _stdList; }
        }


        private ObservableCollection<string> _logList = new ObservableCollection<string>();
        public ObservableCollection<string> LogList
        {
            get { return _logList; }
        }

        private void SetLog()
        {
            _logList.Clear();

            foreach(var str in Log.LoadLog(LogFilePath))
            {
                if(!String.IsNullOrEmpty(str))
                    _logList.Add(str);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            RegistryKey systemKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion");

            BaseDirectory = Path.Combine(systemKey.GetValue("ProgramFilesDir (x86)") as String, "HINF");
            StudentsFilePath = Path.Combine(BaseDirectory, "students.dat");
            LogFilePath = Path.Combine(BaseDirectory, "log.txt");

            if (!Directory.Exists(BaseDirectory))
                Directory.CreateDirectory(BaseDirectory);

            if (File.Exists(StudentsFilePath))
                Reload_StdList();
            else
            {
                File.Create(StudentsFilePath);
            }

            if (File.Exists(LogFilePath))
                SetLog();
            else
            {
                File.Create(LogFilePath);
            }

            btnClose.IsEnabled = false;

            thread = new Thread(RecieveFunc);
            thread.IsBackground = true;
            
        }

        private List<T> GetNormalList<T>(ObservableCollection<T> data)
        {
            List<T> stdData = new List<T>();

            foreach (var std in data)
            {
                stdData.Add(std);
            }
            return stdData;
        }

        private ObservableCollection<T> GetObservableList<T>(List<T> data)
        {
            ObservableCollection<T> stdData = new ObservableCollection<T>();

            foreach (var std in data)
            {
                stdData.Add(std);
            }
            return stdData;
        }

        private void SyncList(ObservableCollection<StudentData> data, ref ObservableCollection<StudentData> output)
        {
            var copy = GetNormalList(data);
            if (output.Count > 0)
                output.Clear();
            if (copy.Count == 0)
                return;
            foreach (var std in copy)
            {
                output.Add(std);
            }
        }

        private void Reload_StdList()
        {
            ObservableCollection<StudentData> list = InputStudentData.GetStudentDataFrom(StudentsFilePath);
            SyncList(list, ref _stdList);

            if (StudentData.HasOverlapedClassNum(_stdList, out Tuple<StudentData, StudentData> overlapped) == true)
            {
                MessageBox.Show($"중복된 학생번호가 있습니다! {Environment.NewLine}: {overlapped.Item1}{Environment.NewLine}: {overlapped.Item2}{Environment.NewLine}오류 방지를 위해 수정해주세요",
                    "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // m_sp1 값이 null 일때만 새로운 SerialPort 를 생성합니다.
                if (null == m_sp1)
                {
                    m_sp1 = new SerialPort();
                    m_sp1.PortName = txtPort.Text;   // 컴포트명
                    m_sp1.BaudRate = BaudRate;   // 보레이트

                    m_sp1.Open();
                }

                btnOpen.IsEnabled = !m_sp1.IsOpen;    // OPEN BUTTON Disable
                btnClose.IsEnabled = m_sp1.IsOpen;     // CLOSE BUTTON Enable

                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            // m_sp1 이 null 아닐때만 close 처리를 해준다.
            if (null != m_sp1)
            {
                if (m_sp1.IsOpen)
                {
                    m_sp1.Close();
                    m_sp1.Dispose();
                    m_sp1 = null;
                }
                
            }
            btnOpen.IsEnabled = true;
            btnClose.IsEnabled = false;

            thread.Abort();
        }

        private void btnRecieve_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                while(_inputQueue.Count > 0)
                {
                    ProcessData input = _inputQueue.Dequeue();

                    if (StudentData.FindMatchingAndAdd(input.processString,_stdList, out StudentData student) == true)
                    {
                        InputStudentData.PutStudentDataTo(StudentsFilePath, _stdList);
                        Log.LogText(LogFilePath, $"{student.Name}({student.ClassNum})(이)가 장애우를 도와 마일리지를 1 적립시켰습니다!", input.processTime);
                        Reload_StdList();
                        SetLog();
                    }
                    else
                    {
                        Log.LogText(LogFilePath, $"학번 {input.processString}(이)가 장애우를 도왔지만 리스트에 학생이 없어 마일리지 적립에 실패했습니다.", input.processTime);
                        SetLog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private delegate void RecieveDelegate(object sender, RoutedEventArgs e);

        private void RecieveFunc()
        {
            while (true)
            {
                try
                {
                    int iRecSize = m_sp1.BytesToRead; // 수신된 데이터 갯수
                    string strRxData;

                    if (iRecSize != 0)
                    {
                        Thread.Sleep(500);
                        iRecSize = m_sp1.BytesToRead;

                        strRxData = "";
                        byte[] buff = new byte[iRecSize];

                        m_sp1.Read(buff, 0, iRecSize);
                        strRxData = Encoding.UTF8.GetString(buff);

                        _inputQueue.Enqueue(new ProcessData(strRxData, DateTime.Now));
                    }
                }
                catch (Exception)
                {
                    //MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnCreateStds_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<StudentData> data = new ObservableCollection<StudentData>
            {
                new StudentData("한승우", "1"),
                new StudentData("최호영", "2"),
                new StudentData("김혜진", "3"),
                new StudentData("최은석", "4")
            };

            InputStudentData.PutStudentDataTo(StudentsFilePath, data);
            MessageBox.Show(InputStudentData.GetStudentDataFrom(StudentsFilePath)[0].ToString());
        }

        private void btnRegStd_Click(object sender, RoutedEventArgs e)
        {
            RegStudent regStudent = new RegStudent();
            if(regStudent.ShowDialog() == true)
            {
                SyncList(regStudent.StdList, ref _stdList);
            }
            InputStudentData.PutStudentDataTo(StudentsFilePath, _stdList);

            if (StudentData.HasOverlapedClassNum(_stdList, out Tuple<StudentData, StudentData> overlapped) == true)
            {
                MessageBox.Show($"중복된 학생번호가 있습니다! {Environment.NewLine}: {overlapped.Item1}{Environment.NewLine}: {overlapped.Item2}{Environment.NewLine}오류 방지를 위해 수정해주세요",
                    "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            InputStudentData.PutStudentDataTo(StudentsFilePath, _stdList);
        }

        private void btnRemStd_Click(object sender, RoutedEventArgs e)
        {
            RemoveStudent removeStudent = new RemoveStudent();
            removeStudent.StdList = _stdList;
            if(removeStudent.ShowDialog() == true)
            {
                SyncList(removeStudent.StdList, ref _stdList);
            }
            InputStudentData.PutStudentDataTo(StudentsFilePath, _stdList);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchStudent searchStudent = new SearchStudent();
            searchStudent.StdList = _stdList;
            searchStudent.ShowDialog();
            SyncList(searchStudent.StdList, ref _stdList);
            InputStudentData.PutStudentDataTo(StudentsFilePath, _stdList);
            SetLog();
        }
    }
}
