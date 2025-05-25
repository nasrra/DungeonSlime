using System.Diagnostics;
using System;

using HowlEngine;
using HowlEngine.Graphics;
using HowlEngine.Physics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using HowlEngine.ECS;
using HowlEngine.Collections;

namespace DungeonSlime;

public class Game : HowlApp{
    private AABBPhysicSystem physics;
    private SpriteRenderer spriteRenderer;
    
    private Token slimeSprite;
    private Token slimeBody;
    private Vector2 slimePos = new Vector2(200,200);

    // private RectangleCollider batBox;
    private static int batAmount = 10;
    private Token[] batBoxes = new Token[batAmount];
    private Token[] batSprites = new Token[batAmount];
    private Vector2[] batPositions = new Vector2[batAmount];

    private Camera camera;

    private const string atlasName = "Characters";

    public Game() : base("Dungeon Slime", 1920, 1080, false, true){
        
    }

    protected override void Initialize(){
        base.Initialize();

        DisableVSync();
        SetFrameRate(170);
        Random r = new Random();

        camera = new Camera(Vector2.Zero, Vector2.Zero, new Vector2(1280,720));

        physics = new AABBPhysicSystem(0, batAmount+1);
        slimeBody = physics.AllocatePyhsicsBody(new PhysicsBodyAABB(new RectangleColliderStruct((int)slimePos.X,(int)slimePos.Y,80,80)));

        for(int i = 0; i < batAmount; i++){
            Vector2 position = new Vector2(r.Next(100,1180), r.Next(100,620));
            batPositions[i] = position;
            batBoxes[i] = physics.AllocatePyhsicsBody(new PhysicsBodyAABB(new RectangleColliderStruct((int)position.X,(int)position.Y,80,80)));
        }


        spriteRenderer = new SpriteRenderer(
            new Dictionary<string, TextureAtlas>(){
                {atlasName, TextureAtlas.FromFile(Content, "images/atlas-data.xml")},
            },
            batAmount + 1,
            batAmount + 1
        );

        if(spriteRenderer.GetTextureAtlas(atlasName).TryGetTarget(out TextureAtlas atlas)){
            slimeSprite = spriteRenderer.AllocateAnimatedSprite(atlas.CreateAnimatedSprite("slime-animation"));
            RefView<AnimatedSprite> slimeSpriteRef = spriteRenderer.GetAnimatedSprite(ref slimeSprite);
            if(slimeSpriteRef.Valid == true){
                slimeSpriteRef.Data.Scale = new Vector2(4.0f,4.0f);
                slimeSpriteRef.Data.Position  = slimePos;
            }

            for(int i = 0; i < batAmount; i++){
                batSprites[i] = spriteRenderer.AllocateAnimatedSprite(atlas.CreateAnimatedSprite("bat-animation"));
                RefView<AnimatedSprite> batSpriteRef = spriteRenderer.GetAnimatedSprite(ref batSprites[i]);
                if(batSpriteRef.Valid == true){
                    batSpriteRef.Data.Scale = new Vector2(4.0f, 4.0f);
                    batSpriteRef.Data.Position = batPositions[i];
                }
            }
        }


        // slimeBox = new RectangleCollider((int)slimePos.X, (int)slimePos.Y, 80,80);
        // batBox = new RectangleCollider((int)batPos.X, (int)batPos.Y, 80,80);

        // Any initializations you need to perform that have a dependency on assets being loaded should be done after the base.
        // Initialize call and not before it.
    }
    
    protected override void LoadContent(){        
        // TODO: use this.Content to load your game content here
        base.LoadContent();        
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
        GraphicsDevice.Clear(Color.Black);


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
}
