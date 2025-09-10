using System;

namespace Player2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SPACE INVADERS - IGRAČ 2 ===");
            Console.WriteLine("Pokretanje klijenta...\n");

            try
            {
                SpaceInvadersClient client = new SpaceInvadersClient(2);
                client.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kritična greška: {ex.Message}");
                Console.WriteLine("Pritisnite bilo koji taster za izlaz...");
                Console.ReadKey();
            }
        }
    }
}