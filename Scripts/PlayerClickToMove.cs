using Godot;
using GodotPlugins.Game;
using System;

public partial class PlayerClickToMove : CharacterBody2D
{
	// Exported variable to set movement speed in the editor
	[Export]
	public float Speed = 400.0f;

	// Wobble effect parameters
	[Export]
	public float wobbleAmount = 0.3f; // Maximum rotation in radians
	[Export]
	public float wobbleSpeed = 50.0f; // Speed of the wobble effect (higher is slower frequency)

	private Vector2 _targetPosition;
	private bool _isMoving = false;

	public string playerName = MainMenu.GetPlayerName();

	public override void _Ready()
	{
		// Initialize target at current position so we don't move immediately
		_targetPosition = GlobalPosition;

		GetNode<Label>("UserName").Text = playerName;

		if (IsMultiplayerAuthority())
		{
			GetNode<Camera2D>("Camera2D").MakeCurrent();
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!IsMultiplayerAuthority())
			return;
		// Check for left mouse click
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
		{
			_targetPosition = GetGlobalMousePosition();
			_isMoving = true;
			GD.Print(_targetPosition);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsMultiplayerAuthority())
			return;
		if (!_isMoving) return;

		// Calculate distance to target
		float distanceToTarget = GlobalPosition.DistanceTo(_targetPosition);

		// If we are close enough, stop moving (prevents jitter)
		if (distanceToTarget < 5.0f)
		{
			_isMoving = false;
			Velocity = Vector2.Zero;

			GetNode<Sprite2D>("Sprite2D").Rotation = 0;

		}
		else
		{
			// Calculate direction and set velocity
			Vector2 direction = (_targetPosition - GlobalPosition).Normalized();
			Velocity = direction * Speed;

			// Make the player flip horizontally based on movement direction
			if (Velocity.X != 0)
			{
				Sprite2D sprite = GetNode<Sprite2D>("Sprite2D");
				sprite.FlipH = Velocity.X < 0;
			}

			// Add a slight wobble effect
			GetNode<Sprite2D>("Sprite2D").Rotation = (float)Math.Sin(Time.GetTicksMsec() / wobbleSpeed) * wobbleAmount;

		}

		MoveAndSlide();
		SyncRemoteTransform();
	}

	public void SetPlayerName(string name)
	{
		playerName = name;
		GetNode<Label>("UserName").Text = playerName;
	}

	private void SyncRemoteTransform()
	{
		var sprite = GetNode<Sprite2D>("Sprite2D");
		RpcId(1, nameof(UpdateServerTransform), GlobalPosition, sprite.FlipH, sprite.Rotation);
	}

	[Rpc]
	private void UpdateServerTransform(Vector2 position, bool flipH, float rotation)
	{
		if (!Multiplayer.IsServer())
			return;

		GlobalPosition = position;
		var sprite = GetNode<Sprite2D>("Sprite2D");
		sprite.FlipH = flipH;
		sprite.Rotation = rotation;
		// Forward the authoritative transform to other clients via the NetworkManager autoload
		Rpc(nameof(ApplyRemoteTransform), position, flipH, rotation);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void ApplyRemoteTransform(Vector2 position, bool flipH, float rotation)
	{
		if (IsMultiplayerAuthority())
			return;

		GlobalPosition = position;
		var sprite = GetNode<Sprite2D>("Sprite2D");
		sprite.FlipH = flipH;
		sprite.Rotation = rotation;
	}
}
