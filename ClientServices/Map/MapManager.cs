﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ClientServices.Lighting;
using ClientServices.Map.Tiles.Floor;
using ClientServices.Map.Tiles.Wall;
using ClientServices.Resources;
using SS13_Shared;
using System.IO;
using Lidgren.Network;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using ClientInterfaces;
using ClientServices.Map.Tiles;

namespace ClientServices.Map
{
    public class MapManager : IMapManager
    {
        #region Variables
        public Tile[,] tileArray; // The array holding all the tiles that make up the map
        public int mapWidth; // Number of tiles across the map (must be a multiple of StaticGeoSize)
        public int mapHeight; // Number of tiles up the map (must be a multiple of StaticGeoSize)
        public int tileSpacing = 64; // Distance between tiles
        public Dictionary<string, Sprite> tileSprites;
        private string floorSpriteName = "floor_texture";
        private string wallTopSpriteName = "wall_texture";
        private string wallSideSpriteName = "wall_side";
        private List<Vector2D> cardinalList;
        private static PORTAL_INFO[] portal = new PORTAL_INFO[4];
        public Point lastVisPoint;

        public bool needVisUpdate = false;
        public bool loaded = false;

        private readonly IResourceManager _resourceManager;
        private readonly ILightManager _lightManager;
        private readonly ICollisionManager _collisionManager;
        #endregion

        public MapManager(IResourceManager resourceManager, ILightManager lightManager, ICollisionManager collisionManager)
        {
            _resourceManager = resourceManager;
            _lightManager = lightManager;
            _collisionManager = collisionManager;

            tileSprites = new Dictionary<string, Sprite>();
            tileSprites.Add(floorSpriteName, _resourceManager.GetSprite(floorSpriteName));
            tileSprites.Add(wallSideSpriteName, _resourceManager.GetSprite(wallSideSpriteName));
            for (int i = 0; i < 16; i++)
            {
                tileSprites.Add(wallTopSpriteName + i, _resourceManager.GetSprite(wallTopSpriteName + i));
            }
            tileSprites.Add("space_texture", _resourceManager.GetSprite("space_texture"));

            cardinalList = new List<Vector2D>();
            cardinalList.Add(new Vector2D(0, 0));
            cardinalList.Add(new Vector2D(0, 1));
            cardinalList.Add(new Vector2D(0, -1));
            cardinalList.Add(new Vector2D(1, 0));
            cardinalList.Add(new Vector2D(-1, 0));
            cardinalList.Add(new Vector2D(1, 1));
            cardinalList.Add(new Vector2D(-1, -1));
            cardinalList.Add(new Vector2D(-1, 1));
            cardinalList.Add(new Vector2D(1, -1));
            portal[0] = new PORTAL_INFO(1, 1, 1, -1, 1, 0); // East
            portal[1] = new PORTAL_INFO(-1, 1, 1, 1, 0, 1); // South
            portal[2] = new PORTAL_INFO(-1, -1, -1, 1, -1, 0); // West
            portal[3] = new PORTAL_INFO(1, -1, -1, -1, 0, -1); // North


            lastVisPoint = new System.Drawing.Point(0, 0);
        }
        
        #region Startup / Loading

