﻿using System;
using System.Collections.Generic;
using GorgonLibrary;
using GorgonLibrary.InputDevices;

namespace SS3D.Modules
{
  /************************************************************************/
  /* base class for program states                                        */
  /************************************************************************/
  public abstract class State
  {
    public State() //constructor
    {
       
    }
    public Program prg;

    public abstract bool Startup( Program _prg );

    public abstract void Shutdown();

    public abstract void Update( long _frameTime );
    public abstract void GorgonRender();

      //GORGON
    public abstract void KeyDown(KeyboardInputEventArgs e);
    public abstract void KeyUp(KeyboardInputEventArgs e);
    public abstract void MouseUp(MouseInputEventArgs e);
    public abstract void MouseDown(MouseInputEventArgs e);
    public abstract void MouseMove(MouseInputEventArgs e);
    public abstract void FormResize();

  }

}
