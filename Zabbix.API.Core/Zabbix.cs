using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ZabbixAPICore
{
    public class Zabbix
    {
        private string user;
        private string password;
        private string apiURL;
        private string auth;
        private string basicAuthentication;
        
        public Zabbix(string user, string password, string apiURL, bool useBasicAuthorization = false)
        {
            
            if (!Uri.IsWellFormedUriString(apiURL, UriKind.Absolute))
            {
                throw new UriFormatException();
            }

            this.user = user ?? throw new ArgumentNullException("user");
            this.password = password ?? throw new ArgumentNullException("password");
            this.apiURL = apiURL;
            if (useBasicAuthorization) basicAuthentication = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(this.user + ":" + this.password));
        }

        public async Task LoginAsync()
        {
            dynamic userAuth = new ExpandoObject();
            userAuth.user = user;
            userAuth.password = password;
            Response response = await GetResponseObjectAsync("user.login", userAuth);
            auth = response.result;
        }

        public async Task<bool> LogoutAsync()
        {
            Response response = await GetResponseObjectAsync("user.logout", new string[] { });
            var result = response.result;
            return result;
        }

        public async Task<string> GetResponseJsonAsync(string method, object parameters)
        {
            Request request = new Request("2.0", method, 1, auth, parameters);

            string jsonParams = JsonConvert.SerializeObject(request);
            string jsonResponse = await SendRequestAsync(jsonParams);

            return jsonResponse;
        }
        public async Task<Response> GetResponseObjectAsync(string method, object parameters)
        {
            string jsonResponse = await GetResponseJsonAsync(method, parameters);
            var objectResponse = ConvertJsonToResponse(jsonResponse);

            return objectResponse;
        }

        private Response ConvertJsonToResponse(string json)
        {
            Response response = JsonConvert.DeserializeObject<Response>(json);
            return response;
        }

        private Task<string> SendRequestAsync(string jsonParams)
        {
            return Task.Run(() =>
            {
                var request = WebRequest.Create(apiURL);
                request.Method = "POST";
                request.Headers.Add("Authorization", $"Basic {basicAuthentication}");
                request.ContentType = "application/json; charset=UTF-8";

                var postBytes = Encoding.UTF8.GetBytes(jsonParams);
                request.ContentLength = postBytes.Length;
                
                var requestStream = request.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                var response = (HttpWebResponse) request.GetResponse();
                string result;
                using (var rdr = new StreamReader(response.GetResponseStream()))
                {
                    result = rdr.ReadToEnd();
                }

                return result;
            });
        }
    }
}