using Microsoft.Xna.Framework;
using Monogame.Processing;

namespace AsteroidesCliente.Game
{
    /// <summary>
    /// Classe Asteroide adaptada do código original para multiplayer
    /// </summary>
    public class Asteroide
    {
        public Vector2 Posicao { get; set; }
        public Vector2 Velocidade { get; set; }
        public float Raio { get; set; }
        public int Id { get; set; }

        public Asteroide(Vector2 pos, Vector2 vel, float raio, int id = 0)
        {
            Posicao = pos;
            Velocidade = vel;
            Raio = raio;
            Id = id;
        }

        /// <summary>
        /// Atualiza posição do asteroide (usado localmente para smoothing)
        /// </summary>
        public void Atualizar()
        {
            Posicao += Velocidade;
        }

        /// <summary>
        /// Desenha o asteroide - mantém visual do original
        /// </summary>
        public void Desenhar(Processing g)
        {
            g.fill(150, 100, 100);
            g.stroke(200);
            g.ellipse(Posicao.X, Posicao.Y, Raio * 2, Raio * 2);
        }

        /// <summary>
        /// Verifica colisão com tiro (não usado no cliente, mas mantido para compatibilidade)
        /// </summary>
        public bool Colide(Tiro t)
        {
            return Vector2.Distance(t.Posicao, Posicao) < Raio;
        }

        /// <summary>
        /// Verifica colisão com nave (não usado no cliente, mas mantido para compatibilidade)
        /// </summary>
        public bool Colide(Nave n)
        {
            return Vector2.Distance(n.Posicao, Posicao) < Raio + 8;
        }
        
        /// <summary>
        /// Verifica se o asteroide saiu da tela
        /// </summary>
        public bool ForaDaTela(int altura)
        {
            return Posicao.Y > altura + 50;
        }
    }
}