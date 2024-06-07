using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using Myra;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Thickness = Myra.Graphics2D.Thickness;

namespace SynthSharp;

public class MainScene : GameScreen
{
    private new Game1 Game => (Game1)base.Game;

    private readonly Vector2 targetResolution = new(1280, 720);

    public static Texture2D SharedPixel;
    private static KeyboardState KbState => Keyboard.GetState();

    private SongManager songManager;
    private Level[] levels;
    private SpriteFontBase font;
    private List<Song> songs;
    private Ball _ball;
    private Paddle _paddle;
    private Text _scoreText;
    private Text _livesText;
    private GameStatus gameStatus;

    private readonly Color backgroundColor = new Color(0x7B, 0xD3, 0xEA, 0xFF);
    private readonly Color textColor = new Color(0xFF, 0xFF, 0xFF, 0xFF);
    private readonly Color buttonColor = new Color(0xF6, 0xF7, 0xC4, 0xFF);
    private readonly Color accentColor = new Color(0xF6, 0xD6, 0xD6, 0xFF);

    private readonly SpriteBatch spriteBatch;
    private readonly ContentManager contentManager;
    private readonly EntityManager entityManager;
    private readonly FontSystem fontSystem;
    private readonly SoundEffect deathSound;
    private readonly Vector2 paddlePosition;
    private readonly Vector2 ballPosition;
    private readonly Vector2 scoreTextPosition;
    private readonly Vector2 ballVelocity = new(5, 12);
    private readonly List<string> textureNames = new()
    {
        "Textures\\brick_red",
        "Textures\\brick_orange",
        "Textures\\brick_yellow",
        "Textures\\brick_green",
        "Textures\\brick_blue"
    };

    private int score;
    private int lives = 3;
    private int levelNumber = 0;
    private int oldScore;

    // Menu Variables
    private bool menuSetup = false;
    private Desktop _desktop;

    public MainScene(Game1 game) : base(game)
    {
        game.ChangeResolution(targetResolution);

        paddlePosition = new(targetResolution.X / 2, targetResolution.Y - (0.1f * targetResolution.Y));
        ballPosition = new(targetResolution.X / 2, targetResolution.Y / 2);
        scoreTextPosition = new(targetResolution.X * 0.01f, targetResolution.Y - (targetResolution.Y * 0.01f + 48));

        fontSystem = new FontSystem();
        spriteBatch = new SpriteBatch(GraphicsDevice);
        entityManager = new EntityManager();
        contentManager = new ContentManager(Game.Services, "Content");
        deathSound = contentManager.Load<SoundEffect>("death");

        entityManager.AllBlocksGone += MoveToNextLevel;

        gameStatus = GameStatus.NotStarted;
    }

    public override void Draw(GameTime gameTime)
    {
        spriteBatch.Begin();

        GraphicsDevice.Clear(Color.Black);

        // Draw Entities (Paddle, Ball, Blocks
        entityManager.Draw(spriteBatch);

        spriteBatch.End();

        // Draw menu if paused
        if (gameStatus is GameStatus.Paused && menuSetup)
        {
            _desktop.Render();
        }
    }

