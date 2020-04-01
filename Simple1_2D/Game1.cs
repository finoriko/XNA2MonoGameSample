using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Simple1_2D
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Texture2D backgroundTexture;
        Rectangle viewportRect;
        SpriteBatch spriteBatch;

        //GameObject 개체변수 추가
        GameObject cannon;

        //대포 탄환의 최대갯수
        const int maxCannonBalls = 3;

        //대포 탄환 배열선언
        GameObject[] cannonBalls;

        //컨트롤러의 이전정보
        GamePadState previousGamePadState = GamePad.GetState(PlayerIndex.One);

        //키보드의 이전정보
        KeyboardState previousKeyboardState = Keyboard.GetState();

        //UFO 배열선언
        GameObject[] enemies;

        //UFO의 최대개수
        const int maxEnemies = 3;

        //UFO 위치의 최대 높이값
        const float maxEnemyHeight = 0.1f;

        //UFO 위치의 최저 높이값
        const float minEnemyHeight = 0.5f;

        //UFO의 최대 속력값
        const float maxEnemyVelocity = 5.0f;

        //UFO의 최저 속력값
        const float minEnemyVelocity = 1.0f;

        //UFO 발생 위치가 랜덤해지기 위한 랜덤값 생성
        Random random = new Random();

        //UFO가 격추될때 마다 증가하는 점수
        int score;

        //폰트정보 변수
        SpriteFont font;

        //점수를 표시할 위치
        Vector2 scoreDrawPoint = new Vector2(0.1f, 0.1f);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            backgroundTexture =
                   Content.Load<Texture2D>("Sprites\\background");

            //GameObject 개체 인스턴스 cannon의 생성
            cannon = new GameObject(Content.Load<Texture2D>("Sprites\\cannon"));

            //대포의 처음 위치 설정
            cannon.position = new Vector2(120, graphics.GraphicsDevice.Viewport.Height - 80);

            //대포 탄환의 개체 인스턴스 cannonBalls 생성 
            cannonBalls = new GameObject[maxCannonBalls];
            for (int i = 0; i < maxCannonBalls; i++)
            {
                cannonBalls[i] = new GameObject(Content.Load<Texture2D>(
                    "Sprites\\cannonball"));
            }

            //UFO의 개체 인스턴스 enemies 생성 
            enemies = new GameObject[maxEnemies];
            for (int i = 0; i < maxEnemies; i++)
            {
                enemies[i] = new GameObject(
                    Content.Load<Texture2D>("Sprites\\enemy"));
            }

            //게임의 배경화면에 사용할 화면의 가로, 세로값 설정
            viewportRect = new Rectangle(0, 0,
                graphics.GraphicsDevice.Viewport.Width,
                graphics.GraphicsDevice.Viewport.Height);

            //텍스트에 사용할 폰트파일 적재
            font = Content.Load<SpriteFont>("Fonts\\GameFont");

            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        protected override void Update(GameTime gameTime)
        {
            // 게임을 종료하려면 Xbox 컨트롤러의 Back 버튼을 누른다.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // 키보드 상태 읽어오기
            KeyboardState keyboardState = Keyboard.GetState();

            //왼쪽 방향키가 눌렸으면
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                //대포를 왼쪽으로 회전 이동하도록 회전 변수값을 0.1 만큼 뺀다.
                cannon.rotation -= 0.1f;
            }

            //오른쪽 방향키가 눌렸으면
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                //대포를 오른쪽으로 회전 이동하도록 회전 변수값을 0.1 만큼 더한다.
                cannon.rotation += 0.1f;
            }

            //스페이스바를 눌렀다면 대포 탄환을 발사한다.
            if (keyboardState.IsKeyDown(Keys.Space) &&
               previousKeyboardState.IsKeyUp(Keys.Space))
            {
                FireCannonBall();
            }
            cannon.rotation = MathHelper.Clamp(cannon.rotation, -MathHelper.PiOver2, 0);
            base.Update(gameTime);

            //발사된 탄환이 계속 날아가도록 업데이트 한다.
            UpdateCannonBalls();

            //UFO들의 생성과 이동이 계속 진행되도록 업데이트 한다.
            UpdateEnemies();

            //연속발사를 막기 위해 현재의 키보드상태를 따로 저장한다.
            previousKeyboardState = keyboardState;
        }

        public void FireCannonBall()
        {
            //배열로 선언한 모든 대포탄환의 개수만큼 반복
            foreach (GameObject ball in cannonBalls)
            {
                //만약 탄환의 생명주기가 false 라면
                if (!ball.alive)
                {
                    // 생명주기를  true로바꾼다
                    ball.alive = true;
                    // 탄환의 첫 위치는 대포의 위치에서 탄환의 원점을 뺀만큼 잡는다
                    ball.position = cannon.position - ball.center;

                    //탄환이 날라가는 속도와 방향을 설정한다
                    ball.velocity = new Vector2(
                    (float)Math.Cos(cannon.rotation),
                    (float)Math.Sin(cannon.rotation)) * 5.0f;
                    return;
                }
            }
        }

        public void UpdateCannonBalls()
        {
            //배열로 선언한 모든 대포탄환의 개수만큼 반복
            foreach (GameObject ball in cannonBalls)
            {
                //만약 탄환의 생명주기가 true 라면
                if (ball.alive)
                {
                    //탄환의 위치를 velocity값에 따라 변화 시킨다.
                    ball.position += ball.velocity;

                    //만약 탄환이 배경화면을 벗어난다면
                    if (!viewportRect.Contains(new Point(
                        (int)ball.position.X,
                        (int)ball.position.Y)))
                    {
                        //탄환의 생명주기는 false이다.
                        ball.alive = false;
                        continue;
                    }

                    //충돌감지를 위해 대포 탄환의 Rectangle을 만든다.
                    Rectangle cannonBallRect = new Rectangle(
                        (int)ball.position.X,
                        (int)ball.position.Y,
                        ball.sprite.Width,
                        ball.sprite.Height);

                    //foreach문으로 모든 UFO의 Rectangle을 생성하고
                    //대포 탄환 Rectangle과 충돌검사를 한다.
                    foreach (GameObject enemy in enemies)
                    {
                        //충돌감지를 위해 UFO의 Rectangle을 만든다.
                        Rectangle enemyRect = new Rectangle(
                            (int)enemy.position.X,
                            (int)enemy.position.Y,
                            enemy.sprite.Width,
                            enemy.sprite.Height);

                        //Rectagle.Intersects() 메소드를 이용한 충돌 감지
                        if (cannonBallRect.Intersects(enemyRect))
                        {
                            //충돌이 되었다면 대포 탄환과 UFO 모두 지운다.
                            ball.alive = false;
                            enemy.alive = false;

                            //점수를 1증가 한다.
                            score++;
                            break;
                        }
                    }
                }
            }
        }

        public void UpdateEnemies()
        {
            //모든 UFO적기들마다..
            foreach (GameObject enemy in enemies)
            {
                //만약 UFO의 생명주기가 true라면..
                if (enemy.alive)
                {
                    //해당 적기의 위치는 진행되는 방향과 속도에 맞게 증가시킨다.
                    enemy.position += enemy.velocity;

                    //만약 게임화면을 벗어났다면
                    if (!viewportRect.Contains(new Point(
                        (int)enemy.position.X,
                        (int)enemy.position.Y)))
                    {
                        //해당 UFO의 생명주기는 false 이다.
                        enemy.alive = false;
                    }
                }

                //만약 UFO의 생명주기가 false라면..
                else
                {
                    //새로 탄생하므로 생명주기를 true로 업데이트
                    enemy.alive = true;

                    //UFO의 첫 위치 설정
                    enemy.position = new Vector2(
                        viewportRect.Right,
                        MathHelper.Lerp(
                        (float)viewportRect.Height * minEnemyHeight,
                        (float)viewportRect.Height * maxEnemyHeight,
                        (float)random.NextDouble()));

                    //UFO의 속력 및 방향 설정
                    enemy.velocity = new Vector2(
                        MathHelper.Lerp(
                        -minEnemyVelocity,
                        -maxEnemyVelocity,
                        (float)random.NextDouble()), 0);
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            //spriteBatch.Begin을 통해 이미지가 그려지는 것이 시작됨을 알려줌
            spriteBatch.Begin(SpriteSortMode.Deferred,BlendState.AlphaBlend);

            //배경을 먼저 그린다.
            spriteBatch.Draw(backgroundTexture, viewportRect, Color.White);

            //대포를 그린다.
            spriteBatch.Draw(cannon.sprite,
            cannon.position,
            null,
            Color.White,
            cannon.rotation,
            cannon.center, 1.0f,
            SpriteEffects.None, 0);


            //생명주기가 true인 대포 탄환만을 그린다.
            foreach (GameObject ball in cannonBalls)
            {
                if (ball.alive)
                {
                    spriteBatch.Draw(ball.sprite,
                        ball.position, Color.White);
                }
            }

            //생명주기가 true인 UFO만 그린다.
            foreach (GameObject enemy in enemies)
            {
                if (enemy.alive)
                {
                    spriteBatch.Draw(enemy.sprite,
                        enemy.position, Color.White);
                }
            }

            //텍스트를 표시한다.
            spriteBatch.DrawString(font,
               "Score: " + score.ToString(),
               new Vector2(scoreDrawPoint.X * viewportRect.Width,
               scoreDrawPoint.Y * viewportRect.Height),
               Color.Yellow);

            //spriteBatch.End을 통해 이미지가 그려지는 것이 끝났음을 알려줌
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
