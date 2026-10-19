# StudyAI Campus — Product Requirements Document (PRD)

**Versi:** 1.0  
**Tanggal:** 12 Oktober 2026  
**Status:** **FINAL sebagai baseline scope dan requirement produk Campus V1**; belum merupakan persetujuan final atas semua angka komersial, kontrak partner, ataupun physical database schema.  
**Produk:** StudyAI Campus (B2C, mahasiswa)  
**Nama rilis:** StudyAI Campus V1 / *Feature-Rich MVP*  
**Pemilik dokumen:** Product Owner, StudyAI  
**Kegunaan dokumen:** sumber acuan *what/why/acceptance criteria* untuk desain UX, backlog, arsitektur, Physical ERD, implementasi, QA, serta peluncuran.

> **Penting:** Dalam proyek ini istilah **MVP** berarti **peluncuran B2C Campus yang kaya fitur dan mencakup sembilan modul utama**, bukan prototipe dengan satu-dua workflow. Pengembangan boleh bertahap secara internal; target produk rilis publik tetap sembilan modul dengan alur lintas modul yang utuh. Rincian yang diberi label **BERSYARAT** tidak boleh dijanjikan tersedia sebelum lisensi, izin, integrasi, dan kualitasnya terpenuhi.

---

## 0. Aturan penggunaan PRD dan status keputusan

### 0.1 Hirarki keputusan

1. **Keputusan eksplisit terbaru pemilik produk dalam diskusi** mengungguli usulan terdahulu.
2. **PRD ini** menjadi acuan utama ruang lingkup, perilaku produk, UX dan acceptance criteria.
3. **Architecture Decision Record / dokumen arsitektur** menjadi acuan rancangan teknis. Arsitektur v1.2 yang dibahas masih *proposed baseline* sampai disahkan tersendiri.
4. `StudyAI_Master_Feature_List(1).md` adalah referensi visi lebih luas; item roadmap P2/P3 **tidak otomatis berarti semua wajib tersedia pada Campus V1**, kecuali dinyatakan dalam matriks kebutuhan dokumen ini.
5. Jika terjadi konflik antara dokumen, **catat dan eskalasi**; jangan diam-diam menghapus atau menambah fitur.

### 0.2 Label dalam dokumen

| Label | Arti |
|---|---|
| **DIKUNCI** | Scope/aturan produk disepakati; perubahan harus melalui keputusan eksplisit. |
| **V1-WAJIB** | Termasuk target penerimaan rilis Campus V1. |
| **V1-BERSYARAT** | Target produk, tetapi bergantung izin/lisensi, kualitas, biaya, atau layanan eksternal. Jika belum lolos gate, tidak boleh diklaim siap; pemilik produk menentukan dampak ke jadwal rilis. |
| **PASCA-V1 / EKSPANSI** | Tidak menjadi penghalang peluncuran Campus V1. |
| **HIPOTESIS** | Angka/harga/kuota/target yang perlu diuji, bukan janji komersial. |
| **TEKNIS-TERBUKA** | Pilihan implementasi final diputuskan dalam arsitektur/Physical ERD, tidak diada-adakan oleh PRD. |

### 0.3 Definisi selesai

Sebuah modul dinyatakan *complete* jika workflow utamanya bisa dilakukan dari awal sampai akhir; data tersimpan dengan benar; akses terlindungi; error/kuota ditangani; biaya pekerjaan AI tercatat; dan acceptance criteria pada bagian terkait lolos uji. Tersedianya tombol atau halaman saja **tidak cukup**.

---

## 1. Executive summary

### 1.1 Visi

**StudyAI — Your Personal Academic Operating System.** Satu workspace akademik pribadi tempat mahasiswa mengelola materi kuliah, catatan, pengetahuan, belajar dengan AI, mengerjakan tugas, mencari dan menganalisis jurnal, menulis karya ilmiah, membuat presentasi, serta mengelola aktivitas akademik.

**Prinsip produk:** **One Account + One Personal Knowledge Base + One Learning Profile + Multiple Connected AI Workflows.**

### 1.2 Masalah yang diselesaikan

Mahasiswa sering membagi aktivitas akademik ke banyak alat: penyimpanan berkas, aplikasi catatan, chatbot, pencari jurnal, pengelola sitasi, editor dokumen, pembuat slide, serta kalender. Materi menjadi tersebar, konteks perlu diulang, rujukan rentan tidak terlacak, dan kemajuan belajar sulit dipantau.

StudyAI menyatukan **sumber pengetahuan dan konteks kerja**, sehingga materi yang disimpan sekali dapat digunakan pada beberapa alur akademik tanpa mengunggah ulang.

### 1.3 Nilai utama (value proposition)

- **Organize:** ruang akademik terstruktur untuk mata kuliah, project, skripsi dan library pribadi.
- **Understand:** ringkasan, AI Tutor, flashcards, quiz dan umpan balik dengan rujukan materi.
- **Research:** cari sumber akademik, simpan metadata dan full text yang sah, analisis/matriks literatur, sitasi terverifikasi.
- **Create:** susun outline, draft akademik, revisi, bibliografi dan presentasi yang dapat diedit.
- **Manage:** jadwal, tugas, target, deadline dan progres dalam satu akun.

### 1.4 Strategi peluncuran

- **Target rilis pertama:** mahasiswa Indonesia, Web responsif.
- **Android/iOS:** disiapkan dalam desain API, format data, autentikasi dan billing; **tidak wajib rilis bersamaan dengan Web V1**.
- **Model bisnis:** Free yang tetap berguna setiap hari + Study Plus + Academic Pro (harga dan kuota saat ini hipotesis).
- **Pengembangan internal:** modul dibangun bertahap, dengan gate QA lintas fitur; **bukan** keputusan meluncurkan versi publik yang memangkas sembilan modul.

---

## 2. Target pengguna, persona, jobs-to-be-done

### 2.1 Pengguna utama — DIKUNCI

**Mahasiswa aktif**, termasuk mahasiswa tingkat awal, mahasiswa dengan banyak tugas, dan mahasiswa semester akhir yang sedang menyusun riset/skripsi. Produk harus nyaman untuk teks bahasa Indonesia dan dokumen akademik Inggris.

| Persona | Kebutuhan inti | Skenario kunci |
|---|---|---|
| **Mahasiswa perkuliahan reguler** | Mengorganisasi mata kuliah, memahami materi, latihan ujian | Upload materi → summary → tutor → quiz → review |
| **Mahasiswa tugas/proyek** | Mengelola project, sumber, draft dan presentasi | Project → outline → writing → PPTX |
| **Mahasiswa riset/skripsi** | Cari jurnal, verifikasi DOI, mengelola literatur, menyusun tulisan | Search → library → paper analysis → literature matrix → citation/draft |
| **Mahasiswa pengguna Free** | Tetap memakai notes, library, planner tiap hari meski AI terbatas | Buka workspace → catat → jadwal → review materi |

### 2.2 Aktor non-pengguna

- **Platform administrator/support:** memantau kesehatan pekerjaan, akun, kuota, pembayaran dan permintaan bantuan dalam lingkup minimal yang sah.
- **Penyedia eksternal:** AI models, metadata jurnal, storage, pembayaran, similarity partner; bukan aktor yang otomatis memperoleh akses ke seluruh data pengguna.

### 2.3 JTBD

1. Ketika menerima bahan kuliah, mahasiswa ingin menyimpan, menemukan dan memahami isi materi dengan cepat.
2. Ketika mempersiapkan ujian, mahasiswa ingin berlatih dari materi sendiri, menerima pembahasan dan mengulang kesalahan.
3. Ketika menerima tugas, mahasiswa ingin menyusun langkah pekerjaan, kerangka, tulisan dan presentasi dengan jejak sumber.
4. Ketika mengerjakan skripsi, mahasiswa ingin menemukan referensi asli, membandingkan paper dan membuat sitasi/bibliografi dengan metadata valid.
5. Ketika tidak mempunyai kuota AI, mahasiswa masih ingin membaca, mencatat dan mengatur seluruh aktivitas akademik.

---

## 3. Batas produk (scope)

### 3.1 Sembilan modul MVP — DIKUNCI

| Kode | Modul | Nilai utama |
|---|---|---|
| M01 | **Academic Workspace & Knowledge Management** | Pusat materi dan catatan pribadi; course, project, folder, nested pages, library |
| M02 | **Document Intelligence** | Upload/ekstraksi/ringkasan/analisis dokumen dengan rujukan sumber |
| M03 | **AI Tutor & Personalized Learning** | Bantuan memahami materi, belajar terpandu dan rekomendasi |
| M04 | **Quiz, Flashcards & Exam Preparation** | Latihan, hasil, pembahasan, review salah, progres |
| M05 | **Academic Research** | Paper/jurnal/buku, DOI, library, analisis paper dan literature matrix |
| M06 | **Academic Writing** | Outline, draft, literature review, parafrasa dengan atribusi, sitasi, export |
| M07 | **Presentation AI** | Membuat, meninjau, mengedit, dan mengekspor PPTX/PDF |
| M08 | **Academic Integrity** | Citation audit dan pemeriksaan sumber; similarity resmi bersyarat |
| M09 | **Academic Planner** | Jadwal, tugas, deadline, rencana belajar, reminder |

