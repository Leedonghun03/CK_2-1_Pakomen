using Pakoman.cs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hedonism
{
    public enum LAYER
    {
        LOBBY,

        MAP,
        ITEM,
        PORTAL,
        ENEMY,
        PLAYER,
        UI,
        
        LAYER_END
    }

    public enum INPUT // Reference by enum ConsoleKey
    {
        Backspace = 8,
        Tab = 9,
        Enter = 13,
        Escape = 27,
        Spacebar = 32,
        LeftArrow = 37,
        UpArrow = 38,
        RightArrow = 39,
        DownArrow = 40,

        NumPad1 = 97,
        NumPad2 = 98,

        INPUT_END
    }

    public enum LEVEL
    {
        Level_Lobby,
        Level_1,
        Level_2,
        Level_3,
        Level_GameEnd,
        Level_GameOver,

        LEVEL_END
    }

    public class LayerCollisionMatrix
    {
        public LayerCollisionMatrix(LAYER layer1, LAYER layer2)
        {
            this.layer1 = layer1;
            this.layer2 = layer2;
        }
        public LAYER layer1;
        public LAYER layer2;
    }


    public class GameManager : Singleton<GameManager>
    {
        public const int DefaultThreadCount = 8;
        public const int ScreenWidth = 50;
        public const int ScreenHeight = 50;

        public const int TargetFrame = 10;

        public const char SpaceChar = 'ㅤ';
        public const char NullChar = '\0';
        public const int MilliSecond = 1000;

        public const LEVEL StartLevelInfo = LEVEL.Level_Lobby;

        private List<GameObject>[] currentGameObjects = new List<GameObject>[(int)LAYER.LAYER_END];
        private List<GameObject> reservedRemoveObjects = new List<GameObject>();
        private List<Actor> currentRenderObjects = new List<Actor>();
        private char[,] previousRenderState = new char[ScreenHeight, ScreenWidth];
        private char[,] currentRenderState = new char[ScreenHeight, ScreenWidth];

        private List<LayerCollisionMatrix> setupLayerCollisionMatrix = new List<LayerCollisionMatrix>();

        public delegate void SetupLevelFunc();
        private Dictionary<LEVEL, SetupLevelFunc> levelSetupFunctions = new Dictionary<LEVEL, SetupLevelFunc>();
        private LEVEL currentLevel = LEVEL.LEVEL_END;
        private LEVEL nextLevel = StartLevelInfo;
        private bool isRestart = false;


        private Stopwatch fpsStopwatch = new Stopwatch();
        private bool isRunning = true;

        private double deltaTime = 0.0;

        private INPUT currentPressedInput = INPUT.INPUT_END;

        //===================================================================================================================================================
        //▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

        public LEVEL GetCurrentLevel()
        {
            return currentLevel;
        }

        public void RegisterGameObject(LAYER layer, GameObject gameObject) // 오브젝트를 등록.
        {
            if (true == (gameObject is GameObject))
                currentGameObjects[(int)layer].Add(gameObject);
        }


        public void SetupLobby()
        {
            ScoreManager.Instance.ResetScore();


            Text Message = new Text(LAYER.UI, new Vector2(18, 15), "Welcom PaKoMaN");
            Text StartButton = new Text(LAYER.UI, new Vector2(18, 20), "Start Press Tap");
            Text EndButton = new Text(LAYER.UI, new Vector2(18, 22), "Exit Press Enter");

            Lobby Scene = new Lobby(LAYER.LOBBY);
        }

        public void SetupLevel_GameOver()
        {
            Text Message = new Text(LAYER.UI, new Vector2(22, 15), "GameOver");
            Text ScorText = new Text(LAYER.UI, new Vector2(18, 20), "Score: " + ScoreManager.Instance.GetScore().ToString());
            Text ReStartButton = new Text(LAYER.UI, new Vector2(18, 22), "ReStart Press Tap");
            Text EndButton = new Text(LAYER.UI, new Vector2(18, 24), "Exit Press Enter");

            Lobby Scene = new Lobby(LAYER.LOBBY);
        }

        public void SetupLEVEL_END()
        {
            Text Message = new Text(LAYER.UI, new Vector2(20, 15), "Complet Game");
            Text ScorText = new Text(LAYER.UI, new Vector2(18, 20), "Score: " + ScoreManager.Instance.GetScore().ToString());
            Text ReStartButton = new Text(LAYER.UI, new Vector2(18, 22), "ReStart Press Tap");
            Text EndButton = new Text(LAYER.UI, new Vector2(18, 24), "Exit Press Enter");

            Lobby Scene = new Lobby(LAYER.LOBBY);
        }

        public void SetupLevel1()
        {
            Player.SetPlayerMoveIntervalTime(0.2);
            Ghost.SetGhostMoveIntervalTime(0.3);

            Player player = new Player(LAYER.PLAYER);
            GameManager.Instance.RegisterGameObject(player.layer, player);

            CreateMap createMap = new CreateMap();
            createMap.BuildMaze();

            Ghost ghost1 = new Ghost(LAYER.ENEMY, new Vector2(createMap.walls.GetLength(1) / 2 - 1, createMap.walls.GetLength(0) / 2 + 1));
            ghost1.playerActor = player;
            ghost1.mapWalls = createMap.walls;

            Ghost ghost2 = new Ghost(LAYER.ENEMY, new Vector2(createMap.walls.GetLength(1) / 2 + 1, createMap.walls.GetLength(0) / 2 + 1));
            ghost2.playerActor = player;
            ghost2.mapWalls = createMap.walls;

            Ghost ghost3 = new Ghost(LAYER.ENEMY, new Vector2(createMap.walls.GetLength(1) / 2 - 1, createMap.walls.GetLength(0) / 2 - 1));
            ghost3.playerActor = player;
            ghost3.mapWalls = createMap.walls;

            Ghost ghost4 = new Ghost(LAYER.ENEMY, new Vector2(createMap.walls.GetLength(1) / 2 + 1, createMap.walls.GetLength(0) / 2 - 1));
            ghost4.playerActor = player;
            ghost4.mapWalls = createMap.walls;

            UI PlayerHP = new UI(LAYER.UI);
            Lobby Scene = new Lobby(LAYER.LOBBY);
        }

        public void SetupLevel2()
        {
            Player.SetPlayerMoveIntervalTime(0.2);
            Ghost.SetGhostMoveIntervalTime(0.25);

            Player player = new Player(LAYER.PLAYER);
            GameManager.Instance.RegisterGameObject(player.layer, player);

            CreateMap createMap = new CreateMap();
            createMap.BuildMaze();

            Ghost ghost1 = new Ghost(LAYER.ENEMY, new Vector2(11, 11));
            ghost1.playerActor = player;
            ghost1.mapWalls = createMap.walls;

            Ghost ghost2 = new Ghost(LAYER.ENEMY, new Vector2(11, 13));
            ghost2.playerActor = player;
            ghost2.mapWalls = createMap.walls;

            Ghost ghost3 = new Ghost(LAYER.ENEMY, new Vector2(13, 11));
            ghost3.playerActor = player;
            ghost3.mapWalls = createMap.walls;

            Ghost ghost4 = new Ghost(LAYER.ENEMY, new Vector2(13, 13));
            ghost4.playerActor = player;
            ghost4.mapWalls = createMap.walls;

            UI PlayerHP = new UI(LAYER.UI);
            Lobby Scene = new Lobby(LAYER.LOBBY);
        }

        public void SetupLevel3()
        {
            Player.SetPlayerMoveIntervalTime(0.2);
            Ghost.SetGhostMoveIntervalTime(0.2);

            Player player = new Player(LAYER.PLAYER);
            GameManager.Instance.RegisterGameObject(player.layer, player);

            CreateMap createMap = new CreateMap();
            createMap.BuildMaze();

            Ghost ghost1 = new Ghost(LAYER.ENEMY, new Vector2(11, 11));
            ghost1.playerActor = player;
            ghost1.mapWalls = createMap.walls;

            Ghost ghost2 = new Ghost(LAYER.ENEMY, new Vector2(11, 13));
            ghost2.playerActor = player;
            ghost2.mapWalls = createMap.walls;

            Ghost ghost3 = new Ghost(LAYER.ENEMY, new Vector2(13, 11));
            ghost3.playerActor = player;
            ghost3.mapWalls = createMap.walls;

            Ghost ghost4 = new Ghost(LAYER.ENEMY, new Vector2(13, 13));
            ghost4.playerActor = player;
            ghost4.mapWalls = createMap.walls;

            UI PlayerHP = new UI(LAYER.UI);
            Lobby Scene = new Lobby(LAYER.LOBBY);
        }

        private void SetupLevelFuncInfo(LEVEL targetLevel) //여기다가 각 씬의 초기화 함수 등록하기.
        {
            switch (targetLevel)
            {
                case LEVEL.Level_Lobby:
                    levelSetupFunctions[targetLevel] += SetupLobby;
                    break;
                case LEVEL.Level_1:
                    levelSetupFunctions[targetLevel] += SetupLevel1;
                    break;
                case LEVEL.Level_2:
                    levelSetupFunctions[targetLevel] += SetupLevel2;
                    break;
                case LEVEL.Level_3:
                    levelSetupFunctions[targetLevel] += SetupLevel3;
                    break;
                case LEVEL.Level_GameOver:
                    levelSetupFunctions[targetLevel] += SetupLevel_GameOver;
                    break;
                case LEVEL.Level_GameEnd:
                    levelSetupFunctions[targetLevel] += SetupLEVEL_END;
                    break;
                default:
                    break;
            }
        }

        public void Initialize() // 초기화 함수.
        {
            DefaultGameSetup();

            LevelChange(StartLevelInfo);


        }

        public void SetupCollisionLayerMatrix() // 충돌 레이어 설정.
        {
            setupLayerCollisionMatrix.Add(new LayerCollisionMatrix(LAYER.PLAYER, LAYER.ENEMY));
            setupLayerCollisionMatrix.Add(new LayerCollisionMatrix(LAYER.PLAYER, LAYER.MAP));
            setupLayerCollisionMatrix.Add(new LayerCollisionMatrix(LAYER.PLAYER, LAYER.ITEM));
            setupLayerCollisionMatrix.Add(new LayerCollisionMatrix(LAYER.PLAYER, LAYER.PORTAL));

        }

        public void Destroy(GameObject gameObject) //오브젝트 삭제 함수.
        {
            reservedRemoveObjects.Add(gameObject);
            
        }

        public void GameProcess()
        {
            double targetFrameTime = 1.0 / TargetFrame;

            while (true == isRunning)
            {
                deltaTime = fpsStopwatch.Elapsed.TotalSeconds;
                fpsStopwatch.Restart();

                CheckChangeLevel();

                RemoveReservedObject();
                CheckCurrentPressedKeyInfo();
                Update();
                CheckCollision();
                Render();

                double remaining = targetFrameTime - deltaTime;
                if (0 < remaining)
                    Thread.Sleep((int)(remaining * MilliSecond));
            }


            GameExitProcess();

        }

        public void RestratCurrentLevel()
        {
            isRestart = true;

        }
        public void LevelChange(LEVEL targetLevel)
        {
            nextLevel = targetLevel;

        }

        public INPUT GetCurrentPressedKey()
        {
            return currentPressedInput;
        }

        public double GetDeltaTime()
        {
            return deltaTime;
        }

        public void GameQuit() => isRunning = false;
        //▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
        //===================================================================================================================================================

        public void RegisterRenderer(Actor actorObject)
        {
            if (true == (actorObject is Actor))
                currentRenderObjects.Add(actorObject);
        }

        private void DefaultGameSetup()
        {
            ThreadManager.Instance.Initialize(DefaultThreadCount);

            Console.CursorVisible = false;

            Console.SetWindowSize(1, 1);
            Console.SetBufferSize(ScreenWidth * 2, ScreenHeight);
            Console.SetWindowSize(ScreenWidth * 2, ScreenHeight);

            for (int i = 0; i < currentGameObjects.Length; i++)
            {
                currentGameObjects[i] = new List<GameObject>();
            }
            for (int i = 0; i < (int)LEVEL.LEVEL_END; i++)
            {
                levelSetupFunctions[(LEVEL)i] = null;
            }

            SetupCollisionLayerMatrix();
            InitializeLevelSetupFunctions();

        }

        private void InitializeLevelSetupFunctions()
        {
            for (int i = 0; i < (int)LEVEL.LEVEL_END; ++i)
            {
                LEVEL targetLevel = (LEVEL)i;
                SetupLevelFuncInfo(targetLevel);

            }
        }


        private void GameExitProcess()
        {
            ClearAllObjects();
            ThreadManager.Instance.Shutdown();


        }

        private void ClearAllObjects()
        {
            for (int i = 0; i < currentGameObjects.Length; ++i)
            {
                ClearObjects((LAYER)i);

            }
        }

        private void ClearObjects(LAYER targetLayer)
        {
            currentGameObjects[(int)targetLayer].RemoveAll(obj =>
            (null != obj)
            && (false == obj.isDestroyOnLoad));

        }

        private void Restart()
        {
            ClearAllObjects();
            levelSetupFunctions[nextLevel].Invoke();

            isRestart = false;
        }

        private void CheckChangeLevel()
        {
            if(currentLevel != nextLevel)
            {
                ClearAllObjects();
                levelSetupFunctions[nextLevel].Invoke();

                currentLevel = nextLevel;
            }
            if(true == isRestart)
            {
                Restart();
            }


        }

        public void CheckCurrentPressedKeyInfo()
        {
            if (true == Console.KeyAvailable)
            {
                ConsoleKey pressedKey = Console.ReadKey(true).Key;

                ClearInputBuffer();
                if (true == Enum.IsDefined(typeof(INPUT), (int)pressedKey))
                {
                    currentPressedInput = (INPUT)(int)pressedKey;
                    return;
                }
            }

            currentPressedInput = INPUT.INPUT_END;
        }
        private void ClearInputBuffer()
        {
            while (true == Console.KeyAvailable)
                Console.ReadKey(true);
        }


        private void RemoveReservedObject()
        {
            foreach (GameObject removeObject in reservedRemoveObjects)
            {
                currentGameObjects[(int)removeObject.layer].Remove(removeObject);
            }
            reservedRemoveObjects.Clear();
        }

        private void Update()
        {
            int iSize = (int)LAYER.LAYER_END;
            for (int i = 0; i < iSize; ++i)
            {
                foreach (GameObject gameObject in currentGameObjects[i])
                {
                    gameObject.Update();
                }
            }


        }

        private void CheckCollision()
        {
            foreach (LayerCollisionMatrix layers in setupLayerCollisionMatrix)
            {
                foreach (Actor actorObject1 in currentGameObjects[(int)layers.layer1])
                {
                    foreach (Actor actorObject2 in currentGameObjects[(int)layers.layer2])
                    {
                        if (true == IsCollision(actorObject1, actorObject2))
                        {
                            actorObject1.OnCollision(actorObject2 as Actor);
                            actorObject2.OnCollision(actorObject1 as Actor);
                        }
                    }
                }
            }
        }
        private bool IsCollision(Actor actorObject1, Actor actorObject2)
        {
            if (actorObject1.position == actorObject2.position) //Defualt Colliison
                return true;
            else
                return false;
        }


        private void Render()
        {
            RenderInfoSetupProcess();
            RenderFromRenderInfo();

        }


        private void RenderInfoSetupProcess()
        {
            foreach (Actor actorObject in currentRenderObjects)
            {
                SetupRenderState(actorObject.renderInfo);
            }
            currentRenderObjects.Clear();
        }
        private void SetupRenderState(RenderInfo renderInfo)
        {
            currentRenderState[renderInfo.renderPosition.y, renderInfo.renderPosition.x] = renderInfo.renderImage;
        }
        private void RenderFromRenderInfo()
        {
            Console.SetCursorPosition(0, 0);

            int MakeSpace(int index) => index * 2;

            for (int i = 0; i < ScreenHeight; i++)
            {
                for (int j = 0; j < ScreenWidth; j++)
                {
                    char currentImage = currentRenderState[i, j];
                    char previousImage = previousRenderState[i, j];

                    if (currentImage != previousImage)
                    {
                        Console.SetCursorPosition(MakeSpace(j), i);
                        Console.Write((NullChar == currentImage)
                            ? SpaceChar : currentImage);
                    }
                }
            }
            Array.Copy(currentRenderState, previousRenderState, currentRenderState.Length);
            Array.Clear(currentRenderState, 0, currentRenderState.Length);


        }

        public Ghost[] GetGhosts()
        {
            return currentGameObjects[(int)LAYER.ENEMY].OfType<Ghost>().ToArray();
        }
    }
}
