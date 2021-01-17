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
        public int MapSize { get; } = 100; //The square dimensions of the tile map in terms of tiles.
        public int TileSize { get; } = 8; //The square dimensions of each tile sprite.
        
        private const int MaxFloors = 1200;
        
        private List<Eater> eaters;
        
        private bool EnoughWallsRemoved => CurrentFloors <= 0;

        public RoomGenerator()
        {
            TileMap = new TileType[MapSize, MapSize];
            eaters = new List<Eater>();
        }

        public void Update()
        {
            /*Console.WriteLine($"Number floors created: {MaxFloors - CurrentFloors}, " +
                              $"Number Eaters: {eaters.Count}");*/

            try
            {
                while (!EnoughWallsRemoved)
                {
                    //PrintTileCount();
                    MoveEaters();
                    AddChildEaters();
                }
            }
            finally
            {
                TileMap[MapSize / 2, MapSize / 2] = TileType.Player;
            }
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
            if (TileMap[(int) position.X, (int) position.Y] == TileType.Floor
            && IsLegalPosition(position))
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