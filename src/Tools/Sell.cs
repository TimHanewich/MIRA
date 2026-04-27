using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimHanewich.AgentFramework;
using TimHanewich.Foundry.OpenAI.Responses;
using Yahoo.Finance;

namespace AIA
{
    public class Sell : ExecutableFunction
    {

        public State UseState {get; set;}

        public Sell(State use_state)
        {
            Name = "sell";
            Description = "Sell a quantity of a particular security.";
            InputParameters.Add(new FunctionInputParameter("symbol", "The symbol of the security to sell, for example 'PG' or 'MSFT' or 'WMT'"));
            InputParameters.Add(new FunctionInputParameter("quantity", "The quantity of the security to sell (i.e. shares)", "integer"));
        
            UseState = use_state;        
        }

        public override async Task<string> ExecuteAsync(JObject? arguments = null)
        {
            if (arguments == null)
            {
                return "Must provide arguments.";
            }

            //Get symbol
            string symbol = "";
            JProperty? prop_symbol = arguments.Property("symbol");
            if (prop_symbol == null)
            {
                return "You must provide the 'symbol' parameter.";
            }
            symbol = prop_symbol.Value.ToString();

            //Get quantity
            int quantity = 0;
            JProperty? prop_quantity = arguments.Property("quantity");
            if (prop_quantity == null)
            {
                return "You must provide the 'quantity' parameter.";
            }

            //try converting quantity
            try
            {
                quantity = Convert.ToInt32(prop_quantity.Value.ToString());
            }
            catch (Exception ex)
            {
                return "Provided value for 'quantity' did not parse into a Int32: " + ex.Message;
            }
            
            
            //Sell
            try
            {
                await UseState.Portfolio.TradeAsync(symbol, quantity, TimHanewich.Investing.Simulation.TransactionType.Sell);
            }
            catch (Exception ex)
            {
                return "Error while selling " + quantity.ToString() + " of " + symbol + ": " + ex.Message;
            }

            //Say it was successful
            return "Sale of " + quantity.ToString() + " of " + symbol + " successful! Your portfolio, now: \n" + UseState.Portfolio.ToString();
        }


    }
}
