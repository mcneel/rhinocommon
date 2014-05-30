#pragma warning disable 1591
using System;
using System.Collections.Generic;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Runtime.InteropWrappers;


#if RDK_CHECKED
namespace Rhino.Render.Fields
{
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
      foreach (KeyValuePair<string, Field> kvp in m_dictionary)
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
      IntPtr content_pointer = m_content.ConstPointer();
      // Content C++ pointer has not been created yet so there is no place to
      // look for the field.
      if (IntPtr.Zero == content_pointer) return null;
      // Call the C++ SDK and get a pointer to the requested field
      IntPtr field_pointer = UnsafeNativeMethods.Rdk_RenderContent_FindField(content_pointer, key);
      // Field not found so return null
      if (IntPtr.Zero == field_pointer) return null;
      // Create a new temporary Field[data type] object and attach it to this pointer.
      Field result = Field.FieldFromPointer(m_content, field_pointer, key);
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

    #region Public methods
    /// <summary>
    /// Call this method to determine if a this FieldsList contains a field
    /// with the specified field name.
    /// </summary>
    /// <param name="fieldName">Field to search for</param>
    /// <returns>
    /// Returns true if a field with that matches fieldName is found or false
    /// if it is not found.
    /// </returns>
    public bool ContainsField(string fieldName)
    {
      if (string.IsNullOrEmpty(fieldName)) return false;
      var found = FindField(fieldName, true);
      return (null != found);
    }
    /// <summary>
    /// Call this method to get the field with the matching name.
    /// </summary>
    /// <param name="fieldName">Field name to search for.</param>
    /// <returns>
    /// If the field exists in the Fields dictionary then the field is returned
    /// otherwise; null is returned.
    /// </returns>
    public Field GetField(string fieldName)
    {
      if (string.IsNullOrEmpty(fieldName)) return null;
      var field = FindField(fieldName, true);
      return field;
    }

    #endregion Public methods

    #region Overloaded AddField methods for the supported data types
    /// <summary>
    /// Add a new Field to the dictionary.  Will throw an exception if the key
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
        if (string.IsNullOrEmpty(field.Key)) throw new ArgumentNullException("field.Key");
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
    /// Add a new StringField to the dictionary.  This will be a data only
    /// field and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public StringField Add(string key, string value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new StringField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public StringField Add(string key, string value, string prompt)
    {
      return AddField(new StringField(m_content, IntPtr.Zero, key, prompt, value, false)) as StringField;
    }

    /// <summary>
    /// Add a new StringField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public StringField AddTextured(string key, string value, string prompt)
    {
      return AddField(new StringField(m_content, IntPtr.Zero, key, prompt, value, true)) as StringField;
    }

    /// <summary>
    /// Add a new BoolField to the dictionary. This will be a data only field
    /// and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public BoolField Add(string key, bool value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new BoolField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public BoolField Add(string key, bool value, string prompt)
    {
      return AddField(new BoolField(m_content, IntPtr.Zero, key, prompt, value, false)) as BoolField;
    }

    /// <summary>
    /// Add a new BoolField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public BoolField AddTextured(string key, bool value, string prompt)
    {
      return AddField(new BoolField(m_content, IntPtr.Zero, key, prompt, value, true)) as BoolField;
    }

    /// <summary>
    /// Add a new IntField to the dictionary. This will be a data only field
    /// and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public IntField Add(string key, int value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new IntField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public IntField Add(string key, int value, string prompt)
    {
      return AddField(new IntField(m_content, IntPtr.Zero, key, prompt, value, false)) as IntField;
    }

    /// <summary>
    /// Add a new IntField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public IntField AddTextured(string key, int value, string prompt)
    {
      return AddField(new IntField(m_content, IntPtr.Zero, key, prompt, value, true)) as IntField;
    }

