FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /w
COPY . /w
RUN dotnet publish src/biorand-recv -c release -o /out -p:PublishSingleFile=true

FROM alpine
RUN apk add --no-cache libstdc++
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
COPY --from=build /out/biorand-recv /usr/bin/biorand-recv
RUN biorand-recv --version

CMD /usr/bin/biorand-recv
