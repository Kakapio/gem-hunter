using System;
using Gem_Hunter;
using Microsoft.Xna.Framework;

namespace Gem_Hunter
{
    public class Eater
    {
        public Vector2 Direction { get; private set; }
        public Vector2 Position { get; private set; }
        public Boolean HasChild { get; set; }

        private readonly Random random = new Random();
        private const int SpawnEaterChance = 400; //Higher value = less likely to spawn another.
        private IRoomGenerator roomGenerator;

        public Eater(Vector2 position, IRoomGenerator roomGenerator)
        {
            GenerateDirection();
            Position = position;
            this.roomGenerator = roomGenerator;
        }

        /// <summary>
        /// Move the Eater in its Direction by one step, remove whatever wall is in the way.
        /// </summary>
        public void TryMove()
        {
            var newPos = Position + Direction;
            if (!roomGenerator.IsLegalPosition(newPos))
            {
                GenerateDirection();
                return;
            }

            MoveTo(newPos);
            TrySpawnEater();
            GenerateDirection();
        }

        private void TrySpawnEater()
        {
            int genNewEater = random.Next(SpawnEaterChance);
            if (genNewEater == 0) // 1/SpawnEaterChance of spawning new eater.
            {
                HasChild = true;
            }
        }

        private void MoveTo(Vector2 newPos)
        {
            Position = newPos;

            if (roomGenerator.TileMap[(int) Position.X, (int) Position.Y] != TileType.Floor)
            {
                roomGenerator.TileMap[(int) Position.X, (int) Position.Y] = TileType.Floor;
                roomGenerator.CurrentFloors--;
            }
        }

        /// <summary>
        /// Generate a new direction for the eater.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void GenerateDirection()
        {
            int dir = random.Next(4);

            switch (dir)
            {
                //Left
                case 0:
                    Direction = new Vector2(-1, 0);
                    break;
                //Up
                case 1:
                    Direction = new Vector2(0, 1);
                    break;
                //Right
                case 2:
                    Direction = new Vector2(1, 0);
                    break;
                //Down
                case 3:
                    Direction = new Vector2(0, -1);
                    break;
                default:
                    throw new ArgumentException("Invalid value generated for new direction.");
            }
        }
    }
}