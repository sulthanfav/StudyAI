\set ON_ERROR_STOP on

DO $studyai$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'studyai_app') THEN
        RAISE EXCEPTION 'Required role studyai_app is absent; run the role bootstrap before applying runtime privileges.';
    END IF;
END
$studyai$;

ALTER ROLE studyai_app WITH
    LOGIN
    NOSUPERUSER
    NOCREATEDB
    NOCREATEROLE
    INHERIT
    NOREPLICATION
    NOBYPASSRLS;

SELECT format('REVOKE %I FROM studyai_app', role_name)
FROM (VALUES ('studyai_migrator'), ('studyai_worker'), ('studyai_ro')) AS elevated_roles(role_name)
WHERE pg_has_role('studyai_app', role_name, 'MEMBER')
\gexec

SELECT format('REVOKE ALL PRIVILEGES ON DATABASE %I FROM studyai_app', current_database())
\gexec
SELECT format('REVOKE TEMPORARY ON DATABASE %I FROM PUBLIC', current_database())
\gexec
SELECT format('GRANT CONNECT ON DATABASE %I TO studyai_app', current_database())
\gexec

REVOKE ALL PRIVILEGES ON SCHEMA public FROM studyai_app;
REVOKE ALL PRIVILEGES ON SCHEMA identity, ownership, workspace, knowledge, documents, ai, ops FROM studyai_app;
GRANT USAGE ON SCHEMA identity, ownership, workspace, knowledge, documents, ai TO studyai_app;

REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA identity, ownership, workspace, knowledge, documents, ai, ops FROM studyai_app;
REVOKE ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA identity, ownership, workspace, knowledge, documents, ai, ops FROM studyai_app;

GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE
    identity.users,
    identity.user_sessions,
    ownership.data_spaces,
    ownership.data_space_memberships,
    workspace.content_spaces,
    knowledge.knowledge_items,
    documents.storage_objects,
    documents.documents,
    documents.document_versions,
    documents.processing_runs,
    documents.document_chunks,
    documents.chunk_embeddings,
    ai.embedding_models
TO studyai_app;

\echo 'StudyAI runtime privileges were replaced with the reviewed explicit allowlist.'
