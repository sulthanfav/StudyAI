using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable
#pragma warning disable CA1861

namespace StudyAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPersistenceBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "documents");

            migrationBuilder.EnsureSchema(
                name: "workspace");

            migrationBuilder.EnsureSchema(
                name: "ownership");

            migrationBuilder.EnsureSchema(
                name: "ai");

            migrationBuilder.EnsureSchema(
                name: "knowledge");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "embedding_models",
                schema: "ai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "text", nullable: false),
                    model_key = table.Column<string>(type: "text", nullable: false),
                    dimensions = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_embedding_models", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "active"),
                    locale = table.Column<string>(type: "text", nullable: false, defaultValue: "id-ID"),
                    timezone = table.Column<string>(type: "text", nullable: false, defaultValue: "Asia/Jakarta"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_spaces",
                schema: "ownership",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false, defaultValue: "personal"),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_spaces", x => x.id);
                    table.ForeignKey(
                        name: "FK_data_spaces_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "content_spaces",
                schema: "workspace",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_spaces", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_spaces_data_spaces_data_space_id",
                        column: x => x.data_space_id,
                        principalSchema: "ownership",
                        principalTable: "data_spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_items",
                schema: "knowledge",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_knowledge_items_data_spaces_data_space_id",
                        column: x => x.data_space_id,
                        principalSchema: "ownership",
                        principalTable: "data_spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "storage_objects",
                schema: "documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage_key = table.Column<string>(type: "text", nullable: false),
                    checksum_sha256 = table.Column<string>(type: "text", nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    mime_type = table.Column<string>(type: "text", nullable: false),
                    charge_class = table.Column<string>(type: "text", nullable: false, defaultValue: "billable"),
                    state = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storage_objects", x => x.id);
                    table.ForeignKey(
                        name: "FK_storage_objects_data_spaces_data_space_id",
                        column: x => x.data_space_id,
                        principalSchema: "ownership",
                        principalTable: "data_spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "documents",
                columns: table => new
                {
                    knowledge_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_kind = table.Column<string>(type: "text", nullable: false, defaultValue: "document"),
                    original_filename = table.Column<string>(type: "text", nullable: false),
                    current_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.knowledge_item_id);
                    table.ForeignKey(
                        name: "FK_documents_knowledge_items_knowledge_item_id",
                        column: x => x.knowledge_item_id,
                        principalSchema: "knowledge",
                        principalTable: "knowledge_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "document_versions",
                schema: "documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_no = table.Column<int>(type: "integer", nullable: false),
                    storage_object_id = table.Column<Guid>(type: "uuid", nullable: false),
                    page_count = table.Column<int>(type: "integer", nullable: true),
                    processing_status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    processing_version = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_versions_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "documents",
                        principalTable: "documents",
                        principalColumn: "knowledge_item_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_versions_storage_objects_storage_object_id",
                        column: x => x.storage_object_id,
                        principalSchema: "documents",
                        principalTable: "storage_objects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "processing_runs",
                schema: "documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processor_version = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    error_code = table.Column<string>(type: "text", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processing_runs", x => x.id);
                    table.ForeignKey(
                        name: "FK_processing_runs_document_versions_document_version_id",
                        column: x => x.document_version_id,
                        principalSchema: "documents",
                        principalTable: "document_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "document_chunks",
                schema: "documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processing_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chunk_index = table.Column<int>(type: "integer", nullable: false),
                    content_text = table.Column<string>(type: "text", nullable: false),
                    locator = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    token_count = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_chunks", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_chunks_document_versions_document_version_id",
                        column: x => x.document_version_id,
                        principalSchema: "documents",
                        principalTable: "document_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_document_chunks_processing_runs_processing_run_id",
                        column: x => x.processing_run_id,
                        principalSchema: "documents",
                        principalTable: "processing_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "chunk_embeddings",
                schema: "documents",
                columns: table => new
                {
                    chunk_id = table.Column<Guid>(type: "uuid", nullable: false),
                    embedding_model_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    embedding = table.Column<Vector>(type: "vector", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chunk_embeddings", x => new { x.chunk_id, x.embedding_model_id });
                    table.ForeignKey(
                        name: "FK_chunk_embeddings_document_chunks_chunk_id",
                        column: x => x.chunk_id,
                        principalSchema: "documents",
                        principalTable: "document_chunks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chunk_embeddings_embedding_models_embedding_model_id",
                        column: x => x.embedding_model_id,
                        principalSchema: "ai",
                        principalTable: "embedding_models",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chunk_embeddings_embedding_model_id",
                schema: "documents",
                table: "chunk_embeddings",
                column: "embedding_model_id");

            migrationBuilder.CreateIndex(
                name: "ix_embeddings_tenant",
                schema: "documents",
                table: "chunk_embeddings",
                columns: new[] { "data_space_id", "embedding_model_id" });

            migrationBuilder.CreateIndex(
                name: "IX_content_spaces_id_data_space_id",
                schema: "workspace",
                table: "content_spaces",
                columns: new[] { "id", "data_space_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_spaces_id_data_space_id_kind",
                schema: "workspace",
                table: "content_spaces",
                columns: new[] { "id", "data_space_id", "kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_spaces_owner",
                schema: "workspace",
                table: "content_spaces",
                columns: new[] { "data_space_id", "kind", "position" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_library_per_owner",
                schema: "workspace",
                table: "content_spaces",
                column: "data_space_id",
                unique: true,
                filter: "kind = 'library'");

            migrationBuilder.CreateIndex(
                name: "IX_data_spaces_id_owner_user_id",
                schema: "ownership",
                table: "data_spaces",
                columns: new[] { "id", "owner_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_one_personal_space",
                schema: "ownership",
                table: "data_spaces",
                column: "owner_user_id",
                unique: true,
                filter: "kind = 'personal'");

            migrationBuilder.CreateIndex(
                name: "IX_document_chunks_document_version_id_processing_run_id_chunk~",
                schema: "documents",
                table: "document_chunks",
                columns: new[] { "document_version_id", "processing_run_id", "chunk_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_chunks_id_data_space_id",
                schema: "documents",
                table: "document_chunks",
                columns: new[] { "id", "data_space_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_chunks_processing_run_id",
                schema: "documents",
                table: "document_chunks",
                column: "processing_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_document_id_version_no",
                schema: "documents",
                table: "document_versions",
                columns: new[] { "document_id", "version_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_id_data_space_id",
                schema: "documents",
                table: "document_versions",
                columns: new[] { "id", "data_space_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_id_data_space_id_document_id",
                schema: "documents",
                table: "document_versions",
                columns: new[] { "id", "data_space_id", "document_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_versions_storage_object_id",
                schema: "documents",
                table: "document_versions",
                column: "storage_object_id");

            migrationBuilder.CreateIndex(
                name: "IX_documents_knowledge_item_id_data_space_id",
                schema: "documents",
                table: "documents",
                columns: new[] { "knowledge_item_id", "data_space_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_embedding_models_provider_model_key_dimensions",
                schema: "ai",
                table: "embedding_models",
                columns: new[] { "provider", "model_key", "dimensions" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_items_id_data_space_id",
                schema: "knowledge",
                table: "knowledge_items",
                columns: new[] { "id", "data_space_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_items_id_data_space_id_kind",
                schema: "knowledge",
                table: "knowledge_items",
                columns: new[] { "id", "data_space_id", "kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_knowledge_owner_recent",
                schema: "knowledge",
                table: "knowledge_items",
                columns: new[] { "data_space_id", "updated_at", "id" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_processing_runs_id_data_space_id_document_version_id",
                schema: "documents",
                table: "processing_runs",
                columns: new[] { "id", "data_space_id", "document_version_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_processing_active_run",
                schema: "documents",
                table: "processing_runs",
                column: "document_version_id",
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "IX_storage_objects_id_data_space_id",
                schema: "documents",
                table: "storage_objects",
                columns: new[] { "id", "data_space_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_storage_objects_storage_key",
                schema: "documents",
                table: "storage_objects",
                column: "storage_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_storage_owner_checksum",
                schema: "documents",
                table: "storage_objects",
                columns: new[] { "data_space_id", "checksum_sha256" },
                filter: "state = 'verified'");

            migrationBuilder.CreateIndex(
                name: "ux_users_email_normalized",
                schema: "identity",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.EnsureSchema(name: "ops");

            // Immutability trigger function from reviewed schema v1.1
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION ops.reject_snapshot_update()
RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    RAISE EXCEPTION 'immutable academic source snapshot: %', TG_TABLE_NAME
        USING ERRCODE = '23514';
END;
$$;

DROP TRIGGER IF EXISTS trg_document_chunk_immutable ON documents.document_chunks;
CREATE TRIGGER trg_document_chunk_immutable BEFORE UPDATE
    ON documents.document_chunks FOR EACH ROW EXECUTE FUNCTION ops.reject_snapshot_update();
");

            // Ensure runtime application role exists before configuring RLS policies
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'studyai_app') THEN
        CREATE ROLE studyai_app WITH LOGIN PASSWORD 'AppRuntimeSecret123!' NOSUPERUSER NOCREATEDB NOBYPASSRLS;
    END IF;
END $$;
");

            // Row-Level Security: Enable and force RLS on tenant-scoped tables with fail-closed policy
            var rlsTables = new[]
            {
                "knowledge.knowledge_items",
                "workspace.content_spaces",
                "documents.storage_objects",
                "documents.documents",
                "documents.document_versions",
                "documents.processing_runs",
                "documents.document_chunks",
                "documents.chunk_embeddings"
            };

            foreach (var table in rlsTables)
            {
                migrationBuilder.Sql($@"
ALTER TABLE {table} ENABLE ROW LEVEL SECURITY;
ALTER TABLE {table} FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS tenant_isolation_policy ON {table};
CREATE POLICY tenant_isolation_policy ON {table}
    FOR ALL TO studyai_app
    USING (data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid)
    WITH CHECK (data_space_id = NULLIF(current_setting('app.current_data_space_id', true), '')::uuid);
");
            }

            // Least-privilege DML grants for application runtime role (NO DDL, NO BYPASSRLS)
            migrationBuilder.Sql(@"
GRANT USAGE ON SCHEMA identity, ownership, workspace, knowledge, documents, ai, ops TO studyai_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA identity, ownership, workspace, knowledge, documents, ai, ops TO studyai_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA identity, ownership, workspace, knowledge, documents, ai, ops TO studyai_app;
REVOKE CREATE ON SCHEMA identity, ownership, workspace, knowledge, documents, ai, ops FROM studyai_app;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chunk_embeddings",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "content_spaces",
                schema: "workspace");

            migrationBuilder.DropTable(
                name: "document_chunks",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "embedding_models",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "processing_runs",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "document_versions",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "storage_objects",
                schema: "documents");

            migrationBuilder.DropTable(
                name: "knowledge_items",
                schema: "knowledge");

            migrationBuilder.DropTable(
                name: "data_spaces",
                schema: "ownership");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity");
        }
    }
}
