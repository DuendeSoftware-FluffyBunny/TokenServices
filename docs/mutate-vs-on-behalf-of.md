# [Azure's on-behalf-of (OBO)](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow)  
This is really nothing more than a OAuth2 token_exchange flow where the subject_token == access_token.  The requirement for both the Azure OBO and the OAuth2 flows is that the access_token must be valid and contain a known subject.  

Why Azure would cook their own OBO vs just using the OAuth2 token_exchange specification is for them to answer.

Saying that, this token_service supports passing an access_token via the token_exchange flow.

# token_exchange_mutate vs on-behalf-of (OBO)

Both of these try to address the "Oops, that access_token that I have isn't going to work for that downstream service" problem.  

The [Azure OBO](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow) version is based on JWTs.  So the JWT that came into my service is self-contained and cannot be changed.  I have to get another JWT that has the claims I need, but now I have to store that in a cache or keep asking Azure to create one on every signal call.
That doesn't sound appealing at all.

The token_exchange_mutate, having the same problem, decides that mutating the access_token without changing it or its refresh_token is better.  This requires that the access_token be in the form of a reference_token.  This way only the data that the access_token is pointing to changes, and that change is in a backend datastore.
Once the mutate happens you don't have to store it, because its already stored by the TokenService.

Now I understand fully why Google doesn't use JWT type access_tokens.  


# Alternative
Another approach if you don't want the original access_token (still needs to be a reference) to get all these new scopes is to do the following;

1. Run a token_exhange by passing in the original access_token (this contains the subject)
2. Ask for the new scopes
3. Get a brand new access_token/refresh_token
4. Problem: The TokenService is storing the access_token/refresh_token, but we need to store them as well.
5. A new custom mutate allows you to add arbitrary json where you store those downstream access_token/refresh_token in the original access_token.

So that original access_token CANNOT be used to talk directly to the downstream service.  A native app CANNOT crack open the access_token to see them, thanks to it being a reference. Only the middle-teir can use introspection to crack open the access_token and then use the contained access_token/refresh_token to talk to the downstream service.  




