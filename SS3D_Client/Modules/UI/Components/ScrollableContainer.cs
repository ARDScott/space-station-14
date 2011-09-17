﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;
using GorgonLibrary.GUI;

using Lidgren.Network;
using SS3D_shared;

namespace SS3D.Modules.UI.Components
{
    class ScrollableContainer : GuiComponent
    {
        Scrollbar scrollbarH;
        Scrollbar scrollbarV;

        RenderImage clippingRI;

        public List<GuiComponent> components = new List<GuiComponent>();

        float max_x = 0;
        float max_y = 0;

        Scrollbar focus_scrollbar;

        bool disposing = false;

        public ScrollableContainer(string uniqueName, Size size)
            : base()
        {
            clippingRI = new RenderImage(uniqueName, size.Width, size.Height, ImageBufferFormats.BufferRGB888A8);
            scrollbarH = new Scrollbar();
            scrollbarH.size = size.Width;
            scrollbarH.Horizontal = true;
            scrollbarV = new Scrollbar();
            scrollbarV.size = size.Height;
            clippingRI.SourceBlend = AlphaBlendOperation.SourceAlpha;
            clippingRI.DestinationBlend = AlphaBlendOperation.InverseSourceAlpha;
            Update();
        }

        public override void Update()
        {
            if (disposing || !IsVisible()) return;
            clientArea = new Rectangle(position, new Size(clippingRI.Width, clippingRI.Height));
            scrollbarH.Position = new Point(position.X, clientArea.Bottom);
            scrollbarV.Position = new Point(clientArea.Right, position.Y);
            max_x = 0;
            max_y = 0;

            foreach (GuiComponent component in components)
            {
                if (component.Position.X + component.ClientArea.Width > max_x) max_x = component.Position.X  + component.ClientArea.Width;
                if (component.Position.Y + component.ClientArea.Height > max_y) max_y = component.Position.Y  + component.ClientArea.Height;
            }

            scrollbarH.max = (int)max_x - clientArea.Width;
            if (max_x > clippingRI.Width) scrollbarH.SetVisible(true);
            else scrollbarH.SetVisible(false);

            scrollbarV.max = (int)max_y - clientArea.Height;
            if (max_y > clippingRI.Height) scrollbarV.SetVisible(true);
            else scrollbarV.SetVisible(false);

            scrollbarH.Update();
            scrollbarV.Update();
        }

        public override void Render()
        {
            if (disposing || !IsVisible()) return;
            clippingRI.Clear(System.Drawing.Color.Transparent);
            clippingRI.BeginDrawing();
            foreach (GuiComponent component in components)
            {
                Point oldPos = component.Position;
                component.Position = new Point(component.Position.X - (int)scrollbarH.Value, component.Position.Y - (int)scrollbarV.Value);
                component.Update(); //2 Updates per frame D:
                component.Render();
                component.Position = oldPos;
                component.Update();
            }
            clippingRI.EndDrawing();
            clippingRI.Blit(position.X, position.Y);
            scrollbarH.Render();
            scrollbarV.Render();
            Gorgon.Screen.Rectangle(clientArea.X, clientArea.Y, clientArea.Width, clientArea.Height, System.Drawing.Color.Black);
        }

        public override void Dispose()
        {
            if (disposing) return;
            disposing = true;
            clippingRI.Dispose();
            clippingRI = null;
            GC.SuppressFinalize(this);
        }

        private void setFocusScrollbar()
        {
            if (scrollbarH.IsVisible() && !scrollbarV.IsVisible())
                focus_scrollbar = scrollbarH;

            if (!scrollbarH.IsVisible() && scrollbarV.IsVisible())
                focus_scrollbar = scrollbarV;
        }

        public override bool MouseDown(MouseInputEventArgs e)
        {
            if (disposing || !IsVisible()) return false;
            if (scrollbarH.MouseDown(e))
            {
                focus_scrollbar = scrollbarH;
                return true;
            }
            if (scrollbarV.MouseDown(e))
            {
                focus_scrollbar = scrollbarV;
                return true;
            }

            MouseInputEventArgs modArgs = new MouseInputEventArgs
                (e.Buttons,
                e.ShiftButtons,
                new Vector2D(e.Position.X - position.X + scrollbarH.Value, e.Position.Y - position.Y + scrollbarV.Value),
                e.WheelPosition,
                e.RelativePosition,
                e.WheelDelta,
                e.ClickCount);

            foreach (GuiComponent component in components)
            {
                if (component.MouseDown(modArgs))
                {
                    setFocusScrollbar();
                    return true;
                }
            }

            if (clientArea.Contains(new Point((int)e.Position.X, (int)e.Position.Y)))
            {
                setFocusScrollbar();
                return true;
            }

            return false;
        }

        public override bool MouseUp(MouseInputEventArgs e)
        {
            if (disposing || !IsVisible()) return false;
            if (scrollbarH.MouseUp(e)) return true;
            if (scrollbarV.MouseUp(e)) return true;
            return false;
        }

        public override void MouseMove(MouseInputEventArgs e)
        {
            if (disposing || !IsVisible()) return;
            scrollbarH.MouseMove(e);
            scrollbarV.MouseMove(e);
            return;
        }

        public override bool MouseWheelMove(MouseInputEventArgs e)
        {
            if(focus_scrollbar != null) focus_scrollbar.Value += ((Math.Sign(e.WheelDelta) * -1) * Math.Max(((focus_scrollbar.max / 20)),1));
            return false;
        }

    }
}
