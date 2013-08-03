﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using ClientInterfaces.Resource;
using ClientInterfaces.UserInterface;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;
using SS13.IoC;

namespace ClientServices.UserInterface.Components
{
    internal class Showcase : GuiComponent
    {
        //TODO Make selection repond to mousewheel
        public delegate void ShowcaseSelectionChangedHandler(ImageButton sender, Object associatedData);

        private readonly IResourceManager _resourceManager;
        private ImageButton _buttonLeft;
        private ImageButton _buttonRight;

        private List<KeyValuePair<ImageButton, Object>>  _items = new List<KeyValuePair<ImageButton, object>>();
        private int _selected = 0;

        public int DisplayAmount = 2; //Number of additional visible columns beside the selection. 1 = 3 total visible. selection + 1 left + 1 right.
        public int ItemSpacing = 10;  //Additional space between items.

        public Size Size = new Size(300, 100);

        public string ButtonRight
        {
            get
            {
                if (_buttonRight != null) return _buttonRight.ImageNormal;
                else return "";
            }
            set { _buttonRight.ImageNormal = value; }
        }

        public string ButtonLeft
        {
            get
            {
                if (_buttonLeft != null) return _buttonLeft.ImageNormal;
                else return "";
            }
            set { _buttonLeft.ImageNormal = value; }
        }

        public event ShowcaseSelectionChangedHandler SelectionChanged;

        private SimpleImage _selectionGlow;

        public Showcase()
        {
            _resourceManager = IoCManager.Resolve<IResourceManager>();

            _buttonLeft = new ImageButton();
            _buttonLeft.Clicked += new ImageButton.ImageButtonPressHandler(_buttonLeft_Clicked);

            _buttonRight = new ImageButton();
            _buttonRight.Clicked += new ImageButton.ImageButtonPressHandler(_buttonRight_Clicked);

            _selectionGlow = new SimpleImage() //TODO add way to change this.
                {
                    Sprite = "job_glow"
                };

            Update(0);
        }

        void _buttonRight_Clicked(ImageButton sender)
        {
            if (_selected + 1 <= _items.Count - 1) _selected++;
        }

        void _buttonLeft_Clicked(ImageButton sender)
        {
            if (_selected - 1 >= 0) _selected--;
        }

        public void AddItem(ImageButton button, Object associatedData)
        {
            if (button == null || associatedData == null) return;

            if (!_items.Exists(x => x.Key == button || x.Value == associatedData))
            {
                _items.Add(new KeyValuePair<ImageButton, object>(button, associatedData));
                button.Clicked += new ImageButton.ImageButtonPressHandler(button_Clicked);
            }

            _selected = (int)Math.Floor(_items.Count / 2f); //start in the middle. cosmetic thing only.
        }

        void button_Clicked(ImageButton sender)
        {
            if (_items.Exists(x => x.Key == sender))
            {
                var sel = _items.Find(x => x.Key == sender);
                _selected = _items.IndexOf(sel);
            }
        }

        public void ClearItems()
        {
            _items.Clear();
        }

        public void RemoveItem(Object toRemove)
        {
            if (toRemove is ImageButton)
                _items.RemoveAll(x => x.Key == toRemove);
            else
                _items.RemoveAll(x => x.Value == toRemove);
        }

        public override sealed void Update(float frameTime)
        {
            ClientArea = new Rectangle(Position, Size);

            _buttonLeft.Position = new Point(ClientArea.Left,
                                                ClientArea.Top +
                                                (int) (ClientArea.Height/2f - _buttonLeft.ClientArea.Height/2f));
            _buttonLeft.Update(frameTime);

            _buttonRight.Position = new Point(ClientArea.Right - _buttonRight.ClientArea.Width,
                                                ClientArea.Top +
                                                (int) (ClientArea.Height/2f - _buttonRight.ClientArea.Height/2f));
            _buttonRight.Update(frameTime);

            foreach (var curr in _items)
            {
                curr.Key.Update(frameTime);
            }

            _selectionGlow.Update(frameTime);
        }

