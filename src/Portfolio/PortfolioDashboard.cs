using System;
using System.ComponentModel;
using TimHanewich.Investing.Simulation;
using AIA.YFinanceServer;

namespace AIA.Portfolio
{
    public class PortfolioDashboard
    {
        public PortfolioHolding[] Holdings {get; set;}
        public float CashBalance {get; set;}            //Current cash balance
        public float CashInjected {get; set;}           //How much cash was started with
        
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
            PortfolioDashboard ToReturn = new PortfolioDashboard();

            //Sum how much cash was injected (deposited)
            foreach (CashTransaction ct in portfolio.CashTransactionLog)
            {
                if (ct.ChangeType == CashTransactionType.Edit)
                {
                    ToReturn.CashInjected = ToReturn.CashInjected + ct.CashChange;
                }
            }

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
            ToReturn.Holdings = phs.ToArray();
            return ToReturn;
        }

        public float HoldingsValue
        {
            get
            {
                float ToReturn = 0.0f;
                foreach (PortfolioHolding ph in Holdings)
                {
                    ToReturn = ToReturn + ph.PositionValue;
                }
                return ToReturn;
            }
        }

        //Net gain/loss
        public float TotalGainLoss
        {
            get
            {
                //Math is:
                //Sum of value of holdings + cash balance - cash injected
                return HoldingsValue + CashBalance - CashInjected;
            }
        }

        public string Print()
        {
            string ToReturn = "";

            //Add totals
            ToReturn = ToReturn + "**PORTFOLIO DETAILS**";
            ToReturn = ToReturn + "\n" + "Cash Injected: $" + CashInjected.ToString("#,##0.00");
            ToReturn = ToReturn + "\n" + "Total Gain/Loss: $" + TotalGainLoss.ToString("#,##0.00");
            ToReturn = ToReturn + "\n\n";

            //Table
            ToReturn = ToReturn + "|Symbol|Current Price|Day Change Percent|Position Value|Position Cost Basis|Unrealized Gain/Loss|Unrealized Gain/Loss %|";
            ToReturn = ToReturn + "\n" + "|-|-|-|-|-|-|-|";
            PortfolioHolding[] sorted = Holdings.OrderByDescending(h => h.UnrealizedGainLoss).ToArray();
            foreach (PortfolioHolding ph in sorted)
            {
                ToReturn = ToReturn + "\n" + "|" + ph.Symbol + "|$" + ph.CurrentPrice.ToString("#,##0.00") + "|" + ph.DayChangePercent.ToString("#,##0.0") + "%|$" + ph.PositionValue.ToString("#,##0.00") + "|$" + ph.TotalCostBasis.ToString("#,##0.00") + "|$" + ph.UnrealizedGainLoss.ToString("#,##0.00") + "|" + ph.UnrealizedGainLossPercent.ToString("#,##0.0") + "%|";
            }
            ToReturn = ToReturn + "\n\n";


            return ToReturn.Trim();
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
                return (UnrealizedGainLoss / TotalCostBasis)*100;
            }
        }

        public PortfolioHolding(string symbol)
        {
            Symbol = symbol;
        }
    }
}