using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace localize_3dm
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length < 1)
      {
        var exe_name = System.AppDomain.CurrentDomain.FriendlyName;
        Console.WriteLine("Syntax: {0} -action:[extract|localize] -xml:XML -3dm:MODEL [-target3dm:Output]", exe_name);
        return;
      }

      string action = "", model = "", xml = "", target3dm = "";

      foreach (string arg in args)
      {
        if (arg.StartsWith("-action:", StringComparison.InvariantCultureIgnoreCase))
          action = arg.Substring("-action:".Length);
        else if (arg.StartsWith("-3dm:", StringComparison.InvariantCultureIgnoreCase))
          model = arg.Substring("-3dm:".Length);
        else if (arg.StartsWith("-xml:", StringComparison.InvariantCultureIgnoreCase))
          xml = arg.Substring("-xml:".Length);
        else if (arg.StartsWith("-target3dm:", StringComparison.InvariantCultureIgnoreCase))
          target3dm = arg.Substring("-target3dm:".Length);
      }

      switch (action)
      {
        case "extract":
          Extract(xml, model);
          return;
        case "localize":
          Localize(xml, model, target3dm);
          return;
        default:
          return;
      }
      
      System.IO.TextWriter alternate_output_stream = null;
      for (int i = 0; i < args.Length; i++)
      {
        string arg = args[i];
        // check for -out or /out option
        if (arg.StartsWith("-out:", StringComparison.InvariantCultureIgnoreCase) || arg.StartsWith("/out:", StringComparison.InvariantCultureIgnoreCase))
        {
          // change destination of dump file
          string dump_file_name = arg.Substring(5).Trim();
          alternate_output_stream = new System.IO.StreamWriter(dump_file_name);
          Console.SetOut(alternate_output_stream);
          continue;
        }

        string filename = arg;
        Console.Out.WriteLine("OpenNURBS Archive File:  {0}", filename);
        using (var file3dm = Rhino.FileIO.File3dm.Read(filename))
        {
          if (file3dm == null)
          {
            Console.Out.WriteLine("Unable to open file");
            continue;
          }

          Console.Out.WriteLine("Layers:");
          foreach (var layer in file3dm.Layers)
          {
            Console.Out.WriteLine("  {0} ({1}) color: {2}", layer.FullPath, layer.Name, layer.Color);
          }

          Console.Out.WriteLine("Objects:");
          foreach (var obj in file3dm.Objects)
          {
            Console.Out.WriteLine("  {0} ({1}) {3}->{2}", obj.Attributes.ObjectId, obj.Attributes.Name, obj.Attributes.ObjectColor, obj.Attributes.ColorSource);
          }

          if (null != file3dm.Views)
          {
            Console.Out.WriteLine("Views:");
            foreach (var view in file3dm.Views)
            {
              Console.Out.WriteLine("  {0}", view.Name);
            }
          }

          if (null != file3dm.NamedViews)
          {
            Console.Out.WriteLine("Named Views:");
            foreach (var view in file3dm.NamedViews)
            {
              Console.Out.WriteLine("  {0}", view.Name);
            }
          }

          Console.Out.WriteLine("Notes:");
          Console.Out.WriteLine(file3dm.Notes.Notes);

        }
      }

      if (alternate_output_stream != null)
      {
        // close the text dump file
        alternate_output_stream.Close();
      }
    }

    static void Localize(string xml, string sourceModel, string targetModel)
    {
      XmlDocument doc = new XmlDocument();
      doc.Load(xml);

      using (var file3dm = Rhino.FileIO.File3dm.Read(sourceModel))
      {
        foreach (var layer in file3dm.Layers)
        {
          var path = string.Format("//RhinoModel/Layers/Layer/Name[@English='{0}']", layer.Name);
          var nameNode = (XmlElement)doc.SelectSingleNode(path);
          if (nameNode != null)
            layer.Name = nameNode.GetAttribute("Localized");
        }

        file3dm.Polish();
        file3dm.Write(targetModel, 5);
      }

      
    }

    static void Extract(string xml, string model)
    {
      XmlDocument doc = new XmlDocument();
      doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
      XmlElement body = (XmlElement)doc.AppendChild(doc.CreateElement("RhinoModel"));

      using (var file3dm = Rhino.FileIO.File3dm.Read(model))
      {
        if (file3dm == null)
        {
          Console.Out.WriteLine("Unable to open file");
          return;
        }

        XmlElement layers = (XmlElement)body.AppendChild(doc.CreateElement("Layers"));
        foreach (var layer in file3dm.Layers)
        {
          XmlElement layerElem = (XmlElement)layers.AppendChild(doc.CreateElement("Layer"));
          XmlElement layerNameElem = (XmlElement)layerElem.AppendChild(doc.CreateElement("Name"));
          layerNameElem.SetAttribute("English", layer.Name);
          layerNameElem.SetAttribute("Localized", layer.Name);
          XmlElement layerPathElem = (XmlElement)layerElem.AppendChild(doc.CreateElement("FullPath"));
          layerPathElem.SetAttribute("English", layer.FullPath);
          layerPathElem.SetAttribute("Localized", layer.FullPath);
        }

        XmlNode objects = (XmlElement)body.AppendChild(doc.CreateElement("Objects"));
        foreach (var obj in file3dm.Objects)
        {
          if (string.IsNullOrEmpty(obj.Attributes.Name))
            continue;
          XmlElement objNode = (XmlElement)objects.AppendChild(doc.CreateElement("Object"));
          objNode.SetAttribute("ID", obj.Attributes.ObjectId.ToString());
          XmlElement nameNode = (XmlElement)objNode.AppendChild(doc.CreateElement("Name"));
          nameNode.SetAttribute("English", obj.Attributes.Name);
          nameNode.SetAttribute("Localized", obj.Attributes.Name);
        }

        if (null != file3dm.Views)
        {
          XmlNode views = (XmlElement)body.AppendChild(doc.CreateElement("Views"));
          foreach (var view in file3dm.Views)
          {
            XmlElement viewNode = (XmlElement)views.AppendChild(doc.CreateElement("View"));
            XmlElement nameNode = (XmlElement)viewNode.AppendChild(doc.CreateElement("Name"));
            nameNode.SetAttribute("English", view.Name);
            nameNode.SetAttribute("Localized", view.Name);
          }
        }

        //if (null != file3dm.NamedViews)
        //{
        //  XmlNode namedViews = (XmlElement)body.AppendChild(doc.CreateElement("NamedViews"));
        //  foreach (var view in file3dm.NamedViews)
        //  {
        //    XmlElement viewNode = (XmlElement)namedViews.AppendChild(doc.CreateElement("NamedView"));
        //    XmlElement nameNode = (XmlElement)viewNode.AppendChild(doc.CreateElement("Name"));
        //    nameNode.SetAttribute("English", view.Name);
        //    nameNode.SetAttribute("Localized", view.Name);
        //  }
        //}

        XmlNode notes = (XmlElement)body.AppendChild(doc.CreateElement("Notes"));
        notes.InnerText = file3dm.Notes.Notes;

        doc.Save(xml);
      }      
    }
  }

}