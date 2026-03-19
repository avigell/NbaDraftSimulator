using Microsoft.Maui.Media;
using Plugin.Maui.Audio;
using NBADraftSimulator.Models;
using NBADraftSimulator.Services;
using System.Collections.ObjectModel;

namespace NBADraftSimulator.Views
{
    public partial class DraftPage : ContentPage
    {
        private DraftService _draftService;
        private List<Squadra> _squadre;
        private bool _staAnimando = false;

        // NUOVO: Gestione audio
        private IAudioPlayer _drumrollPlayer;
        private IAudioPlayer _applausePlayer;
        private IAudioPlayer _buzzerPlayer;
        private IAudioManager _audioManager;

        public DraftPage(DraftService draftService, List<Squadra> squadre)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            _draftService = draftService;
            _squadre = squadre;  // <-- PRIMA assegna _squadre

            _draftService.InizializzaDraft(_squadre);

            // POI puoi usare _squadre.Count
            lblNumeroScelta.Text = $"PICK #{_squadre.Count}";
            btnProssimaScelta.IsVisible = true;
            lblFineDraft.IsVisible = false;

            // NUOVO: Inizializza audio
            InizializzaAudio();
        }

        // NUOVO: Metodo per caricare i suoni
        private async void InizializzaAudio()
        {
            try
            {
                _audioManager = AudioManager.Current;

                var drumrollStream = await FileSystem.OpenAppPackageFileAsync("drum-roll.wav");
                _drumrollPlayer = _audioManager.CreatePlayer(drumrollStream);

                var applauseStream = await FileSystem.OpenAppPackageFileAsync("applause.wav");
                _applausePlayer = _audioManager.CreatePlayer(applauseStream);

                var buzzerStream = await FileSystem.OpenAppPackageFileAsync("buzzer.mp3");
                _buzzerPlayer = _audioManager.CreatePlayer(buzzerStream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore caricamento audio: {ex.Message}");
            }
        }

        private async void OnProssimaSceltaClicked(object sender, EventArgs e)
        {
            if (_staAnimando) return;

            if (_draftService.HaProssimaScelta())
            {
                _staAnimando = true;
                btnProssimaScelta.IsVisible = false;

                var squadra = _draftService.EstraiProssimaScelta();
                int sceltaCorrente = _draftService.NumeroSceltaCorrente - 1;
                int totaleSquadre = _squadre.Count;
                int numeroSceltaDaMostrare = totaleSquadre - sceltaCorrente; 

                await MostraSceltaConSuspence(squadra, numeroSceltaDaMostrare);

                _staAnimando = false;

                if (_draftService.HaProssimaScelta())
                {
                    btnProssimaScelta.IsVisible = true;
                    int prossimoNumeroDaMostrare = totaleSquadre - (sceltaCorrente + 1);
                    lblNumeroScelta.Text = $"PICK #{prossimoNumeroDaMostrare}";
                }
                else
                {
                    // Draft completato - VAI AL RIEPILOGO
                    var ordineCompleto = _draftService.GetOrdineCompleto();
                    var squadreOriginali = _squadre; // Le squadre con i pesi originali

                    // Naviga alla pagina di riepilogo
                    Navigation.PushAsync(new RiepilogoPage(ordineCompleto, squadreOriginali));
                }
            }
        }

        private async Task MostraSceltaConSuspence(Squadra squadra, int numeroScelta)
        {
            if (squadra == null) return;

            // === 1. RESET UI ===
            lblAnnuncio.Opacity = 0;
            lblAnnuncio.Scale = 1;
            lblAnnuncio.Text = $"LA SCELTA NUMERO {numeroScelta} VA A...";
            lblAnnuncio.TextColor = Colors.White;

            imgLogo.Opacity = 0;
            imgLogo.Scale = 0.1;
            lblSquadra.Opacity = 0;

            // Assicurati che il logo temporaneo sia nascosto
            imgSquadraLogo.IsVisible = false;

            // === 2. ANNUNCIO INIZIALE ===
            _applausePlayer?.Stop();
            await lblAnnuncio.FadeTo(1, 500);
            await Parla($"La scelta numero {numeroScelta} va, a");
            _drumrollPlayer?.Play();
            

            // === 3. FASE DI SUSPENSE (RULLO DI TAMBURI + COUNTDOWN) ===
            

            for (int i = 5; i > 0; i--)
            {
                lblCountdown.Text = i.ToString();
                lblCountdown.Opacity = 0;
                lblCountdown.Scale = 0.5;

                await Task.WhenAll(
                    lblCountdown.FadeTo(1, 400),
                    lblCountdown.ScaleTo(2.5, 400, Easing.SpringOut)
                );

                await Task.WhenAll(
                    lblCountdown.FadeTo(0, 300),
                    lblCountdown.ScaleTo(0.5, 300)
                );
            }

            _drumrollPlayer?.Stop();

            // === 4. MOMENTO CLIMATICO (BUZZER + LOGO SQUADRA GIGANTE) ===
            _buzzerPlayer?.Play();

            // Mostra il logo della squadra IN GRANDE al posto del pallone
            try
            {
                imgSquadraLogo.Source = squadra.LogoPath;  // Usa il logo della squadra corrente
            }
            catch
            {
                imgSquadraLogo.Source = "team_default.png";
            }

            imgSquadraLogo.Opacity = 0;
            imgSquadraLogo.Scale = 0.1;
            imgSquadraLogo.IsVisible = true;

            // Animazione: il logo appare gigante e poi svanisce
            //await Task.WhenAll(
            //    imgSquadraLogo.FadeTo(1, 200),
            //    imgSquadraLogo.ScaleTo(4.0, 200, Easing.SpringOut)
            //);

            await Task.WhenAll(
                imgSquadraLogo.FadeTo(0, 300),
                imgSquadraLogo.ScaleTo(0.1, 300)
            );

            imgSquadraLogo.IsVisible = false;

            // === 5. RESET COUNTDOWN ===
            lblCountdown.Text = "5";
            lblCountdown.FontSize = 72;
            lblCountdown.TextColor = Color.FromArgb("#C8102E");

            // === 6. RIVELAZIONE SQUADRA ===
            
            // Mostra il logo normale
            try
            {
                imgLogo.Source = squadra.LogoPath;
            }
            catch
            {
                imgLogo.Source = "team_default.png";
            }

            // Animazione logo normale
            await Task.WhenAll(
                imgLogo.FadeTo(1, 600),
                imgLogo.ScaleTo(1.2, 600, Easing.SpringOut)
            );
            await imgLogo.ScaleTo(1.0, 200);
            await Parla(squadra.Nome);
            // Animazione nome squadra
            lblSquadra.Text = squadra.Nome.ToUpper();
            lblSquadra.TextColor = Color.FromArgb(squadra.ColorePrimario);

            await Task.WhenAll(
                lblSquadra.FadeTo(1, 500),
                lblSquadra.ScaleTo(1.3, 500, Easing.SpringOut)
            );
            await lblSquadra.ScaleTo(1.0, 300);

            // === 7. FINALE ===
            lblAnnuncio.Text = $"CON LA SCELTA #{numeroScelta}";
            lblAnnuncio.TextColor = Color.FromArgb(squadra.ColoreSecondario);

            _applausePlayer?.Play();
        }

        private async Task Parla(string testo)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(testo))
                {
                    // Ottieni la lista delle lingue disponibili
                    var locales = await TextToSpeech.Default.GetLocalesAsync();

                    // Cerca l'italiano
                    var italiano = locales.FirstOrDefault(l => l.Language == "it");

                    var settings = new SpeechOptions
                    {
                        Volume = 1.0f,      // Massimo volume
                        Pitch = 1.0f,        // Tono normale
                        Locale = italiano    // Usa la lingua italiana se disponibile
                    };

                    await TextToSpeech.Default.SpeakAsync(testo, settings);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore sintesi vocale: {ex.Message}");
            }
        }

        // Gestione back button
        protected override bool OnBackButtonPressed()
        {
            // Torna alla configurazione
            Navigation.PopAsync();
            return true;
        }
    }
}