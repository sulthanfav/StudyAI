# StudyAI — Master Feature List & Product Roadmap

> **Versi:** 1.0 — 11 Oktober 2026  
> **Status:** Dokumen perencanaan produk (brainstorming → eksekusi), **bukan** daftar fitur yang sudah tersedia.  
> **Tujuan:** Satu referensi fitur B2C dan B2B untuk product backlog, desain arsitektur, dan prioritas pengembangan.

## 0. Ringkasan produk

**Visi:** *StudyAI — Your AI Education Operating System.* Satu ekosistem untuk belajar, mengelola pengetahuan, riset, tugas, presentasi, evaluasi, pengajaran, dan pertumbuhan karier.

**Prinsip desain:** **One Account + One Personal Knowledge Base + One Learning Profile + Multiple AI Workflows.** Data personal dan institusi tetap dipisahkan sesuai kepemilikan serta permission.

**Lini produk:**

| Lini | Pembeli / pengguna | Fokus |
|---|---|---|
| **StudyAI Campus (B2C)** | Mahasiswa | Academic workspace, riset, tulisan, PPT, belajar & ujian |
| **StudyAI School (B2C/B2B2C)** | Siswa, orang tua | AI tutor, PR, latihan adaptif, ujian, laporan perkembangan |
| **StudyAI Teacher (B2C/B2B)** | Guru, dosen, tutor | Bahan ajar, soal, feedback, insight pembelajaran |
| **StudyAI Academy (B2B)** | Bimbel, kursus, bootcamp | LMS, kelas, pembayaran kursus, tryout, AI tutor |
| **StudyAI Institution (B2B)** | Sekolah, kampus | LMS, pengelolaan kelas, analitik, akses institusi |
| **StudyAI Connect (B2B)** | Institusi dengan LMS sendiri | AI layer, API, LMS integrations |
| **StudyAI Enterprise (ekspansi)** | Perusahaan & lembaga pelatihan | Training, onboarding, assessment, knowledge AI |

### Legenda prioritas

- **P0 — Launch foundation:** perlu untuk produk berbayar pertama yang sempit dan aman.
- **P1 — B2C expansion:** setelah ada validasi pembelian dan penggunaan.
- **P2 — B2B/School expansion:** setelah produk inti stabil; pilih modul sesuai calon klien.
- **P3 — Superapp/Enterprise:** visi jangka panjang, bukan komitmen sprint awal.
- **[PARTNER]** = fitur bergantung pada integrasi, hak akses, lisensi, atau kontrak pihak ketiga.

> **Catatan penting:** P0 tidak berarti membangun semua modul di bawah. Untuk penjualan pertama, fokus pada **satu workflow lengkap**, lalu perkuat fondasi bersama. Daftar di bawah sengaja mencakup visi lengkap.

---

# 1. B2C — StudyAI Campus (Mahasiswa)

## 1.1 Account & Onboarding

- [ ] **P0** Registrasi, login, logout, reset password.
- [ ] **P0** Profil pengguna, preferensi bahasa, zona waktu.
- [ ] **P0** Onboarding: pilih peran, jurusan, semester, dan tujuan belajar.
- [ ] **P0** Dashboard personal dan recent activities.
- [ ] **P1** Login Google/SSO konsumen.
- [ ] **P1** Pengaturan notifikasi dan preferensi AI.
- [ ] **P1** Impor data dan export akun.

## 1.2 Knowledge Management / Notion-like Workspace

- [ ] **P0** Workspace personal.
- [ ] **P0** Folder, mata kuliah (*course*), project, dan nested pages.
- [ ] **P0** Editor catatan berbasis blok: teks, heading, list, checklist, tautan, gambar, tabel.
- [ ] **P0** Autosave, rename, move, delete, dan restore sederhana.
- [ ] **P0** Upload serta organisasi PDF/PPTX/DOCX dan file pendukung.
- [ ] **P0** Pencarian berdasarkan judul, folder, dan isi notes yang terindeks.
- [ ] **P1** Tag, favorit, template notes, arsip.
- [ ] **P1** Rich blocks: code block, LaTeX/rumus, embed, callouts.
- [ ] **P1** Backlinks, referensi silang dan knowledge graph sederhana.
- [ ] **P1** AI auto-summary, auto-tagging, dan saran pengelompokan materi.
- [ ] **P1** **Ask My Knowledge:** tanya-jawab lintas notes, dokumen, dan jurnal; jawaban dengan tautan sumber.
- [ ] **P1** **Transform Knowledge:** catatan → kuis, flashcards, mind map, outline, PPT.
- [ ] **P1** Versi dokumen dan pemulihan perubahan.
- [ ] **P2** Markdown/CSV/DOCX import-export dan migrasi dari aplikasi catatan lain.
- [ ] **P2** Offline reading/drafting, sinkronisasi aman ketika online.
- [ ] **P3** Notion-style database views (table, board, calendar) dan formula.
- [ ] **P3** Real-time multi-user document editing.

