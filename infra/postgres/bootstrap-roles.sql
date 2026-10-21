\set ON_ERROR_STOP on

\getenv migrator_password STUDYAI_DB_MIGRATOR_PASSWORD
\getenv app_password STUDYAI_DB_APP_PASSWORD
\getenv worker_password STUDYAI_DB_WORKER_PASSWORD
\getenv readonly_password STUDYAI_DB_READONLY_PASSWORD

SELECT :'migrator_password' <> '' AS migrator_password_present,
       :'app_password' <> '' AS app_password_present,
       :'worker_password' <> '' AS worker_password_present,
       :'readonly_password' <> '' AS readonly_password_present
\gset

\if :migrator_password_present
\else
  \echo 'STUDYAI_DB_MIGRATOR_PASSWORD is required.'
  \quit 1
\endif
\if :app_password_present
\else
  \echo 'STUDYAI_DB_APP_PASSWORD is required.'
  \quit 1
\endif
\if :worker_password_present
\else
  \echo 'STUDYAI_DB_WORKER_PASSWORD is required.'
  \quit 1
\endif
\if :readonly_password_present
\else
  \echo 'STUDYAI_DB_READONLY_PASSWORD is required.'
  \quit 1
\endif

SELECT format(
    'CREATE ROLE %I WITH LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE INHERIT NOBYPASSRLS PASSWORD %L',
    role_name,
    role_password)
FROM (VALUES
    ('studyai_migrator', :'migrator_password'),
    ('studyai_app', :'app_password'),
    ('studyai_worker', :'worker_password'),
    ('studyai_ro', :'readonly_password')
) AS requested_roles(role_name, role_password)
WHERE NOT EXISTS (
    SELECT 1
    FROM pg_roles
    WHERE rolname = requested_roles.role_name
)
\gexec

SELECT format(
    'ALTER ROLE %I WITH LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE INHERIT NOBYPASSRLS PASSWORD %L',
    role_name,
    role_password)
FROM (VALUES
    ('studyai_migrator', :'migrator_password'),
    ('studyai_app', :'app_password'),
    ('studyai_worker', :'worker_password'),
    ('studyai_ro', :'readonly_password')
) AS requested_roles(role_name, role_password)
\gexec

\unset migrator_password
\unset app_password
\unset worker_password
\unset readonly_password

\echo 'StudyAI database roles were pre-created or rotated from approved environment configuration.'
