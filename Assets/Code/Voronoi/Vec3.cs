using System;
using System.Diagnostics;
using UnityEngine;

namespace Growth.Voronoi
{
    // purpose here is to have an immutable 3d vector type, Unity's is mutable...
    [DebuggerDisplay("({X}, {Y}, {Z})")]
    public class Vec3
    {
        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vec3(Vector3 p)
        {
            X = p.x;
            Y = p.y;
            Z = p.z;
        }

        public Vec3()
        {
            X = Y = Z = 0;
        }

        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public static Vec3 operator +(Vec3 lhs, Vec3 rhs)
        {
            return new Vec3(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        }

        public static Vec3 operator -(Vec3 lhs, Vec3 rhs)
        {
            return new Vec3(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
        }

        public static Vec3 operator -(Vec3 lhs)
        {
            return new Vec3(-lhs.X, -lhs.Y, -lhs.Z);
        }

        public static Vec3 operator *(Vec3 lhs, float rhs)
        {
            return new Vec3(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);
        }

        public static Vec3 operator /(Vec3 lhs, float rhs)
        {
            return new Vec3(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs);
        }

        public bool IsBefore(Vec3 other)
        {
            if (X < other.X)
            {
                return true;
            }
            else if (X > other.X)
            {
                return false;
            }

            if (Y < other.Y)
            {
                return true;
            }
            else if (Y > other.Y)
            {
                return false;
            }

            if (Z < other.Z)
            {
                return true;
            }
            else if (Z > other.Z)
            {
                return false;
            }

            // the two points are the same, so "IsBefore" is false, but we really do not expect to get asked this...
            return false;
        }

        public Vec3Int ToVec3Int()
        {
            return new Vec3Int(Mathf.FloorToInt(X), Mathf.FloorToInt(Y), Mathf.FloorToInt(Z));
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        public Vec3 Cross(Vec3 rhs)
        {
            return new Vec3(
                Y * rhs.Z - Z * rhs.Y,
                Z * rhs.X - X * rhs.Z,
                X * rhs.Y - Y * rhs.X);
        }

        public float Dot(Vec3 rhs)
        {
            return X * rhs.X + Y * rhs.Y + Z * rhs.Z;
        }

        public Vec3 Normalised()
        {
            return this / Length();
        }

        private float Length()
        {
            return Mathf.Sqrt(Length2());
        }

        public float Length2()
        {
            return Dot(this);
        }
    }
}
