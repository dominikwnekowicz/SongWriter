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
using SQLite;

namespace SongWriter.Model
{
    public class Work
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Title { get; set; }

        [NotNull]
        public string Text { get; set; }

        [NotNull]
        public DateTime Modified { get; set; }
    }
}