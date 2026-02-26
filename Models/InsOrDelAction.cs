using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Sheet_App.Models
{
    public class InsOrDelAction
    {
        public List<(int, int, String)> OldValues{ get; set; } 
        public List<(int, int, String)> NewValues { get; set; }
        public String ActionType { get; set; }

        public InsOrDelAction(List<(int, int, String)> oldValues, List<(int, int, String)> newValues, String actionType)
        {
            OldValues = oldValues;
            NewValues = newValues;
            ActionType = actionType;
        }
    }
}