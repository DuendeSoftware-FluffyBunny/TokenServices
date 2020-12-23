# External Services
These should be considered islands.  Islands that expose a discovery endpoint that is very similar to how OIDC exposes a discovery endpoint.  
| Authority | Endpoint | Description  |
| --------- | -------- | ----  |
| https://localhost:7301/zep/api/Consent | .well-known/consent-configuration | Consent discovery Document|  

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
