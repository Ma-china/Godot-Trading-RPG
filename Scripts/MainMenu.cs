using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GD.Print("init");
		GetNode<Button>("HostButton").Pressed += OnHostPressed;
		GetNode<Button>("JoinButton").Pressed += OnJoinPressed;
	}

	private void OnHostPressed()
	{
		GD.Print("clicked");
		GetNode<NetworkManager>("/root/NetworkManager").HostGame();
		
	}

	private void OnJoinPressed()
	{
		GetNode<NetworkManager>("/root/NetworkManager").JoinGame("127.0.0.1"); // replace with input later
	}
}
