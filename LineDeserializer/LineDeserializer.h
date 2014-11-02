#pragma once

#include "serializer_defines.h"
#include "Point3D.h"

#include <iosfwd>
#include <vector>

class SERIALIZER_DLL_CLASS LineDeserializer
{
public:
  bool Deserialize(const std::string& sharedMemoryMarker, std::vector<CPoint3D>& points);
};

