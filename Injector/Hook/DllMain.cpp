#include "stdafx.h"

#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "psapi.lib")
//#pragma comment(lib, "detours/lib.x86/detours.lib")
#pragma comment(lib, "detours_old/lib/detours.lib")

/*###############################################*/
/*################# DETOUR FUNCS ################*/
/*###############################################*/

typedef HRESULT (WINAPI* tconnect)(
	_In_ SOCKET s,
	     _In_ const struct sockaddr* name,
	     _In_ int namelen
);
tconnect oconnect;

typedef VOID (WINAPI * tOutputDebugStringA)(
	_In_opt_ LPCSTR lpOutputString
);

tOutputDebugStringA oOutputDebugStringA;

typedef VOID (WINAPI * tOutputDebugStringW)(
	_In_opt_ LPCWSTR lpOutputString
);

tOutputDebugStringA oOutputDebugStringW;

int WINAPI ConnectHook(SOCKET s, const struct sockaddr* name, int namelen)
{
	sockaddr_in* service = (sockaddr_in*)name;

	// We only care about login server.
	if (ntohs(service->sin_port) == 11005)
	{
		service->sin_addr.s_addr = inet_addr("127.0.0.1");
		service->sin_port = htons(11005);

		printf("original request rewritten to: %s:%d\n", inet_ntoa(service->sin_addr), ntohs(service->sin_port));
	}
	else
		printf("connect request to: %s:%d\n", inet_ntoa(service->sin_addr), ntohs(service->sin_port));

	return oconnect(s, name, namelen);
}

void WINAPI OutputDebugStringAHook(LPCSTR lpOutputString)
{
	printf("%s", lpOutputString);
}

void WINAPI OutputDebugStringWHook(LPCWSTR lpOutputString)
{
	printf("%ls", lpOutputString);
}

const int versionMajor = 1;
const int versionMinor = 0;

static DWORD dwModSize;
static DWORD dwEXEBase;
static DWORD dwD3DBase;

void killProcessByName(const char *filename)
{
	HANDLE hSnapShot = CreateToolhelp32Snapshot(TH32CS_SNAPALL, NULL);
	PROCESSENTRY32 pEntry;
	pEntry.dwSize = sizeof(pEntry);
	BOOL hRes = Process32First(hSnapShot, &pEntry);
	while (hRes)
	{
		if (strcmp(pEntry.szExeFile, filename) == 0)
		{
			HANDLE hProcess = OpenProcess(PROCESS_TERMINATE, 0,
				static_cast<DWORD>(pEntry.th32ProcessID));
			if (hProcess != NULL)
			{
				bool processKilled = false;
				while (!processKilled)
					TerminateProcess(hProcess, 9);

				CloseHandle(hProcess);
			}
		}
		hRes = Process32Next(hSnapShot, &pEntry);
	}
	CloseHandle(hSnapShot);
}

BOOL WINAPI consoleHandler(DWORD signal) {

	if (signal == CTRL_C_EVENT || signal == CTRL_CLOSE_EVENT)
	{
		printf("Killing DC Processes");

		killProcessByName("gameguard.des");
		killProcessByName("gamemon.des");
		killProcessByName("gamemon64.des");
		killProcessByName("driftcity.exe");

		ExitThread(9);
	}

	return TRUE;
}

HRESULT DCNCHook(LPVOID lpReserved)
{
	LPTSTR cmd = GetCommandLine();
	printf("Started game with %s\n", cmd);
	printf("Loading DCNC Hook Version %d.%d\n", versionMajor, versionMinor);

	dwEXEBase = reinterpret_cast<DWORD>(GetModuleHandle(nullptr));
	dwD3DBase = reinterpret_cast<DWORD>(GetModuleHandleA("d3d9.dll"));

	if (dwEXEBase != 0 && dwD3DBase != 0)
	{
#if _DEBUG
		AllocConsole();
		AttachConsole(GetCurrentProcessId());
		freopen("CON", "w", stdout);
		COORD cordinates = {80, 32766};
		HANDLE handle = GetStdHandle(STD_OUTPUT_HANDLE);
		SetConsoleScreenBufferSize(handle, cordinates);

		if (!SetConsoleCtrlHandler(consoleHandler, TRUE)) {
			printf("\nERROR: Could not set control handler");
			return 1;
		}
#endif

		if (!VALID(dwModSize))
		{
			printf("Getting Game Size... \n");
			dwModSize = CTools::GetModuleSize(nullptr);
			Sleep(500);
			printf("<Size>DWORD: %p \n\n", dwModSize);
		}

		printf("Hooking functions..\n");
		Sleep(2000);

		DWORD connect = reinterpret_cast<DWORD>(GetProcAddress(GetModuleHandleA("WS2_32.dll"), "connect"));
		oconnect = reinterpret_cast<tconnect>(DetourFunction(reinterpret_cast<PBYTE>(connect), reinterpret_cast<PBYTE>(ConnectHook)));

#if _DEBUG
		DWORD outputDebugStringA = reinterpret_cast<DWORD>(GetProcAddress(GetModuleHandleA("Kernel32.dll"), "OutputDebugStringA"));
		oOutputDebugStringA = reinterpret_cast<tOutputDebugStringA>(DetourFunction(reinterpret_cast<PBYTE>(outputDebugStringA), reinterpret_cast<PBYTE>(OutputDebugStringAHook)));

		DWORD outputDebugStringW = reinterpret_cast<DWORD>(GetProcAddress(GetModuleHandleA("Kernel32.dll"), "OutputDebugStringW"));
		oOutputDebugStringW = reinterpret_cast<tOutputDebugStringA>(DetourFunction(reinterpret_cast<PBYTE>(outputDebugStringW), reinterpret_cast<PBYTE>(OutputDebugStringWHook)));
#endif
	}
	return 1;
}

BOOL WINAPI DllMain(HINSTANCE hInstDll, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	UNREFERENCED_PARAMETER(lpReserved);

	//DisableThreadLibraryCalls(hModule);7
	// this will fail.
	/*HidePEB(hInstDll);
	RemovePeHeader(hInstDll);
	TCHAR szFileName[MAX_PATH + 1];
	GetModuleFileName(nullptr, szFileName, MAX_PATH + 1);
	UnlinkModule(szFileName);*/

#if _DEBUG
	AllocConsole();
	AttachConsole(GetCurrentProcessId());
	freopen("CON", "w", stdout);
	/*
	freopen("CONIN$", "r", stdin);
	freopen("CONOUT$", "w", stdout);
	freopen("CONOUT$", "w", stderr);
	*/
	COORD cordinates = { 80, 32766 };
	HANDLE handle = GetStdHandle(STD_OUTPUT_HANDLE);
	SetConsoleScreenBufferSize(handle, cordinates);

	if (!SetConsoleCtrlHandler(consoleHandler, TRUE)) {
		printf("\nERROR: Could not set control handler");
		return 1;
	}
#endif

	if (ul_reason_for_call != DLL_PROCESS_ATTACH) return true;

	DisableThreadLibraryCalls(hInstDll);
	CreateThread(nullptr, 0, reinterpret_cast<LPTHREAD_START_ROUTINE>(DCNCHook), hInstDll, 0, nullptr);
	return true;
}