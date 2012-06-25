/////////////////////////////////////////////////////////////////////////////
// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently

#pragma once

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN		      // Exclude rarely-used stuff from Windows headers
#endif

// Rhino SDK Preamble
#if defined(GRASSHOPPER_V4)
////////////////////////////////////////////////////////////////////////////
// BEGIN VOODOO CODE
// Serious voodoo code that Dale Fugier recommend be added to force the V4
// Grasshopper build of rhcommon_c to use the exact same CRT/MFC library
// versions that Rhino4 SR9 is using
#ifndef __midl
#define _SXS_ASSEMBLY_VERSION "8.0.50727.762"
#define _CRT_ASSEMBLY_VERSION _SXS_ASSEMBLY_VERSION
#define _MFC_ASSEMBLY_VERSION _SXS_ASSEMBLY_VERSION
#define _ATL_ASSEMBLY_VERSION _SXS_ASSEMBLY_VERSION

#ifdef __cplusplus
extern "C" {
#endif
__declspec(selectany) int _forceCRTManifest;
__declspec(selectany) int _forceMFCManifest;
__declspec(selectany) int _forceAtlDllManifest;
__declspec(selectany) int _forceCRTManifestRTM;
__declspec(selectany) int _forceMFCManifestRTM;
__declspec(selectany) int _forceAtlDllManifestRTM;
#ifdef __cplusplus
}
#endif
#endif
// END VOODOO CODE
////////////////////////////////////////////////////////////////////////////




#include "../../../rhinosdk/rhino4/sdk/inc/rhinoSdkStdafxPreamble.h"
#elif defined(OPENNURBS_BUILD)
#include "./opennurbs/opennurbs.h"
#include "./opennurbs/zlib/zlib.h"
#else
#include "../../../rhino4/SDK/Inc/RhinoSdkStdafxPreamble.h"
#endif

#if defined(_WIN32) && !defined(OPENNURBS_BUILD)

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// some CString constructors will be explicit

#include <afxwin.h>           // MFC core and standard components
#include <afxext.h>           // MFC extensions

#ifndef _AFX_NO_OLE_SUPPORT
#include <afxole.h>           // MFC OLE classes
#include <afxodlgs.h>         // MFC OLE dialog classes
#include <afxdisp.h>          // MFC Automation classes
#endif // _AFX_NO_OLE_SUPPORT

#ifndef _AFX_NO_DB_SUPPORT
#include <afxdb.h>			      // MFC ODBC database classes
#endif // _AFX_NO_DB_SUPPORT

#ifndef _AFX_NO_DAO_SUPPORT
#include <afxdao.h>			      // MFC DAO database classes
#endif // _AFX_NO_DAO_SUPPORT

#include <afxdtctl.h>		      // MFC support for Internet Explorer 4 Common Controls
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			      // MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT

// TODO: Add additional include files here
#include <afxadv.h>

#endif (_WIN32)

#if defined (__APPLE__)
#include "../../../rhino4/AfxMac.h"
#include "../../../rhino4/OSX/MacHelpers.h"
#endif

#if !defined(OPENNURBS_BUILD)
// GRASSHOPPER_V4 is a special "build flavor" of rhinocommon to allow V4 grasshopper
// to build and include a copy of RhinoCommon/rhcommon_c as part of the installation
// In Rhino4, Grasshopper loads it's own private copy of RhinoCommon
// In Rhino5, Grasshopper uses the RhinoCommon that ships with Rhino5
// GRASSHOPPER_V4 == Building for Grasshopper usage in Rhino4 (NOT Rhino5)
#if defined(GRASSHOPPER_V4)
#include "../../../rhinosdk/rhino4/rhino3SystemPlugIn.h"
#else
// When building for Rhino5, this dll is part of the shipping Rhino and acts
// like a system plug-in
#include "../../../rhino4/rhino3SystemPlugIn.h"
#if defined(_WIN32)
#include "../../../rhino4/SDK/inc/RhinoSdkUiFile.h"
#endif
#endif

// gh_private_sdk.h is where all of the "private" TL and Rhino functions are declared
// This lets us figure out exactly which private classes/functions are being used by
// grasshopper
#include "gh_private_sdk.h"
#endif

// Rhino System Plug-in linking pragmas
#include "plugin_linking_pragmas.h"

#include "rhcommon_c/rhcommon_c_api.h"

