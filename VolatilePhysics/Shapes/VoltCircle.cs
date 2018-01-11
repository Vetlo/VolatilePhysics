﻿/*
 *  VolatilePhysics - A 2D Physics Library for Networked Games
 *  Copyright (c) 2015-2016 - Alexander Shoulson - http://ashoulson.com
 *
 *  This software is provided 'as-is', without any express or implied
 *  warranty. In no event will the authors be held liable for any damages
 *  arising from the use of this software.
 *  Permission is granted to anyone to use this software for any purpose,
 *  including commercial applications, and to alter it and redistribute it
 *  freely, subject to the following restrictions:
 *  
 *  1. The origin of this software must not be misrepresented; you must not
 *     claim that you wrote the original software. If you use this software
 *     in a product, an acknowledgment in the product documentation would be
 *     appreciated but is not required.
 *  2. Altered source versions must be plainly marked as such, and must not be
 *     misrepresented as being the original software.
 *  3. This notice may not be removed or altered from any source distribution.
 */

namespace Volatile
{
  public sealed class VoltCircle : VoltShape
  {
    #region Factory Functions
    internal void InitializeFromWorldSpace(
      VoltVec2 worldSpaceOrigin, 
      float radius,
      float density,
      float friction,
      float restitution)
    {
      base.Initialize(density, friction, restitution);

      this.worldSpaceOrigin = worldSpaceOrigin;
      this.worldSpaceAABB = new VoltAABB(worldSpaceOrigin, radius);

      this.radius = radius;
      this.sqrRadius = radius * radius;
    }

    internal void InitializeFromBodySpace(
      VoltVec2 bodySpaceOrigin,
      float radius,
      float density,
      float friction,
      float restitution)
    {
      base.Initialize(density, friction, restitution);

      this.radius = radius;
      this.sqrRadius = radius * radius;

      this.bodySpaceOrigin = bodySpaceOrigin;
      this.bodySpaceAABB = new VoltAABB(bodySpaceOrigin, radius);
      this.hasBodySpace = true;
    }
    #endregion

    #region Properties
    public override VoltShapeType Type { get { return VoltShapeType.Circle; } }

    public VoltVec2 Origin { get { return this.worldSpaceOrigin; } }
    public float Radius { get { return this.radius; } }
    #endregion

    #region Fields
    internal VoltVec2 worldSpaceOrigin;
    internal float radius;
    internal float sqrRadius;

    // Precomputed body-space values (these should never change unless we
    // want to support moving shapes relative to their body root later on)
    private VoltVec2 bodySpaceOrigin;
    private bool hasBodySpace;
    #endregion

    public VoltCircle() 
    {
      this.Reset();
    }

    protected override void Reset()
    {
      base.Reset();

      this.worldSpaceOrigin = VoltVec2.ZERO;
      this.radius = 0.0f;
      this.sqrRadius = 0.0f;

      this.bodySpaceOrigin = VoltVec2.ZERO;
      this.hasBodySpace = false;
    }

    #region Functionality Overrides
    protected override void ComputeMetrics()
    {
      if (this.hasBodySpace == false)
      {
        this.bodySpaceOrigin =
          this.Body.WorldToBodyPointCurrent(this.worldSpaceOrigin);
        this.bodySpaceAABB = new VoltAABB(this.bodySpaceOrigin, this.radius);
        this.hasBodySpace = true;
      }

      this.Area = this.sqrRadius * VoltMath.PI;
      this.Mass = this.Area * this.Density * VoltConfig.AreaMassRatio;
      this.Inertia =
        this.sqrRadius / 2.0f + this.bodySpaceOrigin.SqrMagnitude;
    }

    protected override void ApplyBodyPosition()
    {
      this.worldSpaceOrigin =
        this.Body.BodyToWorldPointCurrent(this.bodySpaceOrigin);
      this.worldSpaceAABB = new VoltAABB(this.worldSpaceOrigin, this.radius);
    }
    #endregion

    #region Test Overrides
    protected override bool ShapeQueryPoint(
      VoltVec2 bodySpacePoint)
    {
      return 
        Collision.TestPointCircleSimple(
          this.bodySpaceOrigin,
          bodySpacePoint, 
          this.radius);
    }

    protected override bool ShapeQueryCircle(
      VoltVec2 bodySpaceOrigin, 
      float radius)
    {
      return 
        Collision.TestCircleCircleSimple(
          this.bodySpaceOrigin,
          bodySpaceOrigin, 
          this.radius, 
          radius);
    }

    protected override bool ShapeRayCast(
      ref VoltRayCast bodySpaceRay, 
      ref VoltRayResult result)
    {
      return Collision.CircleRayCast(
        this,
        this.bodySpaceOrigin,
        this.sqrRadius,
        ref bodySpaceRay, 
        ref result);
    }

    protected override bool ShapeCircleCast(
      ref VoltRayCast bodySpaceRay, 
      float radius,
      ref VoltRayResult result)
    {
      float totalRadius = this.radius + radius;
      return Collision.CircleRayCast(
        this,
        this.bodySpaceOrigin,
        totalRadius * totalRadius,
        ref bodySpaceRay,
        ref result);
    }
    #endregion
  }
}
