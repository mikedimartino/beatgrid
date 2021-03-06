﻿namespace BeatGrid
{
	//  16 is probably the max that could fit on the screen.
	//	Can just add more measures and increase tempo to simulate 32nd, 64th, etc..
	public enum NoteType
	{
		Unknown,
		Whole,
		Half,
		Quarter,
		Eight,
		Sixteenth,
		ThirtySecond
	}

	public enum XOption
	{
		ClearMeasure,
		DeleteMeasure,
		DeleteBeat
	}
}
