using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SynthSharp;

public class Text : Entity
{
    public string Content;

    private SpriteFontBase font;
    private Color color;

    public Text(Vector2 position, string content, SpriteFontBase font, Color color)
    {
        this.Position = position;
        this.Content = content;
        this.font = font;
        this.color = color;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        SpriteBatchExtensions.DrawString(spriteBatch, font, Content, Position, color, effect: FontSystemEffect.Stroked, effectAmount: 1);
    }

    public override void Load()
    {
    }

    public override void Update(GameTime gameTime)
    {
    }
}
