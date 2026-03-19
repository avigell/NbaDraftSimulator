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
            _squadre.Add(new Squadra { Nome = "The Dogtors", Peso = 1, ColorePrimario = "#FFFFFF", LogoPath = "logo02.png" });
            _squadre.Add(new Squadra { Nome = "Fankulez", Peso = 1, ColorePrimario = "#FFFFFF", LogoPath = "logo06.png" });
            _squadre.Add(new Squadra { Nome = "The Fluffers", Peso = 1, ColorePrimario = "#FFFFFF", LogoPath = "logo03.png" });
            _squadre.Add(new Squadra { Nome = "CallMeMamba", Peso = 1, ColorePrimario = "#FFFFFF", LogoPath = "logo01.png" });
            _squadre.Add(new Squadra { Nome = "Wu-Tang Clan", Peso = 1, ColorePrimario = "#FFFFFF", LogoPath = "logo01.png" });
            _squadre.Add(new Squadra { Nome = "Average Joe's", Peso = 1, ColorePrimario = "#FFFFFF", LogoPath = "logo05.png" });
            _squadre.Add(new Squadra { Nome = "Slam Dunkerz", Peso = 1, ColorePrimario = "#FFFFFF", LogoPath = "logo04.png" });
            _squadre.Add(new Squadra { Nome = "San Candido Sinners", Peso = 1, ColorePrimario = "#FFFFFF", LogoPath = "logo01.png" });
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