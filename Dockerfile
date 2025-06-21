# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com / dotnet / sdk:7.0 AS build
WORKDIR /app

# Copy everything
COPY . ./

# Restore dependencies and build
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Use runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Tell Docker what port the app will listen on
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "NotesApp.dll"]
