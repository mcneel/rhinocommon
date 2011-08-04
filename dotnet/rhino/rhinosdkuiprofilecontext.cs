using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

using Rhino.Geometry;

namespace Rhino
{
  //////////////////////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// PersistentSettings contains a dictionary of these items
  /// </summary>
  class SettingValue : ISerializable
  {
    protected SettingValue(SerializationInfo info, StreamingContext context)
    {
    }

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      //info.AddValue("X", m_x);
    }
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="value">Current value string</param>
    /// <param name="default_value">Default value string</param>
    public SettingValue(string value, string default_value)
    {
      this.m_value = value;
      this.m_default_value = default_value;
    }
    /// <summary>
    /// Set either the current or default value string
    /// </summary>
    /// <param name="bDefault">If true then the current value string is set otherwise the default value string is</param>
    /// <param name="s">New value string</param>
    public void SetValue(bool bDefault, string s)
    {
      if (bDefault)
        this.m_default_value = s;
      else
        this.m_value = s;
    }
    /// <summary>
    /// Get the current or default value as requested
    /// </summary>
    /// <param name="bDefault">If true then the default value string is returned otherwise the current value string is returned</param>
    /// <returns>If bDefault is true then the default value string is returned otherwise the current value string is returned</returns>
    public string GetValue(bool bDefault)
    {
      if (bDefault || string.IsNullOrEmpty(m_value))
        return m_default_value;
      return m_value;
    }
    /// <summary>
    /// Compare current and default values and return true if they are identical, compare is case sensitive
    /// </summary>
    public bool ValueSameAsDefault
    {
      get { return (0 == string.Compare(m_value, m_default_value, false)); }
    }
    /// <summary>
    /// Compare current and default values and return true if they differ, compare is case sensitive
    /// </summary>
    public bool ValueDifferentThanDefault
    {
      get { return (!ValueSameAsDefault); }
    }

    public bool TryGetBool(bool bDefault, out bool value)
    {
      value = false;
      return bool.TryParse(GetValue(bDefault), out value);
    }

    public bool TryGetByte(bool bDefault, out byte value)
    {
      value = 0;
      return byte.TryParse(GetValue(bDefault), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out value);
    }

    public bool TryGetInteger(bool bDefault, out int value)
    {
      value = int.MaxValue;
      return int.TryParse(GetValue(bDefault), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out value);
    }

    public bool TryGetUnsignedInteger(bool bDefault, out uint value)
    {
      value = uint.MaxValue;
      return uint.TryParse(GetValue(bDefault), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out value);
    }

    public bool TryGetDouble(bool bDefault, out double value)
    {
      value = double.MaxValue;
      return double.TryParse(GetValue(bDefault), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out value);
    }

    public bool TryGetChar(bool bDefault, out char value)
    {
      value = char.MaxValue;
      return char.TryParse(GetValue(bDefault), out value);
    }

    public bool TryGetString(bool bDefault, out string value)
    {
      value = GetValue(bDefault);
      return true;
    }

    public bool TryGetDate(bool bDefault, out DateTime value)
    {
      value = DateTime.MinValue;
      return DateTime.TryParse(GetValue(bDefault), CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out value);
    }

    public bool TryGetColor(bool bDefault, out Color value)
    {
      value = Color.Empty;
      string[] argb = GetValue(bDefault).Split(',');

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

    public bool TryGetRect(bool bDefault, out System.Drawing.Rectangle value)
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
        value = new System.Drawing.Rectangle(x, y, width, height);
        return true;
      }

