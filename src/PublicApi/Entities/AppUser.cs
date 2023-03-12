namespace PublicApi.Entities
{
    public class AppUser
    {

        public string Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public ICollection<AppUserRole> UserRoles { get; set; }

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
