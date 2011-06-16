using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace Rhino.UI
{
  static class LocalizationUtils
  {
    static AssemblyTranslations m_assembly_translations = null;
    static LocalizationStringTable GetStringTable(Assembly assembly, int languageId)
    {
      if (null == m_assembly_translations || m_assembly_translations.LanguageID != languageId)
        m_assembly_translations = new AssemblyTranslations(languageId);
      return m_assembly_translations.GetStringTable(assembly);
    }

    public static string LocalizeString(Assembly assembly, int languageId, string english, int contextId)
    {
      string loc_str = english;
      // If Rhino set the CurrentUICulture correctly, we could use System.Threading.Thread.CurrentUICulture
      // and not have any dependencies on Rhino. This would allow for use of this DLL in any RMA product
      LocalizationStringTable st = GetStringTable(assembly, languageId);

      // 16 September 2010 John Morse
      // Need to massage the English string to compensate for string extractor limitations
      //
      // Make a copy of the English string so it can get messaged to form a proper key string
      StringBuilder key = new StringBuilder(english);
      // Check for leading spaces and save the number of spaces removed (iStart)
      int iStart = 0;
      for (iStart = 0; key[iStart] == ' '; iStart++)
        ;
      if (iStart > 0)
        key.Remove(0, iStart);
      // Check for trailing spaces and save the number of spaces removed (jEnd - iEnd)
      int iEnd = key.Length - 1, jEnd = 0;
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
        string form_class_name = string.Empty;
        Type type = form.GetType();
        if (null != type)
          form_class_name = type.Name;
        st.LocalizeControlTree(form_name, form_class_name, form);
      }
    }

    public static void LocalizeToolStripItemCollection(Assembly a, int language_id, Control parent, ToolStripItemCollection collection)
    {
      LocalizationStringTable st = LocalizationUtils.GetStringTable(a, language_id);
      if (st != null)
      {
        string form_name = parent.Name;
        string form_class_name = string.Empty;
        Type type = parent.GetType();
        if (null != type)
          form_class_name = type.Name;
        st.LocalizeToolStripCollection(form_name, form_class_name, collection);
      }
    }
  }


  class AssemblyTranslations
  {
    int m_language_id;
    Dictionary<string, LocalizationStringTable> m_string_tables;

    public AssemblyTranslations(int language_id)
    {
      m_language_id = language_id;
      m_string_tables = new Dictionary<string, LocalizationStringTable>();
    }

    public int LanguageID
    {
      get { return m_language_id; }
    }

    public LocalizationStringTable GetStringTable(Assembly a)
    {
      string key = a.Location;
      LocalizationStringTable st = null;
      if (m_string_tables.TryGetValue(key, out st))
        return st;

      // If we get here, the key does not exist in the dictionary.
      // Add a new string table
      st = new LocalizationStringTable();
      if (st.LoadFromFile(a, m_language_id))
        m_string_tables.Add(key, st);
      else
        st = null;
      return st;
    }
  }
}
