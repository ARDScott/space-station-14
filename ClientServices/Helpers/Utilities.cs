﻿using System;
using System.Collections.Generic;
using System.Linq;
using ClientInterfaces.GOC;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using CGO;
using SS13_Shared;
using SS13_Shared.GO;
using System.Drawing;

namespace ClientServices.Helpers
{
    static class Utilities
    {
        public static string GetObjectSpriteName(Type type)
        {
            if (type.IsSubclassOf(typeof(ClientServices.Map.Tiles.Tile)))
            {
                return "tilebuildoverlay";
            }
            return "nosprite";
        }

        public static Sprite GetSpriteComponentSprite(IEntity entity)
        {
            var replies = new List<ComponentReplyMessage>();
            entity.SendMessage(entity, ComponentMessageType.GetSprite, replies, null);
            if (replies.Any(l => l.MessageType == ComponentMessageType.CurrentSprite))
            {
                var spriteMsg = replies.First(l => l.MessageType == ComponentMessageType.CurrentSprite);
                var sprite = (Sprite)spriteMsg.ParamsList[0];
                return sprite;
            }
            return null;
        }

        public static bool SpritePixelHit(Sprite toCheck, Vector2D clickPos)
        {
            var clickPoint = new PointF(clickPos.X, clickPos.Y);
            if (!toCheck.AABB.Contains(clickPoint)) return false;

            var spritePosition = new Point((int)clickPos.X - (int)toCheck.Position.X + (int)toCheck.ImageOffset.X, (int)clickPos.Y - (int)toCheck.Position.Y + (int)toCheck.ImageOffset.Y);

            var imgData = toCheck.Image.GetImageData();

            imgData.Lock(false);
            var pixColour = Color.FromArgb((int)(imgData[spritePosition.X, spritePosition.Y]));
            imgData.Dispose();
            imgData.Unlock();

            return pixColour.A != 0;
        } 
    }

    class ColorInterpolator
    {
        delegate byte ComponentSelector(Color color);
        static readonly ComponentSelector RedSelector = color => color.R;
        static readonly ComponentSelector GreenSelector = color => color.G;
        static readonly ComponentSelector BlueSelector = color => color.B;

        public static Color InterpolateBetween(
            Color endPoint1,
            Color endPoint2,
            double lambda)
        {
            if (lambda < 0 || lambda > 1)
            {
                throw new ArgumentOutOfRangeException("lambda");
            }
            Color color = Color.FromArgb(
                InterpolateComponent(endPoint1, endPoint2, lambda, RedSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, GreenSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, BlueSelector)
                );

            return color;
        }

        static byte InterpolateComponent(
            Color endPoint1,
            Color endPoint2,
            double lambda,
            ComponentSelector selector)
        {
            return (byte)(selector(endPoint1)
                          + (selector(endPoint2) - selector(endPoint1)) * lambda);
        }
    }
}