    public override void LoadContent()
    {
        base.LoadContent();

        FontSystemDefaults.FontResolutionFactor = 1.0f;
        FontSystemDefaults.KernelWidth = 2;
        FontSystemDefaults.KernelHeight = 2;

        SharedPixel = new Texture2D(GraphicsDevice, 1, 1);

        entityManager.AddEntity(new Paddle(contentManager, paddlePosition));
        entityManager.AddEntity(new Ball(contentManager, entityManager, ballPosition, ballVelocity, targetResolution));

        // Load Level from Content, Stream the JSON file
        string json = new StreamReader(TitleContainer.OpenStream($"{Content.RootDirectory}/Levels/levels.json")).ReadToEnd();
        levels = JsonConvert.DeserializeObject<Level[]>(json);

        // Create level
        LoadLevel(levels[levelNumber]);

        // Song Manager
        songs = new()
        {
            contentManager.Load<Song>("Music\\2018"),
            contentManager.Load<Song>("Music\\extrapole"),
            contentManager.Load<Song>("Music\\illusions"),
            contentManager.Load<Song>("Music\\socialmechwarrior"),
            contentManager.Load<Song>("Music\\vape")
        };
        songManager = new SongManager(songs);
        //

        // Add Score Label
        // font = contentManager.Load<SpriteFont>("Fonts/Default");
        fontSystem.AddFont(TitleContainer.OpenStream($"{Content.RootDirectory}/Fonts/Renogare-Regular.ttf"));
        font = fontSystem.GetFont(48);
        entityManager.AddEntity(new Text(scoreTextPosition, $"Score: {score}", font, Color.White));
        //

        // Add Lives Label
        entityManager.AddEntity(new Text(new Vector2(targetResolution.X - 200, targetResolution.Y - 50), $"Lives: {lives}", font, Color.White));

        // Initialize Object References
        _ball = entityManager.GetEntitiesByType<Ball>().FirstOrDefault();
        _paddle = entityManager.GetEntitiesByType<Paddle>().FirstOrDefault();
        _scoreText = entityManager.GetEntitiesByType<Text>().Where(x => x.Content.StartsWith("Score")).FirstOrDefault();
        _livesText = entityManager.GetEntitiesByType<Text>().Where(x => x.Content.StartsWith("Lives")).FirstOrDefault();

        // Subscribe to Events
        _ball.Died += HandleBallDeath;
    }

    public override void Update(GameTime gameTime)
    {
        songManager.Update();

        switch (gameStatus)
        {
            case GameStatus.Playing:
                UpdateGameRunning(gameTime);
                break;

            case GameStatus.Paused:
                UpdateGamePauseMenu(gameTime);
                break;

            case GameStatus.NotStarted:
                UpdateGameStopped(gameTime);
                break;
        }
    }

    private void UpdateGameRunning(GameTime gameTime)
    {
        // Update System for when the game has started, and ball is moving.
        oldScore = score;
        score = _ball.internalScore;

        if (KbState.IsKeyDown(Keys.Escape))
        {
            gameStatus = GameStatus.Paused;
        }

        entityManager.Update(gameTime);

        if (oldScore != score)
        {
            _scoreText.Content = $"Score: {score}";
        }

        if (lives <= 0)
        {
            gameStatus = GameStatus.GameOver;
            deathSound.Play();
            _ball.Position.X = -100;
            _ball.Position.Y = -100;
            _ball.Velocity = Vector2.Zero;
            lives = 3;
            Game._screenManager.LoadScreen(new DeathScene(Game), new FadeTransition(GraphicsDevice, Color.Black));
        }

    }

    private void UpdateGameStopped(GameTime gameTime)
    {
        // This update system will keep the ball velocity at 0
        // Keep bottom of ball attached to top of paddle
        // Start Game when Space is pressed
        _ball.Velocity = Vector2.Zero;
        _ball.Position.X = _paddle.HitBox.Center.X - 16;
        _ball.Position.Y = _paddle.Position.Y - 33;

        // If space is pressed, set game to running
        if (KbState.IsKeyDown(Keys.Space))
        {
            _ball.Velocity = ballVelocity;
            gameStatus = GameStatus.Playing;
        }

        _ball.Update(gameTime);
        _paddle.Update(gameTime);
    }

    private void UpdateGamePauseMenu(GameTime gameTime)
    {
        if (!menuSetup)
        {
            SetupPauseMenu();
            menuSetup = true;
        }
    }

