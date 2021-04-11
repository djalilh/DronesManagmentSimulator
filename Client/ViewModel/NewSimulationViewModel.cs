using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client
{
    public class NewSimulationViewModel
    {
        #region Public Events

        public Action<int, int> OnStartSimulation;

        #endregion

        #region Public Propreties

        public List<int> OneToTenList
        {
            get
            {
                if(_oneToTenList == null)
                {
                    _oneToTenList = new List<int>();
                    for(int i = 1; i <= 100; i++)
                    {
                        _oneToTenList.Add(i);
                    }
                }
                return _oneToTenList;
            }
        }

        public int Positions { get; set; }
        public int Drones { get; set; }

        #endregion

        #region Constructors

        public NewSimulationViewModel()
        {

        }

        #endregion

        #region Commands

        public ICommand StartCommand
        {
            get
            {
                if(_startCommand == null)
                {
                    _startCommand = new RelayCommand(() => StartCommand_Executed());
                }
                return _startCommand;
            }
        }

        #endregion

        #region Private Methods

        private void StartCommand_Executed()
        {
            if(Positions != 0 && Drones != 0)
            {
                OnStartSimulation?.Invoke(Positions, Drones);
            }
        }

        #endregion

        #region Private Propreties

        private List<int> _oneToTenList;
        private ICommand _startCommand;
        #endregion
    }
}
