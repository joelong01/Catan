using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class ErrorDlg : ContentDialog
    {
        public ErrorDlg()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(ErrorDlg), new PropertyMetadata(""));
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(ErrorDlg), new PropertyMetadata(""));
        public static readonly DependencyProperty ExtendedMessageProperty = DependencyProperty.Register("ExtendedMessage", typeof(string), typeof(ErrorDlg), new PropertyMetadata(""));
        public string ExtendedMessage
        {
            get => (string)GetValue(ExtendedMessageProperty);
            set => SetValue(ExtendedMessageProperty, value);
        }
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }
        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private Visibility HideIfEmpty(string s)
        {
            if (String.IsNullOrEmpty(s)) return Visibility.Collapsed;
            return Visibility.Visible;
        }

    }
}