**Fondasi lintas produk (tidak dihitung sebagai modul ke-10):** Identity, auth, billing, quota, AI orchestration, source retrieval, document processing, data protection, admin minimum, observability, analytics.

### 3.2 Secara eksplisit di luar MVP — DIKUNCI

- **Collaboration:** sharing publik/antaruser, workspace kelompok, project collaboration, komentar multiuser dan real-time co-editing.
- **Career & Opportunities:** beasiswa, CV, internship finder, career roadmap dan skill-gap karier.
- **School Experience:** produk khusus SD/SMP/SMA, fitur berbasis usia, tryout sekolah dan gamifikasi sekolah.
- **Parent Experience:** portal wali, parental controls dan laporan orang tua.
- **B2B:** Teacher, Academy, Institution, Connect, Enterprise, LMS kelas/institusi, tenant administration penuh.

**Ditunda, bukan dihapus dari master roadmap.** Struktur kepemilikan harus dapat diperluas, tetapi pengguna Campus V1 tetap personal dan privat.

### 3.3 Batas fitur lanjutan dalam modul yang termasuk

Agar kata **complete** dapat diuji tanpa menyamakan semua ide P3 dengan kewajiban rilis, gunakan pembagian ini:

- **V1-WAJIB:** seluruh workflow sembilan modul dalam bagian 6–14, termasuk kemampuan riset, sitasi dan PPTX editable; bukan hanya demo.
- **V1-BERSYARAT:** similarity provider berlisensi; integrasi sumber yang pembatasan aksesnya belum disepakati; fitur media yang hak aksesnya berbeda menurut provider.
- **PASCA-V1 / EKSPANSI:** real-time voice tutor, full Notion-style database/formula engine, kolaborasi multiuser, multimedia understanding video panjang yang kompleks, proctoring institusional, agen akademik otonom end-to-end tanpa checkpoint, native mobile launch dan enterprise features.

**Catatan keputusan produk:** fitur Campus P2 seperti OCR, transkripsi, importer, analisis paper lanjutan dan planner lanjutan tetap dicantumkan sebagai target V1 sesuai requirement per modul. Butuh evaluasi kualitas, batas pemakaian dan izin sebelum dianggap siap. Jika pemilik produk menafsirkan “B2C complete” harus mencakup *setiap* ide P3 pada master roadmap, buat *change request* eksplisit terhadap daftar di atas — jangan diam-diam menambah scope.

---

## 4. Product experience, informasi dan navigasi

### 4.1 Struktur navigasi utama

1. **Dashboard** — aktivitas terbaru, continue learning, progres, deadline, usage plan.
2. **Workspace** — personal library, courses, projects, notes, files, folders, knowledge search.
3. **Study & Learn** — tutor, quiz, flashcards, exam prep dan learning profile.
4. **Research** — discover, reference library, reader, paper analysis/comparison, literature matrix.
5. **Create Studio** — writing, citations/integrity, presentation generator/editor.
6. **Planner** — tasks, kalender, deadline, study plan dan reminders.

Document Intelligence bekerja **di dalam reader/notes dan AI tools**, bukan harus menjadi satu menu top-level tambahan.

### 4.2 Hierarki organisasi

```text
Personal Account
└── Personal Data Space (batas ownership)
    ├── Personal Knowledge Library
    │   ├── Folders / nested pages / tags
    │   ├── Notes & documents
    │   └── References & saved AI outputs
    ├── Course Spaces
    │   └── Materi, notes, practice, assignments, learning progress
    └── Project Spaces
        └── Sources, notes, research, writing, presentations, tasks
```

Satu knowledge item boleh digunakan di banyak course/project **tanpa file fisik digandakan**. Pengguna dapat membuka sumber dari banyak konteks, sementara kepemilikannya tetap jelas.

### 4.3 Contextual AI

- Saat user membuka satu course/project/dokumen, AI dapat menggunakan **konteks yang dipilih dan diizinkan**.
- User dapat melihat, menambahkan dan mengeluarkan sumber dari konteks chat/pekerjaan AI.
- AI tidak otomatis membaca semua file personal hanya karena file itu tersimpan dalam Library.
- Output AI bisa disimpan ke note/project/library, tetapi **statusnya tetap output AI**, bukan otomatis referensi akademik asli.
- Rujukan hasil harus dapat membuka dokumen/halaman/slide atau versi sumber yang tersedia.

### 4.4 UX umum yang wajib

- Loading, empty state, processing, success, partial result, failed state dan retry yang jelas.
- Autosave notes dengan indikator simpan dan penanganan konflik.
- Riwayat hasil AI, pencarian, pekerjaan aktif dan progress job; tidak ada hilangnya output saat berpindah halaman.
- Kuota bisa dilihat dengan penjelasan ramah pengguna, sebelum tugas besar dikirim.
- Mobile web responsif: membuka/membaca/menavigasi harus nyaman di layar kecil, meski editor desktop dapat lebih kaya.
- Bahasa produk Indonesia sebagai default, dengan dukungan konten Indonesia dan Inggris.

---

## 5. Alur inti lintas modul (*golden paths*)

### GP-01 — Daily Learning

`Buat course → upload materi → sistem ekstrak & indeks → buka ringkasan → tanya AI berdasar sumber → generate quiz/flashcards → kerjakan → lihat skor & pembahasan → review salah → progres tersimpan.`

**Lulus jika:** user dapat menuntaskan alur tanpa upload ulang materi; sumber jawaban dapat ditelusuri; sesi/hasil tetap ada sesudah keluar/login kembali.

### GP-02 — Research to Thesis

`Buat project skripsi → cari paper → lihat DOI/metadata/verifikasi → simpan referensi → hubungkan PDF jika sah → analisis paper → bandingkan → literature matrix → susun outline/literature review → sisipkan sitasi → export draft.`

**Lulus jika:** referensi tidak dikarang, full-text yang tidak tersedia tidak direka, sitasi dapat ditelusuri ke metadata/sumber, export dapat dibuka.

### GP-03 — Assignment to Presentation

`Buat project tugas → kumpulkan notes/referensi → generate outline → edit draft → periksa citation audit → jadikan presentasi → edit & reorder slide → export PPTX editable/PDF.`

**Lulus jika:** dokumen dan slide saling terkait dengan project, pengguna dapat mengedit hasil, PPTX terbuka dan elemen utama tetap editable.

### GP-04 — Reuse Knowledge

`Simpan jurnal/file sekali → tempatkan di Course dan Project → gunakan sebagai sumber Tutor/Research/Writing → remove dari Course → item tetap ada di Library dan Project.`

**Lulus jika:** tidak dibuat copy blob baru, storage tidak dihitung dua kali, remove tidak menghapus sumber secara global.

### GP-05 — Everyday Free Usage

`Login Free → buat/buka course → tulis notes → baca PDF tersimpan → cek planner → kerjakan ulang kuis tersimpan → gunakan AI selama kuota tersedia → ketika kuota AI habis, kegiatan non-AI tetap berjalan.`

**Lulus jika:** limit AI tidak menyandera catatan, dokumen, export data pribadi yang diizinkan, planner, atau hasil latihan sebelumnya.

---

## 6. M01 — Academic Workspace & Knowledge Management

**Tujuan:** workspace akademik pribadi yang berguna bahkan tanpa menggunakan AI hari itu.

| ID | Requirement | Prioritas |
|---|---|---|
| WKS-01 | Dashboard dan onboarding: jurusan (opsional), semester/periode, tujuan belajar, preferensi bahasa. | V1-WAJIB |
| WKS-02 | Buat, ubah, arsip dan hapus **Course** serta **Project**; kelompokkan berdasarkan semester. | V1-WAJIB |
| WKS-03 | Personal Knowledge Library yang memuat notes, documents, references, saved AI output. | V1-WAJIB |
| WKS-04 | Folder bertingkat pada Library dan dalam Course/Project; buat, rename, move, reorder. | V1-WAJIB |
| WKS-05 | Notes/pages dengan nested pages, editor blocks: paragraf, heading, list, checklist, link, image, table; code/math/callout untuk konten akademik. | V1-WAJIB |
| WKS-06 | Autosave, revisi, restore, rename, move, Trash; perubahan dari perangkat lain tidak boleh diam-diam ditimpa. | V1-WAJIB |
| WKS-07 | Upload/organisasi materi dan lampiran; item bisa digunakan di banyak ruang tanpa copy blob. | V1-WAJIB |
| WKS-08 | Cari judul, konten notes, kata kunci dokumen, tag dan sumber berdasarkan izin pengguna. | V1-WAJIB |
| WKS-09 | Semantic **Ask My Knowledge** dengan pilihan sumber dan sitasi. | V1-WAJIB |
| WKS-10 | Transform Notes/Knowledge → summary, quiz, flashcards, outline, PPT melalui workflow modul terkait. | V1-WAJIB |
| WKS-11 | Backlinks/related items sederhana, favourites, tags, templates, pinned/recent. | V1-WAJIB |
| WKS-12 | Import/export minimal Markdown, DOCX dan format relevan yang didukung, dengan batas dan preview error. | V1-WAJIB |
| WKS-13 | Full relational Notion database engine, formula, real-time collaborative editor. | PASCA-V1 |

