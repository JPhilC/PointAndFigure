# PointAndFigure
Point and Figure Charting inspired by Thomas J Dorsey

My attempt to create equivalents of the bullish percent and relative strength data outlined in the book "Point & Figure Charting" by Thomas J Dorsey.
While Dorsey used US stocks and indexes I am initially concentrating on LSE stocks and building my own index data with which to ultimately build
P & F charts and automate signal notifications.

The code uses:

SQL Server (or SQL Server Express) to hold data
An export from SharePad (tm) to get a list of the London shares.
AlphaVantage to download historic and daily eod prices.

You will need:

1> SQL Server Developer Edition (https://www.microsoft.com/en-GB/sql-server/sql-server-downloads) 
2> Visual Studio Community with C# workspace (https://visualstudio.microsoft.com/downloads/)
3> A subscription to SharePad to allow you to export the Share/Company data used. (https://www.sharescope.co.uk/sharepad.jsp)
4> An Alpha Vantage API key allowing unlimited API calls a day. The can be obtained my joining the Alpha Tournament (https://www.alphatournament.com/)

You should enter you AlphaVantage API key in the file AlphaVantageService.cs. Look for the "demo" in that file.

16/02/2022
The main project is PnFImports which currently allows you to import either share information, 5 years of OLHCV for each share or a daily update of the OLHCV data to bring it up to the close on the previous working day. There are also a couple or T-SQL scripts to create stored procedures which generate market (LSE Full and AIM) priced weighed index data and equal weighted Sector index data.

04/04/2022
PnFImports now has an argument 'fullrun' which will download end of day prices from Alpha Vantage and then generate the stats and charts automatically. This needs
several hours to run using the free Alpha Vantage API so it is best run as a scheduled task starting around 2am UK time. Several new stored procedures have been
added to the database. The SQL scripts to create them are in the SQLScripts folder. Had to move from SQL Server Express to the free Developer Version as the data
generated for the LSE now exceeds the 10 Gb limit.

The UI project is not really started yet. Instead there are two stored procedures uspGetMarketAndSectorStats and uspGetSectorShareStats that return most of the 
statistics used and referred to by Dorsey when evaluating the market and sectors. I still need to code the "Positive Trend", the "High Low Index" and the "Advance-Decline Line".
