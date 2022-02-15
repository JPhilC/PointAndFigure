using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFData.Model
{
    public class Index:EntityData
    {
        public string Name { get; set; }

        public List<IndexDate> Dates { get; } = new List<IndexDate>();
    }
}
