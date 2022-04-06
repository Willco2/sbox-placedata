using Sandbox;
using System;
using Willco2;

namespace PlaceData;

abstract class PlaceVisualizer {

	private readonly PlaceManager manager;

	public PlaceVisualizer() {
		manager = PlaceManager.Instance;
	}

	public abstract void CommitEdit(TileEdit edit, bool rewind);
	public virtual void ElapseTime(long ticks) { }

	public class Standard : PlaceVisualizer {
		public override void CommitEdit(TileEdit edit, bool rewind) => manager.SetPixel(edit.X, edit.Y, rewind ? edit.PreviousColor : edit.Color);
	}

	public class MostCommon : PlaceVisualizer {
		private int[][] histogram;

		public MostCommon() : base() {
			histogram = new int[1000 * 1000][];
			for (int i = 0; i < histogram.Length; i++) {
				histogram[i] = new int[16];
			}
		}

		public override void CommitEdit(TileEdit edit, bool rewind) {
			int tileIndex = edit.Y * 1000 + edit.X;
			int[] tile = histogram[tileIndex];
			tile[TileColor.ToIndex(edit.Color)] -= rewind.ToSignedInt();
			int mostCommonCount = 0, mostCommonIndex = 0;
			for (int i = 0; i < tile.Length; i++) {
				if (tile[i] > mostCommonCount) {
					mostCommonCount = tile[i];
					mostCommonIndex = i;
				}
			}

			manager.SetPixel(tileIndex, TileColor.FromIndex(mostCommonIndex));
		}
	}

}
