#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Rhino.Drawing;
using System.Globalization;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
//using System.Security.Permissions;

using Rhino.Geometry;

namespace Rhino
{
  /// <summary> PersistentSettings contains a dictionary of these items. </summary>
  class SettingValue //: ISerializable
  {
    ///// <summary>
    ///// ISerializable constructor.
    ///// </summary>
    ///// <param name="info">Serialization data.</param>
    ///// <param name="context">Serialization stream.</param>
    //protected SettingValue(SerializationInfo info, StreamingContext context)
    //{
    //  m_value = info.GetString("value");
    //  m_default_value = info.GetString("default_value");
    //}

    /// <summary> Constructor. </summary>
    /// <param name="value">Current value string.</param>
    /// <param name="default_value">Default value string.</param>
    public SettingValue(string value, string default_value)
    {
      if (!string.IsNullOrEmpty(value))
        m_value = value;
      if (!string.IsNullOrEmpty(default_value))
        m_default_value = default_value;
    }

    ///// <summary> ISerializable required method. </summary>
    ///// <param name="info">Serialization data.</param>
    ///// <param name="context">Serialization stream.</param>
    //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    //void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    //{
    //  info.AddValue("value", m_value);
    //  info.AddValue("default_value", m_default_value);
    //}
    /// <summary>
    /// Copies values from another SettingsValue object. If the destination contains more than one item,
    /// assumes it is a string list and appends values from the source object that are not currently in
    /// the array.
    /// </summary>
    /// <param name="source">The source settings.</param>
    public void CopyFrom(SettingValue source)
    {
      if (null != source)
      {
        m_value = source.m_value;
        m_default_value = source.m_default_value;
      }
    }
    /// <summary>
    /// Determines if two SettingsValue have the same data. Does not compare default values.
    /// </summary>
    /// <param name="other">The other value.</param>
    /// <returns>true if this and other setting have the same value, without comparing the default. Otherwise, false.</returns>
    public bool ValuesAreEqual(SettingValue other)
    {
      return ValuesAreEqual(other, false);
    }
    /// <summary>
    /// Determines if two SettingsValues have the same data and optionally compares default data.
    /// </summary>
    /// <param name="other">The other value.</param>
    /// <param name="compareDefaults">true if the default value should be compared.</param>
    /// <returns>true if this and other setting have the same value, optionally comparing the default. Otherwise, false.</returns>
    public bool ValuesAreEqual(SettingValue other, bool compareDefaults)
    {
      if (null == other)
        return false;
      if (0 != string.Compare(m_value, other.m_value, StringComparison.Ordinal))
        return false;
      if (compareDefaults && 0 != string.Compare(m_default_value, other.m_default_value, StringComparison.Ordinal))
        return false;
      return true;
    }
    /// <summary>
    /// Set either the current or default value string.
    /// </summary>
    /// <param name="bDefault">If true then the current value string is set otherwise the default value string is.</param>
    /// <param name="s">New value string.</param>
    public void SetValue(bool bDefault, string s)
    {
      if (bDefault)
        m_default_value = s;
      else
        m_value = s;
    }
    /// <summary>
    /// Gets the current or default value as requested.
    /// </summary>
    /// <param name="bDefault">If true then the default value string is returned otherwise the current value string is returned.</param>
    /// <returns>If bDefault is true then the default value string is returned otherwise the current value string is returned.</returns>
    public string GetValue(bool bDefault)
    {
      return (bDefault ? m_default_value : m_value);
    }
    /// <summary>
    /// Compare current and default values and return true if they are identical, compare is case sensitive.
    /// </summary>
    public bool ValueSameAsDefault { get { return (0 == string.Compare(m_value, m_default_value, StringComparison.Ordinal)); } }
    /// <summary>
    /// Compare current and default values and return true if they differ, compare is case sensitive.
    /// </summary>
    public bool ValueDifferentThanDefault { get { return (!ValueSameAsDefault); } }

    public bool TryGetBool(bool bDefault, out bool value)
    {
      return bool.TryParse(GetValue(bDefault), out value);
    }

    public bool TryGetByte(bool bDefault, out byte value)
    {
      return byte.TryParse(GetValue(bDefault), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out value);
    }

    public bool TryGetInteger(bool bDefault, out int value)
    {
      return int.TryParse(GetValue(bDefault), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out value);
    }

    public bool TryGetUnsignedInteger(bool bDefault, out uint value)
    {
      return uint.TryParse(GetValue(bDefault), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out value);
    }

    public bool TryGetDouble(bool bDefault, out double value)
    {
      return double.TryParse(GetValue(bDefault), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out value);
    }

    public bool TryGetChar(bool bDefault, out char value)
    {
      return char.TryParse(GetValue(bDefault), out value);
    }

    public bool TryGetString(bool bDefault, out string value)
    {
      value = GetValue(bDefault);
      return true;
    }
    /// <summary>
    /// I was going to use Path.PathSeparator, ';' which works when specifying a path but is a valid file name character so
    /// it does not work in a file name list, the '|' character is in both the Path.GetInvalidFileNameChars() and 
    /// Path.GetInvalidPathChars() list of characters so I went ahead and used it for now.
    /// </summary>
    public static readonly char StringListSeparator = '|';

    /// <summary>
    /// Gets the <see cref="StringListSeparator"/> value in an array.
    /// </summary>
    public static char[] StringListSeparatorAsArray { get { return new char[] { StringListSeparator }; } }

    public static readonly string StringListRootKey = "%root%";

    public bool TryGetStringList(bool bDefault, string rootString, out string[] value)
    {
      value = null;
      string s = GetValue(bDefault);
      if (!string.IsNullOrEmpty(s))
      {
        string listSeporator = new string(StringListSeparatorAsArray);
        s = s.Replace(StringListSeparator + StringListRootKey + StringListSeparator, string.IsNullOrEmpty(rootString) ? listSeporator : listSeporator + rootString + listSeporator);
        s = s.Replace(StringListSeparator + StringListRootKey, string.IsNullOrEmpty(rootString) ? string.Empty : listSeporator + rootString);
        s = s.Replace(StringListRootKey + StringListSeparator, string.IsNullOrEmpty(rootString) ? string.Empty : rootString + listSeporator);
        s = s.Replace(StringListRootKey, string.IsNullOrEmpty(rootString) ? string.Empty : rootString);
        value = s.Split(StringListSeparatorAsArray);
      }
      return true;
    }

    public bool TryGetDate(bool bDefault, out DateTime value)
    {
      return DateTime.TryParse(GetValue(bDefault), CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out value);
    }

