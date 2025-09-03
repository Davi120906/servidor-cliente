using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using AsteroidesCliente.Modelos;
using AsteroidesCliente.Utils;

namespace AsteroidesCliente.Rede
{
    public enum EstadoConexao
    {
        Desconectado,
        Conectando,
        Conectado,
        Erro
    }

    public class ClienteTCP
    {
        private TcpClient? _cliente;
        private NetworkStream? _stream;
        private CancellationTokenSource _cancelToken = new();
        
        public EstadoConexao Estado { get; private set; } = EstadoConexao.Desconectado;
        public string EnderecoServidor { get; set; } = "localhost";
        public int PortaServidor { get; set; } = Constantes.PORTA_PADRAO;
        public int MeuId { get; private set; } = -1;
        public string MensagemErro { get; private set; } = "";

        // Eventos
        public event Action<EstadoJogo>? EstadoJogoRecebido;
        public event Action<string>? ErroConexao;
        public event Action<int>? IdJogadorRecebido;

        /// <summary>
        /// Conecta ao servidor de forma assíncrona (REQUISITO: async/await)
        /// </summary>
        public async Task<bool> ConectarAsync()
        {
            try
            {
                Estado = EstadoConexao.Conectando;
                MensagemErro = "";
                
                _cliente = new TcpClient();
                
                // Conectar com timeout
                var taskConexao = _cliente.ConnectAsync(EnderecoServidor, PortaServidor);
                var taskTimeout = Task.Delay(Constantes.TIMEOUT_CONEXAO);
                
                var taskCompleta = await Task.WhenAny(taskConexao, taskTimeout);
                
                if (taskCompleta == taskTimeout)
                {
                    Estado = EstadoConexao.Erro;
                    MensagemErro = "Timeout na conexão";
                    return false;
                }
                
                if (!_cliente.Connected)
                {
                    Estado = EstadoConexao.Erro;
                    MensagemErro = "Falha ao conectar com o servidor";
                    return false;
                }
                
                _stream = _cliente.GetStream();
                Estado = EstadoConexao.Conectado;
                
                // Iniciar recepção de mensagens em paralelo (REQUISITO: não bloquear)
                _ = Task.Run(ReceberMensagensAsync);
                
                return true;
            }
            catch (Exception ex)
            {
                Estado = EstadoConexao.Erro;
                MensagemErro = $"Erro ao conectar: {ex.Message}";
                ErroConexao?.Invoke(MensagemErro);
                return false;
            }
        }

