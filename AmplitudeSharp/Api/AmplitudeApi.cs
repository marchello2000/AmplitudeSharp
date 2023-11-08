using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AmplitudeSharp.Api
{
    public abstract class IAmplitudeApi
    {
        public enum SendResult
        {
            Success,
            ProxyNeeded,
            Throttled,
            ServerError,
        }

        public bool OfflineMode { get; set; }

        public abstract Task<SendResult> Identify(AmplitudeIdentify identification);

        public abstract Task<SendResult> SendEvents(List<AmplitudeEvent> events);
    }

    class AmplitudeApi : IAmplitudeApi
    {
        private string apiKey;
        private string apiRegion;

        private HttpClient httpClient;
        private HttpClientHandler httpHandler;

        public AmplitudeApi(string apiKey, string apiRegion)
        {
            this.apiKey = apiKey;
            this.apiRegion = apiRegion;

            httpHandler = new HttpClientHandler();
            httpHandler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            httpHandler.Proxy = WebRequest.GetSystemWebProxy();
            httpHandler.UseProxy = true;

            httpClient = new HttpClient(httpHandler);
        }

        public void ConfigureProxy(string proxyUserName, string proxyPassword)
        {
            if ((string.IsNullOrEmpty(proxyPassword) && string.IsNullOrEmpty(proxyUserName)))
            {
                httpHandler.Proxy = WebRequest.GetSystemWebProxy();
            }
            else
            {
                httpHandler.Proxy.Credentials = new NetworkCredential(proxyUserName, proxyPassword);
            }
        }

        public override Task<SendResult> Identify(AmplitudeIdentify identification)
        {
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(identification);

            return DoApiCall("identify", "identification", data);
        }

        public override Task<SendResult> SendEvents(List<AmplitudeEvent> events)
        {
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(events);

            return DoApiCall("httpapi", "event", data);
        }

        private async Task<SendResult> DoApiCall(string endPoint, string paramName, string paramData)
        {
            SendResult result = SendResult.Success;

            if (!OfflineMode)
            {
                string boundary = "----" + DateTime.Now.Ticks;

                MultipartFormDataContent content = new MultipartFormDataContent(boundary);
                var keyContent = new StringContent(apiKey, UTF8Encoding.UTF8, "text/plain");
                content.Add(keyContent, "api_key");

                var data = new StringContent(paramData, UTF8Encoding.UTF8, "application/json");
                content.Add(data, paramName);

                var apiURL = "https://api.amplitude.com";
                if (apiRegion == "eu")
                    apiURL = "https://api.eu.amplitude.com";

                try
                {
                    var postResult = await httpClient.PostAsync($"{apiURL}/{endPoint}", content);

                    if (postResult.StatusCode >= HttpStatusCode.InternalServerError)
                    {
                        result = SendResult.ServerError;
                    }
                    if (postResult.StatusCode >= HttpStatusCode.BadRequest)
                    {
                        switch (postResult.StatusCode)
                        {
                            case HttpStatusCode.ProxyAuthenticationRequired:
                                result = SendResult.ProxyNeeded;
                                break;

                            case (HttpStatusCode)429:
                                result = SendResult.Throttled;
                                break;

                            default:
                                result = SendResult.ServerError;
                                break;
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    // Ignore connection errors
                    result = SendResult.ServerError;
                }
                catch (Exception e)
                {
                    result = SendResult.ServerError;
                    AmplitudeService.s_logger(LogLevel.Warning, $"Failed to get device make/model: {e.ToString()}");
                }
            }

            return result;
        }
    }
}
