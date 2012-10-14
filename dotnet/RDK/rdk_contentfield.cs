#pragma warning disable 1591
using System;
using System.Collections.Generic;

using Rhino.Display;
using Rhino.Geometry;


#if RDK_UNCHECKED
namespace Rhino.Render.Fields
{
  /////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Dictionary containing RenderContent data fields, add fields to this
  /// dictionary in your derived RenderContent classes constructor.  Get field
  /// values using the TryGet[data type]() methods and set them using the Set()
  /// method.
  /// </summary>
  /// <example>
  /// [System.Runtime.InteropServices.Guid("ABE4059B-9BD7-451C-91B2-67C2F188860A")]
  /// public class CustomMaterial : RenderMaterial
  /// {
  ///   public override string TypeName { get { return "CSharp Custom Material"; } }
  ///   public override string TypeDescription { get { return "My first custom .NET material"; } }
  /// 
  ///   public CustomMaterial()
  ///   {
  ///     Fields.AddField("bool", false, "Yes/No");
  ///     Fields.AddField("color", Rhino.Display.Color4f.White, "Color");
  ///   }
  /// }
  /// </example>
  public sealed class FieldDictionary
  {
    #region members
    /// <summary>
    /// RenderContent that owns this dictionary.
    /// </summary>
    private readonly RenderContent m_content;
    /// <summary>
    /// Field dictionary used to store and access field values.
    /// </summary>
    private readonly Dictionary<string, Field> m_dictionary = new Dictionary<string, Field>();
    #endregion members

    #region Construction/creation
    /// <summary>
    /// Internal constructor, this object should only be created by the
    /// RenderContent constructor.
    /// </summary>
    /// <param name="content">Owner of this dictionary</param>
    internal FieldDictionary(RenderContent content)
    {
      m_content = content;
    }
    #endregion Construction/creation

    #region IDisposable required
    /// <summary>
    /// Clean up the Field C++ pointers
    /// </summary>
    internal void InternalDispose()
    {
      foreach (KeyValuePair<string,Field>kvp in m_dictionary)
        kvp.Value.IneternalDispose();
      m_dictionary.Clear();
    }
    #endregion IDisposable required

    #region Private and internal helper methods
    /// <summary>
    /// Search the underlying C++ content objects field list and return a
    /// temporary Field object which can be used to access the field data.
    /// The Field object can safely be disposed of when done.
    /// </summary>
    /// <param name="key">Field dictionary key, it is case insensitive.</param>
    /// <returns>
    /// Returns a temporary Field pointer attached to the C++ field if it is
    /// found otherwise; returns null if it is not found.
    /// </returns>
    private Field FieldFromContent(string key)
    {
      // Content pointer
      IntPtr contentPointer = m_content.ConstPointer();
      // Content C++ pointer has not been created yet so there is no place to
      // look for the field.
      if (IntPtr.Zero == contentPointer) return null;
      // Call the C++ SDK and get a pointer to the requested field
      IntPtr fieldPointer = UnsafeNativeMethods.Rdk_RenderContent_FindField(contentPointer, key);
      // Field not found so return null
      if (IntPtr.Zero == fieldPointer) return null;
      // Create a new temporary Field[data type] object and attach it to this pointer.
      Field result = Field.FieldFromPointer(m_content, fieldPointer, key);
      return result;
    }
    /// <summary>
    /// Search dictionary for field that matches the key name, if the field is
    /// not found in the runtime dictionary and searchContent is true the check
    /// the content for the field, if the field is found on the content object
    /// and addToDictionary is true then add it to the dictionary.
    /// </summary>
    /// <param name="key">Look for a field with a matching Key value</param>
    /// <param name="searchContent">If this is true and the field is not found
    /// in the runtime dictionary call FindContentField to search the content
    /// object for the field.
    /// </param>
    /// <returns>Content field associated with the requested key or null if the
    /// field is not found.</returns>
    private Field FindField(string key, bool searchContent)
    {
      Field field;
      if (!m_dictionary.TryGetValue(key, out field) && searchContent)
        field = FieldFromContent(key);
      return field;
    }
    #endregion Private and internal helper methods

