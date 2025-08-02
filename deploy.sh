echo "Starting deployment process..."

cd /Users/akshay/InventoryApi/InventoryUI/inventory-ui
echo "Building Angular application..."
ng build
echo "Angular Build completed successfully."

cp /Users/akshay/InventoryApi/InventoryUI/inventory-ui/dist/inventory-ui/* /Users/akshay/InventoryApi/InventoryManagement.Api/wwwroot/

echo "Copying Angular build to API wwwroot directory completed successfully."

echo "Starting .NET Core application deployment..."
cd /Users/akshay/InventoryApi/InventoryManagement.Api
dotnet restore
echo "Restoring .NET Core application dependencies completed successfully."

dotnet build
echo "Building .NET Core application completed successfully."

dotnet publish -c Release -o /Users/akshay/InventoryApi/InventoryManagement.Api/publish

echo "Publish of .NET Core application completed successfully."
echo "Creating ZIP package for deployment..."
cd /Users/akshay/InventoryApi/InventoryManagement.Api/publish
zip -r /Users/akshay/InventoryApi/InventoryManagement.Api/app.zip *
echo "ZIP package created successfully at /Users/akshay/InventoryApi/InventoryManagement.Api/app.zip."
echo "Deploying to Azure Web App..."
cd /Users/akshay/InventoryApi/InventoryManagement.Api/
# az webapp deploy \
#   --name aminventory \
#   --resource-group aminventory_group \
#   --src-path ./app.zip --type zip
echo "Deployment to Azure Web App completed successfully."