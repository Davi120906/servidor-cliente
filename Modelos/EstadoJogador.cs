using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;

namespace AsteroidesCliente.Modelos
{
    public class EstadoJogador
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("nome")]
        public string Nome { get; set; } = "";
        
        [JsonPropertyName("posicao")]
        public Vector2 Posicao { get; set; }
        
        [JsonPropertyName("pontuacao")]
        public int Pontuacao { get; set; }
        
        [JsonPropertyName("ativo")]
        public bool Ativo { get; set; } = true;
        
        [JsonPropertyName("vidas")]
        public int Vidas { get; set; } = 3;
    }
}