param appServiceName string

@description('The client ID of the Microsoft Entra application.')
param entraClientId string
@description('The client ID of the GitHub application.')
param gitHubClientId string

param openIdIssuer string

@allowed([
  'AllowAnonymous'
  'RedirectToLoginPage'
  'Return401'
  'Return403'
])
param unauthenticatedClientAction string = 'RedirectToLoginPage'

resource appService 'Microsoft.Web/sites@2022-03-01' existing = {
  name: appServiceName
}

resource appServiceAuthConfig 'Microsoft.Web/sites/config@2022-03-01' = {
  name: 'authsettingsV2'
  parent: appService
  properties: {
    globalValidation: {
      requireAuthentication: true
      unauthenticatedClientAction: unauthenticatedClientAction
      redirectToProvider: 'AzureActiveDirectory'
    }
    identityProviders: {
      azureActiveDirectory: {
        enabled: true
        registration: {
          clientId: entraClientId
          openIdIssuer: openIdIssuer
        }
        validation: {
          defaultAuthorizationPolicy: {
            allowedApplications: [
              entraClientId
            ]
          }
        }
      }
      gitHub: gitHubClientId != '' ? {
        enabled: true
        registration: {
          clientId: gitHubClientId
          clientSecretSettingName: 'GITHUB_PROVIDER_AUTHENTICATION_SECRET'
        }
      } : null
    }
    login: {
      tokenStore: {
        enabled: true
      }
    }
  }
}
