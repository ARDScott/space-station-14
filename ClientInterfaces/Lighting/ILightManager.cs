﻿using System.Collections.Generic;
using System.Drawing;
using ClientInterfaces.Map;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using SS13_Shared;

namespace ClientInterfaces.Lighting
{
    public interface ILightManager
    {
        void ApplyLightsToSprite(List<ILight> lights, Sprite sprite, Vector2D screenOffset);
        ILight CreateLight(IMapManager mapManager, Color color, int range, LightState lightState, Vector2D position);
    }
}
