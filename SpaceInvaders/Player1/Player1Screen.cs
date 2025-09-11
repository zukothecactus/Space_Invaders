using System;

namespace Player1
{
    class Player1Screen
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SPACE INVADERS - IGRAČ 1 ===");
            Console.WriteLine("Pokretanje klijenta...\n");

            try
            {
                SpaceInvadersClient client = new SpaceInvadersClient(1);
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