using System.ComponentModel.DataAnnotations;

namespace DrawItFasterGame.Models
{
	public class User
	{
		[Key]
		public int UserID { get; set; }
		public string Username { get; set; }
		public string PasswordHash { get; set; }
		public string? ProfilePictureUrl { get; set; }
		public int Highscore { get; set; }
		public int Rankscore { get; set; }
    }
}
