using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

// RMA_DONT_LOCALIZE

namespace Rhino.UI
{
  class AssemblyTranslations
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="language_id"></param>
    public AssemblyTranslations(uint language_id)
    {
      this.m_language_id = language_id;
      this.m_string_tables = new Dictionary<string, LocalizationStringTable>();
    }
    /// <summary>
    /// 
    /// </summary>
    public uint LanguageID
    {
      get
      {
        return this.m_language_id;
      }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public LocalizationStringTable GetStringTable(Assembly a)
    {
      if (null != a)
      {
        string key = a.Location;
        LocalizationStringTable st = null;
        if (this.m_string_tables.TryGetValue(key, out st))
          return st;
        // If we get here, the key does not exist in the dictionary.
        // Add a new string table
        st = new LocalizationStringTable();
        if (null != st && st.LoadFromFile(a, this.m_language_id))
          this.m_string_tables.Add(key, st);
        else
          st = null;
        return st;
      }
      return null;
    }
    #region Member variables
    /// <summary>
    /// 
    /// </summary>
    protected uint m_language_id;
    /// <summary>
    /// 
    /// </summary>
    protected Dictionary<string,LocalizationStringTable> m_string_tables;
    #endregion Member variables
  }
}
