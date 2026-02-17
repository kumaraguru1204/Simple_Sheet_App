using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.NetworkOperators;

namespace Simple_Sheet_App.Models
{
    public class Cell
    {
        public int RowNum { get; set; }
        public int ColNum { get; set; }
        public String Value { get; set; }
        public DateTime LastModified { get; set; }

        public Cell(int rowNum, int colNum, String value, DateTime lastModified)
        {
            RowNum = rowNum;
            ColNum = colNum;
            Value = value;
            LastModified = DateTime.Now;
        }
        public void UpdateValue(String val)
        {
            Value = val;
            LastModified = DateTime.Now;
        }
    }
}