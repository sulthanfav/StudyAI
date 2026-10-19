-- StudyAI DB smoke tests v1.1; READ FIRST.
-- PRECONDITION: isolated/empty PostgreSQL database with
--   StudyAI_Physical_Schema_Reviewed_v1.1.sql successfully applied.
-- TEST ONLY: IDs below are deterministic non-v7 UUIDs for repeatable fixtures.
-- Each test traps the expected SQLSTATE. Do NOT execute on production.
-- Entire fixture setup is rolled back, even if all tests pass.
BEGIN;

INSERT INTO identity.users (id,email,display_name) VALUES
 ('10000000-0000-4000-8000-000000000001','smoke-a@example.invalid','Student A'),
 ('10000000-0000-4000-8000-000000000002','smoke-b@example.invalid','Student B');
INSERT INTO ownership.data_spaces (id,owner_user_id,kind,name) VALUES
 ('20000000-0000-4000-8000-000000000001','10000000-0000-4000-8000-000000000001','personal','A'),
 ('20000000-0000-4000-8000-000000000002','10000000-0000-4000-8000-000000000002','personal','B');
INSERT INTO workspace.content_spaces(id,data_space_id,kind,title) VALUES
 ('30000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000001','library','Library A'),
 ('30000000-0000-4000-8000-000000000002','20000000-0000-4000-8000-000000000002','library','Library B');
INSERT INTO knowledge.knowledge_items(id,data_space_id,kind,title) VALUES
 ('40000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000001','note','Note A'),
 ('40000000-0000-4000-8000-000000000002','20000000-0000-4000-8000-000000000002','note','Note B'),
 ('40000000-0000-4000-8000-000000000003','20000000-0000-4000-8000-000000000001','document','PDF A');
INSERT INTO knowledge.notes(knowledge_item_id,data_space_id,content_json,format_version) VALUES
 ('40000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000001','{}',1),
 ('40000000-0000-4000-8000-000000000002','20000000-0000-4000-8000-000000000002','{}',1);

-- TEST 01: cannot place another tenant's item in A's library.
DO $$
DECLARE rejected boolean := false;
BEGIN
 BEGIN
  INSERT INTO workspace.item_placements(id,data_space_id,content_space_id,knowledge_item_id,item_kind)
  VALUES ('50000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000001',
          '30000000-0000-4000-8000-000000000001','40000000-0000-4000-8000-000000000002','note');
 EXCEPTION WHEN foreign_key_violation THEN rejected := true;
 END;
 IF NOT rejected THEN RAISE EXCEPTION 'FAIL 01: cross-tenant placement accepted'; END IF;
 RAISE NOTICE 'PASS 01: cross-tenant placement blocked';
END $$;

-- TEST 02: cannot insert note subtype for a document knowledge item.
DO $$
DECLARE rejected boolean := false;
BEGIN
 BEGIN
  INSERT INTO knowledge.notes(knowledge_item_id,data_space_id,content_json,format_version)
  VALUES ('40000000-0000-4000-8000-000000000003','20000000-0000-4000-8000-000000000001','{}',1);
 EXCEPTION WHEN foreign_key_violation THEN rejected := true;
 END;
 IF NOT rejected THEN RAISE EXCEPTION 'FAIL 02: subtype mismatch accepted'; END IF;
 RAISE NOTICE 'PASS 02: subtype mismatch blocked';
END $$;

INSERT INTO knowledge.note_revisions(id,data_space_id,note_id,revision,format_version,content_json) VALUES
 ('60000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000001','40000000-0000-4000-8000-000000000001',1,1,'{}'),
 ('60000000-0000-4000-8000-000000000002','20000000-0000-4000-8000-000000000001','40000000-0000-4000-8000-000000000001',2,1,'{}');
INSERT INTO knowledge.source_versions(id,data_space_id,knowledge_item_id,source_kind,note_revision_id)
VALUES ('70000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000001',
        '40000000-0000-4000-8000-000000000001','note','60000000-0000-4000-8000-000000000001');

-- TEST 03: used provenance pointer cannot change.
DO $$
DECLARE rejected boolean := false;
BEGIN
 BEGIN
  UPDATE knowledge.source_versions
   SET note_revision_id='60000000-0000-4000-8000-000000000002'
  WHERE id='70000000-0000-4000-8000-000000000001';
 EXCEPTION WHEN check_violation THEN rejected := true;
 END;
 IF NOT rejected THEN RAISE EXCEPTION 'FAIL 03: provenance identity was mutable'; END IF;
 RAISE NOTICE 'PASS 03: immutable provenance';
