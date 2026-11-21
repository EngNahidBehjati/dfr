ARG SDK_BASE_IMAGE=starrepo.mohaymen.ir/sdk:8.0-noble
ARG RUNTIME_BASE_IMAGE=starrepo.mohaymen.ir/diap-aspnet:8.0-alpine3.18-amd64
FROM ${SDK_BASE_IMAGE} AS build

ARG username=none
ARG password=none
ARG SOLUTION_DIRECTORY=DataFlowRunner
ARG build_mode=Debug
ARG tagversion=0.0.0.0
ARG NUGET_URL=https://analytics.mohaymen.ir/nexus/repository/nuget-group/

RUN dotnet nuget remove source nuget.org || true && \
    dotnet nuget remove source "Microsoft and .NET" || true && \
    if [ "$username" != "none" ] && [ "$password" != "none" ]; then \
      dotnet nuget add source "$NUGET_URL" --name nuget-group --username "$username" --password "$password" --store-password-in-clear-text ; \
    else \
      dotnet nuget add source "$NUGET_URL" --name nuget-group ; \
    fi && \
    dotnet nuget list source --format detailed


COPY NuGet.Config opt
COPY Directory.Build.props opt

WORKDIR /opt/$SOLUTION_DIRECTORY

COPY ["DataFlowRunner/Directory.Build.props","."]
COPY ["DataFlowRunner/DataFlowRunner.sln","."]
COPY ["DataFlowRunner/DataFlowRunner/DataFlowRunner.csproj","DataFlowRunner/"]
COPY ["DataFlowRunner/DataFlowRunner.Tests/DataFlowRunner.Tests.csproj","DataFlowRunner.Tests/"]
COPY ["DataFlowRunner/DataFlowRunner.Integration.Tests/DataFlowRunner.Integration.Tests.csproj","DataFlowRunner.Integration.Tests/"]

ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
ENV NUGET_XMLDOC_MODE=skip
ENV NUGET_CERT_REVOCATION_MODE=offline
ENV NUGET_PACKAGES=/root/.nuget/packages

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages dotnet restore --no-cache

COPY $SOLUTION_DIRECTORY .

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet build --no-restore -c $build_mode /p:Version=$tagversion -o output 
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet test --no-restore --logger "trx;LogFileName=/opt/$SOLUTION_DIRECTORY/output/testResults.trx" 

RUN dotnet nuget remove source nuget-group
RUN rm -rf $(ls --hide=output)

FROM ${RUNTIME_BASE_IMAGE} AS runtime

ARG ug_id=200000
ARG SOLUTION_DIRECTORY=DataFlowRunner
ENV service_port=58095
ENV service_executable=DataFlowRunner.dll

USER root

WORKDIR /opt/$SOLUTION_DIRECTORY
COPY --from=build --chown=$ug_id:$ug_id /opt/$SOLUTION_DIRECTORY .
ENV ASPNETCORE_URLS=http://+:$service_port

USER $ug_id

EXPOSE $service_port

WORKDIR /opt/$SOLUTION_DIRECTORY/output
CMD dotnet $service_executable
