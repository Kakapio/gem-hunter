using System;
using Gem_Hunter;
using Microsoft.Xna.Framework;

namespace Gem_Hunter
{
    public interface IRoomGenerator
    {
        public Boolean IsLegalPosition(Vector2 position);
        public Boolean AvailableForPlayer(Vector2 position);
        public void Update();
        public void Reset();
        
        public TileType[,] TileMap { get; }
        public int CurrentFloors { get; set; }
        public int MapSize { get; }
        public int TileSize { get; }
    }
}