**Acceptance:**

- Membuat `Semester 5 / Machine Learning` dan `Skripsi` serta menempatkan satu jurnal di keduanya tidak menggandakan file maupun menaikkan storage dua kali.
- Nested page dapat dipindah tanpa menghasilkan siklus/hierarki rusak; parent dan child tetap satu pemilik.
- Autosave berhasil, history dipulihkan, dan konflik revisi dari dua perangkat ditangani secara eksplisit.
- Menghapus item dari satu workspace tidak otomatis menghapusnya dari Library.
- Pencarian tidak menampilkan item milik akun lain.

**Kasus batas:** folder kosong, item hilang karena Trash, move parent ke child, rename duplikat, import gagal, export konten dengan elemen editor yang belum didukung.

---

## 7. M02 — Document Intelligence

**Tujuan:** semua bahan belajar/riset dapat dibaca, dianalisis, disimpan dan dikutip dengan asal yang jelas.

| ID | Requirement | Prioritas |
|---|---|---|
| DOC-01 | Upload PDF, PPTX, DOCX; validasi tipe sebenarnya, ukuran, jumlah dan kuota. | V1-WAJIB |
| DOC-02 | Pembacaan dokumen dan preview; status ekstraksi/indeks dan penjelasan saat gagal. | V1-WAJIB |
| DOC-03 | Ekstraksi teks disertai halaman, slide, posisi bila tersedia; simpan versi sumber. | V1-WAJIB |
| DOC-04 | Ringkasan singkat/detail, key concepts, glossary, poin penting, FAQ. | V1-WAJIB |
| DOC-05 | Chat dengan satu/multiple documents, sumber dipilih secara eksplisit dan jawaban dengan rujukan. | V1-WAJIB |
| DOC-06 | Annotation/highlight pada bagian PDF yang mampu dipetakan; transform ke note. | V1-WAJIB |
| DOC-07 | OCR untuk dokumen scan/foto, status kualitas dan informasi bahwa hasil dapat tidak akurat. | V1-WAJIB; kualitas diuji |
| DOC-08 | Transkripsi rekaman kuliah/voice memo menjadi notes, hanya bila pengguna memiliki izin dan consent. | V1-BERSYARAT (media/izin/biaya) |
| DOC-09 | Pengolahan konten video/YouTube yang sah diakses dan diproses; metadata dan batas durasi jelas. | V1-BERSYARAT (hak akses/API) |
| DOC-10 | Memproses dokumen ulang tanpa upload ulang bila extractor/embedding model diperbarui. | V1-WAJIB |
| DOC-11 | Pemahaman multimedia panjang/diagram kompleks yang berakurasi tinggi tanpa tinjauan manusia. | PASCA-V1 |

**Acceptance:**

- Upload sukses langsung menjadi item Library, walau ekstraksi masih antre; kegagalan worker tidak menghilangkan file asli.
- Sumber halaman/slide berasal dari pemrosesan asli, tidak boleh ditebak LLM.
- Dokumen yang hanya memiliki gambar atau teks gagal dibaca tidak boleh ditampilkan sebagai “sudah dianalisis lengkap”.
- Versi file baru tidak secara diam-diam mengubah provenance jawaban AI terdahulu.
- File berbahaya/rusak/terlalu besar ditolak dengan pesan dan tidak dieksekusi.

---

## 8. M03 — AI Tutor & Personalized Learning

**Tujuan:** membantu pemahaman mahasiswa berdasarkan materi yang relevan, bukan sekadar menjawab bebas tanpa sumber.

| ID | Requirement | Prioritas |
|---|---|---|
| TUT-01 | AI Tutor dari course, project, document atau Knowledge Library sesuai pemilihan sumber. | V1-WAJIB |
| TUT-02 | Mode penjelasan sederhana, contoh/analogi, langkah demi langkah, penjelasan mendalam. | V1-WAJIB |
| TUT-03 | Guided/Socratic mode: memberikan petunjuk bertahap dan pertanyaan pemantik. | V1-WAJIB |
| TUT-04 | Chat history tersimpan; sumber yang dipakai dapat dilihat; konteks tidak menyeberang akun. | V1-WAJIB |
| TUT-05 | Rekomendasi topik berdasarkan hasil quiz, kesalahan dan tujuan belajar. | V1-WAJIB |
| TUT-06 | Learning path dan targeted study plan yang dapat diubah pengguna. | V1-WAJIB |
| TUT-07 | Explain-back: mahasiswa menjelaskan konsep, AI memberi feedback; bukan nilai resmi akademik. | V1-WAJIB |
| TUT-08 | AI oral exam/presentation Q&A dalam bentuk chat/teks. | V1-WAJIB |
| TUT-09 | Voice tutor real-time dua arah. | PASCA-V1 |

**Acceptance:**

- Tutor dapat menjawab dari sumber yang dipilih, menampilkan rujukan dan menyatakan ketidakpastian bila sumber tidak mendukung.
- Prompt dalam PDF tidak dapat mengubah permission, meminta file lain atau memicu tindakan tak diizinkan.
- Jika user meminta jawaban di luar cakupan, tutor menawarkan pengetahuan umum yang dilabeli jelas (jika mode mengizinkan) atau meminta sumber tambahan.
- Perubahan learning profile hanya berdasarkan data aktivitas nyata atau pilihan pengguna, bukan asumsi bebas model.

---

## 9. M04 — Quiz, Flashcards & Exam Preparation

**Tujuan:** latihan berbasis materi yang dapat diulang dan menunjukkan progres yang benar-benar terukur.

| ID | Requirement | Prioritas |
|---|---|---|
| LRN-01 | Generate quiz dari materi terpilih; jumlah soal, topik, tingkat kesulitan dan tipe soal. | V1-WAJIB |
| LRN-02 | Pilihan ganda, true/false, isian, esai sederhana, studi kasus; output tervalidasi. | V1-WAJIB |
| LRN-03 | Mode practice/exam: timer opsional, submit, skor otomatis untuk soal objektif, pembahasan. | V1-WAJIB |
| LRN-04 | Jawaban soal/pembahasan punya rujukan materi bila sumber tersedia. | V1-WAJIB |
| LRN-05 | History attempt, review wrong answers, retry, bank soal personal, randomization. | V1-WAJIB |
| LRN-06 | Flashcard generator, review manual dan spaced repetition schedule. | V1-WAJIB |
| LRN-07 | Dashboard penguasaan topik dan tren latihan; weakness detection berbasis attempts. | V1-WAJIB |
| LRN-08 | Exam countdown, targeted practice dan study planning terhubung Planner. | V1-WAJIB |
| LRN-09 | Mengulang quiz/flashcard yang sudah disimpan tanpa perlu panggilan AI baru. | V1-WAJIB |
| LRN-10 | Link berbagi kuis publik, proctoring institusional, bank soal sekolah/UTBK sebagai produk khusus. | PASCA-V1 / di luar scope |

**Acceptance:**

- Sesi quiz yang telah di-submit tersimpan; nilai dan jawaban salah bisa dibuka kemudian.
- Soal objektif dinilai deterministik; esai diberi *AI feedback*, bukan dinyatakan sebagai nilai resmi.
- Pertanyaan dengan kunci ambigu atau tidak didukung sumber harus dapat ditandai/dilaporkan.
- Repeat practice tidak membuat tagihan AI kecuali meminta soal/pembahasan baru.

---

## 10. M05 — Academic Research

**Tujuan:** pencarian, organisasi, evaluasi dan pemanfaatan sumber ilmiah yang dapat diverifikasi.

| ID | Requirement | Prioritas |
|---|---|---|
| RES-01 | Search paper/jurnal menurut topik, judul, DOI, penulis; cari buku dan metadata. | V1-WAJIB |
| RES-02 | Integrasi metadata akademik melalui sumber sah (mis. OpenAlex, Crossref, Semantic Scholar, Google Books bila sesuai syarat). | V1-WAJIB, provider final tergantung ketentuan |
| RES-03 | Filter tahun, field, bahasa, open access, relevansi; tampilkan sumber data dan link asli. | V1-WAJIB |
| RES-04 | Normalisasi dan cek DOI/metadata sebelum sitasi dinyatakan terverifikasi. | V1-WAJIB |
| RES-05 | Save to Research Library, hubungan ke Project/Course, dedup DOI dalam satu Data Space. | V1-WAJIB |
| RES-06 | Ringkas paper dari full text yang tersedia secara sah: tujuan, metode, hasil, keterbatasan. | V1-WAJIB |
| RES-07 | Paper compare, literature matrix, note/annotation, sumber per klaim. | V1-WAJIB |
| RES-08 | Similar paper/recommendation dan citation-network exploration jika metadata tersedia. | V1-WAJIB, dataset terbatas boleh ditandai |
| RES-09 | Research gap explorer menghasilkan *hipotesis dan bukti/ketidakpastian*, bukan klaim novelty pasti. | V1-WAJIB |
| RES-10 | Import/export referensi: BibTeX, RIS dan metadata yang didukung. | V1-WAJIB |
| RES-11 | Workflow systematic review otomatis penuh yang menyatakan cakupan lengkap tanpa verifikasi. | PASCA-V1 |

