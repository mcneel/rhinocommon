RhinoCommon SDK for Rhino
=========================

---

_:warning: This is a mirror of the RhinoCommon source from Rhino 5._

_**An up-to-date version of the source code is available in the [rhino3dm](https://github.com/mcneel/rhino3dm) repository.**_

---

![Rhino](https://lh6.googleusercontent.com/-pQtuyrwmcmg/TYtWECHGYNI/AAAAAAAAA7Y/rphjSmq1cuo/s200/Rhino_logo_wire.jpg)

Get Involved (contributing)
---------------------------
There are many ways to contribute to this project:

* Report bugs/wishes to the issue list at https://github.com/mcneel/rhinocommon/issues
* Directly edit the source code using git. If you need help with this, please let us know.

Compiling the Source
--------------------
You are going to need:

* The latest Rhino5 (http://www.rhino3d.com/nr.htm)
* Visual C# 2010

Steps:

* Get the source code by downloading everything as a zip or using git

I'm going to need to come up with better instructions on compiling and using the dll, but in a nutshell you should be able to debug into the RhinoCommon source code by:

* rename the shipping RhinoCommon.dll to something like RhinoCommon.dll.original
* place the RhinoCommon.dll and pdb that gets compiled by this project in the Rhino5 system directory

Installation / Configuration
----------------------------
RhinoCommon is written to work under different "modes".
- Assembly running in Rhino: Rhino 5 (and Grasshopper in Rhino 4) ship a precompiled version of RhinoCommon that contains full access to the Rhino SDK. This means highre level functionality of things like intersections or working with the RhinoDoc are supported
- Stand alone assembly accessing OpenNURBS: A special build flavor of RhinoCommon is supported that let's you build RhinoCommon as a .NET layer on top of the C++ OpenNURBS toolkit (www.opennurbs.org)

Authors
-------
Really the entire McNeel programming staff (http://www.mcneel.com)

Specific people to contact about this project:

* Steve Baer - https://github.com/sbaer steve@mcneel.com
* David Rutten - https://github.com/DavidRutten
* Giulio Piacentino - https://github.com/piac

Legal Stuff
-----------
Copyright (c) 2015 Robert McNeel & Associates. All Rights Reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
Software.

THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT EXPRESS OR IMPLIED WARRANTY. ALL IMPLIED
WARRANTIES OF FITNESS FOR ANY PARTICULAR PURPOSE AND OF MERCHANTABILITY ARE HEREBY
DISCLAIMED.

Rhinoceros is a registered trademark of Robert McNeel & Associates.
