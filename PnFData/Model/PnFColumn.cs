using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

            string? mthIndicator = monthIndicator;  // Internal copy as we only want to use it once.

            // Subsequent boxes so may need to add more than one to get to the specified index.
            if (ColumnType == PnFColumnType.O)
            {
                // Falling
                while (CurrentBoxIndex > index)
                {
                    CurrentBoxIndex--;
                    AddBoxInternal(boxType, boxSize, CurrentBoxIndex, value, day, mthIndicator);
                    mthIndicator = null;
                }
            }
            else
            {
                // Rising
                while (CurrentBoxIndex < index)
                {
                    CurrentBoxIndex++;
                    AddBoxInternal(boxType, boxSize, CurrentBoxIndex, value, day, mthIndicator);
                    mthIndicator = null;    // Don't show month indicator multiple times
                }
            }

            EndAt = day;

        }

        private void AddBoxInternal(PnFBoxType boxType, double boxSize, int index, double value, DateTime day, string? monthIndicator = null)
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

            if (monthIndicator != null)
            {
                currentBox.MonthIndicator = monthIndicator;
            }

        }

    }
}
