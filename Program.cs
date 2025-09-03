using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monogame.Processing;
using System.Text.Json;
using AsteroidesCliente.Rede;
using AsteroidesCliente.Modelos;
using AsteroidesCliente.Game;
using AsteroidesCliente.Utils;

namespace AsteroidesCliente
{
    public static class PConstants
    {
        public const int LEFT = 0;
        public const int CENTER = 1;
        public const int RIGHT = 2;
        public const int TOP = 3;
        public const int BOTTOM = 4;
    }

    public class JogoAsteroidesMultiplayer : Processing
    {
        // Rede
        private ClienteTCP _cliente = new();
        private EstadoJogo _estadoJogoRede = new();
        private readonly object _estadoLock = new();

        // Estado local
        private Nave? _minhaave;
        private readonly Dictionary<int, Nave> _navesJogadores = new();
        private readonly List<Tiro> _tirosLocais = new();
        private readonly List<Asteroide> _asteroidesLocais = new();

        // Interface
        private enum TelaAtual { Menu, Conectando, Jogo, GameOver, Erro }
        private TelaAtual _telaAtual = TelaAtual.Menu;
        private string _mensagemStatus = "";
        private string _enderecoInput = "localhost";
        private string _portaInput = "12345";
        private bool _editandoEndereco = false;
        private bool _editandoPorta = false;
        private string _inputBuffer = "";

        // Input
        private bool _esquerda, _direita, _cima, _baixo;
        private bool _podeAtirar = true;
        private int _cooldownTiro = 0;
        private KeyboardState _keyboardAnterior;

        public override void Setup()
        {
            size(Constantes.LARGURA_TELA, Constantes.ALTURA_TELA);

            // Configurar eventos do cliente de rede
            _cliente.EstadoJogoRecebido += OnEstadoJogoRecebido;
            _cliente.ErroConexao += OnErroConexao;
            _cliente.IdJogadorRecebido += OnIdJogadorRecebido;
        }

        public override void Draw()
        {
            background(0);

            switch (_telaAtual)
            {
                case TelaAtual.Menu:
                    DesenharMenu();
                    break;
                case TelaAtual.Conectando:
                    DesenharConectando();
                    break;
                case TelaAtual.Jogo:
                    DesenharJogo();
                    break;
                case TelaAtual.GameOver:
                    DesenharGameOver();
                    break;
                case TelaAtual.Erro:
                    DesenharErro();
                    break;
            }
        }

        #region Telas da Interface

