﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using Mogre;

using SS3D.Modules;
using SS3D.Modules.UI;
using SS3D.Modules.Map;
using SS3D.Modules.Items;
using SS3D.Modules.Network;

using SS3D_shared;

using Miyagi;
using Miyagi.UI;
using Miyagi.UI.Controls;
using Miyagi.Common;
using Miyagi.Common.Data;
using Miyagi.Common.Animation;
using Miyagi.Common.Resources;
using Miyagi.Common.Events;
using Miyagi.TwoD;

namespace SS3D.States
{
    public class EditorScreen : State
    {
        public OgreManager mEngine;
        public StateManager mStateMgr;

        public ItemManager itemManager;
        public Map currentMap;

        public LoadingTracker currentLoadingTracker = null;

        public EditorStatusBar statusBar;
        public EditorToolbar toolBar;

        //private AtomBaseClass prevMouseOverEntity;

        //private AtomBaseClass selectedEntity;
        //private AtomBaseClass prevSelectedEntity;

        public EditorScreen()
        {
            mEngine = null;
        }

        public override bool Startup(StateManager _mgr)
        {
            mEngine = _mgr.Engine;
            mStateMgr = _mgr;

            mEngine.mMiyagiSystem.GUIManager.DisposeAllGUIs();

            mEngine.SceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_TEXTURE_ADDITIVE_INTEGRATED;
            mEngine.SceneMgr.SetShadowTexturePixelFormat(PixelFormat.PF_FLOAT16_RGB);
            mEngine.SceneMgr.SetShadowTextureCasterMaterial("shadow_caster");
            mEngine.SceneMgr.ShadowCasterRenderBackFaces = false;
            mEngine.SceneMgr.ShadowTextureSelfShadow = true;
            mEngine.SceneMgr.SetShadowTextureSize(512);

            mEngine.SceneMgr.SetSkyBox(true, "SkyBox", 900f, true);

            mEngine.Camera.Position = new Mogre.Vector3(0, 300, 0);
            mEngine.Camera.LookAt(new Mogre.Vector3(160, 64, 160));

            currentMap = new Map(mEngine, true);
            itemManager = new ItemManager(mEngine, currentMap, mEngine.mNetworkMgr, null);

            toolBar = new EditorToolbar(mStateMgr, this);
            mEngine.mMiyagiSystem.GUIManager.GUIs.Add(toolBar);

            statusBar = new EditorStatusBar(mStateMgr, this);
            mEngine.mMiyagiSystem.GUIManager.GUIs.Add(statusBar);

            return true;
        }

        public override void Shutdown()
        {
            mEngine.mMiyagiSystem.GUIManager.DisposeAllGUIs();
        }

        public override void Update(long _frameTime)
        {
            mEngine.SceneMgr.SkyBoxNode.Rotate(Mogre.Vector3.UNIT_Y, 0.0001f);

            if (currentLoadingTracker != null)
            {
                statusBar.StatusTextPanel.Text = currentLoadingTracker.loadingText;
                statusBar.StatusProgressBar.Value = (int)currentLoadingTracker.loadingPercent;
                statusBar.StatusProgressBar.Text = System.Math.Floor(currentLoadingTracker.loadingPercent).ToString() + "%";
            }
        }

        public void LoadMap(string mapPath)
        {
            this.currentLoadingTracker = this.currentMap;
            MapFile loadedMap = MapFileHandler.LoadMap(mapPath);
            this.currentMap.Shutdown();
            System.GC.Collect(); //Oh boy.
            this.currentMap.LoadMap(loadedMap);
        }

        #region Input
        public override void UpdateInput(Mogre.FrameEvent evt, MOIS.Keyboard keyState, MOIS.Mouse mouseState)
        {
        }

        public override void KeyDown(MOIS.KeyEvent keyState)
        {
        }

        public override void KeyUp(MOIS.KeyEvent keyState)
        {
        }

        public override void MouseUp(MOIS.MouseEvent mouseState, MOIS.MouseButtonID button)
        {
        }

        public override void MouseDown(MOIS.MouseEvent mouseState, MOIS.MouseButtonID button)
        {
            if (mEngine.mMiyagiSystem.GUIManager.GetTopControl() != null) return;
        }

        public override void MouseMove(MOIS.MouseEvent mouseState)
        {
        }
        #endregion
    }

    public sealed class EditorLoadWindow : ModalGUI
    {
        Panel loadPanel;
        Panel filePanel;
        Button cancelButton;

        StateManager mStateMgr;
        EditorScreen editorState;