    private void SetupPauseMenu()
    {
        MyraEnvironment.Game = Game;
        var topPanel = new Panel()
        {
            Background = new SolidBrush(Color.Black * 0.8f)
        };
        var childPanel = new Panel()
        {
            Margin = new Thickness(36),
            BorderThickness = new Thickness(2),
            Padding = new Thickness(20, 20, 0, 0),
            Background = new SolidBrush(Color.Transparent)
        };
        var verticalStackPanel = new VerticalStackPanel()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 10
        };
        var labelText = new Label()
        {
            Text = "/esPause Menu",
            Font = fontSystem.GetFont(72),
            TextColor = textColor,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        var resetButton = new Button()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidBrush(Color.Transparent),
            OverBackground = new SolidBrush(Color.Transparent),
            PressedBackground = new SolidBrush(Color.Transparent),
            Content = new Label()
            {
                Text = "/es/tuReset/tu",
                Font = fontSystem.GetFont(48),
                TextColor = Color.White,
                OverTextColor = accentColor,
                Width = (int)fontSystem.GetFont(48).MeasureString("Reset").X,
                Height = (int)fontSystem.GetFont(48).MeasureString("Reset").Y,
                TextAlign = (FontStashSharp.RichText.TextHorizontalAlignment)HorizontalAlignment.Left,
                Padding = new Thickness(5, 5, 5, 0)
            }
        };
        resetButton.Click += (s, a) =>
        {
            // Play Click Sound
            SongManager.PlaySpecific(contentManager.Load<SoundEffect>("click"));
            CleanupScene();
            Game._screenManager.LoadScreen(new MainScene(Game), new FadeTransition(GraphicsDevice, Color.Black));
        };
        var resumeButton = new Button()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidBrush(Color.Transparent),
            OverBackground = new SolidBrush(Color.Transparent),
            PressedBackground = new SolidBrush(Color.Transparent),
            Content = new Label()
            {
                Text = "/es/tuResume/tu",
                Font = fontSystem.GetFont(48),
                TextColor = Color.White,
                OverTextColor = accentColor,
                Width = (int)fontSystem.GetFont(48).MeasureString("Resume").X,
                Height = (int)fontSystem.GetFont(48).MeasureString("Resume").Y,
                TextAlign = (FontStashSharp.RichText.TextHorizontalAlignment)HorizontalAlignment.Left,
                Padding = new Thickness(5, 5, 5, 0)
            }
        };
        resumeButton.Click += (s, a) =>
        {
            // Play Click Sound
            SongManager.PlaySpecific(contentManager.Load<SoundEffect>("click"));
            gameStatus = GameStatus.Playing;
        };
        var quitButton = new Button()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidBrush(Color.Transparent),
            OverBackground = new SolidBrush(Color.Transparent),
            PressedBackground = new SolidBrush(Color.Transparent),
            Content = new Label()
            {
                Text = "/es/tuQuit/tu",
                Font = fontSystem.GetFont(48),
                TextColor = Color.White,
                OverTextColor = accentColor,
                Width = (int)fontSystem.GetFont(48).MeasureString("Quit").X,
                Height = (int)fontSystem.GetFont(48).MeasureString("Quit").Y,
                TextAlign = (FontStashSharp.RichText.TextHorizontalAlignment)HorizontalAlignment.Left,
                Padding = new Thickness(5, 5, 5, 0)
            }
        };
        quitButton.Click += (s, a) =>
        {
            // Play Click Sound
            SongManager.PlaySpecific(contentManager.Load<SoundEffect>("click"));
            CleanupScene();
            Game._screenManager.LoadScreen(new IntroScene(Game), new FadeTransition(GraphicsDevice, Color.Black));
        };
        childPanel.Widgets.Add(labelText);

        verticalStackPanel.Widgets.Add(resumeButton);
        verticalStackPanel.Widgets.Add(resetButton);
        verticalStackPanel.Widgets.Add(quitButton);

        childPanel.Widgets.Add(verticalStackPanel);
        topPanel.Widgets.Add(childPanel);
        _desktop = new Desktop
        {
            Root = topPanel
        };
    }

    private void LoadLevel(Level level)
    {
        int rows = level.Layout.GetLength(0);
        int columns = level.Layout.GetLength(1);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (level.Layout[i, j] == 1)
                {
                    var block = new Block(contentManager, new Vector2(j * 128, i * 32), textureNames[i]);
                    entityManager.AddEntity(block);
                }
            }
        }
    }

    private void HandleBallDeath()
    {
        lives--;
        _ball.internalScore -= 100;
        _livesText.Content = $"Lives: {lives}";
        gameStatus = GameStatus.NotStarted;
    }

    private void MoveToNextLevel()
    {
        gameStatus = GameStatus.NotStarted;
        levelNumber++;
        LoadLevel(levels[levelNumber]);
        lives = 3;
    }

    private void CleanupScene()
    {
        entityManager.Clear();
        contentManager.Unload();
    }
}