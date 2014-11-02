#include "LineDeserializer.h"

#include "vs_debug_helper.h"
#include "habra_line.h"

bool LineDeserializer::Deserialize(const std::string& sharedMemoryMarker, std::vector<CPoint3D>& points)
{
  try
  {
    std::shared_ptr<HabraLine> spNativeLine = debugger::loadObject<HabraLine>(sharedMemoryMarker);

    if(spNativeLine)
    {
      CPoint3D startPoint(spNativeLine->_start._x, spNativeLine->_start._y, 0);
      CPoint3D endPoint(spNativeLine->_end._x, spNativeLine->_end._y, 0);
      points.push_back(startPoint);
      points.push_back(endPoint);

      return true;
    }
  }
  catch (...)
  {    
  }

  return false;
}






