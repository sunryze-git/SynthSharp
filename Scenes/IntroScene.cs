using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Tweening;
using SynthSharp;

public class Vector2Wrapper
{
    public Vector2 Value
    {
        get; set;
    }
}

public class IntroScene : GameScreen
{
    private new Game1 Game => (Game1)base.Game;

    private readonly Color purple = new Color(0x7B, 0xD3, 0xEA, 0xFF);
    private readonly Vector2 targetResolution = new(1280, 720);
    private readonly int introSeconds = 3;
    private readonly int fadeSeconds = 1;
    private readonly string textLabelContent = "Produced by SR-GIT";
    private readonly Tweener _tweener = new Tweener();

    private Vector2Wrapper textPos;
    private Vector2 textPosTarget;

    private Vector2Wrapper iconPos;
    private Vector2 iconPosTarget;

    private double elapsedSeconds = 0;
    private float alpha = 0;

    private SpriteBatch spriteBatch;
    private ContentManager contentManager;
    private Texture2D Icon;
    private Texture2D Background;
    private SpriteFontBase font;
    private FontSystem fontSystem;

    private bool hasFlipped = false;
    private bool iconReversed = false;
    private bool soundPlayed = false;

    // Procedure:
    // 1. Image is the icon texture in middle of display, with TextLabel below it.
    // 2. Once we reach FadeOut, reverse the colors of the icon and text, play the click sound, and set BG Color to purple.
    // 3. Fade Out Afterward.

    public IntroScene(Game1 game) : base(game)
    {
        game.ChangeResolution(targetResolution);

        fontSystem = new();
        fontSystem.AddFont(TitleContainer.OpenStream($"{Content.RootDirectory}/Fonts/Renogare-Regular.ttf"));
        font = fontSystem.GetFont(72);
    }

    public override void Draw(GameTime gameTime)
    {
        spriteBatch.Begin();

        switch (hasFlipped)
        {
            case true:
                spriteBatch.Draw(Background, Vector2.Zero, Color.White);
                break;
            case false:
                GraphicsDevice.Clear(Color.Black);
                break;
        }

        // Draw Icon and Text based on alpha value set
        spriteBatch.Draw(Icon, iconPos.Value, Color.White * alpha);
        SpriteBatchExtensions.DrawString(spriteBatch, font, textLabelContent, textPos.Value, Color.White * alpha, effect: FontSystemEffect.Stroked, effectAmount: 1);

        spriteBatch.End();
    }

    public override void Update(GameTime gameTime)
    {
        _tweener.Update(gameTime.GetElapsedSeconds());

        // Fade image in fadeSeconds
        elapsedSeconds += gameTime.ElapsedGameTime.TotalSeconds;

        if (elapsedSeconds < fadeSeconds)
        {
            // fade In
            alpha = (float)(elapsedSeconds / fadeSeconds);
        }
        else if (elapsedSeconds < fadeSeconds + introSeconds)
        {
            // Stay Faded In
            alpha = 1;
        }
        else if (elapsedSeconds < fadeSeconds + introSeconds + 2)
        {
            // Flip
            hasFlipped = true;
            PlaySound();
            ReverseIconColors();
            alpha = 1;
        }
        else if (elapsedSeconds < fadeSeconds + introSeconds + 2 + fadeSeconds)
        {
            // Fade Out
            alpha = 1 - (float)((elapsedSeconds - fadeSeconds - introSeconds - 2) / fadeSeconds);
        }
        else
        {
            // Transition to next scene
            Game._screenManager.LoadScreen(new MenuScene(Game));
        }
    }

    public override void LoadContent()
    {
        base.LoadContent();

        spriteBatch = new SpriteBatch(GraphicsDevice);
        contentManager = new ContentManager(Game.Services, "Content");
        Icon = contentManager.Load<Texture2D>("Images//icon");
        Background = contentManager.Load<Texture2D>("Images//background");

        iconPos = new Vector2Wrapper { Value = new Vector2((GraphicsDevice.Viewport.Width - Icon.Width) / 2, -Icon.Height - 10) };
        textPos = new Vector2Wrapper { Value = new Vector2((GraphicsDevice.Viewport.Width - font.MeasureString(textLabelContent).X) / 2, GraphicsDevice.Viewport.Height + font.MeasureString(textLabelContent).Y + 5) };

        iconPosTarget = new Vector2((GraphicsDevice.Viewport.Width - Icon.Width) / 2, (GraphicsDevice.Viewport.Height - Icon.Height) / 3);
        textPosTarget = new Vector2((GraphicsDevice.Viewport.Width - font.MeasureString(textLabelContent).X) / 2, iconPosTarget.Y + Icon.Height + 100);

        _tweener.TweenTo(target: textPos, expression: x => textPos.Value, toValue: textPosTarget, duration: 2, delay: 1)
            .Easing(EasingFunctions.ExponentialOut);

        _tweener.TweenTo(target: iconPos, expression: x => iconPos.Value, toValue: iconPosTarget, duration: 2, delay: 1)
            .Easing(EasingFunctions.ExponentialOut);

    }

    private void PlaySound()
    {
        if (!soundPlayed)
        {
            // Play intro sound
            SongManager.PlaySpecific(contentManager.Load<SoundEffect>("click"));
            soundPlayed = true;
        }
    }

    private void ReverseIconColors()
    {
        if (!iconReversed)
        {
            // Reverse Icon Colors
            Color[] data = new Color[Icon.Width * Icon.Height];
            Icon.GetData(data);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new Color(255 - data[i].R, 255 - data[i].G, 255 - data[i].B, data[i].A);
            }
            Icon.SetData(data);
            iconReversed = true;
        }
    }
}