   M S B A M L   `   `   `  ����  MyHybridApp    MyHybridApp.MainWindow   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5        � Title$  WPF App��5       � WindowState$ 	Maximized=�5       � WindowStyle$ SingleBorderWindow=�5       � 
ResizeMode$ 	CanResize=�5      ���#1E1E1E � 	Resources 5         AccentBrush �� Color �� 
TargetType �� Property �� Value	 �� 
TargetType
 �� CornerRadius �� Triggers P� Property P� Value&   System.Windows.Controls.DataGrid=  7System.Windows.Controls.Primitives.DataGridColumnHeader��%O  &	        '�� !     '�� �     '  �    '  U    �� 5   
   $ #00C896��6.   �� 5   
   " ��6   .�5      ��  � �� �#D0FFD66+   �� 5       � ��$ Segoe UI)�6+   �� 5       � ��$ 14(�6)   5      �� 5   
   " ��6   .�5      ��  � ��
 ���� 6+   �� 5       � �� �#D0FFD66+   �� 5       � ��$ Segoe UI Semibold)�6+   �� 5       � �� �#00C8966,   �� 5       � ��$ 1q�60   �� 5       � ��$	 8,4q�6(   �� 5       �@�Cursor$
 Hand��6'   �� 5       � ��$ 4q�6'   �� 5       � �� 5      �� 5      "	 ��6&   .R�5      �� #��z��6!   #��z��5    !   #��z��5!   !   $
 4��5"   !   ._�5#      �� $��Center=�6/   $��Center=�6L   5$       5%      P� 5&       �@J�IsMouseOver$
 True��6=   . �5'   "   ��  � �� �#66FFE06@   �� 5(   "    � ��
 �����6?   5)      5+      5-      5.      �� 5/   
   "  6   .�50      ��  � �� �#2525266+   �� 51       � �� �#D0FFD66+   �� 52       � ��$ Segoe UI)�6+   �� 53       � ��$ 13(�6)   �� 54       �@ RowBackground �#1E1E1E6.   �� 55        �@ AlternatingRowBackground �#2D2D3069   �� 56       �@ GridLinesVisibility$ 
Horizontal=�64   �� 57       �@ HeadersVisibility$ Column=�62   58      �� 59   
   "  6   .�5:      ��  � �� �#2D2D306+   �� 5;       � �� �#00C8966+   �� 5<       � ��$
 Bold"�6+   �� 5=       � ��$ Segoe UI)�6+   5>      5?      .��5A      �� $��
0,0,0,-226q�6   .��5B   
   �� $��20q�6    �� Orientation$ Vertical=�6!   .+�5T      �� -   5         ��Name$ IsCenterText��5T      $8��2Центральний вузол активний��5U      $��18(�6:   $
��Bold"�6H   ���#00C896$��Center=�6o   �� 5X      $ 
Horizontal=�6   $��Center=�62   .+�5Y      �� -   5        $2��,Відключитися від мережі��5Y      $	��200��6;   $��29��6G   6S   5Z      �� 5]      $ 
Horizontal=�6   $��Center=�62   .+�5^      �� -   5        $ PeerCountText��5^      $(��"Кількість вузлів: 0��5_      $��0,0,10,0q�67   �� 5`      -   5        $ TaskCountText��5`      $&�� Кількість задач: 0��5a      $��0,0,10,0q�66   �� 5b      -   5        $ TaskResultText��5b      $E��?Кількість проміжних результатів: 0��5c      $��0,0,10,0q�6G   5d      �� 5g      $ 
Horizontal=�6   $��Stretch=�62   $��0,10,0,0q�6P   .+�5h      �� $!��Статуси вузлів��6   $
��Bold"�62   $	��400��6D    �� TextAlignment$ Center=�6P   �� 5i      $��Задачі��6   $
��Bold"�6*   $	��400��6<   $ Center=�6H   �� 5j      $��Результати��6   $
��Bold"�6.   $	��400��6@   $ Center=�6L   5k      A� 5m       A� Columns$ 3��6   $��0,5,0,0q�6&   $	��319��67   .+�5n        -   5          Name$ 
StatusGrid��5n         AutoGenerateColumns$ False��6/   $��	0,10,0,10q�6K   $	��384��6^      Columns 5o      0  *System.Windows.Controls.DataGridTextColumn  5p      ,  &System.Windows.Controls.DataGridColumn   Header$ 
Вузол��6)      MinWidth$	 120Q�6U   1  +System.Windows.Controls.DataGridBoundColumn   Binding 68   �� *
ClientId+6U     5q      $ CPU %��6)   $ 80Q�6U    68   �� *
CpuUsage+6U     5r      $ Пам’ять (MB)��6)   $	 100Q�6_    6?   �� *MemoryUsage+6_     5s      $ Роль��6)   $	 100Q�6X    67   �� *isCenterView+6X   5u        5w      -   5        $ 	TasksGrid��5w      $ False��6.   $��	0,10,0,10q�6J    5x        5y      $ ID��6-   $ 50Q�6T    69   �� *TaskId+6T     5z      $ Статус��6-   $ 50Q�6X    6=   �� *Status+6X     5{      $ Тип��6-   $ 50Q�6S    6:   �� *Type+6S     5|      $ Результат��6-   $ 80Q�6[    6@   �� *Result+6[     5}      $ Підзадачі��6-   $ 80Q�6d    6@   �� *CountOfSubTasks+6d     5~      $ 
Вираз��6-   $ 50Q�6[    6<   �� *
Expression+6[     5      $ Час��6-   $ 50Q�6X    6:   �� *	Timestamp+6X   5�        5�      -   5        $ 
ResultGrid��5�      $ False��6/   $��	0,10,0,10q�6K   $	��384��6^    5�        5�      $ Отримувач��6-   $	 120Q�6c    6@   �� *TargetClientId+6c     5�      $ Результат��6-   $	 100Q�6[    6@   �� *Result+6[     5�      $ 
Вираз��6-   $	 120Q�6[    6<   �� *
Expression+6[     5�      $ Час��6-   $ 10Q�6X    6:   �� *	Timestamp+6X   5�      5�      �� 5�      $��	Логи:��6   $��0,10,0,0q�6%   �� 5�      -	   5         ��Name$ LogBox��5�       �� AcceptsReturn$
 True��6&   " �� VerticalScrollBarVisibility$
 Auto=�6;   ���#1E1E1E���#D0FFD6$	��100��5�   @   $��Consolas)�5�      $��12(�6,   5�      5�      5�      