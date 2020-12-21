# TokenServices

This project requires Duende as its base and as such you must agree to the Duende Software [licensing terms](https://github.com/DuendeSoftware/IdentityServer/blob/main/LICENSE).

# OAuth2 Services
Whilst IdentityServer provided OIDC functionality, this assumes that SSO services (Authentication) is provided externally.  In my case there is an existing SSO, like AzureAD or OKTA, that is already in place.  For a lot of my proofs I use Google as my OIDC IDP.

The top OAuth2 services I have seen are;

1. **client_credentials**    
  We get this for free with IdentityServer.  This is used for service-2-service trust.  
2.  **token_exchange**  
  This is custom because there is no reference answer when it comes to exchanging an id_token (issued by google) for an access_token to services that I control.  
  This is used for granting authorization to services that a user can access.  
  In this flow I fan our calls to well known external services, passing them the user and scopes that were requested.  The external service rejects, accepts or accepts and modifies the requeted scopes.  The real control of what a user gets access to is delegated to the actual services.  
3. **device_code_flow**   
  Nothing more than a variant of a token_exchange.  You see this flow on your TV or ROKU device, where a user_code is presented and the user is asked to go to a web portal to authorize the application.  This is accomplished by an orchestrator validating he user_code, having the user login, and updating the backend device code record that that native ROKU app is polling against.  Even here, a token exchange happens and the native app is delivered access_tokens via the poll.   
4. **arbitrary_token**  
  This is a custom extension grant that accounts for systems where a token exchange doesn't fit.  Its essentially externalizes the creation of an access_token by providing commodity services like storage and if a JWT key signing managment.  Its nothing more then having those private JWT libraries in your code without having to maintain your own database or signing services.  



# OIDC Orchestrator
The role of an orchestrator is to take care of what comes after a login.  Authentication is NOT Authorization.  All an SSO/IDP does is provide proof-of-life, what an orchestrator does is lookup what the user has access to and issues in EVERY CASE authorization tokens AKA access_tokens to a meriade or services.  A simple example is that the user hasn't bought a subscription to the service, having an account is good because we can start asking for money so that finally we can issue access_tokens to paid services.

The OIDC Orchestrator here exposes itself via the OIDC protocol.  The primary reason is that it allows clients (Web or Native) to pick an OIDC library for their particual app.  I wanted to avoid a custom protocol.  The orchestorator, being an OIDC compliant service, has the ```/.well-known/openid-configuration``` endpoint.  However, the orchestrator is NOT a real OIDC IDP.  It doesn't issue id_tokens.   It relys on the downstream IDP to do that, and as an orchestrator, it holds on to the id_token until its work is done (The work of what comes next).  When the orchestrator replys to the client, it delivers the id_tokens from the downstream IDP, along with access_tokens to various servies that were produced separate from the IDP.  

The what next for a simple orchestrator is calling the TokenService's token_exchange flow.

# External Services
These should be considered islands.  Islands that expose a discovery endpoint that is very similar to how OIDC exposes a discovery endpoint.  
| Authority | Endpoint | Data  |
| --------- | -------- | ----  |
| https://localhost:7301/zep/api/Consent | .well-known/consent-configuration | {<br>  "authorization\_endpoint": "https://localhost:7301/zep/api/consent/authorize",<br>  "scopes\_supported": \[<br>    "https://www.companyapis.com/auth/zep",<br>    "https://www.companyapis.com/auth/zep.readonly",<br>    "https://www.companyapis.com/auth/zep.modify"<br>  \],<br>  "authorization\_type": "subject\_and\_scopes"<br>} |  

When a call is made to the external service the service can return the following;
1. scopes allowed
2. claims that the service wants added.  These claims will be namespaced.  
3. custom json payload.  This is namespaced as well 

Namespacing is the service key ```zep``` prepended to the claims and custom payload.  

```
{
  "authorized": true,
  "scopes": [
    "https://www.companyapis.com/auth/zep",
    "https://www.companyapis.com/auth/zep.readonly",
    "https://www.companyapis.com/auth/zep.modify"
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


## External Services Authorization Types
| Type | Description |
| --------- | -------- |
| implicit           | Don't call the external service to authorize.  Simple authentication is good enough and give the caller all scopes requested |
| subject_and_scopes | The service is the final shot caller.  The token_exchange will delegate all scopes to the service. |

# Token Exchange
This is exchanging an id_token + requested scopes for access.  
The token exchange implementation here is fanning out calls to external services asking for consent to issue an access token with the requested scopes to those services.  The requested scopes are follow a well known format.  

| paramater  | Description |
| ---------  | -------- |
| grant_type | urn:ietf:params:oauth:grant-type:token-exchange |
| scope | offline_access <br>https://www.companyapis.com/auth/myphotos <br>https://www.companyapis.com/auth/myphotos.readonly <br>https://www.companyapis.com/auth/myphotos.modify |
| subject_token_type | urn:ietf:params:oauth:token-type:id_token |
| subject_token | {id_token} |




