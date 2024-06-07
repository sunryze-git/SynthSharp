using System.Collections.Generic;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Screens;
using SynthSharp;
using Myra;
using Myra.Graphics2D.UI;
using MonoGame.Extended.Screens.Transitions;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Microsoft.Xna.Framework.Audio;

public class MenuScene : GameScreen
{
    private new Game1 Game => (Game1)base.Game;

    private SpriteBatch spriteBatch;
    private ContentManager contentManager;
    private EntityManager entityManager;
    private SongManager songManager;
    private FontSystem fontSystem;
    private Desktop _desktop;
    private List<Song> songs;
    private Texture2D Background;

    //  7BD3EA Primary
    //  A1EEBD Secondary
    //  F6F7C4 Third
    //  F6D6D6 Fourth

    private readonly Vector2 targetResolution = new(1280, 720);
    private readonly Color backgroundColor = new Color(0x7B, 0xD3, 0xEA, 0xFF);
    private readonly Color textColor = new Color(0xFF, 0xFF, 0xFF, 0xFF);
    private readonly Color buttonColor = new Color(0xF6, 0xF7, 0xC4, 0xFF);
    private readonly Color accentColor = new Color(0xF6, 0xD6, 0xD6, 0xFF);

    public MenuScene(Game1 game) : base(game)
    {
        game.ChangeResolution(targetResolution);

        fontSystem = new();
        spriteBatch = new SpriteBatch(GraphicsDevice);
        entityManager = new EntityManager();
        contentManager = new ContentManager(Game.Services, "Content");

    }

    public override void Draw(GameTime gameTime)
    {
        spriteBatch.Begin();

        GraphicsDevice.Clear(backgroundColor);
        spriteBatch.Draw(Background, Vector2.Zero, Color.White);

        entityManager.Draw(spriteBatch);
        spriteBatch.End();

        _desktop.Render();
    }

    public override void LoadContent()
    {
        base.LoadContent();

        Background = contentManager.Load<Texture2D>("Images\\background");

        // Load Menu Song
        songs = new()
        {
            contentManager.Load<Song>("Music\\menu")
        };
        songManager = new(songs);
        //

        // Add Renogare Font
        fontSystem.AddFont(TitleContainer.OpenStream($"{Content.RootDirectory}/Fonts/Renogare-Regular.ttf"));

        // Add New Particle
        entityManager.AddEntity(new StupidEffect(Game.GraphicsDevice));

        // Load Stupid UI
        MyraEnvironment.Game = Game;

        var topPanel = new Panel();

        var childPanel = new Panel()
        {
            Margin = new Thickness(36),
            BorderThickness = new Thickness(2),
            Padding = new Thickness(20, 20, 0, 0)
        };

        var labelText = new Label()
        {
            Text = "/esPaddle Pop!",
            Font = fontSystem.GetFont(72),
            TextColor = textColor,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        var paddedCenterButton = new Button()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidBrush(Color.Transparent),
            OverBackground = new SolidBrush(Color.Transparent),
            PressedBackground = new SolidBrush(Color.Transparent),
            Content = new Label()
            {
                Text = "/es/tuPlay/tu",
                Font = fontSystem.GetFont(48),
                TextColor = Color.White,
                OverTextColor = accentColor,
                Width = (int)fontSystem.GetFont(48).MeasureString("Play").X,
                Height = (int)fontSystem.GetFont(48).MeasureString("Play").Y,
                TextAlign = (FontStashSharp.RichText.TextHorizontalAlignment)HorizontalAlignment.Left,
                Padding = new Thickness(5, 5, 5, 0)
            }
        };
        paddedCenterButton.Click += (s, a) =>
        {
            // Play Click Sound
            SongManager.PlaySpecific(contentManager.Load<SoundEffect>("click"));

            Game._screenManager.LoadScreen(new MainScene(Game), new FadeTransition(GraphicsDevice, Color.Black));
        };

        childPanel.Widgets.Add(labelText);
        childPanel.Widgets.Add(paddedCenterButton);

        topPanel.Widgets.Add(childPanel);

        _desktop = new Desktop
        {
            Root = topPanel
        };
    }

    public override void Update(GameTime gameTime)
    {
        songManager.Update();

        entityManager.Update(gameTime);
    }
}