## 1.3 Document & Learning Content Intelligence

- [ ] **P0** Upload file tunggal / multi-file sesuai kuota.
- [ ] **P0** Ekstraksi teks PDF/PPTX/DOCX dan peringatan jika gagal.
- [ ] **P0** Ringkasan materi: singkat, detail, poin penting.
- [ ] **P0** Key concept / glossary extractor.
- [ ] **P0** Sumber jawaban tertaut ke dokumen dan halaman/slide jika tersedia.
- [ ] **P1** Chat with document / chat dengan beberapa dokumen sekaligus.
- [ ] **P1** Topik otomatis, FAQ, tabel perbandingan, concept map.
- [ ] **P1** Highlight, annotation, dan Q&A pada bagian spesifik PDF.
- [ ] **P2** OCR dokumen scan, foto papan tulis, dan tulisan tangan.
- [ ] **P2** Transkripsi rekaman kuliah dengan izin, timestamps, dan note generator.
- [ ] **P2** Video/YouTube learning notes dari konten yang boleh diakses/diproses.
- [ ] **P2** Voice memo → notes / knowledge base.
- [ ] **P3** Multimedia understanding untuk grafik kompleks, diagram dan video panjang.

## 1.4 AI Tutor & Personalized Learning

- [ ] **P0** AI tutor berbasis materi yang diunggah.
- [ ] **P0** Penjelasan sederhana, analogi, contoh nyata, langkah demi langkah.
- [ ] **P1** Guided/Socratic learning: AI memberi petunjuk bertahap, tidak hanya jawaban.
- [ ] **P1** Flashcard generator dan kartu hafalan interaktif.
- [ ] **P1** Spaced repetition / jadwal review.
- [ ] **P1** Learning path per topik dan target ujian.
- [ ] **P1** Weakness / knowledge gap detection berdasarkan hasil latihan.
- [ ] **P1** Mistake journal, review topik lemah, personalized recommendations.
- [ ] **P2** Explain-back mode: pengguna menjelaskan ulang konsep, AI memberi feedback.
- [ ] **P2** AI oral exam / latihan tanya-jawab presentasi.
- [ ] **P3** Voice tutor real-time dan dukungan multimodal interaktif.

## 1.5 Quiz, Practice & Exam Preparation

- [ ] **P0** Generate soal pilihan ganda dari materi.
- [ ] **P0** Pengaturan jumlah soal, tingkat kesulitan, topik.
- [ ] **P0** Mode ujian, timer, submit, skor, pembahasan.
- [ ] **P0** Riwayat sesi dan rujukan sumber soal/jawaban.
- [ ] **P1** Soal esai, benar/salah, isian, studi kasus.
- [ ] **P1** Retry wrong answers, bank soal personal, randomization.
- [ ] **P1** Dashboard penguasaan topik dan tren nilai latihan.
- [ ] **P1** Exam countdown, targeted study plan, adaptive practice.
- [ ] **P1** Shareable quiz link tanpa otomatis membocorkan dokumen sumber.
- [ ] **P2** Exam packs untuk UTS, UAS, TKA, UTBK (sesuai segmen dan kualitas konten).
- [ ] **P2** Rubrik dan feedback esai dengan manusia sebagai penilai akhir untuk nilai resmi.
- [ ] **P3** Proctoring/ujian institusional (kebijakan privasi dan kebutuhan legal khusus).

## 1.6 Research AI / Journal & Book Discovery

