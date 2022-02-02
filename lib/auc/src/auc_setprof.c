#include <stdio.h>
#include <windows.h>
#include "auc.h"

static char usage[] = "usage: auc_setprof [#window] #profile";

int main(int argc, char** argv)
{
  HWND hwnd, hwndp;
  int num, i;
  
  if(argc < 2 || argc > 3) error_exit(usage);
  if(argv[argc - 1][0] < '0' || argv[argc - 1][0] > '9') error_exit(usage);
  
  num = atoi(argv[argc - 1]);
  
  if(argc == 3)
    hwnd = gethwnd_str(argv[1]);
  else
    hwnd = gethwnd();
  
  PostMessage(hwnd, WM_COMMAND, 8006 + num, 0);
  
  return 0;
}
