using Godot;

public partial class PNGTuberCharacter : Node2D
{
	public long PeerId { get; set; } = -1;
	public string PlayerName { get; set; } = "Player";
	public string CharacterFolder { get; set; } = "Default";

	private Sprite2D _sprite;
	private Label _nameLabel;
	private Label _emoteLabel;

	private Texture2D _idleTexture;
	private Texture2D _talkingTexture;

	private bool _isTalking;
	private float _emoteTimer;
	private float _bobPhase;
	private Vector2 _basePosition;

	public override void _Ready()
	{
		_basePosition = Position;

		_sprite = new Sprite2D();
		AddChild(_sprite);

		_nameLabel = new Label();
		_nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_nameLabel.Position = new Vector2(-80, -150);
		_nameLabel.Size = new Vector2(160, 30);
		_nameLabel.Text = PlayerName;
		_nameLabel.AddThemeColorOverride("font_color", Colors.White);
		_nameLabel.AddThemeFontSizeOverride("font_size", 18);
		AddChild(_nameLabel);

		_emoteLabel = new Label();
		_emoteLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_emoteLabel.Position = new Vector2(-80, -185);
		_emoteLabel.Size = new Vector2(160, 35);
		_emoteLabel.Text = "";
		_emoteLabel.AddThemeColorOverride("font_color", Colors.Yellow);
		_emoteLabel.AddThemeFontSizeOverride("font_size", 28);
		AddChild(_emoteLabel);

		LoadTextures();
		UpdateSprite();
	}

	private void LoadTextures()
	{
		string basePath = $"res://Characters/{CharacterFolder}";
		string idlePath = $"{basePath}/idle.png";
		string talkPath = $"{basePath}/talking.png";

		if (ResourceLoader.Exists(idlePath))
			_idleTexture = GD.Load<Texture2D>(idlePath);
		if (ResourceLoader.Exists(talkPath))
			_talkingTexture = GD.Load<Texture2D>(talkPath);

		// Generate unique-colored placeholders if PNGs aren't found
		Color baseColor = GetColorFromPeerId();
		if (_idleTexture == null)
			_idleTexture = GeneratePlaceholder(baseColor, false);
		if (_talkingTexture == null)
			_talkingTexture = GeneratePlaceholder(baseColor, true);
	}

	private Color GetColorFromPeerId()
	{
		var rng = new RandomNumberGenerator();
		rng.Seed = (ulong)(PeerId > 0 ? PeerId : 42);
		return Color.FromHsv(rng.Randf(), 0.6f, 0.85f);
	}

	private static Texture2D GeneratePlaceholder(Color color, bool talking)
	{
		const int size = 200;
		const int half = size / 2;
		var img = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);

		// Body circle
		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				float dx = x - half;
				float dy = y - half;
				float dist = Mathf.Sqrt(dx * dx + dy * dy);

				if (dist < half - 6)
					img.SetPixel(x, y, color);
				else if (dist < half - 2)
					img.SetPixel(x, y, color.Lightened(0.4f));
				else
					img.SetPixel(x, y, new Color(0, 0, 0, 0));
			}
		}

		// Eyes
		DrawCircle(img, 70, 72, 14, Colors.White);
		DrawCircle(img, 130, 72, 14, Colors.White);
		DrawCircle(img, 73, 72, 7, Colors.Black);
		DrawCircle(img, 133, 72, 7, Colors.Black);
		// Eye shine
		DrawCircle(img, 76, 68, 3, Colors.White);
		DrawCircle(img, 136, 68, 3, Colors.White);

		// Mouth
		if (talking)
		{
			// Open mouth (ellipse)
			for (int x = 75; x < 125; x++)
			{
				for (int y = 110; y < 145; y++)
				{
					float mx = (x - 100f) / 25f;
					float my = (y - 127f) / 17f;
					if (mx * mx + my * my < 1f)
						img.SetPixel(x, y, new Color(0.15f, 0.05f, 0.05f));
				}
			}
			// Tongue
			for (int x = 88; x < 112; x++)
			{
				for (int y = 130; y < 145; y++)
				{
					float tx = (x - 100f) / 12f;
					float ty = (y - 137f) / 8f;
					if (tx * tx + ty * ty < 1f)
						img.SetPixel(x, y, new Color(0.85f, 0.4f, 0.4f));
				}
			}
		}
		else
		{
			// Closed smile
			for (int x = 78; x < 122; x++)
			{
				float curve = (x - 100f) / 22f;
				int y = 125 + (int)(curve * curve * 8f);
				for (int dy = 0; dy < 3; dy++)
				{
					if (y + dy < size)
						img.SetPixel(x, y + dy, new Color(0.15f, 0.05f, 0.05f));
				}
			}
		}

		return ImageTexture.CreateFromImage(img);
	}

	private static void DrawCircle(Image img, int cx, int cy, int radius, Color color)
	{
		for (int x = cx - radius; x <= cx + radius; x++)
		{
			for (int y = cy - radius; y <= cy + radius; y++)
			{
				if (x < 0 || x >= img.GetWidth() || y < 0 || y >= img.GetHeight()) continue;
				float dx = x - cx;
				float dy = y - cy;
				if (dx * dx + dy * dy <= radius * radius)
					img.SetPixel(x, y, color);
			}
		}
	}

	public void SetTalking(bool talking)
	{
		_isTalking = talking;
		UpdateSprite();
	}

	public void ShowEmote(string emote)
	{
		_emoteTimer = 3.0f;
		_emoteLabel.Text = emote;
	}

	private void UpdateSprite()
	{
		if (_sprite == null) return;
		_sprite.Texture = _isTalking ? _talkingTexture : _idleTexture;
	}

	public override void _Process(double delta)
	{
		if (_isTalking)
		{
			_bobPhase += (float)delta * 10f;
			float bob = Mathf.Sin(_bobPhase) * 5f;
			Position = _basePosition + new Vector2(0, bob);
			float squash = 1.0f + Mathf.Sin(_bobPhase * 0.7f) * 0.04f;
			Scale = new Vector2(1f / squash, squash);
		}
		else
		{
			_bobPhase = 0;
			Position = _basePosition;
			Scale = Vector2.One;
		}

		if (_emoteTimer > 0)
		{
			_emoteTimer -= (float)delta;
			// Fade out in last second
			if (_emoteTimer < 1f)
				_emoteLabel.Modulate = new Color(1, 1, 1, _emoteTimer);
			else
				_emoteLabel.Modulate = Colors.White;

			if (_emoteTimer <= 0)
				_emoteLabel.Text = "";
		}
	}

	public void SetBasePosition(Vector2 pos)
	{
		_basePosition = pos;
		Position = pos;
	}
}
