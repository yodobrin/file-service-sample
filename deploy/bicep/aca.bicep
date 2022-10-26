
@description('Provide a location for the registry.')
param location string = resourceGroup().location

@description('Name of the Container App Environment')
param name string

param sharedKey string

param customerId string

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2022-06-01-preview' = {
  name: name
  location: location 
    properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: customerId
        sharedKey: sharedKey
      }
    }
  }
}

@description('Name of the TodoApi Container App')
param fsApiContainerName string
param targetPort int
param managedIdentityName string 

param keyvaultname string
param azureADManagedIdentityClientId string

param registryLoginServer string


resource ContainerApp 'Microsoft.App/containerApps@2022-06-01-preview' = {
  name: 'file-server'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityName}':{}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnvironment.id
    
    configuration: {
      ingress: {
        external: true
        targetPort: targetPort
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
    }
    template: {
      containers: [
        {
          name: 'fileserver' 
          image: '${registryLoginServer}/${fsApiContainerName}' 
          resources: {
            cpu: '0.5'
            memory: '1Gi'
          } 
          env: [
            {
              name: 'keyvault'
              value: keyvaultname
            }
            {
              name: 'AzureADManagedIdentityClientId'
              value: azureADManagedIdentityClientId
            }
            {
              name: 'access_period_minutes'
              value: '30'
            }
          ]        
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
      }
    }
  }
}

output fqdn string = ContainerApp.properties.configuration.ingress.fqdn
