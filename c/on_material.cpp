#include "StdAfx.h"

RH_C_FUNCTION ON_Material* ON_Material_New(const ON_Material* pConstOther)
{
  ON_Material* rc = new ON_Material();
  if( pConstOther )
    *rc = *pConstOther;
  return rc;
}

RH_C_FUNCTION void ON_Material_Default(ON_Material* pMaterial)
{
  if( pMaterial )
    pMaterial->Default();
}

RH_C_FUNCTION int ON_Material_FindBitmapTexture(const ON_Material* pConstMaterial, const RHMONO_STRING* filename)
{
  int rc = -1;
  INPUTSTRINGCOERCE(_filename, filename);
  if( pConstMaterial )
  {
    rc = pConstMaterial->FindTexture(_filename, ON_Texture::bitmap_texture);
  }
  return rc;
}

RH_C_FUNCTION void ON_Material_SetBitmapTexture(ON_Material* pMaterial, int index, const RHMONO_STRING* filename)
{
  INPUTSTRINGCOERCE(_filename, filename);
  if( pMaterial && index>=0 && index<pMaterial->m_textures.Count())
  {
    pMaterial->m_textures[index].m_filename = _filename;
  }
}

RH_C_FUNCTION int ON_Material_AddBitmapTexture(ON_Material* pMaterial, const RHMONO_STRING* filename)
{
  int rc = -1;
  INPUTSTRINGCOERCE(_filename, filename);
  if( pMaterial && _filename)
  {
    rc = pMaterial->AddTexture(_filename, ON_Texture::bitmap_texture);
  }
  return rc;
}

RH_C_FUNCTION int ON_Material_AddBumpTexture(ON_Material* pMaterial, const RHMONO_STRING* filename)
{
  int rc = -1;
  INPUTSTRINGCOERCE(_filename, filename);
  if( pMaterial && _filename)
  {
    rc = pMaterial->AddTexture(_filename, ON_Texture::bump_texture);
  }
  return rc;
}

RH_C_FUNCTION int ON_Material_AddEnvironmentTexture(ON_Material* pMaterial, const RHMONO_STRING* filename)
{
  int rc = -1;
  INPUTSTRINGCOERCE(_filename, filename);
  if( pMaterial && _filename)
  {
    rc = pMaterial->AddTexture(_filename, ON_Texture::emap_texture);
  }
  return rc;
}

