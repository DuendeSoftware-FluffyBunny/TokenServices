# Arbitrary Token
This is an extension grant where the OAuth2 server will mint a token with anything you would like in it.  The biggest reason to do this is that the lifetime managment of an OAuth2 token should be a commodity at this point.  
The OAuth2 service can mint a JWT or a reference as well as a partner refresh_token.  The OAuth2 service will take care of rotating signing keys per tenant, as well as all storage requirements for refrence and refresh_tokens.  

You get all OAuth2 features like introspection, refresh and revoke.  

```
POST /{{tenant}}/connect/token HTTP/1.1
Host: localhost:7001
Content-Type: application/x-www-form-urlencoded
Authorization: Basic YTFjZTE5N2ItZGQxMy00M2EwLTgzNzYtN2I3NjIzMDRmZGQ1OkRxekVWTUJza1JPM2x6N0o2R2paSEoxZkVOcFNRR2NF
Content-Length: 594

grant_type=arbitrary_token&subject=66C8C5A139F65007808EF00716203B09B5C157C3&issuer=https://accounts.google.com&client_id=arbitrary-resource-owner-client&client_secret=secret&arbitrary_claims={"amr":["password","homer"],"aud":["aud1","aud2"],"scope":["scope1","offline_access"],"azp":["azp1","azp2"],"role": ["admin","limited"],"query": ["dashboard", "licensing"],"seatId": ["8c59ec41-54f3-460b-a04e-520fc5b9973d"],"piid": ["2368d213-d06c-4c2a-a099-11c34adc3579"]}&arbitrary_json={"tip":{"dog":"cat","rats":["a"]},"top":{"dog":"cat","rats":["a"]} }&access_token_lifetime=29&access_token_type=jwt
```

|Argument |Description  | 
--- | --- |
|grant_type|(required) arbitrary_token|
|subject|(optional) The subject, usually a user id|
|issuer|(required) Usually the authority of this OAuth2 service|
|client_id|(Requied) This is required unless it is as a basic auth|
|client_secret|(Requied) This is required unless it is as a basic auth|
|arbitrary_claims|(optional) a json map of []string. i.e. {"amr":["password","homer"]}|
|access_token_lifetime|(optional) the access token lifetime is in the client configuration.  This value can be used to decrease it.  i.e. configured to be 100, but here we can pull it back to 50|
|access_token_type|(optional) this is configured in the client, but can be overriden here.  [jwt or reference]|
