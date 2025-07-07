using Hedonism;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Pakoman.cs
{
    // 한 글자 단위를 담당하는 클래스
    internal class Word : Actor
    {
        // 생성자: 글자 UI와 위치를 설정
        public Word(LAYER layer, char ui, Vector2 value) : base(layer)
        {
            renderInfo.renderImage = ui;  // 화면에 표시할 글자 설정
            position = value;             // 위치 설정
            SynchronizeRenderPosition();  // 렌더링 좌표와 동기화
        }

        // Word는 이동하지 않음
        public override void Move(Vector2 direction) { }

        // 충돌시 아무 행동 없음
        public override void OnCollision(Actor other) { }

        // 매 프레임마다 호출되는 업데이트 (렌더링 등록)
        public override void Update()
        {
            SynchronizeRenderPosition();
            RegisterRenderer();
        }
    }

    // 문자열 전체를 담당하는 클래스
    class Text
    {
        LAYER layer;        // 렌더링 레이어
        Vector2 position;   // 문자열의 시작 위치
        string value;       // 현재 문자열 값

        List<Word> texts = new List<Word>(); // 문자열을 구성하는 Word 리스트

        // 생성자: 문자열 생성
        public Text(LAYER layer, Vector2 pos, string targetValue)
        {
            this.layer = layer;
            position = pos;
            value = targetValue;

            // 문자열을 글자 단위로 분리하여 Word 객체로 생성
            for (int i = 0; i < value.Length; i++)
            {
                Word word = new Word(layer, value[i], new Vector2(position.x + i, position.y));
                GameManager.Instance.RegisterGameObject(layer, word);
                texts.Add(word);
            }
        }

        // 문자열을 새 값으로 교체하는 함수
        public void SetString(string newValue)
        {
            // 기존 Word 객체 파괴
            for (int i = 0; i < texts.Count; i++)
            {
                GameManager.Instance.Destroy(texts[i]);
            }

            // 새 문자열로 초기화
            value = newValue;
            texts = new List<Word>();

            // 새 문자열을 다시 Word 객체로 생성
            for (int i = 0; i < value.Length; i++)
            {
                Word word = new Word(layer, value[i], new Vector2(position.x + i, position.y));
                GameManager.Instance.RegisterGameObject(layer, word);
                texts.Add(word);
            }
        }
    }
    // UI 클래스 (예시 출력 담당)
    class UI : Actor
    {
        Text text;
        // 생성자: 예시로 텍스트 생성 및 변경
        public UI(LAYER layer) : base(layer)
        {
            string HP = "";
            for (int i = 0; i < Player.GetCurrentPlayerHP(); i++) HP += "● ";

            text = new Text(LAYER.UI, new Vector2(35, 15), HP);
        }

        // Word는 이동하지 않음
        public override void Move(Vector2 direction) { }

        // 충돌시 아무 행동 없음
        public override void OnCollision(Actor other) { }

        // 매 프레임마다 호출되는 업데이트 (렌더링 등록)
        public override void Update()
        {
            SynchronizeRenderPosition();
            RegisterRenderer();
        }
    }
}
