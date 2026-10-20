-- StudyAI Campus / Tenant Isolation & Role Separation PoC Setup
-- For non-production isolated validation database: studyai_tenant_poc_20261020

-- 1. Create Roles -------------------------------------------------------------
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'studyai_migrator') THEN
        CREATE ROLE studyai_migrator WITH LOGIN PASSWORD 'MigratorSecret123!' CREATEDB;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'studyai_app') THEN
        CREATE ROLE studyai_app WITH LOGIN PASSWORD 'AppRuntimeSecret123!' NOSUPERUSER NOCREATEDB NOBYPASSRLS;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'studyai_worker') THEN
        CREATE ROLE studyai_worker WITH LOGIN PASSWORD 'WorkerSecret123!' NOSUPERUSER NOCREATEDB NOBYPASSRLS;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'studyai_ro') THEN
        CREATE ROLE studyai_ro WITH LOGIN PASSWORD 'ReadOnlySecret123!' NOSUPERUSER NOCREATEDB NOBYPASSRLS;
    END IF;
END $$;

-- 2. Grant Permissions --------------------------------------------------------
-- Revoke all direct CREATE privileges on schemas from public and app roles
REVOKE CREATE ON SCHEMA public FROM PUBLIC, studyai_app, studyai_worker, studyai_ro;

-- Grant USAGE on all application schemas
GRANT USAGE ON SCHEMA identity, ownership, workspace, knowledge, documents, research, ai, learning, writing, presentation, integrity, planner, billing, ops
    TO studyai_app, studyai_worker, studyai_ro;

-- Grant DML (SELECT, INSERT, UPDATE, DELETE) to studyai_app on application tables
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA identity, ownership, workspace, knowledge, documents, research, ai, learning, writing, presentation, integrity, planner, billing, ops
    TO studyai_app;

-- Grant USAGE on all sequences
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA identity, ownership, workspace, knowledge, documents, research, ai, learning, writing, presentation, integrity, planner, billing, ops
    TO studyai_app;

-- Grant worker scoped privileges (ops, documents, ai)
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA ops, documents, ai TO studyai_worker;
GRANT SELECT ON ALL TABLES IN SCHEMA identity, ownership, workspace, knowledge, learning TO studyai_worker;

-- Grant read-only to studyai_ro
GRANT SELECT ON ALL TABLES IN SCHEMA identity, ownership, workspace, knowledge, documents, research, ai, learning, writing, presentation, integrity, planner, billing, ops
    TO studyai_ro;

-- Explicitly ensure studyai_app cannot perform DDL (no CREATE in schemas)
REVOKE CREATE ON SCHEMA identity, ownership, workspace, knowledge, documents, research, ai, learning, writing, presentation, integrity, planner, billing, ops
    FROM studyai_app, studyai_worker, studyai_ro;

-- 3. Row-Level Security Policies on Representative High-Risk Tables ------------

-- Table 1: knowledge.knowledge_items
ALTER TABLE knowledge.knowledge_items ENABLE ROW LEVEL SECURITY;
ALTER TABLE knowledge.knowledge_items FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS tenant_isolation_policy ON knowledge.knowledge_items;
CREATE POLICY tenant_isolation_policy ON knowledge.knowledge_items
    FOR ALL TO studyai_app
    USING (data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid)
    WITH CHECK (data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid);

-- Table 2: documents.documents
ALTER TABLE documents.documents ENABLE ROW LEVEL SECURITY;
ALTER TABLE documents.documents FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS tenant_isolation_policy ON documents.documents;
CREATE POLICY tenant_isolation_policy ON documents.documents
    FOR ALL TO studyai_app
    USING (data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid)
    WITH CHECK (data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid);

-- Table 3: documents.document_chunks
ALTER TABLE documents.document_chunks ENABLE ROW LEVEL SECURITY;
ALTER TABLE documents.document_chunks FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS tenant_isolation_policy ON documents.document_chunks;
CREATE POLICY tenant_isolation_policy ON documents.document_chunks
    FOR ALL TO studyai_app
    USING (data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid)
    WITH CHECK (data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid);

