@description('The location of the resource group and the location in which all resurces would be created')
param location string = resourceGroup().location
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
  name: 'fsdmz${suffix}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

resource mng_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' = {
  name: 'fsidentity${suffix}'
  location: location
}


module AKV 'keyvault.bicep' = {
  name: 'keyVault'
  params: {
    key_vault_name: '${key_vault_name}${suffix}'
    userObjId : userObjId
    location: location
    managedIdentity: mng_identity.properties.principalId 
    dmzSecName : 'storagecs'
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

@description('Name of the log analytics workspace')
param logAnalyticsName string

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: '${logAnalyticsName}${suffix}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

@description('Name of the Container App Environment')
param containerAppEnvName string

param azureadinstance string

param azureaddomain string

param azureadappreg string

module ACA 'aca.bicep' = {
  name: 'aca'
  params: {
    location: location
    name: '${containerAppEnvName}${suffix}'
    customerId: logAnalytics.properties.customerId
    sharedKey: logAnalytics.listKeys().primarySharedKey
    managedIdentityName: mng_identity.id
    targetPort: 7267
    fsApiContainerName: 'yodobrin/file-service-sample:latest'
    registryLoginServer: 'ghcr.io'
    keyvaultname: AKV.outputs.key_vault_name
    azureADManagedIdentityClientId: mng_identity.properties.clientId
    azureadinstance: azureadinstance
    azureaddomain:azureaddomain
    azureadappreg: azureadappreg
  }
}

output fqdn string = ACA.outputs.fqdn


