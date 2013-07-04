﻿using System.Collections.Generic;
using System.Linq;
using SS13_Shared;
using SS13_Shared.GO;
using GorgonLibrary;

namespace CGO
{
    //Moves an entity based on key binding input
    public class KeyBindingMoverComponent : GameObjectComponent
    {
        private const float BaseMoveSpeed = 300f;
        private const float FastMoveSpeed = 500f;
        private float _moveTimeCache = 0;
        private const float MoveRateLimit = .06666f; // 15 movements allowed to be sent to the server per second.

        private float _currentMoveSpeed;

        private bool _moveUp;
        private bool _moveDown;
        private bool _moveLeft;
        private bool _moveRight;

        private Constants.MoveDirs _movedir;

        public override ComponentFamily Family
        {
            get { return ComponentFamily.Mover; }
        }

        public KeyBindingMoverComponent()
        {
            _currentMoveSpeed = BaseMoveSpeed;
            _movedir = Constants.MoveDirs.south;
        }

        public override void HandleNetworkMessage(IncomingEntityComponentMessage message)
        {
            /*var x = (double)message.MessageParameters[0];
            var y = (double)message.MessageParameters[1];
            if((bool)message.MessageParameters[2]) //"forced" parameter -- if true forces position update
            PlainTranslate((float)x, (float)y);*/
        }

        public override ComponentReplyMessage RecieveMessage(object sender, ComponentMessageType type, params object[] list)
        {
            var reply = base.RecieveMessage(sender, type, list);

            if (sender == this) //Don't listen to our own messages!
                return ComponentReplyMessage.Empty;
            switch (type)
            {
                case ComponentMessageType.BoundKeyChange:
                    HandleKeyChange(list);
                    break;
                case ComponentMessageType.GetMoveDir:
                    reply = new ComponentReplyMessage(ComponentMessageType.MoveDirection, _movedir);
                    break;
            }
            return reply;
        }

        /// <summary>
        /// Handles a changed keystate message
        /// </summary>
        /// <param name="list">0 - Function, 1 - Key State</param>
        private void HandleKeyChange(params object[] list)
        {
            var function = (BoundKeyFunctions)list[0];
            var state = (BoundKeyState)list[1];
            var setting = state == BoundKeyState.Down;

            if(state == BoundKeyState.Up)
                SendPositionUpdate(); // Send a position update so that the server knows what position the client ended at.

            if (function == BoundKeyFunctions.MoveDown)
                _moveDown = setting;
            if (function == BoundKeyFunctions.MoveUp)
                _moveUp = setting;
            if (function == BoundKeyFunctions.MoveLeft)
                _moveLeft = setting;
            if (function == BoundKeyFunctions.MoveRight)
                _moveRight = setting;
            if (function == BoundKeyFunctions.Run)
            {
                _currentMoveSpeed = setting ? FastMoveSpeed : BaseMoveSpeed;
            }
        }

        /// <summary>
        /// Update function. Processes currently pressed keys and does shit etc.
        /// </summary>
        /// <param name="frameTime"></param>
        public override void Update(float frameTime)
        {
            _moveTimeCache += frameTime;

            base.Update(frameTime);
            
            if (_moveUp && !_moveLeft && !_moveRight && !_moveDown) // Move Up
            {
                Translate(new Vector2D(0, -1) * _currentMoveSpeed * frameTime);
            }
            else if (_moveDown && !_moveLeft && !_moveRight && !_moveUp) // Move Down
            {
                Translate(new Vector2D(0, 1) * _currentMoveSpeed * frameTime);
            }
            else if (_moveLeft && !_moveRight && !_moveUp && !_moveDown) // Move Left
            {
                Translate(new Vector2D(-1, 0) * _currentMoveSpeed * frameTime);
            }
            else if (_moveRight && !_moveLeft && !_moveUp && !_moveDown) // Move Right
            {
                Translate(new Vector2D(1, 0) * _currentMoveSpeed * frameTime);
            }
            else if (_moveUp && _moveRight && !_moveLeft && !_moveDown) // Move Up & Right
            {
                Translate(new Vector2D(0.7071f, -0.7071f) * _currentMoveSpeed * frameTime);
            }
            else if (_moveUp && _moveLeft && !_moveRight && !_moveDown) // Move Up & Left
            {
                Translate(new Vector2D(-0.7071f, -0.7071f) * _currentMoveSpeed * frameTime);
            }
            else if (_moveDown && _moveRight && !_moveLeft && !_moveUp) // Move Down & Right
            {
                Translate(new Vector2D(0.7071f, 0.7071f) * _currentMoveSpeed * frameTime);
            }
            else if (_moveDown && _moveLeft && !_moveRight && !_moveUp) // Move Down & Left
            {
                Translate(new Vector2D(-0.7071f, 0.7071f) * _currentMoveSpeed * frameTime);
            }
        }

        private void SetMoveDir(Constants.MoveDirs movedir)
        {
            if (movedir == _movedir) return;

            _movedir = movedir;
            Owner.SendMessage(this, ComponentMessageType.MoveDirection, _movedir);
        }

        public virtual void SendPositionUpdate()
        {
            Owner.SendComponentNetworkMessage(this, Lidgren.Network.NetDeliveryMethod.ReliableUnordered, Owner.Position.X, Owner.Position.Y);
        }

        public void PlainTranslate(float x, float y)
        {
            var delta = new Vector2D(x, y) - Owner.Position;

            Owner.Position = new Vector2D(x, y);
            
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

            Owner.Moved();
        }

        /// <summary>
        /// Moves the entity and sends an update packet to the serverside mover component.
        /// </summary>
        /// <param name="translationVector"></param>
        public virtual void Translate(Vector2D translationVector)
        {
            var oldPos = Owner.Position;

            var translated = TryTranslate(translationVector, false); //Only bump once...
            if (!translated)
                translated = TryTranslate(new Vector2D(translationVector.X, 0), true);
            if (!translated)
                translated = TryTranslate(new Vector2D(0, translationVector.Y), true);
            if (translated)
            {
                var delta = Owner.Position - oldPos;
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


                if (_moveTimeCache >= MoveRateLimit)
                {
                    SendPositionUpdate();

                    _moveTimeCache = 0;
                }
            }
            Owner.Moved();
        }

        /// <summary>
        /// Tries to move the entity. Checks collision. If the entity _is_ colliding, move it back and return false.
        /// </summary>
        /// <param name="translationVector"></param>
        /// <param name="suppressBump"></param>
        /// <returns></returns>
        public bool TryTranslate(Vector2D translationVector, bool suppressBump)
        {
            var oldPosition = Owner.Position;
            Owner.Position += translationVector; // We move the sprite here rather than the position, as we can then use its updated AABB values.
            //Check collision.
            var reply = Owner.SendMessage(this, ComponentFamily.Collider, ComponentMessageType.CheckCollision, false);
            if (reply.MessageType == ComponentMessageType.CollisionStatus)
            {
                var colliding = (bool)reply.ParamsList[0];
                if (colliding) //Collided, reset position and return false.
                {
                    Owner.Position = oldPosition;
                    return false;
                }
            }
            return true;
        }
    }
}
