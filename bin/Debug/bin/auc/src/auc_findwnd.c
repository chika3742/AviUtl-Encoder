#include <stdio.h>
#include <windows.h>

static void error_exit(LPTSTR m)
{
  printf("%s\n", m);
  exit(-1);
}

static char usage[] = "usage: auc_findwnd [<caption>]";

int main(int argc, char** argv)
{
  HWND hwnd, hwndp;
  int i;
  
  if(argc < 1 || argc > 2) error_exit(usage);
  
  if(argc == 1)
    hwnd = FindWindow("AviUtl", NULL);
  else
    hwnd = FindWindow("AviUtl", argv[1]);
  
  if(hwnd != NULL){
    while(hwndp = (HWND)GetWindowLong(hwnd, GWL_HWNDPARENT))
      hwnd = hwndp;
  }
  
  printf("%d\n", (int)hwnd);
  
  return hwnd;
}
