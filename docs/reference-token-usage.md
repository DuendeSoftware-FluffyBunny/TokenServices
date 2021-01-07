# access_token as a reference_token

Before talking about reference_tokens, I want to focus on another reference_token (the refresh_token).
A refresh_token is really only needed when a JWT access_token is being refreshed.  This is because a JWT, once issued, cannot be revoked - well not without a messy bunch of work that invovles any service accepting that JWT.

So, we set that JWT to a short TTL, and a refresh_token for a large TTL, and make the clients keep coming home to get a new JWT.  At least I can revoke that refresh_token.


What if I don't have a JWT?  Is a refresh_token just redundant in this case?  

Yes, the refresh_token is redundant.

Lets go through an extreme example of an reference_token type access_token

1. Set the access_token to the same TTL that you would normally have set the absolute TTL of the refresh_token in the JWT case.  
**One Year**

2. Since its a reference_token the services that accept it need to call the OAuth2 introspection endpoint to find out what the token represents.

3. Make your services **cache** the access_token's introspection response to something reasonable TTL  
  **Your Original JWT access_token's TTL**  
  Since introspection is in the OAuth2 specification find yourself a validation library that accounts for it.  Asp.Net Core has [one](https://leastprivilege.com/2020/07/06/flexible-access-token-validation-in-asp-net-core/#:~:text=The%20ASP.NET%20Core%20authentication%20system%20went%20through%20a,e.g.%20reference%20tokens%20that%20get%20validated%20via%20introspection.)  

4. Your paranoia will set your TTL cache.    


  
 
