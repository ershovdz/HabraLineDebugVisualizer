#pragma once

#pragma warning(disable: 4251)  //AP class '1' needs to have dll-interface to be used by clients of class '2' (using non-exported in exported)

#define SERIALIZER_CALL_DECLARATION    __cdecl

#if defined ( _BUILDDLL )
#define SERIALIZER_DLL_CLASS         __declspec( dllexport )
#define SERIALIZER_DLL_FUNC(retType) __declspec( dllexport ) retType SERIALIZER_CALL_DECLARATION
#else
#define SERIALIZER_DLL_CLASS         __declspec( dllimport )
#define SERIALIZER_DLL_FUNC(retType) __declspec( dllimport ) retType SERIALIZER_CALL_DECLARATION
#endif