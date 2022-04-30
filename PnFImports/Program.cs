using Microsoft.Data.SqlClient;
using PnFData.Model;
using System.Data;
using System.Globalization;

namespace PnFImports
{
    internal partial class PnFImports
    {
        private static int _LastReturnValue = -1;

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "shares":
                        PnFImports.ImportShares();
                        break;

                    case "history":
                        PnFImports.ImportEodHistoricPrices();
                        break;

                    case "daily":
                        if (args.Length > 1)
                        {
                            PnFImports.ImportEodDailyPrices(args[1].ToUpper());
                        }
                        else
                        {
                            PnFImports.ImportEodDailyPrices(null);
                        }
                        break;

                    case "hilocharts":
                        GenerateAllHiLoCharts();
                        break;

                    case "sharerscharts":
                        GenerateShareRSCharts();
                        break;

                    case "indexcharts":
                        GenerateIndexCharts();
                        break;

                    case "indexrscharts":
                        GenerateIndexRSCharts();
                        break;

                    case "indexpercentcharts":
                        GenerateIndexPercentCharts();
                        break;

                    case "fullrun":
                        FullRun();
                        break;

                    case "hilochart":
                        if (args.Length > 2)
                        {
                            PnFImports.GenerateHiLoChart(args[1].ToUpper(),
                                DateTime.ParseExact(args[2], "yyyy-MM-dd",
                                    CultureInfo.InvariantCulture, DateTimeStyles.None));
                        }
                        break;

                }
            }

            Console.WriteLine("Completed. Press a key to exit.");
            Console.ReadKey();

        }

        internal static void FullRun()
        {
            Console.WriteLine("Starting full run ...");
            _LastReturnValue = 0;
            ImportEodDailyPrices(null);

            if (_LastReturnValue == 0)
            {
                // Generate values;
                RunLongStoredProcedure("uspGenerateDailyValues", 60);
            }


            if (_LastReturnValue == 0)
            {
                // Generate charts (HiLo and ShareRS may have concurrency issues so process separately)
                Task hiLoCharts = Task.Run(() => GenerateAllHiLoCharts());
                Task indexRsCharts = Task.Run(() => GenerateIndexRSCharts());
                Task indexCharts = Task.Run(() => GenerateIndexCharts());
                Task.WaitAll(new Task[] { hiLoCharts, indexCharts, indexRsCharts });
                Task.WaitAll(Task.Run(() => GenerateShareRSCharts()));

            }

            if (_LastReturnValue == 0)
            {
                // Generate SIB Indicators
                RunLongStoredProcedure("uspUpdateSIBIndicators", 60);
            }

            if (_LastReturnValue == 0)
            {
                // Generate index percent Charts
                GenerateIndexPercentCharts();
            }

            if (_LastReturnValue == 0)
            {
                // Generate the bullish percent and other percentage stats
                RunLongStoredProcedure("uspUpdateIndexPercentIndicators", 30);
            }

            if (_LastReturnValue == 0)
            {
                Console.WriteLine("Full run completed OK.");
            }
            else
            {
                Console.WriteLine("Error! Full run failed.");
            }
        }


        #region Helper methods ...

        private static DateTime PreviousWorkDay(DateTime date)
        {
            do
            {
                date = date.AddDays(-1);
            } while (IsWeekend(date));

            return date;
        }

        private static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ||
                   date.DayOfWeek == DayOfWeek.Sunday;
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
                Console.WriteLine($"Problem with executing {storedProcedure}! - [{ ex.Message}]");
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
                Console.WriteLine($"Received {e.Errors.Count} messages");
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
