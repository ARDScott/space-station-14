﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary;
using SS13_Shared;
using SS13_Shared.GO;
using SS13_Shared.GO.Component.Mover;

namespace CGO
{
    /// <summary>
    /// Recieves movement data from the server and updates the entity's position accordingly.
    /// </summary>
    public class NetworkMoverComponent : GameObjectComponent
    {
        private Constants.MoveDirs movedir = Constants.MoveDirs.south;
        Vector2D targetPosition;
        Vector2D startPosition;
        private MoverComponentState previousState;
        private MoverComponentState lastState;
        bool interpolating = false;
        float movetime = 0.05f; // Milliseconds it should take to move.
        float movedtime = 0; // Amount of time we've been moving since the last update packet.

        public override ComponentFamily Family
        {
            get { return ComponentFamily.Mover; }
        }

        public override Type StateType
        {
            get { return typeof (MoverComponentState); }
        }

        public override void HandleNetworkMessage(IncomingEntityComponentMessage message)
        {
            /*var x = (float)message.MessageParameters[0];
            var y = (float)message.MessageParameters[1];
            Translate(x, y);*/
        }

        public override ComponentReplyMessage RecieveMessage(object sender, ComponentMessageType type, params object[] list)
        {
            var reply = base.RecieveMessage(sender, type, list);

            if (sender == this) //Don't listen to our own messages!
                return ComponentReplyMessage.Empty;

            switch (type)
            {
                case ComponentMessageType.GetMoveDir:
                    reply = new ComponentReplyMessage(ComponentMessageType.MoveDirection, movedir);
                    break;
            }

            return reply;
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            if (interpolating)
            {
                movedtime = movedtime + frameTime;
                Vector2D delta = targetPosition - Owner.Position;
                if (movedtime >= movetime)
                {
                    //targetPosition = Owner.Position;
                    Owner.Position = targetPosition;
                    //startPosition = Owner.Position;
                    startPosition = targetPosition;
                    interpolating = false;
                    //movedtime = 0;
                }
                else
                {
                    float X = Ease(movedtime, startPosition.X, targetPosition.X, movetime);
                    float Y = Ease(movedtime, startPosition.Y, targetPosition.Y, movetime);
                    Owner.Position = new Vector2D(X, Y);
                }

                Owner.Moved();
            }
        }
        
        private void Translate(float x, float y, float velx, float vely)
        {
            Vector2D delta = new Vector2D(x, y) - Owner.Position;
            interpolating = true;
            movedtime = 0;
            
            //Owner.Position = new Vector2D(x, y);
            targetPosition = new Vector2D(x, y);
            startPosition = Owner.Position;

            if (delta.X > 0 && delta.Y > 0)
                SetMoveDir(Constants.MoveDirs.southeast);
            if (delta.X > 0 && delta.Y < 0)
                SetMoveDir(Constants.MoveDirs.northeast);
            if (delta.X < 0 && delta.Y > 0)
                SetMoveDir(Constants.MoveDirs.southwest);
            if (delta.X < 0 && delta.Y < 0)
                SetMoveDir(Constants.MoveDirs.northwest);
            if (delta.X > 0 && delta.Y == 0)
                SetMoveDir(Constants.MoveDirs.east);
            if (delta.X < 0 && delta.Y == 0)
                SetMoveDir(Constants.MoveDirs.west);
            if (delta.Y > 0 && delta.X == 0)
                SetMoveDir(Constants.MoveDirs.south);
            if (delta.Y < 0 && delta.X == 0)
                SetMoveDir(Constants.MoveDirs.north);
        }

        /// <summary>
        /// Returns a float position eased from a start position to an end position.
        /// </summary>
        /// <param name="time">elapsed time since the start of the easing</param>
        /// <param name="start">start position</param>
        /// <param name="end">end position</param>
        /// <param name="duration">duration of the movement</param>
        /// <returns>current position</returns>
        private float Ease(float time, float start, float end, float duration = 1) // duration is in ms.
        {
            time = time / duration;// - 1;
            //return (float)(end * (Math.Pow(time, 5) + 1) * Math.Sign(end - start) + start);
            return time * (end - start) + start;
        }

        private void SetMoveDir(Constants.MoveDirs _movedir)
        {
            if (_movedir != movedir)
            {
                movedir = _movedir;
                Owner.SendMessage(this, ComponentMessageType.MoveDirection, movedir);
            }
        }

        public override void HandleComponentState(dynamic state)
        {
            SetNewState(state);
        }

        private void SetNewState(MoverComponentState state)
        {
            if (lastState != null)
                previousState = lastState;
            lastState = state;
            Translate(state.X, state.Y, state.VelocityX, state.VelocityY);
        }
    }
}
