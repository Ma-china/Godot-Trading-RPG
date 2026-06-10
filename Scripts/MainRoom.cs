using Godot;
using System;

public partial class MainRoom : Node
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	public float cameraZoom = 1.5f;
	
	public int money = 0;
	public override void _Ready()
	{
		if (Multiplayer.IsServer())
		{
			var serverCamera = GetNode<Camera2D>("ServerCamera");
			serverCamera.MakeCurrent();
			serverCamera.Zoom = new Vector2(cameraZoom, cameraZoom);
			return;
		}
		var scene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
		var player = scene.Instantiate<PlayerClickToMove>();

		player.Name = $"Player_{Multiplayer.GetUniqueId()}";
		player.SetMultiplayerAuthority((int)Multiplayer.GetUniqueId());
		GetTree().CurrentScene.AddChild(player);
	}
	
	//Cheats
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("cheat_money"))
		{
			money = money + 1;
			GD.Print("Money: " + money);
			Label moneyLabel = GetNode<Label>("CanvasLayer/MoneyLabel");
			moneyLabel.Text = "$" + money;
		}
	}
}