- [ ] **P1** Cari paper berdasarkan topik, judul, penulis, DOI, kata kunci.
- [ ] **P1** Cari buku dan metadata referensinya.
- [ ] **P1** Integrasi metadata akademik: OpenAlex, Crossref, Semantic Scholar, Google Books (sesuai kebijakan API).
- [ ] **P1** Filter tahun, bidang, bahasa, open access, relevansi.
- [ ] **P1** DOI / metadata verification sebelum menampilkan sitasi final.
- [ ] **P1** Simpan referensi ke personal research library.
- [ ] **P1** Ringkas jurnal serta ekstraksi tujuan, metode, hasil, keterbatasan.
- [ ] **P1** Paper comparison dan literature matrix.
- [ ] **P2** Similar/recommended papers dan citation network exploration.
- [ ] **P2** Research gap hypothesis explorer dengan bukti dan ketidakpastian.
- [ ] **P2** Highlight/annotation paper serta linking ke catatan.
- [ ] **P2** Export BibTeX, RIS, dan format referensi yang didukung.
- [ ] **P3** Systematic literature review workflow dan deduplikasi studi.

## 1.7 Academic Writing AI

- [ ] **P1** Generate outline makalah/laporan/proposal/artikel ilmiah.
- [ ] **P1** Draft assistant per bagian: latar belakang, pendahuluan, pembahasan, kesimpulan.
- [ ] **P1** Literature review assistant berbasis referensi terpilih.
- [ ] **P1** Citation generator: APA, IEEE, MLA, Chicago (gunakan formatting engine, bukan karangan model).
- [ ] **P1** Kutipan langsung yang **diverifikasi** dari dokumen dan lokasi aslinya.
- [ ] **P1** Parafrasa dengan atribusi, menjaga makna dan referensi.
- [ ] **P1** Bibliografi otomatis dari sumber terverifikasi.
- [ ] **P1** Claim-to-source linking: tandai klaim yang belum didukung bukti.
- [ ] **P1** Grammar, academic tone, kejelasan dan konsistensi istilah.
- [ ] **P1** Export DOCX/PDF dengan format yang diperiksa.
- [ ] **P2** Template makalah/proposal/laporan berdasarkan style guide yang diunggah.
- [ ] **P2** Multi-document collaborative writing dan review comments.
- [ ] **P2** AI revision suggestions / track changes.
- [ ] **P3** End-to-end research project assistant dengan checkpoint persetujuan pengguna.

> Produk ini **asisten penulisan**, bukan layanan yang menjanjikan plagiarisme nol, referensi fiktif, atau karya ilmiah otomatis siap submit. Penulis tetap memverifikasi fakta, metode, kutipan, dan kepatuhan akademik.

## 1.8 Presentation AI

- [ ] **P1** Topic/prompt → outline presentasi.
- [ ] **P1** PDF/DOCX/makalah → slide.
- [ ] **P1** Template presentasi (akademik, laporan, seminar).
- [ ] **P1** Generate konten dan struktur per slide.
- [ ] **P1** Preview, edit teks, reorder, regenerate slide tertentu.
- [ ] **P1** Export **PPTX editable** dan PDF dengan validasi layout.
- [ ] **P2** Grafik, tabel, infografik, diagram dari sumber yang relevan.
- [ ] **P2** Speaker notes dan presentation script.
- [ ] **P2** AI presentation rehearsal dan tanya-jawab penguji.
- [ ] **P3** Editor visual drag-and-drop penuh, animasi, branding lanjutan.

## 1.9 Academic Integrity / Similarity

- [ ] **P1** Citation audit: referensi kosong, format salah, klaim tanpa dukungan.
- [ ] **P1** Periksa kutipan langsung vs parafrasa dan konsistensi bibliografi.
- [ ] **P2 [PARTNER]** Similarity check melalui penyedia API/lisensi komersial yang sah.
- [ ] **P2 [PARTNER]** Laporan sumber yang cocok, highlight teks, export report.
- [ ] **P2 [PARTNER]** Opsi *no-repository* hanya jika benar-benar didukung provider.
- [ ] **P2** Perbaikan sitasi, atribusi dan kejelasan setelah review; bukan alat menyamarkan plagiarisme.
- [ ] **P3 [PARTNER]** **Turnitin resmi** hanya jika kemitraan/lisensi menyetujui penggunaan dan distribusi; jangan menyebut checker lain sebagai Turnitin.

> Similarity percentage **bukan** vonis plagiarisme. Pemeriksaan eksternal ditawarkan sebagai **add-on per cek** kecuali biaya dan hak penggunaan telah disepakati.

## 1.10 Academic Life Management

