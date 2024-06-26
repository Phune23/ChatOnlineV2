﻿namespace ChatOnlineV2.Data.Entities
{
    public class Room
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ManageUser Admin { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
