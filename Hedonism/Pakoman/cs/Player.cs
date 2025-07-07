using Hedonism;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pakoman.cs
{
    public enum PlayerState
    {
        Default,
        Pill_Burst,
        Dead,

        PlayerState_END
    }


    public class Player : Actor
    {

        public enum Direction
        { 
            Left,
            Right,
            Up,
            Down,

            Direction_END
        }

        

        public Direction currentDirection = Direction.Direction_END;
        public PlayerState currentPlayerState = PlayerState.Default;

        public const double PillBurstDuration = 5.0;
        public static double playerMoveIntervalTime = 0.25;

        private static int PlayerMaxHP = 3;
        private static int currentPlayerHP = 3;

        public static int GetCurrentPlayerHP()
        {
            return currentPlayerHP;
        }
        public static void ResetPlayerHP()
        {
            currentPlayerHP = PlayerMaxHP;
        }
        private static void DecreasePlayerHP()
        {
            --currentPlayerHP;
        }



        public static void SetPlayerMoveIntervalTime(double intervalTime) => playerMoveIntervalTime = intervalTime;

        private double accTime = 0.0;
        private double accMoveTime = 0.0;
        private Vector2 prePosition = new Vector2();

        private bool isAlreadyCollisionEnter = false;

        public PlayerState GetCurrentPlayerState()
        {
            return currentPlayerState;
        }
        public void TurnOnPillBurst()
        {
            renderInfo.renderImage = '●';
            accTime = 0.0;
            currentPlayerState = PlayerState.Pill_Burst;

        }
        public void TurnOffPillBurst()
        {
            renderInfo.renderImage = '○';
            currentPlayerState = PlayerState.Default;
        }

        //5초.
        private bool CheckPillBurstIsDone(double deltaTime)
        {
            accTime += deltaTime;
            if (accTime >= PillBurstDuration)
                return true;
            else
                return false;
        }

        public Player(LAYER layer)
          : base(layer)
        {
            renderInfo.renderImage = '○';

            position = new Vector2(1, CreateMap.Instance.size / 2);
            SynchronizeRenderPosition();


        }
        public override void Move(Vector2 direction)
        {
            accMoveTime += GameManager.Instance.GetDeltaTime();
            if (accMoveTime < playerMoveIntervalTime)
                return;

            accMoveTime = 0.0;
            switch (currentDirection)
            {
                case Direction.Left:
                    if (0 < position.x)
                        --position.x;
                    break;
                case Direction.Right:
                    if (GameManager.ScreenWidth - 1 > position.x)
                        ++position.x;
                    break;
                case Direction.Up:
                    if (0 < position.y)
                        --position.y;
                    break;
                case Direction.Down:
                    if (GameManager.ScreenHeight - 1 > position.y)
                        ++position.y;
                    break;
                default:
                    break;
            }


        }

        private void UpdateDirection()
        {
            INPUT curInput = GameManager.Instance.GetCurrentPressedKey();
            switch (curInput)
            {
                case INPUT.LeftArrow:
                    currentDirection = Direction.Left;
                    break;
                case INPUT.RightArrow:
                    currentDirection = Direction.Right;
                    break;
                case INPUT.UpArrow:
                    currentDirection = Direction.Up;
                    break;
                case INPUT.DownArrow:
                    currentDirection = Direction.Down;
                    break;
                default:
                    break;
            }

            if (INPUT.Tab == curInput)
                GameManager.Instance.LevelChange(GameManager.Instance.GetCurrentLevel() + 1 % 5);
        }

        public override void OnCollision(Actor other)
        {
            switch (other.layer)
            {
                case LAYER.MAP:
                    position.x = prePosition.x;
                    position.y = prePosition.y;
                    SynchronizeRenderPosition();

                    break;
                case LAYER.ENEMY:
                    if (PlayerState.Pill_Burst == currentPlayerState)
                    {
                        //(other as Ghost).ChangeState(GhostState.ReturnHome);


                    }
                    else
                    {
                        if (true == isAlreadyCollisionEnter)
                            break;

                        isAlreadyCollisionEnter = true;

                        GameManager.Instance.RestratCurrentLevel();
                        DecreasePlayerHP();
                        if(0 >= GetCurrentPlayerHP())
                        {
                            ResetPlayerHP();
                            GameManager.Instance.LevelChange(LEVEL.Level_GameOver);
                        }

                    }
                    break;
                case LAYER.PORTAL:
                    if (position.x < CreateMap.Instance.size / 2) position.x += CreateMap.Instance.size - 2;
                    else position.x -= CreateMap.Instance.size - 2;
                    break;
                default:
                    break;

            }


        }
        public override void Update()
        {
            isAlreadyCollisionEnter = false;
            bool isDestroy = false;
            prePosition.x = position.x;
            prePosition.y = position.y;

            UpdateDirection();
            switch (currentPlayerState)
            {
                case PlayerState.Dead:
                    GameManager.Instance.Destroy(this);

                    return;
                //break;
                case PlayerState.Default:
                    Move(new Vector2());

                    break;
                case PlayerState.Pill_Burst:
                    Move(new Vector2());

                    if (true == CheckPillBurstIsDone(GameManager.Instance.GetDeltaTime()))
                    {
                        TurnOffPillBurst();
                    }
                    break;
                default:
                    break;
            }





            if (false == isDestroy)
            {
                SynchronizeRenderPosition();
                RegisterRenderer();
            }
        }
    }
}
