import rhinoscriptsyntax as rs
from scriptcontext import *
import Rhino
import System.Collections.Generic as scg
import System as s

def RunCommand():
  srfid = rs.GetObject("select surface", rs.filter.surface | rs.filter.polysurface)
  if not srfid: return
 
  crvid = rs.GetObject("select curve", rs.filter.curve)
  if not crvid: return

  evs = rs.CurveSurfaceIntersection(crvid, srfid)

  if evs:
      addedObjs = []
      for ev in evs:
          if ev[0] == 2: #overlap
              crv = rs.coercecurve(crvid)
              if crv:
                  t0 = crv.ClosestPoint(ev[1])[1]
                  t1 = crv.ClosestPoint(ev[2])[1]
                  overlapCrv = crv.DuplicateCurve().Trim(t0,t1)
                  addedObjs.Add(doc.Objects.AddCurve(overlapCrv))
          else: #point
              addedObjs.Add(doc.Objects.AddPoint(ev[1]))

      if len(addedObjs) > 0:
          doc.Objects.Select.Overloads[scg.IEnumerable[s.Guid]](addedObjs)
  doc.Views.Redraw()

if __name__ == "__main__":
  RunCommand()
