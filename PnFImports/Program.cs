using Microsoft.Data.SqlClient;
using PnFData.Interfaces;
using PnFData.Model;
using PnFData.Services;
using System.Data;
using System.Globalization;

namespace PnFImports
{
    internal partial class PnFImports
    {
        private static object _progressLock = new object();
        private static double _progress;
        private static double _total;

        private static int _LastReturnValue = -1;
        private static DateTime _runStart;

        public static void Main(string[] args)
        {
            _runStart = DateTime.Now;
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "importshares":
                        PnFImports.ImportShares(args[1]);
                        break;

                    case "history":
                        if (args.Length > 1)
                        {
                            if (args.Length > 2)
                            {
                                if (args[2].ToUpper() == "RETRYERRORS")
                                {
                                    PnFImports.ImportEodDailyPrices(args[1].ToUpper(), null, true, true);
                                }
                                else
                                {
                                    PnFImports.ImportEodDailyPrices(args[1].ToUpper(), args[2].ToUpper(), false, true);
                                }
                            }
                            else
                            {
                                PnFImports.ImportEodDailyPrices(args[1].ToUpper(), null, false, true);
                            }
                        }
                        else
                        {
                            PnFImports.ImportEodDailyPrices("LSE", null, false, true);
                        }
                        break;

                    case "daily":
                        if (args.Length > 1)
                        {
                            if (args.Length > 2)
                            {
                                if (args[2].ToUpper() == "RETRYERRORS")
                                {
                                    PnFImports.ImportEodDailyPrices(args[1].ToUpper(), null, true);
                                }
                                else
                                {
                                    PnFImports.ImportEodDailyPrices(args[1].ToUpper(), args[2].ToUpper());
                                }
                            }
                            else
                            {
                                PnFImports.ImportEodDailyPrices(args[1].ToUpper(), null);
                            }
                        }
                        else
                        {
                            PnFImports.ImportEodDailyPrices("LSE", null);
                        }
                        break;

                    case "hilocharts":
                        GenerateAllHiLoCharts(args[1].ToUpper(), DateTime.Now.Date);
                        break;

                    case "sharerscharts":
                        GenerateShareRSCharts(args[1].ToUpper(), DateTime.Now.Date);
                        break;

                    case "sharerschart":
                        GenerateShareRSChart(args[1].ToUpper(), args[2].ToUpper(), DateTime.Now.Date);
                        break;

                    case "indexcharts":
                        GenerateIndexCharts(args[1].ToUpper(), DateTime.Now.Date);
                        break;

                    case "indexrscharts":
                        GenerateIndexRSCharts(args[1].ToUpper(), DateTime.Now.Date);
                        break;

                    case "indexpercentcharts":
                        GenerateIndexPercentCharts(args[1].ToUpper(), DateTime.Now.Date);
                        break;

                    case "fullrun":
                        if (args.Length > 2)
                        {
                            FullRun(args[1].ToUpper(), args[2].ToUpper());
                        }
                        else
                        {
                            FullRun(args[1].ToUpper(), null);
                        }
                        break;

                    case "hilochart":
                        if (args.Length > 2)
                        {
                            PnFImports.GenerateHiLoChart(args[1].ToUpper(),
                                DateTime.ParseExact(args[2], "yyyy-MM-dd",
                                    CultureInfo.InvariantCulture, DateTimeStyles.None));
                        }
                        break;

                    case "lastreliabledate":
                        DateTime test = GetLastReliableDate("ALL");
                        Console.WriteLine($"Last reliable date = {test}");
                        break;

                    case "test":
                        Guid indexId = new Guid("922EC9E9-31A1-42E9-A559-08AD9F69925B");
                        IEnumerable<IndexValue> rawTickData = null;
                        using (PnFDataContext db = new PnFDataContext())
                        {
                            rawTickData = db.IndexValues.Where(i => i.IndexId == indexId).ToList();
                        }

                        // Generate values;
                        List<IDayValue> tickData = rawTickData.Where(r => r.PercentRsRising.HasValue).Select(r => new SimpleDayValue()
                        {
                            Day = r.Day,
                            Value = r.PercentRsRising.Value
                        }
                        ).ToList<IDayValue>();

                        GeneratePercentChart(indexId, "Test", new DateTime(2022, 07, 20), tickData, PnFChartSource.IndexPercentShareRsX);
                        break;

                }
            }
            Console.WriteLine($"Started at {_runStart:g}, completed at {DateTime.Now:g}. Press a key to exit.");
            Console.ReadKey();

        }

        internal static void FullRun(string exchangeCode, string? parameter)
        {
            //DateTime now = DateTime.Now.Date;

            _LastReturnValue = 0;
            if (parameter != "SKIPPRICES")
            {
                if (parameter == "RETRYERRORS")
                {
                    ImportEodDailyPrices(exchangeCode, null, true);
                }
                else if (parameter == "HISTORIC")
                {
                    ImportEodDailyPrices(exchangeCode, null, false, true);
                }
                else
                {
                    ImportEodDailyPrices(exchangeCode, null);
                    // Second pass to retry any errors
                    ImportEodDailyPrices(exchangeCode, null, true);

                }
            }

            // Determine the last reliable date (T+2 settlement etc)
            DateTime TplusTwoDate = GetLastReliableDate(exchangeCode);

            if (_LastReturnValue == 0)
            {
                // Fill in any blanks using the last non-null values.
                RunLongStoredProcedure("uspApproximateMissingPriceData", 60);
            }

            if (_LastReturnValue == 0)
            {
                // Delete the existing charts in case indexes are updated affecting RS values.
                RunLongStoredProcedure("uspDeleteAllCharts", 60);
            }

            if (_LastReturnValue == 0)
            {
                // Generate values;
                RunLongStoredProcedure("uspGenerateDailyValues", TplusTwoDate, 60);
            }

            if (_LastReturnValue == 0)
            {
                // Only use data upto the T+2 date
                GenerateAllHiLoCharts(exchangeCode, TplusTwoDate);
                GenerateIndexCharts(exchangeCode, TplusTwoDate);
                GenerateIndexRSCharts(exchangeCode, TplusTwoDate);
                GenerateShareRSCharts(exchangeCode, TplusTwoDate);

            }

            if (_LastReturnValue == 0)
            {
                // Generate SIB Indicators
                RunLongStoredProcedure("uspUpdateSIBIndicators", TplusTwoDate, 60);
            }

            if (_LastReturnValue == 0)
            {
                // Generate index percent Charts
                GenerateIndexPercentCharts(exchangeCode, TplusTwoDate);
            }

            if (_LastReturnValue == 0)
            {
                // Generate the bullish percent and other percentage stats
                RunLongStoredProcedure("uspUpdateIndexPercentIndicators", 30);
            }

            if (_LastReturnValue == 0)
            {
                Console.WriteLine($"\nFull run for ({exchangeCode}) completed OK.");
                Console.WriteLine($"T+2 day used was {TplusTwoDate:d}");
            }
            else
            {
                Console.WriteLine($"Error! Full run ({exchangeCode}) failed @.");
            }
        }

        #region Helper methods ...
        private static void UpdateProgress()
        {
            lock (_progressLock)
            {
                _progress += 1.0;
                Console.Write($"Completed ({_progress / _total * 100.0:N2} %) ...\r");
            }
        }

        private static DateTime PreviousWorkDay(DateTime date, int howManyDaysBack = 1)
        {
            for (int i = 0; i < howManyDaysBack; i++)
            {
                do
                {
                    date = date.AddDays(-1);
                } while (IsWeekend(date));
            }
            return date;
        }

        private static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ||
                   date.DayOfWeek == DayOfWeek.Sunday;
        }

        private static DateTime GetLastReliableDate(string exchangeCode)
        {
            DateTime reliableDate = PreviousWorkDay(DateTime.Now, 2);
            List<DateTime?> days;
            DateTime cutOff = DateTime.Now.AddDays(-10);
            using (PnFDataContext db = new PnFDataContext())
            {
                if (exchangeCode == "ALL")
                {
                    days = db.Shares
                        .Where(s => s.LastEodDate > cutOff)
                        .Select(s => s.LastEodDate)
                        .Distinct()
                        .OrderByDescending(s => s!.Value)
                        .ToList();
                }
                else
                {
                    days = db.Shares
                        .Where(s => s.ExchangeCode == exchangeCode && s.LastEodDate > cutOff)
                        .Select(s => s.LastEodDate)
                        .Distinct()
                        .OrderByDescending(s => s!.Value)
                        .ToList();
                }
            }
            if (days.Count() > 2)
            {
                if (days[2] != null)
                {
                    reliableDate = days[2].Value;
                }
            }
            return reliableDate.Date;
        }

        #endregion

        #region Testing long running calls ...
        private static readonly ManualResetEvent _reset = new ManualResetEvent(false);
        private static SqlParameter _returnParameter;

        internal static void RunLongStoredProcedure(string storedProcedure, int connectionTimeout)
        {
            _reset.Reset();
            try
            {
                using (SqlConnection conn = new SqlConnection(PnFDataContext.ConnectionString))
                using (SqlCommand command = new SqlCommand(storedProcedure, conn))
                {
                    _returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    _returnParameter.Direction = ParameterDirection.ReturnValue;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandTimeout = connectionTimeout;   // Default to one minute
                    command.Connection.FireInfoMessageEventOnUserErrors = true;
                    command.Connection.InfoMessage += ConnectionInfoMessage;
                    AsyncCallback runResult = new AsyncCallback(NonQueryCallBack);
                    command.Connection.Open();
                    command.BeginExecuteNonQuery(runResult, command);
                    Console.WriteLine($"Waiting for completion of {storedProcedure} ....");
                    _reset.WaitOne();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Problem with executing {storedProcedure}! - [{ex.Message}]");
            }
            if (_LastReturnValue == 0)
            {
                Console.WriteLine($"{storedProcedure} Completed successfully");
            }
            else
            {
                Console.WriteLine($"Error! {storedProcedure} failed.");
            }
        }

        internal static void RunLongStoredProcedure(string storedProcedure, DateTime cutOffDate, int connectionTimeout)
        {
            _reset.Reset();
            try
            {
                using (SqlConnection conn = new SqlConnection(PnFDataContext.ConnectionString))
                using (SqlCommand command = new SqlCommand(storedProcedure, conn))
                {
                    var cutoffParameter = command.Parameters.Add("CutOffDate", SqlDbType.Date);
                    cutoffParameter.Value = cutOffDate;
                    _returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    _returnParameter.Direction = ParameterDirection.ReturnValue;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandTimeout = connectionTimeout;   // Default to one minute
                    command.Connection.FireInfoMessageEventOnUserErrors = true;
                    command.Connection.InfoMessage += ConnectionInfoMessage;
                    AsyncCallback runResult = new AsyncCallback(NonQueryCallBack);
                    command.Connection.Open();
                    command.BeginExecuteNonQuery(runResult, command);
                    Console.WriteLine($"Waiting for completion of {storedProcedure} ....");
                    _reset.WaitOne();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Problem with executing {storedProcedure}! - [{ex.Message}]");
            }
            if (_LastReturnValue == 0)
            {
                Console.WriteLine($"{storedProcedure} Completed successfully");
            }
            else
            {
                Console.WriteLine($"Error! {storedProcedure} failed.");
            }
        }

        private static void ConnectionInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            if (e.Errors.Count > 0)
            {
                foreach (SqlError info in e.Errors)
                {
                    if (info.Class > 9) // Severity
                    {
                        Console.WriteLine($"Error Message : {info.Message} : State : {info.State}");
                    }
                    else
                    {
                        Console.WriteLine(info.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void NonQueryCallBack(IAsyncResult result)
        {
            SqlCommand command = (SqlCommand)result.AsyncState;
            try
            {
                if (command != null)
                {
                    Console.WriteLine($"Waiting for completion of the Async call, result = {command.EndExecuteNonQuery(result)}");
                    Console.WriteLine($"ReturnParameter = {_returnParameter.Value}");
                    _LastReturnValue = (int)_returnParameter.Value;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Problem with executing command! - [{0}]", ex.Message);
            }
            finally
            {
                Console.WriteLine("Completed call back so signal main thread to continue....");
                _reset.Set();
            }
        }
        #endregion
    }
}
