# Define the region and resource group name to deploy
region="southeastasia"
resourceGroupName=$1
bicepFileName="deploy.bicep"

# Get Object ID of logined AAD user to let .NET app access to Azure Digital Twin Instance by Managed ID
loginUserName=`az account show --query "user.name" --output tsv`
userPrincipalId=`az ad user show --id $loginUserName --query "objectId" --output tsv`

# Create a resource group
az group create \
   --location $region \
   --resource-group $resourceGroupName

# Deploy the Bicep template
az deployment group create \
   --resource-group $resourceGroupName \
   --template-file $bicepFileName \
   --parameters principalId=$userPrincipalId

# Get created Azure Digital Twin instance name
adtInstanceName=`az deployment group show --name 'deploy' --resource-group $resourceGroupName --query 'properties.outputs.adtInstanceName.value' --output tsv`

# Run the .NET app to register sample models, twins, relations to created Azure Digital Twin instance
cd GenerateTwinComponents
dotnet run --instance-name $adtInstanceName
cd ..

# Show the URL for Azure Digital Twins Explorer
echo 'Please access to Azure Digital Twins Explorer to check registered components:'
echo 'https://explorer.digitaltwins.azure.net'
