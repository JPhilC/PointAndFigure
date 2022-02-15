using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PnFConsole
{

    public enum LevelType
    {
        [Description("Support")]
        Support, 
        [Description("Resistance")]
        Resistance
    }

    public class SupportLevels
    {

        public LevelType MyType { get; set; }
        private List<string> _datesOfLevel;
        private int _dateHits;
        private double _levelValue;
        private readonly List<PafColumn> _myColumns;

        public SupportLevels(LevelType inLevelType, List<PafColumn> inColumns)
        {

            MyType = inLevelType;
            _myColumns = inColumns;

            _datesOfLevel = new List<string>();

            if (MyType == LevelType.Support)
                FindSupport();
            else
                FindResistance();

        }

        public void FindSupport()
        {

            double last = double.MaxValue;
            _dateHits = 0;
            int i;
            for (i = _myColumns.Count() - 1; i > -1; i--)
            {
                PafColumn pafc = _myColumns[i];
                if (pafc.MyType == Paftype.X)
                    continue;
                if (pafc.GetStartBox() > last)
                    continue;
                if (Math.Abs(pafc.GetStartBox() - last) < PointAndFigure.Tolerance)
                {
                    _datesOfLevel.Add(pafc.StartDate);
                    _dateHits++;
                    continue;
                }
                if (_dateHits > 1) // we found the most recent so stop
                {
                    break;
                }
                last = pafc.GetStartBox();
                _datesOfLevel = new List<string> { pafc.StartDate };
                _dateHits = 1;
            }

            _levelValue = last;
        }

        public void FindResistance()
        {

            double last = -Double.MaxValue;
            int dateHits = 0;
            int i;
            for (i = _myColumns.Count - 1; i > -1; i--)
            {
                PafColumn pafc = _myColumns[i];

                if (pafc.MyType == Paftype.O)
                    continue;
                if (pafc.GetCurrentBox() < last)
                    continue;
                if (Math.Abs(pafc.GetCurrentBox() - last) < PointAndFigure.Tolerance)
                {
                    _datesOfLevel.Add(pafc.StartDate);
                    dateHits++;
                    continue;
                }
                if (dateHits > 1) // we found the most recent so stop
                {
                    break;
                }
                _datesOfLevel = new List<string> { pafc.EndDate };
                dateHits = 1;
            }
            _levelValue = last;

        }

        public new string ToString()
        {
            if (_datesOfLevel.Count < 2)
            {
                return $"{MyType} none found";
            }

            StringBuilder sb = new StringBuilder();
            foreach (string date in _datesOfLevel)
            {
                if (sb.Length > 0) sb.Append(" - ");
                sb.Append(date);
            }
            
            return $"{MyType} {sb} value is {_levelValue}";
        }

    }

}
