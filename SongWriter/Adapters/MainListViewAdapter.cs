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
using SongWriter.Model;

namespace SongWriter.Adapters
{
    class MainListViewAdapter : BaseAdapter
    {
        private readonly List<Work> works;
        private readonly Context context;

        public MainListViewAdapter(Context context, IEnumerable<Work> works)
        {
            this.context = context;
            this.works = works.ToList();
        }


        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            View view = inflater.Inflate(Android.Resource.Layout.SimpleListItemActivated2, null);
            TextView text, title;
            title = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            text = view.FindViewById<TextView>(Android.Resource.Id.Text2);
            title.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.DarkGray));
            text.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.DarkGray));
            text.SetPadding(text.PaddingLeft, (text.PaddingTop*3), text.PaddingRight, text.PaddingBottom);
            text.SetTextSize(Android.Util.ComplexUnitType.Dip, 16);
            title.SetTextSize(Android.Util.ComplexUnitType.Dip, 20);
            title.Text = works[position].Title;
            var textToCut = works[position].Text.Replace("\n", " ");
            try
            {
                text.Text = textToCut.Substring(0, 40) + "...";
            }
            catch
            {
                text.Text = textToCut;
            }
            return view;
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return works.Count;
            }
        }

    }

    class MainListViewAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        //public TextView Title { get; set; }
    }
}