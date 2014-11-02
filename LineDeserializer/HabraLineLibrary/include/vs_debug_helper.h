#pragma once

#include "defines.h"
#include "shared_storage.h"

#include <vector>
#include <sstream>
#include <memory>

namespace debugger
{
  DLL_FUNC (bool) ExtendedDebugEnabled();
  DLL_FUNC (void) EnableExtendedDebug();
  DLL_FUNC (void) DisableExtendedDebug();

  template<class T>
  std::shared_ptr<T> loadObject(const std::string& sharedMemoryMarker)
  {
    if(sharedMemoryMarker.empty())
      return nullptr;

    std::string strSerializedData = SharedStorage::getData(sharedMemoryMarker.data());
    if( strSerializedData.empty() )
      return nullptr;

    return T::fromString(strSerializedData);
  }

  template <typename T>
  class VSDebugHelper
  {
  public:
    VSDebugHelper()
    {    
    }

    virtual ~VSDebugHelper()
    {
      if( debugger::ExtendedDebugEnabled() )
        removeDebugData();
    }

    void saveDebugInfo(const T* object)
    {
      if( debugger::ExtendedDebugEnabled() )
      {
        SharedStorage::addData(debugMarker(), object->toString());
      }    
    }  

  private:
    void removeDebugData()
    {
      if(!_debugMarker.empty())
        SharedStorage::removeData(_debugMarker.data());
    }

    char* debugMarker()
    {
      if(_debugMarker.empty())
      {
        const void * address = static_cast<const void*>(this);
        std::stringstream ss;
        ss << address;  
        std::string strAddress = ss.str();

        _debugMarker.insert(_debugMarker.end(), strAddress.begin(), strAddress.end());
        _debugMarker.push_back('\0');
      }  

      return _debugMarker.data();
    }

  public:
    std::vector<char> _debugMarker;
  };

  template<class TYPE>
  class scope_helper
  {
  public:
    scope_helper(VSDebugHelper<TYPE>& helper, const TYPE* pObj)
      : _debugHelper(helper)
      , _pObj(pObj)
    {	
    }

    ~scope_helper()
    {	
      _debugHelper.saveDebugInfo(_pObj);
    }

  private:
    scope_helper(const scope_helper&);
    scope_helper& operator=(const scope_helper&);

  private:
    VSDebugHelper<TYPE>& _debugHelper;
    const TYPE* _pObj;
  };
}



