using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Security;

namespace ppgSH
{
    /**
    * MyShell
    */
    class Program
    {
        

        /**
        * Maksymalna liczba parametrów polecenia
        */
        const int MAX_IL_PARAM = 10;

        static string[] polecenie; //tablica elementów polecenia z parametrami
        static string komenda, argumenty; //polecenie z parametrami w postaci stringów
        static bool warunek = true; //warunek czy konsola ma pracowac.
        static string prompt = "#"; //wyswietlana podpowiedź
        static string path_MyShell; //ścieżka dostępu do programu MyShell
        static int poz_znaku_out, poz_znaku_in; //pozycje przekierowań in i out w tabicy polecenie
        static bool przekierowanie_in, przekierowanie_out; //czy należy obsłużyć przekierowanie strumienia

        static void Main(string[] args)
        {
            polecenie = new string[MAX_IL_PARAM + 1];
            //ustawienie tytułu
            Console.Title = "MyShell";
            
            //pobranie katalogu w którym jest MyShell
            //sposób alternatywny(chyba pewniejszy) do Directory.GetCurrentDirectory();
            string path_sys = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
            Uri path_uri = new Uri(path_sys);
            path_MyShell = Path.GetDirectoryName(path_uri.LocalPath);
            Console.WriteLine("MyShell v1.0");

            // W przypadku wywolania z argumentem sprawdza czy jest to prawidlowy plik batch (.ppgsh) jesli tak to wykonuje go.
            if (args.Length != 0)
                batchParse(args[0]);

            //wykonuj pętlę aż do momentu wyjścia (warunek=false)
            while (warunek)
            {
                string pol = czytaj_z_konsoli();
                Wykonaj_komende(pol); 
            }
            
        }

