using System;
using System.Collections;
using System.Collections.Generic;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using SongWriter.Adapters;
using SongWriter.Model;
using AlertDialog = Android.App.AlertDialog;

namespace SongWriter.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        List<Work> worksList = new List<Work>();
        List<Work> worksToDelete = new List<Work>();
        ListView mainListView;
        bool deletingItems = false;
        Android.Support.V7.Widget.Toolbar toolbar;

        protected override void OnRestart() // reload list when coming back from work
        {
            base.OnRestart();
            worksList = SQLiteDb.GetWorks();
            UpdateAdapter();
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            //DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            //ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            //drawer.AddDrawerListener(toggle);
            //toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            worksList = SQLiteDb.GetWorks();
            mainListView = FindViewById<ListView>(Resource.Id.mainListView);
            mainListView.SetSelection(0);

            mainListView.ItemClick += MainListView_ItemClick;
            mainListView.ItemLongClick += MainListView_ItemLongClick;
            UpdateAdapter();
            SQLiteDb.StartApp(this); // starting load words to library
        }
        private void MainListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (!deletingItems) //moving to work view
            {
                var intent = new Intent(this, typeof(WorkActivity));
                intent.PutExtra("choosenWork", worksList[e.Position].Id);
                StartActivity(intent);
                mainListView.SetSelection(0);
            }
            else // deleting items
            {
                var title = e.View.FindViewById<TextView>(Android.Resource.Id.Text1);
                var text = e.View.FindViewById<TextView>(Android.Resource.Id.Text2);
                var work = worksList[e.Position];
                if(!worksToDelete.Contains(work)) //selecting item to delete
                {
                    worksToDelete.Add(work);
                    title.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.White));
                    text.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.White));
                }
                else // deselecting item
                {
                    worksToDelete.Remove(work);
                    title.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.DarkGray));
                    text.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.DarkGray));
                }
            }
        }

        private void MainListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e) //calling delete items
        {
            Toast.MakeText(this, "Usuwanie", ToastLength.Short).Show();
            toolbar.Menu.FindItem(Resource.Id.deleteItems).SetVisible(true);
            toolbar.Menu.FindItem(Resource.Id.cancelDeletingItems).SetVisible(true);
            worksToDelete = new List<Work>();
            deletingItems = true;
            mainListView.ChoiceMode = ChoiceMode.Multiple;
            mainListView.PerformItemClick(e.View, e.Position, e.Id); // selecting long-clicked item to delete
        }

        private void UpdateAdapter()
        {

            try //loading works to adapter
            {
                    MainListViewAdapter adapter = new MainListViewAdapter(this, worksList);
                    mainListView.Adapter = adapter;
            }
            catch (Exception exception)
            {
                AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                AlertDialog alert = dialog.Create();
                alert.SetTitle(exception.Source);
                alert.SetMessage(exception.Message);
                alert.SetButton("OK", (c, ev) =>
                {
                    alert.Hide();
                });
                alert.Show();
            }
        }

        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if(drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item) 
        {
            int id = item.ItemId;
            if (id == Resource.Id.deleteItems)
            {
                toolbar.Menu.FindItem(Resource.Id.deleteItems).SetVisible(false);
                toolbar.Menu.FindItem(Resource.Id.cancelDeletingItems).SetVisible(false);
                deletingItems = false;
                SQLiteDb.DeleteWorks(worksToDelete);
                worksList = SQLiteDb.GetWorks();
                UpdateAdapter();
                return true;
            }
            if (id == Resource.Id.cancelDeletingItems)
            {
                toolbar.Menu.FindItem(Resource.Id.deleteItems).SetVisible(false);
                toolbar.Menu.FindItem(Resource.Id.cancelDeletingItems).SetVisible(false);
                UpdateAdapter();
                deletingItems = false;
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            var intent = new Intent(this, typeof(WorkActivity));
            StartActivity(intent);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.nav_camera)
            {
                // Handle the camera action
            }
            else if (id == Resource.Id.nav_gallery)
            {

            }
            else if (id == Resource.Id.nav_slideshow)
            {

            }
            else if (id == Resource.Id.nav_manage)
            {

            }
            else if (id == Resource.Id.nav_share)
            {

            }
            else if (id == Resource.Id.nav_send)
            {

            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