    /// <summary>
    /// Add a new FloatField to the dictionary. This will be a data only field
    /// and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public FloatField Add(string key, float value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// AddField a new FloatField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public FloatField Add(string key, float value, string prompt)
    {
      return AddField(new FloatField(m_content, IntPtr.Zero, key, prompt, value, false)) as FloatField;
    }

    /// <summary>
    /// Add a new FloatField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public FloatField AddTextured(string key, float value, string prompt)
    {
      return AddField(new FloatField(m_content, IntPtr.Zero, key, prompt, value, true)) as FloatField;
    }
    /// <summary>
    /// AddField a new DoubleField to the dictionary. This will be a data only
    /// field and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public DoubleField Add(string key, double value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new DoubleField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public DoubleField Add(string key, double value, string prompt)
    {
      return AddField(new DoubleField(m_content, IntPtr.Zero, key, prompt, value, false)) as DoubleField;
    }

    /// <summary>
    /// Add a new DoubleField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public DoubleField AddTextured(string key, double value, string prompt)
    {
      return AddField(new DoubleField(m_content, IntPtr.Zero, key, prompt, value, true)) as DoubleField;
    }

    /// <summary>
    /// Add a new Color4fField to the dictionary. This will be a data only
    /// field and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Color4fField Add(string key, Color4f value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new Color4fField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Color4fField Add(string key, Color4f value, string prompt)
    {
      return AddField(new Color4fField(m_content, IntPtr.Zero, key, prompt, value, false)) as Color4fField;
    }

    /// <summary>
    /// Add a new Color4fField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Color4fField AddTextured(string key, Color4f value, string prompt)
    {
      return AddField(new Color4fField(m_content, IntPtr.Zero, key, prompt, value, true)) as Color4fField;
    }

    /// <summary>
    /// Add a new Color4fField to the dictionary. This will be a data only
    /// field and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Color4fField Add(string key, Rhino.Drawing.Color value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new Color4fField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Color4fField Add(string key, Rhino.Drawing.Color value, string prompt)
    {
      return Add(key, new Color4f(value), prompt);
    }

    /// <summary>
    /// Add a new Color4fField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Color4fField AddTextured(string key, Rhino.Drawing.Color value, string prompt)
    {
      return AddTextured(key, new Color4f(value), prompt);
    }

    /// <summary>
    /// Add a new Vector2dField to the dictionary. This will be a data only
    /// field and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Vector2dField Add(string key, Vector2d value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new Vector2dField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Vector2dField Add(string key, Vector2d value, string prompt)
    {
      return AddField(new Vector2dField(m_content, IntPtr.Zero, key, prompt, value, false)) as Vector2dField;
    }

    /// <summary>
    /// Add a new Vector2dField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Vector2dField AddTextured(string key, Vector2d value, string prompt)
    {
      return AddField(new Vector2dField(m_content, IntPtr.Zero, key, prompt, value, true)) as Vector2dField;
    }

    /// <summary>
    /// Add a new Vector3dField to the dictionary. This will be a data only
    /// field and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Vector3dField Add(string key, Vector3d value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new Vector3dField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Vector3dField Add(string key, Vector3d value, string prompt)
    {
      return AddField(new Vector3dField(m_content, IntPtr.Zero, key, prompt, value, false)) as Vector3dField;
    }

    /// <summary>
    /// Add a new Vector3dField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Vector3dField AddTextured(string key, Vector3d value, string prompt)
    {
      return AddField(new Vector3dField(m_content, IntPtr.Zero, key, prompt, value, true)) as Vector3dField;
    }

    /// <summary>
    /// Add a new Point2dField to the dictionary. This will be a data only
    /// field and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Point2dField Add(string key, Point2d value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new Point2dField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Point2dField Add(string key, Point2d value, string prompt)
    {
      return AddField(new Point2dField(m_content, IntPtr.Zero, key, prompt, value, false)) as Point2dField;
    }

    /// <summary>
    /// Add a new Point2dField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Point2dField AddTextured(string key, Point2d value, string prompt)
    {
      return AddField(new Point2dField(m_content, IntPtr.Zero, key, prompt, value, true)) as Point2dField;
    }

