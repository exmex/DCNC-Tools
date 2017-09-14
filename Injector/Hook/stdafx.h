#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include <winsock2.h>
#include <windows.h>
#include <intrin.h>
#include <detours.h>
#include <stdio.h>

#include <winuser.h>
#include <fstream>
#include <tlhelp32.h>
#include <stdio.h>
#include <time.h>
#include <iostream>
#include <conio.h>
#include <time.h>
#include <ddraw.h>
#include <vector>
#include <Shlwapi.h>
#include <math.h>
#include <Gdiplus.h>
#include <assert.h>
#include <sstream>
#include <iomanip>
#include <string>
#include <map>
#include <list>
#include <vector>
#include <algorithm>

#include <vector>

#define VALID( x ) ( x != NULL && HIWORD( x ) )
#include "CTools.h"

//===========================================================================
typedef struct _UNICODE_STRING
{
	USHORT Length;
	USHORT MaximumLength;
	PWCH Buffer;
} UNICODE_STRING;

typedef UNICODE_STRING* PUNICODE_STRING;

//===========================================================================
typedef struct _PEB_LDR_DATA
{
	ULONG Length;
	BOOLEAN Initialized;
	PVOID SsHandle;
	LIST_ENTRY InLoadOrderModuleList;
	LIST_ENTRY InMemoryOrderModuleList;
	LIST_ENTRY InInitializationOrderModuleList;
} PEB_LDR_DATA, *PPEB_LDR_DATA;

//===========================================================================
typedef struct _LDR_MODULE
{
	LIST_ENTRY InLoadOrderModuleList;
	LIST_ENTRY InMemoryOrderModuleList;
	LIST_ENTRY InInitializationOrderModuleList;
	PVOID BaseAddress;
	PVOID EntryPoint;
	ULONG SizeOfImage;
	UNICODE_STRING FullDllName;
	UNICODE_STRING BaseDllName;
	ULONG Flags;
	SHORT LoadCount;
	SHORT TlsIndex;
	LIST_ENTRY HashTableEntry;
	ULONG TimeDateStamp;
} LDR_MODULE, *PLDR_MODULE;