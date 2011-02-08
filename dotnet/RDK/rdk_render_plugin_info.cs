using System;
using System.Diagnostics;
using System.Collections.Generic;

#if USING_RDK

namespace Rhino.Render
{
  public class RenderPlugInList : List<RenderPlugInInfo>
  {
    public RenderPlugInList()
    {
      IntPtr pList = UnsafeNativeMethods.Rdk_RenderPlugInInfo_New();
      if (pList != IntPtr.Zero)
      {
        bool b = true;

        while (b)
        {
          using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
          {
            IntPtr pString = sh.NonConstPointer();
            Guid uuid = new Guid();
            b = 1==UnsafeNativeMethods.Rdk_RenderPlugInInfo(pList, ref uuid, pString);
            
            this.Add(new RenderPlugInInfo(uuid, pString.ToString()));
          }
        }

        UnsafeNativeMethods.Rdk_RenderPlugInInfo_Delete(pList);
      }
    }

  }

  public class RenderPlugInInfo
  {
    internal RenderPlugInInfo(Guid plugInId, String name)//, Rhino.PlugIns.RenderPlugIn plugIn)
    {
      _plugInId = plugInId;
      _name = name;
      //_plugIn = plugIn;
    }

    private Guid _plugInId;
    private String _name;
    //private Rhino.PlugIns.RenderPlugIn _plugIn;

    public String Name
    {
      get { return _name; }
      set { _name = value; }
    }

    //internal Rhino.PlugIns.RenderPlugIn PlugIn
    //{
    //  get { return _plugIn; }
    //  set { _plugIn = value; }
    //}

    public Guid PlugInId
    {
      get { return _plugInId; }
      set { _plugInId = value; }
    }
  }
}

#endif