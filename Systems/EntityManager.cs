using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SynthSharp;

public class EntityManager
{
    private readonly LinkedList<Entity> entities;
    private HashSet<Entity> pendingRemoval;

    public event Action AllBlocksGone;

    public EntityManager()
    {
        entities = new();
        pendingRemoval = new();
    }

    public void AddEntity(Entity entity)
    {
        entity.Load();
        entities.AddLast(entity);
    }

    public void RemoveEntity(Entity entity)
    {
        pendingRemoval.Add(entity);
    }

    public List<T> GetEntitiesByType<T>() where T : Entity
    {
        List<T> result = new();
        foreach (Entity entity in entities)
        {
            if (entity is T tEntity)
            {
                result.Add(tEntity);
            }
        }
        return result;
    }

    public void Update(GameTime gameTime)
    {
        LinkedListNode<Entity> node = entities.First;
        while (node != null)
        {
            LinkedListNode<Entity> next = node.Next;
            if (pendingRemoval.Contains(node.Value))
            {
                entities.Remove(node);
                pendingRemoval.Remove(node.Value);
            }
            else
            {
                node.Value.Update(gameTime);
            }
            node = next;
        }

        if (!entities.OfType<Block>().Any())
        {
            AllBlocksGone?.Invoke();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (Entity entity in entities)
        {
            entity.Draw(spriteBatch);
        }
    }

    public void Clear()
    {
        entities.Clear();
    }
}