using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimHanewich.AgentFramework;
using TimHanewich.Foundry.OpenAI.Responses;
using Yahoo.Finance;

namespace AIA
{
    public class Quote : ExecutableFunction
    {
        public Quote()
        {
            Name = "quote";
            Description = "Get price information for a particular stock, ETF, or other security.";
            InputParameters.Add(new FunctionInputParameter("symbol", "The symbol of the security to quote, for example 'PG' or 'MSFT' or 'WMT'"));
        }

        public override async Task<string> ExecuteAsync(JObject? arguments = null)
        {
            //No args?
            if (arguments == null)
            {
                return "Must provide arguments.";
            }

            //Get symbol
            string symbol = "";
            JProperty? prop_symbol = arguments.Property("symbol");
            if (prop_symbol == null)
            {
                return "Must provide 'symbol' argument.";
            }
            symbol = prop_symbol.Value.ToString();

            //Get quote
            Equity e = Equity.Create(symbol);

            //Try to download summary data
            try
            {
                await e.DownloadSummaryAsync();
                return JsonConvert.SerializeObject(e.Summary, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return "Error while quoting '" + symbol + "': " + ex.Message;
            }
        }


    }
}