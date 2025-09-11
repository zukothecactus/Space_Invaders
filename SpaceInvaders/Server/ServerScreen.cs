using System;

namespace Server
{
    class ServerScreen
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SPACE INVADERS SERVER ===");
            Console.WriteLine("Pokretanje servera...\n");
            
            try
            {
                GameServer server = new GameServer();
                server.Start();
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