# StudyAI Campus — Database Implementation Review & EF Core Handoff

> **Dokumen:** Technical Implementation Review / Migration Readiness  
> **Versi:** 1.1 — 13 Oktober 2026  
> **Status:** **REVIEWED DRAFT / NOT PRODUCTION APPROVED**  
> **Produk:** StudyAI Campus V1 (B2C mahasiswa, sembilan modul)  
> **Landasan:** `StudyAI_Campus_MVP_Final_PRD_v1.0.md`, `StudyAI_Core_Architecture_v1.2.md`, `StudyAI_Physical_ERD_and_SQL_Handbook_v1.0.md`  
> **SQL sumber:** `StudyAI_Physical_Schema_Reference_v1.0.sql`  
> **SQL hasil review:** `StudyAI_Physical_Schema_Reviewed_v1.1.sql`  
> **Test referensi:** `StudyAI_DB_Smoke_Tests_v1.1.sql`

## 0. Ringkasan eksekutif

Struktur utama dari Physical ERD v1.0 **dipertahankan**: PostgreSQL + pgvector, satu database modular, satu `DbContext` awal, canonical `knowledge_items`, `content_spaces` untuk Library/Course/Project, `item_placements`, source versions, scoped retrieval, serta penggunaan ledger untuk biaya AI.

Audit menemukan beberapa gap *implementasi*, bukan konflik visi produk. SQL v1.1 membuat revisi **tanpa menghapus tabel atau mengubah scope sembilan modul**. Semua perubahan bersifat **proposal implementasi** dan wajib diuji di PostgreSQL sebenarnya sebelum migration ditandatangani.

### Hasil audit yang benar-benar dilakukan

| Pemeriksaan | Hasil |
|---|---|
| Jumlah tabel | **68** (tetap) |
| Foreign key eksplisit | **85 composite/table-level FK dalam `CREATE TABLE`, ditambah 1 composite FK lewat ALTER dan 21 inline references** |
| Target composite foreign key dibanding candidate unique key | **85/85 ditemukan** untuk FK `CREATE TABLE`; satu `ALTER TABLE` dan 21 inline references juga memiliki tabel target tersedia; target kunci perlu runtime verification |
| Tabel target referensi inline tersedia | Tidak ditemukan referensi ke tabel yang hilang |
| Perbaikan scope per tenant | Composite FK asli dipertahankan |
| Query utama dan workflow billing | Ditinjau secara manual, beberapa revisi ditandai |
| PostgreSQL DDL dieksekusi pada server sesungguhnya | **Belum** — server PostgreSQL + pgvector tidak tersedia pada runtime pemeriksaan |
| EF Core entities berhasil dikompilasi | **Belum** — source lengkap belum dibuat dan .NET SDK tidak tersedia pada runtime pemeriksaan |
| Integration/load/security test | **Belum** — ada naskah smoke tests dan rencana pengujian |

**Penting:** static verification tidak sama dengan database migration yang lulus runtime test. Jangan menggunakan dokumen atau SQL ini untuk mengubah database production.

---

## 1. Hierarki dokumen dan scope

1. **PRD Final v1.0:** *apa* yang harus dapat dilakukan StudyAI Campus V1, termasuk 9 modul, acceptance, dan partner-gated similarity.
2. **Core Architecture v1.2 (approved):** invariant konseptual; kepemilikan, hubungan konten, provenance, kuota dan keamanan.
3. **Physical ERD v1.0:** sketsa teknis lengkap 68 tabel dan query sebagai proposal.
4. **Implementation Review v1.1 (dokumen ini):** temuan spesifik, SQL review candidate, dan test/EF migration handoff; **tidak mengubah scope produk**.

Ketika terjadi benturan, PRD menang untuk behavior produk; Core Architecture menang untuk invariant yang disetujui; Physical SQL bisa diperbaiki melalui ADR + migration.

---

## 2. Temuan audit — tingkat prioritas

**P0** = perlu diselesaikan sebelum initial migration candidate diterima. **P1** = perlu ditangani sebelum fitur/domain terkait dibuka di produksi. **P2** = diukur/dioptimalkan berdasarkan evidence dan benchmark.

