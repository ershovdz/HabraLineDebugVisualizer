#pragma once

#pragma warning(disable: 4251)  //AP class '1' needs to have dll-interface to be used by clients of class '2' (using non-exported in exported)

#define CALL_DECLARATION    __cdecl

#if defined ( _BUILDDLL )
#define DLL_CLASS         __declspec( dllexport )
#define DLL_FUNC(retType) __declspec( dllexport ) retType CALL_DECLARATION
#else
#define DLL_CLASS         __declspec( dllimport )
#define DLL_FUNC(retType) __declspec( dllimport ) retType CALL_DECLARATION
#endif