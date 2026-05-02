using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimHanewich.AgentFramework;
using TimHanewich.Foundry.OpenAI.Responses;
using SecuritiesExchangeCommission.Edgar;
using SecuritiesExchangeCommission.Edgar.Data;

namespace MIRA
{
    public class GetFinancialData : ExecutableFunction
    {
        private SECBandwidthManager _bwm;

        public GetFinancialData(SECBandwidthManager bwm)
        {
            Name = "get_financial_data";
            Description = "Gather financial data for a particular company for a particular financial XBRL fact (i.e. 'Assets' or 'CurrentLiabilities')";
            InputParameters.Add(new FunctionInputParameter("CIK", "The company's central index key (CIK), i.e. '1655210'", "number"));
            InputParameters.Add(new FunctionInputParameter("fact", "The name (tag) of the specific XBRL fact you are requesting historical financial data for (i.e. 'Assets' or 'CurrentLiabilities' or 'RevenueNet')"));

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

            //Get fact (tag)
            JProperty? prop_fact = arguments.Property("fact");
            if (prop_fact == null)
            {
                return "Argument 'fact' not provided.";
            }
            string fact = prop_fact.Value.ToString();

            //Get all the data
            CompanyFactsQuery cfq;
            try
            {
                cfq = await _bwm.CompanyFactsQueryAsync(cik);
            }
            catch (Exception bex)
            {
                return "Failure while pulling company facts for CIK '" + cik.ToString() + "': " + bex.Message;
            }

            //Find it
            Fact? SelectedFact = null;
            foreach (Fact f in cfq.Facts)
            {
                if (f.Tag == fact)
                {
                    SelectedFact = f;
                }
            }

            //Didn't find it?
            if (SelectedFact == null)
            {
                return "Unable to find financial fact with tag '" + fact + "'.";
            }

            //Prep return
            string ToReturn = "Data Points for '" + SelectedFact.Tag + "': ";
            foreach (FactDataPoint fdp in SelectedFact.DataPoints)
            {
                //Construct this line
                string ThisLine = "";
                if (fdp.Start != null) // it is a "period-based" fact, like a period of time with start and end
                {
                    if (fdp.Period == FiscalPeriod.FiscalYear)
                    {
                        ThisLine = "Year Ending " + fdp.End.ToShortDateString() + ": " + fdp.Value.ToString("#,##0.00");
                    }
                    else //it is for a quarter ending
                    {
                        ThisLine = "Quarter Ending " + fdp.End.ToShortDateString() + ": " + fdp.Value.ToString("#,##0.00");
                    }
                }
                else //it is a moment in time fact (i.e. Assets on this day)
                {
                    ThisLine = fdp.End.ToShortDateString() + ": " + fdp.Value.ToString("#,##0.00");
                }

                ToReturn = ToReturn + "\n" + ThisLine;
            }

            //Return
            return ToReturn;
        }
    }
}