**Acceptance:**

- Search yang tidak mengembalikan hasil menampilkan empty state, bukan jurnal buatan model.
- Menyimpan DOI yang telah ada di Trash menawarkan restore, bukan membuat referensi duplikat tanpa penjelasan.
- Reference tanpa PDF tetap bisa disimpan, dikutip berdasarkan metadata yang benar dan ditandai belum tersedia full text.
- Analisis paper tidak mengklaim membaca keseluruhan isi ketika baru memiliki abstrak/metadata.
- Sumber ekspor bibliografi menampilkan metadata yang digunakan dan status verifikasinya.

---

## 11. M06 — Academic Writing

**Tujuan:** membantu mahasiswa menulis dengan sumber yang jelas, menghindari referensi fiktif dan menjaga integritas akademik.

| ID | Requirement | Prioritas |
|---|---|---|
| WRT-01 | Writing project/dokumen: struktur bagian, outline makalah/laporan/proposal/skripsi. | V1-WAJIB |
| WRT-02 | Draft assistant per bagian dengan sumber pilihan, revisi dan riwayat perubahan. | V1-WAJIB |
| WRT-03 | Literature review assistant menggunakan sumber Research Library yang valid. | V1-WAJIB |
| WRT-04 | Perbaikan bahasa, academic tone, koherensi, istilah; parafrasa dengan atribusi. | V1-WAJIB |
| WRT-05 | Citation generator dan bibliography APA, IEEE, MLA, Chicago memakai formatting engine dan metadata yang diverifikasi. | V1-WAJIB |
| WRT-06 | Direct quote hanya bila teks dan lokasi cocok dengan sumber yang diakses. | V1-WAJIB |
| WRT-07 | Claim-to-source linking: tandai klaim yang belum mempunyai dukungan sumber. | V1-WAJIB |
| WRT-08 | Template penulisan/style guide yang di-upload; peringatan bila aturan tak terbaca. | V1-WAJIB |
| WRT-09 | Preview serta export DOCX/PDF; struktur teks, heading, tabel, referensi diperiksa. | V1-WAJIB |
| WRT-10 | Saran revisi/compare versions dalam dokumen personal. | V1-WAJIB |
| WRT-11 | Multiuser comments/review bersama dan submit tugas otomatis sebagai karya final tanpa verifikasi mahasiswa. | PASCA-V1 / di luar scope |

**Acceptance:**

- Model AI tidak mengarang DOI, nomor halaman, penulis, teks kutipan langsung atau mengubah kutipan tanpa label.
- Ketika sumber tidak cukup, sistem menandai klaim unsupported; tidak otomatis membuat referensi pengganti.
- Pengguna dapat mengedit hasil dan mengekspor file DOCX/PDF yang terbuka dengan struktur yang layak.
- Data referensi yang digunakan tercatat pada dokumen/draft version yang relevan.
- Produk diposisikan sebagai **asisten penulisan**, penulis tetap memverifikasi fakta, metodologi dan kepatuhan akademik.

---

## 12. M07 — Presentation AI

**Tujuan:** dari materi/draft akademik menjadi presentasi yang dapat diedit dan diekspor.

| ID | Requirement | Prioritas |
|---|---|---|
| PPT-01 | Prompt, outline, PDF/DOCX, makalah atau source bundle → struktur presentasi. | V1-WAJIB |
| PPT-02 | Template akademik: tugas kuliah, seminar, laporan dan presentasi skripsi. | V1-WAJIB |
| PPT-03 | Generate judul, struktur dan konten tiap slide dengan rujukan jika berbasis sumber. | V1-WAJIB |
| PPT-04 | Preview deck, edit teks, reorder, delete dan regenerate slide tertentu. | V1-WAJIB |
| PPT-05 | Chart/table/diagram dari data yang diberikan, dengan label sumber dan peringatan jika data tidak lengkap. | V1-WAJIB |
| PPT-06 | Speaker notes dan presentasi script. | V1-WAJIB |
| PPT-07 | Rehearsal Q&A berbasis teks/sumber presentasi. | V1-WAJIB |
| PPT-08 | Export PPTX **editable** dan PDF; pemeriksaan layout/overflow. | V1-WAJIB |
| PPT-09 | Editor grafis bebas sekelas Canva, animasi kompleks, voice rehearsal real-time. | PASCA-V1 |

**Acceptance:**

- PPTX hasil export dapat dibuka di aplikasi presentasi dan teks utama dapat diedit, bukan sekadar rangkaian gambar.
- Pengguna dapat mengubah satu slide tanpa membuat ulang seluruh deck.
- Gambar/grafik yang membutuhkan aset berhak cipta mengikuti kebijakan sumber dan izin; tabel/grafik tidak memalsukan angka.
- Tugas berat memberikan status, retry, dan pesan kegagalan yang bisa dipahami.

---

## 13. M08 — Academic Integrity & Similarity

**Tujuan:** membantu menjaga atribusi dan sumber karya akademik; tidak menjanjikan plagiarisme nol.

| ID | Requirement | Prioritas |
|---|---|---|
| INT-01 | Citation audit: referensi tak lengkap, DOI tak valid, bibliografi tak konsisten. | V1-WAJIB |
| INT-02 | Deteksi klaim tanpa sumber dan perbedaan kutipan langsung versus parafrasa. | V1-WAJIB |
| INT-03 | Rekomendasi perbaikan atribusi/sitasi, tanpa saran menyamarkan plagiarisme. | V1-WAJIB |
| INT-04 | Similarity check melalui layanan yang **memiliki lisensi API dan hak penggunaan/redistribusi yang sah**. | **V1-BERSYARAT / PARTNER GATE** |
| INT-05 | Report/highlight sumber yang cocok, export laporan, opsi no-repository **hanya jika provider mendukung**. | **V1-BERSYARAT / PARTNER GATE** |
| INT-06 | Menyebut alat pihak lain sebagai **Turnitin resmi** tanpa persetujuan/kontrak Turnitin. | **DILARANG** |

**Acceptance:**

- Citation audit bekerja meskipun tidak ada penyedia similarity eksternal.
- Similarity percentage ditampilkan sebagai indikator kemiripan, **bukan** keputusan otomatis bahwa tulisan plagiarisme.
- Jika partner belum ada, fitur similarity berstatus “belum tersedia”, tidak ditagih dan tidak dipasarkan seolah aktif.
- Hasil similarity disimpan/dihapus sesuai kontrak partner dan kebijakan privasi.
- Similarity dapat menjadi **add-on per pemeriksaan**; harga/entitlement ditentukan terpisah.

**Keputusan peluncuran yang memerlukan owner:** jika kerja sama belum tersedia menjelang rilis, pemilik produk perlu memilih *menahan rilis atau meluncurkan delapan modul + citation audit dengan similarity resmi dinyatakan pending*. PRD **tidak** memalsukan status partner demi label “complete”.

---

## 14. M09 — Academic Planner

**Tujuan:** membuat StudyAI relevan setiap hari di luar aktivitas AI berbayar.

| ID | Requirement | Prioritas |
|---|---|---|
| PLN-01 | Kalender kegiatan akademik: kelas, ujian, deadline, event personal. | V1-WAJIB |
| PLN-02 | Task/assignment manager dengan due date, priority, status, hubungan ke course/project. | V1-WAJIB |
| PLN-03 | Reminder dalam aplikasi/email sesuai preferensi dan izin; daily/weekly briefing. | V1-WAJIB |
| PLN-04 | Study plan, target, exam countdown, dan Pomodoro/focus timer. | V1-WAJIB |
| PLN-05 | Upload RPS/silabus → saran jadwal/topik/deadline **untuk dikonfirmasi sebelum disimpan**. | V1-WAJIB |
| PLN-06 | AI task breakdown untuk tugas besar/skripsi; user dapat mengedit langkah. | V1-WAJIB |
| PLN-07 | Grade tracker dan target IP/IPK dengan perhitungan yang transparan serta penjelasan asumsi. | V1-WAJIB |
| PLN-08 | Calendar sync pihak ketiga (Google/Microsoft) hanya setelah izin dan konektor tersedia. | V1-BERSYARAT |
| PLN-09 | Automasi proaktif yang mengambil tindakan sensitif tanpa persetujuan pengguna. | DI LUAR SCOPE |

