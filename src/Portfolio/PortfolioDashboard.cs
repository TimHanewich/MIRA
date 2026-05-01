using System;
using System.ComponentModel;
using TimHanewich.Investing.Simulation;
using AIA.YFinanceServer;

namespace AIA.Portfolio
{
    public class PortfolioDashboard
    {
        public PortfolioHolding[] Holdings {get; set;}
        
        public float ToalGainLoss
        {
            get
            {
                float ToReturn = 0.0f;
                foreach (PortfolioHolding ph in Holdings)
                {
                    ToReturn = ToReturn + ph.UnrealizedGainLoss;
                }
                return ToReturn;
            }
        }

        public PortfolioDashboard()
        {
            Holdings = new PortfolioHolding[]{};
        }

        public async static Task<PortfolioDashboard> ConstructAsync(TimHanewich.Investing.Simulation.Portfolio portfolio)
        {
            //Get list of all stocks held
            List<string> AllHoldingSymbols = new List<string>();
            foreach (Holding h in portfolio.Holdings())
            {
                AllHoldingSymbols.Add(h.Symbol.Trim().ToUpper());
            }

            //Get them
            YFinanceServerBridge yfsb = new YFinanceServerBridge();
            YFinanceServer.Quote[] quotes = await yfsb.QuoteMultipleAsync(AllHoldingSymbols.ToArray());
            
            //Construct the portflio holdings
            List<PortfolioHolding> phs = new List<PortfolioHolding>();
            foreach (Holding h in portfolio.Holdings())
            {
                //Find the Quote
                YFinanceServer.Quote? ThisQuote = null;
                foreach (YFinanceServer.Quote quote in quotes)
                {
                    if (quote.Symbol.ToUpper().Trim() == h.Symbol.ToUpper().Trim())
                    {
                        ThisQuote = quote;
                    }
                }

                //If we have it
                if (ThisQuote != null)
                {
                    PortfolioHolding ph = new PortfolioHolding(h.Symbol.Trim().ToUpper());
                    ph.CurrentPrice = ThisQuote.Price;
                    ph.DayChangePercent = ThisQuote.ChangePercent;
                    ph.QuantityOwned = h.Quantity;
                    ph.TotalCostBasis = h.CostBasis * h.Quantity; //h.CostBasis I believe stores it on a per-share basis (not total)
                    phs.Add(ph);
                }
            }

            //Return
            PortfolioDashboard ToReturn = new PortfolioDashboard();
            ToReturn.Holdings = phs.ToArray();
            return ToReturn;
        }

    }

    public class PortfolioHolding
    {
        public string Symbol {get; set;}                        //Symbol
        public float CurrentPrice {get; set;}                   //current price
        public float DayChangePercent {get; set;}               //How much it is up/down today as a percent
        public int QuantityOwned {get; set;}                    //How many shares owned
        public float TotalCostBasis {get; set;}                 //The cost basis paid for all those shars (avg. share price * all)

        //Position Value: the TOTAL value of those shares (price * quanity)
        public float PositionValue
        {
            get
            {
                return CurrentPrice * QuantityOwned;
            }
        }

        //The Unrealized Gain/Loss on the entire position, in dollars
        public float UnrealizedGainLoss
        {
            get
            {
                return PositionValue - TotalCostBasis;
            }
        }

        //The Unrealized Gain/Loss on the entire position, in dollars
        public float UnrealizedGainLossPercent
        {
            get
            {
                return UnrealizedGainLoss / TotalCostBasis;
            }
        }

        public PortfolioHolding(string symbol)
        {
            Symbol = symbol;
        }
    }
}