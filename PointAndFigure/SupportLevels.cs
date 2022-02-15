using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PointAndFigure
{

    public enum levelType
    {
        [Description("Support")]
        Support, 
        [Description("Resistance")]
        Resistance
    }

    /**
 * support or resistance levels <br>
 * maybe a bit of overkill
 * 
 */
    public class SupportLevels
    {

        public levelType myType { get; set; }
        List<string> datesOfLevel;
        int dateHits;
        double levelValue = 0;
        List<PAFColumn> myColumns;

        public SupportLevels(levelType inLevelType, List<PAFColumn> inColumns)
        {

            myType = inLevelType;
            myColumns = inColumns;

            datesOfLevel = new List<string>();

            if (myType == levelType.Support)
                findSupport();
            else
                findResistance();

        }

        public void findSupport()
        {

            double last = double.MaxValue;
            dateHits = 0;
            int i;
            for (i = myColumns.Count() - 1; i > -1; i--)
            {
                PAFColumn pafc = myColumns[i];
                if (pafc.myType == paftype.X)
                    continue;
                if (pafc.getStartBox() > last)
                    continue;
                if (pafc.getStartBox() == last)
                {
                    datesOfLevel.Add(pafc.startDate);
                    dateHits++;
                    continue;
                }
                if (dateHits > 1) // we found the most recent so stop
                {
                    break;
                }
                last = pafc.getStartBox();
                datesOfLevel = new List<string>();
                datesOfLevel.Add(pafc.startDate);
                dateHits = 1;
            }

            levelValue = last;
        }

        public void findResistance()
        {

            double last = -Double.MaxValue;
            int dateHits = 0;
            int i;
            for (i = myColumns.Count - 1; i > -1; i--)
            {
                PAFColumn pafc = myColumns[i];

                if (pafc.myType == paftype.O)
                    continue;
                if (pafc.getCurrentBox() < last)
                    continue;
                if (pafc.getCurrentBox() == last)
                {
                    datesOfLevel.Add(pafc.startDate);
                    dateHits++;
                    continue;
                }
                if (dateHits > 1) // we found the most recent so stop
                {
                    break;
                }
                datesOfLevel = new List<string>();
                datesOfLevel.Add(pafc.endDate);
                dateHits = 1;
            }
            levelValue = last;

        }

        public string Tostring()
        {
            if (datesOfLevel.Count < 2)
            {
                return $"{myType} none found";
            }
            return $"{myType} {datesOfLevel} value is {levelValue}";
        }

    }

}
