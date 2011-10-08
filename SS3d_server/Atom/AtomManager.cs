﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;
using System.Text;
using CSScriptLibrary;
using Lidgren.Network;
using SGO;
using SS3D_Server.Atom.Mob;
using SS3D_Server.Modules;
using SS3D_Server.Modules.Map;
using SS3D_shared;
using SS3D_shared.HelperClasses;

namespace SS3D_Server.Atom
{
    public class AtomManager //SERVERSIDE
    {
        #region Vars

        public Dictionary<int, Atom> atomDictionary;
        private List<Module> m_loadedModules;

        //private int lastUID = 0;
        public int lastUID //Linked to entity manager to keep uids consistent.
        {
            get
            {
                return m_entityManager.lastId;
            }
            set
            {
                m_entityManager.lastId = value;
            }
        }

        private EntityManager m_entityManager;
        #endregion

        #region instantiation
        public AtomManager(EntityManager entityManager)
        {
            atomDictionary = new Dictionary<int, Atom>();
            m_entityManager = entityManager;
            //loadAtomScripts();
        }

        /// <summary>
        /// Big deal method to load atom scripts, compile them, stuff the compiled modules into a list, and repopulate the edit menu.
        /// </summary>
        private void loadAtomScripts()
        {
            m_loadedModules = new List<Module>();
            PermissionSet permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            string[] filePaths = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\Scripts\Atom\", "*.cs");
            foreach (string path in filePaths)
            {
                var asm = CSScript.Load(path);
                
                asm.PermissionSet.SetPermission(permissions.GetPermission(typeof(SecurityPermission)));
                Module[] modules = asm.GetModules();
                foreach (Module m in modules)
                {
                    m_loadedModules.Add(m);
                    var types = m.GetTypes().Where(t => t.IsSubclassOf(typeof(Atom))).ToArray();
                }
            }

        }
        #endregion

        #region updating
        public void Update(float framePeriod)
        {
            // Using LINQ to find atoms that have flagged themselves as needing an update.
            // TODO: Modify to add an update queue that will run updates for atoms that need it on a time schedule
            var updateList =
                from atom in atomDictionary
                where atom.Value.updateRequired == true
                select atom.Value;
           
            //Update all of the bastards in the update list
            foreach (Atom a in updateList)
            {
                a.Update(framePeriod);
            }
        }
        #endregion

        #region Network
        public void HandleNetworkMessage(NetIncomingMessage message)
        {
            AtomManagerMessage messageType = (AtomManagerMessage)message.ReadByte();
            switch (messageType)
            {
                case AtomManagerMessage.Passthrough:
                    // Pass a message to the atom in question
                    PassMessage(message);
                    break;
                case AtomManagerMessage.SpawnAtom:
                    string type = message.ReadString();
                    Vector2 position = new Vector2(message.ReadFloat(), message.ReadFloat());
                    float rotation = message.ReadFloat();
                    SpawnAtom(type, position, rotation);
                    break;
                case AtomManagerMessage.DeleteAtom:
                    int uid = message.ReadInt32();
                    DeleteAtom(uid);
                    break;
                default:
                    break;
            }
        }

        #region atom synchronization
        private void PassMessage(NetIncomingMessage message)
        {
            // Get atom id
            int uid = message.ReadInt32();

            var atom = atomDictionary[uid];
            // Pass the message
            atom.HandleNetworkMessage(message);
        }

        private void SendMessage(int uid, NetOutgoingMessage message)
        {
            //Message should already have the uid attached by the atom in question. Just need to send it here.
        }

        public void NewPlayer(NetConnection connection)
        {
            SendAllAtoms(connection);
        }
        #endregion

        #region deletion
        private void SendDeleteAtom(int uid)
        {
            NetOutgoingMessage message = SS3DNetServer.Singleton.CreateMessage();
            message.Write((byte)NetMessage.AtomManagerMessage);
            message.Write((byte)AtomManagerMessage.DeleteAtom);
            message.Write(uid);
            SS3DServer.Singleton.SendMessageToAll(message);
        }
        #endregion

