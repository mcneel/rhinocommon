#pragma warning disable 1591
using System;
using System.Reflection;
#if RHINO_SDK
using Rhino.Runtime.InteropWrappers;
#endif

// RMA_DONT_LOCALIZE (Tells the build process string parser to ignore this file)

namespace Rhino.UI
{
  /// <summary>
  /// Used a placeholded which is used by LocalizationProcessor application to create contextId
  /// mapped localized strings.
  /// </summary>
  public static class LOC
  {
    ///<summary>
    /// Strings that need to be localized should call this function. The STR function doesn't actually
    /// do anything but return the original string. The LocalizationProcessor application walks
    /// through the source code of a project and looks for LOC.STR. The function is then replaced with a
    /// call to Localization.LocalizeString using a unique context ID.
    ///</summary>
    ///<param name='english'>[in] The English string to localize.</param>
    public static string STR(string english)
    {
      return english;
    }

    /// <summary>
    /// Similar to <see cref="string.Format(string, object)"/> function.
    /// </summary>
    /// <param name="english">The English name.</param>
    /// <param name="assemblyOrObject">Unused.</param>
    /// <returns>English name.</returns>
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
    ///<param name='english'>[in] The English string to localize.</param>
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
    ///<param name='english'>[in] The English string to localize.</param>
    /// <returns>Returns localized string pair with both the English and local names set to the English value.</returns>
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
    ///<param name='english'>[in] The English string to localize.</param>
    ///<param name='assemblyFromObject'>[in] The object that identifies the assembly that owns the command option name.</param>
    /// <returns>Returns localized string pair with both the English and local names set to the English value.</returns>
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
    ///<param name='engilsh'>[in] The English string to localize.</param>
    /// <returns>Returns localized string pair with both the English and local names set to the English value.</returns>
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
    ///<param name='engilsh'>[in] The English string to localize.</param>
    ///<param name='assemblyFromObject'>[in] The object that identifies the assembly that owns the command option value.</param>
    /// <returns>Returns localized string pair with both the English and local names set to the English value.</returns>
    public static LocalizeStringPair COV(string engilsh, object assemblyFromObject)
    {
      return new LocalizeStringPair(engilsh, engilsh);
    }
  }

  public enum DistanceDisplayMode
  {
    Decimal = 0,
    Fractional = 1,
    FeetInches = 2
  }

  public static class Localization
  {
#if RHINO_SDK
    /// <summary>
    /// Gets localized unit system name.  Uses current application locale id.
    /// </summary>
    /// <param name="units">The unit system.</param>
    /// <param name="capitalize">true if the name should be capitalized.</param>
    /// <param name="singular">true if the name is expressed for a singular element.</param>
    /// <param name="abbreviate">true if name should be the abbreviation.</param>
    /// <returns>The unit system name.</returns>
    public static string UnitSystemName(UnitSystem units, bool capitalize, bool singular, bool abbreviate)
    {
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoApp_UnitSystemName((int)units, capitalize, singular, abbreviate, pString);
        return sh.ToString();
      }
    }

    /// <summary>
    /// Get a string version of a number in a given unit system / display mode.
    /// </summary>
    /// <param name="x">The number to format into a string.</param>
    /// <param name="units">The unit system for the number.</param>
    /// <param name="mode">How the number should be formatted.</param>
    /// <param name="precision">The precision of the number.</param>
    /// <param name="appendUnitSystemName">Adds unit system name to the end of the number.</param>
    /// <returns>The formatted number.</returns>
    public static string FormatNumber( double x, UnitSystem units, DistanceDisplayMode mode, int precision, bool appendUnitSystemName )
    {
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.RHC_RhinoFormatNumber(x, (int)units, (int)mode, precision, appendUnitSystemName, pString);
        return sh.ToString();
      }
    }
#endif
    /// <summary>
    /// Returns localized version of a given English string. This function should be autogenerated by the
    /// RmaLDotNetLocalizationProcessor application for every function that uses RMASTR.
    /// </summary>
    /// <param name="english">The text in English.</param>
    /// <param name="contextId">The context ID.</param>
    /// <returns>The localized string.</returns>
    public static string LocalizeString(string english, int contextId)
    {
      Assembly assembly = Assembly.GetCallingAssembly();
      return LocalizationUtils.LocalizeString(assembly, CurrentLanguageID, english, contextId);
    }
    /// <summary>
    /// Returns localized version of a given English string. This function should be autogenerated by the
    /// RmaLDotNetLocalizationProcessor application for every function that uses RMASTR.
    /// </summary>
    /// <param name="english">The text in English.</param>
    /// <param name="assemblyOrObject">An assembly or an object from an assembly.</param>
    /// <param name="contextId">The context ID.</param>
    /// <returns>The localized string.</returns>
    public static string LocalizeString(string english, object assemblyOrObject, int contextId)
    {
      Assembly assembly = GetAssemblyFromObject(assemblyOrObject);
      return LocalizationUtils.LocalizeString(assembly, CurrentLanguageID, english, contextId);
    }
    /// <summary>
    /// Look in the dialog item list for the specified key and return the translated
    /// localized string if the key is found otherwise return the English string.
    /// </summary>
    /// <param name="assemblyOrObject">An assembly or an object from an assembly.</param>
    /// <param name="key"></param>
    /// <param name="english">The text in English.</param>
    /// <returns>
    /// Look in the dialog item list for the specified key and return the translated
    /// localized string if the key is found otherwise return the English string.
    /// </returns>
    public static string LocalizeDialogItem(object assemblyOrObject, string key, string english)
    {
      Assembly assembly = GetAssemblyFromObject(assemblyOrObject);
      return LocalizationUtils.LocalizeDialogItem(assembly, CurrentLanguageID, key, english);
    }
    /// <summary>
    /// Check to see if the passed object is an assembly, if not then get the assembly that owns the object type.
    /// </summary>
    /// <param name="assemblyOrObject">An assembly or an object from an assembly.</param>
    /// <returns>The localized string.</returns>
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
    ///<param name='english'>The localized command name.</param>
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

#if RHINO_SDK
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
#endif

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
          m_language_id = Rhino.Runtime.HostUtils.RunningInRhino ? Rhino.ApplicationSettings.AppearanceSettings.LanguageIdentifier : 1033;
#else
          m_language_id = 1033;
#endif
        }
        return m_language_id;
      }
    }

    /// <summary>
    /// Sets the Id used for Localization in RhinoCommon.  Only useful for when
    /// using RhinoCommon outside of the Rhino process
    /// </summary>
    /// <param name="id"></param>
    /// <returns>true if the language id could be set</returns>
    public static bool SetLanguageId(int id)
    {
#if RHINO_SDK
      if (Rhino.Runtime.HostUtils.RunningInRhino)
        return false;
#endif
      m_language_id = id;
      return true;
    }
  }

  /// <summary>
  /// Pair of strings used for localization.
  /// </summary>
  public sealed class LocalizeStringPair
  {
    public LocalizeStringPair(string english, string local)
    {
      English = english;
      Local = local;
    }

    public string English { get; private set; }
    public string Local { get; private set; }
  }
}
