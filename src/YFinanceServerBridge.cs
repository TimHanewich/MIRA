using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AIA.YFinanceServer
{
    public class YFinanceServerBridge
    {
        private string _endpoint;
        private static HttpClient _client = new HttpClient();

        public YFinanceServerBridge(string endpoint = "http://localhost:8080")
        {
            _endpoint = endpoint;
        }

        //Check it is alive
        public async Task<bool> AliveAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(_endpoint + "/alive");
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }

        //Quote one stock
        public async Task<Quote> QuoteAsync(string symbol)
        {
            HttpResponseMessage response = await _client.GetAsync(_endpoint + "/quote/" + symbol);
            response.EnsureSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();
            JObject jo = JObject.Parse(body);

            Quote q = new Quote();
            q.Symbol = symbol.ToUpper();
            q.Price = jo.Value<float>("price");
            q.Change = jo.Value<float>("change");
            q.ChangePercent = jo.Value<float>("changePercent");
            return q;
        }

        //Quote multiple stocks
        public async Task<Quote[]> QuoteMultipleAsync(params string[] symbols)
        {
            string joined = string.Join(",", symbols);
            HttpResponseMessage response = await _client.GetAsync(_endpoint + "/quote/" + joined);
            response.EnsureSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();
            JObject jo = JObject.Parse(body);

            List<Quote> quotes = new List<Quote>();
            foreach (JProperty prop in jo.Properties())
            {
                //If one of them did not come back (there was an issue), just don't return it (omit it)
                if (prop.Value.Type != JTokenType.Object)
                {
                    continue;
                }

                JObject qo = (JObject)prop.Value;
                Quote q = new Quote();
                q.Symbol = prop.Name.ToUpper();
                q.Price = qo.Value<float>("price");
                q.Change = qo.Value<float>("change");
                q.ChangePercent = qo.Value<float>("changePercent");
                quotes.Add(q);
            }

            return quotes.ToArray();
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