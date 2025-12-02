#!/bin/bash

version="10.0.4"

rm -rf build artifacts
dotnet build -c Release /property:Version=$version

apiKey=$NUGET_API_KEY
source="https://api.nuget.org/v3/index.json"

nuget push artifacts/Storm.Api.$version.nupkg -Source $source -ApiKey $apiKey
nuget push artifacts/Storm.Api.Dtos.$version.nupkg -Source $source -ApiKey $apiKey
nuget push artifacts/Storm.Api.SourceGenerators.$version.nupkg -Source $source -ApiKey $apiKey