using PnFData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PnFDesktop.Interfaces
{
    public interface IChartLayoutManager
    {
        double SheetWidth{get; }

        double SheetHeight{get; }

        double GridSize {get;}

        void Initialize(PnFChart chart, double gridSize, double margin, double lableSpace);

        double GetColumnXCoordinate(int colIndex);

        double GetRowYCoordinate(int rowIndex);

        RowData GetRowData (int rowIndex);
        ColumnData GetColumnData (int colIndex);
    }
}
