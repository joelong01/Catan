using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Catan10
{


	public class MainPageModel : INotifyPropertyChanged
	{
		
        public MainPageModel() 
		{
			_Log.PropertyChanged += Log_PropertyChanged;
		}

		private void Log_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			// this.TraceMessage($"Log_PropertyChanged: [Property={e.PropertyName}] [GameState={_Log.GameState}");
			if (e.PropertyName == "GameState")
			{
				
				NotifyPropertyChanged("EnableNextButton");
				NotifyPropertyChanged("EnableRedo");
			}

			
		}

		private ObservableCollection<PlayerModel> _PlayingPlayers = new ObservableCollection<PlayerModel>();
		[JsonIgnore]
		public ObservableCollection<PlayerModel> PlayingPlayers
		{
			get
			{
				return _PlayingPlayers;
			}
			set
			{
				if (_PlayingPlayers != value)
				{
					_PlayingPlayers = value;
					NotifyPropertyChanged();
				}
			}
		}



		bool _EnableUiInteraction = true;
		public bool EnableUiInteraction
		{
			get
			{
				return _EnableUiInteraction;
			}
			set
			{
				if (_EnableUiInteraction != value)
				{
					_EnableUiInteraction = value;
					NotifyPropertyChanged();
					NotifyPropertyChanged("EnableRedo");
					NotifyPropertyChanged("EnableNextButton");
				}
			}
		}

		
		public bool EnableNextButton
		{
			get
			{
				GameState state = _Log.GameState;
				return (EnableUiInteraction && (state == GameState.WaitingForNewGame || state == GameState.WaitingForNext || state == GameState.WaitingForStart ||
						state == GameState.DoneSupplemental || state == GameState.Supplemental || state == GameState.AllocateResourceForward || state == GameState.AllocateResourceReverse ||
						state == GameState.DoneResourceAllocation));
				
			}			
		}



		
		public bool EnableRedo
		{
			get
			{
				return (Log.UndoCount > 0 && EnableUiInteraction);
			}			
		}

		Log _Log = new Log();
		[JsonIgnore]
		public Log Log
		{
			get
			{
				return _Log;
			}
			set
			{
				if (_Log != value)
				{
					_Log = value;
					NotifyPropertyChanged();
				}
			}
		}

		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}