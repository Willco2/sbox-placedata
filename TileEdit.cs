using System;

namespace PlaceData;

readonly struct TileEdit : IComparable<TileEdit> {
	public long Ticks { get; init; }
	public ulong UserHashA { get; init; }
	public ulong UserHashB { get; init; }
	public int X { get; init; }
	public int Y { get; init; }
	public uint Color { get; init; }
	public uint PreviousColor { get; init; }

	public int CompareTo(TileEdit other) => Ticks.CompareTo(other.Ticks);
}
