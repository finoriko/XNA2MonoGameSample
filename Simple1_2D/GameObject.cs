using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simple1_2D
{
    class GameObject
    {
        public Texture2D sprite; // 그려줄 이미지 파일 변수
        public Vector2 position; // 캐릭터의 위치
        public float rotation;     // 캐릭터의 회전이동 변수
        public Vector2 center;   // 캐릭터의 회전 중심이 되는 중심점
        public Vector2 velocity;  // 캐릭터의 속력
        public bool alive;         // 캐릭터의 생명주기 - 살거나 죽거나

        public GameObject(Texture2D loadedTexture)
        {
            rotation = 0.0f;
            position = Vector2.Zero;
            sprite = loadedTexture;
            center = new Vector2(sprite.Width / 2, sprite.Height / 2);
            velocity = Vector2.Zero;
            alive = false;
        }
    }
}