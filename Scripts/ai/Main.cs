using Godot;
using System.Collections.Generic;

public partial class Main : Control
{
	private NetworkManager _network;
	private AudioDetector _audio;

	// UI references
	private PanelContainer _connectionPanel;
	private LineEdit _nameInput;
	private LineEdit _ipInput;
	private LineEdit _portInput;
	private Button _hostButton;
	private Button _joinButton;
	private Button _disconnectButton;
	private Label _statusLabel;
	private HBoxContainer _emoteBar;
	private HBoxContainer _micControls;
	private ProgressBar _micBar;
	private HSlider _thresholdSlider;
    private DetectAudio audioDetector;

    // Characters
    private Node2D _characterContainer;
	private readonly Dictionary<long, PNGTuberCharacter> _characters = new();

	// State
	private bool _wasTalking;
	private string _playerName = "Player";
	private string _characterFolder = "Default";

	private static readonly string[] Emotes =
		{ "\ud83d\udc4b", "\ud83d\ude0a", "\ud83d\ude22", "\ud83d\ude2e", "\u2764\ufe0f", "\ud83d\ude02", "\ud83c\udf89", "\ud83d\udc4d" };

	public override void _Ready()
	{
		RenderingServer.SetDefaultClearColor(new Color(0.12f, 0.12f, 0.18f));

		_network = new NetworkManager();
		_network.Name = "NetworkManager";
		AddChild(_network);

		_audio = new AudioDetector();
		_audio.Name = "AudioDetector";
		AddChild(_audio);

		_characterContainer = new Node2D();
		_characterContainer.Name = "Characters";
		AddChild(_characterContainer);

		BuildUI();
		ConnectSignals();

		GetTree().Root.SizeChanged += RepositionCharacters; 
    }

	// ── UI Construction ─────────────────────────────────────────────

