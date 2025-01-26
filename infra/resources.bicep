@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@description('The location used for all deployed resources')
param location string = resourceGroup().location

@description('Tags that will be applied to all resources')
param tags object = {}


param containerAppExists bool
@secure()
param containerAppDefinition object

@description('Id of the user or app to assign application roles')
param principalId string

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = uniqueString(subscription().id, resourceGroup().id, location)

// Monitor application with Azure Monitor
module monitoring 'br/public:avm/ptn/azd/monitoring:0.1.0' = {
  name: 'monitoring'
  params: {
    logAnalyticsName: '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: '${abbrs.insightsComponents}${resourceToken}'
    applicationInsightsDashboardName: '${abbrs.portalDashboards}${resourceToken}'
    location: location
    tags: tags
  }
}

// Storage account
module storageAccount 'br/public:avm/res/storage/storage-account:0.15.0' = {
  name: 'storageAccount'
  params: {
    name: '${abbrs.storageStorageAccounts}${resourceToken}'
    kind: 'StorageV2'
    location: location
    tags: tags
    skuName: 'Standard_LRS'
    blobServices: {
      containers: [
        {
          name: 'token-store'
          publicAccess: 'None'
        }
      ]
    }
  }
}

// Container registry
module containerRegistry 'br/public:avm/res/container-registry/registry:0.6.0' = {
  name: 'registry'
  params: {
    name: '${abbrs.containerRegistryRegistries}${resourceToken}'
    location: location
    acrAdminUserEnabled: true
    tags: tags
    exportPolicyStatus: 'enabled'
    publicNetworkAccess: 'Enabled'
    roleAssignments:[
      {
        principalId: containerAppIdentity.outputs.principalId
        principalType: 'ServicePrincipal'
        roleDefinitionIdOrName: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
      }
      {
        principalId: webAppIdentity.outputs.principalId
        principalType: 'ServicePrincipal'
        roleDefinitionIdOrName: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
      }
    ]
  }
}

// Container apps environment
module containerAppsEnvironment 'br/public:avm/res/app/managed-environment:0.8.1' = {
  name: 'container-apps-environment'
  params: {
    logAnalyticsWorkspaceResourceId: monitoring.outputs.logAnalyticsWorkspaceResourceId
    name: '${abbrs.appManagedEnvironments}${resourceToken}'
    location: location
    zoneRedundant: false
  }
}

// User-assigned identity for container app
module containerAppIdentity 'br/public:avm/res/managed-identity/user-assigned-identity:0.4.0' = {
  name: 'container-app-identity'
  params: {
    name: '${abbrs.managedIdentityUserAssignedIdentities}containerapp-${resourceToken}'
    location: location
  }
}

// Role assignment to user-assigned identity for container app
module containerAppIdentityRoleAssignment './modules/role-assignment.bicep' = {
  name: 'container-app-identity-role-assignment'
  params: {
    managedIdentityName: containerAppIdentity.outputs.name
    storageAccountName: storageAccount.outputs.name
    principalType: 'ServicePrincipal'
  }
}

// Fetch latest image from container registry
module containerAppFetchLatestImage './modules/fetch-container-image.bicep' = {
  name: 'container-app-fetch-image'
  params: {
    exists: containerAppExists
    name: 'easyauth-containerapp'
  }
}

var containerAppAppSettingsArray = filter(array(containerAppDefinition.settings), i => i.name != '')
var containerAppSecrets = map(filter(containerAppAppSettingsArray, i => i.?secret != null), i => {
  name: i.name
  value: i.value
  secretRef: i.?secretRef ?? take(replace(replace(toLower(i.name), '_', '-'), '.', '-'), 32)
})
var containerAppEnv = map(filter(containerAppAppSettingsArray, i => i.?secret == null), i => {
  name: i.name
  value: i.value
})

// Container app
module containerApp 'br/public:avm/res/app/container-app:0.11.0' = {
  name: 'container-app'
  params: {
    name: 'easyauth-containerapp'
    ingressTargetPort: 8080
    scaleMinReplicas: 1
    scaleMaxReplicas: 10
    secrets: {
      secureList: union([
      ],
      map(containerAppSecrets, secret => {
        name: secret.secretRef
        value: secret.value
      }))
    }
    containers: [
      {
        image: containerAppFetchLatestImage.outputs.?containers[?0].?image ?? 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
        name: 'main'
        resources: {
          cpu: json('0.5')
          memory: '1.0Gi'
        }
        env: union([
          {
            name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
            value: monitoring.outputs.applicationInsightsConnectionString
          }
          {
            name: 'AZURE_CLIENT_ID'
            value: containerAppIdentity.outputs.clientId
          }
          {
            name: 'PORT'
            value: '8080'
          }
        ],
        containerAppEnv,
        map(containerAppSecrets, secret => {
            name: secret.name
            secretRef: secret.secretRef
        }))
      }
    ]
    managedIdentities: {
      systemAssigned: false
      userAssignedResourceIds: [
        containerAppIdentity.outputs.resourceId
      ]
    }
    registries: [
      {
        server: containerRegistry.outputs.loginServer
        identity: containerAppIdentity.outputs.resourceId
      }
    ]
    environmentResourceId: containerAppsEnvironment.outputs.resourceId
    location: location
    tags: union(tags, { 'azd-service-name': 'easyauth-containerapp' })
  }
}