END $$;

-- TEST 03A: a note revision used by source versions cannot mutate.
DO $$
DECLARE rejected boolean := false;
BEGIN
 BEGIN
  UPDATE knowledge.note_revisions SET content_json='{"tampered":true}'::jsonb
  WHERE id='60000000-0000-4000-8000-000000000001';
 EXCEPTION WHEN check_violation THEN rejected := true;
 END;
 IF NOT rejected THEN RAISE EXCEPTION 'FAIL 03A: note source snapshot mutated'; END IF;
 RAISE NOTICE 'PASS 03A: note source snapshot immutable';
END $$;

INSERT INTO documents.storage_objects(id,data_space_id,storage_key,checksum_sha256,size_bytes,mime_type,state)
VALUES('80000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000001',
       'smoke/a/object1', repeat('a',64),10,'application/pdf','verified');
INSERT INTO documents.documents(knowledge_item_id,data_space_id,original_filename)
VALUES('40000000-0000-4000-8000-000000000003','20000000-0000-4000-8000-000000000001','a.pdf');
INSERT INTO documents.document_versions(id,data_space_id,document_id,version_no,storage_object_id)
VALUES('90000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000001',
       '40000000-0000-4000-8000-000000000003',1,'80000000-0000-4000-8000-000000000001');
-- TEST 03B: document version cannot silently change its storage identity.
DO $$
DECLARE rejected boolean := false;
BEGIN
 BEGIN
  UPDATE documents.document_versions SET version_no=9
  WHERE id='90000000-0000-4000-8000-000000000001';
 EXCEPTION WHEN check_violation THEN rejected := true;
 END;
 IF NOT rejected THEN RAISE EXCEPTION 'FAIL 03B: document version was mutable'; END IF;
 RAISE NOTICE 'PASS 03B: document version identity immutable';
END $$;

INSERT INTO documents.processing_runs(id,data_space_id,document_version_id,processor_version,status,is_active)
VALUES ('a0000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000001',
        '90000000-0000-4000-8000-000000000001','extract-v1','ready',true),
       ('a0000000-0000-4000-8000-000000000002','20000000-0000-4000-8000-000000000001',
        '90000000-0000-4000-8000-000000000001','extract-v2','ready',false);

-- TEST 04: only one active processing run per document version.
DO $$
DECLARE rejected boolean := false;
BEGIN
 BEGIN
  UPDATE documents.processing_runs SET is_active=true
   WHERE id='a0000000-0000-4000-8000-000000000002';
 EXCEPTION WHEN unique_violation THEN rejected := true;
 END;
 IF NOT rejected THEN RAISE EXCEPTION 'FAIL 04: two active processing runs'; END IF;
 RAISE NOTICE 'PASS 04: single active processing run';
END $$;

INSERT INTO billing.plans(id,code,title) VALUES
 ('b0000000-0000-4000-8000-000000000001','smoke-free','Smoke Free');
INSERT INTO billing.subscription_periods(id,data_space_id,plan_id,starts_at,ends_at,state)
VALUES ('c0000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000001',
        'b0000000-0000-4000-8000-000000000001','2026-11-01T00:00:00Z','2026-12-01T00:00:00Z','active');

-- TEST 05: overlapping active subscriptions rejected.
DO $$
DECLARE rejected boolean := false;
BEGIN
 BEGIN
  INSERT INTO billing.subscription_periods(id,data_space_id,plan_id,starts_at,ends_at,state)
  VALUES ('c0000000-0000-4000-8000-000000000002','20000000-0000-4000-8000-000000000001',
          'b0000000-0000-4000-8000-000000000001','2026-11-15T00:00:00Z','2026-12-15T00:00:00Z','active');
 EXCEPTION WHEN exclusion_violation THEN rejected := true;
 END;
 IF NOT rejected THEN RAISE EXCEPTION 'FAIL 05: overlapping active subscriptions'; END IF;
 RAISE NOTICE 'PASS 05: active subscription periods do not overlap';
END $$;