- [ ] **P1** Academic calendar: jadwal kuliah, ujian, deadline.
- [ ] **P1** Assignment / task manager dengan status dan prioritas.
- [ ] **P1** Deadline reminder dan daily/weekly briefing.
- [ ] **P1** Study planner dan Pomodoro / focus timer.
- [ ] **P1** Upload RPS/silabus → usulan jadwal topik & deadline untuk dikonfirmasi.
- [ ] **P2** Grade tracker, target nilai dan estimasi IP/IPK yang transparan.
- [ ] **P2** AI task breakdown untuk makalah, presentasi, dan skripsi.
- [ ] **P2** Calendar sync (Google/Microsoft) dengan izin pengguna.
- [ ] **P3** Automasi proaktif berbasis event, selalu minta persetujuan untuk tindakan sensitif.

## 1.11 Collaboration, Community & Growth

- [ ] **P1** Share quiz, flashcards, catatan tertentu dengan izin.
- [ ] **P2** Group project workspace, undang anggota, assign task.
- [ ] **P2** Komentar, review tugas, meeting notes.
- [ ] **P2** Shared reference library dan class study room.
- [ ] **P2** Group presentation rehearsal.
- [ ] **P3** Real-time editing, discussion forum, peer tutoring.
- [ ] **P3** Marketplace materi yang hak distribusinya terverifikasi.
- [ ] **P3** Learning communities dan gamified challenges yang sehat.

## 1.12 Career, Scholarship & Skills (Superapp Extension)

- [ ] **P2** Scholarship/event/opportunity finder dengan sumber aktual.
- [ ] **P2** Scholarship application & deadline tracker.
- [ ] **P2** AI CV, portfolio dan application essay assistant.
- [ ] **P2** Internship finder, skill gap analyzer, career roadmap.
- [ ] **P2** AI mock interview dan public speaking coaching.
- [ ] **P3** AI coding lab dengan sandbox aman.
- [ ] **P3** Language speaking / pronunciation partner.
- [ ] **P3** Interactive STEM lab dan simulasi pembelajaran.
- [ ] **P3** Student discounts, certification tracker, curated tutor marketplace.

---

# 2. B2C — StudyAI School (SD / SMP / SMA)

## 2.1 School Student Experience

- [ ] **P2** Pilih jenjang, kelas, mata pelajaran, target belajar.
- [ ] **P2** AI homework helper: foto soal, petunjuk, langkah, latihan serupa.
- [ ] **P2** AI tutor sesuai usia dan tingkat kemampuan; dukung bahasa Indonesia.
- [ ] **P2** Materi, ringkasan bab, flashcards, concept maps.
- [ ] **P2** Diagnostik kemampuan dan knowledge gap detector.
- [ ] **P2** Adaptive quiz, tryout, pembahasan, review salah.
- [ ] **P2** Planner belajar dan exam countdown (ulangan, TKA, UTBK sesuai segmen).
- [ ] **P2** Progress belajar, mastery per topik, mistake journal.
- [ ] **P2** Gamifikasi yang sehat: XP, level, achievement, challenge.
- [ ] **P2** School assignment & simple presentation maker.
- [ ] **P3** Voice tutor, pronunciation, virtual science/math labs.
- [ ] **P3** Group quiz/study room dengan moderasi yang sesuai usia.

## 2.2 Parent / Guardian Portal

- [ ] **P2** Akun wali dan mekanisme persetujuan sesuai usia/hukum.
- [ ] **P2** Dashboard kemajuan: latihan, topik, target, laporan mingguan.
- [ ] **P2** Ringkasan kesulitan dan saran dukungan belajar di rumah.
- [ ] **P2** Pengaturan waktu/kuota dan pembelian paket keluarga.
- [ ] **P2** Kontrol privasi dan akses anak berbasis kebutuhan, bukan pengawasan total percakapan.
- [ ] **P3** Komunikasi orang tua–pengajar melalui institusi.

> **Gate khusus:** pengguna anak memerlukan desain keamanan, persetujuan orang tua/wali, privasi, moderasi dan penilaian kepatuhan terhadap regulasi Indonesia sebelum diluncurkan.

---

# 3. B2B — Teacher, Academy, School & Campus LMS

## 3.1 Teacher / Lecturer Copilot

