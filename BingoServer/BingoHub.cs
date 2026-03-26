using Microsoft.AspNetCore.SignalR;

public class Room
{
    public string Id { get; set; }
    public string HostId { get; set; }

    public Dictionary<string, List<int>> Players = new();
    public List<int> CalledNumbers = new();
    public List<int> BingoNumbers = new();

    public bool GameStarted = false;
    public bool FiveNumbersCalled = false;
}

public class BingoHub : Hub
{
    private static Dictionary<string, Room> rooms = new();
    private static Dictionary<string, string> connectionToRoom = new();
    private static object _lock = new();

    // ROOM MANAGEMENT

    public async Task CreateRoom(string roomId)
    {
        if (rooms.ContainsKey(roomId)) {
            await Clients.Caller.SendAsync("Message", "This room already exists");
            return;
        }

        lock (_lock)
        {

            Room room = new Room
            {
                Id = roomId,
                HostId = Context.ConnectionId
            };

            rooms.Add(roomId, room);
        }

        await JoinRoom(roomId);
    }

    public async Task JoinRoom(string roomId)
    {
        if (!rooms.ContainsKey(roomId)) {
            await Clients.Caller.SendAsync("Message", "This room doesn't exist");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        bool isHost = false;

        lock (_lock)
        {
            connectionToRoom[Context.ConnectionId] = roomId;
            isHost = rooms[roomId].HostId == Context.ConnectionId;
        }
        await Clients.Caller.SendAsync("RoomJoined", roomId, isHost);
    }

    // GAME MANAGEMENT

    public async Task StartGame()
    {
        if (!connectionToRoom.ContainsKey(Context.ConnectionId)) {
            await Clients.Caller.SendAsync("Message", "You are not in a Room");
            return;
        }

        string roomId = connectionToRoom[Context.ConnectionId];
        Room room = rooms[roomId];

        if (room.GameStarted)
        {
            await Clients.Caller.SendAsync("Message", "Game already started");
            return;
        }

        if (room.HostId != Context.ConnectionId) {
            await Clients.Caller.SendAsync("Message", "You are not the host of this room");
            return;
        }

        Random rng = new Random();
        int[] numbers = Enumerable.Range(1, 99).ToArray();
        numbers = numbers.OrderBy(x => rng.Next()).ToArray();

        room.BingoNumbers = numbers.ToList();
        room.CalledNumbers.Clear();
        room.FiveNumbersCalled = false;
        room.GameStarted = true;

        foreach (var player in room.Players.Keys.ToList())
        {
            int[] possible = Enumerable.Range(1, 99).OrderBy(x => rng.Next()).ToArray();
            List<int> clientNumbers = possible.Take(15).ToList();

            room.Players[player] = clientNumbers;
            await Clients.Client(player).SendAsync("ReceiveNumbers", clientNumbers);
            Console.WriteLine($"Sended ReceiveNumbers to {player}");
        }

        await Clients.Group(roomId).SendAsync("GameStarted");

        foreach (int number in room.BingoNumbers)
        {
            room.CalledNumbers.Add(number);
            await Clients.Group(roomId).SendAsync("NumberCalled", number);
            await Task.Delay(10000);
        }
    }

    public async Task RegisterPlayer()
    {
        if (!connectionToRoom.ContainsKey(Context.ConnectionId)) return;

        string roomId = connectionToRoom[Context.ConnectionId];
        Room room = rooms[roomId];

        lock (_lock)
        {
            if (!room.Players.ContainsKey(Context.ConnectionId))
                room.Players.Add(Context.ConnectionId, new List<int>());
        }
    }

    public async Task CallFiveNumbers()
    {
        if (!connectionToRoom.ContainsKey(Context.ConnectionId)) return;

        string roomId = connectionToRoom[Context.ConnectionId];
        Room room = rooms[roomId];

        if (room.FiveNumbersCalled)
        {
            await Clients.Caller.SendAsync("Message", "Already claimed");
            return;
        }

        if (!room.Players.ContainsKey(Context.ConnectionId)) return;

        var numbers = room.Players[Context.ConnectionId];

        int valid = numbers.Count(n => room.CalledNumbers.Contains(n));

        if (valid < 5)
        {
            await Clients.Caller.SendAsync("Message", "Not valid");
            return;
        }

        room.FiveNumbersCalled = true;
        await Clients.Group(roomId).SendAsync("SuccessfulFiveNumbers");
    }

    public async Task CallBingo()
    {
        if (!connectionToRoom.ContainsKey(Context.ConnectionId)) return;

        string roomId = connectionToRoom[Context.ConnectionId];
        Room room = rooms[roomId];

        if (!room.Players.ContainsKey(Context.ConnectionId)) return;

        var numbers = room.Players[Context.ConnectionId];

        bool bingo = numbers.All(n => room.CalledNumbers.Contains(n));

        if (!bingo)
        {
            await Clients.Caller.SendAsync("Message", "Not bingo");
            return;
        }

        await Clients.Group(roomId).SendAsync("SuccessfulBingo");
        ResetRoom(room);
    }

    private void ResetRoom(Room room)
    {
        room.CalledNumbers.Clear();
        room.BingoNumbers.Clear();
        room.FiveNumbersCalled = false;
        room.GameStarted = false;
        room.Players.Clear();
    }

    // DISCONNECT

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (!connectionToRoom.ContainsKey(Context.ConnectionId))
        {
            await base.OnDisconnectedAsync(exception);
            return;
        }

        string roomId = connectionToRoom[Context.ConnectionId];
        Room room = rooms[roomId];

        lock (_lock)
        {
            room.Players.Remove(Context.ConnectionId);
            connectionToRoom.Remove(Context.ConnectionId);

            if (room.HostId == Context.ConnectionId && room.Players.Count > 0)
            {
                room.HostId = room.Players.Keys.First();
            }

            if (room.Players.Count == 0)
            {
                rooms.Remove(roomId);
            }
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

        await base.OnDisconnectedAsync(exception);
    }
}