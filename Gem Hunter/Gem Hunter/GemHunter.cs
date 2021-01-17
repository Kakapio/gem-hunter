using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Gem_Hunter
{
    public enum TileType
    {
        Floor,
        Wall,
        Player
    }
    
    public class GemHunter : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D block;
        private IRoomGenerator roomGenerator = new RoomGenerator();
        
        private bool ExitRequested => GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                                      || Keyboard.GetState().IsKeyDown(Keys.Escape);
        private bool RestartRequested => Keyboard.GetState().IsKeyDown(Keys.R);

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
            
            roomGenerator.Reset();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            block = Content.Load<Texture2D>("Block");
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            if (ExitRequested)
                Exit();

            PlayerInput();

            roomGenerator.Update();
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
                        case TileType.Player:
                            spriteBatch.Draw(block, 
                                new Rectangle(i * roomGenerator.TileSize, j * roomGenerator.TileSize, 
                                    roomGenerator.TileSize, roomGenerator.TileSize), 
                                new Color(255, 255, 255));
                            break;
                        default:
                            throw new InvalidEnumArgumentException("Given tile type is not supported.");
                    }
                }
            }
            
            spriteBatch.End();
            
            //Render target to back buffer.
            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
            spriteBatch.Draw(target, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
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
        
        private Vector2 GetPlayerPosition()
        {
            for (int i = 0; i < roomGenerator.TileMap.GetLength(0); i++)
            {
                for (int j = 0; j < roomGenerator.TileMap.GetLength(1); j++)
                {
                    if (roomGenerator.TileMap[i, j] == TileType.Player)
                        return new Vector2(i, j);
                }
            }
            
            throw new InvalidOperationException("There is no player in the map.");
        }

        private void MovePlayer(Vector2 movement)
        {
            var playerPosition = GetPlayerPosition();
            var newPosition = playerPosition + movement;

            if (roomGenerator.AvailableForPlayer(newPosition))
            {
                roomGenerator.TileMap[(int) playerPosition.X, (int) playerPosition.Y] = TileType.Floor;
                roomGenerator.TileMap[(int) newPosition.X, (int) newPosition.Y] = TileType.Player;
            }
        }
    }
}