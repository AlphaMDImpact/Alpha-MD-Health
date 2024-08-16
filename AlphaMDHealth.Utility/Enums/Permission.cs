namespace AlphaMDHealth.Utility;

/// <summary>
/// Device Permissions
/// </summary>
public enum Permission
{
    /// <summary>
    /// Generic Permission
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Permission for calendar
    /// </summary>
    Calendar = 1,

    /// <summary>
    /// Permission for camera
    /// </summary>
    Camera = 2,

    /// <summary>
    /// Permission for contacts
    /// </summary>
    Contacts = 3,

    /// <summary>
    /// Permission for location
    /// </summary>
    Location = 4,

    /// <summary>
    /// Permission for microphone
    /// </summary>
    Microphone = 5,

    /// <summary>
    /// Permission for phone
    /// </summary>
    Phone = 6,

    /// <summary>
    /// Permission for photos
    /// </summary>
    Photos = 7,

    /// <summary>
    /// Permission for reminders
    /// </summary>
    Reminders = 8,

    /// <summary>
    /// Permission for sensors
    /// </summary>
    Sensors = 9,

    /// <summary>
    /// Permission for SMS
    /// </summary>
    Sms = 10,

    /// <summary>
    /// Permission for storage
    /// </summary>
    Storage = 11,

    /// <summary>
    /// Permission for speech
    /// </summary>
    Speech = 12,

    /// <summary>
    /// Permission for location always
    /// </summary>
    LocationAlways = 13,

    /// <summary>
    /// Permission for location when in use
    /// </summary>
    LocationWhenInUse = 14,

    /// <summary>
    /// Permission for accessing media library
    /// </summary>
    MediaLibrary = 15,

    /// <summary>
    /// Permission for activity recognition in Android (custom implementation)
    /// </summary>
    ActivityRecognition = 101
}