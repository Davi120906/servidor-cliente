using Microsoft.Xna.Framework;
using Monogame.Processing;

namespace AsteroidesCliente.Game
{
    /// <summary>
    /// Classe Tiro adaptada do código original para multiplayer
    /// </summary>
    public class Tiro
    {
        public Vector2 Posicao { get; set; }
        public Vector2 Velocidade { get; set; }
        public int JogadorId { get; set; }
        public int Id { get; set; }
        
        public Tiro(Vector2 pos, Vector2 vel, int jogadorId, int id = 0)
        {
            Posicao = pos;
            Velocidade = vel;
            JogadorId = jogadorId;
            Id = id;
        }

        /// <summary>
        /// Atualiza posição do tiro (usado localmente para smoothing)
        /// </summary>
        public void Atualizar()
        {
            Posicao += Velocidade;
        }

        /// <summary>
        /// Desenha o tiro com cor baseada no jogador
        /// </summary>
        public void Desenhar(Processing g, bool ehMeu, int meuId)
        {
            g.strokeWeight(5);
            
            if (ehMeu || JogadorId == meuId)
            {
                // Meus tiros - amarelo brilhante
                g.stroke(255, 255, 0);
            }
            else
            {
                // Tiros de outros jogadores - cor diferente
                g.stroke(0, 150, 255);
            }
            
            g.point(Posicao.X, Posicao.Y);
            g.strokeWeight(1);
        }

        /// <summary>
        /// Verifica se o tiro saiu da tela
        /// </summary>
        public bool ForaDaTela(int altura)
        {
            return Posicao.Y < -5 || Posicao.Y > altura + 5;
        }
    }
}