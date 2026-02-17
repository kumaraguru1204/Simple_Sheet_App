using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Sheet_App.Models
{
    public class CellAction
    {
        public int row { get; set; }
        public int col { get; set; }
        public string oldValue { get; set; }
        public string newValue { get; set; }

        //public string mode { get; set; }
        public CellAction(int row, int col, String oldValue, String newValue)
        {
            this.row = row;
            this.col = col;
            this.oldValue = oldValue;
            this.newValue = newValue;
            //this.mode = mode;
        }


    }
}