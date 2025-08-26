#!/bin/bash

# Azure deployment script for Fexa API Function App
set -e

# Variables
RESOURCE_GROUP="aafm_chatcmb"  # Using existing resource group
LOCATION="eastus2"  # Match existing resource group location
STORAGE_ACCOUNT="stfexaapi$(date +%s | tail -c 5)"  # Unique storage name
FUNCTION_APP_NAME="func-fexa-api-$(date +%s | tail -c 5)"  # Unique function name
PLAN_NAME="asp-fexa-api"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting Azure Function deployment...${NC}"
echo -e "${YELLOW}Function App Name: $FUNCTION_APP_NAME${NC}"
echo -e "${YELLOW}Storage Account: $STORAGE_ACCOUNT${NC}"

# 1. Check Resource Group exists
echo -e "${YELLOW}Checking Resource Group...${NC}"
az group show --name $RESOURCE_GROUP --output table

# 2. Create Storage Account
echo -e "${YELLOW}Creating Storage Account...${NC}"
az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS \
  --output table

# 3. Create Function App Plan (Consumption)
echo -e "${YELLOW}Creating Function App Plan...${NC}"
az functionapp plan create \
  --name $PLAN_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Y1 \
  --is-linux \
  --output table

# 4. Create Function App
echo -e "${YELLOW}Creating Function App...${NC}"
az functionapp create \
  --name $FUNCTION_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan $PLAN_NAME \
  --runtime dotnet-isolated \
  --runtime-version 8 \
  --functions-version 4 \
  --storage-account $STORAGE_ACCOUNT \
  --os-type Linux \
  --output table

# 5. Configure App Settings (with placeholder credentials)
echo -e "${YELLOW}Configuring App Settings...${NC}"
az functionapp config appsettings set \
  --name $FUNCTION_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    "FexaApi__BaseUrl=https://aafmapisandbox.fexa.io/" \
    "FexaApi__ClientId=REPLACE_WITH_YOUR_CLIENT_ID" \
    "FexaApi__ClientSecret=REPLACE_WITH_YOUR_CLIENT_SECRET" \
    "FexaApi__TokenEndpoint=/oauth/token" \
    "FexaApi__RetryCount=3" \
    "FexaApi__RetryDelayMilliseconds=1000" \
    "FUNCTIONS_WORKER_RUNTIME=dotnet-isolated" \
  --output none

# 6. Build the project
echo -e "${YELLOW}Building the project...${NC}"
cd src/Fexa.ApiClient.Function
dotnet clean
dotnet build --configuration Release

# 7. Publish the function
echo -e "${YELLOW}Publishing to Azure...${NC}"
func azure functionapp publish $FUNCTION_APP_NAME

# 8. Get the function URL and save deployment info
echo -e "${GREEN}Deployment complete!${NC}"
echo -e "${GREEN}Function App URL: https://$FUNCTION_APP_NAME.azurewebsites.net${NC}"

# Save deployment info
cat > ../../deployment-info.txt << EOF
===============================================
Deployment Information
Date: $(date)
===============================================
Resource Group: $RESOURCE_GROUP
Function App: $FUNCTION_APP_NAME
Storage Account: $STORAGE_ACCOUNT
Plan: $PLAN_NAME
URL: https://$FUNCTION_APP_NAME.azurewebsites.net
Swagger UI: https://$FUNCTION_APP_NAME.azurewebsites.net/api/swagger/ui

IMPORTANT: You need to update the API credentials!
-------------------------------------------------
Go to Azure Portal > Function App > Configuration > Application Settings
Update these values:
- FexaApi__ClientId
- FexaApi__ClientSecret

Or use Azure CLI:
az functionapp config appsettings set \\
  --name $FUNCTION_APP_NAME \\
  --resource-group $RESOURCE_GROUP \\
  --settings \\
    "FexaApi__ClientId=YOUR_CLIENT_ID" \\
    "FexaApi__ClientSecret=YOUR_CLIENT_SECRET"

Example API calls (after setting credentials):
curl https://$FUNCTION_APP_NAME.azurewebsites.net/api/health
EOF

cat ../../deployment-info.txt