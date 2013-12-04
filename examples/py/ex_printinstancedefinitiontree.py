from scriptcontext import doc
import Rhino

def RunCommand():
  idefs = doc.InstanceDefinitions
  idefCount = idefs.Count

  if idefCount == 0:
    print "Document contains no instance definitions."
    return

  dump = Rhino.FileIO.TextLog()
  dump.IndentSize = 4

  for i in range(0, idefCount):
    DumpInstanceDefinition(idefs[i], dump, True)

  print dump.ToString()

def DumpInstanceDefinition(idef, dump, bRoot):
  if idef != None and not idef.IsDeleted:
    if bRoot:
      node = u'\u2500'
    else:
      node = u'\u2514'
    dump.Print(u"{0} Instance definition {1} = {2}\n".format(node, idef.Index, idef.Name))

    idefObjectCount = idef.ObjectCount
    if idefObjectCount  > 0:
      dump.PushIndent()
      for i in range(0, idefObjectCount):
        obj = idef.Object(i)
        if obj != None and type(obj) == Rhino.DocObjects.InstanceObject:
          DumpInstanceDefinition(obj.InstanceDefinition, dump, False) # Recursive...
        else:
          dump.Print(u"\u2514 Object {0} = {1}\n".format(i, obj.ShortDescription(False)))
      dump.PopIndent()

if __name__ == "__main__":
  RunCommand()
