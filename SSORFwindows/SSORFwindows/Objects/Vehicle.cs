using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SSORF.Management;

//Contains 3D model and specs for a scooter
namespace SSORF.Objects
{
    public class Vehicle
    {
        StaticModel geometry;
        //Units for simulation:
        //Torque:       Newton-meters
        //Force:        Newtons
        //Weight:       Kilograms
        //Speed:        Meters/Sec
        //Distance:     Meters
        //Angles:       Radians
        //Grip:         Meters/Sec^2
        float meterToInchScale = 39.37f;
        float outputPower;
        float brakePower = 2;
        float speed;
        float yaw;
        float weight;
        float wheelAngle = 0;
        float wheelMaxAngle = .785f;
        float wheelRadius = 0.175f;
        float wheelBaseLength = 1;
        float gripRating = 2.94f;
        float coefficientDrag = .8f;
        float frontalArea = 6;

        //vehicle still needs list of specs such as weight, name, etc
        //Also need a way to add upgrades to vehicles

        public void load(ContentManager content, SSORFlibrary.ScooterData VehicleSpecs, upgradeSpecs Upgrades)
        {
            //implemented specs and upgrades for weight and power
            weight = VehicleSpecs.weight + Upgrades.weight;
            outputPower = (VehicleSpecs.power + Upgrades.power) / 45;

            geometry = new StaticModel(content, "Models\\scooter" + VehicleSpecs.IDnum.ToString(),
                Vector3.Zero, Matrix.Identity, Matrix.Identity);
            geometry.LoadModel();

            //Things to load here:
            //value of wheelMaxAngle - DO IT IN RADIANS
            //value of wheelBaseLength
            //other vehicle specs
            //upgrade specs
        }

        public void setNormal(TerrainInfo terrainInfo)
        {
            float height;
            Vector3 normal;
            Vector3 location;

            if(terrainInfo.IsOnHeightmap(geometry.Location))
            {
                terrainInfo.GetHeightAndNormal(geometry.Location, out height, out normal);
                location = geometry.Location;
                location.Y = height;
                geometry.Location = location;
            }
        }

        public void setStartingPosition(float startingYaw, Vector3 startingPosition, float startingSpeed)
        {
            yaw = startingYaw;
            geometry.Orientation = Matrix.CreateRotationY(yaw);
            geometry.Location = startingPosition;
            //speed = startingSpeed;
        }

        public void update(GameTime gameTime, float steerValue, float throttleValue, float brakeValue)
        {
            //Get the integral of the vehicle's velocity
            float tempDistance = speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            //Find the vehicle's current turning radius
            float turnRadius = wheelBaseLength/(float)Math.Tan(wheelAngle);
            //TODO: calculate lateral force here - remember to fix yaw
            if (gripRating < ((float)Math.Pow(speed/turnRadius, 2) * Math.Abs(turnRadius)))
            {
                if (turnRadius < 0)
                    turnRadius = (float)Math.Pow(speed, 2) / -gripRating;
                else
                    turnRadius = (float)Math.Pow(speed, 2) / gripRating;
            }
            //Now use those to get the vehicle's yaw offset
            float deltaYaw = tempDistance / turnRadius;
            //Update rotations
            yaw += deltaYaw;
            geometry.Orientation = Matrix.CreateRotationY(yaw);
            //Derive and update position
            geometry.Location += geometry.Orientation.Forward * tempDistance * (float)Math.Cos(deltaYaw) * meterToInchScale;
            geometry.Location += geometry.Orientation.Left * tempDistance * (float)Math.Sin(deltaYaw) * meterToInchScale;
            //Capture the wheel angle for the next frame's worth of motion
            wheelAngle = steerValue * wheelMaxAngle;
            //TODO: calculate drag here
            float dragForce = coefficientDrag * frontalArea * .5f * (float)Math.Pow(speed, 2);
            dragForce += .1f;   //For powertrain loss/rolling resistance
            //Calculate delta-v
            float longForce = (outputPower / wheelRadius) * throttleValue;
            longForce -= (brakePower / wheelRadius) * brakeValue;
            longForce -= dragForce;
            float deltaV = (longForce) / weight; 
            speed += deltaV;
            if (speed < 0)
                speed = 0;
            UpdateAudio(throttleValue);
        }

        public void UpdateAudio(float throttleValue)
        {
            AudioManager.getEngineSounds().SetVariable("throttleValue", throttleValue + speed);
            AudioManager.getEngineSounds().SetVariable("Speed", speed);
            if (throttleValue != 0)
            {

                if (AudioManager.getEngineSounds().IsPaused)
                    AudioManager.getEngineSounds().Resume();
                else if (AudioManager.getEngineSounds().IsStopped)
                {
                    AudioManager.resetEngineSounds();
                    AudioManager.getEngineSounds().Play();
                }
                else if (AudioManager.getEngineSounds().IsPlaying == false &&
                    AudioManager.getEngineSounds().IsPrepared)
                    AudioManager.getEngineSounds().Play();
            }
            else
            {
                AudioManager.getEngineSounds().Stop(AudioStopOptions.AsAuthored);
            }
        }
 
        //Accessors and Mutators
        public StaticModel Geometry { get { return geometry; } set { geometry = value; } }

        public float Yaw { get { return yaw; } set { yaw = value; } }

    }
}