    #region Overloaded AddField methods for the supported data types
    /// <summary>
    /// AddField a new Field to the dictionary, will throw an exception if the key
    /// is already in the dictionary or if value is not a supported type.
    /// </summary>
    /// <param name="field">
    /// The new Field object to add, this will not create the Fields C++
    /// pointer, that happens later on after the dictionary C++ object has
    /// been created.
    /// </param>
    /// <returns>Returns the newly added Field object.</returns>
    private Field AddField(Field field)
    {
      try
      {
        // Key name can not be empty
        if (string.IsNullOrEmpty(field.Key)) throw new ArgumentNullException("key");
        // AddField the new field to the dictionary, this will throw an exception if
        // the key was previously added
        m_dictionary.Add(field.Key, field);
      }
      catch (Exception)
      {
        if (null != field) field.IneternalDispose();
        field = null;
        throw;
      }
      return field;
    }
    /// <summary>
    /// AddField a new StringField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public StringField Add(string key, string value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new StringField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public StringField Add(string key, string value, string prompt)
    {
      return AddField(new StringField(m_content, IntPtr.Zero, key, prompt, value)) as StringField;
    }
    /// <summary>
    /// AddField a new BoolField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public BoolField Add(string key, bool value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new BoolField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public BoolField Add(string key, bool value, string prompt)
    {
      return AddField(new BoolField(m_content, IntPtr.Zero, key, prompt, value)) as BoolField;
    }
    /// <summary>
    /// AddField a new IntField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public IntField Add(string key, int value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new IntField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public IntField Add(string key, int value, string prompt)
    {
      return AddField(new IntField(m_content, IntPtr.Zero, key, prompt, value)) as IntField;
    }
    /// <summary>
    /// AddField a new FloatField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public FloatField Add(string key, float value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new FloatField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public FloatField Add(string key, float value, string prompt)
    {
      return AddField(new FloatField(m_content, IntPtr.Zero, key, prompt, value)) as FloatField;
    }
    /// <summary>
    /// AddField a new DoubleField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public DoubleField Add(string key, double value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new DoubleField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public DoubleField Add(string key, double value, string prompt)
    {
      return AddField(new DoubleField(m_content, IntPtr.Zero, key, prompt, value)) as DoubleField;
    }
    /// <summary>
    /// AddField a new Color4fField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public Color4fField Add(string key, Color4f value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new Color4fField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public Color4fField Add(string key, Color4f value, string prompt)
    {
      return AddField(new Color4fField(m_content, IntPtr.Zero, key, prompt, value)) as Color4fField;
    }
    /// <summary>
    /// AddField a new Color4fField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public Color4fField Add(string key, System.Drawing.Color value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new Color4fField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public Color4fField Add(string key, System.Drawing.Color value, string prompt)
    {
      return Add(key, new Color4f(value), prompt);
    }
    /// <summary>
    /// AddField a new Vector2dField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public Vector2dField Add(string key, Vector2d value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new Vector2dField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public Vector2dField Add(string key, Vector2d value, string prompt)
    {
      return AddField(new Vector2dField(m_content, IntPtr.Zero, key, prompt, value)) as Vector2dField;
    }
    /// <summary>
    /// AddField a new Vector3dField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public Vector3dField Add(string key, Vector3d value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new Vector3dField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public Vector3dField Add(string key, Vector3d value, string prompt)
    {
      return AddField(new Vector3dField(m_content, IntPtr.Zero, key, prompt, value)) as Vector3dField;
    }
    /// <summary>
    /// AddField a new Point2dField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public Point2dField Add(string key, Point2d value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new Point2dField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public Point2dField Add(string key, Point2d value, string prompt)
    {
      return AddField(new Point2dField(m_content, IntPtr.Zero, key, prompt, value)) as Point2dField;
    }
    /// <summary>
    /// AddField a new Point3dField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public Point3dField Add(string key, Point3d value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new Point3dField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public Point3dField Add(string key, Point3d value, string prompt)
    {
      return AddField(new Point3dField(m_content, IntPtr.Zero, key, prompt, value)) as Point3dField;
    }
    /// <summary>
    /// AddField a new Point4dField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public Point4dField Add(string key, Point4d value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new Point4dField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public Point4dField Add(string key, Point4d value, string prompt)
    {
      return AddField(new Point4dField(m_content, IntPtr.Zero, key, prompt, value)) as Point4dField;
    }
    /// <summary>
    /// AddField a new GuidField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public GuidField Add(string key, Guid value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new GuidField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public GuidField Add(string key, Guid value, string prompt)
    {
      return AddField(new GuidField(m_content, IntPtr.Zero, key, prompt, value)) as GuidField;
    }
    /// <summary>
    /// AddField a new TransformField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public TransformField Add(string key, Transform value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new TransformField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public TransformField Add(string key, Transform value, string prompt)
    {
      return AddField(new TransformField(m_content, IntPtr.Zero, key, prompt, value)) as TransformField;
    }
    /// <summary>
    /// AddField a new DateTimeField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public DateTimeField Add(string key, DateTime value)
    {
      return Add(key, value, string.Empty);
    }
    /// <summary>
    /// AddField a new DateTimeField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    public DateTimeField Add(string key, DateTime value, string prompt)
    {
      return AddField(new DateTimeField(m_content, IntPtr.Zero, key, prompt, value)) as DateTimeField;
    }
    /// <summary>
    /// AddField a new ByteArrayField to the dictionary, will throw an exception if the
    /// key is already in the dictionary.  This will be a data only field and
    /// not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    public ByteArrayField Add(string key, byte[] value)
    {
      return AddField(new ByteArrayField(m_content, IntPtr.Zero, key, string.Empty, value)) as ByteArrayField;
    }

