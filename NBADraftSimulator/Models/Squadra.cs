using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel; // Per INotifyPropertyChanged
using System.Runtime.CompilerServices; // Per CallerMemberName

namespace NBADraftSimulator.Models
{
    public class Squadra : BindableObject  // <-- Eredita da BindableObject
    {
        private string _nome;
        public string Nome
        {
            get => _nome;
            set
            {
                if (_nome != value)
                {
                    _nome = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _peso = 50; // Valore default
        public int Peso
        {
            get => _peso;
            set
            {
                if (_peso != value)
                {
                    _peso = value;
                    OnPropertyChanged();  // <-- NOTIFICA IL CAMBIAMENTO!
                }
            }
        }

        private string _colorePrimario = "#1D428A";
        public string ColorePrimario
        {
            get => _colorePrimario;
            set
            {
                if (_colorePrimario != value)
                {
                    _colorePrimario = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _coloreSecondario = "#C8102E";
        public string ColoreSecondario
        {
            get => _coloreSecondario;
            set
            {
                if (_coloreSecondario != value)
                {
                    _coloreSecondario = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _logoPath = "team_default.png";
        public string LogoPath
        {
            get => _logoPath;
            set
            {
                if (_logoPath != value)
                {
                    _logoPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _eStataEstratta = false;
        public bool EStataEstratta
        {
            get => _eStataEstratta;
            set
            {
                if (_eStataEstratta != value)
                {
                    _eStataEstratta = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _escludiDaUltimaScelta = false;
        public bool EscludiDaUltimaScelta
        {
            get => _escludiDaUltimaScelta;
            set
            {
                if (_escludiDaUltimaScelta != value)
                {
                    _escludiDaUltimaScelta = value;
                    OnPropertyChanged();
                }
            }
        }

        // Per MVVM
        public override string ToString()
        {
            return $"{Nome} (Peso: {Peso})";
        }
    }
}