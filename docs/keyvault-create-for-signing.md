```
az account set --subscription="39ac48fb-fea0-486a-ba84-e0ae9b06c663
az ad sp create-for-rbac -n heidi-oauth2
{
  "appId": "145dbce3-8267-4b43-b82f-454369b02dea",
  "displayName": "heidi-oauth2",
  "name": "http://heidi-oauth2",
  "password": "b5b5b3b0-7954-4c7f-9533-a93578b12707",
  "tenant": "3b217a9b-6c58-428b-b022-5ad741ce2016"
}
az login --service-principal -u 145dbce3-8267-4b43-b82f-454369b02dea  -p b5b5b3b0-7954-4c7f-9533-a93578b12707 --tenant 3b217a9b-6c58-428b-b022-5ad741ce2016
az group create -l eastus2 -n rg-heidi-oauth2
az keyvault create --resource-group rg-heidi-oauth2 --name kv-heidi-oauth2
az keyvault set-policy --name kv-heidi-oauth2 --spn 145dbce3-8267-4b43-b82f-454369b02dea --key-permissions backup create decrypt delete encrypt get import list purge recover restore sign unwrapKey update verify wrapKey --certificate-permissions backup create delete deleteissuers get getissuers import list listissuers managecontacts manageissuers purge recover restore setissuers update --secret-permissions backup delete get list purge recover restore set
$env:ARM_CLIENT_ID="145dbce3-8267-4b43-b82f-454369b02dea"
$env:ARM_CLIENT_SECRET="b5b5b3b0-7954-4c7f-9533-a93578b12707"
$env:ARM_SUBSCRIPTION_ID="39ac48fb-fea0-486a-ba84-e0ae9b06c663"
$env:ARM_TENANT_ID="3b217a9b-6c58-428b-b022-5ad741ce2016"
$env:ARM_SUBSCRIPTION_ID=$(az account show --query id | xargs)
$env:ARM_TENANT_ID=$(az account show --query tenantId | xargs)
```
