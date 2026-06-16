using System;
using static System.Console;

namespace AppGraZaDuzoZaMaloCLI
{
    class WidokCLI
    {
        public const char ZNAK_ZAKONCZENIA_GRY = 'X';
        private KontrolerCLI kontroler;

        public WidokCLI(KontrolerCLI kontroler) => this.kontroler = kontroler;

        public void CzyscEkran() => Clear();

        public void KomunikatPowitalny() => WriteLine("Wylosowałem liczbę z zakresu ");

        public int WczytajPropozycje()
        {
            int wynik = 0;
            bool sukces = false;
            while (!sukces)
            {
                Write("Podaj swoją propozycję (lub " + ZNAK_ZAKONCZENIA_GRY + " aby zapisać stan i wyjść): ");
                try
                {
                    string value = ReadLine().TrimStart().ToUpper();
                    if (value.Length > 0 && value[0].Equals(ZNAK_ZAKONCZENIA_GRY))
                        throw new KoniecGryException();

                    wynik = Int32.Parse(value);
                    sukces = true;
                }
                catch (FormatException) { WriteLine("To nie jest liczba! Spróbuj raz jeszcze."); }
                catch (OverflowException) { WriteLine("Liczba jest za duża lub za mała! Spróbuj raz jeszcze."); }
                catch (KoniecGryException ex) { throw ex; }
                catch (Exception) { WriteLine("Nieznany błąd! Spróbuj raz jeszcze."); }
            }
            return wynik;
        }

        public void OpisGry()
        {
            WriteLine("Gra w \"Za dużo za mało\".\nTwoim zadaniem jest odgadnąć liczbę, którą wylosował komputer.\nNa twoje propozycje komputer odpowiada: za dużo, za mało albo trafiłeś");
        }

        public bool ChceszKontynuowac(string prompt)
        {
            Write(prompt);
            char odp = ReadKey().KeyChar;
            WriteLine();
            return (odp == 't' || odp == 'T');
        }

        public void HistoriaGry()
        {
            if (kontroler.ListaRuchow.Count == 0)
            {
                WriteLine("--- pusto ---");
                return;
            }

            WriteLine("\nNr   Propozycja      Odpowiedź      Czas      Status");
            WriteLine("======================================================");
            int i = 1;
            foreach (var ruch in kontroler.ListaRuchow)
            {
                string prop = ruch.Liczba?.ToString() ?? "Brak";
                string wyn = ruch.Wynik?.ToString() ?? "Brak";
                WriteLine($"{i,-4} {prop,-15} {wyn,-14} {ruch.Czas.ToShortTimeString()}   {ruch.StatusGry}");
                i++;
            }
            WriteLine();
        }

        public void KomunikatZaDuzo() { ForegroundColor = ConsoleColor.Red; WriteLine("Za dużo!"); ResetColor(); }
        public void KomunikatZaMalo() { ForegroundColor = ConsoleColor.Red; WriteLine("Za mało!"); ResetColor(); }
        public void KomunikatTrafiono() { ForegroundColor = ConsoleColor.Green; WriteLine("Trafiono!"); ResetColor(); }
    }
}