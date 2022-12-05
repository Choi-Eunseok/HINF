using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Collections.ObjectModel;

namespace HINF
{
    public static class InputStudentData
    {
        private static char SplitChar = '/';

        public static ObservableCollection<StudentData> GetStudentDataFrom(string path)
        {
            using (FileStream fs =
                new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                ObservableCollection<StudentData> data = new ObservableCollection<StudentData>();

                if (fs.Length == 0)
                    return data;

                DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(StudentData));

                MemoryStream ms = new MemoryStream();

                byte[] buf = new byte[fs.Length];

                fs.Read(buf, 0, buf.Length);

                string content = Encoding.UTF8.GetString(buf);
                foreach(var each in content.Split(SplitChar))
                {
                    if (String.IsNullOrEmpty(each)) continue;
                    ms.Write(Encoding.UTF8.GetBytes(each), 0, Encoding.UTF8.GetBytes(each).Length);
                    ms.Position = 0;
                    data.Add(dcjs.ReadObject(ms) as StudentData);
                    ms = new MemoryStream();
                }

                return data;
            }
        }

        public static void PutStudentDataTo(string path, ObservableCollection<StudentData> studentList)
        {
            string content = String.Empty;

            using (FileStream fs =
                new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(StudentData));

                MemoryStream ms = new MemoryStream();
                
                foreach(var student in studentList)
                {
                    dcjs.WriteObject(ms, student);
                    content += Encoding.UTF8.GetString(ms.ToArray()) + SplitChar;
                    ms = new MemoryStream();
                }
                
            }
            File.WriteAllText(path, content);
        }
    }
}
