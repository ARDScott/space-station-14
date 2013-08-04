﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using ClientInterfaces;
using ClientInterfaces.GOC;
using ClientInterfaces.Placement;
using ClientInterfaces.Resource;
using ClientServices.Placement;
using GameObject;
using GorgonLibrary;
using GorgonLibrary.InputDevices;
using SS13.IoC;
using SS13_Shared;
using EntityManager = CGO.EntityManager;

namespace ClientServices.UserInterface.Components
{
    class EntitySpawnPanel : Window
    {
        private readonly IResourceManager _resourceManager;
        private readonly IPlacementManager _placementManager;

        private readonly ScrollableContainer _entityList;
        private readonly Label _clearLabel;
        private readonly Label _overLabel;
        private readonly Textbox _entSearchTextbox;
        private readonly ImageButton _eraserButton;
        private readonly Listbox _lstOverride;

        public EntitySpawnPanel(Size size, IResourceManager resourceManager, IPlacementManager placementManager)
            : base("Entity Spawn Panel", size, resourceManager)
        {
            _resourceManager = resourceManager;
            _placementManager = placementManager;

            _entityList = new ScrollableContainer("entspawnlist", new Size(200, 400), _resourceManager) {Position = new Point(5, 5)};
            components.Add(_entityList);

            var searchLabel = new Label("Entity Search:", "CALIBRI", _resourceManager) { Position = new Point(210, 0) };
            components.Add(searchLabel);

            _entSearchTextbox = new Textbox(125, _resourceManager) {Position = new Point(210, 20)};
            _entSearchTextbox.OnSubmit += entSearchTextbox_OnSubmit;
            components.Add(_entSearchTextbox);

            _clearLabel = new Label("[Clear Filter]", "CALIBRI", _resourceManager)
                             {
                                 DrawBackground = true,
                                 DrawBorder = true,
                                 Position = new Point(210, 55)
                             };

            _overLabel = new Label("Override Placement:", "CALIBRI", _resourceManager)
            {
                Position = _clearLabel.Position + new Size(0, _clearLabel.ClientArea.Height + 15)
            };

            components.Add(_overLabel);

            List<string> initOpts = new List<string>();

            initOpts.AddRange(new string[]
                {   "None", 
                    "AlignNone", 
                    "AlignFree", 
                    "AlignSimilar", 
                    "AlignTileAny", 
                    "AlignTileEmpty",
                    "AlignTileNonSolid",
                    "AlignTileSolid",
                    "AlignWall",
                    "AlignWallTops"
                });

            _lstOverride = new Listbox(150, 125, resourceManager, initOpts);
            _lstOverride.SelectItem("None");
            _lstOverride.ItemSelected += new Listbox.ListboxPressHandler(_lstOverride_ItemSelected);
            _lstOverride.Position = _overLabel.Position + new Size(0, _overLabel.ClientArea.Height);
            components.Add(_lstOverride);

            _clearLabel.Clicked += ClearLabelClicked;
            _clearLabel.BackgroundColor = Color.Gray;
            components.Add(_clearLabel);

            _eraserButton = new ImageButton()
                               {
                                   ImageNormal = "erasericon",
                                   Position = new Point(_clearLabel.Position.X + _clearLabel.ClientArea.Width + 5, _clearLabel.Position.Y)
                               };

            //eraserButton.Position = new Point(clearLabel.ClientArea.Right + 5, clearLabel.ClientArea.Top); Clientarea not updating properly. FIX THIS
            _eraserButton.Clicked += EraserButtonClicked;
            components.Add(_eraserButton);

            BuildEntityList();

            Position = new Point((int)(Gorgon.CurrentRenderTarget.Width / 2f) - (int)(ClientArea.Width / 2f), (int)(Gorgon.CurrentRenderTarget.Height / 2f) - (int)(ClientArea.Height / 2f));
            _placementManager.PlacementCanceled += PlacementManagerPlacementCanceled;
        }

        void _lstOverride_ItemSelected(Label item, Listbox sender)
        {
            PlacementManager pMan = (PlacementManager) _placementManager;

            if (pMan.CurrentMode != null)
            {
                var newObjInfo = new PlacementInformation
                {
                    PlacementOption = item.Text.Text,
                    EntityType = pMan.CurrentPermission.EntityType,
                    Range = -1,
                    IsTile = pMan.CurrentPermission.IsTile
                };

                _placementManager.Clear();
                _placementManager.BeginPlacing(newObjInfo);
            }
        }

