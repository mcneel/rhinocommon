#include "StdAfx.h"

#if defined(__APPLE__) && defined(OPENNURBS_BUILD)
ON_wString UniChar2on(const UniChar* inStr)
{
  // get length of inStr
  int inStrLen;
  for (inStrLen=0; inStr[inStrLen]; inStrLen++)
    ;
  
  // create an ON_wString with sufficient length
  ON_wString wstr;
  wstr.SetLength(inStrLen);
  
  // copy inStr into wstr
  int idx;
  for (idx=0; idx<inStrLen; idx++)
    wstr[idx] = inStr[idx];
  return wstr;
}
#endif

CRhCmnStringHolder::CRhCmnStringHolder()
{
#if defined(__APPLE__)
  m_macString = NULL;
#endif
}

CRhCmnStringHolder::~CRhCmnStringHolder()
{
#if defined(__APPLE__)
  if( NULL!=m_macString )
    free(m_macString);
#endif
}

void CRhCmnStringHolder::Set(const ON_wString& s)
{
#if defined(__APPLE__)
  if( NULL!=m_macString )
  {
    free(m_macString);
    m_macString = NULL;
  }
  
  const wchar_t* inStr = s.Array();
  if( inStr != NULL )
  {
    int strLen = s.Length();
    m_macString = (UniChar*) calloc (sizeof(UniChar), strLen+1);
    for (int i=0; i<strLen; i++)
      m_macString[i] = inStr[i];
  }
#else
  m_winString = s;
#endif
}

const RHMONO_STRING* CRhCmnStringHolder::Array() const
{
#if defined(__APPLE__)
  return m_macString;
#else
  return m_winString.Array();
#endif
}
