using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;



namespace My_2D_Game
{
    public enum GameState
    {
        Start,
        InGame,
        GameOver
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Represents the player
        Player player;

        // determine key presses
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        // determine button presses
        GamePadState currentGamePadState;
        GamePadState previousGamePadState;

        // A movement speed for the player
        float playerMoveSpeed;

        // Image used to display the static background
        Texture2D mainBackground;

        // Parallaxing Layers
        ParallaxingBackground bgLayer1;
        ParallaxingBackground bgLayer2;

        // Enemies
        Texture2D enemyTexture;
        List<Enemy> enemies;

        // Enemies
        //Texture2D enemyTexture1;
        //List<Enemy> enemies1;

        // The rate at which the enemies appear
        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;

        // A random number generator
        Random random;

        //bullets
        Texture2D projectileTexture;
        List<Projectile> projectiles;

        // The rate of fire of the player laser
        TimeSpan fireTime;
        TimeSpan previousFireTime;

        //Explosion
        Texture2D explosionTexture;
        List<Animation> explosions;

        
        SoundEffect laserSound;
      
        SoundEffect explosionSound;

        Song gameplayMusic;
        Song gamestartMusic;
        Song gameendMusic;

        //Number that holds the player score
        int score;
        // The font used to display UI elements
        SpriteFont font;

        GameState currentGameState = new GameState();

        Texture2D startBackground;
        Texture2D endBackground;

        Boolean isFirstTime = false;

        Boolean isHealthOver = false;

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
            currentGameState = GameState.Start;
            isFirstTime = true;

            isHealthOver = false;

            player = new Player();

            // Set a constant player move speed
            playerMoveSpeed = 5.0f;

            bgLayer1 = new ParallaxingBackground();
            bgLayer2 = new ParallaxingBackground();

            //enemies list
            enemies = new List<Enemy>();
            //enemies1 = new List<Enemy>();

            //set the time keepers to zero
            previousSpawnTime = TimeSpan.Zero;

            // Used to determine how fast enemy respawns
            enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            random = new Random();

            projectiles = new List<Projectile>();

            // Set the laser to fire every quarter second
            fireTime = TimeSpan.FromSeconds(.15f);

            explosions = new List<Animation>();

            //Set player's score to zero
            score = 0;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //player.Initialize(Content.Load<Texture2D>("player1"), playerPosition);

            Animation playerAnimation = new Animation();
            Texture2D playerTexture = Content.Load<Texture2D>("shipAnimation1");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 113, 63, 8, 30, Color.White, 1f, true);

            // Load the parallaxing background
            bgLayer1.Initialize(Content, "bgLayer1", GraphicsDevice.Viewport.Width, -1);
            bgLayer2.Initialize(Content, "bgLayer2", GraphicsDevice.Viewport.Width, -2);

            enemyTexture = Content.Load<Texture2D>("enemyAnimation");
            //enemyTexture1 = Content.Load<Texture2D>("enemyAnimation1");

            projectileTexture = Content.Load<Texture2D>("laser");
            //projectileTexture = Content.Load<Texture2D>("bullet");

            explosionTexture = Content.Load<Texture2D>("explosion");

            // Load the score font
            font = Content.Load<SpriteFont>("gameFont");

            // Load the music
            gameplayMusic = Content.Load<Song>("sound/gameplay");
            gamestartMusic = Content.Load<Song>("sound/start");
            gameendMusic = Content.Load<Song>("sound/end");


            // Load the laser and explosion sound effect
            laserSound = Content.Load<SoundEffect>("sound/laserFire");
            explosionSound = Content.Load<SoundEffect>("sound/explosion");

            mainBackground = Content.Load<Texture2D>("mainbackground");

