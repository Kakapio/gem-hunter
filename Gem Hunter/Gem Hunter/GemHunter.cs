using System;
using System.ComponentModel;
using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gem_Hunter
{
    public enum TileType
    {
        Floor,
        Wall,
        Gem
    }
    
    public class GemHunter : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private FontSystem fontSystem;
        private Texture2D block;
        private Song song;
        private SoundEffect gotGem;
        private IRoomGenerator roomGenerator = new RoomGenerator();
        private Vector2 playerPosition = new Vector2(0, 0);
        private int currentPlayerColor = 0;
        private int score;
        private Random random = new Random();

        private bool ExitRequested => GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                                      || Keyboard.GetState().IsKeyDown(Keys.Escape);
        private bool RestartRequested => Keyboard.GetState().IsKeyDown(Keys.R);
        private bool ColorChangeRequested => Keyboard.GetState().IsKeyDown(Keys.C);

        public GemHunter()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            //Resolution of window.
            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 1000;
            graphics.ApplyChanges();

            //Cap framerate.
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 15d);
            
            playerPosition = new Vector2(roomGenerator.MapSize / 2, roomGenerator.MapSize / 2);
            MediaPlayer.Play(song);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            fontSystem = FontSystemFactory.Create(GraphicsDevice);
            fontSystem.AddFont(File.ReadAllBytes(@"Content\dogica.ttf"));
            block = Content.Load<Texture2D>("Block");
            song = Content.Load<Song>("diamond_song");
            gotGem = Content.Load<SoundEffect>("got_diamond");
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (ExitRequested)
                Exit();

            if (roomGenerator.CurrentGems <= 0 || RestartRequested)
                StartNextLevel();

            if (ColorChangeRequested)
            {
                ChangePlayerColor();
            }
            
            LoopSong();
            PlayerInput();
        }

        private void ChangePlayerColor()
        {
            if (currentPlayerColor >= 7)
                currentPlayerColor = 0;
            else
                currentPlayerColor++;
        }

        private void LoopSong()
        {
            if (MediaPlayer.State == MediaState.Stopped)
                MediaPlayer.Play(song);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            
            GraphicsDevice.Clear(Color.SkyBlue);
            
            //Rendering to a RenderTarget and then later passing it to the back buffer.
            RenderTarget2D target = new RenderTarget2D(GraphicsDevice, 
                roomGenerator.MapSize * roomGenerator.TileSize, 
                roomGenerator.MapSize * roomGenerator.TileSize);
            GraphicsDevice.SetRenderTarget(target);
            
            spriteBatch.Begin();

            //Draw each tile in the proper position.
            for (int i = 0; i < roomGenerator.TileMap.GetLength(0); i++)
            {
                for (int j = 0; j < roomGenerator.TileMap.GetLength(1); j++)
                {
                    switch (roomGenerator.TileMap[i, j])
                    {
                        case TileType.Floor:
                            spriteBatch.Draw(block, 
                                new Rectangle(i * roomGenerator.TileSize, j * roomGenerator.TileSize, 
                                    roomGenerator.TileSize, roomGenerator.TileSize), 
                                new Color(164, 132, 108));
                            break;
                        case TileType.Wall:
                            spriteBatch.Draw(block, 
                                new Rectangle(i * roomGenerator.TileSize, j * roomGenerator.TileSize, 
                                    roomGenerator.TileSize, roomGenerator.TileSize), 
                                new Color(44, 28, 52));
                            break;
                        case TileType.Gem:
                            spriteBatch.Draw(block, 
                                new Rectangle(i * roomGenerator.TileSize, j * roomGenerator.TileSize, 
                                    roomGenerator.TileSize, roomGenerator.TileSize), 
                                GetGemColor());
                            break;
                        default:
                            throw new InvalidEnumArgumentException("Given tile type is not supported.");
                    }
                }
            }
            
            //Render player.
            spriteBatch.Draw(block,
                new Rectangle((int) playerPosition.X * roomGenerator.TileSize, 
                    (int) playerPosition.Y * roomGenerator.TileSize, 
                    roomGenerator.TileSize, roomGenerator.TileSize), GetPlayerColor(currentPlayerColor));
            
            spriteBatch.End();
            
            //Render target to back buffer.
            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
            spriteBatch.Draw(target, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
            
            //Render score.
            DynamicSpriteFont font = fontSystem.GetFont(18);
            spriteBatch.DrawString(font, $"Score: {score}", new Vector2(755, 970), Color.White);
            
            spriteBatch.End();
        }

        private void PlayerInput()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                MovePlayer(new Vector2(0, -1));
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                MovePlayer(new Vector2(0, 1));
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                MovePlayer(new Vector2(-1, 0));
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                MovePlayer(new Vector2(1, 0));
            }
        }

        private void MovePlayer(Vector2 movement)
        {
            var newPosition = playerPosition + movement;

            if (roomGenerator.AvailableForPlayer(newPosition))
            {
                if (roomGenerator.TileMap[(int) newPosition.X, (int) newPosition.Y] == TileType.Gem)
                {
                    CollectGem(newPosition);
                }
                playerPosition = newPosition;
            }
        }

        private void StartNextLevel()
        {
            roomGenerator.CurrentGems = 0;
            playerPosition = new Vector2(roomGenerator.MapSize / 2, roomGenerator.MapSize / 2);
            roomGenerator.Generate();
        }

        private void CollectGem(Vector2 position)
        {
            roomGenerator.TileMap[(int) position.X, (int) position.Y] = TileType.Floor;
            gotGem.Play();
            score += 10;
            roomGenerator.CurrentGems--;
        }
        
        private Color GetGemColor() =>
            random.Next(2) switch
            {
                0 => new Color(255, 234, 0),
                1 => new Color(252, 255, 194),
                _ => Color.Purple
            };

        private Color GetPlayerColor(int current)
        {
            switch (current)
            {
                    case 0:
                        return Color.DodgerBlue;
                    case 1:
                        return Color.Fuchsia;
                    case 2:
                        return Color.Green;
                    case 3:
                        return Color.DarkRed;
                    case 4:
                        return Color.Olive;
                    case 5:
                        return Color.SeaGreen;
                    case 6:
                        return Color.Orange;
                    case 7:
                        return Color.Coral;
                    default:
                        return Color.White;
            }
        }
    }
}