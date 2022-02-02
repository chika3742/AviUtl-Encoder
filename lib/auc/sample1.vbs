'''
''' AviUtlエンコード＆ファイル移動 サンプルスクリプト
'''
' あるフォルダの中のファイル全部について、
' プロファイル＆出力プラグイン指定でエンコードし、
' エンコードが終わったら指定のフォルダに移動するスクリプト

On Error Resume Next

' キャプチャしたファイルがあるフォルダ(最後の文字は"\")
Const SOURCE_FOLDER  = "D:\CAPTURE\"

' エンコードが終わったファイルを移すフォルダ(最後の文字は"\")
Const MOVE_FOLDER    = "D:\MOVED\"

' エンコードしたデータを出力するフォルダ(最後の文字は"\")
Const OUTPUT_FOLDER  = "D:\ENCODED\"

' プロファイル番号(メニューの一番上が0)
Const OUTPUT_PROFILE = 0

' 出力プラグイン番号(メニューの一番上が0)
Const OUTPUT_PLUGIN  = 0

' 出力ファイルの拡張子
Const OUTPUT_EXT     = ".wmv"

' AviUtlのフルパス
Const AVIUTL_PATH    = "C:\Program Files\aviutl\aviutl.exe"

' AviUtl Controlを置いてあるフォルダ(最後の文字は"\")
Const AUC_FOLDER     = ".\" ' = スクリプトと同じフォルダ

Dim WHSHell, Fs
Set WSHShell = WScript.CreateObject("WScript.Shell")
Set Fs = CreateObject("Scripting.FileSystemObject")

Function Auc(command, arg)
  Auc = WSHShell.Run("""" & AUC_FOLDER & "auc_" & command & ".exe"" " & arg, 2, True)
End Function

Dim srcFiles, srcFile, date, input, output, i, hwnd, open

' AviUtlのウィンドウ番号を取得

hwnd = Auc("findwnd", "")
open = 0
If hwnd = 0 Then
  hwnd = Auc("exec", """" & AVIUTL_PATH & """")
  If hwnd = 0 Then
    Call WScript.Quit(-1)
  End if
  open = 1
End if

' キャプチャしたファイルがあるフォルダが空になるまで繰り返す
While Fs.GetFolder(SOURCE_FOLDER).Files.Count > 0

  ' SOURCE_FOLDER で一番古いファイルを探す
  Set srcFiles = Fs.GetFolder(SOURCE_FOLDER).Files
  i = 0
  For Each srcFile In srcFiles
    If i = 0 Then
      date   = srcFile.DateLastModified
      input  = srcFile.Path
      output = OUTPUT_FOLDER & Fs.GetBaseName(input) & OUTPUT_EXT
      i = 1
    Elseif date < oFile.DateLastModified then
      date   = srcFile.DateLastModified
      input  = srcFile.Path
      output = OUTPUT_FOLDER & Fs.GetBaseName(input) & OUTPUT_EXT
    End if
  Next
  
  ' キャプチャしたファイルを開く
  Call Auc("open",    CStr(hwnd) & " """ & input & """")
  Call WScript.Sleep(3000)
  
  ' プロファイルを設定する
  Call Auc("setprof", CStr(hwnd) & " " & CStr(OUTPUT_PROFILE))
  Call WScript.Sleep(1000)
  
  ' 出力プラグインから出力する
  Call Auc("plugout", CStr(hwnd) & " " & CStr(OUTPUT_PLUGIN) & " """ & output & """")
  
  ' 出力が終わるまで待つ
  Call Auc("wait",    CStr(hwnd))
  
  ' ファイルを閉じる
  Call Auc("close",   CStr(hwnd))
  Call WScript.Sleep(3000)
  
  ' エンコードが終わったファイルを移動する
  Call Fs.MoveFile(input, MOVE_FOLDER)

Wend

' スクリプトで起動したAviUtlを終了する
If open = 1 Then
  Call Auc("exit", CStr(hwnd))
End if

