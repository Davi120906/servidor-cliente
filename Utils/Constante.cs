namespace AsteroidesCliente.Utils
{
    public static class Constantes
    {
        // Configurações da tela
        public const int LARGURA_TELA = 800;
        public const int ALTURA_TELA = 600;
        
        // Configurações de rede
        public const int PORTA_PADRAO = 12345;
        public const int TIMEOUT_CONEXAO = 30000; // 30 segundos
        
        // Configurações de jogo
        public const int FPS = 60;
        public const int INTERVALO_FRAME = 1000 / FPS; // 16ms
        public const int SPAWN_ASTEROIDE_FRAMES = 40;
        public const int MAX_ASTEROIDES = 10;
        
        // Tipos de mensagem
        public static class TipoMensagem
        {
            public const string CONECTAR = "CONECTAR";
            public const string DESCONECTAR = "DESCONECTAR";
            public const string ID_JOGADOR = "ID_JOGADOR";
            public const string ESTADO_JOGO = "ESTADO_JOGO";
            public const string MOVIMENTO = "MOVIMENTO";
            public const string TIRO = "TIRO";
            public const string PING = "PING";
            public const string PONG = "PONG";
            public const string ERRO = "ERRO";
        }
    }
}