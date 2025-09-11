using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Server
{
    // Pozicija na mapi
    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Position other)
                return X == other.X && Y == other.Y;
            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }

    // Igrač u igri
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Position Position { get; set; }
        public int Score { get; set; }
        public int Lives { get; set; }
        public IPEndPoint TcpEndPoint { get; set; }
        public IPEndPoint UdpEndPoint { get; set; }
        public bool IsConnected { get; set; }
        public bool IsReady { get; set; }

        public Player(int id, string name)
        {
            Id = id;
            Name = name;
            Position = new Position(0, 0);
            Score = 0;
            Lives = 3;
            IsConnected = true;
            IsReady = false;
        }
    }

    // Prepreka koja se spušta
    public class Obstacle
    {
        public Position Position { get; set; }
        public bool IsActive { get; set; }

        public Obstacle(int x, int y)
        {
            Position = new Position(x, y);
            IsActive = true;
        }
    }

    // Metak koji puca igrač
    public class Bullet
    {
        public Position Position { get; set; }
        public int PlayerId { get; set; }
        public bool IsActive { get; set; }

        public Bullet(int x, int y, int playerId)
        {
            Position = new Position(x, y);
            PlayerId = playerId;
            IsActive = true;
        }
    }

    // Glavni server za igru
    public class GameServer
    {
        private const int MAP_WIDTH = 30;
        private const int MAP_HEIGHT = 15;
        private const int TCP_PORT = 51000; // Port za TCP komunikaciju (prosleđivanje porta)
        private const int UDP_PORT = 51001; // Port za UDP komunikaciju (prosleđivanje porta)

        private TcpListener tcpListener;
        private UdpClient udpServer;
        private Dictionary<int, Player> players;
        private List<Obstacle> obstacles;
        private List<Bullet> bullets;
        private char[,] gameMap;
        private Random random;
        private bool gameRunning;
        private bool gameStarted;
        private int targetScore;
        private int playerCount;
        private Timer gameTimer;
        private Timer obstacleTimer;
        private int obstacleUpdateCounter; // Dodajemo kao instance varijablu

        public GameServer()
        {
            players = new Dictionary<int, Player>();
            obstacles = new List<Obstacle>();
            bullets = new List<Bullet>();
            gameMap = new char[MAP_HEIGHT, MAP_WIDTH];
            random = new Random();
            gameRunning = false;
            gameStarted = false;
            obstacleUpdateCounter = 0; // Inicijalizujemo counter
        }

        public void Start()
        {
            InitializeServer();
            WaitForPlayers();
        }

        private void InitializeServer()
        {
            try
            {
                // TCP server za početno povezivanje - sluša na svim interfejsima
                tcpListener = new TcpListener(IPAddress.Any, TCP_PORT);
                tcpListener.Start();

                // UDP server za real-time komunikaciju - sluša na svim interfejsima
                udpServer = new UdpClient(UDP_PORT);

                Console.WriteLine($"=== SPACE INVADERS SERVER ===");
                Console.WriteLine($"Server pokrenut na TCP portu {TCP_PORT} i UDP portu {UDP_PORT}");
                
                // Prikazujemo lokalnu IP adresu
                string localIP = GetLocalIPAddress();
                Console.WriteLine($"Lokalna IP adresa: {localIP}");
                Console.WriteLine($"Za udaljeno povezivanje:");
                Console.WriteLine($"1. Konfiguriši port forwarding na ruteru za portove {TCP_PORT} i {UDP_PORT}");
                Console.WriteLine($"2. Koristi eksternu IP adresu u klijentskim aplikacijama");
                Console.WriteLine($"3. Proveri eksternu IP na: https://whatismyipaddress.com/");
                Console.WriteLine("Čekam igrače...\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška prilikom pokretanja servera: {ex.Message}");
                return;
            }
        }

        private string GetLocalIPAddress()
        {
            try
            {
                string hostName = Dns.GetHostName();
                IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška dobijanja IP adrese: {ex.Message}");
            }
            return "Nepoznato";
        }

        private void WaitForPlayers()
        {
            // Pokretamo UDP listening u posebnom thread-u PRVO
            Task.Run(() => HandleUdpCommunication());

            // Čekamo prvi igrač da se poveže i odabere mod
            TcpClient firstClient = tcpListener.AcceptTcpClient();
            HandleFirstPlayer(firstClient);

            // Ako je odabran mod za 2 igrača, čekamo drugi
            if (playerCount == 2)
            {
                Console.WriteLine("Čekam drugi igrač...");
                TcpClient secondClient = tcpListener.AcceptTcpClient();
                HandleSecondPlayer(secondClient);
            }

            // Čekamo da se svi igrači pripremi
            WaitForPlayersReady();

            // Pokrećemo igru
            StartGame();
        }

        private void WaitForPlayersReady()
        {
            Console.WriteLine("Čekam da se igrači pripremi...");

            int maxWait = 30; // Maksimalno 30 sekundi čekanja
            int waited = 0;

            while (!AreAllPlayersReady() && waited < maxWait)
            {
                Thread.Sleep(1000);
                waited++;
            }

            if (AreAllPlayersReady())
            {
                Console.WriteLine("Svi igrači su spremni!");
            }
            else
            {
                Console.WriteLine("Pokrećem igru...");
            }
            
            Thread.Sleep(1000);
        }

        private bool AreAllPlayersReady()
        {
            if (players.Count == 0) return false;

            foreach (var player in players.Values)
            {
                if (player.IsConnected && !player.IsReady)
                {
                    return false;
                }
            }
            return true;
        }

        private void HandleFirstPlayer(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                // Pošaljemo poruku za odabir moda
                string modeMessage = "Odaberite mod igre:\n1 - Jedan igrač\n2 - Dva igrača\nUnesite broj (1 ili 2):";
                byte[] data = Encoding.UTF8.GetBytes(modeMessage);
                stream.Write(data, 0, data.Length);

                // Čitamo odabir moda
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                
                if (int.TryParse(response, out int mode) && (mode == 1 || mode == 2))
                {
                    playerCount = mode;
                    Console.WriteLine($"Odabran mod za {playerCount} igrač(a)");
                }
                else
                {
                    playerCount = 1; // Default
                    Console.WriteLine("Neispravan unos, postavljam mod za 1 igrača");
                }

                // Pitamo za target score
                string scoreMessage = "Unesite broj poena potreban za pobedu (default 10):";
                data = Encoding.UTF8.GetBytes(scoreMessage);
                stream.Write(data, 0, data.Length);

                bytesRead = stream.Read(buffer, 0, buffer.Length);
                response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                
                if (int.TryParse(response, out int score) && score > 0)
                {
                    targetScore = score;
                }
                else
                {
                    targetScore = 10; // Default
                }

                Console.WriteLine($"Cilj igre: {targetScore} poena");

                // Pitamo za ime
                string nameMessage = "Unesite vaše ime:";
                data = Encoding.UTF8.GetBytes(nameMessage);
                stream.Write(data, 0, data.Length);

                bytesRead = stream.Read(buffer, 0, buffer.Length);
                string playerName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                if (string.IsNullOrEmpty(playerName)) playerName = "Igrač 1";

                // Kreiramo igrača
                Player player1 = new Player(1, playerName);
                player1.Position = new Position(MAP_WIDTH / 4, MAP_HEIGHT - 2);
                player1.TcpEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                players[1] = player1;

                // Pošaljemo potvrdu
                string confirmMessage = $"Dobrodošli {playerName}! Vaša pozicija: ({player1.Position.X}, {player1.Position.Y})";
                data = Encoding.UTF8.GetBytes(confirmMessage);
                stream.Write(data, 0, data.Length);

                Console.WriteLine($"Igrač 1 ({playerName}) se povezao");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška prilikom rukovanja prvim igračem: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        private void HandleSecondPlayer(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                // Pitamo za ime
                string nameMessage = "Unesite vaše ime (Igrač 2):";
                byte[] data = Encoding.UTF8.GetBytes(nameMessage);
                stream.Write(data, 0, data.Length);

                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string playerName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                if (string.IsNullOrEmpty(playerName)) playerName = "Igrač 2";

                // Kreiramo igrača
                Player player2 = new Player(2, playerName);
                player2.Position = new Position(3 * MAP_WIDTH / 4, MAP_HEIGHT - 2);
                player2.TcpEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                players[2] = player2;

                // Pošaljemo potvrdu
                string confirmMessage = $"Dobrodošli {playerName}! Vaša pozicija: ({player2.Position.X}, {player2.Position.Y})";
                data = Encoding.UTF8.GetBytes(confirmMessage);
                stream.Write(data, 0, data.Length);

                Console.WriteLine($"Igrač 2 ({playerName}) se povezao");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška prilikom rukovanja drugim igračem: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        private void StartGame()
        {
            gameRunning = true;
            gameStarted = true;
            
            // Inicijalizujemo mapu
            InitializeMap();

            // Pokretamo game loop - brži refresh za pozicije (100ms)
            gameTimer = new Timer(UpdateGame, null, 0, 100); // Update svakih 100ms (bilo 150ms)

            // Pokretamo timer za prepreke - sporiji pad prepreka
            obstacleTimer = new Timer(GenerateObstacle, null, 3000, 4000); // Nova prepreka svakih 4 sekunde (bilo 3s)

            // Main loop za prikaz igre - brži prikaz
            while (gameRunning)
            {
                Console.Clear();
                DisplayGame();
                Thread.Sleep(150); // Prikaz se osvežava svakih 150ms (bilo 300ms)

                // Proveravamo da li je igra završena
                CheckGameEnd();
            }

            Cleanup();
        }

        private void InitializeMap()
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                for (int x = 0; x < MAP_WIDTH; x++)
                {
                    gameMap[y, x] = ' ';
                }
            }
        }

        private void HandleUdpCommunication()
        {
            while (true)
            {
                try
                {
                    IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedData = udpServer.Receive(ref clientEndPoint);
                    string message = Encoding.UTF8.GetString(receivedData);

                    ProcessPlayerAction(message, clientEndPoint);
                }
                catch (Exception ex)
                {
                    if (gameRunning)
                        Console.WriteLine($"UDP greška: {ex.Message}");
                    break;
                }
            }
        }

        private void ProcessPlayerAction(string action, IPEndPoint clientEndPoint)
        {
            // Parsing: "PLAYER_ID:ACTION" (npr. "1:MOVE_LEFT", "2:SHOOT")
            string[] parts = action.Split(':');
            if (parts.Length != 2) return;

            if (int.TryParse(parts[0], out int playerId) && players.ContainsKey(playerId))
            {
                Player player = players[playerId];
                player.UdpEndPoint = clientEndPoint; // Ažuriramo UDP endpoint

                switch (parts[1])
                {
                    case "READY":
                        player.IsReady = true;
                        break;
                    case "MOVE_LEFT":
                        if (gameStarted && player.Position.X > 0)
                            player.Position.X--;
                        break;
                    case "MOVE_RIGHT":
                        if (gameStarted && player.Position.X < MAP_WIDTH - 1)
                            player.Position.X++;
                        break;
                    case "SHOOT":
                        if (gameStarted)
                            Shoot(player);
                        break;
                    case "QUIT":
                        player.IsConnected = false;
                        break;
                }
            }
        }

        private void Shoot(Player player)
        {
            // Kreiramo metak na poziciji igrača
            Bullet bullet = new Bullet(player.Position.X, player.Position.Y - 1, player.Id);
            bullets.Add(bullet);
        }

        private void UpdateGame(object state)
        {
            if (!gameRunning || !gameStarted) return;

            // Pomeramo prepreke nadole - sporije pomeranje
            // Dodajemo counter da se prepreke pomeraju svakih nekoliko update-ova
            obstacleUpdateCounter++;
            
            if (obstacleUpdateCounter >= 2) // Prepreke se pomeraju svakih 200ms umesto 100ms
            {
                obstacleUpdateCounter = 0;
                
                for (int i = obstacles.Count - 1; i >= 0; i--)
                {
                    obstacles[i].Position.Y++;
                    
                    // Uklanjamo prepreke koje su izašle sa mape
                    if (obstacles[i].Position.Y >= MAP_HEIGHT)
                    {
                        obstacles.RemoveAt(i);
                    }
                }
            }

            // Pomeramo metke naviše - normalna brzina
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i].Position.Y--;
                
                // Uklanjamo metke koji su izašli sa mape
                if (bullets[i].Position.Y < 0)
                {
                    bullets.RemoveAt(i);
                }
            }

            // Proveravamo kolizije metaka sa preprekama
            CheckCollisions();

            // Ažuriramo mapu
            UpdateMap();

            // Šaljemo ažuriranje igračima
            SendGameUpdate();
        }

        private void GenerateObstacle(object state)
        {
            if (!gameRunning || !gameStarted) return;

            // Generišemo prepreku na nasumičnoj poziciji na vrhu
            int x = random.Next(0, MAP_WIDTH);
            Obstacle obstacle = new Obstacle(x, 0);
            obstacles.Add(obstacle);
        }

        private void CheckCollisions()
        {
            // Lista objekata za uklanjanje (da izbegnemo menjanje lista tokom iteracije)
            var bulletsToRemove = new List<int>();
            var obstaclesToRemove = new List<int>();

            // Proveravamo kolizije metaka sa preprekama
            for (int i = 0; i < bullets.Count; i++)
            {
                for (int j = 0; j < obstacles.Count; j++)
                {
                    // Proveravamo tačno poklapanje pozicija ili "near miss" 
                    // (zbog brzine kretanja objekti se mogu "preskočiti")
                    bool collision = false;
                    
                    // Tačno poklapanje
                    if (bullets[i].Position.X == obstacles[j].Position.X && 
                        bullets[i].Position.Y == obstacles[j].Position.Y)
                    {
                        collision = true;
                    }
                    // "Near miss" - metak prošao kroz poziciju prepreke
                    else if (bullets[i].Position.X == obstacles[j].Position.X)
                    {
                        // Metak se kreće naviše, prepreka se kreće nadole
                        // Proveravamo da li su se "mimoišli"
                        int bulletNextY = bullets[i].Position.Y + 1; // Gde će biti metak u sledećem frame-u
                        int obstacleNextY = obstacles[j].Position.Y - 1; // Gde je bila prepreka u prethodnom frame-u
                        
                        if (bulletNextY >= obstacles[j].Position.Y && 
                            bullets[i].Position.Y <= obstacleNextY)
                        {
                            collision = true;
                        }
                    }

                    if (collision)
                    {
                        // Kolizija! Dodajemo poen igraču
                        int playerId = bullets[i].PlayerId;
                        if (players.ContainsKey(playerId))
                        {
                            players[playerId].Score++;
                        }

                        // Označavamo za uklanjanje
                        if (!bulletsToRemove.Contains(i))
                            bulletsToRemove.Add(i);
                        if (!obstaclesToRemove.Contains(j))
                            obstaclesToRemove.Add(j);
                        
                        break; // Jedan metak može pogoditi samo jednu prepreku
                    }
                }
            }

            // Proveravamo da li prepreke pogađaju igrače
            for (int i = 0; i < obstacles.Count; i++)
            {
                foreach (var player in players.Values)
                {
                    // Proveravamo da li prepreka pogađa igrača
                    if (obstacles[i].Position.X == player.Position.X && 
                        obstacles[i].Position.Y == player.Position.Y)
                    {
                        player.Lives--;
                        
                        // Označavamo prepreku za uklanjanje
                        if (!obstaclesToRemove.Contains(i))
                            obstaclesToRemove.Add(i);
                        
                        break; // Jedna prepreka može pogoditi samo jednog igrača
                    }
                }
            }

            // Uklanjamo objekte u obrnutom redoslede (od najvećeg indeksa ka najmanjem)
            bulletsToRemove.Sort();
            bulletsToRemove.Reverse();
            foreach (int index in bulletsToRemove)
            {
                if (index < bullets.Count)
                    bullets.RemoveAt(index);
            }

            obstaclesToRemove.Sort();
            obstaclesToRemove.Reverse();
            foreach (int index in obstaclesToRemove)
            {
                if (index < obstacles.Count)
                    obstacles.RemoveAt(index);
            }
        }

        private void UpdateMap()
        {
            // Očistimo mapu
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                for (int x = 0; x < MAP_WIDTH; x++)
                {
                    gameMap[y, x] = ' ';
                }
            }

            // Dodajemo igrače
            foreach (var player in players.Values)
            {
                if (player.IsConnected && player.Lives > 0)
                {
                    if (IsValidPosition(player.Position.X, player.Position.Y))
                        gameMap[player.Position.Y, player.Position.X] = (char)('0' + player.Id);
                }
            }

            // Dodajemo prepreke
            foreach (var obstacle in obstacles)
            {
                if (IsValidPosition(obstacle.Position.X, obstacle.Position.Y))
                    gameMap[obstacle.Position.Y, obstacle.Position.X] = '#';
            }

            // Dodajemo metke
            foreach (var bullet in bullets)
            {
                if (IsValidPosition(bullet.Position.X, bullet.Position.Y))
                    gameMap[bullet.Position.Y, bullet.Position.X] = '|';
            }
        }

        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < MAP_WIDTH && y >= 0 && y < MAP_HEIGHT;
        }

        private void DisplayGame()
        {
            Console.WriteLine("╔═══════════ SPACE INVADERS ═══════════╗");
            
            // Prikazujemo informacije o igračima
            foreach (var player in players.Values)
            {
                Console.WriteLine($"║ {player.Name,-10} │ Skor: {player.Score,2} │ Životi: {player.Lives} │ Pos:({player.Position.X,2},{player.Position.Y,2}) ║");
            }
            Console.WriteLine($"║ Cilj: {targetScore,2} poena                           ║");
            Console.WriteLine("╚═══════════════════════════════════════╝");
            Console.WriteLine();

            // Prikazujemo gornju granicu
            Console.WriteLine("┌" + new string('─', MAP_WIDTH) + "┐");

            // Prikazujemo mapu
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                Console.Write("│");
                for (int x = 0; x < MAP_WIDTH; x++)
                {
                    Console.Write(gameMap[y, x]);
                }
                Console.WriteLine("│");
            }

            // Prikazujemo donju granicu
            Console.WriteLine("└" + new string('─', MAP_WIDTH) + "┘");
            
            Console.WriteLine($"Prepreke: {obstacles.Count,2} │ Metci: {bullets.Count,2} │ Status: {(gameStarted ? "AKTIVNA" : "ČEKA")} │ FPS: {(gameStarted ? "6.7" : "N/A")}");
        }

        private void SendGameUpdate()
        {
            if (!gameStarted) return;

            // Kreiramo poruku sa stanjem igre
            StringBuilder gameState = new StringBuilder();
            gameState.AppendLine("GAME_UPDATE");
            
            // Dodajemo informacije o igračima
            foreach (var player in players.Values)
            {
                gameState.AppendLine($"PLAYER:{player.Id}:{player.Position.X}:{player.Position.Y}:{player.Score}:{player.Lives}");
            }

            // Dodajemo prepreke
            foreach (var obstacle in obstacles)
            {
                gameState.AppendLine($"OBSTACLE:{obstacle.Position.X}:{obstacle.Position.Y}");
            }

            // Dodajemo metke
            foreach (var bullet in bullets)
            {
                gameState.AppendLine($"BULLET:{bullet.Position.X}:{bullet.Position.Y}:{bullet.PlayerId}");
            }

            byte[] data = Encoding.UTF8.GetBytes(gameState.ToString());

            // Šaljemo svim povezanim igračima
            foreach (var player in players.Values)
            {
                if (player.IsConnected && player.UdpEndPoint != null)
                {
                    try
                    {
                        udpServer.Send(data, data.Length, player.UdpEndPoint);
                    }
                    catch (Exception)
                    {
                        // Ignoriši greške slanja
                    }
                }
            }
        }

        private void CheckGameEnd()
        {
            // Proveravamo da li je neko dostigao target score
            foreach (var player in players.Values)
            {
                if (player.Score >= targetScore)
                {
                    gameRunning = false;
                    Console.Clear();
                    Console.WriteLine("╔══════════════════════════════════════╗");
                    Console.WriteLine("║            IGRA ZAVRŠENA            ║");
                    Console.WriteLine("╚══════════════════════════════════════╝");
                    Console.WriteLine($"\n🎉 POBEDNIK: {player.Name} sa {player.Score} poena! 🎉\n");
                    ShowFinalResults();
                    return;
                }
            }

            // Proveravamo da li svi igrači izgubili
            bool allPlayersLost = true;
            foreach (var player in players.Values)
            {
                if (player.Lives > 0 && player.IsConnected)
                {
                    allPlayersLost = false;
                    break;
                }
            }

            if (allPlayersLost)
            {
                gameRunning = false;
                Console.Clear();
                Console.WriteLine("╔══════════════════════════════════════╗");
                Console.WriteLine("║            IGRA ZAVRŠENA            ║");
                Console.WriteLine("║       Svi igrači su izgubili!       ║");
                Console.WriteLine("╚══════════════════════════════════════╝");
                ShowFinalResults();
            }
        }

        private void ShowFinalResults()
        {
            Console.WriteLine("\n╔════════════ FINALNI REZULTATI ════════════╗");
            var sortedPlayers = players.Values.OrderByDescending(p => p.Score).ToList();
            
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                var player = sortedPlayers[i];
                string medal = i == 0 ? "🥇" : i == 1 ? "🥈" : "🥉";
                Console.WriteLine($"║ {medal} {i + 1,-2}. {player.Name,-15} - {player.Score,3} poena ║");
            }
            Console.WriteLine("╚════════════════════════════════════════════╝");

            Console.WriteLine("\nPritisnite bilo koji taster za izlaz...");
            Console.ReadKey();
        }

        private void Cleanup()
        {
            try
            {
                gameTimer?.Dispose();
                obstacleTimer?.Dispose();
                tcpListener?.Stop();
                udpServer?.Close();
            }
            catch (Exception)
            {
                // Ignoriši cleanup greške
            }
        }
    }
}