        public override void Render()
        {
            if (_items.Count > 0)
            {
                if (_selected < 0 || _selected > _items.Count - 1)
                    _selected = 0;
                else
                {
                    if (_selectionGlow != null)
                    {
                        _selectionGlow.Position = new Point(ClientArea.Left + (int)(ClientArea.Width / 2f - _selectionGlow.ClientArea.Width / 2f), ClientArea.Top + (int)(ClientArea.Height / 2f - _selectionGlow.ClientArea.Height / 2f));
                        _selectionGlow.Render();
                    }

                    KeyValuePair<ImageButton, Object> selected = _items[_selected];
                    selected.Key.Position = new Point(ClientArea.Left + (int)(ClientArea.Width / 2f - selected.Key.ClientArea.Width / 2f), ClientArea.Top + (int)(ClientArea.Height / 2f - selected.Key.ClientArea.Height / 2f));
                    selected.Key.Render();

                    int lastPosLeft = selected.Key.ClientArea.Left - ItemSpacing;
                    int lastPosRight = selected.Key.ClientArea.Right + ItemSpacing;

                    for (int i = 1; i <= DisplayAmount; i++)
                    {
                        //Left
                        if ((_selected - i) >= 0 && (_selected - i) <= _items.Count - 1)
                        {
                            KeyValuePair<ImageButton, Object> selectedLeft = _items[(_selected - i)];
                            selectedLeft.Key.Position = new Point(lastPosLeft - selectedLeft.Key.ClientArea.Width, ClientArea.Top + (int)(ClientArea.Height / 2f - selectedLeft.Key.ClientArea.Height / 2f));
                            lastPosLeft = selectedLeft.Key.ClientArea.Left - ItemSpacing;
                            selectedLeft.Key.Render();
                        }

                        //Right
                        if ((_selected + i) >= 0 && (_selected + i) <= _items.Count - 1)
                        {
                            KeyValuePair<ImageButton, Object> selectedRight = _items[(_selected + i)];
                            selectedRight.Key.Position = new Point(lastPosRight, ClientArea.Top + (int)(ClientArea.Height / 2f - selectedRight.Key.ClientArea.Height / 2f));
                            lastPosRight = selectedRight.Key.ClientArea.Right + ItemSpacing;
                            selectedRight.Key.Render();
                        }
                    }
                }
            }

            _buttonLeft.Render();
            _buttonRight.Render();

            //Gorgon.CurrentRenderTarget.Rectangle(ClientArea.X, ClientArea.Y, ClientArea.Width, ClientArea.Height, System.Drawing.Color.DarkOrange);
        }

        public override void Dispose()
        {
            _buttonRight = null;
            _buttonLeft = null;
            SelectionChanged = null;
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public override void MouseMove(MouseInputEventArgs e)
        {
            if (ClientArea.Contains(new Point((int)e.Position.X, (int)e.Position.Y)))
            {
                _buttonLeft.MouseMove(e);
                _buttonRight.MouseMove(e);

                foreach (var curr in _items)
                {
                    curr.Key.MouseMove(e);
                }
            }
        }

        public override bool MouseDown(MouseInputEventArgs e)
        {
            if (ClientArea.Contains(new Point((int) e.Position.X, (int) e.Position.Y)))
            {
                if(_buttonLeft.MouseDown(e)) return true;
                if (_buttonRight.MouseDown(e)) return true;

                if (_items.Count > 0)
                {
                    if (_selected >= 0 || _selected <= _items.Count - 1)
                    {
                        KeyValuePair<ImageButton, Object> selected = _items[_selected];
                        if (selected.Key.MouseDown(e)) return true;

                        for (int i = 1; i <= DisplayAmount; i++)
                        {
                            if ((_selected - i) >= 0 && (_selected - i) <= _items.Count - 1)
                            {
                                KeyValuePair<ImageButton, Object> selectedLeft = _items[(_selected - i)];
                                if (selectedLeft.Key.MouseDown(e)) return true;
                            }

                            if ((_selected + i) >= 0 && (_selected + i) <= _items.Count - 1)
                            {
                                KeyValuePair<ImageButton, Object> selectedRight = _items[(_selected + i)];
                                if (selectedRight.Key.MouseDown(e)) return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public override bool MouseUp(MouseInputEventArgs e)
        {
            if (ClientArea.Contains(new Point((int)e.Position.X, (int)e.Position.Y)))
            {
                if (_buttonLeft.MouseUp(e)) return true;
                if (_buttonRight.MouseUp(e)) return true;

                if (_items.Count > 0)
                {
                    if (_selected >= 0 || _selected <= _items.Count - 1)
                    {
                        KeyValuePair<ImageButton, Object> selected = _items[_selected];
                        if (selected.Key.MouseUp(e)) return true;

                        for (int i = 1; i <= DisplayAmount; i++)
                        {
                            if ((_selected - i) >= 0 && (_selected - i) <= _items.Count - 1)
                            {
                                KeyValuePair<ImageButton, Object> selectedLeft = _items[(_selected - i)];
                                if (selectedLeft.Key.MouseUp(e)) return true;
                            }

                            if ((_selected + i) >= 0 && (_selected + i) <= _items.Count - 1)
                            {
                                KeyValuePair<ImageButton, Object> selectedRight = _items[(_selected + i)];
                                if (selectedRight.Key.MouseUp(e)) return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}