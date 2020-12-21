# TokenServices

This project requires Duende as its base and as such you must agree to the Duende Software [licensing terms](https://github.com/DuendeSoftware/IdentityServer/blob/main/LICENSE).

# OAuth2 Services
Whilst IdentityServer provided OIDC functionality, this assumes that SSO services (Authentication) is provided externally.  In my case there is an existing SSO, like AzureAD or OKTA, that is already in place.  For a lot of my proofs I use Google as my OIDC IDP.

The top 3 OAuth2 services I have seen are;

1. **client_credentials**    
  We get this for free with IdentityServer.  This is used for service-2-service trust.  
2.  **token_exchange**  
  This is custom because there is no reference answer when it comes to exchanging an id_token (issued by google) for an access_token to services that I control.  
  This is used for granting authorization to services that a user can access.  
  In this flow I fan our calls to well known external services, passing them the user and scopes that were requested.  The external service rejects, accepts or accepts and modifies the requeted scopes.  The real control of what a user gets access to is delegated to the actual services.  
3. **device_code_flow**   
  Nothing more than a variant of a token_exchange.  You see this flow on your TV or ROKU device, where a user_code is presented and the user is asked to go to a web portal to authorize the application.  This is accomplished by an orchestrator validating he user_code, having the user login, and updating the backend device code record that that native ROKU app is polling against.  Even here, a token exchange happens and the native app is delivered access_tokens via the poll.   
 



# OIDC Orchestrator
The role of an orchestrator is to take care of what comes after a login.  Authentication is NOT Authorization.  All an SSO/IDP does is provide proof-of-life, what an orchestrator does is lookup what the user has access to and issues in EVERY CASE authorization tokens AKA access_tokens to a meriade or services.  A simple example is that the user hasn't bought a subscription to the service, having an account is good because we can start asking for money so that finally we can issue access_tokens to paid services.

The OIDC Orchestrator here exposes itself via the OIDC protocol.  The primary reason is that it allows clients (Web or Native) to pick an OIDC library for their particual app.  I wanted to avoid a custom protocol.  The orchestorator, being an OIDC compliant service, has the ```/.well-known/openid-configuration``` endpoint.  However, the orchestrator is NOT a real OIDC IDP.  It doesn't issue id_tokens.   It relys on the downstream IDP to do that, and as an orchestrator, it holds on to the id_token until its work is done (The work of what comes next).  When the orchestrator replys to the client, it delivers the id_tokens from the downstream IDP, along with access_tokens to various servies that were produced separate from the IDP.  

The what next for a simple orchestrator is calling the TokenService's token_exchange flow.


