'''
''' TSエンコードのバッチ登録 サンプルスクリプト
'''
' 引数のファイル全部について、
' BonTsDemuxで分割してAviUtlにバッチ登録するスクリプト
' TSファイルをドラッグし、VBSにドロップして使用する
' バッチ出力中のAviUtlに対して、バッチを追加登録できる

On Error Resume Next

Set Args = WScript.Arguments

' テンポラリパス
Const TMP_FOLDER = "C:\MOVTEMP\"

' エンコードしたデータを出力するフォルダ(最後の文字は"\")
Const OUTPUT_FOLDER = "D:\VIDEO\"

' プロファイル番号(メニューの一番上が0)
Const OUTPUT_PROFILE = 0

' 出力プラグイン番号(メニューの一番上が0)
Const OUTPUT_PLUGIN = 2

' 出力ファイルの拡張子
Const OUTPUT_EXT = ".wmv"

' BonTsDemuxのフルパス
Const BonTsDemux_PATH = "C:\Program Files\BonTsDemux\BonTsDemux.exe"

' mmeのフルパス
Const mme_PATH = "C:\Program Files\m2vfp\mme.exe"

' AviUtlのフルパス
Const AVIUTL_PATH = "C:\Program Files\aviutl\aviutl.exe"

' AviUtl Controlを置いてあるフォルダ(最後の文字は"\")
Const AUC_FOLDER = "C:\Program Files\auc\" ' = スクリプトと同じフォルダ

Dim WHSHell, oExec, Fs
Set WSHShell = WScript.CreateObject("WScript.Shell")
Set Fs = CreateObject("Scripting.FileSystemObject")

Function Auc(command, arg)
  Auc = WSHShell.Run("""" & AUC_FOLDER & "auc_" & command & ".exe"" " & arg, 2, True)
End Function

Dim i, tmp, tmpm2v, tmpwav, tmpgl, output, pid, hwnd

' AviUtlを起動
hwnd = Auc("exec", """" & AVIUTL_PATH & """")
If hwnd = 0 Then
  Call WScript.Quit(-1)
End if

For i=0 to Args.Count - 1
  tmp = TMP_FOLDER & Fs.GetBaseName(Args(i))
  tmpm2v = tmp & ".m2v"
  tmpwav = tmp & ".wav"
  tmpgl = tmp & ".gl"
  output = OUTPUT_FOLDER & Fs.GetBaseName(Args(i)) & OUTPUT_EXT
  
  ' TSファイルを分割
  Call WSHShell.Run("""" & BonTsDemux_PATH & """" & " -i """ & Args(i) & """ -o """ & tmp & """ -vf -start -quit", 0, True)
  
  ' glファイルを作成
  Call WSHShell.Run("""" & mme_PATH & """ -g -q """ & tmpm2v & """", 0, True)
  
  ' M2Vファイルを開く
  Call Auc("open", CStr(hwnd) & " """ & tmpm2v & """")
  Call WScript.Sleep(3000)
  
  ' WAVファイルを開く
  Call Auc("audioadd", CStr(hwnd) & " """ & tmpwav & """")
  Call WScript.Sleep(3000)
  
  ' プロファイルを設定する
  Call Auc("setprof", CStr(hwnd) & " " & CStr(OUTPUT_PROFILE))
  Call WScript.Sleep(3000)
  
  ' 出力プラグインから出力する
  Call Auc("plugbatch", CStr(hwnd) & " " & CStr(OUTPUT_PLUGIN) & " """ & output & """")
  Call WScript.Sleep(3000)
  
  ' ファイルを閉じる
  Call Auc("close", CStr(hwnd))
  
  ' テンポラリファイルを削除する
  Call Fs.Delete(tmpm2v)
  Call Fs.Delete(tmpwav)
  Call Fs.Delete(tmpgl)

Next

' スクリプトで起動したAviUtlを終了する
Call Auc("exit", CStr(hwnd))