    /// <summary>
    /// Add a new Point3dField to the dictionary. This will be a data only
    /// field and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Point3dField Add(string key, Point3d value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new Point3dField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Point3dField Add(string key, Point3d value, string prompt)
    {
      return AddField(new Point3dField(m_content, IntPtr.Zero, key, prompt, value, false)) as Point3dField;
    }

    /// <summary>
    /// Add a new Point3dField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Point3dField AddTextured(string key, Point3d value, string prompt)
    {
      return AddField(new Point3dField(m_content, IntPtr.Zero, key, prompt, value, true)) as Point3dField;
    }

    /// <summary>
    /// Add a new Point4dField to the dictionary. This will be a data only
    /// field and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Point4dField Add(string key, Point4d value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new Point4dField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Point4dField Add(string key, Point4d value, string prompt)
    {
      return AddField(new Point4dField(m_content, IntPtr.Zero, key, prompt, value, false)) as Point4dField;
    }

    /// <summary>
    /// Add a new Point4dField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public Point4dField AddTextured(string key, Point4d value, string prompt)
    {
      return AddField(new Point4dField(m_content, IntPtr.Zero, key, prompt, value, true)) as Point4dField;
    }

    /// <summary>
    /// Add a new GuidField to the dictionary. This will be a data only field
    /// and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public GuidField Add(string key, Guid value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new GuidField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public GuidField Add(string key, Guid value, string prompt)
    {
      return AddField(new GuidField(m_content, IntPtr.Zero, key, prompt, value, false)) as GuidField;
    }

    /// <summary>
    /// Add a new GuidField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public GuidField AddTextured(string key, Guid value, string prompt)
    {
      return AddField(new GuidField(m_content, IntPtr.Zero, key, prompt, value, true)) as GuidField;
    }

    /// <summary>
    /// Add a new TransformField to the dictionary. This will be a data only
    /// field and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public TransformField Add(string key, Transform value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new TransformField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public TransformField Add(string key, Transform value, string prompt)
    {
      return AddField(new TransformField(m_content, IntPtr.Zero, key, prompt, value, false)) as TransformField;
    }

    /// <summary>
    /// Add a new TransformField to the dictionary. This overload will cause
    /// the field to be tagged as "textured" so that the texturing UI will
    /// appear in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public TransformField AddTextured(string key, Transform value, string prompt)
    {
      return AddField(new TransformField(m_content, IntPtr.Zero, key, prompt, value, true)) as TransformField;
    }

    /// <summary>
    /// Add a new DateTimeField to the dictionary. This will be a data only
    /// field and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public DateTimeField Add(string key, DateTime value)
    {
      return Add(key, value, string.Empty);
    }

    /// <summary>
    /// Add a new DateTimeField to the dictionary.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public DateTimeField Add(string key, DateTime value, string prompt)
    {
      return AddField(new DateTimeField(m_content, IntPtr.Zero, key, prompt, value, false)) as DateTimeField;
    }

    /// <summary>
    /// Add a new DateTimeField to the dictionary. This overload will cause the
    /// field to be tagged as "textured" so that the texturing UI will appear
    /// in automatic UIs.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <param name="prompt">
    /// Prompt to display in the user interface (Content Browsers) if this
    /// is null or an empty string the this field is a data only field and will
    /// not appear in the user interface.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public DateTimeField AddTextured(string key, DateTime value, string prompt)
    {
      return AddField(new DateTimeField(m_content, IntPtr.Zero, key, prompt, value, true)) as DateTimeField;
    }