	private void BuildUI()
	{
		var canvas = new CanvasLayer();
		AddChild(canvas);

		var root = new MarginContainer();
		root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		root.AddThemeConstantOverride("margin_left", 20);
		root.AddThemeConstantOverride("margin_right", 20);
		root.AddThemeConstantOverride("margin_top", 12);
		root.AddThemeConstantOverride("margin_bottom", 12);
		canvas.AddChild(root);

		var vbox = new VBoxContainer();
		vbox.AddThemeConstantOverride("separation", 10);
		root.AddChild(vbox);

		// Title bar
		var titleBar = new HBoxContainer();
		titleBar.AddThemeConstantOverride("separation", 12);

		var title = new Label { Text = "PNGTuber Party" };
		title.AddThemeFontSizeOverride("font_size", 26);
		title.AddThemeColorOverride("font_color", Colors.White);
		titleBar.AddChild(title);

		titleBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });

		_statusLabel = new Label { Text = "Disconnected" };
		_statusLabel.AddThemeFontSizeOverride("font_size", 15);
		_statusLabel.AddThemeColorOverride("font_color", Colors.Gray);
		titleBar.AddChild(_statusLabel);

		_disconnectButton = new Button { Text = "Disconnect", Visible = false };
		titleBar.AddChild(_disconnectButton);

		vbox.AddChild(titleBar);

		// Connection panel
		BuildConnectionPanel(vbox);

		// Spacer pushes emote bar to bottom
		vbox.AddChild(new Control { SizeFlagsVertical = Control.SizeFlags.ExpandFill });

		// Mic controls (visible when connected)
		BuildMicControls(vbox);

		// Emote bar (visible when connected)
		BuildEmoteBar(vbox);
	}

	private void BuildConnectionPanel(VBoxContainer parent)
	{
		_connectionPanel = new PanelContainer();
		var inner = new VBoxContainer();
		inner.AddThemeConstantOverride("separation", 8);

		var margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", 12);
		margin.AddThemeConstantOverride("margin_right", 12);
		margin.AddThemeConstantOverride("margin_top", 10);
		margin.AddThemeConstantOverride("margin_bottom", 10);
		margin.AddChild(inner);
		_connectionPanel.AddChild(margin);

		// Name
		var nameRow = new HBoxContainer();
		nameRow.AddChild(MakeLabel("Name:"));
		_nameInput = new LineEdit { PlaceholderText = "Your Name", Text = "Player", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
		nameRow.AddChild(_nameInput);
		inner.AddChild(nameRow);

		// Character folder
		var charRow = new HBoxContainer();
		charRow.AddChild(MakeLabel("Character:"));
		var charInput = new LineEdit { PlaceholderText = "Folder in Characters/", Text = "Default", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
		charInput.TextChanged += t => _characterFolder = t;
		charRow.AddChild(charInput);
		inner.AddChild(charRow);

		inner.AddChild(new HSeparator());

		// Host / Join row
		var row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 8);

		_ipInput = new LineEdit { PlaceholderText = "IP Address", Text = "127.0.0.1", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
		row.AddChild(_ipInput);

		_portInput = new LineEdit { PlaceholderText = "Port", Text = "7000", CustomMinimumSize = new Vector2(80, 0) };
		row.AddChild(_portInput);

		_hostButton = new Button { Text = "Host Session", CustomMinimumSize = new Vector2(120, 0) };
		row.AddChild(_hostButton);

		_joinButton = new Button { Text = "Join Session", CustomMinimumSize = new Vector2(120, 0) };
		row.AddChild(_joinButton);

		inner.AddChild(row);

		// Hint text
		var hint = new Label { Text = "Host: others connect to your IP.  Join: enter the host's IP." };
		hint.AddThemeFontSizeOverride("font_size", 12);
		hint.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
		inner.AddChild(hint);

		parent.AddChild(_connectionPanel);
	}

	private void BuildMicControls(VBoxContainer parent)
	{
		_micControls = new HBoxContainer { Visible = false };
		_micControls.AddThemeConstantOverride("separation", 10);

		_micControls.AddChild(MakeLabel("Mic:"));

		_micBar = new ProgressBar();
		_micBar.MinValue = 0;
		_micBar.MaxValue = 0.15;
		_micBar.Value = 0;
		_micBar.CustomMinimumSize = new Vector2(180, 22);
		_micBar.ShowPercentage = false;
		_micControls.AddChild(_micBar);

		_micControls.AddChild(MakeLabel("  Sensitivity:"));

		_thresholdSlider = new HSlider();
		_thresholdSlider.MinValue = 0.01;
		_thresholdSlider.MaxValue = 0.5;
		_thresholdSlider.Step = 0.01;
		_thresholdSlider.Value = 0.01;
		_thresholdSlider.CustomMinimumSize = new Vector2(140, 0);
		_thresholdSlider.ValueChanged += v => _audio.TalkThreshold = (float)v;
		_micControls.AddChild(_thresholdSlider);

		parent.AddChild(_micControls);
	}

	private void BuildEmoteBar(VBoxContainer parent)
	{
		_emoteBar = new HBoxContainer { Visible = false };
		_emoteBar.Alignment = BoxContainer.AlignmentMode.Center;
		_emoteBar.AddThemeConstantOverride("separation", 6);

		foreach (var emote in Emotes)
		{
			var btn = new Button { Text = emote, CustomMinimumSize = new Vector2(52, 52) };
			btn.AddThemeFontSizeOverride("font_size", 24);
			var e = emote;
			btn.Pressed += () => SendEmote(e);
			_emoteBar.AddChild(btn);
		}

		parent.AddChild(_emoteBar);
	}

	private static Label MakeLabel(string text)
	{
		var l = new Label { Text = text };
		l.AddThemeFontSizeOverride("font_size", 14);
		return l;
	}

	// ── Signal Wiring ───────────────────────────────────────────────

	private void ConnectSignals()
	{
		_hostButton.Pressed += OnHostPressed;
		_joinButton.Pressed += OnJoinPressed;
		_disconnectButton.Pressed += OnDisconnectPressed;

		_network.PlayerConnected += OnPlayerConnected;
		_network.PlayerDisconnected += OnPlayerDisconnected;
		_network.ConnectionSucceeded += OnConnectionSucceeded;
		_network.ConnectionFailed += OnConnectionFailed;
		_network.ServerLost += OnServerLost;
	}

	// ── Network Events ──────────────────────────────────────────────

	private void OnHostPressed()
	{
		_playerName = _nameInput.Text.Trim();
		if (string.IsNullOrEmpty(_playerName)) _playerName = "Host";

		int port = int.TryParse(_portInput.Text, out int p) ? p : 7000;
		_network.Host(port);

		SetConnectedUI($"Hosting on port {port}");
		SpawnCharacter(1, _playerName, _characterFolder);
	}

	private void OnJoinPressed()
	{
		_playerName = _nameInput.Text.Trim();
		if (string.IsNullOrEmpty(_playerName)) _playerName = "Player";

		string ip = _ipInput.Text.Trim();
		int port = int.TryParse(_portInput.Text, out int p) ? p : 7000;

		_network.Join(ip, port);

		_statusLabel.Text = "Connecting...";
		_statusLabel.AddThemeColorOverride("font_color", Colors.Yellow);
		_hostButton.Disabled = true;
		_joinButton.Disabled = true;
	}

	private void OnConnectionSucceeded()
	{
		SetConnectedUI($"Connected (ID: {_network.LocalPeerId})");
		SpawnCharacter(_network.LocalPeerId, _playerName, _characterFolder);

		// Tell the host who we are
		RpcId(1, MethodName.RegisterPlayer, _network.LocalPeerId, _playerName, _characterFolder);
	}

	private void OnConnectionFailed()
	{
		_statusLabel.Text = "Connection failed";
		_statusLabel.AddThemeColorOverride("font_color", Colors.Red);
		_hostButton.Disabled = false;
		_joinButton.Disabled = false;
	}

	private void OnPlayerConnected(long peerId)
	{
		// Host tells new peer about all existing characters
		if (!Multiplayer.IsServer()) return;

		foreach (var kvp in _characters)
			RpcId(peerId, MethodName.SpawnRemoteCharacter, kvp.Key, kvp.Value.PlayerName, kvp.Value.CharacterFolder);
	}

	private void OnPlayerDisconnected(long peerId)
	{
		RemoveCharacter(peerId);
	}

	private void OnServerLost()
	{
		ResetToDisconnected();
		_statusLabel.Text = "Host disconnected";
		_statusLabel.AddThemeColorOverride("font_color", Colors.Red);
	}

	private void OnDisconnectPressed()
	{
		_network.Disconnect();
		ResetToDisconnected();
	}

	// ── UI State Helpers ────────────────────────────────────────────

	private void SetConnectedUI(string status)
	{
		_connectionPanel.Visible = false;
		_emoteBar.Visible = true;
		_micControls.Visible = true;
		_disconnectButton.Visible = true;
		_statusLabel.Text = status;
		_statusLabel.AddThemeColorOverride("font_color", Colors.Green);
	}

	private void ResetToDisconnected()
	{
		foreach (var kvp in _characters)
			kvp.Value.QueueFree();
		_characters.Clear();

		_connectionPanel.Visible = true;
		_emoteBar.Visible = false;
		_micControls.Visible = false;
		_disconnectButton.Visible = false;
		_hostButton.Disabled = false;
		_joinButton.Disabled = false;
		_statusLabel.Text = "Disconnected";
		_statusLabel.AddThemeColorOverride("font_color", Colors.Gray);
	}

	// ── Character Management ────────────────────────────────────────

	private void SpawnCharacter(long peerId, string name, string charFolder)
	{
		if (_characters.ContainsKey(peerId)) return;

		var character = new PNGTuberCharacter
		{
			PeerId = peerId,
			PlayerName = name,
			CharacterFolder = charFolder
		};
		_characterContainer.AddChild(character);
		_characters[peerId] = character;
		RepositionCharacters();
	}

	private void RemoveCharacter(long peerId)
	{
		if (!_characters.TryGetValue(peerId, out var c)) return;
		c.QueueFree();
		_characters.Remove(peerId);
		RepositionCharacters();
	}

	private void RepositionCharacters()
	{
		int count = _characters.Count;
		if (count == 0) return;

		var viewport = GetViewport().GetVisibleRect().Size;
		float totalWidth = viewport.X - 80;
		float spacing = totalWidth / count;
		float startX = 40 + spacing / 2;
		float y = viewport.Y * 0.52f;

		int i = 0;
		foreach (var kvp in _characters)
		{
			kvp.Value.SetBasePosition(new Vector2(startX + i * spacing, y));
			i++;
		}
	}

	// ── RPCs ────────────────────────────────────────────────────────

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RegisterPlayer(long peerId, string playerName, string charFolder)
	{
		// Runs on host: spawn this player and tell everyone about them
		SpawnCharacter(peerId, playerName, charFolder);
		Rpc(MethodName.SpawnRemoteCharacter, peerId, playerName, charFolder);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void SpawnRemoteCharacter(long peerId, string playerName, string charFolder)
	{
		SpawnCharacter(peerId, playerName, charFolder);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	private void SyncTalking(long peerId, bool talking)
	{
		if (_characters.TryGetValue(peerId, out var c))
			c.SetTalking(talking);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void SyncEmote(long peerId, string emote)
	{
		if (_characters.TryGetValue(peerId, out var c))
			c.ShowEmote(emote);
	}

	// ── Per-Frame ───────────────────────────────────────────────────

	public override void _Process(double delta)
	{
		if (!_network.IsConnected) return;

		// Update mic level display
		_micBar.Value = _audio.CurrentLevel;

		// Sync talking state when it changes
		bool talking = _audio.IsTalking;
		if (talking != _wasTalking)
		{
			_wasTalking = talking;
			Rpc(MethodName.SyncTalking, _network.LocalPeerId, talking);

			if (_characters.TryGetValue(_network.LocalPeerId, out var local))
				local.SetTalking(talking);
		}
	}

	private void SendEmote(string emote)
	{
		if (!_network.IsConnected) return;

		Rpc(MethodName.SyncEmote, _network.LocalPeerId, emote);

		if (_characters.TryGetValue(_network.LocalPeerId, out var local))
			local.ShowEmote(emote);
	}
}
