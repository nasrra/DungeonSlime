using System.Numerics;
using DungeonSlime;
using HowlEngine.Collections;
using HowlEngine.ECS;
using HowlEngine.Physics;

public class SolidSquare : Entity{

    public Token PhysicsBody;
    public Token Sprite; 

    public SolidSquare(Vector2 position){
        Position = position;
        _id = Game.EntityManager.AllocateEntity(this);        
        PhysicsBody = Game.PhysicsSystem.AllocateBoxRigidBody(
            position,
            16,
            16,
            20,
            0.5f
        );
        Sprite = Game.SpriteRenderer.AllocateStaticSprite(
            "Debug", "Square", position
        );
    }

    public override void Dispose(){
        Game.EntityManager.FreeEntity(ref _id);
        Game.PhysicsSystem.FreePolygonRigidBody(ref PhysicsBody);
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
        RefView<PolygonPhysicsBody> rv = Game.PhysicsSystem.GetPolygonRigidBody(ref PhysicsBody);
        if(rv.IsValid){
            Position = rv.Data.Position;
        }

    }


    public void SpriteFixedUpdate(float deltaTime){
        Game.SpriteRenderer.SetStaticSpritePosition(ref Sprite, Position);
    }
}