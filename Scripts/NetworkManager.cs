using Godot;

public partial class NetworkManager : Node
{
	private ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

	public void HostGame()
	{
		peer.CreateServer(12345);
		Multiplayer.MultiplayerPeer = peer;

		Multiplayer.PeerConnected += OnPeerConnected;

		GD.Print("Server started");
	}

	public void JoinGame(string ip)
	{
		peer.CreateClient(ip, 12345);
		Multiplayer.MultiplayerPeer = peer;

		GD.Print("Connected to server");
	}

	private void OnPeerConnected(long id)
	{
		GD.Print($"Player connected: {id}");

		if (Multiplayer.IsServer())
			SpawnPlayer(id);
	}

	private void SpawnPlayer(long id)
	{
		var scene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
		var player = scene.Instantiate<Node2D>();

		player.Name = id.ToString();
		GetTree().CurrentScene.AddChild(player);
	}
}
