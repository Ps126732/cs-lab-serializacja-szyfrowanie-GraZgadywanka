using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using GraZaDuzoZaMalo.Model;
using static GraZaDuzoZaMalo.Model.Gra.Odpowiedz;

namespace AppGraZaDuzoZaMaloCLI
{
    public class KontrolerCLI
    {
        public const char ZNAK_ZAKONCZENIA_GRY = 'X';

        private Gra gra;
        private WidokCLI widok;
        private CancellationTokenSource ctsAutosave;

        public int MinZakres { get; private set; } = 1;
        public int MaxZakres { get; private set; } = 100;

        public IReadOnlyList<Gra.Ruch> ListaRuchow => gra?.ListaRuchow;

        public KontrolerCLI()
        {
            widok = new WidokCLI(this);
        }

        public void Uruchom()
        {
            widok.OpisGry();
            SprawdzIWznawianieStanu();

            while (widok.ChceszKontynuowac("Czy chcesz zagrać / kontynuować aplikację (t/n)? "))
            {
                UruchomRozgrywke();
            }
        }

        private void SprawdzIWznawianieStanu()
        {
            if (RejestratorStanuXml.CzyIstniejeStan())
            {
                StanGryData wczytanyStan = RejestratorStanuXml.OdczytajStan();

                if (wczytanyStan != null)
                {
                    widok.CzyscEkran();
                    Console.WriteLine("--- WYKRYTO ZAPISANĄ GRĘ ---");
                    Console.WriteLine($"Poprzednia gra trwała aktywnie: {wczytanyStan.SkumulowanyCzas.TotalSeconds:F1} sekund.");
                    Console.WriteLine($"Oddano strzałów: {wczytanyStan.ListaRuchow.Count}.");

                    if (widok.ChceszKontynuowac("Czy chcesz wznowić poprzednią grę (t/n)? "))
                    {
                        gra = new Gra(wczytanyStan);
                        RejestratorStanuXml.UsunPlikStanu();
                        Console.WriteLine("Stan przywrócony! Gramy dalej...");
                        return;
                    }
                    else
                    {
                        RejestratorStanuXml.UsunPlikStanu();
                    }
                }
                else
                {
                    Console.WriteLine("Plik zapisu był uszkodzony lub zmanipulowany. Rozpoczęcie nowej gry...");
                    RejestratorStanuXml.UsunPlikStanu();
                }
            }
            gra = new Gra(MinZakres, MaxZakres);
        }

        public void UruchomRozgrywke()
        {
            widok.CzyscEkran();

            if (gra == null || gra.StatusGry != Gra.Status.WTrakcie)
                gra = new Gra(MinZakres, MaxZakres);

            RozpocznijAutosave(10);

            do
            {
                int propozycja = 0;
                try
                {
                    propozycja = widok.WczytajPropozycje();
                }
                catch (KoniecGryException)
                {
                    ZatrzymajAutosave();
                    gra.Zawies();

                    StanGryData stanDoZapisu = gra.PobierzStan();
                    if (RejestratorStanuXml.ZapiszStan(stanDoZapisu))
                        Console.WriteLine("\nGra została zawieszona, a jej stan ZAPISANY pomyślnie na dysku. Do zobaczenia!");

                    return;
                }

                if (gra.StatusGry == Gra.Status.Poddana || gra.StatusGry == Gra.Status.Zawieszona) break;

                switch (gra.Ocena(propozycja))
                {
                    case ZaDuzo: widok.KomunikatZaDuzo(); break;
                    case ZaMalo: widok.KomunikatZaMalo(); break;
                    case Trafiony: widok.KomunikatTrafiono(); break;
                }

                widok.HistoriaGry();
            }
            while (gra.StatusGry == Gra.Status.WTrakcie);

            ZatrzymajAutosave();

            if (gra.StatusGry == Gra.Status.Zakonczona)
                Console.WriteLine($"\nKONIEC GRY! Całkowity czas aktywnej rozgrywki: {gra.CalkowityCzasGry.TotalSeconds:F2} sekund.");
        }

        private void RozpocznijAutosave(int interwalSekundy)
        {
            ctsAutosave = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!ctsAutosave.Token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(interwalSekundy), ctsAutosave.Token);
                    if (gra != null && gra.StatusGry == Gra.Status.WTrakcie)
                    {
                        RejestratorStanuXml.ZapiszStan(gra.PobierzStan());
                    }
                }
            }, ctsAutosave.Token);
        }

        private void ZatrzymajAutosave()
        {
            ctsAutosave?.Cancel();
        }

        public int LiczbaProb() => gra.ListaRuchow.Count();

        public void ZakonczGre()
        {
            gra = null;
            widok.CzyscEkran();
            widok = null;
            System.Environment.Exit(0);
        }

        public void ZakonczRozgrywke() => gra.Przerwij();
    }

    [Serializable]
    public class KoniecGryException : Exception { }
}