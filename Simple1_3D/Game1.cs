using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Simple1_3D
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //GameObject 클래스를 이용한 개체의 생성
        //지형 개체 생성
        GameObject terrain = new GameObject();

        //미사일 발사대 개체 생성
        GameObject missileLauncherBase = new GameObject();

        //미사일 포문 개체 생성
        GameObject missileLauncherHead = new GameObject();

        //카메라 위치
        Vector3 cameraPosition = new Vector3(0.0f, 60.0f, 160.0f);

        ////카메라가 바라보는 방향
        Vector3 cameraLookAt = new Vector3(0.0f, 50.0f, 0.0f);

        //카메라의 값들을 변화시킬 행렬값들
        Matrix cameraProjectionMatrix;
        Matrix cameraViewMatrix;

        //미사일의 최대 갯수
        const int numMissiles = 20;

        //각 미사일 개체의 배열
        GameObject[] missiles;

        //대포의 포문 위치를 구하기 위한 상수
        const float launcherHeadMuzzleOffset = 20.0f;

        //대포의 포문 속력을 구하기 위한 상수
        const float missilePower = 20.0f;

        //연속발사 제한을 위한 이전 상태의 키보드 입력 체크용 변수
        KeyboardState previousKeyboardState;

        //적기 위치의 무작위 생성을 위한 난수 생성
        Random r = new Random();

        //적기의 최대수는 3
        const int numEnemyShips = 13;

        //적기 배열
        GameObject[] enemyShips;


        //적기 위치의 최솟값을 가진 벡터
        Vector3 shipMinPosition = new Vector3(-2000.0f, 300.0f, -6000.0f);

        //적기 위치의 최댓값을 가진 벡터
        Vector3 shipMaxPosition = new Vector3(2000.0f, 800.0f, -4000.0f);

        //적기의 최소 속력
        const float shipMinVelocity = 5.0f;

        //적기의 최대 속력
        const float shipMaxVelocity = 10.0f;

        AudioEngine audioEngine;
        SoundBank soundBank;
        WaveBank waveBank;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // 텍스쳐를 그리기 위해 SpriteBatch를 생성한다.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            audioEngine = new AudioEngine("Content\\Audio\\TestAudio.xgs");
            waveBank = new WaveBank(audioEngine, "Content\\Audio\\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content\\Audio\\Sound Bank.xsb");

            //카메라가 바라보는 시점을 표현하는 행렬
            cameraViewMatrix = Matrix.CreateLookAt(
                cameraPosition,
                cameraLookAt,
                Vector3.Up);

            //카메라가 투영하는 시점을 표현하는 행렬
            cameraProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                graphics.GraphicsDevice.Viewport.AspectRatio,
                1.0f,
                10000.0f);

            //생성한 GameObject 클래스 개체의 속성값 초기화
            //지형 개체
            terrain.model = Content.Load<Model>(
                "Models\\terrain");

            //미사일 발사대 개체 이미지 적재
            missileLauncherBase.model = Content.Load<Model>(
                "Models\\launcher_base");
            //미사일 발사대의 크기를 20%로 줄인다.
            missileLauncherBase.scale = 0.2f;

            //미사일 포문 개체 이미지 적재
            missileLauncherHead.model = Content.Load<Model>(
             "Models\\launcher_head");
            //미사일 포문의 크기를 20%로 줄인다.
            missileLauncherHead.scale = 0.2f;
            //미사일 포문의 위치는 미사일 발사대 위치에서Y좌표만 20을 더한 값이다.
            missileLauncherHead.position =
                missileLauncherBase.position +
                new Vector3(0.0f, 20.0f, 0.0f);

            //배열 missles의 크기는 numMissiles 이고, 구성요소는 GameObject 이다.
            missiles = new GameObject[numMissiles];

            //배열의 크기만큼 for문을 통해 각 i번째 배열요소에 같은 미사일 이미지를 적재한다.
            //미사일 이미지의 크기는 원래 미사일 이미지의 3배로 잡는다.
            for (int i = 0; i < numMissiles; i++)
            {
                missiles[i] = new GameObject();
                missiles[i].model =
                    Content.Load<Model>("Models\\missile");
                missiles[i].scale = 3.0f;
            }

            //배열 enemyShips의 크기는 numEnemyShips 이고, 구성요소는 GameObject 이다.
            enemyShips = new GameObject[numEnemyShips];

            //배열의 크기만큼 for문을 통해 각 i번째 배열요소에 같은 적기 이미지를 적재한다.
            //적기 이미지의 크기는 원래 미사일 이미지의 10%이며, 
            //회전 벡터값은 (0, 180도, 0) 즉 Y좌표에만 값이 있음
            for (int i = 0; i < numEnemyShips; i++)
            {
                enemyShips[i] = new GameObject();
                enemyShips[i].model = Content.Load<Model>(
                    "Models\\enemy");
                enemyShips[i].scale = 0.1f;
                enemyShips[i].rotation = new Vector3(
                    0.0f, MathHelper.Pi, 0.0f);
            }
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //키보드 입력 상태 점검을 위한 KeyboardState 변수 선언
            KeyboardState keyboardState = Keyboard.GetState();

            //만약 왼쪽 방향키가 눌렸다면 미사일 발사대의 Y 좌표값 증가
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                missileLauncherHead.rotation.Y += 0.05f;
            }

            //만약 오른쪽 방향키가 눌렸다면 미사일 발사대의 Y 좌표값 감소
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                missileLauncherHead.rotation.Y -= 0.05f;
            }

            //만약 아래쪽 방향키가 눌렸다면 미사일 발사대의 X 좌표값 증가
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                missileLauncherHead.rotation.X += 0.05f;
            }

            //만약 위쪽 방향키가 눌렸다면 미사일 발사대의 X 좌표값 감소
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                missileLauncherHead.rotation.X -= 0.05f;
            }

            //미사일 발사대의 Y값 제한 
            missileLauncherHead.rotation.Y = MathHelper.Clamp(
                missileLauncherHead.rotation.Y,
                -MathHelper.PiOver4, MathHelper.PiOver4);

            //미사일 발사대의 X값 제한 
            missileLauncherHead.rotation.X = MathHelper.Clamp(
                missileLauncherHead.rotation.X,
                0, MathHelper.PiOver4);


            //스페이스바가 눌리면 발사 메서드 호출
            if (keyboardState.IsKeyDown(Keys.Space) &&
              previousKeyboardState.IsKeyUp(Keys.Space))
            {
                //미사일 발사를 실제로 구현하는 메서드 호출
                FireMissile();
            }

            //날아가는 미사일들의 업데이트를 위한 메서드 호출
            UpdateMissiles();

            //현재의 키입력 정보를 previousKeyboardState 변수에 업데이트
            previousKeyboardState = keyboardState;

            //적기의 위치변화 구현
            UpdateEnemyShips();

            base.Update(gameTime);
        }

        void FireMissile()
        {
            //missiles 배열에 존재하는 모든 missile 개체수 만큼 반복한다.
            foreach (GameObject missile in missiles)
            {
                //만약 미사일의 수명이 살아 있다면
                if (!missile.alive)
                {
                    soundBank.PlayCue("missilelaunch");

                    //미사일의 속도와 방향을 결정
                    missile.velocity = GetMissileMuzzleVelocity();

                    //미사일의 위치를 결정
                    missile.position = GetMissileMuzzlePosition();

                    //미사일의 회전정보를 결정
                    missile.rotation = missileLauncherHead.rotation;

                    //미사일의 수명을 살아있는 것으로 업데이트
                    missile.alive = true;

                    break;
                }
            }
        }

        Vector3 GetMissileMuzzleVelocity()
        {
            //회전 행렬
            Matrix rotationMatrix =
                Matrix.CreateFromYawPitchRoll(
                missileLauncherHead.rotation.Y,
                missileLauncherHead.rotation.X,
                0);

            //벡터의 진행방향과 회전 행렬을 통해 단위 벡터값을 구하고 
            //벡터 크기를 크게 만들기 위해 missilePower 상수를 곱함
            return Vector3.Normalize(
                Vector3.Transform(Vector3.Forward,
                rotationMatrix)) * missilePower;
        }

        Vector3 GetMissileMuzzlePosition()
        {
            //미사일의 위치를 결정
            //미사일의 위치는 미사일 포문의 위치에서 특정값을 더한 만큼
            return missileLauncherHead.position +
                (Vector3.Normalize(
                GetMissileMuzzleVelocity()) *
                launcherHeadMuzzleOffset);
        }

        void UpdateMissiles()
        {
            //missiles 배열에 있는 모든 missle 개체에 대하여
            foreach (GameObject missile in missiles)
            {
                //미사일의 생명 주기가 true라면 --> 즉 미사일이 살아 있다면
                if (missile.alive)
                {
                    //미사일의 위치를 미사일의 속도벡터 만큼 증가
                    missile.position += missile.velocity;

                    //만약 미사일의 Z좌표가 -6000보다 작아지면 -> 즉, 화면을 벗어나면
                    if (missile.position.Z < -6000.0f)
                    {
                        //미사일의 생명 주기를 false로 바꾼다. 
                        missile.alive = false;
                    }
                    else
                    {
                        //미사일이 화면을 벗어나지 않은 상태라면
                        //충돌 체크를 한다.
                        TestCollision(missile);
                    }
                }
            }
        }

        void UpdateEnemyShips()
        {
            //enemyShips 배열내에 있는 모든 ship 개체에 대해서.. 
            foreach (GameObject ship in enemyShips)
            {
                //만약 적기의 생명 주기가 true 라면..
                if (ship.alive)
                {
                    //적기의 위치는 적기의 속력벡터만큼 증가
                    ship.position += ship.velocity;

                    //만약 적기의 Z좌표가 500보다 크다면 생명은 끝
                    if (ship.position.Z > 500.0f)
                    {
                        ship.alive = false;
                    }
                }

                //만약 적기의 생명 주기가 false 라면..
                //즉, 생명 주기가 끝난 개체는 버리고 새로운 개체를 탄생시킨다.
                else
                {
                    //생명 주기를 다시 true로 변경하여 부활
                    ship.alive = true;

                    //적기의 탄생 위치 설정
                    //난수로 발생된 r을 사용하여 (X, Y, Z) 좌표값을 생성
                    //각 좌표값은 최솟값, 최댓값의 범위안에서 설정된다.
                    ship.position = new Vector3(
                        MathHelper.Lerp(
                        shipMinPosition.X,
                        shipMaxPosition.X,
                        (float)r.NextDouble()),

                        MathHelper.Lerp(
                        shipMinPosition.Y,
                        shipMaxPosition.Y,
                        (float)r.NextDouble()),

                        MathHelper.Lerp(
                        shipMinPosition.Z,
                        shipMaxPosition.Z,
                        (float)r.NextDouble()));

                    //적기의 방향과 속력 설정
                    //X, Y좌표의 값이 0 -->  Z좌표로만 이동한다.
                    //Z 좌표값 또한 난수 r을 사용하여 각 개체마다 다르다.
                    ship.velocity = new Vector3(
                        0.0f,
                        0.0f,
                        MathHelper.Lerp(shipMinVelocity,
                        shipMaxVelocity, (float)r.NextDouble()));
                }
            }
        }

        void TestCollision(GameObject missile)
        {
            //미사일 개체의 첫번째 메시를 포함하는 구와 똑같은 구를 선언한다.
            BoundingSphere missilesphere =
                missile.model.Meshes[0].BoundingSphere;

            //생성된 구의 원점은 미사일의 위치
            missilesphere.Center = missile.position;

            //생성된 구의 반지름은 미사일의 크기와 같게.
            missilesphere.Radius *= missile.scale;

            //모든 적기 개체에 대하여
            foreach (GameObject ship in enemyShips)
            {
                //만약 생명 주기가 true라면 --> 즉 살아있는 적기라면.
                if (ship.alive)
                {
                    //같은 방법으로 적기 개체의 구를 생성
                    BoundingSphere shipsphere =
                        ship.model.Meshes[0].BoundingSphere;
                    shipsphere.Center = ship.position;
                    shipsphere.Radius *= ship.scale;

                    //두 개의 구의 충돌 체크
                    if (shipsphere.Intersects(missilesphere))
                    {
                        //충돌하였다면 두 개의 개체 모두 생명주기는 false;
                        missile.alive = false;
                        ship.alive = false;

                        //충돌음 효과 발생
                        soundBank.PlayCue("explosion");
                        break;
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            //복잡한 구문은 따로 메서드를 만들어서 실행
            //DrawGameObject 메서드 호출

            //지형을 그린다.
            DrawGameObject(terrain);

            //미사일 발사대를 그린다.
            DrawGameObject(missileLauncherBase);

            //미사일 포문을 그린다.
            DrawGameObject(missileLauncherHead);

            //배열내 선언된 missile 개체에 대하여
            foreach (GameObject missile in missiles)
            {
                //미사일의 생명주기가 true인 개체는
                if (missile.alive)
                {
                    //DrawGameObject 메서드를 통해 그린다.
                    DrawGameObject(missile);
                }
            }

            //배열내 선언된 enemyship 개체에 대하여
            foreach (GameObject enemyship in enemyShips)
            {
                //적기의 생명주기가 true인 개체는
                if (enemyship.alive)
                {
                    //DrawGameObject 메서드를 통해 그린다.
                    DrawGameObject(enemyship);
                }
            }

            // TODO: Add your drawing code here
            base.Draw(gameTime);
        }


        void DrawGameObject(GameObject gameobject)
        {
            //Model내에 있는 모든 모델메시에 대해 루프를 돌며 실행
            foreach (ModelMesh mesh in gameobject.model.Meshes)
            {
                //각각의 메시에 있는 모든 이펙트를 설정
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //이펙트에 기본효과 설정
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    //이펙트의 기본적인 3가지 매트릭스의 값 설정
                    //World, Projection, View
                    effect.World =
                        Matrix.CreateFromYawPitchRoll(
                        gameobject.rotation.Y,
                        gameobject.rotation.X,
                        gameobject.rotation.Z) *

                        Matrix.CreateScale(gameobject.scale) *

                        Matrix.CreateTranslation(gameobject.position);

                    effect.Projection = cameraProjectionMatrix;
                    effect.View = cameraViewMatrix;
                }

                //설정이 완료된 메시를 그린다
                mesh.Draw();
            }
        }
    }
}
