#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

// RMA_DONT_LOCALIZE (Tells the build process string parser to ignore this file)

namespace Rhino.UI
{
  /// <summary>
  /// Used a placeholded which is used by LocalizationProcessor application to create contextId
  /// mapped localized strings
  /// </summary>
  public static class LOC
  {
    ///<summary>
    /// Strings that need to be localized should call this function. The STR function doesn't actually
    /// do anything but return the original string. The LocalizationProcessor application walks
    /// through the source code of a project and looks for LOC.STR. The function is then replaced with a
    /// call to Localization.LocalizeString using a unique context ID.
    ///</summary>
    ///<param name='english'>[in] The English string to localize</param>
    public static string STR(string english)
    {
      return english;
    }

    /// <summary>
    /// Similar to String::Format function
    /// </summary>
    /// <param name="english"></param>
    /// <param name="assemblyOrObject"></param>
    /// <returns></returns>
    public static string STR(string english, object assemblyOrObject)
    {
      return english;
    }

    ///<summary>
    /// Command names that need to be localized should call this function. The COMMANDNAME function doesn't actually
    /// do anything but return the original string. The LocalizationProcessor application walks
    /// through the source code of a project and looks for LOC.COMMANDNAME and builds a record for each command
    /// name for the translators that can be used by developers in a commands overridden Rhino.Commands.Command.LocalName
    /// which should call Rhino.UI.Localization.LocalizeCommandName(EnglishName)
    ///</summary>
    ///<param name='english'>[in] The English string to localize</param>
    public static string COMMANDNAME(string english)
    {
      return english;
    }

    /// <summary>
    /// Command option name strings that need to be localized should call this function. The CON function
    /// doesn't actually do anything but return the original string. The LocalizationProcessor application walks
    /// through the source code of a project and looks for LOC.CON. The function is then replaced with a
    /// call to Localization.LocalizeCommandOptionName using a unique context ID.
    ///</summary>
    ///<param name='english'>[in] The English string to localize</param>
    /// <returns>Returns localized string pair with both the English and local names set to the English value</returns>
    public static LocalizeStringPair CON(string english)
    {
      return new LocalizeStringPair(english, english);
    }

    /// <summary>
    /// Command option name strings that need to be localized should call this function. The CON function
    /// doesn't actually do anything but return the original string. The LocalizationProcessor application walks
    /// through the source code of a project and looks for LOC.CON. The function is then replaced with a
    /// call to Localization.LocalizeCommandOptionName using a unique context ID.
    ///</summary>
    ///<param name='english'>[in] The English string to localize</param>
    ///<param name='assemblyFromObject'>[in] The object that identifies the assembly that owns the command option name</param>
    /// <returns>Returns localized string pair with both the English and local names set to the English value</returns>
    public static LocalizeStringPair CON(string english, object assemblyFromObject)
    {
      return new LocalizeStringPair(english, english);
    }

    /// <summary>
    /// Command option name strings that need to be localized should call this function. The COV function
    /// doesn't actually do anything but return the original string. The LocalizationProcessor application walks
    /// through the source code of a project and looks for LOC.COV. The function is then replaced with a
    /// call to Localization.LocalizeCommandOptionValue using a unique context ID.
    ///</summary>
    ///<param name='engilsh'>[in] The English string to localize</param>
    /// <returns>Returns localized string pair with both the English and local names set to the English value</returns>
    public static LocalizeStringPair COV(string engilsh)
    {
      return new LocalizeStringPair(engilsh, engilsh);
    }

    /// <summary>
    /// Command option name strings that need to be localized should call this function. The COV function
    /// doesn't actually do anything but return the original string. The LocalizationProcessor application walks
    /// through the source code of a project and looks for LOC.COV. The function is then replaced with a
    /// call to Localization.LocalizeCommandOptionValue using a unique context ID.
    ///</summary>
    ///<param name='engilsh'>[in] The English string to localize</param>
    ///<param name='assemblyFromObject'>[in] The object that identifies the assembly that owns the command option value</param>
    /// <returns>Returns localized string pair with both the English and local names set to the English value</returns>
    public static LocalizeStringPair COV(string engilsh, object assemblyFromObject)
    {
      return new LocalizeStringPair(engilsh, engilsh);
    }
  }

  public static class Localization
  {
    /// <summary>
    /// Gets localized unit system name.  Uses current application locale id
    /// </summary>
    /// <param name="units"></param>
    /// <param name="capitalize"></param>
    /// <param name="singular"></param>
    /// <param name="abbreviate"></param>
    /// <returns></returns>
    public static string UnitSystemName(UnitSystem units, bool capitalize, bool singular, bool abbreviate)
    {
      using (var sh = new Rhino.Runtime.StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoApp_UnitSystemName((int)units, capitalize, singular, abbreviate, pString);
        return sh.ToString();
      }
    }

