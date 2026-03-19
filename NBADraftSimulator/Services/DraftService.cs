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
                EscludiDaUltimaScelta = s.EscludiDaUltimaScelta,
                EStataEstratta = false
            }).ToList();

            _squadreDisponibili = _squadreOriginali.ToList();
            _ordineEstratto.Clear();
            _sceltaCorrente = 1;
        }

        public bool HaProssimaScelta()
        {
            // Se non ci sono squadre disponibili, fine draft
            if (_squadreDisponibili == null || !_squadreDisponibili.Any())
                return false;

            // In tutti gli altri casi, c'è ancora una scelta da fare
            return true;
        }

        public Squadra EstraiProssimaScelta()
        {
            if (!_squadreDisponibili.Any())
                return null;

            // --- GESTIONE ULTIMA SCELTA (quando rimane 1 squadra) ---
            if (_squadreDisponibili.Count == 1)
            {
                var ultimaSquadra = _squadreDisponibili.First();
                _squadreDisponibili.Remove(ultimaSquadra);
                _ordineEstratto.Add(ultimaSquadra);
                return ultimaSquadra;
            }

            // --- SEPARA I DUE CONTENITORI ---
            var squadreNonEscluse = _squadreDisponibili.Where(s => !s.EscludiDaUltimaScelta).ToList();
            var squadreEscluse = _squadreDisponibili.Where(s => s.EscludiDaUltimaScelta).ToList();

            int conteggioNonEscluse = squadreNonEscluse.Count;
            int conteggioEscluse = squadreEscluse.Count;

            // --- DECIDE DA DOVE PESCARE ---
            List<Squadra> poolEstrazione;

            if (conteggioNonEscluse >= 2)
            {
                // CASO 1: Almeno 2 non escluse -> pesca da TUTTE (tanto ne rimarrà una)
                poolEstrazione = _squadreDisponibili.ToList();
                System.Diagnostics.Debug.WriteLine($"Estrazione da TUTTE ({_squadreDisponibili.Count} squadre) - Non escluse: {conteggioNonEscluse}");
            }
            else if (conteggioNonEscluse == 1 && conteggioEscluse > 0)
            {
                // CASO 2: UNA sola non esclusa e CI SONO escluse -> pesca SOLO dalle escluse
                // La non esclusa è PROTETTA e non può essere estratta ora
                poolEstrazione = squadreEscluse;

                var squadraProtetta = squadreNonEscluse.First();
                System.Diagnostics.Debug.WriteLine($"PROTEZIONE: {squadraProtetta.Nome} non può essere estratta ora");
                System.Diagnostics.Debug.WriteLine($"Estrazione solo da ESCLUSE ({squadreEscluse.Count} squadre)");
            }
            else if (conteggioNonEscluse == 1 && conteggioEscluse == 0)
            {
                // CASO 3: UNA sola non esclusa e NESSUNA esclusa (impossibile se totale >1, ma per sicurezza)
                poolEstrazione = _squadreDisponibili.ToList();
                System.Diagnostics.Debug.WriteLine($"Solo non escluse, estrazione da TUTTE");
            }
            else // conteggioNonEscluse == 0
            {
                // CASO 4: Tutte sono escluse -> pesca da TUTTE
                poolEstrazione = _squadreDisponibili.ToList();
                System.Diagnostics.Debug.WriteLine($"Tutte escluse, estrazione da TUTTE");
            }

            // --- ESTRAZIONE PESATA DAL POOL SELEZIONATO ---
            if (!poolEstrazione.Any())
            {
                // Fallback
                poolEstrazione = _squadreDisponibili.ToList();
            }

            int pesoMassimo = poolEstrazione.Max(s => s.Peso);

            var squadreConPesoInverso = poolEstrazione
                .Select(s => new { Squadra = s, PesoInverso = (pesoMassimo + 1) - s.Peso })
                .ToList();

            int totalePesiInversi = squadreConPesoInverso.Sum(item => item.PesoInverso);
            int estratto = _random.Next(1, totalePesiInversi + 1);

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

            if (squadraSelezionata != null)
            {
                _squadreDisponibili.Remove(squadraSelezionata);
                _ordineEstratto.Add(squadraSelezionata);

                if (squadreNonEscluse.Contains(squadraSelezionata))
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ ATTENZIONE: Estratta una non esclusa: {squadraSelezionata.Nome}");
                }
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
                    EscludiDaUltimaScelta = s.EscludiDaUltimaScelta,
                    EStataEstratta = false
                }).ToList();

                _ordineEstratto.Clear();
                _sceltaCorrente = 1;
            }
        }
    }
}