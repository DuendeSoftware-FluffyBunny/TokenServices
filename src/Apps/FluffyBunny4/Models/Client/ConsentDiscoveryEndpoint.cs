using Microsoft.AspNetCore.Mvc;
using System;

namespace FluffyBunny4.Models.Client
{
    /// <summary>
    /// Represents a URL to a discovery endpoint - parsed to separate the URL and authority
    /// </summary>
    public class ConsentDiscoveryEndpoint
    {
        /// <summary>
        /// Parses a URL and turns it into authority and discovery endpoint URL.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">
        /// Malformed URL
        /// </exception>
        public static ConsentDiscoveryEndpoint ParseUrl(string input)
        {
            var success = Uri.TryCreate(input, UriKind.Absolute, out var uri);
            if (success == false)
            {
                throw new InvalidOperationException("Malformed URL");
            }

            if (!ConsentDiscoveryEndpoint.IsValidScheme(uri))
            {
                throw new InvalidOperationException("Malformed URL");
            }

            var url = input.RemoveTrailingSlash();

            if (url.EndsWith(Constants.Discovery.DiscoveryEndpoint, StringComparison.OrdinalIgnoreCase))
            {
                return new ConsentDiscoveryEndpoint(url.Substring(0, url.Length - Constants.Discovery.DiscoveryEndpoint.Length - 1), url);
            }
            else
            {
                return new ConsentDiscoveryEndpoint(url, url.EnsureTrailingSlash() + Constants.Discovery.DiscoveryEndpoint);
            }
        }
        /// <summary>
        /// Determines whether uses a secure scheme accoding to the policy.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="policy">The policy.</param>
        /// <returns>
        ///   <c>true</c> if [is secure scheme] [the specified URL]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSecureScheme(Uri url, ConsentDiscoveryPolicy policy)
        {
            return string.Equals(url.Scheme, "https", StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// Determines whether the URL uses http or https.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>
        ///   <c>true</c> if [is valid scheme] [the specified URL]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidScheme(Uri url)
        {
            if (string.Equals(url.Scheme, "http", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(url.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentDiscoveryEndpoint"/> class.
        /// </summary>
        /// <param name="authority">The authority.</param>
        /// <param name="url">The discovery endpoint URL.</param>
        public ConsentDiscoveryEndpoint(string authority, string url)
        {
            Authority = authority;
            Url = url;
        }
        /// <summary>
        /// Gets or sets the authority.
        /// </summary>
        /// <value>
        /// The authority.
        /// </value>
        public string Authority { get; }

        /// <summary>
        /// Gets or sets the discovery endpoint.
        /// </summary>
        /// <value>
        /// The discovery endpoint.
        /// </value>
        public string Url { get; }
    }
}
