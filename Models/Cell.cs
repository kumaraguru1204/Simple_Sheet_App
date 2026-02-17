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
        public String Value { get; set; }
        public DateTime LastModified { get; set; }
        public int CellKey { get; set; }

        public Cell(String value, DateTime lastModified, int cellKey)
        {

            Value = value;
            LastModified = DateTime.Now;
            CellKey = cellKey;
        }
        public void UpdateValue(String val)
        {
            Value = val;
            LastModified = DateTime.Now;
        }
    }
}