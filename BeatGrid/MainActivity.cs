﻿using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using BeatGrid;
using SQLite;

namespace BeatGridAndroid
{
	[Activity(Label = "BeatGrid", MainLauncher = true, Icon = "@drawable/BeatGridLogo")]
	public class MainActivity : Activity
	{
		private TableLayout beatGridTable;
		private TableLayout.LayoutParams rowParams;
		private TableRow.LayoutParams cellParams;
		private TableRow.LayoutParams soundNameParams;

		private Typeface fontAwesome;

		private Button homeButton;
		private Button saveButton;
		private Button trashButton;
		private Button settingsButton;
		private Button previousButton;
		private Button playPauseButton;
		private Button nextButton;

		protected override void OnCreate(Bundle bundle)
		{
			fontAwesome = Typeface.CreateFromAsset(Assets, "fontawesome-webfont.ttf");

			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			InitLayout();
			DrawGrid(Measure.GetRandomTestMeasure());

			var tb = Beat.GetTestBeat();
			var tbRow = tb.ToDbRow();

			TestSQLite();
		}

		private void InitLayout()
		{
			#region BeatGrid Table
			beatGridTable = FindViewById<TableLayout>(Resource.Id.BeatGrid);

			// From http://stackoverflow.com/questions/2393847/how-can-i-get-an-android-tablelayout-to-fill-the-screen
			rowParams = new TableLayout.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent,
					1.0f);

			cellParams = new TableRow.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent,
					1.0f);
			cellParams.Width = 0;
			cellParams.SetMargins(5, 5, 5, 5);

			soundNameParams = new TableRow.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent,
					2.0f);
			soundNameParams.Width = 0;
			#endregion

			#region Top Bar Buttons
			homeButton = FindViewById<Button>(Resource.Id.HomeButton);
			saveButton = FindViewById<Button>(Resource.Id.SaveButton);
			trashButton = FindViewById<Button>(Resource.Id.TrashButton);
			settingsButton = FindViewById<Button>(Resource.Id.SettingsButton);
			previousButton = FindViewById<Button>(Resource.Id.PreviousButton);
			playPauseButton = FindViewById<Button>(Resource.Id.PlayPauseButton);
			nextButton = FindViewById<Button>(Resource.Id.NextButton);

			homeButton.SetTypeface(fontAwesome, TypefaceStyle.Normal);
			saveButton.SetTypeface(fontAwesome, TypefaceStyle.Normal);
			trashButton.SetTypeface(fontAwesome, TypefaceStyle.Normal);
			settingsButton.SetTypeface(fontAwesome, TypefaceStyle.Normal);
			previousButton.SetTypeface(fontAwesome, TypefaceStyle.Normal);
			playPauseButton.SetTypeface(fontAwesome, TypefaceStyle.Normal);
			nextButton.SetTypeface(fontAwesome, TypefaceStyle.Normal);
			#endregion

			#region Main Menu Button
			//mainMenuButton = FindViewById<Button>(Resource.Id.MainMenuButton);
			//mainMenuButton.SetTypeface(fontAwesome, TypefaceStyle.Normal);
			#endregion
		}

		private void DrawGrid(Measure measure)
		{
			int rows = measure.Cells.GetLength(0);
			int columns = measure.Cells.GetLength(1);

			for (int r = 0; r < rows; r++)
			{
				var row = new TableRow(this);

				// Sounds:
				var soundName = new TextView(this);
				soundName.Text = $"Sound {r}"; // Decide on max length
				soundName.Gravity = GravityFlags.CenterVertical;
				soundName.SetPadding(10, 0, 0, 0);
				row.AddView(soundName, soundNameParams);

				// Grid:
				for (int c = 0; c < columns; c++)
				{
					var background = measure.Cells[r, c].On ?
						Resource.Drawable.button_on :
						Resource.Drawable.button_off;

					var button = new Button(this);
					button.SetBackgroundResource(background);
					row.AddView(button, cellParams);
				}
				beatGridTable.AddView(row, rowParams);
			}
		}



		public void TestSQLite()
		{
			try
			{
				// From http://stackoverflow.com/questions/25882837/sqlite-could-not-open-database-file
				string applicationFolderPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "BeatGridDb");

				// Create the folder path.
				System.IO.Directory.CreateDirectory(applicationFolderPath);

				string databaseFileName = System.IO.Path.Combine(applicationFolderPath, "BeatGrid.db");

				SQLiteProvider provider = new SQLiteProvider(databaseFileName);
				provider.SaveBeat(Beat.GetTestBeat());
				var beats = provider.GetAllBeats();
				var firstBeat = provider.GetBeat(beats.First().Id);
			}
			catch(Exception ex) { }
		}



	}

}

