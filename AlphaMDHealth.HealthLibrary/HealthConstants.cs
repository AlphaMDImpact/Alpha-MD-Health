namespace AlphaMDHealth.HealthLibrary
{
    public static class HealthConstants
    {
        public static string PERMISSION_REQUEST_NAME
        {
            get { return "PermissionStatus"; }
        } 
        public static int POST_BREAKFAST_TIME
        {
            get { return 10; }
        }
        public static int POST_LUNCH_TIME
        {
            get { return 15; }
        }
        public static int PRE_BREAKFAST_TIME
        {
            get { return 8; }
        }
        public static int PRE_LUNCH_TIME
        {
            get { return 12; }
        }
        public static int PRE_DINNER_TIME
        {
            get { return 19; }
        }
        public static int GOOGLE_FIT_PERMISSIONS_REQUEST_CODE => 1903;

        public static string HEALTH_LIBRARY_PERMISSION_DENIED_PREFERENCE_KEY => "AlphaMDHealth.HealthLibrary.PermissionDenied";
    }
}