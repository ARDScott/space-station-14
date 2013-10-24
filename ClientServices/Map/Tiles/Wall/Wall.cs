﻿using System;
using System.Drawing;
using ClientInterfaces.Collision;
using ClientInterfaces.Map;
using GameObject;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using SS13.IoC;
using SS13_Shared;

namespace ClientServices.Tiles
{
    public class Wall : Tile, ICollidable
    {
        private readonly IMapManager mapMgr;
        public Sprite topSpriteNW;
        public Sprite topSpriteSE;

        public Wall(TileState state, RectangleF rect, Direction dir)
            : base(state, rect)
        {
            ConnectSprite = true;
            Opaque = true;
            _dir = dir;
            name = "Wall";

            if (dir == Direction.East)
            {
                Sprite = _resourceManager.GetSprite("wall_EW");
            }
            else
            {
                Sprite = _resourceManager.GetSprite("wall_NS");
            }

            mapMgr = IoCManager.Resolve<IMapManager>();
        }

        public override void Initialize()
        {
            SetSprite();
            base.Initialize();
        }

        public override void SetSprite()
        {
            surroundDirsNW = 0;
            surroundDirsSE = 0;
            float halfSpacing = mapMgr.GetTileSpacing() / 2f;
            Vector2D checkPos = Position + new Vector2D(1f, 1f);
            if (mapMgr.GetWallAt(checkPos + new Vector2D(0,-halfSpacing)) != null) // North side
            {
                surroundDirsNW += 1;
            }
            if (mapMgr.GetWallAt(checkPos + new Vector2D(halfSpacing, 0)) != null && _dir != Direction.East) // East side
            {
                surroundDirsNW += 2;
            }
            if (mapMgr.GetWallAt(checkPos + new Vector2D(0, halfSpacing)) != null && _dir != Direction.North) // South side
            {
                surroundDirsNW += 4;
            }
            if (mapMgr.GetWallAt(checkPos + new Vector2D(-halfSpacing, 0)) != null) // West side, yo
            {
                surroundDirsNW += 8;
            }

            checkPos += new Vector2D(bounds.Width - 2f, bounds.Height - 2f);
            if (mapMgr.GetWallAt(checkPos + new Vector2D(0, -halfSpacing)) != null && _dir != Direction.North) // North side
            {
                surroundDirsSE += 1;
            }
            if (mapMgr.GetWallAt(checkPos + new Vector2D(halfSpacing, 0)) != null) // East side
            {
                surroundDirsSE += 2;
            }
            if (mapMgr.GetWallAt(checkPos + new Vector2D(0, halfSpacing)) != null) // South side
            {
                surroundDirsSE += 4;
            }
            if (mapMgr.GetWallAt(checkPos + new Vector2D(-halfSpacing, 0)) != null && _dir != Direction.East) // West side, yo
            {
                surroundDirsSE += 8;
            }

            int first = 0, second = 0;

            if (_dir == Direction.East)
            {
                if ((surroundDirsNW & 8) != 0)  first = 1;
                if ((surroundDirsSE & 2) != 0)  second = 1;
                Sprite = _resourceManager.GetSprite("wall_EW_" + first + "_" + second);
                topSpriteNW = _resourceManager.GetSprite("wall_top_W_" + surroundDirsNW);
                topSpriteSE = _resourceManager.GetSprite("wall_top_E_" + surroundDirsSE);
            }
            else
            {
                Sprite = _resourceManager.GetSprite("wall_NS");
                topSpriteNW = _resourceManager.GetSprite("wall_top_N_" + surroundDirsNW);
                topSpriteSE = _resourceManager.GetSprite("wall_top_S_" + surroundDirsSE);
            }
        }


        #region ICollidable Members

        public bool IsHardCollidable
        {
            get { return true; }
        }

        

        public RectangleF AABB
        {
            get { return bounds; }
        }

        public void Bump(Entity collider)
        {
        }

        #endregion

        public override void Render(float xTopLeft, float yTopLeft, Batch batch)
        {
            Sprite.SetPosition((float)bounds.X - xTopLeft,
                                        (float)bounds.Y - (Sprite.Height - bounds.Height) - yTopLeft);

            Sprite.Color = Color.White;
            batch.AddClone(Sprite);
        }

