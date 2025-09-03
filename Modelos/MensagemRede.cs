using System.Text.Json.Serialization;

namespace AsteroidesCliente.Modelos
{
    public class MensagemRede
    {
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = "";
        
        [JsonPropertyName("dados")]
        public string Dados { get; set; } = "";
        
        [JsonPropertyName("jogadorId")]
        public int JogadorId { get; set; }
        
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}