-- TEST 06: stale note write affects no rows, so API must return 409.
DO $$
DECLARE affected integer;
BEGIN
 UPDATE knowledge.notes SET revision=revision+1
 WHERE knowledge_item_id='40000000-0000-4000-8000-000000000001'
   AND data_space_id='20000000-0000-4000-8000-000000000001' AND revision=99;
 GET DIAGNOSTICS affected = ROW_COUNT;
 IF affected <> 0 THEN RAISE EXCEPTION 'FAIL 06: stale save unexpectedly updated note'; END IF;
 RAISE NOTICE 'PASS 06: stale note version detected';
END $$;

INSERT INTO billing.quota_periods(id,data_space_id,starts_at,ends_at,period_kind)
VALUES('d0000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000001',
       '2026-11-01T00:00:00Z','2026-12-01T00:00:00Z','monthly');
INSERT INTO billing.quota_balances(data_space_id,period_id,resource,limit_amount)
VALUES('20000000-0000-4000-8000-000000000001','d0000000-0000-4000-8000-000000000001','ai_credit',10);
INSERT INTO ai.ai_jobs(id,data_space_id,feature,idempotency_key)
VALUES('e0000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000001','tutor','smoke-job-1');
UPDATE billing.quota_balances SET reserved_amount=reserved_amount+8
WHERE data_space_id='20000000-0000-4000-8000-000000000001'
 AND period_id='d0000000-0000-4000-8000-000000000001' AND resource='ai_credit'
 AND used_amount+reserved_amount+8<=limit_amount;
INSERT INTO billing.usage_reservations(id,data_space_id,job_id,period_id,resource,reserved_amount)
VALUES('f0000000-0000-4000-8000-000000000001','20000000-0000-4000-8000-000000000001',
       'e0000000-0000-4000-8000-000000000001','d0000000-0000-4000-8000-000000000001','ai_credit',8);

-- TEST 07: cannot overdraw quota after reservation.
DO $$
DECLARE affected integer;
BEGIN
 UPDATE billing.quota_balances SET reserved_amount=reserved_amount+5
 WHERE data_space_id='20000000-0000-4000-8000-000000000001'
  AND period_id='d0000000-0000-4000-8000-000000000001' AND resource='ai_credit'
  AND used_amount+reserved_amount+5<=limit_amount;
 GET DIAGNOSTICS affected = ROW_COUNT;
 IF affected <> 0 THEN RAISE EXCEPTION 'FAIL 07: quota overdraw was allowed'; END IF;
 RAISE NOTICE 'PASS 07: quota overdraw blocked';
END $$;

-- TEST 08: multiple limit periods can reserve same job/resource under distinct periods.
INSERT INTO billing.quota_periods(id,data_space_id,starts_at,ends_at,period_kind)
VALUES('d0000000-0000-4000-8000-000000000002','20000000-0000-4000-8000-000000000001',
       '2026-11-01T00:00:00Z','2026-11-02T00:00:00Z','daily');
INSERT INTO billing.quota_balances(data_space_id,period_id,resource,limit_amount)
VALUES('20000000-0000-4000-8000-000000000001','d0000000-0000-4000-8000-000000000002','ai_credit',8);
INSERT INTO billing.usage_reservations(id,data_space_id,job_id,period_id,resource,reserved_amount)
VALUES('f0000000-0000-4000-8000-000000000002','20000000-0000-4000-8000-000000000001',
       'e0000000-0000-4000-8000-000000000001','d0000000-0000-4000-8000-000000000002','ai_credit',8);
-- Keep both test counters aligned with their reservation rows; production needs
-- one service transaction covering all applicable quota periods, unlike this fixture.
UPDATE billing.quota_balances SET reserved_amount=reserved_amount+8
 WHERE data_space_id='20000000-0000-4000-8000-000000000001'
  AND period_id='d0000000-0000-4000-8000-000000000002' AND resource='ai_credit'
  AND used_amount+reserved_amount+8<=limit_amount;
-- The test proves the new unique key permits both rows; it does NOT prove
-- that a production reserve/settle state machine is atomic under concurrency.
DO $$ BEGIN RAISE NOTICE 'PASS 08: same job/resource can be bound to daily and monthly periods'; END $$;

ROLLBACK;
-- Note: passing these 10 tests is necessary but not sufficient for production sign-off.
