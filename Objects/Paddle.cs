using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SynthSharp;

public class Paddle : Entity
{
    private Texture2D Texture;
    private ContentManager contentManager;

    private readonly Vector2 paddleSize = new(256, 64);
    private const float paddleScale = 0.5f;

    private Vector2 desiredPaddleSize;

    public Paddle(ContentManager contentManager, Vector2 position)
    {
        this.desiredPaddleSize = new Vector2(paddleSize.X * paddleScale, paddleSize.Y * paddleScale);
        this.contentManager = contentManager;
        Position = position;
        Velocity = Vector2.Zero;
    }

    public override void Load()
    {
        Texture = contentManager.Load<Texture2D>("Textures\\paddle");
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Texture, HitBox, Color.White);
    }

    public override void Update(GameTime gameTime)
    {
        Position += Velocity;
        HitBox = new Rectangle((int)Position.X, (int)Position.Y, (int)desiredPaddleSize.X, (int)desiredPaddleSize.Y);

        // Adjust velocity of paddle according to X distance to mouse (smoothly)
        var mouseState = Mouse.GetState();
        var mousePosition = new Vector2(mouseState.X, mouseState.Y);
        var paddleCenter = Position.X + (int)desiredPaddleSize.X / 2;
        var distance = mousePosition.X - paddleCenter;
        Velocity = new Vector2(distance / 10, 0);
    }
}