| ID | Severity | Area | Temuan v1.0 | Tindakan v1.1 / status |
|---|---|---|---|---|
| DBR-01 | **P0** | Quota multiperiod | `UNIQUE(job_id,resource)` melarang job yang sama dibatasi harian **dan** bulanan. | Ganti ke `UNIQUE(job_id,period_id,resource)`; **SQL diperbarui**. |
| DBR-02 | **P0** | Re-extraction | Lebih dari satu `processing_run` dengan status `ready` bisa membuat RAG membaca chunk dua kali. | Tambahkan `is_active`, CHECK dan unique partial index; **SQL diperbarui**. Semua query retrieval harus filter run aktif. |
| DBR-03 | **P0** | Immutable provenance | `source_versions` bisa diarahkan ulang ke snapshot lain lewat UPDATE, membuat sitasi historis menyesatkan. | Tambahkan trigger immutable identity; **SQL diperbarui**. |
| DBR-04 | **P0** | Subscription periods | Dua record `active` untuk pemilik yang sama bisa saling tumpang-tindih. | Partial GiST exclusion constraint dengan `btree_gist`; **SQL diperbarui**. Keputusan produk jika ingin *stacked periods* harus ADR tersendiri. |
| DBR-05 | **P1** | Document version | `storage_object_id` / `version_no` bisa berubah tanpa membentuk versi baru. | Tambahkan trigger immutable identity; **SQL diperbarui**. |
| DBR-06 | **P1** | Provider cost | Tidak ada identitas invocation lokal yang wajib ketika provider request ID tidak tersedia. | Tambahkan `invocation_id NOT NULL` dan `UNIQUE(job_id,invocation_id)`; **SQL diperbarui**. |
| DBR-07 | **P1** | Embedding catalog | Metadata dimensi/model dapat diubah setelah embedding dibuat. | Trigger menolak perubahan metadata model yang sedang digunakan; **SQL diperbarui**. |
| DBR-08 | **P1** | Retrieval query | Q12–Q14 v1.0 belum memfilter `processing_run` aktif; Q14 belum optimal untuk top-K. | Query revisi §5; **handbook v1.0 query disupersede**. |
| DBR-09 | **P1** | User content lifecycle | Parent page/folder yang di-remove atau masuk Trash dapat meninggalkan descendant aktif secara konseptual. | Tetapkan transaksi subtree + recovery rules; **belum ditegakkan oleh FK saja**. |
| DBR-10 | **P1** | RLS & workers | RLS belum diaktifkan di clean-slate schema; tidak boleh mengandalkan query filter sebagai satu-satunya bukti keamanan. | Role isolation, RLS trial, transaction-local context dan negative tests; **OPEN hingga diuji**. |
| DBR-11 | **P1** | Billing reconciliation | Ledger/projection dapat menyimpang bila service tidak memakai transaksi atomik; crash di tengah provider call harus ditangani. | State machine + invariants + compensating reconciliation; **service code/test masih wajib**. |
| DBR-12 | **P1** | Payment/webhook | Signature/order/value checks tidak dapat dijamin oleh DDL; duplicate webhook dan upgrade/downgrade perlu transisi terkontrol. | Verifikasi trusted provider lalu inbox idempotent dan aktivasi di satu transaksi; **service code/test masih wajib**. |
| DBR-13 | **P2** | Indexes/read path | Beberapa foreign-key-side/read-path indexes dapat dibutuhkan setelah traffic tumbuh. | Tambahkan indeks pesan/conversation dan chunks/run; benchmark lanjut. |
| DBR-14 | **P2** | Text & embedding | Bahasa Indonesia campuran Inggris, model embedding/dimensi, HNSW dan query plan belum diukur. | Evaluasi dokumen nyata; jangan menganggap ANN wajib. |
| DBR-15 | **P1** | Note checkpoints | Save `revision=13` belum tentu mempunyai immutable `note_revisions` snapshot. | Buat checkpoint snapshot atomik saat versi benar-benar digunakan AI/dirujuk; **service policy**. |
| DBR-17 | **P0** | Mutable snapshot payload | `note_revisions`, `reference_snapshots`, `saved_output_versions`, dan `document_chunks` masih bisa di-UPDATE sehingga pointer immutable saja belum cukup. | Tambah trigger tolak UPDATE snapshot; **SQL diperbarui**. |
| DBR-16 | **P1** | Auth & provisioning | `identity.users` bukan tabel credential; provider auth belum dipilih. | Pisahkan authentication migration, lock ADR dan migrasi contract; **OPEN**. |

### Mengapa DBR-01 sangat penting?

Mahasiswa bisa punya paket dengan batas, misalnya **8 pesan AI per hari** dan **200 pesan per bulan** (hanya ilustrasi). Satu AI job harus mengurangi kedua counter secara konsisten. Jika `UNIQUE(job_id,resource)` tetap dipakai, job tersebut tidak bisa memiliki dua reservasi `ai_credit` dengan periode berbeda. Sebab itu uniqueness harus memasukkan `period_id`.

**Konsekuensi wajib di service:** untuk satu AI job, reserve semua periode yang relevan **dalam satu transaksi**; urutkan lock berdasarkan `(data_space_id,period_id,resource)` untuk mengurangi risiko deadlock. Jika salah satu periode penuh, batalkan semuanya. Settle/release semua record secara deterministik, tanpa debit dua kali. **Counter harian dan bulanan mencatat satu aktivitas yang sama** pada dua horizon limit—bukan dua kali tagihan provider atau dua kali pengurangan saldo finansial.

### Mengapa DBR-02 penting?

Re-upload file membuat `document_version` baru. Tetapi **re-processing** file versi yang sama (misalnya processor diperbaiki) menghasilkan beberapa `processing_runs` dan chunk set untuk satu version. Query yang hanya filter `status='ready'` akan menampilkan chunk lama dan baru bersamaan. Tambahkan `is_active` dan hanya satu run yang boleh aktif.

Saat publish reprocessing: dalam transaksi (1) lock `document_version`, (2) pastikan run baru `ready`, (3) nonaktifkan run lama, (4) aktifkan run baru, (5) ubah status/version yang relevan, (6) tulis outbox. **Jangan nonaktifkan run lama sebelum konten pengganti siap**, kecuali terdapat mekanisme rollback yang aman.

### Mengapa DBR-03 dan DBR-05 penting?

Sitasi AI harus tetap menunjuk versi file/note/jurnal yang sama ketika diciptakan. Bila ID *source version* tetap tetapi snapshot dibelokkan, audit histori jadi tidak bisa dipercaya. Trigger menjaga pointer identity dan isi source snapshot (`note_revisions`, `reference_snapshots`, `saved_output_versions`, `document_chunks`) agar tidak berubah melalui UPDATE biasa. Kebijakan retention tetap mengatur kapan versi asli dan snapshot boleh dihapus atau dianonimkan.

---

## 3. Revised SQL — daftar perubahan fisik

