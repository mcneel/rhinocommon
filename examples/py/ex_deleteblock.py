import rhinoscriptsyntax as rs
from scriptcontext import doc

def Delete():
    bn = rs.GetString("block to delete")
    idef = doc.InstanceDefinitions.Find(bn, True)
    if not idef:
        print "{0} block does not exist".format(bn)
        return
        
    rs.DeleteBlock(bn)
    
if __name__ == "__main__":
    Delete()