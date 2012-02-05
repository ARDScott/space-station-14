﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ClientServices.Resources;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;
using GorgonLibrary.GUI;
using SS13.UserInterface;
using Lidgren.Network;
using SS13_Shared;

namespace SS13.UserInterface
{
    class Label : GuiComponent
    {
        public TextSprite Text { get; private set; }

        public delegate void LabelPressHandler(Label sender);
        public event LabelPressHandler Clicked;

        public bool drawBorder = false;
        public bool drawBackground = false;

        public System.Drawing.Color borderColor = System.Drawing.Color.Black;
        public System.Drawing.Color backgroundColor = System.Drawing.Color.Gray;

        public int fixed_width = -1;
        public int fixed_height = -1;

        public Label(string text)
            : base()
        {
            Text = new TextSprite("Label" + text, text, ResourceManager.GetFont("CALIBRI"));
            Text.Color = System.Drawing.Color.Black;
            Update();
        }

        public override void Update()
        {
            Text.Position = position;
            ClientArea = new Rectangle(this.position, new Size(fixed_width == -1 ? (int)Text.Width : fixed_width, fixed_height == -1 ? (int)Text.Height : fixed_height));
        }

        public override void Render()
        {
            if (drawBackground) Gorgon.Screen.FilledRectangle(ClientArea.X, ClientArea.Y, ClientArea.Width, ClientArea.Height, backgroundColor);
            if (drawBorder) Gorgon.Screen.Rectangle(ClientArea.X, ClientArea.Y, ClientArea.Width, ClientArea.Height, borderColor);
            Text.Draw();
        }

        public override void Dispose()
        {
            Text = null;
            Clicked = null;
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public override bool MouseDown(MouseInputEventArgs e)
        {
            if (clientArea.Contains(new Point((int)e.Position.X, (int)e.Position.Y)))
            {
                if (Clicked != null) Clicked(this);
                return true;
            }
            return false;
        }

        public override bool MouseUp(MouseInputEventArgs e)
        {
            return false;
        }
    }
}
