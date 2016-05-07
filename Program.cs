using System;
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
        * @params string dir Sciezka do zmiany.
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
    }
}
