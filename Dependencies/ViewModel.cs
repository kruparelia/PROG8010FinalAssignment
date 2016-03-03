/*
    PROG8010 Group 2, Final Assignment: Dependency Manager
    Julia Aryal Sharma, 7375934
    Oscar Lucero, 7177884
    Kunal Ruparelia, 7128416    
    Charles Troster, 7388085
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dependencies
{
    public class ViewModel : INotifyPropertyChanged
    {
        DependencyModel dm;

        private ObservableCollection<string> outputList;
        public ObservableCollection<string> OutputList
        {
            get { return outputList; }
            set { outputList = value; OnPropertyChanged(); }
        }
        
        public void ReadFile(string filePath)
        {
            // For a new opened file, start a new dependency model
            dm = new DependencyModel();
            dm.ParseFile(filePath);
            OutputList = dm.Outputs;
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
}
