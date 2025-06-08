using System;

using HowlEngine.Graphics;
using HowlEngine.Physics;
using HowlEngine.Audio;
using HowlEngine.AssetManagement;
using HowlEngine.Collections;
using HowlEngine.SceneManagement;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using DungeonSlime.Entities;
using System.Numerics;
using HowlEngine.Core.Input;

namespace DungeonSlime;

public class Game : HowlEngine.Core.HowlApp{
    private AssetManager _assetManager;

    private List<Token> spawnedBats = new List<Token>();
    private List<Token> spawnedCircleColliders = new List<Token>();

    private List<HowlEngine.Collections.Shapes.Circle> copiedCircles = new List<HowlEngine.Collections.Shapes.Circle>();
    private List<Vector2[]> copiedBoxesRigids = new List<Vector2[]>();
    private List<Vector2[]> copiedBoxesKinematics = new List<Vector2[]>();

    public Game() : base("Dungeon Slime", 1920, 1080, false, true){}

    protected override void Initialize(){
        base.Initialize();


        DisableVSync();
        SetFrameRate(1000);
        Random r = new Random();
        PhysicsSystem = new PhysicsSystem(200,200,200,200, true);

        _assetManager = new AssetManager(typeof(Game).Assembly);

        AudioManager = new AudioManager(_assetManager);

        EntityManager = new EntityManager(2000);

        CameraManager = new CameraManager(
            new Camera(
                new Vector2(320, 180), 
                Vector2.Zero, 
                new Vector2(640,360),
                new Vector2(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight)),
            new Vector2(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight), 
            0 
        );

        SpriteRenderer = new SpriteRenderer(
            _assetManager,
            GraphicsDevice,
            new List<string>(){
            },
            2000,
            2000
        );

        SceneManager = new SceneManager(
            _assetManager,
            PhysicsSystem,
            SpriteRenderer
        );

        SceneManager.LoadScene("Level1.json");

        AudioManager.LoadBank("Bank A");


    }
    
    protected override void Update(Microsoft.Xna.Framework.GameTime gameTime){
        base.Update(gameTime);

        // Console.WriteLine(collisionChecker.AABB(slimeBox, batBox));
        CameraManager.Update(DeltaTime);
        SpawnEntityAtMouse();
        if(Input.Keyboard.IsKeyJustPressed(Keys.P)){
            CameraManager.MultiplyZoom(0, 1.25f);
        }
        if(Input.Keyboard.IsKeyJustPressed(Keys.O)){
            CameraManager.MultiplyZoom(0, 0.85f);
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
            SpawnCircleColliders(10);
        }
        if(Input.Keyboard.IsKeyJustPressed(Keys.M)){
            DeleteCircleColliders(10);
        }
        SpriteRenderer.Update(DeltaTime);
        EntityManager.Update(DeltaTime);
    
        copiedCircles = PhysicsSystem.CopyCircleRigidBodyColliders();
        copiedBoxesRigids = PhysicsSystem.CopyPolygonRigidBodyColliders();
        copiedBoxesKinematics = PhysicsSystem.CopyPolygonKinematicColliders();
    }

    protected override void FixedUpdate(Microsoft.Xna.Framework.GameTime gameTime){
        base.FixedUpdate(gameTime);
        // TODO: Add your update logic here     
        float cameraSpeed = 6;
        Vector2 cameraPositionAdditive = Vector2.Zero;
        if(Input.Keyboard.IsKeyDown(Keys.Left)){
            cameraPositionAdditive.X -= cameraSpeed;
        }
        if(Input.Keyboard.IsKeyDown(Keys.Right)){
            cameraPositionAdditive.X += cameraSpeed;
        }
        if(Input.Keyboard.IsKeyDown(Keys.Up)){
            cameraPositionAdditive.Y -= cameraSpeed;
        }
        if(Input.Keyboard.IsKeyDown(Keys.Down)){
            cameraPositionAdditive.Y += cameraSpeed;
        }
        PhysicsSystem.FixedUpdate(DeltaTime, 15);
        EntityManager.FixedUpdate(DeltaTime);
        CameraManager.AddPosition(0, cameraPositionAdditive);
    }


    protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime){
        GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.DimGray);

        // Begin the sprite batch to preapre for rendering.
        SpriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, CameraManager.GetMainCamera().ViewMatrix);

        SpriteRenderer.DrawAll(SpriteBatch);

        // Console.WriteLine(copiedCircles.Count);

        for(int i = 0; i < copiedCircles.Count; i++){
            // Console.WriteLine(copiedBoxes[i].X);
            
            SpriteRenderer.DrawCircle(
                SpriteBatch,
                copiedCircles[i],
                System.Drawing.Color.LightGreen, 
                1,
                1,
                32
            );
        }

        for(int i = 0; i < copiedBoxesRigids.Count; i++){
            SpriteRenderer.DrawPolygon(
                SpriteBatch,
                copiedBoxesRigids[i],
                System.Drawing.Color.White,
                1,
                1
            );
        }

        for(int i = 0; i < copiedBoxesKinematics.Count; i++){
            SpriteRenderer.DrawPolygon(
                SpriteBatch,
                copiedBoxesKinematics[i],
                System.Drawing.Color.Orange,
                1,
                1
            );
        }

        SpriteBatch.End();

        // Always end the sprite batch when finished.


        // TODO: Add your drawing code here
        base.Draw(gameTime);        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Window.Title = "DungeonSlime [FPS]: " + (1.0f / deltaTime).ToString();
    }

    protected override void OnExiting(object sender, Microsoft.Xna.Framework.ExitingEventArgs args){
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

    void SpawnCircleColliders(int amount){
        Random r = new Random();
        for(int i = 0; i < amount; i++){
            Token token = PhysicsSystem.AllocateCircleRigidBody(
                    new Vector2(r.Next(0,640), r.Next(0,360)), 
                    r.Next(1,20), 
                    r.Next(1,20), 
                    r.Next(0,1)
            );
            spawnedCircleColliders.Add(token);
        }
    }

    void DeleteCircleColliders(int amount){
        Token[] tokens = spawnedCircleColliders.ToArray();
        for(int i = 0; i < amount; i++){
            PhysicsSystem.FreeCircleRigidBody(ref tokens[i]);
            spawnedCircleColliders.Remove(tokens[i]);
        }
    }

    void SpawnEntityAtMouse(){
        if(Input.Mouse.IsButtonJustPressed(MouseButton.Left)){
            Console.WriteLine(1);
            Matrix4x4.Invert(CameraManager.GetMainCamera().ViewMatrix, out Matrix4x4 toWorldPosition);
            BouncyBall ball = new BouncyBall(
                Vector2.Transform(new Vector2(Input.Mouse.X, Input.Mouse.Y), toWorldPosition)
            );
        }

        if(Input.Mouse.IsButtonJustPressed(MouseButton.Right)){
            Console.WriteLine(1);
            Matrix4x4.Invert(CameraManager.GetMainCamera().ViewMatrix, out Matrix4x4 toWorldPosition);
            SolidSquare square = new SolidSquare(
                Vector2.Transform(new Vector2(Input.Mouse.X, Input.Mouse.Y), toWorldPosition)
            );
        }
    }
}