       private void DesenharMenu()
{
    fill(255);

    // Título "ASTEROIDES MULTIPLAYER"
    textSize(32);
    string titulo = "ASTEROIDES MULTIPLAYER";
    float twTitulo = titulo.Length * 16;  // Estimando 16px por caractere (ajuste conforme necessário)
    text(titulo, width / 2f - twTitulo / 2, 80);

    // Texto "Configure a conexão:"
    textSize(16);
    string configurarConexao = "Configure a conexão:";
    float twConfigurarConexao = configurarConexao.Length * 8;  // Estimando 8px por caractere
    text(configurarConexao, width / 2f - twConfigurarConexao / 2, 150);

    // Campo de endereço
    fill(_editandoEndereco ? 100 : 50);
    rect(width / 2f - 150, 180, 300, 30);
    fill(255);
    string enderecoTexto = $"Endereço: {(_editandoEndereco ? _inputBuffer : _enderecoInput)}";
    float twEndereco = enderecoTexto.Length * 8;  // Estimando 8px por caractere
    text(enderecoTexto, width / 2f - twEndereco / 2, 195);

    // Campo de porta
    fill(_editandoPorta ? 100 : 50);
    rect(width / 2f - 150, 230, 300, 30);
    fill(255);
    string portaTexto = $"Porta: {(_editandoPorta ? _inputBuffer : _portaInput)}";
    float twPorta = portaTexto.Length * 8;  // Estimando 8px por caractere
    text(portaTexto, width / 2f - twPorta / 2, 245);

    // Botão "CONECTAR"
    fill(0, 150, 0);
    rect(width / 2f - 100, 300, 200, 50);
    fill(255);
    string conectarTexto = "CONECTAR (Enter)";
    float twConectar = conectarTexto.Length * 8;  // Estimando 8px por caractere
    text(conectarTexto, width / 2f - twConectar / 2, 325);

    // Botão "SAIR"
    fill(150, 0, 0);
    rect(width / 2f - 100, 370, 200, 50);
    fill(255);
    string sairTexto = "SAIR (Esc)";
    float twSair = sairTexto.Length * 8;  // Estimando 8px por caractere
    text(sairTexto, width / 2f - twSair / 2, 395);

    // Instruções
    textSize(12);
    string instrucoesTitulo = "Instruções:";
    float twInstrucoesTitulo = instrucoesTitulo.Length * 6;  // Estimando 6px por caractere
    text(instrucoesTitulo, 20, 450);

    string instrucao1 = "• Clique nos campos para editar endereço/porta";
    float twInstrucao1 = instrucao1.Length * 6;  // Estimando 6px por caractere
    text(instrucao1, 20, 470);

    string instrucao2 = "• Enter para conectar, Esc para sair";
    float twInstrucao2 = instrucao2.Length * 6;  // Estimando 6px por caractere
    text(instrucao2, 20, 485);

    string instrucao3 = "• WASD ou Setas para mover, Espaço para atirar";
    float twInstrucao3 = instrucao3.Length * 6;  // Estimando 6px por caractere
    text(instrucao3, 20, 500);

    // Processar entradas do menu
    ProcessarInputMenu();
}

        private void DesenharConectando()
{
    fill(255);

    // Texto "Conectando ao servidor..."
    textSize(24);
    string conectandoTexto = "Conectando ao servidor...";
    float twConectando = conectandoTexto.Length * 12;  // Estimando 12px por caractere
    text(conectandoTexto, width / 2f - twConectando / 2, height / 2f - 40);

    // Texto com o endereço e porta do servidor
    string enderecoTexto = $"{_cliente.EnderecoServidor}:{_cliente.PortaServidor}";
    float twEndereco = enderecoTexto.Length * 8;  // Estimando 8px por caractere
    text(enderecoTexto, width / 2f - twEndereco / 2, height / 2f);

    // Mensagem de status, se existir
    if (!string.IsNullOrEmpty(_mensagemStatus))
    {
        textSize(16);
        fill(255, 100, 100);
        string statusTexto = _mensagemStatus;
        float twStatus = statusTexto.Length * 8;  // Estimando 8px por caractere
        text(statusTexto, width / 2f - twStatus / 2, height / 2f + 40);
    }

    // Animação de loading
    var pontos = ((int)(frameCount / 20) % 4);
    var loading = "Conectando" + new string('.', pontos);
    textSize(18);
    fill(200);
    string loadingTexto = loading;
    float twLoading = loadingTexto.Length * 9;  // Estimando 9px por caractere
    text(loadingTexto, width / 2f - twLoading / 2, height / 2f + 80);
}


