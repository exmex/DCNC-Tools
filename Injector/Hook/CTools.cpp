#include "stdafx.h"

PBYTE CTools::HookVTable( PDWORD* dwVTable, PBYTE dwHook, INT Index )
{
	DWORD dwOld = 0;
	VirtualProtect((void*)((*dwVTable) + Index), 4, PAGE_EXECUTE_READWRITE, &dwOld);
	PBYTE pOrig = ((PBYTE)(*dwVTable)[Index]); 
	
	(*dwVTable)[Index] = (DWORD)dwHook;
	VirtualProtect((void*)((*dwVTable) + Index), 4, dwOld, &dwOld);
	
	return pOrig;
}

/*DWORD CTools::FindPattern(DWORD start_offset, DWORD size, BYTE* pattern, char mask[] )
{
		
		
	DWORD pos = 0;
	int searchLen = strlen(mask) - 1;

	for( DWORD retAddress = start_offset; retAddress < start_offset + size; retAddress++ )
	{
		if( *(BYTE*)retAddress == pattern[pos] || mask[pos] == '?' ){
			if( mask[pos+1] == '\0' )
				return (retAddress - searchLen);
			pos++;
		} 
		else 
			pos = 0;
	}

		
	return NULL;
}*/

//------------------
bool CTools::DataCompare(const BYTE* pData, const BYTE* bMask, char* szMask)  
{
	for (; *szMask; ++szMask, ++pData, ++bMask)  
	{
		if (*szMask == 'x' &&*pData !=*bMask)  
		{
			return FALSE;
		}
	}
	return (*szMask) == NULL;
}
//------------------
DWORD CTools::FindPattern(DWORD dwAddress, DWORD dwSize, BYTE* pbMask, char* szMask)  
{
	for (DWORD i = NULL; i <dwSize; i++)  
	{
		if (DataCompare ((BYTE*)(dwAddress + i), pbMask, szMask))  
		{
			return (DWORD)(dwAddress + i);
		}
	}

	return 0;
}
//------------------
DWORD CTools::GetModuleSize( char *pModuleName )
{
	HMODULE hModule = (HMODULE)GetModuleHandle(pModuleName);

	if (!hModule)
		return NULL;

	PIMAGE_DOS_HEADER pDosHeader = PIMAGE_DOS_HEADER(hModule);
	if (!pDosHeader)
		return NULL;

	PIMAGE_NT_HEADERS pNTHeader = PIMAGE_NT_HEADERS((LONG)hModule + pDosHeader->e_lfanew);
	if (!pNTHeader)
		return NULL;

	PIMAGE_OPTIONAL_HEADER pOptionalHeader = &pNTHeader->OptionalHeader;
	if (!pOptionalHeader)
		return NULL;

	return pOptionalHeader->SizeOfCode;
}


DWORD CTools::GetModuleSz(LPSTR strModuleName)
{
	MODULEENTRY32    lpme= {0};
	DWORD            dwSize=0;
	DWORD            PID=GetCurrentProcessId();
	BOOL            isMod=0;
	char            chModName[256];

	strcpy_s(chModName,strModuleName);
	_strlwr_s(chModName);

	// Check Module
	HANDLE hSnapshotModule=CreateToolhelp32Snapshot(TH32CS_SNAPMODULE ,PID);
	if (hSnapshotModule) 
	{
		lpme.dwSize=sizeof(lpme);
		isMod=Module32First(hSnapshotModule,&lpme);
		while(isMod) 
		{
			if (strcmp(_strlwr(lpme.szExePath),chModName)) 
			{
				dwSize=(DWORD)lpme.modBaseSize;
				CloseHandle(hSnapshotModule);

				return dwSize;
			}
			isMod=Module32Next(hSnapshotModule,&lpme);
		}
	}
	CloseHandle(hSnapshotModule);
	return 0;  
}

void* CTools::DetourF(BYTE *src, const BYTE *dst, const int len)
{
	BYTE *jmp = (BYTE*)malloc(len+5);
	DWORD dwBack;

	VirtualProtect(src, len, PAGE_READWRITE, &dwBack);

	memcpy(jmp, src, len);	
	jmp += len;

	jmp[0] = 0xE9;
	*(DWORD*)(jmp+1) = (DWORD)(src+len - jmp) - 5;
	
	src[0] = 0xE9;
	*(DWORD*)(src+1) = (DWORD)(dst - src) - 5;

	for( int i=5; i < len; i++ )
		src[i] = 0x90;

	VirtualProtect(src, len, dwBack, &dwBack);
	return (jmp-len);
}

void CTools::CleanRegistry()
{
	// If the parent key exists then delete it and all the children
	//system("REG DELETE HKLM\\Software\\PBService /f");

	/*if (SHDeleteKey(HKEY_CURRENT_USER, "Software\\PBService") != ERROR_SUCCESS)
	{
		if (SHDeleteKey(HKEY_LOCAL_MACHINE, "Software\\PBService") != ERROR_SUCCESS)
		{
		}
	}*/
}

DWORD CTools::GetProcessIdByNameW(const wchar_t *pProcessName)
{
	PROCESSENTRY32W ProcessEntry;
	ProcessEntry.dwSize = sizeof(PROCESSENTRY32W);
	HANDLE hProcessSnap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);

	if (Process32FirstW(hProcessSnap, &ProcessEntry))
	{
		do
		{
			if (!wcscmp(ProcessEntry.szExeFile, pProcessName))
			{
				return ProcessEntry.th32ProcessID;
			}

		} while(Process32NextW(hProcessSnap, &ProcessEntry));
	}

	return 0;
}