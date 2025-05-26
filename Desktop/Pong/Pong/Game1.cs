using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MiniAventura
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _playerTexture, _enemyTexture, _bulletTexture, _backgroundTexture, _weaponTexture;
        private Vector2 _playerPosition;
        private float _playerSpeed = 200f;

        private List<Vector2> _enemyPositions;
        private List<Vector2> _enemySizes; // Tamanhos variáveis dos inimigos
        private List<Color> _enemyColors; // Cores aleatórias
        private float _enemySpeed = 120f; // Velocidade dos inimigos aumentada para 120

        private List<Vector2> _bullets;
        private float _bulletSpeed = 300f;

        private SpriteFont _font;
        private int _score = 0;
        private int _highScore = 0;
        private Random _random = new Random();
        private bool _prevSpace = false;

        private Vector2 _cameraPosition;

        // Variáveis do sistema de Game Over
        private bool _gameOver = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _playerPosition = new Vector2(0, 0);
            _enemyPositions = new List<Vector2>();
            _enemySizes = new List<Vector2>();
            _enemyColors = new List<Color>();
            _bullets = new List<Vector2>();
            SpawnEnemies(5); // Cria 5 inimigos iniciais
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Textura simples para o jogador
            _playerTexture = new Texture2D(GraphicsDevice, 1, 1);
            _playerTexture.SetData(new[] { Color.Red });

            // Textura para os inimigos
            _enemyTexture = new Texture2D(GraphicsDevice, 1, 1);
            _enemyTexture.SetData(new[] { Color.White });

            // Textura para as balas
            _bulletTexture = new Texture2D(GraphicsDevice, 1, 1);
            _bulletTexture.SetData(new[] { Color.Yellow });

            // Carregar imagens do background e da arma do jogador (adicionar no Content)
            _backgroundTexture = Content.Load<Texture2D>("background");
            _weaponTexture = Content.Load<Texture2D>("weapon");

            // Carregar fonte para o texto
            _font = Content.Load<SpriteFont>("DefaultFont");
        }

        // Método para criar inimigos com posições, cores (sem verde) e tamanhos aleatórios
        private void SpawnEnemies(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _enemyPositions.Add(new Vector2(_random.Next(-1000, 1000), _random.Next(-1000, 1000)));

                // Gera cores aleatórias e remove tons de verde
                Color enemyColor;
                do
                {
                    enemyColor = new Color(_random.Next(256), _random.Next(256), _random.Next(256));
                }
                while (enemyColor.G > enemyColor.R && enemyColor.G > enemyColor.B); // Se verde for dominante, tenta outra cor

                _enemyColors.Add(enemyColor);

                float size = _random.Next(20, 60); // Tamanho variável entre 20 e 60
                _enemySizes.Add(new Vector2(size, size));
            }
        }


        protected override void Update(GameTime gameTime)
        {
            var k = Keyboard.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_gameOver)
            {
                // Se estiver em Game Over, espera pela tecla Espaço para reiniciar
                if (k.IsKeyDown(Keys.Space))
                {
                    _gameOver = false;
                    _playerPosition = new Vector2(0, 0);
                    _enemyPositions.Clear();
                    _enemyColors.Clear();
                    _enemySizes.Clear();
                    SpawnEnemies(5);
                    _score = 0;
                }
                return; // Sai do Update para não continuar a lógica do jogo
            }

            // Movimento do jogador com as teclas WASD
            if (k.IsKeyDown(Keys.W)) _playerPosition.Y -= _playerSpeed * dt;
            if (k.IsKeyDown(Keys.S)) _playerPosition.Y += _playerSpeed * dt;
            if (k.IsKeyDown(Keys.A)) _playerPosition.X -= _playerSpeed * dt;
            if (k.IsKeyDown(Keys.D)) _playerPosition.X += _playerSpeed * dt;

            // Disparo de balas com a barra de espaço
            if (k.IsKeyDown(Keys.Space) && !_prevSpace)
                _bullets.Add(_playerPosition + new Vector2(16, 16));
            _prevSpace = k.IsKeyDown(Keys.Space);

            // Atualiza a posição da câmera
            _cameraPosition = _playerPosition - new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);

            // Movimento dos inimigos em direção ao jogador
            for (int i = 0; i < _enemyPositions.Count; i++)
            {
                Vector2 dir = _playerPosition - _enemyPositions[i];
                if (dir.Length() > 1)
                {
                    dir.Normalize();
                    _enemyPositions[i] += dir * _enemySpeed * dt;
                }
            }

            // Movimento das balas
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                _bullets[i] += new Vector2(1, 0) * _bulletSpeed * dt;
                if ((_bullets[i] - _playerPosition).Length() > 1000)
                    _bullets.RemoveAt(i);
            }

            Rectangle playerRect = new Rectangle((int)_playerPosition.X, (int)_playerPosition.Y, 32, 32);

            // Colisão do jogador com inimigos
            for (int i = _enemyPositions.Count - 1; i >= 0; i--)
            {
                Rectangle enemyRect = new Rectangle((int)_enemyPositions[i].X, (int)_enemyPositions[i].Y, (int)_enemySizes[i].X, (int)_enemySizes[i].Y);
                if (playerRect.Intersects(enemyRect))
                {
                    // Ativa o Game Over
                    _gameOver = true;
                    _highScore = Math.Max(_score, _highScore);
                }

                // Colisão das balas com os inimigos
                for (int j = _bullets.Count - 1; j >= 0; j--)
                {
                    Rectangle bulletRect = new Rectangle((int)_bullets[j].X, (int)_bullets[j].Y, 10, 5);
                    if (bulletRect.Intersects(enemyRect))
                    {
                        _enemyPositions.RemoveAt(i);
                        _enemyColors.RemoveAt(i);
                        _enemySizes.RemoveAt(i);
                        _bullets.RemoveAt(j);
                        _score += 10;
                        SpawnEnemies(1); // Reposição do inimigo morto
                        break;
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (_gameOver)
            {
                // Tela preta de Game Over
                GraphicsDevice.Clear(Color.Black);
                _spriteBatch.Begin();
                _spriteBatch.DrawString(_font, "GAME OVER", new Vector2(_graphics.PreferredBackBufferWidth / 2 - 100, _graphics.PreferredBackBufferHeight / 2 - 20), Color.White);
                _spriteBatch.DrawString(_font, "Pressione ESPACO para jogar novamente", new Vector2(_graphics.PreferredBackBufferWidth / 2 - 200, _graphics.PreferredBackBufferHeight / 2 + 20), Color.White);
                _spriteBatch.End();
                return;
            }


            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            // Desenha o background (move-se suavemente com a câmera)
            float bgScrollFactor = 0.2f;
            Vector2 bgOffset = _cameraPosition * bgScrollFactor;
            Rectangle bgRect = new Rectangle((int)-bgOffset.X, (int)-bgOffset.Y, _backgroundTexture.Width, _backgroundTexture.Height);
            _spriteBatch.Draw(_backgroundTexture, bgRect, Color.White);
            _spriteBatch.End();

            // Início do desenho do mundo com deslocamento da câmera
            Matrix transform = Matrix.CreateTranslation(-_cameraPosition.X, -_cameraPosition.Y, 0);
            _spriteBatch.Begin(transformMatrix: transform);

            // Desenha o jogador (cubo vermelho)
            _spriteBatch.Draw(_playerTexture, new Rectangle((int)_playerPosition.X, (int)_playerPosition.Y, 32, 32), Color.White);

            // Desenha a arma sobre o jogador
            _spriteBatch.Draw(_weaponTexture, new Rectangle((int)_playerPosition.X, (int)_playerPosition.Y, 32, 32), Color.White);

            // Desenha os inimigos com cores e tamanhos variáveis
            for (int i = 0; i < _enemyPositions.Count; i++)
            {
                _spriteBatch.Draw(_enemyTexture, new Rectangle((int)_enemyPositions[i].X, (int)_enemyPositions[i].Y, (int)_enemySizes[i].X, (int)_enemySizes[i].Y), _enemyColors[i]);
            }

            // Desenha as balas
            foreach (var bullet in _bullets)
                _spriteBatch.Draw(_bulletTexture, new Rectangle((int)bullet.X, (int)bullet.Y, 10, 5), Color.White);

            _spriteBatch.End();

            // HUD com score e highscore
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, $"Score: {_score}", new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(_font, $"HighScore: {_highScore}", new Vector2(10, 40), Color.Yellow);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
