using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFData.Interfaces
{
    public interface IDayValue
    {
        DateTime Day { get; }
        double Value { get; }
    }
}
