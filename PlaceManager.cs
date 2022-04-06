using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.IO;
using System;
using Willco2;

namespace PlaceData;

class PlaceManager : Panel {

	public static PlaceManager Instance { get; private set; }

	private const int DataSetSize = 16564308;
	private byte[] pixels;
	private Texture texture;
	private Image image;
	private TileEdit[] tileEdits;
	private int editIndex;
	private long currentTicks, minTicks, maxTicks, lastTimeDirection = 1, timeScale = 4320_0000000;
	private bool isAutoPlaying;

	private const float panThreshold = 10;
	private int scale = 1, totalEdits;
	private bool isPanningView, thresholdPassed;
	private Vector2 panOrigin, panStartOffset, panOffset;

	private RealTimeSince timeSinceUnpaused;
	private PlaceVisualizer visualizer;
	private Label timeScaleMonitor, clock, totalEditsMonitor;

	public PlaceManager() {
		Instance = this;
		pixels = new byte[1000 * 1000 * 4];
		Array.Fill(pixels, byte.MaxValue);

		//reformatCSV();
		//sortBin();
		loadTileEdits();
		currentTicks = new DateTime(2017, 3, 31, 17, 0, 0).Ticks;
		minTicks = currentTicks;
		maxTicks = tileEdits[^1].Ticks;

		StyleSheet.Load("PlaceManager.scss");
		texture = Texture.Create(1000, 1000).WithDynamicUsage().WithName("place_data").WithData(pixels).Finish();
		image = AddChild<Image>("image");
		image.Texture = texture;

		var labelContainer = Add.Panel("label-container");
		timeScaleMonitor = labelContainer.Add.Label("", "timescale");
		clock = labelContainer.Add.Label("");
		totalEditsMonitor = labelContainer.Add.Label("");

		visualizer = new PlaceVisualizer.MostCommon();
	}

	public override void Tick() {
		var tx = new PanelTransform();
		tx.AddTranslate(Length.Pixels(panOffset.x), Length.Pixels(panOffset.y));
		tx.AddScale(scale);
		image.Style.Transform = tx;

		int timeScaleMod = Input.Down(InputButton.Forward).ToInt() - Input.Down(InputButton.Back).ToInt();
		if (timeScaleMod == 1) {
			timeScale *= 51;
			timeScale /= 50;
		} else if (timeScaleMod == -1) {
			timeScale *= 49;
			timeScale /= 50;
		}
		timeScale = Math.Clamp(timeScale, 10_000_000, 1_000_000_000_000);

		if (Input.Pressed(InputButton.Jump)) {
			isAutoPlaying = !isAutoPlaying;
			if (isAutoPlaying) timeSinceUnpaused = 0;
		}

		long timeDirectionInput = Input.Down(InputButton.Right).ToInt() - Input.Down(InputButton.Left).ToInt();
		long timeDirection = timeDirectionInput;
		if (timeDirection != 0) lastTimeDirection = timeDirection;
		if (isAutoPlaying) timeDirection = lastTimeDirection;

		if (timeDirection != 0) {
			timeDirection *= timeScale / (long) (1 / Time.Delta);
			currentTicks += timeDirection;
			currentTicks = Math.Clamp(currentTicks, minTicks, maxTicks);

			int i = editIndex;
			while (timeDirection < 0 ? (tileEdits[i].Ticks > currentTicks) : (tileEdits[i].Ticks < currentTicks)) {
				visualizer.CommitEdit(tileEdits[i], timeDirection < 0);
				var sign = Math.Sign(timeDirection);
				i += sign;
				totalEdits += sign;
			}
			visualizer.ElapseTime(timeDirection);
			editIndex = i;
			texture.Update(pixels);
		}

		timeScaleMonitor.Text = $"{timeScale / 10_000_000}x {(lastTimeDirection < 0 ? "🡸" : "🡺")}";
		if (isAutoPlaying) {
			timeScaleMonitor.AddClass("autoplay");
		} else {
			timeScaleMonitor.RemoveClass("autoplay");
			timeScaleMonitor.SetClass("manual", timeDirectionInput != 0);
		}

		clock.Text = new DateTime(currentTicks).ToString();
		totalEditsMonitor.Text = $"Edits: {totalEdits}";
	}

	public override void OnMouseWheel(float value) {
		var delta = (int) value;
		if (delta < 0) {
			scale <<= Math.Abs(delta);
		} else {
			scale >>= delta;
		}
		if (scale < 1) panOffset = 0;
		scale = scale.Clamp(1, 128);
	}

	protected override void OnMouseMove(MousePanelEvent e) {
		if (isPanningView && Mouse.Position.Distance(panOrigin) >= panThreshold) thresholdPassed = true;
		if (isPanningView && thresholdPassed) panOffset = panStartOffset + (Mouse.Position - panOrigin) / scale;
	}

