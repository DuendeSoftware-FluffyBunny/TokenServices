# External Services
These should be considered islands.  Islands that expose a discovery endpoint that is very similar to how OIDC exposes a discovery endpoint.  
| Authority | Endpoint | Data  |
| --------- | -------- | ----  |
| https://localhost:7301/zep/api/Consent | .well-known/consent-configuration | ```{  "authorization_endpoint": "https://localhost:7301/zep/api/consent/authorize",  "scopes_supported": ["https://www.companyapis.com/auth/zep",    "https://www.companyapis.com/auth/zep.readonly",    "https://www.companyapis.com/auth/zep.modify"  ],  "authorization_type": "subject_and_scopes"} ```|  

