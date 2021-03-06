﻿using System;
using SS14.Client.Graphics.Input;
using SS14.Client.Graphics.VertexData;
using SS14.Shared.Maths;

namespace SS14.Client.UserInterface.Controls
{
    internal class Window : ScrollableContainer
    {
        protected const int titleBuffer = 1;

        public Color TitleColor1 { get; set; } = new Color(112, 128, 144);
        public Color TitleColor2 { get; set; } = new Color(47, 79, 79);
        public bool CloseButtonVisible { get; set; } = true;

        protected ImageButton closeButton;
        protected bool dragging;
        protected Vector2 draggingOffset;
        protected GradientBox gradient;
        protected Label title;
        protected Box2i titleArea;
                
        public Window(string windowTitle, Vector2i size)
            : base(size)
        {
            closeButton = new ImageButton
            {
                ImageNormal = "closewindow"
            };

            closeButton.Clicked += CloseButtonClicked;
            title = new Label(windowTitle, "CALIBRI");
            gradient = new GradientBox();

            BackgroundColor = new Color(169, 169, 169, 255);
            BorderColor = Color.Black;
            DrawBackground = true;
            DrawBorder = true;
            BorderWidth = 1;

            Update(0);
        }

        public override void DoLayout()
        {
            base.DoLayout();
        }

        public override void Update(float frameTime)
        {
            if (Disposing || !Visible) return;
            base.Update(frameTime);
            if (title == null || gradient == null) return;

            var y_pos = ClientArea.Top - 2 * titleBuffer - title.ClientArea.Height + 1;
            title.LocalPosition = Position + new Vector2i(ClientArea.Left + 3, y_pos + titleBuffer);
            titleArea = Box2i.FromDimensions(ClientArea.Left, y_pos, ClientArea.Width, title.ClientArea.Height + 2 * titleBuffer);
            title.DoLayout();
            title.Update(frameTime);

            closeButton.LocalPosition = Position + new Vector2i(titleArea.Right - 5 - closeButton.ClientArea.Width,
                                            titleArea.Top + (int) (titleArea.Height / 2f) -
                                            (int) (closeButton.ClientArea.Height / 2f));
            closeButton.DoLayout();
            gradient.ClientArea = titleArea;
            gradient.Color1 = TitleColor1;
            gradient.Color2 = TitleColor2;
            gradient.Update(frameTime);
            closeButton.Update(frameTime);
        }

        public override void Draw()
        {
            if (Disposing || !Visible) return;
            gradient.Draw();

            //TODO RenderTargetRectangle
            base.Draw();
            title.Draw();
            if (CloseButtonVisible) closeButton.Draw();
        }

        public override void Destroy()
        {
            if (Disposing) return;
            base.Destroy();
        }

        public override bool MouseDown(MouseButtonEventArgs e)
        {
            if (Disposing || !Visible) return false;

            if (closeButton.MouseDown(e)) return true;

            if (base.MouseDown(e)) return true;

            if (titleArea.Translated(Position).Contains(e.X, e.Y))
            {
                draggingOffset = new Vector2(e.X, e.Y) - Position;
                dragging = true;
                return true;
            }

            return false;
        }

        public override bool MouseUp(MouseButtonEventArgs e)
        {
            if (dragging) dragging = false;
            if (Disposing || !Visible) return false;
            if (base.MouseUp(e)) return true;

            return false;
        }

        public override void MouseMove(MouseMoveEventArgs e)
        {
            if (Disposing || !Visible) return;
            if (dragging)
                Position = new Vector2i(e.X - (int) draggingOffset.X,
                    e.Y - (int) draggingOffset.Y);
            base.MouseMove(e);
        }

        public override bool MouseWheelMove(MouseWheelScrollEventArgs e)
        {
            if (base.MouseWheelMove(e)) return true;
            return false;
        }

        public override bool KeyDown(KeyEventArgs e)
        {
            if (base.KeyDown(e)) return true;
            return false;
        }

        protected virtual void CloseButtonClicked(ImageButton sender)
        {
            Destroy();
        }
    }

    public class GradientBox : Control
    {
        private  VertexTypeList.PositionDiffuse2DTexture1[] box =
            new VertexTypeList.PositionDiffuse2DTexture1[4];

        public Color Color1 { get; set;} = new Color(112, 128, 144);
        public Color Color2 { get; set;} = new Color(47, 79, 79);

        public bool Vertical { get; set;} = true;

        public override void Update(float frameTime)
        {
            Vector3 position = box[0].Position;
            position.X = ClientArea.Left;
            position.Y = ClientArea.Top;
            box[0].Position = position;
            box[0].TextureCoordinates = Vector2.Zero;
            box[0].Color = Color1;

            position = box[1].Position;
            position.X = ClientArea.Right;
            position.Y = ClientArea.Top;
            box[1].Position = position;
            box[1].TextureCoordinates = Vector2.Zero;
            if (!Vertical) box[1].Color = Color2;
            else box[1].Color = Color1;

            position = box[2].Position;
            position.X = ClientArea.Right;
            position.Y = ClientArea.Bottom;
            box[2].Position = position;
            box[2].TextureCoordinates = Vector2.Zero;
            box[2].Color = Color2;

            position = box[3].Position;
            position.X = ClientArea.Left;
            position.Y = ClientArea.Bottom;
            box[3].Position = position;
            box[3].TextureCoordinates = Vector2.Zero;
            if (!Vertical) box[3].Color = Color1;
            else box[3].Color = Color2;
        }

        protected override void OnCalcRect()
        {
            throw new NotImplementedException();
        }

        public override void Draw()
        {
            //TODO Window Render
        }
    }
}
