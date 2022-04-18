using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFDesktop.DTOs
{
    public class DayDTO
    {
        public DateTime Day {get;set;}
        public string DayDescription
        {
            get
            {
                return Day.ToString("d");
            }
        }
    }
}
