   M S B A M L   `   `   `  ����I  EAviUtl Encoder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:AUEncoder	AUEncoder  5   	   !    AUEncoder.SelectFilesWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:AUEncoder     5   	     � Title$  ファイル選択��5   	   $	��450��6   $	��400��6%    � 
ResizeMode$ NoResize=�61   .��5	      � .+�5
   
   �� -   5         ��Name$ 	item_list��5
      $��Center=�6%   $	��272��6B   $��0,10,0,0q�6O   $	��Top=�6a   $	��374��6y    �� SelectionMode$ Multiple=�6�   $��14(�6�    J� 	AllowDrop . 6  �� 5   
   -   5         ��Name$ 
add_button��5      $��まとめて追加��6%   $
��Left=�66   $��
10,303,0,0q�6Q   $	��Top=�6e   $��92��6}   $��16(�6�   $	��0,4q�6�   �� 5   
   -   5        $ delete_button��5      $��リストから削除��6(   $
��Left=�6:   $��129,303,0,0q�6U   $	��Top=�6j   $��94��6�   $��16(�6�   {�.  $	��0,4q�6�   �� 5   
   -   5        $ 	OK_Button��5      $��OK��6$   $��Right=�61   $��
0,0,125,10q�6M   $��Bottom=�6a   $��92��6|   $��16(�6�   6�   �� 5   
   -   5        $ Cancel_Button��5      $��キャンセル��6(   $��Right=�68   $��	0,0,10,10q�6T   $��Bottom=�6g   $��92��6�   $��16(�6�   $	��0,4q�6�   �� 5   
   -   5        $ Delete_All_Button��5      $��リストを空にする��6,   $
��Left=�6?   $��247,303,0,0q�6Z   $	��Top=�6o   $	��119��6�   $��16(�6�   $	��0,4q�6�   �� 5   
   $
��Left=�6   $��
10,282,0,0q�6/   $
��Wrap=�6C   $$��D&Dすることもできます��6W   $	��Top=�6o   ���	#B20000006�   5      5      