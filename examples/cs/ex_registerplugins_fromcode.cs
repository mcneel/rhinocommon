
namespace examples_cs
{
    public static class PluginRegistration
    {
        public static bool RegisterPlugins(IEnumerable<FileInfo> rhpFiles)
        {
            // This method was inspired by http://wiki.mcneel.com/developer/installingandregisteringaplugin
            // to register plugin details prior to starting Rhino
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += HandleReflectionOnlyAssemblyResolve;
            try
            {
                // if you want to register for all users, open hklm
                // RegistryKey hklm = Registry.LocalMachine
                RegistryKey hkcu = Registry.CurrentUser;
                // if you have another version of Rhino installed (maybe 32-bit?) change the plugins base key
                
                RegistryKey pluginsBase = hkcu.OpenSubKey(@"Software\McNeel\Rhinoceros\5.0x64\Plug-Ins\", true);
                if (null == pluginsBase)
                {
                    Log(@"Could not find registry key 'HKEY_CURRENT_USER\Software\McNeel\Rhinoceros\5.0x64\Plug-Ins\'");
                    Log(@"Is Rhino 64-bit installed?");
                    return false;
                }
                String[] subkeyNames = pluginsBase.GetSubKeyNames();
                foreach(FileInfo fi in rhpFiles)
                {
                    Assembly rhp = Assembly.ReflectionOnlyLoadFrom(fi.FullName);
                    IList<CustomAttributeData> customData = rhp.GetCustomAttributesData();
                    CustomAttributeData cad = customData.SingleOrDefault(c => c.ToString().Contains("AssemblyTitleAttribute"));
                    String title;
                    if (null != cad)
                    {
                        String assemblyTitleAttribute = cad.ToString();

                        int index = assemblyTitleAttribute.IndexOf("\"", StringComparison.Ordinal) + 1;
                        int end = assemblyTitleAttribute.LastIndexOf("\"", StringComparison.Ordinal);
                        title = assemblyTitleAttribute.Substring(index, end - index);
                    }
                    else
                    {
                        Log("No AssemblyTitleAttribute found on {0}.", fi.Name);
                        return false;
                    }

                    cad = customData.SingleOrDefault(c => c.ToString().Contains("Guid"));
                    Guid id = Guid.Empty;
                    if (null != cad)
                    {
                        String guidAttribute = cad.ToString();
                        int index = guidAttribute.IndexOf("\"", StringComparison.Ordinal) + 1;
                        int end = guidAttribute.LastIndexOf("\"", StringComparison.Ordinal);
                        String guidString = guidAttribute.Substring(index, end - index);
                        if (!Guid.TryParse(guidString, out id))
                        {
                            Log("Could not parse a GUID from {0}", guidString);
                            return false;
                        }
                    }
                    else
                    {
                        Log("Could not find a GuidAttribute on {0}.", fi.Name);
                        return false;
                    }

                    // ok, we have Guid and Title. Now open or create a RegistrySubKey
                    RegistryKey pluginBaseKey;
                    String subKeyName = subkeyNames.SingleOrDefault(sk => sk.Contains(id.ToString()));
                    if (null != subKeyName)
                    {
                        // if the subkey already exists, update the PlugIn\FileName and PlugIn\FolderName values
                        pluginBaseKey = pluginsBase.OpenSubKey(subKeyName);
                        if (null == pluginBaseKey)
                        {
                            Log(@"Could not open plugin base key 'HKEY_CURRENT_USER\Software\McNeel\Rhinoceros\5.0x64\Plug-Ins\{0}'",
                                id.ToString());
                            return false;
                        }

                        RegistryKey pluginKey = pluginBaseKey.OpenSubKey("PlugIn", true);
                        if (null == pluginKey)
                        {
                            Log(@"Could not open plugin base key 'HKEY_CURRENT_USER\Software\McNeel\Rhinoceros\5.0x64\Plug-Ins\{0}\PlugIn'",
                                id.ToString());
                            return false;
                        }

                        pluginKey.SetValue("FileName", fi.FullName, RegistryValueKind.String);
                        pluginKey.SetValue("FolderName", fi.DirectoryName, RegistryValueKind.String);
                    }
                    else
                    {
                        // if the subkey does not already exist, create Name and FileName values in the
                        // pluginBaseKey. Upon opening of Rhino the plug-ins registry key will be populated
                        // with all the other keys.
                        pluginBaseKey = pluginsBase.CreateSubKey(id.ToString());
                        if (null == pluginBaseKey)
                        {
                            Log(@"Could not create plugin base key 'HKEY_CURRENT_USER\Software\McNeel\Rhinoceros\5.0x64\Plug-Ins\{0}'",
                                id.ToString());
                            return false;
                        }
                        pluginBaseKey.SetValue("Name", title, RegistryValueKind.String);
                        pluginBaseKey.SetValue("FileName", fi.FullName, RegistryValueKind.String);
                    }
                }
            }
            catch(Exception e)
            {
                Log("Editing registry failed: {0}", e.Message);
                return false;
            }
            finally
            {
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= HandleReflectionOnlyAssemblyResolve;
            }
            return true;
        }

        private static void Log(String format, params object[] values)
        {
            Console.WriteLine(format, values);
        }

        private static Assembly HandleReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("RhinoCommon"))
                return Assembly.ReflectionOnlyLoadFrom(@"C:\Program Files\Rhinoceros 5.0\System\RhinoCommon.dll");
            return Assembly.ReflectionOnlyLoad(args.Name);
        }
    }
}