        void EraserButtonClicked(ImageButton sender)
        {
            _placementManager.ToggleEraser();
        }

        void ClearLabelClicked(Label sender, MouseInputEventArgs e)
        {
            _clearLabel.BackgroundColor = Color.Gray;
            BuildEntityList();
        }

        void entSearchTextbox_OnSubmit(string text, Textbox sender)
        {
            BuildEntityList(text);
        }

        void PlacementManagerPlacementCanceled(object sender, EventArgs e)
        {
            foreach (var curr in _entityList.components.Where(curr => curr.GetType() == typeof(EntitySpawnSelectButton)))
                ((EntitySpawnSelectButton)curr).selected = false;
        }

        private void BuildEntityList(string searchStr = null)
        {
            var maxWidth = 0;
            var yOffset = 5;

            _entityList.components.Clear();
            _entityList.ResetScrollbars();

            var templates = (searchStr == null) ?
                IoCManager.Resolve<IEntityManagerContainer>().EntityManager.EntityTemplateDatabase.Templates.ToList() :
                IoCManager.Resolve<IEntityManagerContainer>().EntityManager.EntityTemplateDatabase.Templates.Where(x => x.Value.Name.ToLower().Contains(searchStr.ToLower())).ToList();
        

            if (searchStr != null) _clearLabel.BackgroundColor = Color.LightGray;

            foreach (var newButton in templates.Select(entry => new EntitySpawnSelectButton(entry.Value, entry.Key, _resourceManager)))
            {
                _entityList.components.Add(newButton);
                newButton.Position = new Point(5, yOffset);
                newButton.Update(0);
                yOffset += 5 + newButton.ClientArea.Height;
                newButton.Clicked += NewButtonClicked;

                if (newButton.ClientArea.Width > maxWidth) maxWidth = newButton.ClientArea.Width;
            }

            foreach (var curr in _entityList.components.Where(curr => curr.GetType() == typeof(EntitySpawnSelectButton)))
                ((EntitySpawnSelectButton)curr).fixed_width = maxWidth;
        }

        void NewButtonClicked(EntitySpawnSelectButton sender, EntityTemplate template, string templateName)
        {
            if (sender.selected)
            {
                sender.selected = false;
                _placementManager.Clear();
                return;
            }

            foreach (var curr in _entityList.components.Where(curr => curr.GetType() == typeof(EntitySpawnSelectButton)))
                ((EntitySpawnSelectButton)curr).selected = false;

            var overrideMode = "";
            if (_lstOverride.CurrentlySelected != null)
                if(_lstOverride.CurrentlySelected.Text.Text != "None")
                    overrideMode = _lstOverride.CurrentlySelected.Text.Text;

            var newObjInfo = new PlacementInformation
                                 {
                                     PlacementOption = overrideMode.Length > 0 ? overrideMode : template.PlacementMode,
                                     EntityType = templateName,
                                     Range = -1,
                                     IsTile = false
                                 };

            _placementManager.BeginPlacing(newObjInfo);

            sender.selected = true; //This needs to be last.
        }

        public override void Update(float frameTime)
        {
            if (disposing || !IsVisible()) return;
            base.Update(frameTime);
        }

        public override void Render()
        {
            if (disposing || !IsVisible()) return;
            _eraserButton.Color = _placementManager.Eraser ? Color.Tomato : Color.White;
            base.Render();
        }

        public override void Dispose()
        {
            if (disposing) return;
            _placementManager.PlacementCanceled -= PlacementManagerPlacementCanceled;
            base.Dispose();
            _entityList.Dispose();
        }

        public override bool MouseDown(MouseInputEventArgs e)
        {
            if (disposing || !IsVisible()) return false;
            if (base.MouseDown(e)) return true;
            return false;
        }

        public override bool MouseUp(MouseInputEventArgs e)
        {
            if (disposing || !IsVisible()) return false;
            if (base.MouseUp(e)) return true;
            return false;
        }

        public override void MouseMove(MouseInputEventArgs e)
        {
            if (disposing || !IsVisible()) return;
            base.MouseMove(e);
        }

        public override bool MouseWheelMove(MouseInputEventArgs e)
        {
            if (_entityList.MouseWheelMove(e)) return true;
            if (base.MouseWheelMove(e)) return true;
            return false;
        }

        public override bool KeyDown(KeyboardInputEventArgs e)
        {
            if (base.KeyDown(e)) return true;
            return false;
        }
    }
}