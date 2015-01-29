﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TwinStick
{
    class EnemyManager
    {
        public List<Zombie> Enemies { get; private set;}
        private Texture2D zombieTexture;

        List<Vector2> spawnPoints;
        float enemySpawnElapsed;
        float enemySpawnRate;
        float enemySpeed;

        Vector2 scale;

        Rectangle virtualScreenRect;

        public EnemyManager(Texture2D zombieTexture, Rectangle virtualScreenRect, Vector2 scale)
        {
            Enemies = new List<Zombie>();
            this.zombieTexture = zombieTexture;
            enemySpawnElapsed = 0;
            enemySpawnRate = 3.10f;
            enemySpeed = 20f;
            this.virtualScreenRect = virtualScreenRect;
            this.scale = scale;

            InitSpawnPoints();
        }

        public void Update(GameTime gameTime, Player player, TileMap tileMap, Victim victim)
        {
            SpawnEnemies(gameTime);

            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemies[i].Update(gameTime, tileMap, player, victim);
            }

            ResolveEnemyCollision();

            CleanupSprites();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draw all enemies over player
            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemies[i].Draw(spriteBatch);
            }
        }

        private void InitSpawnPoints()
        {
            // Initialize spawn points
            spawnPoints = new List<Vector2>();
            float centerY = (virtualScreenRect.Height / 2.0f) - (zombieTexture.Height / 2.0f);
            float centerX = (virtualScreenRect.Width / 2.0f) - (zombieTexture.Width / 2.0f);
            spawnPoints.Add(new Vector2(0 - (zombieTexture.Width / 2.0f), centerY));
            spawnPoints.Add(new Vector2(virtualScreenRect.Width + (zombieTexture.Width / 2.0f), centerY));
            spawnPoints.Add(new Vector2(centerX, 0 - (zombieTexture.Height / 2.0f)));
            spawnPoints.Add(new Vector2(centerX, virtualScreenRect.Height + (zombieTexture.Height / 2.0f)));
        }

        // spawn an enemy at each point in spawnPoints at spawnRate
        private void SpawnEnemies(GameTime gameTime)
        {
            enemySpawnElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (enemySpawnElapsed > enemySpawnRate)
            {
                foreach (Vector2 spawnPoint in spawnPoints)
                {
                    Enemies.Add(new Zombie(zombieTexture, spawnPoint, enemySpeed, scale));
                }
                // reset timer
                enemySpawnElapsed = 0;
            }
        }

        // Check for and resolve collisions between enemies
        // if two enemies collide, push them back in opposite directions
        private void ResolveEnemyCollision()
        {
            for (int i = 0; i < Enemies.Count; i++)
            {
                for (int j = i + 1; j < Enemies.Count; j++)
                {
                    Zombie zombie1 = Enemies[i] as Zombie;
                    Zombie zombie2 = Enemies[j] as Zombie;

                    // Check if enemies are colliding and get the depth of collision
                    float depth = zombie1.CollisionCircle.GetIntersectionDepth(zombie2.CollisionCircle);

                    // Collision
                    if (depth != 0)
                    {
                        // Get direction to move first zombie away from second zombie
                        Vector2 direction = zombie1.CollisionCircle.Position - zombie2.CollisionCircle.Position;
                        direction.Normalize();

                        // Move first zombie away half the depth of collision
                        zombie1.Position += direction * (depth / 2.0f);
                        // Move second zombie in the opposite direction half the depth of collision
                        zombie2.Position -= direction * (depth / 2.0f);
                    }
                }
            }
        }

        public void Kill(int index)
        {
            Enemies[index].IsAlive = false;
        }

        // Remove all inactive sprites from bullets and enemies
        private void CleanupSprites()
        {
            // Remove inactive enemies
            for (int i = 0; i < Enemies.Count; i++)
            {
                if (!Enemies[i].IsAlive)
                {
                    Enemies.Remove(Enemies[i]);
                }
            }
        }
    }
}
