using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBADraftSimulator.Models
{
    public class Squadra
    {
        public string Nome { get; set; }
        public int Peso { get; set; } = 50; // Valore default
        public string ColorePrimario { get; set; } = "#1D428A"; // Blu NBA default
        public string ColoreSecondario { get; set; } = "#C8102E"; // Rosso NBA default
        public string LogoPath { get; set; } = "team_default.png";
        public bool EStataEstratta { get; set; } = false;

        // Per MVVM
        public override string ToString()
        {
            return $"{Nome} (Peso: {Peso})";
        }
    }
}