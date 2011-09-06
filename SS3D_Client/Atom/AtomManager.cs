﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using SS3D_shared;
using SS3D.States;
using SS3D.Modules;
using Lidgren.Network;
using SS3D.Modules.Network;
using GorgonLibrary;
using GorgonLibrary.Graphics;

using CSScriptLibrary;

namespace SS3D.Atom
{
    public class AtomManager // CLIENTSIDE
    {
        #region Vars
        public GameScreen gameState;
        public Program prg;
        public NetworkManager networkManager;

        public Dictionary<ushort, Atom> atomDictionary;
        public DateTime now;
        public DateTime lastUpdate;
        public int updateRateLimit = 200; //200 updates / second
        private List<Module> m_loadedModules;
        #endregion

        #region Instantiation
        public AtomManager(GameScreen _gameState, Program _prg)
        {
            prg = _prg;
            gameState = _gameState;
            networkManager = prg.mNetworkMgr;
            atomDictionary = new Dictionary<ushort, Atom>();
            loadAtomScripts();
        }

        public void Shutdown()
        {
            atomDictionary.Clear(); // Dump all the atoms, we is gettin da fuck outta here bro
        }

        /// <summary>
        /// Big deal method to load atom scripts, compile them, stuff the compiled modules into a list, and repopulate the edit menu.
        /// </summary>
        private void loadAtomScripts()
        {
            m_loadedModules = new List<Module>();
            string[] filePaths = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\Scripts\Atom\", "*.cs");
            foreach (string path in filePaths)
            {
                var asm = CSScript.Load(path);
                Module[] modules = asm.GetModules();
                foreach (Module m in modules)
                {
                    m_loadedModules.Add(m);
                    var types = m.GetTypes().Where(t => t.IsSubclassOf(typeof(Atom))).ToArray();
                    foreach (Type t in types)
                        prg.GorgonForm.atomTypes.Add(t.Name, t);
                }
            }
            prg.GorgonForm.PopulateEditMenu();

        }
        #endregion

        #region Updating
        public void Update()
        {
            now = DateTime.Now;
            //Rate limit
            TimeSpan timeSinceLastUpdate = now - lastUpdate;
            if (timeSinceLastUpdate.TotalMilliseconds < 1000 / updateRateLimit)
                return;

            var updateList =
                from atom in atomDictionary
                where atom.Value.updateRequired == true
                select atom.Value;

            foreach (Atom a in updateList)
            {
                a.Update(timeSinceLastUpdate.TotalMilliseconds);
            }
            lastUpdate = now;
        }
        #endregion

        #region Network
        public void HandleNetworkMessage(NetIncomingMessage message)
        {
            AtomManagerMessage messageType = (AtomManagerMessage)message.ReadByte();
            
            switch (messageType)
            {
                case AtomManagerMessage.SpawnAtom:
                    HandleSpawnAtom(message);
                    break;
                case AtomManagerMessage.SetDrawDepth:
                    HandleDrawDepth(message);
                    break;
                case AtomManagerMessage.DeleteAtom:
                    HandleDeleteAtom(message); 
                    break;
                case AtomManagerMessage.Passthrough:
                    //Pass the rest of the message through to the specific item in question. 
                    PassMessage(message);
                    break;
                default:
                    break;
            }
        }

        private void HandleDrawDepth(NetIncomingMessage message)
        {
            ushort uid = message.ReadUInt16();
            int depth = message.ReadInt32();
            atomDictionary[uid].drawDepth = depth;
        }

        private void HandleSpawnAtom(NetIncomingMessage message)
        {
            ushort uid = message.ReadUInt16();
            string type = message.ReadString();
            int drawDepth = message.ReadInt32();

            Atom a = SpawnAtom(uid, type);
            a.drawDepth = drawDepth;

            // Tell the atom to pull its position data etc. from the server
            a.SendPullMessage();
        }
        
        public Atom SpawnAtom(ushort uid, string type)
        {
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
            atomDictionary[uid] = (Atom)atom;

            atomDictionary[uid].SetUp(uid, this);

            return atomDictionary[uid]; // Why do we return it? So we can do whatever is needed easily from the calling function.
        }

        private void HandleDeleteAtom(NetIncomingMessage message)
        {
            ushort uid = message.ReadUInt16();
            atomDictionary[uid] = null;
            atomDictionary.Remove(uid);
        }

        // Passes a message to a specific atom.
        private void PassMessage(NetIncomingMessage message)
        {
            // Get the atom id
            ushort uid = message.ReadUInt16();

            //TODO add some real error handling here. We shouldn't be getting bad messages like this, and if we are, it means we're doing something out of sequence.
            if (!atomDictionary.Keys.Contains(uid))
                return;

            var atom = atomDictionary[uid];
            
            // Pass the message to the atom in question.
            atom.HandleNetworkMessage(message);
        }

        private void SendMessage(NetOutgoingMessage message)
        {

        }
        #endregion

        public Atom GetAtom(ushort uid)
        {
            if(atomDictionary.Keys.Contains(uid))
                return atomDictionary[uid];
            return null;
        }

        public Type GetAtomType(string typename)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            Type atomType = currentAssembly.GetType("SS3D." + typename);

            if (atomType == null)
            {
                foreach (Module m in m_loadedModules)
                {
                    atomType = m.GetType("SS3D." + typename);
                    if (atomType != null)
                        break;
                }
            }
            if (atomType == null)
                throw new TypeLoadException("Could not load type " + "SS3D." + typename);
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
    }
}