        public bool LoadNetworkedMap(TileType[,] networkedArray, TileState[,] networkedStates, int _mapWidth, int _mapHeight)
        {
           
            mapWidth = _mapWidth;
            mapHeight = _mapHeight;

            tileArray = new Tile[mapWidth, mapHeight];

            //loadingText = "Building Map...";
            //loadingPercent = 0;

            float maxElements = (mapHeight * mapWidth);
            float oneElement = 100f / maxElements;
            float currCount = 0;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int posX = x * tileSpacing;
                    int posY = y * tileSpacing;
                    TileState state = networkedStates[x, y];
                    switch (networkedArray[x, y])
                    {
                        case TileType.Wall:
                            tileArray[x, y] = GenerateNewTile(TileType.Wall,  state, new Vector2D(posX, posY));
                            break;
                        case TileType.Floor:
                            tileArray[x, y] = GenerateNewTile(TileType.Floor, state, new Vector2D(posX, posY));
                            break;
                        case TileType.Space:
                            tileArray[x, y] = GenerateNewTile(TileType.Space, state, new Vector2D(posX, posY));
                            break;
                        default:
                            break;
                    }
                    currCount += oneElement;
                    if (currCount >= 1)
                    {
                        //loadingPercent += maxElements > 100 ? 1 : oneElement;
                        currCount = 0;
                    }
                }
            }

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if (tileArray[x, y].TileType == TileType.Wall)
                    {
                        byte i = SetSprite(x, y);
                        tileArray[x, y].SetSprites(tileSprites[wallTopSpriteName+i], tileSprites[wallSideSpriteName], i);
                    }
                    if (y > 0)
                    {
                        tileArray[x, y].surroundingTiles[0] = tileArray[x, y - 1]; //north
                    }
                    if (x < mapWidth - 1)
                    {
                        tileArray[x, y].surroundingTiles[1] = tileArray[x + 1, y]; //east
                    }
                    if (y < mapHeight - 1)
                    {
                        tileArray[x, y].surroundingTiles[2] = tileArray[x, y + 1]; //south
                    }
                    if (x > 0)
                    {
                        tileArray[x, y].surroundingTiles[3] = tileArray[x - 1, y]; //west
                    }

                }
            }

            loaded = true;
            return true;
        }

        public void SaveMap()
        {
            string fileName = "SavedMap";

            FileStream fs = new FileStream(fileName, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine(mapWidth);
            sw.WriteLine(mapHeight);

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    sw.WriteLine(tileArray[x, y].name);
                }
            }

            sw.Close();
            fs.Close();
        }

        #endregion

        #region Networking

        public void HandleNetworkMessage(NetIncomingMessage message)
        {
            MapMessage messageType = (MapMessage)message.ReadByte();
            switch (messageType)
            {
                case MapMessage.TurfUpdate:
                    HandleTurfUpdate(message);
                    break;
                case MapMessage.TurfAddDecal:
                    HandleTurfAddDecal(message);
                    break;
                case MapMessage.TurfRemoveDecal:
                    HandleTurfRemoveDecal(message);
                    break;
                default:
                    break;
            }
        }

        private void HandleTurfRemoveDecal(NetIncomingMessage message)
        {
            throw new NotImplementedException();
        }

        private void HandleTurfAddDecal(NetIncomingMessage message)
        {
            int x = message.ReadInt32();
            int y = message.ReadInt32();
            DecalType type = (DecalType)message.ReadByte();

            tileArray[x, y].AddDecal(type);
        }

        public void HandleAtmosDisplayUpdate(NetIncomingMessage message)
        {
            if (!loaded)
                return;
            int count = message.ReadInt32();
            List<AtmosRecord> records = new List<AtmosRecord>();
            for (int i = 1; i <= count; i++)
            {
                records.Add(new AtmosRecord(message.ReadInt32(), message.ReadInt32(), message.ReadByte()));
            }

            foreach (AtmosRecord record in records)
            {
                tileArray[record.x, record.y].SetAtmosDisplay(record.display);
            }
        }

        private struct AtmosRecord
        {
            public int x;
            public int y;
            public byte display;

            public AtmosRecord(int _x, int _y, byte _display)
            {
                x = _x;
                y = _y;
                display = _display;
            }
        }

        private void HandleTurfUpdate(NetIncomingMessage message)
        {
            short x = message.ReadInt16();
            short y = message.ReadInt16();
            TileType type = (TileType)message.ReadByte();
            TileState state = (TileState)message.ReadByte();

            if (tileArray[x, y] == null)
            {
                GenerateNewTile(type, state, new Vector2D(x * tileSpacing, y * tileSpacing));
            }
            else
            {
                if (tileArray[x, y].TileType != type)
                {
                    Tile[] surroundTiles = tileArray[x, y].surroundingTiles;
                    ILight[] lightList = tileArray[x, y].tileLights.ToArray();
                    tileArray[x, y] = GenerateNewTile(type, state, new Vector2D(x * tileSpacing, y * tileSpacing));
                    tileArray[x, y].surroundingTiles = surroundTiles;
                    foreach (Tile T in tileArray[x, y].surroundingTiles)
                    {
                        T.surroundDirs = SetSprite(T.TilePosition.X, T.TilePosition.Y);
                    }
                    foreach (ILight l in lightList)
                    {
                        l.UpdateLight();
                    }
                    needVisUpdate = true;
                }
                else if (tileArray[x, y].tileState != state)
                {
                    tileArray[x, y].tileState = state;
                }
            }
        }

        #endregion

        #region Tile helper functions
        // Returns the position of a tile in the tileArray from world coordinates
        // Returns -1,-1 if an invalid position was passed in.
        public Vector2D GetTileArrayPositionFromWorldPosition(float x, float z)
        {
            if (x < 0 || z < 0)
                return new Vector2D(-1, -1);
            if (x > mapWidth * tileSpacing || z > mapWidth * tileSpacing)
                return new Vector2D(-1, -1);

            var xPos = (int)Math.Floor(x / tileSpacing);
            var zPos = (int)Math.Floor(z / tileSpacing);

            return new Vector2D(xPos, zPos);
        }

        public Point GetTileArrayPositionFromWorldPosition(Vector2D pos)
        {
            if (pos.X < 0 || pos.Y < 0)
                return new System.Drawing.Point(-1, -1);
            if (pos.X > mapWidth * tileSpacing || pos.Y > mapWidth * tileSpacing)
                return new System.Drawing.Point(-1, -1);

            var xPos = (int)Math.Floor(pos.X / tileSpacing);
            var yPos = (int)Math.Floor(pos.Y / tileSpacing);

            return new Point(xPos, yPos);
        }

        public TileType GetTileTypeFromWorldPosition(float x, float z)
        {
            Vector2D arrayPosition = GetTileArrayPositionFromWorldPosition(x, z);
            if (arrayPosition.X < 0 || arrayPosition.Y < 0)
            {
                return TileType.None;
            }
            else
            {
                return GetTileTypeFromArrayPosition((int)arrayPosition.X, (int)arrayPosition.Y);
            }
        }

        public TileType GetTileTypeFromWorldPosition(Vector2D pos)
        {
            Vector2D arrayPosition = GetTileArrayPositionFromWorldPosition(pos.X, pos.Y);
            if (arrayPosition.X < 0 || arrayPosition.Y < 0)
            {
                return TileType.None;
            }
            else
            {
                return GetTileTypeFromArrayPosition((int)arrayPosition.X, (int)arrayPosition.Y);
            }
        }

        public TileType GetTileTypeFromArrayPosition(int x, int y)
        {
            if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight)
            {
                return TileType.None;
            }
            else
            {
                return tileArray[x, y].TileType;
            }
        }

        public ITile GetTileAt(Vector2D pos)
        {
            if (pos.X < 0 || pos.Y < 0) return null;
            var p = GetTileArrayPositionFromWorldPosition(pos);
            return tileArray[(int)p.X, (int)p.Y];
        }


        // Changes a tile based on its array position (get from world
        // coordinates using GetTileFromWorldPosition(int, int). Returns true if successful.
        public bool ChangeTile(Vector2D arrayPosition, TileType newType)
        {
            int x = (int)arrayPosition.X;
            int z = (int)arrayPosition.Y;

            if (x < 0 || z < 0)
                return false;
            if (x > mapWidth || z > mapWidth)
                return false;
            Vector2D pos = tileArray[x, z].Position;
            //Tile tile = GenerateNewTile(newType, pos);

            /*if (tile == null)
            {
                return false;
            }

            tileArray[x, z] = tile;*/
            return true;
        }

        public bool ChangeTile(int x, int z, TileType newType)
        {
            var pos = new Vector2D(x, z);
            return ChangeTile(pos, newType);
        }

        public Tile GenerateNewTile(TileType type, TileState state, Vector2D pos)
        {
            var p = new Point((int) Math.Floor(pos.X / tileSpacing), (int) Math.Floor(pos.Y / tileSpacing));

            switch (type)
            {
                case TileType.Space:
                    return new Space(tileSprites["space_texture"], state, tileSpacing, pos, p, _lightManager, _resourceManager);
                case TileType.Floor:
                    return new Floor(tileSprites[floorSpriteName], state, tileSpacing, pos, p, _lightManager, _resourceManager);
                case TileType.Wall:
                    var wall = new Wall(tileSprites[wallTopSpriteName + "0"], tileSprites[wallSideSpriteName], state, tileSpacing, pos, p, _lightManager, _resourceManager);
                    _collisionManager.AddCollidable(wall);
                    return wall;
                default:
                    return null;
            }
        }


        // Where do we have tiles around us?
        // 0 = None
        // 1 = North
        // 2 = East
        // 4 = South
        // 8 = West
        // So if we have one N and S, we return (N + S) or (1 + 4), so 5.

        public byte SetSprite(int x, int y)
        {
            byte i = 0;

            if (GetTileTypeFromArrayPosition(x, y - 1) == TileType.Wall) // N
            {
                i += 1;
            }
            if (GetTileTypeFromArrayPosition(x + 1, y) == TileType.Wall) // E
            {
                i += 2;
            }
            if (GetTileTypeFromArrayPosition(x, y + 1) == TileType.Wall) // S
            {
                i += 4;
            }
            if (GetTileTypeFromArrayPosition(x - 1, y) == TileType.Wall) // W
            {
                i += 8;
            }

            return i;
        }

        public int GetTileSpacing()
        {
            return tileSpacing;
        }

        public int GetMapWidth()
        {
            return mapWidth;
        }

        public int GetMapHeight()
        {
            return mapHeight;
        }

        /*public List<System.Drawing.RectangleF> GetSurroundingAABB(Vector2D pos)
        {
            List<System.Drawing.RectangleF> AABBList = new List<System.Drawing.RectangleF>();
            Vector2D tilePos = GetTileArrayPositionFromWorldPosition(pos.X, pos.Y);

            foreach (Vector2D dir in cardinalList)
            {
                Vector2D checkPos = pos + dir;
                if (GetTileTypeFromArrayPosition((int)checkPos.X, (int)checkPos.Y) == TileType.Wall)
                {
                    System.Drawing.RectangleF AABB = GetAABB(checkPos);
                    if (AABB != null)
                    {
                        AABBList.Add(AABB);
                    }
                }
            }

            return AABB;
        }*/

        #endregion

        #region Quick collision checks

        public bool IsSolidTile(Vector2D pos)
        {
            TileType tile = GetTileTypeFromWorldPosition(pos);

            if (tile == null) return false; //Hack. This happens when its outside the map.

            if (tile == TileType.None)
            {
                return false;
            }
            else if (tile == TileType.Wall)
            {
                return true;
            }
            else if ((tile == TileType.Floor || tile == TileType.Space))
            {
                return false;
            }
            else
            {
                return false;
            }
        }

        public bool CheckCollision(Vector2D pos)
        {
            TileType tile = GetTileTypeFromWorldPosition(pos);

            if (tile == TileType.None)
            {
                return false;
            }
            else if (tile == TileType.Wall)
            {
                return true;
            }
            else if ((tile == TileType.Floor || tile == TileType.Space))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public TileType GetObjectTypeAt(Vector2D pos)
        {
            return GetTileTypeFromWorldPosition(pos);
        }

        public bool IsFloorUnder(Vector2D pos)
        {
            if (GetTileTypeFromWorldPosition(pos) == TileType.Floor)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Shutdown
        public void Shutdown()
        {
            tileArray = null;
            tileSprites = null;
        }
        #endregion

        #region Visibility

        struct PORTAL_INFO
        {
            // offset of portal's left corner relative to square center (doubled coordinates):
            public int lx;
            public int ly;
            // offset of portal's right corner relative to square center (doubled coordinates):
            public int rx;
            public int ry;
            // offset of neighboring cell relative to this cell's coordinates (not doubled):
            public int nx;
            public int ny;

            public PORTAL_INFO(int _lx, int _ly, int _rx, int _ry, int _nx, int _ny)
            {
                lx = _lx;
                ly = _ly;
                rx = _rx;
                ry = _ry;
                nx = _nx;
                ny = _ny;
            }
        }

        #region Helper methods
        bool IsSightBlocked(int x, int y)
        {
            if (tileArray[x, y].TileType == TileType.Wall || tileArray[x, y].sightBlocked)
            {
                return true;
            }
            return false;
        }

        void ClearVisibility()
        {
            for (var x = 0; x < mapWidth; x++)
            {
                for (var y = 0; y < mapHeight; y++)
                {
                    tileArray[x, y].Visible = false;
                }
            }
        }

        public void SetAllVisible()
        {
            for (var x = 0; x < mapWidth; x++)
            {
                for (var y = 0; y < mapHeight; y++)
                {
                    tileArray[x, y].Visible = true;
                }
            }
        }

        void SetVisible(int x, int y)
        {
            tileArray[x, y].Visible = true;
        }
        #endregion

        public void ComputeVisibility(int viewer_x, int viewer_y)
        {
            ClearVisibility();
            for (var i = 0; i < 4; ++i)
            {
                ComputeVisibility
                (
                    viewer_x, viewer_y,
                    viewer_x, viewer_y,
                    portal[i].lx, portal[i].ly,
                    portal[i].rx, portal[i].ry
                );
            }
        }
        
        bool a_right_of_b(int ax, int ay, int bx, int by)
        {
            return ax * by > ay * bx;
        }
        
        void ComputeVisibility(int viewer_x, int viewer_y, int target_x, int target_y, int ldx, int ldy, int rdx, int rdy)
        {
            if (target_x > viewer_x + 15 || target_x < viewer_x - 15)
                return;
            if (target_y > viewer_y + 15 || target_y < viewer_y - 15)
                return;
            // Abort if we are out of bounds.
            if (target_x < 0 || target_x >= mapWidth)
                return;
            if (target_y < 0 || target_y >= mapHeight)
                return;

            // This square is visible.
            SetVisible(target_x, target_y);

            // A solid target square blocks all further visibility through it.
            if (IsSightBlocked(target_x, target_y))
                return;

            // Target square center position relative to viewer:
            int dx = 2 * (target_x - viewer_x);
            int dy = 2 * (target_y - viewer_y);

            for (int i = 0; i < 4; ++i)
            {
                // Relative positions of the portal's left and right endpoints:
                int pldx = dx + portal[i].lx;
                int pldy = dy + portal[i].ly;
                int prdx = dx + portal[i].rx;
                int prdy = dy + portal[i].ry;

                // Clip portal against current view frustum:
                int cldx, cldy;
                if (a_right_of_b(ldx, ldy, pldx, pldy))
                {
                    cldx = ldx;
                    cldy = ldy;
                }
                else
                {
                    cldx = pldx;
                    cldy = pldy;
                }
                int crdx, crdy;
                if (a_right_of_b(rdx, rdy, prdx, prdy))
                {
                    crdx = prdx;
                    crdy = prdy;
                }
                else
                {
                    crdx = rdx;
                    crdy = rdy;
                }

                // If we can see through the clipped portal, recurse through it.
                if (a_right_of_b(crdx, crdy, cldx, cldy))
                {
                    ComputeVisibility
                    (
                        viewer_x, viewer_y,
                        target_x + portal[i].nx, target_y + portal[i].ny,
                        cldx, cldy,
                        crdx, crdy
                    );
                }
            }
        }

        public Point GetLastVisiblePoint()
        {
            return lastVisPoint;
        }

        public void SetLastVisiblePoint(Point point)
        {
            lastVisPoint = point;
        }

        public bool NeedVisibilityUpdate()
        {
            return needVisUpdate;
        }


#endregion

        #region Lighting
        public void LightComputeVisibility(Vector2D lightPos, ILight light)
        {
            LightClearVisibility(light);
            var lightArrayPos = GetTileArrayPositionFromWorldPosition(lightPos);

            for (int i = 0; i < 4; ++i)
            {
                LightComputeVisibility
                (
                    (int)lightArrayPos.X, (int)lightArrayPos.Y,
                    (int)lightArrayPos.X, (int)lightArrayPos.Y,
                    portal[i].lx, portal[i].ly,
                    portal[i].rx, portal[i].ry,
                    light
                );
            }
        }

        public void LightClearVisibility(ILight light)
        {
            foreach (Tile T in light.GetTiles())
            {
                T.tileLights.Remove(light);
            }
            light.ClearTiles();
        }

        void light_set_visible(int x, int y, ILight light)
        {
            if (!(tileArray[x, y].TileType == TileType.Wall && tileArray[x, y].Position.Y > light.Position.Y))
            {
                light.AddTile(tileArray[x, y]);
                if (!tileArray[x, y].tileLights.Contains(light))
                {
                    tileArray[x, y].tileLights.Add(light);
                }
            }
        }

        bool light_is_sight_blocked(int x, int y)
        {
            if (tileArray[x, y].TileType == TileType.Wall || tileArray[x, y].sightBlocked)
            {
                return true;
            }
            return false;
        }

        void LightComputeVisibility(int viewer_x, int viewer_y, int target_x, int target_y, int ldx, int ldy, int rdx, int rdy, ILight light)
        {
            if (target_x > viewer_x + (light.Range / tileSpacing) + 1 || target_x < viewer_x - (light.Range / tileSpacing) - 1)
                return;
            if (target_y > viewer_y + (light.Range / tileSpacing) + 1 || target_y < viewer_y - (light.Range / tileSpacing) - 1)
                return;
            // Abort if we are out of bounds.
            if (target_x < 0 || target_x >= mapWidth)
                return;
            if (target_y < 0 || target_y >= mapHeight)
                return;

            // This square is visible.
            light_set_visible(target_x, target_y, light);

            // A solid target square blocks all further visibility through it.
            if (IsSightBlocked(target_x, target_y))
                return;

            // Target square center position relative to viewer:
            int dx = 2 * (target_x - viewer_x);
            int dy = 2 * (target_y - viewer_y);

            for (int i = 0; i < 4; ++i)
            {
                // Relative positions of the portal's left and right endpoints:
                int pldx = dx + portal[i].lx;
                int pldy = dy + portal[i].ly;
                int prdx = dx + portal[i].rx;
                int prdy = dy + portal[i].ry;

                // Clip portal against current view frustum:
                int cldx, cldy;
                if (a_right_of_b(ldx, ldy, pldx, pldy))
                {
                    cldx = ldx;
                    cldy = ldy;
                }
                else
                {
                    cldx = pldx;
                    cldy = pldy;
                }
                int crdx, crdy;
                if (a_right_of_b(rdx, rdy, prdx, prdy))
                {
                    crdx = prdx;
                    crdy = prdy;
                }
                else
                {
                    crdx = rdx;
                    crdy = rdy;
                }

                // If we can see through the clipped portal, recurse through it.
                if (a_right_of_b(crdx, crdy, cldx, cldy))
                {
                    LightComputeVisibility
                    (
                        viewer_x, viewer_y,
                        target_x + portal[i].nx, target_y + portal[i].ny,
                        cldx, cldy,
                        crdx, crdy,
                        light
                    );
                }
            }
        }

        #endregion
    }
}
