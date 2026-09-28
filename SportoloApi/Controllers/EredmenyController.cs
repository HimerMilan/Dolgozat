using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SportoloApi.Models;

namespace SportoloApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EredmenyController : ControllerBase
    {
        public string connectionString = "server=localhost;uid=root;password=;database=sportolo13b";

        [HttpGet]
        public object GetEredmenyek()
        {
            List<Eredmeny> eredmenyek = new List<Eredmeny>();

            var connection = new MySqlConnection(connectionString);
            connection.Open();

            string sql = "SELECT * FROM `eredmeny`;";

            var cmd = new MySqlCommand(sql, connection);
            var data = cmd.ExecuteReader();

            while (data.Read())
            {
                var eredmeny = new Eredmeny
                {
                    Id = data.GetInt32("Id"),
                    Competition = data.GetString("Competition"),
                    Description = data.GetString("Description"),
                    ResultTime = data.GetDateTime("ResultTime"),
                    UpdateTime = data.GetDateTime("UpdateTime"),
                    SportoloId = data.GetInt32("SportoloId")
                };

                eredmenyek.Add(eredmeny);
            }

            connection.Close();

            return eredmenyek;
        }



        [HttpGet("{id}")]
        public object GetEredmeny(int id)
        {
            Eredmeny eredmeny = null;

            var connection = new MySqlConnection(connectionString);
            connection.Open();

            string sql = "SELECT * FROM `eredmeny` WHERE Id = @id;";

            var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            var data = cmd.ExecuteReader();

            if (data.Read())
            {
                eredmeny = new Eredmeny
                {
                    Id = data.GetInt32("Id"),
                    Competition = data.GetString("Competition"),
                    Description = data.GetString("Description"),
                    ResultTime = data.GetDateTime("ResultTime"),
                    UpdateTime = data.GetDateTime("UpdateTime"),
                    SportoloId = data.GetInt32("SportoloId")
                };
            }

            connection.Close();

            if (eredmeny == null)
            {
                return NotFound("Nincs ilyen eredmény.");
            }

            return eredmeny;
        }

        [HttpPost]
        public object AddEredmeny(Eredmeny eredmeny)
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();

            DateTime now = DateTime.Now;

            string sql = @"
                INSERT INTO eredmeny
                (Competition, Description, ResultTime, UpdateTime, SportoloId)
                VALUES
                (@competition, @description, @resultTime, @updateTime, @sportoloId);
            ";

            var cmd = new MySqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@competition", eredmeny.Competition);
            cmd.Parameters.AddWithValue("@description", eredmeny.Description);
            cmd.Parameters.AddWithValue("@resultTime", DateTime.Now);
            cmd.Parameters.AddWithValue("@updateTime", DateTime.Now);
            cmd.Parameters.AddWithValue("@sportoloId", eredmeny.SportoloId);

            cmd.ExecuteNonQuery();

            connection.Close();

            return Ok("Az eredmény sikeresen létrehozva.");
        }

        [HttpPut("{id}")]
        public object UpdateEredmeny(int id, Eredmeny eredmeny)
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();

            string vanesql = "SELECT COUNT(*) FROM eredmeny WHERE Id = @id;";

            var vanecmd = new MySqlCommand(vanesql, connection);
            vanecmd.Parameters.AddWithValue("@id", id);

            int count = Convert.ToInt32(vanecmd.ExecuteScalar());

            if (count == 0)
            {
                connection.Close();
                return NotFound("Nincs ilyen eredmény.");
            }

            DateTime now = DateTime.Now;

            string sql = @"
                UPDATE eredmeny
                SET Competition = @competition,
                    Description = @description,
                    ResultTime = @resultTime,
                    UpdateTime = @updateTime,
                    SportoloId = @sportoloId
                WHERE Id = @id;
            ";

            var cmd = new MySqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@competition", eredmeny.Competition);
            cmd.Parameters.AddWithValue("@description", eredmeny.Description);
            cmd.Parameters.AddWithValue("@resultTime", DateTime.Now);
            cmd.Parameters.AddWithValue("@updateTime", DateTime.Now);
            cmd.Parameters.AddWithValue("@sportoloId", eredmeny.SportoloId);

            cmd.ExecuteNonQuery();

            connection.Close();

            return Ok("Az eredmény sikeresen módosítva.");
        }

        [HttpDelete("{id}")]
        public object DeleteEredmeny(int id)
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();

            string sql = "DELETE FROM eredmeny WHERE Id = @id;";

            var cmd = new MySqlCommand(sql, connection);

            cmd.Parameters.AddWithValue("@id", id);

            int sorok = cmd.ExecuteNonQuery();

            connection.Close();

            if (sorok == 0)
            {
                return NotFound("Nincs ilyen eredmény.");
            }

            return Ok("Az eredmény sikeresen törölve.");
        }

        [HttpGet("sportolo/{id}")]
        public object GetSportolo(int id)
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();

            string sql = "SELECT name, email FROM sportolo WHERE Id = @id;";

            var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            var data = cmd.ExecuteReader();

            if (!data.Read())
            {
                connection.Close();
                return NotFound("Nincs ilyen sportoló.");
            }

            var sportolo = new
            {
                Name = data.GetString("name"),
                Email = data.GetString("email")
            };

            connection.Close();

            return sportolo;
        }

        [HttpGet("sportolo/{id}/eredmenyek")]
        public object GetSportoloEredmenyek(int id)
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();

            string sql = @"
                SELECT sportolo.name, eredmeny.Competition, eredmeny.Description
                FROM sportolo
                INNER JOIN eredmeny ON sportolo.Id = eredmeny.SportoloId
                WHERE sportolo.Id = @id;
            ";

            var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            var data = cmd.ExecuteReader();

            string name = "";
            List<object> eredmenyek = new List<object>();

            while (data.Read())
            {
                name = data.GetString("name");

                var eredmeny = new
                {
                    Competition = data.GetString("Competition"),
                    Description = data.GetString("Description")
                };

                eredmenyek.Add(eredmeny);
            }

            connection.Close();

            if (eredmenyek.Count == 0)
            {
                return NotFound("Nincs ilyen sportoló vagy nincs hozzá tartozó eredmény.");
            }

            return new
            {
                Name = name,
                Eredmenyek = eredmenyek
            };
        }

        [HttpGet("eredmenyek/count")]
        public object GetEredmenyekSzama()
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();

            string sql = "SELECT COUNT(*) FROM eredmeny;";

            var cmd = new MySqlCommand(sql, connection);

            int darab = Convert.ToInt32(cmd.ExecuteScalar());

            connection.Close();

            return new
            {
                EredmenyekSzama = darab
            };
        }

        [HttpGet("sportolo/{id}/eredmenyek/count")]
        public object GetSportoloEredmenyekSzama(int id)
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();

            string sql = "SELECT COUNT(*) FROM eredmeny WHERE SportoloId = @id;";

            var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            int darab = Convert.ToInt32(cmd.ExecuteScalar());

            connection.Close();

            return new
            {
                SportoloId = id,
                EredmenyekSzama = darab
            };
        }

    }
}