        #region spawning
        private void SendSpawnAtom(int uid, string type)
        {
            Atom atom = atomDictionary[uid];
            NetOutgoingMessage message = SS3DNetServer.Singleton.CreateMessage();
            message.Write((byte)NetMessage.AtomManagerMessage);
            message.Write((byte)AtomManagerMessage.SpawnAtom);
            message.Write(uid);
            message.Write(type);
            message.Write(atom.drawDepth);
            SS3DServer.Singleton.SendMessageToAll(message);
        }

        private void SendSpawnAtom(int uid, string type, NetConnection client)
        {
            Atom atom = atomDictionary[uid];
            NetOutgoingMessage message = SS3DNetServer.Singleton.CreateMessage();
            message.Write((byte)NetMessage.AtomManagerMessage);
            message.Write((byte)AtomManagerMessage.SpawnAtom);
            message.Write(uid);
            message.Write(type);
            message.Write(atom.drawDepth);
            SS3DServer.Singleton.SendMessageTo(message, client);
        }

        public void SendAllAtoms(NetConnection client)
        {
            // Send all atoms to a specific client. 
            // TODO: Make this less fucking stupid ie: send all of the atoms in a condensed form instead of this heavy-handed looping shit.
            // This will get laggy later on.
            foreach(Atom atom in atomDictionary.Values) 
            {
                SendSpawnAtom(atom.Uid, AtomName(atom), client);
            }
            ///Tell each atom to do its post-instantiation shit. Theoretically, this should all occur after each atom has
            ///been spawned and instantiated on the clientside. Network traffic wise this might be weird
            ///
            foreach (Atom atom in atomDictionary.Values)
            {
                atom.SendState(client);
            }
            
        }

        public string AtomName(object atom)
        {
            string type = atom.GetType().ToString();
            type = type.Substring(type.IndexOf(".") + 1); // Fuckugly method of stripping the assembly name of the type.

            return type;
        }

        public Atom SpawnAtom(string type)
        {
            int uid = lastUID++;

            Type atomType = GetAtomType(type);
            
            object atom = Activator.CreateInstance(atomType); // Create atom of type atomType with parameters uid, this
            atomDictionary[uid] = (Atom)atom;
            
            atomDictionary[uid].SetUp(uid, this);
            m_entityManager.AddAtomEntity((Entity)atom); /// Add atom to entity manager
            SendSpawnAtom(uid, type);
            atomDictionary[uid].SendState();
   
            return atomDictionary[uid]; // Why do we return it? So we can do whatever is needed easily from the calling function.
        }
/*
                   Assembly currentAssembly = Assembly.GetExecutingAssembly();
            Type atomType = currentAssembly.GetType("SS3D." + type);

            if (atomType == null)
            {
                foreach (Module m in m_loadedModules)
                {
                    atomType = m.GetType("SS3D." + type);
                    if (atomType != null)
                        break;
                }
            }
            if (atomType == null)
                throw new TypeLoadException("Could not load type " + "SS3D." + type);
            object atom = Activator.CreateInstance(atomType); // Create atom of type atomType with parameters uid, this
         */

        public Type GetAtomType(string typename)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            Type atomType = currentAssembly.GetType("SS3D_Server." + typename);

            if (atomType == null)
            {
                foreach (Module m in m_loadedModules)
                {
                    atomType = m.GetType("SS3D_Server." + typename);
                    if (atomType != null)
                        break;
                }
            }
            if (atomType == null)
                throw new TypeLoadException("Could not load type " + "SS3D_Server." + typename);
            return atomType;
        }

        public Type[] GetAtomTypes()
        {
            List<Type> types = new List<Type>();
            Assembly ass = Assembly.GetExecutingAssembly(); //LOL ASS
            foreach (Type t in ass.GetTypes().Where(t => t.IsSubclassOf(typeof(Atom))))
                types.Add(t);

            foreach (Module m in m_loadedModules)
            {
                foreach (Type t in m.GetTypes().Where(t => t.IsSubclassOf(typeof(Atom))))
                    types.Add(t);
            }
            return types.ToArray();
        }

        public Atom SpawnAtom(string type, Vector2 position)
        {
            Atom spawned = SpawnAtom(type);
            spawned.Translate(position);
            spawned.spawnTile = SS3D_Server.SS3DServer.Singleton.map.GetTileFromWorldPosition(position);
            spawned.PostSpawnActions();
            return spawned;
        }
        public Atom SpawnAtom(string type, Vector2 position, float rotation)
        {
            Atom spawned = SpawnAtom(type);
            spawned.Translate(position, rotation);
            spawned.spawnTile = SS3D_Server.SS3DServer.Singleton.map.GetTileFromWorldPosition(position);
            spawned.PostSpawnActions();
            return spawned;
        }
        #endregion

