using NBADraftSimulator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBADraftSimulator.Services
{
    public class DraftService
    {
        private List<Squadra> _squadreOriginali;
        private List<Squadra> _squadreDisponibili;
        private List<Squadra> _ordineEstratto;
        private Random _random;
        private int _sceltaCorrente;

        public DraftService()
        {
            _random = new Random();
            _ordineEstratto = new List<Squadra>();
            _sceltaCorrente = 1;
        }

        public void InizializzaDraft(List<Squadra> squadre)
        {
            _squadreOriginali = squadre.Select(s => new Squadra
            {
                Nome = s.Nome,
                Peso = s.Peso,
                ColorePrimario = s.ColorePrimario,
                ColoreSecondario = s.ColoreSecondario,
                LogoPath = s.LogoPath,
                EStataEstratta = false
            }).ToList();

            _squadreDisponibili = _squadreOriginali.ToList();
            _ordineEstratto.Clear();
            _sceltaCorrente = 1;
        }

        public bool HaProssimaScelta()
        {
            return _squadreDisponibili.Any();
        }

        public Squadra EstraiProssimaScelta()
        {
            if (!_squadreDisponibili.Any())
                return null;

            // --- NUOVA LOGICA PER ULTIMA SCELTA ---
            // Se rimane UNA SOLA squadra (è l'ultima scelta)
            if (_squadreDisponibili.Count == 1)
            {
                var unicaSquadra = _squadreDisponibili.First();
                // Se l'unica squadra rimasta DEVE essere esclusa, qualcosa è andato storto.
                // In un flusso corretto, non dovrebbe mai capitare che l'unica squadra rimasta abbia il flag true.
                // Per sicurezza, la escludiamo dal controllo e la restituiamo comunque.
                // Ma se vuoi essere più rigoroso, potresti lanciare un'eccezione o mostrare un errore.
                if (unicaSquadra.EscludiDaUltimaScelta)
                {
                    // Logica di fallback: potrebbe succedere se tutte le squadre hanno il flag?
                    // In questo caso, forziamo l'ammissione dell'ultima.
                    System.Diagnostics.Debug.WriteLine("Attenzione: Ultima squadra ha il flag di esclusione, ma viene comunque estratta.");
                }
                _squadreDisponibili.Remove(unicaSquadra);
                _ordineEstratto.Add(unicaSquadra);
                return unicaSquadra;
            }
            // --------------------------------------

            // --- CALCOLO DEL PESO INVERSO ---
            // 1. Trova il peso massimo tra le squadre disponibili
            int pesoMassimo = _squadreDisponibili.Max(s => s.Peso);

            // 2. Per ogni squadra, calcola un peso inverso: (pesoMassimo + 1) - pesoOriginale
            //    In questo modo, chi ha peso più alto (es. 100) avrà peso inverso basso (es. 1)
            //    e viceversa.
            var squadreConPesoInverso = _squadreDisponibili
                .Select(s => new { Squadra = s, PesoInverso = (pesoMassimo + 1) - s.Peso })
                .ToList();

            // 3. Calcola il totale dei pesi inversi
            int totalePesiInversi = squadreConPesoInverso.Sum(item => item.PesoInverso);

            // 4. Estrai un numero casuale basato sul totale dei pesi inversi
            int estratto = _random.Next(1, totalePesiInversi + 1);

            // 5. Trova la squadra corrispondente usando i pesi inversi
            int cumulativo = 0;
            Squadra squadraSelezionata = null;
            foreach (var item in squadreConPesoInverso)
            {
                cumulativo += item.PesoInverso;
                if (estratto <= cumulativo)
                {
                    squadraSelezionata = item.Squadra;
                    break;
                }
            }
            // ---------------------------------

            if (squadraSelezionata != null)
            {
                _squadreDisponibili.Remove(squadraSelezionata);
                _ordineEstratto.Add(squadraSelezionata);
            }

            return squadraSelezionata;
        }

        public int NumeroSceltaCorrente => _sceltaCorrente++;
        public int TotaleSquadre => _squadreOriginali?.Count ?? 0;

        public List<Squadra> GetOrdineCompleto()
        {
            return new List<Squadra>(_ordineEstratto);
        }

        public void ResetDraft()
        {
            if (_squadreOriginali != null)
            {
                _squadreDisponibili = _squadreOriginali.Select(s => new Squadra
                {
                    Nome = s.Nome,
                    Peso = s.Peso,
                    ColorePrimario = s.ColorePrimario,
                    ColoreSecondario = s.ColoreSecondario,
                    LogoPath = s.LogoPath,
                    EStataEstratta = false
                }).ToList();

                _ordineEstratto.Clear();
                _sceltaCorrente = 1;
            }
        }
    }
}