        private void DesenharJogo()
        {
            ProcessarInputJogo();

            // Copiar estado do jogo de forma thread-safe
            EstadoJogo estadoCopia;
            lock (_estadoLock)
            {
                estadoCopia = JsonSerializer.Deserialize<EstadoJogo>(JsonSerializer.Serialize(_estadoJogoRede)) ?? new EstadoJogo();
            }

            // Desenhar asteroides
            foreach (var asteroideDados in estadoCopia.Asteroides)
            {
                var asteroide = new Asteroide(
                    asteroideDados.Posicao,
                    asteroideDados.Velocidade,
                    asteroideDados.Raio,
                    asteroideDados.Id);
                asteroide.Desenhar(this);
            }

            // Desenhar tiros
            foreach (var tiroDados in estadoCopia.Tiros)
            {
                var tiro = new Tiro(
                    tiroDados.Posicao,
                    tiroDados.Velocidade,
                    tiroDados.JogadorId,
                    tiroDados.Id);
                tiro.Desenhar(this, tiroDados.JogadorId == _cliente.MeuId, _cliente.MeuId);
            }

            // Desenhar jogadores
            foreach (var jogadorDados in estadoCopia.Jogadores)
            {
                var ehMeu = jogadorDados.Id == _cliente.MeuId;
                var cor = ehMeu ? Color.Green : GetCorJogador(jogadorDados.Id);

                var nave = new Nave(jogadorDados.Posicao, cor, ehMeu);
                nave.Desenhar(this, jogadorDados.Ativo);
            }

            // Interface do jogo
            DesenharInterfaceJogo(estadoCopia);

            // Verificar game over
            var meuJogador = estadoCopia.Jogadores.FirstOrDefault(j => j.Id == _cliente.MeuId);
            if (meuJogador != null && !meuJogador.Ativo)
            {
                _telaAtual = TelaAtual.GameOver;
            }
        }

        private void DesenharInterfaceJogo(EstadoJogo estado)
{
    fill(255);
    textSize(16);

    int y = 10;

    // Exibe a quantidade de jogadores
    string jogadoresTexto = $"Jogadores: {estado.Jogadores.Count}";
    float twJogadores = jogadoresTexto.Length * 8;  // Estimando 8px por caractere
    text(jogadoresTexto, 10, y);
    y += 20;

    // Exibe o status de cada jogador
    foreach (var jogador in estado.Jogadores.OrderByDescending(j => j.Pontuacao))
    {
        var status = jogador.Ativo ? "VIVO" : "MORTO";
        var cor = jogador.Id == _cliente.MeuId ? " (VOCÊ)" : "";
        string jogadorTexto = $"{jogador.Nome}{cor}: {jogador.Pontuacao} pts - {status}";

        float twJogador = jogadorTexto.Length * 8;  // Estimando 8px por caractere
        text(jogadorTexto, 10, y);
        y += 16;
    }

    // Status da conexão (alinhamento à direita)
    var estadoTexto = _cliente.EstaConectado ? "CONECTADO" : "DESCONECTADO";
    fill(_cliente.EstaConectado ? 100 : 255, 255, 100);

    string statusTexto = $"Status: {estadoTexto}";
    float twStatus = statusTexto.Length * 8;  // Estimando 8px por caractere
    text(statusTexto, width - twStatus - 10, 10);

    // Instruções (alinhamento centralizado na parte inferior)
    textSize(12);
    fill(200);
    string instrucoesTexto = "WASD/Setas: Mover | Espaço: Atirar | ESC: Menu";
    float twInstrucoes = instrucoesTexto.Length * 8;  // Estimando 8px por caractere
    text(instrucoesTexto, width / 2f - twInstrucoes / 2, height - 10);
}


        private void DesenharGameOver()
{
    fill(255, 0, 0);
    textSize(48);
    string textoGameOver = "GAME OVER";
    float twGameOver = textoGameOver.Length * 24;  // Estimando 24px por caractere (ajuste conforme necessário)
    text(textoGameOver, width / 2f - twGameOver / 2, height / 2f - 50);

    fill(255);
    textSize(20);
    string textoReiniciar = "Pressione R para reconectar ou ESC para o menu";
    float twReiniciar = textoReiniciar.Length * 10;  // Estimando 10px por caractere
    text(textoReiniciar, width / 2f - twReiniciar / 2, height / 2f + 20);

    // Mostrar pontuação final
    lock (_estadoLock)
    {
        var meuJogador = _estadoJogoRede.Jogadores.FirstOrDefault(j => j.Id == _cliente.MeuId);
        if (meuJogador != null)
        {
            textSize(24);
            string textoPontuacao = $"Sua pontuação: {meuJogador.Pontuacao}";
            float twPontuacao = textoPontuacao.Length * 12;  // Estimando 12px por caractere
            text(textoPontuacao, width / 2f - twPontuacao / 2, height / 2f + 60);
        }
    }
}


