using AlphaMDHealth.Model;
using System.Runtime.Serialization;
using System.Security.Claims;

namespace AlphaMDHealth.IntegrationServiceBusinessLayer
{
    public class ServicePrincipal : ClaimsPrincipal
    {
        /// <summary>
        /// Library service details
        /// </summary>
        [DataMember]
        public List<LibraryServiceModel> LibraryDetails { get; set; }
    }
}