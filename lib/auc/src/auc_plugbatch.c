#include <stdio.h>
#include <windows.h>
#include "auc.h"

static char usage[] = "usage: auc_plugout [#window] #plugin <filename>";

int main(int argc, char** argv)
{
  HWND hwnd, hwndp, hwnds, hwndb, hwndcbx, hwndcb, hwnde;
  int num, i;
  
  if(argc < 3 || argc > 4) error_exit(usage);
  if(argv[argc - 2][0] < '0' || argv[argc - 2][0] > '9') error_exit(usage);
  
  num = atoi(argv[argc - 2]);
  
  if(argc == 4)
    hwnd = gethwnd_str(argv[1]);
  else
    hwnd = gethwnd();
  
  PostMessage(hwnd, WM_COMMAND, 5296+num, 0);
  
  hwnds = getdlghwnd(hwnd);
  
  hwndb   = FindWindowEx(hwnds,   NULL, "#32770", NULL);
  hwndb   = FindWindowEx(hwndb,   NULL, "Button", "ƒoƒbƒ`“o˜^");
  hwndcbx = FindWindowEx(hwnds,   NULL, "ComboBoxEx32", NULL);
  if (hwndcbx == NULL) {
    hwndcbx = FindWindowEx(hwnds,   NULL, "DUIViewWndClassName", NULL);
    if (hwndcbx) hwndcbx = FindWindowEx(hwndcbx, NULL, "DirectUIHWND", NULL);
    if (hwndcbx) hwndcbx = FindWindowEx(hwndcbx, NULL, "FloatNotifySink", NULL);
  }
  hwndcb  = FindWindowEx(hwndcbx, NULL, "ComboBox", NULL);
  hwnde   = FindWindowEx(hwndcb,  NULL, "Edit", NULL);
  if(hwndb == NULL || hwnde == NULL) error_exit("Unexpected dialog.");
  
  DeleteFile(argv[argc - 1]);
  SendMessage(hwnde, WM_SETTEXT, 0, (LPARAM)argv[argc - 1]);
  SendMessage(hwndb, WM_LBUTTONDOWN, 0, 0);
  SendMessage(hwndb, WM_LBUTTONUP, 0, 0);
  
  return 0;
}
