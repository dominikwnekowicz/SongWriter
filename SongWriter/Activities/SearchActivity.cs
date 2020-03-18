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

namespace SongWriter.Activities
{
    [Activity(Label = "SearchActivity")]
    public class SearchActivity : Activity
    {
        Button exitSearchingButton;
        public Activity activity;
        TextView foundedAmountTextView;
        double time = 0;
        ListView listView;
        public override void OnBackPressed()
        {
            exitSearchingButton.PerformClick();
            base.OnBackPressed();
        }
        public void SetTime(double value)
        {
            time = value;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SearchLayout);

            listView = FindViewById<ListView>(Resource.Id.SearchListView);
            activity = this;
            var kindOfSearching = Intent.GetStringExtra("KindOfSearching");
            var toFind = Intent.GetStringExtra("ToFind").ToLower();
            int matchingNumber = 0;
            var length = 0;
            var syllables = 0;
            List<string> list = new List<string>();
            List<string> tmpList = new List<string>();
            foundedAmountTextView = FindViewById<TextView>(Resource.Id.FoundedAmountTextView);
            //searching call thread
            Thread findThread = new Thread(ft =>
            {

                var progress = FindViewById<ProgressBar>(Resource.Id.LoadingContentLoadingProgressBar);
                try
                {
                    if (kindOfSearching == "Rhymes") //rhymes
                    {
                        matchingNumber = Intent.GetIntExtra("MatchingNumber", 3);
                        length = Intent.GetIntExtra("Length", 9);
                        syllables = Intent.GetIntExtra("Syllables", 4);
                        list = SQLiteDb.FindRhymes(toFind, matchingNumber, length, syllables, this);
                    }
                    else //synonyms
                    {
                        list = SQLiteDb.FindSynonyms(toFind, this);
                        if (tmpList.Any()) //found for similar
                        {
                            string tmp;
                            for (int i = toFind.Length; i >= toFind.Length / 2; i--)
                            {
                                try
                                {
                                    tmp = toFind.Remove(i);
                                }
                                catch
                                {
                                    tmp = toFind;
                                }
                                if (list.Where(item => item.ToLower().Contains(tmp)).Any()) // found similar word
                                {
                                    foundedAmountTextView.Text = "Dla \"" + list.First(item => item.ToLower().Contains(tmp)) + "\": " + list.Count;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
                finally
                {
                    var adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, list);
                    this.RunOnUiThread(() =>
                    {   
                        listView.Adapter = adapter;
                        progress.Visibility = ViewStates.Gone;
                        if (!list.Any()) foundedAmountTextView.Text = "Nie znaleziono...";
                        if (list.Any()) foundedAmountTextView.Text = "Znaleziono: " + list.Count + " ("+time+"s)";
                    });
                }
            });
            findThread.Start();

            exitSearchingButton = FindViewById<Button>(Resource.Id.ExitSearchingButton);
            exitSearchingButton.Click += (o, e) =>
            {
                Intent intent = new Intent();
                SetResult(Result.Canceled);
                Finish();
            };

            listView.ItemClick += (o, e) =>
            {
                Intent intent = new Intent();
                intent.PutExtra("SelectFoundedWord", list[e.Position]);
                SetResult(Result.Ok, intent);
                Finish();
            };
        }
    }
}