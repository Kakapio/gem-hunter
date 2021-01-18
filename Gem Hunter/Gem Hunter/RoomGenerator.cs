using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Gem_Hunter
{
    public class RoomGenerator : IRoomGenerator
    {
        public TileType[,] TileMap { get; }
        public int CurrentFloors { get; set; } = MaxFloors;
        public int CurrentGems { get; set; } = 0;
        public int MapSize { get; } = 100; //The square dimensions of the tile map in terms of tiles.
        public int TileSize { get; } = 8; //The square dimensions of each tile sprite.

        private const int MaxFloors = 1200;
        private const int MaxGems = 3;
        private List<Eater> eaters;
        private bool EnoughWallsRemoved => CurrentFloors <= 0;
        private bool EnoughGemsAdded => CurrentGems >= MaxGems;

        public RoomGenerator()
        {
            TileMap = new TileType[MapSize, MapSize];
            eaters = new List<Eater>();
            Generate();
        }

        public void Generate()
        {
            Reset();
            
            while (!EnoughWallsRemoved)
            {
                MoveEaters();
            }

            while (!EnoughGemsAdded)
            {
                AddGem();
            }
        }

        private void AddGem()
        {
            Random random = new Random();
            Vector2 gemPosition = new Vector2(random.Next(MapSize), random.Next(MapSize));

            while (TileMap[(int) gemPosition.X, (int) gemPosition.Y] != TileType.Floor)
            {
                gemPosition = new Vector2(random.Next(MapSize), random.Next(MapSize));
            }

            TileMap[(int) gemPosition.X, (int) gemPosition.Y] = TileType.Gem;
            CurrentGems++;
        }

        public Boolean IsLegalPosition(Vector2 position)
        {
            if (position.X < 0 || position.Y < 0 
                               || position.X > MapSize - 1 || position.Y > MapSize - 1)
            {
                return false;
            }

            return true;
        }
        
        public Boolean AvailableForPlayer(Vector2 position)
        {
            if (IsLegalPosition(position)
            && TileMap[(int) position.X, (int) position.Y] != TileType.Wall)
                return true;

            return false;
        }
        
        public void Reset()
        {
            //Set all tiles back to wall.
            for (int i = 0; i < TileMap.GetLength(0); i++)
            {
                for (int j = 0; j < TileMap.GetLength(1); j++)
                {
                    TileMap[i, j] = TileType.Wall;
                }
            }

            CurrentFloors = MaxFloors;
            eaters.Clear();
            
            eaters.Add(new Eater(new Vector2(MapSize / 2, MapSize / 2), this));
        }
        
        private void PrintTileCount()
        {
            int trueCount = 0;

            for (int i = 0; i < TileMap.GetLength(0); i++)
            {
                for (int j = 0; j < TileMap.GetLength(1); j++)
                {
                    if (TileMap[i, j] == TileType.Floor)
                        trueCount++;
                }
            }

            Console.WriteLine($"True Count: {trueCount}");
        }

        private void MoveEaters()
        {
            AddChildEaters();
            foreach (var eater in eaters)
            {
                //End condition check placed here to ensure too many walls are not removed.
                if (EnoughWallsRemoved)
                    return;

                eater.TryMove();
            }
        }
        
        private void AddChildEaters()
        {
            for (int i = 0; i < eaters.Count; i++)
            {
                if (! eaters[i].HasChild)
                    return;

                eaters.Add(new Eater(eaters[i].Position, this));
                eaters[i].HasChild = false;
            }
        }
    }
}