    #endregion Overloaded AddField methods for the supported data types

    #region Overloaded Set methods for supported data types
    /// <summary>
    /// Set the field value with a change notification of 
    /// RenderContent.ChangeContexts.Program.  Will throw an exception if the
    /// key name is invalid or if T is not a supported data type.
    /// </summary>
    /// <typeparam name="T">Data type of the value parameter.</typeparam>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    private void SetHelper<T>(string key, T value)
    {
      SetHelper(key, value, RenderContent.ChangeContexts.Program);
    }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw an exception if the key name is invalid or if
    /// T is not a supported data type.
    /// </summary>
    /// <typeparam name="T">Data type of the value parameter.</typeparam>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    private void SetHelper<T>(string key, T value, RenderContent.ChangeContexts changeContext)
    {
      // Look for the Field in this dictionary first, if it is not found then
      // ask the RenderContent C++ object for the field.
      Field field = FindField(key, true);
      // Field not in this dictionary or in the C++ field list
      if (null == field) throw new InvalidOperationException("Fields dictionary does not contain this key " + key + ".");
      // Set the field value
      field.Set(value, changeContext);
    }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, string value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, string value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, bool value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, bool value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, int value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, int value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, float value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, float value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, double value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, double value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, Color4f value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, Color4f value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, System.Drawing.Color value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, System.Drawing.Color value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, Vector2d value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, Vector2d value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, Vector3d value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, Vector3d value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, Point2d value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, Point2d value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, Point3d value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, Point3d value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, Point4d value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, Point4d value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, Guid value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, Guid value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, Transform value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, Transform value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, DateTime value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, DateTime value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    public void Set(string key, byte[] value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, byte[] value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
    #endregion Overloaded Set methods for supported data types

    #region Tag methods
    /// <summary>
    /// Sets an object that contains data to associate with the field.
    /// </summary>
    /// <param name="key">Key name for the field to tag.</param>
    /// <param name="tag">Data to associate with the field.</param>
    /// <returns>True if the field is found and the tag was set otherwise false is returned.</returns>
    public bool SetTag(string key, object tag)
    {
      Field field = FindField(key, true);
      if (null == field) return false;
      field.Tag = tag;
      return true;
    }
    /// <summary>
    /// Gets object that contains data associate with a field.
    /// </summary>
    /// <param name="key">Key name of the field to get.</param>
    /// <param name="tag">Data associated with the field.</param>
    /// <returns>
    /// Returns true if the field is found and its tag was retrieved otherwise;
    /// returns false.
    /// </returns>
    public bool TryGetTag(string key, out object tag)
    {
      Field field = FindField(key, false);
      tag = (null == field) ? null : field.Tag;
      return (null != field);
    }
    #endregion Tag methods

    #region Overloaded TryGetValue methods
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out string value)
    {
      StringField field = FindField(key, true) as StringField;
      value = null == field ? string.Empty : field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out bool value)
    {
      BoolField field = FindField(key, true) as BoolField;
      value = null != field && field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out int value)
    {
      IntField field = FindField(key, true) as IntField;
      value = null == field ? 0 : field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out double value)
    {
      DoubleField field = FindField(key, true) as DoubleField;
      value = null == field ? 0 : field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out float value)
    {
      FloatField field = FindField(key, true) as FloatField;
      value = null == field ? 0 : field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out Color4f value)
    {
      Color4fField field = FindField(key, true) as Color4fField;
      value = null == field ? Color4f.Empty : field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out System.Drawing.Color value)
    {
      Color4fField field = FindField(key, true) as Color4fField;
      value = null == field ? System.Drawing.Color.Empty : field.SystemColorValue;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out Vector2d value)
    {
      Vector2dField field = FindField(key, true) as Vector2dField;
      value = null == field ? Vector2d.Unset : field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out Vector3d value)
    {
      Vector3dField field = FindField(key, true) as Vector3dField;
      value = null == field ? Vector3d.Unset : field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out Point2d value)
    {
      Point2dField field = FindField(key, true) as Point2dField;
      value = null == field ? Point2d.Unset : field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out Point3d value)
    {
      Point3dField field = FindField(key, true) as Point3dField;
      value = null == field ? Point3d.Unset : field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out Point4d value)
    {
      Point4dField field = FindField(key, true) as Point4dField;
      value = null == field ? Point4d.Unset : field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out Guid value)
    {
      GuidField field = FindField(key, true) as GuidField;
      value = null == field ? Guid.Empty : field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out Transform value)
    {
      TransformField field = FindField(key, true) as TransformField;
      value = null == field ? Transform.Unset : field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out DateTime value)
    {
      DateTimeField field = FindField(key, true) as DateTimeField;
      value = null == field ? DateTime.Now : field.Value;
      return (null != field);
    }
    /// <summary>
    /// Find a field with the specified key and get its value if found.
    /// </summary>
    /// <param name="key">Key name of the field to get a value for.</param>
    /// <param name="value">Output parameter which will receive the field value.</param>
    /// <returns>
    /// Returns true if the key is found and the value parameter is set to the
    /// field value.  Returns false if the field was not found.
    /// </returns>
    public bool TryGetValue(string key, out byte[] value)
    {
      ByteArrayField field = FindField(key, true) as ByteArrayField;
      value = null == field ? null : field.Value;
      return (null != field);
    }
    #endregion Overloaded TryGetValue methods
  }
  /////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Generic data fields used to add publicly accessible properties to
  /// RenderContent.FieldDictionary.  These should be created by calling a
  /// FieldDictaionary.Add() method on a Render content object.  These are
  /// allocated after the RenderContent object's C++ object is created and
  /// added to the underlying C++ objects content dictionary, who ever
  /// allocates a field is responsible for deleting it so these objects clean
  /// up the C++ pointers when they are disposed of.
  /// </summary>
  public abstract class Field
  {
    internal enum ChangeContexts : int
    {
      UI = 0,        // Change occurred as a result of user activity in the content's UI.
      Drop = 1,      // Change occurred as a result of drag and drop.
      Program = 2,   // Change occurred as a result of internal program activity.
      Ignore = 3,    // Change can be disregarded.
      Tree = 4,      // Change occurred within the content tree (e.g., nodes reordered).
      Undo = 5,      // Change occurred as a result of an undo.
      FieldInit = 6, // Change occurred as a result of a field initialization.
      Serialize = 7, // Change occurred during serialization (loading).
    }

