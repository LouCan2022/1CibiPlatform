#PRODUCTION
# Stage 1: Build Blazor WASM
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file and restore dependencies
COPY FrontendWebassembly.csproj ./
RUN dotnet restore "./FrontendWebassembly.csproj"

# Copy all source files and publish
COPY . ./
RUN dotnet publish "./FrontendWebassembly.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 2: Serve with Nginx
FROM nginx:alpine AS final

# Remove default content
RUN rm -rf /usr/share/nginx/html/*

# Copy Blazor published files
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# Copy custom Nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 8080 inside container
EXPOSE 8080

# Start Nginx in foreground
CMD ["nginx", "-g", "daemon off;"]
