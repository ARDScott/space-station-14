﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SS3D_shared.GO;

namespace SGO
{
    public class GameObjectComponent : IGameObjectComponent
    {
        /// <summary>
        /// The entity that owns this component
        /// </summary>
        private Entity m_owner;
        public Entity Owner
        {
            get { return m_owner; }
            set { m_owner = value; }
        }

        /// <summary>
        /// This is the family of the component. This should be set directly in all inherited components' constructors.
        /// </summary>
        protected ComponentFamily family = ComponentFamily.Generic;
        public ComponentFamily Family
        {
            get
            {
                return family;
            }
            set
            { family = value; }
        }
        
        /// <summary>
        /// Recieve a message from another component within the owner entity
        /// </summary>
        /// <param name="sender">the component that sent the message</param>
        /// <param name="type">the message type in CGO.MessageType</param>
        /// <param name="list">parameters list</param>
        public virtual void RecieveMessage(object sender, MessageType type, List<ComponentReplyMessage> replies, params object[] list)
        {
            if (sender == this) //Don't listen to our own messages!
                return;
            return;
        }

        /// <summary>
        /// Base method to shut down the component. 
        /// </summary>
        public virtual void Shutdown()
        {

        }

        /// <summary>
        /// Called when the component is removed from an entity.
        /// Shuts down the component, and removes it from the ComponentManager as well.
        /// </summary>
        public virtual void OnRemove()
        {
            Owner = null;
            Shutdown();
            //Send us to the manager so it knows we're dead.
            ComponentManager.Singleton.RemoveComponent(this);
        }

        /// <summary>
        /// Called when the component gets added to an entity. 
        /// This adds it to the component manager as well. No component should ever be owned
        /// by an entity without being in the ComponentManager.
        /// </summary>
        /// <param name="owner"></param>
        public virtual void OnAdd(Entity owner)
        {
            Owner = owner;
            //Send us to the manager so it knows we're active
            ComponentManager.Singleton.AddComponent(this);
        }

        /// <summary>
        /// Main method for updating the component. This is called from a big loop in Componentmanager.
        /// </summary>
        /// <param name="frameTime"></param>
        public virtual void Update(float frameTime)
        {

        }

        /// <summary>
        /// This allows setting of the component's parameters once it is instantiated.
        /// This should basically be overridden by every inheriting component, as parameters will be different
        /// across the board.
        /// </summary>
        /// <param name="parameter">ComponentParameter object describing the parameter and the value</param>
        public virtual void SetParameter(ComponentParameter parameter)
        {

        }

        /// <summary>
        /// Empty method for handling incoming input messages from counterpart client components
        /// </summary>
        /// <param name="message">the message object</param>
        public virtual void HandleNetworkMessage(IncomingEntityComponentMessage message)
        {

        }
    }
}
