using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimHanewich.AgentFramework;
using TimHanewich.Foundry.OpenAI.Responses;
using SecuritiesExchangeCommission.Edgar;
using SecuritiesExchangeCommission.Edgar.Data;

namespace AIA
{
    public class SearchFinancialData : ExecutableFunction
    {
        private SECBandwidthManager _bwm;

        public SearchFinancialData(SECBandwidthManager bwm)
        {
            Name = "search_financial_data";
            Description = "Search through available XBRL facts the company has reported previously.";
            InputParameters.Add(new FunctionInputParameter("CIK", "The company's central index key (CIK), i.e. '1655210'", "number"));
            InputParameters.Add(new FunctionInputParameter("query", "The search query, i.e. 'revenue', 'assets'"));

            _bwm = bwm;
        }

        public override async Task<string> ExecuteAsync(JObject? arguments)
        {
            if (arguments == null)
            {
                return "Must provide arguments.";
            }

            //Get CIK
            JProperty? prop_cik = arguments.Property("CIK");
            if (prop_cik == null)
            {
                return "Argument 'CIK' not provided.";
            }

            //Try parse cik as int
            int cik;
            try
            {
                cik = Convert.ToInt32(prop_cik.Value.ToString());
            }
            catch
            {
                return "Provided parameter 'CIK' was not an integer.";
            }

            //Get query
            JProperty? prop_query = arguments.Property("query");
            if (prop_query == null)
            {
                return "Argument 'query' not provided.";
            }
            string query = prop_query.Value.ToString();

            //Get data
            CompanyFactsQuery cfq;
            try
            {
                cfq = await _bwm.CompanyFactsQueryAsync(cik);
            }
            catch (Exception bex)
            {
                return "Failure while pulling company facts for CIK '" + cik.ToString() + "': " + bex.Message;
            }

            //Search through
            List<Fact> ToReturn = new List<Fact>();
            foreach (Fact f in cfq.Facts)
            {
                if (f.Tag.ToLower().Contains(query.ToLower()) || f.Label.ToLower().Contains(query.ToLower()) || f.Description.ToLower().Contains(query.ToLower()))
                {
                    ToReturn.Add(f);
                }
            }

            if (ToReturn.Count == 0)
            {
                return "No financial data matched query '" + query + "'.";
            }

            //Construct what to return
            string ToReturnStr = "";
            foreach (Fact f in ToReturn)
            {
                //Add tag
                ToReturnStr = ToReturnStr + f.Tag;

                //Add description
                if (f.Description != "")
                {
                    ToReturnStr = ToReturnStr + " (" + f.Description + ")";
                }

                //Add new line
                ToReturnStr = ToReturnStr + "\n";
            }
            ToReturnStr = ToReturnStr.Trim();

            //return
            return ToReturnStr;
        }
    }
}