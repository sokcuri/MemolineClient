using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Android.Preferences;
using Android.Util;

namespace Memoline
{
    [Activity(Label = "Memoline", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", Icon ="@drawable/logo_256", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        private RestClient client;
        protected bool isFirstRun = true;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (isFirstRun)
            {
                isFirstRun = false;

                Task.Factory.StartNew(() =>
                {
                    var appPref = PreferenceManager.GetDefaultSharedPreferences(this);
                    string user_type = appPref.GetString("pref_user_type", "");
                    string user_token = appPref.GetString("pref_user_token", "");
                    if (user_type == "")
                    {
                        Intent intent = new Intent(this, typeof(LoginActivity));
                        intent.AddFlags(ActivityFlags.ClearTop);
                        intent.AddFlags(ActivityFlags.ClearTask);

                        Thread.Sleep(500);
                        RunOnUiThread(() =>
                        {
                            Finish();
                            Toast.MakeText(this, $"Login Required", ToastLength.Short).Show();
                            StartActivity(intent);
                        });
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, $"Signed in as {user_token}.", ToastLength.Short).Show();

                            client = new RestClient();
                            /*var callbackInfo = await client.RequestAuthLogin(() =>
                            {
                                Log.Debug("SplashActivity", $"RequestAuthCallBack failed");
                                Toast.MakeText(context, "RequestAuthCallBack failed", ToastLength.Short).Show();
                            },
                            // Timeout
                            () =>
                            {
                                Log.Debug("SplashActivity", $"HTTP Timeout raised");
                                Toast.MakeText(context, "HTTP Timeout", ToastLength.Short).Show();
                            }
                            );*/
                            Intent intent = new Intent(this, typeof(MainActivity));
                            StartActivity(intent);
                        });
                    }
                   
                });
            }

            base.OnCreate(Bundle.Empty);
            SetContentView(Resource.Layout.Splash);
        }
    }
}