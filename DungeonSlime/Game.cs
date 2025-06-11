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
using FontStashSharp;
using HowlEngine.Collections.Shapes;

namespace DungeonSlime;

public class Game : HowlEngine.Core.HowlApp{
    private AssetManager _assetManager;
    private FontSystem fontSystem;

    private List<Token> spawnedBats = new List<Token>();
    private List<Token> spawnedCircleColliders = new List<Token>();

    private List<HowlEngine.Collections.Shapes.Circle> copiedCircles = new List<HowlEngine.Collections.Shapes.Circle>();
    private List<Polygon> copiedBoxesRigids = new List<Polygon>();
    private List<Polygon> copiedBoxesKinematics = new List<Polygon>();

    public Game() : base("Dungeon Slime", 1920, 1080, false, true){}

    protected override void Initialize(){
        base.Initialize();


        DisableVSync();
        SetFrameRate(1000);
        Random r = new Random();

        fontSystem = new FontSystem();
        fontSystem.AddFont(System.IO.File.ReadAllBytes(@"Assets/Fonts/DroidSans.ttf"));
        fontSystem.AddFont(System.IO.File.ReadAllBytes(@"Assets/Fonts/DroidSansJapanese.ttf"));

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
        SpawnEntityAtMouse(5);
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

        DebugDrawColliders();

        SpriteBatch.End();

        SpriteBatch.Begin();
        SpriteFontBase font = fontSystem.GetFont(24);
        SpriteBatch.DrawString(
            font,
            "[Movement] step time (ms): \n" + PhysicsSystem.MovementStepTimer.Elapsed+"\n"+
            "[Collision] step time (ms): \n" + PhysicsSystem.CollisionTimer.Elapsed+"\n"+
            "[Response] step time (ms): \n" + PhysicsSystem.ResponseTimer.Elapsed+"\n"+
            "[Totlal] step time (ms): \n" + PhysicsSystem.StepCallTimer.Elapsed,
            new Vector2(0, Graphics.PreferredBackBufferHeight - 200), 
            Microsoft.Xna.Framework.Color.White,
            effect: FontSystemEffect.Stroked,
            effectAmount: 2
        );
        SpriteBatch.End();


        // Always end the sprite batch when finished.


        // TODO: Add your drawing code here
        base.Draw(gameTime);        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Window.Title = "DungeonSlime [FPS]: " + (1.0f / deltaTime).ToString();
    }

    protected override void OnExiting(object sender, Microsoft.Xna.Framework.ExitingEventArgs args){
        AudioManager.Dispose();
        fontSystem.Dispose();
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

    void SpawnEntityAtMouse(int amount){
        if(Input.Mouse.IsButtonJustPressed(MouseButton.Left)){
            Matrix4x4.Invert(CameraManager.GetMainCamera().ViewMatrix, out Matrix4x4 toWorldPosition);
            for(int i = 0; i < amount; i++){
                BouncyBall ball = new BouncyBall(
                    Vector2.Transform(new Vector2(Input.Mouse.X, Input.Mouse.Y), toWorldPosition)
                );
            }
        }

        if(Input.Mouse.IsButtonJustPressed(MouseButton.Right)){
            Matrix4x4.Invert(CameraManager.GetMainCamera().ViewMatrix, out Matrix4x4 toWorldPosition);
            for(int i = 0; i < amount; i++){
                SolidSquare square = new SolidSquare(
                    Vector2.Transform(new Vector2(Input.Mouse.X, Input.Mouse.Y), toWorldPosition)
                );
            }
        }
    }

    void DebugDrawColliders(){
        for(int i = 0; i < copiedCircles.Count; i++){            
            SpriteRenderer.DrawCircle(
                SpriteBatch,
                copiedCircles[i],
                System.Drawing.Color.LightGreen, 
                1,
                1,
                32
            );


            SpriteRenderer.DrawPolygon(
                SpriteBatch,
                [
                    copiedCircles[i].Min,
                    new Vector2(copiedCircles[i].Max.X, copiedCircles[i].Min.Y),
                    copiedCircles[i].Max,
                    new Vector2(copiedCircles[i].Min.X, copiedCircles[i].Max.Y),
                ],
                System.Drawing.Color.Blue,
                1,
                0.25f                
            );
        }

        for(int i = 0; i < copiedBoxesRigids.Count; i++){
            SpriteRenderer.DrawPolygon(
                SpriteBatch,
                copiedBoxesRigids[i].Vertices,
                System.Drawing.Color.White,
                1,
                0.5f
            );

            SpriteRenderer.DrawPolygon(
                SpriteBatch,
                [
                    copiedBoxesRigids[i].Min,
                    new Vector2(copiedBoxesRigids[i].Max.X, copiedBoxesRigids[i].Min.Y),
                    copiedBoxesRigids[i].Max,
                    new Vector2(copiedBoxesRigids[i].Min.X, copiedBoxesRigids[i].Max.Y),
                ],
                System.Drawing.Color.Blue,
                1,
                0.25f
            );
        }

        for(int i = 0; i < copiedBoxesKinematics.Count; i++){
            SpriteRenderer.DrawPolygon(
                SpriteBatch,
                copiedBoxesKinematics[i].Vertices,
                System.Drawing.Color.Orange,
                1,
                1
            );
        }

        for(int i = 0; i < PhysicsSystem.ContactPoints.Count; i++){
            Vector2 point = PhysicsSystem.ContactPoints[i];
            
            SpriteRenderer.DrawPolygon(
                SpriteBatch,
                new Vector2[]{
                    new Vector2(-1+point.X, -1+point.Y),
                    new Vector2(1+point.X, -1+point.Y),
                    new Vector2(1+point.X, 1+point.Y),
                    new Vector2(-1+point.X, 1+point.Y),
                },
                System.Drawing.Color.Red,
                1,
                1
            );
        }

        for(int x = 0; x < PhysicsSystem.SpatialHash.Columns; x++){
            for(int y = 0; y < PhysicsSystem.SpatialHash.Rows; y++){
                float sizeX = PhysicsSystem.SpatialHash.CellSize.X;
                float sizeY = PhysicsSystem.SpatialHash.CellSize.Y;
                
                // polygon data layer.

                SpriteRenderer.DrawPolygon(
                    SpriteBatch,
                    new Vector2[]{
                        new Vector2(x*sizeX, y*sizeY),
                        new Vector2((1+x)*sizeX, y*sizeY),
                        new Vector2((1+x)*sizeX, (1+y)*sizeY),
                        new Vector2(x*sizeX, (1+y)*sizeY),
                    },
                    System.Drawing.Color.LightPink,
                    1,
                    PhysicsSystem.SpatialHash.Cells[y * PhysicsSystem.SpatialHash.Columns + x].Count > 0 ? 0.25f : 0.00f
                );

                // circle data layer.

                SpriteRenderer.DrawPolygon(
                    SpriteBatch,
                    new Vector2[]{
                        new Vector2(x*sizeX, y*sizeY),
                        new Vector2((1+x)*sizeX, y*sizeY),
                        new Vector2((1+x)*sizeX, (1+y)*sizeY),
                        new Vector2(x*sizeX, (1+y)*sizeY),
                    },
                    System.Drawing.Color.LightGreen,
                    1,
                    PhysicsSystem.SpatialHash.Cells[y * PhysicsSystem.SpatialHash.Columns + x].Count > 0 ? 0.25f : 0.00f
                );
            }
        }
    }

}