- [ ] **P2** Guru/dosen workspace dan library materi ajar.
- [ ] **P2** Upload silabus/RPS → course outline, lesson plan, learning objectives.
- [ ] **P2** AI teaching material generator: modul, handout, PPT, worksheet.
- [ ] **P2** Question bank generator: pilihan ganda, esai, kasus, rubrik.
- [ ] **P2** Review dan persetujuan manual sebelum materi/soal dipublikasikan.
- [ ] **P2** Assignment creation & deadline management.
- [ ] **P2** AI grading suggestions dan rubric-based feedback; final grade oleh pengajar.
- [ ] **P2** Class-level insights, question analysis, rekomendasi remedial.
- [ ] **P2** AI teaching assistant berbasis materi resmi kelas.
- [ ] **P3** Lesson planning berulang, diferensiasi materi berdasarkan kemampuan siswa.
- [ ] **P3** Curriculum mapping dan audit capaian pembelajaran.

## 3.2 LMS Core — Learning Delivery

- [ ] **P2** Buat institusi, course, kelas, cohort/batch, enrollment.
- [ ] **P2** Modul pelajaran: artikel, PDF, video, file, tautan, lesson sequence.
- [ ] **P2** Assignment submission, deadline, feedback, status pengerjaan.
- [ ] **P2** Quiz/test engine, bank soal, timer, attempts, scoring.
- [ ] **P2** Gradebook dasar, progress completion, course announcements.
- [ ] **P2** Student course dashboard + AI tutor berbasis course.
- [ ] **P2** Discussion / Q&A berbasis kelas, moderasi.
- [ ] **P2** Dashboard laporan pengajar dan learner progress.
- [ ] **P3** Attendance, certificate issuance, prerequisite paths.
- [ ] **P3** Advanced gradebook, rubric workflows, course templates.
- [ ] **P3** SCORM/QTI import-export sesuai spesifikasi yang didukung.

## 3.3 Bimbel, Bootcamp & Course Business (StudyAI Academy)

- [ ] **P2** Branding lembaga, katalog program, landing page kelas.
- [ ] **P2** Pendaftaran peserta, batch/cohort, paket belajar.
- [ ] **P2** Tryout berbayar, kuis dan analitik hasil.
- [ ] **P2** AI Tutor menggunakan modul milik bimbel.
- [ ] **P2** Pembayaran kursus, akses peserta, invoice dan riwayat transaksi.
- [ ] **P2** Dashboard instruktur & admin, kuota AI per lembaga.
- [ ] **P3** White-label logo/domain/tema.
- [ ] **P3** Coupon, referral lembaga, affiliate instruktur.
- [ ] **P3** Certificate, cohort analytics, membership, bundling course.
- [ ] **P3** Mobile branded app hanya jika ada kebutuhan dan economics kuat.

## 3.4 Institution Administration & Governance

- [ ] **P2** Multi-tenant organization dan pemisahan data antarlembaga.
- [ ] **P2** Role-based access: owner, admin, lecturer, teacher, learner, guardian.
- [ ] **P2** Class management, roster, academic calendar.
- [ ] **P2** Institute billing & AI allocation per organisasi/kursus/user.
- [ ] **P2** Data access permissions, audit log, admin analytics.
- [ ] **P2** Data export dan offboarding institusi.
- [ ] **P3** Attendance, accreditation-oriented reporting, custom report.
- [ ] **P3** SSO/OIDC/SAML, provisioning, advanced retention policy.
- [ ] **P3** SLA, enterprise backup, audit readiness dan incident process.
- [ ] **P3** Multi-campus/branch management.

> **Batas scope:** StudyAI LMS tidak otomatis menggantikan SIAKAD/SIS/ERP (KRS, UKT, payroll, penerbitan ijazah). Dahulukan integrasi daripada membangun modul administrasi berat.

## 3.5 StudyAI Connect — Integrasi LMS eksternal

- [ ] **P2** API internal terstruktur untuk courses, materials, quiz dan AI usage.
- [ ] **P3 [PARTNER]** LTI 1.3 / LTI Advantage: launch tool, context, role, assignment/grade service jika tersedia.
- [ ] **P3 [PARTNER]** Moodle integration / plugin.
- [ ] **P3 [PARTNER]** Canvas LMS integration.
- [ ] **P3 [PARTNER]** Google Classroom sync, sesuai izin dan kebijakan API.
- [ ] **P3** OneRoster, QTI/SCORM interoperability sesuai use case.
- [ ] **P3** Webhooks, API keys, partner portal, developer docs.
- [ ] **P3** White-label/embedded AI Tutor dan Assessment Widget.

