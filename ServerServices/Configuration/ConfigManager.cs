﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Xml;
using System.Xml.Serialization;
using SS13_Shared;
using ServerInterfaces;

namespace ServerServices.Configuration
{
    public sealed class ConfigManager: IConfigManager, IService
    {
        public PersistentConfiguration Configuration;
        private string ConfigFile;
        
        public ConfigManager(string ConfigFilename)
        {
            Initialize(ConfigFilename);
        }

        public void Initialize(string ConfigFileLoc)
        {
            if (File.Exists(ConfigFileLoc))
            {
                System.Xml.Serialization.XmlSerializer ConfigLoader = new System.Xml.Serialization.XmlSerializer(typeof(PersistentConfiguration));
                StreamReader ConfigReader = File.OpenText(ConfigFileLoc);
                PersistentConfiguration Config = (PersistentConfiguration)ConfigLoader.Deserialize(ConfigReader);
                ConfigReader.Close();
                Configuration = Config;
                ConfigFile = ConfigFileLoc;
            }
            else
            {
                //if (LogManager.Singleton != null) LogManager.Singleton.LogMessage("ConfigManager: Could not load config. File not found. " + ConfigFileLoc);
            }
        }

        public void Save()
        {
            if (Configuration == null)
            {
                //if (LogManager.Singleton != null) LogManager.Singleton.LogMessage("ConfigManager: Could not write config. No File loaded. " + Configuration.ToString() + " , " + ConfigFile);
                return;
            }
            else
            {
                System.Xml.Serialization.XmlSerializer ConfigSaver = new System.Xml.Serialization.XmlSerializer(Configuration.GetType());
                StreamWriter ConfigWriter = File.CreateText(ConfigFile);
                ConfigSaver.Serialize(ConfigWriter, Configuration);
                ConfigWriter.Flush();
                ConfigWriter.Close();
            }
        }

        public void LoadResources()
        {
        }

        public string ServerName
        {
            get { return Configuration.ServerName; }
            set { Configuration.ServerName = value; }
        }

        public string ServerMapName
        {
            get { return Configuration.serverMapName; }
            set { Configuration.serverMapName = value; }
        }

        public string ServerWelcomeMessage
        {
            get { return Configuration.serverWelcomeMessage; }
            set { Configuration.serverWelcomeMessage = value; }
        }
        
        public string AdminPassword
        {
            get { return Configuration.AdminPassword; }
            set { Configuration.AdminPassword = value; }
        }

        public string LogPath
        {
            get { return Configuration.LogPath; }
            set { Configuration.LogPath = value; }
        }

        public int Version
        {
            get { return PersistentConfiguration._Version; }
        }

        public int Port
        {
            get { return Configuration.Port; }
            set { Configuration.Port = value; }
        }
        
        public int ServerMaxPlayers
        {
            get { return Configuration.serverMaxPlayers; }
            set { Configuration.serverMaxPlayers = value; }
        }

        public int FramePeriod
        {
            get { return Configuration.framePeriod; }
            set { Configuration.serverMaxPlayers = value; }
        }
        public GameType GameType
        {
            get { return Configuration.gameType; }
            set { Configuration.gameType = value; }
        }

        public bool MessageLogging
        {
            get { return Configuration.MessageLogging; }
            set { Configuration.MessageLogging = value; }
        }

        public ServerServiceType ServiceType
        {
            get { return ServerServiceType.ConfigManager; }
        }
    }
}
