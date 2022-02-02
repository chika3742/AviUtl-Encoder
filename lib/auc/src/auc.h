static void error_exit(LPTSTR m)
{
  printf("%s\n", m);
  exit(-1);
}

static HWND gethwnd_str(char* s)
{
  HWND hwnd, hwnds;
  
  hwnds = (HWND)atoi(s);
  hwnd  = NULL;
  do{
    hwnd = FindWindowEx(NULL, hwnd, "AviUtl", NULL);
    if(hwnd == NULL) error_exit("Not AviUtl window.");
  }while(hwnd != hwnds);
  if((HWND)GetWindowLong(hwnd, GWL_HWNDPARENT) != NULL) error_exit("Not AviUtl root window.");
  
  return hwnd;
}

static HWND gethwnd(void)
{
  HWND hwnd, hwndp;
  
  if((hwnd = FindWindow("AviUtl", NULL)) == NULL) error_exit("No AviUtl active.");
  while(hwndp = (HWND)GetWindowLong(hwnd, GWL_HWNDPARENT))
    hwnd = hwndp;
  
  return hwnd;
}

static HWND getdlghwnd(HWND hwnd)
{
  HWND hwndd, hwndp;
  int i;
  
  Sleep(500);
  for(i = 0; ; i++){
    hwndd = FindWindowEx(NULL, NULL, "#32770", NULL);
    hwndp = (HWND)GetWindowLong(hwndd, GWL_HWNDPARENT);
    while(hwndd != 0 && hwndp != hwnd){
      hwndd = FindWindowEx(NULL, hwndd, "#32770", NULL);
      hwndp = (HWND)GetWindowLong(hwndd, GWL_HWNDPARENT);
    }
    if(hwndd != 0 && hwndp == hwnd) break;
    if(i >= 20) error_exit("Dialog of output-plgin didnt appear.");
    Sleep(500);
  }
  Sleep(2000);
  
  return hwndd;
}