**Acceptance:**

- Pengguna Free yang kuota AI-nya habis dapat tetap melihat dan mengedit seluruh tugas serta jadwal.
- Hasil ekstraksi RPS tidak langsung menjadi deadline nyata sebelum persetujuan pengguna.
- Reminder tidak terduplikasi setelah job retry.
- Kalender menampilkan waktu sesuai zona waktu akun; sistem menyimpan waktu standar dan menghindari ketidakjelasan tanggal.

---

## 15. Platform foundation — persyaratan lintas sembilan modul

### 15.1 Akun, onboarding, keamanan sesi

| ID | Requirement | Prioritas |
|---|---|---|
| PLT-01 | Registrasi, login, logout, verifikasi email, reset password; sesi dikelola dengan aman. | V1-WAJIB |
| PLT-02 | Profil mahasiswa, preferensi bahasa/zona waktu dan onboarding minimum. | V1-WAJIB |
| PLT-03 | Satu Personal Data Space default per akun dan kontrol akses setiap resource. | V1-WAJIB |
| PLT-04 | Pengaturan privasi, export data dan permintaan penghapusan akun/data. | V1-WAJIB |
| PLT-05 | Google/Apple login, MFA, perangkat dan manajemen sesi lanjut. | V1-BERSYARAT sesuai keputusan auth |

### 15.2 AI orchestration dan data handling

| ID | Requirement | Prioritas |
|---|---|---|
| PLT-06 | Gateway model yang dapat mengganti provider dan mengatur model per fitur. | V1-WAJIB |
| PLT-07 | Prompt/source grounding, source selection dan validasi permission **sebelum** retrieval. | V1-WAJIB |
| PLT-08 | AI job status, retry, cancellation yang aman, progress, history dan error handling. | V1-WAJIB |
| PLT-09 | Model/token/cost metering per akun, fitur dan job; budget protection dan rate limit. | V1-WAJIB |
| PLT-10 | Kualitas dicek memakai dataset evaluasi PDF, PPTX, DOCX, tabel, rumus, jurnal, citations dan soal. | V1-WAJIB |

### 15.3 Subscription, quota dan pembayaran

| ID | Requirement | Prioritas |
|---|---|---|
| PLT-11 | Paket Free/Plus/Pro, entitlement per fitur, usage quota dan perubahan paket berbasis data konfigurasi. | V1-WAJIB |
| PLT-12 | Payment provider untuk Web dan verifikasi transaksi dari server, bukan berdasarkan redirect halaman sukses saja. | V1-WAJIB |
| PLT-13 | Reserve → commit/release untuk pekerjaan berbiaya; deduplikasi/idempotency request. | V1-WAJIB |
| PLT-14 | Riwayat pembayaran, masa berlaku paket, reminder perpanjangan, status pembayaran gagal. | V1-WAJIB |
| PLT-15 | Storage usage dapat dilihat, mekanisme downgrade dan over-quota tidak menghapus file otomatis. | V1-WAJIB |
| PLT-16 | Billing iOS/Android: desain entitlement lintas platform, implementasi store policy saat mobile rilis. | Mobile-ready / nanti |

### 15.4 Admin, analytics dan observability

| ID | Requirement | Prioritas |
|---|---|---|
| PLT-17 | Admin minimal: status akun, transaksi, AI jobs, error, kuota, tiket dukungan. | V1-WAJIB |
| PLT-18 | Funnel: visit → sign up → first value → repeat use → purchase; DAU/MAU dan retensi. | V1-WAJIB |
| PLT-19 | Monitoring aplikasi, antrean, database, storage, biaya API, dan backup/restore testing. | V1-WAJIB |
| PLT-20 | Logs tidak menyimpan password, signed URL, atau isi penuh dokumen/prompt sensitif secara default. | V1-WAJIB |

---

## 16. Free, Plus, Pro — policy dan batas penggunaan

### 16.1 Prinsip freemium — DIKUNCI

- **Free adalah produk yang berguna setiap hari**, bukan trial singkat.
- Notes, workspace, planner, browsing Library dan mengulang kuis/flashcards yang sudah tersimpan tetap tersedia saat kuota AI habis.
- Kuota dibatasi untuk **pekerjaan generatif/komputasi mahal**, bukan akses ke pengetahuan pengguna sendiri.
- Perhitungan internal menggunakan usage metering dan hard budget cap; **tidak ada AI unlimited** pada fase awal.
- Penggunaan ulang sumber/file di beberapa workspace **tidak menghitung storage berulang**.
- Data pengguna tetap miliknya; downgrade atau habis kuota **tidak otomatis menghapus berkas**.

### 16.2 Rancangan harga — HIPOTESIS

| Paket | Harga hipotesis | Sasaran |
|---|---:|---|
| Free | Rp0 | Workspace harian + pengalaman AI terbatas |
| Study Plus | Rp69.000/bulan | Belajar dan tugas rutin |
| Academic Pro | Rp129.000/bulan | Riset, skripsi, presentasi dan AI intensif |

Harga tersebut **belum tervalidasi** melalui willingness-to-pay, biaya model AI nyata, biaya server atau margin. Pembayaran satu semester/top-up/add-on dapat diuji setelah struktur dasar stabil.

### 16.3 Storage/quota usulan — HIPOTESIS, bukan janji komersial

| Batas | Free | Study Plus | Academic Pro |
|---|---:|---:|---:|
| Storage personal | **500 MB** | **5 GB** | **20 GB** |
| Maksimal ukuran satu file | 20 MB | 50 MB | 100 MB |
| Halaman baru diproses AI per bulan | 200 | 1.500 | 4.000 |
| Jumlah Course/Project/Notes | Tidak ada limit jumlah per fitur; tetap tunduk storage/fair use | Sama | Sama |
| Membaca dokumen tersimpan | Tetap diizinkan | Sama | Sama |
| Mengerjakan ulang quiz/flashcard tersimpan | Tidak mengonsumsi generasi AI baru | Sama | Sama |

**Implikasi biaya:** 100 pengguna Pro dengan limit 20 GB berarti **alokasi maksimal 2 TB**, bukan otomatis pemakaian aktual 2 TB. Budget harus menghitung objek aktual, versi, backup, egress/operasi, dan proses AI secara terpisah.

### 16.4 Contoh feature limits awal — HIPOTESIS

| Fitur | Free | Study Plus | Academic Pro |
|---|---|---|---|
| AI Tutor | 8 pesan/hari | 40 pesan/hari | 100 pesan/hari |
| Summary | 3/minggu | 30/bulan | 100/bulan |
| Generate Quiz | 3/minggu | 30/bulan | 100/bulan |
| Flashcards AI | 3/minggu | 30/bulan | 100/bulan |
| Writing edits | 5/minggu | 60/bulan | 200/bulan |
| Outline | 2/minggu | 20/bulan | 60/bulan |
| Journal metadata search | 10/hari | 50/hari | 100/hari |
| Full-text paper analysis | 2/minggu | 20/bulan | 80/bulan |
| AI Presentation | 1 deck pendek/bulan | 5 deck/bulan | 15 deck/bulan |
| Similarity check | Add-on/partner | Add-on/partner | Add-on/partner |

**Aturan tambahan:** angka per fitur adalah *upper bounds*, bukan jaminan pengguna bisa menghabiskan semuanya sekaligus tanpa batas global. Backend menetapkan page/token/slide/duration limit, concurrency cap, fraud/rate limits, dan **aggregated AI budget**. Semua harus dikonfigurasi dari database/config, tidak hardcode. Hasil gagal tidak boleh menghasilkan pemotongan kuota final yang tidak benar; provider cost tetap dicatat.

### 16.5 Aturan langganan dan batas kuota

- Free bisa digunakan tanpa kartu pembayaran; akun baru harus dapat mencoba nilai AI dalam batas wajar.
- Periode langganan dan tanggal reset tampil jelas; metode pembayaran prabayar atau berulang mengikuti kemampuan provider.
- Saat upgrade, entitlement bertambah setelah pembayaran tervalidasi.
- Saat downgrade dan data melebihi kuota: akses baca/download/hapus tetap ada, upload baru dapat dibatasi; tidak menghapus otomatis.
- Saat layanan sedang mahal/overloaded, tampilkan keadaan transparan dan retry; jangan memotong kuota tanpa proses yang berhasil sesuai kebijakan.
- Similarity check sebagai produk partner diperlakukan terpisah dari kredit AI biasa.

### 16.6 Uji unit economics sebelum harga/limit disahkan

Hitung biaya per Free DAU, pengguna Plus dan Pro; biaya per AI workflow (simple chat, grounded chat, OCR, paper analysis, writing, slide export); persentil pemakaian, conversion rate, refund, payment fees, storage + backup, dan global safety cap. Tidak boleh membuat klaim margin profit atau kapasitas pengguna sebelum pengukuran.

---

## 17. AI trust, academic integrity dan kualitas output

### 17.1 Klasifikasi sumber

