using NBADraftSimulator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NBADraftSimulator.Views
{
    public partial class RiepilogoPage : ContentPage
    {
        public class ItemOrdine
        {
            public int NumeroScelta { get; set; }
            public string Nome { get; set; }
            public int PesoOriginale { get; set; }
        }

        public class ItemProbabilita
        {
            public string Nome { get; set; }
            public double Probabilita { get; set; }
            public Color Colore { get; set; }
        }

        public RiepilogoPage(List<Squadra> ordineFinale, List<Squadra> squadreOriginali)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            // Prepara ordine finale: dalla prima scelta (pick #1) in cima
            var ordine = new ObservableCollection<ItemOrdine>();
            int totale = ordineFinale.Count;

            // Scorri l'ordine finale al contrario
            for (int i = totale - 1; i >= 0; i--)
            {
                var squadra = ordineFinale[i];
                int numeroScelta = totale - i; // 1,2,3,4,5,6,7,8

                ordine.Add(new ItemOrdine
                {
                    NumeroScelta = numeroScelta,
                    Nome = squadra.Nome,
                    PesoOriginale = squadra.Peso
                });

                // Salva il vincitore (numero scelta 1)
                if (numeroScelta == 1)
                {
                    lblVincitore.Text = squadra.Nome.ToUpper();
                    lblVincitore.TextColor = Color.FromArgb(squadra.ColorePrimario);
                }
            }

            cvOrdine.ItemsSource = ordine;

            // Calcola probabilità iniziali
            int totalePesi = squadreOriginali.Sum(s => s.Peso);
            var probabilita = new ObservableCollection<ItemProbabilita>();

            // Ordina per probabilità decrescente (chi aveva più probabilità di essere ultimo)
            foreach (var squadra in squadreOriginali.OrderByDescending(s => s.Peso))
            {
                double perc = (double)squadra.Peso / totalePesi * 100;
                probabilita.Add(new ItemProbabilita
                {
                    Nome = squadra.Nome,
                    Probabilita = perc,
                    Colore = Color.FromArgb(squadra.ColorePrimario)
                });
            }

            cvProbabilita.ItemsSource = probabilita;
        }

        private void OnHomeClicked(object sender, EventArgs e)
        {
            // Torna alla MainPage (svuota lo stack)
            Navigation.PopToRootAsync();
        }

        private void OnNuovoDraftClicked(object sender, EventArgs e)
        {
            // Torna alla MainPage per iniziare un nuovo draft
            Navigation.PopToRootAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            // Torna alla MainPage
            Navigation.PopToRootAsync();
            return true;
        }
    }
}