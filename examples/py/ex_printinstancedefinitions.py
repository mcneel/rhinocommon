from scriptcontext import doc

def RunCommand():
  idefNames = [idef.Name for idef in doc.InstanceDefinitions if idef != None and not idef.IsDeleted]

  for n in idefNames:
    print "instance definition = {0}".format(n)

if __name__ == "__main__":
  RunCommand()
