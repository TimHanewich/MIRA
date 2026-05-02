using System;
using TimHanewich.AgentFramework;
using Newtonsoft.Json.Linq;

namespace AIA
{
    public class ViewPortfolio : ExecutableFunction
    {
        State UseState {get; set;}

        public ViewPortfolio(State use_state)
        {
            Name = "view_portfolio";
            Description = "Open and view your investment portfolio, including cash balance and holdings and their values.";
            
            UseState = use_state;
        }

        public override async Task<string> ExecuteAsync(JObject? arguments = null)
        {
            //Get portfolio dashboard
            PortfolioDashboard pd;
            try
            {
                pd = await PortfolioDashboard.ConstructAsync(UseState.Portfolio);
            }
            catch (Exception ex)
            {
                return "Failure while opening Portfolio: " + ex.Message;
            }

            //Print it and return
            try
            {
                return pd.Print();
            }
            catch (Exception ex)
            {
                return "Portfolio was opened, but failure while providing it in text form: " + ex.Message;
            }
        }
    }
}