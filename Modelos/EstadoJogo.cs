using System.Text.Json.Serialization;

namespace AsteroidesCliente.Modelos
{
    public class EstadoJogo
    {
        [JsonPropertyName("jogadores")]
        public List<EstadoJogador> Jogadores { get; set; } = new();
        
        [JsonPropertyName("asteroides")]
        public List<AsteroideRede> Asteroides { get; set; } = new();
        
        [JsonPropertyName("tiros")]
        public List<TiroRede> Tiros { get; set; } = new();
        
        [JsonPropertyName("frameCount")]
        public long FrameCount { get; set; }
        
        [JsonPropertyName("jogoAtivo")]
        public bool JogoAtivo { get; set; } = true;
    }
}