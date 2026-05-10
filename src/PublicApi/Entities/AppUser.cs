namespace PublicApi.Entities
{
    public class AppUser
    {

        public required string Id { get; set; }

        public required string UserName { get; set; }

        public required string Password { get; set; }

        public required ICollection<AppUserRole> UserRoles { get; set; }

        public static readonly List<AppUser> ValidUsers = new()
        {
            new AppUser
            {
                Id = "admin" ,
                UserName = "admin",
                Password = "admin",
                UserRoles = new List<AppUserRole>()
                {
                    new AppUserRole { RoleId = "admin"},
                    new AppUserRole { RoleId = "user"},
                }
            },
            new AppUser
            {
                Id = "john" ,
                UserName = "john",
                Password = "john",
                UserRoles = new List<AppUserRole>()
                {
                    new AppUserRole { RoleId = "user"}
                }
            }
        };
    }
}
