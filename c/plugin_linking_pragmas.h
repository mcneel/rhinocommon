#pragma once

#if defined(OPENNURBS_BUILD)

#if defined(WIN64) && defined(_M_X64)

// 64 bit Windows zlib linking instructions

#if defined(NDEBUG)

// release x64 libs
#pragma comment(lib, "../../opennurbs/zlib/x64/Release/zlib.lib")
#pragma comment(lib, "./x64/Release/opennurbs_staticlib.lib")

#else // _DEBUG

// debug  x64 libs
#pragma comment(lib, "../../opennurbs/zlib/x64/Debug/zlib.lib")
#pragma comment(lib, "./x64/Debug/opennurbs_staticlib.lib")

#endif // if NDEBUG else _DEBUG

#elif defined(WIN32) && defined(_M_IX86)

// 32 bit Windows zlib linking instructions

#if defined(NDEBUG)

// release 32 bit WIndows libs
#pragma comment(lib, "../../opennurbs/zlib/Release/zlib.lib")
#pragma comment(lib, "./Release/opennurbs_staticlib.lib")

#else // _DEBUG

// debug 32 bit WIndows libs
#pragma comment(lib, "../../opennurbs/zlib/Debug/zlib.lib")
#pragma comment(lib, "./Debug/opennurbs_staticlib.lib")

#endif // if NDEBUG else _DEBUG

#endif // if WIN64 else WIN32


#else

#if defined(RHINO_SYSTEM_PLUGIN_LINKING_PRAGMAS)
#error You goofed up
#endif
#define RHINO_SYSTEM_PLUGIN_LINKING_PRAGMAS

#if defined(GRASSHOPPER_V4)
#pragma comment(lib, "../../../rhinosdk/rhino4/Release/rhino4.lib")
#pragma comment(lib, "../../../rhinosdk/openNURBS/Release/opennurbs.lib")
#pragma comment(lib, "../../../rhinosdk/tl/Release/tl.lib")

#else

#include "../../../rhino_core_plugin_linking_pragmas4.h"

#endif // GRASSHOPPER_V4 else V5 BUILD

#endif

#pragma comment(lib, "OPENGL32.LIB")
#pragma comment(lib, "GLU32.LIB")
