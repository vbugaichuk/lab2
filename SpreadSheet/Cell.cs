using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTable;
namespace SmartTable
{
    public class Cell
    {
        public string Expression { get; set; } //рядок, що зберігає вираз, записаний в комірку(наприклад "Min(A1;10) + B1"
        public double Value { get; set; } //числове значення, пов'язане з коміркою
        public string Error { get; set; } = null;
        public int RowNumber;
        public string ColumnLetter;
        public List<Cell> References { get; set; } = new List<Cell>();

        public Class26BaseSys Class26BaseSys
        {
            get => default(Class26BaseSys);
            set { }
        }
    }
}