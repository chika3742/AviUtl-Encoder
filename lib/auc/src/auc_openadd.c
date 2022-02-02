#include <stdio.h>
#include <windows.h>
#include "auc.h"

static char usage[] = "usage: auc_openadd [#window] <filename>";

int main(int argc, char** argv)
{
  HWND hwnd, hwndp, hwnds, hwndb, hwndcbx, hwndcb, hwnde;
  int i;
  
  if(argc < 2 || argc > 3) error_exit(usage);
  
  if(argc == 3)
    hwnd = gethwnd_str(argv[1]);
  else
    hwnd = gethwnd();
  
  PostMessage(hwnd, WM_COMMAND, 5100, 0);
  
  hwnds = getdlghwnd(hwnd);
  
  hwndb   = FindWindowEx(hwnds,   NULL, "Button", "ŠJ‚­(&O)");
  hwndcbx = FindWindowEx(hwnds,   NULL, "ComboBoxEx32", NULL);
  if (hwndcbx == NULL) {
    hwndcbx = FindWindowEx(hwnds,   NULL, "DUIViewWndClassName", NULL);
    if (hwndcbx) hwndcbx = FindWindowEx(hwndcbx, NULL, "DirectUIHWND", NULL);
    if (hwndcbx) hwndcbx = FindWindowEx(hwndcbx, NULL, "FloatNotifySink", NULL);
  }
  hwndcb  = FindWindowEx(hwndcbx, NULL, "ComboBox", NULL);
  hwnde   = FindWindowEx(hwndcb,  NULL, "Edit", NULL);
  SendMessage(hwnde, WM_SETTEXT, 0, (LPARAM)argv[argc - 1]);
  SendMessage(hwndb, WM_LBUTTONDOWN, 0, 0);
  SendMessage(hwndb, WM_LBUTTONUP, 0, 0);
  
  return 0;
}