        private void DesenharErro()
{
    fill(255, 100, 100);
    textSize(24);
    string erroConexao = "ERRO DE CONEXÃO";
    float twErro = erroConexao.Length * 12;  // Estimando 12px por caractere (ajuste conforme necessário)
    text(erroConexao, width / 2f - twErro / 2, height / 2f - 50);

    fill(255);
    textSize(16);
    string mensagemStatus = _mensagemStatus;
    float twMensagemStatus = mensagemStatus.Length * 8;  // Estimando 8px por caractere (ajuste conforme necessário)
    text(mensagemStatus, width / 2f - twMensagemStatus / 2, height / 2f);

    string textoMenu = "Pressione ESC para voltar ao menu";
    float twMenu = textoMenu.Length * 8;  // Estimando 8px por caractere
    text(textoMenu, width / 2f - twMenu / 2, height / 2f + 40);
}


        #endregion

        #region Processamento de Input

        private void ProcessarInputMenu()
        {
            var keyboardAtual = Keyboard.GetState();

            // Detectar cliques nos campos
            if (mousePressed)
            {
                int mx = this.mouseX;
                int my = this.mouseY;

                // Campo endereço
                if (mx >= width / 2f - 150 && mx <= width / 2f + 150 &&
                    my >= 180 && my <= 210)
                {
                    _editandoEndereco = true;
                    _editandoPorta = false;
                    _inputBuffer = _enderecoInput;
                }
                // Campo porta
                else if (mx >= width / 2f - 150 && mx <= width / 2f + 150 &&
                         my >= 230 && my <= 260)
                {
                    _editandoPorta = true;
                    _editandoEndereco = false;
                    _inputBuffer = _portaInput;
                }
                else
                {
                    _editandoEndereco = false;
                    _editandoPorta = false;
                }
            }

            // Processar teclas
            if (keyboardAtual.IsKeyDown(Keys.Enter) && !_keyboardAnterior.IsKeyDown(Keys.Enter))
            {
                if (_editandoEndereco)
                {
                    _enderecoInput = _inputBuffer;
                    _editandoEndereco = false;
                }
                else if (_editandoPorta)
                {
                    _portaInput = _inputBuffer;
                    _editandoPorta = false;
                }
                else
                {
                    // Conectar
                    IniciarConexao();
                }
            }

            if (keyboardAtual.IsKeyDown(Keys.Escape) && !_keyboardAnterior.IsKeyDown(Keys.Escape))
            {
                if (_editandoEndereco || _editandoPorta)
                {
                    _editandoEndereco = false;
                    _editandoPorta = false;
                }
                else
                {
                    Exit();
                }
            }

            // Input de texto
            ProcessarInputTexto(keyboardAtual);

            _keyboardAnterior = keyboardAtual;
        }

        private void ProcessarInputTexto(KeyboardState keyboardAtual)
        {
            if (!_editandoEndereco && !_editandoPorta) return;

            // Processar caracteres digitados
            foreach (Keys key in Enum.GetValues<Keys>())
            {
                if (keyboardAtual.IsKeyDown(key) && !_keyboardAnterior.IsKeyDown(key))
                {
                    if (key == Keys.Back && _inputBuffer.Length > 0)
                    {
                        _inputBuffer = _inputBuffer[..^1];
                    }
                    else if (key >= Keys.A && key <= Keys.Z)
                    {
                        var letra = key.ToString().ToLower();
                        _inputBuffer += letra;
                    }
                    else if (key >= Keys.D0 && key <= Keys.D9)
                    {
                        var numero = ((int)key - (int)Keys.D0).ToString();
                        _inputBuffer += numero;
                    }
                    else if (key == Keys.OemPeriod)
                    {
                        _inputBuffer += ".";
                    }
                }
            }
        }

