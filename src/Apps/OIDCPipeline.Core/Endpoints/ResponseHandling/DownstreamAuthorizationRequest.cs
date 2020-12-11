using System.Collections.Specialized;

namespace OIDCPipeline.Core.Endpoints.ResponseHandling
{
    internal class DownstreamAuthorizationRequest
    {
        public NameValueCollection ExtraValues { get; set; }


        /*
client_id=mvc
&redirect_uri=https%3A%2F%2Fp7core.127.0.0.1.xip.io%3A44311%2Fsignin-oidc
&response_type=id_token
&scope=openid%20profile
&response_mode=form_post
&nonce=636973229335838266.ZWJhM2U4M2YtYWNiYi00YjZkLTkwMWYtNjRmMjM3MWRiYTk5OWNkNDIzMWUtZmY4OS00YWE0LTk4MGUtMTdiMjYxNmNmZjRk&state=CfDJ8KOz5LEySMhBtqpccMk4UVhA1PvGQQvpqQBUyR-97TDZvaPuNquTLJIUxKMYzF-Ov_HHCnnmcTForzd5RJ4jmLONvcZLY3XCHnrhh9Sc2oR2Lv2HACvPVBMy2oYmmPBtNIoXroQ9WePE_KtPyFw8ntRsHIYMmT5a0fLKGeJcwK3ewoiRHxjKpOr9hXZau9f7CVVqMvtWC2ngWrFsEeh8S0YtRZQFT-7XyjE9dNiyKp_Z-4iBUbbqzVnT4GmEmErZXUjmBhmVsMLz5h9y_F3usRT3lg7LxUNamnJuROnYIqmJzf0fYVJq1mcB5hcUipo2SNcILG3xkUikc84VznSGvD7V_qFjOHVPtOEX02JH9M4ymb3iZtZSE9dDr2RkwTU7StoKgM-x195bBULpwms8weJO-kx5I6UrY_lmWl0SFqYN
*/
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string redirect_uri { get; set; }
        public string response_type { get; set; }
        public string nonce { get; set; }
        public string response_mode { get; set; }
        public string state { get; set; }
        public string scope { get; set; }
    }
}
