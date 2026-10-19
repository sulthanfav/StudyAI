-- StudyAI Campus / Physical ERD REVISED review candidate v1.1
-- PostgreSQL 16+ and pgvector; clean database only. NO destructive DROP statements.
-- Status: REVIEWED DRAFT (static checks only), NOT EXECUTED IN POSTGRESQL.
-- DO NOT apply directly to existing databases; use reviewed EF migrations and staging tests.
-- UUIDv7: generate in .NET Guid.CreateVersion7(), client passes IDs.
-- All timestamps UTC timestamptz. Long binary data lives in private object storage.
BEGIN;
CREATE EXTENSION IF NOT EXISTS vector;
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE EXTENSION IF NOT EXISTS btree_gist; -- for overlapping subscription periods
CREATE SCHEMA IF NOT EXISTS identity;
CREATE SCHEMA IF NOT EXISTS ownership;
CREATE SCHEMA IF NOT EXISTS workspace;
CREATE SCHEMA IF NOT EXISTS knowledge;
CREATE SCHEMA IF NOT EXISTS documents;
CREATE SCHEMA IF NOT EXISTS research;
CREATE SCHEMA IF NOT EXISTS ai;
CREATE SCHEMA IF NOT EXISTS learning;
CREATE SCHEMA IF NOT EXISTS writing;
CREATE SCHEMA IF NOT EXISTS presentation;
CREATE SCHEMA IF NOT EXISTS integrity;
CREATE SCHEMA IF NOT EXISTS planner;
CREATE SCHEMA IF NOT EXISTS billing;
CREATE SCHEMA IF NOT EXISTS ops;
-- Source snapshots and extracted chunks must remain immutable after insert.
-- Purge by controlled DELETE/retention workflow, not UPDATE to a new claim.
CREATE OR REPLACE FUNCTION ops.reject_snapshot_update()
RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
 RAISE EXCEPTION 'immutable academic source snapshot: %', TG_TABLE_NAME
   USING ERRCODE = '23514';
END;
$$;

-- 01 Identity / ownership ----------------------------------------------------
CREATE TABLE identity.users (
 id uuid PRIMARY KEY, email text NOT NULL, display_name text NOT NULL,
 status text NOT NULL DEFAULT 'active' CHECK(status IN ('active','suspended','closing')),
 locale text NOT NULL DEFAULT 'id-ID', timezone text NOT NULL DEFAULT 'Asia/Jakarta',
 created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NOT NULL DEFAULT now(),
 deleted_at timestamptz
);
CREATE UNIQUE INDEX ux_users_email_normalized ON identity.users (lower(email));
-- Auth credentials/sessions/refresh tokens belong in an auth-provider migration;
-- do not create homegrown password storage here.
CREATE TABLE ownership.data_spaces (
 id uuid PRIMARY KEY, owner_user_id uuid NOT NULL REFERENCES identity.users(id) ON DELETE RESTRICT,
 kind text NOT NULL DEFAULT 'personal' CHECK(kind = 'personal'),
 name text NOT NULL, created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,owner_user_id)
);
CREATE UNIQUE INDEX ux_one_personal_space ON ownership.data_spaces(owner_user_id) WHERE kind='personal';

-- 02 Organization and placements ------------------------------------------
CREATE TABLE workspace.content_spaces (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 kind text NOT NULL CHECK(kind IN ('library','course','project')),
 title text NOT NULL, description text, position int NOT NULL DEFAULT 0,
 created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NOT NULL DEFAULT now(),
 deleted_at timestamptz,
 UNIQUE(id,data_space_id), UNIQUE(id,data_space_id,kind)
);
CREATE UNIQUE INDEX ux_library_per_owner ON workspace.content_spaces(data_space_id) WHERE kind='library';
CREATE INDEX ix_content_spaces_owner ON workspace.content_spaces(data_space_id,kind,position) WHERE deleted_at IS NULL;
CREATE TABLE workspace.workspaces (
 content_space_id uuid PRIMARY KEY, data_space_id uuid NOT NULL,
 content_kind text NOT NULL CHECK(content_kind IN ('course','project')),
 academic_term text, course_code text, project_type text, start_date date, end_date date,
 status text NOT NULL DEFAULT 'active' CHECK(status IN ('active','archived','completed')),
 created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(content_space_id,data_space_id),
 FOREIGN KEY (content_space_id,data_space_id,content_kind)
  REFERENCES workspace.content_spaces(id,data_space_id,kind) ON DELETE RESTRICT
);
CREATE TABLE workspace.folders (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, content_space_id uuid NOT NULL,
 parent_folder_id uuid, name text NOT NULL, position int NOT NULL DEFAULT 0,
 created_at timestamptz NOT NULL DEFAULT now(), deleted_at timestamptz,
 UNIQUE(id,data_space_id,content_space_id),
 FOREIGN KEY(content_space_id,data_space_id) REFERENCES workspace.content_spaces(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(parent_folder_id,data_space_id,content_space_id)
  REFERENCES workspace.folders(id,data_space_id,content_space_id) ON DELETE RESTRICT,
 CHECK(parent_folder_id IS NULL OR parent_folder_id <> id)
);
CREATE INDEX ix_folders_children ON workspace.folders(data_space_id,content_space_id,parent_folder_id,position) WHERE deleted_at IS NULL;

-- 03 Canonical knowledge item ---------------------------------------------
CREATE TABLE knowledge.knowledge_items (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 kind text NOT NULL CHECK(kind IN ('note','document','reference','saved_ai_output')),
 title text NOT NULL, description text, created_at timestamptz NOT NULL DEFAULT now(),
 updated_at timestamptz NOT NULL DEFAULT now(), deleted_at timestamptz,
 UNIQUE(id,data_space_id), UNIQUE(id,data_space_id,kind)
);
CREATE INDEX ix_knowledge_owner_recent ON knowledge.knowledge_items(data_space_id,updated_at DESC,id) WHERE deleted_at IS NULL;
CREATE TABLE knowledge.notes (
 knowledge_item_id uuid PRIMARY KEY, data_space_id uuid NOT NULL,
 item_kind text NOT NULL DEFAULT 'note' CHECK(item_kind='note'),
 content_json jsonb NOT NULL DEFAULT '{}'::jsonb, format_version int NOT NULL CHECK(format_version>0),
 revision bigint NOT NULL DEFAULT 1 CHECK(revision>0), search_text text NOT NULL DEFAULT '',
 updated_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(knowledge_item_id,data_space_id),
 FOREIGN KEY(knowledge_item_id,data_space_id,item_kind)
  REFERENCES knowledge.knowledge_items(id,data_space_id,kind) ON DELETE RESTRICT
);
CREATE TABLE knowledge.note_revisions (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, note_id uuid NOT NULL,
 revision bigint NOT NULL CHECK(revision>0), format_version int NOT NULL,
 content_json jsonb NOT NULL, search_text text NOT NULL DEFAULT '',
 created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id,note_id), UNIQUE(note_id,revision),
 FOREIGN KEY(note_id,data_space_id) REFERENCES knowledge.notes(knowledge_item_id,data_space_id) ON DELETE RESTRICT
);
CREATE TRIGGER trg_note_revision_immutable BEFORE UPDATE
 ON knowledge.note_revisions FOR EACH ROW EXECUTE FUNCTION ops.reject_snapshot_update();
