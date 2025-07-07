using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hedonism
{
    public interface IMovable
    {
        void Move(Vector2 direction);
    }
    public class Vector2
    {
        public int x;
        public int y;

        public Vector2(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(Vector2 lhs, Vector2 rhs)
        {
            if ((lhs.x == rhs.x)
                && (lhs.y == rhs.y))
                return true;
            return false;
        }
        public static bool operator !=(Vector2 lhs, Vector2 rhs)
        {
            if ((lhs.x == rhs.x)
                && (lhs.y == rhs.y))
                return false;
            return true;
        }

        public static int Distance(Vector2 a, Vector2 b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }
    }


    public interface ICollidable
    {
        void OnCollision(Actor other);
    }

    public delegate void CollisionHandler(Actor other);

    public abstract class Actor : GameObject, IMovable, IRenderable, ICollidable
    {
        public Actor(LAYER layer)
            : base(layer)
        {

        }
        public Vector2 position { get; protected set; } = new Vector2();
        public RenderInfo renderInfo { get; protected set; } = new RenderInfo();

        public abstract void Move(Vector2 direction);
        public abstract void OnCollision(Actor other);
        public void RegisterRenderer() => GameManager.Instance.RegisterRenderer(this);
        public void SynchronizeRenderPosition() => renderInfo.renderPosition = position;


    }
}
