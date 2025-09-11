using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Player2
{
    public class SpaceInvadersClient
    {
        private const int TCP_PORT = 51000; // Port za TCP komunikaciju (port forwarding)
        private const int UDP_PORT = 51001; // Port za UDP komunikaciju (port forwarding)
        private string serverIP; // Konfigurisaći se dinamički

        private int playerId;
        private TcpClient tcpClient;
        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private bool isConnected;
        private bool gameRunning;

        // Game state
        private int playerX, playerY;
        private int score, lives;

        public SpaceInvadersClient(int id)
        {
            playerId = id;
            isConnected = false;
            gameRunning = false;
        }

        public void Start()
        {
            try
            {
                Console.WriteLine("=== SPACE INVADERS - IGRAČ 2 ===");
                Console.WriteLine("Konfiguracija mrežne konekcije...\n");
                
                // Konfigurišemo server IP adresu
                ConfigureServerConnection();
                
                // TCP konekcija za početno povezivanje
                ConnectToServer();
                
                if (isConnected)
                {
                    Console.WriteLine("Uspešno povezan sa serverom!\n");
                    
                    // Učestvuj u početnom setup-u
                    HandleInitialSetup();
                    
                    // Pokreni UDP klijent za real-time komunikaciju
                    StartUdpClient();
                    
                    // Pokreni game loop
                    StartGameLoop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška: {ex.Message}");
            }
            finally
            {
                Cleanup();
            }
        }

        private void ConfigureServerConnection()
        {
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║            KONFIGURACIJA KONEKCIJE            ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("Izaberite tip konekcije:");
            Console.WriteLine("1. Lokalna konekcija (127.0.0.1)");
            Console.WriteLine("2. Udaljenu konekcija (eksterna IP adresa)");
            Console.WriteLine();
            Console.Write("Unesite izbor (1 ili 2): ");
            
            string choice = Console.ReadLine();
            
            if (choice == "2")
            {
                Console.WriteLine();
                Console.WriteLine("Za udaljenu konekciju potrebno je:");
                Console.WriteLine("1. Server mora imati konfigurisan port forwarding");
                Console.WriteLine("2. Portovi {0} (TCP) i {1} (UDP) moraju biti prosleđeni", TCP_PORT, UDP_PORT);
                Console.WriteLine("3. Morate znati eksternu IP adresu servera");
                Console.WriteLine();
                Console.Write("Unesite eksternu IP adresu servera: ");
                serverIP = Console.ReadLine().Trim();
                
                if (string.IsNullOrEmpty(serverIP))
                {
                    Console.WriteLine("Neispravna IP adresa, koristim lokalnu konekciju...");
                    serverIP = "127.0.0.1";
                }
                else
                {
                    Console.WriteLine($"Pokušavam konekciju na {serverIP}:{TCP_PORT}");
                }
            }
            else
            {
                serverIP = "127.0.0.1"; // Lokalna konekcija
                Console.WriteLine("Koristim lokalnu konekciju...");
            }
            
            Console.WriteLine();
        }

        private void ConnectToServer()
        {
            try
            {
                Console.WriteLine($"Povezujem se na {serverIP}:{TCP_PORT}...");
                tcpClient = new TcpClient();
                
                // Timeout za konekciju
                var result = tcpClient.BeginConnect(serverIP, TCP_PORT, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(10));
                
                if (!success)
                {
                    throw new TimeoutException("Timeout konekcije - proverite da li je server pokrenut i dostupan");
                }
                
                tcpClient.EndConnect(result);
                isConnected = true;
                serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), UDP_PORT);
                
                Console.WriteLine("✓ TCP konekcija uspostavljena");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Neuspešno povezivanje sa serverom: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Mogući uzroci:");
                Console.WriteLine("- Server nije pokrenut");
                Console.WriteLine("- Neispravna IP adresa");
                Console.WriteLine("- Port forwarding nije konfigurisan (za udaljenu konekciju)");
                Console.WriteLine("- Firewall blokira konekciju");
                Console.WriteLine("- Mreža nije dostupna");
                throw;
            }
        }

        private void HandleInitialSetup()
        {
            NetworkStream stream = tcpClient.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                // Čitamo poruku za ime (Player 2 ne odabira mod)
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string nameMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(nameMessage);

                // Pošaljemo ime
                Console.Write("Vaše ime: ");
                string playerName = Console.ReadLine();
                if (string.IsNullOrEmpty(playerName))
                    playerName = $"Igrač {playerId}";
                
                byte[] nameData = Encoding.UTF8.GetBytes(playerName);
                stream.Write(nameData, 0, nameData.Length);

                // Čitamo potvrdu
                int confirmBytes = stream.Read(buffer, 0, buffer.Length);
                string confirmMessage = Encoding.UTF8.GetString(buffer, 0, confirmBytes);
                Console.WriteLine(confirmMessage);

                Console.WriteLine("\nPripremate se za igru...");
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška tokom setup-a: {ex.Message}");
                throw;
            }
        }

        private void StartUdpClient()
        {
            try
            {
                // Kreiramo UDP klijent sa bilo kojim dostupnim portom
                udpClient = new UdpClient(0); // 0 znači da OS automatski dodeljuje port
                Console.WriteLine($"✓ UDP klijent pokrenut za komunikaciju sa {serverIP}:{UDP_PORT}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pokretanja UDP klijenta: {ex.Message}");
                throw;
            }
        }

        private void StartGameLoop()
        {
            gameRunning = true;
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║               IGRA POČINJE                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine($"Povezan na server: {serverIP}");
            Console.WriteLine("Kontrole:");
            Console.WriteLine("  A / ← - kretanje levo");
            Console.WriteLine("  D / → - kretanje desno");
            Console.WriteLine("  SPACE - pucanje");
            Console.WriteLine("  Q - izlaz iz igre");
            Console.WriteLine();
            Console.WriteLine("• Glavni prikaz igre je na server konzoli");
            Console.WriteLine("• Vaš status će se prikazati u naslovnoj liniji");
            Console.WriteLine();
            Console.WriteLine("Pritisnite bilo koji taster za početak...");
            
            // Čekamo da korisnik pritisne taster
            Console.ReadKey();
            
            // Prvo pošaljemo inicijalni signal serveru da smo spremni
            SendActionToServer("READY");
            
            // Pokretamo thread za slušanje UDP poruka od servera
            Task.Run(() => ListenForUpdates());

            // Čekamo malo da se thread pokrene
            Thread.Sleep(1000);

            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║              IGRA JE AKTIVNA                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine($"🌐 Server: {serverIP}:{TCP_PORT}/{UDP_PORT}");
            Console.WriteLine("🎮 Koristite kontrole za igranje:");
            Console.WriteLine("   A/D - kretanje levo/desno");
            Console.WriteLine("   SPACE - pucanje");
            Console.WriteLine("   Q - izlaz");
            Console.WriteLine();
            Console.WriteLine("📺 Glavni prikaz igre je na server konzoli");
            Console.WriteLine("📊 Vaš status se prikazuje u naslovnoj liniji");
            Console.WriteLine();
            Console.WriteLine("Srećno! 🚀");

            // Glavni input loop
            while (gameRunning && isConnected)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    HandleInput(keyInfo.Key);
                }

                Thread.Sleep(50); // Sprečava da loop troši previše CPU-a
            }
        }

        private void HandleInput(ConsoleKey key)
        {
            string action = "";

            switch (key)
            {
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    action = "MOVE_LEFT";
                    break;
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    action = "MOVE_RIGHT";
                    break;
                case ConsoleKey.Spacebar:
                    action = "SHOOT";
                    break;
                case ConsoleKey.Q:
                    action = "QUIT";
                    gameRunning = false;
                    break;
                default:
                    return; // Ignorišemo druge tastere
            }

            if (!string.IsNullOrEmpty(action))
            {
                SendActionToServer(action);
            }
        }

        private void SendActionToServer(string action)
        {
            try
            {
                string message = $"{playerId}:{action}";
                byte[] data = Encoding.UTF8.GetBytes(message);
                udpClient.Send(data, data.Length, serverEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška slanja akcije serveru: {ex.Message}");
                
                // Pokušaj reconnect
                try
                {
                    udpClient?.Close();
                    udpClient = new UdpClient(0);
                }
                catch (Exception)
                {
                    // Ignoriši reconnect greške
                }
            }
        }

        private void ListenForUpdates()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (gameRunning && isConnected)
            {
                try
                {
                    // Postavimo timeout da ne blokira beskončno
                    udpClient.Client.ReceiveTimeout = 2000; // 2 sekunde timeout

                    byte[] receivedData = udpClient.Receive(ref remoteEndPoint);
                    string gameUpdate = Encoding.UTF8.GetString(receivedData);
                    
                    ProcessGameUpdate(gameUpdate);
                }
                catch (SocketException ex)
                {
                    // Ignorišemo timeout greške, one su očekivane
                    if (ex.SocketErrorCode != SocketError.TimedOut && gameRunning)
                    {
                        Console.WriteLine($"UDP socket greška: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    if (gameRunning)
                        Console.WriteLine($"Greška prijema ažuriranja: {ex.Message}");
                }
            }
        }

        private void ProcessGameUpdate(string gameUpdate)
        {
            try
            {
                string[] lines = gameUpdate.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                
                if (lines.Length > 0 && lines[0] == "GAME_UPDATE")
                {
                    // Parsiramo ažuriranje igre
                    // Format: PLAYER:id:x:y:score:lives
                    //         OBSTACLE:x:y
                    //         BULLET:x:y:playerId
                    
                    foreach (string line in lines)
                    {
                        if (line.StartsWith($"PLAYER:{playerId}:"))
                        {
                            string[] parts = line.Split(':');
                            if (parts.Length >= 6)
                            {
                                if (int.TryParse(parts[2], out int x) &&
                                    int.TryParse(parts[3], out int y) &&
                                    int.TryParse(parts[4], out int s) &&
                                    int.TryParse(parts[5], out int l))
                                {
                                    playerX = x;
                                    playerY = y;
                                    score = s;
                                    lives = l;
                                }
                            }
                        }
                    }

                    // Prikazujemo lokalne informacije
                    UpdateDisplay();
                }
            }
            catch (Exception)
            {
                // Ignoriši greške parsiranja
            }
        }

        private void UpdateDisplay()
        {
            // Prikazujemo informacije o igraču u naslovnoj liniji
            Console.Title = $"Space Invaders [{serverIP}] - Pozicija: ({playerX},{playerY}) | Skor: {score} | Životi: {lives}";
        }

        private void Cleanup()
        {
            try
            {
                gameRunning = false;
                isConnected = false;

                udpClient?.Close();
                tcpClient?.Close();

                Console.WriteLine("\n╔══════════════════════════════════════════════╗");
                Console.WriteLine("║            Veza sa serverom zatvorena        ║");
                Console.WriteLine("╚══════════════════════════════════════════════╝");
                Console.WriteLine("\nHvala što ste igrali! 👋");
                Console.WriteLine("Pritisnite bilo koji taster za izlaz...");
                Console.ReadKey();
            }
            catch (Exception)
            {
                // Ignoriši cleanup greške
            }
        }
    }
}
