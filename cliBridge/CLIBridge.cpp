#include "CLIBridge.h"

#include <LineDeserializer.h>

#include <msclr/marshal_cppstd.h>
#include <iostream>

using namespace VisualizerService;

bool CLIBridge::getLineData(String^ sharedMemoryMarker, [Out] array<Vertex^>^%  vertices)
{
  std::string nativeStrMarker = msclr::interop::marshal_as<std::string, System::String^>(sharedMemoryMarker);

  LineDeserializer lineDeserializer;
  std::vector<CPoint3D> nativePoints;

  bool isSuccess = lineDeserializer.Deserialize(nativeStrMarker, nativePoints);

  if(isSuccess)
  {
    vertices = gcnew cli::array<Vertex^>(nativePoints.size());

    for(size_t i = 0; i < nativePoints.size(); ++i)
    {
      const CPoint3D& nativePoint = nativePoints[i];
      vertices[i] = gcnew Vertex(nativePoint._x, nativePoint._y, nativePoint._z);
    }

    return true;
  }

  return false;
};


