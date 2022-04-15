using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace PnFData.Model
{
    public enum PnFColumnType
    {
        O,      // Going down
        X,      // Going up
        XO,     // Special case of 1 box reversal charts (going down after one box reversal up)
        OX      // Special case for 1 box reversal charts (going up after a one box reversal down)
    }

    public class PnFColumn : EntityData
    {
        [Required]
        public Guid PnFChartId { get; set; }

        [ForeignKey("PnFChartId")]
        public PnFChart PnFChart { get; set; }

        public PnFColumnType ColumnType { get; set; }

        /// <summary>
        /// The horizontal position of this column in the chart
        /// </summary>
        public int Index { get; set; }

        public int CurrentBoxIndex { get; set; } = 0;

        public double Volume { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        public int BullSupportIndex {get; set;}

        public bool ShowBullishSupport {get;set;}

        public bool ContainsNewYear { get; set; }

        public List<PnFBox> Boxes { get; } = new List<PnFBox>();

        [NotMapped]
        public int StartAtIndex
        {
            get
            {
                if (ColumnType == PnFColumnType.O
                    || ColumnType == PnFColumnType.XO)
                {
                    // Going down.
                    return Boxes.Max(b => b.Index);
                }
                else
                {
                    // Going up.
                    return Boxes.Min(b => b.Index);
                }
            }
        }

        [NotMapped]
        public int EndAtIndex
        {
            get
            {
                if (ColumnType == PnFColumnType.O
                    || ColumnType == PnFColumnType.XO)
                {
                    // Going down.
                    return Boxes.Min(b => b.Index);
                }
                else
                {
                    // Going up.
                    return Boxes.Max(b => b.Index);
                }
            }
        }


        public void AddBox(PnFBoxType boxType, double boxSize, int index, double value, DateTime day, string? monthIndicator = null)
        {
            if (Boxes.Count == 0)
            {
                StartAt = day;
            }

            // Subsequent boxes so may need to add more than one to get to the specified index.
            if (ColumnType == PnFColumnType.O)
            {
                // Falling
                while (CurrentBoxIndex > index)
                {
                    CurrentBoxIndex--;
                    AddBoxInternal(boxType, boxSize, CurrentBoxIndex, value, day);
                }

                // Push the bullish support index down if necessary.
                if (index <= BullSupportIndex)
                {
                    BullSupportIndex = index-1;
                    ShowBullishSupport = false;
                }
            }
            else
            {
                // Rising
                while (CurrentBoxIndex < index)
                {
                    CurrentBoxIndex++;
                    AddBoxInternal(boxType, boxSize, CurrentBoxIndex, value, day);
                }
            }
            Boxes[Boxes.Count - 1].MonthIndicator = monthIndicator;
            EndAt = day;

        }

        private void AddBoxInternal(PnFBoxType boxType, double boxSize, int index, double value, DateTime day)
        {
            PnFBox currentBox = new PnFBox()
            {
                BoxType = boxType,
                Size = boxSize,
                Index = index,
                Value = value,
                Ticked = day,
                Column = this
            };
            Boxes.Add(currentBox);
            //System.Diagnostics.Debug.Write(boxType.ToString());
        }

        public string GetTooltip()
        {
            if (this.PnFChart.BoxSize == null) return "";
            int boxCount = Boxes.Count;
            double boxSize = this.PnFChart.BoxSize.Value;
            string columnType = "";
            switch (ColumnType)
            {
                case PnFColumnType.O:
                case PnFColumnType.XO:
                    columnType = "O";
                    break;
                case PnFColumnType.X:
                case PnFColumnType.OX:
                    columnType = "X";
                    break;
            }
            if (EndAt != null && boxCount > 0)
            {
                return
                    $"{boxCount} {columnType}'s\nFrom {StartAtIndex * boxSize} on {StartAt.Date:d}\nTo {EndAtIndex * boxSize} on {EndAt.Value.Date:d}";
            }
            else
            {
                return $"From {Boxes[0].Index * boxSize} on {StartAt.Date:d}";
            }
        }

    }
}