**File:** `StudyAI_Physical_Schema_Reviewed_v1.1.sql`. Dibuat dari SQL v1.0 sebagai **clean-slate schema candidate**. Bukan patch yang boleh diterapkan langsung pada database yang sudah berisi data. Jika ada instance existing, tulis migration incremental dengan pra-kondisi, data cleanup, `NOT VALID`/validasi bila relevan, downtime assessment, dan rollback plan.

| Perubahan | Objektif | Enforced by |
|---|---|---|
| Tambah `btree_gist` | Penerapan equality UUID + range GiST | PostgreSQL extension |
| `usage_reservations` unique 3 kolom | Multiperiod quotas | Unique constraint |
| `processing_runs.is_active` | Satu hasil ekstraksi aktif | CHECK + unique partial index |
| `knowledge.guard_source_version_identity()` | Snapshot source tidak diubah | Trigger |
| `documents.guard_document_version_identity()` | File version tidak diubah ke object lain | Trigger |
| `ai.guard_embedding_model_identity()` | Embedding catalog tidak diubah setelah dipakai | Trigger |
| `ops.reject_snapshot_update()` + triggers | Mencegah perubahan isi source snapshot/chunk lama | Triggers |
| `ai_provider_usage.invocation_id` | Dedup per provider call lokal | `NOT NULL` + unique |
| Active subscription exclusion | Tidak ada overlap antarperiode aktif | GiST exclusion |
| Indeks akses tinggi | History messages dan chunk run | B-tree indexes |

### SQL contoh: active subscription exclusion

```sql
CREATE EXTENSION IF NOT EXISTS btree_gist;

ALTER TABLE billing.subscription_periods
  ADD CONSTRAINT ex_subscription_one_active_per_space
  EXCLUDE USING gist (
    data_space_id WITH =,
    tstzrange(starts_at, ends_at, '[)') WITH &&
  )
  WHERE (state = 'active');
```

`[)` bermakna awal termasuk, akhir tidak. Periode 1–30 November diikuti periode mulai 30 November tidak overlap. Jika penerapan upgrade plan mengharuskan mulai sebelum akhir periode sebelumnya, service harus mengakhiri/menyesuaikan periode lama lebih dahulu **dengan aturan proration yang disepakati**. Alternatif tanpa `btree_gist` dapat menggunakan lock/transaksi plus test, tetapi tidak memiliki pengaman exclusion yang setara secara langsung.

### SQL contoh: processing runs aktif

```sql
-- Pada tabel baru dalam reviewed schema:
-- is_active boolean NOT NULL DEFAULT false,
-- CHECK (NOT is_active OR status = 'ready')

CREATE UNIQUE INDEX ux_processing_active_run
  ON documents.processing_runs(document_version_id)
  WHERE is_active;
```

**Caveat:** `unique partial index` melindungi satu run aktif secara global per document version, namun belum memastikan **setiap** dokumen dengan status `ready` pasti punya active run. Itu merupakan invariant publish service + reconciliation query.

---

## 4. Transaction recipes wajib

### 4.1 Create note baru

Satu transaksi: `knowledge_items` (`kind='note'`) → `notes` → checkpoint `note_revisions` pertama → `source_versions` → canonical Library `item_placements` → outbox. Jangan commit jika penempatan canonical gagal atau Library bukan milik pengguna.

### 4.2 Notes autosave

`UPDATE notes SET revision=revision+1, content_json=..., search_text=... WHERE note_id=? AND data_space_id=? AND revision=? RETURNING revision`. Jika 0 baris: HTTP 409. Pada saat source AI akan menggunakan revisi spesifik, buat snapshot immutable jika belum ada untuk revisi itu; ikat job ke `source_version_id` tersebut **dalam transaksi yang konsisten**. Jangan menyimpan snapshot revision yang sebenarnya tidak pernah dibaca.

### 4.3 Nested page/folder reparent

Satu transaction-scoped advisory lock per `content_space_id`, validasi parent aktif/se-owner/se-space, recursive CTE untuk mendeteksi siklus, kemudian UPDATE. Semua operasi move yang relevan harus mengikuti lock convention sama. Saat remove parent, tetapkan **subtree policy**: (a) remove subtree dengan snapshot restore, atau (b) reparent descendant secara atomik; jangan meninggalkan child aktif dengan parent tersembunyi tanpa aturan UI.

### 4.4 Upload + reprocessing

Upload file ke object storage privat dalam temporary staging, validasi MIME/magic/checksum/size dan izin pemrosesan, commit metadata `storage_objects`/`document_versions` + outbox, worker ekstraksi membangun chunk baru, kemudian publish satu `processing_runs.is_active=true`. Jika DB gagal setelah upload, outbox/cleanup dapat menghapus orphan setelah TTL; jika ekstraksi gagal, status untuk UI harus dapat di-retry tanpa membuat file asli ganda.

### 4.5 AI reserve/settlement

State machine reference:

```text
ai_jobs: queued → reserved → running → succeeded|failed|cancelled
usage_reservations: reserved → settled|released

reserve(job, resource, applicable_periods):
  BEGIN
  lock job, check idempotency
  for periods in deterministic order:
    atomically UPDATE quota_balances
      SET reserved_amount += estimate
      WHERE used + reserved + estimate <= limit
    assert exactly one affected row
    insert usage_reservations + ledger(reserve)
  COMMIT

settle(job, actual):
  BEGIN
  lock job and all reservations in stable order
  assert one terminal transition only
  for each reservation:
    adjust reserved -= estimate; used += actual_for_resource
    add commit and release ledger events with distinct stable keys
    set reservation state = settled
  mark AI job terminal, outbox
  COMMIT
```

