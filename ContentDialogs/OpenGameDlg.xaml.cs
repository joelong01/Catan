﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage.Search;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{




    public sealed partial class OpenGameDlg : ContentDialog
    {
        public ObservableCollection<string> SavedGames { get; set; } = new ObservableCollection<string>();


        public OpenGameDlg()
        {
            this.InitializeComponent();
        }

        public async Task LoadGames()
        {

            QueryOptions queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, new[] { MainPage.SAVED_GAME_EXTENSION })
            {
                FolderDepth = FolderDepth.Shallow
            };
            Windows.Storage.StorageFolder folder = await StaticHelpers.GetSaveFolder();
            StorageFileQueryResult query = folder.CreateFileQueryWithOptions(queryOptions);
            System.Collections.Generic.IReadOnlyList<Windows.Storage.StorageFile> files = await query.GetFilesAsync();
            foreach (Windows.Storage.StorageFile file in files)
            {
                SavedGames.Add(file.Name);
            }
        }

        public string SavedGame => _lbGames.SelectedValue as String;

        private void ContentDialog_OnOk(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void ContentDialog_OnCancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void Game_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


    }


}
