targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

param containerAppExists bool
@secure()
param containerAppDefinition object

@description('The client ID of the GitHub application.')
param gitHubClientId string
@description('The client secret of the GitHub application.')
@secure()
param gitHubClientSecret string

@description('Id of the user or app to assign application roles')
param principalId string

// Tags that should be applied to all resources.
// 
// Note that 'azd-service-name' tags should be applied separately to service host resources.
// Example usage:
//   tags: union(tags, { 'azd-service-name': <service name in azure.yaml> })
var tags = {
  'azd-env-name': environmentName
}

// Organize resources in a resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module resources 'resources.bicep' = {
  scope: rg
  name: 'resources'
  params: {
    environmentName: environmentName
    location: location
    tags: tags
    gitHubClientId: gitHubClientId
    gitHubClientSecret: gitHubClientSecret
    principalId: principalId
    containerAppExists: containerAppExists
    containerAppDefinition: containerAppDefinition
  }
}

output AZURE_PRINCIPAL_ID string = resources.outputs.AZURE_PRINCIPAL_ID

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = resources.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT

output AZURE_KEY_VAULT_ENDPOINT string = resources.outputs.AZURE_KEY_VAULT_ENDPOINT
output AZURE_KEY_VAULT_NAME string = resources.outputs.AZURE_KEY_VAULT_NAME

output AZURE_CONTAINERAPP_ID string = resources.outputs.AZURE_CONTAINERAPP_ID
output AZURE_WEBAPP_ID string = resources.outputs.AZURE_WEBAPP_ID

output AZURE_CONTAINERAPP_URL string = resources.outputs.AZURE_CONTAINERAPP_URL
output AZURE_WEBAPP_URL string = resources.outputs.AZURE_WEBAPP_URL