CREATE TABLE knowledge.saved_ai_outputs (
 knowledge_item_id uuid PRIMARY KEY, data_space_id uuid NOT NULL,
 item_kind text NOT NULL DEFAULT 'saved_ai_output' CHECK(item_kind='saved_ai_output'),
 origin_job_id uuid, content_json jsonb NOT NULL, created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(knowledge_item_id,data_space_id),
 FOREIGN KEY(knowledge_item_id,data_space_id,item_kind)
  REFERENCES knowledge.knowledge_items(id,data_space_id,kind) ON DELETE RESTRICT
);
CREATE TABLE knowledge.saved_output_versions (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, saved_output_id uuid NOT NULL,
 version_no int NOT NULL CHECK(version_no>0), content_json jsonb NOT NULL,
 created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id,saved_output_id), UNIQUE(saved_output_id,version_no),
 FOREIGN KEY(saved_output_id,data_space_id) REFERENCES knowledge.saved_ai_outputs(knowledge_item_id,data_space_id) ON DELETE RESTRICT
);
CREATE TRIGGER trg_saved_output_version_immutable BEFORE UPDATE
 ON knowledge.saved_output_versions FOR EACH ROW EXECUTE FUNCTION ops.reject_snapshot_update();
CREATE TABLE workspace.item_placements (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, content_space_id uuid NOT NULL,
 knowledge_item_id uuid NOT NULL, item_kind text NOT NULL CHECK(item_kind IN ('note','document','reference','saved_ai_output')),
 folder_id uuid, parent_page_placement_id uuid,
 parent_page_kind text NOT NULL DEFAULT 'note' CHECK(parent_page_kind='note'),
 position int NOT NULL DEFAULT 0, created_at timestamptz NOT NULL DEFAULT now(),
 removed_at timestamptz,
 UNIQUE(id,data_space_id,content_space_id),
 UNIQUE(id,data_space_id,content_space_id,item_kind),
 FOREIGN KEY(content_space_id,data_space_id) REFERENCES workspace.content_spaces(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(knowledge_item_id,data_space_id,item_kind)
  REFERENCES knowledge.knowledge_items(id,data_space_id,kind) ON DELETE RESTRICT,
 FOREIGN KEY(folder_id,data_space_id,content_space_id)
  REFERENCES workspace.folders(id,data_space_id,content_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(parent_page_placement_id,data_space_id,content_space_id,parent_page_kind)
  REFERENCES workspace.item_placements(id,data_space_id,content_space_id,item_kind) ON DELETE RESTRICT,
 CHECK (folder_id IS NULL OR parent_page_placement_id IS NULL),
 CHECK(parent_page_placement_id IS NULL OR parent_page_placement_id <> id)
);
CREATE UNIQUE INDEX ux_active_item_per_content_space ON workspace.item_placements(content_space_id,knowledge_item_id) WHERE removed_at IS NULL;
CREATE INDEX ix_placement_by_folder ON workspace.item_placements(data_space_id,content_space_id,folder_id,position) WHERE removed_at IS NULL;
CREATE INDEX ix_placement_by_page ON workspace.item_placements(data_space_id,content_space_id,parent_page_placement_id,position) WHERE removed_at IS NULL;
CREATE INDEX ix_placement_by_item ON workspace.item_placements(data_space_id,knowledge_item_id) WHERE removed_at IS NULL;
CREATE TABLE knowledge.knowledge_links (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL,
 source_item_id uuid NOT NULL, target_item_id uuid NOT NULL,
 relation text NOT NULL CHECK(relation IN ('related','supports','derived_from','mentions')),
 created_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(source_item_id,data_space_id) REFERENCES knowledge.knowledge_items(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(target_item_id,data_space_id) REFERENCES knowledge.knowledge_items(id,data_space_id) ON DELETE RESTRICT,
 UNIQUE(data_space_id,source_item_id,target_item_id,relation), CHECK(source_item_id<>target_item_id)
);
CREATE TABLE knowledge.tags (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 name text NOT NULL, UNIQUE(id,data_space_id)
);
CREATE UNIQUE INDEX ux_tag_name ON knowledge.tags(data_space_id,lower(name));
CREATE TABLE knowledge.item_tags (
 data_space_id uuid NOT NULL, knowledge_item_id uuid NOT NULL, tag_id uuid NOT NULL,
 PRIMARY KEY(data_space_id,knowledge_item_id,tag_id),
 FOREIGN KEY(knowledge_item_id,data_space_id) REFERENCES knowledge.knowledge_items(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(tag_id,data_space_id) REFERENCES knowledge.tags(id,data_space_id) ON DELETE RESTRICT
);

-- 04 Private file storage, documents and ingestion -------------------------
CREATE TABLE documents.storage_objects (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 storage_key text NOT NULL UNIQUE, checksum_sha256 text NOT NULL CHECK(length(checksum_sha256)=64),
 size_bytes bigint NOT NULL CHECK(size_bytes>=0), mime_type text NOT NULL,
 charge_class text NOT NULL DEFAULT 'billable' CHECK(charge_class IN ('billable','internal','temporary')),
 state text NOT NULL DEFAULT 'pending' CHECK(state IN ('pending','verified','quarantined','deleting','deleted')),
 created_at timestamptz NOT NULL DEFAULT now(), deleted_at timestamptz,
 UNIQUE(id,data_space_id)
);
CREATE INDEX ix_storage_owner_checksum ON documents.storage_objects(data_space_id,checksum_sha256) WHERE state='verified';
CREATE TABLE documents.documents (
 knowledge_item_id uuid PRIMARY KEY, data_space_id uuid NOT NULL,
 item_kind text NOT NULL DEFAULT 'document' CHECK(item_kind='document'),
 original_filename text NOT NULL, current_version_id uuid,
 created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(knowledge_item_id,data_space_id),
 FOREIGN KEY(knowledge_item_id,data_space_id,item_kind)
  REFERENCES knowledge.knowledge_items(id,data_space_id,kind) ON DELETE RESTRICT
);
CREATE TABLE documents.document_versions (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, document_id uuid NOT NULL,
 version_no int NOT NULL CHECK(version_no>0), storage_object_id uuid NOT NULL,
 page_count int CHECK(page_count IS NULL OR page_count>=0),
 processing_status text NOT NULL DEFAULT 'pending'
  CHECK(processing_status IN ('pending','queued','processing','ready','failed')),
 processing_version text, created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id), UNIQUE(id,data_space_id,document_id), UNIQUE(document_id,version_no),
 FOREIGN KEY(document_id,data_space_id) REFERENCES documents.documents(knowledge_item_id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(storage_object_id,data_space_id) REFERENCES documents.storage_objects(id,data_space_id) ON DELETE RESTRICT
);
ALTER TABLE documents.documents ADD CONSTRAINT fk_document_current_version
 FOREIGN KEY(current_version_id,data_space_id,knowledge_item_id)
 REFERENCES documents.document_versions(id,data_space_id,document_id)
 DEFERRABLE INITIALLY IMMEDIATE;
-- Keep file identity stable after a version has been registered.
CREATE OR REPLACE FUNCTION documents.guard_document_version_identity()
RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
 IF ROW(NEW.data_space_id,NEW.document_id,NEW.version_no,NEW.storage_object_id)
    IS DISTINCT FROM
    ROW(OLD.data_space_id,OLD.document_id,OLD.version_no,OLD.storage_object_id)
 THEN
   RAISE EXCEPTION 'document version identity is immutable' USING ERRCODE = '23514';
 END IF;
 RETURN NEW;
END;
$$;
CREATE TRIGGER trg_document_version_immutable BEFORE UPDATE
 ON documents.document_versions FOR EACH ROW
 EXECUTE FUNCTION documents.guard_document_version_identity();
CREATE TABLE documents.processing_runs (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, document_version_id uuid NOT NULL,
 processor_version text NOT NULL, status text NOT NULL
  CHECK(status IN ('queued','running','ready','failed','cancelled')),
 is_active boolean NOT NULL DEFAULT false,
 CHECK (NOT is_active OR status='ready'),
 error_code text, started_at timestamptz, ended_at timestamptz,
 created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id,document_version_id),
 FOREIGN KEY(document_version_id,data_space_id) REFERENCES documents.document_versions(id,data_space_id) ON DELETE RESTRICT
);
-- Exactly one published extraction run per document version.
CREATE UNIQUE INDEX ux_processing_active_run
  ON documents.processing_runs(document_version_id) WHERE is_active;
CREATE TABLE documents.document_chunks (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, document_version_id uuid NOT NULL,
 processing_run_id uuid NOT NULL, chunk_index int NOT NULL CHECK(chunk_index>=0),
 content_text text NOT NULL, locator jsonb NOT NULL DEFAULT '{}'::jsonb,
 token_count int CHECK(token_count IS NULL OR token_count>=0),
 search_vector tsvector GENERATED ALWAYS AS (to_tsvector('simple',content_text)) STORED,
 UNIQUE(id,data_space_id), UNIQUE(document_version_id,processing_run_id,chunk_index),
 FOREIGN KEY(document_version_id,data_space_id) REFERENCES documents.document_versions(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(processing_run_id,data_space_id,document_version_id)
  REFERENCES documents.processing_runs(id,data_space_id,document_version_id) ON DELETE RESTRICT
);
CREATE TRIGGER trg_document_chunk_immutable BEFORE UPDATE
 ON documents.document_chunks FOR EACH ROW EXECUTE FUNCTION ops.reject_snapshot_update();
CREATE INDEX ix_chunks_scope ON documents.document_chunks(data_space_id,document_version_id,processing_run_id);
CREATE INDEX ix_chunks_fts ON documents.document_chunks USING gin(search_vector);
CREATE INDEX ix_chunks_active_run ON documents.document_chunks(data_space_id,processing_run_id,chunk_index);
CREATE TABLE ai.embedding_models (
 id uuid PRIMARY KEY, provider text NOT NULL, model_key text NOT NULL,
 dimensions int NOT NULL CHECK(dimensions BETWEEN 1 AND 2000),
 created_at timestamptz NOT NULL DEFAULT now(), UNIQUE(provider,model_key,dimensions)
);
CREATE TABLE documents.chunk_embeddings (
 chunk_id uuid NOT NULL, data_space_id uuid NOT NULL, embedding_model_id uuid NOT NULL,
 embedding vector NOT NULL, created_at timestamptz NOT NULL DEFAULT now(),
 PRIMARY KEY(chunk_id,embedding_model_id),
 FOREIGN KEY(chunk_id,data_space_id) REFERENCES documents.document_chunks(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(embedding_model_id) REFERENCES ai.embedding_models(id) ON DELETE RESTRICT,
 CHECK(vector_dims(embedding)>0)
);
CREATE INDEX ix_embeddings_tenant ON documents.chunk_embeddings(data_space_id,embedding_model_id);
-- Intentionally no ANN index. The trigger enforces embedding model dimension.
CREATE OR REPLACE FUNCTION documents.ensure_embedding_dimensions()
RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE expected int;
BEGIN
 SELECT dimensions INTO expected FROM ai.embedding_models WHERE id = NEW.embedding_model_id;
 IF expected IS NULL THEN
   RAISE EXCEPTION 'embedding model % not found', NEW.embedding_model_id;
 END IF;
 IF vector_dims(NEW.embedding) <> expected THEN
   RAISE EXCEPTION 'embedding dimensions %, expected % for model %',
     vector_dims(NEW.embedding), expected, NEW.embedding_model_id;
 END IF;
 RETURN NEW;
END;
$$;
CREATE TRIGGER trg_embedding_dimensions BEFORE INSERT OR UPDATE OF embedding,embedding_model_id
 ON documents.chunk_embeddings FOR EACH ROW EXECUTE FUNCTION documents.ensure_embedding_dimensions();
-- embedding_models dimensions are immutable after use; guard updates in the service/migrations.

CREATE OR REPLACE FUNCTION ai.guard_embedding_model_identity()
RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
 IF ROW(NEW.provider,NEW.model_key,NEW.dimensions) IS DISTINCT FROM
    ROW(OLD.provider,OLD.model_key,OLD.dimensions) AND
    EXISTS (SELECT 1 FROM documents.chunk_embeddings WHERE embedding_model_id=OLD.id) THEN
    RAISE EXCEPTION 'embedding model in use cannot be mutated' USING ERRCODE = '23514';
 END IF;
 RETURN NEW;
END;
$$;
CREATE TRIGGER trg_embedding_model_immutable BEFORE UPDATE
 ON ai.embedding_models FOR EACH ROW EXECUTE FUNCTION ai.guard_embedding_model_identity();
CREATE TABLE documents.note_attachments (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, note_id uuid NOT NULL,
 storage_object_id uuid NOT NULL, label text, created_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(note_id,data_space_id) REFERENCES knowledge.notes(knowledge_item_id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(storage_object_id,data_space_id) REFERENCES documents.storage_objects(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE documents.document_annotations (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, document_version_id uuid NOT NULL,
 locator jsonb NOT NULL, body_text text, color text, created_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(document_version_id,data_space_id) REFERENCES documents.document_versions(id,data_space_id) ON DELETE RESTRICT
);

-- 05 Research and unified immutable provenance ----------------------------
CREATE TABLE research.bibliographic_references (
 knowledge_item_id uuid PRIMARY KEY, data_space_id uuid NOT NULL,
 item_kind text NOT NULL DEFAULT 'reference' CHECK(item_kind='reference'),
 doi_normalized text, title text NOT NULL, publication_year int,
 venue_name text, source_provider text, verification_status text NOT NULL DEFAULT 'unverified'
  CHECK(verification_status IN ('unverified','verified','failed','manual')),
 created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(knowledge_item_id,data_space_id),
 FOREIGN KEY(knowledge_item_id,data_space_id,item_kind)
  REFERENCES knowledge.knowledge_items(id,data_space_id,kind) ON DELETE RESTRICT
);
-- No deleted_at predicate here: DOI uniqueness remains across Trash.
CREATE UNIQUE INDEX ux_ref_doi_owner ON research.bibliographic_references(data_space_id,doi_normalized) WHERE doi_normalized IS NOT NULL;
CREATE TABLE research.reference_authors (
 data_space_id uuid NOT NULL, reference_id uuid NOT NULL,
 author_order int NOT NULL CHECK(author_order>0), full_name text NOT NULL, orcid text,
 PRIMARY KEY(reference_id,author_order),
 FOREIGN KEY(reference_id,data_space_id) REFERENCES research.bibliographic_references(knowledge_item_id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE research.reference_documents (
 data_space_id uuid NOT NULL, reference_id uuid NOT NULL, document_id uuid NOT NULL,
 role text NOT NULL DEFAULT 'full_text',
 PRIMARY KEY(reference_id,document_id),
 FOREIGN KEY(reference_id,data_space_id) REFERENCES research.bibliographic_references(knowledge_item_id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(document_id,data_space_id) REFERENCES documents.documents(knowledge_item_id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE research.reference_snapshots (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, reference_id uuid NOT NULL,
 snapshot_no int NOT NULL CHECK(snapshot_no>0), metadata_json jsonb NOT NULL,
 created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id,reference_id), UNIQUE(reference_id,snapshot_no),
 FOREIGN KEY(reference_id,data_space_id) REFERENCES research.bibliographic_references(knowledge_item_id,data_space_id) ON DELETE RESTRICT
);
CREATE TRIGGER trg_reference_snapshot_immutable BEFORE UPDATE
 ON research.reference_snapshots FOR EACH ROW EXECUTE FUNCTION ops.reject_snapshot_update();
CREATE TABLE knowledge.source_versions (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, knowledge_item_id uuid NOT NULL,
 source_kind text NOT NULL CHECK(source_kind IN ('note','document','reference','saved_ai_output')),
 note_revision_id uuid, document_version_id uuid, reference_snapshot_id uuid, saved_output_version_id uuid,
 created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id), UNIQUE(id,data_space_id,knowledge_item_id),
 FOREIGN KEY(knowledge_item_id,data_space_id,source_kind)
  REFERENCES knowledge.knowledge_items(id,data_space_id,kind) ON DELETE RESTRICT,
 FOREIGN KEY(note_revision_id,data_space_id,knowledge_item_id)
  REFERENCES knowledge.note_revisions(id,data_space_id,note_id) ON DELETE RESTRICT,
 FOREIGN KEY(document_version_id,data_space_id,knowledge_item_id)
  REFERENCES documents.document_versions(id,data_space_id,document_id) ON DELETE RESTRICT,
 FOREIGN KEY(reference_snapshot_id,data_space_id,knowledge_item_id)
  REFERENCES research.reference_snapshots(id,data_space_id,reference_id) ON DELETE RESTRICT,
 FOREIGN KEY(saved_output_version_id,data_space_id,knowledge_item_id)
  REFERENCES knowledge.saved_output_versions(id,data_space_id,saved_output_id) ON DELETE RESTRICT,
 CHECK(num_nonnulls(note_revision_id,document_version_id,reference_snapshot_id,saved_output_version_id)=1),
 CHECK((source_kind='note' AND note_revision_id IS NOT NULL) OR
       (source_kind='document' AND document_version_id IS NOT NULL) OR
       (source_kind='reference' AND reference_snapshot_id IS NOT NULL) OR
       (source_kind='saved_ai_output' AND saved_output_version_id IS NOT NULL))
);
CREATE UNIQUE INDEX ux_source_note_rev ON knowledge.source_versions(note_revision_id) WHERE note_revision_id IS NOT NULL;
CREATE UNIQUE INDEX ux_source_doc_ver ON knowledge.source_versions(document_version_id) WHERE document_version_id IS NOT NULL;
CREATE UNIQUE INDEX ux_source_ref_snap ON knowledge.source_versions(reference_snapshot_id) WHERE reference_snapshot_id IS NOT NULL;
CREATE UNIQUE INDEX ux_source_saved_ver ON knowledge.source_versions(saved_output_version_id) WHERE saved_output_version_id IS NOT NULL;
-- Immutable provenance: a source_version may never silently change its underlying snapshot.
CREATE OR REPLACE FUNCTION knowledge.guard_source_version_identity()
RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
 IF ROW(NEW.data_space_id,NEW.knowledge_item_id,NEW.source_kind,NEW.note_revision_id,
        NEW.document_version_id,NEW.reference_snapshot_id,NEW.saved_output_version_id)
    IS DISTINCT FROM
    ROW(OLD.data_space_id,OLD.knowledge_item_id,OLD.source_kind,OLD.note_revision_id,
        OLD.document_version_id,OLD.reference_snapshot_id,OLD.saved_output_version_id)
 THEN
   RAISE EXCEPTION 'source version identity is immutable' USING ERRCODE = '23514';
 END IF;
 RETURN NEW;
END;
$$;
CREATE TRIGGER trg_source_version_immutable
 BEFORE UPDATE ON knowledge.source_versions
 FOR EACH ROW EXECUTE FUNCTION knowledge.guard_source_version_identity();

-- 06 AI orchestration + provenance ----------------------------------------
CREATE TABLE ai.ai_conversations (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 title text, created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NOT NULL DEFAULT now(),
 deleted_at timestamptz, UNIQUE(id,data_space_id)
);
CREATE TABLE ai.ai_messages (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, conversation_id uuid NOT NULL,
 role text NOT NULL CHECK(role IN ('user','assistant','tool','system')),
 body_text text NOT NULL, created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id),
 FOREIGN KEY(conversation_id,data_space_id) REFERENCES ai.ai_conversations(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE ai.ai_conversation_sources (
 data_space_id uuid NOT NULL, conversation_id uuid NOT NULL, source_version_id uuid NOT NULL,
 added_at timestamptz NOT NULL DEFAULT now(),
 PRIMARY KEY(conversation_id,source_version_id),
 FOREIGN KEY(conversation_id,data_space_id) REFERENCES ai.ai_conversations(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(source_version_id,data_space_id) REFERENCES knowledge.source_versions(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE ai.ai_jobs (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 conversation_id uuid, feature text NOT NULL, state text NOT NULL DEFAULT 'queued'
  CHECK(state IN ('queued','reserved','running','succeeded','failed','cancelled')),
 idempotency_key text NOT NULL, input_json jsonb NOT NULL DEFAULT '{}'::jsonb,
 output_json jsonb, error_code text,
 created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id), UNIQUE(data_space_id,idempotency_key),
 FOREIGN KEY(conversation_id,data_space_id) REFERENCES ai.ai_conversations(id,data_space_id) ON DELETE RESTRICT
);
CREATE INDEX ix_ai_jobs_pending ON ai.ai_jobs(state,created_at) WHERE state IN ('queued','reserved');
CREATE TABLE ai.ai_job_sources (
 data_space_id uuid NOT NULL, job_id uuid NOT NULL, source_version_id uuid NOT NULL,
 PRIMARY KEY(job_id,source_version_id),
 FOREIGN KEY(job_id,data_space_id) REFERENCES ai.ai_jobs(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(source_version_id,data_space_id) REFERENCES knowledge.source_versions(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE ai.ai_message_citations (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, message_id uuid NOT NULL,
 source_version_id uuid NOT NULL, locator jsonb NOT NULL DEFAULT '{}'::jsonb,
 quote_snapshot text NOT NULL, claim_text text, source_confidence text NOT NULL DEFAULT 'extracted'
  CHECK(source_confidence IN ('verified_metadata','extracted','user_input','inference')),
 created_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(message_id,data_space_id) REFERENCES ai.ai_messages(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(source_version_id,data_space_id) REFERENCES knowledge.source_versions(id,data_space_id) ON DELETE RESTRICT
);
CREATE INDEX ix_citations_message ON ai.ai_message_citations(data_space_id,message_id);
CREATE INDEX ix_ai_messages_conversation ON ai.ai_messages(data_space_id,conversation_id,created_at,id);

-- 07 Learning + personalized study ----------------------------------------
CREATE TABLE learning.learning_profiles (
 data_space_id uuid PRIMARY KEY REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 goals_json jsonb NOT NULL DEFAULT '{}'::jsonb, preferences_json jsonb NOT NULL DEFAULT '{}'::jsonb,
 updated_at timestamptz NOT NULL DEFAULT now()
);
CREATE TABLE learning.quizzes (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 content_space_id uuid, title text NOT NULL, status text NOT NULL DEFAULT 'draft'
  CHECK(status IN ('draft','published','archived')),
 created_at timestamptz NOT NULL DEFAULT now(), UNIQUE(id,data_space_id),
 FOREIGN KEY(content_space_id,data_space_id) REFERENCES workspace.content_spaces(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE learning.quiz_sources (
 data_space_id uuid NOT NULL, quiz_id uuid NOT NULL, source_version_id uuid NOT NULL,
 PRIMARY KEY(quiz_id,source_version_id),
 FOREIGN KEY(quiz_id,data_space_id) REFERENCES learning.quizzes(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(source_version_id,data_space_id) REFERENCES knowledge.source_versions(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE learning.quiz_questions (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, quiz_id uuid NOT NULL,
 sequence_no int NOT NULL, question_type text NOT NULL CHECK(question_type IN ('mcq','true_false','short_answer')),
 prompt text NOT NULL, choices_json jsonb, answer_key_json jsonb NOT NULL,
 explanation text, points numeric(8,2) NOT NULL DEFAULT 1 CHECK(points>=0),
 UNIQUE(id,data_space_id,quiz_id), UNIQUE(quiz_id,sequence_no),
 FOREIGN KEY(quiz_id,data_space_id) REFERENCES learning.quizzes(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE learning.quiz_attempts (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, quiz_id uuid NOT NULL,
 state text NOT NULL DEFAULT 'in_progress' CHECK(state IN ('in_progress','submitted','graded')),
 score numeric(10,2), started_at timestamptz NOT NULL DEFAULT now(), submitted_at timestamptz,
 UNIQUE(id,data_space_id,quiz_id),
 FOREIGN KEY(quiz_id,data_space_id) REFERENCES learning.quizzes(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE learning.quiz_answers (
 data_space_id uuid NOT NULL, quiz_id uuid NOT NULL, attempt_id uuid NOT NULL, question_id uuid NOT NULL,
 response_json jsonb NOT NULL, awarded_points numeric(8,2), answered_at timestamptz NOT NULL DEFAULT now(),
 PRIMARY KEY(attempt_id,question_id),
 FOREIGN KEY(attempt_id,data_space_id,quiz_id) REFERENCES learning.quiz_attempts(id,data_space_id,quiz_id) ON DELETE RESTRICT,
 FOREIGN KEY(question_id,data_space_id,quiz_id) REFERENCES learning.quiz_questions(id,data_space_id,quiz_id) ON DELETE RESTRICT
);
CREATE TABLE learning.flashcards (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 content_space_id uuid, front_text text NOT NULL, back_text text NOT NULL,
 source_version_id uuid, due_at timestamptz, interval_days int NOT NULL DEFAULT 0 CHECK(interval_days>=0),
 ease_factor numeric(4,2) NOT NULL DEFAULT 2.5, repetitions int NOT NULL DEFAULT 0,
 created_at timestamptz NOT NULL DEFAULT now(), UNIQUE(id,data_space_id),
 FOREIGN KEY(content_space_id,data_space_id) REFERENCES workspace.content_spaces(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(source_version_id,data_space_id) REFERENCES knowledge.source_versions(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE learning.flashcard_reviews (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, flashcard_id uuid NOT NULL,
 quality smallint NOT NULL CHECK(quality BETWEEN 0 AND 5),
 reviewed_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(flashcard_id,data_space_id) REFERENCES learning.flashcards(id,data_space_id) ON DELETE RESTRICT
);

-- 08 Research workflows beyond bibliography -------------------------------
CREATE TABLE research.literature_matrices (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 content_space_id uuid, title text NOT NULL, created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id),
 FOREIGN KEY(content_space_id,data_space_id) REFERENCES workspace.content_spaces(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE research.literature_matrix_rows (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, matrix_id uuid NOT NULL,
 reference_id uuid NOT NULL, summary text, method text, findings text, limitations text,
 position int NOT NULL DEFAULT 0,
 UNIQUE(matrix_id,reference_id),
 FOREIGN KEY(matrix_id,data_space_id) REFERENCES research.literature_matrices(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(reference_id,data_space_id) REFERENCES research.bibliographic_references(knowledge_item_id,data_space_id) ON DELETE RESTRICT
);

-- 09 Writing / citations ---------------------------------------------------
CREATE TABLE writing.writing_documents (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 content_space_id uuid, title text NOT NULL,
 content_json jsonb NOT NULL DEFAULT '{}'::jsonb, revision bigint NOT NULL DEFAULT 1 CHECK(revision>0),
 status text NOT NULL DEFAULT 'draft' CHECK(status IN ('draft','review','final')),
 created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id),
 FOREIGN KEY(content_space_id,data_space_id) REFERENCES workspace.content_spaces(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE writing.writing_revisions (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, writing_document_id uuid NOT NULL,
 revision bigint NOT NULL, content_json jsonb NOT NULL, created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(writing_document_id,revision),
 FOREIGN KEY(writing_document_id,data_space_id) REFERENCES writing.writing_documents(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE writing.citations (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, writing_document_id uuid NOT NULL,
 reference_id uuid NOT NULL, source_version_id uuid,
 locator jsonb NOT NULL DEFAULT '{}'::jsonb, csl_style text NOT NULL DEFAULT 'apa',
 cited_text_snapshot text, created_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(writing_document_id,data_space_id) REFERENCES writing.writing_documents(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(reference_id,data_space_id) REFERENCES research.bibliographic_references(knowledge_item_id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(source_version_id,data_space_id,reference_id)
  REFERENCES knowledge.source_versions(id,data_space_id,knowledge_item_id) ON DELETE RESTRICT
);

-- 10 Presentation & exports -----------------------------------------------
CREATE TABLE presentation.decks (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 content_space_id uuid, title text NOT NULL,
 theme_json jsonb NOT NULL DEFAULT '{}'::jsonb, revision bigint NOT NULL DEFAULT 1,
 created_at timestamptz NOT NULL DEFAULT now(), updated_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id),
 FOREIGN KEY(content_space_id,data_space_id) REFERENCES workspace.content_spaces(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE presentation.slides (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, deck_id uuid NOT NULL,
 slide_no int NOT NULL CHECK(slide_no>0), elements_json jsonb NOT NULL,
 speaker_notes text, source_version_id uuid,
 UNIQUE(deck_id,slide_no),
 FOREIGN KEY(deck_id,data_space_id) REFERENCES presentation.decks(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(source_version_id,data_space_id) REFERENCES knowledge.source_versions(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE presentation.export_jobs (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, deck_id uuid NOT NULL,
 format text NOT NULL CHECK(format IN ('pptx','pdf')),
 status text NOT NULL DEFAULT 'queued' CHECK(status IN ('queued','running','ready','failed')),
 storage_object_id uuid, error_code text, created_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(deck_id,data_space_id) REFERENCES presentation.decks(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(storage_object_id,data_space_id) REFERENCES documents.storage_objects(id,data_space_id) ON DELETE RESTRICT
);

-- 11 Integrity/audits (similarity only if legally integrated) --------------
CREATE TABLE integrity.citation_audits (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, writing_document_id uuid NOT NULL,
 state text NOT NULL DEFAULT 'queued' CHECK(state IN ('queued','running','completed','failed')),
 findings_json jsonb, created_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(writing_document_id,data_space_id) REFERENCES writing.writing_documents(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE integrity.similarity_checks (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 writing_document_id uuid, provider text NOT NULL, provider_job_id text,
 status text NOT NULL DEFAULT 'pending' CHECK(status IN ('pending','running','completed','failed')),
 result_storage_object_id uuid, created_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(writing_document_id,data_space_id) REFERENCES writing.writing_documents(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(result_storage_object_id,data_space_id) REFERENCES documents.storage_objects(id,data_space_id) ON DELETE RESTRICT
);

-- 12 Academic planner ------------------------------------------------------
CREATE TABLE planner.tasks (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 content_space_id uuid, title text NOT NULL, description text, due_at timestamptz,
 status text NOT NULL DEFAULT 'todo' CHECK(status IN ('todo','doing','done','cancelled')),
 priority smallint NOT NULL DEFAULT 2 CHECK(priority BETWEEN 1 AND 3),
 completed_at timestamptz, created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id),
 FOREIGN KEY(content_space_id,data_space_id) REFERENCES workspace.content_spaces(id,data_space_id) ON DELETE RESTRICT
);
CREATE INDEX ix_tasks_owner_due ON planner.tasks(data_space_id,status,due_at);
CREATE TABLE planner.calendar_events (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 content_space_id uuid, title text NOT NULL, starts_at timestamptz NOT NULL, ends_at timestamptz NOT NULL,
 timezone text NOT NULL DEFAULT 'Asia/Jakarta', recurrence_rule text,
 created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id), CHECK(ends_at>starts_at),
 FOREIGN KEY(content_space_id,data_space_id) REFERENCES workspace.content_spaces(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE planner.reminders (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, task_id uuid, event_id uuid,
 remind_at timestamptz NOT NULL, state text NOT NULL DEFAULT 'pending'
  CHECK(state IN ('pending','sent','failed','cancelled')),
 created_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(task_id,data_space_id) REFERENCES planner.tasks(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(event_id,data_space_id) REFERENCES planner.calendar_events(id,data_space_id) ON DELETE RESTRICT,
 CHECK(num_nonnulls(task_id,event_id)=1)
);

-- 13 Plans, prepaid subscriptions, payment and quota accounting -----------
CREATE TABLE billing.plans (
 id uuid PRIMARY KEY, code text NOT NULL UNIQUE, title text NOT NULL,
 active boolean NOT NULL DEFAULT true, created_at timestamptz NOT NULL DEFAULT now()
);
CREATE TABLE billing.plan_entitlements (
 plan_id uuid NOT NULL REFERENCES billing.plans(id) ON DELETE RESTRICT,
 resource text NOT NULL, period_kind text NOT NULL CHECK(period_kind IN ('daily','weekly','monthly','subscription','lifetime')),
 limit_amount bigint NOT NULL CHECK(limit_amount>=0),
 PRIMARY KEY(plan_id,resource,period_kind)
);
CREATE TABLE billing.payments (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 provider text NOT NULL, provider_payment_id text,
 amount_minor bigint NOT NULL CHECK(amount_minor>=0), currency char(3) NOT NULL DEFAULT 'IDR',
 state text NOT NULL DEFAULT 'pending' CHECK(state IN ('pending','paid','failed','refunded','cancelled')),
 created_at timestamptz NOT NULL DEFAULT now(), paid_at timestamptz,
 UNIQUE(id,data_space_id), UNIQUE(provider,provider_payment_id)
);
CREATE TABLE billing.subscription_periods (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 plan_id uuid NOT NULL REFERENCES billing.plans(id) ON DELETE RESTRICT,
 payment_id uuid, starts_at timestamptz NOT NULL, ends_at timestamptz NOT NULL,
 state text NOT NULL CHECK(state IN ('pending','active','expired','cancelled','refunded')),
 created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(id,data_space_id), CHECK(ends_at>starts_at),
 FOREIGN KEY(payment_id,data_space_id) REFERENCES billing.payments(id,data_space_id) ON DELETE RESTRICT
);
CREATE INDEX ix_subscription_active ON billing.subscription_periods(data_space_id,starts_at DESC,ends_at DESC) WHERE state='active';
-- Active prepaid periods must not overlap for the same Data Space.
-- If product introduces stacking/overlap, define a new explicit entitlement policy first.
ALTER TABLE billing.subscription_periods
 ADD CONSTRAINT ex_subscription_one_active_per_space
 EXCLUDE USING gist (data_space_id WITH =, tstzrange(starts_at,ends_at,'[)') WITH &&)
 WHERE (state = 'active');
CREATE TABLE billing.payment_webhook_events (
 id uuid PRIMARY KEY, provider text NOT NULL, external_event_id text NOT NULL,
 payment_id uuid, received_at timestamptz NOT NULL DEFAULT now(),
 signature_verified boolean NOT NULL DEFAULT false, payload_digest text,
 processed_at timestamptz, UNIQUE(provider,external_event_id),
 FOREIGN KEY(payment_id) REFERENCES billing.payments(id) ON DELETE RESTRICT
);
CREATE TABLE billing.quota_periods (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 starts_at timestamptz NOT NULL, ends_at timestamptz NOT NULL, period_kind text NOT NULL,
 UNIQUE(id,data_space_id), CHECK(ends_at>starts_at)
);
CREATE TABLE billing.quota_balances (
 data_space_id uuid NOT NULL, period_id uuid NOT NULL, resource text NOT NULL,
 limit_amount bigint NOT NULL CHECK(limit_amount>=0),
 reserved_amount bigint NOT NULL DEFAULT 0 CHECK(reserved_amount>=0),
 used_amount bigint NOT NULL DEFAULT 0 CHECK(used_amount>=0),
 PRIMARY KEY(data_space_id,period_id,resource),
 FOREIGN KEY(period_id,data_space_id) REFERENCES billing.quota_periods(id,data_space_id) ON DELETE RESTRICT,
 CHECK(used_amount + reserved_amount <= limit_amount)
);
CREATE TABLE billing.usage_reservations (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, job_id uuid NOT NULL,
 period_id uuid NOT NULL, resource text NOT NULL,
 reserved_amount bigint NOT NULL CHECK(reserved_amount>0),
 committed_amount bigint NOT NULL DEFAULT 0 CHECK(committed_amount>=0),
 state text NOT NULL DEFAULT 'reserved' CHECK(state IN ('reserved','settled','released')),
 created_at timestamptz NOT NULL DEFAULT now(), settled_at timestamptz,
 UNIQUE(id,data_space_id), UNIQUE(job_id,period_id,resource),
 FOREIGN KEY(job_id,data_space_id) REFERENCES ai.ai_jobs(id,data_space_id) ON DELETE RESTRICT,
 FOREIGN KEY(data_space_id,period_id,resource)
  REFERENCES billing.quota_balances(data_space_id,period_id,resource) ON DELETE RESTRICT,
 CHECK(committed_amount<=reserved_amount)
);
CREATE TABLE billing.usage_ledger (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL,
 reservation_id uuid NOT NULL, entry_type text NOT NULL CHECK(entry_type IN ('reserve','commit','release','adjust')),
 amount bigint NOT NULL CHECK(amount>=0), idempotency_key text NOT NULL,
 note text, created_at timestamptz NOT NULL DEFAULT now(),
 UNIQUE(data_space_id,idempotency_key),
 FOREIGN KEY(reservation_id,data_space_id)
  REFERENCES billing.usage_reservations(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE billing.ai_provider_usage (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, job_id uuid NOT NULL,
 invocation_id uuid NOT NULL,
 provider text NOT NULL, model text NOT NULL, provider_request_id text,
 input_tokens bigint NOT NULL DEFAULT 0 CHECK(input_tokens>=0),
 output_tokens bigint NOT NULL DEFAULT 0 CHECK(output_tokens>=0),
 cost_usd_micros bigint NOT NULL DEFAULT 0 CHECK(cost_usd_micros>=0),
 occurred_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(job_id,data_space_id) REFERENCES ai.ai_jobs(id,data_space_id) ON DELETE RESTRICT,
 UNIQUE(job_id,invocation_id),
 UNIQUE(provider,provider_request_id)
);

-- 14 Reliable work and deletion -------------------------------------------
CREATE TABLE ops.outbox_events (
 id uuid PRIMARY KEY, data_space_id uuid, event_type text NOT NULL,
 aggregate_id uuid NOT NULL, payload_json jsonb NOT NULL DEFAULT '{}'::jsonb,
 published_at timestamptz, attempts int NOT NULL DEFAULT 0 CHECK(attempts>=0),
 claim_token uuid, claimed_until timestamptz, next_attempt_at timestamptz NOT NULL DEFAULT now(),
 last_error_code text,
 created_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(data_space_id) REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT
);
CREATE INDEX ix_outbox_unpublished ON ops.outbox_events(next_attempt_at,created_at) WHERE published_at IS NULL;
CREATE TABLE ops.file_cleanup_jobs (
 id uuid PRIMARY KEY, data_space_id uuid NOT NULL, storage_object_id uuid NOT NULL,
 status text NOT NULL DEFAULT 'queued' CHECK(status IN ('queued','running','completed','failed')),
 attempt_count int NOT NULL DEFAULT 0 CHECK(attempt_count>=0),
 last_error_code text, created_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(storage_object_id,data_space_id) REFERENCES documents.storage_objects(id,data_space_id) ON DELETE RESTRICT
);
CREATE TABLE ops.operation_audit (
 id uuid PRIMARY KEY, data_space_id uuid, actor_user_id uuid,
 action text NOT NULL, subject_type text NOT NULL, subject_id uuid NOT NULL,
 outcome text NOT NULL, occurred_at timestamptz NOT NULL DEFAULT now(),
 FOREIGN KEY(data_space_id) REFERENCES ownership.data_spaces(id) ON DELETE RESTRICT,
 FOREIGN KEY(actor_user_id) REFERENCES identity.users(id) ON DELETE RESTRICT
);
COMMIT;
