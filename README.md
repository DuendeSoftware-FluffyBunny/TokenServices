# TokenServices

This project requires Duende as its base and as such you must agree to the Duende Software [licensing terms](https://github.com/DuendeSoftware/IdentityServer/blob/main/LICENSE).

# OAuth2 Services
Whilst IdentityServer provided OIDC functionality, this assumes that SSO services (Authentication) is provided externally.  In my case there is an existing SSO, like AzureAD or OKTA, that is already in place.  

# OIDC Orchestrator
The role of an orchestrator is to take care of what comes after a login.  Authentication is NOT Authorization.  All an SSO/IDP does is provide proof-of-life, what an orchestrator does is lookup what the user has access to and issues in EVERY CASE authorization tokens AKA access_tokens to a meriade or services.  A simple example is that the user hasn't bought a subscription to the service, having an account is good because we can start asking for money so that finally we can issue access_tokens to paid services.

The OIDC Orchestrator here exposes itself via the OIDC protocol.  The primary reason is that it allows clients (Web or Native) to pick an OIDC library for their particual app.  I wanted to avoid a custom protocol.  The orchestorator, being an OIDC compliant service, has the ```/.well-known/openid-configuration``` endpoint.  However, the orchestrator is NOT a real OIDC IDP.  It doesn't issue id_tokens.   It relys on the downstream IDP to do that, and as an orchestrator, it holds on to the id_token until its work is done.  The work of what comes next.  When the orchestrator replys to the client, it delivers the id_tokens from the downstream IDP, along with access_tokens to various servies that were produced separate from the IDP.


