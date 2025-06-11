using System;
using System.Numerics;
using DungeonSlime;
using HowlEngine.Collections;
using HowlEngine.ECS;

public class BouncyBall : Entity{

    public Token PhysicsBody;
    public Token Sprite; 

    public BouncyBall(Vector2 position){
        Position = position;
        _id = Game.EntityManager.AllocateEntity(this);        
        PhysicsBody = Game.PhysicsSystem.AllocateCircleRigidBody(
            position,
            8,
            2,
            1
        );
        Sprite = Game.SpriteRenderer.AllocateStaticSprite(
            "Debug", "Circle", position
        );
    }

    public override void Dispose(){
        Game.EntityManager.FreeEntity(ref _id);
        Game.PhysicsSystem.FreeCircleRigidBody(ref PhysicsBody);
        Game.SpriteRenderer.FreeStaticSprite(ref Sprite);
    }

    public override void FixedUpdate(float deltaTime){
        PhysicsFixedUpdate(deltaTime);
        SpriteFixedUpdate(deltaTime);
    }

    public override void Start(){
    }

    public override void Update(float deltaTime){
    }

    public void PhysicsFixedUpdate(float deltaTime){
        RefView<CirclePhysicsBody> rv = Game.PhysicsSystem.GetCircleRigidBody(ref PhysicsBody);
        if(rv.IsValid){
            Position = rv.Data.Position;
        }

    }


    public void SpriteFixedUpdate(float deltaTime){
        Game.SpriteRenderer.SetStaticSpritePosition(ref Sprite, Position);
    }
}