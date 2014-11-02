#pragma once

#include "serializer_defines.h"

struct SERIALIZER_DLL_CLASS CPoint3D
{
public:
  CPoint3D(float x, float y, float z)
    : _x(x)
    , _y(y)
    , _z(z)
  {
  }

  float _x,_y,_z;
};


