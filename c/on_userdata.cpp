#include "StdAfx.h"

struct CUserDataHolderPiece
{
  ON_UserDataHolder* m_pHolder;
  ON_UUID m_id;
};

static ON_SimpleArray<CUserDataHolderPiece> m_all_holders;

RH_C_FUNCTION bool ON_UserDataHolder_MoveUserDataFrom( ON_UUID id, const ON_Object* pConstObject)
{
  bool rc = false;
  if( ON_UuidIsNotNil(id) && pConstObject && pConstObject->FirstUserData()!=NULL )
  {
    //make sure the id is not already in the list. Note this list should almost
    //always have around 1 element, so linear search is fine
    for( int i=0; i<m_all_holders.Count(); i++ )
    {
      if( m_all_holders[i].m_id == id )
        return false;
    }

    ON_UserDataHolder* pHolder = new ON_UserDataHolder();
    rc = pHolder->MoveUserDataFrom(*pConstObject);
    if( !rc )
    {
      delete pHolder;
      return false;
    }

    CUserDataHolderPiece& piece = m_all_holders.AppendNew();
    piece.m_id = id;
    piece.m_pHolder = pHolder;
    rc = true;
  }
  return rc;
}

RH_C_FUNCTION void ON_UserDataHolder_MoveUserDataTo( ON_UUID id, const ON_Object* pConstObject, bool append)
{
  if( ON_UuidIsNotNil(id) && pConstObject )
  {
    for( int i=0; i<m_all_holders.Count(); i++ )
    {
      if( m_all_holders[i].m_id == id )
      {
        ON_UserDataHolder* pHolder = m_all_holders[i].m_pHolder;
        m_all_holders.Remove(i);
        if( pHolder )
        {
          pHolder->MoveUserDataTo(*pConstObject, append);
        }
      }
    }
  }
}
