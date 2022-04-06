namespace PlaceData;

static class TileColor {
	public static uint FromIndex(int index) => index switch {
		0 => 0xFFFFFFFF,
		1 => 0xE4E4E4FF,
		2 => 0x888888FF,
		3 => 0x222222FF,
		4 => 0xFFA7D1FF,
		5 => 0xE50000FF,
		6 => 0xE59500FF,
		7 => 0xA06A42FF,
		8 => 0xE5D900FF,
		9 => 0x94E044FF,
		10 => 0x02BE01FF,
		11 => 0x00E5F0FF,
		12 => 0x0083C7FF,
		13 => 0x0000EAFF,
		14 => 0xE04AFFFF,
		15 => 0x820080FF,
		_ => 0xFFFFFFFF
	};

	public static int ToIndex(uint color) => color switch {
		0xFFFFFFFF => 0,
		0xE4E4E4FF => 1,
		0x888888FF => 2,
		0x222222FF => 3,
		0xFFA7D1FF => 4,
		0xE50000FF => 5,
		0xE59500FF => 6,
		0xA06A42FF => 7,
		0xE5D900FF => 8,
		0x94E044FF => 9,
		0x02BE01FF => 10,
		0x00E5F0FF => 11,
		0x0083C7FF => 12,
		0x0000EAFF => 13,
		0xE04AFFFF => 14,
		0x820080FF => 15,
		_ => 0
	};
}
