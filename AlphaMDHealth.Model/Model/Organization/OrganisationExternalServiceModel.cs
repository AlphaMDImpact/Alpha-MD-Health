using AlphaMDHealth.Utility;
using SQLite;

namespace AlphaMDHealth.Model;

public class OrganisationExternalServiceModel
{
    [PrimaryKey]
    public long OrganisationServiceID { get; set; }
    public long OrganisationID { get; set; }

    [MyCustomAttributes(ResourceConstants.R_SELECT_SERVICE_KEY)]
    public int ExternalServiceID { get; set; }

    [MyCustomAttributes(ResourceConstants.R_UNIT_PRICE_KEY)]
    public decimal UnitPrice { get; set; }

    [MyCustomAttributes(ResourceConstants.R_DISCOUNT_PERCENTAGE_KEY)]
    public decimal DiscountPercentage { get; set; }

    [MyCustomAttributes(ResourceConstants.R_MINIMUM_TO_BUY_KEY)]
    public long MinimumQuantityToBuy { get; set; }
    public bool ForPatient { get; set; }
    public bool IsActive { get; set; }
}
