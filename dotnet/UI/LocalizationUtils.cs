#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

// RMA_DONT_LOCALIZE (Tells the build process string parser to ignore this file)

namespace Rhino.UI
{
  static class LocalizationUtils
  {
    static AssemblyTranslations m_assembly_translations;
    static LocalizationStringTable GetStringTable(Assembly assembly, int languageId)
    {
      if (null == m_assembly_translations || m_assembly_translations.LanguageID != languageId)
        m_assembly_translations = new AssemblyTranslations(languageId);
      return m_assembly_translations.GetStringTable(assembly);
    }

    public static string LocalizeCommandName(Assembly assembly, int languageId, string english)
    {
      // If Rhino set the CurrentUICulture correctly, we could use System.Threading.Thread.CurrentUICulture
      // and not have any dependencies on Rhino. This would allow for use of this DLL in any RMA product
      LocalizationStringTable st = GetStringTable(assembly, languageId);
      // No string table with the requested languaeId so just return the English string
      if (null == st)
        return english;
      string loc_str;
      if (!st.CommandList.TryGetValue(english, out loc_str) || string.IsNullOrEmpty(loc_str))
        loc_str = english;
      return loc_str;
    }

    public static string LocalizeString(Assembly assembly, int languageId, string english, int contextId)
    {
      // If Rhino set the CurrentUICulture correctly, we could use System.Threading.Thread.CurrentUICulture
      // and not have any dependencies on Rhino. This would allow for use of this DLL in any RMA product
      LocalizationStringTable st = GetStringTable(assembly, languageId);
      // No string table with the requested languaeId so just return the English string
      if (null == st)
        return english;
      // 16 September 2010 John Morse
      // Need to massage the English string to compensate for string extractor limitations
      //
      // Make a copy of the English string so it can get messaged to form a proper key string
      StringBuilder key = new StringBuilder(english);
      // Check for leading spaces and save the number of spaces removed (iStart)
      int iStart;
      for (iStart = 0; key[iStart] == ' '; iStart++)
        ;
      if (iStart > 0)
        key.Remove(0, iStart);
      // Check for trailing spaces and save the number of spaces removed (jEnd - iEnd)
      int iEnd = key.Length - 1, jEnd;
      for (jEnd = iEnd; key[iEnd] == ' '; iEnd--)
        ;
      if (jEnd > iEnd)
        key.Remove(iEnd + 1, jEnd - iEnd);
      // If string to localize contains special characters then replace them with \\ versions since
      // \<char> is written to the XML file and used as a key
      // 
      bool bN = english.Contains("\n");
      if (bN)
        key.Replace("\n", "\\n");
      bool bR = english.Contains("\r");
      if (bR)
        key.Replace("\r", "\\r");
      bool bT = english.Contains("\t");
      if (bT)
        key.Replace("\t", "\\t");
      bool bBS = english.Contains("\\");
      if (bBS)
        key.Replace("\\", "\\\\");
      bool bQuot = english.Contains("\"");
      if (bQuot)
        key.Replace("\"", "\\\"");

      if (contextId >= 0)
      {
        key.Append("[[");
        key.Append(contextId.ToString());
        key.Append("]]");
      }
      string loc_str;
      if (!st.StringList.TryGetValue(key.ToString(), out loc_str) || string.IsNullOrEmpty(loc_str))
        loc_str = english;
      else if (iStart > 0 || jEnd > iEnd || bN || bR || bT || bQuot || bBS)
      {
        // String was massaged so reverse the process
        StringBuilder _loc_str = new StringBuilder(loc_str);
        // Pad front of string with number of spaces removed
        if (iStart > 0)
          _loc_str.Insert(0, " ", iStart);
        // Pad end of string with number of spaces removed
        if (jEnd > iEnd)
          _loc_str.Append(' ', jEnd - iEnd);
        // If the English string contained special character then strip the extra \ so it can get expanded
        if (bN)
          _loc_str.Replace("\\n", "\n");
        if (bR)
          _loc_str.Replace("\\r", "\r");
        if (bT)
          _loc_str.Replace("\\t", "\t");
        if (bBS)
          _loc_str.Replace("\\\\", "\\");
        if (bQuot)
          _loc_str.Replace("\\\"", "\"");
        // Make return string
        loc_str = _loc_str.ToString();
      }

      return loc_str;
    }

    public static string LocalizeString(Assembly assembly, int languageId, string english )
    {
      return LocalizeString(assembly, languageId, english, -1);
    }

