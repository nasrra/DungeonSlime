using System;
using HowlEngine.Collections;
using HowlEngine.ECS;
using System.Numerics;
using HowlEngine.Physics;

namespace DungeonSlime.Entities;

public class Bat : Entity{
    private Token _sprite;
    private Token _physicsBody;

    public Bat(){
        new Bat(Vector2.Zero);
    }

    public Bat(Vector2 position){
        _id = Game.EntityManager.AllocateEntity(this);

        Random r = new Random();

        // Allocate components.
        
        _sprite          = Game.SpriteRenderer.AllocateAnimatedSprite("Entities", "BatIdle", position);
        _physicsBody     = Game.physicsSystem.AllocateBoxRigidBody(new 
            BoxRigidBody(
                position, 
                16, 
                16,
                1,
                0
        ));

        // set unique data.

        Position = new Vector2((int)position.X, (int)position.Y);    
    }

    public override void FixedUpdate(float deltaTime){
        PhysicsFixedUpdate();
        SpriteFixedUpdate();
    }

    public override void Start(){
    }

    public override void Update(float deltaTime){
    
    }

    public override void Dispose(){
        Game.EntityManager.FreeEntity(ref _id);
        Game.SpriteRenderer.FreeAnimatedSprite(ref _sprite);
        Game.physicsSystem.FreeBoxRigidBody(ref _physicsBody);
    }

    private void PhysicsFixedUpdate(){
        RefView<BoxRigidBody> rv = Game.physicsSystem.GetBoxRigidBody(ref _physicsBody);
        if(rv.IsValid){
            Position = rv.Data.Position;
            rv.Data.Rotation += 0.025f;
        }
    }

    private void SpriteFixedUpdate(){
        Game.SpriteRenderer.SetAnimatedSpritePosition(ref _sprite, Position);
    }
}