        /// <summary>
        ///  <para>Broadcasts draw depth value of atom to all connected players.</para>
        /// </summary>
        private void SendAtomDrawDepth(int uid, int depth)
        {
            NetOutgoingMessage message = SS3DNetServer.Singleton.CreateMessage();
            message.Write((byte)NetMessage.AtomManagerMessage);
            message.Write((byte)AtomManagerMessage.SetDrawDepth);
            message.Write(uid);
            message.Write(depth);
            SS3DServer.Singleton.SendMessageToAll(message);
        }
        #endregion

        /// <summary>
        ///  <para>Sets draw depth value of serverside instance and broadcasts draw depth value of atom to all connected players.</para>
        /// </summary>
        public void SetDrawDepthAtom(int uid, int depth)
        {
            //SetDrawDepthAtom(atomDictionary[uid], depth);
        }

        /// <summary>
        ///  <para>Sets draw depth value of serverside instance and broadcasts draw depth value of atom to all connected players.</para>
        /// </summary>
        public void SetDrawDepthAtom(Atom atom, int depth)
        {
            //atom.drawDepth = depth;
            //SendAtomDrawDepth(atom.Uid, depth);
        }

        public void DeleteAtom(int uid)
        {
            // Delete the atom and send a delete atom message
            DeleteAtom(atomDictionary[uid]);
        }

        public void DeleteAtom(Atom atom)
        {
            atomDictionary.Remove(atom.Uid);
            atom.Destruct();
            SendDeleteAtom(atom.Uid);
        }

        public Atom GetAtom(int uid)
        {
            if (atomDictionary.Keys.Contains(uid))
                return atomDictionary[uid];
            else
                return null;
        }

        public void SaveAtoms()
        {
            Stream s = File.Open("atoms.ss13", FileMode.Create);
            BinaryFormatter f = new BinaryFormatter();
            f.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple; //Stores types without assembly versions

            LogManager.Log("Writing atoms to file...");
            List<Atom> saveList = new List<Atom>();
            foreach (Atom a in atomDictionary.Values)
            {
                if (a.IsChildOfType(typeof(Mob.Mob)))
                    continue;
                saveList.Add(a);
            }
            f.Serialize(s, saveList);
            s.Close();
            LogManager.Log("Done writing atoms to file.");
        }

        public void LoadAtoms()
        {
            if (!File.Exists("atoms.ss13"))
            {
                LogManager.Log("***** Cannot find file atoms.ss13. Map starting empty *****", LogLevel.Warning);
                return;
            }

            Stream s = new FileStream("atoms.ss13", FileMode.Open);
            BinaryFormatter f = new BinaryFormatter();
            //Specifies that we can load serialized types without versioning issues.
            f.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
            //Binder from below
            f.Binder = new VersionConfigToNamespaceAssemblyObjectBinder();
            List<Atom> o = (List<Atom>)f.Deserialize(s);
            foreach (Atom a in o)
            {
                a.SetUp(lastUID++, this);
                a.SerializedInit();
                a.spawnTile = SS3D_Server.SS3DServer.Singleton.map.GetTileFromWorldPosition(a.position);
                a.PostSpawnActions();
                atomDictionary.Add(a.Uid, a);
                m_entityManager.AddAtomEntity((Entity)a);
            }
            s.Close();
        }
    }

    /// <summary>
    /// This binder will search all of the loaded assemblies to find the typename specified by the binaryformatter.
    /// This allows us to load scripted objects from the serialized atoms file.
    /// </summary>
    internal sealed class VersionConfigToNamespaceAssemblyObjectBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;
            try
            {
                string ToAssemblyName = assemblyName.Split(',')[0];
                Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly ass in Assemblies)
                {
                    if (ass.FullName.Split(',')[0] == ToAssemblyName)
                    {
                        typeToDeserialize = ass.GetType(typeName);
                        break;
                    }
                }
            }
            catch (System.Exception exception)
            {
                throw exception;
            }
            return typeToDeserialize;
        }
    }
}
