@minLength(5)
@maxLength(50)
@description('Provide a globally unique name of your Azure Container Registry')
param acrName string //= 'acr${uniqueString(resourceGroup().id)}'

@description('Provide a location for the registry.')
param location string = resourceGroup().location

@description('Provide a tier of your Azure Container Registry.')
param acrSku string = 'Basic'

@description('The objid of the container app managed identity')
param mngIdentity string

var roleIdMapping = {
  AcrPull : '7f951dda-4ed3-4680-a7ca-43fe172d538d' //Pull artifacts from a container registry.	
}
	
resource acrResource 'Microsoft.ContainerRegistry/registries@2021-06-01-preview' = {
  name: acrName
  location: location
  sku: {
    name: acrSku
  }
  properties: {
    adminUserEnabled: false
  }
  
}

resource AcrPullAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(roleIdMapping.AcrPull,mngIdentity,acrResource.id)
  scope: acrResource
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleIdMapping.AcrPull)
    principalId: mngIdentity
    principalType: 'ServicePrincipal'
  }
}

@description('Output the login server property for later use')
output loginServer string = acrResource.properties.loginServer

//https://dev.to/willvelida/creating-and-provisioning-azure-container-apps-with-bicep-4gfb
