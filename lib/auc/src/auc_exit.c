#include <stdio.h>
#include <windows.h>
#include "auc.h"

static char usage[] = "usage: auc_exit [#window]";

int main(int argc, char** argv)
{
  HWND hwnd, hwndp;
  
  if(argc < 1 || argc > 2) error_exit(usage);
  
  if(argc == 2)
    hwnd = gethwnd_str(argv[1]);
  else
    hwnd = gethwnd();
  
  PostMessage(hwnd, WM_COMMAND, 1002, 0);
  
  return 0;
}