    public static void LocalizeForm(Assembly assembly, int languageId, Control form)
    {
      LocalizationStringTable st = LocalizationUtils.GetStringTable(assembly, languageId);
      if (st != null)
      {
        string form_name = form.Name;
        Type type = form.GetType();
        string form_class_name = type.Name;
        st.LocalizeControlTree(form_name, form_class_name, form, GetToolTip(form));
      }
    }
    /// <summary>
    /// Obtains container properties from a control.
    /// </summary>
    /// <param name="c">A control.</param>
    /// <param name="components">A list of components is returned.</param>
    public static void GetContainersPropertiesFromControl(Control c, out List<System.ComponentModel.IContainer> components)
    {
      components = null;
      Type t = null == c ? null : c.GetType();
      Type typeIContainer = typeof(System.ComponentModel.IContainer);
      if (null != t && null != typeIContainer)
      {
        // Get a list of all of the public, private and protected members in the control "c"
        MemberInfo[] members = t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (null != members)
        {
          // Iterate the member list and exit when the first IContainer is found
          components = new List<System.ComponentModel.IContainer>();
          for (int i = 0; i < members.Length; i++)
          {
            if (0 != (members[i].MemberType & MemberTypes.Field))
            {
              FieldInfo fi = members[i] as FieldInfo;
              if (null != fi && typeIContainer.IsAssignableFrom(fi.FieldType))
              {
                System.ComponentModel.IContainer test = fi.GetValue(c) as System.ComponentModel.IContainer;
                if (null != test)
                  components.Add(test);
              }
            }
          }
        }
      }
    }
    /// <summary>
    /// Check to see if there are any System.Window.Control items in any components lists associated with the control like System.Windows.Forms.ContextMenuStrip
    /// which need to be localized.
    /// </summary>
    /// <param name="control">A control that might need to be localized.</param>
    /// <returns>An array of controls.</returns>
    static public Control[] GetComponentControls(Control control)
    {
      List<Control> result = null;
      List<System.ComponentModel.IContainer> components;
      GetContainersPropertiesFromControl(control, out components);
      if (null != components)
      {
        result = new List<Control>();
        // Iterate the IContainer and stop when a ToolTip component is found
        for (int x = 0; x < components.Count; x++)
        {
          System.ComponentModel.IContainer container = components[x];
          for (int i = 0, cnt = container.Components.Count; i < cnt; i++)
          {
            Control _control = container.Components[i] as Control;
            if (null != _control)
              result.Add(_control);
          }
        }
      }
      return (null != result && result.Count > 0 ? result.ToArray() : null);
    }
    /// <summary>
    /// Iterate a controls list of data members for filed's of type System.ComponentModel.IContainer and if found
    /// then iterate the containers control components and return the ToolTip components found.
    /// </summary>
    /// <param name="control">A control.</param>
    /// <returns>An array of tooltips.</returns>
    static public ToolTip[] GetToolTip(Control control)
    {
      List<ToolTip> result = null;
      List<System.ComponentModel.IContainer> components;
      GetContainersPropertiesFromControl(control, out components);
      if (null != components)
      {
        result = new List<ToolTip>();
        // Iterate the IContainer and stop when a ToolTip component is found
        for (int x = 0; x < components.Count; x++)
        {
          System.ComponentModel.IContainer container = components[x];
          for (int i = 0, cnt = container.Components.Count; i < cnt; i++)
          {
            ToolTip tooltip = container.Components[i] as ToolTip;
            if (null != tooltip)
              result.Add(tooltip);
          }
        }
      }
      return (null != result && result.Count > 0 ? result.ToArray() : null);
    }

    public static void LocalizeToolStripItemCollection(Assembly a, int language_id, Control parent, ToolStripItemCollection collection)
    {
      LocalizationStringTable st = LocalizationUtils.GetStringTable(a, language_id);
      if (st != null && parent!=null)
      {
        string form_name = parent.Name;
        Type type = parent.GetType();
        string form_class_name = type.Name;
        st.LocalizeToolStripCollection(form_name, form_class_name, collection);
      }
    }
  }


  sealed class AssemblyTranslations
  {
    readonly int m_language_id;
    readonly Dictionary<string, LocalizationStringTable> m_string_tables;
    readonly object m_sync_object;

    public AssemblyTranslations(int language_id)
    {
      m_language_id = language_id;
      m_string_tables = new Dictionary<string, LocalizationStringTable>();
      m_sync_object = new object();
    }

    public int LanguageID
    {
      get { return m_language_id; }
    }

    public LocalizationStringTable GetStringTable(Assembly a)
    {
      string key = a.Location;
      if (string.IsNullOrEmpty(key))
        return null;

      LocalizationStringTable st;
      if (m_string_tables.TryGetValue(key, out st))
        return st;

      // 18 July 2012 S. Baer
      // In the case that two threads are attempting to create the string
      // table at the same time, use a syncronization lock to only allow
      // one thread in the following block at any time
      lock (m_sync_object)
      {
        // perform the "TryGet" again in case a second thread was
        // blocked while the string table was being build
        if (m_string_tables.TryGetValue(key, out st))
          return st;

        // If we get here, the key does not exist in the dictionary.
        // Add a new string table
        st = new LocalizationStringTable();
        if (!st.LoadFromFile(a, m_language_id))
        {
          // If string table fails to load then set it to null so that
          // the next call which looks for this string table will return
          // the null string table instead of searching the disk or assemblies
          // embedded resources for the file
          st = null;
        }

        m_string_tables[key] = st;
      }

      return st;
    }
  }
}
