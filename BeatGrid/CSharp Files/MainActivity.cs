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
using BeatGrid.ViewModel;
using System.Collections.Generic;
using Android.Media;

namespace BeatGridAndroid
{
	[Activity(Label = "BeatGrid", MainLauncher = true, Icon = "@drawable/BeatGridLogo", ScreenOrientation = Android.Content.PM.ScreenOrientation.UserLandscape)]
	public class MainActivity : Activity
	{
		private MainViewModel _mvm;

		#region UI Related
		private TableLayout _beatGridTable;
		private TableLayout.LayoutParams _rowParams;
		private TableRow.LayoutParams _cellParams;
		private TableRow.LayoutParams _soundNameParams;

		private Button _openButton; 
		private Button _saveButton;
		private Button _xButton;
		//private Button _trashButton;
		private Button _settingsButton;
		private Button _previousButton;
		private Button _playPauseButton;
		private Button _nextButton;

		private Dictionary<string, Button> _cellButtons;
		// key = cell coordinate, value = off color res id (since color will alternate per beat)
		private Dictionary<string, int> _cellOffColorResIds;
		#endregion

		#region Global properties
		public Typeface FontAwesome { get; set; }
		public List<Sound> AllSounds { get;set; }
		public SoundManager SoundManager { get; set; }
		#endregion

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			Window.RequestFeature(WindowFeatures.NoTitle);

			FontAwesome = Typeface.CreateFromAsset(Assets, "fontawesome-webfont.ttf");

			_mvm = new MainViewModel(GetSQLiteProvider());

			_mvm.MeasureChanged += OnMeasureChanged;
			_mvm.BeatChanged += OnBeatChanged;
			_mvm.PlaySoundsList += OnPlaySounds;

			SetContentView(Resource.Layout.Main);
			InitLayout();

			_cellButtons = new Dictionary<string, Button>();
			_cellOffColorResIds = new Dictionary<string, int>();

			SoundManager = new SoundManager(this);

			AllSounds = SoundManager.AllSounds;

			DrawMeasure(_mvm.CurrentBeat.CurrentMeasure);
		}

		// Initialize buttons and other layout items
		private void InitLayout()
		{
			#region BeatGrid Table
			_beatGridTable = FindViewById<TableLayout>(Resource.Id.BeatGrid);

			// From http://stackoverflow.com/questions/2393847/how-can-i-get-an-android-tablelayout-to-fill-the-screen
			_rowParams = new TableLayout.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent,
					1.0f);

