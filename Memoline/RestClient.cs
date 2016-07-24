using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

using Memoline.JsonObjects;
using System.Threading.Tasks;
using Android.Preferences;

namespace Memoline
{
    internal class RestClient
    {
        static readonly string baseURI = "http://memoline.neko.kr";

        private HttpClient client;
        private ISharedPreferences appPref;

        public RestClient()
        {
            client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(5000);
        }

        public async Task<AuthInfo> RequestAuth(Action onError, Action onTimeout = null)
        {
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync($"{baseURI}/auth");
            }
            catch(TaskCanceledException)
            {
                onTimeout?.Invoke();
                return null;
            }
            
            Log.Debug("RestClient::RequestAuth", $"statusCode = {response.StatusCode}");
            if (response.StatusCode != HttpStatusCode.OK)
                onError();
            return JsonConvert.DeserializeObject<AuthInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<AuthCallBack> RequestAuthCallBack(string callbackURI, Action onError, Action onTimeout = null)
        {
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync($"{callbackURI}");
            }
            catch (TaskCanceledException)
            {
                onTimeout?.Invoke();
                return null;
            }

            Log.Debug("RestClient::RequestAuth", $"statusCode = {response.StatusCode}");
            if (response.StatusCode != HttpStatusCode.OK)
                onError();
            return JsonConvert.DeserializeObject<AuthCallBack>(await response.Content.ReadAsStringAsync());
        }

        public async Task<List<Memo>> RequestMemoList(string token, string secret, Action onError, Action onTimeout = null)
        {
            HttpResponseMessage response;
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"
                //2016-07-24T08:07:15.042Z
            };
            try
            {
                response = await client.GetAsync($"{baseURI}/memo?user_token={token}&user_secret={secret}");
            }
            catch (TaskCanceledException)
            {
                onTimeout?.Invoke();
                return null;
            }
            if (response.StatusCode != HttpStatusCode.OK)
                onError();
            return JsonConvert.DeserializeObject<List<Memo>>(await response.Content.ReadAsStringAsync(), settings);
        }

        public async Task<bool> PostMemoData(string token, string secret, string memoText, Action onTimeout = null)
        {
            HttpResponseMessage response;
            try
            {
                var content = JsonConvert.SerializeObject(new
                {
                    user_token = token,
                    user_secret = secret,
                    memo = memoText
                });
                response = await client.PostAsync($"{baseURI}/memo", new StringContent(content));
                if (response.StatusCode != HttpStatusCode.OK)
                    return false;
            }
            catch (TaskCanceledException)
            {
                onTimeout?.Invoke();
                return false;
            }
            return true;
        }

        public async Task<bool> DeleteMemo(string token, string secret, string _id, Action onTimeout = null)
        {
            HttpResponseMessage response;
            try
            {
                response = await client.DeleteAsync($"{baseURI}/memo?user_token={token}&user_secret={secret}&_id={_id}");
                if (response.StatusCode != HttpStatusCode.OK)
                    return false;
            }
            catch (TaskCanceledException)
            {
                onTimeout?.Invoke();
                return false;
            }
            return true;
        }
    }
}