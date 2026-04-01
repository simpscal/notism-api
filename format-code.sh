echo "Running dotnet format: whitespace, style, and analyzers"
echo ""
dotnet format Notism.sln whitespace
dotnet format Notism.sln style
dotnet format Notism.sln analyzers
echo ""
echo "Formatting complete!"
exit 0