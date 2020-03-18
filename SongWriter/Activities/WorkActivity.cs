using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SongWriter.Model;

namespace SongWriter.Activities
{
    [Activity(Label = "WorkActivity")]
    public class WorkActivity : Activity
    {
        EditText workEditText, titleEditText;
        SearchView searchEditText;
        LinearLayout searchingLinearLayout;
        Work work;
        TextView syllabelsTextView;
        LinearLayout numberPickersLinearLayout;
        Button findSynonymsButton, findRhymesButton, searchButton;
        Android.Content.Res.ColorStateList colors;
        Android.Graphics.Typeface typeface;
        string toSearch;
        public override void OnBackPressed() // BackPressed or Save button clicked
        {
            try
            {
                if (string.IsNullOrEmpty(workEditText.Text)) throw new ArgumentNullException(); // not saving if text is empty
                if (work == null) work = new Work(); // if not loaded creating new work
                if (string.IsNullOrEmpty(titleEditText.Text)) titleEditText.Text = "Nowe dzieło (" + (SQLiteDb.GetWorks().Where(w => w.Title.Contains("Nowe dzieło")).Count()+1) + ")"; // filling title if empty
                if (work.Text != workEditText.Text || work.Title != titleEditText.Text) //saving work if changed or new
                {
                    work.Text = workEditText.Text;
                    work.Title = titleEditText.Text;
                    work.Modified = DateTime.Now;
                    SQLiteDb.SaveWork(work);
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            catch
            {
                Toast.MakeText(this, "Nie zapisano", ToastLength.Short).Show();
            }
            base.OnBackPressed();
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.WorkLayout);

            // View elements
            var saveImageButton = FindViewById<ImageButton>(Resource.Id.SaveWorkImageButton);
            titleEditText = FindViewById<EditText>(Resource.Id.TitleEditText);
            syllabelsTextView = FindViewById<TextView>(Resource.Id.SyllablesCountTextView);
            workEditText = FindViewById<EditText>(Resource.Id.WorkEditText);

            findSynonymsButton = FindViewById<Button>(Resource.Id.FindSynonymsButton);
            findRhymesButton = FindViewById<Button>(Resource.Id.FindRhymesButton);

            searchButton = FindViewById<Button>(Resource.Id.SearchButton);
            searchEditText = FindViewById<SearchView>(Resource.Id.SearchEditText);
            searchingLinearLayout = FindViewById<LinearLayout>(Resource.Id.SearchingLinearLayout);

            numberPickersLinearLayout = FindViewById<LinearLayout>(Resource.Id.NumberPickersLinearLayout);

            var matchingNumberTextView = FindViewById<TextView>(Resource.Id.MatchingNumberTextView);
            var matchingNumberDecreaseButton = FindViewById<Button>(Resource.Id.MatchingNumberDecreaseButton);
            var matchingNumberIncreaseButton = FindViewById<Button>(Resource.Id.MatchingNumberIncreaseButton);
            
            var lengthNumberTextView = FindViewById<TextView>(Resource.Id.LengthNumberTextView);
            var lengthNumberDecreaseButton = FindViewById<Button>(Resource.Id.LengthNumberDecreaseButton);
            var lengthNumberIncreaseButton = FindViewById<Button>(Resource.Id.LengthNumberIncreaseButton);

            var syllablesNumberTextView = FindViewById<TextView>(Resource.Id.SyllablesNumberTextView);
            var syllablesNumberDecreaseButton = FindViewById<Button>(Resource.Id.SyllablesNumberDecreaseButton);
            var syllablesNumberIncreaseButton = FindViewById<Button>(Resource.Id.SyllablesNumberIncreaseButton);


            syllabelsTextView.Visibility = ViewStates.Gone;
            findSynonymsButton.Click += FindSynonymsButton_Click;
            typeface = findSynonymsButton.Typeface;
            workEditText.TextChanged += WorkEditText_TextChanged;
            findRhymesButton.Click += FindRhymesButton_Click;

            //matching number picker
            {

                matchingNumberDecreaseButton.Click += (o, e) => //decreasing number of matching letters
                {
                    if (matchingNumberTextView.Text != "1") matchingNumberTextView.Text = (Convert.ToInt32(matchingNumberTextView.Text) - 1).ToString();
                };
                matchingNumberDecreaseButton.LongClick += (o, e) => // fast decreasing
                {
                    matchingNumberTextView.Text = "1";
                };
                matchingNumberIncreaseButton.Click += (o, e) => //increasing number of matching letters
                {
                    matchingNumberTextView.Text = (Convert.ToInt32(matchingNumberTextView.Text) + 1).ToString();
                };
            }

            //length number picker
            {

                lengthNumberDecreaseButton.Click += (o, e) => //decreasing number of words length
                {
                    if (lengthNumberTextView.Text != "1") lengthNumberTextView.Text = (Convert.ToInt32(lengthNumberTextView.Text) - 1).ToString();
                };
                lengthNumberDecreaseButton.LongClick += (o, e) => // fast decreasing
                {
                    lengthNumberTextView.Text = "1";
                };
                lengthNumberIncreaseButton.Click += (o, e) => //increasing number of words length
                {
                    lengthNumberTextView.Text = (Convert.ToInt32(lengthNumberTextView.Text) + 1).ToString();
                };
            }

            //syllables number picker
            {

                syllablesNumberDecreaseButton.Click += (o, e) => //decreasing number of syllables
                {
                    if (syllablesNumberTextView.Text != "1") syllablesNumberTextView.Text = (Convert.ToInt32(syllablesNumberTextView.Text) - 1).ToString();
                };
                syllablesNumberDecreaseButton.LongClick += (o, e) => // fast decreasing
                {
                    syllablesNumberTextView.Text = "1";
                };
                syllablesNumberIncreaseButton.Click += (o, e) => //increasing number of syllables
                {
                    syllablesNumberTextView.Text = (Convert.ToInt32(syllablesNumberTextView.Text) + 1).ToString();
                };
            }

            //search button clicked
            searchButton.Click += (o, e) =>
            {
                if (searchEditText.Query != "") // searching if not empty
                {
                    var progressBar = FindViewById<ProgressBar>(Resource.Id.SearchingContentLoadingProgressBar);
                    progressBar.Visibility = ViewStates.Visible;
                    var intent = new Intent(this, typeof(SearchActivity));
                    intent.PutExtra("KindOfSearching", toSearch);
                    intent.PutExtra("ToFind", searchEditText.Query);
                    if (toSearch == "Rhymes")
                    {
                        intent.PutExtra("MatchingNumber", Convert.ToInt32(matchingNumberTextView.Text));
                        intent.PutExtra("Length", Convert.ToInt32(lengthNumberTextView.Text));
                        intent.PutExtra("Syllables", Convert.ToInt32(syllablesNumberTextView.Text));
                        findRhymesButton.PerformClick();
                    }
                    else findSynonymsButton.PerformClick();
                    StartActivityForResult(intent, 1);
                    progressBar = null;
                    intent = null;
                }
            };

            //search button clicked on keayboard
            searchEditText.QueryTextSubmit += (o, e) =>
            {
                searchButton.PerformClick();
            };

            colors = findSynonymsButton.TextColors; //saving states of button text colors

            findSynonymsButton.SetTextColor(Android.Graphics.Color.Gray); //setting buttons text grey color
            findRhymesButton.SetTextColor(Android.Graphics.Color.Gray);

            // loading work if id was passed
            var choosenWorkId = Intent.GetIntExtra("choosenWork", 0);
            if (choosenWorkId != 0) 
            {
                work = SQLiteDb.GetWorks().First(w => w.Id == choosenWorkId);
                workEditText.Text = work.Text;
                titleEditText.Text = work.Title;
            }

            saveImageButton.Click += (o, e) => // saving work with save button
            {
                OnBackPressed();
            };
            CheckIfDataLoaded();
        }

        //passing data from SearchActivity
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if(requestCode == 1)
            {
                if(resultCode == Result.Ok)
                {
                    string result = data.GetStringExtra("SelectFoundedWord");
                    if (string.IsNullOrEmpty(workEditText.Text)) workEditText.Text = string.Concat(result.ToUpper().First(), string.Concat(result.Skip(1)), " ");
                    else if(Char.IsWhiteSpace(workEditText.Text.Last())) workEditText.Text += result;
                    else workEditText.Text += " " + result;
                }
            }
            var progressBar = FindViewById<ProgressBar>(Resource.Id.SearchingContentLoadingProgressBar);
            progressBar.Visibility = ViewStates.Gone;
            progressBar = null;
            base.OnActivityResult(requestCode, resultCode, data);
        }