        private void RenderOccluder(Direction d, Direction from, float x, float y, int tileSpacing)
        {
            int bx = 0;
            int by = 0;
            float drawX = 0;
            float drawY = 0;
            int width = 0;
            int height = 0;
            switch (from)
            {
                case Direction.North:
                    by = -2;
                    break;
                case Direction.NorthEast:
                    by = -2;
                    bx = 2;
                    break;
                case Direction.East:
                    bx = 2;
                    break;
                case Direction.SouthEast:
                    by = 2;
                    bx = 2;
                    break;
                case Direction.South:
                    by = 2;
                    break;
                case Direction.SouthWest:
                    by = 2;
                    bx = -2;
                    break;
                case Direction.West:
                    bx = -2;
                    break;
                case Direction.NorthWest:
                    bx = -2;
                    by = -2;
                    break;
            }
            switch (d)
            {
                case Direction.North:
                    drawX = x;
                    drawY = y;
                    width = tileSpacing;
                    height = 2;
                    break;
                case Direction.East:
                    drawX = x + tileSpacing;
                    drawY = y;
                    width = 2;
                    height = tileSpacing;
                    break;
                case Direction.South:
                    drawX = x;
                    drawY = y + tileSpacing;
                    width = tileSpacing;
                    height = 2;
                    break;
                case Direction.West:
                    drawX = x;
                    drawY = y;
                    width = 2;
                    height = tileSpacing;
                    break;
            }

            Gorgon.CurrentRenderTarget.FilledRectangle(drawX + bx, drawY + bx, width + Math.Abs(bx),
                                                       height + Math.Abs(by), Color.Black);
        }

        public override void RenderPos(float x, float y, int tileSpacing, int lightSize)
        {

            //Gorgon.CurrentRenderTarget.FilledRectangle(x, y, bounds.Width,
                                                       //bounds.Height, Color.Black);



            //Not drawing occlusion for tiles on the edge. Fuck this. Looks better too since there isnt actually anything to hide behind them.
            /*if ((Position.X == ((mapMgr.GetMapWidth() - 1) * mapMgr.GetTileSpacing()) || Position.X == 0) ||
                (Position.Y == ((mapMgr.GetMapHeight() - 1) * mapMgr.GetTileSpacing()) || Position.Y == 0))
                return;

            int l = lightSize/2;
            var from = Direction.East;
            if (l < x && l < y)
                from = Direction.NorthWest;
            else if (l > x + tileSpacing && l < y)
                from = Direction.NorthEast;
            else if (l < x && l > y + tileSpacing)
                from = Direction.SouthWest;
            else if (l > x + tileSpacing && l > y + tileSpacing)
                from = Direction.SouthEast;
            else if (l < x)
                from = Direction.West;
            else if (l > x + tileSpacing)
                from = Direction.East;
            else if (l < y)
                from = Direction.North;
            else if (l > y + tileSpacing)
                from = Direction.South;

            if (l < x)
            {
                if (!IsOpaque(1) || (IsOpaque(1) && IsOpaque(2)))
                    RenderOccluder(Direction.East, from, x, y, tileSpacing);

                if (l < y)
                {
                    if (!IsOpaque(2) || (IsOpaque(2) && !IsOpaque(0)))
                    {
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                    }
                    if (!IsOpaque(2))
                        RenderOccluder(Direction.West, from, x, y, tileSpacing);
                }
                else if (l > y + tileSpacing)
                {
                    if (!IsOpaque(0) || (IsOpaque(0) && !IsOpaque(2) &&
                         (l < x + tileSpacing && !IsOpaque(1))))
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                    if (IsOpaque(1) && IsOpaque(3))
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                }
                else if (l >= y && l <= y + tileSpacing)
                {
                    if (!IsOpaque(2) || (IsOpaque(2) && !IsOpaque(0)))
                    {
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                    }
                    if (!IsOpaque(2))
                        RenderOccluder(Direction.West, from, x, y, tileSpacing);

                    if (!IsOpaque(0) || (IsOpaque(0) && !IsOpaque(2)))
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                }
            }
            else if (l > x + tileSpacing)
            {
                if (!IsOpaque(3) || (IsOpaque(3) && IsOpaque(2)))
                    RenderOccluder(Direction.West, from, x, y, tileSpacing);

                if (l < y)
                {
                    if (!IsOpaque(2) || (IsOpaque(2) && !IsOpaque(0)))
                    {
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                    }
                    if (!IsOpaque(2))
                        RenderOccluder(Direction.East, from, x, y, tileSpacing);
                }
                else if (l > y + tileSpacing)
                {
                    if (!IsOpaque(0) ||
                        (IsOpaque(0) && !IsOpaque(2) &&
                         (l < x + tileSpacing && !IsOpaque(1))))
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                    if (IsOpaque(1))
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                }
                else if (l >= y && l <= y + tileSpacing)
                {
                    if (!IsOpaque(2) || (IsOpaque(2) && !IsOpaque(0)))
                    {
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                    }
                    if (!IsOpaque(2))
                        RenderOccluder(Direction.East, from, x, y, tileSpacing);
                    if (!IsOpaque(0) || (IsOpaque(0) && !IsOpaque(2)))
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                }
            }
            else if (l >= x && l <= x + tileSpacing)
            {
                if (!IsOpaque(1) || (IsOpaque(1) && IsOpaque(2)))
                    RenderOccluder(Direction.East, from, x, y, tileSpacing);
                if (!IsOpaque(3) || (IsOpaque(3) && IsOpaque(2)))
                    RenderOccluder(Direction.West, from, x, y, tileSpacing);

                if (l < y)
                {
                    if (!IsOpaque(2) || (IsOpaque(2) && !IsOpaque(0)))
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                }
                else if (l > y + tileSpacing)
                {
                    if (!IsOpaque(0) ||
                        (IsOpaque(0) && !IsOpaque(2) &&
                         (l < x + tileSpacing && !IsOpaque(1))))
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                    RenderOccluder(Direction.North, from, x, y, tileSpacing);
                }
                else if (l >= y && l <= y + tileSpacing)
                {
                    if (!IsOpaque(2) || (IsOpaque(2) && !IsOpaque(0)))
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                    if (!IsOpaque(0) || (IsOpaque(0) && !IsOpaque(2)))
                        RenderOccluder(Direction.North, from, x, y, tileSpacing);
                }
            }*/

        }

