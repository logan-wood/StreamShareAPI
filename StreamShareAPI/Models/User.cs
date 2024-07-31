using System;
using Microsoft.EntityFrameworkCore;

namespace StreamShareAPI.Models
{
	public class User
	{
		public DbSet<User> Users { get; set; }

		public int Id { get; }

		public int SpotifyRefreshToken { get; set; }

		public int SpotifyAccessToken { get; set; }

		public User()
		{
		}
	}
}

