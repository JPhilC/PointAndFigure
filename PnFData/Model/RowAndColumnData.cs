using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFData.Model
{
    public struct RowData
    {
        public Point LeftLabel;
        public Point RightLabel;
        public Highlight RowHighlight;

        public RowData(Point leftLabel, Point rightLabel, Highlight rowHighlight)
        {
            LeftLabel = leftLabel; 
            RightLabel = rightLabel;
            RowHighlight = rowHighlight;    
        }
    }

    public struct ColumnData
    {
        public Point BottomLabel;
        public Highlight ColumnHighlight;

        public ColumnData(Point bottomLabel, Highlight columnHighlight)
        {
            BottomLabel = bottomLabel; 
            ColumnHighlight = columnHighlight;    
        }
    }
}
