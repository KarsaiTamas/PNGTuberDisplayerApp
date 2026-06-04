using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 

public partial class NetworkManager : Node
{
	[Signal] public delegate void PlayerConnectedEventHandler(long peerId);
	[Signal] public delegate void PlayerDisconnectedEventHandler(long peerId);
	[Signal] public delegate void ConnectionSucceededEventHandler();
	[Signal] public delegate void ConnectionFailedEventHandler();
	[Signal] public delegate void ServerLostEventHandler();
    public Action OnHosted;
    public Action OnJoined;
    private const int ChunkSize = 4096; // 4KB per chunk
    private Dictionary<int, List<byte[]>> _incomingChunks = new();
    private Dictionary<int, int> _expectedChunks = new();

    private const int MaxClients = 8;

	public long LocalPeerId => Multiplayer.GetUniqueId();
	public bool IsHost => Multiplayer.IsServer();
     
    public bool isConnected;
    public bool joinedWithCharacter; 
    public  Dictionary<long, SceneData> connectedPlayers = new();
	public bool maxPeer;
    public override void _Ready()
	{
		Multiplayer.PeerConnected += OnPeerConnected;
		Multiplayer.PeerDisconnected += OnPeerDisconnected;
		Multiplayer.ConnectedToServer += OnConnectedToServer;
		Multiplayer.ConnectionFailed += OnConnectionFailed;
		Multiplayer.ServerDisconnected += OnServerDisconnected;
		Multiplayer.MultiplayerPeer.Close(); 
        joinedWithCharacter = false;
        isConnected = false;
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
		 
         
		GD.Print($"Hosting on port {port}");
        isConnected = true;
        OnHosted.Invoke();
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
		//maxPeer = Multiplayer.GetUniqueId();
		int peerID = Multiplayer.GetUniqueId();
        //ConnectedPeers.Add(peerID);
        //Rpc(MethodName.SpawnRemoteCharacter, peerID);
        //ConnectedPeers.Add(Multiplayer.GetUniqueId());
        //ProgramHandler.
        GD.Print($"Connecting to {address}:{port}");
        isConnected = true;
        OnJoined.Invoke();
    }

	public void Disconnect()
	{
		if (Multiplayer.HasMultiplayerPeer())
		{
			Multiplayer.MultiplayerPeer.Close(); 
        } 
        joinedWithCharacter = false;
        isConnected = false;

        if (SceneHandler.instance!= null)
        {
            SceneHandler.instance.TurnOffJoinToSceneButtons();
        }

    }

    private void OnPeerConnected(long id)
    {
        if (!IsHost) return;
        
        GD.Print(connectedPlayers.Count);
        //EmitSignal(SignalName.PlayerConnected, id);
        SyncConnectedPlayersToNewPlayer(id); 
        GD.Print($"Peer connected: {id}");
    }

	private void OnPeerDisconnected(long id)
	{
        //EmitSignal(SignalName.PlayerDisconnected, id);
        RemoveOnlinePlayerFromScene(id);

        GD.Print($"Peer disconnected: {id}");
	}

	private void OnConnectedToServer()
	{ 
		//EmitSignal(SignalName.ConnectionSucceeded);
		GD.Print($"Connected as peer {LocalPeerId}");
	}

	private void OnConnectionFailed()
	{
		Multiplayer.MultiplayerPeer = null;
		//EmitSignal(SignalName.ConnectionFailed);
		GD.PrintErr("Connection failed");
	}

	private void OnServerDisconnected()
	{ 
		Multiplayer.MultiplayerPeer = null;
		//EmitSignal(SignalName.ServerLost);
		GD.Print("Server disconnected");
	}

    private void OnAnimationsFullyRecieved(long peerID, byte[] animBytes)
    { 
        var anims = DeserializeAnims(animBytes);
        GD.Print("Animations fully recieved!"); 
        GD.Print("Loading animations...");
        connectedPlayers[peerID].character.SetupOnlineAnimations(anims, connectedPlayers[peerID].character.frameLenghts,false);
        connectedPlayers[peerID].character.isLoaded=true;
        GD.Print("Animations fully loaded!"); 
    }

    public override void _Process(double delta)
    {
        if (!isConnected) return;

    }

