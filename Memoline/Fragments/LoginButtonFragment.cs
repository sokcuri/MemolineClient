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
using System.Threading.Tasks;

namespace Memoline.Fragments
{
    public class LoginButtonFragment : Fragment
    {
        RestClient client;
        private Button btnLogin;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            View view = inflater.Inflate(Resource.Layout.LoginButton, container, false);

            btnLogin = view.FindViewById<Button>(Resource.Id.SignInButton);
            btnLogin.Click += BtnLogin_Click;

            return view;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            var task = Task.Factory.StartNew(async () =>
            {
                Activity activity = Context as Activity;
                client = new RestClient();
                var authInfo = await client.RequestAuth(() =>
                {
                    
                    Log.Debug("MainActivity::OnCreate_test", $"RequestAuth failed");
                    activity.RunOnUiThread(() =>
                    {
                        Toast.MakeText(activity, "RequestAuth failed", ToastLength.Short).Show();
                    });
                });
                Log.Debug("MainActivity::OnCreate_test", $"token ={authInfo.oauth_token}");
                Log.Debug("MainActivity::OnCreate_test", $"secret={authInfo.oauth_token_secret}");
                Log.Debug("MainActivity::OnCreate_test", $"url   ={authInfo.authorize_url}");
                activity.FragmentManager.BeginTransaction()
                    .Replace(Resource.Id.LoginContainer, new LoginWebViewFragment(authInfo.authorize_url))
                    .Commit();
            });
        }
    }
}