Setiap hasil AI harus bisa membedakan:

1. **Original source:** isi dokumen/file/notes nyata yang diakses.
2. **Verified bibliographic metadata:** judul, DOI, author, venue, tahun dari katalog/metadata source.
3. **AI inference/generated text:** rangkuman, penjelasan, draft, hipotesis.
4. **Unknown/unverified:** konten yang belum tersedia, metadata belum terkonfirmasi atau klaim yang belum didukung.

### 17.2 Source provenance

- Penyimpanan percakapan juga mencatat **sumber yang dipilih user**.
- Jawaban yang mengutip sumber menyimpan **sumber, versi, locator**, dan kutipan bila benar-benar berasal dari teks nyata.
- Versi PDF/notes yang baru tidak menulis ulang provenance jawaban lama.
- Kutipan yang tersimpan mengikuti kebijakan penghapusan data, bukan menjadi backdoor untuk mempertahankan data setelah permintaan delete permanen.
- Model tidak boleh menghasilkan DOI/halaman/kutipan langsung yang tidak didukung indeks sumber.

### 17.3 Pengamanan AI

- Dokumen dan hasil tool merupakan **untrusted input**. Instruksi yang ditemukan di PDF/jurnal tidak memperoleh hak menjadi instruksi sistem.
- AI tidak boleh mengakses sumber di luar hak pengguna, terlepas dari permintaan prompt.
- Pekerjaan berbahaya secara akademik ditangani dengan fitur atribusi/sumber; aplikasi tidak memasarkan upaya menyamarkan plagiarisme.
- Literature gap dan klaim ilmiah harus ditampilkan sebagai bantuan eksplorasi, bukan keputusan riset final.
- Fitur export harus memeriksa editability, layout, overflow dan konsistensi sumber.

---

## 18. Data, ownership, lifecycle dan mobile readiness (product contract)

### 18.1 Konsep inti (DIKUNCI sebagai prinsip, bukan tabel fisik)

- **User Account:** identitas mahasiswa.
- **Personal Data Space:** batas kepemilikan/akses data.
- **Content Space:** konteks organisasi, misalnya Library, Course, Project (usulan architecture v1.2).
- **Knowledge Item:** identitas satu konten/logical item.
- **Item Placement:** pemunculan item di satu atau banyak ruang/folder tanpa duplikasi sumber.
- **Source Version:** versi dokumen/notes/referensi yang menjadi dasar output AI.
- **Learning Profile:** riwayat latihan dan progres yang dapat dijelaskan asalnya.

### 18.2 Perilaku konten

| Aksi user | Hasil yang wajib |
|---|---|
| Add existing item ke project | Tambah relasi/placement, bukan copy file asli |
| Remove item dari satu course | Relasi course hilang, Library dan project lain tidak berubah |
| Move note ke folder | Lokasi tampilan berubah, ID dan riwayat konten bertahan |
| Replace file version | Versi baru diproses; citation lama tetap terkait versi lama |
| Delete item | Masuk Trash dan menampilkan dampak terhadap link/hasil AI |
| Restore | Item dan relasi yang masih layak dipulihkan secara konsisten |
| Permanently delete | Hapus file/indeks/artefak turunan sesuai jadwal retensi dan permintaan hukum yang sah |
| Edit dari perangkat kedua | Revisi lama ditolak/konflik ditangani, tidak silent overwrite |

### 18.3 Mobile

- Backend API adalah pintu masuk Web/Android/iOS; klien tidak berinteraksi langsung dengan database.
- Editor menyimpan format terstruktur, berversi dan dapat diinterpretasikan klien mobile.
- Auth/session mobile harus aman; sync memakai revisi dan konflik eksplisit.
- Web V1 **online-first**; native offline sync, local draft dan push native dievaluasi ketika mobile dikerjakan.
- Entitlement terpusat per akun, tetapi pembayaran digital dalam aplikasi mobile harus mengikuti kebijakan platform yang berlaku saat rilis.

### 18.4 Privacy & legal signoff

Sebelum berbayar publik: kebijakan privasi, terms, refund/cancel, data deletion, hak cipta dan hak memproses materi, pemeriksaan kewajiban regulasi Indonesia (termasuk PSE/UU PDP sesuai penilaian hukum terkini), serta kesesuaian kontrak layanan AI/storage/payment.

---

## 19. Non-functional requirements (NFR)

Target angka di bawah diberi **PROPOSED ENGINEERING SLO**; finalnya harus disahkan setelah performance/load testing pada target deployment. Jangan dibaca sebagai garansi komersial.

| ID | Area | Kriteria |
|---|---|---|
| NFR-01 | Authorization | Semua operasi terhadap konten personal melalui pemeriksaan pemilik/izin; automated cross-user tests wajib. |
| NFR-02 | Secure storage | Dokumen privat default, file di object storage; download melalui akses sementara/terotorisasi. |
| NFR-03 | Stability | Job panjang tidak memblokir request biasa; status, timeout, retry, cancellation dan recovery jelas. |
| NFR-04 | Backup | Backup PostgreSQL, object storage dan konfigurasi kritis di luar server utama; restore diuji sebelum launch. |
| NFR-05 | Portability | Docker-based deployment; staging dan production dapat dipindah laptop → VPS → cloud tanpa perubahan bisnis utama. |
| NFR-06 | API | Kontrak berversi, terdokumentasi, kompatibel Web/future Mobile; validasi input/output dan pagination. |
| NFR-07 | Data integrity | Referential/owner constraints, transaction boundaries dan idempotency untuk kuota/pembayaran/jobs. |
| NFR-08 | Search | Hasil selalu dibatasi kepemilikan dan sumber; indeks dievaluasi terhadap dataset nyata. |
| NFR-09 | Performance | **Usulan uji:** P95 API read umum di bawah ~500 ms pada beban awal yang didefinisikan; AI jobs punya SLO tersendiri. |
| NFR-10 | Mobile Web | Dashboard, reader dan fungsi inti tetap usable di viewport mobile; layout diuji pada perangkat realistis. |
| NFR-11 | Accessibility | Navigasi keyboard, label input, kontras dan teks status AI/kuota yang bisa dipahami. |
| NFR-12 | Observability | Error rate, job success/latency, token/cost, storage, queue, DB pressure dan payment reconciliation terlihat oleh admin. |
| NFR-13 | Data recovery | Trash/restore dan permanent delete mengikuti kebijakan yang diumumkan; backup-retention terpisah. |
| NFR-14 | AI reliability | Dataset eval untuk hallucinated citations, OCR, PDF scans, mixed-language, notes, quiz correctness dan PPT layout. |
| NFR-15 | Cost protection | Rate limiting, concurrency guard, per-user + global cost caps, retries terbatas, fraud/abuse handling. |
| NFR-16 | Maintainability | Modular code boundaries, migrations ber-version, test suite, ADR, CI checks dan API documentation. |

### 19.1 Load/performance testing minimum

Uji listing Library, membuka notes, autosave, mengambil items project, hybrid retrieval bersumber terpilih, upload/scan/parse PDF, AI streaming, quiz submissions, concurrent quota reservations, PPTX export, dan payment webhook retry.

### 19.2 Reliabilitas output AI

Jangan menetapkan “AI akurat 100%”. Tetapkan benchmark internal dan acceptance rate dari corpus evaluasi nyata. Bila sumber ekstraksi gagal, tampilkan batasan; user harus dapat membuka dokumen asli dan melaporkan kesalahan.

---

## 20. Technical architecture constraints (guardrails PRD)

**Baseline yang sudah dipilih/direncanakan:**

| Layer | Keputusan / status |
|---|---|
| Web | Next.js + TypeScript — baseline terpilih |
| Mobile | React Native + Expo — future client baseline |
| Backend API | **C# + ASP.NET Core** — dipilih |
| Architecture | API-first Modular Monolith + independent workers — dipilih |
| Database | **PostgreSQL** — dipilih |
| ORM | EF Core + Npgsql — rekomendasi |
| Search | PostgreSQL FTS + pgvector — rekomendasi |
| Rich text | Tiptap/Web structured document JSON — rekomendasi |
| Storage | S3-compatible interface, provider dapat dipindah — dipilih |
| Heavy processing | Background workers, Python bila perlu untuk extraction — rekomendasi |
| Container/hosting | Docker/Compose; laptop development → VPS launch → cloud scaling — dipilih |
| Auth provider/library | TEKNIS-TERBUKA |
| Queue/scheduler | TEKNIS-TERBUKA |
| Embedding/model routing | TEKNIS-TERBUKA setelah eval dan costing |
| Physical tables, index/RLS | TEKNIS-TERBUKA hingga Physical ERD |

**Catatan penting:** PRD menentukan *behavior/invariants*, bukan memaksa nama tabel persis. Proposed v1.2 `content_spaces` dan `item_placements` adalah desain yang layak diuji pada Physical ERD; bukan migrasi SQL yang sudah disetujui.

---

## 21. User journeys / layar minimum