-- Table 4: documents.chunk_embeddings
ALTER TABLE documents.chunk_embeddings ENABLE ROW LEVEL SECURITY;
ALTER TABLE documents.chunk_embeddings FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS tenant_isolation_policy ON documents.chunk_embeddings;
CREATE POLICY tenant_isolation_policy ON documents.chunk_embeddings
    FOR ALL TO studyai_app
    USING (data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid)
    WITH CHECK (data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid);

-- 4. Seed Data for Tenant Isolation Verification ------------------------------
-- Clean existing test seed in correct reverse dependency order
DELETE FROM documents.chunk_embeddings WHERE data_space_id IN ('0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-0000000000bb');
DELETE FROM documents.document_chunks WHERE data_space_id IN ('0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-0000000000bb');
DELETE FROM documents.processing_runs WHERE data_space_id IN ('0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-0000000000bb');
UPDATE documents.documents SET current_version_id = NULL WHERE data_space_id IN ('0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-0000000000bb');
DELETE FROM documents.document_versions WHERE data_space_id IN ('0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-0000000000bb');
DELETE FROM documents.documents WHERE data_space_id IN ('0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-0000000000bb');
DELETE FROM documents.storage_objects WHERE data_space_id IN ('0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-0000000000bb');
DELETE FROM knowledge.knowledge_items WHERE data_space_id IN ('0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-0000000000bb');
DELETE FROM workspace.content_spaces WHERE data_space_id IN ('0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-0000000000bb');
DELETE FROM ownership.data_spaces WHERE id IN ('0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-0000000000bb');
DELETE FROM identity.users WHERE id IN ('0192a000-0000-7000-8000-000000000001', '0192a000-0000-7000-8000-000000000002');

-- Users
INSERT INTO identity.users (id, email, display_name) VALUES
    ('0192a000-0000-7000-8000-000000000001', 'user.a@studyai.test', 'User A'),
    ('0192a000-0000-7000-8000-000000000002', 'user.b@studyai.test', 'User B');

-- Data Spaces
INSERT INTO ownership.data_spaces (id, owner_user_id, kind, name) VALUES
    ('0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-000000000001', 'personal', 'Personal Space A'),
    ('0192a000-0000-7000-8000-0000000000bb', '0192a000-0000-7000-8000-000000000002', 'personal', 'Personal Space B');

-- Content Spaces (Libraries)
INSERT INTO workspace.content_spaces (id, data_space_id, kind, title) VALUES
    ('0192a000-0000-7000-8000-000000000011', '0192a000-0000-7000-8000-0000000000aa', 'library', 'Library Space A'),
    ('0192a000-0000-7000-8000-000000000022', '0192a000-0000-7000-8000-0000000000bb', 'library', 'Library Space B');

-- Embedding Model (3 dimensions for lightweight testing)
INSERT INTO ai.embedding_models (id, provider, model_key, dimensions)
VALUES ('0192a000-0000-7000-8000-000000000099', 'local', 'test-embedding-v1', 3)
ON CONFLICT (provider, model_key, dimensions) DO NOTHING;

-- Data Space A Assets
INSERT INTO knowledge.knowledge_items (id, data_space_id, kind, title) VALUES
    ('0192a000-0000-7000-8000-0000000000a1', '0192a000-0000-7000-8000-0000000000aa', 'document', 'Document Space A');

INSERT INTO documents.storage_objects (id, data_space_id, storage_key, checksum_sha256, size_bytes, mime_type, state) VALUES
    ('0192a000-0000-7000-8000-0000000000a2', '0192a000-0000-7000-8000-0000000000aa', 'spaces/a/doc_a.pdf', repeat('a', 64), 1024, 'application/pdf', 'verified');

INSERT INTO documents.documents (knowledge_item_id, data_space_id, original_filename) VALUES
    ('0192a000-0000-7000-8000-0000000000a1', '0192a000-0000-7000-8000-0000000000aa', 'doc_a.pdf');

