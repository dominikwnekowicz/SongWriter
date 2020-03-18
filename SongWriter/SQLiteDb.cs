using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
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
using SQLite;

namespace SongWriter
{
	public class SQLiteDb
	{
		static readonly SQLiteDb sQLiteDb = new SQLiteDb();
		private List<string> foundWordsList = new List<string>();
		private Hashtable synonyms, vulgarity;
		private List<IEnumerable<string>> rhymes = new List<IEnumerable<string>>();
		private bool rhymesLoaded = false, synonymsLoaded = false;
		private static SQLiteConnection Connect()
		{
			string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			string path = Path.Combine(documentsPath, "ArtisticWriter.db3");
			const SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create;
			SQLiteConnection db = new SQLiteConnection(path, Flags);
			return db;
		}

		public static bool RhymesLoaded()
		{
			return sQLiteDb.rhymesLoaded;
		}
		public static bool SynonymsLoaded()
		{
			return sQLiteDb.synonymsLoaded;
		}

		private static bool TableExist(SQLiteConnection db, string tableName)
		{
			try
			{
				if (db.GetTableInfo(tableName).Count > 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static List<Work> GetWorks()
		{
			var db = Connect();
			if (!TableExist(db, "Work")) db.CreateTable<Work>();
			var worksList = db.Table<Work>().OrderByDescending(w => w.Modified).ToList();
			return worksList;
		}

		public static bool SaveWork(Work work)
		{
			try
			{
				var db = Connect();
				if (!TableExist(db, "Work")) db.CreateTable<Work>();
				if (db.Table<Work>().Any(w => w.Id == work.Id)) db.Update(work); //updates if exist
				else db.Insert(work);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static void StartApp(Activity activity)
		{
			sQLiteDb.LoadData(activity);
		}
		public static int CountSyllables(string line)
		{
			var wordsInLine = line.Split(' '); //get words in line
			int syllableCount = 0;
			foreach (var word in wordsInLine)
			{
				var vowels = Regex.Split(word.ToLower(), "[bcćdfghjklłmnńpqrsśtvwxzźż]"); //deletes consonants from word
				syllableCount += vowels.Where(w => !string.IsNullOrWhiteSpace(w.ToLower()) && w.All(l => Char.IsLetter(l))).ToList().Count; //count vowels in word
			}
			return syllableCount;
		}

		private bool NotContainsVulgarity(string word)
		{
			if (sQLiteDb.vulgarity.ContainsKey(word)) return false;
			else return true;
		}

		public static List<string> FindSynonyms(string word, Activities.SearchActivity activity)
		{
			if (sQLiteDb.synonyms == null) sQLiteDb.LoadData(activity); //load if not loaded
			var synonymsHashtable = new Hashtable();
			sQLiteDb.foundWordsList = new List<string>();
			var start = DateTime.Now;
			foreach (var s in sQLiteDb.synonyms.Keys.OfType<string>())
			{
				var tempS = ";" + s.ToLower() + ";"; //adding first and last char in line (to check for full words)
				if(tempS.Contains(";"+word.ToLower()+";"))
				{
					var elementsList = s.Split(';');
					foreach(var item in elementsList)
					{
						if (sQLiteDb.NotContainsVulgarity(item.ToLower())) //checking if contains vulgarity
						{
							try
							{
								synonymsHashtable.Add(item, item);
							}
							catch
							{

							}
						}
					}
					elementsList = null;
				}
			}
			if(synonymsHashtable.Count == 0) //if not found find to similar words
			{
				string tmp;
				int min = word.Length / 2;
				for(int i = word.Length; i>=min; i--)
				{
					try
					{
						tmp = word.Remove(i).ToLower();
					}
					catch
					{
						tmp = word.ToLower();
					}

					foreach (var s in sQLiteDb.synonyms.Keys.OfType<string>())
					{
						var tempS = ";" + s.ToLower() + ";"; //adding first and last char in line (to check for full words)
						if (tempS.Contains(";" + tmp.ToLower() + ";"))
						{
							var elementsList = s.Split(';');
							foreach (var item in elementsList)
							{
								if (sQLiteDb.NotContainsVulgarity(item.ToLower())) //checking if contains vulgarity
								{
									try
									{
										synonymsHashtable.Add(item, item);
									}
									catch
									{

									}
								}
							}
							elementsList = null;
						}
					}
					if(synonymsHashtable.Count > 0) break;

					//if (sQLiteDb.synonyms.Keys.OfType<string>().Where(s => s.Contains(";" + tmp.ToLower())).Any()) //if contains a word reduced by i letters
					//{
					//	foreach (var s in sQLiteDb.synonyms.Keys.OfType<string>())
					//	{
					//		var tempS = ";" + s.ToLower();
					//		if (tempS.Contains(";" + tmp.ToLower()))
					//		{
					//			var elementsList = s.Split(';');
					//			foreach (var item in elementsList)
					//			{
					//				if (sQLiteDb.NotContainsVulgarity(item.ToLower()))
					//				{
					//					try
					//					{
					//						synonymsHashtable.Add(item, item);
					//					}
					//					catch
					//					{

					//					}
					//				}
					//			}
					//			elementsList = null;
					//		}
					//	}
					//	break;
					//}
				}
			}
			sQLiteDb.foundWordsList = synonymsHashtable.Keys.OfType<string>().OrderBy(synonym => synonym).ToList();
			var finish = DateTime.Now;
			if (sQLiteDb.foundWordsList.Any())
			{
				activity.SetTime(Math.Round(finish.Subtract(start).TotalSeconds, 2));
			}
			return sQLiteDb.foundWordsList;
		}
		
		public static List<string> FindRhymes(string word, int matchingNumber, int length, int syllables, Activities.SearchActivity activity)
		{
			if (sQLiteDb.rhymes == null) sQLiteDb.LoadData(activity);
			sQLiteDb.foundWordsList = new List<string>();
			var correctWord = String.Concat(word.Where(c => Char.IsLetter(c)));
			var toFind = correctWord.Substring(correctWord.Length - matchingNumber);
			// searchingCount = 0;
			//foundRhymesList = sQLiteDb.rhymes.Keys.OfType<string>().Where(w => w.Length >= matchingNumber && w.Substring(w.Length - matchingNumber).ToLower() == toFind && w.Length <= length && CountSyllables(w) == syllables).OrderBy(word => word).ToList(); //searching for matching rhymes
			var start = DateTime.Now;
			Parallel.ForEach(sQLiteDb.rhymes, rhymesEnum =>
			{
				sQLiteDb.foundWordsList.AddRange(rhymesEnum.Where(rhyme => rhyme.Length >= matchingNumber && rhyme.Substring(rhyme.Length - matchingNumber).ToLower() == toFind && rhyme.Length <= length && CountSyllables(rhyme) == syllables));
				//if (foundRhymesList.Count > searchingCount)
				//{
				//	try
				//	{
				//		activity.ShowPartOfList(foundRhymesList);
				//		searchingCount = foundRhymesList.Count;
				//	}
				//	catch
				//	{

				//	}
				//}
			});
			var finish = DateTime.Now;
			if(sQLiteDb.foundWordsList.Any())
			{
				activity.SetTime(Math.Round(finish.Subtract(start).TotalSeconds, 2));
			}
			return sQLiteDb.foundWordsList.AsParallel().Distinct().OrderBy(f => f).ToList();
		}

		private void LoadData(Activity activity)
		{
			// loading rhymes
			Thread rhymesThread = new Thread(wt => {
				var start = DateTime.Now;
				try
				{
					var assembly = Assembly.GetExecutingAssembly();
					var resourceName = "SongWriter.WordLibraries.rhymes.txt"; 
					Stream stream = assembly.GetManifestResourceStream(resourceName); //load words to stream
					StreamReader reader = new StreamReader(stream);
					string rhymesString = reader.ReadToEnd(); // reading to string
					var rhymesTable = rhymesString.Split(',');
					var n = 5;
					var length = rhymesTable.Length / n;
					Parallel.For(1, n+1, i =>
					{
						if (i != n)
						{
							rhymes.Add(rhymesTable.Skip((i - 1) * length).Take(length));
						}
						else
						{
							rhymes.Add(rhymesTable.Skip((n-1) * length));
						}
					});
				}
				catch
				{
					rhymesLoaded = false;
				}
				finally
				{
					rhymesLoaded = true;
				}
				var finish = DateTime.Now;
				activity.RunOnUiThread(() =>
				{
					Toast.MakeText(activity, "Substract: " + finish.Subtract(start), ToastLength.Long).Show();
				});
			});
			rhymesThread.Start();

			// loading synonyms
			Thread synonymsThread = new Thread(st => {
				try
				{
					synonyms = new Hashtable();
					var assembly = Assembly.GetExecutingAssembly();
					var resourceName = "SongWriter.WordLibraries.synonyms.txt";
					Stream stream = assembly.GetManifestResourceStream(resourceName);
					StreamReader reader = new StreamReader(stream);
					string synonymsString = reader.ReadToEnd();
					var synonymsTable = synonymsString.Split("\n"); //splitting lines
					foreach (var s in synonymsTable)
					{
						var tmp = s;
						if (s.Contains('(')) // removing texts between ()
						{
							do
							{
								try
								{
									tmp = tmp.Remove(tmp.IndexOf('('), (tmp.IndexOf(')') - tmp.IndexOf('(') + 2));
								}
								catch
								{
									tmp = tmp.Remove(tmp.IndexOf('('), (tmp.IndexOf(')') - tmp.IndexOf('(') + 1));
								}
							}
							while (tmp.Contains('('));
						}
						try
						{
							synonyms.Add(tmp, tmp); //adding synonym to hashtable
						}
						catch (Exception) // if contains key
						{

						};

					};
				}
				catch
				{
					synonymsLoaded = false;
				}
				finally
				{
					synonymsLoaded = true;
				}
				
			});
			synonymsThread.Start();

			//loading vulgarity
			Thread vulgarityThread = new Thread(vt => {
				vulgarity = new Hashtable();
				var assembly = Assembly.GetExecutingAssembly();
				var resourceName = "SongWriter.WordLibraries.vulgarity.txt";
				Stream stream = assembly.GetManifestResourceStream(resourceName);
				StreamReader reader = new StreamReader(stream);
				string vulgarityString = reader.ReadToEnd();
				var vulgarityTable = vulgarityString.Split(','); //spliting words
				foreach (var v in vulgarityTable)
				{
					try
					{
						vulgarity.Add(v, v); //adding vulgarity to hashtable
					}
					catch (Exception) //if contains key
					{

					}

				};
			});
			vulgarityThread.Start();
		}

		public static void DeleteWorks(List<Work> worksToDelete) //deletng works from db
		{
			var db = Connect();
			foreach (var work in worksToDelete)
			{
				db.Delete(work);
			}
		}
	}
}