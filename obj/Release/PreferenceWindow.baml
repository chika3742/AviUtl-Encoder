   M S B A M L   `   `   `  ���� I  EAviUtl Encoder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:AUEncoder	AUEncoder  R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R8clr-namespace:System.ComponentModel;assembly=WindowsBaseSystem.ComponentModel      AUEncoder.PreferenceWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     )localclr-namespace:AUEncoder     LWpfApp28clr-namespace:System.ComponentModel;assembly=WindowsBase     -     � Title$  設定��$��299.333��$	��636�� � 
ResizeMode$ NoResize=�.��� $��0,0,4,4q�.+��� $
��Left=�$��	25,13,0,0q�$
��Wrap=�$	��Top=�$��AviUtlのパス��$��14(� J� RenderTransformOrigin$ 0.464,0.167&��� -    ��Name$ 
AviutlPath��$��Right=�$��
0,11,177,0q�$	��Top=�$	��238�� �� 
IsReadOnly . �� -    ��Name$ aviutlPathOpen��$��参照��$��Right=�$��0,11,98.4,0q�$	��Top=�$��74�� � Cursor$
 Hand��$��21��$
��16,0q��� $
��Left=�$��	25,43,0,0q�$
��Wrap=�$	��Top=�$*��$プロファイルラベルの編集�� �� TextAlignment$ Center=�$��14(��� -    ��Name$ Prof_ComboBox��$��Right=�$��
0,36,243,0q�$	��Top=�$	��172��	 �� SelectedIndex$	 0��.@��� $$��＜選択してください＞���� $��新規追加������ .R��� 
 �� Orientation$
 
Horizontal=�.+��� ���� *Number+�� $	�� : ���� ���� *Name+�� -   $ 	Prof_Edit��$��編集��$��Right=�$��	0,37,79,0q�$	��Top=�$��64��{�.  $��21��$
��16,0q��� -   $ Prof_Delete��$��削除��$��Right=�$��	0,36,10,0q�$	��Top=�$��64��{�.  $��22��$
��16,0q��� $
��Left=�$��	24,67,0,0q�$
��Wrap=�$	��Top=�$'��!プラグインラベルの編集��$ Center=�$��14(��� -   $ Plug_ComboBox��$��Right=�$��
0,64,243,0q�$	��Top=�$	��172��$	 0������ .R��� $
 
Horizontal=�.+��� ���� *Number+�� $	�� : ���� ���� *Name+@��� $$��＜選択してください＞���� $��新規追加���� -   $ 	Plug_Edit��$��編集��$��Right=�$��	0,63,79,0q�$	��Top=�$��64��{�.  $��23��$
��16,0q��� -	   $ Plug_Delete��$��削除��$��Right=�$��	0,63,10,0q�$	��Top=�$��64��{�.  $��23���� $
��Left=�$��
149,14,0,0q�$
��Wrap=�$	��Top=�$��D&D可�����	#FF7A7A7A�� $
��Left=�$��	9,118,0,0q�$
��Wrap=�$	��Top=�$��16(�$��22��$ Center=��� $
��Left=�$��	25,93,0,0q�$
��Wrap=�$	��Top=�$!��出力進捗の取得間隔��$ Center=�$��14(��� -
   $ Progress_Interval��$��Right=�$��
0,90,243,0q�$	��Top=�$	��172��$	 2��$��11(�.@��� $��取得しない���� $��0.7秒���� $
��1秒���� $
��2秒���� $
��5秒���� $��10秒���� -    ��Name$ Follow_Settings_Checkbox��$Q��K出力の成功・失敗にかかわらず完了後の動作設定に従う��$
��Left=�$��
25,138,0,0q�$	��Top=�
���   �$��Center=�$
 Hand���� $��
0,67,186,0q�$
��Wrap=�$	��Top=�$��拡張子：��$��Right=����	#CC000000�� -    ��Name$	 ext��$��Right=�$��
0,64,153,0q�$
��Wrap=�$	��Top=�$��-��
���   �$��14(�$��29���� -   $ restore_after_process��$H��B起動時に「処理完了後の動作」の状態を復元する��$
��Left=�$��
25,158,0,0q�$	��Top=�$ -0.85,0.533&��� -   $ do_analize_checkbox��$Q��K開始時にファイルがAviUtlの設定に合っているか確認する��$
��Left=�$��
25,178,0,0q�$	��Top=�$ -0.85,0.533&��� $
��Left=�$��
25,201,0,0q�$
��Wrap=�$	��Top=�$3��-エンコード済みファイルの接尾辞��$ Center=�$��14(��� -   $ SuffixTextBox��$��Right=�$��0,201,288,0q�$	��Top=�$	��127���� $
��Left=�$��
25,220,0,0q�$
��Wrap=�$6��0※ffmpegエンコードモードでのみ動作��$	��Top=�$��11(����	#FF5D5D5D�� $
��Left=�$��
29,112,0,0q�$
��Wrap=�$	��Top=�$��11(����	#FF5D5D5D�� $	�※���� $�AviUtl���� $�モードでのみ���� $�機能��