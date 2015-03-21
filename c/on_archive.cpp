#include "StdAfx.h"

RH_C_FUNCTION CRhCmnStringHolder* StringHolder_New()
{
  return new CRhCmnStringHolder();
}
RH_C_FUNCTION void StringHolder_Delete(CRhCmnStringHolder* pStringHolder)
{
  if( pStringHolder )
    delete pStringHolder;
}
RH_C_FUNCTION const RHMONO_STRING* StringHolder_Get(CRhCmnStringHolder* pStringHolder)
{
  const RHMONO_STRING* rc = NULL;
  if( pStringHolder )
    rc = pStringHolder->Array();
  return rc;
}


RH_C_FUNCTION bool ON_BinaryArchive_AtEnd(const ON_BinaryArchive* pConstArchive)
{
  if( pConstArchive )
    return pConstArchive->AtEnd();
  return true;
}

RH_C_FUNCTION bool ON_BinaryArchive_Read3dmStartSection(ON_BinaryArchive* pBinaryArchive, int* version, CRhCmnStringHolder* pStringHolder)
{
  bool rc = false;
  if( pBinaryArchive && version && pStringHolder )
  {
    ON_String comment;
    rc = pBinaryArchive->Read3dmStartSection(version, comment);
    ON_wString wComment(comment);
    pStringHolder->Set(wComment);
  }
  return rc;
}

RH_C_FUNCTION unsigned int ON_BinaryArchive_Dump3dmChunk(ON_BinaryArchive* pBinaryArchive, ON_TextLog* pTextLog)
{
  unsigned int rc = 0;
  if( pBinaryArchive && pTextLog )
    rc = pBinaryArchive->Dump3dmChunk(*pTextLog);
  return rc;
}

