using Hedonism;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Hedonism
{
    public class TestObj : Actor
    {
        public TestObj(LAYER layer)
            : base(layer)
        {
            renderInfo.renderImage = '★';
            
            position = new Vector2(0, 5);
            SynchronizeRenderPosition();
        }
        public override void Move(Vector2 direction)
        {


        }

        public override void OnCollision(Actor other)
        {



        }
        public override void Update()
        {
            bool isDestroy = false;

            ++position.x;
            if (GameManager.ScreenWidth <= position.x)
            {
                ++position.y;
                if (7 <= position.y)
                {
                    GameManager.Instance.Destroy(this);
                    isDestroy = true;
                }
                position.x = 0;
            }

            if (false == isDestroy)
            {
                SynchronizeRenderPosition();
                RegisterRenderer();
            }

        }


    }
}
