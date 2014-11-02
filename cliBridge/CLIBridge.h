// VisualizerService.h

#pragma once

#include <windows.h>
using namespace System;
using namespace System::Runtime::InteropServices;

namespace VisualizerService 
{
  public ref class Vertex
  {
  public:
    Vertex(float x, float y, float z) 
      : _x(x)
      , _y(y)
      , _z(z)
    {
    }

    float _x;
    float _y;
    float _z;
  };

  public ref class CLIBridge
  {
  public:
    bool getLineData(String^ sharedMemoryMarker, [Out] array<Vertex^>^% vertices);
  };
}
