﻿using SS13_Shared;
using SS13_Shared.GO;
using ServerInterfaces.GameObject;

namespace SGO
{
    /// <summary>
    /// Mover component that responds to movement by an entity.
    /// </summary>
    public class SlaveMoverComponent : GameObjectComponent
    {
        private IEntity master;

        public SlaveMoverComponent()
        {
            Family = ComponentFamily.Mover;
        }

        public override ComponentReplyMessage RecieveMessage(object sender, ComponentMessageType type,
                                                             params object[] list)
        {
            ComponentReplyMessage reply = base.RecieveMessage(sender, type, list);

            if (sender == this)
                return ComponentReplyMessage.Empty;

            switch (type)
            {
                case ComponentMessageType.SlaveAttach:
                    Attach((int) list[0]);
                    break;
            }
            return reply;
        }

        public override void OnRemove()
        {
            Detach();
            base.OnRemove();
        }

        private void Attach(int uid)
        {
            master = EntityManager.Singleton.GetEntity(uid);
            master.OnShutdown += master_OnShutdown;
            master.OnMove += HandleOnMove;
            Translate(master.Position);
        }

        private void master_OnShutdown(IEntity e)
        {
            Detach();
        }

        private void Detach()
        {
            if (master != null)
            {
                master.OnMove -= HandleOnMove;
                master = null;
            }
        }

        private void HandleOnMove(Vector2 toPosition, Vector2 fromPosition)
        {
            Translate(toPosition);
        }

        public void Translate(Vector2 toPosition)
        {
            Vector2 oldPosition = Owner.Position;
            Owner.Position = toPosition;
            Owner.Moved(oldPosition);
        }
    }
}