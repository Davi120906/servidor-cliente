using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;

namespace AsteroidesCliente.Modelos
{
    public class AsteroideRede
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("posicao")]
        public Vector2 Posicao { get; set; }
        
        [JsonPropertyName("velocidade")]
        public Vector2 Velocidade { get; set; }
        
        [JsonPropertyName("raio")]
        public float Raio { get; set; }
    }
}