        /// <summary>
        /// Loop assíncrono para receber mensagens (REQUISITO: async/await)
        /// </summary>
        private async Task ReceberMensagensAsync()
        {
            var buffer = new byte[8192];
            
            try
            {
                while (Estado == EstadoConexao.Conectado && 
                       _stream != null && 
                       !_cancelToken.Token.IsCancellationRequested)
                {
                    try
                    {
                        var bytesLidos = await _stream.ReadAsync(buffer, 0, buffer.Length, _cancelToken.Token);
                        
                        if (bytesLidos == 0)
                        {
                            // Servidor desconectou
                            Estado = EstadoConexao.Desconectado;
                            ErroConexao?.Invoke("Servidor desconectou");
                            break;
                        }
                        
                        var mensagemJson = Encoding.UTF8.GetString(buffer, 0, bytesLidos);
                        
                        // Pode ter múltiplas mensagens concatenadas
                        var mensagens = mensagemJson.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        
                        foreach (var msgJson in mensagens)
                        {
                            try
                            {
                                var mensagem = JsonSerializer.Deserialize<MensagemRede>(msgJson);
                                if (mensagem != null)
                                {
                                    ProcessarMensagem(mensagem);
                                }
                            }
                            catch (JsonException ex)
                            {
                                Console.WriteLine($"Erro ao deserializar mensagem: {ex.Message}");
                                Console.WriteLine($"JSON: {msgJson}");
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancelamento normal
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Estado = EstadoConexao.Erro;
                MensagemErro = $"Erro na recepção: {ex.Message}";
                ErroConexao?.Invoke(MensagemErro);
            }
        }

        /// <summary>
        /// Processa mensagens recebidas do servidor
        /// </summary>
        private void ProcessarMensagem(MensagemRede mensagem)
        {
            try
            {
                switch (mensagem.Tipo)
                {
                    case Constantes.TipoMensagem.ID_JOGADOR:
                        MeuId = int.Parse(mensagem.Dados);
                        IdJogadorRecebido?.Invoke(MeuId);
                        break;
                        
                    case Constantes.TipoMensagem.ESTADO_JOGO:
                        var estado = JsonSerializer.Deserialize<EstadoJogo>(mensagem.Dados);
                        if (estado != null)
                        {
                            EstadoJogoRecebido?.Invoke(estado);
                        }
                        break;
                        
                    case Constantes.TipoMensagem.PONG:
                        // Resposta ao ping - pode calcular latência
                        break;
                        
                    case Constantes.TipoMensagem.ERRO:
                        ErroConexao?.Invoke($"Erro do servidor: {mensagem.Dados}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar mensagem tipo '{mensagem.Tipo}': {ex.Message}");
            }
        }

        /// <summary>
        /// Envia mensagem para o servidor de forma assíncrona (REQUISITO: async/await)
        /// </summary>
        public async Task<bool> EnviarMensagemAsync(MensagemRede mensagem)
        {
            try
            {
                if (_stream == null || Estado != EstadoConexao.Conectado)
                {
                    return false;
                }

                mensagem.JogadorId = MeuId;
                mensagem.Timestamp = DateTime.Now;
                
                var json = JsonSerializer.Serialize(mensagem) + "\n";
                var bytes = Encoding.UTF8.GetBytes(json);
                
                await _stream.WriteAsync(bytes, 0, bytes.Length, _cancelToken.Token);
                await _stream.FlushAsync(_cancelToken.Token);
                
                return true;
            }
            catch (Exception ex)
            {
                Estado = EstadoConexao.Erro;
                MensagemErro = $"Erro ao enviar: {ex.Message}";
                ErroConexao?.Invoke(MensagemErro);
                return false;
            }
        }

        /// <summary>
        /// Envia movimento do jogador para o servidor
        /// </summary>
        public async Task EnviarMovimentoAsync(Microsoft.Xna.Framework.Vector2 posicao)
        {
            var mensagem = new MensagemRede
            {
                Tipo = Constantes.TipoMensagem.MOVIMENTO,
                Dados = JsonSerializer.Serialize(posicao)
            };
            
            await EnviarMensagemAsync(mensagem);
        }

        /// <summary>
        /// Envia tiro do jogador para o servidor
        /// </summary>
        public async Task EnviarTiroAsync(Microsoft.Xna.Framework.Vector2 posicao)
        {
            var tiroRede = new TiroRede
            {
                Posicao = posicao,
                Velocidade = new Microsoft.Xna.Framework.Vector2(0, -8),
                JogadorId = MeuId
            };
            
            var mensagem = new MensagemRede
            {
                Tipo = Constantes.TipoMensagem.TIRO,
                Dados = JsonSerializer.Serialize(tiroRede)
            };
            
            await EnviarMensagemAsync(mensagem);
        }

        /// <summary>
        /// Envia ping para medir latência
        /// </summary>
        public async Task EnviarPingAsync()
        {
            var mensagem = new MensagemRede
            {
                Tipo = Constantes.TipoMensagem.PING,
                Dados = DateTime.Now.Ticks.ToString()
            };
            
            await EnviarMensagemAsync(mensagem);
        }

        /// <summary>
        /// Desconecta do servidor
        /// </summary>
        public void Desconectar()
        {
            try
            {
                Estado = EstadoConexao.Desconectado;
                _cancelToken.Cancel();
                
                _stream?.Close();
                _cliente?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao desconectar: {ex.Message}");
            }
            finally
            {
                _stream = null;
                _cliente = null;
                MeuId = -1;
            }
        }

        /// <summary>
        /// Verifica se está conectado
        /// </summary>
        public bool EstaConectado => Estado == EstadoConexao.Conectado && 
                                    _cliente?.Connected == true;
    }
}