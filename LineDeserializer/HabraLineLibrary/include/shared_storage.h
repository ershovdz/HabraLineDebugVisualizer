#pragma once

#include "defines.h"

#include <string>

class DLL_CLASS SharedStorage
{
public:
  static void addData(const char* name, const std::string& serializedData);
  static void removeData(const char* name);
  static std::string getData(const char* name);

private:
  SharedStorage();
  SharedStorage(const SharedStorage&);
  SharedStorage& operator=(const SharedStorage&);
};
