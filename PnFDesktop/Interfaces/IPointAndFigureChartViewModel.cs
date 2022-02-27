using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PnFData.Model;

namespace PnFDesktop.Interfaces
{
    public interface IPointAndFigureChartViewModel
    {
        Guid ChartId { get; }

        PnFChart Chart { get; }

        double ContentWidth { get; }

        double ContentHeight { get; }

        double ContentViewportWidth { get; }

        double ContentViewportHeight { get; }

    }
}
