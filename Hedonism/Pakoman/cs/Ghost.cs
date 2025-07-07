using Hedonism;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Hedonism
{
    // A*에 사용할 Node 클래스
    public class Node
    {
        public Vector2 position; // 타일 좌표
        public Node parent;      // 이전 노드
        public int g;            // 현재까지 실제 누적 비용
        public int h;            // 목표까지 예상 비용
        public int f => g + h;   // 총 비용

        public Node() { }
        public Node(Vector2 position, Node parent, int g, int h)
        {
            this.position = position;
            this.parent = parent;
            this.g = g;
            this.h = h;
        }
    }

    // FSM에 사용할 enum
    // 1.일반 탐색 2.팩맨 추적 3.팩맨이 알약 섭취
    public enum GhostState { Wander, Chase, Frightened, ReturnHome }

    public class Ghost : Actor
    {
        // 유령 관련 변수
        private Vector2 spawnPoint;

        public GhostState currentState { get; private set; }
        private double frightenedTimer = 0f;
        private int detectRadius = 10;

        //private double GhostMoveIntervalTime = 0.35f;
        public static double ghostMoveIntervalTime = 0.35f;
        public static void SetGhostMoveIntervalTime(double intervalTime) => ghostMoveIntervalTime = intervalTime;


        private double moveTime = 0.0f;     // 이동 관련 타임
        private static readonly Random rng = new Random();

        public Wall[,] mapWalls;

        public Actor playerActor;
        private Vector2 goalPos;

        // === a* 알고리즘 관련 변수 ===
        private List<Node> openList = new List<Node>();         // 확장 후보
        private List<Node> closedList = new List<Node>();       // 이미 평가 완료
        private List<Node> pathList = new List<Node>();         // 최종 경로
        private int pathIndex = 0;                              // 경로 인덱스

        private readonly Vector2[] direction =                  // 탐색 노드 방향
        {
            new Vector2(1, 0), new Vector2(-1, 0),
            new Vector2(0, 1), new Vector2(0, -1)
        };
        // =============================

        public Ghost(LAYER layer, Vector2 spawnPosition) : base(layer)
        {
            renderInfo.renderImage = 'Д';

            currentState = GhostState.Wander;
            position = spawnPosition;
            spawnPoint = new Vector2(spawnPosition.x, spawnPosition.y);

            SynchronizeRenderPosition();
            GameManager.Instance.RegisterGameObject(LAYER.ENEMY, this);
        }

        public override void Move(Vector2 dir)
        {
            position.x += dir.x;
            position.y += dir.y;
        }

        public override void OnCollision(Actor other)
        {
            if (other == playerActor)
            {
                if (currentState == GhostState.Frightened)
                {
                    ChangeState(GhostState.ReturnHome);
                    pathList.Clear();
                }
                //else
                // 플레이어 죽는 함수
            }
        }

        public override void Update()
        {
            bool isDestroy = false;

            if (isDestroy == false)
            {
                SynchronizeRenderPosition();
                RegisterRenderer();

                // 상태 갱신
                UpdateState();

                // 유령 상태에 맞춰 목표 재설정
                ChangeTerget();

                // 경로 재계산
                if (pathList.Count == 0 || pathIndex >= pathList.Count)
                {
                    pathList = PathFind(position, goalPos);

                    // 길을 못 찾은 경우
                    if (pathList.Count == 0)
                        return;

                    pathIndex = 0;

                    //isCalculating = true;
                    //ThreadManager.Instance.RunTask(this);
                }

                moveTime += GameManager.Instance.GetDeltaTime();
                if (moveTime < ghostMoveIntervalTime)
                    return;

                moveTime = 0.0f;

                Node nextNode = pathList[pathIndex];
                Vector2 dir = new Vector2(nextNode.position.x - position.x, nextNode.position.y - position.y);
                Move(dir);
                pathIndex++;
            }
        }

        // 상태 전환 함수
        public void ChangeState(GhostState newState)
        {
            currentState = newState;
            pathList.Clear();
            pathIndex = 0;

            if(newState == GhostState.Frightened)
            {
                frightenedTimer = 5.0f;
            }
        }

        // 유령 상태 갱신하는 함수
        private void UpdateState()
        {
            float dist = Vector2.Distance(position, playerActor.position);

            switch (currentState)
            {
                case GhostState.Wander:
                    renderInfo.renderImage = 'Д';
                    
                    if (dist <= detectRadius)
                        ChangeState(GhostState.Chase);
                    break;

                case GhostState.Chase:
                    if (dist > detectRadius)
                        ChangeState(GhostState.Wander);
                    break;

                case GhostState.Frightened:
                    frightenedTimer -= GameManager.Instance.GetDeltaTime();
                    renderInfo.renderImage = 'g';
                    
                    if (frightenedTimer < 0)
                    {
                        renderInfo.renderImage = 'Д';
                        ChangeState(GhostState.Wander);
                    }
                    break;

                case GhostState.ReturnHome:
                    renderInfo.renderImage = 'a';
                    
                    if (position.x == spawnPoint.x && position.y == spawnPoint.y)
                    {
                        ChangeState(GhostState.Wander);
                    }
                    break;
            }
        }

        // 유령 상태에 맞춰 목표 재설정하는 함수
        private void ChangeTerget()
        {
            switch (currentState)
            {
                // 돌아다니는 상태 (랜덤 포지션)
                case GhostState.Wander:
                    goalPos = GetRandomNaviPos();
                    break;

                // 팩맨 추적 상태 (Player로 지정)
                case GhostState.Chase:
                    goalPos = playerActor.position;
                    break;

                // 팩맨 알약 먹은 상태 (도망가게)
                case GhostState.Frightened:
                    goalPos = SelectEscapePos(playerActor.position);
                    break;

                // 플레이어에게 잡혔을 때 (유령방으로 가게)
                case GhostState.ReturnHome:
                    goalPos = spawnPoint;
                    break;
            }
        }

        // 랜덤 위치 반환 함수
        private Vector2 GetRandomNaviPos()
        {
            int rng_x = rng.Next(0, 25);
            int rng_y = rng.Next(0, 25);

            return new Vector2(rng_x, rng_y);
        }

        private Vector2 SelectEscapePos(Vector2 playerPos)
        {
            const int escapeScanRadius = 4;
            int maxDistance = -1;
            Vector2 escapeTarget = position;

            foreach(Vector2 dir in direction)
            {
                Vector2 p = new Vector2(position.x + dir.x * escapeScanRadius, position.y + dir.y * escapeScanRadius);

                if (IsInBound(p) || IsWall(p))
                    continue;

                int distanceFromPlayer = Vector2.Distance(p, playerPos);
                if(distanceFromPlayer > maxDistance)
                {
                    maxDistance = distanceFromPlayer;
                    escapeTarget = p;
                }
            }

            return escapeTarget;
        }

        // 맵 경계 검사 함수
        private bool IsInBound(Vector2 pos)
        {
            int mapYSize = mapWalls.GetLength(0);
            int mapXSize = mapWalls.GetLength(1);

            return (pos.x < 0 || pos.y < 0 || pos.x >= mapXSize || pos.y >= mapYSize);
        }

        // 벽 충돌 검사 함수
        private bool IsWall(Vector2 pos)
        {
            return mapWalls[pos.y, pos.x].layer == LAYER.MAP;
        }

        // === a* 구현부 ===
        // 이웃 노드를 Open 리스트에 넣는 함수
        private void AddOpenList(Vector2 neighborPos, Node parent, Vector2 targetPos)
        {
            // 맵 경계 검사
            if (IsInBound(neighborPos))
                return;

            // 벽 충돌 검사
            if (IsWall(neighborPos))
                return;

            // Closed 리스트에 이미 존재하면 무시
            foreach (Node closed in closedList)
            {
                if (closed.position == neighborPos)
                    return;
            }

            // g, h 계산
            int g = parent.g + 10;
            int h = (Math.Abs(neighborPos.x - targetPos.x) + Math.Abs(neighborPos.y - targetPos.y)) * 10;

            // Open 리스트에 같은 좌표가 있는지 검색
            Node existing = null;
            foreach (Node open in openList)
            {
                if (open.position == neighborPos)
                {
                    existing = open;
                    break;
                }
            }

            // 새 노드 추가 or 더 짧은 경로로 업데이트
            if (existing == null)
            {
                openList.Add(new Node(neighborPos, parent, g, h));
            }
            else if (g < existing.g)
            {
                existing.g = g;
                existing.parent = parent;
            }
        }

        // 경로 탐색 함수
        private List<Node> PathFind(Vector2 myPos, Vector2 targetPos)
        {
            // 기존 데이터 초기화
            openList.Clear();
            closedList.Clear();
            pathList.Clear();

            // 탐색 시작 노드 설정
            int h = (Math.Abs(myPos.x - targetPos.x) + Math.Abs(myPos.y - targetPos.y)) * 10;
            Node startNode = new Node(myPos, null, 0, h);
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                // f 최솟값 노드 선택
                Node currentNode = openList[0];

                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].f < currentNode.f
                        || (openList[i].f == currentNode.f && openList[i].h < currentNode.h))
                    {
                        currentNode = openList[i];
                    }
                }

                // currentNode가 targetPos의 위치면 탐색 종료 후 역추적 후 반환
                if (currentNode.position == targetPos)
                {
                    pathList.Clear();

                    Node FinishCurNode = currentNode;
                    while (FinishCurNode != null)
                    {
                        pathList.Add(FinishCurNode);
                        FinishCurNode = FinishCurNode.parent;
                    }

                    pathList.Reverse();
                    pathList.RemoveAt(0);
                    return pathList;
                }

                // currentNode를 openNodeList에서 제거하고 closedNodeList에 추가
                openList.Remove(currentNode);
                closedList.Add(currentNode);

                // currentNode 주변(상하좌우) 이웃 좌표마다 AddOpenList 호출
                foreach (Vector2 dir in direction)
                {
                    Vector2 neighborPos = new Vector2(currentNode.position.x + dir.x, currentNode.position.y + dir.y);
                    AddOpenList(neighborPos, currentNode, targetPos);
                }
            }

            // 경로가 없음
            return new List<Node>();
        }
    }
}