ARG PG_VERSION=16.5
FROM postgres:${PG_VERSION} AS builder

ENV DEBIAN_FRONTEND=noninteractive

# Build pgvector in a builder stage
RUN set -eux \
 && apt-get update \
 && apt-get install -y --no-install-recommends ca-certificates build-essential git make gcc \
 # Try to install matching server dev package for the PG_MAJOR provided by the official image, fall back to postgresql-server-dev-all
 && (apt-get install -y --no-install-recommends "postgresql-server-dev-${PG_MAJOR}" || apt-get install -y --no-install-recommends postgresql-server-dev-all) \
 # Clone a pinned pgvector release (shallow)
 && git clone --depth=1 --branch v0.7.0 https://github.com/pgvector/pgvector.git /tmp/pgvector \
 && cd /tmp/pgvector && make \
 # install into staging directory so we can copy only runtime files
 && make install DESTDIR=/build-out \
 && rm -rf /tmp/pgvector \
 # remove build deps; ignore failures removing dev package if it wasn't installed
 && apt-get purge -y --auto-remove build-essential git make gcc || true \
 && apt-get purge -y --auto-remove "postgresql-server-dev-${PG_MAJOR}" postgresql-server-dev-all || true \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*

# Final runtime image: copy the compiled extension artifacts
FROM postgres:${PG_VERSION}

COPY --from=builder /build-out/usr/lib/postgresql /usr/lib/postgresql
COPY --from=builder /build-out/usr/share/postgresql /usr/share/postgresql

# keep default Postgres runtime behavior; user can add init SQL files in /docker-entrypoint-initdb.d
