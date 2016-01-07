﻿using SS14.Client.Interfaces.Resource;
using System;
using System.Drawing;
using SS14.Client.Graphics.Sprite;
using SFML.Window;
using SS14.Client.Graphics;
using SS14.Shared.Maths;
using SFML.Graphics;

namespace SS14.Client.Services.UserInterface.Components
{
    internal class BlueprintButton : GuiComponent
    {
        #region Delegates

        public delegate void BlueprintButtonPressHandler(BlueprintButton sender);

        #endregion

        private readonly IResourceManager _resourceManager;

        public string Compo1;
        public string Compo1Name;

        public string Compo2;
        public string Compo2Name;
        public TextSprite Label;

        public string Result;
        public string ResultName;

        private SFML.Graphics.Color _bgcol = SFML.Graphics.Color.Transparent;
        private Sprite _icon;

        public BlueprintButton(string c1, string c1N, string c2, string c2N, string res, string resname,
                               IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;

            Compo1 = c1;
            Compo1Name = c1N;

            Compo2 = c2;
            Compo2Name = c2N;

            Result = res;
            ResultName = resname;

            _icon = _resourceManager.GetSprite("blueprint");

            Label = new TextSprite("blueprinttext", "", _resourceManager.GetFont("CALIBRI"))
                        {
                            Color = new SFML.Graphics.Color(248, 248, 255),
                            ShadowColor = new SFML.Graphics.Color(105, 105, 105),
                            ShadowOffset = new Vector2(1, 1),
                            Shadowed = true
                        };

            Update(0);
        }

        public event BlueprintButtonPressHandler Clicked;

        public override void Update(float frameTime)
        {
            var bounds = _icon.GetLocalBounds();
            ClientArea = new Rectangle(Position,
                                       new Size((int) (Label.Width + bounds.Width),
                                                (int) Math.Max(Label.Height, bounds.Height)));
            Label.Position = new Point(Position.X + (int)bounds.Width, Position.Y);
            _icon.Position = new Vector2(Position.X, Position.Y + (Label.Height / 2f - bounds.Height / 2f));
            Label.Text = Compo1Name + " + " + Compo2Name + " = " + ResultName;
        }

        public override void Render()
        {
            if (_bgcol != SFML.Graphics.Color.Transparent)
            CluwneLib.drawRectangle(ClientArea.X, ClientArea.Y, ClientArea.Width,
                                                           ClientArea.Height, _bgcol);
            _icon.Draw();
            Label.Draw();
        }

        public override void Dispose()
        {
            Label = null;
            _icon = null;
            Clicked = null;
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public override bool MouseDown(MouseButtonEventArgs e)
        {
            if (ClientArea.Contains(new Point((int) e.X, (int) e.Y)))
            {
                if (Clicked != null) Clicked(this);
                return true;
            }
            return false;
        }

        public override bool MouseUp(MouseButtonEventArgs e)
        {
            return false;
        }

        public override void MouseMove(MouseMoveEventArgs e)
        {
            _bgcol = ClientArea.Contains(new Point((int) e.X, (int) e.Y))
                         ? new SFML.Graphics.Color(70, 130, 180)
                         : SFML.Graphics.Color.Transparent;
        }
    }
}