    #region Members
    /// <summary>
    /// If this is true then the m_fieldPointer is deleted when this object is
    /// disposed of, if Attached is called when accessing data provided by
    /// another plug-in then a temporary version of this field will be returned
    /// and its value extracted by the RenderContent.FieldDictionary, in that
    /// Attach is called instead of CreaetCppPointer() so the m_fieldPointer
    /// should not be deleted.
    /// </summary>
    private bool m_autoDelete = true;
    /// <summary>
    /// Place holder for the initial field value, this will get used by
    /// CreateCppPointer(), it will call Set(m_initialValue) after creating the
    /// C++ pointer to initialize the field value.
    /// </summary>
    private object m_initialValue;
    #endregion Members

    #region Properties
    /// <summary>
    /// C++ pointer associated with this object, used for data access.
    /// </summary>
    private IntPtr m_fieldPointer = IntPtr.Zero;
    /// <summary>
    /// C++ pointer associated with this object, used for data access.
    /// </summary>
    internal IntPtr FieldPointer { get { return m_fieldPointer; } }
    /// <summary>
    /// Field key value string set by constructor
    /// </summary>
    readonly string m_key;
    /// <summary>
    /// Field key value string set by constructor
    /// </summary>
    public string Key { get { return m_key; } }
    /// <summary>
    /// Optional UI prompt string set by constructor
    /// </summary>
    readonly string m_prompt;
    /// <summary>
    /// Optional UI prompt string set by constructor
    /// </summary>
    public string Prompt { get { return m_prompt; } }
    /// <summary>
    /// Gets or sets an object that contains data to associate with the field.
    /// </summary>
    /// <returns>
    /// An object that contains information that is associated with the field.
    /// </returns>
    public object Tag
    {
      get { return _tag; }
      set { _tag = value; }
    }
    private object _tag;
    #endregion Properties

    #region Set methods
    /// <summary>
    /// Set the field value with a change notification of 
    /// RenderContent.ChangeContexts.Program.  Will throw an exception if the
    /// key name is invalid or if T is not a supported data type.
    /// </summary>
    /// <typeparam name="T">Data type of the value parameter.</typeparam>
    /// <param name="value">New value for this field.</param>
    internal void Set<T>(T value)
    {
      Set(value, RenderContent.ChangeContexts.Program);
    }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw an exception if the key name is invalid or if
    /// T is not a supported data type.
    /// </summary>
    /// <typeparam name="T">Data type of the value parameter.</typeparam>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    internal void Set<T>(T value, RenderContent.ChangeContexts changeContext)
    {
      IntPtr fieldPointer = FieldPointer;
      if (IntPtr.Zero == fieldPointer)
      {
        // Cache the value for use by CreateCppPointer()
        m_initialValue = value;
      }
      else
      {
        // Convert the value to a variant, will throw an exception if the value
        // type is not supported
        using (Variant varient = new Variant(value))
        {
          // Get the variant C++ pointer
          IntPtr variantPointer = varient.NonConstPointer();
          // Tell the C++ RDK to change the value
          int rc = UnsafeNativeMethods.Rdk_ContentField_SetVariantParameter(fieldPointer, variantPointer, (int)changeContext);
          // If the C++ RDK failed to set the value throw an exception.
          if (1 != rc) throw new InvalidOperationException("SetNamedParamter doesn't support this type.");
        }
      }
    }
    #endregion Set methods

