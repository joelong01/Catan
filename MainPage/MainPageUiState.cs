﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    /// <summary>
    /// This file should contain the information necessary to deal with the UI state in MainPage
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Fields

        private readonly Stack<GameState> _stateStack = new Stack<GameState>();
        public static readonly DependencyProperty LastRollProperty = DependencyProperty.Register("LastRoll", typeof(int), typeof(MainPage), new PropertyMetadata(0));

        #endregion Fields

        #region Properties

        public int LastRoll
        {
            get => (int)GetValue(LastRollProperty);
            set => SetValue(LastRollProperty, value);
        }

        public List<int> Rolls { get; set; } = new List<int>();

        #endregion Properties

        #region Methods

      

        public async Task PlayerWon()
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }

       

        #endregion Methods
    }

    public class MenuTag
    {
        #region Constructors

        public MenuTag(PlayerModel p)
        {
            Player = p;
        }

        public MenuTag(int n)
        {
            Number = n;
        }

        public MenuTag(StorageFile f)
        {
            File = f;
        }

        public MenuTag(IList<MenuFlyoutItemBase> list)
        {
            PeerMenuItemList = list;
        }

        public MenuTag()
        {
        }

        #endregion Constructors

        #region Properties

        public StorageFile File { get; set; }
        public int Number { get; set; }
        public IList<MenuFlyoutItemBase> PeerMenuItemList { get; set; }

        public PlayerModel Player { get; set; }
        public bool SetKeyUpHandler { get; set; } = false;

        #endregion Properties
    }
}
