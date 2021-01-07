# External Services
These should be considered islands.  Islands that expose a discovery endpoint that is very similar to how OIDC exposes a discovery endpoint.  
| Authority | Endpoint | Description  |
| --------- | -------- | ----  |
| https://localhost:7301/myphotos/api/Consent | .well-known/consent-configuration | Consent discovery Document|  

```
{
  "authorization_endpoint": "https://localhost:7301/myphotos/api/consent/authorize",
  "scopes_supported": [
    "https://www.companyapis.com/auth/myphotos",
    "https://www.companyapis.com/auth/myphotos.readonly",
    "https://www.companyapis.com/auth/myphotos.modify"
  ],
  "authorization_type": "subject_and_scopes"
}
```
# Consent Document
| Value | Description  |
| --------- | -------- | 
| authorization_endpoint | service endpoint for authorizing a subject's request for scopes |  
| scopes_supported       | array of supported scopes |  
| authorization_type     | the type of authorization [implicit \| subject_and_scopes] |

# Authorization_types
| Type | Description |
| --------- | -------- |
| implicit           | This authorization type is for services that find it ok for simple subject authentication done by the token_exchange to be good enough.  This is no reason to call the service.  Just give out any scope that was requested. |
| subject_and_scopes |This authorization type give full control to the service to accept, reject, and ultimatly state what scopes, claims and custom data is put in the final access_token.  |

 
## Authorization Call

### Request
The following is an example of what gets posted to the **myphotos** service authoirization_endpoint.  
***NOTE***: Scopes must follow a strict convention where ***https://www.companyapis.com/auth/*** prepends all service scopes.  
```
{
  "authorization_type": "subject_and_scopes",
  "subject": "1234abcd",
  "scopes": [
    "https://www.companyapis.com/auth/myphotos",
    "https://www.companyapis.com/auth/myphotos.readonly",
    "https://www.companyapis.com/auth/myphotos.modify"
  ] 
}

```
### Response
```
{
  "authorized": true,
  "scopes": [
    "https://www.companyapis.com/auth/myphotos",
    "https://www.companyapis.com/auth/myphotos.readonly",
    "https://www.companyapis.com/auth/myphotos.modify"
  ],
  "subject": "1234abcd",
  "claims": [
    {
      "type": "geo_location",
      "value": "Canada"
    }
  ],
  "custom_payload": {
    "name": "MyCustom",
    "value": 1234
  }
}
```
The response comming back from the service must be honored.  The resulting access_token will **ONLY** have the scopes that the service responded with.  In the above example what was requested was given, but the service can remove or add at its discretion.  The service also has the right to add custom claims and a custom json payload.  These custom claims will be automatically namespaced in the final token.
```
{
    "iss": "https://accounts.google.com",
    "nbf": 1609950477,
    "iat": 1609950477,
    "exp": 1609954077,
    "client_id": "a1ce197b-dd13-43a0-8376-7b762304fdd5",
    "sub": "104758924428036663951",
    "auth_time": 1609950477,
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
    "scope": "https://www.companyapis.com/auth/myphotos https://www.companyapis.com/auth/myphotos.readonly https://www.companyapis.com/auth/myphotos.modify offline_access"
}
```

# Multiple Expernal Services
The token_exchange flow can accept scopes to many external services, so in the end we will have a single access_token with all the scopes allowed from each external servies, as well as the service claims and custom payloads.

