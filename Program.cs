﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace ppgSH
{
    class Program
    {
        /**
        * Info.
        *
        * Kolory:
        * Dla katalogow ConsoleColor.Blue;
        * Dla plikow ConsoleColor.White;
        */        
        static void Main(string[] args)
        {
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
        static void changeDirectory(string dir)
        {
            try
            {
                Directory.SetCurrentDirectory(dir);
            }
            catch (ArgumentNullException e)
            {
                // Pokazuje aktualny katalog roboczy aplikacji.
                Console.WriteLine(Directory.GetCurrentDirectory());
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
        * 
        * @param string|null dir Katalog do sprawdzenia.
        */
        static void showDirectory(string dir)
        {
            IEnumerable<string> dirData;
            IEnumerable<string> fileData;
            try
            {
                // Jesli argument dir nie zostal podany uzyj aktualnego katalogu roboczego.
                if (dir == null)
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
    }
}