    #region Methods to access value as specific data types
    /// <summary>
    /// Get field value as a string.
    /// </summary>
    /// <returns>Returns the field value as a string if possible.</returns>
    protected string ValueAsString()
    {
      IntPtr fieldPointer = FieldPointer;
      // Field is not initialized so return the default value
      if (fieldPointer == IntPtr.Zero)
        return (null != m_initialValue ? m_initialValue.ToString() : string.Empty);
      // Call the C++ RDK and get the Variant value as a string
      using (Runtime.StringHolder stringHolder = new Runtime.StringHolder())
      {
        IntPtr stringPointer = stringHolder.NonConstPointer();
        UnsafeNativeMethods.Rdk_ContentField_StringValue(FieldPointer, stringPointer);
        return stringHolder.ToString();
      }
    }
    /// <summary>
    /// Return field value as a bool.
    /// </summary>
    /// <returns>Returns field value as a bool. </returns>
    protected bool ValueAsBool()
    {
      IntPtr fieldPointer = FieldPointer;
      // Field is not initialized so return the default value
      if (fieldPointer == IntPtr.Zero)
        return (m_initialValue is bool && (bool)m_initialValue);
      // Call the C++ RDK and get the Variant value as a bool
      int result = UnsafeNativeMethods.Rdk_ContentField_BoolValue(fieldPointer);
      return (result == 1);
    }
    /// <summary>
    /// Return field value as integer.
    /// </summary>
    /// <returns>Return the field value as an integer.</returns>
    protected int ValueAsInt()
    {
      IntPtr fieldPointer = FieldPointer;
      // Field is not initialized so return the default value
      if (fieldPointer == IntPtr.Zero)
        return (m_initialValue is int ? (int)m_initialValue : 0);
      // Call the C++ RDK and get the Variant value as an integer
      int result = UnsafeNativeMethods.Rdk_ContentField_IntValue(fieldPointer);
      return result;
    }
    /// <summary>
    /// Return field value as a double precision number.
    /// </summary>
    /// <returns>Return the field value as a double precision number.</returns>
    protected double ValueAsDouble()
    {
      IntPtr fieldPointer = FieldPointer;
      // Field is not initialized so return the default value
      if (fieldPointer == IntPtr.Zero)
        return (m_initialValue is double ? (double)m_initialValue : 0.0);
      // Call the C++ RDK and get the Variant value as a double
      double result = UnsafeNativeMethods.Rdk_ContentField_DoubleValue(fieldPointer);
      return result;
    }
    /// <summary>
    /// Return field value as floating point number.
    /// </summary>
    /// <returns>Return the field value as an floating point number.</returns>
    protected float ValueAsFloat()
    {
      IntPtr fieldPointer = FieldPointer;
      // Field is not initialized so return the default value
      if (fieldPointer == IntPtr.Zero)
        return (m_initialValue is float ? (float)m_initialValue : 0f);
      // Call the C++ RDK and get the Variant value as a float
      float result = UnsafeNativeMethods.Rdk_ContentField_FloatValue(fieldPointer);
      return result;
    }
    /// <summary>
    /// Return field as a Rhino.Display.Color4f color value.
    /// </summary>
    /// <returns>Return field as a Rhino.Display.Color4f color value.</returns>
    protected Color4f ValueAsColor4f()
    {
      IntPtr fieldPointer = FieldPointer;
      // Field is not initialized so return the default value
      if (fieldPointer == IntPtr.Zero)
        return (m_initialValue is Display.Color4f ? (Display.Color4f)m_initialValue : Display.Color4f.Empty);
      Color4f result = Color4f.Empty;
      // Call the C++ RDK and get the Variant value as a Color4f value
      UnsafeNativeMethods.Rdk_ContentField_ColorValue(fieldPointer, ref result);
      return result;
    }
    /// <summary>
    /// Return field as a Rhino.Geometry.Vector2d color value.
    /// </summary>
    /// <returns>Return field as a Rhino.Geometry.Vector2d color value.</returns>
    protected Vector2d ValueAsVector2d()
    {
      IntPtr fieldPointer = FieldPointer;
      // Field is not initialized so return the default value
      if (fieldPointer == IntPtr.Zero)
        return (m_initialValue is Vector2d ? (Vector2d)m_initialValue : Vector2d.Unset);
      Vector2d result = Vector2d.Unset;
      // Call the C++ RDK and get the Variant value as a Vector2d value
      UnsafeNativeMethods.Rdk_ContentField_Vector2dValue(fieldPointer, ref result);
      return result;
    }
    /// <summary>
    /// Return field as a Rhino.Geometry.Vector3d color value.
    /// </summary>
    /// <returns>Return field as a Rhino.Geometry.Vector3d color value.</returns>
    protected Vector3d ValueAsVector3d()
    {
      IntPtr fieldPointer = FieldPointer;
      // Field is not initialized so return the default value
      if (fieldPointer == IntPtr.Zero)
        return (m_initialValue is Vector3d ? (Vector3d)m_initialValue : Vector3d.Unset);
      Vector3d result = Vector3d.Unset;
      // Call the C++ RDK and get the Variant value as a Vector3d value
      UnsafeNativeMethods.Rdk_ContentField_Vector3dValue(fieldPointer, ref result);
      return result;
    }
    /// <summary>
    /// Return field as a Rhino.Geometry.Point2d color value.
    /// </summary>
    /// <returns>Return field as a Rhino.Geometry.Point2d color value.</returns>
    protected Point2d ValueAsPoint2d() { return new Point2d(ValueAsVector2d()); }
    /// <summary>
    /// Return field as a Rhino.Geometry.Point3d color value.
    /// </summary>
    /// <returns>Return field as a Rhino.Geometry.Point3d color value.</returns>
    protected Point3d ValueAsPoint3d() { return new Point3d(ValueAsVector3d()); }
    /// <summary>
    /// Return field as a Rhino.Geometry.Point4d color value.
    /// </summary>
    /// <returns>Return field as a Rhino.Geometry.Point4d color value.</returns>
    protected Point4d ValueAsPoint4d()
    {
      IntPtr fieldPointer = FieldPointer;
      // Field is not initialized so return the default value
      if (fieldPointer == IntPtr.Zero)
        return (m_initialValue is Point4d ? (Point4d)m_initialValue : Point4d.Unset);
      Point4d result = Point4d.Unset;
      // Call the C++ RDK and get the Variant value as a Point4d
      UnsafeNativeMethods.Rdk_ContentField_Point4dValue(fieldPointer, ref result);
      return result;
    }
    /// <summary>
    /// Return field value as Guid.
    /// </summary>
    /// <returns>Return the field value as an Guid.</returns>
    protected Guid ValueAsGuid()
    {
      IntPtr fieldPointer = FieldPointer;
      // Field is not initialized so return the default value
      if (fieldPointer == IntPtr.Zero)
        return (m_initialValue is Guid ? (Guid)m_initialValue : Guid.Empty);
      // Call the C++ RDK and get the Variant value as a Guid
      Guid result = UnsafeNativeMethods.Rdk_ContentField_UUIDValue(fieldPointer);
      return result;
    }
    /// <summary>
    /// Return field as a Rhino.Geometry.Transform color value.
    /// </summary>
    /// <returns>Return field as a Rhino.Geometry.Transform color value.</returns>
    protected Transform ValueAsTransform()
    {
      IntPtr fieldPointer = FieldPointer;
      // Field is not initialized so return the default value
      if (fieldPointer == IntPtr.Zero)
        return (m_initialValue is Transform ? (Transform)m_initialValue : Transform.Unset);
      // Call the C++ RDK and get the Variant value as a Transform value
      Transform result = Transform.Unset;
      UnsafeNativeMethods.Rdk_ContentField_XformValue(fieldPointer, ref result);
      return result;
    }
    /// <summary>
    /// Return field as a DateTime value.
    /// </summary>
    /// <returns>Return field as a DateTime value.</returns>
    protected DateTime ValueAsDateTime()
    {
      IntPtr fieldPointer = FieldPointer;
      // Field is not initialized so return the default value
      if (fieldPointer == IntPtr.Zero)
        return (m_initialValue is DateTime ? (DateTime)m_initialValue : DateTime.Now);
      // Call the C++ RDK and get the Variant value as a DateTime value
      int month = 0, year = 0, day = 0, hours = 0, minutes = 0, seconds = 0;
      UnsafeNativeMethods.Rdk_ContentField_TimeValue(fieldPointer, ref year, ref month, ref day, ref hours, ref minutes, ref seconds);
      DateTime result = new DateTime(year, month, day, hours, minutes, seconds);
      return result;
    }
    /// <summary>
    /// Return field as a byte array.
    /// </summary>
    /// <returns>Return field as a byte array.</returns>
    protected byte[] ValueAsByteArray()
    {
      IntPtr fieldPointer = FieldPointer;
      // Field is not initialized so return the default value
      if (IntPtr.Zero == fieldPointer)
        return (m_initialValue is byte[] ? (byte[])m_initialValue : null);
      // Call the C++ RDK and get the size of the Variant buffer
      int sizeOfResult = UnsafeNativeMethods.Rdk_ContentField_GetByteArrayValueSize(fieldPointer);
      // Allocate a buffer to receive a copy of the Variant data
      byte[] result = new byte[sizeOfResult];
      // Copy the C++ buffer into the result byte array
      UnsafeNativeMethods.Rdk_ContentField_GetByteArrayValue(fieldPointer, result, sizeOfResult);
      return result;
    }
    #endregion Methods to access value as specific data types

