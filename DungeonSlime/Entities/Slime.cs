using System;
using System.Numerics;
using HowlEngine.Collections;
using HowlEngine.ECS;
using HowlEngine.Physics;
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

        // allocation.
        
        _id              = Game.EntityManager.AllocateEntity(this);
        _sprite          = Game.SpriteRenderer.AllocateAnimatedSprite("Entities", "SlimeIdle", position);
        _physicsBody     = Game.physicsSystem.AllocateBoxRigidBody(new 
            BoxRigidBody(
                position, 
                16, 
                16,
                1,
                0
        ));
        
        // setting local variables.

        _movementSpeed = 1f;
        Position = position;

        // Start();
    }
    
    public override void Dispose(){
        Game.EntityManager.FreeEntity(ref _id);
        Game.SpriteRenderer.FreeAnimatedSprite(ref _sprite);
        Game.physicsSystem.FreeBoxRigidBody(ref _physicsBody);
    }

    public override void FixedUpdate(float deltaTime){        
        if(Game.Input.Keyboard.IsKeyDown(Keys.LeftAlt)){
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

    public override void Update(float deltaTime)
    {
        // throw new System.NotImplementedException();
    }

    private void PhysicsFixedUpdate(){

        RefView<BoxRigidBody> rv = Game.physicsSystem.GetBoxRigidBody(ref _physicsBody);

        if(rv.IsValid == true){
            Vector2 newVelocity = Vector2.Zero;
            if(Game.Input.Keyboard.IsKeyDown(Keys.A)){
                newVelocity.X -= 1;
            }
            if(Game.Input.Keyboard.IsKeyDown(Keys.D)){
                newVelocity.X += 1;
            }
            if(Game.Input.Keyboard.IsKeyDown(Keys.W)){
                newVelocity.Y -= 1;
            }
            if(Game.Input.Keyboard.IsKeyDown(Keys.S)){
                newVelocity.Y += 1;
            }
            
            rv.Data.Position += newVelocity.Length() > 0 ? Vector2.Normalize(newVelocity) * _movementSpeed : Vector2.Zero;
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