RH_C_FUNCTION int ON_BinaryArchive_Archive3dmVersion(ON_BinaryArchive* pArchive)
{
  int rc = 0;
  if( pArchive )
    rc = pArchive->Archive3dmVersion();
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_Write3dmChunkVersion(ON_BinaryArchive* pArchive, int major, int minor)
{
  bool rc = false;
  if( pArchive )
  {
    rc = pArchive->Write3dmChunkVersion(major, minor);
  }
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_Read3dmChunkVersion(ON_BinaryArchive* pArchive, int* major, int* minor)
{
  bool rc = false;
  if( pArchive && major && minor )
  {
    rc = pArchive->Read3dmChunkVersion(major, minor);
  }
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadBool(ON_BinaryArchive* pArchive, bool* readBool)
{
  bool rc = false;
  if( pArchive && readBool )
    rc = pArchive->ReadBool(readBool);
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteBool(ON_BinaryArchive* pArchive, bool val)
{
  bool rc = false;
  if( pArchive )
    rc = pArchive->WriteBool(val);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadBool2(ON_BinaryArchive* pArchive, int count, /*ARRAY*/bool* readBool)
{
  bool rc = false;
  if( pArchive && count>0 && readBool )
  {
    char* c = (char*)readBool;
    rc = pArchive->ReadChar(count, c);
  }
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteBool2(ON_BinaryArchive* pArchive, int count, /*ARRAY*/const bool* val)
{
  bool rc = false;
  if( pArchive && count>0 && val )
  {
    const char* c = (const char*)val;
    rc = pArchive->WriteChar(count, c);
  }
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadByte(ON_BinaryArchive* pArchive, char* readByte)
{
  bool rc = false;
  if( pArchive && readByte )
    rc = pArchive->ReadByte(1, readByte);
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteByte(ON_BinaryArchive* pArchive, char val)
{
  bool rc = false;
  if( pArchive )
    rc = pArchive->WriteByte(1, &val);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadByte2(ON_BinaryArchive* pArchive, int count, /*ARRAY*/char* readByte)
{
  bool rc = false;
  if( pArchive && count>0 && readByte )
    rc = pArchive->ReadByte(count, readByte);
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteByte2(ON_BinaryArchive* pArchive, int count, /*ARRAY*/const char* val)
{
  bool rc = false;
  if( pArchive && count>0 && val )
    rc = pArchive->WriteByte(count, val);
  return rc;
}

// 10-Feb-2014 Dale Fugier, http://mcneel.myjetbrains.com/youtrack/issue/RH-24156
RH_C_FUNCTION bool ON_BinaryArchive_ReadCompressedBufferSize( ON_BinaryArchive* pArchive, unsigned int* size )
{
  bool rc = false;
  if( pArchive && size )
  {
    size_t sizeof_outbuffer = 0;
    rc = pArchive->ReadCompressedBufferSize( &sizeof_outbuffer );
    if( rc )
      *size = (unsigned int)sizeof_outbuffer;
  }
  return rc;
}

// 10-Feb-2014 Dale Fugier, http://mcneel.myjetbrains.com/youtrack/issue/RH-24156
RH_C_FUNCTION bool ON_BinaryArchive_ReadCompressedBuffer( ON_BinaryArchive* pArchive, unsigned int size, /*ARRAY*/char* pBuffer )
{
  bool rc = false;
  if( pArchive && size > 0 && pBuffer )
  {
    int bFailedCRC = 0;
    rc = pArchive->ReadCompressedBuffer( size, pBuffer, &bFailedCRC );
  }
  return rc;
}

// 10-Feb-2014 Dale Fugier, http://mcneel.myjetbrains.com/youtrack/issue/RH-24156
RH_C_FUNCTION bool ON_BinaryArchive_WriteCompressedBuffer( ON_BinaryArchive* pArchive, unsigned int size, /*ARRAY*/const char* pBuffer )
{
  bool rc = false;
  if( pArchive && size > 0 && pBuffer )
    rc = pArchive->WriteCompressedBuffer( size, pBuffer );
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadShort(ON_BinaryArchive* pArchive, short* readShort)
{
  bool rc = false;
  if( pArchive && readShort )
    rc = pArchive->ReadShort(readShort);
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteShort(ON_BinaryArchive* pArchive, short val)
{
  bool rc = false;
  if( pArchive )
    rc = pArchive->WriteShort(val);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadShort2(ON_BinaryArchive* pArchive, int count, /*ARRAY*/short* readShort)
{
  bool rc = false;
  if( pArchive && count>0 && readShort )
    rc = pArchive->ReadShort(count, readShort);
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteShort2(ON_BinaryArchive* pArchive, int count, /*ARRAY*/const short* val)
{
  bool rc = false;
  if( pArchive && count>0 && val )
    rc = pArchive->WriteShort(count, val);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadInt(ON_BinaryArchive* pArchive, int* readInt)
{
  bool rc = false;
  if( pArchive && readInt )
    rc = pArchive->ReadInt(readInt);
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteInt(ON_BinaryArchive* pArchive, int val)
{
  bool rc = false;
  if( pArchive )
    rc = pArchive->WriteInt(val);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadInt2(ON_BinaryArchive* pArchive, int count, /*ARRAY*/int* readInt)
{
  bool rc = false;
  if( pArchive && count>0 && readInt )
    rc = pArchive->ReadInt(count, readInt);
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteInt2(ON_BinaryArchive* pArchive, int count, /*ARRAY*/const int* val)
{
  bool rc = false;
  if( pArchive && count>0 && val )
    rc = pArchive->WriteInt(count, val);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadInt64(ON_BinaryArchive* pArchive, ON__INT64* readInt)
{
  bool rc = false;
  if( pArchive && readInt )
  {
    time_t t;
    rc = pArchive->ReadBigTime(&t);
    *readInt = (ON__INT64)t;
  }
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteInt64(ON_BinaryArchive* pArchive, ON__INT64 val)
{
  bool rc = false;
  if( pArchive )
    rc = pArchive->WriteBigTime((time_t)val);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadSingle(ON_BinaryArchive* pArchive, float* readFloat)
{
  bool rc = false;
  if( pArchive && readFloat )
    rc = pArchive->ReadFloat(readFloat);
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteSingle(ON_BinaryArchive* pArchive, float val)
{
  bool rc = false;
  if( pArchive )
    rc = pArchive->WriteFloat(val);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadSingle2(ON_BinaryArchive* pArchive, int count, /*ARRAY*/float* readFloat)
{
  bool rc = false;
  if( pArchive && count>0 && readFloat )
    rc = pArchive->ReadFloat(count, readFloat);
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteSingle2(ON_BinaryArchive* pArchive, int count, /*ARRAY*/const float* val)
{
  bool rc = false;
  if( pArchive && count>0 && val )
    rc = pArchive->WriteFloat(count, val);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadDouble(ON_BinaryArchive* pArchive, double* readDouble)
{
  bool rc = false;
  if( pArchive && readDouble )
    rc = pArchive->ReadDouble(readDouble);
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteDouble(ON_BinaryArchive* pArchive, double val)
{
  bool rc = false;
  if( pArchive )
    rc = pArchive->WriteDouble(val);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadDouble2(ON_BinaryArchive* pArchive, int count, /*ARRAY*/double* readDouble)
{
  bool rc = false;
  if( pArchive && count>0 && readDouble )
    rc = pArchive->ReadDouble(count,readDouble);
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteDouble2(ON_BinaryArchive* pArchive, int count, /*ARRAY*/const double* val)
{
  bool rc = false;
  if( pArchive && count>0 && val )
    rc = pArchive->WriteDouble(count,val);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadGuid(ON_BinaryArchive* pArchive, ON_UUID* readGuid)
{
  bool rc = false;
  if( pArchive && readGuid )
    rc = pArchive->ReadUuid(*readGuid);
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteGuid(ON_BinaryArchive* pArchive, const ON_UUID* val)
{
  bool rc = false;
  if( pArchive && val )
    rc = pArchive->WriteUuid(*val);
  return rc;
}


RH_C_FUNCTION bool ON_BinaryArchive_ReadString(ON_BinaryArchive* pArchive, CRhCmnStringHolder* pStringHolder)
{
  bool rc = false;
  if( pArchive && pStringHolder )
  {
    ON_wString str;
    rc = pArchive->ReadString(str);
    pStringHolder->Set(str);
  }
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteString(ON_BinaryArchive* pArchive, const RHMONO_STRING* str)
{
  bool rc = false;
  if( pArchive && str )
  {
    INPUTSTRINGCOERCE(_str, str);
    ON_wString s(_str);
    rc = pArchive->WriteString(s);
  }
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadColor(ON_BinaryArchive* pArchive, int* argb)
{
  bool rc = false;
  if( pArchive && argb )
  {
    ON_Color c;
    rc = pArchive->ReadColor(c);
    if( rc )
    {
      unsigned int _c = (unsigned int)c;
      *argb = (int)ABGR_to_ARGB(_c);
    }
  }
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteColor(ON_BinaryArchive* pArchive, int argb)
{
  bool rc = false;
  if( pArchive  )
  {
    unsigned int abgr = ARGB_to_ABGR(argb);
    ON_Color c(abgr);
    rc = pArchive->WriteColor(c);
  }
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadTransform(ON_BinaryArchive* pArchive, ON_Xform* xf)
{
  bool rc = false;
  if( pArchive && xf )
    rc = pArchive->ReadXform(*xf);
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WriteTransform(ON_BinaryArchive* pArchive, const ON_Xform* xf)
{
  bool rc = false;
  if( pArchive && xf )
    rc = pArchive->WriteXform(*xf);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadPlane(ON_BinaryArchive* pArchive, ON_PLANE_STRUCT* plane)
{
  bool rc = false;
  if( pArchive && plane )
  {
    ON_Plane _plane;
    rc = pArchive->ReadPlane(_plane);
    CopyToPlaneStruct(*plane, _plane);
  }
  return rc;
}
RH_C_FUNCTION bool ON_BinaryArchive_WritePlane(ON_BinaryArchive* pArchive, const ON_PLANE_STRUCT* plane)
{
  bool rc = false;
  if( pArchive && plane )
  {
    ON_Plane _plane = FromPlaneStruct(*plane);
    rc = pArchive->WritePlane(_plane);
  }
  return rc;
}

RH_C_FUNCTION ON_Object* ON_BinaryArchive_ReadObject(ON_BinaryArchive* pArchive, int* read_rc)
{
  ON_Object* rc = NULL;
  if( pArchive && read_rc )
  {
    *read_rc = pArchive->ReadObject(&rc);
  }
  return rc;
}

RH_C_FUNCTION ON_Geometry* ON_BinaryArchive_ReadGeometry(ON_BinaryArchive* pArchive, int* read_rc)
{
  ON_Geometry* rc = NULL;
  if( pArchive && read_rc )
  {
    ON_Object* pObject = NULL;
    *read_rc = pArchive->ReadObject(&pObject);
    rc = ON_Geometry::Cast(pObject);
    if( NULL==rc )
    {
      *read_rc = 0;
      delete pObject;
    }
  }
  return rc;
}


RH_C_FUNCTION ON_MeshParameters* ON_BinaryArchive_ReadMeshParameters(ON_BinaryArchive* pArchive)
{
  ON_MeshParameters* rc = NULL;
  if( pArchive )
  {
    rc = new ON_MeshParameters();
    if( !rc->Read(*pArchive) )
    {
      delete rc;
      rc = NULL;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_WriteMeshParameters(ON_BinaryArchive* pArchive, const ON_MeshParameters* pConstMeshParameters)
{
  bool rc = false;
  if( pArchive && pConstMeshParameters )
  {
    rc = pConstMeshParameters->Write(*pArchive);
  }
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_WriteGeometry(ON_BinaryArchive* pArchive, const ON_Geometry* pConstGeometry)
{
  bool rc = false;
  if( pArchive && pConstGeometry )
  {
    // 13 March 2013 (S. Baer) RH-16957
    // The geometry reader was using ReadObject, so we need to use the
    // WriteObject function instead of the ON_Geometry::Write function
    rc = pArchive->WriteObject(pConstGeometry);
    //rc = pConstGeometry->Write(*pArchive) ? true:false;
  }
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadObjRef(ON_BinaryArchive* pArchive, ON_ObjRef* pObjRef)
{
  bool rc = false;
  if( pArchive && pObjRef )
    rc = pObjRef->Read(*pArchive);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_WriteObjRef(ON_BinaryArchive* pArchive, const ON_ObjRef* pConstObjRef)
{
  bool rc = false;
  if( pArchive && pConstObjRef )
    rc = pConstObjRef->Write(*pArchive);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_ReadObjRefArray(ON_BinaryArchive* pArchive, ON_ClassArray<ON_ObjRef>* pObjRefArray)
{
  bool rc = false;
  if( pArchive && pObjRefArray )
    rc = pArchive->ReadArray(*pObjRefArray);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_WriteObjRefArray(ON_BinaryArchive* pArchive, const ON_ClassArray<ON_ObjRef>* pConstObjRefArray)
{
  bool rc = false;
  if( pArchive && pConstObjRefArray )
    rc = pArchive->WriteArray(*pConstObjRefArray);
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_BeginReadDictionary(ON_BinaryArchive* pArchive, ON_UUID* dictionary_id, unsigned int* version, CRhCmnStringHolder* pStringHolder)
{
  bool rc = false;
  if( pArchive && dictionary_id && version && pStringHolder )
  {
    ON_wString name;
    rc = pArchive->BeginReadDictionary(dictionary_id, version, name);
    if( rc )
      pStringHolder->Set(name);
  }
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_EndReadDictionary(ON_BinaryArchive* pArchive)
{
  bool rc = false;
  if( pArchive )
    rc = pArchive->EndReadDictionary();
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_BeginWriteDictionary(ON_BinaryArchive* pArchive, ON_UUID dictionary_id, unsigned int version, const RHMONO_STRING* name )
{
  bool rc = false;
  if( pArchive && name )
  {
    INPUTSTRINGCOERCE(_name, name);
    rc = pArchive->BeginWriteDictionary(dictionary_id, version, _name);
  }
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_EndWriteDictionary(ON_BinaryArchive* pArchive)
{
  bool rc = false;
  if( pArchive )
    rc = pArchive->EndWriteDictionary();
  return rc;
}

RH_C_FUNCTION int ON_BinaryArchive_BeginReadDictionaryEntry(ON_BinaryArchive* pArchive, int* de_type, CRhCmnStringHolder* pStringHolder)
{
  int rc = 0;
  if( pArchive && de_type && pStringHolder )
  {
    ON_wString name;
    rc = pArchive->BeginReadDictionaryEntry(de_type, name);
    pStringHolder->Set(name);
  }
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_EndReadDictionaryEntry(ON_BinaryArchive* pArchive)
{
  bool rc = false;
  if( pArchive )
    rc = pArchive->EndReadDictionaryEntry();
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_BeginWriteDictionaryEntry(ON_BinaryArchive* pArchive, int de_type, const RHMONO_STRING* entry_name)
{
  bool rc = false;
  if( pArchive && entry_name )
  {
    INPUTSTRINGCOERCE(_entry_name, entry_name);
    rc = pArchive->BeginWriteDictionaryEntry(de_type, _entry_name);
  }
  return rc;
}

RH_C_FUNCTION bool ON_BinaryArchive_EndWriteDictionaryEntry(ON_BinaryArchive* pArchive)
{
  bool rc = false;
  if( pArchive )
    rc = pArchive->EndWriteDictionaryEntry();
  return rc;
}

RH_C_FUNCTION ON_Object* ON_ReadBufferArchive(int archive_3dm_version, int archive_on_version, int length, /*ARRAY*/const unsigned char* buffer)
{
  // Eliminate potential bogus file versions written
  if (archive_3dm_version > 5 && archive_3dm_version < 50)
    return NULL;
  ON_Object* rc = NULL;
  if( length>0 && buffer )
  {
    ON_Read3dmBufferArchive archive(length, buffer, false, archive_3dm_version, archive_on_version);
    archive.ReadObject( &rc );
  }
  return rc;
}

RH_C_FUNCTION ON_Write3dmBufferArchive* ON_WriteBufferArchive_NewWriter(const ON_Object* pConstObject, int rhinoversion, bool writeuserdata, unsigned int* length)
{
  ON_Write3dmBufferArchive* rc = NULL;

  if( pConstObject && length )
  {
    ON_UserDataHolder holder;
    if( !writeuserdata )
      holder.MoveUserDataFrom(*pConstObject);
    *length = 0;
    size_t sz = pConstObject->SizeOf() + 512; // 256 was too small on x86 builds to account for extra data written
    rc = new ON_Write3dmBufferArchive(sz, 0, rhinoversion, ON::Version());
    if( rc->WriteObject(pConstObject) )
    {
      *length = (unsigned int)rc->SizeOfArchive();
    }
    else
    {
      delete rc;
      rc = NULL;
    }
    if( !writeuserdata )
      holder.MoveUserDataTo(*pConstObject, false);
  }
  return rc;
}

RH_C_FUNCTION void ON_WriteBufferArchive_Delete(ON_BinaryArchive* pBinaryArchive)
{
  if( pBinaryArchive )
    delete pBinaryArchive;
}

RH_C_FUNCTION unsigned char* ON_WriteBufferArchive_Buffer(const ON_Write3dmBufferArchive* pBinaryArchive)
{
  unsigned char* rc = NULL;
  if( pBinaryArchive )
  {
    rc = (unsigned char*)pBinaryArchive->Buffer();
  }
  return rc;
}



/////////////////////////////////
// move to on_extensions.cpp

RH_C_FUNCTION ONX_Model* ONX_Model_New()
{
  return new ONX_Model();
}

RH_C_FUNCTION void ONX_Model_ReadNotes(const RHMONO_STRING* path, CRhCmnStringHolder* pString)
{
  if( path && pString )
  {
    INPUTSTRINGCOERCE(_path, path);

    FILE* fp = ON::OpenFile( _path, L"rb" );
    if( fp )
    {
      ON_BinaryFile file( ON::read3dm, fp);
      int version = 0;
      ON_String comments;
      BOOL rc = file.Read3dmStartSection( &version, comments );
      if(rc)
      {
        ON_3dmProperties prop;
        file.Read3dmProperties(prop);
        if( prop.m_Notes.IsValid() )
        {
          pString->Set( prop.m_Notes.m_notes );
        }
      }
      ON::CloseFile(fp);
    }
  }
}

RH_C_FUNCTION int ONX_Model_ReadArchiveVersion(const RHMONO_STRING* path)
{
  if( path )
  {
    INPUTSTRINGCOERCE(_path, path);
    
    FILE* fp = ON::OpenFile( _path, L"rb" );
    if( fp )
    {
      ON_BinaryFile file( ON::read3dm, fp);
      int version = 0;
      ON_String comment_block;
      ON_BOOL32 rc = file.Read3dmStartSection( &version, comment_block );
      if(rc)
      {
        ON::CloseFile(fp);
        return version;
      }
      ON::CloseFile(fp);
    }
  }
  
  return 0;
}

RH_C_FUNCTION ON_3dmRevisionHistory* ONX_Model_ReadRevisionHistory(const RHMONO_STRING* path, CRhCmnStringHolder* pStringCreated, CRhCmnStringHolder* pStringLastEdited, int* revision)
{
  ON_3dmRevisionHistory* rc = NULL;
  if( path && pStringCreated && pStringLastEdited && revision )
  {
    INPUTSTRINGCOERCE(_path, path);

    FILE* fp = ON::OpenFile( _path, L"rb" );
    if( fp )
    {
      ON_BinaryFile file( ON::read3dm, fp);
      int version = 0;
      ON_String comments;
      
      if(file.Read3dmStartSection( &version, comments ))
      {
        ON_3dmProperties prop;
        file.Read3dmProperties(prop);

        rc = new ON_3dmRevisionHistory(prop.m_RevisionHistory);
        pStringCreated->Set(prop.m_RevisionHistory.m_sCreatedBy);
        pStringLastEdited->Set(prop.m_RevisionHistory.m_sLastEditedBy);
        *revision = prop.m_RevisionHistory.m_revision_count;
      }
      ON::CloseFile(fp);
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_3dmRevisionHistory_GetDate(const ON_3dmRevisionHistory* pConstRevisionHistory, bool created, int* seconds, int* minutes,
                                                 int* hours, int* days, int* months, int* years)
{
  bool rc = false;
  if( pConstRevisionHistory && seconds && minutes && hours && days && months && years )
  {
    rc = created ? pConstRevisionHistory->CreateTimeIsSet() : pConstRevisionHistory->LastEditedTimeIsSet();
    if( rc )
    {
      tm revdate = created ? pConstRevisionHistory->m_create_time : pConstRevisionHistory->m_last_edit_time;
      *seconds = revdate.tm_sec;
      *minutes = revdate.tm_min;
      *hours = revdate.tm_hour;
      *days = revdate.tm_mday;
      *months = revdate.tm_mon;
      *years = revdate.tm_year + 1900;
    }
  }
  return rc;
}

RH_C_FUNCTION ON_3dmRevisionHistory* ONX_Model_RevisionHistory(ONX_Model* pModel)
{
  if( pModel )
    return &(pModel->m_properties.m_RevisionHistory);
  return 0;
}

RH_C_FUNCTION void ON_3dmRevisionHistory_Delete(ON_3dmRevisionHistory* pRevisionHistory)
{
  if( pRevisionHistory )
    delete pRevisionHistory;
}

RH_C_FUNCTION void ONX_Model_ReadApplicationDetails(const RHMONO_STRING* path, CRhCmnStringHolder* pApplicationName, CRhCmnStringHolder* pApplicationUrl, CRhCmnStringHolder* pApplicationDetails)
{
  if( path && pApplicationName && pApplicationUrl && pApplicationDetails )
  {
    INPUTSTRINGCOERCE(_path, path);

    FILE* fp = ON::OpenFile( _path, L"rb" );
    if( fp )
    {
      ON_BinaryFile file( ON::read3dm, fp);
      int version = 0;
      ON_String comments;
      BOOL rc = file.Read3dmStartSection( &version, comments );
      if(rc)
      {
        ON_3dmProperties prop;
        file.Read3dmProperties(prop);
        pApplicationName->Set(prop.m_Application.m_application_name);
        pApplicationUrl->Set(prop.m_Application.m_application_URL);
        pApplicationDetails->Set(prop.m_Application.m_application_details);
      }
      ON::CloseFile(fp);
    }
  }
}

RH_C_FUNCTION ONX_Model* ONX_Model_ReadFile(const RHMONO_STRING* path, CRhCmnStringHolder* pStringHolder)
{
  ONX_Model* rc = NULL;
  if( path )
  {
    INPUTSTRINGCOERCE(_path, path);
    rc = new ONX_Model();
    ON_wString s;
    ON_TextLog log(s);
    ON_TextLog* pLog = pStringHolder ? &log : NULL;
    if( !rc->Read(_path, pLog) )
    {
      delete rc;
      rc = NULL;
    }
    if( pStringHolder )
      pStringHolder->Set(s);
  }
  return rc;
}

enum ReadFileTableTypeFilter : int
{
  ttfNone = 0,
  ttfPropertiesTable          = 0x000001,
  ttfSettingsTable            = 0x000002,
  ttfBitmapTable              = 0x000004,
  ttfTextureMappingTable      = 0x000008,
  ttfMaterialTable            = 0x000010,
  ttfLinetypeTable            = 0x000020,
  ttfLayerTable               = 0x000040,
  ttfGroupTable               = 0x000080,
  ttfFontTable                = 0x000100,
  ttfFutureFontTable          = 0x000200,
  ttfDimstyleTable            = 0x000400,
  ttfLightTable               = 0x000800,
  ttfHatchpatternTable        = 0x001000,
  ttfInstanceDefinitionTable  = 0x002000,
  ttfObjectTable              = 0x004000, 
  ttfHistoryrecordTable       = 0x008000,
  ttfUserTable                = 0x010000
};

enum ObjectTypeFilter : unsigned int
{
  otNone  =          0,
  otPoint         =          1, // some type of ON_Point
  otPointset      =          2, // some type of ON_PointCloud, ON_PointGrid, ...
  otCurve         =          4, // some type of ON_Curve like ON_LineCurve, ON_NurbsCurve, etc.
  otSurface       =          8, // some type of ON_Surface like ON_PlaneSurface, ON_NurbsSurface, etc.
  otBrep          =       0x10, // some type of ON_Brep
  otMesh          =       0x20, // some type of ON_Mesh
  otAnnotation    =      0x200, // some type of ON_Annotation
  otInstanceDefinition  =      0x800, // some type of ON_InstanceDefinition
  otInstanceReference   =     0x1000, // some type of ON_InstanceRef
  otTextDot             =     0x2000, // some type of ON_TextDot
  otDetail        =     0x8000, // some type of ON_DetailView
  otHatch         =    0x10000, // some type of ON_Hatch
  otExtrusion     = 0x40000000, // some type of ON_Extrusion
  otAny           = 0xFFFFFFFF
};

RH_C_FUNCTION bool ONX_Model_WriteFile(ONX_Model* pModel, const RHMONO_STRING* path, int version, CRhCmnStringHolder* pStringHolder)
{
  bool rc = false;
  if( pModel )
  {
    INPUTSTRINGCOERCE(_path, path);
    ON_wString s;
    ON_TextLog log(s);
    ON_TextLog* pLog = pStringHolder ? &log : NULL;
    rc = pModel->Write(_path, version, NULL, pLog);
    if( pStringHolder )
      pStringHolder->Set(s);
  }
  return rc;
}

RH_C_FUNCTION bool ONX_Model_WriteFile2(ONX_Model* pModel, const RHMONO_STRING* path, int version, bool writeRenderMeshes, bool writeAnalysisMeshes, bool writeUserData)
{
  bool rc = false;
  INPUTSTRINGCOERCE(_path, path);
  if( pModel && _path )
  {
    FILE* fp = ON::OpenFile(_path, L"wb");
    if( 0==fp )
      return false;
    ON_BinaryFile binary_file(ON::write3dm, fp);
    binary_file.EnableSave3dmRenderMeshes(writeRenderMeshes?1:0);
    binary_file.EnableSave3dmAnalysisMeshes(writeAnalysisMeshes?1:0);
    binary_file.EnableSaveUserData(writeUserData?1:0);
    rc = pModel->Write(binary_file, version, 0, 0);
    ON::CloseFile(fp);
  }
  return rc;
}

RH_C_FUNCTION void ONX_Model_Delete(ONX_Model* pModel)
{
  if( pModel )
    delete pModel;
}

RH_C_FUNCTION bool ONX_Model_IsValid(const ONX_Model* pConstModel, CRhCmnStringHolder* pString)
{
  bool rc = false;
  if( pConstModel && pString )
  {
    ON_wString s;
    ON_TextLog log(s);
    rc = pConstModel->IsValid(&log);
    pString->Set(s);
  }
  return rc;
}

RH_C_FUNCTION bool ONX_Model_IsValid2(const ONX_Model* pConstModel, ON_TextLog* pTextLog)
{
  bool rc = false;
  if( pConstModel && pTextLog )
    rc = pConstModel->IsValid(pTextLog);
  return rc;
}

RH_C_FUNCTION void ONX_Model_Polish(ONX_Model* pModel)
{
  if( pModel )
    pModel->Polish();
}

RH_C_FUNCTION int ONX_Model_Audit(ONX_Model* pModel, bool attemptRepair, int* repairCount, CRhCmnStringHolder* pString, ON_SimpleArray<int>* warnings)
{
  int rc = -1;
  if( pModel && repairCount )
  {
    ON_wString s;
    ON_TextLog log(s);
    rc = pModel->Audit(attemptRepair, repairCount, &log, warnings);
    if( pString )
      pString->Set(s);
  }
  return rc;
}

RH_C_FUNCTION void ONX_Model_GetStartSectionComments(const ONX_Model* pConstModel, CRhCmnStringHolder* pString)
{
  if( pConstModel && pString )
    pString->Set(pConstModel->m_sStartSectionComments);
}

RH_C_FUNCTION void ONX_Model_SetStartSectionComments(ONX_Model* pModel, const RHMONO_STRING* comments)
{
  if( pModel )
  {
    INPUTSTRINGCOERCE(_comments, comments);
    ON_String ssc(_comments);
    pModel->m_sStartSectionComments = ssc;
  }
}

RH_C_FUNCTION void ONX_Model_GetNotes(const ONX_Model* pConstModel, CRhCmnStringHolder* pString, bool* visible, bool* html, int* left, int* top, int* right, int* bottom)
{
  if( pConstModel && visible && html && left && top && right && bottom )
  {
    const ON_3dmNotes& notes = pConstModel->m_properties.m_Notes;
    if( pString )
      pString->Set(notes.m_notes);
    *visible = notes.m_bVisible?true:false;
    *html = notes.m_bHTML?true:false;
    *left = notes.m_window_left;
    *top = notes.m_window_top;
    *right = notes.m_window_right;
    *bottom = notes.m_window_bottom;
  }
}

RH_C_FUNCTION void ONX_Model_SetNotesString(ONX_Model* pModel, const RHMONO_STRING* notes)
{
  if( pModel )
  {
    INPUTSTRINGCOERCE(_notes, notes);
    pModel->m_properties.m_Notes.m_notes = _notes;
  }
}

RH_C_FUNCTION void ONX_Model_SetNotes(ONX_Model* pModel, bool visible, bool html, int left, int top, int right, int bottom)
{
  if( pModel )
  {
    pModel->m_properties.m_Notes.m_bHTML = html?1:0;
    pModel->m_properties.m_Notes.m_bVisible = visible?1:0;
    pModel->m_properties.m_Notes.m_window_left = left;
    pModel->m_properties.m_Notes.m_window_top = top;
    pModel->m_properties.m_Notes.m_window_right = right;
    pModel->m_properties.m_Notes.m_window_bottom = bottom;
  }
}

enum ONXModelTable : int
{
  onxDumpAll = 0,
  onxDumpSummary = 1,
  onxBitmapTable = 2,
  onxTextureMappingTable = 3,
  onxMaterialTable = 4,
  onxLinetypeTable = 5,
  onxLayerTable = 6,
  onxLightTable = 7,
  onxGroupTable = 8,
  onxFontTable = 9,
  onxDimStyleTable = 10,
  onxHatchPatternTable = 11,
  onxIDefTable = 12,
  onxObjectTable = 13,
  onxHistoryRecordTable = 14,
  onxUserDataTable = 15,
  onxViewTable = 16,
  onxNamedViewTable = 17,
};

RH_C_FUNCTION int ONX_Model_TableCount(const ONX_Model* pConstModel, enum ONXModelTable which)
{

  int rc = 0;
  if( pConstModel )
  {
    switch(which)
    {
    case onxBitmapTable:
      rc = pConstModel->m_bitmap_table.Count();
      break;
    case onxTextureMappingTable:
      rc = pConstModel->m_mapping_table.Count();
      break;
    case onxMaterialTable:
      rc = pConstModel->m_material_table.Count();
      break;
    case onxLinetypeTable:
      rc = pConstModel->m_linetype_table.Count();
      break;
    case onxLayerTable:
      rc = pConstModel->m_layer_table.Count();
      break;
    case onxLightTable:
      rc = pConstModel->m_light_table.Count();
      break;
    case onxGroupTable:
      rc = pConstModel->m_group_table.Count();
      break;
    case onxFontTable:
      rc = pConstModel->m_font_table.Count();
      break;
    case onxDimStyleTable:
      rc = pConstModel->m_dimstyle_table.Count();
      break;
    case onxHatchPatternTable:
      rc = pConstModel->m_hatch_pattern_table.Count();
      break;
    case onxIDefTable:
      rc = pConstModel->m_idef_table.Count();
      break;
    case onxObjectTable:
      rc = pConstModel->m_object_table.Count();
      break;
    case onxHistoryRecordTable:
      rc = pConstModel->m_history_record_table.Count();
      break;
    case onxUserDataTable:
      rc = pConstModel->m_userdata_table.Count();
      break;
    case onxViewTable:
      rc = pConstModel->m_settings.m_views.Count();
      break;
    case onxNamedViewTable:
      rc = pConstModel->m_settings.m_named_views.Count();
      break;
    default:
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ONX_Model_Dump(const ONX_Model* pConstModel, enum ONXModelTable which, CRhCmnStringHolder* pStringHolder)
{
  if( pConstModel && pStringHolder )
  {
    ON_wString s;
    ON_TextLog log(s);
    switch(which)
    {
    case onxDumpAll:
      pConstModel->Dump(log);
      break;
    case onxDumpSummary:
      pConstModel->DumpSummary(log);
      break;
    case onxBitmapTable:
      pConstModel->DumpBitmapTable(log);
      break;
    case onxTextureMappingTable:
      pConstModel->DumpTextureMappingTable(log);
      break;
    case onxMaterialTable:
      pConstModel->DumpMaterialTable(log);
      break;
    case onxLinetypeTable:
      pConstModel->DumpLinetypeTable(log);
      break;
    case onxLayerTable:
      pConstModel->DumpLayerTable(log);
      break;
    case onxLightTable:
      pConstModel->DumpLightTable(log);
      break;
    case onxGroupTable:
      pConstModel->DumpGroupTable(log);
      break;
    case onxFontTable:
      pConstModel->DumpFontTable(log);
      break;
    case onxDimStyleTable:
      pConstModel->DumpDimStyleTable(log);
      break;
    case onxHatchPatternTable:
      pConstModel->DumpHatchPatternTable(log);
      break;
    case onxIDefTable:
      pConstModel->DumpIDefTable(log);
      break;
    case onxObjectTable:
      pConstModel->DumpObjectTable(log);
      break;
    case onxHistoryRecordTable:
      pConstModel->DumpHistoryRecordTable(log);
      break;
    case onxUserDataTable:
      pConstModel->DumpUserDataTable(log);
      break;
    default:
      break;
    }
    pStringHolder->Set(s);
  }
}

RH_C_FUNCTION void ONX_Model_Dump2(const ONX_Model* pConstModel, ON_TextLog* pTextLog)
{
  if( pConstModel && pTextLog )
    pConstModel->Dump(*pTextLog);
}

RH_C_FUNCTION const ON_Geometry* ONX_Model_ModelObjectGeometry(const ONX_Model* pConstModel, int index)
{
  const ON_Geometry* rc = NULL;
  if( pConstModel && index>=0 && index<pConstModel->m_object_table.Count() )
  {
    rc = ON_Geometry::Cast(pConstModel->m_object_table[index].m_object);
  }
  return rc;
}

RH_C_FUNCTION const ON_3dmObjectAttributes* ONX_Model_ModelObjectAttributes(const ONX_Model* pConstModel, int index)
{
  const ON_3dmObjectAttributes* rc = NULL;
  if( pConstModel && index>=0 && index<pConstModel->m_object_table.Count() )
  {
    rc = &(pConstModel->m_object_table[index].m_attributes);
  }
  return rc;
}

RH_C_FUNCTION bool ONX_Model_ObjectTable_LayerIndexTest(const ONX_Model* pConstModel, int objectIndex, int layerIndex)
{
  bool rc = false;
  if( pConstModel && objectIndex>=0 && objectIndex<pConstModel->m_object_table.Count() )
  {
    const ONX_Model_Object& obj = pConstModel->m_object_table[objectIndex];
    rc = obj.m_attributes.m_layer_index == layerIndex;
  }
  return rc;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddPoint(ONX_Model* pModel, ON_3DPOINT_STRUCT point, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel )
  {
    ONX_Model_Object mo;
    if( pConstAttributes )
      mo.m_attributes = *pConstAttributes;
    mo.m_object = new ON_Point(point.val[0], point.val[1], point.val[2]);
    ::ON_CreateUuid(mo.m_attributes.m_uuid);
    pModel->m_object_table.Append(mo);
    return mo.m_attributes.m_uuid;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddPointCloud(ONX_Model* pModel, int count, /*ARRAY*/const ON_3dPoint* points, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel && count>0 && points )
  {
    ONX_Model_Object mo;
    if( pConstAttributes )
      mo.m_attributes = *pConstAttributes;
    ON_PointCloud* pCloud = new ON_PointCloud();
    pCloud->m_P.Append(count, points);
    mo.m_object = pCloud;
    ::ON_CreateUuid(mo.m_attributes.m_uuid);
    pModel->m_object_table.Append(mo);
    return mo.m_attributes.m_uuid;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddPointCloud2(ONX_Model* pModel, const ON_PointCloud* pConstPointCloud, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel && pConstPointCloud )
  {
    ONX_Model_Object mo;
    if( pConstAttributes )
      mo.m_attributes = *pConstAttributes;
    mo.m_object = new ON_PointCloud(*pConstPointCloud);
    ::ON_CreateUuid(mo.m_attributes.m_uuid);
    pModel->m_object_table.Append(mo);
    return mo.m_attributes.m_uuid;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddClippingPlane(ONX_Model* pModel, const ON_PLANE_STRUCT* plane, double du, double dv, int count, /*ARRAY*/const ON_UUID* clippedViewportIds, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel && plane && du>0.0 && dv>0.0 && count>0 && clippedViewportIds )
  {
    ON_Plane temp = FromPlaneStruct(*plane);
    if( temp.IsValid() )
    {
      ON_Interval domain0( 0.0, du );
      ON_Interval domain1( 0.0, dv );

      ON_PlaneSurface srf( temp );
      srf.SetExtents( 0, domain0, true );
      srf.SetExtents( 1, domain1, true );
      srf.SetDomain( 0, domain0.Min(), domain0.Max() );
      srf.SetDomain( 1, domain1.Min(), domain1.Max() );
      if( srf.IsValid() )
      {
        ON_ClippingPlaneSurface* cps = new ON_ClippingPlaneSurface(srf);
        for( int i=0; i<count; i++ )
          cps->m_clipping_plane.m_viewport_ids.AddUuid(clippedViewportIds[i]);

        ONX_Model_Object mo;
        if( pConstAttributes )
          mo.m_attributes = *pConstAttributes;
        mo.m_object = cps;
        ::ON_CreateUuid(mo.m_attributes.m_uuid);
        pModel->m_object_table.Append(mo);
        return mo.m_attributes.m_uuid;
      }
    }
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddLinearDimension( ONX_Model* pModel, const ON_LinearDimension2* pConstDimension, const ON_3dmObjectAttributes* pConstAttributes )
{
  if( pModel && pConstDimension )
  {
    ONX_Model_Object mo;
    if( pConstAttributes )
      mo.m_attributes = *pConstAttributes;
    mo.m_object = new ON_LinearDimension2(*pConstDimension);
    ::ON_CreateUuid(mo.m_attributes.m_uuid);
    pModel->m_object_table.Append(mo);
    return mo.m_attributes.m_uuid;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddLine( ONX_Model* pModel, ON_3DPOINT_STRUCT pt0, ON_3DPOINT_STRUCT pt1, const ON_3dmObjectAttributes* pConstAttributes )
{
  if( pModel )
  {
    ONX_Model_Object mo;
    if( pConstAttributes )
      mo.m_attributes = *pConstAttributes;
    ON_3dPoint _pt0(pt0.val);
    ON_3dPoint _pt1(pt1.val);
    mo.m_object = new ON_LineCurve(_pt0, _pt1);
    ::ON_CreateUuid(mo.m_attributes.m_uuid);
    pModel->m_object_table.Append(mo);
    return mo.m_attributes.m_uuid;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddPolyline( ONX_Model* pModel, int count, /*ARRAY*/const ON_3dPoint* points, const ON_3dmObjectAttributes* pConstAttributes )
{
  if( pModel && count>0 && points )
  {
    ONX_Model_Object mo;
    if( pConstAttributes )
      mo.m_attributes = *pConstAttributes;
    ON_PolylineCurve* pC = new ON_PolylineCurve();
    pC->m_pline.Append(count, points);
    mo.m_object = pC;
    ::ON_CreateUuid(mo.m_attributes.m_uuid);
    pModel->m_object_table.Append(mo);
    return mo.m_attributes.m_uuid;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddArc(ONX_Model* pModel, ON_Arc* pArc, const ON_3dmObjectAttributes* pConstAttributes )
{
  if( pModel && pArc )
  {
    pArc->plane.UpdateEquation();
    ONX_Model_Object mo;
    if( pConstAttributes )
      mo.m_attributes = *pConstAttributes;
    ON_ArcCurve* pC = new ON_ArcCurve(*pArc);
    mo.m_object = pC;
    ::ON_CreateUuid(mo.m_attributes.m_uuid);
    pModel->m_object_table.Append(mo);
    return mo.m_attributes.m_uuid;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddCircle(ONX_Model* pModel, const ON_CIRCLE_STRUCT* pCircle, const ON_3dmObjectAttributes* pConstAttributes )
{
  if( pModel && pCircle )
  {
    ON_Circle circle = FromCircleStruct(*pCircle);
    ONX_Model_Object mo;
    if( pConstAttributes )
      mo.m_attributes = *pConstAttributes;
    ON_ArcCurve* pC = new ON_ArcCurve(circle);
    mo.m_object = pC;
    ::ON_CreateUuid(mo.m_attributes.m_uuid);
    pModel->m_object_table.Append(mo);
    return mo.m_attributes.m_uuid;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddEllipse(ONX_Model* pModel, ON_Ellipse* pEllipse, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel && pEllipse )
  {
    pEllipse->plane.UpdateEquation();
    ON_NurbsCurve* nc = new ON_NurbsCurve();
    if(2 == pEllipse->GetNurbForm(*nc) )
    {
      ONX_Model_Object mo;
      if( pConstAttributes )
        mo.m_attributes = *pConstAttributes;
      mo.m_object = nc;
      ::ON_CreateUuid(mo.m_attributes.m_uuid);
      pModel->m_object_table.Append(mo);
      return mo.m_attributes.m_uuid;
    }
    // didn't work. delete the NurbsCurve
    delete nc;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddSphere(ONX_Model* pModel, ON_Sphere* sphere, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel && sphere )
  {
    // make sure the plane equation is in-sync for this sphere
    sphere->plane.UpdateEquation();
    ON_RevSurface* pRevSurface = sphere->RevSurfaceForm(false);
    if( pRevSurface )
    {
      ONX_Model_Object mo;
      if( pConstAttributes )
        mo.m_attributes = *pConstAttributes;
      mo.m_object = pRevSurface;
      ::ON_CreateUuid(mo.m_attributes.m_uuid);
      pModel->m_object_table.Append(mo);
      return mo.m_attributes.m_uuid;
    }
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddCurve(ONX_Model* pModel, const ON_Curve* pConstCurve, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel && pConstCurve )
  {
    ON_Curve* pCurve = pConstCurve->DuplicateCurve();
    if( pCurve )
    {
      ONX_Model_Object mo;
      if( pConstAttributes )
        mo.m_attributes = *pConstAttributes;
      mo.m_object = pCurve;
      ::ON_CreateUuid(mo.m_attributes.m_uuid);
      pModel->m_object_table.Append(mo);
      return mo.m_attributes.m_uuid;
    }
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddTextDot(ONX_Model* pModel, const ON_TextDot* pConstDot, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel && pConstDot )
  {
    ON_TextDot* pDot = pConstDot->Duplicate();
    if( pDot )
    {
      ONX_Model_Object mo;
      if( pConstAttributes )
        mo.m_attributes = *pConstAttributes;
      mo.m_object = pDot;
      ::ON_CreateUuid(mo.m_attributes.m_uuid);
      pModel->m_object_table.Append(mo);
      return mo.m_attributes.m_uuid;
    }
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddText(ONX_Model* pModel, const RHMONO_STRING* _text, const ON_PLANE_STRUCT* plane, double height, const RHMONO_STRING* _fontName, int fontStyle, int justification, const ON_3dmObjectAttributes* pConstAttributes)
{
  INPUTSTRINGCOERCE(text, _text);
  INPUTSTRINGCOERCE(fontName, _fontName);
  if( pModel && plane && text && text[0]!=0 )
  {
    ON_Plane temp = FromPlaneStruct(*plane);
    if( temp.IsValid() )
    {
      if( height <= 0.0 )
        height = 1.0;
      bool bBold = (0!=(fontStyle&1));
      bool bItalic = (0!=(fontStyle&2));

      ON_wString font_str( fontName );
      font_str.TrimLeftAndRight();
      if( font_str.IsEmpty() )
        font_str = L"Arial";

      int font_index = -1;
      for( int i=0; i<pModel->m_font_table.Count(); i++ )
      {
        const ON_Font& fnt = pModel->m_font_table[i];
        if( fnt.IsItalic()==bItalic && fnt.IsBold()==bBold && font_str.Compare(fnt.m_facename)==0 )
        {
          font_index = i;
          break;
        }
      }
      if( -1==font_index )
      {
        // create a new font and add it to the font table
        ON_Font fnt;
        fnt.SetBold(bBold);
        fnt.SetIsItalic(bItalic);
        fnt.SetFontFaceName(font_str);
        ON_wString fontname(font_str);
        if(bBold)
          fontname += L" Bold";
        if(bItalic)
          fontname += L" Italic";
        fnt.SetFontName(fontname);
        fnt.SetFontIndex( pModel->m_font_table.Count() );
        pModel->m_font_table.Append(fnt);
        font_index = fnt.FontIndex();
      }
      

      ON_TextEntity2* text_entity = new ON_TextEntity2();
      text_entity->SetHeight( height );
      text_entity->SetTextValue( text );
      text_entity->SetTextFormula( 0 );
      text_entity->SetPlane( temp );
      text_entity->SetFontIndex( font_index );
      if( justification>0 )
        text_entity->SetJustification((unsigned int)justification);

      ONX_Model_Object mo;
      if( pConstAttributes )
        mo.m_attributes = *pConstAttributes;
      mo.m_object = text_entity;
      ::ON_CreateUuid(mo.m_attributes.m_uuid);
      pModel->m_object_table.Append(mo);
      return mo.m_attributes.m_uuid;
    }
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddSurface(ONX_Model* pModel, const ON_Surface* pConstSurface, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel && pConstSurface )
  {
    ON_Surface* pSurface = pConstSurface->Duplicate();
    if( pSurface )
    {
      ONX_Model_Object mo;
      if( pConstAttributes )
        mo.m_attributes = *pConstAttributes;
      mo.m_object = pSurface;
      ::ON_CreateUuid(mo.m_attributes.m_uuid);
      pModel->m_object_table.Append(mo);
      return mo.m_attributes.m_uuid;
    }
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddExtrusion(ONX_Model* pModel, const ON_Extrusion* pConstExtrusion, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel && pConstExtrusion )
  {
    ON_Surface* pExtrusion = pConstExtrusion->Duplicate();
    if( pExtrusion )
    {
      ONX_Model_Object mo;
      if( pConstAttributes )
        mo.m_attributes = *pConstAttributes;
      mo.m_object = pExtrusion;
      ::ON_CreateUuid(mo.m_attributes.m_uuid);
      pModel->m_object_table.Append(mo);
      return mo.m_attributes.m_uuid;
    }
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddMesh(ONX_Model* pModel, const ON_Mesh* pConstMesh, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel && pConstMesh )
  {
    ON_Mesh* pMesh = pConstMesh->Duplicate();
    if( pMesh )
    {
      ONX_Model_Object mo;
      if( pConstAttributes )
        mo.m_attributes = *pConstAttributes;
      mo.m_object = pMesh;
      ::ON_CreateUuid(mo.m_attributes.m_uuid);
      pModel->m_object_table.Append(mo);
      return mo.m_attributes.m_uuid;
    }
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddBrep(ONX_Model* pModel, const ON_Brep* pConstBrep, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel && pConstBrep )
  {
    ON_Brep* pBrep = pConstBrep->Duplicate();
    if( pBrep )
    {
      ONX_Model_Object mo;
      if( pConstAttributes )
        mo.m_attributes = *pConstAttributes;
      mo.m_object = pBrep;
      ::ON_CreateUuid(mo.m_attributes.m_uuid);
      pModel->m_object_table.Append(mo);
      return mo.m_attributes.m_uuid;
    }
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddLeader(ONX_Model* pModel, const RHMONO_STRING* _text, const ON_PLANE_STRUCT* plane, int count, /*ARRAY*/const ON_2dPoint* points2d, const ON_3dmObjectAttributes* pConstAttributes)
{
  INPUTSTRINGCOERCE(text, _text);
  if( pModel && plane && count>1 && points2d )
  {
    ON_Leader2* leader = new ON_Leader2();

    ON_Plane temp = FromPlaneStruct(*plane);
    leader->SetPlane(temp);
    for( int i=0; i<count; i++ )
      leader->m_points.Append(points2d[i]);

    leader->SetTextValue(text);

    ONX_Model_Object mo;
    if( pConstAttributes )
      mo.m_attributes = *pConstAttributes;
    mo.m_object = leader;
    ::ON_CreateUuid(mo.m_attributes.m_uuid);
    pModel->m_object_table.Append(mo);
    return mo.m_attributes.m_uuid;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddHatch(ONX_Model* pModel, const ON_Hatch* pConstHatch, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel && pConstHatch )
  {
    ON_Hatch* pHatch = pConstHatch->Duplicate();
    if( pHatch )
    {
      ONX_Model_Object mo;
      if( pConstAttributes )
        mo.m_attributes = *pConstAttributes;
      mo.m_object = pHatch;
      ::ON_CreateUuid(mo.m_attributes.m_uuid);
      pModel->m_object_table.Append(mo);
      return mo.m_attributes.m_uuid;
    }
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_ObjectTable_AddPolyLine(ONX_Model* pModel, int count, /*ARRAY*/const ON_3dPoint* points, const ON_3dmObjectAttributes* pConstAttributes)
{
  if( pModel && points && count>1)
  {
    CHack3dPointArray pl(count, (ON_3dPoint*)points);

    ON_PolylineCurve* pCurve = new ON_PolylineCurve(pl);
    ONX_Model_Object mo;
    if( pConstAttributes )
      mo.m_attributes = *pConstAttributes;
    mo.m_object = pCurve;
    ::ON_CreateUuid(mo.m_attributes.m_uuid);
    pModel->m_object_table.Append(mo);
    return mo.m_attributes.m_uuid;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION bool ONX_Model_ObjectTable_Delete(ONX_Model* pModel, ON_UUID object_id)
{
  bool rc = false;
  if( pModel )
  {
    for( int i=0; i<pModel->m_object_table.Count(); i++ )
    {
      if( pModel->m_object_table[i].m_attributes.m_uuid==object_id )
      {
        pModel->m_object_table.Remove(i);
        rc = true;
        break;
      }
    }
  }
  return rc;
}

RH_C_FUNCTION void ONX_Model_BoundingBox(const ONX_Model* pConstModel, ON_BoundingBox* pBBox)
{
  if( pConstModel && pBBox )
  {
    *pBBox = pConstModel->BoundingBox();
  }
}

RH_C_FUNCTION ON_Linetype* ONX_Model_GetLinetypePointer(ONX_Model* pModel, ON_UUID id)
{
  ON_Linetype* rc = NULL;
  if( pModel )
  {
    int count = pModel->m_linetype_table.Count();
    for( int i=0; i<count; i++ )
    {
      if( pModel->m_linetype_table[i].m_linetype_id == id )
      {
        rc = &(pModel->m_linetype_table[i]);
        break;
      }
    }
  }
  return rc;
}

RH_C_FUNCTION ON_Bitmap* ONX_Model_GetBitmapPointer(ONX_Model* pModel, int index)
{
  ON_Bitmap* rc = NULL;
  if( pModel && index >= 0 && index < pModel->m_bitmap_table.Count())
    rc = pModel->m_bitmap_table[index];
  return rc;
}

RH_C_FUNCTION ON_Layer* ONX_Model_GetLayerPointer(ONX_Model* pModel, ON_UUID id)
{
  ON_Layer* rc = NULL;
  if( pModel )
  {
    int count = pModel->m_layer_table.Count();
    for( int i=0; i<count; i++ )
    {
      if( pModel->m_layer_table[i].m_layer_id == id )
      {
        rc = &(pModel->m_layer_table[i]);
        break;
      }
    }
  }
  return rc;
}

RH_C_FUNCTION ON_DimStyle* ONX_Model_GetDimStylePointer(ONX_Model* pModel, ON_UUID id)
{
  ON_DimStyle* rc = NULL;
  if( pModel )
  {
    int count = pModel->m_dimstyle_table.Count();
    for( int i=0; i<count; i++ )
    {
      if( pModel->m_dimstyle_table[i].m_dimstyle_id == id )
      {
        rc = &(pModel->m_dimstyle_table[i]);
        break;
      }
    }
  }
  return rc;
}

RH_C_FUNCTION ON_HatchPattern* ONX_Model_GetHatchPatternPointer(ONX_Model* pModel, ON_UUID id)
{
  ON_HatchPattern* rc = NULL;
  if( pModel )
  {
    int count = pModel->m_hatch_pattern_table.Count();
    for( int i=0; i<count; i++ )
    {
      if( pModel->m_hatch_pattern_table[i].m_hatchpattern_id == id )
      {
        rc = &(pModel->m_hatch_pattern_table[i]);
        break;
      }
    }
  }
  return rc;
}

RH_C_FUNCTION ON_Material* ONX_Model_GetMaterialPointer(ONX_Model* pModel, ON_UUID id)
{
  ON_Material* rc = NULL;
  if( pModel )
  {
    int count = pModel->m_material_table.Count();
    for( int i=0; i<count; i++ )
    {
      if( pModel->m_material_table[i].m_material_id == id )
      {
        rc = &(pModel->m_material_table[i]);
        break;
      }
    }
  }
  return rc;
}

RH_C_FUNCTION void ONX_Model_LinetypeTable_Insert(ONX_Model* pModel, const ON_Linetype* pConstLinetype, int index)
{
  if( pModel && pConstLinetype && index>=0)
  {
    pModel->m_linetype_table.Insert(index, *pConstLinetype);
    // update indices
    for( int i=index; i<pModel->m_linetype_table.Count(); i++ )
      pModel->m_linetype_table[i].m_linetype_index = i;
  }
}

RH_C_FUNCTION void ONX_Model_LayerTable_Insert(ONX_Model* pModel, const ON_Layer* pConstLayer, int index)
{
  if( pModel && pConstLayer && index>=0)
  {
    pModel->m_layer_table.Insert(index, *pConstLayer);
    // update layer indices
    for( int i=index; i<pModel->m_layer_table.Count(); i++ )
      pModel->m_layer_table[i].m_layer_index = i;
  }
}

RH_C_FUNCTION void ONX_Model_DimStyleTable_Insert(ONX_Model* pModel, const ON_DimStyle* pConstDimStyle, int index)
{
  if( pModel && pConstDimStyle && index>=0)
  {
    pModel->m_dimstyle_table.Insert(index, *pConstDimStyle);
    // update indices
    for( int i=index; i<pModel->m_dimstyle_table.Count(); i++ )
      pModel->m_dimstyle_table[i].m_dimstyle_index = i;
  }
}

RH_C_FUNCTION void ONX_Model_HatchPatternTable_Insert(ONX_Model* pModel, const ON_HatchPattern* pConstHatchPattern, int index)
{
  if( pModel && pConstHatchPattern && index>=0)
  {
    pModel->m_hatch_pattern_table.Insert(index, *pConstHatchPattern);
    // update indices
    for( int i=index; i<pModel->m_hatch_pattern_table.Count(); i++ )
      pModel->m_hatch_pattern_table[i].m_hatchpattern_index = i;
  }
}

RH_C_FUNCTION void ONX_Model_InstanceDefinitionTable_Insert(ONX_Model* pModel, const ON_InstanceDefinition* pConstInstanceDefinition, int index)
{
  if( pModel && pConstInstanceDefinition && index>=0 )
  {
    pModel->m_idef_table.Insert(index, *pConstInstanceDefinition);
  }
}

RH_C_FUNCTION void ONX_Model_MaterialTable_Insert(ONX_Model* pModel, const ON_Material* pConstMaterial, int index)
{
  if( pModel && pConstMaterial && index>=0)
  {
    pModel->m_material_table.Insert(index, *pConstMaterial);
    // update indices
    for( int i=index; i<pModel->m_material_table.Count(); i++ )
      pModel->m_material_table[i].m_material_index = i;
  }
}

RH_C_FUNCTION void ONX_Model_LinetypeTable_RemoveAt(ONX_Model* pModel, int index)
{
  if( pModel && index>=0)
  {
    pModel->m_linetype_table.Remove(index);
    // update indices
    for( int i=index; i<pModel->m_linetype_table.Count(); i++ )
      pModel->m_linetype_table[i].m_linetype_index = i;
  }
}

RH_C_FUNCTION void ONX_Model_LayerTable_RemoveAt(ONX_Model* pModel, int index)
{
  if( pModel && index>=0)
  {
    pModel->m_layer_table.Remove(index);
    // update indices
    for( int i=index; i<pModel->m_layer_table.Count(); i++ )
      pModel->m_layer_table[i].m_layer_index = i;
  }
}

RH_C_FUNCTION void ONX_Model_DimStyleTable_RemoveAt(ONX_Model* pModel, int index)
{
  if( pModel && index>=0)
  {
    pModel->m_dimstyle_table.Remove(index);
    // update indices
    for( int i=index; i<pModel->m_dimstyle_table.Count(); i++ )
      pModel->m_dimstyle_table[i].m_dimstyle_index = i;
  }
}

RH_C_FUNCTION void ONX_Model_HatchPatternTable_RemoveAt(ONX_Model* pModel, int index)
{
  if( pModel && index>=0)
  {
    pModel->m_hatch_pattern_table.Remove(index);
    // update indices
    for( int i=index; i<pModel->m_hatch_pattern_table.Count(); i++ )
      pModel->m_hatch_pattern_table[i].m_hatchpattern_index = i;
  }
}

RH_C_FUNCTION void ONX_Model_InstanceDefinitionTable_RemoveAt(ONX_Model* pModel, int index)
{
  if( pModel && index>=0)
  {
    pModel->m_idef_table.Remove(index);
  }
}

RH_C_FUNCTION void ONX_Model_MaterialTable_RemoveAt(ONX_Model* pModel, int index)
{
  if( pModel && index>=0)
  {
    pModel->m_material_table.Remove(index);
    // update indices
    for( int i=index; i<pModel->m_material_table.Count(); i++ )
      pModel->m_material_table[i].m_material_index = i;
  }
}

RH_C_FUNCTION ON_UUID ONX_Model_LinetypeTable_Id(const ONX_Model* pConstModel, int index)
{
  if( pConstModel && index>=0 && index<pConstModel->m_linetype_table.Count())
  {
    return pConstModel->m_linetype_table[index].m_linetype_id;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_LayerTable_Id(const ONX_Model* pConstModel, int index)
{
  if( pConstModel && index>=0 && index<pConstModel->m_layer_table.Count())
  {
    return pConstModel->m_layer_table[index].m_layer_id;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_DimStyleTable_Id(const ONX_Model* pConstModel, int index)
{
  if( pConstModel && index>=0 && index<pConstModel->m_dimstyle_table.Count())
  {
    return pConstModel->m_dimstyle_table[index].m_dimstyle_id;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_HatchPatternTable_Id(const ONX_Model* pConstModel, int index)
{
  if( pConstModel && index>=0 && index<pConstModel->m_hatch_pattern_table.Count())
  {
    return pConstModel->m_hatch_pattern_table[index].m_hatchpattern_id;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION ON_UUID ONX_Model_InstanceDefinitionTable_Id(const ONX_Model* pConstModel, int index)
{
  if( pConstModel && index>=0 && index<pConstModel->m_idef_table.Count())
  {
    return pConstModel->m_idef_table[index].m_uuid;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION int ONX_Model_InstanceDefinitionTable_Index(const ONX_Model* pConstModel, ON_UUID id)
{
  if( pConstModel )
  {
    for( int i=0; i<pConstModel->m_idef_table.Count(); i++ )
    {
      if( id == pConstModel->m_idef_table[i].m_uuid )
        return i;
    }
  }
  return -1;
}

RH_C_FUNCTION const ON_InstanceDefinition* ONX_Model_GetInstanceDefinitionPointer(const ONX_Model* pConstModel, ON_UUID id)
{
  if( pConstModel )
  {
    for( int i=0; i<pConstModel->m_idef_table.Count(); i++ )
    {
      const ON_InstanceDefinition* pIdef = pConstModel->m_idef_table.At(i);
      if( pIdef && pIdef->m_uuid==id )
        return pIdef;
    }
  }
  return NULL;
}

RH_C_FUNCTION ON_UUID ONX_Model_MaterialTable_Id(const ONX_Model* pConstModel, int index)
{
  if( pConstModel && index>=0 && index<pConstModel->m_material_table.Count())
  {
    return pConstModel->m_material_table[index].m_material_id;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION void ONX_Model_TableClear(ONX_Model* pModel, enum ONXModelTable which_table)
{
  if( pModel )
  {
    pModel->m_layer_table.Empty();
    switch(which_table)
    {
    case onxBitmapTable:
      {
        for( int i=0; i<pModel->m_bitmap_table.Count(); i++ )
        {
          ON_Bitmap* pBitmap = pModel->m_bitmap_table[i];
          if( pBitmap )
            delete pBitmap;
          pModel->m_bitmap_table[i] = NULL;
        }
        pModel->m_bitmap_table.Empty();
      }
      break;
    case onxTextureMappingTable:
      pModel->m_mapping_table.Empty();
      break;
    case onxMaterialTable:
      pModel->m_material_table.Empty();
      break;
    case onxLinetypeTable:
      pModel->m_linetype_table.Empty();
      break;
    case onxLayerTable:
      pModel->m_layer_table.Empty();
      break;
    case onxLightTable:
      pModel->m_light_table.Empty();
      break;
    case onxGroupTable:
      pModel->m_group_table.Empty();
      break;
    case onxFontTable:
      pModel->m_font_table.Empty();
      break;
    case onxDimStyleTable:
      pModel->m_dimstyle_table.Empty();
      break;
    case onxHatchPatternTable:
      pModel->m_hatch_pattern_table.Empty();
      break;
    case onxIDefTable:
      pModel->m_idef_table.Empty();
      break;
    case onxObjectTable:
      pModel->m_object_table.Empty();
      break;
    case onxHistoryRecordTable:
      {
        for( int i=0; i<pModel->m_history_record_table.Count(); i++ )
        {
          ON_HistoryRecord* pRecord = pModel->m_history_record_table[i];
          if( pRecord )
            delete pRecord;
          pModel->m_history_record_table[i] = NULL;
        }
        pModel->m_history_record_table.Empty();
      }
      break;
    case onxUserDataTable:
      pModel->m_userdata_table.Empty();
      break;
    }
  }
}


RH_C_FUNCTION void ONX_Model_GetString( const ONX_Model* pConstModel, int which, CRhCmnStringHolder* pString )
{
  const int idxApplicationName = 0;
  const int idxApplicationUrl = 1;
  const int idxApplicationDetails = 2;
  const int idxCreatedBy = 3;
  const int idxLastCreatedBy = 4;

  if( pConstModel && pString )
  {
    switch(which)
    {
    case idxApplicationName:
      pString->Set( pConstModel->m_properties.m_Application.m_application_name );
      break;
    case idxApplicationUrl:
      pString->Set( pConstModel->m_properties.m_Application.m_application_URL );
      break;
    case idxApplicationDetails:
      pString->Set( pConstModel->m_properties.m_Application.m_application_details );
      break;
    case idxCreatedBy:
      pString->Set( pConstModel->m_properties.m_RevisionHistory.m_sCreatedBy );
      break;
    case idxLastCreatedBy:
      pString->Set( pConstModel->m_properties.m_RevisionHistory.m_sLastEditedBy );
      break;
    }
  }
}

RH_C_FUNCTION void ONX_Model_SetString( ONX_Model* pModel, int which, const RHMONO_STRING* str )
{
  const int idxApplicationName = 0;
  const int idxApplicationUrl = 1;
  const int idxApplicationDetails = 2;
  const int idxCreatedBy = 3;
  const int idxLastCreatedBy = 4;
  INPUTSTRINGCOERCE(_str, str);

  if( pModel )
  {
    switch(which)
    {
    case idxApplicationName:
      pModel->m_properties.m_Application.m_application_name = _str;
      break;
    case idxApplicationUrl:
      pModel->m_properties.m_Application.m_application_URL = _str;
      break;
    case idxApplicationDetails:
      pModel->m_properties.m_Application.m_application_details = _str;
      break;
    case idxCreatedBy:
      pModel->m_properties.m_RevisionHistory.m_sCreatedBy = _str;
      break;
    case idxLastCreatedBy:
      pModel->m_properties.m_RevisionHistory.m_sLastEditedBy = _str;
      break;
    }
  }
}

RH_C_FUNCTION int ONX_Model_GetRevision(const ONX_Model* pConstModel)
{
  int rc = 0;
  if( pConstModel )
    rc = pConstModel->m_properties.m_RevisionHistory.m_revision_count;
  return rc;
}

RH_C_FUNCTION void ONX_Model_SetRevision(ONX_Model* pModel, int rev)
{
  if( pModel )
    pModel->m_properties.m_RevisionHistory.m_revision_count = rev;
}

RH_C_FUNCTION ON_3dmSettings* ONX_Model_3dmSettingsPointer(ONX_Model* pModel)
{
  ON_3dmSettings* rc = NULL;
  if( pModel )
    rc = &(pModel->m_settings);
  return rc;
}

RH_C_FUNCTION ON_3dmView* ONX_Model_ViewPointer(ONX_Model* pModel, ON_UUID id, const ON_3dmView* pConstView, bool named_view_table)
{
  ON_3dmView* rc = NULL;
  if( pModel )
  {
    ON_ClassArray<ON_3dmView>* views = named_view_table ? &(pModel->m_settings.m_named_views) : &(pModel->m_settings.m_views);
    if( views )
    {
      for( int i=0; i<views->Count(); i++ )
      {
        ON_3dmView* pView = views->At(i);
        if( pView && pView->m_vp.ViewportId() == id )
        {
          if( ::ON_UuidIsNil(id) && pConstView!=pView )
            continue;
          rc = pView;
          break;
        }
      }
    }
  }
  return rc;
}

RH_C_FUNCTION ON_3dmView* ONX_Model_ViewTable_Pointer(ONX_Model* pModel, int index, bool named_view_table)
{
  ON_3dmView* pView = NULL;
  if( pModel )
  {
    pView = named_view_table ? pModel->m_settings.m_named_views.At(index) :
                               pModel->m_settings.m_views.At(index);
  }
  return pView;
}

RH_C_FUNCTION ON_UUID ONX_Model_ViewTable_Id(const ONX_Model* pConstModel, int index, bool named_view_table)
{
  if( pConstModel )
  {
    const ON_3dmView* pView = named_view_table ? pConstModel->m_settings.m_named_views.At(index) :
                                                 pConstModel->m_settings.m_views.At(index);
    if( pView )
      return pView->m_vp.ViewportId();
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION void ONX_Model_ViewTable_Clear(ONX_Model* pModel, bool named_view_table)
{
  if( pModel )
  {
    if( named_view_table )
      pModel->m_settings.m_named_views.Empty();
    else
      pModel->m_settings.m_views.Empty();
  }
}

RH_C_FUNCTION int ONX_Model_ViewTable_Index(const ONX_Model* pConstModel, const ON_3dmView* pConstView, bool named_view_table)
{
  int rc = -1;
  if( pConstModel && pConstView )
  {
    const ON_ClassArray<ON_3dmView>* views = named_view_table ? &(pConstModel->m_settings.m_named_views) : &(pConstModel->m_settings.m_views);
    if( views )
    {
      for( int i=0; i<views->Count(); i++ )
      {
        if( views->At(i) == pConstView )
        {
          rc = i;
          break;
        }
      }
    }
  }
  return rc;
}

RH_C_FUNCTION void ONX_Model_ViewTable_Insert(ONX_Model* pModel, const ON_3dmView* pConstView, int index, bool named_view_table)
{
  if( pModel && pConstView && index>=0)
  {
    ON_ClassArray<ON_3dmView>* views = named_view_table ? &(pModel->m_settings.m_named_views) : &(pModel->m_settings.m_views);
    if( views )
    {
      views->Insert(index, *pConstView);
    }
  }
}

RH_C_FUNCTION void ONX_Model_ViewTable_RemoveAt(ONX_Model* pModel, int index, bool named_view_table)
{
  if( pModel && index>=0)
  {
    ON_ClassArray<ON_3dmView>* views = named_view_table ? &(pModel->m_settings.m_named_views) : &(pModel->m_settings.m_views);
    if( views )
      views->Remove(index);
  }
}

RH_C_FUNCTION ON_UUID ONX_Model_UserDataTable_Uuid(const ONX_Model* pConstModel, int index)
{
  if( pConstModel )
  {
    const ONX_Model_UserData* pUD = pConstModel->m_userdata_table.At(index);
    if( pUD )
      return pUD->m_uuid;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION void ONX_Model_UserDataTable_Clear(ONX_Model* pModel)
{
  if( pModel )
    pModel->m_userdata_table.Empty();
}

#if !defined(OPENNURBS_BUILD)
RH_C_FUNCTION bool ONX_Model_ReadPreviewImage(const RHMONO_STRING* path, CRhinoDib* pRhinoDib)
{
  bool rc = false;
  INPUTSTRINGCOERCE(_path, path);
  if( NULL==pRhinoDib )
    return false;

  FILE* fp = ON::OpenFile( _path, L"rb" );
  if( fp )
  {
    ON_BinaryFile file( ON::read3dm, fp);
    int version = 0;
    ON_String comments;
    if( file.Read3dmStartSection( &version, comments ) )
    {
      ON_3dmProperties prop;
      if( file.Read3dmProperties(prop) )
      {
        BITMAPINFO* pBMI = prop.m_PreviewImage.m_bmi;
        if( pBMI )
        {
          pRhinoDib->SetDib(pBMI, false);
          rc = true;
        }
      }
    }
    ON::CloseFile(fp);
  }
  return rc;
}
#endif

class CBinaryFileHelper : public ON_BinaryFile
{
public:
  CBinaryFileHelper(ON::archive_mode mode, FILE* fp)
  : ON_BinaryFile(mode, fp)
  {
    m_file_pointer = fp;
  }

  FILE* m_file_pointer;
};

RH_C_FUNCTION CBinaryFileHelper* ON_BinaryFile_Open(const RHMONO_STRING* path, int mode)
{
  // 22-Jan-2014 Dale Fugier, http://mcneel.myjetbrains.com/youtrack/issue/RH-23765

  ON::archive_mode archive_mode = ON::ArchiveMode(mode);
  if( archive_mode == ON::unknown_archive_mode )
    return NULL;

  INPUTSTRINGCOERCE(_path, path);

  FILE* fp = 0;
  if( archive_mode == ON::read || archive_mode == ON::read3dm )
    fp = ON::OpenFile( _path, L"rb" );
  else if( archive_mode == ON::write || archive_mode == ON::write3dm )
    fp = ON::OpenFile( _path, L"wb" );
  else
    fp = ON::OpenFile( _path, L"r+b" );
  
  if( fp )
    return new CBinaryFileHelper( archive_mode, fp );

  return NULL;
}

RH_C_FUNCTION void ON_BinaryFile_Close(CBinaryFileHelper* pBinaryFile)
{
  if( pBinaryFile )
  {
    if( pBinaryFile->m_file_pointer )
      ON::CloseFile(pBinaryFile->m_file_pointer);
    delete pBinaryFile;
  }
}





class ONX_Model_WithFilter : public ONX_Model
{
public:
  bool FilteredRead( ON_BinaryArchive& archive, unsigned int table_filter, unsigned int model_object_type_filter, ON_TextLog* error_log );

  bool FilteredRead( const wchar_t* filename, unsigned int table_filter, unsigned int model_object_type_filter, ON_TextLog* error_log );
};

static 
bool CheckForCRCErrors( 
          ON_BinaryArchive& archive, 
          ONX_Model& model,
          ON_TextLog* error_log,
          const char* sSection
          )
{
  // returns true if new CRC errors are found
  bool rc = false;
  int new_crc_count = archive.BadCRCCount();
  
  if ( model.m_crc_error_count != new_crc_count ) 
  {
    if ( error_log )
    {
      error_log->Print("ERROR: Corrupt %s. (CRC errors).\n",sSection);
      error_log->Print("-- Attempting to continue.\n");
    }
    model.m_crc_error_count = new_crc_count;
    rc = true;
  }

  return rc;
}


bool ONX_Model_WithFilter::FilteredRead(ON_BinaryArchive& archive, unsigned int table_filter, unsigned int model_object_type_filter, ON_TextLog* error_log )
{
  const int max_error_count = 2000;
  int error_count = 0;
  bool return_code = true;
  int count, rc;

  Destroy(); // get rid of any residual stuff

  // STEP 1: REQUIRED - Read start section
  if ( !archive.Read3dmStartSection( &m_3dm_file_version, m_sStartSectionComments ) )
  {
    if ( error_log) error_log->Print("ERROR: Unable to read start section. (ON_BinaryArchive::Read3dmStartSection() returned false.)\n");
    return false;
  }
  else if ( CheckForCRCErrors( archive, *this, error_log, "start section" ) )
    return_code = false;

  // STEP 2: REQUIRED - Read properties section
  if ( !archive.Read3dmProperties( m_properties ) )
  {
    if ( error_log) error_log->Print("ERROR: Unable to read properties section. (ON_BinaryArchive::Read3dmProperties() returned false.)\n");
    return false;
  }
  else if ( CheckForCRCErrors( archive, *this, error_log, "properties section" ) )
    return_code = false;

  // version of opennurbs used to write the file.
  m_3dm_opennurbs_version = archive.ArchiveOpenNURBSVersion();

  // STEP 3: REQUIRED - Read properties section
  if ( !archive.Read3dmSettings( m_settings ) )
  {
    if ( error_log) error_log->Print("ERROR: Unable to read settings section. (ON_BinaryArchive::Read3dmSettings() returned false.)\n");
    return false;
  }
  else if ( CheckForCRCErrors( archive, *this, error_log, "settings section" ) )
    return_code = false;

  // STEP 4: REQUIRED - Read embedded bitmap table
  if ( archive.BeginRead3dmBitmapTable() )
  {
    // At the moment no bitmaps are embedded so this table is empty
    ON_Bitmap* pBitmap = NULL;
    for( count = 0; true; count++ ) 
    {
      pBitmap = NULL;
      rc = archive.Read3dmBitmap(&pBitmap);
      if ( rc==0 )
        break; // end of bitmap table
      if ( rc < 0 ) 
      {
        if ( error_log) 
        {
          error_log->Print("ERROR: Corrupt bitmap found. (ON_BinaryArchive::Read3dmBitmap() < 0.)\n");
          error_count++;
          if ( error_count > max_error_count )
            return false;
          error_log->Print("-- Attempting to continue.\n");
        }
        return_code = false;
      }
      m_bitmap_table.Append(pBitmap);
    }

    // If BeginRead3dmBitmapTable() returns true, 
    // then you MUST call EndRead3dmBitmapTable().
    if ( !archive.EndRead3dmBitmapTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt bitmap table. (ON_BinaryArchive::EndRead3dmBitmapTable() returned false.)\n");
      return false;
    }
    if ( CheckForCRCErrors( archive, *this, error_log, "bitmap table" ) )
      return_code = false;
  }
  else     
  {
    if ( error_log) 
    {
      error_log->Print("WARNING: Missing or corrupt bitmap table. (ON_BinaryArchive::BeginRead3dmBitmapTable() returned false.)\n");
      error_log->Print("-- Attempting to continue.\n");
    }
    return_code = false;
  }



  // STEP 5: REQUIRED - Read texture mapping table
  if ( archive.BeginRead3dmTextureMappingTable() )
  {
    ON_TextureMapping* pTextureMapping = NULL;
    for( count = 0; true; count++ ) 
    {
      rc = archive.Read3dmTextureMapping(&pTextureMapping);
      if ( rc==0 )
        break; // end of texture_mapping table
      if ( rc < 0 ) 
      {
        if ( error_log) 
        {
          error_log->Print("ERROR: Corrupt render texture_mapping found. (ON_BinaryArchive::Read3dmTextureMapping() < 0.)\n");
          error_count++;
          if ( error_count > max_error_count )
            return false;
          error_log->Print("-- Attempting to continue.\n");
        }
        continue;
      }
      ON_UserDataHolder ud;
      ud.MoveUserDataFrom(*pTextureMapping);
      m_mapping_table.Append(*pTextureMapping);
      pTextureMapping->m_mapping_index = count;
      ud.MoveUserDataTo(*m_mapping_table.Last(),false);
      delete pTextureMapping;
      pTextureMapping = NULL;
    }
    
    // If BeginRead3dmTextureMappingTable() returns true, 
    // then you MUST call EndRead3dmTextureMappingTable().
    if ( !archive.EndRead3dmTextureMappingTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt render texture_mapping table. (ON_BinaryArchive::EndRead3dmTextureMappingTable() returned false.)\n");
      return false;
    }
    if ( CheckForCRCErrors( archive, *this, error_log, "render texture_mapping table" ) )
      return_code = false;
  }
  else     
  {
    if ( error_log)
    {
      error_log->Print("WARNING: Missing or corrupt render texture_mapping table. (ON_BinaryArchive::BeginRead3dmTextureMappingTable() returned false.)\n");
      error_log->Print("-- Attempting to continue.\n");
    }
    return_code = false;
  }


  // STEP 6: REQUIRED - Read render material table
  if ( archive.BeginRead3dmMaterialTable() )
  {
    ON_Material* pMaterial = NULL;
    for( count = 0; true; count++ ) 
    {
      rc = archive.Read3dmMaterial(&pMaterial);
      if ( rc==0 )
        break; // end of material table
      if ( rc < 0 ) 
      {
        if ( error_log) 
        {
          error_log->Print("ERROR: Corrupt render material found. (ON_BinaryArchive::Read3dmMaterial() < 0.)\n");
          error_count++;
          if ( error_count > max_error_count )
            return false;
          error_log->Print("-- Attempting to continue.\n");
        }
        pMaterial = new ON_Material; // use default
        pMaterial->m_material_index = count;
      }
      ON_UserDataHolder ud;
      ud.MoveUserDataFrom(*pMaterial);
      m_material_table.Append(*pMaterial);
      ud.MoveUserDataTo(*m_material_table.Last(),false);
      delete pMaterial;
      pMaterial = NULL;
    }
    
    // If BeginRead3dmMaterialTable() returns true, 
    // then you MUST call EndRead3dmMaterialTable().
    if ( !archive.EndRead3dmMaterialTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt render material table. (ON_BinaryArchive::EndRead3dmMaterialTable() returned false.)\n");
      return false;
    }
    if ( CheckForCRCErrors( archive, *this, error_log, "render material table" ) )
      return_code = false;
  }
  else     
  {
    if ( error_log)
    {
      error_log->Print("WARNING: Missing or corrupt render material table. (ON_BinaryArchive::BeginRead3dmMaterialTable() returned false.)\n");
      error_log->Print("-- Attempting to continue.\n");
    }
    return_code = false;
  }


  // STEP 7: REQUIRED - Read line type table
  if ( archive.BeginRead3dmLinetypeTable() )
  {
    ON_Linetype* pLinetype = NULL;
    for( count = 0; true; count++ ) 
    {
      rc = archive.Read3dmLinetype(&pLinetype);
      if ( rc==0 )
        break; // end of linetype table
      if ( rc < 0 ) 
      {
        if ( error_log) 
        {
          error_log->Print("ERROR: Corrupt render linetype found. (ON_BinaryArchive::Read3dmLinetype() < 0.)\n");
          error_count++;
          if ( error_count > max_error_count )
            return false;
          error_log->Print("-- Attempting to continue.\n");
        }
        pLinetype = new ON_Linetype; // use default
        pLinetype->m_linetype_index = count;
      }
      ON_UserDataHolder ud;
      ud.MoveUserDataFrom(*pLinetype);
      m_linetype_table.Append(*pLinetype);
      ud.MoveUserDataTo(*m_linetype_table.Last(),false);
      delete pLinetype;
      pLinetype = NULL;
    }
    
    // If BeginRead3dmLinetypeTable() returns true, 
    // then you MUST call EndRead3dmLinetypeTable().
    if ( !archive.EndRead3dmLinetypeTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt render linetype table. (ON_BinaryArchive::EndRead3dmLinetypeTable() returned false.)\n");
      return false;
    }
    if ( CheckForCRCErrors( archive, *this, error_log, "render linetype table" ) )
      return_code = false;
  }
  else     
  {
    if ( error_log)
    {
      error_log->Print("WARNING: Missing or corrupt render linetype table. (ON_BinaryArchive::BeginRead3dmLinetypeTable() returned false.)\n");
      error_log->Print("-- Attempting to continue.\n");
    }
    return_code = false;
  }

  // STEP 8: REQUIRED - Read layer table
  if ( archive.BeginRead3dmLayerTable() )
  {
    ON_Layer* pLayer = NULL;
    for( count = 0; true; count++ ) 
    {
      pLayer = NULL;
      rc = archive.Read3dmLayer(&pLayer);
      if ( rc==0 )
        break; // end of layer table
      if ( rc < 0 ) 
      {
        if ( error_log)
        {
          error_log->Print("ERROR: Corrupt layer found. (ON_BinaryArchive::Read3dmLayer() < 0.)\n");
          error_count++;
          if ( error_count > max_error_count )
            return false;
          error_log->Print("-- Attempting to continue.\n");
        }
        pLayer = new ON_Layer; // use default
        pLayer->m_layer_index = count;
      }
      ON_UserDataHolder ud;
      ud.MoveUserDataFrom(*pLayer);
      m_layer_table.Append(*pLayer);
      ud.MoveUserDataTo(*m_layer_table.Last(),false);
      delete pLayer;
      pLayer = NULL;
    }
    
    // If BeginRead3dmLayerTable() returns true, 
    // then you MUST call EndRead3dmLayerTable().
    if ( !archive.EndRead3dmLayerTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt render layer table. (ON_BinaryArchive::EndRead3dmLayerTable() returned false.)\n");
      return false;
    }
    if ( CheckForCRCErrors( archive, *this, error_log, "layer table" ) )
      return_code = false;
  }
  else     
  {
    if ( error_log) 
    {
      error_log->Print("WARNING: Missing or corrupt layer table. (ON_BinaryArchive::BeginRead3dmLayerTable() returned false.)\n");
      error_log->Print("-- Attempting to continue.\n");
    }
    return_code = false;
  }

  // STEP 9: REQUIRED - Read group table
  if ( archive.BeginRead3dmGroupTable() )
  {
    ON_Group* pGroup = NULL;
    for( count = 0; true; count++ ) 
    {
      rc = archive.Read3dmGroup(&pGroup);
      if ( rc==0 )
        break; // end of group table
      if ( rc < 0 ) 
      {
        if ( error_log)
        {
          error_log->Print("ERROR: Corrupt group found. (ON_BinaryArchive::Read3dmGroup() < 0.)\n");
          error_count++;
          if ( error_count > max_error_count )
            return false;
          error_log->Print("-- Attempting to continue.\n");
        }
        pGroup = new ON_Group; // use default
        pGroup->m_group_index = -1;
      }
      ON_UserDataHolder ud;
      ud.MoveUserDataFrom(*pGroup);
      m_group_table.Append(*pGroup);
      ud.MoveUserDataTo(*m_group_table.Last(),false);
      delete pGroup;
      pGroup = NULL;
    }
    
    // If BeginRead3dmGroupTable() returns true, 
    // then you MUST call EndRead3dmGroupTable().
    if ( !archive.EndRead3dmGroupTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt group table. (ON_BinaryArchive::EndRead3dmGroupTable() returned false.)\n");
      return false;
    }
    if ( CheckForCRCErrors( archive, *this, error_log, "group table" ) )
      return_code = false;
  }
  else     
  {
    if ( error_log) 
    {
      error_log->Print("WARNING: Missing or corrupt group table. (ON_BinaryArchive::BeginRead3dmGroupTable() returned false.)\n");
      error_log->Print("-- Attempting to continue.\n");
    }
    return_code = false;
  }

  // STEP 10: REQUIRED - Read font table
  if ( archive.BeginRead3dmFontTable() )
  {
    ON_Font* pFont = NULL;
    for( count = 0; true; count++ ) 
    {
      rc = archive.Read3dmFont(&pFont);
      if ( rc==0 )
        break; // end of font table
      if ( rc < 0 ) 
      {
        if ( error_log)
        {
          error_log->Print("ERROR: Corrupt font found. (ON_BinaryArchive::Read3dmFont() < 0.)\n");
          error_count++;
          if ( error_count > max_error_count )
            return false;
          error_log->Print("-- Attempting to continue.\n");
        }
        pFont = new ON_Font; // use default
        pFont->m_font_index = -1;
      }
      ON_UserDataHolder ud;
      ud.MoveUserDataFrom(*pFont);
      m_font_table.Append(*pFont);
      ud.MoveUserDataTo(*m_font_table.Last(),false);
      delete pFont;
      pFont = NULL;
    }
    
    // If BeginRead3dmFontTable() returns true, 
    // then you MUST call EndRead3dmFontTable().
    if ( !archive.EndRead3dmFontTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt font table. (ON_BinaryArchive::EndRead3dmFontTable() returned false.)\n");
      return false;
    }
    if ( CheckForCRCErrors( archive, *this, error_log, "font table" ) )
      return_code = false;
  }
  else     
  {
    if ( error_log)
    {
      error_log->Print("WARNING: Missing or corrupt font table. (ON_BinaryArchive::BeginRead3dmFontTable() returned false.)\n");
      error_log->Print("-- Attempting to continue.\n");
    }
    return_code = false;
  }

  // STEP 11: REQUIRED - Read dimstyle table
  if ( archive.BeginRead3dmDimStyleTable() )
  {
    ON_DimStyle* pDimStyle = NULL;
    for( count = 0; true; count++ ) 
    {
      rc = archive.Read3dmDimStyle(&pDimStyle);
      if ( rc==0 )
        break; // end of dimstyle table
      if ( rc < 0 ) 
      {
        if ( error_log)
        {
          error_log->Print("ERROR: Corrupt dimstyle found. (ON_BinaryArchive::Read3dmDimStyle() < 0.)\n");
          error_count++;
          if ( error_count > max_error_count )
            return false;
          error_log->Print("-- Attempting to continue.\n");
        }
        pDimStyle = new ON_DimStyle; // use default
        pDimStyle->m_dimstyle_index = count;
      }
      ON_UserDataHolder ud;
      ud.MoveUserDataFrom(*pDimStyle);
      m_dimstyle_table.Append(*pDimStyle);
      ud.MoveUserDataTo(*m_dimstyle_table.Last(),false);
      delete pDimStyle;
      pDimStyle = NULL;
    }
    
    // If BeginRead3dmDimStyleTable() returns true, 
    // then you MUST call EndRead3dmDimStyleTable().
    if ( !archive.EndRead3dmDimStyleTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt dimstyle table. (ON_BinaryArchive::EndRead3dmDimStyleTable() returned false.)\n");
      return false;
    }
    if ( CheckForCRCErrors( archive, *this, error_log, "dimstyle table" ) )
      return_code = false;
  }
  else     
  {
    if ( error_log)
    {
      error_log->Print("WARNING: Missing or corrupt dimstyle table. (ON_BinaryArchive::BeginRead3dmDimStyleTable() returned false.)\n");
      error_log->Print("-- Attempting to continue.\n");
    }
    return_code = false;
  }

  // STEP 12: REQUIRED - Read render lights table
  if ( archive.BeginRead3dmLightTable() )
  {
    ON_Light* pLight = NULL;
    ON_3dmObjectAttributes object_attributes;
    for( count = 0; true; count++ ) 
    {
      object_attributes.Default();
      rc = archive.Read3dmLight(&pLight,&object_attributes);
      if ( rc==0 )
        break; // end of light table
      if ( rc < 0 ) 
      {
        if ( error_log)
        {
          error_log->Print("ERROR: Corrupt render light found. (ON_BinaryArchive::Read3dmLight() < 0.)\n");
          error_count++;
          if ( error_count > max_error_count )
            return false;
          error_log->Print("-- Attempting to continue.\n");
        }
        continue;
      }
      ONX_Model_RenderLight& light = m_light_table.AppendNew();
      ON_UserDataHolder ud;
      ud.MoveUserDataFrom(*pLight);
      light.m_light = *pLight;
      ud.MoveUserDataTo(light.m_light,false);
      light.m_attributes = object_attributes;
      delete pLight;
      pLight = NULL;
    }
    
    // If BeginRead3dmLightTable() returns true, 
    // then you MUST call EndRead3dmLightTable().
    if ( !archive.EndRead3dmLightTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt render light table. (ON_BinaryArchive::EndRead3dmLightTable() returned false.)\n");
      return false;
    }
    if ( CheckForCRCErrors( archive, *this, error_log, "render light table" ) )
      return_code = false;
  }
  else     
  {
    if ( error_log)
    {
      error_log->Print("WARNING: Missing or corrupt render light table. (ON_BinaryArchive::BeginRead3dmLightTable() returned false.)\n");
      error_log->Print("-- Attempting to continue.\n");
    }
    return_code = false;
  }

  // STEP 13 - read hatch pattern table
  if ( archive.BeginRead3dmHatchPatternTable() )
  {
    ON_HatchPattern* pHatchPattern = NULL;
    for( count = 0; true; count++ ) 
    {
      rc = archive.Read3dmHatchPattern(&pHatchPattern);
      if ( rc==0 )
        break; // end of hatchpattern table
      if ( rc < 0 ) 
      {
        if ( error_log)
        {
          error_log->Print("ERROR: Corrupt hatchpattern found. (ON_BinaryArchive::Read3dmHatchPattern() < 0.)\n");
          error_count++;
          if ( error_count > max_error_count )
            return false;
          error_log->Print("-- Attempting to continue.\n");
        }
        pHatchPattern = new ON_HatchPattern; // use default
        pHatchPattern->m_hatchpattern_index = count;
      }
      ON_UserDataHolder ud;
      ud.MoveUserDataFrom(*pHatchPattern);
      m_hatch_pattern_table.Append(*pHatchPattern);
      ud.MoveUserDataTo(*m_hatch_pattern_table.Last(),false);
      delete pHatchPattern;
      pHatchPattern = NULL;
    }
    
    // If BeginRead3dmHatchPatternTable() returns true, 
    // then you MUST call EndRead3dmHatchPatternTable().
    if ( !archive.EndRead3dmHatchPatternTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt hatchpattern table. (ON_BinaryArchive::EndRead3dmHatchPatternTable() returned false.)\n");
      return false;
    }
    if ( CheckForCRCErrors( archive, *this, error_log, "hatchpattern table" ) )
      return_code = false;
  }
  else     
  {
    if ( error_log)
    {
      error_log->Print("WARNING: Missing or corrupt hatchpattern table. (ON_BinaryArchive::BeginRead3dmHatchPatternTable() returned false.)\n");
      error_log->Print("-- Attempting to continue.\n");
    }
    return_code = false;
  }

  // STEP 14: REQUIRED - Read instance definition table
  if ( archive.BeginRead3dmInstanceDefinitionTable() )
  {
    ON_InstanceDefinition* pIDef = NULL;
    for( count = 0; true; count++ ) 
    {
      rc = archive.Read3dmInstanceDefinition(&pIDef);
      if ( rc==0 )
        break; // end of instance definition table
      if ( rc < 0 ) 
      {
        if ( error_log)
        {
          error_log->Print("ERROR: Corrupt instance definition found. (ON_BinaryArchive::Read3dmInstanceDefinition() < 0.)\n");
          error_count++;
          if ( error_count > max_error_count )
            return false;
          error_log->Print("-- Attempting to continue.\n");
        }
        continue;
      }
      ON_UserDataHolder ud;
      ud.MoveUserDataFrom(*pIDef);
      m_idef_table.Append(*pIDef);
      ud.MoveUserDataTo(*m_idef_table.Last(),false);
      delete pIDef;
    }
    
    // If BeginRead3dmInstanceDefinitionTable() returns true, 
    // then you MUST call EndRead3dmInstanceDefinitionTable().
    if ( !archive.EndRead3dmInstanceDefinitionTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt instance definition table. (ON_BinaryArchive::EndRead3dmInstanceDefinitionTable() returned false.)\n");
      return false;
    }
    if ( CheckForCRCErrors( archive, *this, error_log, "instance definition table" ) )
      return_code = false;
  }
  else     
  {
    if ( error_log)
    {
      error_log->Print("WARNING: Missing or corrupt instance definition table. (ON_BinaryArchive::BeginRead3dmInstanceDefinitionTable() returned false.)\n");
      error_log->Print("-- Attempting to continue.\n");
    }
    return_code = false;
  }



  // STEP 15: REQUIRED - Read object (geometry and annotation) table
  if ( archive.BeginRead3dmObjectTable() )
  {
    // optional filter made by setting ON::object_type bits 
    // For example, if you just wanted to just read points and meshes, you would use
    // object_filter = ON::point_object | ON::mesh_object;

    int object_filter = model_object_type_filter;

    for( count = 0; true; count++ ) 
    {
      ON_Object* pObject = NULL;
      ON_3dmObjectAttributes attributes;
      rc = archive.Read3dmObject(&pObject,&attributes,object_filter);
      if ( rc == 0 )
        break; // end of object table
      if ( rc < 0 ) 
      {
        if ( error_log)
        {
          error_log->Print("ERROR: Object table entry %d is corrupt. (ON_BinaryArchive::Read3dmObject() < 0.)\n",count);
          error_count++;
          if ( error_count > max_error_count )
            return false;
          error_log->Print("-- Attempting to continue.\n");
        }
        continue;
      }
      if ( m_crc_error_count != archive.BadCRCCount() ) 
      {
        if ( error_log)
        {
          error_log->Print("ERROR: Object table entry %d is corrupt. (CRC errors).\n",count);
          error_log->Print("-- Attempting to continue.\n");
        }
        m_crc_error_count = archive.BadCRCCount();
      }
      if ( pObject ) 
      {
        // 20 June 2014 S. Baer
        // The filtered Read3dmObject function does not appear to actually do filtering.
        // While we wait for that to get fixed in OpenNURBS, just check the object type
        // here and make sure it passes the filter test. This is being done here because
        // Dan needs access to this funtionality with the currently available OpenNURBS
        if( 0==object_filter || (pObject->ObjectType() & object_filter) != 0)
        {
          ONX_Model_Object& mo = m_object_table.AppendNew();
          mo.m_object = pObject;
          mo.m_bDeleteObject = true;
          mo.m_attributes = attributes;
        }
        else
        {
          delete pObject;
          pObject = 0;
        }
      }
      else
      {
        if ( error_log)
        {
          if ( rc == 2 )
            error_log->Print("WARNING: Skipping object table entry %d because it's filtered.\n",count);
          else if ( rc == 3 )
            error_log->Print("WARNING: Skipping object table entry %d because it's newer than this code.  Update your OpenNURBS toolkit.\n",count);
          else
            error_log->Print("WARNING: Skipping object table entry %d for unknown reason.\n",count);
        }
      }
    }
    
    // If BeginRead3dmObjectTable() returns true, 
    // then you MUST call EndRead3dmObjectTable().
    if ( !archive.EndRead3dmObjectTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt object light table. (ON_BinaryArchive::EndRead3dmObjectTable() returned false.)\n");
      return false;
    }
    if ( CheckForCRCErrors( archive, *this, error_log, "object table" ) )
      return_code = false;
  }
  else     
  {
    if ( error_log)
    {
      error_log->Print("WARNING: Missing or corrupt object table. (ON_BinaryArchive::BeginRead3dmObjectTable() returned false.)\n");
      error_log->Print("-- Attempting to continue.\n");
    }
    return_code = false;
  }

  // STEP 16: Read history table
  if ( archive.BeginRead3dmHistoryRecordTable() )
  {
    for( count = 0; true; count++ ) 
    {
      ON_HistoryRecord* pHistoryRecord = NULL;
      rc = archive.Read3dmHistoryRecord(pHistoryRecord);
      if ( rc == 0 )
        break; // end of history record table
      if ( rc < 0 ) 
      {
        if ( error_log)
        {
          error_log->Print("ERROR: History record table entry %d is corrupt. (ON_BinaryArchive::Read3dmHistoryRecord() < 0.)\n",count);
          error_count++;
          if ( error_count > max_error_count )
            return false;
          error_log->Print("-- Attempting to continue.\n");
        }
        continue;
      }
      if ( m_crc_error_count != archive.BadCRCCount() ) 
      {
        if ( error_log)
        {
          error_log->Print("ERROR: History record table entry %d is corrupt. (CRC errors).\n",count);
          error_log->Print("-- Attempting to continue.\n");
        }
        m_crc_error_count = archive.BadCRCCount();
      }
      if ( pHistoryRecord ) 
      {
        m_history_record_table.Append(pHistoryRecord);
      }
      else
      {
        if ( error_log)
        {
          error_log->Print("WARNING: Skipping history record table entry %d for unknown reason.\n",count);
        }
      }
    }
    
    // If BeginRead3dmHistoryRecordTable() returns true, 
    // then you MUST call EndRead3dmHistoryRecordTable().
    if ( !archive.EndRead3dmHistoryRecordTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt object light table. (ON_BinaryArchive::EndRead3dmObjectTable() returned false.)\n");
      return false;
    }
    if ( CheckForCRCErrors( archive, *this, error_log, "history record table" ) )
      return_code = false;
  }
  else     
  {
    if ( error_log)
    {
      error_log->Print("WARNING: Missing or corrupt history record table. (ON_BinaryArchive::BeginRead3dmHistoryRecordTable() returned false.)\n");
      error_log->Print("-- Attempting to continue.\n");
    }
    return_code = false;
  }

  // STEP 17: OPTIONAL - Read user tables as anonymous goo
  // If you develop a plug-ins or application that uses OpenNURBS files,
  // you can store anything you want in a user table.
  for(count=0;true;count++)
  {
    if ( archive.Archive3dmVersion() <= 1 )
    {
      // no user tables in version 1 archives.
      break;
    }

    {
      ON__UINT32 tcode = 0;
      ON__INT64 big_value = 0;
      if ( !archive.PeekAt3dmBigChunkType(&tcode,&big_value) )
        break;
      if ( TCODE_USER_TABLE != tcode )
        break;
    }
    ON_UUID plugin_id = ON_nil_uuid;
    bool bGoo = false;
    int usertable_3dm_version = 0;
    int usertable_opennurbs_version = 0;
    if ( !archive.BeginRead3dmUserTable( plugin_id, &bGoo, &usertable_3dm_version, &usertable_opennurbs_version ) )
    {
      // attempt to skip bogus user table
      const ON__UINT64 pos0 = archive.CurrentPosition();
      ON__UINT32 tcode = 0;
      ON__INT64 big_value = 0;
      if  ( !archive.BeginRead3dmBigChunk(&tcode,&big_value) )
        break;
      if ( !archive.EndRead3dmChunk() )
        break;
      const ON__UINT64 pos1 = archive.CurrentPosition();
      if (pos1 <= pos0)
        break;
      if ( TCODE_USER_TABLE != tcode )
        break;

      continue; // skip this bogus user table
    }

    ONX_Model_UserData& ud = m_userdata_table.AppendNew();
    ud.m_uuid = plugin_id;
    ud.m_usertable_3dm_version = usertable_3dm_version;
    ud.m_usertable_opennurbs_version = usertable_opennurbs_version;

    if ( !archive.Read3dmAnonymousUserTable( usertable_3dm_version, usertable_opennurbs_version, ud.m_goo ) )
    {
      if ( error_log) error_log->Print("ERROR: User data table entry %d is corrupt. (ON_BinaryArchive::Read3dmAnonymousUserTable() is false.)\n",count);
      break;
    }

    // If BeginRead3dmObjectTable() returns true, 
    // then you MUST call EndRead3dmUserTable().
    if ( !archive.EndRead3dmUserTable() )
    {
      if ( error_log) error_log->Print("ERROR: Corrupt user data table. (ON_BinaryArchive::EndRead3dmUserTable() returned false.)\n");
      break;
    }
  }

  // STEP 18: OPTIONAL - check for end mark
  if ( !archive.Read3dmEndMark(&m_file_length) )
  {
    if ( archive.Archive3dmVersion() != 1 ) 
    {
      // some v1 files are missing end-of-archive markers
      if ( error_log) error_log->Print("ERROR: ON_BinaryArchive::Read3dmEndMark(&m_file_length) returned false.\n");
    }
  }

  // Remap layer, material, linetype, font, dimstyle, hatch pattern, etc., 
  // indices so the correspond to the model's table array index.
  //
  // Polish also sets revision history information if it is missing.
  // In this case, that is not appropriate so the value of
  // m_properties.m_RevisionHistory is saved before calling Polish()
  // and restored afterwards.
  const ON_3dmRevisionHistory saved_revision_history(m_properties.m_RevisionHistory);
  Polish();
  m_properties.m_RevisionHistory = saved_revision_history;

  return return_code;
}

bool ONX_Model_WithFilter::FilteredRead( const wchar_t* filename, unsigned int table_filter, unsigned int model_object_type_filter, ON_TextLog* error_log )
{
  bool bCallDestroy = true;
  bool rc = false;

  if ( 0 != filename )
  {
    FILE* fp = ON::OpenFile(filename,L"rb");
    if ( 0 != fp )
    {
      ON_BinaryFile file(ON::read3dm,fp);
      rc = FilteredRead(file, table_filter, model_object_type_filter, error_log);
      ON::CloseFile(fp);
      bCallDestroy = false;
    }
  }

  if ( bCallDestroy )
    Destroy();

  return rc;
}



RH_C_FUNCTION ONX_Model* ONX_Model_ReadFile2(const RHMONO_STRING* path, ReadFileTableTypeFilter tableFilter, ObjectTypeFilter objectTypeFilter, CRhCmnStringHolder* pStringHolder)
{
  ONX_Model_WithFilter* rc = NULL;
  if( path )
  {
    INPUTSTRINGCOERCE(_path, path);
    rc = new ONX_Model_WithFilter();
    ON_wString s;
    ON_TextLog log(s);
    ON_TextLog* pLog = pStringHolder ? &log : NULL;
    unsigned int table_filter = (unsigned int)tableFilter;
    unsigned int obj_filter = (unsigned int)objectTypeFilter;
    if( !rc->FilteredRead(_path, table_filter, obj_filter, pLog) )
    {
      delete rc;
      rc = NULL;
    }
    if( pStringHolder )
      pStringHolder->Set(s);
  }
  return rc;
}
