using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
namespace PlaceData;

class Hud : HudEntity<UnscaledRootPanel> {

	public override void ClientSpawn() {
		RootPanel.AddChild<PlaceManager>();
	}

}

class UnscaledRootPanel : RootPanel {
	protected override void UpdateScale(Rect screenSize) { Scale = 1; }
}
