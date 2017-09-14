#ifndef _TOOLS_H
#define _TOOLS_H

class CTools {
public:
	static PBYTE HookVTable( PDWORD* dwVTable, PBYTE dwHook, INT Index );
	static bool  DataCompare(const BYTE* pData, const BYTE* bMask, char* szMask)  ;
	static DWORD FindPattern(DWORD dwAddress, DWORD dwSize, BYTE* pbMask, char* szMask);
	static DWORD GetModuleSz(LPSTR strModuleName);
	static void *DetourF(BYTE *src, const BYTE *dst, const int len);
	static void  CleanRegistry();
	static DWORD GetProcessIdByNameW(const wchar_t *pProcessName);
	static DWORD GetModuleSize( char *pModuleName );
};



#endif