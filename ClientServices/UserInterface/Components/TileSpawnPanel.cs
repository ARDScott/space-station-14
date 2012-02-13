﻿using System;
using System.Linq;
using System.Drawing;
using ClientInterfaces;
using ClientInterfaces.Placement;
using ClientInterfaces.Resource;
using GorgonLibrary;
using GorgonLibrary.InputDevices;
using SS13_Shared;

namespace ClientServices.UserInterface.Components
{
    class TileSpawnPanel : Window
    {
        private readonly IResourceManager _resourceManager;
        private readonly IPlacementManager _placementManager;
        private readonly ScrollableContainer _tileList;
        private readonly Label _clearLabel;
        private readonly Textbox _tileSearchTextbox;

        public TileSpawnPanel(Size size, IResourceManager resourceManager, IPlacementManager placementManager)
            : base("Tile Spawn Panel", size, resourceManager)
        {
            _resourceManager = resourceManager;
            _placementManager = placementManager;

            _tileList = new ScrollableContainer("tilespawnlist", new Size(200, 400), _resourceManager) { Position = new Point(5, 5) };
            components.Add(_tileList);

            var searchLabel = new Label("Tile Search:", "CALIBRI", _resourceManager) { Position = new Point(210, 0) };
            components.Add(searchLabel);

            _tileSearchTextbox = new Textbox(125, _resourceManager) { Position = new Point(210, 20) };
            _tileSearchTextbox.OnSubmit += tileSearchTextbox_OnSubmit;
            components.Add(_tileSearchTextbox);

            _clearLabel = new Label("[Clear Filter]", "CALIBRI", _resourceManager)
                              {
                                  DrawBackground = true,
                                  DrawBorder = true,
                                  Position = new Point(210, 55)
                              };

            _clearLabel.Clicked += ClearLabelClicked;
            _clearLabel.BackgroundColor = Color.Gray;
            components.Add(_clearLabel);

            BuildTileList();

            Position = new Point((int)(Gorgon.CurrentRenderTarget.Width / 2f) - (int)(ClientArea.Width / 2f), (int)(Gorgon.CurrentRenderTarget.Height / 2f) - (int)(ClientArea.Height / 2f));
            _placementManager.PlacementCanceled += PlacementManagerPlacementCanceled;
        }

        void ClearLabelClicked(Label sender)
        {
            _clearLabel.BackgroundColor = Color.Gray;
            BuildTileList();
        }

        void tileSearchTextbox_OnSubmit(string text)
        {
            BuildTileList(text);
        }

        void PlacementManagerPlacementCanceled(object sender, EventArgs e)
        {
            foreach (var curr in _tileList.components.Where(curr => curr.GetType() == typeof(Label)))
                ((Label)curr).BackgroundColor = Color.Gray;
        }

        private void BuildTileList(string searchStr = null)
        {
            var maxWidth = 0;
            var yOffset = 5;

            _tileList.components.Clear();
            _tileList.ResetScrollbars();

            var typeNames = (searchStr == null) ?
                Enum.GetNames(typeof(TileType)).Where(x => x.ToLower() != "none").ToList() :
                Enum.GetNames(typeof(TileType)).Where(x => x.ToLower().Contains(searchStr.ToLower()) && x.ToLower() != "none").ToList();
        
            if (searchStr != null) _clearLabel.BackgroundColor = Color.LightGray;

            foreach (string entry in typeNames)
            {
                var tileLabel = new Label(entry, "CALIBRI", _resourceManager);
                _tileList.components.Add(tileLabel);
                tileLabel.Position = new Point(5, yOffset);
                tileLabel.DrawBackground = true;
                tileLabel.DrawBorder = true;
                tileLabel.Update();
                yOffset += 5 + tileLabel.ClientArea.Height;
                tileLabel.Clicked += TileLabelClicked;
                if (tileLabel.ClientArea.Width > maxWidth) maxWidth = tileLabel.ClientArea.Width;
            }

            foreach (var curr in _tileList.components.Where(curr => curr.GetType() == typeof(Label)))
                ((Label)curr).FixedWidth = maxWidth;
        }

        void TileLabelClicked(Label sender)
        {
            foreach (var curr in _tileList.components.Where(curr => curr.GetType() == typeof(Label)))
                ((Label)curr).BackgroundColor = Color.Gray;

            var newObjInfo = new PlacementInformation
                                 {
                                     PlacementOption = PlacementOption.AlignTileAnyFree,
                                     TileType = (TileType) Enum.Parse(typeof (TileType), sender.Text.Text, true),
                                     Range = 400,
                                     IsTile = true
                                 };

            _placementManager.BeginPlacing(newObjInfo);

            sender.BackgroundColor = Color.ForestGreen;
        }

        public override void Update()
        {
            if (disposing || !IsVisible()) return;
            base.Update();
        }

        public override void Render()
        {
            if (disposing || !IsVisible()) return;
            base.Render();
        }

        public override void Dispose()
        {
            if (disposing) return;
            _placementManager.PlacementCanceled -= PlacementManagerPlacementCanceled;
            base.Dispose();
            _tileList.Dispose();
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