            startBackground = Content.Load<Texture2D>("startScreen");
            endBackground = Content.Load<Texture2D>("endScreen");

            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            player.Initialize(playerAnimation, playerPosition);
           
        }

        private void PlayMusic(Song song)
        {
            // Due to the way the MediaPlayer plays music,have to catch the exception. Music will play when the game is not tethered
            try
            {
                // Play the music
                MediaPlayer.Play(song);

                // Loop the currently playing song
                MediaPlayer.IsRepeating = true;
            }
            catch { }
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
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (currentGameState == GameState.Start)
            {
                if (isFirstTime)
                {
                    PlayMusic(gamestartMusic);
                }

                isFirstTime = false;

                if (Keyboard.GetState().GetPressedKeys().Length > 0)
                {
                    currentGameState = GameState.InGame;
                    MediaPlayer.Stop();
                    PlayMusic(gameplayMusic);
                }
            }

            //if (Keyboard.GetState().GetPressedKeys().Length > 0)
            //{
            //    currentGameState = GameState.InGame;
            //    MediaPlayer.Stop();
            //    PlayMusic(gameplayMusic);
            //}

            if (currentGameState == GameState.InGame)
            {
                // Save the previous state of the keyboard and game pad so we can determine single key/button presses
                previousGamePadState = currentGamePadState;
                previousKeyboardState = currentKeyboardState;

                // Read the current state of the keyboard and gamepad and store it
                currentKeyboardState = Keyboard.GetState();
                currentGamePadState = GamePad.GetState(PlayerIndex.One);


                //Update the player
                UpdatePlayer(gameTime);

                // Update the parallaxing background
                bgLayer1.Update();
                bgLayer2.Update();

                // Update the enemies
                UpdateEnemies(gameTime);

                // Update the collision
                UpdateCollision();

                // Update the projectiles
                UpdateProjectiles();

                // Update the explosions
                UpdateExplosions(gameTime);

            }

            if (currentGameState == GameState.GameOver)
            {
                if (isHealthOver)
                {
                    PlayMusic(gameendMusic);
                }

                isHealthOver = false;
            }

            base.Update(gameTime);
        }


        private void UpdateCollision()
        {
            // Use the Rectangle's built-in intersect function to determine if two objects are overlapping
            Rectangle rectangle1;
            Rectangle rectangle2;

            // Only create the rectangle once for the player
            rectangle1 = new Rectangle((int)player.Position.X,(int)player.Position.Y,player.Width,player.Height);

            // Do the collision between the player and the enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                rectangle2 = new Rectangle((int)enemies[i].Position.X,(int)enemies[i].Position.Y,enemies[i].Width,enemies[i].Height);

                // Determine if the two objects collided with each other
                if (rectangle1.Intersects(rectangle2))
                {
                    // Subtract the health from the player based on the enemy damage
                    player.Health -= enemies[i].Damage;

                    // Since the enemy collided with the player destroy it
                    enemies[i].Health = 0;

                    // If the player health is less than zero player died
                    if (player.Health <= 0)
                    {
                        //PlayMusic(gameendMusic);
                        isHealthOver = true;
                        player.Active = false;
                        currentGameState = GameState.GameOver;
                    }

                }

            }


            // Do the collision between the player and the enemies1
            //for (int i = 0; i < enemies1.Count; i++)
            //{
            //    rectangle2 = new Rectangle((int)enemies1[i].Position.X, (int)enemies1[i].Position.Y, enemies1[i].Width, enemies1[i].Height);

            //    // Determine if the two objects collided with each other
            //    if (rectangle1.Intersects(rectangle2))
            //    {
            //        // Subtract the health from the player based on the enemy1 damage
            //        player.Health -= enemies1[i].Damage;

            //        // Since the enemy1 collided with the player destroy it
            //        enemies1[i].Health = 0;

            //        // If the player health is less than zero we died
            //        if (player.Health <= 0)
            //            player.Active = false;
            //    }

            //}




            // Bullets vs Enemy Collision
            for (int i = 0; i < projectiles.Count; i++)
            {
                for (int j = 0; j < enemies.Count; j++)
                {
                    // Create the rectangles need to determine if collided with each other
                    rectangle1 = new Rectangle((int)projectiles[i].Position.X -projectiles[i].Width / 2, (int)projectiles[i].Position.Y -
                    projectiles[i].Height / 2, projectiles[i].Width, projectiles[i].Height);

                    rectangle2 = new Rectangle((int)enemies[j].Position.X - enemies[j].Width / 2, (int)enemies[j].Position.Y - enemies[j].Height / 2, enemies[j].Width, enemies[j].Height);

                    // Determine if the two objects collided with each other
                    if (rectangle1.Intersects(rectangle2))
                    {
                        enemies[j].Health -= projectiles[i].Damage;
                        projectiles[i].Active = false;
                    }
                }
            }
        }


        private void AddProjectile(Vector2 position)
        {
            Projectile projectile = new Projectile();
            projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position);
            projectiles.Add(projectile);
        }

        private void UpdateProjectiles()
        {
            // Update the Projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update();

                if (projectiles[i].Active == false)
                {
                    projectiles.RemoveAt(i);
                }
            }
        }

        private void UpdateExplosions(GameTime gameTime)
        {
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                explosions[i].Update(gameTime);

                if (explosions[i].Active == false)
                {
                    explosions.RemoveAt(i);
                }
            }
        }

        private void UpdatePlayer(GameTime gameTime)
        {
            player.Update(gameTime);

            // Get Thumbstick Controls
            player.Position.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
            player.Position.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;

            // Use the Keyboard / Dpad
            if (currentKeyboardState.IsKeyDown(Keys.Left) || currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                player.Position.X -= playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right) || currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                player.Position.X += playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Up) || currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                player.Position.Y -= playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Down) || currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                player.Position.Y += playerMoveSpeed;
            }

           

            // Make sure that the player does not go out of bounds
            player.Position.X = MathHelper.Clamp(player.Position.X, 50, GraphicsDevice.Viewport.Width - player.Width);
            player.Position.Y = MathHelper.Clamp(player.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);

            // Fire only every interval set as the fireTime
            if (gameTime.TotalGameTime - previousFireTime > fireTime)
            {
                // Reset current time
                previousFireTime = gameTime.TotalGameTime;

                // Add the projectile, but add it to the front and center of the player
                AddProjectile(player.Position + new Vector2(player.Width / 2, 0));

                // Play the laser sound
                laserSound.Play();
            }

            // reset score if player health goes to zero
            //if (player.Health <= 0)
            //{
            //    //player.Health = 100;
            //    //score = 0;
            //    currentGameState = GameState.GameOver;
            //    isHealthOver = true;
            //}

        }

        private void AddEnemy()
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();
            Animation enemyAnimation1 = new Animation();

            // Initialize the animation with the correct animation information
            enemyAnimation.Initialize(enemyTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);
            //enemyAnimation1.Initialize(enemyTexture1, Vector2.Zero, 47, 61, 8, 20, Color.White, 1f, true);

            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width + enemyTexture.Width / 2, random.Next(100, GraphicsDevice.Viewport.Height - 100));
            //Vector2 position1 = new Vector2(GraphicsDevice.Viewport.Width + enemyTexture1.Width / 2, random.Next(100, GraphicsDevice.Viewport.Height - 100));

            // Create an enemy
            Enemy enemy = new Enemy();
            Enemy enemy1 = new Enemy();

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);
           //enemy1.Initialize(enemyAnimation1, position1);

            // Add the enemy to the active enemies list
            enemies.Add(enemy);
            //enemies1.Add(enemy1);
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            // Spawn a new enemy every 1.5 seconds
            if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
            {
                previousSpawnTime = gameTime.TotalGameTime;

                // Add an Enemy
                AddEnemy();
            }

            // Update the Enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Update(gameTime);

                if (enemies[i].Active == false)
                {
                    // If not active and health <= 0
                    if (enemies[i].Health <= 0)
                    {
                        // Add an explosion
                        AddExplosion(enemies[i].Position);

                        // Play the explosion sound
                        explosionSound.Play();

                        //Add to the player's score
                        score += enemies[i].Value;
                    }

                    enemies.RemoveAt(i);
                }




                //enemies1[i].Update(gameTime);

                //if (enemies1[i].Active == false)
                //{
                //    // If not active and health <= 0
                //    if (enemies1[i].Health <= 0)
                //    {
                //        // Add an explosion
                //        AddExplosion(enemies1[i].Position);

                //        // Play the explosion sound
                //        explosionSound.Play();

                //        //Add to the player's score
                //        score += enemies1[i].Value;
                //    }

                //    enemies1.RemoveAt(i);
                //}



            }
        }

        private void AddExplosion(Vector2 position)
        {
            Animation explosion = new Animation();
            explosion.Initialize(explosionTexture, position, 134, 134, 12, 45, Color.White, 0.5f, false);
            explosions.Add(explosion);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            if (currentGameState == GameState.Start)
            {
                spriteBatch.Draw(startBackground, Vector2.Zero, Color.White);
            }

            if (currentGameState == GameState.InGame)
            {
                spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

                // Draw the moving background
                bgLayer1.Draw(spriteBatch);
                bgLayer2.Draw(spriteBatch);


                // Draw the Enemies
                for (int i = 0; i < enemies.Count; i++)
                {
                    enemies[i].Draw(spriteBatch);
                }


                //// Draw the Enemies1
                //for (int i = 0; i < enemies1.Count; i++)
                //{
                //    enemies1[i].Draw(spriteBatch);
                //}



                // Draw the Projectiles
                for (int i = 0; i < projectiles.Count; i++)
                {
                    projectiles[i].Draw(spriteBatch);
                }

                // Draw the explosions
                for (int i = 0; i < explosions.Count; i++)
                {
                    explosions[i].Draw(spriteBatch);
                }

                // Draw the score
                spriteBatch.DrawString(font, "Score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
                // Draw the player health
                spriteBatch.DrawString(font, "Health: " + player.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);

                // Draw the Player
                player.Draw(spriteBatch);

            }

            if (currentGameState == GameState.GameOver)
            {
                //PlayMusic(gameendMusic);
                spriteBatch.Draw(endBackground, Vector2.Zero, Color.White);
                // Allows the game to exit
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                {
                    this.Exit();
                }
                // Draw the score
                spriteBatch.DrawString(font, "Total Score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);

                
            }

            base.Draw(gameTime);

            spriteBatch.End();
        }


    }
}
