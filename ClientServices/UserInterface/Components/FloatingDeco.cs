﻿using System;
using System.Drawing;
using ClientInterfaces;
using ClientInterfaces.Resource;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;

namespace ClientServices.UserInterface.Components
{
    class FloatingDeco : GuiComponent
    {
        private readonly IResourceManager _resourceManager;

        Sprite _drawSprite;

        public bool BounceRotate = false;             //Rotation inverts after hitting a certain angle?
        public float BounceRotateAngle = 0;           //Angle at which to change rotation direction.
        public Vector2D Velocity;                     //Direction and speed this is moving in.
        public float RotationSpeed = 0;               //Speed and direction at which this rotates.

        public Vector2D SpriteLocation; //Have to have a separate one because i made the ui compo pos a Point. Can't change to Vector2d unless i fix 235+ errors. Do this later.
        private float spriteRotation = 0;

        public FloatingDeco(IResourceManager resourceManager, string spriteName)
        {
            _resourceManager = resourceManager;
            _drawSprite = _resourceManager.GetSprite(spriteName);
            _drawSprite.Smoothing = Smoothing.Smooth;

            Update(0);
        }

        public override void Update(float frameTime)
        {
            SpriteLocation = new Vector2D(SpriteLocation.X + (Velocity.X * frameTime), SpriteLocation.Y + (Velocity.Y * frameTime));
            spriteRotation += RotationSpeed*frameTime;

            if (BounceRotate && Math.Abs(spriteRotation) > BounceRotateAngle)
                RotationSpeed = -RotationSpeed;

            ClientArea = new Rectangle(new Point((int)SpriteLocation.X, (int)SpriteLocation.Y), new Size((int)_drawSprite.Width, (int)_drawSprite.Height));

            //Outside screen. Does not respect rotation. FIX.
            if (ClientArea.X > Gorgon.Screen.Width)
                SpriteLocation = new Vector2D((0 - _drawSprite.Width), SpriteLocation.Y);
            else if (ClientArea.X < (0 - _drawSprite.Width))
                SpriteLocation = new Vector2D(Gorgon.Screen.Width, SpriteLocation.Y);

            if (ClientArea.Y > Gorgon.Screen.Height)
                SpriteLocation = new Vector2D(SpriteLocation.X, (0 - _drawSprite.Height));
            else if (ClientArea.Y < (0 - _drawSprite.Height))
                SpriteLocation = new Vector2D(SpriteLocation.X, Gorgon.Screen.Height);

            Position = new Point((int)SpriteLocation.X, (int)SpriteLocation.Y);
        }

        public override void Render()
        {
            _drawSprite.Rotation = spriteRotation;
            _drawSprite.Position = SpriteLocation;
            _drawSprite.Draw();
        }

        public override void Dispose()
        {
            _drawSprite = null;
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public override bool MouseDown(MouseInputEventArgs e)
        {
            return false;
        }

        public override bool MouseUp(MouseInputEventArgs e)
        {
            return false;
        }
    }
}
