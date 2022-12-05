using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HINF
{
    /// <summary>
    /// 학생 하나의 정보를 기록할 클래스입니다.
    /// </summary>
    [DataContract]
    public sealed class StudentData
    {
        [DataMember]
        private string name;

        [DataMember]
        private string classNum;

        [DataMember]
        private int mileage;

        public string Name { get { return name; } }
        public string ClassNum { get { return classNum; } }
        public int Mileage { get { return mileage; } }

        public StudentData(string name, string classNum, int mileage = 0)
        {
            if(String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (String.IsNullOrEmpty(classNum))
            {
                throw new ArgumentNullException("classNum");
            }
       
            this.name = name;

            this.classNum = classNum;
            this.mileage = mileage;
        }

        /// <summary>
        /// 매개변수로 전달된 IList에 중복된 ClassNum이 있는지 확인합니다.
        /// </summary>
        /// <param name="list">전달될 IList 인터페이스입니다.</param>
        /// <returns>중복된 학급번호가 있으면 true, 없으면 false입니다.</return>
        public static bool HasOverlapedClassNum(ObservableCollection<StudentData> list, out Tuple<StudentData, StudentData> overlappedArgument)
        {
            for(int i = 0; i < list.Count; i++)
            {
                for(int j = i + 1; j < list.Count; j++)
                {
                    if(i == j)
                    {
                        continue;
                    }
                    else
                    {
                        if(list[i].classNum == list[j].classNum)
                        {
                            overlappedArgument = new Tuple<StudentData, StudentData>(list[i], list[j]);
                            return true;
                        }
                    }
                }
            }
            overlappedArgument = null;
            return false;
        }

        public static bool FindMatchingAndAdd(string studentNumber, ObservableCollection<StudentData> list , out StudentData std)
        {
            foreach (var student in list)
            {
                if (studentNumber == student.classNum)
                {
                    std = student;
                    student.PlusMileage();
                    return true;
                }
            }
            std = null;
            return false;
        }

        public void PlusMileage()
        {
            this.mileage += 1;
        }

        public void DecreaseMileage(int decrease)
        {
            this.mileage -= decrease;
        }

        public override string ToString()
        {
            return $"이름 : {name} / 학번 : {classNum} / 마일리지 : {mileage}";
        }
    }
}
