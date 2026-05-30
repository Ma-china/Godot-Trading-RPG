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

		if (GetTree().ChangeSceneToFile("res://Scenes/Room.tscn") != Error.Ok)
		{
			GD.PrintErr("Failed to load room scene.");
			return;
		}

		GD.Print("Connected to server");
	}

	// This method is called on the server when a new peer connects
	private void OnPeerConnected(long id)
	{
		GD.Print($"Player connected: {id}");

		if (!Multiplayer.IsServer())
			return;

		//spawn new player for the connected peers
		if(Rpc(nameof(SpawnRemotePlayer), (int)id) == Error.Ok)
		{
			GD.Print("Player spawn RPC sent successfully");
		}
		else
		{
			GD.PrintErr("Failed to send player spawn RPC");
		}

		//spawn the connected peers for the new player
		foreach (var peerId in Multiplayer.GetPeers())
		{
			if (peerId == (int)id)
				continue;

			RpcId((int)id, nameof(SpawnRemotePlayer), peerId);
		}
	}

	// This method will be called on all clients to spawn a new player for the connected peer
	[Rpc]
	private void SpawnRemotePlayer(int peerId)
	{
		if (peerId == (int)Multiplayer.GetUniqueId())
			return;

		if (GetTree().CurrentScene == null)
		{
			CallDeferred(nameof(SpawnRemotePlayer), peerId);
			return;
		}

		var scene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
		var player = scene.Instantiate<PlayerClickToMove>();

		player.Name = $"Player_{peerId}";
		player.SetMultiplayerAuthority(peerId);
		GetTree().CurrentScene.AddChild(player);
	}
}
