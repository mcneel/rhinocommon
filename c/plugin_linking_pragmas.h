#pragma once

#if defined(OPENNURBS_BUILD)

#if !defined(ON_MSC_SOLUTION_DIR)
// ON_MSC_SOLUTION_DIR must have a trailing slash
#define ON_MSC_SOLUTION_DIR "./"
#endif

#if !defined(ON_MSC_LIB_DIR)

#if defined(WIN64)

// x64 (64 bit) static libraries

#if defined(NDEBUG)

// Release x64 (64 bit) libs
#define ON_MSC_LIB_DIR "x64/Release/"
#pragma message( " --- statically linking opennurbs." )
#pragma comment(lib, "opennurbs/zlib/x64/release/zlibx64.lib")
#pragma comment(lib, "opennurbs/x64/ReleaseStaticLib/opennurbsx64_static.lib")

#else // _DEBUG

// Debug x64 (64 bit) libs
#define ON_MSC_LIB_DIR "x64/Debug/"
#pragma message( " --- statically linking opennurbs." )
#pragma comment(lib, "opennurbs/zlib/x64/debug/zlibx64_d.lib")
#pragma comment(lib, "opennurbs/x64/DebugStaticLib/opennurbs_staticlibx64_staticd.lib")

#endif // NDEBUG else _DEBUG

#else // WIN32

#if defined(NDEBUG)

// Release x86 (32 bit) libs
#define ON_MSC_LIB_DIR "Release/"
#pragma message( " --- statically linking opennurbs." )
#pragma comment(lib, "opennurbs/zlib/release/zlib.lib")
#pragma comment(lib, "opennurbs/ReleaseStaticLib/opennurbs_static.lib")

#else // _DEBUG

// Debug x86 (32 bit) libs
#define ON_MSC_LIB_DIR "Debug/"
#pragma message( " --- statically linking opennurbs." )
#pragma comment(lib, "opennurbs/zlib/debug/zlib_d.lib")
#pragma comment(lib, "opennurbs/DebugStaticLib/opennurbs_staticlib_staticd.lib")

#endif // NDEBUG else _DEBUG

#endif // WIN64 else WIN32

#endif //  !defined(ON_MSC_LIB_DIR)

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

#pragma comment(lib, "OPENGL32.LIB")
#pragma comment(lib, "GLU32.LIB")

#endif