    public void RemoveOnlinePlayerFromScene(long peerID)
    {
        GD.Print("Implelemt remove player from scene at 483 in ProgramHandler");
        var cToRemove = SceneHandler.instance.charactersInScene.Where(e => e.GetCharacterByPeerID(peerID)).First();
        if (cToRemove == null) return;
        cToRemove.character.QueueFree();
        SceneHandler.instance.charactersInScene.Remove(cToRemove);
        connectedPlayers.Remove(peerID);


    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SpawnOnlinePlayer(long peerID, string nodeName, Godot.Collections.Array<float> frames)
    {
        var onlinePlayer = new SceneData(
            SceneHandler.instance.GetHighestIDForOnline(), -1, 0, 0, 0, 128, false);
        AddPlayerToConnection(peerID, onlinePlayer);
        SceneHandler.instance.AddOnlineCharacter(onlinePlayer,peerID,nodeName, frames);

        RpcId(peerID, MethodName.SendGotCharacterData, LocalPeerId);

    }
     
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SpawnRemoteCharacter(long peerId, Godot.Collections.Array<float> frames, string nodeName)
    {  

        Rpc(MethodName.SpawnOnlinePlayer, peerId, nodeName, frames);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendGotCharacterData(long targetPeerID)
    { 
        connectedPlayers[LocalPeerId].character.SendImageDataToPlayer(targetPeerID);
    } 

    public void SendImageDataInPieces(byte[] byteSequence, long targetPeerId,long peerDataID)
    { 
        int totalChunks = Mathf.CeilToInt((float)byteSequence.Length / ChunkSize);
        int transferId = GD.RandRange(1000, 9999); // Unique ID for this transfer

        for (int i = 0; i < totalChunks; i++)
        {
            int offset = i * ChunkSize;
            int size = Mathf.Min(ChunkSize, byteSequence.Length - offset);
            byte[] chunk = byteSequence[offset..(offset + size)];

            RpcId(targetPeerId, MethodName.ReceiveChunk, peerDataID, transferId, i, totalChunks, chunk);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ReceiveChunk(long peerID, int transferId, int chunkIndex, int totalChunks, byte[] chunk)
    {
        if (!_incomingChunks.ContainsKey(transferId))
        {
            _incomingChunks[transferId] = new List<byte[]>(new byte[totalChunks][]);
            _expectedChunks[transferId] = totalChunks;
        }

        _incomingChunks[transferId][chunkIndex] = chunk;

        // Check if all chunks received
        if (_incomingChunks[transferId].All(c => c != null))
        {
            byte[] fullData = _incomingChunks[transferId].SelectMany(c => c).ToArray();
            _incomingChunks.Remove(transferId);
            _expectedChunks.Remove(transferId);
             
            OnAnimationsFullyRecieved(peerID, fullData);
        }
    }
     

    public void SyncConnectedPlayersToNewPlayer(long newPlayerId)
    {
        GD.Print("syncing player");
        GD.Print(connectedPlayers.Count);
        foreach (var cPlayer in connectedPlayers)
        {
            GD.Print("data");
            GD.Print(cPlayer.Key);
            // Don't send a player their own data (they already have it)
            if (cPlayer.Key == newPlayerId) continue;
            SceneData data = cPlayer.Value;
            //GD.Print(data.character.SendAnimationFrames());
            //GD.Print(animFramesToSend);
            RpcId(newPlayerId, MethodName.ReceivePlayerData,
                cPlayer.Key,
                data.nodeName,
                //SerializeAnims(data.character.SendAnimationFrames()),
                data.character.SendFrameLengths()
            );
            SendImageDataInPieces(SerializeAnims(data.character.SendAnimationFrames()), newPlayerId,cPlayer.Key);
        }
        GD.Print("player synced");

    } 

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false,TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ReceivePlayerData(
    long peerID,
    string nodeName,
    //byte[] animBytes,       // PackedByteArray maps to byte[] in C#
    Godot.Collections.Array<float> frameLengths)   // PackedFloat32Array maps to float[]
    {
        GD.Print("Receiving player: ", nodeName);

        //var anims = DeserializeAnims(animBytes);

        if (!connectedPlayers.ContainsKey(peerID))
        {
            connectedPlayers.Add(peerID, new SceneData(1000+ connectedPlayers.Count, nodeName));
            SceneHandler.instance.AddOnlineCharacter(
                connectedPlayers[peerID], peerID, nodeName, frameLengths);
        }
    }

    public byte[] SerializeAnims(
    Godot.Collections.Dictionary<string, Godot.Collections.Array<byte[]>> anims)
    {
        // Convert to a plain C# structure and JSON-serialize it
        var plain = new Dictionary<string, List<string>>();
        foreach (var kvp in anims)
        {
            plain[kvp.Key] = kvp.Value
                .Select(frame => System.Convert.ToBase64String(frame))
                .ToList();
        }
        return System.Text.Encoding.UTF8.GetBytes(
            System.Text.Json.JsonSerializer.Serialize(plain));
    }
    
    private Godot.Collections.Dictionary<string, Godot.Collections.Array<byte[]>>
    DeserializeAnims(byte[] animBytes)
    {
        string json = System.Text.Encoding.UTF8.GetString(animBytes);
        var plain = System.Text.Json.JsonSerializer
            .Deserialize<Dictionary<string, List<string>>>(json);

        var result = new Godot.Collections.Dictionary<
        string, Godot.Collections.Array<byte[]>> ();

        foreach (var kvp in plain)
        {
            GD.Print(kvp.Key);
            var frames = new Godot.Collections.Array<byte[]>();
            foreach (var b64 in kvp.Value)
                frames.Add(Convert.FromBase64String(b64));
            result.Add(kvp.Key,frames);
        }
        return result;
    }
    public void AddPlayerToConnection(long peerID,SceneData onlinePlayer)
    {
        if (ProgramHandler.network.connectedPlayers.ContainsKey(peerID))
            ProgramHandler.network.connectedPlayers[peerID]=onlinePlayer;
        else ProgramHandler.network.connectedPlayers.Add(peerID, onlinePlayer);
        

    }




}
