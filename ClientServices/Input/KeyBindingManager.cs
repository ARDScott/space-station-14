﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using GorgonLibrary;
using GorgonLibrary.InputDevices;
using System.Security;
using SS13_Shared;


namespace ClientServices.Input
{
    public class KeyBindingManager 
    {
        private static KeyBindingManager singleton;
        public static KeyBindingManager Singleton
        {
            [SecuritySafeCritical]
            get
            {
                if (singleton == null)
                    throw new TypeInitializationException("KeyBindingManager not initialized.", null);
                else
                    return singleton;
            }
            private set
            {
            }
        }

        private bool enabled = true;
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        private Keyboard m_keyboard;
        public Keyboard Keyboard
        {
            get
            {
                return m_keyboard;
            }
            set
            {
                if (m_keyboard != null)
                {
                    m_keyboard.KeyDown -= new KeyboardInputEvent(KeyDown);
                    m_keyboard.KeyUp -= new KeyboardInputEvent(KeyUp);
                }
                m_keyboard = value;
                m_keyboard.KeyDown += new KeyboardInputEvent(KeyDown);
                m_keyboard.KeyUp += new KeyboardInputEvent(KeyUp);
            }
        }

        private Dictionary<KeyboardKeys, BoundKeyFunctions> BoundKeys;

        public delegate void BoundKeyEventHandler(object sender, BoundKeyEventArgs e);

        public event BoundKeyEventHandler BoundKeyDown;
        public event BoundKeyEventHandler BoundKeyUp;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public KeyBindingManager()
        {
            
        }

        /// <summary>
        /// Destructor -- unbinds from the keyboard input
        /// </summary>
        ~KeyBindingManager()
        {
            if(m_keyboard != null)
            {
                m_keyboard.KeyDown -= new KeyboardInputEvent(KeyDown);
                m_keyboard.KeyUp -= new KeyboardInputEvent(KeyUp);
            }
        }

        /// <summary>
        /// Sets up singleton, binds to the Keyboard device
        /// </summary>
        /// <param name="_keyboard"></param>
        public static void Initialize(Keyboard _keyboard)
        {
            singleton = new KeyBindingManager();
            singleton.Keyboard = _keyboard;
            singleton.LoadKeys();
           
        }
        /// <summary>
        /// Handles key down events from the gorgon keyboard object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void KeyDown(object sender, KeyboardInputEventArgs e)
        {
            //If the key is bound, fire the BoundKeyDown event.
            if (enabled && BoundKeys.Keys.Contains(e.Key) && BoundKeyDown != null)
                BoundKeyDown(this, new BoundKeyEventArgs(BoundKeyState.Down, BoundKeys[e.Key]));
        }
        /// <summary>
        /// Handles key up events from the gorgon keyboard object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void KeyUp(object sender, KeyboardInputEventArgs e)
        {
            //If the key is bound, fire the BoundKeyUp event.
            if (enabled && BoundKeys.Keys.Contains(e.Key) && BoundKeyUp != null)
                BoundKeyUp(this, new BoundKeyEventArgs(BoundKeyState.Up, BoundKeys[e.Key]));
        }

        /// <summary>
        /// Loads key bindings from KeyBindings.xml in the bin directory
        /// </summary>
        public void LoadKeys()
        {
            XmlDocument xml = new XmlDocument();
            StreamReader kb = new StreamReader("KeyBindings.xml");
            xml.Load(kb);
            XmlNodeList resources = xml.SelectNodes("KeyBindings/Binding");
            BoundKeys = new Dictionary<KeyboardKeys, BoundKeyFunctions>();
            foreach (XmlNode node in resources)
            {
                BoundKeys.Add(
                    (KeyboardKeys)Enum.Parse(typeof(KeyboardKeys), node.Attributes["Key"].Value, false),
                    (BoundKeyFunctions)Enum.Parse(typeof(BoundKeyFunctions), node.Attributes["Function"].Value, false));
            }
        }

    }



}
