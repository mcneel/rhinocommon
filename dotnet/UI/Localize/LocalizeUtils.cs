using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

// RMA_DONT_LOCALIZE

namespace Rhino.UI
{
  static class LocalizeUtils
  {
    /// <summary>
    /// 
    /// </summary>
    static LocalizeUtils()
    {
      m_assembly_translations = null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="language_id"></param>
    /// <param name="english_str"></param>
    /// <param name="context_id"></param>
    /// <returns></returns>
    static public string LocalizeString(Assembly a, uint language_id, string english_str, int context_id)
    {
      string loc_str = english_str;
      // If Rhino set the CurrentUICulture correctly, we could use System.Threading.Thread.CurrentUICulture
      // and not have any dependencies on Rhino. This would allow for use of this DLL in any RMA product
      // unsigned int language_id = RhinoApp().AppSettings().AppearanceSettings().m_language_identifier;
      // LocalizationStringTable st = GetStringTable( a, language_id );
      LocalizationStringTable st = LocalizeUtils.GetStringTable(a, language_id);
      if (st != null)
      {
        // 16 September 2010 John Morse
        // Need to massage the English string to compensate for string extractor limitations
        //
        // Make a copy of the English string so it can get messaged to form a proper key string
        StringBuilder key = new StringBuilder(english_str);
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
        bool bN = english_str.Contains("\n");
        if (bN)
          key.Replace("\n", "\\n");
        bool bR = english_str.Contains("\r");
        if (bR)
          key.Replace("\r", "\\r");
        bool bT = english_str.Contains("\t");
        if (bT)
          key.Replace("\t", "\\t");
        bool bBS = english_str.Contains("\\");
        if (bBS)
          key.Replace("\\", "\\\\");
        bool bQuot = english_str.Contains("\"");
        if (bQuot)
          key.Replace("\"", "\\\"");
        key.Append("[[");
        key.Append(context_id.ToString());
        key.Append("]]");
        if (!st.StringList.TryGetValue(key.ToString(), out loc_str) || string.IsNullOrEmpty(loc_str))
          loc_str = english_str;
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
      }
      return loc_str;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="language_id"></param>
    /// <param name="english_str"></param>
    /// <returns></returns>
    static public string LocalizeString(Assembly a, uint language_id, string english_str )
    {
      string loc_str = english_str;
      // If Rhino set the CurrentUICulture correctly, we could use System.Threading.Thread.CurrentUICulture
      // and not have any dependencies on Rhino. This would allow for use of this DLL in any RMA product
      // unsigned int language_id = RhinoApp().AppSettings().AppearanceSettings().m_language_identifier;
      // LocalizationstringTable st = GetstringTable( a, language_id );
      LocalizationStringTable st = LocalizeUtils.GetStringTable(a, language_id);
      if (st != null)
      {
        // 16 September 2010 John Morse
        // Need to massage the English string to compensate for string extractor limitations
        //
        // Make a copy of the English string so it can get messaged to form a proper key string
        StringBuilder key = new StringBuilder(english_str);
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
        bool bN = english_str.Contains("\n");
        if (bN)
          key.Replace("\n", "\\n");
        bool bR = english_str.Contains("\r");
        if (bR)
          key.Replace("\r", "\\r");
        bool bT = english_str.Contains("\t");
        if (bT)
          key.Replace("\t", "\\t");
        bool bBS = english_str.Contains("\\");
        if (bBS)
          key.Replace("\\", "\\\\");
        bool bQuot = english_str.Contains("\"");
        if (bQuot)
          key.Replace("\"", "\\\"");
        if (!st.StringList.TryGetValue(key.ToString(), out loc_str) || string.IsNullOrEmpty(loc_str))
          loc_str = english_str;
        else if (iStart > 0 || jEnd > iEnd || bN || bR || bT || bQuot || bBS)
        {
          // string was massaged so reverse the process
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
      }
      return loc_str;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="language_id"></param>
    /// <param name="form"></param>
    static public void LocalizeForm(Assembly a, uint language_id, Control form )
    {
      if (form != null && a != null)
      {
        LocalizationStringTable st = LocalizeUtils.GetStringTable(a, language_id);
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
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="language_id"></param>
    /// <param name="parent"></param>
    /// <param name="collection"></param>
    static public void LocalizeToolStripItemCollection(Assembly a, uint language_id, Control parent, ToolStripItemCollection collection)
    {
      if (parent != null && a != null)
      {
        LocalizationStringTable st = LocalizeUtils.GetStringTable(a, language_id);
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="language_id"></param>
    /// <returns></returns>
    static private LocalizationStringTable GetStringTable(Assembly a, uint language_id)
    {
      if (a == null)
        return null;
      if (null == LocalizeUtils.m_assembly_translations || LocalizeUtils.m_assembly_translations.LanguageID != language_id)
        LocalizeUtils.m_assembly_translations = new AssemblyTranslations(language_id);
      return LocalizeUtils.m_assembly_translations.GetStringTable(a);
    }
    #region Member variables
    /// <summary>
    /// 
    /// </summary>
    static private AssemblyTranslations m_assembly_translations;
    #endregion Member variables
  }
}
