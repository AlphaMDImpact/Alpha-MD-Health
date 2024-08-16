using System.Security.Claims;

namespace AlphaMDHealth.ServiceBusinessLayer
{
    public class UserPrincipal : ClaimsPrincipal
    {
        /// <summary>
        /// Logged in user id
        /// </summary>
        public long AccountID { get; set; }

        /// <summary>
        /// Determines whether the current principal belongs to the specified role
        /// </summary>
        /// <param name="role">The name of the role for which to check membership</param>
        /// <returns>
        /// true if the current principal is a member of the specified role; otherwise, false
        /// </returns>
        public override bool IsInRole(string role)
        {
            return false;
        }
    }
}