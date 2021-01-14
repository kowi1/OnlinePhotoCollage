FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_10.x | bash \
    && apt-get install nodejs -yq
WORKDIR /app

# Copy csproj and restore as distinct layers
#COPY ./gtbweb/gtbweb/*.csproj ./
#RUN dotnet restore

COPY *.csproj ./
RUN dotnet restore
COPY . ./



RUN dotnet publish -c Release -o out
FROM mcr.microsoft.com/dotnet/aspnet:5.0
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_10.x | bash \
    && apt-get install nodejs -yq
WORKDIR /app
COPY --from=build-env /app/out .

# Copy everything else and build
##COPY . ./
##RUN dotnet publish gtbweb  -c Release -o out

# Build runtime image
##FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
##WORKDIR /app
##COPY --from=build-env /app/gtbweb/gtbweb/out .

#ENV ASPNETCORE_URLS=http://*:$PORT
#ENTRYPOINT ["dotnet", "gtbweb.dll","--server.urls", "http://*:"]
CMD ASPNETCORE_URLS=http://*:$PORT dotnet OnlinePhotoCollage.dll


