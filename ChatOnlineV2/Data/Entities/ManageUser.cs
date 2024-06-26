﻿using Microsoft.AspNetCore.Identity;

namespace ChatOnlineV2.Data.Entities
{
    public class ManageUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public ICollection<Room> Rooms { get; set; }
        public ICollection<Message> Messages { get; set; }

    }
}
