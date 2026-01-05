-- Executed only when the database is first initialized (empty volume)
-- The entrypoint runs these scripts using psql as the 'postgres' user against the default database.
-- Create the extension in the database specified by POSTGRES_DB environment variable (OnePlatform).

CREATE EXTENSION IF NOT EXISTS vector;
