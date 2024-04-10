using Microsoft.AspNetCore.Identity;

namespace ChatOnlineV2.Data.Entities
{
    public class ManageUser : IdentityUser
    {
        public string DisPlayName { get; set; }
        public DateTime BrithDay { get; set; }
    }
}
