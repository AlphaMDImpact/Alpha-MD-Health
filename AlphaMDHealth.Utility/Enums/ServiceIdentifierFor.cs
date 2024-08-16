namespace AlphaMDHealth.Utility;

/// <summary>
/// Library service identifier
/// </summary>
public enum ServiceIdentifierFor
{
    /// <summary>
    /// Blob
    /// </summary>
    Blob = 1,

    /// <summary>
    /// Food data with image
    /// </summary>
    FoodImage = 2,

    /// <summary>
    /// Food data using barcode
    /// </summary>
    FoodBarcode = 3,

    /// <summary>
    /// Food nutritions
    /// </summary>
    FoodNutrition = 4,

    /// <summary>
    /// Food data using search text
    /// </summary>
    FoodSearch = 5,

    /// <summary>
    /// Authentication
    /// </summary>
    Authentication = 6,

    /// <summary>
    /// Register device for notification
    /// </summary>
    RegisterDeviceForNotification = 7,

    /// <summary>
    /// Send notification
    /// </summary>
    SendNotification = 8
}