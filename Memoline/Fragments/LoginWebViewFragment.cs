using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Webkit;
using System.Threading.Tasks;
using Android.Preferences;

namespace Memoline.Fragments
{
    public class LoginWebViewFragment : Fragment
    {
        private string url;

        public LoginWebViewFragment(string url) : base()
        {
            this.url = url;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            View view = inflater.Inflate(Resource.Layout.LoginWebView, container, false);

            WebView wv = view.FindViewById<WebView>(Resource.Id.loginWebView);
            wv.LoadUrl(url);
            wv.SetWebViewClient(new LoginWebViewClient(Context));
            return view;
        }

        public class LoginWebViewClient : WebViewClient
        {
            private RestClient client;
            private Context context;

            public LoginWebViewClient(Context context) : base()
            {
                this.context = context;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                // is callback, page load prevent in webview
                if (url.Contains("/auth/callback"))
                {
                    var task = Task.Factory.StartNew(async () =>
                    {
                        Activity activity = context as Activity;
                        client = new RestClient();
                        var callbackInfo = await client.RequestAuthCallBack(url, () =>
                        {
                            Log.Debug("ShouldOverrideUrlLoading", $"RequestAuthCallBack failed");
                            Toast.MakeText(context, "RequestAuthCallBack failed", ToastLength.Short).Show();
                        });
                        Log.Debug("ShouldOverrideUrlLoading", $"user_token  = {callbackInfo.user_token}");
                        Log.Debug("ShouldOverrideUrlLoading", $"user_secret = {callbackInfo.user_secret}");
                        Log.Debug("ShouldOverrideUrlLoading", $"user_type   = {callbackInfo.user_type}");

                        (context as Activity).RunOnUiThread(() => Toast.MakeText(context, "User Login success", ToastLength.Short).Show());

                        var appPref = PreferenceManager.GetDefaultSharedPreferences(context);
                        var prefEditor = appPref.Edit();
                        prefEditor.PutString("pref_user_token", callbackInfo.user_token);
                        prefEditor.PutString("pref_user_secret", callbackInfo.user_secret);
                        prefEditor.PutString("pref_user_type", callbackInfo.user_type);
                        prefEditor.Commit();

                        Intent intent = new Intent(context, typeof(MainActivity));
                        context.StartActivity(intent);
                        
                        (context as Activity).Finish();
                    });
                    return true;
                }
                return false;
            }

            public override void OnLoadResource(WebView view, string url)
            {
                base.OnLoadResource(view, url);
            }


            public override void OnPageFinished(WebView view, string url)
            {
                if (url.Contains("/auth/callback"))
                {
                }
                Toast.MakeText(context, $"url: {url}", ToastLength.Short).Show();
                Log.Debug("LoginWebViewClient::OnPageFinished()", "url: " + url);
                base.OnPageFinished(view, url);
            }
        }
    }
}