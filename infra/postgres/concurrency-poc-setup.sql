-- StudyAI Campus / Concurrency Invariants Test Seed Setup
-- Database: studyai_tenant_poc_20261020

-- 1. Setup Note for Concurrent Editing (learning/knowledge) -------------------
DO $$
DECLARE
    v_space_id uuid := '0192a000-0000-7000-8000-0000000000aa';
    v_item_id uuid  := '0192a000-0000-7000-8000-0000000000c1';
    v_rev_id uuid   := '0192a000-0000-7000-8000-0000000000c2';
BEGIN
    DELETE FROM knowledge.note_revisions WHERE data_space_id = v_space_id AND note_id = v_item_id;
    DELETE FROM knowledge.notes WHERE data_space_id = v_space_id AND knowledge_item_id = v_item_id;
    DELETE FROM knowledge.knowledge_items WHERE data_space_id = v_space_id AND id = v_item_id;

    INSERT INTO knowledge.knowledge_items (id, data_space_id, kind, title)
    VALUES (v_item_id, v_space_id, 'note', 'Concurrency Test Note');

    INSERT INTO knowledge.notes (knowledge_item_id, data_space_id, item_kind, content_json, format_version, revision, search_text)
    VALUES (v_item_id, v_space_id, 'note', '{"ops":[{"insert":"Initial content\n"}]}', 1, 1, 'Initial content');

    INSERT INTO knowledge.note_revisions (id, data_space_id, note_id, revision, format_version, content_json, search_text)
    VALUES (v_rev_id, v_space_id, v_item_id, 1, 1, '{"ops":[{"insert":"Initial content\n"}]}', 'Initial content');
END $$;

-- 2. Setup Quota Balance for Concurrent Allocation (billing) ------------------
DO $$
DECLARE
    v_space_id uuid  := '0192a000-0000-7000-8000-0000000000aa';
    v_period_id uuid := '0192a000-0000-7000-8000-0000000000d1';
    v_job_id uuid    := '0192a000-0000-7000-8000-0000000000e1';
    v_res_id uuid    := '0192a000-0000-7000-8000-0000000000d2';
BEGIN
    DELETE FROM billing.usage_reservations WHERE data_space_id = v_space_id;
    DELETE FROM ai.ai_jobs WHERE data_space_id = v_space_id AND id = v_job_id;
    DELETE FROM billing.quota_balances WHERE data_space_id = v_space_id;
    DELETE FROM billing.quota_periods WHERE data_space_id = v_space_id;

    INSERT INTO billing.quota_periods (id, data_space_id, starts_at, ends_at, period_kind)
    VALUES (v_period_id, v_space_id, now() - interval '1 day', now() + interval '30 days', 'monthly');

    INSERT INTO billing.quota_balances (data_space_id, period_id, resource, limit_amount, reserved_amount, used_amount)
    VALUES (v_space_id, v_period_id, 'ai_tokens', 100, 0, 0);

    -- Setup job for usage reservation
    INSERT INTO ai.ai_jobs (id, data_space_id, feature, state, idempotency_key)
    VALUES (v_job_id, v_space_id, 'concurrency_test', 'reserved', 'test-job-key-001');

    -- Setup an existing reservation for idempotency test
    INSERT INTO billing.usage_reservations (id, data_space_id, job_id, period_id, resource, reserved_amount, committed_amount, state)
    VALUES (v_res_id, v_space_id, v_job_id, v_period_id, 'ai_tokens', 50, 0, 'reserved');
END $$;

-- 3. Setup Document Version for Duplicate Processing Test (documents) ----------
DO $$
DECLARE
    v_space_id uuid := '0192a000-0000-7000-8000-0000000000aa';
    v_ver_id uuid   := '0192a000-0000-7000-8000-0000000000a3';
BEGIN
    DELETE FROM documents.chunk_embeddings WHERE chunk_id IN (SELECT id FROM documents.document_chunks WHERE document_version_id = v_ver_id);
    DELETE FROM documents.document_chunks WHERE document_version_id = v_ver_id;
    DELETE FROM documents.processing_runs WHERE document_version_id = v_ver_id;
END $$;

-- 4. Setup Plan & Subscription for Overlap Test (billing) ---------------------
DO $$
DECLARE
    v_space_id uuid := '0192a000-0000-7000-8000-0000000000aa';
    v_plan_id uuid  := '0192a000-0000-7000-8000-0000000000f1';
    v_sub_id uuid   := '0192a000-0000-7000-8000-0000000000f2';
BEGIN
    DELETE FROM billing.subscription_periods WHERE data_space_id = v_space_id;
    DELETE FROM billing.plans WHERE id = v_plan_id;

    INSERT INTO billing.plans (id, code, title, active)
    VALUES (v_plan_id, 'campus-pro', 'Campus Pro Plan', true);

    -- Active subscription covering [today, today + 30 days)
    INSERT INTO billing.subscription_periods (id, data_space_id, plan_id, starts_at, ends_at, state)
    VALUES (v_sub_id, v_space_id, v_plan_id, '2026-10-20 00:00:00+00', '2026-11-20 00:00:00+00', 'active');
END $$;
