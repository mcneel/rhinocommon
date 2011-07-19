namespace Rhino.UI
{
  public abstract class OptionsDialogPage : StackedDialogPage
  {
    protected OptionsDialogPage(string englishPageTitle)
      :base(englishPageTitle)
    {

    }

    public virtual Rhino.Commands.Result RunScript(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      RhinoApp.WriteLine(LOC.STR("Scripting not supported for this option"));
      Rhino.UI.Dialogs.ShowMessageBox(LOC.STR("Scripting not supported for this option"), LOC.STR("Unsupported Option"));
      return Rhino.Commands.Result.Success;
    }
  }
}
