#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if RDK_UNCHECKED

namespace Rhino.Render
{

  [AttributeUsage(AttributeTargets.Class)]
  public sealed class CustomRenderContentIoAttribute : System.Attribute
  {
    private readonly String m_ext = "";
    private readonly bool   m_bLoad = false;
    private readonly bool   m_bSave = false;
    //private readonly String m_description = "";
    private readonly RenderContentKind m_kind = RenderContentKind.None;

    public CustomRenderContentIoAttribute(String extension, 
                                          //String description, 
                                          RenderContentKind kind, 
                                          bool canLoad, 
                                          bool canSave)
    {
      m_ext = extension;
      //m_description = description;
      m_kind = kind;
      m_bLoad = canLoad;
      m_bSave = canSave;
    }

    public String Extension       {      get { return m_ext; }            }
    //public String Description     {      get { return m_description; }    }
    public bool CanLoad           {      get { return m_bLoad; }          }
    public bool CanSave           {      get { return m_bSave; }          }
    public RenderContentKind Kind {      get { return m_kind; }           }
  }



  public abstract class IOPlugIn
  {
    //Currently IOPlugIn cannot be initialized from native IO plugins because we don't allow access
    //to the list - so you only get to define custom RhinoCommon ones.
    public IOPlugIn()
    {
      m_runtime_serial_number = m_current_serial_number++;
      m_all_custom_content_io_plugins.Add(m_runtime_serial_number, this);
    }

    public abstract RenderContent Load(String pathToFile);
    public abstract bool Save(String pathToFile, RenderContent rc, CreatePreviewEventArgs ss);

    public abstract String EnglishDescription { get; }
    public virtual String LocalDescription { get { return EnglishDescription; } }

    #region InternalRegistration

    private int m_runtime_serial_number;// = 0; initialized by runtime
    private static int m_current_serial_number = 1;
    private static readonly Dictionary<int, IOPlugIn> m_all_custom_content_io_plugins = new Dictionary<int, IOPlugIn>();

    private String m_ext = "";
    private bool m_bLoad = false;
    private bool m_bSave = false;
    //private String m_description = "";
    private RenderContentKind m_kind = RenderContentKind.None;

    internal void Destroy()
    {
      bool bRet = m_all_custom_content_io_plugins.Remove(m_runtime_serial_number);
      Debug.Assert(bRet);
    }

    internal void Construct(Guid pluginId)
    {
      Type t = GetType();
      object[] attr = t.GetCustomAttributes(typeof(CustomRenderContentIoAttribute), false);
      if (attr != null && attr.Length > 0)
      {
        CustomRenderContentIoAttribute custom = attr[0] as CustomRenderContentIoAttribute;
        if (custom != null)
        {
          m_ext = custom.Extension;
          m_bLoad = custom.CanLoad;
          m_bSave = custom.CanSave;
          //m_description = custom.Description;
          m_kind = custom.Kind;
        }
      }

      String m_description = "";

      UnsafeNativeMethods.CRhCmnContentIOPlugIn_New(m_runtime_serial_number, m_ext, m_description, (int)m_kind, m_bSave, m_bLoad, pluginId);
    }
    #endregion

    #region statics
    internal static IOPlugIn FromSerialNumber(int serial)
    {
      IOPlugIn rc;
      m_all_custom_content_io_plugins.TryGetValue(serial, out rc);
      return rc;
    }

    public static Type[] RegisterContentIo(System.Reflection.Assembly assembly, System.Guid pluginId)
    {
      Rhino.PlugIns.PlugIn plugin = Rhino.PlugIns.PlugIn.GetLoadedPlugIn(pluginId);
      if (plugin == null)
        return null;

      //Find all public types exported in the plug-in that we're dealing with
      Type[] exported_types = assembly.GetExportedTypes();
      if (exported_types != null)
      {
        List<Type> contentio_types = new List<Type>();
        for (int i = 0; i < exported_types.Length; i++)
        {
          //If the type is a Render.IOPlugIn, add it to the "to register" list.
          Type t = exported_types[i];
          if (!t.IsAbstract && t.IsSubclassOf(typeof(Rhino.Render.IOPlugIn)) && t.GetConstructor(new Type[] { }) != null)
          {
            contentio_types.Add(t);
          }
        }

        // make sure that content types have not already been registered
        for (int i = 0; i < contentio_types.Count; i++)
        {
          Type t = contentio_types[i];

          if (!RdkPlugIn.RenderContentIoTypeIsRegistered(t))
          {
            //The object registers itself in a static array
            IOPlugIn pi = System.Activator.CreateInstance(t) as IOPlugIn;
            pi.Construct(pluginId);
          }
        }        
        
        return contentio_types.ToArray();
      }
      return null;
    }
    #endregion

    #region callbacks
    internal delegate void DeleteThisCallback(int serial_number);
    internal static DeleteThisCallback m_DeleteThis = OnDeleteThis;
    static void OnDeleteThis(int serial_number)
    {
      try
      {
        IOPlugIn io = IOPlugIn.FromSerialNumber(serial_number) as IOPlugIn;
        if (io != null)
        {
          io.Destroy();
        }
      }
      catch
      {
      }
    }

    internal delegate int LoadCallback(int serial_number, IntPtr filename);
    internal static LoadCallback m_Load = OnLoad;
    static int OnLoad(int serial_number, IntPtr filename)
    {
      try
      {
        IOPlugIn io = IOPlugIn.FromSerialNumber(serial_number) as IOPlugIn;
        if (io != null)
        {
          string _filename = Marshal.PtrToStringUni(filename);
          RenderContent content = io.Load(_filename);
          if (content != null)
            return content.m_runtime_serial_number;
        }
      }
      catch
      {
      }
      return 0;
    }


    internal delegate bool SaveCallback(int serial_number, IntPtr filename, IntPtr content_ptr, IntPtr scene_server_ptr);
    internal static SaveCallback m_Save = OnSave;
    static bool OnSave(int serial_number, IntPtr filename, IntPtr content_ptr, IntPtr scene_server_ptr)
    {
      try
      {
        IOPlugIn io = IOPlugIn.FromSerialNumber(serial_number) as IOPlugIn;
        RenderContent content = RenderContent.FromPointer(content_ptr);

        CreatePreviewEventArgs pc = null;
        
        if (scene_server_ptr != IntPtr.Zero)
          pc = new CreatePreviewEventArgs(scene_server_ptr, new System.Drawing.Size(100, 100), PreviewSceneQuality.RefineThirdPass);
        
        if (io != null && content != null)
        {
          string _filename = Marshal.PtrToStringUni(filename);
          return io.Save(_filename, content, pc);
        }
      }
      catch
      {
      }
      return false;
    }

    internal delegate void GetRenderContentIoStringCallback(int serial_number, bool isName, IntPtr pON_wString);
    internal static GetRenderContentIoStringCallback m_GetRenderContentIoString = OnGetRenderContentIoString;
    static void OnGetRenderContentIoString(int serial_number, bool local, IntPtr pON_wString)
    {
      try
      {
        IOPlugIn io = IOPlugIn.FromSerialNumber(serial_number) as IOPlugIn;
        if (io != null)
        {
          string str = local ? io.LocalDescription : io.EnglishDescription;
          if (!string.IsNullOrEmpty(str))
            UnsafeNativeMethods.ON_wString_Set(pON_wString, str);
        }
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
    }

    #endregion

  }
}




#endif
