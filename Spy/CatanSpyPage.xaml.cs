using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Catan.Proxy;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10.Spy
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CatanSpyPage : Page
    {
        public static readonly DependencyProperty SelectedMessageProperty = DependencyProperty.Register("SelectedMessage", typeof(CatanMessage), typeof(CatanSpyPage), new PropertyMetadata(null, SelectedMessageChanged));
        public static readonly DependencyProperty LogHeaderJsonProperty = DependencyProperty.Register("LogHeaderJson", typeof(string), typeof(CatanSpyPage), new PropertyMetadata(""));
        public string LogHeaderJson
        {
            get => (string)GetValue(LogHeaderJsonProperty);
            set => SetValue(LogHeaderJsonProperty, value);
        }
        public CatanMessage SelectedMessage
        {
            get => (CatanMessage)GetValue(SelectedMessageProperty);
            set => SetValue(SelectedMessageProperty, value);
        }
        private static void SelectedMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as CatanSpyPage;
            var depPropValue = (CatanMessage)e.NewValue;
            depPropClass?.SetSelectedMessage(depPropValue);
        }
        private void SetSelectedMessage(CatanMessage message)
        {
            LogHeader logHeader = message.Data as LogHeader;
            string json = CatanProxy.Serialize<LogHeader>(logHeader, true);
        }

        public CatanSpyPage()
        {
            this.InitializeComponent();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e == null) return;
            if (e.AddedItems.Count == 0) return;
            LogHeader logHeader = ((CatanMessage)e.AddedItems[0]).Data as LogHeader;
            LogHeaderJson = CatanProxy.Serialize<object>(logHeader, true);
        }
    }
}
