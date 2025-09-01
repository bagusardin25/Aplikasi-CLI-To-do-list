using System;
using MySqlConnector;
using System.Security.Cryptography; // Komentar: untuk hash password
using System.Text;
using System.Data.Common;
using System.Net;
using System.ComponentModel.Design;
using System.Data;
using Microsoft.VisualBasic; // Komentar: untuk encoding string ke byte

class Program
{
    // Variabel global untuk menyimpan user_id user yang sedang login
    static int user_id = -1;
    static string connectionString = "Server=Localhost;Database=todolist;User ID=root;Password=;";
    static int percobaanlogin = 0;

    // Fungsi untuk melakukan hash password dengan SHA256
    // Komentar: Fungsi ini digunakan untuk mengamankan password sebelum disimpan ke database
    static string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
    // Fungsi untuk membaca password dari console dan menampilkan * setiap karakter
    static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            // Kalau bukan Backspace dan bukan Enter
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;    // Tambah ke variabel password
                Console.Write("*");         // Cetak * di layar
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                // Hapus karakter terakhir di password
                password = password.Substring(0, password.Length - 1);

                // Hapus * terakhir di console
                Console.Write("\b \b");
            }
        }
        while (key.Key != ConsoleKey.Enter);

        Console.WriteLine(); // Pindah baris setelah tekan Enter
        return password;
    }


    private static void Main(string[] args)
    {
        bool isLogin = true;

        while (isLogin)
        {
            Console.Clear();
            Console.WriteLine("----- SELAMAT DATANG DI APLIKASI TO DO LIST -----");
            Console.WriteLine("1. Registrasi");
            Console.WriteLine("2. Login");
            Console.WriteLine("0. Keluar");
            Console.Write("Pilih menu (1-2): ");
            string? pilihan1 = Console.ReadLine();
            switch (pilihan1)
            {
                case "1":
                    registrasi();
                    break;
                case "2":
                    login();
                    break;
                case "0":
                    isLogin = false;
                    break;
                default:
                    Console.WriteLine("Pilihan tidak valid. Silahkan pilih menu yang tersedia.");
                    Console.ReadLine();
                    break;

            }

        }
        static void login()
        {
            while (true)
            {
                Console.Clear();
                using MySqlConnection conn = new MySqlConnection(connectionString);
                conn.Open();

                Console.WriteLine("----- HALAMAN LOGIN -----");
                Console.Write("Masukkan username: ");
                string? username = Console.ReadLine();
                Console.Write("Masukkan password: ");
                string? password = ReadPassword();

                // Komentar: Hash password yang diinput user sebelum dibandingkan dengan database
                string hashedPassword = HashPassword(password ?? "");

                string query = "SELECT * FROM users WHERE username = @username AND password = @password";

                using MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", hashedPassword); // Komentar: password yang dikirim ke query sudah di-hash

                using MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    // Ambil user_id dari hasil query dan simpan ke variabel global
                    if (reader.Read())
                    {
                        // Asumsi kolom user_id adalah kolom pertama (index 0) atau bisa pakai nama kolom
                        user_id = reader.GetInt32(reader.GetOrdinal("user_id")); // Simpan user_id
                    }
                    Console.WriteLine("Login berhasil!");
                    Console.WriteLine("Selamat datang, " + username + "!");
                    Console.WriteLine("Tekan enter untuk melanjutkan ke menu utama...");
                    Console.ReadLine();
                    menuutama();
                    break;
                }
                else
                {
                    percobaanlogin++;
                    Console.WriteLine("Login gagal. Username atau password salah.");

                    if (percobaanlogin >= 3)
                    {
                        Console.WriteLine("Belum punya akun? silahkan daftar akun baru terlebih dahulu");
                        Console.WriteLine("Tekan enter untuk melanjutkan ke daftar akun baru...");
                        Console.ReadLine();
                        registrasi();
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Tekan enter untuk mencoba lagi...");
                        Console.ReadLine();
                    }
                }
            }
        }

        static void registrasi()
        {
            Console.Clear();
            using MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            Console.WriteLine("----- HALAMAN REGISTRASI -----");
            Console.Write("Masukkan username: ");
            string? usernamebaru = Console.ReadLine();

            // 🔹 Cek apakah username sudah ada
            string cekQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
            using (MySqlCommand cekCmd = new MySqlCommand(cekQuery, connection))
            {
                cekCmd.Parameters.AddWithValue("@username", usernamebaru);
                // Komentar: Cek hasil ExecuteScalar, jika null maka jumlah = 0
                object? result = cekCmd.ExecuteScalar();
                long jumlah = (result != null) ? Convert.ToInt64(result) : 0;

                if (jumlah > 0)
                {
                    Console.WriteLine("Username sudah digunakan, silakan coba lagi.");
                    Console.ReadKey();
                    return; // keluar dari fungsi registrasi
                }
            }

            Console.Write("Masukkan email: ");
            string? email = Console.ReadLine();

            Console.Write("Masukkan password: ");
            string? password = ReadPassword();
            // string? password = Console.ReadLine(); // Komentar: Menggunakan ReadPassword untuk keamanan

            string? konfirmasipassword;

            while (true)
            {
                Console.Write("Konfirmasi password: ");
                konfirmasipassword = ReadPassword();
                // string? konfirmasipassword = Console.ReadLine(); // Komentar: Meng

                if (konfirmasipassword != password)
                {
                    Console.WriteLine("Password tidak sama, silahkan ulangin lagi");
                    Console.ReadKey();

                }
                else
                {
                    // Komentar: Hash password sebelum disimpan ke database
                    string hashedPassword = HashPassword(password ?? "");

                    string query = "INSERT INTO users (username, email, password) VALUES (@username, @email, @password)";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", usernamebaru);
                        cmd.Parameters.AddWithValue("@password", hashedPassword); // Komentar: password yang disimpan sudah di-hash
                        cmd.Parameters.AddWithValue("@email", email);

                        cmd.ExecuteNonQuery();
                    }
                    Console.WriteLine("Registrasi berhasil!");
                    Console.WriteLine("Tekan enter untuk melanjutkan");
                    Console.ReadLine();
                    login();
                    break;
                }

            }
        }

        static void menuutama()
        {
            bool isRunning = true;
            while (isRunning)
            {
                Console.Clear();
                Console.WriteLine("----- SELAMAT DATANG DI APLIKASI TO DO LIST -----");
                Console.WriteLine("1. Tambah Tugas");
                Console.WriteLine("2. Edit Tugas");
                Console.WriteLine("3. Hapus Tugas");
                Console.WriteLine("4. Tampilkan Semua Tugas");
                Console.WriteLine("5. Tampilkan Tugas Belum Selesai");
                Console.WriteLine("6. Tampilkan Tugas Selesai");
                Console.WriteLine("7. Tandai Tugas Selesai");
                Console.WriteLine("8. Statistik Tugas");
                Console.WriteLine("0. Keluar");
                Console.Write("Pilih menu (1-10): ");


                string? pilihan = Console.ReadLine();

                switch (pilihan)
                {
                    case "1":
                        tambahtugas();
                        break;
                    case "2":
                        edittugas();
                        break;
                    case "3":
                        hapustugas();
                        break;
                    case "4":
                        tampilkansemuatugas();
                        break;
                    case "5":
                        tampilkantugasbelumselesai();
                        break;
                    case "6":
                        tampilkantugasselesai();
                        break;
                    case "7":
                        tandaitugasselesai();
                        break;
                    case "8":
                        Statistik();
                        break;
                    case "0":
                        Console.WriteLine("Terima kasih telah menggunakan aplikasi To Do List. Sampai jumpa!");
                        isRunning = false;
                        break;
                    default:
                        Console.WriteLine("Pilihan tidak valid. Silakan pilih menu yang tersedia.");
                        Console.ReadLine();
                        break;
                }
            }
        }
    }

    static void tambahtugas()
    {
        Console.Clear();
        Console.WriteLine("----- TAMBAH TUGAS -----");
        string? title;
        while (true)
        {
            Console.Write("Judul tugas: ");
            title = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("Judul tugas wajib diisi");
                Console.WriteLine("Tekan enter untuk kembali...");
                Console.ReadKey();
            }
            else
            {
                break;
            }
        }


            Console.Write("Deskripsi tugas (boleh kosong): ");
            string? description = Console.ReadLine();

            Console.Write("Masukkan tanggal dan jam deadline (dd/mm/yyyy HH:mm) *boleh kosong: ");
            string? deadlineinput = Console.ReadLine();

            DateTime? dueDate = null;
            if (!string.IsNullOrWhiteSpace(deadlineinput))
            {
                // Coba parsing dengan format "dd/MM/yyyy HH:mm"
                if (DateTime.TryParseExact(deadlineinput, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDateTime))
                {
                    dueDate = parsedDateTime;
                }
                else
                {
                    Console.WriteLine("Format tanggal tidak valid. Silakan masukkan dalam format dd/MM/yyyy HH:mm.");
                    Console.ReadLine();
                    return; // Keluar dari fungsi jika format salah
                }
            

            using MySqlConnection conn = new MySqlConnection(connectionString);

            conn.Open();
            string query = @"INSERT INTO tasks (user_id, title, description, status, due_date, created_at, updated_at)
                        VALUES (@user_id, @title, @description, 'pending', @due_date, NOW(), NOW());";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                // Gunakan user_id dari variabel global
                cmd.Parameters.AddWithValue("@user_id", user_id); // Komentar: user_id diambil dari user yang login
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@description", description);
                cmd.Parameters.AddWithValue("@due_date", (object?)dueDate ?? DBNull.Value);

                cmd.ExecuteNonQuery(); // Komentar: eksekusi query untuk menambah tugas
            }
            Console.WriteLine("Tugas berhasil ditambahkan!");
            Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
            Console.ReadLine();
            return;
        }
    }

    static void edittugas()
        {
            Console.Clear();
            Console.WriteLine("----- HALAMAN EDIT TUGAS -----");

            //tampilkan semua tugas terlebih dahulu
            TampilkanDaftarTugasDenganStatus();
            Console.Write("\nMasukkan ID tugas yang ingin di edit (atau 0 untuk batal): ");
            if (!int.TryParse(Console.ReadLine(), out int taskId))
            {
                Console.WriteLine("Input tidak valid!");
                Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
                Console.ReadLine();
                return;
            }
            if (taskId == 0)
            {
                Console.WriteLine("Pengeditan tugas dibatalkan.");
                Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
                Console.ReadLine();
                return;
            }

            try
            {
                using MySqlConnection conn = new MySqlConnection(connectionString);
                conn.Open();

                //mengecek apakah tugas ada dan milik user yang login
                string checkQuery = "SELECT id, title, description, status, due_date FROM tasks WHERE id = @id AND user_id = @user_id";
                using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@id", taskId);
                    checkCmd.Parameters.AddWithValue("@user_id", user_id);

                    using (MySqlDataReader baca = checkCmd.ExecuteReader())
                    {
                        if (!baca.HasRows)
                        {
                            Console.WriteLine("Tugas tidak ditemukan atau tidak memiliki akses untuk mengedit!");
                            Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
                            Console.ReadLine();
                            return;
                        }
                        baca.Read();
                        string currentTitle = baca.GetString("title");
                        string currentDescription = baca.IsDBNull(baca.GetOrdinal("description")) ? "" : baca.GetString("description");
                        string currentStatus = baca.GetString("status");
                        DateTime? currentDueDate = baca.IsDBNull(baca.GetOrdinal("due_date")) ? null : baca.GetDateTime("due_date");
                        baca.Close();

                        //tampilkan data tuga saat ini
                        Console.WriteLine("\nData tugas saat ini: ");
                        Console.WriteLine($"Judul: {currentTitle} ");
                        Console.WriteLine($"Deskripsi: {currentDescription}");
                        if (currentDueDate.HasValue)
                        {
                            string countdownInfo;
                            if (currentStatus == "completed")
                            {
                                countdownInfo = "✅ selesai";
                            }
                            else
                            {
                                countdownInfo = hitungmundur(currentDueDate.Value);
                            }

                            Console.WriteLine($"Deadline: {currentDueDate.Value.ToString("dd/MM/yyyy HH:mm")} ({countdownInfo})");
                        }
                        else
                        {
                            if (currentStatus == "completed")
                            {
                                Console.WriteLine($"Deadline: - (✅ Selesai)");
                            }
                            else
                            {
                                Console.WriteLine($"Deadline: - (Tidak ada deadline)");
                            }
                        }
                        //input data baru
                        Console.WriteLine("\nMasukkan data baru(tekan enter untuk tidak mengubah)");
                        Console.Write("Judul baru: ");
                        string? newTitle = Console.ReadLine();
                        Console.Write("Deskripsi tugas: ");
                        string? newDescription = Console.ReadLine();
                        Console.Write("Deadline: (dd/MM/yyyy HH:mm): ");
                        string? newDeadLineInput = Console.ReadLine();

                        DateTime? newDueDate = null;
                        if (!string.IsNullOrWhiteSpace(newDeadLineInput))
                        {
                            // Coba parsing dengan format "dd/MM/yyyy HH:mm"
                            if (DateTime.TryParseExact(newDeadLineInput, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDateTime))
                            {
                                newDueDate = parsedDateTime;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Format tanggal tidak valid. Deadline tidak diubah.");
                            newDueDate = currentDueDate; // tetap gunakan deadline lama
                        }
                        string upDateQuery = @"UPDATE tasks SET title = COALESCE (NULLIF(@title, ''), title), description = COALESCE (NULLIF(@description, ''), description), due_date = COALESCE(@due_date, due_date), updated_at = NOW() WHERE id = @id AND user_id = @user_id";
                        using (MySqlCommand upDateCmd = new MySqlCommand(upDateQuery, conn))
                        {
                            upDateCmd.Parameters.AddWithValue("@id", taskId);
                            upDateCmd.Parameters.AddWithValue("@user_id", user_id);
                            upDateCmd.Parameters.AddWithValue("@title", string.IsNullOrWhiteSpace(newTitle) ? DBNull.Value : newTitle);
                            upDateCmd.Parameters.AddWithValue("@description", string.IsNullOrWhiteSpace(newDescription) ? DBNull.Value : newDescription);
                            upDateCmd.Parameters.AddWithValue("@due_date", (object?)newDueDate ?? DBNull.Value);
                            int rowsAffected = upDateCmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                Console.WriteLine("Tugas berhasil diupdate.");
                                Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
                                Console.ReadLine();
                                return;
                            }
                            else
                            {
                                Console.WriteLine("Tidak ada perubahan yang dilakukan.");
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"terjadi Error: {ex.Message}");

            }
            Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
            Console.ReadLine();
        }
    static void hapustugas()
    {
        Console.Clear();
        Console.WriteLine("----- HAPUS TUGAS -----");

        //tampilkan semua tugas terlebih dahulu
        TampilkanDaftarTugasDenganStatus();

        Console.Write("\nMasukkan ID tugas yang ingin dihapus (atau 0 untuk batal): ");

        if (!int.TryParse(Console.ReadLine(), out int taskId))
        {
            Console.WriteLine("Input tidak valid!");
            Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
            Console.ReadLine();
            return;
        }
        if (taskId == 0)
        {
            Console.WriteLine("Penghapusan tugas dibatalkan.");
            Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
            Console.ReadLine();
            return;
        }

        try
        {
            using MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();

            //mengecek apakah tugas ada dan milik user yang login
            string checkQuery = "SELECT id, title FROM tasks WHERE id = @id AND user_id = @user_id";
            using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
            {
                checkCmd.Parameters.AddWithValue("@id", taskId);
                checkCmd.Parameters.AddWithValue("@user_id", user_id);

                using (MySqlDataReader baca = checkCmd.ExecuteReader())
                {
                    if (!baca.HasRows)
                    {
                        Console.WriteLine("Tugas tidak ditemukan atau tidak memiliki akses untuk menghapus!");
                        Console.WriteLine("Tekan ennter untuk kembali ke menu utama...");
                        Console.ReadLine();
                        return;
                    }
                    baca.Read();
                    string taskTitle = baca.GetString("title");
                    baca.Close();

                    //konfirmasi penghapusan
                    Console.Write($"Apakah anda yakin ingin menghapus tugas '{taskTitle}'? (y/n): ");
                    string? konfirmasi = Console.ReadLine()?.ToLower();
                    if (konfirmasi != "y" && konfirmasi != "ya")
                    {
                        Console.WriteLine("Penghapusan tugas dibatalkan.");
                        Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
                        Console.ReadLine();
                        return;
                    }
                    //hapus tugas
                    string hapusQuery = "DELETE FROM tasks WHERE id = @id AND user_id = @user_id";
                    using (MySqlCommand hapusCmd = new MySqlCommand(hapusQuery, conn))
                    {
                        hapusCmd.Parameters.AddWithValue("@id", taskId);
                        hapusCmd.Parameters.AddWithValue("@user_id", user_id);

                        int rowsAffected = hapusCmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("Tugas berhasil dihapus.");
                        }
                        else
                        {
                            Console.WriteLine("Gagal menghapus tugas. silahkan coba lagi.");
                        }
                    }
                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"terjadi Error: {ex.Message}");

        }
        Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
        Console.ReadLine();
    }
    static void tampilkansemuatugas()
    {
        Console.Clear();
        Console.WriteLine("------ DAFTAR SEMUA TUGAS ------");

        using MySqlConnection conn = new MySqlConnection(connectionString);
        conn.Open();

        string query = @"SELECT 
                        id, 
                        title, 
                        COALESCE(description, '-') AS description, 
                        status, 
                        due_date, 
                        created_at
                    FROM tasks
                    WHERE user_id = @user_id
                    ORDER BY due_date ASC;";

        using (MySqlCommand cmd = new MySqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@user_id", user_id);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)
                {
                    Console.WriteLine("\n⚠️  Belum ada tugas yang ditambahkan\n");
                }
                else
                {
                    // Header tabel
                    Console.WriteLine("\n┌─────┬──────────────────────┬──────────────────────┬─────────────┬────────────────────────────────┬─────────────────────┐");
                    Console.WriteLine("│ ID  │ Judul                │ Deskripsi            │ Status      │ Deadline                       │ Dibuat Pada         │");
                    Console.WriteLine("├─────┼──────────────────────┼──────────────────────┼─────────────┼────────────────────────────────┼─────────────────────┤");

while (reader.Read())
{
    // Ambil data dari reader
    int id = reader.GetInt32("id");
    string title = reader.GetString("title");
    string description = reader.IsDBNull(reader.GetOrdinal("description")) ? "-" : reader.GetString("description");
    string status = reader.GetString("status");
    DateTime? dueDate = reader.IsDBNull(reader.GetOrdinal("due_date")) ? null : reader.GetDateTime("due_date");
    DateTime createdAt = reader.GetDateTime("created_at");

    // Format title (maks 20 char)
    string formattedTitle = title.Length > 20 ? title.Substring(0, 20) : title;
    formattedTitle = formattedTitle.PadRight(20);

    // Format description (maks 20 char)
    string formattedDescription = description.Length > 20 ? description.Substring(0, 20) : description;
    formattedDescription = formattedDescription.PadRight(20);

    // Format status
    string formattedStatus = status.PadRight(11);
    if (status == "completed") 
    {
        formattedStatus = "Selesai".PadRight(11);
    }
    else 
    {
        formattedStatus = "Pending".PadRight(11);
    }

    // Format deadline dengan countdown
// Gunakan format yang sama untuk semua
string deadlineText;
if (dueDate.HasValue)
{
    string datePart = dueDate.Value.ToString("dd/MM/yy HH:mm");
    
    if (status == "completed")
    {
        deadlineText = $"{datePart} (✅)";
    }
    else
    {
        string countdownInfo = hitungmundur(dueDate.Value);
        // Buat countdown lebih compact
        countdownInfo = countdownInfo.Replace("hari", "d")
                                  .Replace("jam", "j")
                                  .Replace("menit", "m")
                                  .Replace("detik", "d")
                                  .Replace("Terlewat", "")
                                  .Replace("lagi", "");
        deadlineText = $"{datePart} ({countdownInfo})";
    }
}
else
{
    deadlineText = status == "completed" ? "- (✅)" : "-";
}

// Pastikan panjang konsisten
deadlineText = deadlineText.PadRight(25);
    // Cetak baris tabel
    Console.WriteLine(
        $"│ {id.ToString().PadLeft(3)} " +
        $"│ {formattedTitle} " +
        $"│ {formattedDescription} " +
        $"│ {formattedStatus} " +
        $"│ {deadlineText} " +
        $"│ {createdAt:dd/MM/yyyy HH:mm} │");
}

                    Console.WriteLine("└─────┴──────────────────────┴──────────────────────┴─────────────┴───────────────────────────────┴─────────────────────┘");
                }
                Console.WriteLine("\nTekan enter untuk kembali ke menu utama...");
                Console.ReadLine();
            }
        }
    }
    // Fungsi untuk menampilkan daftar tugas dengan status
    static void TampilkanDaftarTugasDenganStatus()
    {
        try
        {
            using MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();
            

            string query = @"SELECT id, title, status, due_date
                        FROM tasks WHERE user_id = @user_id 
                        ORDER BY status, id DESC";
                        
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                
                cmd.Parameters.AddWithValue("@user_id", user_id);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("\nDaftar Tugas Anda:");
                    Console.WriteLine("┌─────┬────────────────────────────┬──────────┬──────────────────────────┐");
                    Console.WriteLine("│ ID  │ Judul Tugas                │ Status   │ Deadline                 │");
                    Console.WriteLine("├─────┼────────────────────────────┼──────────┼──────────────────────────┤");

                    while (reader.Read())
                    {
                        int id = reader.GetInt32("id");
                        string title = reader.GetString("title");
                        string status = reader.GetString("status");
                        DateTime? dueDate = reader.IsDBNull(reader.GetOrdinal("due_date")) ? null : reader.GetDateTime("due_date");

                        string truncatedTitle = title.Length > 25 ? title.Substring(0, 22) + "..." : title;
                        string statusDisplay = status == "completed" ? "Selesai" : "Pending";
                        string infohitungmundur;
                        if (status == "completed")
                        {
                            infohitungmundur = "✅ Selesai";
                        }
                        else if (!dueDate.HasValue)
                        {
                            infohitungmundur = "tidak ada deadline";
                        }
                        else
                        {
                            infohitungmundur = hitungmundur(dueDate.Value);
                        }
                        Console.WriteLine($"│ {id,-3} │ {truncatedTitle,-26} │ {statusDisplay,-8} │{infohitungmundur,-25}|");
                    }
                    Console.WriteLine("└─────┴────────────────────────────┴──────────┴──────────────────────────┘");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error menampilkan daftar tugas: {ex.Message}");
        }
    }
static void tampilkantugasbelumselesai()
{
    Console.Clear();
    Console.WriteLine("----- DAFTAR TUGAS BELUM SELESAI -----");

    try
    {
        using MySqlConnection conn = new MySqlConnection(connectionString);
        conn.Open();

        string query = @"SELECT id, title, description, due_date, created_at 
                        FROM tasks 
                        WHERE user_id = @user_id AND status = 'pending' 
                        ORDER BY due_date ASC, created_at DESC";
        
        using (MySqlCommand cmd = new MySqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@user_id", user_id);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)
                {
                    Console.WriteLine("\n🎉 Tidak ada tugas yang belum selesai!");
                    Console.WriteLine("Semua tugas sudah completed. Good job! 👍");
                    Console.WriteLine("\nTekan enter untuk kembali ke menu utama...");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("\n┌─────┬────────────────────────────┬──────────────────────┬─────────────────────┬─────────────────────┐");
                Console.WriteLine("│ ID  │ Judul Tugas                │ Deskripsi            │ Deadline           │ Dibuat Pada         │");
                Console.WriteLine("├─────┼────────────────────────────┼──────────────────────┼─────────────────────┼─────────────────────┤");
                
                int jumlahtugas = 0;
                
                while (reader.Read())
                {
                    jumlahtugas++;
                    int id = reader.GetInt32("id");
                    string title = reader.GetString("title");
                    string description = reader.IsDBNull(reader.GetOrdinal("description")) ? "-" : reader.GetString("description");
                    DateTime? dueDate = reader.IsDBNull(reader.GetOrdinal("due_date")) ? null : reader.GetDateTime("due_date");
                    DateTime? createdAt = reader.GetDateTime("created_at");

                    // Format title (maks 25 karakter)
                    string formatTitle = title.Length > 25 ? title.Substring(0, 22) + "..." : title;
                    formatTitle = formatTitle.PadRight(25);

                    // Format description (maks 20 karakter)
                    string formatDeskripsi = description.Length > 20 ? description.Substring(0, 17) + "..." : description;
                    formatDeskripsi = formatDeskripsi.PadRight(20);

                    // Format deadline
                    string deadlineText;
                    if (dueDate.HasValue)
                    {
                        deadlineText = dueDate.Value.ToString("dd/MM/yyyy HH:mm");
                    }
                    else
                    {
                        deadlineText = "-";
                    }
                    deadlineText = deadlineText.PadRight(19);

                    // Cetak baris tabel
                    Console.WriteLine(
                        $"│ {id.ToString().PadLeft(3)} " +
                        $"│ {formatTitle} " +
                        $"│ {formatDeskripsi} " +
                        $"│ {deadlineText} " +
                        $"│ {createdAt:dd/MM/yyyy HH:mm} │");
                }

                Console.WriteLine("└─────┴────────────────────────────┴──────────────────────┴─────────────────────┴─────────────────────┘");
                
                // Tampilkan jumlah tugas
                Console.WriteLine($"\n📊 Total tugas belum selesai: {jumlahtugas} tugas");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
        Console.WriteLine("Silakan coba lagi atau hubungi administrator.");
    }

    Console.WriteLine("\nTekan enter untuk kembali ke menu utama...");
    Console.ReadLine();
}
    static void tampilkantugasselesai()
    {
        Console.Clear();
        Console.WriteLine("----- HALAMAN TUGAS SELESAI");

        try
        {
            using MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"SELECT id, title, description, due_date, created_at 
                        FROM tasks 
                        WHERE user_id = @user_id AND status = 'completed' 
                        ORDER BY due_date ASC, created_at DESC";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@user_id", user_id);

                using (MySqlDataReader baca = cmd.ExecuteReader())
                {
                    if (!baca.HasRows)
                    {
                        Console.WriteLine("\nBelum ada tugas yang selesai..");
                        Console.WriteLine("Tekan enter untuk kembali...");
                        Console.ReadLine();
                        return;
                    }
                    Console.WriteLine("\n┌─────┬────────────────────────────┬──────────────────────┬─────────────────────┬─────────────────────┐");
                    Console.WriteLine("│ ID  │ Judul Tugas                │ Deskripsi            │ Deadline           │ Dibuat Pada         │");
                    Console.WriteLine("├─────┼────────────────────────────┼──────────────────────┼─────────────────────┼─────────────────────┤");

                    int jumlahtugas = 0;
                    while (baca.Read())
                    {
                        jumlahtugas++;
                        int id = baca.GetInt32("id");
                        string title = baca.GetString("title");
                        string description = baca.IsDBNull(baca.GetOrdinal("description")) ? "-" : baca.GetString("description");
                        DateTime? dueDate = baca.IsDBNull(baca.GetOrdinal("due_Date")) ? null : baca.GetDateTime("due_Date");
                        DateTime? createdat = baca.GetDateTime("created_at");

                        //format title
                        string formatTitle = title.Length > 25 ? title.Substring(0, 22) + "..." : title;
                        formatTitle = formatTitle.PadRight(25);
                        //format deskripsi
                        string formatDescription = description.Length > 20 ? description.Substring(0, 17) + "..." : description;
                        formatDescription = formatDescription.PadRight(20);
                        //format deadline
                        string deadlineText;
                        if (dueDate.HasValue)
                        {
                            deadlineText = dueDate.Value.ToString("dd/MM/yyyy HH:mm");
                        }
                        else
                        {
                            deadlineText = "-";
                        }
                        deadlineText = deadlineText.PadRight(19);
                        //cetak tebal
                        Console.WriteLine(
                        $"│ {id.ToString().PadLeft(3)} " +
                        $"│ {formatTitle} " +
                        $"│ {formatDescription} " +
                        $"│ {deadlineText} " +
                        $"│ {deadlineText:dd/MM/yyyy HH:mm} │");
                    }

                    Console.WriteLine("└─────┴────────────────────────────┴──────────────────────┴─────────────────────┴─────────────────────┘");

                    // Tampilkan jumlah tugas
                    Console.WriteLine($"\n📊 Total tugas selesai: {jumlahtugas} tugas");
                }
            
                       
                    
                    
                
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Silahkan coba lagi nanti");
        }
        Console.WriteLine("\nTekan enter untuk kembali ke menu utama...");
        Console.ReadLine();
    }

    static void tandaitugasselesai()
    {
        Console.Clear();
        Console.WriteLine("----- TANDAI TUGAS SELESAI -----");

        // Tampilkan tugas yang belum selesai
        TampilkanDaftarTugasDenganStatus();

        Console.Write("\nMasukkan ID tugas yang ingin ditandai selesai (atau 0 untuk batal): ");

        if (!int.TryParse(Console.ReadLine(), out int taskId))
        {
            Console.WriteLine("Input tidak valid!");
            Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
            Console.ReadLine();
            return;
        }

        if (taskId == 0)
        {
            Console.WriteLine("Penandaan tugas dibatalkan.");
            Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
            Console.ReadLine();
            return;
        }

        try
        {
            using MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();

            // Cek apakah tugas ada, belum selesai, dan milik user yang login
            string checkQuery = "SELECT id, title, status FROM tasks WHERE id = @id AND user_id = @user_id AND status = 'pending'";
            using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
            {
                checkCmd.Parameters.AddWithValue("@id", taskId);
                checkCmd.Parameters.AddWithValue("@user_id", user_id);

                using (MySqlDataReader reader = checkCmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Tugas tidak ditemukan, sudah selesai, atau tidak memiliki akses!");
                        Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
                        Console.ReadLine();
                        return;
                    }

                    reader.Read();
                    string taskTitle = reader.GetString("title");
                    reader.Close();

                    // Konfirmasi penandaan selesai
                    Console.Write($"Apakah Anda yakin ingin menandai tugas '{taskTitle}' sebagai selesai? (y/n): ");
                    string? confirmation = Console.ReadLine()?.ToLower();

                    if (confirmation != "y" && confirmation != "ya")
                    {
                        Console.WriteLine("Penandaan tugas dibatalkan.");
                        Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
                        Console.ReadLine();
                        return;
                    }

                    // Update status menjadi completed
                    string updateQuery = "UPDATE tasks SET status = 'completed', updated_at = NOW() WHERE id = @id AND user_id = @user_id";
                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@id", taskId);
                        updateCmd.Parameters.AddWithValue("@user_id", user_id);

                        int rowsAffected = updateCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("✅ Tugas berhasil ditandai sebagai selesai!");
                        }
                        else
                        {
                            Console.WriteLine("❌ Gagal menandai tugas sebagai selesai.");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Terjadi Error: {ex.Message}");
        }

        Console.WriteLine("Tekan enter untuk kembali ke menu utama...");
        Console.ReadLine();
    }

    static void Statistik()
    {
        Console.Clear();
        Console.WriteLine("statistik tugas anda:");
        Console.Write("Tekan enter untuk kembali ke menu utama");
        Console.ReadLine();
        return;
    }
    static string hitungmundur(DateTime deadline)
    {
        TimeSpan sisawaktu = deadline - DateTime.Now;
        if (sisawaktu.TotalSeconds < 0)
        {
            TimeSpan waktuLewat = DateTime.Now - deadline;
            if (waktuLewat.TotalDays >= 1)
                return $"⏰ Terlewat {waktuLewat.Days} hari {waktuLewat.Hours} jam";
            else if (waktuLewat.TotalHours >= 1)
                return $"⏰ Terlewat {waktuLewat.Hours} jam {waktuLewat.Minutes} menit";
            else
                return $"⏰ Terlewat {waktuLewat.Minutes} menit {waktuLewat.Seconds} detik";
        }
        else if (sisawaktu.TotalDays >= 1)
        {
            //lebih dari 1 hari
            return $"⏳ {sisawaktu.Days} hari {sisawaktu.Hours} jam lagi";
        }
        else if (sisawaktu.TotalHours >= 1)
        {
            //lebih dari 1 jam
            return $"⏳ {sisawaktu.Hours} jam {sisawaktu.Minutes} menit lagi";
        }
        else if (sisawaktu.TotalMinutes >= 1)
        {
            //lebih dari 1 menit
            return $"⏳ {sisawaktu.Minutes} menit {sisawaktu.Seconds} detik lagi";
        }
        else
        {
            return $"⏳ Hampir habis {sisawaktu.Seconds} detik lagi";
        }
    }

}