Provider invoicing is separate from student credits: `ai_provider_usage` records every actual provider attempt (`invocation_id` per attempt). A provider may bill an unsuccessful or timed-out call. When actual exceeds estimate, **reserve additional amount first** or cap/abort before further chargeable work; never violate quota check silently. For zero-cost service-specific failure, choose user refund policy explicitly in Unit Economics/PRD.

`usage_ledger` is an **audit trail**, `quota_balances` a **projection**. Implement periodic reconciliation: `(reserved,total_used)` must equal derived events/reservations with documented semantics, and discrepancies must alert operations. Only permit `adjust` by controlled audited service path.

### 4.6 Verified payment → entitlement

1. Verify provider notification authenticity with server-side secrets; never trust browser redirect.
2. Validate invoice/order mapping, provider state, payment amount, currency, tenant, policy for duplicate/refund.
3. Begin transaction: insert verified inbox event idempotently, update correct payment, transition subscription period, reconcile entitlements/outbox.
4. Finish transaction; acknowledge callback. Duplicate callback must remain side-effect free.
5. Reconciliation worker queries provider if webhook missed or local state uncertain.

Policy for active overlap, cancellation, refund, upgrade, downgrade, grace period, and zero-cost Free plan must be an ADR before payments launch.

---

## 5. Query cookbook — pengganti Q12/Q13/Q14 dari handbook v1.0

**Semua query di bawah harus dijalankan setelah:** job diautentikasi, `data_space_id` diambil dari konteks server, `ai_job_sources` dibangun dari source versions yang diizinkan, dan body prompt dinormalisasi/divalidasi. Jangan mengambil tenant ID dari klaim request yang belum diverifikasi. `LIMIT` dibatasi di server. Query retrieval hanya untuk **dokumen**; notes dan bibliografi menggunakan retriever sendiri dengan ownership contract yang sama.

### Q12-R — Keyword retrieval versi yang dipilih, hanya run aktif

```sql
-- $1 data_space_id, $2 job_id, $3 query, $4 top_k
SELECT c.id AS chunk_id, sv.id AS source_version_id,
       c.content_text, c.locator,
       ts_rank(c.search_vector, plainto_tsquery('simple',$3)) AS rank
FROM ai.ai_job_sources js
JOIN knowledge.source_versions sv
 ON sv.id=js.source_version_id AND sv.data_space_id=js.data_space_id
JOIN documents.document_chunks c
 ON c.document_version_id=sv.document_version_id
 AND c.data_space_id=sv.data_space_id
JOIN documents.processing_runs r
 ON r.id=c.processing_run_id AND r.data_space_id=c.data_space_id
WHERE js.job_id=$2 AND js.data_space_id=$1
 AND sv.source_kind='document'
 AND r.is_active AND r.status='ready'
 AND c.search_vector @@ plainto_tsquery('simple',$3)
ORDER BY rank DESC,c.id LIMIT $4;
```

### Q13-R — Exact vector retrieval, hanya run aktif

```sql
-- $1 data_space_id, $2 job_id, $3 query_embedding, $4 embedding_model_id, $5 top_k
SELECT c.id AS chunk_id, sv.id AS source_version_id, c.locator,
       c.content_text, 1-(e.embedding <=> $3::vector) AS cosine_similarity
FROM ai.ai_job_sources js
JOIN knowledge.source_versions sv
 ON sv.id=js.source_version_id AND sv.data_space_id=js.data_space_id
JOIN documents.document_chunks c
 ON c.document_version_id=sv.document_version_id AND c.data_space_id=sv.data_space_id
JOIN documents.processing_runs r
 ON r.id=c.processing_run_id AND r.data_space_id=c.data_space_id
JOIN documents.chunk_embeddings e
 ON e.chunk_id=c.id AND e.data_space_id=c.data_space_id
WHERE js.job_id=$2 AND js.data_space_id=$1
 AND sv.source_kind='document'
 AND r.is_active AND r.status='ready'
 AND e.embedding_model_id=$4
ORDER BY e.embedding <=> $3::vector,c.id LIMIT $5;
```

### Q14-R — Hybrid retrieval RRF dengan top-K sebelum ranking fusion

```sql
-- $1 data_space_id, $2 job_id, $3 query_text,
-- $4 embedding_model_id, $5 query_vector, $6 final_limit
WITH allowed AS MATERIALIZED (
 SELECT c.id AS chunk_id, sv.id AS source_version_id,
        c.search_vector,e.embedding
 FROM ai.ai_job_sources js
 JOIN knowledge.source_versions sv
  ON sv.id=js.source_version_id AND sv.data_space_id=js.data_space_id
 JOIN documents.document_chunks c
  ON c.document_version_id=sv.document_version_id AND c.data_space_id=sv.data_space_id
 JOIN documents.processing_runs r
  ON r.id=c.processing_run_id AND r.data_space_id=c.data_space_id
 LEFT JOIN documents.chunk_embeddings e
  ON e.chunk_id=c.id AND e.data_space_id=c.data_space_id
  AND e.embedding_model_id=$4
 WHERE js.job_id=$2 AND js.data_space_id=$1
   AND sv.source_kind='document'
   AND r.is_active AND r.status='ready'
),
kw_top AS (
 SELECT chunk_id,source_version_id,
        ts_rank(search_vector,plainto_tsquery('simple',$3)) AS score
 FROM allowed
 WHERE search_vector @@ plainto_tsquery('simple',$3)
 ORDER BY score DESC,chunk_id LIMIT 100
),
vec_top AS (
 SELECT chunk_id,source_version_id,(embedding <=> $5::vector) AS distance
 FROM allowed WHERE embedding IS NOT NULL
 ORDER BY distance ASC,chunk_id LIMIT 100
),
kw_ranked AS (
 SELECT chunk_id,source_version_id,
        row_number() OVER (ORDER BY score DESC,chunk_id) AS rank_no
 FROM kw_top
),
vec_ranked AS (
 SELECT chunk_id,source_version_id,
        row_number() OVER (ORDER BY distance ASC,chunk_id) AS rank_no
 FROM vec_top
),
unioned AS (
 SELECT chunk_id,source_version_id,1.0/(60+rank_no) AS score FROM kw_ranked
 UNION ALL
 SELECT chunk_id,source_version_id,1.0/(60+rank_no) AS score FROM vec_ranked
)
SELECT chunk_id,source_version_id,sum(score) AS rrf_score
FROM unioned GROUP BY chunk_id,source_version_id
ORDER BY rrf_score DESC,chunk_id LIMIT $6;
```

