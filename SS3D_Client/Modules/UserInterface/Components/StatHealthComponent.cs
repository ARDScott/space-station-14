﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;
using SS13.HelperClasses;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;
using SS13.Modules;
using Lidgren.Network;
using CGO;
using SS13_Shared.GO;
using SS13_Shared;
using ClientResourceManager;

namespace SS13.UserInterface
{
    public class StatHealthComponent : GuiComponent
    {
        public override Point Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
            }
        }

        private Sprite backgroundSprite;
        private Sprite healthDetail;

        private Label healthText;

        private Color backgroundColor;
        private float health;
        public Point size;
        private float step;
        private Vector2D blipPosition;
        private float blipXRelative;
        private float blipHeight;
        private float blipWidth;
        private float blipStart;

        public StatHealthComponent(PlayerController _playerController, Point _size)
            : base(_playerController)
        {
            //componentClass = SS13_Shared.GuiComponentType.???;
            backgroundSprite = ResMgr.Singleton.GetSprite("1pxwhite");
            healthDetail = ResMgr.Singleton.GetSprite("stat_health_detail");
            healthText = new Label("Healthy");
            healthText.Text.Color = Color.DarkBlue;
            size = _size;
            health = 100;
            SetBackgroundColor();
            blipPosition = new Vector2D(Position.X, Position.Y + ((size.Y / 3) * 2));
            step = size.X / 100;
            blipXRelative = 0;
            blipHeight = size.Y / 1.2f;
            blipWidth = size.X / 4;
            blipStart = size.X / 3;
            healthDetail.Position = new Vector2D(Position.X, Position.Y);
        }

        private void SetBackgroundColor()
        {
            int red = 255 - (int)Math.Round(health * 2.5f);
            int green = (int)Math.Round(health * 2.5f);
            int blue = 50;

            backgroundColor = Color.FromArgb(red, green, blue);
        }

        private void UpdateBlip()
        {
            if (health > 40)
            {
                step = Math.Max(size.X / 80, size.X / (health));
            }
            else
            {
                step = size.X / 40;
            }

            blipXRelative += step;

            if (blipXRelative > size.X)
                blipXRelative = 0;
            float blipTemp = 0;

            while (blipTemp <= blipXRelative)
            {
                blipPosition.X = Position.X + blipTemp;

                if (health > 0)
                {
                    if (blipTemp > blipStart + blipWidth || blipTemp < blipStart)
                    {
                        blipPosition.Y = Position.Y + ((size.Y / 3) * 2);
                    }
                    else if (blipTemp < blipStart + blipWidth / 4 || blipTemp > blipStart + ((blipWidth / 4) * 3))
                    {
                        blipPosition.Y--;
                    }
                    else
                    {
                        blipPosition.Y++;
                    }
                }

                if (blipTemp == blipXRelative)
                    backgroundSprite.Color = Color.White;
                else
                    backgroundSprite.Color = Color.Cyan;

                backgroundSprite.Opacity = 130;
                backgroundSprite.Draw(new Rectangle((int)blipPosition.X, (int)blipPosition.Y, 2, 2));

                blipTemp += step;
            }
        }

        public override void Update()
        {

        }

        private void DoText()
        {
            var entity = (Entity)playerController.ControlledEntity;

            float healthPct = 0;
            healthText.Text.Text = "???";

            if (entity.HasComponent(ComponentFamily.Damageable))
            {
                HealthComponent comp = (HealthComponent)entity.GetComponent(ComponentFamily.Damageable);
                health = comp.GetHealth();

                healthPct = comp.GetHealth() / comp.GetMaxHealth();

                if (healthPct > 0.75) healthText.Text.Text = "HEALTHY: " + health.ToString();
                else if (healthPct > 0.50) healthText.Text.Text = "INJURED: " + health.ToString();
                else if (healthPct > 0.25) healthText.Text.Text = "WOUNDED: " + health.ToString();
                else if (healthPct > 0) healthText.Text.Text = "CRITICAL: " + health.ToString();
                else healthText.Text.Text = "DECEASED: " + health.ToString();
            }

            healthText.Position = Position;

            healthText.Text.Color = Color.Blue;

            healthText.Update();
            healthText.Render();
        }

        public override void Render()
        {
            backgroundSprite.Color = backgroundColor;
            backgroundSprite.Opacity = 125;

            backgroundSprite.Draw(new Rectangle(Position, new Size(size.X, size.Y) ) );
            
            healthDetail.Position = new Vector2D(Position.X, Position.Y);
            healthDetail.Draw();

            DoText();

            UpdateBlip();
        }
    }
}