        public EditorLoadWindow(StateManager _mgr, EditorScreen editScreen) 
            : base("editorLoadBox")
        {
            mStateMgr = _mgr;
            editorState = editScreen;

            this.loadPanel = new Panel("EditorLoadPanel")
            {
                Location = new Point(150, 150),
                Size = new Size(236, 300),
                ResizeMode = ResizeModes.None,
                Movable = true,
                BorderStyle =
                {
                    Thickness = new Thickness(3, 3, 3, 3)
                },
                Skin = MiyagiResources.Singleton.Skins["ConsolePanelSkin"]
            };
            this.Controls.Add(loadPanel);

            loadPanel.Controls.Add(new Panel("EditorLoadPanelText")
            {
                Location = new Point(25, 0),
                Size = new Size(180, 30),
                ResizeMode = ResizeModes.None,
                Text = "Load Map",
                HitTestVisible = false,
                TextStyle = 
                {
                    Alignment = Alignment.MiddleCenter,
                    ForegroundColour = Colours.Black
                },
                Skin = MiyagiResources.Singleton.Skins["ConsolePanelSkin"]
            });

            this.filePanel = new Panel("EditorFilePanel")
            {
                Location = new Point(25, 30),
                Size = new Size(180, 220),
                ResizeMode = ResizeModes.None,
                Skin = MiyagiResources.Singleton.Skins["ConsolePanelSkin"],
                BorderStyle =
                {
                    Thickness = new Thickness(3, 3, 3, 3)
                },
                HScrollBarStyle =
                {
                    ShowButtons = false,
                    Extent = 16,
                    BorderStyle =
                    {
                        Thickness = new Thickness(2, 2, 2, 2)
                    },
                    ThumbStyle =
                    {
                        BorderStyle =
                        {
                            Thickness = new Thickness(2, 2, 2, 2)
                        }
                    }
                },
                VScrollBarStyle =
                {
                    ShowButtons = false,
                    Extent = 16,
                    BorderStyle =
                    {
                        Thickness = new Thickness(2, 2, 2, 2)
                    },
                    ThumbStyle =
                    {
                        BorderStyle =
                        {
                            Thickness = new Thickness(2, 2, 2, 2)
                        }
                    }
                },
            };
            loadPanel.Controls.Add(filePanel);

            cancelButton = new Button("mapCancelLoadButton")
            {
                Size = new Size(80, 35),
                Location = new Point(75, filePanel.Bottom + 5),
                Text = "Cancel",
                TextStyle =
                {
                    Alignment = Alignment.MiddleCenter,
                    ForegroundColour = Colours.Black
                },
                Skin = MiyagiResources.Singleton.Skins["ButtonSkinGreen"]
            };
            cancelButton.Click += new EventHandler(cancelButton_Click);
            loadPanel.Controls.Add(cancelButton);

            populateMapList();
        }

        private void populateMapList()
        {
            if (filePanel == null) return;

            if (!Directory.Exists(@".\Maps"))//Create the folder if its not there.
                Directory.CreateDirectory(@".\Maps");

            string[] mapFiles = Directory.GetFiles(@".\Maps", "*.map"); //We want a list of all map files.
            int currY = 0;
            foreach (string currFile in mapFiles)
            {
                Button newButton = new Button("mapLoadButton" + currY.ToString())
                {
                    Size = new Size(130, 35),
                    Location = new Point(0, currY),
                    Text = Path.GetFileNameWithoutExtension(currFile),
                    TextStyle =
                    {
                        Alignment = Alignment.MiddleCenter,
                        ForegroundColour = Colours.Black
                    },
                    Skin = MiyagiResources.Singleton.Skins["ButtonSkinGreen"]
                };
                newButton.UserData = currFile; //The path to the file the button represents.
                filePanel.Controls.Add(newButton);

                currY += newButton.Size.Height;
                newButton.Click += new EventHandler(loadMapFile_Click);
            }
        }

        void loadMapFile_Click(object sender, EventArgs e)
        {
            Button sButton = (Button)sender;
            this.Visible = false;
            //TODO: Block Main toolbar while loading.
            editorState.LoadMap((string)sButton.UserData); //This contains the path.
            this.Dispose();
            mStateMgr.Engine.mMiyagiSystem.GUIManager.GUIs.Remove(this);
        }

        void cancelButton_Click(object sender, EventArgs e)
        {
            this.Pop();
            this.Dispose();
            mStateMgr.Engine.mMiyagiSystem.GUIManager.GUIs.Remove(this);
        }
    }

    public sealed class EditorToolbar : PopupGUI
    {
        Panel toolbarPanel;
        Button newButton;
        Button loadButton;
        Button saveButton;

        private LinearFunctionValueController<float> FaderIn;
        private LinearFunctionValueController<float> FaderOut;

        StateManager mStateMgr;
        EditorScreen editorState;

