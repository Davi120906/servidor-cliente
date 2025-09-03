using Microsoft.Xna.Framework;
using Monogame.Processing;

namespace AsteroidesCliente.Game
{
    /// <summary>
    /// Classe Nave adaptada do código original para multiplayer
    /// </summary>
    public class Nave
    {
        public Vector2 Posicao;
        private const float Vel = 4f;
        private const float HalfW = 10, HalfH = 10;
        
        // Cores para diferentes jogadores
        private readonly Color _cor;
        private readonly bool _ehMinhanave;

        public Nave(Vector2 start, Color cor, bool ehMinhanave = false)
        {
            Posicao = start;
            _cor = cor;
            _ehMinhanave = ehMinhanave;
        }

        /// <summary>
        /// Atualiza posição da nave (apenas para nave local)
        /// </summary>
        public Vector2 Atualizar(bool left, bool right, bool up, bool down, int w, int h)
        {
            if (!_ehMinharave) return Posicao; // Apenas nave local se move
            
            Vector2 dir = Vector2.Zero;
            if (left) dir.X -= 2;
            if (right) dir.X += 2;
            if (up) dir.Y -= 2;
            if (down) dir.Y += 2;

            if (dir != Vector2.Zero) dir.Normalize();
            Posicao += dir * Vel;

            // Mantém dentro da tela
            Posicao.X = Math.Clamp(Posicao.X, HalfW, w - HalfW);
            Posicao.Y = Math.Clamp(Posicao.Y, HalfH, h - HalfH);
            
            return Posicao;
        }

        /// <summary>
        /// Atualiza posição da nave remota (vem da rede)
        /// </summary>
        public void AtualizarPosicaoRede(Vector2 novaPosicao)
        {
            if (!_ehMinharave)
            {
                Posicao = novaPosicao;
            }
        }

        /// <summary>
        /// Desenha a nave com cor específica
        /// </summary>
        public void Desenhar(Processing g, bool ativa = true)
        {
            if (!ativa)
            {
                // Nave destruída - cor vermelha
                g.stroke(255, 0, 0);
                g.fill(100, 0, 0);
            }
            else if (_ehMinharave)
            {
                // Minha nave - destaque verde
                g.stroke(0, 255, 0);
                g.fill(100, 255, 100);
            }
            else
            {
                // Nave de outro jogador - cor personalizada
                g.stroke((int)(_cor.R * 255), (int)(_cor.G * 255), (int)(_cor.B * 255));
                g.fill((int)(_cor.R * 150), (int)(_cor.G * 150), (int)(_cor.B * 150));
            }
            
            // Desenha triângulo da nave
            g.triangle(Posicao.X - 8, Posicao.Y + 10,
                      Posicao.X, Posicao.Y - 10,
                      Posicao.X + 8, Posicao.Y + 10);
                      
            // Desenha indicador se é minha nave
            if (_ehMinharave)
            {
                g.stroke(255, 255, 0);
                g.noFill();
                g.ellipse(Posicao.X, Posicao.Y, 30, 30);
            }
        }

        /// <summary>
        /// Cria um tiro da posição atual da nave
        /// </summary>
        public Vector2 PosicaoTiro()
        {
            return Posicao + new Vector2(0, -12);
        }
    }
}