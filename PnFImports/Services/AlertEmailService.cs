using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using PnFData;
using PnFData.Model;
using System.Text;

namespace PnFImports.Services
{
    public class AlertEmailService
    {

        static Dictionary<int, string> _filters = new Dictionary<int, string>()
        {
            {0x20000, "Switched to rising" },
            {0x40000, "Switched to falling" },
            {0x00001, "New buy signal" },
            {0x00010, "New sell signal" },
            {0x00002, "New triple top buy" },
            {0x00020, "New triple bottom sell" },
            {0x01000, "Breached Bull Support Line" },
            {0x08000, "Momentum turned positive" },
            {0x10000, "Momentum turned negative" },
            {0x00400, "Dropped Below EMA 10" },
            {0x00100, "Closed Above EMA 10" },
            {0x00800, "Dropped Below EMA 30" },
            {0x00200, "Closed Above EMA 30" },
            {0x00004, "New RS buy signal" },
            {0x00040, "New RS sell signal" },
            {0x00008, "New peer RS buy signal" },
            {0x00080, "New peer RS sell signal" },
            {0x02000, "New 52 Week high" },
            {0x04000, "New 52 Week low" },
        };

        public static async Task ProcessAlerts(IEnumerable<PortfolioEventResult> portfolioEvents)
        {
            string bodyText = GenerateBodyText(portfolioEvents);
            await SendEmail(bodyText);
        }


        private static string GenerateBodyText(IEnumerable<PortfolioEventResult> portfolioEvents)
        {
            StringBuilder sb = new StringBuilder();
            string currentPortfolio = "";
            bool tableOpen = false;
            foreach (var filter in _filters)
            {
                var matches = portfolioEvents.Where(e => (e.NewEvents & filter.Key) == filter.Key).OrderBy(e => e.Portfolio).ThenBy(e => e.Tidm).ToList();
                if (matches.Count > 0)
                {
                    currentPortfolio = "";
                    sb.AppendLine($"<h2>{filter.Value}</h2>");
                    foreach (var match in matches)
                    {
                        if (currentPortfolio != match.Portfolio)
                        {
                            if (tableOpen)
                            {
                                sb.AppendLine("</table>");
                                tableOpen = false;
                            }
                            currentPortfolio = match.Portfolio;
                            sb.AppendLine($"<h3>Portfolio: {currentPortfolio}</h3>");
                            // Add table headers
                            sb.AppendLine("<table border=\"1\"><tr><th>TIDM</th><th>Share name</th><th>Holdings</th><th>Value</th><th>Remarks</th></tr>");
                            tableOpen = true;
                        }

                        sb.AppendLine($"<tr><td>{match.Tidm}</td><td>{match.ShareName}</td><td>{match.Holding}</td><td>{match.Holding * match.AdjustedClose:F2}</td><td>{match.Remarks}</td></tr>");
                    }
                    if (tableOpen)
                    {
                        sb.AppendLine("</table>");
                        tableOpen = false;
                    }
                }
            }

            if (sb.Length > 0)
            {
                return sb.ToString();
            }
            else
            {
                return "There are now new events for the shares in you portfolios.";
            }

        }

        public static async Task SendEmail(string bodyText)
        {
            var test = AuthenticationSettings.Current.Config!.EmailServiceSmptServer;

            // create message
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(AuthenticationSettings.Current.Config!.AlertServiceEmailFrom));
            email.To.Add(MailboxAddress.Parse(AuthenticationSettings.Current.Config!.AlertServiceEmailTo));
            email.Subject = $"P&FImport new event notifications {DateTime.Now:f}";
            email.Body = new TextPart(TextFormat.Html) { Text = bodyText };

            // send email
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(AuthenticationSettings.Current.Config!.EmailServiceSmptServer, 
                AuthenticationSettings.Current.Config!.EmailServiceSmptPort, SecureSocketOptions.StartTlsWhenAvailable);
            await smtp.AuthenticateAsync(
                AuthenticationSettings.Current.Config!.EmailServiceUsername,
                AuthenticationSettings.Current.Config!.EmailServicePassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }


    }
}