// App service plan
module webAppServerFarm 'br/public:avm/res/web/serverfarm:0.4.0' = {
  name: 'web-app-serverfarm'
  params: {
    name: '${abbrs.webServerFarms}${resourceToken}'
    location: location
    tags: tags
    reserved: true
    skuName: 'B1'
    skuCapacity: 1
  }
}

// User-assigned identity for web app
module webAppIdentity 'br/public:avm/res/managed-identity/user-assigned-identity:0.2.1' = {
  name: 'web-app-identity'
  params: {
    name: '${abbrs.managedIdentityUserAssignedIdentities}webapp-${resourceToken}'
    location: location
  }
}

// Web ppp
module webApp 'br/public:avm/res/web/site:0.12.1' = {
  name: 'web-app'
  params: {
    name: '${abbrs.webSitesAppService}${resourceToken}'
    kind: 'app,linux'
    serverFarmResourceId: webAppServerFarm.outputs.resourceId
    location: location
    tags: union(tags, { 'azd-service-name': 'easyauth-webapp' })
    managedIdentities: {
      systemAssigned: false
      userAssignedResourceIds: [
        webAppIdentity.outputs.resourceId
      ]
    }
    siteConfig: {
      appSettings: [
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
      ]
      ftpsState: 'FtpsOnly'
      linuxFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: true
      minTlsVersion: '1.2'
    }
    basicPublishingCredentialsPolicies: [
      {
        name: 'scm'
        allow: false
      }
      {
        name: 'ftp'
        allow: false
      }
    ]
  }
}

// EasyAuth
var issuer = '${environment().authentication.loginEndpoint}${tenant().tenantId}/v2.0'

// App registration
module appRegistration './modules/app-registration.bicep' = {
  name: 'appRegistration'
  params: {
    appName: 'spn-${environmentName}'
    issuer: issuer
    containerAppIdentityId: containerAppIdentity.outputs.principalId
    webAppIdentityId: webAppIdentity.outputs.principalId
    containerAppEndpoint: 'https://${containerApp.outputs.fqdn}'
    webAppEndpoint: 'https://${webApp.outputs.defaultHostname}'
  }
}

// EasyAuth configuration for container app
module containerAppAuthConfig './modules/containerapps-authconfigs.bicep' = {
  name: 'container-app-auth-config'
  params: {
    containerAppName: containerApp.outputs.name
    managedIdentityName: containerAppIdentity.outputs.name
    storageAccountName: storageAccount.outputs.name
    clientId: appRegistration.outputs.appId
    openIdIssuer: issuer
    unauthenticatedClientAction: 'AllowAnonymous'
  }
}

// EasyAuth configuration for web app
module webAppAuthConfig './modules/appservice-authconfigs.bicep' = {
  name: 'web-app-auth-config'
  params: {
    appServiceName: webApp.outputs.name
    clientId: appRegistration.outputs.appId
    openIdIssuer: issuer
    unauthenticatedClientAction: 'AllowAnonymous'
  }
}

// KeyVault to store secrets
module keyVault 'br/public:avm/res/key-vault/vault:0.11.1' = {
  name: 'keyvault'
  params: {
    name: '${abbrs.keyVaultVaults}${resourceToken}'
    location: location
    tags: tags
    enableRbacAuthorization: false
    accessPolicies: [
      {
        objectId: principalId
        permissions: {
          secrets: [ 'get', 'list' ]
        }
      }
      {
        objectId: containerAppIdentity.outputs.principalId
        permissions: {
          secrets: [ 'get', 'list' ]
        }
      }
      {
        objectId: webAppIdentity.outputs.principalId
        permissions: {
          secrets: [ 'get', 'list' ]
        }
      }
    ]
    secrets: [
    ]
  }
}

output AZURE_PRINCIPAL_ID string = appRegistration.outputs.appId

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerRegistry.outputs.loginServer

output AZURE_KEY_VAULT_ENDPOINT string = keyVault.outputs.uri
output AZURE_KEY_VAULT_NAME string = keyVault.outputs.name

output AZURE_CONTAINERAPP_ID string = containerApp.outputs.resourceId
output AZURE_WEBAPP_ID string = webApp.outputs.resourceId

output AZURE_CONTAINERAPP_URL string = 'https://${containerApp.outputs.fqdn}'
output AZURE_WEBAPP_URL string = 'https://${webApp.outputs.defaultHostname}'
