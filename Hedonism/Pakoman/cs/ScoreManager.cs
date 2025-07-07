using Hedonism;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pakoman.cs
{
    internal class ScoreManager : Singleton<ScoreManager>
    {
        private int score;
        private int fieldCoinAmount;
        private int collectedCoin;
        Text text;

        LEVEL stageNum;

        public ScoreManager()
        {
            text = new Text(LAYER.UI, new Vector2(30, 11), "");
            ResetScore();
            stageNum = LEVEL.Level_1;
        }

        public void AddScoreByCoin(int addValue)
        {
            score += addValue;
            text.SetString("Score : " + score.ToString());
            AddCurrAmount(); // 수집코인 갯수 증가
        }
        public void AddScore(int addValue, int eatCombo) // 
        {
            if (eatCombo > 0)
            {
                addValue = addValue * (eatCombo + 1);
            }
            score += addValue;
            text.SetString("Score : " + score.ToString());
        }
        public void ResetScore()
        {
            score = 0;
            collectedCoin = 0;
        }
        public void ResetCollectCoin()
        {
            collectedCoin = 0;
        }
        public int GetScore()
        {
            return score;
        }
        public void SetCoinAmount(int amount)
        {
            fieldCoinAmount = amount;
        }
        public void AddCurrAmount()
        {
            collectedCoin++;
            Text txt = new Text(LAYER.UI, new Vector2(30, 13), ("Current : "));
            txt.SetString("Current : " + collectedCoin.ToString() + (" / ") + fieldCoinAmount.ToString());
            if (collectedCoin == fieldCoinAmount)
            {
                Text clearText = new Text(LAYER.UI, new Vector2(30, 9), ("Stage " + stageNum.ToString() + " Cleared!"));
                stageNum++;
                GameManager.Instance.LevelChange(stageNum);
            }
        }
    }
}