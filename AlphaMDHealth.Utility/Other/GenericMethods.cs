using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace AlphaMDHealth.Utility;

/// <summary>
/// Methods which are being used across solution
/// </summary>
public static class GenericMethods
{
    public static IEssentials _essentials;
    /// <summary>
    /// Log Data in Output Window
    /// </summary>
    /// <param name="data"></param>
    public static void LogData(string data)
    {
        System.Diagnostics.Debug.WriteLine($"----------||{data}||{DateTimeOffset.Now:yyyy-MM-ddTHH:mm:ssZ}");
    }

    public static string FetchMauiIcon(string name)
    {
        return name?.ToLower().Replace(".svg", ".png");
    }
    /// <summary>
    /// is list empty or not
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="listToCheck">List to check</param>
    /// <returns>return boolean value</returns>
    public static bool IsListNotEmpty<T>(List<T> listToCheck)
    {
        return listToCheck?.Count > 0;
    }

    /// <summary>
    /// get file picker options
    /// </summary>
    /// <param name="supportedFileTypes"> supported file type as comma seperated string </param>
    /// <param name="fileOption"> FilePickerOptions</param>
    /// <param name="hasCamera"> <returns>Flag representing camera</returns></param>
    /// <param name="hasImageFormats">Flag representing Image</param>
    /// <param name="hasDocumentFormats">Flag representing Documents</param>
    public static void GetFilePickerOption(string supportedFileTypes, out FilePickerOptions fileOption, out bool hasCamera, out bool hasImageFormats, out bool hasDocumentFormats)
    {
        var supportedFormats = supportedFileTypes.Split(',');
        hasCamera = supportedFormats.Contains(Constants.CAMERA_STRING);
        hasImageFormats = supportedFormats.Any(IsImageFile);
        hasDocumentFormats = supportedFormats.Any(IsDocumentFile);
        fileOption = FilePickerOptions.All;
        if (hasCamera && hasImageFormats && hasDocumentFormats)
        {
            fileOption = FilePickerOptions.All;
        }
        else if (hasCamera && hasImageFormats)
        {
            fileOption = FilePickerOptions.Camera_Image;
        }
        else if (hasCamera && hasDocumentFormats)
        {
            fileOption = FilePickerOptions.Camera_Document;
        }
        else if (hasImageFormats && hasDocumentFormats)
        {
            fileOption = FilePickerOptions.Image_Document;
        }
        else if (hasCamera)
        {
            fileOption = FilePickerOptions.Camera;
        }
        else if (hasImageFormats)
        {
            fileOption = FilePickerOptions.Image;
        }
        else if (hasDocumentFormats)
        {
            fileOption = FilePickerOptions.Document;
        }
    }

    /// <summary>
    /// set icon based on fileType
    /// </summary>
    /// <param name="fileExtension"></param>
    /// <returns>returns icon based on filetype</returns>
    public static string SetIconBasedOnFileType(string fileExtension)
    {
        return fileExtension switch
        {
            Constants.PDF_FILE_TYPE => ImageConstants.I_PDF_ICON_PNG,
            Constants.DOC_FILE_TYPE or Constants.DOCX_FILE_TYPE => ImageConstants.I_DOC_ICON_PNG,
            Constants.XLS_FILE_TYPE or Constants.XLSX_FILE_TYPE => ImageConstants.I_EXCEL_ICON_PNG,
            Constants.SVG_FILE_TYPE or Constants.PNG_FILE_TYPE or Constants.JPEG_FILE_TYPE or Constants.JPG_FILE_TYPE => string.Empty,
            _ => ImageConstants.I_UPLOAD_ICON_PNG,
        };
    }
    /// <summary>
    /// file is image or not
    /// </summary>
    /// <param name="extension">file extension</param>
    /// <returns>Flag representing extension is for image or not</returns>
    public static bool IsImageFile(string extension)
    {
        if (extension.Contains(Constants.SVG_FILE_TYPE)
            || extension.Contains(Constants.PNG_FILE_TYPE)
            || extension.Contains(Constants.JPEG_FILE_TYPE)
            || extension.Contains(Constants.JPG_FILE_TYPE)
            || extension.Contains(Constants.JFIF_FILE_TYPE))
        {
            return true;
        };
        return false;
    }

    /// <summary>
    /// file is a document file or not
    /// </summary>
    /// <param name="extension">file extension</param>
    /// <returns>Flag representing extension is for document or not</returns>
    public static bool IsDocumentFile(string extension)
    {
        if (extension.Contains(Constants.DOC_FILE_TYPE)
            || extension.Contains(Constants.DOCX_FILE_TYPE)
            || extension.Contains(Constants.PDF_FILE_TYPE)
            || extension.Contains(Constants.XLSX_FILE_TYPE)
            || extension.Contains(Constants.XLS_FILE_TYPE))
        {
            return true;
        };
        return false;
    }

    /// <summary>
    /// Returns extension of file
    /// </summary>
    /// <param name="value">value who's extension to be checked</param>
    /// <returns></returns>
    public static string GetExtension(string value)
    {
        if(value.Contains(Constants.PDF_FILE_TYPE))
        {
            return Constants.PDF_FILE_TYPE;
        }
        else if (value.Contains(Constants.XLSX_FILE_TYPE))
        {
            return Constants.XLSX_FILE_TYPE;
        }
        else if (value.Contains(Constants.XLS_FILE_TYPE))
        {
            return Constants.XLS_FILE_TYPE;
        }
        else if (value.Contains(Constants.DOCX_FILE_TYPE))
        {
            return Constants.DOCX_FILE_TYPE;
        }
        else if (value.Contains(Constants.DOC_FILE_TYPE))
        {
            return Constants.DOC_FILE_TYPE;
        }
        else
        {
            return string.Empty;
        }
    }
    //public static string GetBackgroundColor(bool isSelected)
    //{
    //    return isSelected
    //        ? Application.Current.Resources[StyleConstants.ST_PRIMARY_APP_COLOR].ToString()
    //        : StyleConstants.TRANSPARENT_COLOR_STRING;
    //}

    //public static string GetTextColor(bool isSelected)
    //{
    //    return isSelected
    //        ? StyleConstants.GENERIC_BACKGROUND_COLOR 
    //        : StyleConstants.PRIMARY_TEXT_COLOR; //Application.Current.Resources[StyleConstants.ST_PRIMARY_TEXT_STRING].ToString();
    //}

    /// <summary>
    /// replace string with given data
    /// </summary>
    /// <param name="inputString">input string</param>
    /// <param name="strToReplace">string to replace</param>
    /// <param name="replacementStr">replacement string</param>
    /// <returns></returns>
    public static string? ReplaceString(string inputString, string strToReplace, string replacementStr)
    {
        return inputString?.Replace(strToReplace, replacementStr);
    }

    /// <summary>
    /// Check token and Extract Value
    /// </summary>
    /// <typeparam name="T">DataType of Value</typeparam>
    /// <param name="value">string value to conert</param>
    /// <returns>value in specific type</returns>
    public static T? MapValueType<T>(string value)
    {
        if (typeof(T).IsEnum)
        {
            System.Diagnostics.Debug.WriteLine($"----------|| kumkum IS Enum{(T)Enum.Parse(typeof(T), value)}||{DateTimeOffset.Now:yyyy-MM-ddTHH:mm:ssZ}");
            return (T)Enum.Parse(typeof(T), value);
        }
        else
        {
            return string.IsNullOrWhiteSpace(value) ? default : (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

    }

    /// <summary>
    /// Gets Image Based on Sepcified Path
    /// </summary>
    /// <param name="imageName"></param>
    /// <returns></returns>
    public static string GetImageWithPath(string imageName)
    {
        return ImageConstants.WEB_IMAGE_PATH + imageName;
    }

    /// <summary>
    /// Get Random Positive number
    /// </summary>
    public static long RandomPositiveNumber
    {
        get
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                int randomInt = BitConverter.ToInt32(bytes, 0);
                return Math.Max(10, Math.Abs(randomInt % 100000));
            }
        }
    }

