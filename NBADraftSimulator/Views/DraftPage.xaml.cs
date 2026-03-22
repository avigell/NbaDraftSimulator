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

        // Audio
        private IAudioPlayer _drumrollPlayer;
        private IAudioPlayer _applausePlayer;
        private IAudioPlayer _buzzerPlayer;
        private IAudioManager _audioManager;

        public DraftPage(DraftService draftService, List<Squadra> squadre)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            // Imposta l'anno corrente nel titolo
            var labelTitolo = this.FindByName<Label>("lblTitoloDraft");
            if (labelTitolo != null)
            {
                labelTitolo.Text = $"DRAFT {DateTime.Now.Year}";
            }

            _draftService = draftService;
            _squadre = squadre;

            _draftService.InizializzaDraft(_squadre);

            // Inizializza UI - il bottone è sempre visibile (opacity 1)
            btnProssimaScelta.Opacity = 1;
            btnProssimaScelta.IsEnabled = true;
            lblFineDraft.Opacity = 0;

            // Inizializza audio
            InizializzaAudio();
        }

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

                // NASCONDI il bottone (ma mantiene lo spazio)
                btnProssimaScelta.Opacity = 0;
                btnProssimaScelta.IsEnabled = false;

                var squadra = _draftService.EstraiProssimaScelta();
                int sceltaCorrente = _draftService.NumeroSceltaCorrente - 1;
                int totaleSquadre = _squadre.Count;
                int numeroSceltaDaMostrare = totaleSquadre - sceltaCorrente;

                await MostraSceltaConSuspence(squadra, numeroSceltaDaMostrare);

                _staAnimando = false;

                if (_draftService.HaProssimaScelta())
                {
                    // MOSTRA il bottone
                    btnProssimaScelta.Opacity = 1;
                    btnProssimaScelta.IsEnabled = true;
                    int prossimoNumeroDaMostrare = totaleSquadre - (sceltaCorrente + 1);
                }
                else
                {
                    // Draft completato - VAI AL RIEPILOGO
                    await Task.Delay(2000);
                    var ordineCompleto = _draftService.GetOrdineCompleto();
                    var squadreOriginali = _squadre;

                    // Navigazione sicura sul thread principale in .NET MAUI
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            // Piccola pausa per lasciare completare le animazioni
                            await Task.Delay(100);
                            await Navigation.PushAsync(new RiepilogoPage(ordineCompleto, squadreOriginali));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Errore navigazione: {ex.Message}");
                        }
                    });
                }
            }
        }
        private async void OnVaiAlRiepilogo(object sender, EventArgs e)
        { 
        }

        private async Task MostraSceltaConSuspence(Squadra squadra, int numeroScelta)
        {
            if (squadra == null) return;

            var isDark = Application.Current.RequestedTheme == AppTheme.Dark;
            var coloreSquadra = Color.FromArgb(squadra.ColorePrimario);

            // RESET UI
            lblAnnuncio.Opacity = 0;
            lblAnnuncio.Scale = 1;
            var testoParlato = $"La scelta numero {numeroScelta} va a";

            // TESTO PERSONALIZZATO PER LA PRIMA SCELTA
            if (numeroScelta == 1)
            {
                lblAnnuncio.Text = $"LA PRIMA SCELTA ASSOLUTA VA A...";
                testoParlato = $"Nicola Iòkich va a";
            }
            else
            {
                lblAnnuncio.Text = $"LA SCELTA NUMERO {numeroScelta} VA A...";
                //testoParlato = $"Nicola Iòkich va a";
            }

            lblAnnuncio.TextColor = isDark ? Colors.White : Colors.Black;

            imgLogo.Opacity = 0;
            imgLogo.Scale = 0.1;
            lblSquadra.Opacity = 0;
            lblSquadra.TextColor = isDark
                ? coloreSquadra.WithLuminosity(0.85f)
                : coloreSquadra.WithLuminosity(0.3f);

            imgSquadraLogo.IsVisible = false;

            // Parto il TTS e il drumroll in parallelo senza bloccare le animazioni
            _ = Parla(testoParlato);
            _drumrollPlayer?.Play();

            await lblAnnuncio.FadeTo(1, 400, Easing.CubicInOut);

            // COUNTDOWN + flash effetto ESPN
            for (int i = 5; i > 0; i--)
            {
                lblCountdown.Text = i.ToString();
                lblCountdown.Opacity = 0;
                lblCountdown.Scale = 0.5;

                await Task.WhenAll(
                    lblCountdown.FadeTo(1, 350, Easing.CubicIn),
                    lblCountdown.ScaleTo(2.5, 350, Easing.SpringOut)
                );

                // Glow/flash effetto ESPN
                lblCountdown.TextColor = Color.FromArgb("#FFD700");
                await Task.Delay(50);
                lblCountdown.TextColor = Color.FromArgb("#C8102E");

                await Task.WhenAll(
                    lblCountdown.FadeTo(0, 250),
                    lblCountdown.ScaleTo(0.5, 250)
                );
            }

            _drumrollPlayer?.Stop();

            // BUZZER + LOGO GIGANTE
            _buzzerPlayer?.Play();
            //try { imgSquadraLogo.Source = squadra.LogoPath; }
            //catch { imgSquadraLogo.Source = "team_default.png"; }

            //imgSquadraLogo.IsVisible = true;
            //imgSquadraLogo.Opacity = 0;
            //imgSquadraLogo.Scale = 0.1;

            //await Task.WhenAll(
            //    imgSquadraLogo.FadeTo(0, 200),
            //    imgSquadraLogo.ScaleTo(0.1, 200)
            //);
            //imgSquadraLogo.IsVisible = false;

            // RESET countdown
            lblCountdown.Text = "5";
            lblCountdown.FontSize = 72;
            lblCountdown.TextColor = Color.FromArgb("#C8102E");

            // RIVELAZIONE SQUADRA
            try { imgLogo.Source = squadra.LogoPath; }
            catch { imgLogo.Source = "team_default.png"; }

            await Task.WhenAll(
                imgLogo.FadeTo(1, 600),
                imgLogo.ScaleTo(1.2, 600, Easing.SpringOut)
            );
            await imgLogo.ScaleTo(1.0, 200);

            lblSquadra.Text = squadra.Nome.ToUpper();

            await Task.WhenAll(
                lblSquadra.FadeTo(1, 500),
                lblSquadra.ScaleTo(1.3, 500, Easing.SpringOut)
            );
            await lblSquadra.ScaleTo(1.0, 300);

            _ = Parla(squadra.Nome);

            _applausePlayer?.Play();
            _ = FadeOutAudioAsync(_applausePlayer, 3000);
        }

        private async Task FadeOutAudioAsync(IAudioPlayer player, int durataMs)
        {
            if (player == null || !player.IsPlaying) return;

            int step = 50;
            int steps = durataMs / step;
            double volumeIniziale = player.Volume;

            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                double easing = t * t;
                player.Volume = volumeIniziale * (1 - easing);
                await Task.Delay(step);
            }

            player.Stop();
            player.Volume = volumeIniziale;
        }

        private async Task Parla(string testo)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(testo))
                {
                    await TextToSpeech.Default.SpeakAsync(testo);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore sintesi vocale: {ex.Message}");
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return true;
        }
    }
}