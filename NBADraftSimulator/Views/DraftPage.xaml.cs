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
            // Reset UI
            lblAnnuncio.Opacity = 0;
            lblAnnuncio.Scale = 1;
            lblAnnuncio.Text = $"La scelta numero {numeroScelta} va a...";
            lblAnnuncio.TextColor = Colors.White;

            imgLogo.Opacity = 0;
            lblSquadra.Opacity = 0;

            // Mostra annuncio iniziale
            await lblAnnuncio.FadeTo(1, 500);

            // Countdown con effetti
            for (int i = 3; i > 0; i--)
            {
                lblCountdown.Text = i.ToString();
                lblCountdown.Opacity = 0;
                lblCountdown.Scale = 0.5;

                // Animazione ingrandimento
                await Task.WhenAll(
                    lblCountdown.FadeTo(1, 400),
                    lblCountdown.ScaleTo(2.5, 400, Easing.SpringOut)
                );

                await Task.WhenAll(
                    lblCountdown.FadeTo(0, 300),
                    lblCountdown.ScaleTo(0.5, 300)
                );

                // Vibrazione per l'ultimo numero (gestione errori inclusa)
                if (i == 1)
                {
                    try
                    {
                        // Verifica se la vibrazione è disponibile prima di usarla
                        if (Vibration.Default.IsSupported)
                        {
                            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ignora silenziosamente - la vibrazione non è fondamentale
                        System.Diagnostics.Debug.WriteLine($"Vibrazione non disponibile: {ex.Message}");
                    }
                }
            }

            // Effetto finale
            lblCountdown.Text = "🏀";
            lblCountdown.TextColor = Colors.Orange;
            await lblCountdown.FadeTo(1, 200);
            await lblCountdown.FadeTo(0, 200);

            // Mostra squadra
            lblSquadra.Text = squadra.Nome.ToUpper();
            lblSquadra.TextColor = Color.FromArgb(squadra.ColorePrimario);

            // Mostra logo
            try
            {
                if (!string.IsNullOrEmpty(squadra.LogoPath))
                {
                    imgLogo.Source = squadra.LogoPath;
                }
                else
                {
                    imgLogo.Source = "team_default.png";
                }
            }
            catch
            {
                imgLogo.Source = "team_default.png";
            }

            // Animazione finale
            await Task.WhenAll(
                imgLogo.FadeTo(1, 600),
                lblSquadra.FadeTo(1, 600),
                lblSquadra.ScaleTo(1.5, 600, Easing.SpringOut)
            );

            await lblSquadra.ScaleTo(1.0, 300);

            // Aggiorna annuncio
            lblAnnuncio.Text = $"CON LA SCELTA #{numeroScelta}";
            lblAnnuncio.TextColor = Color.FromArgb(squadra.ColoreSecondario);
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