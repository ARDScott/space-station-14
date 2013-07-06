using System;
using System.Drawing;
using Lidgren.Network;
using SS13_Shared;
using ServerInterfaces.Tiles;

namespace ServerInterfaces.Map
{
    public interface IMapManager
    {
        bool InitMap(string mapName);
        void HandleNetworkMessage(NetIncomingMessage message);
        void NetworkUpdateTile(int x, int y);
        void SaveMap();
        ITile GetTileAt(int x, int y);
        void AddGasAt(Point position, GasType type, int amount);
        float GetGasAmount(Point position, GasType type);
        float GetGasTotal(Point position);
        Vector2 GetGasVelocity(Point position);
        void UpdateAtmos();
        void SendAtmosStateTo(NetConnection client);

        /// <summary>
        /// This function takes the gas cell from one tile and moves it to another, reconnecting all of the references in adjacent tiles.
        /// Use this when a new tile is generated at a map location.
        /// </summary>
        /// <param name="fromTile">Tile to move gas information/cell from</param>
        /// <param name="toTile">Tile to move gas information/cell to</param>
        void MoveGasCell(ITile fromTile, ITile toTile);

        bool ChangeTile(int x, int z, Type newType);
        ITile GenerateNewTile(int x, int y, string type);
        NetOutgoingMessage CreateMapMessage(MapMessage messageType);

        /// <summary>
        /// Send message to all clients.
        /// </summary>
        /// <param name="message"></param>
        void SendMessage(NetOutgoingMessage message);

        void Shutdown();
        int GetMapWidth();
        int GetMapHeight();
        bool IsWorldPositionInBounds(Vector2 pos);
        Point GetTileArrayPositionFromWorldPosition(float x, float z);
        ITile GetTileFromWorldPosition(Vector2 pos);
        Point GetTileArrayPositionFromWorldPosition(Vector2 pos);
        Type GetTileTypeFromWorldPosition(float x, float z);
        bool IsSaneArrayPosition(int x, int y);
        void SendMap(NetConnection connection);
    }
}