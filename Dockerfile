FROM mcr.microsoft.com/dotnet/runtime:6.0

ARG TARGETPLATFORM

ADD build/${TARGETPLATFORM} /opt/aentp

VOLUME ["/data"]

WORKDIR /data

ENTRYPOINT ["/opt/aentp/Ae.Ntp.Console"]
