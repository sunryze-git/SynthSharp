using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Linq;

namespace SynthSharp;

public class Ball : Entity
{

    private Texture2D texture;
    private ContentManager contentManager;
    private EntityManager entityManager;
    private SoundEffect hitSound;

    private readonly Vector2 ballSize = new(64, 64);
    private const float ballScale = 0.5f;

    private Vector2 desiredBallSize;
    private Vector2 viewportRes;

    public int internalScore = 0;

    public event Action Died;

    public Ball(ContentManager contentManager, EntityManager entityManager, Vector2 position, Vector2 velocity, Vector2 viewport)
    {
        this.contentManager = contentManager;
        this.entityManager = entityManager;
        this.desiredBallSize = new Vector2(ballSize.X * ballScale, ballSize.Y * ballScale);
        this.viewportRes = viewport;
        Position = position;
        Velocity = velocity;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, HitBox, Color.White);
    }

    public override void Load()
    {
        this.texture = contentManager.Load<Texture2D>("Textures\\ball");
        this.hitSound = contentManager.Load<SoundEffect>("hit");
        this.HitBox = new Rectangle((int)Position.X, (int)Position.Y, (int)desiredBallSize.X, (int)desiredBallSize.Y);
    }

    public override void Update(GameTime gameTime)
    {
        Position += Velocity;
        HitBox = new Rectangle((int)Position.X, (int)Position.Y, (int)desiredBallSize.X, (int)desiredBallSize.Y);

        // Check for Paddle Collision
        var paddle = entityManager.GetEntitiesByType<Paddle>().FirstOrDefault();
        if (paddle != null && HitBox.Intersects(paddle.HitBox))
        {
            HandleCollision(paddle);
        }

        // Bounce off blocks
        var block = entityManager.GetEntitiesByType<Block>().Where(x => HitBox.Intersects(x.HitBox)).FirstOrDefault();
        if (block != null)
        {
            HandleCollision(block);
            hitSound.Play();
            entityManager.RemoveEntity(block);
            internalScore += 100;
        }

        // Moderate velocity to a maximum
        if (Velocity.Length() > 10)
        {
            Velocity = Vector2.Normalize(Velocity) * 8;
        }

        // Keep Y velocity at least 4
        if (Math.Abs(Velocity.Y) < 4)
        {
            Velocity.Y = Math.Sign(Velocity.Y) * 4;
        }

        // Bounce off display bounds
        if (Position.X <= 0 || Position.X + desiredBallSize.X >= viewportRes.X)
        {
            Velocity.X = -Velocity.X;
        }

        if (Position.Y <= 0 || Position.Y + desiredBallSize.Y >= viewportRes.Y)
        {
            Velocity.Y = -Velocity.Y;
        }

        if (Position.Y + desiredBallSize.Y >= viewportRes.Y)
        {
            Died?.Invoke();
        }

    }

    private void HandleCollision(Entity entity)
    {
        // Determine side of collision
        float left = Math.Abs(HitBox.Right - entity.HitBox.Left);
        float right = Math.Abs(HitBox.Left - entity.HitBox.Right);
        float top = Math.Abs(HitBox.Bottom - entity.HitBox.Top);
        float bottom = Math.Abs(HitBox.Top - entity.HitBox.Bottom);
        float min = Math.Min(Math.Min(Math.Min(left, right), top), bottom);

        // Add 25% of the entity's X velocity to the Ball X velocity
        if (entity is Paddle paddle)
        {
            Velocity.X += paddle.Velocity.X * 0.25f;
        }

        // Always cut a bit of X velocity to simulate friction
        Velocity.X *= 0.85f;

        // Correct position and reflect velocity
        if (min == left)
        {
            Position.X = entity.HitBox.Left - HitBox.Width;
            Velocity.X = -Math.Abs(Velocity.X);
        }
        else if (min == right)
        {
            Position.X = entity.HitBox.Right;
            Velocity.X = Math.Abs(Velocity.X);
        }
        else if (min == top)
        {
            Position.Y = entity.HitBox.Top - HitBox.Height;
            Velocity.Y = -Math.Abs(Velocity.Y);
        }
        else if (min == bottom)
        {
            Position.Y = entity.HitBox.Bottom;
            Velocity.Y = Math.Abs(Velocity.Y);
        }

        // Update Hitbox
        HitBox = new Rectangle((int)Position.X, (int)Position.Y, (int)desiredBallSize.X, (int)desiredBallSize.Y);
    }
}