      return false;
    }

    public void SetBool(bool bDefault, bool value)
    {
      SetValue(bDefault, value.ToString(CultureInfo.InvariantCulture.NumberFormat));
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
      SetValue(bDefault, value.ToString(CultureInfo.InvariantCulture.NumberFormat));
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

    public void SetRect(bool bDefault, Rectangle value, EventHandler<PersistentSettingsEventArgs> validator)
    {
      if (validator != null)
      {
        Rectangle old_value;
        TryGetRect(bDefault, out old_value);
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
    private string m_value;
    private string m_default_value;
  }
  //////////////////////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// 
  /// </summary>
  public abstract class PersistentSettingsEventArgs : EventArgs
  {
    public PersistentSettingsEventArgs()
    {
      Cancel = false;
    }
    public bool Cancel
    {
      get;
      set;
    }
  }
  //////////////////////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class PersistentSettingsEventArgs<T> : PersistentSettingsEventArgs
  {
    public PersistentSettingsEventArgs(T currentValue, T newValue)
    {
      m_current_value = currentValue;
      m_new_value = newValue;
    }
    public T CurrentValue
    {
      get { return m_current_value; }
      set { m_current_value = value; }
    }
    public T NewValue { get { return m_new_value; } }
    private T m_current_value;
    private T m_new_value;
  }
  //////////////////////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// A dictionary of SettingValue items
  /// </summary>
  [Serializable]
  public class PersistentSettings : ISerializable
  {
    readonly Dictionary<string, SettingValue> m_Settings;
    readonly Dictionary<string, EventHandler<PersistentSettingsEventArgs>> m_SettingsValidators;

    protected PersistentSettings(SerializationInfo info, StreamingContext context)
    {
    }

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      //info.AddValue("X", m_x);
    }

    public PersistentSettings()
    {
      m_Settings = new Dictionary<string, SettingValue>();
      m_SettingsValidators = new Dictionary<string, EventHandler<PersistentSettingsEventArgs>>();
    }

    public void RegisterSettingsValidator(string key, EventHandler<PersistentSettingsEventArgs> validator)
    {
      m_SettingsValidators[key] = validator;
    }

    public EventHandler<PersistentSettingsEventArgs> GetValidator(string key)
    {
      EventHandler<PersistentSettingsEventArgs> validator = null;
      this.m_SettingsValidators.TryGetValue(key, out validator);
      return validator;
    }

    public bool ContainsModifiedValues()
    {
      if (null != m_Settings && m_Settings.Count > 0)
        foreach (var v in m_Settings)
          if (v.Value.ValueDifferentThanDefault)
            return true;
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
        m_Settings[key].SetBool(true, defaultValue);
        return rc;
      }
      this.SetBool(key, defaultValue);
      return defaultValue;
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
      this.SetByte(key, defaultValue);
      return defaultValue;
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
      SetInteger(key, defaultValue);
      return defaultValue;
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
      SetUnsignedInteger(key, defaultValue);
      return defaultValue;
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
      this.SetDouble(key, defaultValue);
      return defaultValue;
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
      this.SetChar(key, defaultValue);
      return defaultValue;
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
      this.SetString(key, defaultValue);
      return defaultValue;
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
      this.SetDate(key, defaultValue);
      return defaultValue;
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
      this.SetColor(key, defaultValue);
      return defaultValue;
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
      this.SetPoint3d(key, defaultValue);
      return defaultValue;
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
      this.SetSize(key, defaultValue);
      return defaultValue;
    }

    public bool TryGetRect(string key, out System.Drawing.Rectangle value)
    {
      value = Rectangle.Empty;
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetRect(false, out value);
      return false;
    }

    [Obsolete("Use GetRectangle - this will be removed in a future WIP")]
    public Rectangle GetRect(string key)
    {
      return GetRectangle(key);
    }

    [Obsolete("Use GetRectangle - this will be removed in a future WIP")]
    public Rectangle GetRect(string key, Rectangle defaultValue)
    {
      return GetRectangle(key, defaultValue);
    }

    public Rectangle GetRectangle(string key)
    {
      if (!m_Settings.ContainsKey(key))
        throw new KeyNotFoundException(key);
      Rectangle rc;
      if (TryGetRect(key, out rc))
        return rc;
      throw new Exception("key '" + key + "' value type is not a Rectangle");
    }

    public Rectangle GetRectangle(string key, Rectangle defaultValue)
    {
      Rectangle rc;
      if (TryGetRect(key, out rc))
      {
        m_Settings[key].SetRect(true, defaultValue, GetValidator(key));
        return rc;
      }
      this.SetRect(key, defaultValue);
      return defaultValue;
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

    public bool TryGetDefault(string key, out System.Drawing.Rectangle value)
    {
      if (m_Settings.ContainsKey(key))
        return m_Settings[key].TryGetRect(true, out value);
      value = Rectangle.Empty;
      return false;
    }

    public void SetBool(string key, bool value)
    {
      GetValue(key).SetBool(false, value);
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

    public void SetRect(string key, System.Drawing.Rectangle value)
    {
      GetValue(key).SetRect(false, value, GetValidator(key));
    }

    public void SetSize(string key, Size value)
    {
      GetValue(key).SetSize(false, value, GetValidator(key));
    }

    public void SetDefault(string key, bool value)
    {
      GetValue(key).SetBool(true, value);
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

    public void SetDefault(string key, DateTime value)
    {
      GetValue(key).SetDate(true, value, GetValidator(key));
    }

    public void SetDefault(string key, Color value)
    {
      GetValue(key).SetColor(true, value, GetValidator(key));
    }

    public void SetDefault(string key, System.Drawing.Rectangle value)
    {
      GetValue(key).SetRect(true, value, GetValidator(key));
    }

    public void SetDefault(string key, Size value)
    {
      GetValue(key).SetSize(true, value, GetValidator(key));
    }

    //public void Write()
    //{
    //  m_SettingsManager.WriteSettings();
    //}

    /// <summary>
    /// If the settings dictionary contains one or more values, which are not equal to the default value, then Write the contents
    /// of this settings dictionary to the specified XmlWriter contained within elementName
    /// </summary>
    /// <param name="xmlWriter">XmlWriter object to write to</param>
    /// <param name="elementName">Element which will contain key value pairs</param>
    /// <param name="attributeName">Optional element attribute</param>
    /// <param name="attributeValue">Optional element attribute value</param>
    internal void WriteXmlElement(XmlWriter xmlWriter, string elementName, string attributeName, string attributeValue)
    {
      if (null != m_Settings && ContainsModifiedValues())
      {
        xmlWriter.WriteStartElement(elementName);
        if (!string.IsNullOrEmpty(attributeName) && !string.IsNullOrEmpty(attributeValue))
          xmlWriter.WriteAttributeString(attributeName, attributeValue);
        foreach (var item in this.m_Settings)
        {
          if (item.Value.ValueDifferentThanDefault)
          {
            // Write current value
            xmlWriter.WriteStartElement("entry");
            xmlWriter.WriteAttributeString("key", item.Key);
            xmlWriter.WriteString(item.Value.GetValue(false));
            xmlWriter.WriteEndElement();
            // If default value is not an empty string then write it out
            //if (!string.IsNullOrEmpty(item.Value.GetValue(true)))
            //{
            //  xmlWriter.WriteStartElement("entry_default");
            //  xmlWriter.WriteAttributeString("key", item.Key);
            //  xmlWriter.WriteString(item.Value.GetValue(true));
            //  xmlWriter.WriteEndElement();
            //}
          }
        }
        xmlWriter.WriteEndElement();
      }
    }
    /// <summary>
    /// Parse XmlNode for settings "entry" and "entry_default" elements, add entry elements to the dictionary
    /// first and if then check the defaults list and make sure the entry is in the list before setting the 
    /// default value
    /// </summary>
    /// <param name="nodeRoot"></param>
    internal void ParseXmlNodes(XmlNode nodeRoot)
    {
      if (null != m_Settings && null != nodeRoot)
      {
        string[] selectNodeStrings = { "./entry", "./entry_default" };
        bool setDefaults = false;
        foreach (string select in selectNodeStrings)
        {
          XmlNodeList nodeList = nodeRoot.SelectNodes(select);
          foreach (XmlNode entry in nodeList)
          {
            XmlNode attr = null == entry.Attributes ? null : entry.Attributes["key"];
            if (attr != null && !string.IsNullOrEmpty(attr.Value))
            {
              if (setDefaults)
                SetDefault(attr.Value, entry.InnerText);
              else
                SetString(attr.Value, entry.InnerText);
            }
          }
          setDefaults = true;
        }
      }
    }
  }
  //////////////////////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// 
  /// </summary>
  class PlugInSettings
  {
    private readonly PlugIns.PlugIn m_plugin; // Initialized by constructor
    private readonly bool LocalSettings; // Initialized by constructor
    private PersistentSettings m_PluginSettings; // = null; initialized by runtime
    private Dictionary<string, PersistentSettings> m_CommandSettingsDict; // = null; initialized by runtime
    /// <summary>
    /// Main settings element id attribute value, used to query valid settings section in settings XML file
    /// </summary>
    private const string CURRENT_XML_FORMAT_VERSION = "1.0";
    private string SettingsFileFolder { get { return LocalSettings ? m_plugin.SettingsDirectory : m_plugin.SettingsDirectoryAllUsers; } }
    private string SettingsFileName { get { return System.IO.Path.Combine(SettingsFileFolder, "settings.xml");
      }
    }
    /// <summary>
    /// PersistentSettingsManager constructor
    /// </summary>
    /// <param name="plugin">Requires a valid PlugIn object to attach to</param>
    /// <param name="localSettings">Identifies this settings instance as AllUsers or Local</param>
    public PlugInSettings(Rhino.PlugIns.PlugIn plugin, bool localSettings)
    {
      m_plugin = plugin;
      LocalSettings = localSettings;
    }
    /// <summary>
    /// Get the Plug-in settings associated with this plug-in, if this is the first time called then
    /// the plug-in settings member variable will get initialized and if a settings file exists it
    /// will get loaded
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
    /// Get the PersistentSettings associated with the specified command.  If the settings file
    /// has not been previously loaded and exists then it will get read.  If the command name is
    /// not in the command settings dictionary then a new entry will get created and its settings
    /// will be returned
    /// </summary>
    /// <param name="name">Command name key to search for and/or add</param>
    /// <returns>Returns PersistentSettings object associated with command name on success or null on error</returns>
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
      m_CommandSettingsDict[name] = new PersistentSettings();
      return m_CommandSettingsDict[name];
    }
    /// <summary>
    /// Reads existing settings for a plug-in and its associated commands.
    /// Clears the dirty flag for the settings. 
    /// </summary>
    /// <returns>
    /// True if settings are successfully read. False if there was no existing
    /// settings file to read, or if a read lock could not be acquired.
    /// </returns>
    public bool ReadSettings()
    {
      if (m_PluginSettings == null)
        m_PluginSettings = new PersistentSettings();

      if (m_CommandSettingsDict == null)
        m_CommandSettingsDict = new Dictionary<string, PersistentSettings>();

      string settingsFileName = SettingsFileName;

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

        XmlNode rootNode = doc.SelectSingleNode("/settings[@xml:id=\'" + CURRENT_XML_FORMAT_VERSION + "\']", ns);
        if (null == rootNode) // The settings attribute will either be "xml:id" or "id", this will check for "id" if "xml:id" is not found.
          rootNode = doc.SelectSingleNode("/settings[@id=\'" + CURRENT_XML_FORMAT_VERSION + "\']", ns);

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
            XmlNode attr = commandNode.Attributes.GetNamedItem("name");
            if (null != attr && !string.IsNullOrEmpty(attr.Value))
            {
              PersistentSettings entries = new PersistentSettings();
              entries.ParseXmlNodes(commandNode);
              m_CommandSettingsDict[attr.Value] = entries;
            }
          }
        }
      }
      catch (Exception ex)
      {
        // TODO: Figure out where to log this kind of thing 
        // throw new Exception(String.Format("Unable to read settings xml file {0}", path));
        System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.StackTrace);
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
    /// Helper method to delete a directory if it is empty
    /// </summary>
    /// <param name="directory">Full path to directory to delete</param>
    /// <returns>Returns true if the directory was empty and successfully deleted otherwise returns false</returns>
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
      catch { }
      return rc;
    }
    /// <summary>
    /// Check the plug-in and command settings dictionaries for values other than default value
    /// </summary>
    /// <returns>Returns true if either the plug-ins or commands dictionaries contains a modified item otherwise false</returns>
    public bool ContainsModifiedValues()
    {
      if (null != m_PluginSettings && m_PluginSettings.ContainsModifiedValues())
        return true; // Plug-in settings contains a modified value
      if (null != this.m_CommandSettingsDict)
        foreach (var item in this.m_CommandSettingsDict)
          if (item.Value.ContainsModifiedValues())
            return true; // This command's settings have been modified
      return false;
    }
    /// <summary>
    /// Flushes the current settings to the user's roaming directory. 
    /// If an xml file for the guid already exists, it attempts to update
    /// the file in order to maintain comments, etc. If the xml is corrupt
    /// or the file does not exist, it writes out a new file.
    /// </summary>
    /// <returns>
    /// True if settings where flushed to disk, otherwise false. 
    /// </returns>
    internal bool WriteSettings()
    {
      string settingsFileName = SettingsFileName;
      string backupFileName = settingsFileName + "_bak";

      if (!this.ContainsModifiedValues())
      {
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
          catch { }
        }
        return false;
      }
      
      // Write settings to a temporary file in the output folder
      string tempFileName = this.WriteTempFile(SettingsFileFolder);
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
        catch { }
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
      catch { }

      return result;
    }
    /// <summary>
    /// Create a temporary file and write plug-in and plug-in command settings to the temp file.
    /// Only writes PersistentSettings that contain one or more item with a value that differs
    /// from the default value.
    /// </summary>
    /// <param name="outputFolder"></param>
    /// <returns></returns>
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

          xmlWriter.WriteComment("RhinoCommon generated file, do not modify");
          xmlWriter.WriteStartElement("settings");
          xmlWriter.WriteAttributeString("id", CURRENT_XML_FORMAT_VERSION);

          // If plug-in settings pointer is initialized write plug-in section
          // Note:  Will only write if one or more items has a non default value
          if (null != m_PluginSettings)
            m_PluginSettings.WriteXmlElement(xmlWriter, "plugin", "", "");

          // Update the command settings
          if (null != m_CommandSettingsDict)
            foreach (var item in m_CommandSettingsDict)
              item.Value.WriteXmlElement(xmlWriter, "command", "name", item.Key);
              
          xmlWriter.WriteEndElement();
          xmlWriter.WriteEndDocument();
          xmlWriter.Flush();
          xmlWriter.Close();
        }
        fileName = tempFile;
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.StackTrace);
      }
      return fileName;
    }
  }
  //////////////////////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// 
  /// </summary>
  class PersistentSettingsManager
  {
    private readonly PlugInSettings m_SettingsLocal;
    private readonly PlugInSettings m_SettingsAllUsers;
    /// <summary>
    /// PersistentSettingsManager constructor
    /// </summary>
    /// <param name="plugin">Requires a valid PlugIn object to attach to</param>
    public PersistentSettingsManager(Rhino.PlugIns.PlugIn plugin)
    {
      m_SettingsLocal = new PlugInSettings(plugin, true);
      m_SettingsAllUsers = new PlugInSettings(plugin, false);
    }
    /// <summary>
    /// Get the Plug-in settings associated with this plug-in, if this is the first time called then
    /// the plug-in settings member variable will get initialized and if a settings file exists it
    /// will get loaded
    /// </summary>
    public PersistentSettings PluginSettings { get { return m_SettingsLocal.PluginSettings; } }
    /// <summary>
    /// Get the Plug-in settings associated with this plug-in, if this is the first time called then
    /// the plug-in settings member variable will get initialized and if a settings file exists it
    /// will get loaded
    /// </summary>
    public PersistentSettings PluginSettingsAllUsers { get { return m_SettingsAllUsers.PluginSettings; } }
    /// <summary>
    /// Get the PersistentSettings associated with the specified command.  If the settings file
    /// has not been previously loaded and exists then it will get read.  If the command name is
    /// not in the command settings dictionary then a new entry will get created and its settings
    /// will be returned
    /// </summary>
    /// <param name="name">Command name key to search for and/or add</param>
    /// <returns>Returns PersistentSettings object associated with command name on success or null on error</returns>
    public PersistentSettings CommandSettings(string name)
    {
      return m_SettingsLocal.CommandSettings(name);
    }
    /// <summary>
    /// Get the PersistentSettings associated with the specified command.  If the settings file
    /// has not been previously loaded and exists then it will get read.  If the command name is
    /// not in the command settings dictionary then a new entry will get created and its settings
    /// will be returned
    /// </summary>
    /// <param name="name">Command name key to search for and/or add</param>
    /// <returns>Returns PersistentSettings object associated with command name on success or null on error</returns>
    public PersistentSettings CommandSettingsAllUsers(string name)
    {
      return m_SettingsAllUsers.CommandSettings(name);
    }
    /// <summary>
    /// Check the plug-in and command settings dictionaries for values other than default value
    /// </summary>
    /// <returns>Returns true if either the plug-ins or commands dictionaries contains a modified item otherwise false</returns>
    public bool ContainsModifiedValues()
    {
      return (m_SettingsLocal.ContainsModifiedValues() || m_SettingsAllUsers.ContainsModifiedValues());
    }
    /// <summary>
    /// If they exist and contain modified values write global settings firs then local settings
    /// </summary>
    /// <returns>Returns true if local settings were successfully written</returns>
    public bool WriteSettings()
    {
      try { m_SettingsAllUsers.WriteSettings(); }
      catch { } // Fail quietly when writing global settings
      return m_SettingsLocal.WriteSettings();
    }
  }
}
