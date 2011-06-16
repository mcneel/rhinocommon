#pragma once

#if defined(RHINO_SYSTEM_PLUGIN_LINKING_PRAGMAS)
#error You goofed up.  See comment above.
#endif
#define RHINO_SYSTEM_PLUGIN_LINKING_PRAGMAS

#if defined(GRASSHOPPER_V4)
#pragma comment(lib, "../../../rhinosdk/rhino4/Release/rhino4.lib")
#pragma comment(lib, "../../../rhinosdk/openNURBS/Release/opennurbs.lib")
#pragma comment(lib, "../../../rhinosdk/tl/Release/tl.lib")

#else

#include "../../../rhino_plugin_linking_pragmas4.h"

#endif // GRASSHOPPER_V4 else V5 BUILD
