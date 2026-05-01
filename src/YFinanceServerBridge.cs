using System;
using System.Threading.Tasks;

namespace AIA.YFinanceServer
{
    public class YFinanceServerBridge
    {
        private string _endpoint;

        public YFinanceServerBridge(string endpoint = "http://localhost:8080")
        {
            _endpoint = endpoint;
        }

        //Check it is alive
        public static async Task<bool> AliveAsync()
        {
            //Run api request to the /alive endpoint to confirm it gives back 200 OK
        }

        //Quote one stock
        public static async Task<Quote> QuoteAsync(string symbol)
        {
            //Run api request to /quote endpoint with that one symbol to and return Quote
        }

        //Quote multiple stocks
        public static async Task<Quote[]> QuoteMultipleAsync(params string[] symbols)
        {
            //Run api request to /quote endpoint for request of multiple stocks and return multiple Quote[]
            //If one of them did not come back (there was an issue), just don't return it (omit it)
        }

        

    }

    public class Quote
    {
        public string Symbol {get; set;}
        public float Price {get; set;}
        public float Change {get; set;}
        public float ChangePercent {get; set;}

        public Quote()
        {
            Symbol = "";
        }
    }
}