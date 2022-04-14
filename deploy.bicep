// Following services will be deployed:
//  - Azure Digital Twins Instance

// Parameters
param location string = resourceGroup().location
param resourceNameSuffix string = uniqueString(resourceGroup().id)
param principalId string
param principalType string = 'User'

// Azure Digital Twins Instance
resource adt 'Microsoft.DigitalTwins/digitalTwinsInstances@2021-06-30-preview' = {
  name: 'adt-${resourceNameSuffix}'
  location: location
}

// Role Definition: Azure Digital Twins Data Owner
resource roleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: adt
  name: 'bcd981a7-7f74-457b-83e1-cceb9e632ffe' // Azure Digital Twins Data Owner
}

// Role Assignment: Logined user's user principal -> Azure Digital Twin Instance (Azure Digital Twins Data Owner)
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroup().id, adt.id, roleDefinition.id)
  properties: {
    roleDefinitionId: roleDefinition.id
    principalId: principalId
    principalType: principalType
  }
}

// Output: Name of created Digital Twins Instance
output adtInstanceName string = adt.name
