using Microsoft.AspNetCore.Authorization;

namespace PublicApi
{
    public class AllowAnonymous : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (IAuthorizationRequirement requirement in context.PendingRequirements.ToList())
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
