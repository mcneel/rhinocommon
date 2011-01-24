using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Globalization;
using Rhino.Geometry;

namespace Rhino
{
  public class PersistentSettings
  {
    //public static string SettingsDirectory
    //{
    //  get
    //  {
    //    System.Reflection.Assembly ass = System.Reflection.Assembly.GetCallingAssembly();
    //    // find the plug-in from the calling assembly
    //    Rhino.PlugIns.PlugIn executingPlugIn = Rhino.PlugIns.PlugIn.Find(ass);
    //    if (null == executingPlugIn)
    //      return null;

    //    string id = executingPlugIn.Id.ToString();

    //    string pluginName = executingPlugIn.Name + " (" + id + ")";

    //    string commonDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
    //    char sep = System.IO.Path.DirectorySeparatorChar;
    //    commonDir = System.IO.Path.Combine(commonDir, "McNeel" + sep + "Rhinoceros" + sep + "5.0" + sep + "Plug-ins");
    //    string path = System.IO.Path.Combine(commonDir, pluginName);
    //    path = System.IO.Path.Combine(path, "settings");
    //    return path;
    //  }
    //}

    readonly PersistentSettingsManager m_SettingsManager;
    readonly Dictionary<string, string> m_Settings;

    internal PersistentSettings(PersistentSettingsManager settingsManager)
    {
      m_SettingsManager = settingsManager;
      m_Settings = new Dictionary<string, string>();
    }

    internal string this[string key]
    {
      get
      {
        return m_Settings[key];
      }
      set
      {
        m_Settings[key] = value;
      }
    }

    internal Dictionary<string,string>.KeyCollection Keys
    {
      get { return m_Settings.Keys; }
    }

    public bool TryGetBool(string key, out bool value)
    {
      value = false;

      if (m_Settings.ContainsKey(key))
      {
        return bool.TryParse(m_Settings[key], out value);
      }

      return false;
    }

    public bool TryGetByte(string key, out byte value)
    {
      value = 0;

      if (m_Settings.ContainsKey(key))
      {
        return byte.TryParse(m_Settings[key], 
          System.Globalization.NumberStyles.Any, 
          CultureInfo.InvariantCulture.NumberFormat, out value);
      }

      return false;
    }

    public bool TryGetInteger(string key, out int value)
    {
      value = 0;

      if (m_Settings.ContainsKey(key))
      {
        return int.TryParse(m_Settings[key],
          System.Globalization.NumberStyles.Any,
          CultureInfo.InvariantCulture.NumberFormat, out value);
      }

      return false;
    }

    public int GetInteger(string key, int defaultValue)
    {
      int rc;
      if (TryGetInteger(key, out rc))
        return rc;
      return defaultValue;
    }

    public bool TryGetDouble(string key, out double value)
    {
      value = 0;

      if (m_Settings.ContainsKey(key))
      {
        return double.TryParse(m_Settings[key], 
          System.Globalization.NumberStyles.Any, 
          CultureInfo.InvariantCulture.NumberFormat, out value);
      }

      return false;
    }

    public bool TryGetChar(string key, out char value)
    {
      value = char.MinValue;

      if (m_Settings.ContainsKey(key))
      {
        return char.TryParse(m_Settings[key], out value);
      }

      return false;
    }

    public bool TryGetString(string key, out string value)
    {
      value = "";

      if (m_Settings.ContainsKey(key))
      {
        value = m_Settings[key];
        return true;
      }

      return false;
    }

    public bool TryGetDate(string key, out DateTime value)
    {
      value = DateTime.Now;

      if (m_Settings.ContainsKey(key))
      {
        return DateTime.TryParse(m_Settings[key],
          CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out value);
      }

      return false;
    }

