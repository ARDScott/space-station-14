﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ClientInterfaces;
using ClientInterfaces.Lighting;
using ClientInterfaces.Map;
using ClientServices.Map.Tiles;
using GorgonLibrary;
using SS13_Shared;
using SS3D.LightTest;

namespace ClientServices.Lighting
{
    public class Light : ILight
    {
        public Vector2D Position { get; private set; }
        public Color Color { get; private set; }
        public int Radius { get; private set; }
        public ILightArea LightArea { get; private set; }

        public Light()
        {
            Radius = 256;
        }

        public void Move(Vector2D toPosition)
        {
            Position = toPosition;
            LightArea.Calculated = false;
        }

        public void SetRadius(int radius)
        {
            if (Radius != radius)
            {
                Radius = radius;
                LightArea = (ILightArea)new LightArea(RadiusToShadowMapSize(Radius));
            }
        }

        public void SetColor(int a, int r, int g, int b)
        {
            Color = Color.FromArgb(a, r, g, b);
        }

        public void SetColor(Color color)
        {
            Color = color;
        }

        public static ShadowmapSize RadiusToShadowMapSize(int Radius)
        {
            switch (Radius)
            {
                case 128:
                    return ShadowmapSize.Size128;
                case 256:
                    return ShadowmapSize.Size256;
                case 512:
                    return ShadowmapSize.Size512;
                case 1024:
                    return ShadowmapSize.Size1024;
                default:
                    return ShadowmapSize.Size1024;
            }
        }

    }
}
