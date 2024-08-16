namespace AlphaMDHealth.Utility;

public static class FoodConstants
{
    #region URLS

    public static string NUTRITIONIX_X_APP_ID_TEXT => "x-app-id";
    public static string NUTRITIONIX_X_APP_KEY_TEXT => "x-app-key";
    public static string NUTRITIONIX_QUERY_PARAM_TEXT => "query";

    public static string EDAMAM_SEARCH_TO_NUTRITION_END_POINT => "&from=0&to=1";

    #endregion

    #region Fields
    public static string RESULTS_TEXT_KEY => "results";
    public static string FOODS_TEXT_KEY => "foods";
    public static string SPOONACULAR_ID => "id";
    public static string SPOONACULAR_TITLE => "title";
    public static string SPOONACULAR_IMAGE => "image";
    public static string SPOONACULAR_IMAGES => "images";
    public static string SPOONACULAR_BAD => "bad";
    public static string SPOONACULAR_AMOUNT => "amount";
    public static string SPOONACULAR_GOOD => "good";
    #endregion

    #region nutritionix reult map keys

    public static string VALUE_TEXT_KEY => "value";
    public static string NIX_FOOD_NAME_TEXT_KEY => "food_name";
    public static string NIX_COMMON_TEXT_KEY => "common";
    public static string NIX_BRANDED_TEXT_KEY => "branded";
    public static string PHOTO_TEXT_KEY => "photo";
    public static string THUMB_TEXT_KEY => "thumb";
    public static string ATTR_ID_TEXT_KEY => "attr_id";
    public static string FULL_NUTRIENTS_TEXT_KEY => "full_nutrients";

    #endregion

    #region Edamam field

    public static string HITS_TEXT_KEY => "hits";
    public static string RECIPE_TEXT_KEY => "recipe";
    public static string TOTALNUTRIENTS_TEXT_KEY => "totalNutrients";
    public static string LABEL_TEXT_KEY => "label";
    public static string QUANTITY_TEXT_KEY => "quantity";
    public static string UNIT_TEXT_KEY => "unit";
    public static string IMAGE_TEXT_KEY => "image";
    public static string HINTS_TEXT_KEY => "hints";
    public static string FOOD_TEXT_KEY => "food";

    #endregion

    #region Fat secret

    public static string FATSECRET_SERVINGS_TEXT_KEY => "servings";
    public static string FATSECRET_SERVING_TEXT_KEY => "serving";
    public static string FATSECRET_FOOD_ID => "food_id";
    public static string FATSECRET_FOOD_NAME => "food_name";
    public static string FATSECRET_PROFILE_TEXT_KEY => "profile";
    public static string AUTH_TOKEN_TEXT_KEY => "auth_token";
    public static string AUTH_SECRET_TEXT_KEY => "auth_secret";
    public static string AUTH_CONTENT_TYPE_TEXT_KEY => "application/x-www-form-urlencoded";
    public static string AUTH_SIGNATURE_TYPE_TEXT_KEY => "HMAC-SHA1";

    public static string OAUTH_VERSION_NUMBER => "1.0";
    public static string OAUTH_PARAMETER_PREFIX => "oauth_";
    public static string XOAUTH_PARAMETER_PREFIX => "xoauth_";
    public static string OPEN_SOCIAL_PARAMETER_PREFIX => "opensocial_";

    public static string OAUTH_CONSUMER_KEY => "oauth_consumer_key";
    public static string OAUTH_VERSION => "oauth_version";
    public static string OAUTH_SIGNATURE_METHOD => "oauth_signature_method";
    public static string OAUTH_SIGNATURE => "oauth_signature";
    public static string OAUTH_TIMESTAMP => "oauth_timestamp";
    public static string OAUTH_NONCE => "oauth_nonce";
    public static string OAUTH_TOKEN => "oauth_token";
    public static string UNRESERVED_CHARS => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

    #endregion

}
