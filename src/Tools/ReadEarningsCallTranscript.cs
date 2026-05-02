using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TheMotleyFool.Transcripts;
using TimHanewich.AgentFramework;

namespace MIRA
{
    public class ReadEarningsCallTranscript : ExecutableFunction
    {
        public ReadEarningsCallTranscript()
        {
            Name = "read_earnings_call_transcript";
            Description = "Read the contents of an earnings call transcript.";
            InputParameters.Add(new TimHanewich.Foundry.OpenAI.Responses.FunctionInputParameter("url", "The URL to the transcript at fool.com, e.g. 'https://www.fool.com/earnings/call-transcripts/2026/01/28/meta-meta-q4-2025-earnings-call-transcript/'"));
        }

        public override async Task<string> ExecuteAsync(JObject? arguments)
        {
            if (arguments == null)
            {
                return "Must provide arguments.";
            }

            //Get url
            JProperty? prop_url = arguments.Property("url");
            if (prop_url == null)
            {
                return "Must provide 'url'.";
            }
            string url = prop_url.Value.ToString();

            //Get it
            Transcript t;
            try
            {
                t = await Transcript.CreateFromUrlAsync(url);
            }
            catch (Exception ex)
            {
                return "Failed to read earnings call transcript at '" + url + "': " + ex.Message;
            }

            //return it
            return JsonConvert.SerializeObject(t, Formatting.Indented);
        }
    }
}