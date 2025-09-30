using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace BancoDeDados1
{
    public class Banco
    {
        private readonly string connectionString;


        public Banco(string connectionString)
        {
            this.connectionString = connectionString;
        }



        public void GetAllUsers()
        {
            string sql = "SELECT * FROM usuarios;";
            using (var conn = new MySqlConnection(this.connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32("id");
                        string nome = reader.GetString("nome");
                        string senha = reader.GetString("senha");
                        string email = reader.GetString("email");

                        Console.WriteLine($"ID: {id}\nNome: {nome}\nSenha: {senha}\n Email:{email}");
                    }
                }
            }
        }

        public bool UserExists(string pNome, string pSenha, string pEmail)
        {
            string sql = $"SELECT COUNT(*) FROM usuarios WHERE nome=@nome AND senha=@senha AND email=@email;";
            using (var conn = new MySqlConnection(this.connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nome", pNome);
                    cmd.Parameters.AddWithValue("@senha", pSenha);
                    cmd.Parameters.AddWithValue("@email", pEmail);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    return count > 0;
                }
            }
        }


        public void AddUser(string nome, string senha, string email)
        {
            string sql = "INSERT INTO usuarios (nome, senha, email) VALUES (@nome, @senha, @email)";

            using (var conn = new MySqlConnection(this.connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nome", nome);
                    cmd.Parameters.AddWithValue("@senha", senha);
                    cmd.Parameters.AddWithValue("@email", email);

                    cmd.ExecuteNonQuery();
                }
            }
        }


        public void UpdateBestScore(string nome, string senha, string email, int bestScore)
        {
            string sql = "UPDATE usuarios SET bestScore=@bestScore WHERE nome=@nome AND senha=@senha AND email=@email;";

            using (var conn = new MySqlConnection(this.connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@bestScore", bestScore);
                    cmd.Parameters.AddWithValue("@nome", nome);
                    cmd.Parameters.AddWithValue("@senha", senha);
                    cmd.Parameters.AddWithValue("@email", email);

                    cmd.ExecuteNonQuery();
                }
            }
        }



        public int GetBestScore(string nome, string senha, string email)
        {
            string sql = "SELECT bestScore FROM usuarios WHERE nome=@nome AND senha=@senha AND email=@email;";

            using (var conn = new MySqlConnection(this.connectionString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nome", nome);
                    cmd.Parameters.AddWithValue("@senha", senha);
                    cmd.Parameters.AddWithValue("@email", email);

                    object result = cmd.ExecuteScalar();

                    int bestScore = Convert.ToInt32(result);

                    return bestScore;
                }
            }
        }
    }
}
