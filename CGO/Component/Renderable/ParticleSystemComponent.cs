﻿using System;
using System.Collections.Generic;
using System.Drawing;
using ClientInterfaces.GOC;
using ClientInterfaces.Resource;
using ClientWindow;
using GameObject;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Utilities;
using SS13.IoC;
using SS13_Shared;
using SS13_Shared.GO;
using SS13_Shared.GO.Component.Particles;

namespace CGO
{
    public class ParticleSystemComponent : Component, IRenderableComponent
    {
        #region Variables.
        private Random _rnd = new Random();										// Random number generator.
        private ParticleSystem _emitter;							// List of particle emitters.
        private RenderImage _particleImage;								// Particle image.
        private Sprite _particleSprite;									// Particle sprite.
        private PreciseTimer _timer = new PreciseTimer();						// Timer object.
        private Vector4D _particlesColorStart = Vector4D.UnitX;
        private Vector4D _particlesColorEnd = Vector4D.Zero;
        private bool _active = false;
        private int _particleRate = 1;

        public DrawDepth DrawDepth { get; set; }
        #endregion
        public ParticleSystemComponent()
        {
            Family = ComponentFamily.Particles;
            DrawDepth = DrawDepth.ItemsOnTables;
            _particleSprite = IoCManager.Resolve<IResourceManager>().GetSprite("");
            CreateEmitter(new Vector2D(0,0));
        }
        
        public override Type StateType
        {
            get { return typeof(ParticleSystemComponentState); }
        }

        public void OnMove(object sender, VectorEventArgs args)
        {
            var offset = new Vector2D(args.VectorTo.X, args.VectorTo.Y) -
                         new Vector2D(args.VectorFrom.X, args.VectorFrom.Y);
            _emitter.MoveEmitter(_emitter.EmitterPosition + offset);
        }
        
        public override void OnAdd(Entity owner)
        {
            base.OnAdd(owner);
            var transform = Owner.GetComponent<TransformComponent>(ComponentFamily.Transform);
            transform.OnMove += OnMove;
            /*
            _particleImage = new RenderImage("ParticleImage" + Owner.Uid, 64, 64, ImageBufferFormats.BufferRGB888A8);
            CreateParticle();
            
            _particleSprite = new Sprite("ParticleSprite" + Owner.Uid, _particleImage);
            _particleSprite.Axis = new Vector2D(32, 32);
             */
            _particleSprite = IoCManager.Resolve<IResourceManager>().GetSprite("star1");
            _emitter.ParticleSprite = _particleSprite;
        }

        public override void OnRemove()
        {
            var transform = Owner.GetComponent<TransformComponent>(ComponentFamily.Transform);
            transform.OnMove -= OnMove;
            _particleSprite.Image = null;
            _particleSprite = null;
            _particleImage.Dispose();
            _particleImage = null;
            _emitter = null;
            base.OnRemove();
        }

        public override void SetParameter(ComponentParameter parameter)
        {
            base.SetParameter(parameter);
            dynamic parameterValue;
            switch (parameter.MemberName)
            {
                case "drawdepth":
                    DrawDepth = ((DrawDepth)Enum.Parse(typeof(DrawDepth), parameter.GetValue<string>(), true));
                    break;
                case "colorStart":
                    parameterValue = parameter.GetValue<Vector4>();
                    _particlesColorStart = new Vector4D(parameterValue.X, parameterValue.Y, parameterValue.Z, parameterValue.W);
                    UpdateParticleColor();
                    break;
                case "colorEnd":
                    parameterValue = parameter.GetValue<Vector4>();
                    _particlesColorEnd = new Vector4D(parameterValue.X, parameterValue.Y, parameterValue.Z, parameterValue.W);
                    UpdateParticleColor();
                    break;
                case "particlesPerSecond":
                    _particleRate = parameter.GetValue<int>();
                    UpdateParticleRate();
                    break;
            }
        }

