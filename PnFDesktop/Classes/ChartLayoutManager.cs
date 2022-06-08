using Microsoft.Toolkit.Mvvm.ComponentModel;
using PnFData.Model;
using PnFDesktop.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFDesktop.Classes
{

    /// <summary>
    /// Manages the layout of a PnF chart based on various parameters
    /// </summary>
    public class ChartLayoutManager : ObservableObject, IChartLayoutManager
    {
        private double _gridSize;            // The PnF grid height in pixels
        private double _columnCount;        // The number of columns of boxes
        private double _rowCount;           // The number of rows of boxes
        private double _margin;             // The space between chart areas
        private double _labelSpace;         // The space allowed for axis labels
        private double _sheetHeight;
        private double _sheetWidth;
        private int _minBoxIndex;
        private int _maxBoxIndex;
        private int _minColIndex;
        private int _maxColIndex;

        public double SheetWidth => _sheetWidth;

        public double SheetHeight => _sheetHeight;

        public double GridSize => _gridSize;

        public void Initialize(PnFChart chart, double gridSize, double margin, double labelSpace)
        {
            UpdateMinMaxValuesFromChart(chart);
            _columnCount = _maxColIndex - _minColIndex;
            _rowCount = _maxBoxIndex - _minBoxIndex;
            _gridSize = gridSize;
            _margin = margin;
            _labelSpace = labelSpace;
            _sheetWidth = (4.0 * _margin) + (2 * _labelSpace) + (_columnCount * _gridSize);
            _sheetHeight = (3.0 * (_margin) + _labelSpace) + (_rowCount * _gridSize);
        }

        private void UpdateMinMaxValuesFromChart(PnFChart chart)
        {
            int minColIndex = int.MaxValue;
            int maxColIndex = int.MinValue;
            int minBoxIndex = int.MaxValue;
            int maxBoxIndex = int.MinValue;
            int maxIndex = 0;

            if (chart.Columns.Count > 0)
            {
                foreach (var column in chart.Columns)
                {
                    minColIndex = Math.Min(minColIndex, column.Index);
                    maxColIndex = Math.Max(maxColIndex, column.Index);
                    foreach (var box in column.Boxes)
                    {
                        minBoxIndex = Math.Min(minBoxIndex, box.Index);
                        maxBoxIndex = Math.Max(maxBoxIndex, box.Index);
                        if (box.Index > maxIndex) maxIndex = box.Index;
                    }
                }
                _minColIndex = minColIndex;
                _maxColIndex = maxColIndex;
                _minBoxIndex = minBoxIndex;
                _maxBoxIndex = maxBoxIndex;
            }

        }
        public double GetColumnXCoordinate(int colIndex)
        {
            double x = ((2.0 * _margin) + _labelSpace + ((colIndex - _minColIndex) * _gridSize));
            return x;
        }

        public double GetRowYCoordinate(int rowIndex)
        {
            double y = _sheetHeight - ((2.0 * _margin) + _labelSpace + ((rowIndex - _minBoxIndex) * _gridSize));
            return y;
        }

        public RowData GetRowData(int rowIndex)
        {
            Point leftLabel = new Point(_margin,
                _sheetHeight - ((2.0 * _margin) + _labelSpace + ((rowIndex - _minBoxIndex) * _gridSize))
                );
            Point rightLabel = new Point(_sheetWidth - _margin - _labelSpace, leftLabel.Y);
            Highlight highlight = new Highlight(leftLabel.X, leftLabel.Y, rightLabel.X - leftLabel.X + _labelSpace, _gridSize);
            return new RowData(leftLabel, rightLabel, highlight);
        }

        public ColumnData GetColumnData(int colIndex)
        {
            Point bottomLabel = new Point((2 * _margin) + _labelSpace + ((colIndex - _minColIndex) * _gridSize),
                               _sheetHeight - _margin - _labelSpace);
            Highlight highlight = new Highlight(bottomLabel.X, _margin, _gridSize, _sheetHeight - (2.0 *_margin));
            return new ColumnData(bottomLabel, highlight);
        }

    }
}
