﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using Color = System.Drawing.Color;

namespace SS14.Client.Graphics.CluwneLib.Render
{
    public class RenderImage : RenderTexture
    {
        private string p1;
        private uint p2;
        private uint p3;
        private ImageBufferFormats imageBufferFormats;
        private string uniqueName;
        private int p4;
        private int p5;
        private string targetName;
     

        

        public RenderImage(uint width, uint height) : base(width, height)
        {
        }

        public RenderImage(uint width, uint height, bool depthBuffer) : base(width, height, depthBuffer)
        {
        }

     
        public void Blit(float x, float y, float width, float height, Color color, RenderStates state)
        {
            
        }

        public uint Width { get; set; }
        public uint Height { get; set; }

        public void Blit(int p1, int p2, uint p3, uint p4, System.Drawing.Color color, BlitterSizeMode blitterSizeMode)
        {
            throw new NotImplementedException();
        }
        public void Blit(int p1, int p2, uint p3, uint p4)
        {
            throw new NotImplementedException();
        }

        public void EndDrawing()
        {
            throw new NotImplementedException();
        }

        public void BeginDrawing()
        {
            throw new NotImplementedException();
        }

        public void Clear(Color Color)
        {
           
        }

        public string setName { get; set; }

        public ImageBufferFormats setImageBuffer { get; set; }
    }
}
