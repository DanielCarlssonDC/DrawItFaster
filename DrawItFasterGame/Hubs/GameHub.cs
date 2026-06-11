using Microsoft.AspNetCore.SignalR;

namespace DrawItFasterGame.Hubs;

public class GameHub : Hub
{
	// save player 
	private static Dictionary<string, List<string>> playersInGames = new();

	// Sparar valt ord tillhörande gameId
	private static Dictionary<string, string> currentWords = new();

	// Vem som ritar i varje game
	private static Dictionary<string, string> currentDrawers = new();

	// Vems tur det är att rita
	private static Dictionary<string, int> currentDrawerIndex = new();

	public async Task JoinGame(string gameId, string username)
	{
		// connection to room
		await Groups.AddToGroupAsync(Context.ConnectionId, gameId);

		if (!playersInGames.ContainsKey(gameId))
        {
            playersInGames[gameId] = new List<string>();
        }

        // ad player to list
        if (!playersInGames[gameId].Contains(username))
        {
            playersInGames[gameId].Add(username);
        }

		await Clients.Group(gameId).SendAsync("UpdatePlayerList", playersInGames[gameId]);

		await Clients.Group(gameId).SendAsync("PlayerJoined", username);
	}


	public async Task SendMessage(string gameId, string user, string message)
	{
		await Clients.Group(gameId).SendAsync("ReceiveMessage", user, message);
	}
	public async Task SendDrawingData(string gameId, object drawingData)
	{
		await Clients.OthersInGroup(gameId).SendAsync("ReceiveDrawingData", drawingData);
	}
	public async Task SendGuess(string gameId, string username, string guess)
	{
		await Clients.Group(gameId).SendAsync("ReceiveGuess", username, guess);

		// Om inget ord är valt gör ingenting
		if (!currentWords.ContainsKey(gameId))
		{
			return;
		}

		string correctWord = currentWords[gameId];

		// Jämför gissningen med rätt ord
		if (guess.Trim().ToLower() == correctWord.Trim().ToLower())
		{
			await Clients.Group(gameId).SendAsync("CorrectGuess", username);
		}
	}

	// Sparar valt ord för detta gameId
	public async Task SelectWord(string gameId, string username, string word)
	{
		// Spara ord som väljs
		currentWords[gameId] = word;

		// Spara ritare
		currentDrawers[gameId] = username;

		// Visa för alla att rundan startat
		await Clients.Group(gameId).SendAsync("WordSelected");
	}

	public async Task StartGame(string gameId)
	{
		if (!playersInGames.ContainsKey(gameId))
		{
			return;
		}

		var players = playersInGames[gameId];

		if (players.Count == 0)
		{
			return;
		}

		// Första spelaren blir ritare
		string drawer = players[0];

		currentDrawers[gameId] = drawer;

		await Clients.Group(gameId)
			.SendAsync("DrawerSelected", drawer);

		await Clients.Group(gameId)
			.SendAsync("GameStarted");
	}

	public async Task ClearCanvas(string gameId)
	{
		await Clients.OthersInGroup(gameId).SendAsync("CanvasCleared");
	}
}