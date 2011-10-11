﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lidgren.Network;
using SS3D_Server.Modules;
using ServerServices;

namespace SS3D_Server
{
    class EntryPoint
    {
        private SS3DServer server;

        static void Main(string[] args)
        {
            EntryPoint main = new EntryPoint();
            main.server = new SS3DServer();
            LogManager.Log("Server -> Starting");

            if (main.server.Start())
            {
                LogManager.Log("Server -> Can not start server", ServerServices.LogLevel.Fatal); //Not like you'd see this, haha. Perhaps later for logging.
                Environment.Exit(0);
            }

            string strVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            LogManager.Log("Server Version " + strVersion + " -> Ready");

            main.server.MainLoop();
        }


    }
}
