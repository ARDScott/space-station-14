﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace SGO
{
    /// <summary>
    /// Manager for entities -- controls things like template loading and instantiation
    /// </summary>
    public class EntityManager
    {
        private EntityFactory m_entityFactory;
        private EntityTemplateDatabase m_entityTemplateDatabase;
        private EntityNetworkManager m_entityNetworkManager;

        private Dictionary<int, Entity> m_entities;
        private int lastId = 0;

        public EntityManager(NetServer netServer)
        {
            m_entityNetworkManager = new EntityNetworkManager(netServer);
            m_entityTemplateDatabase = new EntityTemplateDatabase();
            m_entityFactory = new EntityFactory(m_entityTemplateDatabase);
        }

        /// <summary>
        /// Returns an entity by id
        /// </summary>
        /// <param name="eid">entity id</param>
        /// <returns>Entity or null if entity id doesn't exist</returns>
        public Entity GetEntity(int eid)
        {
            if (m_entities.Keys.Contains(eid))
                return m_entities[eid];
            return null;
        }

        /// <summary>
        /// Creates an entity and adds it to the entity dictionary
        /// </summary>
        /// <param name="templateName">name of entity template to execute</param>
        /// <returns>integer id of added entity</returns>
        public int CreateEntity(string templateName)
        {
            //Get the entity from the factory
            Entity e = m_entityFactory.CreateEntity(templateName);
            if (e != null)
            {
                //It worked, add it.
                m_entities.Add(++lastId, e);
                lastId++;
                return lastId;
            }
            //TODO: throw exception here -- something went wrong.
            return -1;
        }

        /// <summary>
        /// Handle an incoming network message by passing the message to the EntityNetworkManager 
        /// and handling the parsed result.
        /// </summary>
        /// <param name="msg">Incoming raw network message</param>
        public void HandleNetworkMessage(NetIncomingMessage msg)
        {
            IncomingEntityMessage message = m_entityNetworkManager.HandleEntityNetworkMessage(msg);
            m_entities[message.uid].HandleNetworkMessage(message);
        }
    }
}
