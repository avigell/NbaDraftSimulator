using NBADraftSimulator.Models;
using NBADraftSimulator.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            _squadre = squadre;

            _draftService.InizializzaDraft(_squadre);

            // Inizializza UI
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

                if (squadra == null)
                {
                    _staAnimando = false;
                    btnProssimaScelta.IsVisible = true;
                    return;
                }

                int totaleSquadre = _squadre.Count;
                int sceltaCorrente = _draftService.NumeroSceltaCorrente - 1;
                int numeroSceltaDaMostrare = totaleSquadre - sceltaCorrente;

                await MostraSceltaConSuspence(squadra, numeroSceltaDaMostrare);

                _staAnimando = false;

                if (_draftService.HaProssimaScelta())
                {
                    btnProssimaScelta.IsVisible = true;

                    int prossimaScelta = _draftService.NumeroSceltaCorrente;
                    int prossimoNumeroDaMostrare = totaleSquadre - prossimaScelta + 1;
                    lblNumeroScelta.Text = $"PICK #{prossimoNumeroDaMostrare}";
                }
                else
                {
                    // Draft completato!
                    lblFineDraft.IsVisible = true;
                    lblNumeroScelta.Text = "DRAFT COMPLETATO!";
                }
            }
        }

        private async Task MostraSceltaConSuspence(Squadra squadra, int numeroScelta)
        {
            if (squadra == null) return;

            // Reset UI
            lblAnnuncio.Opacity = 0;
            lblAnnuncio.Scale = 1;
            lblAnnuncio.Text = $"LA SCELTA NUMERO {numeroScelta} VA A...";
            lblAnnuncio.TextColor = Colors.White;

            imgLogo.Opacity = 0;
            imgLogo.Scale = 0.1;
            lblSquadra.Opacity = 0;

            // Mostra annuncio iniziale
            await lblAnnuncio.FadeTo(1, 500);

            // Countdown con effetti
            for (int i = 3; i > 0; i--)
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

                if (i == 1)
                {
                    try
                    {
                        if (Vibration.Default.IsSupported)
                            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
                    }
                    catch { }
                }
            }

            // Effetto finale con pallone
            lblCountdown.Text = "🏀";
            lblCountdown.TextColor = Colors.Orange;
            await lblCountdown.FadeTo(1, 200);
            await lblCountdown.ScaleTo(4.0, 200);
            await lblCountdown.FadeTo(0, 200);

            // Reset countdown
            lblCountdown.Text = "3";
            lblCountdown.FontSize = 72;
            lblCountdown.TextColor = Color.FromArgb("#C8102E");

            // Mostra logo squadra
            try
            {
                imgLogo.Source = squadra.LogoPath;
            }
            catch
            {
                imgLogo.Source = "team_default.png";
            }

            // Animazione del logo
            await Task.WhenAll(
                imgLogo.FadeTo(1, 600),
                imgLogo.ScaleTo(1.2, 600, Easing.SpringOut)
            );
            await imgLogo.ScaleTo(1.0, 200);

            // Mostra nome squadra
            lblSquadra.Text = squadra.Nome.ToUpper();
            lblSquadra.TextColor = Color.FromArgb(squadra.ColorePrimario);

            await Task.WhenAll(
                lblSquadra.FadeTo(1, 500),
                lblSquadra.ScaleTo(1.3, 500, Easing.SpringOut)
            );
            await lblSquadra.ScaleTo(1.0, 300);

            // Aggiorna annuncio finale
            lblAnnuncio.Text = $"CON LA SCELTA #{numeroScelta}";
            lblAnnuncio.TextColor = Color.FromArgb(squadra.ColoreSecondario);
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopAsync();
            return true;
        }
    }
}