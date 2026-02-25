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

            // Calcola totale pesi
            int totalePesi = _squadreDisponibili.Sum(s => s.Peso);

            // Estrai numero casuale
            int estratto = _random.Next(1, totalePesi + 1);

            // Trova squadra corrispondente
            int cumulativo = 0;
            Squadra squadraSelezionata = null;

            foreach (var squadra in _squadreDisponibili)
            {
                cumulativo += squadra.Peso;
                if (estratto <= cumulativo)
                {
                    squadraSelezionata = squadra;
                    break;
                }
            }

            if (squadraSelezionata != null)
            {
                squadraSelezionata.EStataEstratta = true;
                _squadreDisponibili.Remove(squadraSelezionata);
                _ordineEstratto.Add(squadraSelezionata);
            }

            return squadraSelezionata;
        }

        public int NumeroSceltaCorrente => _sceltaCorrente++;

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