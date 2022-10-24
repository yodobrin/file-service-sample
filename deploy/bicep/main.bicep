@description('The location of the resource group and the location in which all resurces would be created')
param location string = resourceGroup().location
// @description('The resource group name')
// param rg_name string = resourceGroup().name 
@description('The name of the keyvault')
param key_vault_name string 
@description('the acr name')
param acr_name string
@description('The object id of the user executing this bicep')
param userObjId string
@description('The suffix added to all resources to be created')
param suffix string 



// storage for diagniostic 
resource dmz_storage 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: 'fs${suffix}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

resource mng_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' = {
  name: 'fsidentity${suffix}'
  location: location
  tags: {
    tagName1: 'tagValue1'
    tagName2: 'tagValue2'
  }
}


// param vault_version string
// param tenantId string = subscription().tenantId
// param subscriptionId string = subscription().subscriptionId
param dmzSecName string





module AKV 'keyvault.bicep' = {
  name: 'keyVault'
  params: {
    key_vault_name: '${key_vault_name}${suffix}'
    userObjId : userObjId
    location: location
    managedIdentity: mng_identity.properties.principalId 
    dmzSecName : dmzSecName
    dmzSecVal: 'DefaultEndpointsProtocol=https;AccountName=${dmz_storage.name};AccountKey=${dmz_storage.listKeys().keys[0].value};EndpointSuffix=core.windows.net'    
  }
}

module ACR 'acr.bicep' = {
  name: 'acr'
  params: {
    acrName: '${acr_name}${suffix}'
    location: location
    mngIdentity : mng_identity.properties.principalId 
  }
}


//az deployment group create --resource-group fs-test-bicep --template-file main.bicep --parameters @param.json