	protected override void OnMouseDown(MousePanelEvent e) {
		panOrigin = Mouse.Position;
		thresholdPassed = false;
		isPanningView = true;
		panStartOffset = panOffset;
	}

	protected override void OnMouseUp(MousePanelEvent e) => isPanningView = false;

	public void SetPixel(int x, int y, uint color) => SetPixel(y * 1000 + x, color);
	public void SetPixel(int index, uint color) {
		index *= 4;
		pixels[index] = (byte) ((color & 0xFF000000) >> 24);
		pixels[index + 1] = (byte) ((color & 0x00FF0000) >> 16);
		pixels[index + 2] = (byte) ((color & 0x0000FF00) >> 8);
		pixels[index + 3] = (byte) (color & 0x000000FF);
	}

	private static void reformatCSV() {
		using var stream = FileSystem.Data.OpenRead("place_tiles.csv");
		using var reader = new StreamReader(stream);
		using var output = FileSystem.Data.OpenWrite("place_tiles.bin");
		using var writer = new BinaryWriter(output);

		long lines = 0;
		string line;
		while ((line = reader.ReadLine()) != null) {
			var parts = line.Split(',');
			parts[0] = parts[0].Replace(" UTC", string.Empty);
			if (parts[0].Length == 19) parts[0] += ".";
			if (parts[0].Length == 20) parts[0] += "0";
			if (parts[0].Length == 21) parts[0] += "0";
			if (parts[0].Length == 22) parts[0] += "0";

			byte[] userHash = Convert.FromBase64String(parts[1]);
			ulong userHashA = 0, userHashB = 0;
			for (int i = 0; i < 8; i++) {
				userHashA |= (ulong) userHash[i] << (8 * i);
				userHashB |= (ulong) userHash[i + 8] << (8 * i);
			}

			uint color = TileColor.FromIndex(int.Parse(parts[4]));
			int x = int.Parse(parts[2]), y = int.Parse(parts[3]);
			if (x == 1000 || y == 1000) continue;

			writer.Write(DateTime.ParseExact(parts[0], "yyyy-MM-dd HH:mm:ss.fff", null).Ticks); // Int64
			writer.Write(userHashA); // UInt64
			writer.Write(userHashB); // UInt64
			writer.Write(x); // Int32
			writer.Write(y); // Int32
			writer.Write(color); // UInt32
			lines++;
		}
		$"Reformatted {lines} lines".DebugLog();
	}

	private static void sortBin() {
		TileEdit[] tileEdits = new TileEdit[DataSetSize];
		using (var stream = FileSystem.Data.OpenRead("place_tiles.bin"))
		using (var reader = new BinaryReader(stream)) {
			for (int i = 0; i < tileEdits.Length; i++) {
				tileEdits[i] = new() {
					Ticks = reader.ReadInt64(),
					UserHashA = reader.ReadUInt64(),
					UserHashB = reader.ReadUInt64(),
					X = reader.ReadInt32(),
					Y = reader.ReadInt32(),
					Color = reader.ReadUInt32()
				};
			}
		}

		Array.Sort(tileEdits);

		uint[] colors = new uint[DataSetSize];
		Array.Fill(colors, 0xFFFFFFFF);

		using (var output = FileSystem.Data.OpenWrite("place_tiles.bin"))
		using (var writer = new BinaryWriter(output)) {
			for (int i = 0; i < tileEdits.Length; i++) {
				writer.Write(tileEdits[i].Ticks); // Int64
				writer.Write(tileEdits[i].UserHashA); // UInt64
				writer.Write(tileEdits[i].UserHashB); // UInt64
				writer.Write(tileEdits[i].X); // Int32
				writer.Write(tileEdits[i].Y); // Int32
				writer.Write(tileEdits[i].Color); // UInt32

				int colorsIndex = tileEdits[i].Y * 1000 + tileEdits[i].X;
				writer.Write(colors[colorsIndex]); // UInt32
				colors[colorsIndex] = tileEdits[i].Color;
			}
		}
	}

	private void loadTileEdits() {
		tileEdits = new TileEdit[DataSetSize];
		using (var stream = FileSystem.Data.OpenRead("place_tiles.bin"))
		using (var reader = new BinaryReader(stream)) {
			for (int i = 0; i < tileEdits.Length; i++) {
				tileEdits[i] = new() {
					Ticks = reader.ReadInt64(),
					UserHashA = reader.ReadUInt64(),
					UserHashB = reader.ReadUInt64(),
					X = reader.ReadInt32(),
					Y = reader.ReadInt32(),
					Color = reader.ReadUInt32(),
					PreviousColor = reader.ReadUInt32()
				};
			}
		}
	}

}
