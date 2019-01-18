#include <stdio.h>
#include <windows.h>
#include "auc.h"

static char usage[] = "usage: auc_verclose [#window]";

int main(int argc, char** argv)
{
  HWND hwnd, hwndp, hwnds, hwndb;
  int i;
  
  if(argc < 1 || argc > 2) error_exit(usage);
  
  if(argc == 2)
    hwnd = gethwnd_str(argv[1]);
  else
    hwnd = gethwnd();
  
  hwnds = FindWindowEx(NULL, NULL, "#32770", "バージョン情報");
  hwndp = (HWND)GetWindowLong(hwnds, GWL_HWNDPARENT);
  while(hwnds != 0 && hwndp != hwnd){
    hwnds = FindWindowEx(NULL, hwnds, "#32770", "バージョン情報");
    hwndp = (HWND)GetWindowLong(hwnds, GWL_HWNDPARENT);
  }
  if(hwnds == 0 || hwndp != hwnd) error_exit("Dialog not found.");
  
  hwndb = FindWindowEx(hwnds, NULL, "Button", "OK");
  if(hwndb == NULL) error_exit("Unexpected dialog.");
  
  SendMessage(hwndb, WM_LBUTTONDOWN, 0, 0);
  SendMessage(hwndb, WM_LBUTTONUP, 0, 0);
  
  return 0;
}
