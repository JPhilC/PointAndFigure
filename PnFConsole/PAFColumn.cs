using System.Collections.Generic;

namespace PnFConsole
{
    public enum Paftype
    {
        O,
        X
    }

    /**
	 * a column of o's or x's
	 * 
	 */
    public class PafColumn
    {
        public Paftype MyType { get; set; }
        public int StartAt { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int StopAt { get; set; }
        public double Volume { get; set; }
        public double[] Boxes { get; set; }

        private int _currentBoxPosition;

        private static int _lastCurrentBoxPosition = -1;

        /** used to track months change within a column */
        private static char _latestMonth = ' ';
        public Dictionary<int, string> MonthIndicators { get; private set; }= new Dictionary<int, string>();

        public static void ResetLastCurrentBoxPosition()
        {
            _lastCurrentBoxPosition = -1;
        }

        public PafColumn(Paftype pt, double inWhere, string startDate, double[] inboxes)
        {
            MyType = pt;
            Boxes = inboxes;
            this.StartDate = startDate;

            if (MyType == Paftype.O)
            {
                if (_lastCurrentBoxPosition == -1)
                {
                    for (_currentBoxPosition = 0; inWhere > GetCurrentBox(); _currentBoxPosition++)
                    {
                    }
                }
                else
                    _currentBoxPosition = _lastCurrentBoxPosition;

                _lastCurrentBoxPosition = --_currentBoxPosition;
            }
            else
            { // ==paftype.X
                if (_lastCurrentBoxPosition == -1)
                {
                    for (_currentBoxPosition = Boxes.Length - 1; inWhere < GetCurrentBox() + 1
                            & GetCurrentBox() > 0; _currentBoxPosition--)
                    {
                    }
                }
                else
                    _currentBoxPosition = _lastCurrentBoxPosition;

                _lastCurrentBoxPosition = ++_currentBoxPosition;
            }

            StartAt = _lastCurrentBoxPosition;
            StopAt = StartAt;
            AddToColumn(inWhere, startDate);

        }

        public double GetCurrentBox()
        {
            if (_currentBoxPosition < 0)
            {
                return 0d;
            }

            if (_currentBoxPosition >= Boxes.Length)
            {
                return Boxes[Boxes.Length - 2];
            }

            return Boxes[_currentBoxPosition];
        }

        public double GetStartBox()
        {

            return Boxes[StartAt];
        }

        public void AddToColumn(double toWhere, string currentDate)
        {
            if (currentDate.StartsWith("D") == false)
            { // don't do this for mock data
                char m1 = currentDate[5];
                char m2 = currentDate[6];
                if (m1 == '1')
                {
                    if (m2 == '0')
                        m2 = 'A';
                    if (m2 == '1')
                        m2 = 'B';
                    if (m2 == '2')
                        m2 = 'C';
                }
                if (m2 != _latestMonth)
                {
                    _latestMonth = m2;
                    if (MonthIndicators.ContainsKey(_currentBoxPosition))
                    {
                        MonthIndicators[_currentBoxPosition] = _latestMonth.ToString();
                    }
                    else
                    {
                        MonthIndicators.Add(_currentBoxPosition, _latestMonth.ToString());
                    }
                }
            }
            if (MyType == Paftype.O)
            {
                bool check = false;
                for (; GetCurrentBox() > toWhere; _currentBoxPosition--)
                {
                    check = true;
                    // otherwise just move along;
                }
                if (GetCurrentBox() < toWhere && check)
                    _currentBoxPosition++;
            }
            else
            {
                bool check = false;
                for (; GetCurrentBox() < toWhere; _currentBoxPosition++)
                {
                    check = true;
                }
                if (GetCurrentBox() > toWhere && check)
                    _currentBoxPosition++;

            }
            StopAt = _currentBoxPosition;
            _lastCurrentBoxPosition = _currentBoxPosition;
            EndDate = currentDate;
        }

        public string GetStartDate()
        {
            return StartDate;
        }

        public string GetEndDate()
        {
            return EndDate;
        }

        public void SetEndDate(string endDate)
        {
            this.EndDate = endDate;
        }

        public override string ToString()
        {

            if (StartAt < 0)
                StartAt = 0;

            if (StartAt >= Boxes.Length)
                StartAt = Boxes.Length - 1;

            if (StopAt >= Boxes.Length)
                StopAt = Boxes.Length - 1;

            return MyType + " " + StartDate + " " + EndDate + " startAt:" + Boxes[StartAt] + " endAt:"
                    + Boxes[StopAt];

        }

        public static void ResetLatestMonth()
        {
            _latestMonth = ' ';

        }

        public new Paftype GetType()
        {
            return MyType;
        }

        public double GetBoxNearCurrentBox(int diff)
        {
            return _currentBoxPosition + diff < 0 ? 0 : Boxes[_currentBoxPosition + diff];
        }

    }
}