**Quality/performance warnings:** Keyword results tetap dapat muncul ketika embeddings belum tersedia; fixed `100` candidate limit, `60` fusion constant, index strategy, `MATERIALIZED`, and model dimensions are evaluation baselines, **not production tuning decisions**. Exact vector search over many selected sources may require alternative candidate generation. Validate with `EXPLAIN (ANALYZE,BUFFERS)` and measured recall on Indonesian/English academic documents. If query returns no relevant evidence, the AI must not fabricate a supporting citation.

---

## 6. EF Core implementation blueprint

### 6.1 Baseline stack dan library boundaries

- ASP.NET Core modular monolith; backend modules: `Identity`, `Ownership`, `Workspace`, `Knowledge`, `Documents`, `Research`, `AI`, `Learning`, `Writing`, `Presentation`, `Integrity`, `Planner`, `Billing`, `Ops`.
- PostgreSQL schemas sama dengan namespace domain. **Satu** `StudyAiDbContext` / migration assembly sebagai default awal.
- Gunakan `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design`; `Pgvector.EntityFrameworkCore`/`Pgvector` hanya pada bagian yang benar-benar memerlukannya. Pin versi yang kompatibel dengan .NET/EF Core setelah ADR dependency.
- `DbContext` bukan tempat implementasi permission bisnis atau pemanggilan provider AI.
- Module lain hanya mengakses kapabilitas shared kernel `Identity`, `Ownership`, `Knowledge` lewat interface/contract; jangan memakai internal repository domain lain langsung.
- **Auth tetap open ADR**; `identity.users` adalah profil pengguna, **bukan** skema password/refresh token buatan sendiri.

### 6.2 Struktur monorepo yang disarankan

```text
studyai/
  apps/
    web/                          # Next.js
    mobile/                       # React Native/Expo, kelak
    api/                          # ASP.NET Core host + DI + auth
    workers/                      # .NET background workers
  services/
    document-extractor/           # Python extraction only
  src/
    StudyAI.Domain/
    StudyAI.Application/
    StudyAI.Infrastructure/
      Persistence/
        StudyAiDbContext.cs
        Configurations/
          Identity/
          Ownership/
          Workspace/
          Knowledge/
          Documents/
          Research/
          AI/
          Billing/
        Migrations/
      AI/
      Storage/
      Messaging/
    StudyAI.Contracts/
  tests/
    StudyAI.UnitTests/
    StudyAI.IntegrationTests/
    StudyAI.SecurityTests/
    StudyAI.LoadTests/
  infra/
    postgres/
    compose/
  docs/
    prd/
    architecture/
    database/
    adr/
```

### 6.3 Contoh entities — ilustrasi implementasi (bukan keseluruhan 68 model)

```csharp
public sealed class KnowledgeItem
{
    public Guid Id { get; set; }
    public Guid DataSpaceId { get; set; }
    public string Kind { get; set; } = null!;
    public string Title { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public sealed class Note
{
    public Guid KnowledgeItemId { get; set; }
    public Guid DataSpaceId { get; set; }
    public string ItemKind { get; set; } = "note";
    public string ContentJson { get; set; } = "{}"; // map to PostgreSQL jsonb
    public int FormatVersion { get; set; } = 1;
    public long Revision { get; set; } = 1;
    public string SearchText { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
}
```

`ContentJson` disimpan sebagai string pada ilustrasi supaya eksplisit; untuk editor terstruktur produksi, pertimbangkan mapping POCO + `ToJson()` dan test migrasi struktur. Npgsql menganjurkan EF JSON mapping dengan `ToJson()` untuk objek terstruktur. Jangan menganggap perubahan JSON tanpa validasi aman.

