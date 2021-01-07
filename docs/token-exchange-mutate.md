# OAuth2 Token Exchange Mutate  

This is a token exchange that mutates a previous [token-exchange](token-exchange.md).  It will NOT change the infield access_token and refresh_token.    
The token exchange implementation here is fanning out calls to external services asking for consent to issue an access token with the requested scopes to those services.  The requested scopes are follow a well known format.  


| parameter  | Description |
| ---------  | -------- |
| grant_type | urn:ietf:params:oauth:grant-type:token-exchange-mutate |
| scope | offline_access <br>https://www.companyapis.com/auth/myphotos <br>https://www.companyapis.com/auth/myphotos.readonly <br>https://www.companyapis.com/auth/myphotos.modify |
| subject_token_type | urn:ietf:params:oauth:token-type:access_token |
| subject_token | {access_token} |

Here we already have the issuer and only need the access_token as our means to find the access_token and refresh_token associated with it.  
