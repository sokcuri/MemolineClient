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

namespace Memoline.JsonObjects
{
    public class Memo
    {
        public string user_token { get; set; }
        public string memo { get; set; }
        public DateTime date { get; set; }
        public string _id { get; set; }
    }

    public class JMemo : Java.Lang.Object
    {
        public string user_token { get; set; }
        public string memo { get; set; }
        public DateTime date { get; set; }
        public string _id { get; set; }

        public JMemo(Memo m)
        {
            user_token = m.user_token;
            memo = m.memo;
            date = m.date;
            _id = m._id;
        }
    }
}