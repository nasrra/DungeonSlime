using System;
using HowlEngine.Collections;
using HowlEngine.ECS;
using HowlEngine.Graphics;
using HowlEngine.Physics;
using Microsoft.Xna.Framework;

namespace DungeonSlime.Entities;

public class Bat : Entity{
    private Token _sprite;
    private Token _physicsBody;

    public Bat(){
        new Bat(Vector2.Zero);
    }

    public Bat(Vector2 position){
        
        // Allocate components.
        
        _sprite         = Game.SpriteRenderer.AllocateAnimatedSprite("Entities", "BatIdle");
        _physicsBody    = Game.PhysicsSystem.AllocatePyhsicsBody(new PhysicsBodyAABB(new RectangleCollider((int)position.X,(int)position.Y,20,20)));

        // set unique data.

        Position = new Vector2((int)position.X, (int)position.Y);
    }

    public override void FixedUpdate(GameTime gameTime){
        PhysicsFixedUpdate();
        SpriteFixedUpdate();
    }

    public override void Start(){
    
    }

    public override void Update(GameTime gameTime){
    
    }

    public override void Dispose(){
        base.Dispose();
        Game.SpriteRenderer.FreeAnimatedSprite(ref _sprite);
        Game.PhysicsSystem.FreePhysicsBody(ref _physicsBody);
    }

    private void PhysicsFixedUpdate(){
        RefView<PhysicsBodyAABB> rv = Game.PhysicsSystem.GetPhysicsBody(ref _physicsBody);
        if(rv.IsValid){
            Position = rv.Data.Position;
        }
    }

    private void SpriteFixedUpdate(){
        RefView<AnimatedSprite> rv = Game.SpriteRenderer.GetAnimatedSprite(ref _sprite);
        if(rv.IsValid){
            rv.Data.Position = Position;
        }
    }
}