        private bool IsOpaque(int i)
        {
            /*if (surroundingTiles[i] != null && surroundingTiles[i].Opaque)
                return true;*/
            return false;
        }

        public override void RenderPosOffset(float x, float y, int tileSpacing, Vector2D lightPosition)
        {
            Vector2D lightVec = lightPosition - new Vector2D(x + tileSpacing/2.0f, y + tileSpacing/2.0f);
            lightVec.Normalize();
            lightVec *= 10;
            Sprite.Color = Color.Black;
            Sprite.SetPosition(x + lightVec.X, y + lightVec.Y);
            Sprite.BlendingMode = BlendingModes.Inverted;
            Sprite.DestinationBlend = AlphaBlendOperation.SourceAlpha;
            Sprite.SourceBlend = AlphaBlendOperation.One;
            Sprite.Draw();
            if (lightVec.X < 0)
                lightVec.X = -3;
            if (lightVec.X > 0)
                lightVec.X = 3;
            if (lightVec.Y < 0)
                lightVec.Y = -3;
            if (lightVec.Y > 0)
                lightVec.Y = 3;

            /*if (surroundingTiles[0] != null && IsOpaque(0) && lightVec.Y < 0) // tile to north
                lightVec.Y = 2;
            if (surroundingTiles[1] != null && IsOpaque(1) && lightVec.X > 0)
                lightVec.X = -2;
            if (surroundingTiles[2] != null && IsOpaque(2) && lightVec.Y > 0)
                lightVec.Y = -2;
            if (surroundingTiles[3] != null && IsOpaque(3) && lightVec.X < 0)
                lightVec.X = 2;*/

            Gorgon.CurrentRenderTarget.FilledRectangle(x + lightVec.X, y + lightVec.Y, Sprite.Width + 1,
                                                       Sprite.Height + 1, Color.FromArgb(0, Color.Transparent));
        }

        public override void DrawDecals(float xTopLeft, float yTopLeft, int tileSpacing, Batch decalBatch)
        {
            //d.Draw(xTopLeft, yTopLeft, tileSpacing, decalBatch);
        }

        public override void RenderTop(float xTopLeft, float yTopLeft, Batch wallTopsBatch)
        {
            int tileSpacing = mapMgr.GetTileSpacing();

            Vector2D SEpos = new Vector2D();

            if (_dir == Direction.East)
            {
                topSpriteNW.SetPosition((float)bounds.X - xTopLeft,
                        (float)bounds.Y - (Sprite.Height - bounds.Height) - yTopLeft);

                SEpos += topSpriteNW.Position + new Vector2D(tileSpacing / 2f, 0f);
            }
            else
            {
                topSpriteNW.SetPosition((float)bounds.X - xTopLeft,
                        (float)bounds.Y - (Sprite.Height - bounds.Height) - yTopLeft);

                SEpos += topSpriteNW.Position + new Vector2D(0f, topSpriteNW.Height);
            }

            topSpriteSE.SetPosition(SEpos.X, SEpos.Y);

            topSpriteNW.Color = Color.White;
            topSpriteSE.Color = Color.White;

            wallTopsBatch.AddClone(topSpriteNW);
            wallTopsBatch.AddClone(topSpriteSE);

        }
    }
}