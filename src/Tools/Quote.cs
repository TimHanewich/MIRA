using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimHanewich.AgentFramework;
using TimHanewich.Foundry.OpenAI.Responses;
using MIRA.YFinanceServer;

namespace MIRA
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
            JProperty? prop_symbol = arguments.Property("symbol");
            if (prop_symbol == null)
            {
                return "Must provide 'symbol' argument.";
            }
            string symbol = prop_symbol.Value.ToString();

            //Try to download
            try
            {
                YFinanceServerBridge yfsb = new YFinanceServerBridge();
                MIRA.YFinanceServer.Quote quote = await yfsb.QuoteAsync(symbol);
                return JsonConvert.SerializeObject(quote, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return "Error while quoting '" + symbol + "': " + ex.Message;
            }
        }


    }
}