## 3.6 Enterprise Training (ekspansi non-sekolah)

- [ ] **P3** Employee onboarding/course builder.
- [ ] **P3** SOP/company knowledge AI tutor.
- [ ] **P3** Training & compliance assessment.
- [ ] **P3** Completion, certificate, skill/competency tracking.
- [ ] **P3** Team/department dashboards, SSO, reporting.

---

# 4. Fondasi lintas produk (Engineering / Non-functional)

## 4.1 Platform & Infrastruktur

- [ ] **P0** Responsive web / PWA-ready UI, mobile-first untuk siswa.
- [ ] **P0** Backend/API, database, object storage, job processing.
- [ ] **P0** Auth, authorization, per-resource ownership checks.
- [ ] **P0** File extraction pipeline dengan batas tipe, halaman, ukuran.
- [ ] **P0** LLM abstraction layer dan model routing sesuai jenis tugas.
- [ ] **P0** AI generation history, retry policy, queue, error states.
- [ ] **P0** Source-grounded output & verifikasi dokumen dasar.
- [ ] **P0** Payment, entitlement, paket dan usage quota.
- [ ] **P0** Token and cost metering per user, model, fitur, job.
- [ ] **P0** Hard quotas, rate limiting, concurrency guard dan anti-abuse.
- [ ] **P0** Logging, monitoring, backup, restore test.
- [ ] **P1** Full-text search, semantic retrieval/embeddings, citations.
- [ ] **P1** Background jobs, idempotency, caching, lifecycle policies.
- [ ] **P1** Image/OCR/audio pipeline hanya saat fitur membutuhkan.
- [ ] **P2** Multi-tenancy foundation, tenant isolation tests dan audit log.
- [ ] **P3** API ecosystem, localization, international payments & data residency bila dibutuhkan.

## 4.2 Privacy, Safety, Quality & Legal

- [ ] **P0** Privacy policy, terms, refund/cancel policy, data deletion.
- [ ] **P0** Kontrol privasi dokumen; file dan indeks retrieval privat default.
- [ ] **P0** File scanning, upload validation, size/time limits.
- [ ] **P0** Prompt injection safeguards pada dokumen dan tool responses.
- [ ] **P0** Quality test set: PDF, PPTX, scan, tabel, rumus, jurnal, soal.
- [ ] **P0** Aturan anti-hallucination: jangan mengarang DOI, halaman, kutipan langsung, sumber.
- [ ] **P0** Periksa kebutuhan PSE, UU PDP, hak cipta, dan kontrak API sebelum komersialisasi.
- [ ] **P1** Output validation: PPTX/DOCX render, overflow, editability.
- [ ] **P1** Consent-based sharing, abuse reports dan moderasi.
- [ ] **P2** Child-safety/parental consent khusus School.
- [ ] **P2** Isolation tests antartenant & institutional data ownership.
- [ ] **P3** Security review, penetration testing, enterprise compliance roadmap.

## 4.3 Analytics, Growth & Admin

- [ ] **P0** Funnel: landing → sign-up → first value → purchase → repeat use.
- [ ] **P0** Error rate, generation success, biaya AI per workflow/user.
- [ ] **P0** Admin: users, transactions, jobs, quotas, support requests.
- [ ] **P1** Subscription retention/churn, cohort analysis, refunds, CAC per channel.
- [ ] **P1** Referral links, promo codes, campus ambassadors, affiliate commissions.
- [ ] **P1** Landing pages per use case, SEO/free tools yang memberi nilai nyata.
- [ ] **P2** KOL campaign attribution, community/partner reporting.
- [ ] **P3** International localization, multi-currency, tax/payout reporting.

---

# 5. User workflows yang menjadi produk utama

