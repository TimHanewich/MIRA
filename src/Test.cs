using System;
using System.Diagnostics;

namespace AIA
{
    public class YFinanceBroker
    {
        public float GetStockPriceAsync(string symbol)
        {
            string script = "import yfinance as yf;ticker = yf.Ticker('" + symbol.ToUpper().Trim() + "');latest_data = ticker.history('1d', interval='1m');close_data = latest_data['Close'];last_close:float = close_data.iloc[-1];print(str(last_close))";
            string cmd = "python -c \"" + script + "\"";

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/c " + cmd;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            Process? p = Process.Start(psi);
            if (p != null)
            {
                string result = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();
                p.WaitForExit();
                if (error != "")
                {
                    throw new Exception("Encountered error: " + error);
                }
                
                //Try parse result
                try
                {
                    return Convert.ToSingle(result.Trim());
                }   
                catch 
                {
                    throw new Exception("Value '" + result.Trim() + "' did not parse into a float.");
                }
            }
            else
            {
                throw new Exception("Process did not start.");
            }
        }
    }
}