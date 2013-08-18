#pragma once

// Windows compile
#if defined (_WIN32)
#if defined (USING_RH_C_SDK)
#define RH_CPP_FUNCTION __declspec(dllimport)
#define RH_CPP_CLASS __declspec(dllimport)
#define RH_EXPORT __declspec(dllimport)
#else
#define RH_CPP_FUNCTION __declspec(dllexport)
#define RH_CPP_CLASS __declspec(dllexport)
#define RH_EXPORT __declspec(dllexport)
#endif

// RH_C_FUNCTION is always exported and is the token used in the file processor
#define RH_C_FUNCTION extern "C" __declspec(dllexport)

#define RHMONO_STRING wchar_t
// macro used to convert input strings to their appropriate type
// for a given platform.
#define INPUTSTRINGCOERCE( _variablename, _parametername) \
const wchar_t* _variablename = _parametername;
#endif // _WIN32


// OSX / iOS compiles
#if defined (__APPLE__)
#define RH_CPP_FUNCTION __attribute__ ((visibility ("default")))
#define RH_CPP_CLASS __attribute__ ((visibility ("default")))
#define RH_C_FUNCTION extern "C" __attribute__ ((visibility ("default")))
#define RH_EXPORT __attribute__ ((visibility ("default")))

#if defined (OPENNURBS_BUILD)
#define CALLBACK
typedef signed char     BOOL;
typedef const wchar_t*  LPCTSTR;
typedef unsigned short                  UInt16;
typedef UInt16                          UniChar;
ON_wString UniChar2on(const UniChar* inStr);
#endif

#define RHMONO_STRING UniChar
// macro used to convert input strings to their appropriate type
// for a given platform. On Mac I'm expecting that we will be using
// a class that takes a 2 byte string and performs the proper conversions
// to get a 4 byte wchar_t
#define INPUTSTRINGCOERCE( _variablename, _parametername)          \
const wchar_t* _variablename = NULL;                               \
ON_wString _variablename##_;                                       \
if(_parametername) {                                               \
_variablename##_ = (UniChar2on(_parametername));                   \
_variablename = (LPCTSTR) _variablename##_;                        \
}
#endif // __APPLE__


// Android compile
#if defined (ON_COMPILER_ANDROIDNDK)
#define RH_C_FUNCTION extern "C" 
#define RH_CPP_FUNCTION
#define RH_CPP_CLASS
#define RH_EXPORT

#define RHMONO_STRING ON__UINT16
// macro used to convert input strings to their appropriate type
// for a given platform.
#define INPUTSTRINGCOERCE( _variablename, _parametername)                    \
const wchar_t* _variablename = NULL;                                         \
ON_wString _variablename##_;                                                 \
if(_parametername) {                                                         \
  unsigned int __error_status = 0;                                           \
  int _parametername##count = ON_ConvertUTF16ToUTF32( 0, _parametername,     \
            -1, NULL, 0, &__error_status, 0xFFFFFFFF, 0xFFFD, 0);            \
  _variablename##_.ReserveArray(_parametername##count);                      \
  unsigned int* _variablename##_s = (unsigned int*)_variablename##_.Array(); \
  ON_ConvertUTF16ToUTF32( 0, _parametername, -1, _variablename##_s,          \
            _parametername##count, &__error_status, 0xFFFFFFFF, 0xFFFD, 0);  \
  _variablename = _variablename##_.Array();                                  \
}


typedef signed char     BOOL;
#define TRUE 1
#define FALSE 0
#define CALLBACK
#endif // ON_COMPILER_ANDROIDNDK

// Only applicable for when compiled for running inside Rhino
#if !defined(OPENNURBS_BUILD)
// Always call this function instead of ActiveDoc so
// we have a single place to fix up code to work on Mac multi-doc build
RH_CPP_FUNCTION CRhinoDoc* RhDocFromId( int id );
RH_CPP_FUNCTION int RhIdFromDoc( CRhinoDoc* pDoc );
#endif
RH_CPP_FUNCTION bool RhInShutDown();


struct ON_2DPOINT_STRUCT{ double val[2]; };
struct ON_2DVECTOR_STRUCT{ double val[2]; };
struct ON_INTERVAL_STRUCT{ double val[2]; };

struct ON_3DPOINT_STRUCT{ double val[3]; };
struct ON_LINE_STRUCT{ ON_3DPOINT_STRUCT from; ON_3DPOINT_STRUCT to; };
struct ON_3DVECTOR_STRUCT{ double val[3]; };

struct ON_4DPOINT_STRUCT{ double val[4]; };
struct ON_4DVECTOR_STRUCT{ double val[4]; };
//struct ON_PLANEEQ_STRUCT{ double val[4]; };
struct ON_2INTS{ int val[2]; };

struct ON_3FVECTOR_STRUCT{ float val[3]; };
struct ON_3FPOINT_STRUCT{ float val[3]; };

struct ON_4FVECTOR_STRUCT{ float val[4]; };
struct ON_4FPOINT_STRUCT{ float val[4]; };

struct ON_XFORM_STRUCT { double val[16]; };

struct ON_PLANE_STRUCT
{
  double origin[3];
  double xaxis[3];
  double yaxis[3];
  double zaxis[3];
  double eq[4];
};

struct ON_CIRCLE_STRUCT
{
  ON_PLANE_STRUCT plane;
  double radius;
};

void CopyToPlaneStruct(ON_PLANE_STRUCT& ps, const ON_Plane& plane);
ON_Plane FromPlaneStruct(const ON_PLANE_STRUCT& ps);
void CopyToCircleStruct(ON_CIRCLE_STRUCT& cs, const ON_Circle& circle);
ON_Circle FromCircleStruct(const ON_CIRCLE_STRUCT& cs);

unsigned int ARGB_to_ABGR( unsigned int argb );
unsigned int ABGR_to_ARGB( unsigned int abgr );

class CHack3dPointArray : public ON_Polyline
{
public:
  CHack3dPointArray( int count, ON_3dPoint* pts )
  {
    m_count = count;
    m_capacity = count;
    m_a = pts;
  }
  ~CHack3dPointArray()
  {
    m_count = 0;
    m_capacity = 0;
    m_a = 0;
  }
};


#if defined(OPENNURBS_BUILD)
class CRhCmnStringHolder
#else
class RH_EXPORT CRhCmnStringHolder
#endif
{
public:
  CRhCmnStringHolder();
  ~CRhCmnStringHolder();

  void Set(const ON_wString& s);

  const RHMONO_STRING* Array() const;
private:
#if defined(__APPLE__)
  UniChar* m_macString;
#else
  ON_wString m_winString;
#endif
#if defined(ON_COMPILER_ANDROIDNDK)
  ON_SimpleArray<ON__UINT16> m_android;
#endif
};