1. **Study Workflow:** Upload materi → rangkuman → tanya AI → kuis → review salah → progress.
2. **Research-to-Presentation:** Cari jurnal → simpan & baca → verifikasi sitasi → outline/draft → audit → PPTX → rehearsal.
3. **Academic Knowledge Workflow:** Buat course/project → simpan notes/file → hubungkan pengetahuan → AI search → transform jadi output.
4. **Semester Planner:** Upload RPS → konfirmasi jadwal/deadline → task plan → daily briefing → review progress.
5. **School Adaptive Learning:** Pilih kelas → diagnostik → tutor → latihan adaptif → tryout → parent report.
6. **Teacher Course Creation:** Upload silabus → draft course & soal → review guru → publish → nilai/feedback → analisis kelas.
7. **Academy Operation:** Daftarkan course → terima pembayaran → enrollment → kelas & tryout → AI tutor → laporan.
8. **LMS AI Extension:** LMS eksternal → integrasi StudyAI → tutor/assessment → hasil/insight sesuai permission.
9. **Career Workflow:** Pilih arah karier → skill gap → roadmap → project portfolio → CV → magang → mock interview.

---

# 6. Paket & monetisasi (hipotesis awal, bukan harga final)

| Lini | Contoh paket | Cara monetisasi |
|---|---|---|
| Campus | **Study Plus Rp69.000/bln** | Belajar, quiz, workspace, AI standar |
| Campus | **Academic Pro Rp129.000/bln** | Riset, sitasi, writing, PPT, kuota lebih tinggi |
| School | Free / Plus / Pro | Subscription siswa/orang tua; pricing khusus School diuji |
| Family | Paket beberapa profil | Subscription keluarga, batas kuota per profil |
| Academic Integrity | Similarity/Turnitin check **[PARTNER]** | Add-on bayar per cek/bundle berlisensi |
| Teacher | Individual educator plan | Subscription dengan kuota pembuatan materi/soal |
| Academy | Starter / Growth / White-label | Platform fee + seats aktif + AI credit add-ons |
| School & Campus | Institution License | Kontrak, onboarding, integrasi, support |
| Connect / Enterprise | API/Integration | Platform fee, usage, SLA, setup bila disepakati |

**Aturan margin:** notes dasar tidak memakai AI credits; AI generation, OCR, transcription, PPT berat dan similarity diberi kuota/harga terpisah. Jangan menawarkan *unlimited AI* sebelum usage cost dan abuse terkendali.

---

# 7. Roadmap eksekusi pengembangan

| Fase | Nama | Fokus deliverable | Syarat maju |
|---|---|---|---|
| **F0** | Discovery & Validation | 1 workflow bernilai, demo, interview, pembayaran awal | Ada kebutuhan dan pembeli nyata |
| **F1** | StudyAI Campus Core | Auth, notes/workspace, upload, summary, source, quiz, billing, metering | Pengguna bisa menyelesaikan workflow dan membayar |
| **F2** | Academic Productivity | Research library, DOI/citations, writing, PPTX, exam improvements | Retensi dan biaya tiap fitur terukur |
| **F3** | Daily Learning OS | Planner, task/deadline, knowledge search, collaboration sederhana | Penggunaan berulang & data terjaga |
| **F4** | B2B Pilot | Teacher Copilot, course/quiz/learner dashboard untuk tutor/bimbel | 1–3 pilot institusi benar-benar memakai & membayar |
| **F5** | School & Academy | Learner adaptive journey, parents, LMS, enrollment, institution billing | Safety & tenant controls lolos review |
| **F6** | Integration & Scale | LTI/Google/Moodle, white label, enterprise, internasional | Reliability, CAC, LTV, compliance layak |

### F1 — Backlog paling awal yang gua sarankan

1. **Account:** auth, profile, onboarding.
2. **Workspace:** mata kuliah/project, folder, halaman catatan sederhana, file library.
3. **Document pipeline:** upload PDF/PPTX, extract, validasi, simpan sumber.
4. **First paid workflow:** ringkasan + soal latihan + pembahasan dengan rujukan materi.
5. **Output UX:** preview, simpan ke workspace, riwayat, export ringan.
6. **Business:** paket Rp69rb/Rp129rb sebagai tes, payment, entitlement, kuota, biaya AI.
7. **Trust:** private-by-default, error handling, deletion, monitoring, TOS/privacy.
8. **Analytics:** activation, biaya tugas, conversion ke paid, repeat usage.

**Alternatif jika riset pengguna mengonfirmasi demand lebih tinggi:** jadikan **Research → Citation → Writing** sebagai first paid workflow, sambil tetap membangun shared workspace dan document engine yang sama. Jangan membangun kedua workflow lengkap secara paralel jika resource belum cukup.

### Yang **sengaja tidak** masuk F1

