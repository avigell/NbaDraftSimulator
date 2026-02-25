using NBADraftSimulator.Models;
using NBADraftSimulator.Services;
using NBADraftSimulator.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NBADraftSimulator.ViewModels
{
    public class MainViewModel : BindableObject
    {
        private ObservableCollection<Squadra> _squadre;
        private DraftService _draftService;
        private bool _puoiIniziareDraft;

        public MainViewModel()
        {
            _draftService = new DraftService();
            _squadre = new ObservableCollection<Squadra>();

            // Squadre predefinite (modificabili)
            CaricaSquadreDefault();

            StartDraftCommand = new Command(StartDraft, CanStartDraft);
            ResetCommand = new Command(ResetDraft);
            AggiungiSquadraCommand = new Command(AggiungiSquadra);
            RimuoviSquadraCommand = new Command<Squadra>(RimuoviSquadra);
        }

        private void CaricaSquadreDefault()
        {
            _squadre.Add(new Squadra { Nome = "Lakers", Peso = 100, ColorePrimario = "#552583", LogoPath = "lakers.png" });
            _squadre.Add(new Squadra { Nome = "Celtics", Peso = 90, ColorePrimario = "#007A33", LogoPath = "celtics.png" });
            _squadre.Add(new Squadra { Nome = "Bulls", Peso = 80, ColorePrimario = "#CE1141", LogoPath = "bulls.png" });
            _squadre.Add(new Squadra { Nome = "Warriors", Peso = 70, ColorePrimario = "#1D428A", LogoPath = "warriors.png" });
            _squadre.Add(new Squadra { Nome = "Heat", Peso = 60, ColorePrimario = "#98002E", LogoPath = "heat.png" });
            _squadre.Add(new Squadra { Nome = "Spurs", Peso = 50, ColorePrimario = "#000000", LogoPath = "spurs.png" });
            _squadre.Add(new Squadra { Nome = "Suns", Peso = 40, ColorePrimario = "#1D1160", LogoPath = "suns.png" });
            _squadre.Add(new Squadra { Nome = "Bucks", Peso = 30, ColorePrimario = "#00471B", LogoPath = "bucks.png" });
        }

        public ObservableCollection<Squadra> Squadre
        {
            get => _squadre;
            set
            {
                _squadre = value;
                OnPropertyChanged();
            }
        }

        public bool PuoiIniziareDraft
        {
            get => _squadre.Count >= 2;
            set
            {
                _puoiIniziareDraft = value;
                OnPropertyChanged();
                ((Command)StartDraftCommand).ChangeCanExecute();
            }
        }

        public ICommand StartDraftCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand AggiungiSquadraCommand { get; }
        public ICommand RimuoviSquadraCommand { get; }

        private bool CanStartDraft()
        {
            return _squadre.Count >= 2;
        }

        private async void StartDraft()
        {
            var draftPage = new DraftPage(_draftService, _squadre.ToList());
            await Application.Current.MainPage.Navigation.PushAsync(draftPage);
        }

        private void ResetDraft()
        {
            Squadre.Clear();
            CaricaSquadreDefault();
        }

        private void AggiungiSquadra()
        {
            // Apre popup per aggiungere squadra personalizzata
            Application.Current.MainPage.DisplayPromptAsync(
                "Nuova Squadra",
                "Inserisci nome squadra:",
                "OK",
                "Annulla")
                .ContinueWith(async (task) =>
                {
                    if (!string.IsNullOrWhiteSpace(task.Result))
                    {
                        var nome = task.Result;
                        var pesoStr = await Application.Current.MainPage.DisplayPromptAsync(
                            "Peso",
                            $"Inserisci peso per {nome} (1-100):",
                            "OK",
                            "Annulla",
                            "50",
                            -1,
                            Keyboard.Numeric);

                        if (int.TryParse(pesoStr, out int peso))
                        {
                            peso = Math.Clamp(peso, 1, 100);

                            Device.BeginInvokeOnMainThread(() =>
                            {
                                Squadre.Add(new Squadra
                                {
                                    Nome = nome,
                                    Peso = peso,
                                    ColorePrimario = "#808080",
                                    LogoPath = "team_default.png"
                                });
                                PuoiIniziareDraft = Squadre.Count >= 2;
                            });
                        }
                    }
                });
        }

        private void RimuoviSquadra(Squadra squadra)
        {
            if (squadra != null && Squadre.Contains(squadra))
            {
                Squadre.Remove(squadra);
                PuoiIniziareDraft = Squadre.Count >= 2;
            }
        }
    }
}