#!/bin/bash

version="10.0.8"

rm -rf build artifacts
dotnet build -c Release /property:Version=$version

apiKey=$NUGET_API_KEY
source="https://api.nuget.org/v3/index.json"

dotnet nuget push artifacts/Storm.Api.$version.nupkg --source $source --api-key $apiKey
dotnet nuget push artifacts/Storm.Api.Dtos.$version.nupkg --source $source --api-key $apiKey
dotnet nuget push artifacts/Storm.Api.SourceGenerators.$version.nupkg --source $source --api-key $apiKey
dotnet nuget push artifacts/Storm.Api.Tools.$version.nupkg --source $source --api-key $apiKey