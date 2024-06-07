using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SynthSharp;

public class Block : Entity
{

    private ContentManager contentManager;
    private Texture2D Texture;
    private string textureName;

    private readonly Vector2 blockSize = new(128, 32);

    public Block(ContentManager contentManager, Vector2 position, string textureName)
    {
        this.contentManager = contentManager;
        this.Position = position;
        this.Velocity = Vector2.Zero;
        this.textureName = textureName;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Texture, HitBox, Microsoft.Xna.Framework.Color.White);
    }

    public override void Load()
    {
        this.Texture = contentManager.Load<Texture2D>(textureName);
        this.HitBox = new Rectangle((int)Position.X, (int)Position.Y, (int)blockSize.X, (int)blockSize.Y);

        textureName = null;
    }

    public override void Update(GameTime gameTime)
    {
    }
}