### 6.4 Contoh Fluent API untuk composite FK jenis item

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<KnowledgeItem>(b =>
    {
        b.ToTable("knowledge_items", "knowledge");
        b.HasKey(x => x.Id);
        b.HasAlternateKey(x => new { x.Id, x.DataSpaceId });
        b.HasAlternateKey(x => new { x.Id, x.DataSpaceId, x.Kind });
        b.Property(x => x.Kind).HasColumnName("kind").IsRequired();
        b.Property(x => x.DataSpaceId).HasColumnName("data_space_id");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        b.Property(x => x.DeletedAt).HasColumnName("deleted_at");
    });

    modelBuilder.Entity<Note>(b =>
    {
        b.ToTable("notes", "knowledge");
        b.HasKey(x => x.KnowledgeItemId);
        b.HasAlternateKey(x => new { x.KnowledgeItemId, x.DataSpaceId });
        b.Property(x => x.KnowledgeItemId).HasColumnName("knowledge_item_id");
        b.Property(x => x.DataSpaceId).HasColumnName("data_space_id");
        b.Property(x => x.ItemKind).HasColumnName("item_kind")
            .HasDefaultValue("note");
        b.Property(x => x.ContentJson).HasColumnName("content_json")
            .HasColumnType("jsonb");
        b.Property(x => x.Revision).HasColumnName("revision")
            .IsConcurrencyToken();
        b.Property(x => x.FormatVersion).HasColumnName("format_version");
        b.Property(x => x.SearchText).HasColumnName("search_text");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        b.HasOne<KnowledgeItem>().WithOne()
            .HasForeignKey<Note>(x => new {
                x.KnowledgeItemId, x.DataSpaceId, x.ItemKind
            })
            .HasPrincipalKey<KnowledgeItem>(x => new {
                x.Id, x.DataSpaceId, x.Kind
            })
            .OnDelete(DeleteBehavior.Restrict);
    });
}
```

> Contoh di atas **bukan drop-in final migration**. Map setiap properti termasuk `Id`, `Title`, descriptions, defaults, FK, unique/check/index, schema, dan semua reverse navigation sesuai SQL review candidate. Pastikan generated EF migration tidak diam-diam menghapus constraint, fungsi atau trigger yang ditulis manual. Konfigurasi harus lulus compile dan migration diff tests.

### 6.5 Concurrency kontrak REST

Permintaan menyertakan `expectedRevision`. Service menjalankan conditional update pada pemilik yang terautentikasi. Jika 0 rows terpengaruh, respons `409 Conflict` tanpa menimpa konten. `IsConcurrencyToken()` juga bisa digunakan untuk tracked entity EF; pilih pola tunggal per endpoint dan test keduanya bila hidup berdampingan.

### 6.6 UUID, timestamps dan nullable FK

- Generate UUIDv7 dari aplikasi .NET yang kompatibel, bukan `Guid.NewGuid()` default untuk ID produksi.
- `timestamptz` simpan instan UTC; gunakan `DateTimeOffset` UTC atau representasi UTC yang konsisten dengan konfigurasi Npgsql.
- FK yang sebagian kolomnya nullable memiliki perilaku `MATCH SIMPLE`: bila kolom referensi nullable kosong, validasi FK dapat dilewati. Itu benar untuk `parent_page_placement_id`, `source_version_id` opsional, dll **hanya jika** proses bisnis membatasi keadaan NULL secara eksplisit.
- Semua shared IDs pada foreign key pengguna wajib satu `data_space_id` yang konsisten. FK tidak otomatis melarang referensi ke baris yang `deleted_at` terisi, sehingga *active-only policy* tetap menjadi validasi service.

### 6.7 One migration history, incremental slices

| Slice | Isi minimum | Dependencies |
|---|---|---|
| M000 | Role/extension/bootstrap di administrative deployment | PostgreSQL, pgvector, pg_trgm, btree_gist availability |
| M001 | Identity + ownership | Auth provider ADR |
| M002 | Content Spaces, Workspaces, folders, knowledge items, placements, notes | M001 |
| M003 | Storage objects, documents, versions, processing runs/chunks | M002 |
| M004 | Reference snapshots, `source_versions`, AI jobs, citations, embeddings | M003 |
| M005 | Learning, Research, Writing, Presentation, Integrity, Planner | M004 |
| M006 | Billing plans, payment, quota ledger, provider usage, outbox | M004, needed before paid AI testing |
| M007 | RLS, performance indexes, online migration hardening | After negative authorization and load tests |

**Penting:** urutan di atas adalah **kelompok migration konseptual**, bukan daftar pasti kelas migration; dependencies bisa mengharuskan outbox dan quota schema lebih awal. Semua 9 modul tetap target StudyAI Campus V1.

Contoh perintah (path proyek disesuaikan setelah repo dibuat):

```bash
# Hanya contoh; tidak dieksekusi pada review ini.
dotnet ef migrations add M001_IdentityOwnership \
  --project src/StudyAI.Infrastructure \
  --startup-project apps/api \
  --context StudyAiDbContext

dotnet ef database update \
  --project src/StudyAI.Infrastructure \
  --startup-project apps/api \
  --context StudyAiDbContext
```

**Runtimenya harus test PostgreSQL asli**, bukan EF Core InMemory atau SQLite untuk menguji JSONB, partial index, composite FK, GiST, trigger, dan pgvector.

### 6.8 Transaction boundary, error mapping, dan retries

| SQLSTATE / kondisi | Handling API/workers |
|---|---|
| `23505` duplicate | HTTP 409, atau return existing hanya jika operasi benar-benar idempotent |
| `23503` FK violation | 400/404 tanpa membocorkan keberadaan tenant lain |
| `23514` CHECK/immutable trigger | 422 atau internal invariant failure, sesuai konteks |
| `23P01` exclusion violation | 409 overlapping active subscription |
| Note stale revision | 409 dengan revision server |
| Quota update 0 rows | Guard reject; tidak reserve sebagian periode |
| Deadlock `40P01` / serialization `40001` | Retry *seluruh transaksi* dengan batas retry + jitter, bukan statement acak |
| Provider timeout | Job tetap recoverable; invocation dan biaya direkonsiliasi |

---

## 7. Smoke tests dan QA minimum

### File executable draft

`StudyAI_DB_Smoke_Tests_v1.1.sql` memuat 10 skenario kecil: cross-tenant placement, mismatch subtype, immutable source pointer, immutable note payload, immutable file version, satu processing run aktif, subscription overlap, stale note revision, batas kuota, dan multi-period reservation. Script menggunakan fixture yang dibatalkan di akhir dengan `ROLLBACK`.

Cara menjalankan **setelah** SQL schema berhasil dibuat di PostgreSQL 16+ beserta extension yang dibutuhkan:

```bash
# Gunakan environment isolated: jangan arahkan ke database production.
psql "$STUDYAI_TEST_DATABASE_URL" -v ON_ERROR_STOP=1 \
  -f StudyAI_Physical_Schema_Reviewed_v1.1.sql
