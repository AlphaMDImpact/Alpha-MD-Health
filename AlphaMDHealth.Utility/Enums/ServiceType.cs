namespace AlphaMDHealth.Utility;

/// <summary>
/// Micro services service type
/// </summary>
public enum ServiceType
{
    /// <summary>
    /// Azure blob storage
    /// </summary>
    AzureBlobStorage,

    /// <summary>
    /// AWS document storage
    /// </summary>
    AmazonS3,

    /// <summary>
    /// Google document storage
    /// </summary>
    GoogleStorage,

    /// <summary>
    /// Nutritionix food data provider
    /// </summary>
    Nutritionix,

    /// <summary>
    /// Fat secret food data provider
    /// </summary>
    FatSecret,

    /// <summary>
    /// Spoonacular food data provider
    /// </summary>
    Spoonacular,

    /// <summary>
    /// Edamam food data provider
    /// </summary>
    Edamam,

    /// <summary>
    /// Send grid email service provider
    /// </summary>
    SendGrid,

    /// <summary>
    /// Text local sms provider
    /// </summary>
    TextLocal,

    /// <summary>
    /// Twillio Video Call provider
    /// </summary>
    Twillio,

    /// <summary>
    /// Liveswitch video call provider (Frozen Mountain)
    /// </summary>
    LiveSwitch,

    /// <summary>
    /// Vidyo.Io video call Provider
    /// </summary>
    Vidyo_Io,

    /// <summary>
    /// OpenTok video call provider
    /// </summary>
    OpenTok,

    /// <summary>
    /// Daily.Co video call provider
    /// </summary>
    Daily_Co,

    /// <summary>
    /// Azure notification
    /// </summary>
    AzureNotification,

    /// <summary>
    /// AlertIn SMS 
    /// </summary>
    AlertInSMS,

    /// <summary>
    /// InfoBIP 
    /// </summary>
    InfoBip
}