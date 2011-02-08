// 29 Jan 2010 - S. Baer
// Commented this class out. It doesn't appear to do anything yet, but looks like a work in progress.
// If you want to continue to work on this class, make it private until it has some functionality
/*
using System;
using System.Collections.Generic;
using System.Threading;
using Rhino.Collections;

namespace Rhino.Geometry
{
  public class MesherUtility
  {
    #region members
    private RhinoList<Brep> m_brep;
    #endregion

    #region constructors
    public MesherUtility()
    {
      m_brep = new RhinoList<Brep>();
    }
    public MesherUtility(int initialCapacity)
    {
      if (initialCapacity < 0) { throw new ArgumentException("initialCapacity must be a positive integer"); }
      m_brep = new RhinoList<Brep>(initialCapacity);
    }
    #endregion

    #region database access
    public int AddSurface(Surface surface)
    {
      Brep brep = surface.BrepForm();
      if (brep == null) { return -1; }
      return AddBrep(brep);
    }
    public int AddBrep(Brep brep)
    {
      m_brep.Add(brep);
      return m_brep.Count - 1;
    }

    public Brep this[int index] { get { return m_brep[index]; } }
    #endregion

    #region threaded methods

    #endregion

    internal class ON_MeshAction
    {
      //private Brep m_brep;
      //Problem! Need mesh settings.
    }
  }
}
*/