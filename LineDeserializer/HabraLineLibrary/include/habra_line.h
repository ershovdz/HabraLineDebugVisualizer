#pragma once

#include "vs_debug_helper.h"

#include <memory>

struct DLL_CLASS HabraPoint
{
  HabraPoint(float x, float y)
    : _x(x), _y(y)
  {
  }

  float _x, _y;
};

class DLL_CLASS HabraLine
{
public:
  HabraLine();
  HabraLine(HabraPoint start, HabraPoint end);
  void modify(HabraPoint newStart, HabraPoint newEnd);
  std::string toString() const;
  static std::shared_ptr<HabraLine> fromString(const std::string& serializedData);

public:
  HabraPoint _start;
  HabraPoint _end;

private:
  debugger::VSDebugHelper<HabraLine> _debugHelper;
};
