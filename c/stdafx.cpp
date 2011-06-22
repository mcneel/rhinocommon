/////////////////////////////////////////////////////////////////////////////
// stdafx.cpp : source file that includes just the standard includes
// rhcommon_c.pch will be the pre-compiled header
// stdafx.obj will contain the pre-compiled type information

#include "stdafx.h"

// These defines disable the CDynLinkLibrary tests
// because that test is not needed for Rhino.exe.
#define ON_NEW_MFC_EARLY_ALLOCATION_MAX 0
#define ON_NEW_MFC_EARLY_ALLOCATION_MIN 2
#include "opennurbs_memory_new.cpp"
