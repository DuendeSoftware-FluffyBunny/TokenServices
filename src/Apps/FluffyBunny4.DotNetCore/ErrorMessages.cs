namespace FluffyBunny4.DotNetCore
{
    /// <summary>
    ///     Static/Constant Error Messages.
    /// </summary>
    public static class ErrorMessages
    {
        /// <summary>
        ///     Error message for when a parameter is null.
        /// </summary>
        public static string ParameterNull = $"{Constants.Placeholders.ParameterName} cannot be null.";

        /// <summary>
        ///     Error message for when a parameter is null or has a default value.
        /// </summary>
        public static string ParameterNullOrDefault = $"{Constants.Placeholders.ParameterName} cannot be null or default.";

        /// <summary>
        ///     Error message for when a value is less than another value.
        /// </summary>
        public static string ValueLessThan =
            $"{Constants.Placeholders.ParameterName} must be more than {Constants.Placeholders.MinimumValue}.";
    }
}