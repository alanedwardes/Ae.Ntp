FROM mcr.microsoft.com/dotnet/aspnet:8.0

ARG TARGETPLATFORM

ADD build/${TARGETPLATFORM} /opt/aentp

VOLUME ["/data"]

WORKDIR /data

ENTRYPOINT ["/opt/aentp/Ae.Ntp.Console"]