    /// <summary>
    /// Get Random Negative number
    /// </summary>
    public static long RandomNegativeNumber
    {
        get
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                int randomInt = BitConverter.ToInt32(bytes, 0);
                return -Math.Max(10, Math.Abs(randomInt % 100000));
            }
        }
    }

    /// <summary>
    /// Generated new GUID for local id
    /// </summary>
    /// <returns>Generated GUID</returns>
    public static Guid GenerateGuid()
    {
        return Guid.NewGuid();
    }

    /// <summary>
    /// Get parameter list from string array 
    /// </summary>
    /// <param name="targetParamArray">target string array</param>
    /// <returns>Array of Parameter object</returns>
    public static object[] GetParamList(string[] targetParamArray)
    {
        object[] paramArray = new object[(targetParamArray.Length - 1) / 2];
        int paramIndex = 0;
        for (int i = 1; i < targetParamArray.Length; i += 2)
        {
            var paramType = Type.GetType(targetParamArray[i]);
            var paramValue = targetParamArray[i + 1];
            paramArray[paramIndex++] = paramType.GetTypeInfo().IsEnum
                ? Enum.Parse(paramType, paramValue)
                : Convert.ChangeType(GetValueToConvert(paramValue), paramType, CultureInfo.InvariantCulture);
        }
        return paramArray;
    }

    private static string GetValueToConvert(string paramValue)
    {
        return string.IsNullOrWhiteSpace(paramValue) || paramValue == "null" ? null : paramValue;
    }

    /// <summary>
    /// Converts machine depended UTC date time offset value to proper UTC date time format
    /// </summary>
    /// <param name="datetime">UTC Date time offset value</param>
    /// <returns>formatted UTC date time</returns>
    public static string ApplyUtcDateTimeFormatToUtcValue(DateTimeOffset datetime)
    {
        return datetime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Returns the SHA256 conversion of stream into byte array.
    /// </summary>
    /// <param name="content">Http request body in the form of stream</param>
    /// <returns>Http request body in the form of byte array</returns>
    public static byte[] ComputeHash(Stream content)
    {
        using (SHA256Managed sha256 = new SHA256Managed())
        {
            byte[] hash = null;
            if (content?.Length != 0)
            {
                hash = sha256.ComputeHash(content);
            }
            return hash;
        }
    }

    /// <summary>
    /// Creates a random string with letters from A-Z, a-z and numbers from 0-9
    /// </summary>
    /// <param name="length">The length of the result random string.</param>
    public static string RandomString(int length)
    {
        byte[] randomBytes = new byte[length]; // or any other size you need
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Gives default Date time
    /// </summary>
    /// <returns></returns>
    public static DateTimeOffset GetDefaultDateTime => new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Gives date time value offset in utc time zone
    /// </summary>
    /// <returns>date part of datetimeoffset value</returns>
    public static DateTimeOffset GetDateTimeOffsetValue(DateTimeOffset dt)
    {
        return new DateTimeOffset(dt.Year, dt.Month, dt.Day, 0, 0, 0, TimeSpan.Zero);
    }

    /// <summary>
    /// Convert datetime to universal datetime.
    /// </summary>
    public static DateTimeOffset GetUtcDateTime
    {
        get
        {
            //try
            //{
            //    return DateTimeOffset.UtcNow.AddSeconds(_essentials.GetPreferenceValue(StorageConstants.PR_UTC_TO_LOCAL_TIME_DIFF_KEY, 0));
            //}
            //catch
            //{
                
                
                return DateTimeOffset.UtcNow.AddSeconds(0);
            //}
        }
    }

    public static DateTimeOffset GetFullDateTimeFromDate(DateTimeOffset dt, IEssentials Essentials)
    {
        var currentUtcDateTime = GenericMethods.GetUtcDateTime;
        var currentLocalDateTime = Essentials.ConvertToLocalTime(currentUtcDateTime);
        return new DateTimeOffset(dt.Year, dt.Month, dt.Day, currentLocalDateTime.Hour, currentLocalDateTime.Minute, currentLocalDateTime.Second, currentLocalDateTime.Millisecond, currentLocalDateTime.Offset);
    }


    /// <summary>
    /// Check Regex pattern with value
    /// </summary>
    /// <param name="value">Value in which opattern need to check</param>
    /// <param name="pattern">Pattern for check value</param>
    /// <returns>Success or fail status</returns>
    public static bool IsRegexMatched(string value, string pattern)
    {
        try
        {
            return Regex.Match(value, pattern).Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Set Platform specific data in any type
    /// </summary>
    /// <typeparam name="T">Type of values, which will be passed in parameter</typeparam>
    /// <param name="iOS">iOS value</param>
    /// <param name="Android">android value</param>
    /// <param name="Blazor">Blazor value</param>
    /// <returns>Value of specified type</returns>
    public static T GetPlatformSpecificValue<T>(T iOS, T Android, T Blazor)
    {
        try
        {
            if (MobileConstants.IsIosPlatform)
            {
                return iOS;
            }
            else if (MobileConstants.IsAndroidPlatform)
            {
                return Android;
            }
            else
            {
                return Blazor;
            }
        }
        catch
        {
            return Blazor;
        }
    }

    /// <summary>
    /// Gets DateTime with Time set to start of day i.e. 00:00
    /// </summary>
    /// <param name="dateTime">DateTime from which time portion is to be removed</param>
    /// <returns>DateTime without time portion</returns>
    public static DateTimeOffset GetDateFromDateTime(DateTimeOffset dateTime)
    {
        return new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Offset);
    }

    /// <summary>
    /// Get DateTime BasedOn Culture
    /// </summary>
    /// <param name="inputDate">input date</param>
    /// <param name="dateTimeType">datetime type</param>
    /// <param name="dayFormatString">Day format string</param>
    /// <param name="monthFormatString">month format </param>
    /// <param name="yearFormatString">year format</param>
    /// <returns>Local date time based on format applied</returns>
    public static string GetDateTimeBasedOnCulture(DateTimeOffset? inputDate, DateTimeType dateTimeType, string dayFormatString, string monthFormatString, string yearFormatString)
    {
        if (inputDate.HasValue)
        {
            string formatString = GetDateTimeFormat(dateTimeType, dayFormatString, monthFormatString, yearFormatString);
            return GetDateTimeBasedOnFormatString(inputDate.Value, formatString);
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Get Local formatted DateTime BasedOn Culture
    /// </summary>
    /// <param name="inputDate">input date</param>
    /// <param name="dateTimeType">datetime type</param>
    /// <param name="dayFormatString">Day format string</param>
    /// <param name="monthFormatString">month format </param>
    /// <param name="yearFormatString">year format</param>
    /// <returns>Local date time based on format applied</returns>
    public static string GetLocalDateTimeBasedOnCulture(DateTimeOffset inputDate, DateTimeType dateTimeType, string dayFormatString, string monthFormatString, string yearFormatString)
    {
        string formatString = GetDateTimeFormat(dateTimeType, dayFormatString, monthFormatString, yearFormatString);
        return GetDateTimeBasedOnFormatString(GetDateTimeAccordingEssentials(inputDate), formatString);
    }

    /// <summary>
    /// Get date-time based upon input format
    /// </summary>
    /// <param name="inputDate">Input date-time</param>
    /// <param name="formatString">Date-time format</param>
    /// <returns>Date-time string in the given format</returns>
    public static string GetDateTimeBasedOnFormatString(DateTimeOffset inputDate, string formatString)
    {
        return inputDate.ToString(formatString, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Converts a string value to date value
    /// </summary>
    /// <param name="dateString">date in string format</param>
    /// <returns>Date value</returns>
    public static DateTimeOffset? ConvertStringToDateFormat(string dateString)
    {
        string format = Constants.EXCEL_UPLOAD_DATE_TIME_STRING_FORMAT;
        DateTime dateTime;
        if (DateTime.TryParseExact(dateString.Trim(), format, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out dateTime))
        {
            return dateTime;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Convert date time ticks string into date time offset
    /// </summary>
    /// <param name="dateTimeOffset">date time ticks in string</param>
    /// <returns>converted date time offset value</returns>
    public static string ConvertToIsoDatetimeOffset(DateTimeOffset? dateTimeOffset)
    {
        if (!dateTimeOffset.HasValue)
        {
            return string.Empty;
        }
        string isoDateString = dateTimeOffset.Value.ToString(Constants.ISO_DATE_STRING_FORMAT, CultureInfo.InvariantCulture);
        return isoDateString;
    }

    /// <summary>
    /// Convert date time ticks string into date time offset
    /// </summary>
    /// <param name="dateTimeOffset">date time ticks in string</param>
    /// <returns>converted date time offset value</returns>
    public static string ConvertDatetimeOffsetToIsoDateTimeString(DateTimeOffset? dateTimeOffset)
    {
        if (!dateTimeOffset.HasValue)
        {
            return string.Empty;
        }
        string isoDateString = dateTimeOffset.Value.ToUniversalTime().ToString(Constants.DATE_TIME_STRING_FORMAT, CultureInfo.InvariantCulture);
        return isoDateString;
    }

    /// <summary>
    /// Convert date time ticks string into date time offset
    /// </summary>
    /// <param name="isoDateString">date time ticks in string</param>
    /// <returns>converted date time offset value</returns>
    public static DateTimeOffset? ConvertIsoDateStringToDateTimeOffset(string isoDateString)
    {
        if (string.IsNullOrWhiteSpace(isoDateString))
        {
            return null;
        }
        DateTimeOffset dateTimeOffset;
        if (DateTimeOffset.TryParseExact(isoDateString.Trim(), Constants.ISO_DATE_STRING_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeOffset))
        {
            return dateTimeOffset;
        }
        else
        {
            return null;
        }
    }

    ///// <summary>
    ///// Convert date time ticks string into date time offset
    ///// </summary>
    ///// <param name="currentValueInTicks">date time ticks in string</param>
    ///// <returns>converted date time offset value</returns>
    //public static DateTimeOffset? ConvertTicksStringToDateTimeOffset(string currentValueInTicks) //ConvertTicksStringToDateTimeOffset
    //{
    //    if (string.IsNullOrWhiteSpace(currentValueInTicks))
    //    {
    //        return null;
    //    }
    //    long currentDateTicks = Convert.ToInt64(currentValueInTicks, CultureInfo.InvariantCulture);
    //    return new DateTimeOffset(new DateTime(currentDateTicks, DateTimeKind.Local));
    //}

    /// <summary>
    /// Get DateTime Format based upon Culture
    /// </summary>
    /// <param name="dateTimeType">Date or Time Or Datetime</param>
    /// <param name="dayFormatString">Day format string</param>
    /// <param name="monthformatString">Month format string</param>
    /// <param name="yearFormatString">Year format string</param>
    /// <returns>DateTime format</returns>
    public static string GetDateTimeFormat(DateTimeType dateTimeType, string dayFormatString, string monthformatString, string yearFormatString)
    {
        //var timeString = CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern;
        var dateFormatstring = (dateTimeType == DateTimeType.MonthYear
            ? CultureInfo.CurrentUICulture.DateTimeFormat.YearMonthPattern
            : GetDatePattern(dateTimeType, dayFormatString))
        .Replace(GetFormatSubString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern, 'M'), monthformatString)
        .Replace(GetFormatSubString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern, 'y'), yearFormatString);

        if (monthformatString?.Length > 2)
        {
            dateFormatstring = dateFormatstring.Replace(CultureInfo.CurrentUICulture.DateTimeFormat.DateSeparator, " ");
        }
        return ApplyDateTimeFormat(dateTimeType, monthformatString, dateFormatstring);
    }

    private static string ApplyDateTimeFormat(DateTimeType dateTimeType, string monthformatString, string dateFormatstring)
    {
        string returnString;
        switch (dateTimeType)
        {
            case DateTimeType.Date:
            case DateTimeType.MonthYear:
            case DateTimeType.ExtendedDate:
                returnString = dateFormatstring;
                break;
            case DateTimeType.DayMonth:
                returnString = CultureInfo.CurrentUICulture.DateTimeFormat.MonthDayPattern
                                .Replace(GetFormatSubString(CultureInfo.CurrentUICulture.DateTimeFormat.MonthDayPattern, 'M'), monthformatString);
                break;
            case DateTimeType.Day:
                returnString = "ddd";
                break;
            case DateTimeType.Hour:
                returnString = "HH";
                break;
            case DateTimeType.Time:
                returnString = GetTimeFormat();
                break;
            default:
                returnString = dateFormatstring + " " + GetTimeFormat();
                break;
        }
        return returnString;
    }

    /// <summary>
    /// Time format based on systems 12hour or 24hour format setting value
    /// </summary>
    /// <returns>time format</returns>
    public static string GetTimeFormat()
    {
        return _essentials?.GetPreferenceValue(StorageConstants.PR_IS_24_HOUR_FORMAT, false) ?? false
            ? Constants.DEFAULT_TIME_FORMAT
            : Constants.TIME_FORMAT;
    }

    private static string GetDatePattern(DateTimeType dateTimeType, string dayFormatString)
    {
        if (dateTimeType == DateTimeType.ExtendedDateTime || dateTimeType == DateTimeType.ExtendedDate)
        {
            string datePattern = CultureInfo.CurrentUICulture.DateTimeFormat.LongDatePattern;
            string replacePart = string.Empty;
            if (datePattern.Contains(Constants.DEFAULT_EXTENDED_DAY_FORMAT))
            {
                replacePart = Constants.DEFAULT_DAY_FORMAT;
            }
            else
            {
                if (datePattern.Contains(Constants.DEFAULT_DATE_FORMAT))
                {
                    replacePart = Constants.DEFAULT_DATE_FORMAT;
                }
            }
            if (!string.IsNullOrEmpty(replacePart))
            {
                datePattern = datePattern.Replace(replacePart, dayFormatString);
            }
            return datePattern;
        }
        return CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern;
    }

    public static void SortByDate<T>(List<T> dataList, Func<T, DateTimeOffset?> getDate, Func<T, DateTimeOffset?> getToDate)
    {
        if (GenericMethods.IsListNotEmpty(dataList))
        {
            List<T> todaysList = new List<T>();
            List<T> futureList = new List<T>();
            List<T> pastList = new List<T>();

            DateTime today = DateTime.Today;

            foreach (var dataModel in dataList)
            {
                DateTimeOffset? fromDate = getDate(dataModel);
                DateTimeOffset? toDate = getToDate(dataModel);

                if (fromDate.Value.Date <= today.Date && toDate.Value.Date >= today.Date)
                {
                    todaysList.Add(dataModel);
                }
                else if (fromDate.Value.Date > today.Date)
                {
                    futureList.Add(dataModel);
                }
                else if (toDate.Value.Date < today.Date)
                {
                    pastList.Add(dataModel);
                }
            }

            todaysList = todaysList
              .OrderBy(x => getDate(x) == getToDate(x) ? 0 : 1)
              .ThenBy(x => getDate(x))
              .ThenBy(x => getToDate(x))
              .ToList();

            futureList = futureList
                .OrderBy(x => getDate(x))
                .ThenBy(x => getToDate(x))
                .ToList();

            pastList = pastList
                .OrderByDescending(x => getDate(x))
                .ThenBy(x => getToDate(x))
                .ToList();

            dataList.Clear();
            dataList.AddRange(todaysList);
            dataList.AddRange(futureList);
            dataList.AddRange(pastList);
        }
    }

    /// <summary>
    /// Generates paramString with placeholder
    /// </summary>
    /// <param name="paramName">list of parameters</param>
    /// <returns>paramString</returns>
    public static string GenerateParamsWithPlaceholder(params Param[] paramName)
    {
        StringBuilder paramString = new StringBuilder();
        paramString.Append(Constants.SYMBOL_QUESTION_MARK);
        if (paramName?.Length > 0)
        {
            for (int i = 0; i < paramName.Length; i++)
            {
                paramString.Append(paramName[i].ToString() + Constants.SYMBOL_EQUAL + "{" + i + "}");
                if (i != paramName.Length - 1)
                {
                    paramString.Append(Constants.SYMBOL_AMPERSAND);
                }
            }
        }
        return paramString.ToString();
    }

    /// <summary>
    /// Returns MemoryStream from ByteArray of Image
    /// </summary>
    /// <param name="image">ByteArray of encoded Image</param>
    /// <returns>Memory stream to use as a source to image</returns>
    public static MemoryStream GetMemoryStreamFromByteArray(byte[] image)
    {
        try
        {
            return new MemoryStream(image);
        }
        catch
        {
            return new MemoryStream();
        }
    }

    /// <summary>
    /// Returns MemoryStream from base 64 string of Image
    /// </summary>
    /// <param name="base64String">Base64 string of encoded Image</param>
    /// <returns>Memory stream to use as a source to image</returns>
    public static MemoryStream GetMemoryStreamFromBase64(string base64String)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(base64String))
            {
                MemoryStream memoryStream;
                if (base64String.Contains(","))
                {
                    memoryStream = new MemoryStream(Convert.FromBase64String(base64String.Split(',')[1]));
                }
                else if (base64String.Contains(";"))
                {
                    memoryStream = new MemoryStream(Convert.FromBase64String(base64String.Split(';')[1]));
                }
                else
                {
                    memoryStream = new MemoryStream(Convert.FromBase64String(base64String));
                }
                return _ = memoryStream;
            }
            return new MemoryStream();
        }
        catch
        {
            return new MemoryStream();
        }
    }

    /// <summary>
    /// Get one link parameters
    /// </summary>
    /// <param name="oneLink">string to get one link parameter</param>
    /// <returns>one link parameters</returns>
    public static string[] GetOneLinkParameters(string oneLink)
    {
        return oneLink?.TrimEnd('&').Split(new[] { Constants.ONELINK_SEPERATOR_KEY }, StringSplitOptions.None);
    }

    /// <summary>
    /// Returns the installed certificate on the machine based on the thumbprint)
    /// </summary>
    /// <param name="thumbprint">the certificate thumb print</param>
    /// <returns>return the certificate data</returns>
    public static X509Certificate2 GetCertificate(string thumbprint)
    {
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            var bytes1 = File.ReadAllBytes($"/var/ssl/private/{thumbprint}.p12");
            var privatecert = new X509Certificate2(bytes1);
            if (privatecert == null)
            {
                throw new Exception("Certificate is not installed in Linux machine");
            }
            return privatecert;
        }
        else
        {
            using (var store = new X509Store(StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                var certificateCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (certificateCollection.Count == 0)
                {
                    using (var store1 = new X509Store(StoreLocation.CurrentUser))
                    {
                        store1.Open(OpenFlags.ReadOnly);
                        certificateCollection = store1.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                        if (certificateCollection.Count == 0)
                        {
                            throw new Exception("Certificate is not installed in windows machine");
                        }
                        store1.Close();
                    }
                }
                store.Close();
                return certificateCollection[0];
            }
        }
    }

    /// <summary>
    /// Assign a ErroCode Based upon Operation Type
    /// </summary>
    /// <param name="operationType">Enum Operation Type</param>
    /// <returns>ErrorCode</returns>
    public static ErrorCode AssignErrorCode(OperationType operationType)
    {
        switch (operationType)
        {
            case OperationType.Retirieve:
                return ErrorCode.ErrorWhileRetrievingRecords;
            case OperationType.Save:
                return ErrorCode.ErrorWhileSavingRecords;
            case OperationType.Delete:
                return ErrorCode.ErrorWhileDeletingRecords;
            default:
                return ErrorCode.InternalServerError;
        }
    }

    /// <summary>
    /// Mapping ErroCode
    /// </summary>
    /// <param name="errorCode">errCode Integter</param>
    /// <param name="operationType">operation TYpe</param>
    /// <returns>ErrorCode</returns>
    public static ErrorCode MapDBErrorCodes(int errorCode, OperationType operationType)
    {
        return errorCode == 999 ? AssignErrorCode(operationType) : (ErrorCode)errorCode;
    }

    /// <summary>
    /// Generates user full name
    /// </summary>
    /// <param name="userFirstName"> user first name</param>
    /// <param name="userLastName">user last name</param>
    /// <returns>user full name</returns>
    public static string GenerateUserFullName(string userFirstName, string userLastName)
    {
        return $"{userFirstName} {userLastName}";
    }

    private static string GetFormatSubString(string inputFormat, char toFind)
    {
        int charCount = inputFormat.Count(x => x == toFind);
        int getIndex = inputFormat.IndexOf(toFind);
        return inputFormat.Substring(getIndex, charCount);
    }

    /// <summary>
    /// GetResolution
    /// </summary>
    /// <param name="actualHeight">image actual height</param>
    /// <param name="actualWidth">image actual width</param>
    /// <param name="maxHeight">image max height</param>
    /// <param name="maxWidth">image max width</param>
    /// <returns></returns>
    public static Tuple<int, int> GetResolution(int actualHeight, int actualWidth, float maxHeight, float maxWidth)
    {
        float imgRatio = actualWidth / (float)actualHeight;
        float maxRatio = maxWidth / maxHeight;
        if (actualHeight > maxHeight || actualWidth > maxWidth)
        {
            if (imgRatio < maxRatio)
            {
                imgRatio = maxHeight / actualHeight;
                actualWidth = (int)(imgRatio * actualWidth);
                actualHeight = (int)maxHeight;
            }
            else if (imgRatio > maxRatio)
            {
                imgRatio = maxWidth / actualWidth;
                actualHeight = (int)(imgRatio * actualHeight);
                actualWidth = (int)maxWidth;
            }
            else
            {
                actualHeight = (int)maxHeight;
                actualWidth = (int)maxWidth;
            }
        }
        return new Tuple<int, int>(actualHeight, actualWidth);
    }

    /// <summary>
    /// get image tag prefix base upon the file extention
    /// </summary>
    /// <param name="ext">file extention</param>
    /// <returns>image prefix tag</returns>
    public static string GetImagePrefix(string ext)
    {
        const string base64Tag = ";base64,";
        const string applicationTag = "application/";
        switch (ext)
        {
            case ".png":
                return ImageConstants.IMAGE_BASE64_PNG_PREFIX;
            case ".jpg":
                return ImageConstants.IMAGE_BASE64_JPG_PREFIX;
            case ".jpeg":
                return ImageConstants.IMAGE_BASE64_JPEG_PREFIX;
            case ".svg":
                return ImageConstants.IMAGE_BASE64_SVG_PREFIX;
            case ".pdf":
                return string.Concat(applicationTag, ImageConstants.PDF_TAG, base64Tag);
            case ".doc":
                return string.Concat(applicationTag, ImageConstants.DOC_TAG, base64Tag);
            case ".docx":
                return string.Concat(applicationTag, ImageConstants.DOCX_TAG, base64Tag);
            case ".xls":
                return string.Concat(applicationTag, ImageConstants.XLS_TAG, base64Tag);
            case ".xlsx":
                return string.Concat(applicationTag, ImageConstants.XLSX_TAG, base64Tag);
            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// Calculates BMI based on height and weight
    /// </summary>
    /// <param name="weight">users weight</param>
    /// <param name="weightUnit"></param>
    /// <param name="height">users height</param>
    /// <param name="heightUnit"></param>
    /// <returns>BMI value in kg/cm2</returns>
    public static double CalculateBMIValue(double weight, string weightUnit, double height, string heightUnit)
    {
        double heightInM;
        switch (heightUnit)
        {
            case "ft":
                heightInM = 30.48 * height / 100;
                break;
            case "in":
                heightInM = (2.54 * height) / 100;
                break;
            case "m":
                heightInM = height;
                break;
            default:
                heightInM = height / 100;
                break;
        }
        double weightInKg;
        weightInKg = weightUnit == "lbs" ? Math.Round(703 * weight / Math.Pow(height * 0.39, 2), Constants.DIGITS_AFTER_DECIMAL) : weight;
        return Math.Round(weightInKg / Math.Pow(heightInM, 2), Constants.DIGITS_AFTER_DECIMAL);
    }

    /// <summary>
    /// Method to Convert a pipe separated string a dictionary
    /// </summary>
    /// <param name="stringValue">pipe seperated string of values</param>
    /// <returns></returns>
    public static Dictionary<long, string> ConvertStringToDictionary(string stringValue)
    {
        return stringValue?.Split(Constants.SYMBOL_PIPE_SEPERATOR).Select(p => p.Split(Constants.SYMBOL_DASH))
            .ToDictionary(sp => Convert.ToInt64(sp[0], CultureInfo.InvariantCulture), sp => sp[1])
            .Where(d => !string.IsNullOrWhiteSpace(d.Value))?.ToDictionary(x => Convert.ToInt64(x.Key), x => x.Value);
    }

    /// <summary>
    /// Map date time value
    /// </summary>
    /// <param name="observationType">reading type</param>
    /// <param name="observationDateTime">reading date </param>
    /// <param name="mergeValueOfEntireDay">merge value of entire day</param>
    /// <param name="adjustDate">should adjust date time</param>
    /// <returns>Date time based on observation type</returns>
    public static DateTimeOffset MapDateTimeValue(ReadingType observationType, DateTimeOffset observationDateTime, string mergeValueOfEntireDay, bool adjustDate)
    {
        DateTimeOffset localDateTime = GetDateTimeAccordingEssentials(observationDateTime);
        if (!string.IsNullOrWhiteSpace(mergeValueOfEntireDay) && Convert.ToBoolean(mergeValueOfEntireDay, CultureInfo.InvariantCulture))
        {
            return new DateTimeOffset(localDateTime.Year, localDateTime.Month, localDateTime.Day, 0, 0, 0, localDateTime.Offset).ToUniversalTime();
        }
        else if (observationType == ReadingType.HeartRate && adjustDate)
        {
            return new DateTimeOffset(localDateTime.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, 0, 0, localDateTime.Offset).ToUniversalTime();
        }
        else
        {
            return new DateTimeOffset(localDateTime.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, localDateTime.Minute, 0, localDateTime.Offset).ToUniversalTime();
        }
    }

    /// <summary>
    /// Convert to culture specific number
    /// </summary>
    /// <param name="metricValue">value to be converted</param>
    /// <param name="culture">culture</param>
    /// <returns>culture specifc string</returns>
    public static string ConvertToLocalNumber(double metricValue, CultureInfo culture)
    {
        try
        {
            return metricValue.ToString(culture.NumberFormat);
        }
        catch
        {
            return metricValue.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// returns the value eparator based on observation type and current culture
    /// </summary>
    /// <param name="observationType">type of observation</param>
    /// <param name="currentCulture">user's current culture</param>
    /// <returns>value separator "." "/" "," etc</returns>
    public static string GetValueSeparator(ReadingType observationType, CultureInfo currentCulture)
    {
        if (observationType == ReadingType.BloodPressure)
        {
            return Constants.SYMBOL_SLASH.ToString(CultureInfo.InvariantCulture);
        }
        else if (observationType == ReadingType.Sleep)
        {
            return currentCulture.DateTimeFormat.TimeSeparator;
        }
        else
        {
            return currentCulture.NumberFormat.NumberDecimalSeparator;
        }
    }

    /// <summary>
    /// Converts the given duration in hours to hours and minutes in the given string format
    /// </summary>
    /// <param name="value">Hour value</param>
    /// <param name="format">string format in which hour and minutes are to be represented</param>
    /// <returns>string representing hours and minutes for the given hour value</returns>
    public static string ConvertValueToHoursMinutes(double? value, string format)
    {
        TimeSpan duration = TimeSpan.FromHours(value ?? 0);
        if (string.IsNullOrWhiteSpace(format))
        {
            return format;
        }
        return string.Format(format, (int)duration.TotalHours, duration.Minutes, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts given hour and minute string to hour duration
    /// </summary>
    /// <param name="hours">string representing hour value</param>
    /// <param name="minutes">string representing minute value</param>
    /// <returns>hour value based on the given hour and minutes</returns>
    public static double ConvertHoursMinutesToHours(string hours, string minutes)
    {
        if (string.IsNullOrWhiteSpace(hours))
        {
            hours = Constants.NUMBER_ZERO;
        }
        if (string.IsNullOrWhiteSpace(minutes))
        {
            minutes = Constants.NUMBER_ZERO;
        }
        return (TimeSpan.FromHours(Convert.ToInt32(hours, CultureInfo.InvariantCulture)) + TimeSpan.FromMinutes(Convert.ToInt32(minutes, CultureInfo.InvariantCulture))).TotalHours;
    }

    /// <summary>
    /// Generate list of string from a set of numbers
    /// </summary>
    /// <param name="startValue">The start number</param>
    /// <param name="endValue">The last number</param>
    /// <param name="incrementValue">The increment value</param>
    /// <param name="digitFormat">digital format</param>
    /// <returns></returns>
    public static List<string> GenerateStringList(double startValue, double endValue, double incrementValue, string digitFormat)
    {
        List<string> returnList = new List<string>();
        while (startValue <= endValue)
        {
            returnList.Add(string.IsNullOrWhiteSpace(digitFormat) ? startValue.ToString(CultureInfo.InvariantCulture) : startValue.ToString(digitFormat, CultureInfo.InvariantCulture));
            startValue += incrementValue;
        }
        return returnList;
    }

    /// <summary>
    /// returns a list AM, PM strings based on current culture
    /// </summary>
    /// <param name="currentCulture">user's current culture</param>
    /// <returns>list having culture specific AM and PM strings</returns>
    public static List<string> GetMeridiemDesignators(CultureInfo currentCulture)
    {
        return new List<string> { currentCulture.DateTimeFormat.AMDesignator, currentCulture.DateTimeFormat.PMDesignator };
    }

    /// <summary>
    /// Returns number of days.
    /// If observation date is very old then add noOfDays before and after that observationDate.
    /// If observation date is near to current date then add noOfDays before and till current date.
    /// </summary>
    /// <param name="noOfDays">No of days we need to go backwords</param>
    /// <param name="cultureInfo">User current culture</param>
    /// <param name="obsDate">Observation Date. Used to add dates before and after observation added date.</param>
    /// <returns>List of Date in string format.</returns>
    public static List<string> GetDateList(int noOfDays, CultureInfo cultureInfo, DateTimeOffset obsDate)
    {
        const string dateFormat = "{0:MMM dd}";
        List<string> days = new List<string>();
        DateTimeOffset startDate = obsDate.AddDays(-noOfDays);
        DateTimeOffset endDate = obsDate.AddDays(noOfDays);
        if (endDate > DateTime.Now)
        {
            int endDateTest = (DateTime.Now - obsDate).Days;
            endDate = obsDate.AddDays(endDateTest);
        }
        int diffDate = (endDate - startDate).Days;
        string cultureDate;
        for (int i = 1; i <= diffDate; i++)
        {
            cultureDate = string.Format(cultureInfo, dateFormat, startDate.AddDays(i));
            days.Add(CultureInfo.CurrentCulture.TextInfo.ToUpper(cultureDate.Substring(0, 1)) + cultureDate.Substring(1, cultureDate.Length - 1));
        }
        days.Reverse();
        return days;
    }

    /// <summary>
    /// Get date list
    /// </summary>
    /// <param name="noOfDays"></param>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public static List<string> GetDateList(int noOfDays, CultureInfo cultureInfo)
    {
        const string dateFormat = "{0:MMM dd}";
        List<string> days = new List<string>();
        string cultureDate;
        for (int i = 0; i < noOfDays; i++)
        {
            cultureDate = string.Format(cultureInfo, dateFormat, DateTime.Now.AddDays(-i));
            days.Add(CultureInfo.CurrentCulture.TextInfo.ToUpper(cultureDate.Substring(0, 1)) + cultureDate.Substring(1, cultureDate.Length - 1));
        }
        return days;
    }

    /// <summary>
    /// Checks if given file extension belongs to supported file extensions
    /// </summary>
    /// <param name="supportedFileTypes">Comma separated string of supported extensions</param>
    /// <param name="selectedFileExtension">extension of the selected file</param>
    /// <returns>true if given file extension is supported</returns>
    public static bool IsExtensionSupported(string supportedFileTypes, string selectedFileExtension)
    {
        return !string.IsNullOrWhiteSpace(supportedFileTypes)
            && !string.IsNullOrWhiteSpace(selectedFileExtension)
            && supportedFileTypes.Split(Constants.SYMBOL_COMMA).Any(x => x.Trim().ToLowerInvariant() == selectedFileExtension.ToLowerInvariant());
    }

    /// <summary>
    /// Gets the start end date based on the given input duration
    /// </summary>
    /// <param name="duration">number of days</param>
    /// <param name="currentDate">current date</param>
    /// <param name="startDate">start date output</param>
    /// <param name="endDate">end date output</param>
    /// <returns>Calculated start and end date</returns>
    public static bool TryCalculateStartEndDate(int duration, DateTimeOffset currentDate, out DateTimeOffset? startDate, out DateTimeOffset? endDate)
    {
        currentDate = GetDateTimeAccordingEssentials(currentDate);
        if (duration == -1)
        {
            endDate = GetStartEndOfDay(false, currentDate.ToUniversalTime());
            startDate = GetDefaultDateTime;
        }
        else if (duration < 2)
        {
            endDate = GetStartEndOfDay(false, currentDate);
            startDate = GetStartEndOfDay(true, currentDate);
        }
        else if (duration < 8)
        {
            startDate = GetStartEndOfDay(true, currentDate.AddDays(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - currentDate.DayOfWeek));
            endDate = GetStartEndOfDay(false, GetDateTimeAccordingEssentials(startDate).AddDays(6));
        }
        else if (duration < 32)
        {
            startDate = new DateTimeOffset(currentDate.Year, currentDate.Month, 1, 0, 0, 0, 0, currentDate.Offset).ToUniversalTime();
            endDate = GetStartEndOfDay(false, GetDateTimeAccordingEssentials(startDate).AddMonths(1).AddDays(-1));
        }
        else if (duration < 93)
        {
            startDate = new DateTimeOffset(currentDate.Year, currentDate.Month, 1, 0, 0, 0, 0, currentDate.Offset).AddMonths(-2).ToUniversalTime();
            endDate = GetStartEndOfDay(false, GetDateTimeAccordingEssentials(startDate).AddMonths(3).AddDays(-1));
        }
        //// condition for (duration < 366)
        else
        {
            endDate = GetStartEndOfDay(false, new DateTimeOffset(currentDate.Year, 12, 31, 0, 0, 0, 0, currentDate.Offset));
            startDate = GetStartEndOfDay(true, GetDateTimeAccordingEssentials(endDate).AddMonths(-11).AddDays(-30).AddYears(-(int)Math.Floor(duration / 366.0)));
        }
        return true;
    }

    /// <summary>
    /// Get Start or End of input Date
    /// </summary>
    /// <param name="shouldFetchStart">should get start of Day</param>
    /// <param name="dateTime">input Datetime</param>
    /// <returns>start or end of Day</returns>
    public static DateTimeOffset GetStartEndOfDay(bool shouldFetchStart, DateTimeOffset dateTime)
    {
        DateTimeOffset localDateTime = GetDateTimeAccordingEssentials(dateTime);
        localDateTime = new DateTimeOffset(localDateTime.Year, localDateTime.Month, localDateTime.Day, 0, 0, 0, localDateTime.Offset);
        return shouldFetchStart
            ? localDateTime.ToUniversalTime()
            : localDateTime.AddDays(1).AddTicks(-1).ToUniversalTime();
    }

    /// <summary>
    /// Get Image Base64 from SVGpath
    /// </summary>
    /// <param name="svgPath">SVG path of image</param>
    /// <param name="assembly">Assembly of the Used Application</param>
    /// <returns>Return Base64 of the SVG Image</returns>
    public static async Task<string> SvgToBase64StringAsync(string svgPath, Assembly assembly)
    {
        using (Stream stream = assembly.GetManifestResourceStream(svgPath))
        {
            long length = stream.Length;
            byte[] buffer = new byte[length];
            await stream.ReadAsync(buffer, 0, (int)length).ConfigureAwait(true);
            return Constants.SVG_BASE64_PREFIX + Convert.ToBase64String(buffer);
        }
    }

    /// <summary>
    /// Creates base 64 string from received stream
    /// </summary>
    /// <param name="fileName">name of the file</param>
    /// <param name="content">stream content to convert</param>
    /// <returns>files base 64 string</returns>
    public static async Task<string> CreateFileBase64StringAsync(string fileName, Stream content)
    {
        MemoryStream memStream = new MemoryStream();
        await content.CopyToAsync(memStream).ConfigureAwait(false);
        string fileBase64 = Convert.ToBase64String(memStream.ToArray());
        return string.IsNullOrWhiteSpace(fileBase64)
            ? string.Empty
            : GetBase64MetaDataPrefix(fileName) + fileBase64;
    }

    /// <summary>
    /// fetch extension from file name
    /// </summary>
    /// <param name="fileValue">full name of file</param>
    /// <returns>file extension</returns>
    public static string GetFileExtensionFromName(string fileValue)
    {
        return fileValue?.Split(Constants.DOT_SEPARATOR).Last().Split(Constants.SYMBOL_QUESTIONMARK).First() ?? string.Empty;
    }

    /// <summary>
    /// Get Base64 tag based upon file extension
    /// </summary>
    /// <param name="fileNameWithExtension">filename with extension</param>
    /// <returns>image tag</returns>
    public static string GetBase64MetaDataPrefix(string fileNameWithExtension)
    {
        AppFileExtensions fileExtension = GetFileExtensionFromName(fileNameWithExtension).ToEnum<AppFileExtensions>(true);
        switch (fileExtension)
        {
            case AppFileExtensions.pdf:
                return string.Format(CultureInfo.InvariantCulture, Constants.BASE64_METADATA_FORMAT, Constants.BASE64_APPLICATION_CONTENT, fileExtension.ToString());
            case AppFileExtensions.docx:
                return string.Format(CultureInfo.InvariantCulture, Constants.BASE64_METADATA_FORMAT, Constants.BASE64_APPLICATION_CONTENT, Constants.DOCX_TAG);
            case AppFileExtensions.doc:
                return string.Format(CultureInfo.InvariantCulture, Constants.BASE64_METADATA_FORMAT, Constants.BASE64_APPLICATION_CONTENT, Constants.DOC_TAG);
            case AppFileExtensions.xlsx:
                return string.Format(CultureInfo.InvariantCulture, Constants.BASE64_METADATA_FORMAT, Constants.BASE64_APPLICATION_CONTENT, Constants.XLSX_TAG);
            case AppFileExtensions.xls:
                return string.Format(CultureInfo.InvariantCulture, Constants.BASE64_METADATA_FORMAT, Constants.BASE64_APPLICATION_CONTENT, Constants.XLS_TAG);

            case AppFileExtensions.svg:
                return string.Format(CultureInfo.InvariantCulture, Constants.BASE64_METADATA_FORMAT, Constants.BASE64_IMAGE_CONTENT, Constants.SVG_PLUS_XML_FILE_TYPE);

            case AppFileExtensions.ico:
                return string.Format(CultureInfo.InvariantCulture, Constants.BASE64_METADATA_FORMAT, Constants.BASE64_IMAGE_CONTENT, Constants.X_ICON_FILE_TYPE);

            case AppFileExtensions.jpg:
                return string.Format(CultureInfo.InvariantCulture, Constants.BASE64_METADATA_FORMAT, Constants.BASE64_IMAGE_CONTENT, Constants.JPG_FILE_TYPE);
            case AppFileExtensions.jpeg:
                return string.Format(CultureInfo.InvariantCulture, Constants.BASE64_METADATA_FORMAT, Constants.BASE64_IMAGE_CONTENT, Constants.JPEG_FILE_TYPE);
            case AppFileExtensions.png:
            case AppFileExtensions.gif:
            case AppFileExtensions.jfif:
            default:
                return string.Format(CultureInfo.InvariantCulture, Constants.BASE64_METADATA_FORMAT, Constants.BASE64_IMAGE_CONTENT, fileExtension.ToString());

        }
    }

    /// <summary>
    /// Get File extension based on tag
    /// </summary>
    /// <param name="fileValue">base 64 file string</param>
    /// <returns>image extension</returns>
    public static string GetFileExtension(string fileValue)
    {
        if (fileValue.IndexOf(Constants.IMAGE_TAG_JPG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.JPG_FILE_TYPE;
        }
        else if (fileValue.IndexOf(Constants.IMAGE_TAG_JPEG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.JPEG_FILE_TYPE;
        }
        else if (fileValue.IndexOf(Constants.IMAGE_TAG_PNG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.PNG_FILE_TYPE;
        }
        else if (fileValue.IndexOf(Constants.IMAGE_TAG_ICO, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.ICO_FILE_TYPE;
        }
        else if (fileValue.IndexOf(Constants.IMAGE_TAG_SVG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.SVG_FILE_TYPE;
        }
        else if (fileValue.IndexOf(Constants.IMAGE_TAG_JFIF, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.JFIF_FILE_TYPE;
        }
        else if (fileValue.IndexOf(Constants.PDF_TAG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.PDF_FILE_TYPE;
        }
        else if (fileValue.IndexOf(Constants.DOCX_TAG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.DOCX_FILE_TYPE;
        }
        else if (fileValue.IndexOf(Constants.DOC_TAG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.DOC_FILE_TYPE;
        }
        else if (fileValue.IndexOf(Constants.XLSX_TAG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.XLSX_FILE_TYPE;
        }
        else if (fileValue.IndexOf(Constants.XLS_TAG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.XLS_FILE_TYPE;
        }
        else
        {
            return string.Empty;
        }
    }


    /// <summary>
    /// Get type of content
    /// </summary>
    /// <param name="imageValue">image Value</param>
    /// <returns>image content type</returns>
    public static string GetFileContentType(string imageValue)
    {
        if (imageValue.IndexOf(Constants.IMAGE_TAG_JPG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.IMAGE_TAG_JPG;
        }
        else if (imageValue.IndexOf(Constants.IMAGE_TAG_JPEG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.IMAGE_TAG_JPEG;
        }
        else if (imageValue.IndexOf(Constants.IMAGE_TAG_PNG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.IMAGE_TAG_PNG;
        }
        else if (imageValue.IndexOf(Constants.IMAGE_TAG_ICO, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.IMAGE_TAG_ICO;
        }
        else if (imageValue.IndexOf(Constants.IMAGE_TAG_SVG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.IMAGE_TAG_SVG;
        }
        else if (imageValue.IndexOf(Constants.PDF_TAG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.PDF_TAG;
        }
        else if (imageValue.IndexOf(Constants.DOCX_TAG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return string.Concat(Constants.BASE64_APPLICATION_CONTENT, Constants.BACK_SLASH, Constants.DOCX_TAG);
        }
        else if (imageValue.IndexOf(Constants.DOC_TAG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.DOC_TAG;
        }
        else if (imageValue.IndexOf(Constants.XLSX_TAG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return string.Concat(Constants.BASE64_APPLICATION_CONTENT, Constants.BACK_SLASH, Constants.XLSX_TAG);
        }
        else if (imageValue.IndexOf(Constants.XLS_TAG, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return string.Concat(Constants.BASE64_APPLICATION_CONTENT, Constants.BACK_SLASH, Constants.XLS_TAG);
        }
        else if (imageValue.IndexOf(Constants.SVG_FILE_TYPE, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.IMAGE_TAG_SVG;
        }
        else if (imageValue.IndexOf(Constants.JFIF_FILE_TYPE, StringComparison.InvariantCultureIgnoreCase) > -1)
        {
            return Constants.JFIF_FILE_TYPE;
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Generate a Guid Filename
    /// </summary>
    /// <param name="fileExt">file extension</param>
    /// <returns>guid file name with extension</returns>
    public static string GenerateGuidWithFileExt(string fileExt)
    {
        fileExt = "." + fileExt;
        return string.Format(CultureInfo.InvariantCulture, "{0}{1:10}{2}", Guid.NewGuid().ToString("n", CultureInfo.InvariantCulture), DateTime.UtcNow.Ticks, fileExt);
    }

    /// <summary>
    /// Checks value is path or not
    /// </summary>
    /// <param name="value">string value to check</param>
    /// <returns>Flag representing string is path</returns>
    public static bool IsPathString(string value)
    {
        return value.StartsWith("https://") || value.StartsWith("http://");
    }

    /// <summary>
    /// Get Range Label for Mobile and Wen
    /// </summary>
    /// <param name="dayFormat">input Date Format</param>
    /// <param name="monthFormat">input month Format</param>
    /// <param name="yearFormat">year input Year format</param>
    /// <param name="startDate">start Date</param>
    /// <param name="endDate">end Date</param>
    /// <returns>Date String</returns>
    public static string GetRangeLabel(string dayFormat, string monthFormat, string yearFormat, DateTimeOffset startDate, DateTimeOffset endDate)
    {
        int days = (endDate - startDate).Days;
        if (days >= 27)
        {
            if (days < 32)
            {
                if (startDate.LocalDateTime.Month == endDate.LocalDateTime.Month && startDate.LocalDateTime.Year == endDate.LocalDateTime.Year)
                {
                    return GetLocalDateTimeBasedOnCulture(startDate, DateTimeType.MonthYear, dayFormat, monthFormat, yearFormat);
                }
            }
            else
            {
                if (IsWithinYearRange(ref startDate, ref endDate, days))
                {
                    return GetDateTimeBasedOnFormatString(GetDateTimeAccordingEssentials(startDate), yearFormat);
                }
            }
        }
        string start = GetLocalDateTimeBasedOnCulture(startDate, DateTimeType.Date, dayFormat, monthFormat, yearFormat);
        string end = GetLocalDateTimeBasedOnCulture(endDate, DateTimeType.Date, dayFormat, monthFormat, yearFormat);
        return start + (start == end ? string.Empty : $" - {end}");
    }

    public static string HtmlToPlainText(string html)
    {
        if (html != null)
        {
            return HttpUtility.HtmlDecode(Regex.Replace(html, "<" + "<(.|\n)*?>" + ">", ""));
        }
        return html;
    }

    private static bool IsWithinYearRange(ref DateTimeOffset startDate, ref DateTimeOffset endDate, int days)
    {
        return days < 367
            && startDate.LocalDateTime.Month == 1
            && endDate.LocalDateTime.Month == 12
            && startDate.LocalDateTime.Year == endDate.LocalDateTime.Year;
    }

    public static DateTimeOffset GetDateTimeAccordingEssentials(DateTimeOffset? date)
    {
        return _essentials.ConvertToLocalTime(date.Value);
    }

    /// <summary>
    /// Returns From date and to date range values based on current date and type of duration
    /// </summary>
    /// <param name="currentValue">The current date value</param>
    /// <param name="durationRange">Indicates the duration is Day/Week/Month/Quarter/Year</param>
    /// <param name="nextOrPreviousAction">Indicates we are going back/ forward</param>
    /// <returns>Tuple with from date and to date values</returns>
	public static (DateTimeOffset, DateTimeOffset) CalculateFromToDateRanges(DateTimeOffset currentValue, long durationRange, string nextOrPreviousAction)
    {
        currentValue = _essentials.ConvertToLocalTime(currentValue);

        DateTimeOffset fromDate = currentValue;
        DateTimeOffset toDate = currentValue;
        DateTime startOfWeek = currentValue.Date.AddDays(-(int)currentValue.DayOfWeek);
        DateTime startOfQuarter = new DateTime(currentValue.Year, (currentValue.Month - 1) / 3 * 3 + 1, 1);
        DateTime currentMonthStart = new DateTime(currentValue.Year, currentValue.Month, 1);
        if (nextOrPreviousAction == ResourceConstants.R_NEXT_ACTION_KEY)
        {
            switch (durationRange)
            {
                case ResourceConstants.R_DAY_FILTER_KEY_ID:
                    fromDate = currentValue.AddDays(1);
                    toDate = currentValue.AddDays(1);
                    break;
                case ResourceConstants.R_WEEK_FILTER_KEY_ID:
                    fromDate = startOfWeek.AddDays(7);
                    toDate = startOfWeek.AddDays(13);
                    break;
                case ResourceConstants.R_MONTH_FILTER_KEY_ID:
                    fromDate = currentMonthStart.AddMonths(1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case ResourceConstants.R_QUARTER_FILTER_KEY_ID:
                    fromDate = startOfQuarter.AddMonths(3);
                    toDate = startOfQuarter.AddMonths(6).AddDays(-1);
                    break;
                case ResourceConstants.R_YEAR_FILTER_KEY_ID:
                    fromDate = new DateTime(currentValue.Year + 1, 1, 1);
                    toDate = new DateTime(currentValue.Year + 1, 12, 31);
                    break;
                default:
                    toDate = currentValue;
                    fromDate = GetDefaultDateTime;
                    break;
            }
        }
        else if (nextOrPreviousAction == ResourceConstants.R_PREVIOUS_ACTION_KEY)
        {
            switch (durationRange)
            {
                case ResourceConstants.R_DAY_FILTER_KEY_ID:
                    fromDate = currentValue.AddDays(-1);
                    toDate = currentValue.AddDays(-1);
                    break;
                case ResourceConstants.R_WEEK_FILTER_KEY_ID:
                    fromDate = startOfWeek.AddDays(-7);
                    toDate = startOfWeek.AddDays(-1);
                    break;
                case ResourceConstants.R_MONTH_FILTER_KEY_ID:
                    fromDate = currentMonthStart.AddMonths(-1);
                    toDate = currentMonthStart.AddDays(-1);
                    break;
                case ResourceConstants.R_QUARTER_FILTER_KEY_ID:
                    fromDate = startOfQuarter.AddMonths(-3);
                    toDate = startOfQuarter.AddDays(-1);
                    break;
                case ResourceConstants.R_YEAR_FILTER_KEY_ID:
                    fromDate = new DateTime(currentValue.Year - 1, 1, 1);
                    toDate = new DateTime(currentValue.Year - 1, 12, 31);
                    break;
                default:
                    toDate = currentValue;
                    fromDate = GetDefaultDateTime;
                    break;
            }
        }
        else
        {
            switch (durationRange)
            {
                case ResourceConstants.R_DAY_FILTER_KEY_ID:
                    fromDate = DateTimeOffset.Now;
                    toDate = DateTimeOffset.Now; 
                    break;
                case ResourceConstants.R_WEEK_FILTER_KEY_ID:
                    fromDate = startOfWeek;
                    toDate = startOfWeek.AddDays(6);
                    break;
                case ResourceConstants.R_MONTH_FILTER_KEY_ID:
                    fromDate = currentMonthStart;
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
                case ResourceConstants.R_QUARTER_FILTER_KEY_ID:
                    fromDate = startOfQuarter;
                    toDate = startOfQuarter.AddMonths(3).AddDays(-1);
                    break;
                case ResourceConstants.R_YEAR_FILTER_KEY_ID:
                    fromDate = new DateTime(currentValue.Year, 1, 1);
                    toDate = new DateTime(currentValue.Year, 12, 31);
                    break;
                default:
                    toDate = currentValue;
                    fromDate = GetDefaultDateTime;
                    break;
            }
        }
        DateTimeOffset fromDateOffset = new DateTimeOffset(fromDate.Year, fromDate.Month, fromDate.Day, 00, 00, 00, 0001, fromDate.Offset);
        DateTimeOffset toDateOffset =  new DateTimeOffset(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59, 999, toDate.Offset);
        return (fromDateOffset, toDateOffset);
    }

    /// <summary>
    /// Calculates the duration for the first time 
    /// </summary>
    /// <param name="minDate">Min date of the data range</param>
    /// <param name="maxDate">Min date of the data range</param>
    /// <returns>Duration of first range</returns>
    public static short CalculateDuration(DateTime minDate, DateTime maxDate)
    {
        var days = Math.Abs((maxDate - minDate).TotalDays);
        if (days <= 7)
        {
            return ResourceConstants.R_DAY_FILTER_KEY_ID;
        }
        else if (days > 7 && days <= 30)
        {
            return ResourceConstants.R_WEEK_FILTER_KEY_ID;
        }
        else if (days > 30 && days <= 90)
        {
            return ResourceConstants.R_MONTH_FILTER_KEY_ID;
        }
        else if (days > 90 && days <= 360)
        {
            return ResourceConstants.R_QUARTER_FILTER_KEY_ID;
        }
        else
        {
            return ResourceConstants.R_YEAR_FILTER_KEY_ID;
        }
    }

    public static bool CheckAddEditPermissions()
    {
        return true;
    }

    public static string GetMobileNumberWithoutCountryCode(string mobileNumberString)
    {
        var mobileNumberParts = mobileNumberString?.Split('-');
        if (mobileNumberParts?.Length == 2)
        {
            if (!string.IsNullOrWhiteSpace(mobileNumberParts[1]))
            {
                return mobileNumberParts[1];
            }
        }
        return string.Empty;
    }
}