    public bool TryGetColor(bool bDefault, out Color value)
    {
      value = Color.Empty;
      string getValue = GetValue(bDefault);
      
      if (string.IsNullOrEmpty(getValue))
        return true; // Color set to Color.Empty

      string[] argb = getValue.Split(',');

      if (argb.Length != 4)
        return false;

      Int32 alpha, red, green, blue;
      if (   Int32.TryParse(argb[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out alpha)
          && Int32.TryParse(argb[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out red)
          && Int32.TryParse(argb[2], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out green)
          && Int32.TryParse(argb[3], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out blue))
      {
        value = Color.FromArgb(alpha, red, green, blue);
        return true;
      }

      return false;
    }

    public bool TryGetPoint3d(bool bDefault, out Point3d value)
    {
      value = Point3d.Unset;
      string[] point = GetValue(bDefault).Split(',');

      if (point.Length != 3)
        return false;

      double x, y, z;
      if (   double.TryParse(point[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out x)
          && double.TryParse(point[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out y)
          && double.TryParse(point[2], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out z))
      {
        value = new Point3d(x, y, z);
        return true;
      }

      return false;
    }

    public bool TryGetSize(bool bDefault, out Size value)
    {
      value = Size.Empty;
      string[] size = GetValue(bDefault).Split(',');

      if (size.Length != 2)
        return false;

      int width, height;
      if (int.TryParse(size[0], System.Globalization.NumberStyles.Any,
        CultureInfo.InvariantCulture.NumberFormat, out width)
        && int.TryParse(size[1], System.Globalization.NumberStyles.Any,
        CultureInfo.InvariantCulture.NumberFormat, out height))
      {
        value = new Size(width, height);
        return true;
      }

      return false;
    }

    public bool TryGetPoint(bool bDefault, out Rhino.Drawing.Point value)
    {
      Size sz;
      bool rc = TryGetSize(bDefault, out sz);
      value = new Rhino.Drawing.Point(sz.Width, sz.Height);
      return rc;
    }

    public bool TryGetRectangle(bool bDefault, out Rhino.Drawing.Rectangle value)
    {
      value = Rectangle.Empty;

      string[] rect = GetValue(bDefault).Split(',');

      if (rect.Length != 4)
        return false;

      int x, y, width, height;

      if (   int.TryParse(rect[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out x)
          && int.TryParse(rect[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out y)
          && int.TryParse(rect[2], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out width)
          && int.TryParse(rect[3], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out height))
      {
        value = new Rhino.Drawing.Rectangle(x, y, width, height);
        return true;
      }

      return false;
    }

    public void SetBool(bool bDefault, bool value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        bool old_value;
        TryGetBool(bDefault, out old_value);
        PersistentSettingsEventArgs<bool> a = new PersistentSettingsEventArgs<bool>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      SetValue(bDefault, value.ToString());
    }

    public void SetByte(bool bDefault, byte value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        byte old_value;
        TryGetByte(bDefault, out old_value);
        PersistentSettingsEventArgs<byte> a = new PersistentSettingsEventArgs<byte>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      SetValue(bDefault, value.ToString(CultureInfo.InvariantCulture.NumberFormat));
    }

    public void SetInteger(bool bDefault, int value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        int old_value;
        TryGetInteger(bDefault, out old_value);
        PersistentSettingsEventArgs<int> a = new PersistentSettingsEventArgs<int>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      SetValue(bDefault, value.ToString(CultureInfo.InvariantCulture.NumberFormat));
    }

    public void SetUnsignedInteger(bool bDefault, uint value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        uint old_value;
        TryGetUnsignedInteger(bDefault, out old_value);
        PersistentSettingsEventArgs<uint> a = new PersistentSettingsEventArgs<uint>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      SetValue(bDefault, value.ToString(CultureInfo.InvariantCulture.NumberFormat));
    }

    public void SetDouble(bool bDefault, double value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        double old_value;
        TryGetDouble(bDefault, out old_value);
        PersistentSettingsEventArgs<double> a = new PersistentSettingsEventArgs<double>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      SetValue(bDefault, value.ToString(CultureInfo.InvariantCulture.NumberFormat));
    }

    public void SetChar(bool bDefault, char value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        char old_value;
        TryGetChar(bDefault, out old_value);
        PersistentSettingsEventArgs<char> a = new PersistentSettingsEventArgs<char>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      SetValue(bDefault, value.ToString());
    }

    public void SetString(bool bDefault, string value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        string old_value;
        TryGetString(bDefault, out old_value);
        PersistentSettingsEventArgs<string> a = new PersistentSettingsEventArgs<string>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      SetValue(bDefault, value);
    }

    public void SetStringList(bool bDefault, string[] value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        string[] old_value;
        TryGetStringList(bDefault, string.Empty, out old_value);
        PersistentSettingsEventArgs<string[]> a = new PersistentSettingsEventArgs<string[]>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      string newValue = string.Empty;
      if (null != value)
      {
        foreach (var s in value)
        {
          if (!string.IsNullOrEmpty(newValue))
            newValue += SettingValue.StringListSeparator;
          newValue += s;
        }
      }
      SetValue(bDefault, newValue);
    }

    public void SetDate(bool bDefault, DateTime value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        DateTime old_value;
        TryGetDate(bDefault, out old_value);
        PersistentSettingsEventArgs<DateTime> a = new PersistentSettingsEventArgs<DateTime>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      SetValue(bDefault, value.ToString("F", CultureInfo.InvariantCulture));
    }

    public void SetColor(bool bDefault, Color value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        Color old_value;
        TryGetColor(bDefault, out old_value);
        PersistentSettingsEventArgs<Color> a = new PersistentSettingsEventArgs<Color>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      if (value.IsEmpty)
        SetValue(bDefault, string.Empty);
      else
        SetValue(bDefault, string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}",
                                         value.A.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                         value.R.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                         value.G.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                         value.B.ToString(CultureInfo.InvariantCulture.NumberFormat)));
    }

    public void SetPoint3d(bool bDefault, Point3d value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        Point3d old_value;
        TryGetPoint3d(bDefault, out old_value);
        PersistentSettingsEventArgs<Point3d> a = new PersistentSettingsEventArgs<Point3d>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      SetValue(bDefault, string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}",
                                       value.m_x.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                       value.m_y.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                       value.m_z.ToString(CultureInfo.InvariantCulture.NumberFormat)));
    }

    public void SetRectangle(bool bDefault, Rectangle value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        Rectangle old_value;
        TryGetRectangle(bDefault, out old_value);
        PersistentSettingsEventArgs<Rectangle> a = new PersistentSettingsEventArgs<Rectangle>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      SetValue(bDefault, string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}",
                                       value.Left.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                       value.Top.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                       value.Width.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                       value.Height.ToString(CultureInfo.InvariantCulture.NumberFormat)));
    }

    public void SetSize(bool bDefault, Size value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        Size old_value;
        TryGetSize(bDefault, out old_value);
        PersistentSettingsEventArgs<Size> a = new PersistentSettingsEventArgs<Size>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      SetValue(bDefault, string.Format(CultureInfo.InvariantCulture, "{0},{1}",
                                       value.Width.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                       value.Height.ToString(CultureInfo.InvariantCulture.NumberFormat)));
    }

    public void SetPoint(bool bDefault, Rhino.Drawing.Point value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        Rhino.Drawing.Point old_value;
        TryGetPoint(bDefault, out old_value);
        PersistentSettingsEventArgs<Rhino.Drawing.Point> a = new PersistentSettingsEventArgs<Rhino.Drawing.Point>(old_value, value);
        validator(this, a);
        if (a.Cancel)
          return;
        value = a.NewValue;
      }
      SetValue(bDefault, string.Format(CultureInfo.InvariantCulture, "{0},{1}",
                                       value.X.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                       value.Y.ToString(CultureInfo.InvariantCulture.NumberFormat)));
    }

