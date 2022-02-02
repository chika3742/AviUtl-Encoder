#include <stdio.h>
#include <windows.h>

static void error_exit(LPTSTR m)
{
  printf("%s\n", m);
  exit(-1);
}

static char usage[] = "usage: auc_exec \"<command line>\"";

static HWND g_hwnd = 0;

BOOL CALLBACK EnumWindowsProc(HWND hwnd, LPARAM lParam)
{
  DWORD dwProcessId;
  char buf[8];
  
  GetWindowThreadProcessId(hwnd, &dwProcessId);
  if ( dwProcessId != (DWORD)lParam ) return TRUE;
  
  GetWindowText(hwnd, buf, 7);
  if ( lstrcmp ( buf, "AviUtl" ) ) return TRUE;
  
  g_hwnd = hwnd;
  return FALSE;
}

int main(int argc, char** argv)
{
  PROCESS_INFORMATION pi;
  STARTUPINFO si;
  HWND hwnd;
  DWORD pid;
  
  if(argc != 2) error_exit(usage);
  
  ZeroMemory(&si, sizeof(si));
  ZeroMemory(&pi, sizeof(pi));
  si.cb = sizeof(si);
  
  if ( !CreateProcess(NULL, (LPTSTR)argv[1], NULL, NULL, FALSE, NORMAL_PRIORITY_CLASS, NULL, NULL, &si, &pi) ) return 0;
  while ( EnumWindows(EnumWindowsProc, pi.dwProcessId) ) Sleep(100);
  printf("%d\n", (int)g_hwnd);
  
  CloseHandle(pi.hProcess);
  CloseHandle(pi.hThread);
  
  return g_hwnd;
}

