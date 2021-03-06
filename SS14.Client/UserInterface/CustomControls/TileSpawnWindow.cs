﻿using System;
using System.Linq;
using SS14.Client.Graphics;
using SS14.Client.Graphics.Input;
using SS14.Client.Interfaces.Placement;
using SS14.Client.UserInterface.Controls;
using SS14.Shared.Enums;
using SS14.Shared.Interfaces.Map;
using SS14.Shared.IoC;
using SS14.Shared.Maths;

namespace SS14.Client.UserInterface.CustomControls
{
    internal class TileSpawnWindow : Window
    {
        private readonly Label _clearLabel;
        private readonly IPlacementManager _placementManager;
        private readonly ScrollableContainer _tileList;

        public TileSpawnWindow(Vector2i size)
            : base("Tile Spawn Panel", size)
        {
            _placementManager = IoCManager.Resolve<IPlacementManager>();

            _tileList = new ScrollableContainer(new Vector2i(200, 400));
            _tileList.LocalPosition = new Vector2i(5, 5);
            _tileList.BorderColor = Color.Black;
            _tileList.BackgroundColor = Color.White;
            Container.AddControl(_tileList);

            var searchLabel = new Label("Tile Search:", "CALIBRI");
            searchLabel.LocalPosition = new Vector2i(210, 0);
            Container.AddControl(searchLabel);

            var tileSearchTextbox = new Textbox(125);
            tileSearchTextbox.LocalPosition = new Vector2i(210, 20);
            tileSearchTextbox.OnSubmit += tileSearchTextbox_OnSubmit;
            Container.AddControl(tileSearchTextbox);

            //TODO: This needs to be a button.
            _clearLabel = new Label("[Clear Filter]", "CALIBRI");
            _clearLabel.BackgroundColor = Color.Gray;
            _clearLabel.DrawBackground = true;
            _clearLabel.DrawBorder = true;
            _clearLabel.LocalPosition = new Vector2i(210, 55);
            _clearLabel.Clicked += ClearLabelClicked;
            Container.AddControl(_clearLabel);

            BuildTileList();
            
            _placementManager.PlacementCanceled += PlacementManagerPlacementCanceled;
        }

        protected override void OnCalcPosition()
        {
            base.OnCalcPosition();
            
            _screenPos = new Vector2i((int)(CluwneLib.CurrentRenderTarget.Size.X / 2f) - (int)(ClientArea.Width / 2f),
                (int)(CluwneLib.CurrentRenderTarget.Size.Y / 2f) - (int)(ClientArea.Height / 2f));
        }
        
        public override void Destroy()
        {
            if (Disposing) return;
            _placementManager.PlacementCanceled -= PlacementManagerPlacementCanceled;
            _tileList.Destroy();
            base.Destroy();
        }
        
        public override bool KeyDown(KeyEventArgs e)
        {
            if (base.KeyDown(e))
                return true;

            if (e.Key == Keyboard.Key.Escape)
            {
                Destroy();
                return true;
            }
            return false;
        }

        private void ClearLabelClicked(Label sender, MouseButtonEventArgs e)
        {
            _clearLabel.BackgroundColor = Color.Gray;
            BuildTileList();
        }

        private void tileSearchTextbox_OnSubmit(Textbox sender, string text)
        {
            BuildTileList(text);
        }

        private void PlacementManagerPlacementCanceled(object sender, EventArgs e)
        {
            foreach (Label curr in _tileList.Container.Children.Where(curr => curr is Label))
            {
                curr.BackgroundColor = Color.Gray;
            }
        }

        private void BuildTileList(string searchStr = null)
        {
            _tileList.Container.DisposeAllChildren();
            _tileList.ResetScrollbars();

            var tileDefs = IoCManager.Resolve<ITileDefinitionManager>().Select(td => td.Name);

            if (!string.IsNullOrEmpty(searchStr))
            {
                tileDefs = tileDefs.Where(s => s.IndexOf(searchStr, StringComparison.InvariantCultureIgnoreCase) >= 0);
                _clearLabel.BackgroundColor = new Color(211, 211, 211, 255);
            }

            var maxWidth = 0;
            Control lastControl = _tileList.Container;
            foreach (var entry in tileDefs)
            {
                var tileLabel = new Label(entry, "CALIBRI");
                tileLabel.Parent = lastControl;

                if (!(lastControl is Label)) // if first loop
                    tileLabel.LocalPosition = new Vector2i(5, 0);
                else
                    tileLabel.LocalPosition = new Vector2i(0, 5);

                lastControl = tileLabel;
                tileLabel.Alignment = ControlAlignments.Bottom;

                tileLabel.BackgroundColor = Color.Gray;
                tileLabel.DrawBackground = true;
                tileLabel.DrawBorder = true;
                tileLabel.Update(0);
                tileLabel.DoLayout();
                tileLabel.Clicked += TileLabelClicked;

                if (tileLabel.ClientArea.Width > maxWidth)
                    maxWidth = tileLabel.ClientArea.Width;
            }

            foreach (var curr in _tileList.Container.Children.Where(curr => curr.GetType() == typeof(Label)))
            {
                ((Label) curr).FixedWidth = maxWidth;
            }
        }

        private void TileLabelClicked(Label sender, MouseButtonEventArgs e)
        {
            foreach (var curr in _tileList.Container.Children.Where(curr => curr.GetType() == typeof(Label)))
            {
                ((Label) curr).BackgroundColor = Color.Gray;
            }

            var newObjInfo = new PlacementInformation
            {
                PlacementOption = "AlignTileAny",
                TileType = IoCManager.Resolve<ITileDefinitionManager>()[sender.Text].TileId,
                Range = 400,
                IsTile = true
            };

            _placementManager.BeginPlacing(newObjInfo);

            sender.BackgroundColor = new Color(34, 139, 34, 255);
        }
    }
}
