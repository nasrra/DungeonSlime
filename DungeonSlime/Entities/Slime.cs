using System;
using HowlEngine;
using HowlEngine.Collections;
using HowlEngine.ECS;
using HowlEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DungeonSlime.Entities;

public class Slime : Entity{
    private Token _sprite;
    private Token _physicsBody;
    private float _movementSpeed;

    public Slime(){
        new Slime(Vector2.Zero);
    }

    public Slime(Vector2 position){
        Name = "Slime";        
        // allocation.
        
        _sprite          = Game.SpriteRenderer.AllocateAnimatedSprite("Entities", "SlimeIdle", position);
        _physicsBody     = Game.PhysicsSystem.AllocatePyhsicsBody(new PhysicsBodyAABB(new RectangleCollider((int)position.X,(int)position.Y,20,20)));
        
        // setting local variables.

        _movementSpeed = 1f;
        Position = position;

        // Start();
    }
    
    public override void Dispose(){
        base.Dispose();
        Game.SpriteRenderer.FreeAnimatedSprite(ref _sprite);
        Game.PhysicsSystem.FreePhysicsBody(ref _physicsBody);
    }

    public override void FixedUpdate(GameTime gameTime){        
        if(HowlApp.Input.Keyboard.IsKeyDown(Keys.LeftAlt)){
            Dispose();
            return;
        }

        PhysicsFixedUpdate();
        SpriteFixedUpdate();
    }

    public override void Start()
    {
        throw new System.NotImplementedException();
    }

    public override void Update(GameTime gameTime)
    {
        // throw new System.NotImplementedException();
    }

    private void PhysicsFixedUpdate(){

        RefView<PhysicsBodyAABB> rv = Game.PhysicsSystem.GetPhysicsBody(ref _physicsBody);

        if(rv.IsValid == true){
            Vector2 newVelocity = new Vector2();
            if(HowlApp.Input.Keyboard.IsKeyDown(Keys.A)){
                newVelocity.X -= _movementSpeed;
            }
            if(HowlApp.Input.Keyboard.IsKeyDown(Keys.D)){
                newVelocity.X += _movementSpeed;
            }
            if(HowlApp.Input.Keyboard.IsKeyDown(Keys.W)){
                newVelocity.Y -= _movementSpeed;
            }
            if(HowlApp.Input.Keyboard.IsKeyDown(Keys.S)){
                newVelocity.Y += _movementSpeed;
            }

            rv.Data.Velocity = newVelocity;
            Position = rv.Data.Position;
        }
    }

    private void SpriteFixedUpdate(){
        Game.SpriteRenderer.SetAnimatedSpritePosition(ref _sprite, Position);
    }

    ~Slime(){
        Console.WriteLine("Slime freed.");
    }
}