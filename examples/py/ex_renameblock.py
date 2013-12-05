import rhinoscriptsyntax as rs
from scriptcontext import doc

def Rename():
    bn = rs.GetString("block to rename")
    idef = doc.InstanceDefinitions.Find(bn, True)
    if not idef: 
        print "{0} block does not exist".format(bn)
        return
    
    nn = rs.GetString("new name")
    idef = doc.InstanceDefinitions.Find(nn, True)
    if idef: 
        print "the name '{0}' is already taken by another block".format(nn)
        return

    rs.RenameBlock(bn, nn)
    
if __name__ == "__main__":
    Rename()