namespace PublicApi.Entities
{
    public class AppRole
    {
        public ICollection<AppUserRole> UserRoles { get; set; }
    }
}