INSERT INTO documents.document_versions (id, data_space_id, document_id, version_no, storage_object_id, processing_status) VALUES
    ('0192a000-0000-7000-8000-0000000000a3', '0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-0000000000a1', 1, '0192a000-0000-7000-8000-0000000000a2', 'ready');

UPDATE documents.documents
SET current_version_id = '0192a000-0000-7000-8000-0000000000a3'
WHERE knowledge_item_id = '0192a000-0000-7000-8000-0000000000a1';

INSERT INTO documents.processing_runs (id, data_space_id, document_version_id, processor_version, status, is_active) VALUES
    ('0192a000-0000-7000-8000-0000000000a4', '0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-0000000000a3', 'v1', 'ready', true);

INSERT INTO documents.document_chunks (id, data_space_id, document_version_id, processing_run_id, chunk_index, content_text) VALUES
    ('0192a000-0000-7000-8000-0000000000a5', '0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-0000000000a3', '0192a000-0000-7000-8000-0000000000a4', 0, 'Secret academic notes for Space A');

INSERT INTO documents.chunk_embeddings (chunk_id, data_space_id, embedding_model_id, embedding) VALUES
    ('0192a000-0000-7000-8000-0000000000a5', '0192a000-0000-7000-8000-0000000000aa', '0192a000-0000-7000-8000-000000000099', '[0.1, 0.2, 0.3]');

-- Data Space B Assets
INSERT INTO knowledge.knowledge_items (id, data_space_id, kind, title) VALUES
    ('0192a000-0000-7000-8000-0000000000b1', '0192a000-0000-7000-8000-0000000000bb', 'document', 'Document Space B');

INSERT INTO documents.storage_objects (id, data_space_id, storage_key, checksum_sha256, size_bytes, mime_type, state) VALUES
    ('0192a000-0000-7000-8000-0000000000b2', '0192a000-0000-7000-8000-0000000000bb', 'spaces/b/doc_b.pdf', repeat('b', 64), 2048, 'application/pdf', 'verified');

INSERT INTO documents.documents (knowledge_item_id, data_space_id, original_filename) VALUES
    ('0192a000-0000-7000-8000-0000000000b1', '0192a000-0000-7000-8000-0000000000bb', 'doc_b.pdf');

INSERT INTO documents.document_versions (id, data_space_id, document_id, version_no, storage_object_id, processing_status) VALUES
    ('0192a000-0000-7000-8000-0000000000b3', '0192a000-0000-7000-8000-0000000000bb', '0192a000-0000-7000-8000-0000000000b1', 1, '0192a000-0000-7000-8000-0000000000b2', 'ready');

UPDATE documents.documents
SET current_version_id = '0192a000-0000-7000-8000-0000000000b3'
WHERE knowledge_item_id = '0192a000-0000-7000-8000-0000000000b1';

INSERT INTO documents.processing_runs (id, data_space_id, document_version_id, processor_version, status, is_active) VALUES
    ('0192a000-0000-7000-8000-0000000000b4', '0192a000-0000-7000-8000-0000000000bb', '0192a000-0000-7000-8000-0000000000b3', 'v1', 'ready', true);

INSERT INTO documents.document_chunks (id, data_space_id, document_version_id, processing_run_id, chunk_index, content_text) VALUES
    ('0192a000-0000-7000-8000-0000000000b5', '0192a000-0000-7000-8000-0000000000bb', '0192a000-0000-7000-8000-0000000000b3', '0192a000-0000-7000-8000-0000000000b4', 0, 'Confidential research findings for Space B');

INSERT INTO documents.chunk_embeddings (chunk_id, data_space_id, embedding_model_id, embedding) VALUES
    ('0192a000-0000-7000-8000-0000000000b5', '0192a000-0000-7000-8000-0000000000bb', '0192a000-0000-7000-8000-000000000099', '[0.7, 0.8, 0.9]');
