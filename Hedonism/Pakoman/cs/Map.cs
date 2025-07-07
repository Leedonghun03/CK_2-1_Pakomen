using Hedonism;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hedonism
{
    // 벽 객체 클래스
    public class Wall : Actor
    {
        public Wall(LAYER layer) : base(layer)
        {
            renderInfo.renderImage = '■'; // 벽은 ■로 렌더링

            position = new Vector2(0, 0); // 초기 위치 (0,0)
            SynchronizeRenderPosition();
        }

        public override void Move(Vector2 direction) { } // 벽은 이동 불가
        
        public override void OnCollision(Actor other) { } // 충돌 없음

        public override void Update()
        {
            SynchronizeRenderPosition();
            RegisterRenderer(); // 매 프레임 렌더 등록
        }
    }

    // DFS를 위한 정보 클래스 (위치 + 랜덤 값)
    class DfsInfo
    {
        public Vector2 position;
        public int value;

        public DfsInfo(Vector2 position, int value)
        {
            this.position = position;
            this.value = value;
        }
    };

    // 맵 생성 클래스
    public class CreateMap : Singleton<CreateMap>
    {
        public int size = 29; // 미로 크기
        public Wall[,] walls;
        public List<Vector2> coins = new List<Vector2>(); // 코인 위치 리스트

        int center;
        ItemSpawn spawner;
        Random rand = new Random();

        // 전체 미로 생성 함수
        public void BuildMaze()
        {
            spawner = new ItemSpawn();
            walls = RecursiveBacktracking(size, size);

            // 벽 및 길 렌더링 및 등록
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (walls[i, j].layer == LAYER.MAP || walls[i, j].layer == LAYER.PORTAL)
                    {
                        walls[i, j].position.x = j;
                        walls[i, j].position.y = i;
                        GameManager.Instance.RegisterGameObject(walls[i, j].layer, walls[i, j]);
                    }
                    else
                    {
                        coins.Add(new Vector2(j, i)); // 코인 스폰 후보에 추가
                    }
                }
            }
            spawner.SetSpawn(coins);
            spawner.PillSpawn(walls, size, 5);
            spawner.AppleSpawn(5);
            spawner.CoinSpawn();
            spawner.Spawn();
        }

        // DFS 기반 재귀 백트래킹으로 미로 생성
        Wall[,] RecursiveBacktracking(int maxX, int maxY)
        {
            center = size / 2;

            Wall[,] walls = new Wall[maxX, maxY];

            // 전체 맵을 벽으로 초기화
            for (int i = 0; i < maxX; i++)
            {
                for (int j = 0; j < maxY; j++)
                {
                    walls[i, j] = new Wall(LAYER.MAP);  // 처음엔 다 벽으로
                }
            }

            Vector2[] directions = new Vector2[]
            {
        new Vector2(0, 1),   // 위
        new Vector2(1, 0),   // 오른쪽
        new Vector2(-1, 0),  // 왼쪽
        new Vector2(0, -1)   // 아래
            };

            Stack<Vector2> stack = new Stack<Vector2>();

            // 시작점: 스폰룸 옆
            Vector2 startPos = new Vector2(-1 + size / 2, -1 + size / 2);
            stack.Push(startPos);
            walls[maxX / 2, 1].SetLayer(LAYER.LAYER_END); // 첫 길 생성

            while (stack.Count > 0)
            {
                Vector2 currentPos = stack.Pop();
                directions = Shuffle(directions); // 방향 랜덤 섞기

                for (int dirIndex = 0; dirIndex < 4; dirIndex++)
                {
                    Vector2 nextPos = new Vector2(
                        currentPos.x + directions[dirIndex].x * 2,
                        currentPos.y + directions[dirIndex].y * 2
                    );

                    if (IsInBound(nextPos) && walls[nextPos.x, nextPos.y].layer == LAYER.MAP)
                    {
                        if (!IsInSpawnRoom(nextPos))
                        {
                            // 벽 제거하고 길 생성
                            walls[nextPos.x, nextPos.y].SetLayer(LAYER.LAYER_END);
                            walls[currentPos.x + directions[dirIndex].x, currentPos.y + directions[dirIndex].y].SetLayer(LAYER.LAYER_END);

                            stack.Push(currentPos);
                            stack.Push(nextPos);
                            break;
                        }
                    }
                }
            }

            DeadZone(ref walls); // 막힌 구역 연결
            InitBaseMap(ref walls); // 스폰룸 및 입구 출구 생성

            return walls;
        }

        // 사망 구역 제거
        void DeadZone(ref Wall[,] walls)
        {
            Queue<Vector2> queue = new Queue<Vector2>();
            bool[,] visited = new bool[size, size];
            queue.Enqueue(new Vector2(center, 0));

            Vector2[] directions = new Vector2[]
            {
                new Vector2(0, 1), new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, -1)
            };

            while (queue.Count > 0)
            {
                Vector2 currentPos = queue.Dequeue();
                visited[currentPos.x, currentPos.y] = true;

                int availableDir = 0;

                for (int i = 0; i < directions.Length; i++)
                {
                    Vector2 nextPos = new Vector2(currentPos.x + directions[i].x, currentPos.y + directions[i].y);
                    if (IsInBound(nextPos) && !visited[nextPos.x, nextPos.y] && walls[nextPos.x, nextPos.y].layer == LAYER.LAYER_END)
                    {
                        queue.Enqueue(nextPos);
                        availableDir++;
                    }
                }

                // 막힌 구역 발견 시 추가로 길 생성
                if (availableDir == 0)
                {
                    for (int i = 0; i < directions.Length; i++)
                    {
                        Vector2 nextPos = new Vector2(currentPos.x + directions[i].x * 2, currentPos.y + directions[i].y * 2);
                        if (IsInBound(nextPos) && walls[nextPos.x, nextPos.y].layer == LAYER.LAYER_END)
                        {
                            walls[currentPos.x + directions[i].x, currentPos.y + directions[i].y].SetLayer(LAYER.LAYER_END);
                        }
                    }
                }
            }
        }

        // 스폰룸 및 입출구 초기화
        void InitBaseMap(ref Wall[,] walls)
        {
            // 스폰룸 외곽 벽 생성
            for (int i = center - 2; i <= center + 2; i++)
            {
                for (int j = center - 2; j <= center + 2; j++)
                {
                    walls[i, j].SetLayer(LAYER.MAP);
                }
            }

            // 스폰룸 내부 길 생성
            for (int i = center - 1; i <= center + 1; i++)
            {
                for (int j = center - 1; j <= center + 1; j++)
                {
                    walls[i, j].SetLayer(LAYER.LAYER_END);
                }
            }

            // 스폰룸 입구 4방향 개방
            walls[center - 2, center].SetLayer(LAYER.LAYER_END);
            walls[center + 2, center].SetLayer(LAYER.LAYER_END);
            walls[center, center - 2].SetLayer(LAYER.LAYER_END);
            walls[center, center + 2].SetLayer(LAYER.LAYER_END);

            // 미로 입구/출구 개방
            walls[center, 0].SetLayer(LAYER.PORTAL);
            walls[center, 0].renderInfo.renderImage = '◎';
            walls[center, size - 2].SetLayer(LAYER.LAYER_END);
            walls[center, size - 1].SetLayer(LAYER.PORTAL);
            walls[center, size - 1].renderInfo.renderImage = '◎';
        }

        // 경계 검사
        public bool IsInBound(Vector2 pos)
        {
            return (0 < pos.x && 0 < pos.y && pos.x < size - 1 && pos.y < size - 1);
        }

        // 스폰룸 내부 검사
        public bool IsInSpawnRoom(Vector2 pos)
        {
            return (pos.x >= center - 2 && pos.x <= center + 2 && pos.y >= center - 2 && pos.y <= center + 2);
        }

        // 방향 배열 섞기
        Vector2[] Shuffle(Vector2[] list)
        {
            for (int i = list.Length - 1; i > 0; i--)
            {
                int j = rand.Next(0, i + 1);
                Vector2 temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
            return list;
        }
    }
}
