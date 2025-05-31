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
using HowlEngine.ECS;
using HowlEngine.Collections;
using System.IO;

namespace DungeonSlime;

public class Game : HowlApp{
    private AABBPhysicSystem physics;
    private SpriteRenderer spriteRenderer;
    private HowlEngine.SceneManagement.Config.SceneData sceneData;
    
    private Token slimeSprite;
    private Token slimeBody;
    private Vector2 slimePos = new Vector2(200,200);

    // private RectangleCollider batBox;
    private static int batAmount = 10;
    private Token[] batBoxes = new Token[batAmount];
    private Token[] batSprites = new Token[batAmount];
    private Vector2[] batPositions = new Vector2[batAmount];

    private Camera camera;

    private const string atlasName = "Entities";

    public Game() : base("Dungeon Slime", 1920, 1080, false, true){
        
    }

    protected override void Initialize(){
        base.Initialize();

        DisableVSync();
        SetFrameRate(1000);
        Random r = new Random();

        camera = new Camera(new Vector2(320, 180), Vector2.Zero, new Vector2(640,360));

        physics = new AABBPhysicSystem(0, 1000);
        slimeBody = physics.AllocatePyhsicsBody(new PhysicsBodyAABB(new RectangleColliderStruct((int)slimePos.X,(int)slimePos.Y,20,20)));

        for(int i = 0; i < batAmount; i++){
            Vector2 position = new Vector2(r.Next(0,640), r.Next(0,360));
            batPositions[i] = position;
            batBoxes[i] = physics.AllocatePyhsicsBody(new PhysicsBodyAABB(new RectangleColliderStruct((int)position.X,(int)position.Y,20,20)));
        }


        spriteRenderer = new SpriteRenderer(
            new List<string>(){
                "Entities\\Entities.json"
            },
            1000,
            1000
        );


        slimeSprite = spriteRenderer.AllocateAnimatedSprite(atlasName, "SlimeIdle");
        RefView<AnimatedSprite> slimeSpriteRef = spriteRenderer.GetAnimatedSprite(ref slimeSprite);
        if(slimeSpriteRef.Valid == true){
            slimeSpriteRef.Data.Scale = new Vector2(1.0f,1.0f);
            slimeSpriteRef.Data.Position  = slimePos;
        }

        for(int i = 0; i < batAmount; i++){
            batSprites[i] = spriteRenderer.AllocateAnimatedSprite(atlasName, "BatIdle");
            RefView<AnimatedSprite> batSpriteRef = spriteRenderer.GetAnimatedSprite(ref batSprites[i]);
            if(batSpriteRef.Valid == true){
                batSpriteRef.Data.Scale = new Vector2(1.0f, 1.0f);
                batSpriteRef.Data.Position = batPositions[i];
            }
        }

        // slimeBox = new RectangleCollider((int)slimePos.X, (int)slimePos.Y, 80,80);
        // batBox = new RectangleCollider((int)batPos.X, (int)batPos.Y, 80,80);

        // Any initializations you need to perform that have a dependency on assets being loaded should be done after the base.
        // Initialize call and not before it.
        
        // Read in SceneData.
        
        // Read in TilesetData.
        
        // foreach(TilesetToken token in sceneData.TilesetTokens){
        //     // find find the directory of the tileset data.
        //     string tileSetDataDir = System.IO.Path.Combine(ImagesFileDirectory,token.Source);
        //     string tileSetDataJson = File.ReadAllText(tileSetDataDir);

        // }

        string sceneDataJson = File.ReadAllText(System.IO.Path.Combine(ScenesFileDirectory,"DungeonSlime.json"));
        HowlEngine.SceneManagement.Config.SceneData sceneData = HowlEngine.SceneManagement.Config.SceneData.FromJson(sceneDataJson);

        // load tilesets.
        foreach(HowlEngine.SceneManagement.Config.Tileset token in sceneData.Tilesets){
            spriteRenderer.LoadTilesetData(token);
        }

        for(int i = 0; i < sceneData.LayerGroup.Length; i++){
            HowlEngine.SceneManagement.Config.LayerGroup layerGroup = sceneData.LayerGroup[i];

            // Draw all tile layers.
            if(layerGroup.Name == "TileLayers"){
                for(int j = 0; j < layerGroup.Layers.Length; j++){
                    HowlEngine.SceneManagement.Config.Layer layer = layerGroup.Layers[j];
                    if(layer.Visible == true){
                        spriteRenderer.LoadTileMap(layer.Data, layer.Name, (int)sceneData.Width, (int)sceneData.Height);
                    }
                }
            }

            if(layerGroup.Name == "ObjectLayers"){
                
                for(int j = 0; j < layerGroup.Layers.Length; j++){
                    HowlEngine.SceneManagement.Config.Layer layer = layerGroup.Layers[j];

                    // add all colliders.

                    if(layer.Visible == false){
                        continue;
                    }

                    if(layer.Name == "Collisions"){
                        foreach(HowlEngine.SceneManagement.Config.Object col in layer.Objects){
                            physics.AllocatePyhsicsBody(new PhysicsBodyAABB(new RectangleColliderStruct((int)col.X,(int)col.Y,(int)col.Width,(int)col.Height)));
                        }
                    }
                }
            


            }

        }

        // Layer layer = sceneData.Layers[0];
        // int index = 0;
        // for(long y = 0; y < layer.Height; y++){
        //     for(long x = 0; x < layer.Width; x++){
        //         int tileId = (int)layer.Data[index];
        //         if(tileId > 0){
        //             spriteRenderer.AllocateTileSprite(new Vector2(x*tileset.TileWidth,y*tileset.TileHeight),1,tileId);
        //         }
        //         index++;
        //     }
        // }

        // spriteRenderer.UnloadTilesetData(1);

        // Tileset s = new();

        // tileset = null;  // Remove your local strong reference
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
        if(Input.Keyboard.IsKeyJustPressed(Keys.Q)){
            Random r = new Random();
            physics.AllocatePyhsicsBody(new PhysicsBodyAABB(new RectangleColliderStruct(r.Next(100,1180),r.Next(100,620),r.Next(10,80),r.Next(10,80))));
        }

        if(Input.Keyboard.IsKeyJustPressed(Keys.E)){
            physics.FreeLastPhysicsBody();
        }

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
        spriteRenderer.Update(gameTime);
    }

