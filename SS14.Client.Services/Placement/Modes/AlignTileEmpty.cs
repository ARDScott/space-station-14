﻿using SS14.Shared.Maths;

using SS14.Client.GameObjects;
using SS14.Client.Interfaces.Map;
using SS14.Shared.GO;
using System.Drawing;
using SS14.Client.Graphics;

namespace SS14.Client.Services.Placement.Modes
{
    public class AlignTileEmpty : PlacementMode
    {
        public AlignTileEmpty(PlacementManager pMan)
            : base(pMan)
        {
        }

        public override bool Update(Vector2 mouseS, IMapManager currentMap)
        {
            if (currentMap == null) return false;

            spriteToDraw = GetDirectionalSprite(pManager.CurrentBaseSpriteKey);

            mouseScreen = mouseS;
            mouseWorld = CluwneLib.ScreenToWorld(mouseScreen);

            var bounds = spriteToDraw.GetLocalBounds();
            var spriteSize = CluwneLib.PixelToTile(new Vector2(bounds.Width, bounds.Height));
            var spriteRectWorld = new RectangleF(mouseWorld.X - (spriteSize.X / 2f),
                                                 mouseWorld.Y - (spriteSize.Y / 2f),
                                                 spriteSize.X, spriteSize.Y);

            currentTile = currentMap.GetTileRef(mouseWorld);

            if (currentTile.Tile.TileId != 0)
                return false;

            if (pManager.CurrentPermission.Range > 0)
                if (
                    (pManager.PlayerManager.ControlledEntity.GetComponent<TransformComponent>(ComponentFamily.Transform)
                         .Position - mouseWorld).Length > pManager.CurrentPermission.Range)
                    return false;

            if (pManager.CurrentPermission.IsTile)
            {
                mouseWorld = new Vector2(currentTile.X + 0.5f,
                                         currentTile.Y + 0.5f);
                mouseScreen = CluwneLib.WorldToScreen(mouseWorld);
            }
            else
            {
                mouseWorld = new Vector2(currentTile.X + 0.5f + pManager.CurrentTemplate.PlacementOffset.Key,
                                         currentTile.Y + 0.5f + pManager.CurrentTemplate.PlacementOffset.Value);
                mouseScreen = CluwneLib.WorldToScreen(mouseWorld);

                spriteRectWorld = new RectangleF(mouseWorld.X - (bounds.Width/2f),
                                                 mouseWorld.Y - (bounds.Height/2f), bounds.Width,
                                                 bounds.Height);
                if (pManager.CollisionManager.IsColliding(spriteRectWorld))
                    return false;
                //Since walls also have collisions, this means we can't place objects on walls with this mode.
            }

            return true;
        }

        public override void Render()
        {
            if (spriteToDraw != null)
            {
                var bounds = spriteToDraw.GetLocalBounds();
                spriteToDraw.Color = pManager.ValidPosition ? new SFML.Graphics.Color(34, 139, 34) : new SFML.Graphics.Color(205, 92, 92);
                spriteToDraw.Position = new Vector2(mouseScreen.X - (bounds.Width/2f),
                                                    mouseScreen.Y - (bounds.Height/2f));
                //Centering the sprite on the cursor.
                spriteToDraw.Draw();
                spriteToDraw.Color = SFML.Graphics.Color.White;
            }
        }
    }
}