psql "$STUDYAI_TEST_DATABASE_URL" -v ON_ERROR_STOP=1 \
  -f StudyAI_DB_Smoke_Tests_v1.1.sql
```

Catatan test: sebagian smoke tests memvalidasi **struktur constraint**, bukan end-to-end concurrency. Misalnya multi-period fixture hanya membuktikan uniqueness memperbolehkan job yang sama pada dua period; *reservation service* tetap harus menaikkan kedua balances secara atomik. Gunakan isolated DB kosong untuk script schema. Jangan menggunakan `BEGIN; ... ROLLBACK;` smoke test untuk membuktikan storage/object cleanup; integrasi object storage perlu test service terpisah.

### Test suite yang masih harus dibuat

| Suite | Skenario wajib | Passing criteria |
|---|---|---|
| TenantIsolationTests | Tenant A mencoba baca/edit data B lewat EF, raw SQL dan object URLs | Tidak ada akses lintas tenant |
| HierarchyTests | Folder cycle, nested page cycle, move bersamaan, remove/restore subtree | Tidak ada cycle/orphan aktif |
| ProvenanceTests | PDF v1→v2, note revision→snapshot, source tombstone | Sitasi tidak bergeser ke sumber lain |
| RetrievalTests | Multiple processing runs, source_scope, embedding model dimension | Hanya satu run aktif dan sumber terpilih |
| QuotaConcurrencyTests | 2–100 job paralel, monthly/daily, retry/release/settle | Tidak overdraw, no double debit |
| PaymentTests | Duplicate/reordered webhook, amount mismatch, refund, overlap | Maksimum satu transisi entitlement per verified event |
| StorageTests | Upload gagal, orphan, quota, purge/retention/downgrade | Tidak bocor object, no silent loss |
| MigrationTests | Clean DB up, populated DB evolve, rollback/restore | Reproducible schema dan data utuh |
| PerformanceTests | `EXPLAIN ANALYZE`, keyset, RAG/FTS, worker throughput | Baseline terukur; bukan klaim kapasitas tanpa test |

**Kriteria sign-off:** minimal 10 smoke tests (setelah benar-benar dijalankan dan tercatat), seluruh suite P0/P1 lulus, migration compatible, backup & restore diuji, dan reviewer menandatangani ADR yang masih terbuka.

---

## 8. Security / RLS implementation gate

1. Application authorization setiap read/write, termasuk Library, note, AI history, source set, storage.
2. Ownership-safe FK, unique, subtype enforcement; **jangan mengandalkan** EF global query filters saja, sebab raw SQL retrieval berjalan di luar filter LINQ.
3. RLS sebagai *defense-in-depth* pada tenant-sensitive retrieval tabel, terutama `documents.document_chunks` dan `documents.chunk_embeddings`.
4. Runtime role tidak boleh superuser, `BYPASSRLS`, atau table owner yang tanpa sengaja bypass policy; `FORCE ROW LEVEL SECURITY` setelah risk review bila relevan.
5. RLS context selalu transaction-local: `SELECT set_config('app.current_data_space_id', :owner_id, true);` dalam transaksi yang sama dengan query.
6. Gunakan role worker berbeda hanya bila benar-benar diperlukan; akses worker dibatasi pada job yang diberi otorisasi, bukan bebas membaca seluruh data.
7. Jangan log file penuh, token, signed URL, atau prompt/jurnal sensitif.
8. Uji pooled connections bergantian user A→B dan cursor/query cancellation/error path.

**Status:** schema v1.1 **tidak otomatis mengaktifkan RLS**, karena role/pooling architecture masih membutuhkan test dan ADR. Jangan salah menganggap RLS sudah aktif hanya karena disebut dalam dokumen.

---

## 9. Penanganan lifecycle dan delete yang tidak cukup dengan FK

- **Canonical Library:** setiap Knowledge Item aktif harus memiliki penempatan canonical di Library owner (atau kebijakan pengecualian eksplisit, misal intermediate AI output yang belum disimpan). SQL FK saja tidak menjamin existence ini; transaction + integrity auditor wajib.
- **Soft delete:** `deleted_at`/`removed_at` tidak otomatis memutus FK. Setiap query user harus menyaring parent spaces, folder, placement, dan item yang aktif sesuai konteks.
- **Remove course:** hanya menonaktifkan placement di course, tidak menghapus canonical item di Library/Project lain.
- **Nested subtree:** parent removal harus disertai descendant transition; jika tidak, child yang masih aktif dapat menjadi tidak terlihat atau tampil yatim.
- **Permanent delete:** wipe original blob dan retained versions sesuai retention; citation snapshots, prompts, thumbnails, embeddings, cached outputs dan analytics personal mengikuti kebijakan purge, legal hold, serta backup deletion schedule yang tertulis.
- **Referensi DOI:** DOI unik per Data Space meskipun item berada di Trash; input DOI yang sama harus menawarkan restore, bukan membuat duplikat.
- **Storage downgrade:** tidak menghapus otomatis data pengguna; batasi write baru saat melewati quota setelah periode kebijakan/grace.

---

## 10. Open ADR (jangan diputuskan sembarangan oleh AI coding agent)

| ADR | Isu | Dampak dan kapan harus selesai |
|---|---|---|
| ADR-AUTH | ASP.NET Core Identity atau auth provider lain; email/OAuth/mobile token | **Sebelum M001** |
| ADR-QUEUE | Postgres queue/Hangfire/ops worker, retry & leases | Sebelum job orchestration |
| ADR-SOURCE | Source retention and note snapshot frequency, document purge references | Sebelum AI Tutor/Research production |
| ADR-QUOTA | Debit policy, period boundaries, model override, user-visible credits & failures | Sebelum monetization |
| ADR-PAY | Prepaid terms, free-to-paid overlap, refunds, upgrade/downgrade/grace | Sebelum payment activation |
| ADR-RLS | Role, policy SQL, worker access, pooled transactions | Sebelum external user data di produksi |
| ADR-EMBED | Model, dimensions, Indonesian eval, index/ANN choice | Sebelum retrieval quality sign-off |
| ADR-STORAGE | Provider, object quota, retained versions, trash purge & backup retention | Sebelum upload publik |
| ADR-SIMILARITY | Lisensi dan hak penyedia eksternal | Fitur gated; jangan mengiklankan tanpa izin |

**Perlu dicatat:** memilih library/konfigurasi teknis tidak harus mengubah PRD atau Core Architecture v1.2, selama product behavior dan invariant tetap sama.

---

## 11. Handoff untuk fase implementasi

### Phase A — Postgres Runtime Validation

- [ ] Sediakan Docker Compose PostgreSQL 16+ dengan pgvector dan ekstensi `pg_trgm` + `btree_gist`.
- [ ] Jalankan reviewed schema pada DB kosong (admin bootstrap), periksa DDL error dan `pg_constraint`/`pg_indexes`.
- [ ] Jalankan `StudyAI_DB_Smoke_Tests_v1.1.sql` dengan `ON_ERROR_STOP=1`.
- [ ] Perbaiki semua error runtime, simpan execution log yang disanitasi, **baru** naikkan status schema menjadi tested.

### Phase B — EF Core foundation

- [ ] Tentukan Auth ADR, target .NET SDK dan compatible EF Core/Npgsql packages.
- [ ] Buat solution modular monolith dan `StudyAiDbContext` tunggal.
- [ ] Implement Identity/Ownership/ContentSpaces/Knowledge + mapping complete dari SQL.
- [ ] Tambahkan migrations incremental; manual SQL untuk constraint/function yang tidak termodelkan oleh provider jika diperlukan.
- [ ] Build CI: compile, migration diff/snapshot check, testcontainers Postgres, negative FK tests.

### Phase C — Knowledge & RAG

- [ ] Implement canonical item + placements + hierarchy transactions.
- [ ] Implement storage adapter, ingestion pipeline, publish processing_run aktif.
- [ ] Implement source versions, job sources, Q12-R/Q13-R/Q14-R, provenance retention.
- [ ] Negative cross-user test termasuk raw retrieval dan worker access.

### Phase D — Learning/Research/Writing/Presentation/Planner/Billing

- [ ] Migrations per modul dengan semua 9 modul target PRD Campus V1.
- [ ] Quota reserve/settle transaction & ledger reconciliation **sebelum** pengujian AI berbayar.
- [ ] Payment webhooks verified + idempotent subscription state machine.
- [ ] PPTX/DOCX export dan Academic Integrity partner gating.

### Phase E — Performance, operational hardening & release

- [ ] RLS tests, backup/restore drill, migration staging, outbox reconciliation.
- [ ] Bahasa Indonesia/English retrieval quality tests; benchmark ANN sebelum menambah ANN index.
- [ ] Unit-cost test + load test, tentukan launch SLO; jangan klaim jumlah user dari spesifikasi VPS semata.

---

## 12. Referensi eksternal untuk pemeriksaan implementasi

1. PostgreSQL constraints, unique, foreign key dan NULL FK: https://www.postgresql.org/docs/16/ddl-constraints.html
2. PostgreSQL GiST exclusion/ranges: https://www.postgresql.org/docs/current/rangetypes.html
3. PostgreSQL `btree_gist` untuk UUID dan operator class: https://www.postgresql.org/docs/15/btree-gist.html
4. PostgreSQL Row-Level Security: https://www.postgresql.org/docs/16/ddl-rowsecurity.html
5. Npgsql EF Core JSON mapping: https://www.npgsql.org/efcore/mapping/json.html
6. EF Core concurrency tokens: https://learn.microsoft.com/en-us/ef/core/saving/concurrency

Catatan: referensi eksternal mendukung perilaku database/ORM. **Keputusan arsitektur StudyAI sendiri bersumber dari PRD v1.0 dan Core Architecture v1.2**, bukan dari sumber eksternal.

---

## 13. Revision log & keputusan akhir review

| Versi | Tanggal | Perubahan |
|---|---|---|
| Physical v1.0 | 12 Oct 2026 | 68 tabel, data dictionary, 85 FK, 30 query; proposal awal |
| Review v1.1 | 13 Oct 2026 | Hardening multiperiod quota, active processing run, immutable source/document versions, subscription overlap, provider invocation, revised retrieval queries, test script dan EF handoff |

**Putusan:** `StudyAI_Physical_Schema_Reviewed_v1.1.sql` merupakan **candidate yang lebih aman untuk dibawa ke Postgres runtime tests**, *bukan* schema approved production. PRD v1.0 dan Core Architecture v1.2 tetap LOCKED; tidak ada pemangkasan modul MVP. **Berhenti pada review sampai runtime, integration, authorization, dan concurrency tests dijalankan dan benar-benar lulus.**
