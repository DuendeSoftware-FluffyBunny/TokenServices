# token_exchange_mutate vs on-behalf-of (OBO)

Both of these try to address the "Oops, that access_token that I have isn't going to work for that downstream service" problem.  

The [Azure OBO](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow) version is based on JWTs.  So the JWT that came into my service is self-contained and cannot be changed.  I have to get another JWT that has the claims I need, but now I have to store that in a cache or keep asking Azure to create one on every signal call.
That doesn't sound appealing at all.

The token_exchange_mutate, having the same problem, decides that mutating the access_token without changing it or its refresh_token is better.  This requires that the access_token be in the form of a reference_token.  This way only the data that the access_token is pointing to changes, and that change is in a backend datastore.
Once the mutate happens you don't have to store it, because its already stored by the TokenService.

Now I understand fully why Google doesn't use JWT type access_tokens.  



