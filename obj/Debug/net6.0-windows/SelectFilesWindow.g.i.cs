﻿#pragma checksum "..\..\..\SelectFilesWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "5FD069AD63B74B2AA20ECF31634C53046EB505CA"
//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

using AUEncoder;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace AUEncoder {
    
    
    /// <summary>
    /// SelectFilesWindow
    /// </summary>
    public partial class SelectFilesWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\..\SelectFilesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox item_list;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\..\SelectFilesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button add_button;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\..\SelectFilesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button delete_button;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\SelectFilesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button OK_Button;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\SelectFilesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Cancel_Button;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\SelectFilesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Delete_All_Button;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.1.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/AviUtl Encoder;component/selectfileswindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\SelectFilesWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.1.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.item_list = ((System.Windows.Controls.ListBox)(target));
            
            #line 10 "..\..\..\SelectFilesWindow.xaml"
            this.item_list.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ListBox_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 10 "..\..\..\SelectFilesWindow.xaml"
            this.item_list.PreviewDragOver += new System.Windows.DragEventHandler(this.Item_list_PreviewDragOver);
            
            #line default
            #line hidden
            
            #line 10 "..\..\..\SelectFilesWindow.xaml"
            this.item_list.Drop += new System.Windows.DragEventHandler(this.Item_list_Drop);
            
            #line default
            #line hidden
            return;
            case 2:
            this.add_button = ((System.Windows.Controls.Button)(target));
            
            #line 11 "..\..\..\SelectFilesWindow.xaml"
            this.add_button.Click += new System.Windows.RoutedEventHandler(this.Add_button_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.delete_button = ((System.Windows.Controls.Button)(target));
            
            #line 12 "..\..\..\SelectFilesWindow.xaml"
            this.delete_button.Click += new System.Windows.RoutedEventHandler(this.Delete_button_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.OK_Button = ((System.Windows.Controls.Button)(target));
            
            #line 13 "..\..\..\SelectFilesWindow.xaml"
            this.OK_Button.Click += new System.Windows.RoutedEventHandler(this.OK_Button_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.Cancel_Button = ((System.Windows.Controls.Button)(target));
            
            #line 14 "..\..\..\SelectFilesWindow.xaml"
            this.Cancel_Button.Click += new System.Windows.RoutedEventHandler(this.Cancel_Button_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.Delete_All_Button = ((System.Windows.Controls.Button)(target));
            
            #line 15 "..\..\..\SelectFilesWindow.xaml"
            this.Delete_All_Button.Click += new System.Windows.RoutedEventHandler(this.Delete_All_Button_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

