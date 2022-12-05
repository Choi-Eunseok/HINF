using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HINF
{
    class ProcessData
    {
        public string processString;
        public DateTime processTime;

        public ProcessData(string processString, DateTime processTime)
        {
            this.processString = processString;
            this.processTime = processTime;
        }
    }
}