        public override ComponentReplyMessage RecieveMessage(object sender, ComponentMessageType type,
                                                             params object[] list)
        {
            ComponentReplyMessage reply = base.RecieveMessage(sender, type, list);

            if (sender == this) //Don't listen to our own messages!
                return ComponentReplyMessage.Empty;

            switch (type)
            {
                case ComponentMessageType.SetDrawDepth:
                    DrawDepth = (DrawDepth)list[0];
                    break;
            }

            return reply;
        }


        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            _emitter.Update(frameTime);
        }

        public virtual void Render(Vector2D topLeft, Vector2D bottomRight)
        {            
            var blend = Gorgon.CurrentRenderTarget.BlendingMode;
            Gorgon.CurrentRenderTarget.BlendingMode = BlendingModes.Additive;
            
            Vector2D renderPos =
                ClientWindowData.WorldToScreen(
                    Owner.GetComponent<TransformComponent>(ComponentFamily.Transform).Position);

            _emitter.Move(renderPos);
            _emitter.Render();
            Gorgon.CurrentRenderTarget.BlendingMode = blend;
        }

        private void UpdateParticleColor()
        {
            _emitter.ColorRange = new SS13_Shared.Utility.Range<Vector4D>(_particlesColorStart, _particlesColorEnd);
        }

        private void UpdateParticleRate()
        {
            _emitter.EmitRate = _particleRate;
        }

        private void UpdateActive()
        {
            _emitter.Emit = _active;
        }

        /// <summary>
        /// Function to create a particle.
        /// </summary>
        private void CreateParticle()
        {
            Color particleColor;

            _particleImage.Clear(Color.Transparent);
            _particleImage.BeginDrawing();
            for (int x = 64; x > 0; x--)
            {
                particleColor = Color.FromArgb(255 - ((x * 4) - 1), 255, 255, 255);
                _particleImage.FilledCircle(32.0f, 32.0f, x / 2, particleColor);
            }
            _particleImage.EndDrawing();
        }

        /// <summary>
        /// Function to create a particle emitter.
        /// </summary>
        /// <param name="position">The position of the emitter.</param>
        /// <returns>A new emitter.</returns>
        private void CreateEmitter(Vector2D position)
        {
            _emitter = null;

            _emitter = new ParticleSystem(_particleSprite, position);
            _emitter.ColorRange = new SS13_Shared.Utility.Range<Vector4D>(_particlesColorStart, _particlesColorEnd);
            _emitter.Emit = _active;
            _emitter.Lifetime = 10f;
            _emitter.LifetimeVariance = 2f;
            _emitter.SizeRange = new SS13_Shared.Utility.Range<float>(0.1f, 0.05f);
            _emitter.SizeVariance = 0.05f;
            _emitter.Acceleration = new Vector2D(0, 1.5f);
            _emitter.RadialVelocity = 10f;
            _emitter.RadialAcceleration = -1 * _emitter.RadialVelocity/(_emitter.Lifetime-2);
            _emitter.EmissionRadiusRange = new SS13_Shared.Utility.Range<float>(5, 20);
        }

        public float Bottom
        {
            get
            {
                return Owner.GetComponent<TransformComponent>(ComponentFamily.Transform).Position.Y +
                       (_particleSprite.Height / 2);
            }
        }

        public override void HandleComponentState(dynamic __state)
        {
            var state = (ParticleSystemComponentState) __state;
            if(state.Active != _active)
            {
                _active = state.Active;
                UpdateActive();
            }
            if (state.StartColor.X != _particlesColorStart.X
                || state.StartColor.Y != _particlesColorStart.Y
                || state.StartColor.Z != _particlesColorStart.Z
                || state.StartColor.W != _particlesColorStart.W)
            {
                _particlesColorStart = new Vector4D(state.StartColor.X, state.StartColor.Y,
                                                      state.StartColor.Z, state.StartColor.W);
                UpdateParticleColor();
            }
            if (state.EndColor.X != _particlesColorEnd.X
                || state.EndColor.Y != _particlesColorEnd.Y
                || state.EndColor.Z != _particlesColorEnd.Z
                || state.EndColor.W != _particlesColorEnd.W)
            {
                _particlesColorEnd = new Vector4D(state.EndColor.X, state.EndColor.Y,
                                                      state.EndColor.Z, state.EndColor.W);
                UpdateParticleColor();
            }
        }

    }
}