        //choosen searching type
        private void ChooseSearchingType(Button button)
        {
            //if clicked is already choosen
            if (button.Typeface.IsBold) 
            {
                searchingLinearLayout.Visibility = ViewStates.Invisible;
                if (button == findRhymesButton)
                {
                    numberPickersLinearLayout.Visibility = ViewStates.Gone;
                }
                searchEditText.Iconified = true;
                button.SetTypeface(typeface, Android.Graphics.TypefaceStyle.Normal);
                toSearch = null;
            }
            //if data is not loaded
            else if (button.TextColors != colors) Toast.MakeText(this, "Ładowanie danych", ToastLength.Long).Show();
            //when toFind is null or clicked which was not already choosen
            else
            {
                searchingLinearLayout.Visibility = ViewStates.Visible;
                searchEditText.Iconified = false;
                button.SetTypeface(typeface, Android.Graphics.TypefaceStyle.Bold);
                if (button == findRhymesButton)
                {
                    numberPickersLinearLayout.Visibility = ViewStates.Visible;
                    findSynonymsButton.SetTypeface(typeface, Android.Graphics.TypefaceStyle.Normal);
                    toSearch = "Rhymes";
                }
                else
                {
                    toSearch = "Synonyms";
                    findRhymesButton.SetTypeface(typeface, Android.Graphics.TypefaceStyle.Normal);
                    numberPickersLinearLayout.Visibility = ViewStates.Gone;
                }
            }
        }
        private void FindRhymesButton_Click(object sender, EventArgs e)
        {
            ChooseSearchingType(findRhymesButton);
        }