RH_C_FUNCTION int ON_Material_AddTransparencyTexture(ON_Material* pMaterial, const RHMONO_STRING* filename)
{
  int rc = -1;
  INPUTSTRINGCOERCE(_filename, filename);
  if( pMaterial && _filename)
  {
    rc = pMaterial->AddTexture(_filename, ON_Texture::transparency_texture);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Material_ModifyTexture(ON_Material* pMaterial, ON_UUID texture_id, const ON_Texture* pConstTexture)
{
  bool rc = false;
  if( pMaterial && pConstTexture )
  {
    int index = pMaterial->FindTexture(texture_id);
    if( index>=0 )
    {
      pMaterial->m_textures[index] = *pConstTexture;
      rc = true;
    }
  }
  return rc;
}

RH_C_FUNCTION double ON_Material_GetDouble(const ON_Material* pConstMaterial, int which)
{
  const int idxShine = 0;
  const int idxTransparency = 1;
  
  double rc = 0;
  if( pConstMaterial )
  {
    switch(which)
    {
    case idxShine:
      rc = pConstMaterial->m_shine;
      break;
    case idxTransparency:
      rc = pConstMaterial->m_transparency;
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_Material_SetDouble(ON_Material* pMaterial, int which, double val)
{
  const int idxShine = 0;
  const int idxTransparency = 1;
  if( pMaterial )
  {
    switch(which)
    {
    case idxShine:
      pMaterial->SetShine(val);
      break;
    case idxTransparency:
      pMaterial->SetTransparency(val);
      break;
    }
  }
}

RH_C_FUNCTION bool ON_Material_AddTexture(ON_Material* pMaterial, const RHMONO_STRING* filename, int which)
{
  const int idxBitmapTexture = 0;
  const int idxBumpTexture = 1;
  const int idxEmapTexture = 2;
  const int idxTransparencyTexture = 3;
  bool rc = false;
  if( pMaterial && filename )
  {
    ON_Texture::TYPE tex_type = ON_Texture::bitmap_texture;
    if( idxBumpTexture==which ) tex_type = ON_Texture::bump_texture;
    if( idxEmapTexture==which ) tex_type = ON_Texture::emap_texture;
    if( idxTransparencyTexture==which ) tex_type = ON_Texture::transparency_texture;
    int index = pMaterial->FindTexture(NULL, tex_type);
    if( index>=0 )
      pMaterial->DeleteTexture(NULL, tex_type);
    INPUTSTRINGCOERCE(_filename, filename);
    rc = pMaterial->AddTexture(_filename, tex_type)>=0;
  }
  return rc;
}

RH_C_FUNCTION bool ON_Material_SetTexture(ON_Material* pMaterial, const ON_Texture* pConstTexture, int which)
{
  const int idxBitmapTexture = 0;
  const int idxBumpTexture = 1;
  const int idxEmapTexture = 2;
  const int idxTransparencyTexture = 3;
  bool rc = false;
  if( pMaterial && pConstTexture )
  {
    ON_Texture::TYPE tex_type = ON_Texture::bitmap_texture;
    if( idxBumpTexture==which ) tex_type = ON_Texture::bump_texture;
    if( idxEmapTexture==which ) tex_type = ON_Texture::emap_texture;
    if( idxTransparencyTexture==which ) tex_type = ON_Texture::transparency_texture;
    int index = pMaterial->FindTexture(NULL, tex_type);
    if( index>=0 )
      pMaterial->DeleteTexture(NULL, tex_type);

    ON_Texture texture(*pConstTexture);
    texture.m_type = tex_type;
    rc = pMaterial->AddTexture(texture)>=0;
  }
  return rc;
}

RH_C_FUNCTION int ON_Material_GetTexture(const ON_Material* pConstMaterial, int which)
{
  const int idxBitmapTexture = 0;
  const int idxBumpTexture = 1;
  const int idxEmapTexture = 2;
  const int idxTransparencyTexture = 3;
  int rc = -1;
  if( pConstMaterial )
  {
    ON_Texture::TYPE tex_type = ON_Texture::bitmap_texture;
    if( idxBumpTexture==which ) tex_type = ON_Texture::bump_texture;
    if( idxEmapTexture==which ) tex_type = ON_Texture::emap_texture;
    if( idxTransparencyTexture==which ) tex_type = ON_Texture::transparency_texture;
    rc = pConstMaterial->FindTexture(NULL, tex_type);
  }
  return rc;
}

RH_C_FUNCTION int ON_Material_GetColor( const ON_Material* pConstMaterial, int which )
{
  const int idxDiffuse = 0;
  const int idxAmbient = 1;
  const int idxEmission = 2;
  const int idxSpecular = 3;
  const int idxReflection = 4;
  const int idxTransparent = 5;
  int rc = 0;
  if( pConstMaterial )
  {
    unsigned int abgr = 0;
    switch(which)
    {
    case idxDiffuse:
      abgr = (unsigned int)(pConstMaterial->m_diffuse);
      break;
    case idxAmbient:
      abgr = (unsigned int)(pConstMaterial->m_ambient);
      break;
    case idxEmission:
      abgr = (unsigned int)(pConstMaterial->m_emission);
      break;
    case idxSpecular:
      abgr = (unsigned int)(pConstMaterial->m_specular);
      break;
    case idxReflection:
      abgr = (unsigned int)(pConstMaterial->m_reflection);
      break;
    case idxTransparent:
      abgr = (unsigned int)(pConstMaterial->m_transparent);
      break;
    }
    rc = (int)abgr;
  }
  return rc;
}

RH_C_FUNCTION void ON_Material_SetColor( ON_Material* pMaterial, int which, int argb )
{
  const int idxDiffuse = 0;
  const int idxAmbient = 1;
  const int idxEmission = 2;
  const int idxSpecular = 3;
  const int idxReflection = 4;
  const int idxTransparent = 5;
  int abgr = ARGB_to_ABGR(argb);
  if( pMaterial )
  {
    switch(which)
    {
    case idxDiffuse:
      pMaterial->m_diffuse = abgr;
      break;
    case idxAmbient:
      pMaterial->m_ambient = abgr;
      break;
    case idxEmission:
      pMaterial->m_emission = abgr;
      break;
    case idxSpecular:
      pMaterial->m_specular = abgr;
      break;
    case idxReflection:
      pMaterial->m_reflection = abgr;
      break;
    case idxTransparent:
      pMaterial->m_transparent = abgr;
      break;
    }
  }
}

RH_C_FUNCTION void ON_Material_GetName(const ON_Material* pConstMaterial, CRhCmnStringHolder* pString)
{
  if( pConstMaterial && pString )
  {
    pString->Set(pConstMaterial->m_material_name);
  }
}

RH_C_FUNCTION void ON_Material_SetName(ON_Material* pMaterial, const RHMONO_STRING* name)
{
  if( pMaterial )
  {
    INPUTSTRINGCOERCE(_name, name);
    pMaterial->m_material_name = _name;
  }
}
/////////////////////////////////////////////////////////////

RH_C_FUNCTION ON_Texture* ON_Texture_New()
{
  return new ON_Texture();
}

RH_C_FUNCTION const ON_Texture* ON_Material_GetTexturePointer(const ON_Material* pConstMaterial, int index)
{
  const ON_Texture* rc = NULL;
  if( pConstMaterial && index>=0 )
  {
    rc = pConstMaterial->m_textures.At(index);
  }
  return rc;
}

RH_C_FUNCTION int ON_Material_NextBitmapTexture(const ON_Material* pConstMaterial, int index)
{
  int rc = -1;
  if( pConstMaterial )
  {
    rc = pConstMaterial->FindTexture(NULL, ON_Texture::bitmap_texture, index);
  }
  return rc;
}

RH_C_FUNCTION int ON_Material_NextBumpTexture(const ON_Material* pConstMaterial, int index)
{
  int rc = -1;
  if( pConstMaterial )
  {
    rc = pConstMaterial->FindTexture(NULL, ON_Texture::bump_texture, index);
  }
  return rc;
}

RH_C_FUNCTION int ON_Material_NextEnvironmentTexture(const ON_Material* pConstMaterial, int index)
{
  int rc = -1;
  if( pConstMaterial )
  {
    rc = pConstMaterial->FindTexture(NULL, ON_Texture::emap_texture, index);
  }
  return rc;
}

RH_C_FUNCTION int ON_Material_NextTransparencyTexture(const ON_Material* pConstMaterial, int index)
{
  int rc = -1;
  if( pConstMaterial )
  {
    rc = pConstMaterial->FindTexture(NULL, ON_Texture::transparency_texture, index);
  }
  return rc;
}

RH_C_FUNCTION void ON_Texture_GetFileName(const ON_Texture* pConstTexture, CRhCmnStringHolder* pString)
{
  if( pConstTexture && pString )
  {
    pString->Set(pConstTexture->m_filename);
  }
}

RH_C_FUNCTION void ON_Texture_SetFileName(ON_Texture* pTexture, const RHMONO_STRING* filename)
{
  if( pTexture )
  {
    INPUTSTRINGCOERCE(_filename, filename);
    pTexture->m_filename = _filename;
  }
}

RH_C_FUNCTION ON_UUID ON_Texture_GetId(const ON_Texture* pConstTexture)
{
  if( pConstTexture )
    return pConstTexture->m_texture_id;
  return ON_nil_uuid;
}

