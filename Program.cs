using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ppgSH
{
    class Program
    {
        static string sciezka_dost = ""; //aktualna sciezka gdzie się znajdujemy
        static string sciezka_domyslna=@"c:\"; //w przypadku nieustawienia ścieżki
        const int MAX_IL_PARAM = 10; //maksymalna liczba parametrów polecenia
        static string[] polecenie; //tablica elementów polecenia z parametrami
        static string komenda, argumenty; //polecenie z parametrami w postaci stringów
        static bool warunek = true; //warunek czy konsola ma pracowac.
        static string prompt = "#";
        static void Main(string[] args)
        {
            polecenie = new string[MAX_IL_PARAM + 1];
            Console.WriteLine("MyShell v1.0");
            while (warunek)
            {
                string pol = czytaj_z_konsoli();
                Wykonaj_komende(pol);
            }

        }
        static public string czytaj_z_konsoli()
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }
        static void Wykonaj_komende(string pol)
        {
            char[] separator = { ' '};
            //rozbicie na tablicę stringów z redukcją spacji powtarzających się
            polecenie = pol.Split(separator, MAX_IL_PARAM + 1, StringSplitOptions.RemoveEmptyEntries);
            //pierwszy wyraz jest komendą
            komenda = polecenie[0];
            //połączenie w 1 string
            argumenty = String.Join(" ", polecenie, 1, polecenie.Length - 1);
            komenda.ToLower(); //myshell nie rozróżnia wielkości liter
            switch (komenda)
            {
                case "echo":                    
                    //argumenty = pol.Remove(0, 5); ///wersja ze spacjami wielokrotnymi
                    Echo(argumenty); //wersja bez spacji wielokrotnych
                    break;
                case "quit":warunek = false;
                    break;
                case "cd":Cd(@argumenty);
                    break;
                case "clr": Clr();
                    break;
                case "dir":Dir(@argumenty);
                    break;
                case "eviron":Eviron();
                    break;
                case "help": Help(argumenty);
                    break;
                case "pause":Pause();
                    break;
                default:
                    if (Uruchom_w_tle(komenda,argumenty)) break;
                    else Nierozpoznana_komenda(komenda); 
                    break;
            }
        }
        //prosty przykład komendy echo
        static void Echo(string lista)
        {
            Console.WriteLine(@lista);
        }
        
        static bool Uruchom_w_tle(string nazwa,string argumenty)
        {
            string sciezka_procesu="";
            //sprawdzenie czy sciezka kończy się znakiem /
            if (sciezka_dost.Length == 0) sciezka_procesu = sciezka_domyslna; 
                else if (sciezka_dost[sciezka_dost.Length - 1] != '\\') sciezka_procesu = sciezka_dost +@"\" ;
            //sprawdzenie czy polecenie kończy się na .exe
            if (nazwa.Length < 5) nazwa += ".exe"; //dla krótkich nazw
                else
            {
                char[] koncowka = new char[4];
                nazwa.CopyTo(nazwa.Length - 4, koncowka, 0, 4);
                string konc = new String(koncowka);
                if (konc != ".exe") nazwa = nazwa + ".exe";
            }
            sciezka_procesu += nazwa;
            //sprawdzenie czy program istnieje
            if (File.Exists(sciezka_procesu))
            {
                if (argumenty == "") System.Diagnostics.Process.Start(sciezka_procesu);
                else System.Diagnostics.Process.Start(@sciezka_procesu, argumenty);
                return true;
            }
            else
            {
                //Console.WriteLine("Brak: (" + sciezka_procesu + ")");
                return false;
            }

        }
        static void Nierozpoznana_komenda(string plik)
        {
            Console.WriteLine("Nieznana komenda lub plik nie istnieje.("+plik+")");
        }
        static void Cd(string katalog)
        {

        }
        static void Clr()
        {

        }
        static void Dir(string katalog)
        {

        }
        static void Eviron()
        {

        }
        static void Help(string polecenie)
        {

        }
        static void Pause()
        {

        }
    }
}
