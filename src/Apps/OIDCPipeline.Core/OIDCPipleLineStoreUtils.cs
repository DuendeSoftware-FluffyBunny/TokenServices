namespace OIDCPipeline.Core
{
    public static class OIDCPipleLineStoreUtils
    {
        public static string GenerateDownstreamIdTokenResponseKey(string key)
        {
            key = $"_oidcSession.downstream.{key}";
            return key;
        }
        public static string GenerateOriginalIdTokenRequestKey(string key)
        {
            key = $"_oidcSession.original.{key}";
            return key;
        }
    }
}
