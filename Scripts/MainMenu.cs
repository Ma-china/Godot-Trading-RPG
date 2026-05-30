using System;
using Godot;

public partial class MainMenu : Control
{
	public static string playerName = "Player";
	public static string ipAddress = "127.0.0.1";
	public override void _Ready()
	{
		GD.Print("init");
		GetNode<Button>("HostButton").Pressed += OnHostPressed;
		GetNode<Button>("JoinButton").Pressed += OnJoinPressed;
		GetNode<LineEdit>("PlayerName").TextChanged += OnPlayerNameEntered;
		GetNode<LineEdit>("ServerIP").TextChanged += OnIPAddressEntered;
	}

	private void OnHostPressed()
	{
		GD.Print("clicked");
		GetNode<NetworkManager>("/root/NetworkManager").HostGame();
		
	}

	private void OnJoinPressed()
	{
		GetNode<NetworkManager>("/root/NetworkManager").JoinGame(ipAddress);
	}

	private void OnPlayerNameEntered(String newName)
	{
		GD.Print(newName);
		playerName = newName;
	}

	private void OnIPAddressEntered(String newIP)
	{
		GD.Print(newIP);
		ipAddress = newIP;
	}

	public static string GetPlayerName()
	{
		return playerName;
	}
}