    private string m_value;
    private string m_default_value;
  }

  /// <summary>
  /// Represents event data that is passed as state in persistent settings events.
  /// </summary>
  public abstract class PersistentSettingsEventArgs : EventArgs
  {
    protected PersistentSettingsEventArgs()
    {
      Cancel = false;
    }
    public bool Cancel
    {
      get;
      set;
    }
  }

  /// <summary>
  /// Represents the persistent settings modification event arguments.
  /// </summary>
  /// <typeparam name="T">The type of the current and new setting that is being modified.</typeparam>
  public class PersistentSettingsEventArgs<T> : PersistentSettingsEventArgs
  {
    public PersistentSettingsEventArgs(T currentValue, T newValue)
    {
      CurrentValue = currentValue;
      NewValue = newValue;
    }

    public T CurrentValue { get; set; }
    public T NewValue { get; private set; }
  }


  /// <summary>
  /// A dictionary of SettingValue items.
  /// </summary>
  //[Serializable]
  public class PersistentSettings //: ISerializable
  {
    readonly Dictionary<string, SettingValue> m_Settings;
    readonly Dictionary<string, EventHandler<PersistentSettingsEventArgs>> m_SettingsValidators;

    //protected PersistentSettings(SerializationInfo info, StreamingContext context)
    //{
    //}

    //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    //void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    //{
    //  m_Settings.GetObjectData(info, context);
    //}

    private readonly PersistentSettings AllUserSettings;

    public static PersistentSettings FromPlugInId(Guid pluginId)
    {
#if RHINO_SDK
      PersistentSettingsManager manager = PersistentSettingsManager.Create(pluginId);
      return manager.PluginSettings;
#else
      return null;
#endif
    }

    public PersistentSettings(PersistentSettings allUserSettings)
    {
      AllUserSettings = allUserSettings;
      m_Settings = new Dictionary<string, SettingValue>();
      m_SettingsValidators = new Dictionary<string, EventHandler<PersistentSettingsEventArgs>>();
    }

    internal void CopyFrom(PersistentSettings source)
    {
      if (null != source)
      {
        foreach (var item in source.m_Settings)
        {
          if (m_Settings.ContainsKey(item.Key))
            m_Settings[item.Key].CopyFrom(item.Value);
          else
            m_Settings.Add(item.Key, new SettingValue(item.Value.GetValue(false), item.Value.GetValue(true)));
        }
      }
    }

    public void RegisterSettingsValidator(string key, EventHandler<PersistentSettingsEventArgs> validator)
    {
      m_SettingsValidators[key] = validator;
    }

    public EventHandler<PersistentSettingsEventArgs> GetValidator(string key)
    {
      EventHandler<PersistentSettingsEventArgs> validator;
      m_SettingsValidators.TryGetValue(key, out validator);
      return validator;
    }

    public bool ContainsModifiedValues(PersistentSettings allUserSettings)
    {
      if (null != m_Settings && m_Settings.Count > 0)
      {
        foreach (var v in m_Settings)
        {
          if (v.Value.ValueDifferentThanDefault)
            return true;
          if (null != allUserSettings && allUserSettings.m_Settings.ContainsKey(v.Key) && 0 != string.Compare(v.Value.GetValue(false), allUserSettings.m_Settings[v.Key].GetValue(false), StringComparison.Ordinal))
            return true;
        }
      }
      return false;
    }

    internal string this[string key]
    {
      get
      {
        return m_Settings[key].GetValue(false);
      }
      set
      {
        if (m_Settings.ContainsKey(key))
          m_Settings[key].SetValue(false, value);
        else
          m_Settings.Add(key, new SettingValue(value, ""));
      }
    }

    internal SettingValue GetValue(string key)
    {
      if (!m_Settings.ContainsKey(key))
        m_Settings.Add(key, new SettingValue("", ""));
      return m_Settings[key];
    }

    internal Dictionary<string, SettingValue>.KeyCollection Keys
    {
      get { return m_Settings.Keys; }
    }

    internal Dictionary<string, SettingValue> Dict
    {
      get { return m_Settings; }
    }

    public bool TryGetBool(string key, out bool value)
    {
      value = false;
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetBool(false, out value);
      return false;
    }

    public bool GetBool(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      bool rc;
      if (TryGetBool(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a bool");
    }

    public bool GetBool(string key, bool defaultValue)
    {
      bool rc;
      if (TryGetBool(key, out rc))
      {
        m_Settings[key].SetBool(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetBool(false, defaultValue, GetValidator(key));
      return GetBool(key);
    }

    public bool TryGetByte(string key, out byte value)
    {
      value = 0;
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetByte(false, out value);
      return false;
    }

    public byte GetByte(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      byte rc;
      if (TryGetByte(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a byte");
    }

    public byte GetByte(string key, byte defaultValue)
    {
      byte rc;
      if (TryGetByte(key, out rc))
      {
        m_Settings[key].SetByte(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetByte(false, defaultValue, GetValidator(key));
      return GetByte(key);
    }

    public bool TryGetInteger(string key, out int value)
    {
      value = int.MaxValue;
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetInteger(false, out value);
      return false;
    }

    public int GetInteger(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      int rc;
      if (TryGetInteger(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a integer");
    }

    public int GetInteger(string key, int defaultValue)
    {
      int rc;
      if (TryGetInteger(key, out rc))
      {
        m_Settings[key].SetInteger(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetInteger(false, defaultValue, GetValidator(key));
      return GetInteger(key);
    }

    [CLSCompliant(false)]
    public bool TryGetUnsignedInteger(string key, out uint value)
    {
      value = uint.MaxValue;
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetUnsignedInteger(false, out value);
      return false;
    }

    [CLSCompliant(false)]
    public uint GetUnsignedInteger(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      uint rc;
      if (TryGetUnsignedInteger(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a integer");
    }

    [CLSCompliant(false)]
    public uint GetUnsignedInteger(string key, uint defaultValue)
    {
      uint rc;
      if (TryGetUnsignedInteger(key, out rc))
      {
        m_Settings[key].SetUnsignedInteger(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetUnsignedInteger(false, defaultValue, GetValidator(key));
      return GetUnsignedInteger(key);
    }

    public bool TryGetDouble(string key, out double value)
    {
      value = double.MaxValue;
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetDouble(false, out value);
      return false;
    }

    public double GetDouble(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      double rc;
      if (TryGetDouble(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a double");
    }

    public double GetDouble(string key, double defaultValue)
    {
      double rc;
      if (TryGetDouble(key, out rc))
      {
        m_Settings[key].SetDouble(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetDouble(false, defaultValue, GetValidator(key));
      return GetDouble(key);
    }

    public bool TryGetChar(string key, out char value)
    {
      value = char.MaxValue;
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetChar(false, out value);
      return false;
    }

    public char GetChar(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      char rc;
      if (TryGetChar(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a char");
    }

    public char GetChar(string key, char defaultValue)
    {
      char rc;
      if (TryGetChar(key, out rc))
      {
        m_Settings[key].SetChar(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetChar(false, defaultValue, GetValidator(key));
      return GetChar(key);
    }

    public bool TryGetString(string key, out string value)
    {
      value = "";
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetString(false, out value);
      return false;
    }

    public string GetString(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      string rc;
      if (TryGetString(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a string");
    }

    public string GetString(string key, string defaultValue)
    {
      string rc;
      if (TryGetString(key, out rc))
      {
        m_Settings[key].SetString(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetString(false, defaultValue, GetValidator(key));
      return GetString(key);
    }

    public bool TryGetStringList(string key, out string[] value)
    {
      value = null;
      if (m_Settings.ContainsKey(key))
      {
        string rootString = string.Empty;
        if (null != AllUserSettings && AllUserSettings.m_Settings.ContainsKey(key))
          rootString = AllUserSettings.m_Settings[key].GetValue(false);
        return m_Settings[key].TryGetStringList(false, rootString, out value);
      }
      return false;
    }

    public string[] GetStringList(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      string[] rc;
      if (TryGetStringList(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a string list");
    }

    public string[] GetStringList(string key, string[] defaultValue)
    {
      string[] rc;
      if (TryGetStringList(key, out rc))
      {
        m_Settings[key].SetStringList(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetStringList(false, defaultValue, GetValidator(key));
      return GetStringList(key);
    }

    public bool TryGetDate(string key, out DateTime value)
    {
      value = DateTime.MinValue;
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetDate(false, out value);
      return false;
    }

    public DateTime GetDate(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      DateTime rc;
      if (TryGetDate(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a DateTime");
    }

    public DateTime GetDate(string key, DateTime defaultValue)
    {
      DateTime rc;
      if (TryGetDate(key, out rc))
      {
        m_Settings[key].SetDate(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetDate(false, defaultValue, GetValidator(key));
      return GetDate(key);
    }

    public bool TryGetColor(string key, out Color value)
    {
      value = Color.Empty;
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetColor(false, out value);
      return false;
    }

    public Color GetColor(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      Color rc;
      if (TryGetColor(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a Color");
    }

    public Color GetColor(string key, Color defaultValue)
    {
      Color rc;
      if (TryGetColor(key, out rc))
      {
        m_Settings[key].SetColor(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetColor(false, defaultValue, GetValidator(key));
      return GetColor(key);
    }

    public bool TryGetPoint(string key, out Rhino.Drawing.Point value)
    {
      value = Rhino.Drawing.Point.Empty;
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetPoint(false, out value);
      return false;
    }

    public Rhino.Drawing.Point GetPoint(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      Rhino.Drawing.Point rc;
      if (TryGetPoint(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a Point");
    }

    public Rhino.Drawing.Point GetPoint(string key, Rhino.Drawing.Point defaultValue)
    {
      Rhino.Drawing.Point rc;
      if (TryGetPoint(key, out rc))
      {
        m_Settings[key].SetPoint(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetPoint(false, defaultValue, GetValidator(key));
      return GetPoint(key);
    }

    public bool TryGetPoint3d(string key, out Point3d value)
    {
      value = Point3d.Unset;
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetPoint3d(false, out value);
      return false;
    }

    public Point3d GetPoint3d(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      Point3d rc;
      if (TryGetPoint3d(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a Point3d");
    }

    public Point3d GetPoint3d(string key, Point3d defaultValue)
    {
      Point3d rc;
      if (TryGetPoint3d(key, out rc))
      {
        m_Settings[key].SetPoint3d(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetPoint3d(false, defaultValue, GetValidator(key));
      return GetPoint3d(key);
    }

    public bool TryGetSize(string key, out Size value)
    {
      value = Size.Empty;
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetSize(false, out value);
      return false;
    }

    public Size GetSize(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      Size rc;
      if (TryGetSize(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a Size");
    }

    public Size GetSize(string key, Size defaultValue)
    {
      Size rc;
      if (TryGetSize(key, out rc))
      {
        m_Settings[key].SetSize(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetSize(false, defaultValue, GetValidator(key));
      return GetSize(key);
    }

    public bool TryGetRectangle(string key, out Rhino.Drawing.Rectangle value)
    {
      value = Rectangle.Empty;
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetRectangle(false, out value);
      return false;
    }

    public Rectangle GetRectangle(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      Rectangle rc;
      if (TryGetRectangle(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a Rectangle");
    }

    public Rectangle GetRectangle(string key, Rectangle defaultValue)
    {
      Rectangle rc;
      if (TryGetRectangle(key, out rc))
      {
        m_Settings[key].SetRectangle(true, defaultValue, GetValidator(key));
        return rc;
      }
      SetDefault(key, defaultValue);
      m_Settings[key].SetRectangle(false, defaultValue, GetValidator(key));
      return GetRectangle(key);
    }

    public bool TryGetDefault(string key, out bool value)
    {
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetBool(true, out value);
      value = false;
      return false;
    }

    public bool TryGetDefault(string key, out byte value)
    {
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetByte(true, out value);
      value = 0;
      return false;
    }

    public bool TryGetDefault(string key, out int value)
    {
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetInteger(true, out value);
      value = 0;
      return false;
    }

    public bool TryGetDefault(string key, out double value)
    {
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetDouble(true, out value);
      value = 0;
      return false;
    }

    public bool TryGetDefault(string key, out char value)
    {
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetChar(true, out value);
      value = '\0';
      return false;
    }

    public bool TryGetDefault(string key, out string value)
    {
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetString(true, out value);
      value = null;
      return false;
    }

    public bool TryGetDefault(string key, out string[] value)
    {
      if (m_Settings.ContainsKey(key))
      {
        string rootString = string.Empty;
        if (null != AllUserSettings && AllUserSettings.m_Settings.ContainsKey(key))
          rootString = AllUserSettings.m_Settings[key].GetValue(true);
        return m_Settings[key].TryGetStringList(true, rootString, out value);
      }
      value = null;
      return false;
    }

    public bool TryGetDefault(string key, out DateTime value)
    {
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetDate(true, out value);
      value = DateTime.MinValue;
      return false;
    }

    public bool TryGetDefault(string key, out Color value)
    {
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetColor(true, out value);
      value = Color.Empty;
      return false;
    }

    public bool TryGetDefault(string key, out Point3d value)
    {
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetPoint3d(true, out value);
      value = Point3d.Unset;
      return false;
    }

    public bool TryGetDefault(string key, out Size value)
    {
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetSize(true, out value);
      value = Size.Empty;
      return false;
    }

    public bool TryGetDefault(string key, out Rhino.Drawing.Rectangle value)
    {
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetRectangle(true, out value);
      value = Rectangle.Empty;
      return false;
    }

    /// <summary>
    /// Get a stored enum value, or return default value if not found
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    [CLSCompliant(false)]
    public T GetEnumValue<T>(T defaultValue)
        where T : struct//, IConvertible
    {
        Type enumType = typeof(T);
        return GetEnumValue(enumType.Name, defaultValue);
    }

    /// <summary>
    /// Get a stored enum value using a custom key, or return default value if not found. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"> </param>
    /// <param name="defaultValue"> </param>
    /// <returns></returns>
    [CLSCompliant(false)]
    public T GetEnumValue<T>(String key, T defaultValue)
        where T : struct//, IConvertible
    {
        if (null == key) throw new ArgumentNullException("key");
        if (!typeof(T).GetTypeInfo().IsEnum) throw new ArgumentException("!typeof(T).GetTypeInfo().IsEnum");

        String value = GetString(key, defaultValue.ToString());
        return (T)Enum.Parse(typeof(T), value);
    }

    /// <summary>
    /// Get a stored enum value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [CLSCompliant(false)]
    public T GetEnumValue<T>()
        where T : struct//, IConvertible
    {
      return GetEnumValue<T>(typeof(T).Name);
    }

    /// <summary>
    /// Get a stored enum value using a custom key.
    /// </summary>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    [CLSCompliant(false)]
    public T GetEnumValue<T>(String key)
        where T : struct//, IConvertible
    {
      if (null == key) throw new ArgumentNullException("key");
      if (!typeof(T).GetTypeInfo().IsEnum) throw new ArgumentException("!typeof(T).GetTypeInfo().IsEnum");

      Type enumType = typeof(T);

      String value;
      if (TryGetString(key, out value))
      {
        return (T)Enum.Parse(typeof(T), value);
      }
      String errMsg = String.Format("Value for key={0} for enum type {1} not found.", key, enumType.Name);
      throw new KeyNotFoundException(errMsg);
    }

    /// <summary>
    /// Attempt to get the stored value for an enum setting using a custom key. Note: the enum value ALWAYS gets assigned!
    /// Be sure to check for success of this method to prevent erroneous use of the value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"> </param>
    /// <param name="enumValue"></param>
    /// <returns>true if successful</returns>
    [CLSCompliant(false)]
    public bool TryGetEnumValue<T>(String key, out T enumValue)
        where T : struct//, IConvertible
    {
      if (null == key) throw new ArgumentNullException("key");
      Type enumType = typeof(T);
      if (!enumType.GetTypeInfo().IsEnum) throw new ArgumentException("!typeof(T).GetTypeInfo().IsEnum");

      enumValue = default(T);

      String value;
      if (TryGetString(key, out value))
      {
        try
        {
          enumValue = (T)Enum.Parse(typeof(T), value);
          return true;
        }
        catch (Exception)
        {
          // do nothing, fall through and return false
        }
      }
      return false;
    }

    /// <summary>
    /// Attempt to get the stored value for an enum setting. Note: the enum value ALWAYS gets assigned!
    /// Be sure to check for success of this method to prevent erroneous use of the value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    [CLSCompliant(false)]
    public bool TryGetEnumValue<T>(out T enumValue)
        where T : struct//, IConvertible
    {
      Type enumType = typeof(T);
      if (!enumType.GetTypeInfo().IsEnum)
        throw new ArgumentException("!typeof(T).GetTypeInfo().IsEnum");

      return TryGetEnumValue(enumType.Name, out enumValue);
    }

    public void SetBool(string key, bool value)
    {
      GetValue(key).SetBool(false, value, GetValidator(key));
    }

    public void SetByte(string key, byte value)
    {
      GetValue(key).SetByte(false, value, GetValidator(key));
    }

    public void SetInteger(string key, int value)
    {
      GetValue(key).SetInteger(false, value, GetValidator(key));
    }

    [CLSCompliant(false)]
    public void SetUnsignedInteger(string key, uint value)
    {
      GetValue(key).SetUnsignedInteger(false, value, GetValidator(key));
    }

    public void SetDouble(string key, double value)
    {
      GetValue(key).SetDouble(false, value, GetValidator(key));
    }

    public void SetChar(string key, char value)
    {
      GetValue(key).SetChar(false, value, GetValidator(key));
    }

    public void SetString(string key, string value)
    {
      GetValue(key).SetString(false, value, GetValidator(key));
    }
    /// <summary>
    /// Adding this string to a string list when calling SetStringList will cause the ProgramData setting to
    /// get inserted at that location in the list.
    /// </summary>
    public static string StringListRootKey { get { return SettingValue.StringListRootKey; } }
    /// <summary>
    /// Including a item with the value of StringListRootKey will cause the ProgramData value to get inserted at
    /// that location in the list when calling GetStringList.
    /// </summary>
    /// <param name="key">The string key.</param>
    /// <param name="value">An array of values to set.</param>
    public void SetStringList(string key, string[] value)
    {
      GetValue(key).SetStringList(false, value, GetValidator(key));
    }

    public void DeleteItem(string key)
    {
      if (m_Settings.ContainsKey(key))
        m_Settings.Remove(key);
    }

    public void SetDate(string key, DateTime value)
    {
      GetValue(key).SetDate(false, value, GetValidator(key));
    }

    public void SetColor(string key, Color value)
    {
      GetValue(key).SetColor(false, value, GetValidator(key));
    }

    public void SetPoint3d(string key, Point3d value)
    {
      GetValue(key).SetPoint3d(false, value, GetValidator(key));
    }

    public void SetRectangle(string key, Rhino.Drawing.Rectangle value)
    {
      GetValue(key).SetRectangle(false, value, GetValidator(key));
    }

    public void SetSize(string key, Size value)
    {
      GetValue(key).SetSize(false, value, GetValidator(key));
    }

    public void SetPoint(string key, Rhino.Drawing.Point value)
    {
      GetValue(key).SetPoint(false, value, GetValidator(key));
    }

    public void SetDefault(string key, bool value)
    {
      GetValue(key).SetBool(true, value, GetValidator(key));
    }

    public void SetDefault(string key, byte value)
    {
      GetValue(key).SetByte(true, value, GetValidator(key));
    }

    public void SetDefault(string key, int value)
    {
      GetValue(key).SetInteger(true, value, GetValidator(key));
    }

    public void SetDefault(string key, double value)
    {
      GetValue(key).SetDouble(true, value, GetValidator(key));
    }

    public void SetDefault(string key, char value)
    {
      GetValue(key).SetChar(true, value, GetValidator(key));
    }

    public void SetDefault(string key, string value)
    {
      GetValue(key).SetString(true, value, GetValidator(key));
    }

    public void SetDefault(string key, string[] value)
    {
      GetValue(key).SetStringList(true, value, GetValidator(key));
    }

    public void SetDefault(string key, DateTime value)
    {
      GetValue(key).SetDate(true, value, GetValidator(key));
    }

    public void SetDefault(string key, Color value)
    {
      GetValue(key).SetColor(true, value, GetValidator(key));
    }

    public void SetDefault(string key, Rhino.Drawing.Rectangle value)
    {
      GetValue(key).SetRectangle(true, value, GetValidator(key));
    }

    public void SetDefault(string key, Size value)
    {
      GetValue(key).SetSize(true, value, GetValidator(key));
    }

    public void SetDefault(string key, Rhino.Drawing.Point value)
    {
      GetValue(key).SetPoint(true, value, GetValidator(key));
    }

    public void SetDefault(string key, Point3d value)
    {
      GetValue(key).SetPoint3d(true, value, GetValidator(key));
    }

    /// <summary>
    /// Set an enum value in the settings.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumValue"></param>
    [CLSCompliant(false)]
    public void SetEnumValue<T>(T enumValue)
        where T : struct//, IConvertible
    {
        Type enumType = typeof(T);
        SetEnumValue(enumType.Name, enumValue);
    }

    /// <summary>
    /// Set an enum value in the settings using a custom key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"> </param>
    /// <param name="enumValue"></param>
    [CLSCompliant(false)]
    public void SetEnumValue<T>(String key, T enumValue)
        where T : struct//, IConvertible
    {
        if (null == key) throw new ArgumentNullException("key");

        if (!typeof(T).GetTypeInfo().IsEnum) 
            throw new ArgumentException("!typeof(T).GetTypeInfo().IsEnum");

        SetString(key, enumValue.ToString());
    }

    /// <summary>
    /// If the settings dictionary contains one or more values, which are not equal to the default value, then Write the contents
    /// of this settings dictionary to the specified XmlWriter contained within elementName.
    /// </summary>
    /// <param name="xmlWriter">XmlWriter object to write to.</param>
    /// <param name="elementName">Element which will contain key value pairs.</param>
    /// <param name="attributeName">Optional element attribute.</param>
    /// <param name="attributeValue">Optional element attribute value.</param>
    /// <param name="allUserSettings">All users settings to compare with.</param>
    internal void WriteXmlElement(XmlWriter xmlWriter, string elementName, string attributeName, string attributeValue, PersistentSettings allUserSettings)
    {
      if (null != m_Settings && ContainsModifiedValues(allUserSettings))
      {
        xmlWriter.WriteStartElement(elementName);
        if (!string.IsNullOrEmpty(attributeName) && !string.IsNullOrEmpty(attributeValue))
          xmlWriter.WriteAttributeString(attributeName, attributeValue);

        foreach (var item in m_Settings)
        {
          string allUserValue = null;
          if (null != allUserSettings && allUserSettings.m_Settings.ContainsKey(item.Key))
            allUserValue = allUserSettings.m_Settings[item.Key].GetValue(false);
          string value = item.Value.GetValue(false);
          bool valueDifferentThanAllUser = (null != allUserValue && 0 != string.Compare(value, allUserValue, StringComparison.Ordinal));
          if (valueDifferentThanAllUser || item.Value.ValueDifferentThanDefault)
          {
            // Write current value
            xmlWriter.WriteStartElement("entry");
            xmlWriter.WriteAttributeString("key", item.Key);

            // The following is used when you want to write the default and all user values as item attributes
            // to the settings output file, useful when trying to determine why a value was written
            //const bool bWriteDefaultValue = false;
            //if (bWriteDefaullValue)
            //{
            //  string defaultValue = item.Value.GetValue(true);
            //  xmlWriter.WriteAttributeString("DefaultValue", null == defaultValue ? "" : defaultValue);
            //  if (null != allUserValue)
            //    xmlWriter.WriteAttributeString("AllUsersValue", allUserValue);
            //}

            if (!string.IsNullOrEmpty(value))
              xmlWriter.WriteString(value);
            xmlWriter.WriteEndElement();
          }
        }
        xmlWriter.WriteEndElement();
      }
    }
    ///// <summary>
    ///// Parse XmlNode for settings "entry" elements, add entry elements to the dictionary
    ///// first and if then check the defaults list and make sure the entry is in the list before setting the 
    ///// default value.
    ///// </summary>
    //internal void ParseXmlNodes(XmlNode nodeRoot)
    //{
    //  if (null != m_Settings && null != nodeRoot)
    //  {
    //    XmlNodeList nodeList = nodeRoot.SelectNodes("./entry");
    //    if (nodeList != null)
    //    {
    //      foreach (XmlNode entry in nodeList)
    //      {
    //        XmlNode attr = null == entry.Attributes ? null : entry.Attributes["key"];
    //        if (attr != null && !string.IsNullOrEmpty(attr.Value))
    //        {
    //          SetString(attr.Value, entry.InnerText);
    //          //// Set this to true if you want to read the "DefaultValue" attribute from the node
    //          //const bool bSetDefault = false;
    //          //if (bSetDefault)
    //          //{
    //          //  XmlNode attrDefault = null == entry.Attributes ? null : entry.Attributes["DefaultValue"];
    //          //  if (null != attrDefault && !string.IsNullOrEmpty(attrDefault.Value))
    //          //    SetDefault(attr.Value, attrDefault.Value);
    //          //}
    //        }
    //      }
    //    }
    //  }
    //}

    /* commented out since this does not appear to be used anywhere
    private string[] XmlNodeToStringArray(XmlNode node)
    {
      if (null == node)
        return null;
      XmlNodeList nodeList = node.SelectNodes("item");
      if (null == nodeList || nodeList.Count < 1)
        return (string.IsNullOrEmpty(node.InnerText) ? null : new string[] { node.InnerText });
      string[] result = new string[nodeList.Count];
      for (int i = 0, cnt = nodeList.Count; i < cnt; i++)
        result[i] = nodeList[i].InnerText;
      return result;
    }
    */
  }

#if RHINO_SDK
  class PlugInSettings
  {
    private readonly System.Reflection.Assembly m_assembly; // plug-in or skin assembly
    private readonly Guid m_plugin_id;
    private readonly PlugInSettings AllUserSettings;
    private PersistentSettings m_PluginSettings; // = null; initialized by runtime
    private Dictionary<string, PersistentSettings> m_CommandSettingsDict; // = null; initialized by runtime
    private PersistentSettings AllUserPlugInSettings { get { return (null == AllUserSettings ? null : AllUserSettings.m_PluginSettings); } }
    private PersistentSettings AllUserCommandSettings(string commandName)
    {
      if (null != AllUserSettings && !string.IsNullOrEmpty(commandName) && null != AllUserSettings.m_CommandSettingsDict && AllUserSettings.m_CommandSettingsDict.ContainsKey(commandName))
        return AllUserSettings.m_CommandSettingsDict[commandName];
      return null;
    }
    /// <summary>
    /// Main settings element id attribute value, used to query valid settings section in settings XML file.
    /// </summary>
    private const string CURRENT_XML_FORMAT_VERSION = "1.0";

    /// <summary>
    /// Computes folder to read or write settings files.
    /// </summary>
    private string SettingsFileFolder(bool localSettings)
    {
      return Rhino.PlugIns.PlugIn.SettingsDirectoryHelper(localSettings, m_assembly, m_plugin_id);
    }
    /// <summary>
    /// Computes full path to settings file to read or write.
    /// </summary>
    private string SettingsFileName(bool localSettings)
    {
      return Path.Combine(SettingsFileFolder(localSettings), "settings.xml");
    }

    /// <summary>PersistentSettingsManager constructor.</summary>
    /// <param name="pluginAssembly">Requires a valid Skin, DLL or PlugIn object to attach to.</param>
    /// <param name="pluginId">Requires a PlugIn Id to attach to.</param>
    /// <param name="allUserSettings">All user setting to compare for changes.</param>
    internal PlugInSettings(System.Reflection.Assembly pluginAssembly, Guid pluginId, PlugInSettings allUserSettings)
    {
      m_assembly = pluginAssembly;
      m_plugin_id = pluginId;
      AllUserSettings = allUserSettings;
    }
    /// <summary>PersistentSettingsManager constructor</summary>
    /// <param name="pluginId"></param>
    /// <param name="allUserSettings"></param>
    internal PlugInSettings(Guid pluginId, PlugInSettings allUserSettings)
    {
      m_plugin_id = pluginId;
      AllUserSettings = allUserSettings;
    }
    /// <summary>
    /// Gets the Plug-in settings associated with this plug-in, if this is the first time called then
    /// the plug-in settings member variable will get initialized and if a settings file exists it
    /// will get loaded.
    /// </summary>
    public PersistentSettings PluginSettings
    {
      get 
      {
        if (m_PluginSettings == null)
          ReadSettings();
        return m_PluginSettings; 
      }
    }
    /// <summary>
    /// Gets the PersistentSettings associated with the specified command.  If the settings file
    /// has not been previously loaded and exists then it will get read.  If the command name is
    /// not in the command settings dictionary then a new entry will get created and its settings
    /// will be returned.
    /// </summary>
    /// <param name="name">Command name key to search for and/or add.</param>
    /// <returns>Returns PersistentSettings object associated with command name on success or null on error.</returns>
    public PersistentSettings CommandSettings(string name)
    {
      if (m_CommandSettingsDict == null)
      {
        ReadSettings();
        if (m_CommandSettingsDict == null)
          return null;
      }
      if (m_CommandSettingsDict.ContainsKey(name))
        return m_CommandSettingsDict[name];
      // There were no settings available for the command, so create one
      // for writing
      m_CommandSettingsDict[name] = new PersistentSettings(AllUserCommandSettings(name));
      return m_CommandSettingsDict[name];
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool ReadSettings()
    {
      bool result = false;
      // If reading the local settings
      if (null != AllUserSettings)
      {
        // First read All User settings
        result = AllUserSettings.ReadSettingsHelper(false);
        // Now read the local settings
        if (ReadSettingsHelper(true))
          result = true;
      }
      return result;
    }
    /// <summary>
    /// Reads existing settings for a plug-in and its associated commands.
    /// Clears the dirty flag for the settings. 
    /// </summary>
    /// <returns>
    /// true if settings are successfully read. false if there was no existing
    /// settings file to read, or if a read lock could not be acquired.
    /// </returns>
    public bool ReadSettingsHelper(bool localSettings)
    {
      if (m_PluginSettings == null)
      {
        m_PluginSettings = new PersistentSettings(AllUserPlugInSettings);
        // If AllUserSettings is not null then we are reading local settings, when
        // reading local settings first get values previously read from the All Users
        // location and add them to the local dictionary so the settings will propagate
        // to the current user.
        if (localSettings && null != AllUserSettings)
          m_PluginSettings.CopyFrom(AllUserPlugInSettings);
      }

      if (m_CommandSettingsDict == null)
      {
        m_CommandSettingsDict = new Dictionary<string, PersistentSettings>();
        // If AllUserSettings is not null then we are reading local settings, when
        // reading local settings first get values previously read from the All Users
        // location and add them to the local dictionary so the settings will propagate
        // to the current user.
        if (null != AllUserSettings && null != AllUserSettings.m_CommandSettingsDict)
        {
          foreach (var item in AllUserSettings.m_CommandSettingsDict)
          {
            // Make a new settings dictionary to associate with this command
            PersistentSettings settings = new PersistentSettings(item.Value);
            // Copy settings from global command dictionary to local dictionary
            settings.CopyFrom(item.Value);
            // Add the settings to the local dictionary
            m_CommandSettingsDict.Add(item.Key, settings);
          }
        }
      }

      string settingsFileName = SettingsFileName(localSettings);

      if (File.Exists(settingsFileName) == false)
        return false;

      FileStream fs = null;
      try
      {
        bool bKeepTrying = false;
        int lockTryCount = 0;

        // Lame attempt to handle file locking. For performance reasons, only
        // try once more after failure to acquire lock.
        do
        {
          try
          {
            bKeepTrying = false;
            fs = new FileStream(settingsFileName, FileMode.Open, FileAccess.Read);
          }
          catch (IOException ioe)
          {
            if (!(ioe is FileNotFoundException) &&
              !(ioe is DirectoryNotFoundException) &&
              !(ioe is PathTooLongException))
            {
              // File is locked. Try once more then give up.  
              if (lockTryCount < 1)
              {
                bKeepTrying = true;
                lockTryCount++;
                System.Threading.Thread.Sleep(50);
              }
            }
          }
        } while (bKeepTrying);

        // Couldn't acquire lock
        if (fs == null)
          return false;

        XmlDocument doc = new XmlDocument();
        XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
        ns.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace");

        doc.Load(fs);

        fs.Close();
        fs.Dispose();

        // The settings attribute will either be "xml:id" or "id", this will check for "id" if "xml:id" is not found.
        XmlNode rootNode = doc.SelectSingleNode("/settings[@xml:id=\'" + CURRENT_XML_FORMAT_VERSION + "\']", ns) ??
                           doc.SelectSingleNode("/settings[@id=\'" + CURRENT_XML_FORMAT_VERSION + "\']", ns);
        if (rootNode == null)
          return false;

        // Parse main <plug-in> entry, if it exists, for plug-in settings
        m_PluginSettings.ParseXmlNodes(rootNode.SelectSingleNode("./plugin"));

        // Look for <command> nodes which will have a command name property that identifies the plug-in command settings
        XmlNodeList commandNodes = rootNode.SelectNodes("./command");
        if (commandNodes != null)
        {
          foreach (XmlNode commandNode in commandNodes)
          {
            XmlAttributeCollection attr_collection = commandNode.Attributes;
            if( attr_collection==null )
              continue;
            XmlNode attr = attr_collection.GetNamedItem("name");
            if (null != attr && !string.IsNullOrEmpty(attr.Value))
            {
              PersistentSettings entries = new PersistentSettings(AllUserCommandSettings(attr.Value));
              entries.ParseXmlNodes(commandNode);
              if (null != AllUserSettings && m_CommandSettingsDict.ContainsKey(attr.Value))
                m_CommandSettingsDict[attr.Value].CopyFrom(entries);
              else
                m_CommandSettingsDict[attr.Value] = entries;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
        return false;
      }
      finally
      {
        if (fs != null)
        {
          fs.Close();
          fs.Dispose();
        }
      }

      return true;
    }
    /// <summary>
    /// Helper method to delete a directory if it is empty.
    /// </summary>
    /// <param name="directory">Full path to directory to delete.</param>
    /// <returns>Returns true if the directory was empty and successfully deleted otherwise returns false.</returns>
    private bool DeleteDirectory(string directory)
    {
      bool rc = false;
      try
      {
        if (Directory.Exists(directory))
        {
          string[] files = Directory.GetFiles(directory);
          string[] folders = Directory.GetDirectories(directory);
          if ((null == files || files.Length < 1) && (null == folders || folders.Length < 1))
          {
            Directory.Delete(directory);
            rc = true;
          }
        }
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
      return rc;
    }
    /// <summary>
    /// Check the plug-in and command settings dictionaries for values other than default value.
    /// </summary>
    /// <returns>Returns true if either the plug-ins or commands dictionaries contains a modified item otherwise false.</returns>
    public bool ContainsModifiedValues()
    {
      if (null != m_PluginSettings && m_PluginSettings.ContainsModifiedValues(AllUserPlugInSettings))
        return true; // Plug-in settings contains a modified value
      if (null != m_CommandSettingsDict)
        foreach (var item in m_CommandSettingsDict)
          if (item.Value.ContainsModifiedValues(AllUserCommandSettings(item.Key)))
            return true; // This command's settings have been modified
      return false;
    }

    internal bool WriteSettings()
    {
      // The following, commented out code, would attempt to write to the All Users (global) file first, if that succeeded then the
      // current user file would get hosed and the All Users file would be the only remaining file. Steve, Brian and John decided
      // that settings would get read from the All Users area and those values would get replaced by the ones in Current User and
      // when closing values that were different than the default value or that did not match the ones in All Users would get written
      // to the Current User section and that we would provide a tool for migrating settings to the All User area.
      //bool result = this.WriteSettingsHelper(false);
      //if (result)
      //  this.DeleteSettingsFile(true); // All User file written successfully so delete the current user file if it exists
      //else
      //  result = this.WriteSettingsHelper(true);
      //return result;

      // Simply write the settings to current user, if the code above is run then comment this out, see comment at top of this
      // function for more information.
      return WriteSettingsHelper(true);
    }

    private void DeleteSettingsFile(bool localSettings)
    {
      string settingsFileName = SettingsFileName(localSettings);
      // If there are no settings, the settings dictionary is empty or the settings dictionary only contains default values
      // and the settings file exists
      if (File.Exists(settingsFileName))
      {
        try
        {
          // Delete the settings file
          File.Delete(settingsFileName);
          // Move up the settings path and delete the folder and its parent as long as they don't contain
          // files or sub folders
          string folder = Path.GetDirectoryName(settingsFileName);
          for (int i = 0; i < 2 && DeleteDirectory(folder); i++)
            folder = Path.GetDirectoryName(folder);
        }
        catch (Exception ex)
        {
          Rhino.Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    /// <summary>
    /// Flushes the current settings to the user's roaming directory. 
    /// If an xml file for the guid already exists, it attempts to update
    /// the file in order to maintain comments, etc. If the xml is corrupt
    /// or the file does not exist, it writes out a new file.
    /// </summary>
    /// <returns>
    /// true if settings where flushed to disk, otherwise false. 
    /// </returns>
    internal bool WriteSettingsHelper(bool localSettings)
    {
      if (!ContainsModifiedValues())
      {
        DeleteSettingsFile(localSettings);
        return true;
      }

      string settingsFileName = SettingsFileName(localSettings);
      string backupFileName = settingsFileName + "_bak";

      // Write settings to a temporary file in the output folder
      string tempFileName = WriteTempFile(SettingsFileFolder(localSettings));
      // If the temporary file was successfully created then tempFileName will be the full path to
      // the file name otherwise it will be null
      if (string.IsNullOrEmpty(tempFileName) || !File.Exists(tempFileName))
        return false; // Error creating temp file so bail

      // If there was a previous settings file then back it up
      if (File.Exists(settingsFileName))
      {
        try
        {
          // If previous back file exists then delete it
          if (File.Exists(backupFileName))
            File.Delete(backupFileName);
          // Rename existing settings file as backup
          File.Move(settingsFileName, backupFileName);
        }
        catch (Exception ex)
        {
          Rhino.Runtime.HostUtils.ExceptionReport(ex);
        }
      }

      // Attempt to copy the temp file to the settingsFileName, try several times just in case
      // another instance is currently writing the file
      bool result = false;
      const int max_trys = 5; // No more than five times
      for (int i = 0; i < max_trys; i++)
      {
        try
        {
          File.Copy(tempFileName, settingsFileName, true);
          result = true; // File successfully copied
          i = max_trys; // Will cause the for loop to end
        }
        catch
        {
          // Error copying the file so pause then try again
          System.Threading.Thread.Sleep(50);
        }
      }

      // Delete the temporary file
      try { File.Delete(tempFileName); }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return result;
    }
    /// <summary>
    /// Constructs a temporary file and write plug-in and plug-in command settings to the temp file.
    /// Only writes PersistentSettings that contain one or more item with a value that differs
    /// from the default value.
    /// </summary>
    private string WriteTempFile(string outputFolder)
    {
      string fileName = null;
      try
      {
        if (!Directory.Exists(outputFolder))
          Directory.CreateDirectory(outputFolder);
        string tempFile = Path.Combine(outputFolder, "settings " + Guid.NewGuid().ToString() + ".tmp.xml");
        using (FileStream writeStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
        {
          XmlWriterSettings xmlSettings = new XmlWriterSettings();
          xmlSettings.Encoding = Encoding.UTF8;
          xmlSettings.Indent = true;
          xmlSettings.IndentChars = "  ";
          xmlSettings.OmitXmlDeclaration = false;
          xmlSettings.NewLineOnAttributes = false;
          xmlSettings.CloseOutput = false;
          XmlWriter xmlWriter = XmlWriter.Create(writeStream, xmlSettings);
          xmlWriter.WriteStartDocument();

          xmlWriter.WriteComment("RhinoCommon generated, (" + (AllUserSettings == null ? "ProgramData" : "AppData") + "), file, do not modify");
          xmlWriter.WriteStartElement("settings");
          xmlWriter.WriteAttributeString("id", CURRENT_XML_FORMAT_VERSION);

          // If plug-in settings pointer is initialized write plug-in section
          // Note:  Will only write if one or more items has a non default value
          if (null != m_PluginSettings)
            m_PluginSettings.WriteXmlElement(xmlWriter, "plugin", "", "", AllUserPlugInSettings);

          // Update the command settings
          if (null != m_CommandSettingsDict)
            foreach (var item in m_CommandSettingsDict)
              item.Value.WriteXmlElement(xmlWriter, "command", "name", item.Key, AllUserCommandSettings(item.Key));
              
          xmlWriter.WriteEndElement();
          xmlWriter.WriteEndDocument();
          xmlWriter.Flush();
          xmlWriter.Close();
        }
        fileName = tempFile;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
      return fileName;
    }
  }

  class PersistentSettingsManager
  {
    static readonly List<PersistentSettingsManager> m_all_managers = new List<PersistentSettingsManager>();
    System.Reflection.Assembly m_assembly;
    /// <summary>
    /// If this settings PersistentSettingsManager is created by plug-in provided DLL then save the plug-in ID
    /// so that when the plug-in requests its setting it will get the same PersistentSettingsManager
    /// </summary>
    internal readonly Guid m_plugin_id;
    private readonly PlugInSettings SettingsLocal;

    /// <summary>
    /// PersistentSettingsManager constructor.
    /// </summary>
    /// <param name="assembly">Requires a valid Assembly object to attach to.</param>
    /// <param name="pluginId">Optional plug-in Id which identifies the plug-in associated with this settings class.</param>
    PersistentSettingsManager(System.Reflection.Assembly assembly, Guid pluginId)
    {
      m_assembly = assembly;
      m_plugin_id = pluginId;
      SettingsLocal = new PlugInSettings(m_assembly, m_plugin_id, new PlugInSettings(m_assembly, m_plugin_id, null));
    }
    /// <summary>
    /// PersistentSettingsManager constructor.
    /// </summary>
    /// <param name="pluginId">Requires a valid pluginId to attach to</param>
    PersistentSettingsManager(Guid pluginId)
    {
      m_plugin_id = pluginId;
      SettingsLocal = new PlugInSettings(pluginId, new PlugInSettings(pluginId, null));
    }
    /// <summary>
    /// PersistentSettingsManager constructor.
    /// </summary>
    /// <param name="skin"></param>
    PersistentSettingsManager(Rhino.Runtime.Skin skin)
    {
      m_plugin_id = Guid.Empty;
      System.Reflection.Assembly assembly = skin.GetType().Assembly;
      SettingsLocal = new PlugInSettings(assembly, m_plugin_id, new PlugInSettings(assembly, m_plugin_id, null));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pluginId"></param>
    /// <returns></returns>
    public static PersistentSettingsManager Create(Guid pluginId)
    {
      for (int i = 0; i < m_all_managers.Count; i++)
        if (m_all_managers[i].m_plugin_id == pluginId)
          return m_all_managers[i];
      var ps = new PersistentSettingsManager(pluginId);
      m_all_managers.Add(ps);
      return ps;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="plugin"></param>
    /// <returns></returns>
    public static PersistentSettingsManager Create(Rhino.PlugIns.PlugIn plugin)
    {
      Guid pluginId = plugin.Id;
      System.Reflection.Assembly assembly = plugin.GetType().Assembly;
      for (int i = 0; i < m_all_managers.Count; i++)
      {
        if (m_all_managers[i].m_assembly == assembly || m_all_managers[i].m_plugin_id == pluginId)
        {
          m_all_managers[i].m_assembly = assembly;
          return m_all_managers[i];
        }
      }
      var ps = new PersistentSettingsManager(assembly, pluginId);
      m_all_managers.Add(ps);
      return ps;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skin"></param>
    /// <returns></returns>
    public static PersistentSettingsManager Create(Rhino.Runtime.Skin skin)
    {
      System.Reflection.Assembly assembly = skin.GetType().Assembly;
      for (int i = 0; i < m_all_managers.Count; i++)
        if (m_all_managers[i].m_assembly == assembly)
          return m_all_managers[i];
      var ps = new PersistentSettingsManager(assembly, Guid.Empty);
      m_all_managers.Add(ps);
      return ps;
    }

    /// <summary>
    /// Gets the Plug-in settings associated with this plug-in, if this is the first time called then
    /// the plug-in settings member variable will get initialized and if a settings file exists it
    /// will get loaded.
    /// </summary>
    public PersistentSettings PluginSettings { get { return SettingsLocal.PluginSettings; } }
    /// <summary>
    /// Gets the PersistentSettings associated with the specified command.  If the settings file
    /// has not been previously loaded and exists then it will get read.  If the command name is
    /// not in the command settings dictionary then a new entry will get created and its settings
    /// will be returned.
    /// </summary>
    /// <param name="name">Command name key to search for and/or add.</param>
    /// <returns>Returns PersistentSettings object associated with command name on success or null on error.</returns>
    public PersistentSettings CommandSettings(string name)
    {
      return SettingsLocal.CommandSettings(name);
    }
    /// <summary>
    /// Check the plug-in and command settings dictionaries for values other than default value.
    /// </summary>
    /// <returns>Returns true if either the plug-ins or commands dictionaries contains a modified item otherwise false.</returns>
    public bool ContainsModifiedValues()
    {
      return SettingsLocal.ContainsModifiedValues();
    }
    /// <summary>
    /// If they exist and contain modified values write global settings first then local settings.
    /// </summary>
    /// <returns>Returns true if local settings were successfully written.</returns>
    public bool WriteSettings()
    {
      return SettingsLocal.WriteSettings();
    }
  }
#endif
}
