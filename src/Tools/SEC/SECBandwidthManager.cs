using System;
using System.Collections.Generic;
using SecuritiesExchangeCommission.Edgar;
using SecuritiesExchangeCommission.Edgar.Data;
using System.Collections.Generic;

namespace AIA
{
    //Manages API calls for data to the SEC API so we aren't constantly calling to it (it is a lot of data to pull down each time)
    public class SECBandwidthManager
    {
        private List<CompanyFactsQuery> CachedCompanyFactsQueries;

        public SECBandwidthManager()
        {
            CachedCompanyFactsQueries = new List<CompanyFactsQuery>();
        }

        public async Task<CompanyFactsQuery> CompanyFactsQueryAsync(int cik)
        {
            //Search if we have one
            foreach (CompanyFactsQuery existing_cfq in CachedCompanyFactsQueries)
            {
                if (existing_cfq.CIK == cik)
                {
                    return existing_cfq;
                }
            }

            //We must not have one, so get it
            CompanyFactsQuery cfq = await CompanyFactsQuery.QueryAsync(cik);
            CachedCompanyFactsQueries.Add(cfq);
            return cfq;
        }
    }
}