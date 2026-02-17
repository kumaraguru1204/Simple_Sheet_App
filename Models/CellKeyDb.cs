using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Sheet_App.Models
{
    public class CellKeyDb
    {
        public int CellKey { get; set; }
        public int RowNum { get; set; }
        public int ColNum { get; set; }

        public CellKeyDb(int cellKey, int rowNum, int colNum)
        {
            CellKey = cellKey;
            RowNum = rowNum;
            ColNum = colNum;
        }
    }
}
