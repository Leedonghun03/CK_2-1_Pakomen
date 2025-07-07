using Hedonism;
using Pakoman.cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

enum ITEMVALUE
{
    Coin,
    Pill,
    Apple
}
namespace Hedonism
{
    class spawnInfo
    {
        public Vector2 position;
        public ITEMVALUE spawnValue;
        public spawnInfo(Vector2 position, ITEMVALUE value)
        {
            this.position = position;
            this.spawnValue = value;
        }
        public spawnInfo(Vector2 position)
        {
            this.position = position;
            this.spawnValue = ITEMVALUE.Coin;
        }
    }
    internal class ItemSpawn : Singleton<ItemSpawn>
    {
        int maxCoinNum; // 맵의 전체 코인 갯수
        int appleAmount; // 사과 갯수
        List<spawnInfo> avaliablePos;

        public ItemSpawn(){
            avaliablePos = new List<spawnInfo>();
        }
        ScoreManager scoreManager { get; set; }

        public void SetSpawn(List<Vector2> avaliable)
        {
            for (int i = 0; i < avaliable.Count; i++)
{
                avaliablePos.Add(new spawnInfo(avaliable[i]));
            }
        }
        public ItemSpawn(int spawnValue, Vector2 spawnPosition) { }

        public void Spawn()
        {
            for (int i=0; i< avaliablePos.Count; i++)
    {
                if (avaliablePos[i].spawnValue == ITEMVALUE.Coin)
        {
                    Coin tempCoin = new Coin(LAYER.ITEM, avaliablePos[i].position);
                    GameManager.Instance.RegisterGameObject(LAYER.ITEM, tempCoin);
                }
                else if (avaliablePos[i].spawnValue == ITEMVALUE.Pill)
                {
                    Pill tempPill = new Pill(LAYER.ITEM, avaliablePos[i].position);
                    GameManager.Instance.RegisterGameObject(LAYER.ITEM, tempPill);
                }
                else if (avaliablePos[i].spawnValue == ITEMVALUE.Apple)
            {
                    Apple tempApple = new Apple(LAYER.ITEM, avaliablePos[i].position);
                    GameManager.Instance.RegisterGameObject(LAYER.ITEM, tempApple);
                }
            }
        }

        public void CoinSpawn()
        {
            maxCoinNum = 0;
            for (int i = 0; i < avaliablePos.Count; i++)
            {
                //coins.Add(new Coin(LAYER.ITEM, spawnPoints[i]));
                //GameManager.Instance.RegisterGameObject(coins[i].layer, coins[i]);
                if (avaliablePos[i].spawnValue == ITEMVALUE.Coin)
                {
                    maxCoinNum++;
                }
            }
            var scoreManage = ScoreManager.Instance;
            scoreManage.SetCoinAmount(maxCoinNum+appleAmount);
            scoreManage.ResetCollectCoin();
        }

        public void PillSpawn(Wall[,] mazeInfo, int size, int spawnAmount)
        {
            int center = size / 2;
            List<int> oneWayRoute = new List<int>();

            for (int i=0; i< avaliablePos.Count; i++)
            {
                if ((0 < avaliablePos[i].position.x && 0 < avaliablePos[i].position.y && avaliablePos[i].position.x < size - 1 && avaliablePos[i].position.y < size - 1)
                   && !((avaliablePos[i].position.x >= center - 2 && avaliablePos[i].position.x <= center + 2 && avaliablePos[i].position.y >= center - 2 && avaliablePos[i].position.y <= center + 2)))
                {
                    {
                        oneWayRoute.Add(i); // 조건을 만족하면 Pill 스폰 후보에 추가
                        /*if ((mazeInfo[avaliablePos[i].position.x + 1, avaliablePos[i].position.y].renderInfo.renderImage == '■' && mazeInfo[avaliablePos[i].position.x - 1, avaliablePos[i].position.y].renderInfo.renderImage == '■') // 양옆의 벽 검사
                            || (mazeInfo[avaliablePos[i].position.x, avaliablePos[i].position.y + 1].renderInfo.renderImage == '■' && mazeInfo[avaliablePos[i].position.x, avaliablePos[i].position.y - 1].renderInfo.renderImage == '■')) // 위아래 벽 검사
                        {
                            oneWayRoute.Add(i); // 두 조건중 하나라도 참이면 Pill 스폰 후보에 추가
                        }*/ //넌 나가잇!

                    }
                }
            }
            Random random = new Random();
            Vector2[] spawnedPillPos = new Vector2[spawnAmount];

            for (int i = 0; i < spawnAmount; i++)
            {
                int spawnIndex = random.Next(oneWayRoute.Count);
                spawnedPillPos[i] = avaliablePos[spawnIndex].position;

                if (i != 0) {
                    int incount = 0;
                    while (InNear(spawnedPillPos, avaliablePos[spawnIndex].position, i, size / spawnAmount) && incount < 10)
                    {
                        spawnIndex = random.Next(oneWayRoute.Count);
                        spawnedPillPos[i] = avaliablePos[spawnIndex].position;
                        incount++;
                    }
                }
                avaliablePos[spawnIndex].spawnValue = ITEMVALUE.Pill;
                //Pill pill = new Pill(LAYER.ITEM, new Vector2(oneWayRoute[spawnIndex].x, oneWayRoute[spawnIndex].y));
                //GameManager.Instance.RegisterGameObject(pill.layer, pill);
            }
        }
        public void AppleSpawn(int spawnAmount)
        {
            appleAmount = spawnAmount;
            Random random = new Random();
            for(int num = 0; num<spawnAmount; num++)
            {
                int spawn = random.Next(avaliablePos.Count);
                while (avaliablePos[spawn].spawnValue == ITEMVALUE.Pill)
                {
                    spawn = random.Next(avaliablePos.Count);
                }
                avaliablePos[spawn].spawnValue = ITEMVALUE.Apple;
            }
        }

        private int CalcVectorX(Vector2 pos1, Vector2 pos2)
        {
            return (pos1.x - pos2.x);
        }
        private int CalcVectorY(Vector2 pos1, Vector2 pos2)
        {
            return (pos1.y - pos2.y);
        }
        private bool InNear(Vector2[] spawned, Vector2 tempVector, int currIdx, int minSize)
        {
            bool isNeighbor = false;
            for (int i=0; i<currIdx; i++)
            {
                if ((-minSize < CalcVectorX(tempVector, spawned[i])) && CalcVectorX(tempVector, spawned[i]) < minSize)
                {
                    isNeighbor = true;
                    return isNeighbor;
                }
                else
                {
                    isNeighbor = false;
                }
                if ((-minSize < CalcVectorY(tempVector, spawned[i])) && CalcVectorY(tempVector, spawned[i]) < minSize)
                {
                    isNeighbor = true;
                    return isNeighbor;
                }
                else
                {
                    isNeighbor = false;
                }
            }
            return isNeighbor;
        }
    }
}
