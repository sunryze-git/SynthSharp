using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SynthSharp;

public abstract class Entity
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Rectangle HitBox;

    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch);
    public abstract void Load();
}