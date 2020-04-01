using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simple1_3D
{
    class GameObject
    {
        // 어떤 3D 모델을 사용할지 이미지 설정
        public Model model = null;

        //캐릭터를 표시할 위치좌표
        public Vector3 position = Vector3.Zero;

        //캐릭터의 회전 좌표
        public Vector3 rotation = Vector3.Zero;

        //캐릭터의 크기 변수
        public float scale = 1.0f;

        //캐릭터의 속력 및 방향 설정용 벡터
        public Vector3 velocity = Vector3.Zero;

        //캐릭터의 생명 주기
        public bool alive = false;
    }
}