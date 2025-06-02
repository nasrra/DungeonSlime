using System.Diagnostics;
using System;

using HowlEngine;
using HowlEngine.Graphics;
using HowlEngine.Physics;
using HowlEngine.Audio;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using HowlEngine.Collections;
using DungeonSlime.Entities;
using HowlEngine.SceneManagement;
using HowlEngine.Helpers;

namespace DungeonSlime;

public class Game : HowlApp{
    private Camera camera;

    private List<Token> spawnedBats = new List<Token>();

    public Game() : base("Dungeon Slime", 1920, 1080, false, true){
        
    }

    protected override void Initialize(){
        base.Initialize();

        DisableVSync();
        SetFrameRate(1000);
        Random r = new Random();

        camera = new Camera(new Vector2(320, 180), Vector2.Zero, new Vector2(640,360));

        EntityManager = new EntityManager(2000);

        PhysicsSystem = new AABBPhysicSystem(0, 2000);

        SpriteRenderer = new SpriteRenderer(
            new List<string>(){
            },
            2000,
            2000
        );

        TypeFactory = new TypeFactory(typeof(Game).Assembly);

        SceneManager = new SceneManager();

        SceneManager.LoadScene("Level1.json");
    }
    
    protected override void LoadContent(){        
        base.LoadContent();
        AudioManager.LoadBank("Bank A");
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime){
        base.Update(gameTime);
        // Console.WriteLine(DeltaTime);


        // Console.WriteLine(collisionChecker.AABB(slimeBox, batBox));
        camera.Update(gameTime);
        if(Input.Keyboard.IsKeyJustPressed(Keys.P)){
            camera.MultiplyZoom(1.25f);
        }
        if(Input.Keyboard.IsKeyJustPressed(Keys.O)){
            camera.MultiplyZoom(0.85f);
        }
        if(Input.Keyboard.IsKeyJustPressed(Keys.Tab)){
            AudioManager.PlayOneShot("Test");
        }
        if(Input.Keyboard.IsKeyJustPressed(Keys.L)){
            float vol = AudioManager.GetBusVolume("Master");
            AudioManager.SetBusVolume("Master", vol-=0.1f);
        }
        if(Input.Keyboard.IsKeyJustPressed(Keys.K)){
            float vol = AudioManager.GetBusVolume("Master");
            AudioManager.SetBusVolume("Master", vol+=0.1f);
        }
        if(Input.Keyboard.IsKeyJustPressed(Keys.N)){
            SpawnBats(100);
        }
        if(Input.Keyboard.IsKeyJustPressed(Keys.M)){
            DeleteBats(100);
        }
        SpriteRenderer.Update(gameTime);
        EntityManager.Update(gameTime);
    }

    protected override void FixedUpdate(GameTime gameTime){
        base.FixedUpdate(gameTime);
        // TODO: Add your update logic here     
        float cameraSpeed = 6;
        if(Input.Keyboard.IsKeyDown(Keys.Left)){
            camera.Position.X -= cameraSpeed;
        }
        if(Input.Keyboard.IsKeyDown(Keys.Right)){
            camera.Position.X += cameraSpeed;
        }
        if(Input.Keyboard.IsKeyDown(Keys.Up)){
            camera.Position.Y -= cameraSpeed;
        }
        if(Input.Keyboard.IsKeyDown(Keys.Down)){
            camera.Position.Y += cameraSpeed;
        }
        PhysicsSystem.FixedUpdate(gameTime);
        EntityManager.FixedUpdate(gameTime);
    }


    protected override void Draw(GameTime gameTime){
        GraphicsDevice.Clear(Color.DimGray);


        // Begin the sprite batch to preapre for rendering.
        SpriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, camera.ViewMatrix);
        SpriteRenderer.DrawAll(SpriteBatch);
        SpriteBatch.End();


        SpriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, camera.ViewMatrix);
        PhysicsSystem.DrawAllOutlines(SpriteBatch, Color.White * 0.5f, 1);
        SpriteBatch.End();

        // Always end the sprite batch when finished.


        // TODO: Add your drawing code here
        base.Draw(gameTime);        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Window.Title = "DungeonSlime [FPS]: " + (1.0f / deltaTime).ToString();
    }

    protected override void OnExiting(object sender, ExitingEventArgs args){
        AudioManager.Dispose();
        base.OnExiting(sender, args);
    }

    void SpawnBats(int amount){
        Random r = new Random();
        for(int i = 0; i < amount; i++){

            Bat bat = new Bat(
                new Vector2(r.Next(0,640), r.Next(0,360))
            );
            spawnedBats.Add(bat.Id);
        }
    }

    void DeleteBats(int amount){
        Token[] tokens = spawnedBats.ToArray();
        if(tokens.Length >= amount){
            for(int i = 0; i < amount; i++){
                EntityManager.DisposeAt(ref tokens[i]);
                spawnedBats.Remove(tokens[i]);
            }
        }
    }
}
