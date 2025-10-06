FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copiar arquivos de projeto e restaurar dependencias
COPY ["TechChallenge.Payments.sln", "./"]
COPY ["TechChallenge.Payments/TechChallenge.Payments.csproj", "TechChallenge.Payments/"]
COPY ["TechChallenge.Payments.Tests/TechChallenge.Payments.Tests.csproj", "TechChallenge.Payments.Tests/"]
RUN dotnet restore "TechChallenge.Payments.sln"

# Copiar o restante dos arquivos
COPY . .

# Executar testes
RUN dotnet test "TechChallenge.Payments.sln" --no-restore --verbosity normal

# Publicar a aplicacao
FROM build AS publish
RUN dotnet publish "TechChallenge.Payments/TechChallenge.Payments.csproj" -c Release -o /app/publish

# Imagem final
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Instalar o agente do New Relic (opcional, mantido do original)
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
&& echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
&& wget https://download.newrelic.com/548C16BF.gpg \
&& apt-key add 548C16BF.gpg \
&& apt-get update \
&& apt-get install -y 'newrelic-dotnet-agent' \
&& rm -rf /var/lib/apt/lists/*

# Habilitar o agente do New Relic
ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so

WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TechChallenge.Payments.dll"]
