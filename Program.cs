using System;
using MySqlConnector;
using System.Security.Cryptography; // Komentar: untuk hash password
using System.Text;
using System.Data.Common; // Komentar: untuk encoding string ke byte

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
                Console.WriteLine("9. Cari Tugas");
                Console.WriteLine("10 Simpan / Muat Database");
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
                    case "9":
                        caritugas();
                        break;
                    case "10":
                        simpanmuatandatabase();
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
        Console.Write("Judul tugas: ");
        string? title = Console.ReadLine();

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

    static void edittugas()
    {
        Console.Clear();
        Console.WriteLine(" Belum ada isinya");
        Console.Write("Tekan enter untuk kembali ke menu utama");
        Console.ReadLine();
        return;
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
                    Console.WriteLine("\n┌─────┬──────────────────────┬──────────────────────┬──────────┬─────────────────────┬─────────────────────┐");
                    Console.WriteLine("│ ID  │ Judul                │ Deskripsi            │ Status   │ Deadline            │ Dibuat Pada         │");
                    Console.WriteLine("├─────┼──────────────────────┼──────────────────────┼──────────┼─────────────────────┼─────────────────────┤");

                    while (reader.Read())
                    {
                        // Ambil due_date dengan aman (bisa null)
                        string dueDateText;
                        if (reader.IsDBNull(reader.GetOrdinal("due_date")))
                            dueDateText = "-";
                        else
                            dueDateText = reader.GetDateTime("due_date").ToString("dd/MM/yyyy HH:mm");

                        // Ambil created_at (wajib ada)
                        DateTime createdAt = reader.GetDateTime("created_at");

                        // Title (maks 20 char + padding)
                        string title = reader["title"]?.ToString() ?? "-";
                        if (title.Length > 20) title = title.Substring(0, 20);
                        title = title.PadRight(20);

                        // Description (maks 20 char + padding)
                        string description = reader["description"]?.ToString() ?? "-";
                        if (description.Length > 20) description = description.Substring(0, 20);
                        description = description.PadRight(20);

                        // ID dan status
                        string idStr = reader["id"]?.ToString() ?? "-";
                        string statusStr = reader["status"]?.ToString() ?? "-";

                        // Cetak baris tabel
                        Console.WriteLine(
                            $"│ {idStr.PadLeft(3)} " +
                            $"│ {title} " +
                            $"│ {description} " +
                            $"│ {statusStr.PadRight(8)} " +

                            $"│ {dueDateText.PadRight(19)} " +
                            $"│ {createdAt:dd/MM/yyyy HH:mm} │");
                    }

                    Console.WriteLine("└─────┴──────────────────────┴──────────────────────┴──────────┴─────────────────────┴─────────────────────┘");
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

        string query = @"SELECT id, title, status 
                        FROM tasks WHERE user_id = @user_id 
                        ORDER BY status, id DESC";
        using (MySqlCommand cmd = new MySqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@user_id", user_id);
            
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                Console.WriteLine("\nDaftar Tugas Anda:");
                Console.WriteLine("┌─────┬────────────────────────────┬──────────┐");
                Console.WriteLine("│ ID  │ Judul Tugas                │ Status   │");
                Console.WriteLine("├─────┼────────────────────────────┼──────────┤");
                
                while (reader.Read())
                {
                    int id = reader.GetInt32("id");
                    string title = reader.GetString("title");
                    string status = reader.GetString("status");
                    
                    string truncatedTitle = title.Length > 25 ? title.Substring(0, 22) + "..." : title;
                    string statusDisplay = status == "completed" ? "Selesai" : "Pending";
                    
                    Console.WriteLine($"│ {id,-3} │ {truncatedTitle,-26} │ {statusDisplay,-8} │");
                }
                Console.WriteLine("└─────┴────────────────────────────┴──────────┘");
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
        Console.WriteLine("Ini adalah daftar tugas yang belum selesai: ");
        Console.Write("Tekan enter untuk kembali ke menu utama");
        Console.ReadLine();
        return;
    }
    static void tampilkantugasselesai()
    {
        Console.Clear();
        Console.WriteLine("Ini adalah daftar tugas yang selesai: ");
        Console.Write("Tekan enter untuk kembali ke menu utama");
        Console.ReadLine();
        return;
    }
                
    static void tandaitugasselesai()
    {
        Console.Clear();
        Console.WriteLine("belum ada isinya: ");
        Console.Write("Tekan enter untuk kembali ke menu utama");
        Console.ReadLine();
        return;
    }
                
    static void Statistik()
    {
        Console.Clear();
        Console.WriteLine("statistik tugas anda:");
        Console.Write("Tekan enter untuk kembali ke menu utama");
        Console.ReadLine();
        return;
    }
                
    static void caritugas()
    {
        Console.Clear();
        Console.WriteLine("cari tugas anda:");
        Console.Write("Tekan enter untuk kembali ke menu utama");
        Console.ReadLine();
        return;
    }
                
    static void simpanmuatandatabase()
    {
        Console.Clear();
        Console.WriteLine("belum ada isinya ");
        Console.Write("Tekan enter untuk kembali ke menu utama");
        Console.ReadLine();
        return;
    }
}