        public EditorToolbar(StateManager _mgr, EditorScreen editScreen) 
            : base("EditToolbar")
        {
            mStateMgr = _mgr;
            editorState = editScreen;

            this.toolbarPanel = new Panel("ToolbarPanel")
            {
                Location = new Point(0, 0),
                Size = new Size((int)mStateMgr.Engine.mWindow.Width, 50),
                ResizeMode = ResizeModes.None,
                Skin = MiyagiResources.Singleton.Skins["ConsolePanelSkin"]
            };

            this.newButton = new Button("EditNewButton")
            {
                Size = new Size(80, 32),
                Location = new Point(10, 10),
                Text = "New",
                TextStyle =
                {
                    Alignment = Alignment.MiddleCenter,
                    ForegroundColour = Colours.Black
                },
                Skin = MiyagiResources.Singleton.Skins["ButtonSkinGreen"]
            };

            this.loadButton = new Button("EditLoadButton")
            {
                Size = new Size(80, 32),
                Location = new Point(100, 10),
                Text = "Load",
                TextStyle =
                {
                    Alignment = Alignment.MiddleCenter,
                    ForegroundColour = Colours.Black
                },
                Skin = MiyagiResources.Singleton.Skins["ButtonSkinGreen"]
            };
            loadButton.Click += new EventHandler(loadButton_Click);

            this.saveButton = new Button("EditSaveButton")
            {
                Size = new Size(80, 32),
                Location = new Point(190, 10),
                Text = "Save",
                TextStyle =
                {
                    Alignment = Alignment.MiddleCenter,
                    ForegroundColour = Colours.Black
                },
                Skin = MiyagiResources.Singleton.Skins["ButtonSkinGreen"]
            };

            this.PopupOrientation = Orientation.Vertical;
            this.PopupRange = new Range(0, 50);

            this.toolbarPanel.Controls.AddRange(loadButton, saveButton, newButton);
            this.Controls.Add(toolbarPanel);
        }

        void loadButton_Click(object sender, EventArgs e)
        {
            mStateMgr.Engine.mMiyagiSystem.GUIManager.GUIs.Add(new EditorLoadWindow(mStateMgr, editorState) { Visible = true });
        }

        #region Toolbar Fading
        protected override void OnPopupClosed()
        {
            if (this.FaderIn != null)
            {
                this.FaderIn.Stop();
            }

            this.FaderOut = new LinearFunctionValueController<float>(this.toolbarPanel.Opacity, 0, TimeSpan.FromMilliseconds(100));
            this.FaderOut.Finished += (o, s) => this.Visible = false;
            this.FaderOut.Start(this.MiyagiSystem, true, val => this.toolbarPanel.Opacity = val);
        }

        protected override void OnPopupOpened()
        {
            if (this.FaderOut != null)
            {
                this.FaderOut.Stop();
            }

            this.Visible = true;

            this.FaderIn = new LinearFunctionValueController<float>(this.toolbarPanel.Opacity, 1, TimeSpan.FromMilliseconds(100));
            this.FaderIn.Start(this.MiyagiSystem, true, val => this.toolbarPanel.Opacity = val);
        } 
        #endregion

        }

    public sealed class EditorStatusBar : GUI
    {
        Panel BackgroundPanel;
        public Panel StatusTextPanel;
        public ProgressBar StatusProgressBar;

        StateManager mStateMgr;
        EditorScreen editorState;

        public EditorStatusBar(StateManager _mgr, EditorScreen editScreen)
            : base("editorStatusBar")
        {
            mStateMgr = _mgr;
            editorState = editScreen;

            this.BackgroundPanel = new Panel("EditorStatusBackground")
            {
                Location = new Point(0, (int)mStateMgr.Engine.mWindow.Height - 35),
                Size = new Size((int)mStateMgr.Engine.mWindow.Width, 35),
                ResizeMode = ResizeModes.None,
                HitTestVisible = true,
                Movable = false,
                Skin = MiyagiResources.Singleton.Skins["ConsolePanelSkin"]
            };
            this.Controls.Add(BackgroundPanel);

            StatusTextPanel = new Panel("EditorStatusTextPanel")
            {
                Location = new Point(0, 3),
                Size = new Size(180, 30),
                ResizeMode = ResizeModes.None,
                Text = "Status...",
                HitTestVisible = false,
                TextStyle =
                {
                    Alignment = Alignment.MiddleCenter,
                    ForegroundColour = Colours.White,
                },
                TextureFiltering = TextureFiltering.Anisotropic,
                Skin = MiyagiResources.Singleton.Skins["ConsolePanelSkin"]
            };
            BackgroundPanel.Controls.Add(StatusTextPanel);

            StatusProgressBar = new ProgressBar("EditorStatusProgressBar")
            {
                Location = new Point(180, 3),
                Size = new Size(300, 30),
                ProgressBarStyle = 
                {
                    Orientation = Orientation.Horizontal,
                    Mode = ProgressBarMode.Continuous, 
                    Offset = new Point(1,1)
                }, 
                Text = "0%",
                Max = 100,
                Min = 0,
                HitTestVisible = false,
                BorderStyle =
                {
                    Thickness = new Thickness(3, 3, 3, 3)
                },
                TextStyle =
                {
                    Alignment = Alignment.MiddleCenter,
                    ForegroundColour = Colours.White
                },
                TextureFiltering = TextureFiltering.Anisotropic,
                Skin = MiyagiResources.Singleton.Skins["ProgressBarHSkin"]
            };
            BackgroundPanel.Controls.Add(StatusProgressBar);
        }
    }
}