| Layar | Komponen utama |
|---|---|
| Landing & Pricing | Positioning, contoh workflow, Free/Plus/Pro, transparansi limit |
| Sign up / Onboarding | Akun, preferensi dasar, buat course/project awal |
| Dashboard | Recent activity, continue learning, tasks, usage panel |
| Workspace Explorer | Library/Course/Project, folder, search, add existing item |
| Notes Editor | Rich-text blocks, nested pages, autosave, history, AI actions |
| Document Reader | Viewer, annotation, summary/chat, sitasi halaman dan status ekstraksi |
| Study Session | AI Tutor, pilihan sumber, mode penjelasan, riwayat |
| Quiz / Flashcards | Generator, exam/practice mode, review, progres |
| Research Discover/Library | Search, filter, paper detail, DOI, save, matrix, compare |
| Writing Studio | Outline, editor, sources/citation panel, audit, export |
| Presentation Studio | Outline, slide editing/reorder, generate/regenerate, export |
| Planner | Calendar, tasks, study plan, reminders, RPS review |
| Billing/Settings | Plan, quotas, storage, payments, deletion/export |
| Minimal Admin | Operational health, jobs, transactions, refunds/support |

### 21.1 Empty/error states yang tidak boleh terlewat

Workspace baru tanpa materi; PDF belum diindeks; DOI tidak valid; paper tanpa full text; AI kuota habis; storage penuh; payment pending; AI model unavailable; job PPT gagal; export overflow; versi notes konflik; referensi di Trash; pengguna downgrade melebihi kapasitas baru.

---

## 22. Analytics, north-star dan evaluasi bisnis

### 22.1 North-star metric (usulan)

**Weekly Active Academic Workspaces (WAAW):** jumlah mahasiswa yang melakukan aktivitas akademik bermakna dalam workspace selama seminggu. Dipasangkan dengan DAU/WAU, retensi dan biaya variabel per active user; bukan sekadar menghitung pesan AI.

Aktivitas bermakna dapat mencakup baca/ubah notes, review materi, menyelesaikan kuis, mengelola tugas, atau melakukan research/writing. Definisi event final diuji setelah instrumentasi.

### 22.2 KPI prioritas

| KPI | Tujuan pengukuran |
|---|---|
| Activation | Berapa user membuat ruang dan menyelesaikan first value (materi + tindakan) |
| DAU / WAU / MAU | Frekuensi penggunaan; tidak tertipu sekadar log in |
| D7 / D30 retention | Apakah pengguna kembali setelah menggunakan produk |
| Repeat non-AI use | Apakah Free tetap berguna saat kuota AI habis |
| Course/project adoption | Berapa workspace bermakna per user |
| Research-to-writing completion | Seberapa sering referensi yang disimpan dipakai menulis |
| AI grounded success | Jawaban dengan sumber yang benar, cukup, dan dapat dibuka |
| Quiz completion/review | Pengguna menyelesaikan dan mengulang latihan |
| Free-to-paid conversion | Rasio upgrade karena nilai, bukan jebakan data |
| Variable cost per user | AI, storage, worker, egress, pembayaran per kategori |
| Gross contribution per tier | Pendapatan setelah biaya variabel paket |
| Job failures & support | Kualitas operasi dan pengalaman pengguna |

**Tidak ada angka DAU/retention/conversion yang sudah disepakati sebagai target bisnis.** Tentukan target kuantitatif setelah beta dan proyeksi biaya valid.

---

## 23. Dependency, partner gates dan keputusan risiko

| Dependency / risiko | Dampak | Gate sebelum klaim fitur siap |
|---|---|---|
| AI model dan embedding | Cost, kualitas jawaban | Uji source grounding, token usage, fallback & budget |
| Paper metadata providers | Rate limits, akses | Review terms, caching, DOI verification |
| Full-text publisher | Hak cipta dan lisensi | Hanya memproses dokumen yang pengguna berhak akses/proses |
| Similarity provider | Hak penggunaan, biaya per cek | Kontrak/lisensi/redistribusi, billing dan privacy review |
| OCR/transcription/video | Kualitas, biaya, izin | Coverage test, consent, quota dan error disclosure |
| PPTX rendering | Editability, layout | Render-test dan validasi editable slides |
| Payment | Status pending/failed, webhook | Verifikasi server, idempotency, refund/cancel flows |
| Notes nested placement | Siklus/duplikasi | Hierarchy constraints dan move/restore tests |
| RAG retrieval | Kebocoran data | Data-space/source scoped tests dan prompt injection eval |
| Quota ledger | Debit ganda, over-budget | Concurrency/idempotency/reconciliation tests |
| Storage/versioning | Orphan file dan biaya | Garbage collection, retention, restore tests |
| Mobile readiness | Perubahan format/API | Versioned notes/API, cross-device conflict tests |

---

## 24. Tahapan internal pengembangan (bukan pemangkasan scope rilis)

| Urutan | Fokus internal | Output verifikasi |
|---|---|---|
| I-0 | Foundation & security | Accounts, auth, data ownership, Docker/dev infra, monitoring dasar |
| I-1 | Workspace & Knowledge | Library/Course/Project, folder, notes, placement, storage/restore |
| I-2 | Document & Retrieval | Upload, extraction, indexing, source-aware document Q&A |
| I-3 | Learning | Tutor, quiz, flashcards, learning profile, review dan planner basics |
| I-4 | Research & Writing | Journal sources, metadata DOI, library, literature matrix, citation, DOCX |
| I-5 | Presentation & Integrity | Slides/PPTX export, citation audit, similarity partner gate |
| I-6 | Planner Complete & Monetization | RPS, study plans, subscription, credits, usage, admin, analytics |
| I-7 | QA & Release Gate | E2E, load/cost/security tests, policy, launch readiness, pricing validation |

**Keterangan:** seluruh fase internal dapat dikerjakan dan diuji bertahap tanpa merilis setiap fase sebagai produk publik. Modul dependent external tetap mengikuti gate kontrak/izin.

---

## 25. Definition of Done dan acceptance rilis

### 25.1 DoD setiap fitur

- [ ] Masalah pengguna dan outcome terdefinisi; skenario normal serta error ditangani.
- [ ] Hanya mengakses data yang benar-benar dimiliki/diizinkan user.
- [ ] Status processing, retry dan loading terlihat; data tidak hilang saat refresh.
- [ ] Unit/integration tests untuk logic, akses dan kasus kegagalan penting.
- [ ] Ada event/metric untuk mengukur pemakaian, kualitas dan biaya.
- [ ] Output disimpan/di-export dalam format yang dijanjikan.
- [ ] AI feature: sumber dan ketidakpastian ditampilkan; biaya/kuota dicatat, tidak ada DOI/halaman fiktif.
- [ ] Accessibility dan responsivitas untuk aktivitas utama terpenuhi.
- [ ] Dokumentasi API, perubahan schema dan behavior diperbarui.

### 25.2 Gate peluncuran StudyAI Campus V1

**Produk dan kualitas**

- [ ] Kesembilan modul memiliki workflow inti berfungsi, bukan halaman placeholder.
- [ ] GP-01 s.d. GP-05 lolos E2E dengan beberapa akun uji dan materi nyata.
- [ ] Rujukan akademik, DOI dan citation audit tidak membuat sumber fiktif.
- [ ] PPTX export editable, DOCX/PDF valid, quiz bisa dikerjakan dan disimpan.
- [ ] Free tetap usable untuk aktivitas non-AI setelah limit tercapai.

**Keamanan, operasional dan biaya**

- [ ] Cross-user isolation tests pada Library, retrieval, object storage, AI history.
- [ ] Payment dan usage ledger diuji untuk retry/gagal/permintaan paralel.
- [ ] Backup PostgreSQL dan objek di luar server; restore drill sukses.
- [ ] Load tests dan model unit-cost menunjukkan anggaran launch dapat dikendalikan.
- [ ] Kebijakan privasi, hak cipta, retention/deletion, PSE/UU PDP dan syarat provider sudah ditinjau.
- [ ] Admin bisa menemukan jobs macet, pemakaian tinggi, payment mismatch dan error kritis.

**Partner-gated**

- [ ] Similarity hanya dapat ditawarkan jika lisensi/hak distribusi sah; jika belum, status pending di PRD dan materi pemasaran.
- [ ] Video/transkrip atau metadata connector eksternal hanya tersedia sesuai izin/syarat provider.

### 25.3 Acceptance scenarios wajib untuk QA

