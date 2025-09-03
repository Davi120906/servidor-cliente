using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;

namespace AsteroidesCliente.Modelos
{
    public class TiroRede
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("posicao")]
        public Vector2 Posicao { get; set; }
        
        [JsonPropertyName("velocidade")]
        public Vector2 Velocidade { get; set; }
        
        [JsonPropertyName("jogadorId")]
        public int JogadorId { get; set; }
    }
}