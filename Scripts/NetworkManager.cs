using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public partial class NetworkManager : Node
{
    public static NetworkManager instance;
    public static Node steamFunction;
    const int SafeMessageSize = 256 * 1024;
    private const int ChunkSize = 4096; // 4KB per chunk
    public static bool isHost = false;
    public static bool isMultiplayer=false;
    private Dictionary<int, List<byte[]>> _incomingChunks = new();
    private Dictionary<int, int> _expectedChunks = new();
    public Dictionary<int, Character> joinedPlayers = new();
    public override void _EnterTree()
    {
        instance = this;
    }
    public override void _Ready()
    {

        steamFunction = GetNode("/root/Control/AppManager/SteamConnection");
        steamFunction.Connect("host_created", new Callable(this, nameof(OnHostConnected)));
        steamFunction.Connect("lobby_joined", new Callable(this, nameof(OnJoinedLobby)));
        GD.Print("steam initialized");/*
        Multiplayer.MultiplayerPeer.ConnectedToServer+= ()=> 
        { 
            GD.Print(Multiplayer.MultiplayerPeer.GetUniqueId()); 
            Rpc(MethodName.SpawnPlayerToEveryone,Multiplayer.MultiplayerPeer.GetUniqueId()); };*/
        Multiplayer.PeerConnected += (peer) => { SpawnPlayerToEveryone((int)peer); };
        Multiplayer.PeerDisconnected += (peer) => { PeerDisconnect((int)peer); };

    }
    #region just for testing
    private ENetMultiplayerPeer enetPeer;

    public void HostLocalServer()
    {
        enetPeer = new ENetMultiplayerPeer();
        Error err = enetPeer.CreateServer(7000, maxClients: 8);

        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to create ENet server: {err}");
            return;
        }

        Multiplayer.MultiplayerPeer = enetPeer;
        isHost = true;
        isMultiplayer=true;
        GD.Print($"ENet server hosting on port {7000}");

        // Server is always "connected" to itself immediately, no need to await anything
        SpawnPlayerToEveryone(Multiplayer.GetUniqueId());
    }

    public async void JoinLocalServer(string address = "127.0.0.1")
    {
        enetPeer = new ENetMultiplayerPeer();
        Error err = enetPeer.CreateClient(address, 7000);

        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to create ENet client: {err}");
            return;
        }

        Multiplayer.MultiplayerPeer = enetPeer;
        GD.Print($"Attempting to connect to {address}:{7000}");

        if (enetPeer.GetConnectionStatus() != MultiplayerPeer.ConnectionStatus.Connected)
        {
            await ToSignal(Multiplayer, MultiplayerApi.SignalName.ConnectedToServer);
        }

        GD.Print("Connected to ENet server");
        isMultiplayer=true;
        SpawnPlayerToEveryone(Multiplayer.GetUniqueId());
    }
    #endregion
    public void HostLobby()
    {
        steamFunction.Call("host_lobby");
    }
    public void OnJoinedLobby(int peer)
    {
        UIManager.instance.ToggleNetworkConnectionButtons(true);
        GD.Print($"joined with peer: {peer} to {Multiplayer.GetUniqueId()}");
        isMultiplayer = true;

        SpawnPlayerToEveryone(peer);
        SyncConnectedPlayersToNewPlayer(peer);
        //Rpc(MethodName.SpawnPlayerToEveryone,peer);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SpawnPlayerToEveryone(int peer)
    {
        GD.Print($"Peerid: {peer} spawn player");
        var character = SpawnManager.SpawnCharacter();
        character.Name = $"Really_Cool_Player_With_Peer_{peer}";
        character.SetMultiplayerAuthority(peer);
        character.SpawnOnline();
        joinedPlayers.Add(peer, character);
    }
    public void OnHostConnected()
    {
        UIManager.instance.ToggleNetworkMenuButtons(false);
        isHost = true;
        isMultiplayer = true;
        GD.Print("host connected");
        //SpawnPlayerToEveryone(Multiplayer.MultiplayerPeer.GetUniqueId());
    }
    public void OnHostDisconnected()
    {
        Disconnect();
    }
    public void JoinToLobby()
    {
        //   ConfirmUI.Instance.ShowTextConfirm("Join via code: ", "Join", 
        //       () => 
        //       {
        try
        {
            GD.Print("Joining to lobby");
            steamFunction.Call("join_to_lobby_via_code", Convert.ToUInt64(DisplayServer.ClipboardGet()));

        }
        catch (Exception e)
        {
            GD.Print(e.Message);
            ConfirmUI.Instance.ShowConfirm("Couldn't join to lobby. Invalid room code in clip board.");
        }
        //       });
    }
    public void PeerDisconnect(int peer)
    {
        joinedPlayers[peer].RemoveOnlineCharacter();
        Multiplayer.MultiplayerPeer.DisconnectPeer(peer);
    }
    public void Disconnect()
    {

        UIManager.instance.ToggleNetworkMenuButtons(true);
        isHost = false;
        foreach (var item in joinedPlayers)
        {
            item.Value.RemoveOnlineCharacter();
        }
        joinedPlayers.Clear();
        Multiplayer.MultiplayerPeer.DisconnectPeer(Multiplayer.GetUniqueId());
        isMultiplayer = false;
    }
    public void Kick(int peer)
    {
        PeerDisconnect(peer);

    }



    public void SyncConnectedPlayersToNewPlayer(long newPlayerId)
    {
        foreach (var cPlayer in joinedPlayers)
        {
            GD.Print("data");
            GD.Print(cPlayer.Key);
            // Don't send a player their own data (they already have it)
            if (cPlayer.Key == newPlayerId) continue;
            //GD.Print(data.character.SendAnimationFrames());
            //GD.Print(animFramesToSend);
            SendDataToPeer(SaveLoadManager.CharacterDataToBytes(joinedPlayers[cPlayer.Key].data), (int)newPlayerId, cPlayer.Key);
            /*RpcId(newPlayerId, MethodName.RecieveDataFromPeer,
                SaveLoadManager.CharacterDataToBytes(joinedPlayers[cPlayer.Key].data),
                cPlayer.Key);
            */
            GD.Print("ImageData being sent:");
            var byteSequence = SaveLoadManager.AnimsToByte(cPlayer.Value);
            GD.Print($"Sent byteSequence length: {byteSequence.Length}, hash: {Convert.ToBase64String(System.Security.Cryptography.MD5.HashData(byteSequence))}");
            SendImageDataInPieces(byteSequence, (int)newPlayerId, cPlayer.Key);
        }
        GD.Print("player synced"); 
    }
    public void SendDataToPeer(byte[] data, int targetPeerID, long peerDataID)
    { 
        RpcId(targetPeerID, MethodName.RecieveDataFromPeer,
            data,
            peerDataID);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RecieveDataFromPeer(byte[]data,long peerDataID)
    { 
        var charData = SaveLoadManager.BytesToCharacterData(data);
        joinedPlayers[(int)peerDataID].data = charData;
    }

    public void SendTalkData(bool data, int peerDataID)
    {
        if (!isMultiplayer) return;
        Rpc(MethodName.RecieveTalk, data, peerDataID);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RecieveTalk(bool data, int peerDataID)
    {
        joinedPlayers[(int)peerDataID].isTalking= data;
    }

    public void SendImageDataInPieces(byte[] byteSequence, long targetPeerId, long peerDataID)
    {
        int totalChunks = Mathf.CeilToInt((float)byteSequence.Length / ChunkSize);
        int transferId = (int)(targetPeerId+peerDataID); // Unique ID for this transfer
        GD.Print($"Sending image data from peer: {peerDataID} to peer: {targetPeerId}");
        /*
         if (fullDataBytes.Length <= SafeMessageSize)
{
    RpcId(targetPeerId, MethodName.ReceiveFullAnimData, peerDataID, fullDataBytes);
}
else
{
    // send per-frame instead of arbitrary chunking
    SendPerFrame(...);
}
         
         */

        /*
        for (int i = 0; i < totalChunks; i++)
        {
            int offset = i * ChunkSize;
            int size = Mathf.Min(ChunkSize, byteSequence.Length - offset);
            byte[] chunk = byteSequence[offset..(offset + size)];

            RpcId(targetPeerId, MethodName.ReceiveChunk, peerDataID, transferId, i, totalChunks, chunk);
        }
        */
        RpcId(targetPeerId, MethodName.OnAnimationsFullyRecieved, (int)peerDataID, byteSequence);

    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ReceiveChunk(int peerID, int transferId, int chunkIndex, int totalChunks, byte[] chunk)
    {
        GD.Print($"Recieved animation chunk {chunkIndex}/{totalChunks}");
        GD.Print($"Amount of bites: {chunk.Length}");

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
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void OnAnimationsFullyRecieved(int peerID, byte[] fullData) 
    {
        var animsData = SaveLoadManager.ByteArrayToAnim(fullData);
        GD.Print($"Received byteSequence length: {fullData.Length}, hash: {Convert.ToBase64String(System.Security.Cryptography.MD5.HashData(fullData))}");

        int i = 0;
        GD.Print("Recieved animation data");
        foreach (var anim in animsData)
        {
            var images = new List<ImageTexture>();
            for (int j = 0; j < anim.Value.Count; j++)
            {
                var sprite = anim.Value[j];
                images.Add(new ImageTexture());
                images[j].SetImage(SaveLoadManager.BytesToImage(sprite));
            }
            int animToOverride = i > (int)EBaseAnims.Mouth ? (int)EBaseAnims.Base : i;
            joinedPlayers[peerID].CharacterAnims.Add(i, ((EBaseAnims)animToOverride, images));
            i++;
        }
        GD.Print("All animations had been got.");

    }
    public UInt64 GetJoinCode()
    {
       return steamFunction.Call("GetLobbyID").AsUInt64();
    }
}
