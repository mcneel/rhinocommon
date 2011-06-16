#include "StdAfx.h"

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
