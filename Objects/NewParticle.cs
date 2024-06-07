using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.TextureAtlases;

//  7BD3EA Primary
//  A1EEBD Secondary
//  F6F7C4 Third
//  F6D6D6 Fourth

namespace SynthSharp;

public class StupidEffect : Entity
{
    private ParticleEffect _particleEffect;
    private Texture2D _particleTexture;

    private GraphicsDevice graphicsDevice;

    private Color color = new Color(0xF6, 0xD6, 0xD6, 0xFF);

    public StupidEffect(GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_particleEffect);
    }

    public override void Load()
    {
        _particleTexture = new Texture2D(graphicsDevice, 1, 1);
        _particleTexture.SetData(new[] { Color.White });

        TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture);
        _particleEffect = new ParticleEffect(autoTrigger: false)
        {
            Position = new Vector2(1280 / 2, 720 / 2),
            Emitters = new List<ParticleEmitter>
            {
                new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(2.5),
                    Profile.BoxFill(1280,720))
                {
                    Parameters = new ParticleReleaseParameters
                    {
                        Speed = new Range<float>(0f, 50f),
                        Quantity = 3,
                        Rotation = new Range<float>(-1f, 1f),
                        Scale = new Range<float>(3.0f, 4.0f)
                    },
                    Modifiers =
                    {
                        new AgeModifier
                        {
                            Interpolators =
                            {
                                new ColorInterpolator
                                {
                                    StartValue = color.ToHsl(),
                                    EndValue = new HslColor(0.5f, 0.9f, 1.0f)
                                }
                            }
                        },
                        new RotationModifier {RotationRate = -2.1f},
                        new RectangleContainerModifier {Width = 1280, Height = 720},
                    }
                }
            }
        };
    }

    public override void Update(GameTime gameTime)
    {
        _particleEffect.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }
}