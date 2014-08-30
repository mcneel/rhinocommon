example\_read\_mac
=========================
This example program uses the Rhino file IO toolkit.  The program reads in a Rhino 3dm model file and writes a description to a text file.  The program is a terminal application that runs within mono and takes a filename as a command line argument.

Compiling the Source
--------------------
You are going to need:

* Xamarin Studio 4.2.5 or better
* Xcode 5.1.1 or better (along with xcode command line tools)

Steps to build
----------------------------
* Open Rhino3dmIo.sln in Xamarin Studio for Mac OS X.  You may get a number of warnings that some of the projects will not open correctly...ignore these.
* Right-click the example\_read\_mac project and select "Set as Startup Project."  Select the configuration Release or Debug.
* Build the project.  This will open a terminal window and run the target exe in mono.

Testing/Running
----------------------------
To run from terminal, use:

    mono example_read_mac.exe -out:list.txt AirCleaner.3dm

Special Notes
----------------------------
Rhino3dmIo on Mac OS X depends on two libraries:

- Rhino3dmIO.dll (the C# .NET wrapper around opennurbs)
- libopennurbs.dylib (the C/C++ native opennurbs library)

example\_read\_mac references Rhino3dmIO.  For the native libopennurbs.dylib, there is BeforeBuild command that builds the native libopennurbs.dylib if it is not already built (consequently, building this project for the first time takes longer than subsequent builds).  There is an After Build command that copies the libopennurbs.dylib to the target folder and renames it to rhino3dmio_native (note the lack of file extension).  According to [this documentation](http://www.mono-project.com/docs/advanced/pinvoke/), the rename step should be unnecessary, but it only seems to work if the name of the library is literally the same name as that found in rhinocommon/c/Import.cs.  We hope to fix this problem if possible.


Authors
-------
Really the entire McNeel programming staff (http://www.mcneel.com)

Specific people to contact about this project:

* Dan Belcher - https://github.com/dbelcher dan@mcneel.com
* Steve Baer - https://github.com/sbaer steve@mcneel.com
* Alain Cormier - https://github.com/acormier alain@mcneel.com

Legal Stuff
-----------
Copyright (c) 2014 Robert McNeel & Associates. All Rights Reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
Software.

THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT EXPRESS OR IMPLIED WARRANTY. ALL IMPLIED
WARRANTIES OF FITNESS FOR ANY PARTICULAR PURPOSE AND OF MERCHANTABILITY ARE HEREBY
DISCLAIMED.

Rhinoceros is a registered trademark of Robert McNeel & Associates.
