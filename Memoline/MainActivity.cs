using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin;
using Android.Util;
using Java.Lang;
using Memoline.JsonObjects;
using Android.Preferences;

namespace Memoline
{
    public class BackPressCloseHandler
    {
        private long backKeyPressedTime = 0;
        private Toast toast;

        private Activity activity;
        public BackPressCloseHandler(Activity context)
        {
            activity = context;
        }

        public void onBackPressed()
        {
            Log.Debug("DEBUG", "Tick: " + DateTime.Now.Ticks);
            if(DateTime.Now.Ticks > backKeyPressedTime + 20000000)
            {
                backKeyPressedTime = DateTime.Now.Ticks;
                showToast();
                return;
            }
            else
            {
                activity.Finish();
                Java.Lang.JavaSystem.Exit(0);
                return;
            }
        }

        public void showToast()
        {
            toast = Toast.MakeText(activity, "'뒤로' 버튼을 한번 더 누르면 종료합니다", ToastLength.Short);
            toast.Show();
        }
    }

    [Activity(Label = "Memoline", Theme = "@android:style/Theme.Material", Icon = "@drawable/logo_256")]
    public class MainActivity : Activity
    {
        private BackPressCloseHandler backPressCloseHandler;
        private ListView memoListView;
        List<Memo> memoList;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            backPressCloseHandler = new BackPressCloseHandler(this);

            // Get our button from the layout resource,
            // and attach an event to it
            Button writebutton = FindViewById<Button>(Resource.Id.writeButton);
            writebutton.Click += Writebutton_Click;
            
            memoList = new List<Memo>();
            memoListView = FindViewById<ListView>(Resource.Id.MemoList);
            memoListView.Adapter = new MemoListAdapter(this, memoList);

            Refresh();
            WriteActivity.onSent = () => Refresh();
        }

