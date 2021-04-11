using Client.Views;
using GalaSoft.MvvmLight.Command;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Client
{
    public class MainWindowViewModel: INotifyPropertyChanged
    {
        #region Public Propreties

        public ObservableCollection<PositionViewModel> Positions { get; set; }
        public ObservableCollection<DroneViewModel> Drones { get; set; }
        public List<DroneViewModel> RechargingDrones
        {
            get
            {
                return Drones.Where(d => d.State == DroneState.Recharging || d.State == DroneState.Waiting).ToList();
            }
        }
        public Visibility CanAddDrone { 
            get 
            {
                if (CordenatorTask != null) return Visibility.Visible; else return Visibility.Hidden;
            } 
        }

        #endregion

        #region Constuctors

        public MainWindowViewModel()
        {
            Positions = new ObservableCollection<PositionViewModel>();
            Drones = new ObservableCollection<DroneViewModel>();
        }

        #endregion

        #region Commands

        public ICommand NewCommand
        {
            get
            {
                if(_newCommand == null)
                {
                    _newCommand = new RelayCommand(() => NewCommand_Executed());
                }
                return _newCommand;
            }
        }

        public ICommand AddDroneCommand
        {
            get
            {
                if (_addDroneCommand == null)
                {
                    _addDroneCommand = new RelayCommand(() => AddDroneCommand_Executed());
                }
                return _addDroneCommand;
            }
        }
        
        #endregion

        #region Private Methods

        private void RefreshViews()
        {
            RaisePropertyChanged(nameof(RechargingDrones));
            RaisePropertyChanged(nameof(CanAddDrone));
        }

        private void AddDroneCommand_Executed()
        {
            Drones.Add(
                new DroneViewModel(
                    new Drone() { Name = "Drone " + Drones.Count(), Battery = 100 }
                    )
                );
            RefreshViews();
        }

        private void NewCommand_Executed()
        {
            _newSimulationView = new NewSimulationView();
            NewSimulationViewModel vm = new NewSimulationViewModel();
            vm.OnStartSimulation += OnStartSimulation;
            _newSimulationView.DataContext = vm;
            _newSimulationView.Show();
        }

        private void OnStartSimulation(int positions, int drones)
        {
            SetPositions(positions);
            SetDrones(drones);
            _newSimulationView.Close();
            _newSimulationView = null;
            RunSimulation();
        }

        private void SetPositions(int count)
        {
            Random random = new Random();
            
            Positions.Clear();
            for(int i = 0; i < count; i++)
            {
                int threatLevel = random.Next(0, 100);
                Positions.Add(
                    new PositionViewModel(
                        new Position() { Name = "Position "+i, ThreatLevel = threatLevel, ThreatLevelColor = "#"+ threatLevel + "ff0000"}
                        )
                    );
            }
        }

        private void SetDrones(int count)
        {
            Drones.Clear();
            for(int i = 0; i < count; i++)
            {
                Drone drone = new Drone() { Name = "Drone " + i, Battery = 100 };
                DroneViewModel DroneVM = new DroneViewModel(drone); 
                Drones.Add(DroneVM);
            }
        }

        private void RunSimulation()
        {
            CordenatorTask = Task.Run(async () => {
                for(; ; )
                {
                    Cordinator();
                    RefreshViews();
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });

            RechargeStationTask = Task.Run(async () => {

                for(; ; )
                {
                    Drones.Where(d => d.State == DroneState.Recharging).ToList().ForEach((drone) => {
                        if (drone.Battery < 100)
                            drone.Battery += 1;
                        else
                            drone.State = DroneState.Waiting;
                    });
                    RefreshViews();
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            });

            
        }

        private void Cordinator()
        {
            Positions.OrderByDescending(p => p.Position.ThreatLevel).ToList().ForEach((position) => {

                if (position.ActiveDrone != null && position.ActiveDrone.Battery <= 10)
                {
                    var drone = position.ActiveDrone;
                    drone.State = DroneState.Recharging;
                    position.ActiveDrone = null;
                }
                else if (position.ActiveDrone == null && Drones.Where(d => d.State == DroneState.Waiting).Count() != 0)
                {
                    var drone = Drones.Where(d => d.State == DroneState.Waiting).FirstOrDefault();
                    drone.State = DroneState.OnGarde;
                    position.ActiveDrone = drone;
                }
                else if (position.ActiveDrone == null && Drones.Where(d => d.State == DroneState.Waiting).Count() == 0)
                {
                    var found = Positions.Where(p => p.ActiveDrone != null && p.Position.ThreatLevel < position.Position.ThreatLevel)
                                           .OrderBy(p => p.Position.ThreatLevel).FirstOrDefault();
                    if (found != null)
                    {
                        var newDrone = found.ActiveDrone;
                        found.ActiveDrone = null;
                        position.ActiveDrone = newDrone;
                    }
                }
                else if (position.ActiveDrone != null && position.ActiveDrone.Battery > 10)
                {
                    position.ActiveDrone.Battery -= (int)Math.Round(position.Position.ThreatLevel * 0.1);
                }
            });
        }

        #endregion

        #region Protected Methods

        protected void RaisePropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Private Propreties

        private ICommand _newCommand, _addDroneCommand;
        private NewSimulationView _newSimulationView;
        private Task CordenatorTask, RechargeStationTask;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }


    public class DroneViewModel : INotifyPropertyChanged
    {

        #region Public Propreties

        public Drone Drone { get; set; }

        public DroneState State { get; set; } = DroneState.Waiting;

        public string Name
        {
            get { return Drone.Name; }
        }

        public int Battery
        {
            get { return Drone.Battery; }
            set {
                Drone.Battery = value;
                RaisePropertyChanged("Battery");
            }
        }

        #endregion


        #region Constructor

        public DroneViewModel(Drone drone)
        {
            Drone = drone;
        }

        #endregion

        #region Protected Methods

        protected void RaisePropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Private Prorreties

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }

    public class PositionViewModel : INotifyPropertyChanged
    {
        #region Public Propreties
        public Position Position { get; set; }

        private DroneViewModel _activeDrone;
        public DroneViewModel ActiveDrone
        {
            get { return _activeDrone; }
            set { _activeDrone = value; RaisePropertyChanged(nameof(ActiveDrone)); RaisePropertyChanged(nameof(HasActiveDrone)); }
        }

        public Visibility HasActiveDrone
        {
            get { if (ActiveDrone != null) return Visibility.Visible; else return Visibility.Hidden; }
        }

        public string ThreatLevelColor
        {
            get { return Position.ThreatLevelColor; }
        }

        #endregion

        #region Constructors

        public PositionViewModel(Position position)
        {
            Position = position;
        }

        #endregion

        #region Protected Methods

        protected void RaisePropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Private Prorreties

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }

    public enum DroneState: int
    {
        OnGarde = 0,
        Recharging = 1,
        Waiting = 2
    }
}
