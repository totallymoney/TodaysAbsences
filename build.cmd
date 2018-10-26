@echo off

dotnet build src\TodaysAbsences\TodaysAbsences.fsproj
dotnet build src\TodaysAbsences.Lambda\TodaysAbsences.Lambda.fsproj
dotnet build src\TodaysAbsences.Tests\TodaysAbsences.Tests.fsproj