﻿using System;
using NUnit.Framework;
using SS14.Client.Interfaces.Resource;
using SS14.Client.Graphics.Render;
using SS14.Client.Graphics.Sprite;
using Color = System.Drawing.Color;
using SS14.Client.Graphics.Event;
using SS14.Client.Graphics;
using System.Drawing;
using SFML.System;
using SFML.Graphics;
using SS14.Client.Graphics.Shader;

namespace SS14.UnitTesting.SS14.Client.Graphics.Shaders
{
    [TestFixture]
    public class TestShader_Test : SS14UnitTest
    {
        private IResourceManager resources;
        private RenderImage testRenderImage;
        private CluwneSprite testsprite;
    
        public TestShader_Test()
        {
            base.InitializeCluwneLib(1280,720,false,60);
                     
            resources = base.GetResourceManager;
            testRenderImage = new RenderImage("TestShaders",1280,720);
            testsprite = resources.GetSprite("flashlight_mask");

            SS14UnitTest.InjectedMethod += LoadTestShader_ShouldDrawAllRed;

            base.StartCluwneLibLoop();
            
        }    

        [Test]
        public void LoadTestShader_ShouldDrawAllRed()
        {

            testRenderImage.BeginDrawing();

           
            GLSLShader currshader = resources.GetShader("RedShader");
            currshader.SetParameter("TextureUnit0", testsprite.Texture);
            currshader.setAsCurrentShader();
            testsprite.Draw();
            testRenderImage.EndDrawing();
            currshader.ResetCurrentShader();
            testRenderImage.Blit(0, 0, 1280, 720, Color.White, BlitterSizeMode.Crop);

            resources.GetSprite("flashlight_mask").Draw();
           
           
        }
            


    }
}
