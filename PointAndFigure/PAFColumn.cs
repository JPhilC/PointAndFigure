using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnFConsole
{
    public enum paftype
    {
        O, 
        X
    }

	/**
	 * a column of o's or x's
	 * 
	 */
	public class PAFColumn
	{
		public paftype myType { get; set; }
		int startAt;
		public string startDate { get; set; }
		public string endDate { get; set; }
		int stopAt;
		public double volume { get; set; }
		double[] boxes;
		int currentBoxPosition = -1;
		static int lastCurrentBoxPosition = -1;

		/** used to track months change within a column */
		static char latestMonth = ' ';
		Dictionary<int, string> monthIndicators = new Dictionary<int, string>();

		public static void resetLastCurrentBoxPosition()
		{
			lastCurrentBoxPosition = -1;
		}

		public PAFColumn(paftype pt, double inWhere, string startDate, double[] inboxes)
		{
			myType = pt;
			boxes = inboxes;
			this.startDate = startDate;

			if (myType == paftype.O)
			{
				if (lastCurrentBoxPosition == -1)
				{
					for (currentBoxPosition = 0; inWhere > getCurrentBox(); currentBoxPosition++)
						;
				}
				else
					currentBoxPosition = lastCurrentBoxPosition;

				lastCurrentBoxPosition = --currentBoxPosition;
			}
			else
			{ // ==paftype.X
				if (lastCurrentBoxPosition == -1)
				{
					for (currentBoxPosition = boxes.Length - 1; inWhere < getCurrentBox() + 1
							& getCurrentBox() > 0; currentBoxPosition--)
						;
				}
				else
					currentBoxPosition = lastCurrentBoxPosition;

				lastCurrentBoxPosition = ++currentBoxPosition;
			}

			startAt = lastCurrentBoxPosition;
			stopAt = startAt;
			addToColumn(inWhere, startDate);

		}

		string df = "##.##";

		public double getCurrentBox()
		{
			if (currentBoxPosition < 0)
				return 0d;
			if (currentBoxPosition >= boxes.Length)
				return boxes[boxes.Length - 2];
			return boxes[currentBoxPosition];
		}

		public double getStartBox()
		{

			return boxes[startAt];
		}

		public void addToColumn(double toWhere, string currentDate)
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
				if (m2 != latestMonth)
				{
					latestMonth = m2;
					monthIndicators.Add(currentBoxPosition, latestMonth.Tostring());
				}
			}
			if (myType == paftype.O)
			{
				bool check = false;
				for (; getCurrentBox() > toWhere; currentBoxPosition--)
				{
					check = true;
					// otherwise just move along;
				}
				if (getCurrentBox() < toWhere && check)
					currentBoxPosition++;
			}
			else
			{
				bool check = false;
				for (; getCurrentBox() < toWhere; currentBoxPosition++)
				{
					check = true;
				}
				if (getCurrentBox() > toWhere && check)
					currentBoxPosition++;

			}
			stopAt = currentBoxPosition;
			lastCurrentBoxPosition = currentBoxPosition;
			endDate = currentDate;
		}

		public string getStartDate()
		{
			return startDate;
		}

		public string getEndDate()
		{
			return endDate;
		}

		public void setEndDate(string endDate)
		{
			this.endDate = endDate;
		}

		public string Tostring()
		{

			if (startAt < 0)
				startAt = 0;

			if (startAt >= boxes.Length)
				startAt = boxes.Length - 1;

			if (stopAt >= boxes.Length)
				stopAt = boxes.Length - 1;

			return myType.Tostring() + " " + startDate + " " + endDate + " startAt:" + boxes[startAt] + " endAt:"
					+ boxes[stopAt];

		}

		public static void resetLatestMonth()
		{
			latestMonth = ' ';

		}

		public paftype getType()
		{
			return myType;
		}

		public double getBoxNearCurrentBox(int diff)
		{
			return currentBoxPosition + diff < 0 ? 0 : boxes[currentBoxPosition + diff];
		}

	}
}
