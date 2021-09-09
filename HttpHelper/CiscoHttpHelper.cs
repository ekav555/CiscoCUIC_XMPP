using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CiscoCUIC_XMPP
{
    public class CiscoHttpHelper
    {
        public CiscoHttpHelper()
        {
            this._httpClient = new HttpClient(); 
        }
        private readonly HttpClient _httpClient;
        private HttpResponseMessage SendAsync(string method, string xml, string url, bool useLocalCreds = false)
        {
            Console.WriteLine($"Making {method} REST CALL to {url}");
            HttpRequestMessage req = null;
            HttpResponseMessage res = null;
            HttpContent BodyContent = null;
            string username = Properties.Settings.Default.Username;
            string password = Properties.Settings.Default.Password;

            //SOME API CALLS CAN ONLY BE DONE BY THE USER WHO OWNS THE ACTIVE CALL
            if(useLocalCreds)
            {
                username = Properties.Settings.Default.LocalUser;
                password = Properties.Settings.Default.LocalPassword;
            }

            if (!string.IsNullOrWhiteSpace(url))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                //Convert to HTTPContent                
                if (!string.IsNullOrWhiteSpace(xml))
                {
                    if (method.Trim().ToUpper() == "PATCH")
                    {
                        req = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                        {
                            Content = new StringContent(xml, Encoding.UTF8, "application/xml")
                        };
                    }
                    else
                    {
                        BodyContent = new StringContent(xml, Encoding.UTF8, "application/xml");
                    }
                }

                //Call API based on the method
                switch (method.Trim().ToUpper())
                {
                    case "GET":
                        res = _httpClient.GetAsync(url).Result;
                        break;
                    case "POST":
                        res = _httpClient.PostAsync(url, BodyContent).Result;
                        break;
                    case "PATCH":
                        res = _httpClient.SendAsync(req).Result;
                        break;
                    case "PUT":
                        res = _httpClient.PutAsync(url, BodyContent).Result;
                        break;
                    case "DELETE":
                        res = _httpClient.DeleteAsync(url).Result;
                        break;
                    default:
                        //Default is GET
                        res = _httpClient.GetAsync(url).Result;
                        break;

                }
            }
            Console.WriteLine($"Call Result: {res.StatusCode}");
            return (res);
        }

        public string GetUsers()
        {
            var response = SendAsync("GET", null, $"http://{Properties.Settings.Default.Hostname}:8082/finesse/api/Users");
            return response.Content.ReadAsStringAsync().Result;
        }

        public string GetUser(string UserID)
        {
            var response = SendAsync("GET", null, $"http://{Properties.Settings.Default.Hostname}:8082/finesse/api/User/{UserID}");
            return response.Content.ReadAsStringAsync().Result;
        }

        public string GetSystemInfo()
        {
            var response = SendAsync("GET", null, $"http://{Properties.Settings.Default.Hostname}:8082/finesse/api/SystemInfo");
            return response.Content.ReadAsStringAsync().Result;
        }

        public string GetTeam(string TeamID)
        {
            var response = SendAsync("GET", null, $"http://{Properties.Settings.Default.Hostname}:8082/finesse/api/Team/{TeamID}");
            return response.Content.ReadAsStringAsync().Result;
        }

        public string GetTeams()
        {
            var response = SendAsync("GET", null, $"http://{Properties.Settings.Default.Hostname}:8082/finesse/api/Teams");
            return response.Content.ReadAsStringAsync().Result;
        }

        public void UpdateDialog(string dialogID, string xml)
        {
            Console.WriteLine(xml);
            var response = SendAsync("PUT", xml, $"http://{Properties.Settings.Default.Hostname}:8082/finesse/api/Dialog/{dialogID}", true);
        }
    }
}
