﻿using Lidgren.Network;
using SFML.System;
using SFML.Window;
using SS14.Client.Graphics;
using SS14.Client.Interfaces.Network;
using SS14.Client.Interfaces.Resource;
using SS14.Shared;

namespace SS14.Client.Services.UserInterface.Components
{
    internal class AdminPasswordDialog : Window
    {
        private readonly INetworkManager _networkManager;

        private readonly Button _okayButton;
        private readonly Textbox _textboxPassword;

        public AdminPasswordDialog(Vector2i size, INetworkManager networkManager, IResourceManager resourceManager)
            : base("Admin Login", size, resourceManager)
        {
            _networkManager = networkManager;

            _textboxPassword = new Textbox((int) (size.X/2f), _resourceManager);
            _okayButton = new Button("Submit", _resourceManager);
            _okayButton.Clicked += OkayButtonClicked;
            _okayButton.mouseOverColor = new SFML.Graphics.Color(135, 206, 250);
            _textboxPassword.OnSubmit += textboxPassword_OnSubmit;
            components.Add(_textboxPassword);
            components.Add(_okayButton);
            Position = new Vector2i((int) ( CluwneLib.CurrentRenderTarget.Size.X/2f) - (int) (ClientArea.Width/2f),
                                 (int) (CluwneLib.CurrentRenderTarget.Size.Y/2f) - (int) (ClientArea.Height/2f));
        }

        private void textboxPassword_OnSubmit(string text, Textbox sender)
        {
            if (text.Length > 1 && !string.IsNullOrWhiteSpace(text))
            {
                TryAdminLogin(text);
                _textboxPassword.Text = string.Empty;
            }
        }

        private void OkayButtonClicked(Button sender)
        {
            if (_textboxPassword.Text.Length <= 1 || string.IsNullOrWhiteSpace(_textboxPassword.Text)) return;

            TryAdminLogin(_textboxPassword.Text);
            _textboxPassword.Text = string.Empty;
        }

        public override void Update(float frameTime)
        {
            if (disposing || !IsVisible()) return;
            base.Update(frameTime);
            if (_okayButton == null || _textboxPassword == null) return;

            _okayButton.Position = new Vector2i((int) (Size.X/2f) - (int) (_okayButton.ClientArea.Width/2f),
                                             (Size.Y - _okayButton.ClientArea.Height - 5));
            _textboxPassword.Position = new Vector2i((int) (Size.X/2f) - (int) (_textboxPassword.ClientArea.Width/2f), 5);
        }

        private void TryAdminLogin(string password)
        {
            NetOutgoingMessage msg = _networkManager.CreateMessage();
            msg.Write((byte) NetMessage.RequestAdminLogin);
            msg.Write(password);

            _networkManager.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);

            Dispose();
        }

        public override void Render()
        {
            if (disposing || !IsVisible()) return;
            base.Render();
        }

        public override void Dispose()
        {
            if (disposing) return;
            base.Dispose();
        }

        public override bool MouseDown(MouseButtonEventArgs e)
        {
            if (disposing || !IsVisible()) return false;
            if (base.MouseDown(e)) return true;
            return false;
        }

        public override bool MouseUp(MouseButtonEventArgs e)
        {
            if (disposing || !IsVisible()) return false;
            if (base.MouseUp(e)) return true;
            return false;
        }

        public override void MouseMove(MouseMoveEventArgs e)
        {
            if (disposing || !IsVisible()) return;
            base.MouseMove(e);
        }

        public override bool MouseWheelMove(MouseWheelEventArgs e)
        {
            if (base.MouseWheelMove(e)) return true;
            return false;
        }

        public override bool KeyDown(KeyEventArgs e)
        {
            if (base.KeyDown(e)) return true;
            return false;
        }
    }
}