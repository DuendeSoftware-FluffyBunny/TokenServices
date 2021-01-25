# TokenServices

This project requires [Duende](https://duendesoftware.com/) as its base and as such you must agree to the Duende Software [licensing terms](https://github.com/DuendeSoftware/IdentityServer/blob/main/LICENSE).

# OAuth2 Services
Duende Software provides OIDC SSO functionality, this assumes that SSO services (Authentication) is provided externally.  In my case there is an existing SSO, like AzureAD or OKTA, and a stand-alone Duende SSO deployment that is already in place.  For a lot of my proofs I use Google as my OIDC IDP.  

The OAuth2 services supported by this project are;

1. [**client_credentials**](https://identityserver4.readthedocs.io/en/latest/endpoints/token.html?highlight=client_credentials#token-endpoint)    
  We get this for free with IdentityServer.  This is used for service-2-service trust.  
  
2.  [**token_exchange**](docs/token-exchange.md)  
  This is custom because there is no reference answer when it comes to exchanging an id_token (issued by google) for an access_token to services that I control.  
  This is used for granting authorization to services that a user can access.  
  In this flow I fan out calls to well known external services, passing them the user and scopes that were requested.  The external service rejects, accepts or accepts and modifies the requested scopes.  The real control of what a user gets access to is delegated to the actual services.  I also support exchanging an access_token for another access_token (exactly like the Azure OBO flows)  

3. [**token_exchange_mutate**](docs/token-exchange-mutate.md)  
   This is a followup to a token_exhange where ***"Oops, I wish I would have asked for more scopes in the original token_exchange!"***  This is similar, but [better](docs/mutate-vs-on-behalf-of.md), to the a ["On Behalf Of" flow](docs/mutate-vs-on-behalf-of.md).  This flow requires that reference_tokens be used, because we want to modify the backend database by changing the scopes the original access_token is referencing.  In this case a special token_exchange is rerun but with more scopes than before.  The output doesn't change the infield access_token or refresh_token(if there was one).     

4. **device_code_flow**   
  Nothing more than a variant of a token_exchange.  You see this flow on your TV or ROKU device, where a user_code is presented and the user is asked to go to a web portal to authorize the application.  This is accomplished by an orchestrator validating he user_code, having the user login, and updating the backend device code record that that native ROKU app is polling against.  Even here, a token exchange happens and the native app is delivered access_tokens via the poll.   

5. **arbitrary_token**  
  This is a custom extension grant that accounts for systems where a token exchange doesn't fit.  Its essentially externalizes the creation of an access_token by providing commodity services like storage and in the case of a JWT key signing management.  Its nothing more then having those private JWT libraries in your code without having to maintain your own database or signing services.  Here the only real requirement is that the caller has to take responsibility of becoming the issuer.

6. **arbitrary_identity**  
  Its essentially externalizes the creation of an id_token and access_token by providing commodity services like storage and if a JWT key signing managment.   Its nothing more then having those private JWT libraries in your code without having to maintain your own database or signing services.  Here the only real requirement is that the caller has to take responsibility of becoming the issuer.  This api could be used as the id_token and access_token minter for an OIDC provider.  The access_token created is only meant to call the issuers user_info endpoint and thus forces that opinion in the implementation.  You can have a truely arbitrary identity, but NOT an arbitrary access_token.



# OIDC Orchestrator
The role of an orchestrator is to take care of what comes after a login.  Authentication is NOT Authorization.  All an SSO/IDP does is provide proof-of-life, what an orchestrator does is lookup what the user has access to and issues in EVERY CASE authorization tokens AKA access_tokens to a meriade of services.  A simple example is that the user hasn't bought a subscription to the service.  Having an account is NOT good enough to grant you access to a downstream service, so pay up and then the access_token issued will have the scopes to the paid service.

The OIDC Orchestrator here exposes itself via the OIDC protocol.  The primary reason is that it allows clients (Web or Native) to pick an OIDC library for their particual app.  I wanted to avoid a custom protocol.  The orchestorator, being an OIDC compliant service, has the ```/.well-known/openid-configuration``` endpoint.  However, the orchestrator is NOT a real OIDC IDP.  It doesn't issue id_tokens.   It relys on the downstream IDP to do that, and as an orchestrator, it holds on to the id_token until its work is done (The work of what comes next).  When the orchestrator responds to the client, it delivers the id_tokens from the downstream IDP, along with access_tokens to various servies that were produced separate from the IDP.  

The what next for a simple orchestrator is calling the TokenService's token_exchange flow.

# [External Services](docs/external-services.md)
 These should be considered islands. Islands that expose a discovery endpoint that is very similar to how OIDC exposes a discovery endpoint.  
 



