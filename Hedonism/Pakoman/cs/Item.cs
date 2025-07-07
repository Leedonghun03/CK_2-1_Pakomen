using Pakoman.cs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hedonism
{
    public abstract class Item : Actor
    {
        public Item(LAYER layer) : base(layer) { }
        public abstract void setPos(Vector2 pos);
    }

    public class Coin : Item
    {
        private int score;
        private bool isDestroy;
        public Coin(LAYER layer, Vector2 spawnVec) : base(layer)
        {
            score = 10;
            isDestroy = false;
            this.SetLayer(LAYER.ITEM);
            renderInfo.renderImage = '·';
            position = spawnVec;
        }
        public override void setPos(Vector2 pos) { }
        public override void OnCollision(Actor other)
        {
            // 플레이어가 해당 아이템을 먹었을 때
            if (other.layer == LAYER.PLAYER)
            {
                var scoreManage = ScoreManager.Instance; // 싱글톤 변수인 scoreManager를 받아옴
                scoreManage.AddScoreByCoin(score); //점수 추가
                isDestroy = true;
                GameManager.Instance.Destroy(this); // 해당 오브젝트 삭제
        }
        }
        // Item은 이동하지 않음
        public override void Move(Vector2 direction) { }

        public override void Update()
        {
            if (!isDestroy)
            {
                SynchronizeRenderPosition();
                RegisterRenderer();
            }
            else
            {
                var scoreManage = ScoreManager.Instance;
                scoreManage.AddScoreByCoin(score);
            }
        }

    }
    public class Pill : Item
    {
        private bool isDestroy;
        public Pill(LAYER layer, Vector2 spawnVec) : base(layer)
        {
            isDestroy = false;
            this.SetLayer(LAYER.ITEM);
            renderInfo.renderImage = 'θ';
            position = spawnVec;
        }
        public override void setPos(Vector2 pos) { }
        public override void OnCollision(Actor other)
        {
            // 플레이어가 해당 아이템을 먹었을 때
            if (other.layer == LAYER.PLAYER)
            {
                (other as Player).TurnOnPillBurst();

                // 플레이어가 유령을 잡아먹는 부분 활성화
                foreach (var ghost in GameManager.Instance.GetGhosts())
                    ghost.ChangeState(GhostState.Frightened);

                isDestroy = true;
                GameManager.Instance.Destroy(this);
            }
        }
        // Item은 이동하지 않음
        public override void Move(Vector2 direction) {        }

        public override void Update()
        {
            if (!isDestroy)
            {
                SynchronizeRenderPosition();
                RegisterRenderer();
            }
        }

    }
    public class Apple : Item
    {
        private bool isDestroy;
        private int score = 200;
        public Apple(LAYER layer, Vector2 spawnVec) : base(layer)
        {
            isDestroy = false;
            this.SetLayer(LAYER.ITEM);
            //renderInfo.renderImage = 'ё';
            renderInfo.renderImage = 'ð';
            this.position = spawnVec;
        }
        public override void setPos(Vector2 pos) { }
        public override void OnCollision(Actor other)
        {
            // 플레이어가 해당 아이템을 먹었을 때
            if (other.layer == LAYER.PLAYER)
            {
                var scoreManage = ScoreManager.Instance; // 싱글톤 변수인 scoreManager를 받아옴
                scoreManage.AddScoreByCoin(score); //점수 추가
                isDestroy = true;
                GameManager.Instance.Destroy(this);
            }
        }
        // Item은 이동하지 않음
        public override void Move(Vector2 direction) {       }

        public override void Update()
        {
            if (!isDestroy)
            {
                SynchronizeRenderPosition();
                RegisterRenderer();
            }
        }
    }
}
