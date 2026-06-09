using Microsoft.AspNetCore.SignalR;

namespace DrawItFasterGame.Hubs;

public class GameHub : Hub
{
	// save player 
	private static Dictionary<string, List<string>> playersInGames = new();
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
	}
}