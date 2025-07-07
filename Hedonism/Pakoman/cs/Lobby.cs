using Pakoman.cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pakoman.cs.Player;

namespace Hedonism
{
    public class Lobby : Actor
    {
        public Lobby(LAYER layer)
          : base(layer)
        {
            GameManager.Instance.RegisterGameObject(layer, this);
        }
        public override void Move(Vector2 direction)
        {


        }

        public override void OnCollision(Actor other)
        {



        }
        public override void Update()
        {
            INPUT curInput = GameManager.Instance.GetCurrentPressedKey();
            LEVEL curLevel = GameManager.Instance.GetCurrentLevel();

            if (INPUT.Enter == curInput)
            {
                GameManager.Instance.GameQuit();
            }

            if (INPUT.Tab == curInput)
            {
                switch (curLevel)
                {
                    case LEVEL.Level_Lobby:
                        GameManager.Instance.LevelChange(LEVEL.Level_1);
                        break;
                    case LEVEL.Level_GameOver:
                        GameManager.Instance.LevelChange(LEVEL.Level_Lobby);
                        break;
                    case LEVEL.Level_GameEnd:
                        GameManager.Instance.LevelChange(LEVEL.Level_Lobby);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
