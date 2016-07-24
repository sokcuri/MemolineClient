using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Views.InputMethods;
using Android.Preferences;
using System.Threading.Tasks;

namespace Memoline
{
    [Activity(Label = "WriteActivity", WindowSoftInputMode = SoftInput.StateHidden | SoftInput.AdjustResize)]
    public class WriteActivity : Activity
    {
        public static Action onSent;

        RestClient client = new RestClient();
        private Button saveButton;
        private EditText memoText;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Write);

            saveButton = FindViewById<Button>(Resource.Id.saveMemo);
            memoText = FindViewById<EditText>(Resource.Id.memoText);

            saveButton.Enabled = false;
            memoText.TextChanged += MemoText_TextChanged;
            saveButton.Click += SaveButton_Click;
            /*
            EditText memoText = FindViewById<EditText>(Resource.Id.memoText);
            memoText.RequestFocus();

            InputMethodManager imm = GetSystemService(InputMethodService) as InputMethodManager;
            imm.ShowSoftInput(memoText, ShowFlags.Forced);
            imm.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
            */        
        }

        private void SaveButton_Click(object sender, EventArgs e)
            => Task.Factory.StartNew(async () =>
            {
                var appPref = PreferenceManager.GetDefaultSharedPreferences(this);
                if (await client.PostMemoData(
                    appPref.GetString("pref_user_token", ""),
                    appPref.GetString("pref_user_secret", ""), memoText.Text.Trim()))
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "메모가 저장되었습니다", ToastLength.Short).Show();
                        onSent?.Invoke();
                        Finish();
                    });
                else
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "메모 전송에 실패했습니다", ToastLength.Short).Show(); 
                    });
            });

        private void MemoText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            saveButton.Enabled = memoText.Text.Trim().Length != 0;
        }
    }
}