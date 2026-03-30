using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class NetworkManager : Node
{
	[Signal] public delegate void PlayerConnectedEventHandler(long peerId);
	[Signal] public delegate void PlayerDisconnectedEventHandler(long peerId);
	[Signal] public delegate void ConnectionSucceededEventHandler();
	[Signal] public delegate void ConnectionFailedEventHandler();
	[Signal] public delegate void ServerLostEventHandler();

	private const int MaxClients = 8;

	public long LocalPeerId => Multiplayer.GetUniqueId();
	public bool IsHost => Multiplayer.IsServer();
	
	public bool IsConnected =>
		Multiplayer.HasMultiplayerPeer() &&
		Multiplayer.MultiplayerPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected;

	public HashSet<long> ConnectedPeers { get; private set; } = new();
    private readonly Dictionary<long, Character> connectedPlayers = new();

    public override void _Ready()
	{
		Multiplayer.PeerConnected += OnPeerConnected;
		Multiplayer.PeerDisconnected += OnPeerDisconnected;
		Multiplayer.ConnectedToServer += OnConnectedToServer;
		Multiplayer.ConnectionFailed += OnConnectionFailed;
		Multiplayer.ServerDisconnected += OnServerDisconnected;
	}

	public void Host(int port)
	{
		var peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, MaxClients);
		if (error != Error.Ok)
		{
			GD.PrintErr($"Failed to create server: {error}");
			return;
		}
		Multiplayer.MultiplayerPeer = peer;
		ConnectedPeers.Add(1);
		GD.Print($"Hosting on port {port}");
	}

	public void Join(string address, int port)
	{
		var peer = new ENetMultiplayerPeer();
		var error = peer.CreateClient(address, port);
		if (error != Error.Ok)
		{
			GD.PrintErr($"Failed to connect: {error}");
			return;
		}
		Multiplayer.MultiplayerPeer = peer;
		GD.Print($"Connecting to {address}:{port}");
	}

	public void Disconnect()
	{
		if (Multiplayer.HasMultiplayerPeer())
		{
			Multiplayer.MultiplayerPeer.Close();
			Multiplayer.MultiplayerPeer = null;
		}
		ConnectedPeers.Clear();
	}

	private void OnPeerConnected(long id)
	{
		ConnectedPeers.Add(id);
		EmitSignal(SignalName.PlayerConnected, id);
		GD.Print($"Peer connected: {id}");
	}

	private void OnPeerDisconnected(long id)
	{
		ConnectedPeers.Remove(id);
		EmitSignal(SignalName.PlayerDisconnected, id);
		GD.Print($"Peer disconnected: {id}");
	}

	private void OnConnectedToServer()
	{
		ConnectedPeers.Add(LocalPeerId);
		EmitSignal(SignalName.ConnectionSucceeded);
		GD.Print($"Connected as peer {LocalPeerId}");
	}

	private void OnConnectionFailed()
	{
		Multiplayer.MultiplayerPeer = null;
		EmitSignal(SignalName.ConnectionFailed);
		GD.PrintErr("Connection failed");
	}

	private void OnServerDisconnected()
	{
		ConnectedPeers.Clear();
		Multiplayer.MultiplayerPeer = null;
		EmitSignal(SignalName.ServerLost);
		GD.Print("Server disconnected");
	}

    public void RemoveOnlinePlayerFromScene(long peerID)
    {
        GD.Print("Implelemt remove player from scene at 483 in ProgramHandler");
        var cToRemove = SceneHandler.instance.charactersInScene.Where(e => e.GetCharacterByPeerID(peerID)).First();
        if (cToRemove == null) return;
        cToRemove.character.QueueFree();
        SceneHandler.instance.charactersInScene.Remove(cToRemove);


    }
    public void SpawnOnlinePlayer(long peerID)
    {
        var onlinePlayer = new SceneData(
            SceneHandler.instance.GetHighestIDForOnline(), -1, 1, 0, 0, 128, false);
        SceneHandler.instance.charactersInScene.Add(onlinePlayer);
        AddOnlineCharacter(onlinePlayer, peerID);
    }

    public override void _Process(double delta)
    {
		if (!IsConnected) return;

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RegisterPlayer(long peerId, string playerName, string charFolder)
    {
		// Runs on host: spawn this player and tell everyone about them
		SpawnOnlinePlayer(peerId);
        Rpc(MethodName.SpawnRemoteCharacter, peerId, playerName, charFolder);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SpawnRemoteCharacter(long peerId)
    { 
		SpawnOnlinePlayer(peerId);
        //AddOnlineCharacter(new SceneData,peerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    private void SyncTalking(long peerId, bool talking)
    {
        if (connectedPlayers.TryGetValue(peerId, out var c)) { }
        //c.SetTalking(talking);
    }
    private void Host()
    {

    }
	
	public void GetCharacterFromOtherPlayer()
	{

	}

    public void AddOnlineCharacter(SceneData data, long peerID)
    {

        data.character = (Character)SpawnHandler.Spawn(SpawnableScenes.Character, this);
        GD.Print(data.character.Name);
        data.character.sceneID = data.ID;
        data.character.peerId = peerID;
        data.character.characterID = data.characterID;
        data.character.PivotOffsetRatio = new Vector2(0, 0);
        data.character.GlobalPosition = new Vector2(data.posX, data.posY);
        data.character.mirrored = data.mirrored;
        data.character.Flip(data.character.mirrored);
        data.character.PivotOffsetRatio = new Vector2(0.5f, 0.5f);
        data.character.isOnlineCharacter = true;
        data.character.SetupOnlineCharacter();
        data.character.SetupOnlineAnimations();
    }
}
