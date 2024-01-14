using Godot;
using OSSForts;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

public enum MultiplayerMode
{ 
	Standalone,
	Dedicated,
	Listen,
	Client,
	Mesh,
}

public partial class MultiplayerManager : Node
{
	public MultiplayerMode CurrentMultiplayerMode
	{
		get;
		set;
	} = MultiplayerMode.Standalone;

	public bool IsServer
	{
        get => CurrentMultiplayerMode == MultiplayerMode.Dedicated || CurrentMultiplayerMode == MultiplayerMode.Listen;
    } 

	/// <summary>
	/// True if the server has a client mode (May be true if IsServer is true, for listen servers)
	/// </summary>
	public bool HasClient
	{
		get => CurrentMultiplayerMode != MultiplayerMode.Dedicated;
	}

	
	public bool IsClient
	{
		get => !IsServer;
	}

	public bool IsDedicated
	{
		get => CurrentMultiplayerMode == MultiplayerMode.Dedicated;
	}

	public const int PORT = 25252;
	const string PlayerControllerPath = "res://Gameplay/Player/PlayerController.tscn";

	ENetMultiplayerPeer Peer = new();

	PackedScene? PlayerControllerResource;

	Dictionary<long, PlayerController> Players = new();

	MultiplayerSpawner? Spawner;
	

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		PlayerControllerResource = ResourceLoader.Load<PackedScene>(PlayerControllerPath);

		if(DisplayServer.GetName() == "headless")
		{
			GD.Print("Starting server in Dedicated Server mode!");
			HostServer(DesiredMode: MultiplayerMode.Dedicated);
		}
	}

	public bool HostServer(int desired_port = PORT, MultiplayerMode DesiredMode = MultiplayerMode.Listen)
	{
		Error err = Peer.CreateServer(desired_port, 4095);
		if(err != Error.Ok)
		{
            GD.PushError($"Multiplayer Manager: Failed to create server! Error: {Enum.GetName(err)}");
            return false;
        }
		Multiplayer.MultiplayerPeer = Peer;

		CurrentMultiplayerMode = DesiredMode;

        Multiplayer.PeerConnected += OnPlayerConnected;
		Multiplayer.PeerDisconnected += OnPlayerDisconnected;

		if(HasClient)
		{
			OnPlayerConnected(Multiplayer.GetUniqueId());
		}

		return true;
	}

	public bool TryConnectToServer(string address, int desired_port = PORT)
	{
		if(string.IsNullOrWhiteSpace(address))
		{
			return false;
		}

        Error err = Peer.CreateClient(address, desired_port);
        if (err != Error.Ok)
        {
            GD.PushError($"Multiplayer Manager: Failed to join server! Error: {Enum.GetName(err)}");
            return false;
        }

		Multiplayer.MultiplayerPeer = Peer;
		CurrentMultiplayerMode = MultiplayerMode.Client;

		Multiplayer.ConnectionFailed += OnConnectionFailed;
		Multiplayer.ServerDisconnected += OnServerDisconnected;

		Multiplayer.ConnectedToServer += OnConnectedToServer;

		GD.Print("Connecting to server...");

		return true;
    }

    private void OnConnectedToServer()
    {
		GD.Print($"Connected to server! Id {Multiplayer.GetUniqueId()}");
		//We've successfully connected to the server, lets create our player controller and set it's authority
		CreatePlayerControllerFor(Multiplayer.GetUniqueId());
    }

    private void OnServerDisconnected()
    {
        GD.PrintErr("Multiplayer Manager: Disconnected from server!");
    }

    private void OnConnectionFailed()
    {
        GD.PrintErr("Multiplayer Manager: Failed to connect to server!");
    }

    public void ShutdownMultiplayer()
	{
		//Disconnect all peers
		var Peers = Multiplayer.GetPeers();
		foreach(var peer in Peers)
		{
            Peer.DisconnectPeer(peer);
        }

		Peer.Close();

		Players.Clear();
		CurrentMultiplayerMode = MultiplayerMode.Standalone;
	}

    private void OnPlayerDisconnected(long id)
	{
		if(Players.TryGetValue(id, out PlayerController? PC))
		{
            PC.QueueFree();
            Players.Remove(id);
        }
        else
		{
            GD.PushError($"Multiplayer Manager: Player {id} disconnected, but they were not in the player list!");
        }
	}

	private void OnPlayerConnected(long id)
	{		
		CreatePlayerControllerFor(id);
	}

	private void DisconnectPlayer(long id)
	{
		//Disconnect the peer
		Peer.DisconnectPeer((int)id);
    }

	private PlayerController CreatePlayerControllerFor(long id)
	{
        if (PlayerControllerResource == null)
        {
            GD.PushError("Multiplayer Manager: PlayerControllerResource is null!");
            throw new InvalidOperationException("PlayerControllerResource is null!");
        }
        PlayerController PC = PlayerControllerResource.Instantiate<PlayerController>();
        PC.NetworkId = id;
        Players.Add(id, PC);

        Coroutine.NextFrame(() => AddChild(PC));

		return PC;
    }
}
