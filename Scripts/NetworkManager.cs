using System.Collections.Generic;
using System.Data;
using Godot;

public partial class NetworkManager : Node
{
	private ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

	private Godot.Collections.Dictionary<long, string> playerInfo = new Godot.Collections.Dictionary<long, string>();
	
	public override void _Ready()
	{
		Multiplayer.PeerConnected += OnPeerConnected;
		Multiplayer.ConnectedToServer += OnConnectOk;
		Multiplayer.PeerDisconnected += OnPlayerDisconnected;
		Multiplayer.ServerDisconnected += OnServerDisconnected;
	}

	public void HostGame()
	{
		peer.CreateServer(12345);
		Multiplayer.MultiplayerPeer = peer;

		if (GetTree().ChangeSceneToFile("res://Scenes/Room.tscn") != Error.Ok)
		{
			GD.PrintErr("Failed to load room scene.");
			return;
		}

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

	private void OnServerDisconnected()
	{
		GD.Print("Disconnected from server");
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}

	private void OnPlayerDisconnected(long id)
	{
		GD.Print($"Player disconnected: {id}");
		playerInfo.Remove(id);

		GetTree().CurrentScene.GetNodeOrNull($"Player_{id}")?.QueueFree();

		Rpc(nameof(RemovePlayer), id);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void RemovePlayer(long id)
	{
		GetTree().CurrentScene.GetNodeOrNull($"Player_{id}")?.QueueFree();
	}

	private void OnConnectOk()
	{
		GD.Print("Successfully connected to server");

		// Send player info to the server
		string playerName = MainMenu.GetPlayerName();
		RpcId(1, nameof(CreatePlayerOnServer), playerName);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void CreatePlayerOnServer(string playerName)
	{
		if (!Multiplayer.IsServer())
			return;

		long peerId = Multiplayer.GetRemoteSenderId();
		playerInfo[peerId] = playerName;
		GD.Print($"Player {playerName} connected with ID {peerId}");

		SpawnPlayer(peerId, playerName);

		Rpc(nameof(SpawnPlayer), peerId, playerName);
	}

	// This method is called on the server when a new peer connects
	private void OnPeerConnected(long id)
	{
		GD.Print($"Player connected: {id}");

		if (!Multiplayer.IsServer())
			return;

		RpcId((int)id, nameof(SpawnOthersForNewPlayer), playerInfo);
	}

	[Rpc]
	private void SpawnOthersForNewPlayer(Godot.Collections.Dictionary<long, string> existingPlayers)
	{
		foreach (var kvp in existingPlayers)
		{
			long peerId = kvp.Key;
			string playerName = kvp.Value;
			if (peerId == Multiplayer.GetUniqueId())
				continue;

			SpawnPlayer(peerId, playerName);
		}
	}


	// This method will be called on all clients to spawn a new player for the connected peer
	[Rpc]
	private void SpawnPlayer(long peerId, string playerName)
	{
		if (peerId == Multiplayer.GetUniqueId())
			return;

		if (GetTree().CurrentScene == null)
		{
			CallDeferred(nameof(SpawnPlayer), peerId, playerName);
			return;
		}

		var scene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
		var player = scene.Instantiate<PlayerClickToMove>();

		player.Name = $"Player_{peerId}";
		player.SetMultiplayerAuthority((int)peerId);
		player.SetPlayerName(playerName);
		GetTree().CurrentScene.AddChild(player);
	}
}