    /// <summary>
    /// Field constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">C++ pointer to attach to.</param>
    /// <param name="key">Unique key for this field</param>
    /// <param name="prompt">Display string used by the user interface</param>
    /// <param name="initialValue">
    /// Initial value used to initialize the field after creating the C++
    /// pointer.
    /// </param>
    protected Field(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, object initialValue)
    {
      // Value which will be used to set the initial state of the C++ field
      // variant when calling CreateCppPointer()
      m_initialValue = initialValue;
      // Field key
      m_key = key;
      // User interface display prompt
      m_prompt = prompt;
      // Create the underlying C++ pointer
      CreateCppPointer(renderContent, attachToPointer);
    }
    /// <summary>
    /// Create the RDK C++ field object and set its initial value, fields are
    /// added to a RenderContent.FieldDictionary in the RenderContent
    /// constructor before the RenderContent C++ pointer is created, the
    /// RenderContent C++ pointer is required when creating a field in order
    /// for the field to get added to the RenderContent C++ Field list so this
    /// method is called by RenderContent when it is safe to create the Field
    /// C++ pointers.
    /// </summary>
    /// <param name="content">RenderContent.FiledDictionary that owns this Field.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    protected void CreateCppPointer(RenderContent content, IntPtr attachToPointer)
    {
      // Should not happen but you never know, this will bail if the C++ pointer
      // was created by a previous call to CreateCppPointer or Attach
      if (IntPtr.Zero != m_fieldPointer) return;
      if (IntPtr.Zero == attachToPointer)
      {
        // Get the RenderContent C++ pointer, fields get added to content field lists
        // so you have to have a valid Content C++ pointer when creating a field.
        IntPtr contentPointer = content.ConstPointer();
        // If there is a user interface prompt string then set the add to user interface flag.
        bool isVisibleToAutoUi = !string.IsNullOrEmpty(Prompt);
        // Allocate the objects C++ pointer.
        m_fieldPointer = UnsafeNativeMethods.Rdk_ContentField_New(contentPointer, Key, Prompt, isVisibleToAutoUi ? 0 : 0x8001);
        // Initialize the field value
        Set(m_initialValue);
        // If m_initialValue can be disposed of then dispose of it now
        if (m_initialValue is IDisposable) (m_initialValue as IDisposable).Dispose();
        m_initialValue = null;
      }
      else
      {
        m_autoDelete = false;
        m_fieldPointer = attachToPointer;
      }
    }
    /// <summary>
    /// IDisposable required method
    /// </summary>
    internal void IneternalDispose()
    {
      if (IntPtr.Zero != m_fieldPointer && m_autoDelete)
      {
        UnsafeNativeMethods.Rdk_ContentField_Delete(m_fieldPointer);
        m_fieldPointer = IntPtr.Zero;
      }
    }