    /// <summary>
    /// Returns localized version of a given English string. This function should be autogenerated by the
    /// RmaLDotNetLocalizationProcessor application for every function that uses RMASTR
    /// </summary>
    /// <param name="english"></param>
    /// <param name="contextId"></param>
    /// <returns></returns>
    public static string LocalizeString(string english, int contextId)
    {
      Assembly assembly = Assembly.GetCallingAssembly();
      return LocalizationUtils.LocalizeString(assembly, CurrentLanguageID, english, contextId);
    }
    /// <summary>
    /// Returns localized version of a given English string. This function should be autogenerated by the
    /// RmaLDotNetLocalizationProcessor application for every function that uses RMASTR
    /// </summary>
    /// <param name="english"></param>
    /// <param name="assemblyOrObject"></param>
    /// <param name="contextId"></param>
    /// <returns></returns>
    public static string LocalizeString(string english, object assemblyOrObject, int contextId)
    {
      Assembly assembly = GetAssemblyFromObject(assemblyOrObject);
      return LocalizationUtils.LocalizeString(assembly, CurrentLanguageID, english, contextId);
    }

    /// <summary>
    /// Check to see if the passed object is an assembly, if not then get the assembly that owns the object type
    /// </summary>
    /// <param name="assemblyOrObject"></param>
    /// <returns></returns>
    public static Assembly GetAssemblyFromObject(object assemblyOrObject)
    {
      Assembly assembly = assemblyOrObject as Assembly;
      if (null == assembly)
      {
        Type type = assemblyOrObject.GetType();
        assembly = type.Assembly;
      }
      return assembly;
    }

    ///<summary>
    /// Commands that need to be localized should call this function.
    ///</summary>
    ///<param name='english'></param>
    public static string LocalizeCommandName(string english)
    {
      Assembly assembly = Assembly.GetCallingAssembly();
      return LocalizationUtils.LocalizeCommandName(assembly, CurrentLanguageID, english);
    }

    public static string LocalizeCommandName(string english, object assemblyOrObject)
    {
      Assembly assembly = GetAssemblyFromObject(assemblyOrObject);
      return LocalizationUtils.LocalizeCommandName(assembly, CurrentLanguageID, english);
    }

    public static LocalizeStringPair LocalizeCommandOptionName(string english, int contextId)
    {
      Assembly assembly = Assembly.GetCallingAssembly();
      return Localization.LocalizeCommandOptionName(english, assembly, contextId);
    }

    public static LocalizeStringPair LocalizeCommandOptionName(string english, object assemblyOrObject, int contextId)
    {
      string local = Localization.LocalizeString(english, assemblyOrObject, contextId);
      return new LocalizeStringPair(english, local);
    }

    public static LocalizeStringPair LocalizeCommandOptionValue(string english, int contextId)
    {
      Assembly assembly = Assembly.GetCallingAssembly();
      return Localization.LocalizeCommandOptionValue(english, assembly, contextId);
    }

    public static LocalizeStringPair LocalizeCommandOptionValue(string english, object assemblyOrObject, int contextId)
    {
      string local = Localization.LocalizeString(english, assemblyOrObject, contextId);
      return new LocalizeStringPair(english, local);
    }

    ///<summary>
    /// A form or user control should call this in its constructor if it wants to be localized
    /// the typical constructor for a localize form would look like:
    /// MyForm::MyForm()
    /// {
    ///   SuspendLayout();
    ///   InitializeComponent();
    ///   Rhino.UI.Localize.LocalizeForm( this );
    ///   ResumeLayout(true);
    /// }
    ///</summary>
    public static void LocalizeForm(Control form)
    {
      if (null == form)
        return;
      Assembly assembly = form.GetType().Assembly;
      LocalizationUtils.LocalizeForm(assembly, CurrentLanguageID, form);
    }

    ///<summary>
    /// A form or user control should call this in its constructor if it wants to localize
    /// context menus that are set on the fly and not assigned to a forms control in design
    /// studio.
    /// MyForm::MyForm()
    /// {
    ///   SuspendLayout();
    ///   InitializeComponent();
    ///   Rhino.UI.Localize.LocalizeToolStripItemCollection( this, this.MyToolStrip.Items );
    /// }
    ///</summary>
    public static void LocalizeToolStripItemCollection(Control parent, ToolStripItemCollection collection)
    {
      Assembly assembly = null == collection ? null : collection.GetType().Assembly;
      LocalizationUtils.LocalizeToolStripItemCollection(assembly, CurrentLanguageID, parent, collection);
    }

    static int m_language_id = -1;
    static int CurrentLanguageID
    {
      get
      {
        // we don't want the language id to change since Rhino in general does not
        // support swapping localizations on the fly. Use a cached language id after the
        // initial language id has been read
        if (m_language_id == -1)
        {
#if RHINO_SDK
          // This code is commonly called while working in theVisual Studio designer
          // and we want to try and not throw exceptions in order to show the winform
          if (Rhino.Runtime.HostUtils.RunningInRhino)
            m_language_id = Rhino.ApplicationSettings.AppearanceSettings.LanguageIdentifier;
          else
            m_language_id = 1033;
#else
          m_language_id = 1033;
#endif
        }
        return m_language_id;
      }
    }
  }

  /// <summary>
  /// Pair of strings used for localization
  /// </summary>
  public sealed class LocalizeStringPair
  {
    public LocalizeStringPair(string english, string local)
    {
      m_english = english;
      m_local = local;
    }
    string m_english, m_local;
    public string English { get { return m_english; } }
    public string Local { get { return m_local; } }
  }
}