    protected override void FixedUpdate(GameTime gameTime){
        base.FixedUpdate(gameTime);
        // TODO: Add your update logic here     
        float speed = 5f;
        Vector2 slimeVel = new Vector2();

        if(Input.Keyboard.IsKeyDown(Keys.A)){
            slimeVel.X -= speed;
        }
        if(Input.Keyboard.IsKeyDown(Keys.D)){
            slimeVel.X += speed;
        }
        if(Input.Keyboard.IsKeyDown(Keys.W)){
            slimeVel.Y -= speed;
        }
        if(Input.Keyboard.IsKeyDown(Keys.S)){
            slimeVel.Y += speed;
        }

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
        physics.FixedUpdate(gameTime);
        
        RefView<PhysicsBodyAABB> spriteBodyRef = physics.GetPhysicsBody(ref slimeBody);
        if(spriteBodyRef.Valid == true){
            ref PhysicsBodyAABB data = ref spriteBodyRef.Data;
            spriteRenderer.SetAnimatedSpritePosition(ref slimeSprite, data.Position);
            data.Velocity = slimeVel;

        }

        RefView<PhysicsBodyAABB> rf = physics.GetPhysicsBody(ref slimeBody);
        if(rf.Valid == true){
            slimePos = rf.Data.Position;
        }

    }


    protected override void Draw(GameTime gameTime){
        GraphicsDevice.Clear(Color.DimGray);


        // Begin the sprite batch to preapre for rendering.
        SpriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, camera.ViewMatrix);
        spriteRenderer.DrawAll(SpriteBatch, SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, camera.ViewMatrix);
        SpriteBatch.End();


        SpriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, camera.ViewMatrix);
        physics.DrawAllOutlines(SpriteBatch, Color.White, 1);
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

}