        private void FindSynonymsButton_Click(object sender, EventArgs e)
        {
            ChooseSearchingType(findSynonymsButton);
        }

        private void CheckIfDataLoaded() // thread checking if WordLibraries are loaded
        {
            Thread dataLoaded = new Thread(dl =>
            {
                var rhymesLoaded = false;
                var synonymsLoaded = false;
                do
                {
                    if (SQLiteDb.RhymesLoaded())
                    {
                        rhymesLoaded = true;
                        this.RunOnUiThread(() =>
                        {
                            findRhymesButton.SetTextColor(colors);
                        });
                    }
                    if (SQLiteDb.SynonymsLoaded())
                    {
                        synonymsLoaded = true;
                        this.RunOnUiThread(() =>
                        {
                            findSynonymsButton.SetTextColor(colors);
                        });
                    }
                }
                while (!(rhymesLoaded && synonymsLoaded));
            });
            dataLoaded.Start();
        }

        private void WorkEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(syllabelsTextView.Text)) syllabelsTextView.Text = ""; //clearing textview content
            List<string> list = String.Concat(e.Text).Split("\n").ToList(); //loading words in text
            if (string.IsNullOrEmpty(String.Concat(e.Text))) syllabelsTextView.Visibility = ViewStates.Gone; //hide syllables edittext if text is empty
            else syllabelsTextView.Visibility = ViewStates.Visible;
            foreach (var line in list)
            {
                if (!string.IsNullOrWhiteSpace(line)) syllabelsTextView.Text += SQLiteDb.CountSyllables(line) + "\n"; // adding count of syllables in line
                else syllabelsTextView.Text += 0 + "\n";
            }
        }
    }
}