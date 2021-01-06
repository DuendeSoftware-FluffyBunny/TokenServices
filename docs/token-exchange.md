# OAuth2 Token Exchange  
[OAuth 2.0 Token Exchange](https://tools.ietf.org/html/rfc8693)  

This is exchanging an id_token + requested scopes for access.  
The token exchange implementation here is fanning out calls to external services asking for consent to issue an access token with the requested scopes to those services.  The requested scopes are follow a well known format.  


| parameter  | Description |
| ---------  | -------- |
| grant_type | urn:ietf:params:oauth:grant-type:token-exchange |
| scope | offline_access <br>https://www.companyapis.com/auth/myphotos <br>https://www.companyapis.com/auth/myphotos.readonly <br>https://www.companyapis.com/auth/myphotos.modify |
| subject_token_type | urn:ietf:params:oauth:token-type:id_token |
| subject_token | {id_token} |

This is custom and there are no reference example of this exchange.  This is the closest I got to designing one and used Google's login with consent to model mine.  Thank You Google designer.  

Here I require that [external services](external-services.md) be registered with my token exchange;

| Service Name  | Authority Endpoint |
| ---------  | -------- |
| myphotos | https://localhost:7301/myphotos/api/Consent |

As with OIDC the Authority here must have the following discovery document handler;
```
{{Authority}/.well-known/consent-configuration
```

Again, I copied [Google's scope naming convention](https://developers.google.com/identity/protocols/oauth2/scopes)  

```
https://www.companyapis.com/auth/{{service_name}}
```

Any scope that matches this pattern is sent to the matching service authorization endpoint.

# Response
```
{
    "access_token": "831B0B1AED66C9C19B590C8480B7D8E04F06481C1720E648B457649F241CA2A0",
    "expires_in": 3600,
    "token_type": "Bearer",
    "refresh_token": "411D266ECB49F6D71B4C196EE4A039341C4C7419CC070E466CF65D0A8899CDB0",
    "scope": "https://www.companyapis.com/auth/myphotos https://www.companyapis.com/auth/myphotos.modify https://www.companyapis.com/auth/myphotos.readonly offline_access"
}
```
# Introspection  
```
{
    "iss": "https://localhost:7001/zep",
    "nbf": 1608750808,
    "iat": 1608750808,
    "exp": 1608754408,
    "client_id": "a1ce197b-dd13-43a0-8376-7b762304fdd5",
    "sub": "104758924428036663951",
    "auth_time": 1608750642,
    "idp": "local",
    "amr": "urn:ietf:params:oauth:grant-type:token-exchange",
    "myphotos.geo_location": "Canada",
    "custom_payload": {
        "myphotos": {
            "name": "MyCustom",
            "value": 1234
        }
    },
    "active": true,
    "scope": "https://www.companyapis.com/auth/myphotos https://www.companyapis.com/auth/myphotos.modify https://www.companyapis.com/auth/myphotos.readonly offline_access"
}
```
![your-UML-diagram-name](http://www.plantuml.com/plantuml/proxy?cache=no&src=https://raw.githubusercontent.com/DuendeSoftware-FluffyBunny/TokenServices/main/docs/token-exchange.iuml)


