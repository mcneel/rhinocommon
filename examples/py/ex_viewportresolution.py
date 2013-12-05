from scriptcontext import doc

avp = doc.Views.ActiveView.ActiveViewport
print "Name = {0}: Width = {1}, Height = {2}".format(
    avp.Name, avp.Size.Width, avp.Size.Height)
