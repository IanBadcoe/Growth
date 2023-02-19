using Growth.Util;
using System;
using System.Diagnostics;
using UnityEngine;

namespace Growth.Voronoi
{
    // C# equality is a total mess, having a policy here of:
    // - never override operator ==/!=
    // -- so that x != null always means pure reference comparison
    // - never override Equals(object obj)
    // -- so we get type-safety on x.Equals(y)
    // - use T.Equals(T other) for _value_ compare
    // -- so it errors for null
    // -- which is the override for IEquatable<T> which at least some of the built-in containers use
    // - and override GetHashCode when you override that, also for containers...

    // purpose here is to have an immutable 3d vector type, Unity's is mutable...
    [DebuggerDisplay("({X}, {Y}, {Z})")]
    public class Vec3 : IEquatable<Vec3>, IBounded
    {
        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;

            MyAssert.IsTrue(!float.IsNaN(X), "NaN!");
            MyAssert.IsTrue(!float.IsNaN(Y), "NaN!");
            MyAssert.IsTrue(!float.IsNaN(Z), "NaN!");
        }

        public Vec3(Vector3 p)
        {
            X = p.x;
            Y = p.y;
            Z = p.z;

            MyAssert.IsTrue(!float.IsNaN(X), "NaN!");
            MyAssert.IsTrue(!float.IsNaN(Y), "NaN!");
            MyAssert.IsTrue(!float.IsNaN(Z), "NaN!");
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

        public Vec3 Max(Vec3 other)
        {
            return new Vec3(Mathf.Max(X, other.X), Mathf.Max(Y, other.Y), Mathf.Max(Z, other.Z));
        }

        internal Vec3 Min(Vec3 other)
        {
            return new Vec3(Mathf.Min(X, other.X), Mathf.Min(Y, other.Y), Mathf.Min(Z, other.Z));
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

        #region IEquatable
        public bool Equals(Vec3 other)
        {
            return X == other.X
                && Y == other.Y
                && Z == other.Z;
        }
        #endregion

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() * 3 + Z.GetHashCode() * 7;
        }

        #region IBounded
        public VBounds GetBounds()
        {
            return new VBounds(this, this);
        }
        #endregion
    }
}
