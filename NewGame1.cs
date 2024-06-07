using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Screens;

namespace SynthSharp;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    public readonly ScreenManager _screenManager;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);

        // Set Titlebar
        Window.Title = "Paddle Pop!";

        _screenManager = new();
        Components.Add(_screenManager);

        _screenManager = Components.Add<ScreenManager>();
    }

    public void ChangeResolution(Vector2 resolution)
    {
        _graphics.PreferredBackBufferWidth = (int)resolution.X;
        _graphics.PreferredBackBufferHeight = (int)resolution.Y;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _screenManager.LoadScreen(new IntroScene(this));
    }
}