			_cellParams = new TableRow.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent,
					1.0f);
			_cellParams.Width = 0;
			_cellParams.SetMargins(5, 5, 5, 5);

			_soundNameParams = new TableRow.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent,
					2.0f);
			_soundNameParams.Width = 0;
			#endregion

			#region Top Bar Buttons
			// Maybe just call MVM directly instead of these events?
			_saveButton = FindViewById<Button>(Resource.Id.SaveButton);
			_saveButton.SetTypeface(FontAwesome, TypefaceStyle.Normal);
			_saveButton.Click += OnSaveBeatClicked;

			_settingsButton = FindViewById<Button>(Resource.Id.SettingsButton);
			_settingsButton.SetTypeface(FontAwesome, TypefaceStyle.Normal);
			_settingsButton.Click += OnSettingsClicked;

			_openButton = FindViewById<Button>(Resource.Id.OpenButton);
			_openButton.SetTypeface(FontAwesome, TypefaceStyle.Normal);
			_openButton.Click += OnOpenBeatClicked;

			_xButton = FindViewById<Button>(Resource.Id.XButton);
			_xButton.SetTypeface(FontAwesome, TypefaceStyle.Normal);
			_xButton.Click += OnXClicked;

			_previousButton = FindViewById<Button>(Resource.Id.PreviousButton);
			_previousButton.SetTypeface(FontAwesome, TypefaceStyle.Normal);

			_playPauseButton = FindViewById<Button>(Resource.Id.PlayPauseButton);
			_playPauseButton.SetTypeface(FontAwesome, TypefaceStyle.Normal);
			_playPauseButton.Click += OnPlayPauseClicked;

			_nextButton = FindViewById<Button>(Resource.Id.NextButton);
			_nextButton.SetTypeface(FontAwesome, TypefaceStyle.Normal);

			#endregion

		}

		private void DrawMeasure(Measure measure) //TODO: Change to DrawBeat(Beat beat, int measure)
		{
			_cellButtons.Clear();
			_cellOffColorResIds.Clear();
			_beatGridTable.RemoveAllViews();

			int rows = Constants.MAX_ACTIVE_SOUNDS;
			int columns = measure.Cells.GetLength(1);

			for (int r = 0; r < rows; r++)
			{
				var row = new TableRow(this);

				Sound sound = _mvm.CurrentBeat.Sounds[r];

				var soundName = new TextView(this);
				soundName.Text = sound.ShortName;
				soundName.Gravity = GravityFlags.CenterVertical;
				soundName.SetPadding(10, 0, 0, 0);

				soundName.Click += (sender, eventArgs) => { OnSoundClicked(sound); };
				soundName.LongClick += (sender, eventArgs) => { OnSoundLongClicked(sound); };

				row.AddView(soundName, _soundNameParams);

				// Grid:
				for (int c = 0; c < columns; c++)
				{
					Cell cell = measure.Cells[r, c];

					var button = new Button(this);
					button.Click += (sender, eventArgs) => { OnCellClicked(cell); };

					
					_cellButtons.Add(cell.GetCoordinate(), button);

					int offColorResId = _mvm.CurrentBeat.CellShouldUseAlternateOffColor(cell) ?
						Resource.Drawable.button_off2 : Resource.Drawable.button_off1;
					_cellOffColorResIds.Add(cell.GetCoordinate(), offColorResId);

					row.AddView(button, _cellParams);

					DrawCell(cell);
				}
				_beatGridTable.AddView(row, _rowParams);
			}
		}

		#region Event Handlers
		private void OnMeasureChanged(object source, MeasureEventArgs e)
		{
			foreach (var cell in e.Measure.Cells) DrawCell(cell);
		}

		private void OnBeatChanged(object source, BeatEventArgs e)
		{
			OnMeasureChanged(this, new MeasureEventArgs(e.Beat.CurrentMeasure));
			//TODO: Handle other stuff
		}

		private void OnCellClicked(Cell cell)
		{
			_mvm.ToggleCell(cell);
			DrawCell(cell);
		}

		private void OnSoundClicked(Sound sound)
		{
			SoundManager.PlaySound(sound);
		}

		private void OnSoundLongClicked(Sound sound)
		{
			var soundLibraryDialog = new SoundLibraryDialogFragment(_mvm, SoundManager);
			soundLibraryDialog.Show(FragmentManager.BeginTransaction(), "sound_library_dialog_fragment");
		}

		private void OnPlaySounds(object source, PlaySoundsListEventArgs e)
		{
			SoundManager.PlaySounds(e.SoundFileNames);
		}

		#region XClicked
		public void OnXClicked(object sender, EventArgs e)
		{
			var xDialog = new XDialogFragment();
			xDialog.XOptionSelected += OnXSelectionMade;
			xDialog.Show(FragmentManager.BeginTransaction(), "x_dialog_fragment");
		}

		public void OnXSelectionMade(object source, XOptionEventArgs e)
		{
			switch (e.Option)
			{
				case XOption.ClearMeasure:
					_mvm.ClearCurrentMeasure();
					break;
				case XOption.DeleteMeasure:
					_mvm.DeleteCurrentMeasure();
					break;
				case XOption.DeleteBeat:
					_mvm.DeleteCurrentBeat();
					break;
			}
		}
		#endregion


		#region OpenBeat
		/// <summary>
		/// Called when the open beat button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnOpenBeatClicked(object sender, EventArgs e)
		{
			var transaction = FragmentManager.BeginTransaction();
			var openBeatDialog = new OpenBeatDialogFragment();
			openBeatDialog.BeatSelected += OnOpenBeatSelected;
			openBeatDialog.Show(transaction, "open_beat_dialog_fragment");
		}

		/// <summary>
		/// Called when user has selected a beat in the open beat dialog.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		public void OnOpenBeatSelected(object source, OpenBeatEventArgs e)
		{
			_mvm.OpenBeat(e.Id);
			//TODO: Show prompt if unsaved changes.
			//TODO: Open beat.
		}
		#endregion

		#region SaveBeat
		public void OnSaveBeatClicked(object sender, EventArgs e)
		{
			//_mvm.OnSaveClick();
		}
		#endregion

		#region Settings
		public void OnSettingsClicked(object sender, EventArgs e)
		{
			var transaction = FragmentManager.BeginTransaction();
			var beatSettingsDialog = new BeatSettingsDialogFragment(_mvm);
			beatSettingsDialog.Show(transaction, "beat_settings_dialog_fragment");
		}
		#endregion

		public void OnPlayPauseClicked(object sender, EventArgs e)
		{
			_mvm.PlayPauseBeat();
			if (_mvm.IsPlaying)
			{
				_playPauseButton.Text = Resources.GetString(Resource.String.icon_pause);
			}
			else
			{
				_playPauseButton.Text = Resources.GetString(Resource.String.icon_play);
			}
		}

		#endregion

		// Assumes that the cell coordinate will map to an existing position in the measure
		private void DrawCell(Cell cell)
		{
			int backgroundResId = cell.On ? Resource.Drawable.button_on 
				: _cellOffColorResIds[cell.GetCoordinate()];
			Button button = _cellButtons[cell.GetCoordinate()];
			button.SetBackgroundResource(backgroundResId);
		}

		private SQLiteProvider GetSQLiteProvider()
		{
			// From http://stackoverflow.com/questions/25882837/sqlite-could-not-open-database-file
			string applicationFolderPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "BeatGridDb");

			// Create the folder path.
			System.IO.Directory.CreateDirectory(applicationFolderPath);

			string databaseFileName = System.IO.Path.Combine(applicationFolderPath, "BeatGrid.db");

			return new SQLiteProvider(databaseFileName);
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
				provider.SaveBeat(new Beat());
				var beats = provider.GetAllBeats();
				var firstBeat = provider.GetBeat(beats.First().Id);
			}
			catch(Exception) { }
		}


	}
}

