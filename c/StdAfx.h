/////////////////////////////////////////////////////////////////////////////
// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently

#pragma once

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN		      // Exclude rarely-used stuff from Windows headers
#endif


// Rhino SDK Preamble
#if defined(OPENNURBS_BUILD)
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

#endif //(_WIN32)

#if defined (__APPLE__) && !defined(OPENNURBS_BUILD)
#include "../../../rhino4/AfxMac.h"
#include "../../../rhino4/OSX/MacHelpers.h"
#endif

#if !defined(OPENNURBS_BUILD)

// When building for Rhino5, this dll is part of the shipping Rhino and acts
// like a system plug-in
#include "../../../rhino4/rhino3SystemPlugIn.h"
#if defined(_WIN32)
#include "../../../rhino4/SDK/inc/RhinoSdkUiFile.h"
#endif

// gh_private_sdk.h is where all of the "private" TL and Rhino functions are declared
// This lets us figure out exactly which private classes/functions are being used by
// grasshopper
#include "gh_private_sdk.h"
#endif

// Rhino System Plug-in linking pragmas
#include "plugin_linking_pragmas.h"

#include "rhcommon_c/rhcommon_c_api.h"

