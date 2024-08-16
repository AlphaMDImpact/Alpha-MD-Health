namespace AlphaMDHealth.Model
{
    /// <summary>
    /// Methods which are being used across solution
    /// </summary>
    public static class ParameterMethods
    {
        /// <summary>
        /// Maps parameter name and value into model
        /// </summary>
        /// <param name="name">name of parameter</param>
        /// <param name="value">value of parameter</param>
        /// <returns>Feature parameter model</returns>
        public static SystemFeatureParameterModel CreateParameter(string name, string value)
        {
            return new SystemFeatureParameterModel { ParameterName = name, ParameterValue = value };
        }

        /// <summary>
        /// Map data into system feature parameters
        /// </summary>
        /// <param name="parameters">parameters</param>
        /// <returns>list of feature parameters</returns>
        public static List<SystemFeatureParameterModel> AddParameters(params SystemFeatureParameterModel[] parameters)
        {
            var list = new List<SystemFeatureParameterModel>();
            list.AddRange(parameters.ToList());
            return list;
        }
    }
}