| ID | Scenario | Hasil yang diwajibkan |
|---|---|---|
| E2E-01 | User A mencoba memasukkan item milik User B | Ditolak pada API dan integritas DB |
| E2E-02 | Jurnal sama di tiga spaces | Satu konten, banyak placement, quota storage tidak triple |
| E2E-03 | Hapus dari Course | Tetap ada di Library/Project lainnya |
| E2E-04 | Dua klien simpan note revision lama | Konflik eksplisit, tidak overwrite |
| E2E-05 | Jawaban AI mengutip PDF v1, kemudian PDF v2 diunggah | Jawaban lama tetap merujuk v1 |
| E2E-06 | Paper tanpa full text | Metadata tersedia, analisis full text dibatasi |
| E2E-07 | AI source tidak mendukung DOI/quote | Sistem menolak membuat sitasi fiktif |
| E2E-08 | Pengguna Free habis kuota | Notes, planner, reader, quiz saved tetap berjalan |
| E2E-09 | Dua AI job menghabiskan sisa credit bersamaan | Tidak overdraw, tidak double debit |
| E2E-10 | Gagal generate PPT | Status gagal/retry jelas, charge konsisten |
| E2E-11 | Storage penuh / downgrade plan | Existing files tetap readable, upload baru dibatasi |
| E2E-12 | Pindah VPS ke cloud pada salinan staging | API, database, file dan sumber AI berfungsi dengan data utuh |

---

## 26. Open decisions yang sengaja tidak dipalsukan menjadi final

PRD produk dapat menjadi **final baseline** sekalipun implementasi di bawah ini belum diputuskan. Sebelum tiket terkait dikerjakan, keputusan harus dicatat dalam ADR/technical spec.

| ID | Keputusan yang masih terbuka | Pemilik tahap |
|---|---|---|
| OD-01 | Auth stack: ASP.NET Core Identity vs solusi lain, Google login/MFA, mobile token pattern | Architecture/Auth Design |
| OD-02 | Queue background: Hangfire/alternatif, idempotent retry dan dispatch | Backend Infrastructure |
| OD-03 | Physical ERD untuk `content_spaces`, nested pages, `item_placements`, restore semantics | Physical Database Design |
| OD-04 | RLS policy, composite FK target dan isolasi worker | Security/Data Design |
| OD-05 | Embedding model/dimensi, Indonesian retrieval eval, keyword/tokenization dan ANN benchmark | AI & Search Design |
| OD-06 | Citation provenance model notes/reference/document, snapshot retention dan purge | AI/Data Design |
| OD-07 | File lifecycle, versi lama yang ditagih, quota calculation dan retensi Trash | Storage & Policy |
| OD-08 | Provider AI, harga per workflow dan budget safety caps | Unit Economics |
| OD-09 | Final harga/limit paket serta metode payment Web/mobile | Business Decision |
| OD-10 | Similarity partner, lisensi/hak distribusi, biaya dan kapan dapat ditawarkan | Commercial/Legal |
| OD-11 | Prioritas OCR, audio/video connectors dan kualitas layanan partner | Product + Legal/Tech |
| OD-12 | SLO performa/retensi/kapasitas hardware berdasarkan pengujian nyata | SRE/Launch Planning |
| OD-13 | UX detail rich-text/nested notes dan perluasan format import/export | Product Design |
| OD-14 | In-App Purchase policy dan tanggal rilis Android/iOS | Mobile Launch Planning |

**Aturan perubahan:** perubahan scope V1 mengubah nomor versi PRD, dicatat dalam *change log*, diikuti pembaruan roadmap, acceptance dan kontrak sistem. Detail implementasi yang tidak mengubah perilaku produk cukup ditangani lewat ADR/technical design.

---

## 27. Matriks traceability PRD → arsitektur/QA

| Domain produk | Data/komponen konseptual | Uji utamanya |
|---|---|---|
| Workspace/Library | Account, Data Space, Content Spaces, Knowledge Items, Placements, Notes, Versions | E2E-01/02/03/04 |
| Documents & Search | Document Versions, Chunks, Retrieval Scope, Object Storage | E2E-01/05/12 |
| AI Tutor & Quiz | Conversations, Sources, Citations, Attempts, Learning Profile | GP-01, E2E-05/07 |
| Research & Writing | Reference Metadata, DOI, Source Linking, Draft Versions, Citations | GP-02, E2E-06/07 |
| Presentation | Slide Structure, Render/Export Jobs | GP-03, E2E-10 |
| Integrity | Citation Audit, Similarity Provider Adapter | GP-02/03, partner gate |
| Planner | Tasks, Events, Deadlines, Reminders | GP-05 |
| Billing/Free | Plans, Entitlements, Subscription Periods, Usage Ledger, Storage Metering | E2E-08/09/11 |
| Portability | Docker, PostgreSQL, S3 Adapter, Backup/Restore | E2E-12 |

---

## 28. Product decisions register

| ID | Keputusan | Status |
|---|---|---|
| PD-001 | Produk awal adalah **StudyAI Campus untuk mahasiswa** | DIKUNCI |
| PD-002 | **MVP kaya fitur**: sembilan modul lengkap secara workflow, tidak dipangkas menjadi satu tool | DIKUNCI |
| PD-003 | Collaboration, Career, School, Parent, B2B di luar MVP | DIKUNCI |
| PD-004 | Workspace/Knowledge sebagai pusat; AI menyatu ke konteks | DIKUNCI |
| PD-005 | Satu konten boleh digunakan di banyak ruang tanpa menggandakan file | DIKUNCI |
| PD-006 | Rilis Web pertama, architecture-ready untuk Android/iOS | DIKUNCI |
| PD-007 | Free benar-benar usable harian, AI berkuota, non-AI tetap accessible | DIKUNCI |
| PD-008 | Harga Free/Plus/Pro dan limit angka masih hipotesis | BELUM DIKUNCI |
| PD-009 | Similarity membutuhkan kontrak/lisensi resmi; tidak boleh mengaku Turnitin jika bukan | DIKUNCI / BERSYARAT |
| PD-010 | ASP.NET Core, PostgreSQL, API-first modular monolith, Docker portability | DIKUNCI secara baseline |
| PD-011 | Rancangan unified `content_spaces` + placements & provenance versi adalah arah arsitektur | USULAN v1.2, perlu Physical ERD |
| PD-012 | Fitur P3 yang secara eksplisit dinyatakan PASCA-V1 tidak menjadi release blocker | Scope PRD ini; revisi jika owner memutuskan lain |

---

## 29. Glossary

| Istilah | Arti |
|---|---|
| **MVP / Campus V1** | Peluncuran B2C kaya fitur dengan sembilan modul |
| **Data Space** | Batas ownership/permission pengguna |
| **Content Space** | Konteks organisasi konten, contoh Library/Course/Project |
| **Knowledge Item** | Identitas umum note, file, reference, atau output AI |
| **Placement** | Relasi penempatan item ke beberapa spaces tanpa copy binary |
| **Provenance** | Asal, versi dan lokasi sumber yang digunakan output AI |
| **RAG** | Mengambil sumber relevan untuk membantu generasi jawaban |
| **Grounded output** | Output AI yang menyatakan/merujuk bukti yang benar-benar tersedia |
| **Entitlement** | Hak akses fitur/limit sesuai paket |
| **Usage ledger** | Jejak transaksi kuota/biaya, harus idempotent |
| **Release gate** | Syarat wajib sebelum fitur/produk diklaim siap |

---

## 30. Hubungan dokumen dan revision log

### Dasar dokumen

- **Sumber visi fitur:** `StudyAI_Master_Feature_List(1).md` (11 Oktober 2026).
- **Keputusan produk/stack:** `StudyAI_Campus_MVP_Decisions_v1.md` (12 Oktober 2026).
- **Fondasi data:** `StudyAI_Core_Database_Architecture_v1.md` (12 Oktober 2026).
- **Revisi arsitektur yang ditinjau:** `Teks yang ditempel (1)(1).txt` / *StudyAI Core Database & Knowledge Architecture Revisi v1.1*.
- **Diskusi arsitektur v1.2:** Unified Content Spaces, nested pages, scoped retrieval, citation provenance dan usage ledger, status **proposed baseline** belum physical migration.
- **Diskusi Free Plan/budget/hosting:** strategi freemium generus untuk aktivitas non-AI; cloud object storage, development laptop dan produksi VPS → cloud.

### Revision history

| Version | Date | Keterangan |
|---|---|---|
| 1.0 | 12 Oct 2026 | PRD konsolidasi pertama dengan sembilan modul Campus, golden paths, acceptance criteria, Free/Plus/Pro hipotesis, dan status keputusan teknis/partner yang eksplisit. |

### Checklist untuk pemilik produk setelah PRD dibuat

- [ ] Review keseluruhan V1-WAJIB dan beri persetujuan terhadap kedalaman fitur lanjutan (mis. OCR, media, tracking nilai, related papers).
- [ ] Tentukan perlakuan similarity partner gate terhadap tanggal peluncuran.
- [ ] Validasi strategi/angka freemium dan harga memakai cost test.
- [ ] Finalisasi Architecture v1.2 dan lanjut ke Physical ERD berdasarkan *product behaviors* dokumen ini.

**Pernyataan penutup:** PRD ini adalah *single source of truth* untuk **apa yang harus dapat dilakukan StudyAI Campus V1**. Physical ERD dan technical design harus mengikuti perilaku dan guardrails-nya, sambil mencatat pilihan detail yang belum diputuskan secara terbuka.
