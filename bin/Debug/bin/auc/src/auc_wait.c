#include <stdio.h>
#include <windows.h>
#include "auc.h"

static char usage[] = "usage: auc_wait [#window]";

int main(int argc, char** argv)
{
  HANDLE hprc;
  HWND hwnd, hwndp;
  TCHAR buf[256];
  int i;
  
  if(argc < 1 || argc > 2) error_exit(usage);
  
  if(argc == 2)
    hwnd = gethwnd_str(argv[1]);
  else
    hwnd = gethwnd();
  
  hprc = GetCurrentProcess();
  SetPriorityClass(hprc, IDLE_PRIORITY_CLASS);
  
  for(i = 0; i < 7; i++){
    GetWindowText(hwnd, buf, 256);
    if(strncmp(buf, "o—Í’†", 6) == 0 || strncmp(buf, "ŒŸõ’†", 6) == 0){
      Sleep(5000);
      i = -1;
    }else
      Sleep(500);
  }
  
  return 0;
}
