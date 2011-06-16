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

RH_C_FUNCTION bool ON_BinaryArchive_ReadColor(ON_BinaryArchive* pArchive, int* abgr)
{
  bool rc = false;
  if( pArchive && abgr )
  {
    ON_Color c;
    rc = pArchive->ReadColor(c);
    if( rc )
    {
      unsigned int _c = (unsigned int)c;
      *abgr = (int)_c;
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
    rc = pConstGeometry->Write(*pArchive) ? true:false;
  }
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
  ON_Object* rc = NULL;
  if( length>0 && buffer )
  {
    ON_Read3dmBufferArchive archive(length, buffer, false, archive_3dm_version, archive_on_version);
    archive.ReadObject( &rc );
  }
  return rc;
}

#if defined(RHINO_V5SR)
typedef ON_Write3dmBufferArchive CRhCmnWrite3dmBufferArchive;
#else
// Ughh... What a pain!!!!
// ON_Write3dmBufferArchive had a bug in the write function which was fixed in
// V5. Placing the fix in RhinoCommon so this will also work in V4/Grasshopper
class CRhCmnWrite3dmBufferArchive : public ON_BinaryArchive
{
public:
  CRhCmnWrite3dmBufferArchive( size_t initial_sizeof_buffer, size_t max_sizeof_buffer, 
    int archive_3dm_version,int archive_opennurbs_version );

  ~CRhCmnWrite3dmBufferArchive();
  size_t SizeOfArchive() const;
  size_t SizeOfBuffer() const;
  const void* Buffer() const;
  void* HarvestBuffer();
  size_t CurrentPosition() const; 
  bool SeekFromCurrentPosition(int); 
  bool SeekFromStart(size_t);
  bool AtEnd() const;
protected:
  size_t Read( size_t, void* ); 
  size_t Write( size_t, const void* ); // return actual number of bytes written (like fwrite())
  bool Flush();
private:
  void AllocBuffer(size_t);
  void* m_p;
  unsigned char* m_buffer;
  size_t m_sizeof_buffer;
  const size_t m_max_sizeof_buffer;
  size_t m_sizeof_archive;
  size_t m_buffer_position;
  ON__INT_PTR m_reserved1;
  ON__INT_PTR m_reserved2;
  ON__INT_PTR m_reserved3;
  ON__INT_PTR m_reserved4;
private:
  // prohibit use - no implementation
  CRhCmnWrite3dmBufferArchive(); 
  CRhCmnWrite3dmBufferArchive( const CRhCmnWrite3dmBufferArchive& );
  CRhCmnWrite3dmBufferArchive& operator=(const CRhCmnWrite3dmBufferArchive&);
};

static void ON_SetBinaryArchiveOpenNURBSVersion(ON_BinaryArchive& file, int value)
{
  if ( value >= 200012210 )
  {
    file.m_3dm_opennurbs_version = value;
  }
  else
  {
    ON_ERROR("ON_SetBinaryArchiveOpenNURBSVersion - invalid opennurbs version");
    file.m_3dm_opennurbs_version = 0;
  }
}


CRhCmnWrite3dmBufferArchive::CRhCmnWrite3dmBufferArchive( 
          size_t initial_sizeof_buffer, 
          size_t max_sizeof_buffer, 
          int archive_3dm_version,
          int archive_opennurbs_version
          )
: ON_BinaryArchive(ON::write3dm)
, m_p(0)
, m_buffer(0)
, m_sizeof_buffer(0)
, m_max_sizeof_buffer(max_sizeof_buffer)
, m_sizeof_archive(0)
, m_buffer_position(0)
, m_reserved1(0)
, m_reserved2(0)
, m_reserved3(0)
, m_reserved4(0)
{
  if ( initial_sizeof_buffer > 0 )
    AllocBuffer(initial_sizeof_buffer);
  SetArchive3dmVersion(archive_3dm_version);
  ON_SetBinaryArchiveOpenNURBSVersion(*this,archive_opennurbs_version);
}

CRhCmnWrite3dmBufferArchive::~CRhCmnWrite3dmBufferArchive()
{
  if ( m_p )
    onfree(m_p);
}

void CRhCmnWrite3dmBufferArchive::AllocBuffer( size_t sz )
{
  if ( sz > m_sizeof_buffer 
       && (m_max_sizeof_buffer <= 0 || sz <= m_max_sizeof_buffer) 
     )
  {
    if ( sz < 2*m_sizeof_buffer )
    {
      sz = 2*m_sizeof_buffer;
      if ( sz > m_max_sizeof_buffer )
        sz = m_max_sizeof_buffer;
    }

    m_p = onrealloc(m_p,sz);
    m_buffer = (unsigned char*)m_p;

    if ( 0 != m_buffer )
    {
      memset( m_buffer + m_sizeof_buffer, 0, sz - m_sizeof_buffer );
      m_sizeof_buffer = sz;
    }
    else
    {
      m_sizeof_buffer = 0;
    }

  }
}

// ON_BinaryArchive overrides
size_t CRhCmnWrite3dmBufferArchive::CurrentPosition() const
{
  return m_buffer_position;
}

bool CRhCmnWrite3dmBufferArchive::SeekFromCurrentPosition( int offset )
{
  bool rc = false;
  if ( m_buffer )
  {
    if (offset >= 0 )
    {
      m_buffer_position += offset;
      rc = true;
    }
    else if ( size_t(-offset) <= m_buffer_position )
    {
      m_buffer_position -= (size_t(-offset));
      rc = true;
    }
  }
  return rc;
}

bool CRhCmnWrite3dmBufferArchive::SeekFromStart( size_t offset )
{
  bool rc = false;
  if ( m_buffer && offset >= 0 ) 
  {
    m_buffer_position = offset;
    rc = true;
  }
  return rc;
}

bool CRhCmnWrite3dmBufferArchive::AtEnd() const
{
  return (m_buffer_position >= m_sizeof_buffer) ? true : false;
}

size_t CRhCmnWrite3dmBufferArchive::Read( size_t count, void* buffer )
{
  if ( count <= 0 || 0 == buffer )
    return 0;

  size_t maxcount = ( m_sizeof_buffer > m_buffer_position ) 
                  ? (m_sizeof_buffer - m_buffer_position)
                  : 0;
  if ( count > maxcount )
    count = maxcount;

  if ( count > 0 ) 
  {
    memcpy( buffer, m_buffer+m_buffer_position, count );
    m_buffer_position += count;
  }

  return count;
}

size_t CRhCmnWrite3dmBufferArchive::Write( size_t sz, const void* buffer )
{
  if ( sz <= 0 || 0 == buffer )
    return 0;

  if ( m_buffer_position + sz > m_sizeof_buffer )
  {
    AllocBuffer(m_buffer_position + sz);
  }

  if ( m_buffer_position + sz > m_sizeof_buffer )
    return 0;

  memcpy( m_buffer + m_buffer_position, buffer, sz );
  m_buffer_position += sz;
  if ( m_buffer_position > m_sizeof_archive )
    m_sizeof_archive = m_buffer_position;

  return sz;
}

bool CRhCmnWrite3dmBufferArchive::Flush()
{
  // Nothing to flush
  return true;
}


size_t CRhCmnWrite3dmBufferArchive::SizeOfBuffer() const
{
  return m_sizeof_buffer;
}

const void* CRhCmnWrite3dmBufferArchive::Buffer() const
{
  return (const void*)m_buffer;
}

void* CRhCmnWrite3dmBufferArchive::HarvestBuffer()
{
  void* buffer = m_buffer;

  m_p = 0;
  m_buffer = 0;
  m_sizeof_buffer = 0;
  m_sizeof_archive = 0;
  m_buffer_position = 0;

  return buffer;
}

size_t CRhCmnWrite3dmBufferArchive::SizeOfArchive() const
{
  return m_sizeof_archive;
}

#endif

RH_C_FUNCTION CRhCmnWrite3dmBufferArchive* ON_WriteBufferArchive_NewWriter(const ON_Object* pConstObject, int rhinoversion, bool writeuserdata, unsigned int* length)
{
  CRhCmnWrite3dmBufferArchive* rc = NULL;

  if( pConstObject && length )
  {
    ON_UserDataHolder holder;
    if( !writeuserdata )
      holder.MoveUserDataFrom(*pConstObject);
    *length = 0;
    size_t sz = pConstObject->SizeOf() + 256;
    rc = new CRhCmnWrite3dmBufferArchive(sz, -1, rhinoversion, ON::Version());
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

RH_C_FUNCTION unsigned char* ON_WriteBufferArchive_Buffer(const CRhCmnWrite3dmBufferArchive* pBinaryArchive)
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

RH_C_FUNCTION ONX_Model* ONX_Model_ReadFile(const RHMONO_STRING* path)
{
  ONX_Model* rc = NULL;
  if( path )
  {
    INPUTSTRINGCOERCE(_path, path);
    rc = new ONX_Model();
    if( !rc->Read(_path) )
    {
      delete rc;
      rc = NULL;
    }
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
  if( pConstModel && pString && visible && html && left && top && right && bottom )
  {
    const ON_3dmNotes& notes = pConstModel->m_properties.m_Notes;
    pString->Set(notes.m_notes);
    *visible = notes.m_bVisible?true:false;
    *html = notes.m_bHTML?true:false;
    *left = notes.m_window_left;
    *top = notes.m_window_top;
    *right = notes.m_window_right;
    *bottom = notes.m_window_bottom;
  }
}

RH_C_FUNCTION void ONX_Model_SetNotes(ONX_Model* pModel, const RHMONO_STRING* notes, bool visible, bool html, int left, int top, int right, int bottom)
{
  if( pModel )
  {
    INPUTSTRINGCOERCE(_notes, notes);
    pModel->m_properties.m_Notes.m_notes = _notes;
    pModel->m_properties.m_Notes.m_bHTML = html?1:0;
    pModel->m_properties.m_Notes.m_bVisible = visible?1:0;
    pModel->m_properties.m_Notes.m_window_left = left;
    pModel->m_properties.m_Notes.m_window_top = top;
    pModel->m_properties.m_Notes.m_window_right = right;
    pModel->m_properties.m_Notes.m_window_bottom = bottom;
  }
}

RH_C_FUNCTION int ONX_Model_TableCount(const ONX_Model* pConstModel, int which)
{
  const int idxBitmapTable = 2;
  const int idxTextureMappingTable = 3;
  const int idxMaterialTable = 4;
  const int idxLinetypeTable = 5;
  const int idxLayerTable = 6;
  const int idxLightTable = 7;
  const int idxGroupTable = 8;
  const int idxFontTable = 9;
  const int idxDimStyleTable = 10;
  const int idxHatchPatternTable = 11;
  const int idxIDefTable = 12;
  const int idxObjectTable = 13;
  const int idxHistoryRecordTable = 14;
  const int idxUserDataTable = 15;

  int rc = 0;
  if( pConstModel )
  {
    switch(which)
    {
    case idxBitmapTable:
      rc = pConstModel->m_bitmap_table.Count();
      break;
    case idxTextureMappingTable:
      rc = pConstModel->m_mapping_table.Count();
      break;
    case idxMaterialTable:
      rc = pConstModel->m_material_table.Count();
      break;
    case idxLinetypeTable:
      rc = pConstModel->m_linetype_table.Count();
      break;
    case idxLayerTable:
      rc = pConstModel->m_layer_table.Count();
      break;
    case idxLightTable:
      rc = pConstModel->m_light_table.Count();
      break;
    case idxGroupTable:
      rc = pConstModel->m_group_table.Count();
      break;
    case idxFontTable:
      rc = pConstModel->m_font_table.Count();
      break;
    case idxDimStyleTable:
      rc = pConstModel->m_dimstyle_table.Count();
      break;
    case idxHatchPatternTable:
      rc = pConstModel->m_hatch_pattern_table.Count();
      break;
    case idxIDefTable:
      rc = pConstModel->m_idef_table.Count();
      break;
    case idxObjectTable:
      rc = pConstModel->m_object_table.Count();
      break;
    case idxHistoryRecordTable:
      rc = pConstModel->m_history_record_table.Count();
      break;
    case idxUserDataTable:
      rc = pConstModel->m_userdata_table.Count();
      break;
    default:
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ONX_Model_Dump(const ONX_Model* pConstModel, int which, CRhCmnStringHolder* pStringHolder)
{
  const int idxDumpAll = 0;
  const int idxDumpSummary = 1;
  const int idxBitmapTable = 2;
  const int idxTextureMappingTable = 3;
  const int idxMaterialTable = 4;
  const int idxLinetypeTable = 5;
  const int idxLayerTable = 6;
  const int idxLightTable = 7;
  const int idxGroupTable = 8;
  const int idxFontTable = 9;
  const int idxDimStyleTable = 10;
  const int idxHatchPatternTable = 11;
  const int idxIDefTable = 12;
  const int idxObjectTable = 13;
  const int idxHistoryRecordTable = 14;
  const int idxUserDataTable = 15;

  if( pConstModel && pStringHolder )
  {
    ON_wString s;
    ON_TextLog log(s);
    switch(which)
    {
    case idxDumpAll:
      pConstModel->Dump(log);
      break;
    case idxDumpSummary:
      pConstModel->DumpSummary(log);
      break;
    case idxBitmapTable:
      pConstModel->DumpBitmapTable(log);
      break;
    case idxTextureMappingTable:
      pConstModel->DumpTextureMappingTable(log);
      break;
    case idxMaterialTable:
      pConstModel->DumpMaterialTable(log);
      break;
    case idxLinetypeTable:
      pConstModel->DumpLinetypeTable(log);
      break;
    case idxLayerTable:
      pConstModel->DumpLayerTable(log);
      break;
    case idxLightTable:
      pConstModel->DumpLightTable(log);
      break;
    case idxGroupTable:
      pConstModel->DumpGroupTable(log);
      break;
    case idxFontTable:
      pConstModel->DumpFontTable(log);
      break;
    case idxDimStyleTable:
      pConstModel->DumpDimStyleTable(log);
      break;
    case idxHatchPatternTable:
      pConstModel->DumpHatchPatternTable(log);
      break;
    case idxIDefTable:
      pConstModel->DumpIDefTable(log);
      break;
    case idxObjectTable:
      pConstModel->DumpObjectTable(log);
      break;
    case idxHistoryRecordTable:
      pConstModel->DumpHistoryRecordTable(log);
      break;
    case idxUserDataTable:
      pConstModel->DumpUserDataTable(log);
      break;
    default:
      break;
    }
    pStringHolder->Set(s);
  }
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

RH_C_FUNCTION void ONX_Model_BoundingBox(const ONX_Model* pConstModel, ON_BoundingBox* pBBox)
{
  if( pConstModel && pBBox )
  {
    *pBBox = pConstModel->BoundingBox();
  }
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

RH_C_FUNCTION void ONX_Model_LayerTable_RemoveAt(ONX_Model* pModel, int index)
{
  if( pModel && index>=0)
  {
    pModel->m_layer_table.Remove(index);
    // update layer indices
    for( int i=index; i<pModel->m_layer_table.Count(); i++ )
      pModel->m_layer_table[i].m_layer_index = i;
  }
}

RH_C_FUNCTION ON_UUID ONX_Model_LayerTable_Id(const ONX_Model* pConstModel, int index)
{
  if( pConstModel && index>=0 && index<pConstModel->m_layer_table.Count())
  {
    return pConstModel->m_layer_table[index].m_layer_id;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION void ONX_Model_LayerTable_Clear(ONX_Model* pModel)
{
  if( pModel )
    pModel->m_layer_table.Empty();
}