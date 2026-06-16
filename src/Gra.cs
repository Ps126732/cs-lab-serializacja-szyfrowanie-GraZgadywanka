using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GraZaDuzoZaMalo.Model
{
    [Serializable]
    [DataContract]
    public class StanGryData
    {
        [DataMember] public int Min { get; set; }
        [DataMember] public int Max { get; set; }
        [DataMember] public int LiczbaDoOdgadniecia { get; set; }
        [DataMember] public Gra.Status StatusGry { get; set; }
        [DataMember] public TimeSpan SkumulowanyCzas { get; set; }
        [DataMember] public DateTime CzasRozpoczecia { get; set; }
        [DataMember] public List<Gra.Ruch> ListaRuchow { get; set; }
    }

    public class Gra
    {
        public int MaxLiczbaDoOdgadniecia { get; } = 100;
        public int MinLiczbaDoOdgadniecia { get; } = 1;

        readonly private int liczbaDoOdgadniecia;

        public enum Status
        {
            WTrakcie,
            Zakonczona,
            Poddana,
            Zawieszona // Dodany status
        };

        public Status StatusGry { get; private set; }

        private List<Ruch> listaRuchow;
        public IReadOnlyList<Ruch> ListaRuchow { get { return listaRuchow.AsReadOnly(); } }

        public DateTime CzasRozpoczecia { get; private set; }
        public DateTime? CzasZakonczenia { get; private set; }

        private TimeSpan skumulowanyCzas = TimeSpan.Zero;
        private DateTime czasRozpoczeciaAktualnegoSegmentu;

        public TimeSpan AktualnyCzasGry
        {
            get
            {
                if (StatusGry == Status.WTrakcie)
                    return skumulowanyCzas + (DateTime.Now - czasRozpoczeciaAktualnegoSegmentu);

                return skumulowanyCzas;
            }
        }

        public TimeSpan CalkowityCzasGry => AktualnyCzasGry;

        public Gra(int min, int max)
        {
            if (min >= max) throw new ArgumentException();
            MinLiczbaDoOdgadniecia = min;
            MaxLiczbaDoOdgadniecia = max;
            liczbaDoOdgadniecia = (new Random()).Next(MinLiczbaDoOdgadniecia, MaxLiczbaDoOdgadniecia + 1);
            CzasRozpoczecia = DateTime.Now;
            czasRozpoczeciaAktualnegoSegmentu = DateTime.Now;
            CzasZakonczenia = null;
            StatusGry = Status.WTrakcie;
            listaRuchow = new List<Ruch>();
        }

        public Gra() : this(1, 100) { }

        // Konstruktor odtwarzający stan z pliku
        public Gra(StanGryData stan)
        {
            MinLiczbaDoOdgadniecia = stan.Min;
            MaxLiczbaDoOdgadniecia = stan.Max;
            liczbaDoOdgadniecia = stan.LiczbaDoOdgadniecia;
            StatusGry = stan.StatusGry;
            skumulowanyCzas = stan.SkumulowanyCzas;
            CzasRozpoczecia = stan.CzasRozpoczecia;
            listaRuchow = stan.ListaRuchow ?? new List<Ruch>();

            if (StatusGry == Status.Zawieszona)
            {
                StatusGry = Status.WTrakcie;
                czasRozpoczeciaAktualnegoSegmentu = DateTime.Now;
            }
        }

        public Odpowiedz Ocena(int pytanie)
        {
            Odpowiedz odp;
            if (pytanie == liczbaDoOdgadniecia)
            {
                odp = Odpowiedz.Trafiony;
                ZakonczGreSegment(Status.Zakonczona);
                listaRuchow.Add(new Ruch(pytanie, odp, Status.Zakonczona));
            }
            else if (pytanie < liczbaDoOdgadniecia) odp = Odpowiedz.ZaMalo;
            else odp = Odpowiedz.ZaDuzo;

            if (StatusGry == Status.WTrakcie)
                listaRuchow.Add(new Ruch(pytanie, odp, Status.WTrakcie));

            return odp;
        }

        public int Przerwij()
        {
            if (StatusGry == Status.WTrakcie)
            {
                ZakonczGreSegment(Status.Poddana);
                listaRuchow.Add(new Ruch(null, null, Status.WTrakcie));
            }
            return liczbaDoOdgadniecia;
        }

        public void Zawies()
        {
            if (StatusGry == Status.WTrakcie) ZakonczGreSegment(Status.Zawieszona);
        }

        public StanGryData PobierzStan()
        {
            TimeSpan obecnySkumulowany = skumulowanyCzas;
            if (StatusGry == Status.WTrakcie)
                obecnySkumulowany += (DateTime.Now - czasRozpoczeciaAktualnegoSegmentu);

            return new StanGryData
            {
                Min = this.MinLiczbaDoOdgadniecia,
                Max = this.MaxLiczbaDoOdgadniecia,
                LiczbaDoOdgadniecia = this.liczbaDoOdgadniecia,
                StatusGry = this.StatusGry,
                SkumulowanyCzas = obecnySkumulowany,
                CzasRozpoczecia = this.CzasRozpoczecia,
                ListaRuchow = new List<Ruch>(this.listaRuchow)
            };
        }

        private void ZakonczGreSegment(Status nowyStatus)
        {
            skumulowanyCzas += (DateTime.Now - czasRozpoczeciaAktualnegoSegmentu);
            StatusGry = nowyStatus;
            CzasZakonczenia = DateTime.Now;
        }

        public enum Odpowiedz { ZaMalo = -1, Trafiony = 0, ZaDuzo = 1 };

        [Serializable]
        [DataContract]
        public class Ruch
        {
            [DataMember] public int? Liczba { get; private set; }
            [DataMember] public Odpowiedz? Wynik { get; private set; }
            [DataMember] public Status StatusGry { get; private set; }
            [DataMember] public DateTime Czas { get; private set; }

            public Ruch(int? propozycja, Odpowiedz? odp, Status statusGry)
            {
                this.Liczba = propozycja;
                this.Wynik = odp;
                this.StatusGry = statusGry;
                this.Czas = DateTime.Now;
            }

            public override string ToString() => $"({Liczba}, {Wynik}, {Czas}, {StatusGry})";
        }
    }
}