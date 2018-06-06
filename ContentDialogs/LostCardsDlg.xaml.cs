using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class LostCardsDlg : ContentDialog
    {

        public ObservableCollection<PlayerData> PlayerDataList { get; } = new ObservableCollection<PlayerData>();

        public LostCardsDlg()
        {
            this.InitializeComponent();
        }

        public LostCardsDlg(IList<PlayerData> playerData)
        {
            this.InitializeComponent();

            PlayerDataList.AddRange(playerData);
            this.Height = PlayerDataList.Count * 75 + 200;

        }

        private void OnClose(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void Text_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == null) return;

            tb.SelectAll();
        }
    }
}
