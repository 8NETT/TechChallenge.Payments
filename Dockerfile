# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and restore
COPY ["TechChallenge.Payments.sln", "./"]
COPY ["TechChallenge.Payments/TechChallenge.Payments.csproj", "TechChallenge.Payments/"]
COPY ["TechChallenge.Payments.Tests/TechChallenge.Payments.Tests.csproj", "TechChallenge.Payments.Tests/"]
RUN dotnet restore "TechChallenge.Payments.sln"

# Copy everything and build
COPY . .
RUN dotnet test "TechChallenge.Payments.sln" --no-restore --verbosity normal
RUN dotnet publish "TechChallenge.Payments/TechChallenge.Payments.csproj" -c Release -o /app/publish

# Final runtime image: use Azure Functions isolated worker base image
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated9.0 AS final

# (Optional) install New Relic agent if you still need it
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
    && echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
    && wget https://download.newrelic.com/548C16BF.gpg \
    && apt-key add 548C16BF.gpg \
    && apt-get update \
    && apt-get install -y 'newrelic-dotnet-agent' \
    && rm -rf /var/lib/apt/lists/*

# New Relic env vars (optional)
ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so

# Copy publish output
WORKDIR /home/site/wwwroot
COPY --from=build /app/publish .