﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    [TestClass]
    public class TestShader_Test : SS14UnitTest
    {
        private IResourceManager resources;
        private RenderImage testRenderImage;
        private CluwneSprite testsprite;
        private Clock clock;
        private FrameEventArgs frameEvent;

        public TestShader_Test()
        {
            base.InitializeCluwneLib();
            clock = new Clock();
                      
            resources = base.GetResourceManager;
            testRenderImage = new RenderImage("TestShaders",1000,1000);
            testsprite = resources.GetSprite("ChatBubble");
         
        }    

        [TestMethod]
        public void LoadTestShader_ShouldDrawAllRed()
        {
           

           while (CluwneLib.IsRunning)
           {
               var lastFrameTime = clock.ElapsedTime.AsSeconds();
               clock.Restart();
               frameEvent = new FrameEventArgs(lastFrameTime);
               CluwneLib.ClearCurrentRendertarget(Color.Black);
               CluwneLib.Screen.DispatchEvents();
               

               testRenderImage.BeginDrawing();
               
               testsprite.SetPosition(0, 0);
               GLSLShader currshader = resources.GetShader("RedShader");
               currshader.SetParameter("TextureUnit0", GLSLShader.CurrentTexture);
               currshader.setAsCurrentShader();
               testsprite.Draw(); 
               testRenderImage.EndDrawing();
               currshader.ResetCurrentShader();
               testRenderImage.Blit(0,0, 1000,1000); 
           




               CluwneLib.Screen.Display();

           }
        }
            


    }
}
