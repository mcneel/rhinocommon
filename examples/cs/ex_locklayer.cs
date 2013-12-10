using Rhino;
using System;
using System.Linq;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("A77507C3-DEEE-4A2C-ADB3-3FFAF89B7EDD")]
  public class ex_locklayer : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csLockLayer"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      string layerName = "";
      var rc = Rhino.Input.RhinoGet.GetString("Name of layer to lock", true, ref layerName);
      if (rc != Rhino.Commands.Result.Success)
        return rc;
      if (String.IsNullOrWhiteSpace(layerName))
        return Rhino.Commands.Result.Nothing;
     
      // because of sublayers it's possible that mone than one layer has the same name
      // so simply calling doc.Layers.Find(layerName) isn't good enough.  If "layerName" returns
      // more than one layer then present them to the user and let him decide.
      var matchingLayers = (from layer in doc.Layers
                            where layer.Name == layerName
                            select layer).ToList<Rhino.DocObjects.Layer>();

      Rhino.DocObjects.Layer layerToRename = null;
      if (matchingLayers.Count == 0)
      {
        RhinoApp.WriteLine(String.Format("Layer \"{0}\" does not exist.", layerName));
        return Rhino.Commands.Result.Nothing;
      }
      else if (matchingLayers.Count == 1)
      {
        layerToRename = matchingLayers[0];
      }
      else if (matchingLayers.Count > 1)
      {
        for (int i = 0; i < matchingLayers.Count; i++)
        {
          RhinoApp.WriteLine(String.Format("({0}) {1}", i+1, matchingLayers[i].FullPath.Replace("::", "->")));
        }
        int selectedLayer = -1;
        rc = Rhino.Input.RhinoGet.GetInteger("which layer?", true, ref selectedLayer);
        if (rc != Rhino.Commands.Result.Success)
          return rc;
        if (selectedLayer > 0 && selectedLayer <= matchingLayers.Count)
          layerToRename = matchingLayers[selectedLayer - 1];
        else return Rhino.Commands.Result.Nothing;
      }

      if (!layerToRename.IsLocked)
      {
        layerToRename.IsLocked = true;
        layerToRename.CommitChanges();
        return Rhino.Commands.Result.Success;
      }
      else
      {
        RhinoApp.WriteLine(String.Format("layer {0} is already locked.", layerToRename.FullPath));
        return Rhino.Commands.Result.Nothing;
      } 
    }
  }
}