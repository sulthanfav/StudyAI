\set ON_ERROR_STOP on
\pset pager off

\echo === SERVER AND EXTENSIONS ===
SELECT version() AS postgres_version;
SELECT current_database() AS validation_database;
SELECT extname, extversion
FROM pg_extension
WHERE extname IN ('vector', 'pg_trgm', 'btree_gist')
ORDER BY extname;

DO $$
DECLARE
  server_num integer := current_setting('server_version_num')::integer;
  extension_count integer;
BEGIN
  IF server_num < 160000 THEN
    RAISE EXCEPTION 'PostgreSQL 16+ required, server_version_num=%', server_num;
  END IF;

  SELECT count(*) INTO extension_count
  FROM pg_extension
  WHERE extname IN ('vector', 'pg_trgm', 'btree_gist');

  IF extension_count <> 3 THEN
    RAISE EXCEPTION 'Expected vector, pg_trgm, and btree_gist; found % required extensions', extension_count;
  END IF;
END
$$;

\echo === STUDYAI SCHEMAS ===
SELECT nspname AS schema_name
FROM pg_namespace
WHERE nspname IN (
  'identity', 'ownership', 'workspace', 'knowledge', 'documents',
  'research', 'ai', 'learning', 'writing', 'presentation',
  'integrity', 'planner', 'billing', 'ops'
)
ORDER BY nspname;

DO $$
DECLARE
  schema_count integer;
BEGIN
  SELECT count(*) INTO schema_count
  FROM pg_namespace
  WHERE nspname IN (
    'identity', 'ownership', 'workspace', 'knowledge', 'documents',
    'research', 'ai', 'learning', 'writing', 'presentation',
    'integrity', 'planner', 'billing', 'ops'
  );

  IF schema_count <> 14 THEN
    RAISE EXCEPTION 'Expected 14 StudyAI schemas, found %', schema_count;
  END IF;
END
$$;

\echo === TABLE INVENTORY ===
SELECT schemaname, count(*) AS table_count
FROM pg_tables
WHERE schemaname IN (
  'identity', 'ownership', 'workspace', 'knowledge', 'documents',
  'research', 'ai', 'learning', 'writing', 'presentation',
  'integrity', 'planner', 'billing', 'ops'
)
GROUP BY schemaname
ORDER BY schemaname;

DO $$
DECLARE
  table_count integer;
BEGIN
  SELECT count(*) INTO table_count
  FROM pg_tables
  WHERE schemaname IN (
    'identity', 'ownership', 'workspace', 'knowledge', 'documents',
    'research', 'ai', 'learning', 'writing', 'presentation',
    'integrity', 'planner', 'billing', 'ops'
  );

  IF table_count <> 68 THEN
    RAISE EXCEPTION 'Expected 68 StudyAI tables, found %', table_count;
  END IF;
END
$$;

\echo === CONSTRAINT INVENTORY ===
SELECT
  CASE c.contype
    WHEN 'p' THEN 'primary_key'
    WHEN 'f' THEN 'foreign_key'
    WHEN 'u' THEN 'unique'
    WHEN 'c' THEN 'check'
    WHEN 'x' THEN 'exclusion'
    ELSE c.contype::text
  END AS constraint_type,
  count(*) AS constraint_count
FROM pg_constraint c
JOIN pg_namespace n ON n.oid = c.connamespace
WHERE n.nspname IN (
  'identity', 'ownership', 'workspace', 'knowledge', 'documents',
  'research', 'ai', 'learning', 'writing', 'presentation',
  'integrity', 'planner', 'billing', 'ops'
)
GROUP BY c.contype
ORDER BY constraint_type;

DO $$
DECLARE
  constraint_count integer;
  check_count integer;
  exclusion_count integer;
  foreign_key_count integer;
  primary_key_count integer;
  unique_count integer;
BEGIN
  SELECT
    count(*),
    count(*) FILTER (WHERE c.contype = 'c'),
    count(*) FILTER (WHERE c.contype = 'x'),
    count(*) FILTER (WHERE c.contype = 'f'),
    count(*) FILTER (WHERE c.contype = 'p'),
    count(*) FILTER (WHERE c.contype = 'u')
  INTO
    constraint_count,
    check_count,
    exclusion_count,
    foreign_key_count,
    primary_key_count,
    unique_count
  FROM pg_constraint c
  JOIN pg_namespace n ON n.oid = c.connamespace
  WHERE n.nspname IN (
    'identity', 'ownership', 'workspace', 'knowledge', 'documents',
    'research', 'ai', 'learning', 'writing', 'presentation',
    'integrity', 'planner', 'billing', 'ops'
  );

  IF (constraint_count, check_count, exclusion_count, foreign_key_count, primary_key_count, unique_count)
     <> (320, 84, 1, 107, 68, 60) THEN
    RAISE EXCEPTION
      'Unexpected constraint inventory: total=%, check=%, exclusion=%, foreign_key=%, primary_key=%, unique=%',
      constraint_count, check_count, exclusion_count, foreign_key_count, primary_key_count, unique_count;
  END IF;
END
$$;

\echo === INDEX INVENTORY ===
SELECT schemaname, count(*) AS index_count
FROM pg_indexes
WHERE schemaname IN (
  'identity', 'ownership', 'workspace', 'knowledge', 'documents',
  'research', 'ai', 'learning', 'writing', 'presentation',
  'integrity', 'planner', 'billing', 'ops'
)
GROUP BY schemaname
ORDER BY schemaname;

DO $$
DECLARE
  index_count integer;
