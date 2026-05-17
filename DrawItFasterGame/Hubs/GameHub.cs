using Microsoft.AspNetCore.SignalR;

namespace DrawItFasterGame.Hubs;

public class GameHub : Hub
{
	public async Task JoinGame(string gameId, string username)
	{
		await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
		await Clients.Group(gameId).SendAsync("PlayerJoined", username);
	}
	public async Task SendMessage(string gameId, string user, string message)
	{
		await Clients.Group(gameId).SendAsync("ReceiveMessage", user, message);
	}
	public async Task SendDrawingData(string gameId, object drawingData)
	{
		await Clients.Group(gameId).SendAsync("ReceiveDrawingData", drawingData);
	}
	public async Task SendGuess(string gameId, string username, string guess)
	{
		await Clients.Group(gameId).SendAsync("ReceiveGuess", username, guess);
	}
}