        private void ProcessarInputJogo()
        {
            var keyboardAtual = Keyboard.GetState();

            // Atualizar cooldown do tiro
            if (_cooldownTiro > 0) _cooldownTiro--;

            // Ler input de movimento
            _esquerda = keyboardAtual.IsKeyDown(Keys.A) || keyboardAtual.IsKeyDown(Keys.Left);
            _direita = keyboardAtual.IsKeyDown(Keys.D) || keyboardAtual.IsKeyDown(Keys.Right);
            _cima = keyboardAtual.IsKeyDown(Keys.W) || keyboardAtual.IsKeyDown(Keys.Up);
            _baixo = keyboardAtual.IsKeyDown(Keys.S) || keyboardAtual.IsKeyDown(Keys.Down);

            // Processar movimento
            if (_minhaave != null)
            {
                var novaPosicao = _minhaave.Atualizar(_esquerda, _direita, _cima, _baixo, width, height);

                // Enviar posição para servidor de forma assíncrona
                _ = Task.Run(() => _cliente.EnviarMovimentoAsync(novaPosicao));
            }

            // Processar tiro
            if (keyboardAtual.IsKeyDown(Keys.Space) && !_keyboardAnterior.IsKeyDown(Keys.Space) &&
                _cooldownTiro <= 0 && _minhaave != null)
            {
                var posicaoTiro = _minhaave.PosicaoTiro();
                _ = Task.Run(() => _cliente.EnviarTiroAsync(posicaoTiro));
                _cooldownTiro = 15; // Cooldown de ~250ms a 60fps
            }

            // Menu
            if (keyboardAtual.IsKeyDown(Keys.Escape) && !_keyboardAnterior.IsKeyDown(Keys.Escape))
            {
                _cliente.Desconectar();
                _telaAtual = TelaAtual.Menu;
            }

            _keyboardAnterior = keyboardAtual;
        }

        #endregion

        #region Eventos de Rede

        private void OnEstadoJogoRecebido(EstadoJogo estado)
        {
            lock (_estadoLock)
            {
                _estadoJogoRede = estado;
            }
        }

        private void OnErroConexao(string erro)
        {
            _mensagemStatus = erro;
            _telaAtual = TelaAtual.Erro;
        }

        private void OnIdJogadorRecebido(int id)
        {
            // Criar minha nave
            _minhaave = new Nave(
                new Vector2(width / 2f, height - 60),
                Color.Green,
                true);

            _telaAtual = TelaAtual.Jogo;
            _mensagemStatus = $"Conectado! Você é o jogador {id}";
        }

        #endregion

        #region Métodos Auxiliares

        private async void IniciarConexao()
        {
            _telaAtual = TelaAtual.Conectando;
            _mensagemStatus = "Conectando...";

            _cliente.EnderecoServidor = _enderecoInput;

            if (int.TryParse(_portaInput, out int porta))
            {
                _cliente.PortaServidor = porta;
            }
            else
            {
                _cliente.PortaServidor = Constantes.PORTA_PADRAO;
            }

            var conectado = await _cliente.ConectarAsync();

            if (!conectado)
            {
                _telaAtual = TelaAtual.Erro;
                _mensagemStatus = _cliente.MensagemErro;
            }
        }

        private Color GetCorJogador(int jogadorId)
        {
            // Gerar cores diferentes para cada jogador
            var cores = new Color[]
            {
                Color.Blue,
                Color.Red,
                Color.Yellow,
                Color.Magenta,
                Color.Cyan,
                Color.Orange
            };

            return cores[jogadorId % cores.Length];
        }

        #endregion

        #region Cleanup

        protected override void UnloadContent()
        {
            _cliente.Desconectar();
            base.UnloadContent();
        }

        #endregion
    }

    class Program
    {
        [STAThread]
        static void Main()
        {
            using var jogo = new JogoAsteroidesMultiplayer();
            jogo.Run();
        }
    }
}
