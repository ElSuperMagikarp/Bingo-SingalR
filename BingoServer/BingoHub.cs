using Microsoft.AspNetCore.SignalR;

public class BingoHub : Hub
{
    List<int> bingoNumbers;
    List<int> calledNumbers;

    bool fiveNumbersCalled = false;

    Dictionary<string, List<int>> activePlayers;

    // GAME MANAGEMENT
    public async Task StartGame()
    {
        resetBingo();

        await Clients.All.SendAsync("GameStarted");

        foreach (int number in bingoNumbers)
        {
            calledNumbers.Add(number);
            await Clients.All.SendAsync("NumberCalled", number);
        }
    }

    public async Task AskForNumbers()
    {
        Random rng = new Random();
        int[] possibleNumbers = Enumerable.Range(1, 99).ToArray();
        rng.Shuffle(possibleNumbers);
        
        List<int> clientNumbers = possibleNumbers.Take(15).ToList();

        activePlayers.Add(Context.ConnectionId, clientNumbers);
        await Clients.Caller.SendAsync("RecieveNumbers", clientNumbers);
    }

    public async Task CallFiveNumbers()
    {
        if (fiveNumbersCalled) { sendMessageToClient(Clients.Caller, "Five Numbers was already called"); return; }

        if (!activePlayers.ContainsKey(Context.ConnectionId)) { sendErrorToClient(Clients.Caller, "You are not in the player list"); return; }

        List<int> clientNumbers = activePlayers[Context.ConnectionId];

        int validNumbers = 0;
        foreach (int clientNumber in clientNumbers)
        {
            if (calledNumbers.Contains(clientNumber)) validNumbers++;
        }

        if(validNumbers<5)
        {
            sendMessageToClient(Clients.Caller, "You do not have at least 5 valid numbers");
            return;
        }

        fiveNumbersCalled = true;
        await Clients.All.SendAsync("SuccessfulFiveNumbers");
    }

    public async Task CallBingo()
    {
        if (!activePlayers.ContainsKey(Context.ConnectionId)) { sendErrorToClient(Clients.Caller, "You are not in the player list"); return; }

        List<int> clientNumbers = activePlayers[Context.ConnectionId];

        bool hasBingo = true;
        foreach (int clientNumber in clientNumbers)
        {
            if (!calledNumbers.Contains(clientNumber)) hasBingo = false;
        }

        if (!hasBingo)
        {
            sendMessageToClient(Clients.Caller, "You do not have a Bingo");
            return;
        }

        await Clients.All.SendAsync("SuccessfulBingo");
    }

    private void resetBingo()
    {
        Random rng = new Random();
        int[] bingoNumbersArray = Enumerable.Range(1, 99).ToArray();
        rng.Shuffle(bingoNumbersArray);
        
        bingoNumbers = bingoNumbersArray.ToList();
        calledNumbers = new List<int>();
        fiveNumbersCalled = false;
        activePlayers.Clear();
    }

    // CONNECTION MANAGEMENT
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        activePlayers.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
    private void sendMessageToClient(IClientProxy client, string message)
    {
        client.SendAsync("Message", message);
    }

    private void sendErrorToClient(IClientProxy client, string message)
    {
        client.SendAsync("Error", message);
    }
}