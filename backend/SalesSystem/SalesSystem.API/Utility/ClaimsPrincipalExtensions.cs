using System.Security.Claims;
using SalesSystem.Utility;

namespace SalesSystem.API.Utility
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Id of the authenticated user, taken from the JWT instead of trusting a request parameter.
        /// </summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            string? value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(value, out int idUser))
            {
                throw new BusinessException("Invalid session, please log in again", StatusCodes.Status401Unauthorized);
            }
            return idUser;
        }
    }
}
