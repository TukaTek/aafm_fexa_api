#!/bin/bash

# Azure deployment script for Fexa API Function App
set -e

# Variables - MODIFY THESE AS NEEDED
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

# 1. Check Resource Group exists
echo -e "${YELLOW}Checking Resource Group...${NC}"
az group show --name $RESOURCE_GROUP --output table || {
    echo -e "${RED}Resource group $RESOURCE_GROUP not found!${NC}"
    exit 1
}

# 2. Create Storage Account
echo -e "${YELLOW}Creating Storage Account...${NC}"
az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS

# 3. Create Function App Plan (Consumption)
echo -e "${YELLOW}Creating Function App Plan...${NC}"
az functionapp plan create \
  --name $PLAN_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Y1 \
  --is-linux

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
  --os-type Linux

# 5. Configure App Settings
echo -e "${YELLOW}Configuring App Settings...${NC}"
echo -e "${RED}IMPORTANT: You need to set your Fexa API credentials!${NC}"
echo "Enter your Fexa API Client ID:"
read CLIENT_ID
echo "Enter your Fexa API Client Secret:"
read -s CLIENT_SECRET

az functionapp config appsettings set \
  --name $FUNCTION_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    "FexaApi__BaseUrl=https://aafmapisandbox.fexa.io/" \
    "FexaApi__ClientId=$CLIENT_ID" \
    "FexaApi__ClientSecret=$CLIENT_SECRET" \
    "FexaApi__TokenEndpoint=/oauth/token" \
    "FexaApi__RetryCount=3" \
    "FexaApi__RetryDelayMilliseconds=1000" \
    "FUNCTIONS_WORKER_RUNTIME=dotnet-isolated"

# 6. Build the project
echo -e "${YELLOW}Building the project...${NC}"
cd src/Fexa.ApiClient.Function
dotnet clean
dotnet build --configuration Release

# 7. Publish the function
echo -e "${YELLOW}Publishing to Azure...${NC}"
func azure functionapp publish $FUNCTION_APP_NAME

# 8. Get the function URL
echo -e "${GREEN}Deployment complete!${NC}"
echo -e "${GREEN}Function App URL: https://$FUNCTION_APP_NAME.azurewebsites.net${NC}"
echo -e "${YELLOW}Getting function keys...${NC}"
FUNCTION_KEY=$(az functionapp function keys list \
  --name $FUNCTION_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --function-name GetWorkOrder \
  --query "default" -o tsv 2>/dev/null || echo "Use portal to get keys")

echo -e "${GREEN}===============================================${NC}"
echo -e "${GREEN}Deployment Summary:${NC}"
echo -e "Resource Group: $RESOURCE_GROUP"
echo -e "Function App: $FUNCTION_APP_NAME"
echo -e "URL: https://$FUNCTION_APP_NAME.azurewebsites.net"
echo -e "Swagger UI: https://$FUNCTION_APP_NAME.azurewebsites.net/api/swagger/ui"
echo -e ""
echo -e "${YELLOW}Example API calls:${NC}"
echo -e "curl https://$FUNCTION_APP_NAME.azurewebsites.net/api/health"
echo -e "curl https://$FUNCTION_APP_NAME.azurewebsites.net/api/vendors/2995?code=$FUNCTION_KEY"
echo -e "${GREEN}===============================================${NC}"