    /// <summary>
    /// Create a Field[data type] object from a content field pointer using its values
    /// variant type to figure out what kind of Field[data type] object to return.
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields dictionary the field belongs to. </param>
    /// <param name="fieldPointer">
    /// C++ pointer to the field object, will throw a ArgumentNullException if this
    /// value is null.
    /// </param>
    /// <param name="key">Key value for the new field</param>
    /// <returns></returns>
    static internal Field FieldFromPointer(RenderContent renderContent, IntPtr fieldPointer, string key)
    {
      if (null == fieldPointer) throw new ArgumentNullException("fieldPointer");
      // Get the field user interface prompt string
      string prompt;
      using (Runtime.StringHolder sh = new Runtime.StringHolder())
      {
        IntPtr stringPointer = sh.NonConstPointer();
        UnsafeNativeMethods.Rdk_ContentField_FriendlyName(fieldPointer, stringPointer);
        prompt = sh.ToString();
      }
      // Create the field from the values Variant type
      Field result;
      IntPtr variantPointer = UnsafeNativeMethods.Rdk_ContentField_Value(fieldPointer);
      switch (UnsafeNativeMethods.Rdk_Variant_Type(variantPointer))
      {
        case (int)Variant.VariantTypes.Bool:
          result = new BoolField(renderContent, fieldPointer, key, prompt, false);
          break;
        case (int)Variant.VariantTypes.Color:
          result = new Color4fField(renderContent, fieldPointer, key, prompt, Color4f.Empty);
          break;
        case (int)Variant.VariantTypes.Double:
          result = new DoubleField(renderContent, fieldPointer, key, prompt, 0.0);
          break;
        case (int)Variant.VariantTypes.Float:
          result = new FloatField(renderContent, fieldPointer, key, prompt, 0f);
          break;
        case (int)Variant.VariantTypes.Integer:
          result = new IntField(renderContent, fieldPointer, key, prompt, 0);
          break;
        case (int)Variant.VariantTypes.Matrix:
          result = new TransformField(renderContent, fieldPointer, key, prompt, Transform.Unset);
          break;
        case (int)Variant.VariantTypes.Point4d:
          result = new Point4dField(renderContent, fieldPointer, key, prompt, Point4d.Unset);
          break;
        case (int)Variant.VariantTypes.String:
          result = new StringField(renderContent, fieldPointer, key, prompt, string.Empty);
          break;
        case (int)Variant.VariantTypes.Time:
          result = new DateTimeField(renderContent, fieldPointer, key, prompt, DateTime.Now);
          break;
        case (int)Variant.VariantTypes.Uuid:
          result = new GuidField(renderContent, fieldPointer, key, prompt, Guid.Empty);
          break;
        case (int)Variant.VariantTypes.Vector2d:
          result = new Vector2dField(renderContent, fieldPointer, key, prompt, Vector2d.Unset);
          break;
        case (int)Variant.VariantTypes.Vector3d:
          result = new Vector3dField(renderContent, fieldPointer, key, prompt, Vector3d.Unset);
          break;
        //case (int)Variant.VariantTypes.Pointer:
        default:
          result = new ByteArrayField(renderContent, fieldPointer, key, prompt, null);
          break;
      }
      return result;
    }
  }
  /// <summary>
  /// String field value class
  /// </summary>
  public class StringField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal StringField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, string value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public string Value
    {
      get { return ValueAsString(); }
      set { Set(value); }
    }
  }
  /// <summary>
  /// bool field value class
  /// </summary>
  public class BoolField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal BoolField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, bool value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public bool Value
    {
      get { return ValueAsBool(); }
      set { Set(value); }
    }
  }
  /// <summary>
  /// Integer field value class
  /// </summary>
  public class IntField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this IntField</param>
    internal IntField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, int value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public int Value
    {
      get { return ValueAsInt(); }
      set { Set(value); }
    }
  }
  /// <summary>
  /// float field value class
  /// </summary>
  public class FloatField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal FloatField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, float value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public float Value
    {
      get { return ValueAsFloat(); }
      set { Set(value); }
    }
  }
  /// <summary>
  /// double field value class
  /// </summary>
  public class DoubleField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal DoubleField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, double value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public double Value
    {
      get { return ValueAsDouble(); }
      set { Set(value); }
    }
  }
  /// <summary>
  /// Color4f field value class
  /// </summary>
  public class Color4fField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal Color4fField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Color4f value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal Color4fField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, System.Drawing.Color value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Display.Color4f Value
    {
      get { return ValueAsColor4f(); }
      set { Set(value); }
    }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public System.Drawing.Color SystemColorValue
    {
      get { return ValueAsColor4f().AsSystemColor(); }
      set { Value = new Color4f(value); }
    }
  }
  /// <summary>
  /// Vector2d field value class
  /// </summary>
  public class Vector2dField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal Vector2dField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Vector2d value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Vector2d Value
    {
      get { return ValueAsVector2d(); }
      set { Set(value); }
    }
  }
  /// <summary>
  /// Vector3d field value class
  /// </summary>
  public class Vector3dField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal Vector3dField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Vector3d value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Vector3d Value
    {
      get { return ValueAsVector3d(); }
      set { Set(value); }
    }
  }
  /// <summary>
  /// Point2d field value class
  /// </summary>
  public class Point2dField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal Point2dField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Point2d value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Point2d Value
    {
      get { return ValueAsPoint2d(); }
      set { Set(value); }
    }
  }
  /// <summary>
  /// Point3d field value class
  /// </summary>
  public class Point3dField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal Point3dField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Point3d value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Point3d Value
    {
      get { return ValueAsPoint3d(); }
      set { Set(value); }
    }
  }
  /// <summary>
  /// Point4d field value class
  /// </summary>
  public class Point4dField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal Point4dField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Point4d value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Point4d Value
    {
      get { return ValueAsPoint4d(); }
      set { Set(value); }
    }
  }
  /// <summary>
  /// Guid field value class
  /// </summary>
  public class GuidField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal GuidField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Guid value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Guid Value
    {
      get { return ValueAsGuid(); }
      set { Set(value); }
    }
  }
  /// <summary>
  /// Transform field value class
  /// </summary>
  public class TransformField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal TransformField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Transform value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Transform Value
    {
      get { return ValueAsTransform(); }
      set { Set(value); }
    }
  }
  /// <summary>
  /// DateTime field value class
  /// </summary>
  public class DateTimeField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal DateTimeField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, DateTime value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public DateTime Value
    {
      get { return ValueAsDateTime(); }
      set { Set(value); }
    }
  }
  /// <summary>
  /// ByteArray field value class
  /// </summary>
  public class ByteArrayField : Field
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    internal ByteArrayField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, byte[] value) : base(renderContent, attachToPointer, key, prompt, value) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public byte[] Value
    {
      get { return ValueAsByteArray(); }
      set { Set(value); }
    }
  }
}

#endif
