using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Crypto.Digests;
using SendGrid;
using SendGrid.Helpers.Mail;



namespace BancoDeDados1
{
    class Program
    {
        private static Banco banco;

        async static Task Main()
        {
            banco = new Banco("server=localhost;database=meu_banco1;user=root;");

            await Server();
        }


        private async static Task Server()
        {
            int porta = 8080;
            TcpListener listener = new TcpListener(IPAddress.Any, porta);
            listener.Start();

            Console.WriteLine($"Servidor rodando na porta {porta}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Cliente conectado de: " + client.Client.RemoteEndPoint);

                _ = Task.Run(() => HandleClient(client));
            }
        }




        private async static Task HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            try
            {
                byte[] buffer = new byte[8192];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);


                if (bytesRead == 0) return;





                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);


                Console.WriteLine($"Requisição recebida:\n{request.Split("\r\n")[0]}");


                string firstLine = request.Split("\r\n")[0];
                string method = firstLine.Split(' ')[0];
                string pathAndQuery = firstLine.Split(" ")[1];


                string path = pathAndQuery.Split("?")[0];

                string query = pathAndQuery.Contains("?") ? WebUtility.UrlDecode(pathAndQuery.Split("?")[1]) : "";



                int bodyIndex = request.IndexOf("\r\n\r\n");
                string jsonBody = bodyIndex >= 0 ? request.Substring(bodyIndex + 4) : "";

                //Dictionary<string, string>? body = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonBody);
                


                Dictionary<string, string> queryParams = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(query))
                {
                    foreach (string part in query.Split("&"))
                    {
                        var kv = part.Split('=');

                        if (kv.Length == 2)
                        {
                            string key = kv[0];
                            string value = kv[1];

                            queryParams[key] = value;
                        }
                    }
                }
                


                

                string respostaJson = "{}";
                string statusLine = "HTTP/1.1 200 OK\r\n";




                if (method == "GET" && path == "/usuarios")
                {
                    if (queryParams.ContainsKey("nome") && queryParams.ContainsKey("senha") && queryParams.ContainsKey("email"))
                    {
                        bool userExists = banco.UserExists(queryParams["nome"], queryParams["senha"], queryParams["email"]);

                        respostaJson = JsonSerializer.Serialize(new { mensagem = userExists });
                    }
                    //"{\"\":\"\"}";
                }
                else if (method == "GET")
                {
                    respostaJson = "{\"mensagem\":\"O servidor recebeu seu GET\"}";
                }
                else if (method == "POST" && path == "/usuarios")
                {
                    jsonBody = jsonBody.Trim();

                    Console.WriteLine("JSON recebido: " + jsonBody);

                    MensagemUsuario? mensagemUsuario = JsonSerializer.Deserialize<MensagemUsuario>(jsonBody);

                    Usuario? usuario = mensagemUsuario?.mensagem;

                    Console.WriteLine("Usuário desserializado: Nome=" + usuario?.Nome + ", Senha=" + usuario?.Senha + ", Email=" + usuario?.Email);
                    
                    if (usuario != null)
                    {
                        banco.AddUser(usuario.Nome, usuario.Senha, usuario.Email);
                        respostaJson = JsonSerializer.Serialize(new { mensagem = "Usuário cadastrado com sucesso!" });
                    }
                }
                else if (method == "POST" && path == "/updateBestScore")
                {
                    jsonBody = jsonBody.Trim();

                    Console.WriteLine("JSON recebido: " + jsonBody);


                    MensagemUsuarioEmail? mensagemUsuarioEmail = JsonSerializer.Deserialize<MensagemUsuarioEmail>(jsonBody);


                    bool bestScoreWasUpdated = false;

                    if (mensagemUsuarioEmail?.mensagemUsuario != null)
                    {
                        Usuario? usuario = mensagemUsuarioEmail?.mensagemUsuario;

                        Console.WriteLine("Usuário desserializado: Nome=" + usuario?.Nome + ", Senha=" + usuario?.Senha + ", Email=" + usuario?.Email);
                        
                        if (usuario != null && usuario.BestScore != null)
                        {
                            int oldBestScore = banco.GetBestScore(usuario.Nome, usuario.Senha, usuario.Email);

                            if ((int)usuario.BestScore > oldBestScore)
                            {
                                banco.UpdateBestScore(usuario.Nome, usuario.Senha, usuario.Email, (int)usuario.BestScore);
                                bestScoreWasUpdated = true;
                            }
                        }
                    }



                    if (mensagemUsuarioEmail?.mensagemEmail != null && bestScoreWasUpdated)
                    {
                        Email email = mensagemUsuarioEmail.mensagemEmail;

                        await SendEmail(email.Endereco, email.Content, email.Subject);

                        respostaJson = JsonSerializer.Serialize(new { mensagem = $"Email enviado com sucesso para {email.Endereco}" });
                    }
                    else if (mensagemUsuarioEmail?.mensagemEmail == null)
                    {
                        respostaJson = JsonSerializer.Serialize(new { mensagem = "JSON inválido!" });
                    }
                    else
                    {
                        respostaJson = JsonSerializer.Serialize(new { mensagem = "A nova pontuação não é maior que a anterior" });
                    }
                }
                else if (method == "OPTIONS")
                {
                    string responseOptions = "HTTP/1.1 204 No Content\r\n" +
                                "Access-Control-Allow-Origin: *\r\n" +
                                "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n" +
                                "Access-Control-Allow-Headers: Content-Type\r\n" +
                                "Connection: close\r\n" +
                                "\r\n";

                    byte[] responseBytesOptions = Encoding.UTF8.GetBytes(responseOptions);

                    await stream.WriteAsync(responseBytesOptions, 0, responseBytesOptions.Length);
                    await stream.FlushAsync();
                    return;
                }
                else
                {
                    statusLine = "HTTP/1.1 405 Method Not Allowed\r\n";
                    respostaJson = "{\"mensagem\":\"Método não suportado\"}";
                }



                string response = statusLine +
                                    "Content-Type: application/json; charset=UTF-8\r\n" +
                                    "Access-Control-Allow-Origin: *\r\n" +
                                    $"Content-Length: {Encoding.UTF8.GetByteCount(respostaJson)}\r\n" +
                                    "Connection: close\r\n" +
                                    "\r\n" +
                                    respostaJson;



                byte[] responseBytes = Encoding.UTF8.GetBytes(response);


                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                await stream.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro no HandleClient: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }



        private async static Task SendEmail(string destinatario, string content, string subject)
        {
            var apikey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var client = new SendGridClient(apikey);

            var from = new EmailAddress("diasdebarrospaulo@gmail.com", "Meu Sistema");
            var to = new EmailAddress(destinatario, "Usuário");
            var htmlContent = $"<strong>{content}</strong>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, content, htmlContent);

            var response = await client.SendEmailAsync(msg);

            Console.WriteLine($"Status: {response.StatusCode}");
        }
    }



    public class Usuario
    {
        public string Nome { get; set; }
        public string Senha { get; set; }
        public string Email { get; set; }
        public int? BestScore { get; set; }
    }

    public class MensagemUsuario
    {
        public Usuario mensagem { get; set; }
    }


    public class Email
    {
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Endereco { get; set; }
    }

    public class MensagemEmail
    {
        public Email email { get; set; }
    }



    public class MensagemUsuarioEmail
    {
        public Email mensagemEmail { get; set; }
        public Usuario mensagemUsuario { get; set; }
    }
}