BEGIN
  SELECT count(*) INTO index_count
  FROM pg_indexes
  WHERE schemaname IN (
    'identity', 'ownership', 'workspace', 'knowledge', 'documents',
    'research', 'ai', 'learning', 'writing', 'presentation',
    'integrity', 'planner', 'billing', 'ops'
  );

  IF index_count <> 157 THEN
    RAISE EXCEPTION 'Expected 157 StudyAI indexes, found %', index_count;
  END IF;
END
$$;

\echo === FUNCTION INVENTORY ===
SELECT n.nspname AS schema_name, p.proname AS function_name
FROM pg_proc p
JOIN pg_namespace n ON n.oid = p.pronamespace
WHERE n.nspname IN ('knowledge', 'documents', 'ai', 'ops')
  AND p.proname IN (
    'reject_snapshot_update',
    'guard_document_version_identity',
    'ensure_embedding_dimensions',
    'guard_embedding_model_identity',
    'guard_source_version_identity'
  )
ORDER BY n.nspname, p.proname;

\echo === TRIGGER INVENTORY ===
SELECT
  event_object_schema AS table_schema,
  event_object_table AS table_name,
  trigger_name,
  event_manipulation,
  action_timing
FROM information_schema.triggers
WHERE event_object_schema IN (
  'identity', 'ownership', 'workspace', 'knowledge', 'documents',
  'research', 'ai', 'learning', 'writing', 'presentation',
  'integrity', 'planner', 'billing', 'ops'
)
ORDER BY event_object_schema, event_object_table, trigger_name, event_manipulation;

DO $$
DECLARE
  function_count integer;
  application_function_count integer;
  trigger_count integer;
BEGIN
  SELECT count(*) INTO function_count
  FROM pg_proc p
  JOIN pg_namespace n ON n.oid = p.pronamespace
  WHERE (n.nspname, p.proname) IN (
    ('ops', 'reject_snapshot_update'),
    ('documents', 'guard_document_version_identity'),
    ('documents', 'ensure_embedding_dimensions'),
    ('ai', 'guard_embedding_model_identity'),
    ('knowledge', 'guard_source_version_identity')
  );

  IF function_count <> 5 THEN
    RAISE EXCEPTION 'Expected 5 critical trigger functions, found %', function_count;
  END IF;

  SELECT count(*) INTO application_function_count
  FROM pg_proc p
  JOIN pg_namespace n ON n.oid = p.pronamespace
  WHERE n.nspname IN (
    'identity', 'ownership', 'workspace', 'knowledge', 'documents',
    'research', 'ai', 'learning', 'writing', 'presentation',
    'integrity', 'planner', 'billing', 'ops'
  );

  IF application_function_count <> 5 THEN
    RAISE EXCEPTION 'Expected exactly 5 StudyAI functions, found %', application_function_count;
  END IF;

  SELECT count(*) INTO trigger_count
  FROM pg_trigger t
  JOIN pg_class c ON c.oid = t.tgrelid
  JOIN pg_namespace n ON n.oid = c.relnamespace
  WHERE NOT t.tgisinternal
    AND n.nspname IN (
      'identity', 'ownership', 'workspace', 'knowledge', 'documents',
      'research', 'ai', 'learning', 'writing', 'presentation',
      'integrity', 'planner', 'billing', 'ops'
    );

  IF trigger_count <> 8 THEN
    RAISE EXCEPTION 'Expected 8 StudyAI triggers, found %', trigger_count;
  END IF;
END
$$;

\echo === CRITICAL OBJECTS ===
SELECT
  to_regclass('workspace.item_placements') AS item_placements,
  to_regclass('documents.document_chunks') AS document_chunks,
  to_regclass('documents.chunk_embeddings') AS chunk_embeddings,
  to_regclass('billing.usage_reservations') AS usage_reservations,
  to_regclass('billing.subscription_periods') AS subscription_periods;

SELECT schemaname, indexname, indexdef
FROM pg_indexes
WHERE (schemaname, indexname) IN (
  ('workspace', 'ux_active_item_per_content_space'),
  ('documents', 'ux_processing_active_run'),
  ('billing', 'ix_subscription_active')
)
ORDER BY schemaname, indexname;

SELECT conname, pg_get_constraintdef(oid) AS definition
FROM pg_constraint
WHERE conname IN (
  'ex_subscription_one_active_per_space',
  'fk_document_current_version'
)
ORDER BY conname;

DO $$
DECLARE
  critical_table_count integer;
  critical_index_count integer;
  critical_constraint_count integer;
BEGIN
  SELECT count(*) INTO critical_table_count
  FROM unnest(ARRAY[
    to_regclass('workspace.item_placements'),
    to_regclass('documents.document_chunks'),
    to_regclass('documents.chunk_embeddings'),
    to_regclass('billing.usage_reservations'),
    to_regclass('billing.subscription_periods')
  ]) AS relation_oid
  WHERE relation_oid IS NOT NULL;

  SELECT count(*) INTO critical_index_count
  FROM pg_indexes
  WHERE (schemaname, indexname) IN (
    ('workspace', 'ux_active_item_per_content_space'),
    ('documents', 'ux_processing_active_run'),
    ('billing', 'ix_subscription_active')
  );

  SELECT count(*) INTO critical_constraint_count
  FROM pg_constraint
  WHERE conname IN (
    'ex_subscription_one_active_per_space',
    'fk_document_current_version'
  );

  IF (critical_table_count, critical_index_count, critical_constraint_count) <> (5, 3, 2) THEN
    RAISE EXCEPTION
      'Missing critical objects: tables=%, indexes=%, constraints=%',
      critical_table_count, critical_index_count, critical_constraint_count;
  END IF;
END
$$;

\echo === EMPTY DATABASE CHECK ===
SELECT count(*) AS identity_user_rows_before_smoke
FROM identity.users;

