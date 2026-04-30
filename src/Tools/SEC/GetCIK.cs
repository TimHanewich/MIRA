using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimHanewich.AgentFramework;
using TimHanewich.Foundry.OpenAI.Responses;
using SecuritiesExchangeCommission.Edgar;

namespace AIA
{
    public class GetCIK : ExecutableFunction
    {
        public GetCIK()
        {
            Name = "get_cik";
            Description = "Get the SEC's CIK (Central Index Key) for a company based on its stock symbol.";
            InputParameters.Add(new FunctionInputParameter("symbol", "Stock symbol, i.e. 'MSFT'"));
        }

        public override async Task<string> ExecuteAsync(JObject? arguments)
        {
            if (arguments == null)
            {
                return "Must provide arguments.";
            }

            //Get symbol
            JProperty? prop_symbol = arguments.Property("symbol");
            if (prop_symbol == null)
            {
                return "Argument 'symbol' not provided.";
            }
            string symbol = prop_symbol.Value.ToString();

            try
            {
                string cik = await SecToolkit.GetCompanyCikFromTradingSymbolAsync(symbol);
                return "CIK for '" + symbol.Trim().ToUpper() + "': " + cik;
            }
            catch (Exception bex)
            {
                return "Failure while determining CIK for '" + symbol.Trim().ToUpper() + "': " + bex.Message;
            }
        }
    }
}