    public bool TryGetColor(string key, out Color value)
    {
      value = Color.Black;

      if (m_Settings.ContainsKey(key))
      {
        string[] argb = m_Settings[key].Split(',');

        if (argb.Length != 4)
          return false;

        Int32 alpha, red, green, blue;
        if (Int32.TryParse(argb[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out alpha) 
          && Int32.TryParse(argb[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out red)
          && Int32.TryParse(argb[2], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out green) 
            && Int32.TryParse(argb[3], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out blue))
        {
          value = Color.FromArgb(alpha, red, green, blue);
          return true;
        }
      }

      return false;
    }

    public bool TryGetPoint3d(string key, out Point3d value)
    {
      value = Point3d.Unset;

      if (m_Settings.ContainsKey(key))
      {
        string[] point = m_Settings[key].Split(',');

        if (point.Length != 3)
          return false;

        double x, y, z;
        if (double.TryParse(point[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out x)
          && double.TryParse(point[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out y)
          && double.TryParse(point[2], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out z))
        {
          value = new Point3d(x, y, z);
          return true;
        }
      }

      return false;
    }

    public bool TryGetSize(string key, out Size value)
    {
      value = Size.Empty;

      if (m_Settings.ContainsKey(key))
      {
        string[] size = m_Settings[key].Split(',');

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
      }

      return false;
    }

    public bool TryGetRect(string key, out System.Drawing.Rectangle value)
    {
      value = System.Drawing.Rectangle.Empty;

      if (m_Settings.ContainsKey(key))
      {
        string[] rect = m_Settings[key].Split(',');

        if (rect.Length != 4)
          return false;

        int x, y, width, height;

        if (int.TryParse(rect[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out x)
          && int.TryParse(rect[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out y)
          && int.TryParse(rect[2], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out width)
          && int.TryParse(rect[3], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out height))
        {
          value = new System.Drawing.Rectangle(x, y, width, height);
          return true;
        }
      }
      return false;
    }

    public System.Drawing.Rectangle GetRect(string key, System.Drawing.Rectangle defaultValue)
    {
      System.Drawing.Rectangle rc;
      if (TryGetRect(key, out rc))
        return rc;

      return defaultValue;
    }

    public void SetBool(string key, bool value)
    {
      m_Settings[key] = value.ToString(CultureInfo.InvariantCulture.NumberFormat);
      m_SettingsManager.IsDirty = true;
    }

    public void SetByte(string key, byte value)
    {
      m_Settings[key] = value.ToString(CultureInfo.InvariantCulture.NumberFormat);
      m_SettingsManager.IsDirty = true;
    }

    public void SetInteger(string key, int value)
    {
      m_Settings[key] = value.ToString(CultureInfo.InvariantCulture.NumberFormat);
      m_SettingsManager.IsDirty = true;
    }

    public void SetDouble(string key, double value)
    {
      m_Settings[key] = value.ToString(CultureInfo.InvariantCulture.NumberFormat);
      m_SettingsManager.IsDirty = true;
    }

    public void SetChar(string key, char value)
    {
      m_Settings[key] = value.ToString(CultureInfo.InvariantCulture.NumberFormat);
      m_SettingsManager.IsDirty = true;
    }

    public void SetString(string key, string value)
    {
      m_Settings[key] = value;
      m_SettingsManager.IsDirty = true;
    }

    public void DeleteItem(string key)
    {
      if (m_Settings.ContainsKey(key))
      {
        m_Settings.Remove(key);
        m_SettingsManager.IsDirty = true;
      }
    }

    public void SetDate(string key, DateTime value)
    {
      m_Settings[key] = value.ToString("F", CultureInfo.InvariantCulture);
      m_SettingsManager.IsDirty = true;
    }

    public void SetColor(string key, Color value)
    {
      m_Settings[key] = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}",
          value.A.ToString(CultureInfo.InvariantCulture.NumberFormat), 
          value.R.ToString(CultureInfo.InvariantCulture.NumberFormat), 
          value.G.ToString(CultureInfo.InvariantCulture.NumberFormat),
          value.B.ToString(CultureInfo.InvariantCulture.NumberFormat));
        m_SettingsManager.IsDirty = true;
    }

    public void SetRect(string key, System.Drawing.Rectangle value)
    {
      m_Settings[key] = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}",
        value.Left.ToString(CultureInfo.InvariantCulture.NumberFormat), 
        value.Top.ToString(CultureInfo.InvariantCulture.NumberFormat), 
        value.Width.ToString(CultureInfo.InvariantCulture.NumberFormat),
        value.Height.ToString(CultureInfo.InvariantCulture.NumberFormat));
      m_SettingsManager.IsDirty = true;
    }

    public void SetSize(string key, Size value)
    {
      m_Settings[key] = string.Format(CultureInfo.InvariantCulture, "{0},{1}",
        value.Width.ToString(CultureInfo.InvariantCulture.NumberFormat),
        value.Height.ToString(CultureInfo.InvariantCulture.NumberFormat));
      m_SettingsManager.IsDirty = true;
    }

    public bool IsDirty
    {
      get { return m_SettingsManager.IsDirty; }
    }

    //public void Write()
    //{
    //  m_SettingsManager.WriteSettings();
    //}
  }

  class PersistentSettingsManager
  {
    private readonly PlugIns.PlugIn m_plugin;

    private PersistentSettings m_PluginSettings; // = null; initialized by runtime
    private Dictionary<string, PersistentSettings> m_CommandSettingsDict; // = null; initialized by runtime

    private bool m_bDirty;

    private const string CURRENT_XML_FORMAT_VERSION = "1.0";

    public PersistentSettingsManager(Rhino.PlugIns.PlugIn plugin)
    {
      m_plugin = plugin;
    }

    public PersistentSettings PluginSettings
    {
      get 
      {
        if (m_PluginSettings == null)
          ReadSettings();

        return m_PluginSettings; 
      }
    }

    public PersistentSettings CommandSettings(string name)
    {
      if (m_CommandSettingsDict == null)
        ReadSettings();

      if (m_CommandSettingsDict.ContainsKey(name))
        return m_CommandSettingsDict[name];
      else
      {
        // There were no settings available for the command, so create one
        // for writing
        m_CommandSettingsDict[name] = new PersistentSettings(this);
      }

      return m_CommandSettingsDict[name];
    }

    internal bool IsDirty
    {
      get { return m_bDirty; }
      set { m_bDirty = value; }
    }

    /// <summary>
    /// Reads existing settings for a plugin and its associated commands.
    /// Clears the dirty flag for the settings. 
    /// </summary>
    /// <returns>
    /// True if settings are successfully read. False if there was no existing
    /// settings file to read, or if a read lock could not be acquired.
    /// </returns>
    public bool ReadSettings()
    {
      m_bDirty = false;

      if (m_PluginSettings == null)
        m_PluginSettings = new PersistentSettings(this);

      if (m_CommandSettingsDict == null)
        m_CommandSettingsDict = new Dictionary<string, PersistentSettings>();

      // TODO: STEVE - Added on 3 Mar 2010
      // Remove after a couple of WIPS
      string oldsettingsDir = SettingsDirectory(false);
      string oldfileName = m_plugin.Id.ToString("D").ToUpperInvariant() + ".settings.xml";
      string oldpath = System.IO.Path.Combine(oldsettingsDir, oldfileName);

      string path = System.IO.Path.Combine(m_plugin.SettingsDirectory, "settings.xml");
      if (File.Exists(oldpath))
      {
        if( !Directory.Exists(m_plugin.SettingsDirectory) )
          Directory.CreateDirectory(m_plugin.SettingsDirectory);

        if (File.Exists(path))
          File.Delete(oldpath);
        else
          File.Move(oldpath, path);
        string[] oldfiles = Directory.GetFiles(oldsettingsDir);
        if (null == oldfiles || oldfiles.Length < 1)
          Directory.Delete(oldsettingsDir);
      }

      if (File.Exists(path) == false)
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
            fs = new FileStream(path, FileMode.Open, FileAccess.Read);
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

        XmlNode rootNode = doc.SelectSingleNode("/settings[@xml:id=\'"
          + CURRENT_XML_FORMAT_VERSION + "\']", ns);

        if (rootNode == null)
          return false;

        XmlNodeList pluginSettings = rootNode.SelectNodes("./plugin/entry");
        if (pluginSettings != null)
        {
          foreach (XmlNode entry in pluginSettings)
          {
            if (entry.Attributes!=null && entry.Attributes["key"] != null)
            {
              string key = entry.Attributes["key"].Value;
              m_PluginSettings[key] = entry.InnerText;
            }
          }
        }

        XmlNodeList commandNodes = rootNode.SelectNodes("./command");

        if (commandNodes != null)
        {
          foreach (XmlNode command in commandNodes)
          {
            string commandName = command.Attributes["name"].Value;
            XmlNodeList commandEntries = command.SelectNodes("./entry");
            PersistentSettings entries = new PersistentSettings(this);
            foreach (XmlNode entry in commandEntries)
            {
              if (entry.Attributes["key"] != null)
              {
                string key = entry.Attributes["key"].Value;
                entries[key] = entry.InnerText;
              }
            }
            m_CommandSettingsDict[commandName] = entries;
          }
        }
      }
      catch
      {
        // TODO: Figure out where to log this kind of thing 
        // throw new Exception(String.Format("Unable to read settings xml file {0}", path));

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
    /// Flushes the current settings to the user's roaming directory. 
    /// If an xml file for the guid already exists, it attempts to update
    /// the file in order to maintain comments, etc. If the xml is corrupt
    /// or the file does not exist, it writes out a new file.
    /// </summary>
    /// <returns>
    /// True if settings where flushed to disk, otherwise false. 
    /// </returns>
    public bool WriteSettings()
    {
      if (!m_bDirty)
        return false;


      string settingsDir = m_plugin.SettingsDirectory;
      string path = System.IO.Path.Combine(settingsDir, "settings.xml");

      // If a settings file already exists just update it, otherwise create a new one
      if (File.Exists(path) == false)
      {
        WriteNewSettings(path);
      }
      else
      {
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
              fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
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
          doc.Load(fs);
          fs.Close();
          fs.Dispose();

          XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
          ns.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace");

          // Verify that there is a root element. If one doesn't exist, the
          // document is useless, so just create a new one with the current
          // setting values.
          XmlNode rootNode = doc.SelectSingleNode("/settings[@xml:id=\'"
            + CURRENT_XML_FORMAT_VERSION + "\']", ns);
          if (rootNode == null)
          {
            WriteNewSettings(path);
            m_bDirty = false;
            return true;
          }

          // Update the plugin settings
          foreach (string key in m_PluginSettings.Keys)
          {
            // If node exists, update it, otherwise create a new one
            XmlNode settingNode = doc.SelectSingleNode("//settings//plugin//entry[@key=\"" + key + "\"]");
            if (null != settingNode)
            {
              settingNode.InnerText = m_PluginSettings[key];
            }
            else
            {
              XmlNode pluginNode = doc.SelectSingleNode("//settings//plugin");
              if (null != pluginNode)
              {
                XmlElement settingElement = doc.CreateElement("entry");
                settingElement.SetAttribute("key", key);
                settingElement.InnerText = m_PluginSettings[key];
                pluginNode.AppendChild(settingElement);
              }
              else
              {
                // This is a new command. Create a new command element
                // and add the new setting entries
                XmlElement pluginElement = doc.CreateElement("plugin");
                XmlElement settingElement = doc.CreateElement("entry");
                settingElement.SetAttribute("key", key);
                settingElement.InnerText = m_PluginSettings[key];
                pluginElement.AppendChild(settingElement);
                rootNode.AppendChild(pluginElement);
              }
            }
          }

          // Update the command settings
          foreach (string command in m_CommandSettingsDict.Keys)
          {
            XmlNode commandNode = doc.SelectSingleNode("//settings//command[@name=\"" + command + "\"]");
            if (null != commandNode)
            {
              foreach (string key in m_CommandSettingsDict[command].Keys)
              {
                XmlNode settingNode = commandNode.SelectSingleNode("./entry[@key=\"" + key + "\"]");
                if (null != settingNode)
                  settingNode.InnerText = m_CommandSettingsDict[command][key];
              }
            }
            else
            {
              // This is a new command. Create a new command element
              // and add the new setting entries
              XmlElement commandElement = doc.CreateElement("command");
              commandElement.SetAttribute("name", command);

              foreach (string key in m_CommandSettingsDict[command].Keys)
              {
                XmlElement settingNode = doc.CreateElement("entry");
                settingNode.SetAttribute("key", key);
                settingNode.InnerText = m_CommandSettingsDict[command][key];
                commandElement.AppendChild(settingNode);
              }
              rootNode.AppendChild(commandElement);
            }
          }

          fs = new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
          doc.Save(fs);
          fs.Close();
          fs.Dispose();
        }
        catch (XmlException)
        {
          // Need to make sure the file handles are cleaned up before writing
          // a new settings file.
          if (fs != null)
          {
            fs.Close();
            fs.Dispose();
          }

          // The existing xml file is garbage. Attempt to write out a new file. 
          WriteNewSettings(path);
        }
        catch
        {
          throw new IOException("Unable to write settings xml file " + path);
        }
        finally
        {
          if (fs != null)
          {
            fs.Close();
            fs.Dispose();
          }
        }
      }

      m_bDirty = false;

      return true;
    }

    private void WriteNewSettings(string path)
    {
      try
      {
        XmlDocument doc = new XmlDocument();
        XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
        doc.InsertBefore(declaration, doc.DocumentElement);

        XmlElement rootNode = doc.CreateElement("settings");
        rootNode.SetAttribute("xml:id", CURRENT_XML_FORMAT_VERSION);
        doc.AppendChild(rootNode);

        XmlElement pluginNode = doc.CreateElement("plugin");

        // Update the plugin settings
        foreach (string key in m_PluginSettings.Keys)
        {
          XmlElement settingNode = doc.CreateElement("entry");
          settingNode.SetAttribute("key", key);
          settingNode.InnerText = m_PluginSettings[key];
          pluginNode.AppendChild(settingNode);
        }
        rootNode.AppendChild(pluginNode);

        // Update the command settings
        foreach (string command in m_CommandSettingsDict.Keys)
        {
          XmlElement commandNode = doc.CreateElement("command");
          commandNode.SetAttribute("name", command);

          foreach (string key in m_CommandSettingsDict[command].Keys)
          {
            XmlElement settingNode = doc.CreateElement("entry");
            settingNode.SetAttribute("key", key);
            settingNode.InnerText = m_CommandSettingsDict[command][key];
            commandNode.AppendChild(settingNode);
          }
          rootNode.AppendChild(commandNode);
        }

        string tmpFile = path + ".tmp";

        // Ensure that the target file does not exist, or the save will croak.
        if (File.Exists(tmpFile))
          File.Delete(tmpFile);

        doc.Save(tmpFile);

        if (File.Exists(path))
          File.Delete(path);

        File.Move(tmpFile, path);
      }
      catch
      {
        throw new IOException("Unable to write settings xml file " + path);
      }
    }

    private static string SettingsDirectory(bool createIfMissing)
    {
      string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      path = System.IO.Path.Combine(path, "McNeel");

      if (createIfMissing && !string.IsNullOrEmpty(path) && !System.IO.Directory.Exists(path))
        System.IO.Directory.CreateDirectory(path);

      path = System.IO.Path.Combine(path, "Rhinoceros");

      if (createIfMissing && !string.IsNullOrEmpty(path) && !System.IO.Directory.Exists(path))
        System.IO.Directory.CreateDirectory(path);

      path = System.IO.Path.Combine(path, "5.0");

      if (createIfMissing && !string.IsNullOrEmpty(path) && !System.IO.Directory.Exists(path))
        System.IO.Directory.CreateDirectory(path);

      path = System.IO.Path.Combine(path, "Settings");

      if (createIfMissing && !string.IsNullOrEmpty(path) && !System.IO.Directory.Exists(path))
        System.IO.Directory.CreateDirectory(path);

      return path;
    }
  }
}