- Full Notion clone/database relations dan real-time editor.
- Full LMS/SIAKAD, integrasi enterprise, white-label.
- AI audio/video real-time, virtual lab, full mobile native.
- Public marketplace & peer tutoring.
- Turnitin check tanpa kontrak resmi atau hak redistribusi.
- Akses sekolah untuk pengguna anak sebelum child-safety matang.

---

# 8. Urutan ekspansi bisnis

1. **Local:** 1 komunitas / jurusan / kampus; validasi produk berbayar.
2. **Multi-campus:** ambassador, referral, KOL kecil, repeatable acquisition.
3. **National:** SEO/free tools, partnerships, content, paid ads dengan CAC sehat.
4. **B2B Pilot:** tutor atau bimbel kecil sebelum kontrak kampus besar.
5. **School:** pelajar/orang tua setelah keamanan dan learning engine cukup matang.
6. **International pilot:** English-first workflow untuk mahasiswa dewasa, satu pasar uji.
7. **Global B2B:** LMS integrations, institutional contracts, white-label, enterprise.

**Stage gate:** lanjut ekspansi berdasarkan bukti pembayaran, retention, kualitas, CAC, kontribusi per pelanggan dan kas; bukan berdasarkan tenggat kalender semata.

---

# 9. Peran/akses pengguna (perlu sejak desain database)

| Peran | Ruang lingkup |
|---|---|
| Personal Student | Notes, project, materi, pembelian & hasil pribadi |
| School Student | Materi kelas, latihan, tugas sesuai usia/izin |
| Parent / Guardian | Laporan perkembangan terbatas & pengaturan izin |
| Teacher / Lecturer | Kelola course, materi, assignments, review nilai |
| Tutor / Instructor | Seperti pengajar + kegiatan komersial kursus jika diizinkan |
| Institution Admin | Kelola organisasi, enrollment, billing, quota, laporan institusi |
| Platform Admin | Monitoring, support, penanganan insiden dan biaya |
| Integration Partner | API permissions yang dibatasi scope & kontrak |

**Aturan utama:** data personal mahasiswa tidak otomatis terlihat oleh kampus saat ia bergabung ke organization tenant. Akses lintas workspace harus eksplisit dan dapat dicabut sesuai kebijakan yang berlaku.

---

# 10. Product backlog / tiket yang dibuat setelah dokumen ini

Untuk tiap fitur yang akan dieksekusi, buat tiket dengan kolom minimum:

- **Feature ID & nama**
- **Persona** (student/teacher/parent/admin)
- **User problem & nilai bisnis**
- **Priority** (P0/P1/P2/P3)
- **Acceptance criteria / definition of done**
- **Dependencies** (API, data, lisensi, modul lain)
- **Cost driver** (tokens/storage/CPU/bandwidth/vendor)
- **Safety/privacy review**
- **Success metric** (activation, task success, repeat use, paid conversion, CAC, margin)

### Definition of Done — AI Feature

- [ ] Output berguna pada contoh dokumen dan kasus nyata.
- [ ] Sumber/kutipan jelas; ketidakpastian ditampilkan saat relevan.
- [ ] Batas token, halaman, retry, dan rate limit diterapkan.
- [ ] Biaya per job tercatat; kegagalan tidak menagih pengguna secara keliru.
- [ ] Akses ke data benar dan tidak bocor lintas akun/tenant.
- [ ] Loading, cancellation, errors, empty states, dan hasil tersimpan ditangani.
- [ ] Ada metric dan logging untuk menilai kualitas serta penggunaan.
- [ ] Dokumen dapat dihapus, termasuk turunannya sesuai kebijakan retensi.

---

## Kesimpulan keputusan produk

**Visi akhir:** StudyAI = Education Superapp / AI Education OS (B2C + B2B + AI learning infrastructure).  
**Fondasi bersama:** Identity, Knowledge, Document Intelligence, Learning Engine, AI Orchestration, Assessment, Billing, Analytics, Permission.  
**Produk awal untuk dijual:** **StudyAI Campus** dengan *satu academic workflow yang selesai end-to-end dan bisa dibayar*.  
**Ekspansi B2B paling realistis:** **Teacher Copilot / StudyAI Academy** untuk tutor dan bimbel, lalu integrasi dengan LMS yang sudah ada.  
**Aturan founder:** simpan daftar ini sebagai visi besar, tetapi pilih fitur sprint berdasarkan bukti pengguna dan cashflow, bukan banyaknya ide.
