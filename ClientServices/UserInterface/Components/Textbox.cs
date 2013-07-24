﻿using System;
using System.Drawing;
using ClientInterfaces;
using ClientInterfaces.Resource;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;
using System.Diagnostics;

namespace ClientServices.UserInterface.Components
{
    class Textbox : GuiComponent
    {
        private readonly IResourceManager _resourceManager;

        private Sprite _textboxMain;
        private Sprite _textboxLeft;
        private Sprite _textboxRight;

        public TextSprite Label;

        public delegate void TextSubmitHandler(string text, Textbox sender);
        public event TextSubmitHandler OnSubmit;

        private Rectangle _clientAreaMain;
        private Rectangle _clientAreaLeft;
        private Rectangle _clientAreaRight;

        private int _caretIndex = 0;
        private int _displayIndex = 0;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                SetVisibleText();
            }
        }

        private string _text = "";
        private string _displayText = "";

        private Vector2D _caretPos;
        private const int _caretShrink = 2;

        public bool ClearOnSubmit = true;
        public bool ClearFocusOnSubmit = true;
        public int MaxCharacters = 255;
        public int Width;

        public Textbox(int width, IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
            _textboxLeft = _resourceManager.GetSprite("text_left");
            _textboxMain = _resourceManager.GetSprite("text_middle");
            _textboxRight = _resourceManager.GetSprite("text_right");

            Width = width;

            Label = new TextSprite("Textbox", "", _resourceManager.GetFont("CALIBRI")) {Color = Color.Black};

            Update(0);
        }

        public override void Update(float frameTime)
        {

            _clientAreaLeft = new Rectangle(Position, new Size((int)_textboxLeft.Width, (int)_textboxLeft.Height));
            _clientAreaMain = new Rectangle(new Point(_clientAreaLeft.Right, Position.Y), new Size(Width, (int)_textboxMain.Height));
            _clientAreaRight = new Rectangle(new Point(_clientAreaMain.Right, Position.Y), new Size((int)_textboxRight.Width, (int)_textboxRight.Height));
            ClientArea = new Rectangle(Position, new Size(_clientAreaLeft.Width + _clientAreaMain.Width + _clientAreaRight.Width, Math.Max(Math.Max(_clientAreaLeft.Height,_clientAreaRight.Height), _clientAreaMain.Height)));
            Label.Position = new Point(_clientAreaLeft.Right, Position.Y + (int)(ClientArea.Height / 2f) - (int)(Label.Height / 2f));

        }

        public override void Render()
        {
            _textboxLeft.Draw(_clientAreaLeft);
            _textboxMain.Draw(_clientAreaMain);
            _textboxRight.Draw(_clientAreaRight);

            Gorgon.CurrentRenderTarget.FilledRectangle(_caretPos.X, _caretPos.Y, 1, Label.Height - (_caretShrink * 2), Color.HotPink);

            Label.Text = _displayText;
            Label.Draw();


            Gorgon.CurrentRenderTarget.Rectangle(Label.Position.X, Label.Position.Y, Label.Width, Label.Height, Color.DarkRed);
        }

        public override void Dispose()
        {
            Label = null;
            _textboxLeft = null;
            _textboxMain = null;
            _textboxRight = null;
            OnSubmit = null;
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public override bool MouseDown(MouseInputEventArgs e)
        {
            if (ClientArea.Contains(new Point((int) e.Position.X, (int) e.Position.Y)))
            {
                return true; 
            }

            return false;
        }

        public override bool MouseUp(MouseInputEventArgs e)
        {
            return false;
        }

        public override bool KeyDown(KeyboardInputEventArgs e)
        {
            if (!Focus) return false;

            if (e.Key == KeyboardKeys.Left)
            {
                if (_caretIndex > 0) _caretIndex--;
                SetVisibleText();
                return true;
            }
            else if (e.Key == KeyboardKeys.Right)
            {
                if (_caretIndex < _text.Length) _caretIndex++;
                SetVisibleText();
                return true;
            }

            if (e.Key == KeyboardKeys.Return && Text.Length >= 1)
            {
                Submit();
                return true;
            }

            if (e.Key == KeyboardKeys.Back && Text.Length >= 1)
            {
                Text = Text.Remove(_caretIndex - 1, 1);
                if (_caretIndex > 0) _caretIndex--;
                SetVisibleText();
                return true;
            }

            if (char.IsLetterOrDigit(e.CharacterMapping.Character) || char.IsPunctuation(e.CharacterMapping.Character) || char.IsWhiteSpace(e.CharacterMapping.Character))
            {
                if (Text.Length == MaxCharacters) return false;
                if (e.Shift)
                {
                    Text = Text.Insert(_caretIndex, e.CharacterMapping.Shifted.ToString());
                    if (_caretIndex < _text.Length) _caretIndex++;
                    SetVisibleText();
                }
                else
                {
                    Text = Text.Insert(_caretIndex, e.CharacterMapping.Character.ToString());
                    if (_caretIndex < _text.Length) _caretIndex++;
                    SetVisibleText();
                }
                return true;
            }
            return false;
        }

        private void SetVisibleText() 
        {
            _displayText = "";

            if (Label.MeasureLine(_text) >= _clientAreaMain.Width) //Text wider than box.
            {
                if (_caretIndex < _displayIndex) //Caret outside to the left. Move display text to the left by setting its index to the caret.
                    _displayIndex = _caretIndex;

                int glyphCount = 0;

                while (_displayIndex + (glyphCount + 1) < _text.Length && Label.MeasureLine(Text.Substring(_displayIndex + 1, glyphCount + 1)) < _clientAreaMain.Width)
                    glyphCount++; //How many glyphs we could/would draw with the current index.

                if (_caretIndex > _displayIndex + glyphCount) //Caret outside?
                {
                    if (_text.Substring(_displayIndex + 1).Length != glyphCount) //Still stuff outside the screen?
                    {
                        _displayIndex++; //Increase display index by one since the carret is one outside to the right. But only if there's still letters to the right.

                        glyphCount = 0;  //Update glyphcount with new index.

                        while (_displayIndex + (glyphCount + 1) < _text.Length && Label.MeasureLine(Text.Substring(_displayIndex + 1, glyphCount + 1)) < _clientAreaMain.Width)
                            glyphCount++;
                    }
            }
                _displayText = Text.Substring(_displayIndex + 1, glyphCount);

                string str1 = Text.Substring(_displayIndex, _caretIndex - _displayIndex);
                float carretx = Label.MeasureLine(str1);

                _caretPos.X = Label.Position.X + carretx;                   //caret still misaligned.
                _caretPos.Y = Label.Position.Y + _caretShrink;
            }
            else //Text fits completely inside box.
            {
                _displayIndex = 0;
                _displayText = Text;

                string str1 = Text.Substring(_displayIndex, _caretIndex);
                float carretx = Label.MeasureLine(str1);

                _caretPos.X = Label.Position.X + carretx;                   //caret still misaligned.
                _caretPos.Y = Label.Position.Y + _caretShrink;
            }
        }

        private void Submit()
        {
            if (OnSubmit != null) OnSubmit(Text, this);
            if (ClearOnSubmit)
            {
                Text = string.Empty;
                _displayText = string.Empty;
            }
            if (ClearFocusOnSubmit) Focus = false;
        }
    }
}
