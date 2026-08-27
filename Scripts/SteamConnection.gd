extends Node

signal host_created()
signal lobby_joined(peer:int)
signal host_quit()
signal disconnected()
const LOBBY_TYPE := Steam.LobbyType.LOBBY_TYPE_FRIENDS_ONLY
const MAX_MEMBERS :=20
var lobbyID=-1
var peer: SteamMultiplayerPeer
var testing:=true
var isHost:=false
func _ready() -> void:
	Steam.initRelayNetworkAccess() 
	
	Steam.lobby_created.connect(on_lobby_created)
	Steam.lobby_joined.connect(on_lobby_joined)
	Steam.join_requested.connect(on_join_requested)
	multiplayer.connected_to_server.connect(func(): print(">>> CONNECTED TO SERVER"))
	multiplayer.connection_failed.connect(func(): print(">>> CONNECTION FAILED"))
	multiplayer.server_disconnected.connect(func(): print(">>> SERVER DISCONNECTED"))
	multiplayer.peer_connected.connect(func(id): print(">>> PEER CONNECTED: ", id))
	#Steam.steam_server_disconnected.connect()
	#Steam.remote_play_session_disconnected.connect() 
	
func _process(delta: float) -> void:
	Steam.run_callbacks()
	
func host_lobby()-> void:
	Steam.createLobby(LOBBY_TYPE,MAX_MEMBERS)
	
func on_lobby_created(connect: int,lobby_id: int) -> void:
	if connect== Steam.RESULT_OK:
		peer=SteamMultiplayerPeer.new()
		lobbyID=lobby_id
		peer.server_relay=true
		peer.create_host()
		multiplayer.multiplayer_peer=peer
		host_created.emit()
		isHost=true
		print("Lobby created with peer: ")
		print(peer)
		print("Lobby id: ")
		print(lobbyID)
		lobby_joined.emit(peer.get_unique_id())
		
func on_lobby_joined(lobby_id: int, permissions: int, locked: bool, response: int)-> void:
	if response==Steam.CHAT_ROOM_ENTER_RESPONSE_SUCCESS:
		if Steam.getLobbyOwner(lobby_id)==Steam.getSteamID():
			if (isHost):
				print("Host not getting this again")
				return
		peer=SteamMultiplayerPeer.new()
		peer.server_relay=true
		peer.create_client(Steam.getLobbyOwner(lobby_id))
		multiplayer.multiplayer_peer=peer 
		print("Joined lobby with peer: ")
		print(peer)
		lobby_joined.emit(peer.get_unique_id())

func on_join_requested(lobby_id: int, steam_id: int)->void:
	Steam.joinLobby(lobby_id)
		
func GetLobbyID()->int: 
	return lobbyID
		
func join_to_lobby_via_code(lobby_id: int)->void:
	
	print("tring to join")
	Steam.joinLobby(lobby_id)
		
#func disconnect_from_game()->void:
	#peer.disconnect_peer(multiplayer.multiplayer_peer.)
		
	
