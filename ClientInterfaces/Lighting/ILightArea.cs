﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace ClientInterfaces.Lighting
{
    public interface ILightArea
    {
        RenderImage renderTarget { get; }
        Vector2D LightPosition { get; set; }
        Vector2D LightAreaSize { get; set; }
        bool Calculated { get; set; }
        Vector2D ToRelativePosition(Vector2D worldPosition);
        void BeginDrawingShadowCasters();
        void EndDrawingShadowCasters();
    }
    
}
