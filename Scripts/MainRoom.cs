using Godot;
using System;

public partial class MainRoom : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var scene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
		var player = scene.Instantiate<PlayerClickToMove>();

		player.Name = $"Player_{Multiplayer.GetUniqueId()}";
		player.SetMultiplayerAuthority((int)Multiplayer.GetUniqueId());
		GetTree().CurrentScene.AddChild(player);
	}
}