    /// <summary>
    /// AddField a new ByteArrayField to the dictionary. This will be a data
    /// only field and not show up in the content browsers.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">Initial value for this field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the dictionary
    /// </exception>
    public ByteArrayField Add(string key, byte[] value)
    {
      return AddField(new ByteArrayField(m_content, IntPtr.Zero, key, string.Empty, value, false)) as ByteArrayField;
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
    public void Set(string key, Rhino.Drawing.Color value) { SetHelper(key, value); }
    /// <summary>
    /// Set the field value and send the appropriate change notification to the
    /// render SDK.  Will throw a InvalidOperationException exception if the key
    /// name is not valid.
    /// </summary>
    /// <param name="key">Key name for the field value to change.</param>
    /// <param name="value">New value for this field.</param>
    /// <param name="changeContext">The reason why the value is changing.</param>
    public void Set(string key, Rhino.Drawing.Color value, RenderContent.ChangeContexts changeContext) { SetHelper(key, value, changeContext); }
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
    public bool TryGetValue(string key, out Rhino.Drawing.Color value)
    {
      Color4fField field = FindField(key, true) as Color4fField;
      value = null == field ? Rhino.Drawing.Color.Empty : field.SystemColorValue;
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
    /// Attach is called instead of CreateCppPointer() so the m_fieldPointer
    /// should not be deleted.
    /// </summary>
    private bool m_auto_delete = true;
    /// <summary>
    /// Place holder for the initial field value, this will get used by
    /// CreateCppPointer(), it will call Set(m_initialValue) after creating the
    /// C++ pointer to initialize the field value.
    /// </summary>
    private object m_initial_value;
    #endregion Members

    #region Properties
    private IntPtr m_field_pointer = IntPtr.Zero;
    /// <summary>
    /// C++ pointer associated with this object, used for data access.
    /// </summary>
    internal IntPtr FieldPointer { get { return m_field_pointer; } }

    readonly string m_key;
    /// <summary>
    /// Field key value string set by constructor
    /// </summary>
    public string Key { get { return m_key; } }

    readonly string m_prompt;
    /// <summary>
    /// Optional UI prompt string set by constructor
    /// </summary>
    public string Prompt { get { return m_prompt; } }

    readonly bool m_is_textured;
    public bool IsTextured
    {
      get { return m_is_textured; }
    }

    /// <summary>
    /// Gets or sets an object that contains data to associate with the field.
    /// </summary>
    /// <returns>
    /// An object that contains information that is associated with the field.
    /// </returns>
    public object Tag { get; set; }

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
      var field_pointer = FieldPointer;
      if (IntPtr.Zero == field_pointer)
      {
        // Cache the value for use by CreateCppPointer()
        m_initial_value = value;
      }
      else
      {
        // Convert the value to a variant, will throw an exception if the value
        // type is not supported
        using (var varient = new Variant(value))
        {
          // Get the variant C++ pointer
          var variant_pointer = varient.NonConstPointer();
          // Tell the C++ RDK to change the value
          var rc = UnsafeNativeMethods.Rdk_ContentField_SetVariantParameter(field_pointer, variant_pointer, (int)changeContext);
          // If the C++ RDK failed to set the value throw an exception.
          //  Note: A return value of 1 means the value was changed, 2 means
          //        the current value is equal to "value" so nothing changed
          //        and a value of 0 means there was an error setting the field
          //        value.
          if (rc < 1) throw new InvalidOperationException("SetNamedParamter doesn't support this type.");
        }
      }
    }
    #endregion Set methods

    #region Methods to access value as specific data types
    public abstract object ValueAsObject();
    /// <summary>
    /// Get field value as a string.
    /// </summary>
    /// <returns>Returns the field value as a string if possible.</returns>
    protected string ValueAsString()
    {
      IntPtr field_pointer = FieldPointer;
      // Field is not initialized so return the default value
      if (field_pointer == IntPtr.Zero)
        return (null != m_initial_value ? m_initial_value.ToString() : string.Empty);
      // Call the C++ RDK and get the Variant value as a string
      using (var string_holder = new StringHolder())
      {
        IntPtr string_pointer = string_holder.NonConstPointer();
        UnsafeNativeMethods.Rdk_ContentField_StringValue(FieldPointer, string_pointer);
        return string_holder.ToString();
      }
    }
    /// <summary>
    /// Return field value as a bool.
    /// </summary>
    /// <returns>Returns field value as a bool. </returns>
    protected bool ValueAsBool()
    {
      IntPtr field_pointer = FieldPointer;
      // Field is not initialized so return the default value
      if (field_pointer == IntPtr.Zero)
        return (m_initial_value is bool && (bool)m_initial_value);
      // Call the C++ RDK and get the Variant value as a bool
      int result = UnsafeNativeMethods.Rdk_ContentField_BoolValue(field_pointer);
      return (result == 1);
    }
    /// <summary>
    /// Return field value as integer.
    /// </summary>
    /// <returns>Return the field value as an integer.</returns>
    protected int ValueAsInt()
    {
      IntPtr field_pointer = FieldPointer;
      // Field is not initialized so return the default value
      if (field_pointer == IntPtr.Zero)
        return (m_initial_value is int ? (int)m_initial_value : 0);
      // Call the C++ RDK and get the Variant value as an integer
      int result = UnsafeNativeMethods.Rdk_ContentField_IntValue(field_pointer);
      return result;
    }
    /// <summary>
    /// Return field value as a double precision number.
    /// </summary>
    /// <returns>Return the field value as a double precision number.</returns>
    protected double ValueAsDouble()
    {
      IntPtr field_pointer = FieldPointer;
      // Field is not initialized so return the default value
      if (field_pointer == IntPtr.Zero)
        return (m_initial_value is double ? (double)m_initial_value : 0.0);
      // Call the C++ RDK and get the Variant value as a double
      double result = UnsafeNativeMethods.Rdk_ContentField_DoubleValue(field_pointer);
      return result;
    }
    /// <summary>
    /// Return field value as floating point number.
    /// </summary>
    /// <returns>Return the field value as an floating point number.</returns>
    protected float ValueAsFloat()
    {
      IntPtr field_pointer = FieldPointer;
      // Field is not initialized so return the default value
      if (field_pointer == IntPtr.Zero)
        return (m_initial_value is float ? (float)m_initial_value : 0f);
      // Call the C++ RDK and get the Variant value as a float
      float result = UnsafeNativeMethods.Rdk_ContentField_FloatValue(field_pointer);
      return result;
    }
    /// <summary>
    /// Return field as a Rhino.Display.Color4f color value.
    /// </summary>
    /// <returns>Return field as a Rhino.Display.Color4f color value.</returns>
    protected Color4f ValueAsColor4f()
    {
      IntPtr field_pointer = FieldPointer;
      // Field is not initialized so return the default value
      if (field_pointer == IntPtr.Zero)
        return (m_initial_value is Display.Color4f ? (Display.Color4f)m_initial_value : Display.Color4f.Empty);
      Color4f result = Color4f.Empty;
      // Call the C++ RDK and get the Variant value as a Color4f value
      UnsafeNativeMethods.Rdk_ContentField_ColorValue(field_pointer, ref result);
      return result;
    }
    /// <summary>
    /// Return field as a Rhino.Geometry.Vector2d color value.
    /// </summary>
    /// <returns>Return field as a Rhino.Geometry.Vector2d color value.</returns>
    protected Vector2d ValueAsVector2d()
    {
      IntPtr field_pointer = FieldPointer;
      // Field is not initialized so return the default value
      if (field_pointer == IntPtr.Zero)
        return (m_initial_value is Vector2d ? (Vector2d)m_initial_value : Vector2d.Unset);
      Vector2d result = Vector2d.Unset;
      // Call the C++ RDK and get the Variant value as a Vector2d value
      UnsafeNativeMethods.Rdk_ContentField_Vector2dValue(field_pointer, ref result);
      return result;
    }
    /// <summary>
    /// Return field as a Rhino.Geometry.Vector3d color value.
    /// </summary>
    /// <returns>Return field as a Rhino.Geometry.Vector3d color value.</returns>
    protected Vector3d ValueAsVector3d()
    {
      IntPtr field_pointer = FieldPointer;
      // Field is not initialized so return the default value
      if (field_pointer == IntPtr.Zero)
        return (m_initial_value is Vector3d ? (Vector3d)m_initial_value : Vector3d.Unset);
      Vector3d result = Vector3d.Unset;
      // Call the C++ RDK and get the Variant value as a Vector3d value
      UnsafeNativeMethods.Rdk_ContentField_Vector3dValue(field_pointer, ref result);
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
      IntPtr field_pointer = FieldPointer;
      // Field is not initialized so return the default value
      if (field_pointer == IntPtr.Zero)
        return (m_initial_value is Point4d ? (Point4d)m_initial_value : Point4d.Unset);
      Point4d result = Point4d.Unset;
      // Call the C++ RDK and get the Variant value as a Point4d
      UnsafeNativeMethods.Rdk_ContentField_Point4dValue(field_pointer, ref result);
      return result;
    }
    /// <summary>
    /// Return field value as Guid.
    /// </summary>
    /// <returns>Return the field value as an Guid.</returns>
    protected Guid ValueAsGuid()
    {
      IntPtr field_pointer = FieldPointer;
      // Field is not initialized so return the default value
      if (field_pointer == IntPtr.Zero)
        return (m_initial_value is Guid ? (Guid)m_initial_value : Guid.Empty);
      // Call the C++ RDK and get the Variant value as a Guid
      Guid result = UnsafeNativeMethods.Rdk_ContentField_UUIDValue(field_pointer);
      return result;
    }
    /// <summary>
    /// Return field as a Rhino.Geometry.Transform color value.
    /// </summary>
    /// <returns>Return field as a Rhino.Geometry.Transform color value.</returns>
    protected Transform ValueAsTransform()
    {
      IntPtr field_pointer = FieldPointer;
      // Field is not initialized so return the default value
      if (field_pointer == IntPtr.Zero)
        return (m_initial_value is Transform ? (Transform)m_initial_value : Transform.Unset);
      // Call the C++ RDK and get the Variant value as a Transform value
      Transform result = Transform.Unset;
      UnsafeNativeMethods.Rdk_ContentField_XformValue(field_pointer, ref result);
      return result;
    }
    /// <summary>
    /// Return field as a DateTime value.
    /// </summary>
    /// <returns>Return field as a DateTime value.</returns>
    protected DateTime ValueAsDateTime()
    {
      IntPtr field_pointer = FieldPointer;
      // Field is not initialized so return the default value
      if (field_pointer == IntPtr.Zero)
        return (m_initial_value is DateTime ? (DateTime)m_initial_value : DateTime.Now);
      // Call the C++ RDK and get the Variant value as a DateTime value
      int month = 0, year = 0, day = 0, hours = 0, minutes = 0, seconds = 0;
      UnsafeNativeMethods.Rdk_ContentField_TimeValue(field_pointer, ref year, ref month, ref day, ref hours, ref minutes, ref seconds);
      DateTime result = new DateTime(year, month, day, hours, minutes, seconds);
      return result;
    }
    /// <summary>
    /// Return field as a byte array.
    /// </summary>
    /// <returns>Return field as a byte array.</returns>
    protected byte[] ValueAsByteArray()
    {
      IntPtr field_pointer = FieldPointer;
      // Field is not initialized so return the default value
      if (IntPtr.Zero == field_pointer)
        return (m_initial_value is byte[] ? (byte[])m_initial_value : null);
      // Call the C++ RDK and get the size of the Variant buffer
      int size_of_result = UnsafeNativeMethods.Rdk_ContentField_GetByteArrayValueSize(field_pointer);
      // Allocate a buffer to receive a copy of the Variant data
      byte[] result = new byte[size_of_result];
      // Copy the C++ buffer into the result byte array
      UnsafeNativeMethods.Rdk_ContentField_GetByteArrayValue(field_pointer, result, size_of_result);
      return result;
    }
    #endregion Methods to access value as specific data types

    /// <summary>
    /// Field constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this field.</param>
    /// <param name="attachToPointer">C++ pointer to attach to.</param>
    /// <param name="key">Unique key for this field</param>
    /// <param name="prompt">Display string used by the user interface</param>
    /// <param name="initialValue">
    /// Initial value used to initialize the field after creating the C++
    /// pointer.
    /// </param>
    /// <param name="isTextured">Determines whether auto-UIs will show the texture control set</param>
    protected Field(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, object initialValue, bool isTextured)
    {
      // Value which will be used to set the initial state of the C++ field
      // variant when calling CreateCppPointer()
      m_initial_value = initialValue;
      // Field key
      m_key = key;
      // User interface display prompt
      m_prompt = prompt;
      // Textured flag for auto-UI
      m_is_textured = isTextured;
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
      if (IntPtr.Zero != m_field_pointer) return;
      if (IntPtr.Zero == attachToPointer)
      {
        // Get the RenderContent C++ pointer, fields get added to content field lists
        // so you have to have a valid Content C++ pointer when creating a field.
        IntPtr content_pointer = content.ConstPointer();
        // If there is a user interface prompt string then set the add to user interface flag.
        bool is_visible_to_auto_ui = !string.IsNullOrEmpty(Prompt);
        // Allocate the objects C++ pointer.
        m_field_pointer = UnsafeNativeMethods.Rdk_ContentField_New(content_pointer, Key, Prompt, is_visible_to_auto_ui ? 0 : 0x8001);

        if (IsTextured)
        {
          UnsafeNativeMethods.Rdk_ContentField_SetIsTextured(m_field_pointer, 1);
        }

        // Initialize the field value (RenderContent.ChangeContexts.Ignore is
        // used to avoid a ContentModified event from getting raised)
        Set(m_initial_value, RenderContent.ChangeContexts.Ignore);
        // If m_initialValue can be disposed of then dispose of it now
        if (m_initial_value is IDisposable) (m_initial_value as IDisposable).Dispose();
        m_initial_value = null;
      }
      else
      {
        m_auto_delete = false;
        m_field_pointer = attachToPointer;
      }
    }
    /// <summary>
    /// IDisposable required method
    /// </summary>
    internal void IneternalDispose()
    {
      if (IntPtr.Zero != m_field_pointer && m_auto_delete)
      {
        UnsafeNativeMethods.Rdk_ContentField_Delete(m_field_pointer);
        m_field_pointer = IntPtr.Zero;
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
      if (IntPtr.Zero == fieldPointer) throw new ArgumentNullException("fieldPointer");
      // Get the field user interface prompt string
      string prompt;
      using (var sh = new StringHolder())
      {
        IntPtr string_pointer = sh.NonConstPointer();
        UnsafeNativeMethods.Rdk_ContentField_FriendlyName(fieldPointer, string_pointer);
        prompt = sh.ToString();
      }
      // Create the field from the values Variant type
      Field result;
      IntPtr variant_pointer = UnsafeNativeMethods.Rdk_ContentField_Value(fieldPointer);
      bool is_textured = 0 != UnsafeNativeMethods.Rdk_ContentField_IsTextured(fieldPointer);

      switch (UnsafeNativeMethods.Rdk_Variant_Type(variant_pointer))
      {
        case (int)Variant.VariantTypes.Bool:
          result = new BoolField(renderContent, fieldPointer, key, prompt, false, is_textured);
          break;
        case (int)Variant.VariantTypes.Color:
          result = new Color4fField(renderContent, fieldPointer, key, prompt, Color4f.Empty, is_textured);
          break;
        case (int)Variant.VariantTypes.Double:
          result = new DoubleField(renderContent, fieldPointer, key, prompt, 0.0, is_textured);
          break;
        case (int)Variant.VariantTypes.Float:
          result = new FloatField(renderContent, fieldPointer, key, prompt, 0f, is_textured);
          break;
        case (int)Variant.VariantTypes.Integer:
          result = new IntField(renderContent, fieldPointer, key, prompt, 0, is_textured);
          break;
        case (int)Variant.VariantTypes.Matrix:
          result = new TransformField(renderContent, fieldPointer, key, prompt, Transform.Unset, is_textured);
          break;
        case (int)Variant.VariantTypes.Point4d:
          result = new Point4dField(renderContent, fieldPointer, key, prompt, Point4d.Unset, is_textured);
          break;
        case (int)Variant.VariantTypes.String:
          result = new StringField(renderContent, fieldPointer, key, prompt, string.Empty, is_textured);
          break;
        case (int)Variant.VariantTypes.Time:
          result = new DateTimeField(renderContent, fieldPointer, key, prompt, DateTime.Now, is_textured);
          break;
        case (int)Variant.VariantTypes.Uuid:
          result = new GuidField(renderContent, fieldPointer, key, prompt, Guid.Empty, is_textured);
          break;
        case (int)Variant.VariantTypes.Vector2d:
          result = new Vector2dField(renderContent, fieldPointer, key, prompt, Vector2d.Unset, is_textured);
          break;
        case (int)Variant.VariantTypes.Vector3d:
          result = new Vector3dField(renderContent, fieldPointer, key, prompt, Vector3d.Unset, is_textured);
          break;
        //case (int)Variant.VariantTypes.Pointer:
        default:
          result = new ByteArrayField(renderContent, fieldPointer, key, prompt, null, is_textured);
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
    /// <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal StringField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, string value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public string Value
    {
      get { return ValueAsString(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
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
    /// <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal BoolField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, bool value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public bool Value
    {
      get { return ValueAsBool(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
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
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal IntField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, int value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public int Value
    {
      get { return ValueAsInt(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
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
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal FloatField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, float value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public float Value
    {
      get { return ValueAsFloat(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
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
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal DoubleField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, double value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public double Value
    {
      get { return ValueAsDouble(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
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
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal Color4fField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Color4f value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="renderContent">RenderContent whose Fields owns this filed.</param>
    /// <param name="attachToPointer">Existing C++ pointer to attach to.</param>
    /// <param name="key">Field key name</param>
    /// <param name="prompt">Field user interface prompt string</param>
    /// <param name="value">Initial value for this string field</param>
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal Color4fField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Rhino.Drawing.Color value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Color4f Value
    {
      get { return ValueAsColor4f(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Rhino.Drawing.Color SystemColorValue
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
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal Vector2dField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Vector2d value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Vector2d Value
    {
      get { return ValueAsVector2d(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
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
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal Vector3dField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Vector3d value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Vector3d Value
    {
      get { return ValueAsVector3d(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
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
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal Point2dField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Point2d value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Point2d Value
    {
      get { return ValueAsPoint2d(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
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
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal Point3dField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Point3d value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Point3d Value
    {
      get { return ValueAsPoint3d(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
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
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal Point4dField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Point4d value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Point4d Value
    {
      get { return ValueAsPoint4d(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
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
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal GuidField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Guid value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Guid Value
    {
      get { return ValueAsGuid(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
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
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal TransformField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, Transform value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public Transform Value
    {
      get { return ValueAsTransform(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
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
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal DateTimeField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, DateTime value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public DateTime Value
    {
      get { return ValueAsDateTime(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
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
    ///  <param name="isTextured">Determines whether the texture control set is added to auto gen UIs for this field</param>
    internal ByteArrayField(RenderContent renderContent, IntPtr attachToPointer, string key, string prompt, byte[] value, bool isTextured) : base(renderContent, attachToPointer, key, prompt, value, isTextured) { }
    /// <summary>
    /// Gets or sets the field value
    /// </summary>
    public byte[] Value
    {
      get { return ValueAsByteArray(); }
      set { Set(value); }
    }
    public override object ValueAsObject() { return Value; }
  }
}

#endif