        /**
        * Parser dla wykonywania plikow batch.
        * Wymagane rozszerzenie pliku to .ppgsh
        * Linie z dowolna iloscia bialych znakow i // na poczatku beda ignorowane i traktowane jako komentarze.
        */
        static void batchParse(string file)
        {
            if (!file.EndsWith(".ppgsh"))
                printEcho("That's not a batch file.");
            try
            {
                if (File.Exists(file))
                {
                    string line;
                    System.IO.StreamReader batch = new System.IO.StreamReader(file);
                    while ((line = batch.ReadLine()) != null)
                    {
                        if (!line.TrimStart().StartsWith("////"))
                            Wykonaj_komende(line);
                    }

                    batch.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured while parsing batch file.");
            }
        }

        /**
        * Zmienia aktualny katalog roboczy.
        *
        * Katalog roboczy aplikacji jest zmieniany na wybrany przez usera.
        * Sciezka nie jest case sensitive.
        * Akceptuje sciezki relatywne oraz absolutne.
        * W przypadku braku sciezki wyswietla aktualna sciezke.
        * Przy podaniu blednej lub zbyt dlugiej sciezki wyswietla blad.
        *
        * @params string|null dir Sciezka do zmiany.
        */
        static void changeDirectory(string dir = null)
        {
            try
            {
                if (dir != "")
                { 
                    Directory.SetCurrentDirectory(dir);
                } else {
                    Console.WriteLine(Directory.GetCurrentDirectory());
                }
                
            }
            catch (ArgumentNullException e)
            {
                // Pokazuje aktualny katalog roboczy aplikacji.
                
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine("Provided path was not found.");
            }
            catch (PathTooLongException e)
            {
                Console.WriteLine("File or directory name exceed system-defined maximum lenght.");
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Provided path contains invalid characters.");
            }
            catch (Exception e)
            {
                Console.WriteLine("An unknown error occured.");
            }
        }

        /**
        * Pokazuje wszystie pliki w aktualnym katalogu.
        *
        * Wypluwa date, godzine, typ (katalog/plik) i nazwe pliku/katalogu.
        * W przypadku podania pustej sciezki sprawdza aktualny katalog.
        *
        * @param string|null dir Katalog do sprawdzenia.
        */
        static void showDirectory(string dir = null)
        {
            IEnumerable<string> dirData;
            IEnumerable<string> fileData;
            try
            {
                // Jesli argument dir nie zostal podany lub jest pusty uzyj aktualnego katalogu roboczego.
                if (dir == null || dir == "")
                {
                    dirData = Directory.EnumerateDirectories(Directory.GetCurrentDirectory());
                    fileData = Directory.EnumerateFiles(Directory.GetCurrentDirectory());
                    Console.WriteLine("Directory of {0}\n", Directory.GetCurrentDirectory());
                }
                else
                {
                    dirData = Directory.EnumerateDirectories(dir);
                    fileData = Directory.EnumerateFiles(dir);
                    Console.WriteLine("Directory of {0}\n", dir);
                }

                // Da magics
                Console.ForegroundColor = ConsoleColor.Blue;
                // Info o aktualnym katalogu.
                Console.WriteLine("{0,-18}{1,-7}{2,-10}",
                    Directory.GetCreationTime(".").ToString("yyyy-MM-dd HH:mm"),
                    "<DIR>",
                    ".");
                // Info o katalogu wyzej.
                Console.WriteLine("{0,-18}{1,-7}{2,-10}",
                    Directory.GetCreationTime("..").ToString("yyyy-MM-dd HH:mm"),
                    "<DIR>",
                    "..");

                foreach (string record in dirData)
                {
                    Console.WriteLine("{0,-18}{1,-7}{2,-10}",
                        Directory.GetCreationTime(record).ToString("yyyy-MM-dd HH:mm"),
                        "<DIR>",
                        record.Substring(record.LastIndexOf('\\') + 1));
                }

                Console.ForegroundColor = ConsoleColor.White;
                foreach (string record in fileData)
                {
                    Console.WriteLine("{0,-18}{1,-7}{2,-10}",
                        Directory.GetCreationTime(record).ToString("yyyy-MM-dd HH:mm"),
                        " ",
                        record.Substring(record.LastIndexOf('\\') + 1));
                }

                // Powrot do standardowego koloru.
                Console.ResetColor();
            }
            catch (ArgumentException e)
            {
                // Bledny argument same spacje etc
                Console.WriteLine("Provided path contains invalid characters.");
            }
            catch (DirectoryNotFoundException e)
            {
                // Katalogu nie znaleziono
                Console.WriteLine("Provided path was not found.");
            }
            catch (IOException e)
            {
                // Podane coś jest plikiem
                Console.WriteLine("Provided path is a file.");
            }
            catch (UnauthorizedAccessException e)
            {
                // Brak pozwolenia na otwarcie katalogu.
                Console.WriteLine("You don't have required permisssion.");
            }
            catch (Exception e)
            {
                //Pozostale bledy jesli jakies wystapia
                Console.WriteLine("Unexpected error occured.");
            }
        }

        /**
        * Pokazuje w konsoli komunikat wpisany przez usera.
        *
        * W ramach dzialania funkcji usuwane sa nadmiarowe znaki biale.
        * W przypadku pustego badz blednego wejscia zostanie wydrukowany tylko znak nowej linii.
        *
        * @param string|null input Linia do wyplucia.
        */
        static void printEcho(string input = null)
        {
            try
            {
                input = Regex.Replace(input, @"\s+", " ");
            }
            catch (Exception e)
            {
                input = "";
            }
            Console.WriteLine(input);
        }

       /**
        * Kasuje wszystkie znaki z okna konsolowego.
        *
        * W przypadku bledu funkcja konczy swoje dzialanie z efektem dzwiekowym.
        *
        * @param string|null input Tekst ktory ma zostac wypisany do pierwszej linii konsoli.
        */
        static void clearScreen(string input = null)
        {
            try
            {
                Console.Clear();
            }
            catch (Exception e)
            {
                Console.Beep();
            }
            if (input != null)
                Console.WriteLine(input);
        }
        
        /**
        * Wyswietla wszystkie zmienne systemowe.
        */
        static void showEnvironmentVariables()
        {
            foreach (DictionaryEntry e in System.Environment.GetEnvironmentVariables())
            {
                Console.WriteLine(e.Key + ": " + e.Value);
            }
        }
    
        /**
        * Czyta wpisy usera z linii komend.
        *
        * @return string
        */
        static public string czytaj_z_konsoli()
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        /**
        * Modul wykonujacy komendy.
        * Przetwarza komende otrzymana od uzytkownika i wykonuje zwiazane z nia akcje.
        */
        static void Wykonaj_komende(string pol)
        {
            char[] separator = { ' '};
            //rozbicie na tablicę stringów z redukcją spacji powtarzających się

            try
            {
                polecenie = pol.Split(separator, MAX_IL_PARAM + 1, StringSplitOptions.RemoveEmptyEntries);

            }
            catch (ArgumentException)
            {
                return;
            } //pierwszy wyraz jest komendą
            if ((polecenie == null) ||(polecenie.Length==0)) return;
            komenda = polecenie[0];
            
            //połączenie w 1 string
            argumenty = String.Join(" ", polecenie, 1, polecenie.Length - 1);
            //sprawdzenie czy polecenie zawiera znaki przekierowań < i >
            przekierowanie_in = false;przekierowanie_out = false;
            for (int n = 1; n < polecenie.GetUpperBound(0); n++)
            {
                if (polecenie[n] == ">")
                {
                    poz_znaku_out = n;
                    przekierowanie_out = true;
                }
                if (polecenie[n] == "<")
                {
                    poz_znaku_in = n;
                    przekierowanie_in = true;
                }
            }
            //sprawdzenie czy nie chcemy wywołać pomocy dla komendy (/?)
            if (argumenty!="") //jeśli polecenie zawiera argumenty
                if ((polecenie[1]=="/?") || (polecenie[1]==@"\?"))//wywołanie pomocy dla komendy
                {
                    Help(komenda);
                    return;
                }
            komenda.ToLower(); //myshell nie rozróżnia wielkości liter
            switch (komenda)
            {
                case "echo":                    
                    printEcho(argumenty);
                    break;
                case "quit":
                    warunek = false;
                    break;
                case "exit":
                    warunek = false;
                    break;
                case "clr":
                    clearScreen();
                    break;
                case "cd":
                    changeDirectory(argumenty);
                    break;
                case "environ":
                    showEnvironmentVariables();
                    break;
                case "help":
                    Help(argumenty);
                    break;
                case "pause":
                    pause();
                    break;
                case "dir":
                    showDirectory(argumenty);
                    break;
                default:
                    if (!Uruchom(komenda, argumenty))
                        //jeśli nie udało sie uruchomić komendy
                        Console.WriteLine("Operacja zakończona niepowodzeniem");
                    break;
            }
        }

        /**
        * Uruchomienie lokalnego programu bez przekierowania wejscia/wyjscia.
        * Następuje wyświetlanie wyników w oknie konsoli. Konsola czeka
        * na zakończenie procesu.
        * @return boolean W zaleznosci czy polecenie wykonalo sie poprawnie.
        */
        static bool uruchom_bez_przekierowania(string program,string argumenty)
        {
            Process process = new Process();
            process.StartInfo.FileName = program;
            //wpisanie argumentów
            process.StartInfo.Arguments = argumenty;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            try
            {
                process.Start();
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                //błąd otwarcia pliku
                Console.WriteLine(e.Message);
                return false;
            }
            // Synchronously read the standard output of the spawned process. 
            StreamReader reader = process.StandardOutput;
            string output = reader.ReadToEnd();
            // Write the redirected output to this application's window.
            Console.WriteLine(output);
            process.WaitForExit();
            process.Close();
            return true;

            
        }
        static bool uruchom_bez_przekierowania_w_tle(string program, string argumenty)
        {
            Process process = new Process();
            process.StartInfo.FileName = program;
            //wpisanie argumentów
            process.StartInfo.Arguments = argumenty;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.RedirectStandardOutput = false;
            try
            {
                process.Start();
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                //błąd otwarcia pliku
                Console.WriteLine(e.Message);
                return false;
            }
            return true;


        }

        /**
        * Uruchomia proces jednocześnie przekierowując wy lub/i we programu na plik.
        *
        * @return boolean W zaleznosci czy polecenie wykonalo sie poprawnie.
        */
        static bool uruchom_z_przekierowaniem(string program, string argumenty, string plik_in, string plik_out)
        {
            string plik = polecenie[poz_znaku_out + 1];
            Process process = new Process();
            //ustawienie parametrów procesu
            process.StartInfo.FileName = program;
            process.StartInfo.Arguments = argumenty;
            process.StartInfo.UseShellExecute = false;
            if (przekierowanie_in) process.StartInfo.RedirectStandardInput = true; //przekierowanie strumienia wejściowego
            //przekierowanie strumienia wyjściowego
            //jesli jest przekierowanie wyjściowe lub samo przekierowanie wejściowe
            //w 2 przypadku przekieruj na konsolę
            if (przekierowanie_out || (przekierowanie_in && !przekierowanie_out))
            {
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
            }
            process.Start(); //uruchomienie procesu potomka
            StreamWriter writer = null;
            StreamReader reader = null, stdError = null;
            string text = "";
            //obsługa przekierowania wejściowego
            if (przekierowanie_in)
            {
                writer = process.StandardInput;
                //odczytanie pliku
                try
                {
                    text = File.ReadAllText(@plik_in);
                }
                catch (IOException e)
                {
                    //błąd we/wy przy czytaniu pliku wejściowego
                    Console.WriteLine(e.Message);
                    process.Close();
                    writer.Close();
                    return false;
                }
                catch (SecurityException e)
                {
                    //brak uprawnien do czytania pliku wejściowego
                    Console.WriteLine(e.Message);
                    process.Close();
                    writer.Close();
                    return false;
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    process.Close();
                    writer.Close();
                    return false;
                }
                //zapisanie do procesu
                writer.WriteLine(text);
            }


            // jesli jest przekierowanie wyjścia lub
            //jeśli jest tylko przekierowanie wejściowe to skieruj wyjście procesu na konsolę
            if (przekierowanie_out || (przekierowanie_in && !przekierowanie_out))
            {
                reader = process.StandardOutput;
                stdError = process.StandardError;
                string output = reader.ReadToEnd();
                string error = stdError.ReadToEnd();

                if (przekierowanie_out)
                {
                    try
                    {
                        //jeśli plik nie ustnieje to utwórz i zapisz treść a jeśli istnieje to dopisz
                        File.AppendAllText(@plik_out, output + error);
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("Błąd we/wy ({0})", @plik_out);
                        process.Close();
                        reader.Close();
                        stdError.Close();
                        if (przekierowanie_in) writer.Close();
                        return false;
                    }
                    catch (SecurityException e)
                    {
                        Console.WriteLine(e.Message);
                        process.Close();
                        reader.Close();
                        stdError.Close();
                        if (przekierowanie_in) writer.Close();
                        return false;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.WriteLine(e.Message);
                        process.Close();
                        reader.Close();
                        stdError.Close();
                        if (przekierowanie_in) writer.Close();
                        return false;
                    }

                } else
                    //wypisz na konsoli
                    Console.WriteLine(output + error);

                //oczekiwanie na zakończenie procesu
                process.WaitForExit();
                //zwolnienie zasobów 
                process.Close();
                if (przekierowanie_in) writer.Close();
                if (przekierowanie_out)
                {
                    reader.Close();
                    stdError.Close();
                }
                

            }
            return true;
        }

        /**
        * Uruchamia program po jego nazwie.
        */
        static bool Uruchom(string nazwa,string argumenty)
        {  
            // Jesli nazwa nie istnieje nie wykonuj dalszej czesci funkcji.
            if (nazwa == null)
                return false;
            bool uruchom_w_tle = false; //czy urchamiać w tle(true) czy przekierowywać wyniki na konsolę(false)
            string sciezka_procesu="";
            //sprawdzenie czy sciezka kończy się znakiem /
            string sciezka_dost = Directory.GetCurrentDirectory(); 
            if (sciezka_dost[sciezka_dost.Length - 1] != '\\') sciezka_procesu = sciezka_dost +@"\" ;
            //sprawdzenie czy polecenie kończy się na & (dla wywołania w tle)
            if (nazwa[nazwa.Length-1]=='&')
            {
                uruchom_w_tle = true;
                //wykasuj & z nazwy
                nazwa=nazwa.Remove(nazwa.Length-1);
            }
            sciezka_procesu += nazwa;
            FileInfo fin = new FileInfo(sciezka_procesu);
            //jeżeli nazwa programu nie kończy się rozszerzeniem to dodaj .exe
            if (fin.Extension == "") sciezka_procesu += ".exe";
             //jeżeli jest to plik niewykonywalny to wypisz komunikat
            else if ((fin.Extension!=".exe") && (fin.Extension != ".com"))
            {
                if (fin.Exists)
                {
                    Console.WriteLine("plik {0} jest niewykonywalny", nazwa);
                    return true;
                } else return false;
                
            }
            // Sprawdza czy plik istnieje w aktualnym katalogu jesli nie, to przeszukuje katalogi ze zmiennej systemowej PATH
            // W przypadku znalezienia pasujacego pliku wykonywalnego uruchamia go.
            if (!File.Exists(sciezka_procesu))
            {
                try
                {
                    string path = Environment.GetEnvironmentVariable("path");
                    // Oszukana metoda na dodanie system32 do przeszukiwanych katalogow
                    path += ";C:\\Windows\\system32\\";
                    string[] tPath = path.Split(';');
                    string nazwa_ext;
                    if (!nazwa.EndsWith(".exe"))
                    {
                        nazwa_ext = nazwa + ".exe";
                    } else
                    {
                        nazwa_ext = nazwa;
                    }
                    // Przeszukuje tablice ze sciezkami ze zmiennej PATH szukajac pliku nazwa(.exe)
                    foreach (string pathDir in tPath)
                    {
                        string fullPath = pathDir + nazwa_ext;
                        if (File.Exists(fullPath))
                        {
                            sciezka_procesu = fullPath;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Well something went really wrong.");
                }
            }

            if (File.Exists(sciezka_procesu))
            {
                //jeśli polecenie nie zawiera przekierowań
                if (!przekierowanie_out && !przekierowanie_in)
                {
                    if (uruchom_w_tle) uruchom_bez_przekierowania_w_tle(sciezka_procesu, argumenty);
                      else uruchom_bez_przekierowania(sciezka_procesu, argumenty);
                    return true;
                } else
                {
                    //uruchom polecenie z przekierowaniem
                    string file_in, file_out;
                    if (przekierowanie_out) file_out = polecenie[poz_znaku_out + 1]; else file_out = "";
                    if (przekierowanie_in)
                    {
                        file_in = polecenie[poz_znaku_in + 1];
                        //sprawdzenie czy plik wejściowy istnieje
                        if (!File.Exists(file_in))
                        {
                            Console.WriteLine("Pliku wejściowy nie istnieje ({0})", file_in);
                            return false;
                        }
                    }
                    else file_in = "";

                    //pozycja końca argumentów
                    int poz;
                    if (!przekierowanie_in) poz = poz_znaku_out;
                    else if (!przekierowanie_out) poz = poz_znaku_in;
                    else poz = Math.Min(poz_znaku_in, poz_znaku_out);
                    //argumenty programu
                    argumenty = String.Join(" ", polecenie, 1, poz - 1);
                    uruchom_z_przekierowaniem(sciezka_procesu, argumenty, file_in, file_out);
                    return true;
                }
            }
            else
            {
                Console.WriteLine("Nieznana komenda lub plik nie istnieje ({0})", nazwa);
                return true;
            }

        }

        /**
        * Zwraca komunikat o nieznanej komendzie do konsoli.
        * @return void
        */
        static void Nierozpoznana_komenda(string plik)
        {
            Console.WriteLine("Nieznana komenda lub plik nie istnieje.({0})",plik);
        }

        /**
        * Zwraca kominikaty pomocy dla danego polecenia.
        */
        static void Help(string polecenie)
        {
           
            string line; //zmienna zawierająca wczytywaną linie
            string plik_help = @path_MyShell + @"\help.txt";
            //jeśli plik pomocy nie istnieje
            if (!File.Exists(plik_help))
                { Console.WriteLine("Brak pliku pomocy (help.txt)"); return; }
            //utworzenie strumienia czytającego plik help
            StreamReader file = new StreamReader(@plik_help);
            //przy braku parametrów wyświetl pomoc ogólną
            if (polecenie == "" || polecenie == null)
            {
                while ((line = file.ReadLine()) != "koniec")
                {
                    System.Console.WriteLine(line);
                }
                return;
            } else 
            //jeśli podano komendę dla której mamy wyświetlić  pomoc
            {
                //szukana linia
                string title_line = "Polecenie " + polecenie.ToUpper();
                //szukanie komendy w pliku help.txt
                do
                {
                    line = file.ReadLine();
                    if (line == null)
                    {
                        Console.WriteLine("Nie znaleziono pomocy dla {0}", polecenie);
                        return;
                    }
                } while (line != title_line);
                //wczytywanie pomocy linia po linii do linii "koniec"
                while ((line = file.ReadLine()) != "koniec")
                {
                    System.Console.WriteLine(line);
                }
            }
            
        }

        /**
        * Wstrzymuje dzialanie programu do czasu wcisniecia klawisza Enter.
        *
        * Znaki wpisywane przez usera nie są wyświetlane.
        * Zwraca pusta linie w razie sukcesu.
        */
        static void pause()
        {
            Console.WriteLine("Naciśnij klawisz Enter aby kontynuować...");
            ConsoleKeyInfo c;
            do
            {
                c = Console.ReadKey(true);
            } while (c.Key != ConsoleKey.Enter);
            printEcho();
        }
    }
}