        private void Writebutton_Click(object sender, EventArgs e)
        {
            Toast.MakeText(this, "새로운 메모를 작성합니다", ToastLength.Short).Show();
            Intent intent = new Intent(this, typeof(WriteActivity));
            StartActivity(intent);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.mainMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Resource.Id.refresh:
                    Refresh();
                    Toast.MakeText(this, "새로고침 되었습니다", ToastLength.Short).Show();
                    return true;

                case Resource.Id.logout:
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetMessage("로그아웃 하시겠습니까?");
                    alert.SetPositiveButton("예", (sender, argv) =>
                    {
                        var appPref = PreferenceManager.GetDefaultSharedPreferences(this);
                        var editPref = appPref.Edit();
                        editPref.PutString("pref_user_token", "");
                        editPref.PutString("pref_user_secret", "");
                        editPref.PutString("pref_user_type", "");
                        editPref.Commit();

                        Toast.MakeText(this, "로그아웃 되었습니다", ToastLength.Short).Show();

                        Intent intent = new Intent(this, typeof(LoginActivity));
                        StartActivity(intent);
                        Finish();
                    });
                    alert.SetNegativeButton("아니오", (sender, argv) => { });
                    alert.Show();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private async void Refresh()
        {
            var appPref = PreferenceManager.GetDefaultSharedPreferences(this);
            var user_token = appPref.GetString("pref_user_token", "");
            var user_secret = appPref.GetString("pref_user_secret", "");
            RestClient client = new RestClient();
            var tmp = await client.RequestMemoList(user_token, user_secret, () =>
            {
                RunOnUiThread(() => Toast.MakeText(this, "메모를 불러오지 못했습니다", ToastLength.Short));
            });
            memoList.Clear();
            memoList.AddRange(tmp);
            RunOnUiThread(() => (memoListView.Adapter as MemoListAdapter).NotifyDataSetChanged());
        }

        public override void OnBackPressed()
        {
            backPressCloseHandler.onBackPressed();
            //base.OnBackPressed();
        }

        [Java.Interop.Export("OnDelClick")]
        public void onDelClick(View v)
        {
            var t = (ImageButton)v;
            Log.Debug("GetView", "tag: " + t.GetTag(Resource.Id.delButton).ToString());

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetMessage("이 메모를 삭제하시겠습니까?");
            alert.SetPositiveButton("예", async (sender, argv) =>
            {
                var appPref = PreferenceManager.GetDefaultSharedPreferences(this);
                bool result = await (new RestClient()).DeleteMemo(appPref.GetString("pref_user_token", ""), appPref.GetString("pref_user_secret", ""), t.GetTag(Resource.Id.delButton).ToString());
                if (result)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "메모를 삭제하였습니다", ToastLength.Short).Show();
                        Refresh();
                    });
                }
                else
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "메모를 삭제하지 못했습니다", ToastLength.Short).Show();
                    });
                }
            });
            alert.SetNegativeButton("아니오", (sender, argv) => { });
            alert.Show();

        }


        public class MemoListAdapter : BaseAdapter<Memo>
        {
            RestClient client = new RestClient();
            Activity context;
            List<Memo> items;
            static private bool isFirst = true;

            public MemoListAdapter(Activity context, List<Memo> items)
            {
                this.context = context;
                this.items = items;
            }

            public override Memo this[int position]
            {
                get
                {
                    return items[position];
                }
            }

            public override int Count { get { return items.Count; } }

            public override Java.Lang.Object GetItem(int position) => new JMemo(items[position]);

            public override long GetItemId(int position) => position;

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View view = convertView;
                if (view == null)
                    view = context.LayoutInflater.Inflate(Resource.Layout.MemoItem, null);
                var currentItem = items[position];
                view.FindViewById<TextView>(Resource.Id.MemoDate).Text = currentItem.date.ToLocalTime().ToString();
                view.FindViewById<TextView>(Resource.Id.MemoText).Text = currentItem.memo;
                Log.Debug("GetView", "labelFor: " + view.FindViewById<ImageButton>(Resource.Id.delButton).LabelFor);
                ImageButton btn = view.FindViewById<ImageButton>(Resource.Id.delButton);

                btn.SetTag(Resource.Id.delButton, currentItem._id);

                //if (isFirst)
                //{
                //    isFirst = false;
                //    btn.Click += (s, e) =>
                //    {
                //        var t = (ImageButton)s;
                //        Log.Debug("GetView", "tag: " + t.GetTag(Resource.Id.delButton).ToString());

                //        AlertDialog.Builder alert = new AlertDialog.Builder(context);
                //        alert.SetMessage("이 메모를 삭제하시겠습니까?");
                //        alert.SetPositiveButton("예", async (sender, argv) =>
                //        {
                //            var appPref = PreferenceManager.GetDefaultSharedPreferences(context);
                //            bool result = await client.DeleteMemo(appPref.GetString("pref_user_token", ""), appPref.GetString("pref_user_secret", ""), t.GetTag(Resource.Id.delButton).ToString());
                //            if (result)
                //            {
                //                context.RunOnUiThread(() =>
                //                {
                //                    Toast.MakeText(context, "메모를 삭제하였습니다", ToastLength.Short).Show();
                //                    (context as MainActivity).Refresh();
                //                });
                //            }
                //            else
                //            {
                //                context.RunOnUiThread(() =>
                //                {
                //                    Toast.MakeText(context, "메모를 삭제하지 못했습니다", ToastLength.Short).Show();
                //                });
                //            }
                //        });
                //        alert.SetNegativeButton("아니오", (sender, argv) => { });
                //        alert.Show();

                //    };
                //}
                //view.FindViewById<ImageButton>(Resource.Id.delButton).SetOnClickListener(new DeleteButtonClickListener(context, client, currentItem));
                
                return view;
            }
            /*
            private class DeleteButtonClickListener : View.IOnClickListener
            {
                private Activity context;
                private RestClient client;
                private Memo item;

                public DeleteButtonClickListener(Activity context, RestClient client, Memo item)
                {
                    this.context = context;
                    this.client = client;
                    this.item = item;
                }

                public IntPtr Handle { get { return IntPtr.Zero; } }

                public void Dispose() { }

                public override void
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(context);
                    alert.SetMessage("이 메모를 삭제하시겠습니까?");
                    alert.SetPositiveButton("예", async (sender, argv) =>
                    {
                        var appPref = PreferenceManager.GetDefaultSharedPreferences(context);
                        bool result = await client.DeleteMemo(appPref.GetString("pref_user_token", ""), appPref.GetString("pref_user_secret", ""), item._id);
                        if (result)
                        {
                            context.RunOnUiThread(() =>
                            {
                                Toast.MakeText(context, "메모를 삭제하였습니다", ToastLength.Short).Show();
                                (context as MainActivity).Refresh();
                            });
                        }
                        else
                        {
                            context.RunOnUiThread(() =>
                            {
                                Toast.MakeText(context, "메모를 삭제하지 못했습니다", ToastLength.Short).Show();
                            });
                        }
                    });
                    alert.SetNegativeButton("아니오", (sender, argv) => { });
                    alert.Show();
                }
        }*/
        }
    }
}

