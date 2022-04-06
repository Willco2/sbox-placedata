using Sandbox;

namespace PlaceData;

class Game : GameBase {

	public Game() {
		Transmit = TransmitType.Always;
	}

	public override void Shutdown() { }
	public override void ClientJoined(Client cl) { }
	public override void ClientDisconnect(Client cl, NetworkDisconnectionReason reason) { }
	public override void PostLevelLoaded() { }
	public override void OnVoicePlayed(long playerId, float level) { }
	public override bool CanHearPlayerVoice(Client source, Client dest) => false;
	public override CameraSetup BuildCamera(CameraSetup camSetup) => camSetup with { Position = 0, Rotation = Rotation.Identity, ZNear = 1, ZFar = 1000, FieldOfView = 90 };

	public override void Spawn() {
		new Hud();
	}

}
