﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Growth.Voronoi
{
    // purpose here is to have an immutable 3d vector type, Unity's is mutable...
    [DebuggerDisplay("({X}, {Y}, {Z})")]
    public class Vec3Int : IEquatable<Vec3Int>
    {
        public Vec3Int(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vec3Int(Vector3Int p)
        {
            X = p.x;
            Y = p.y;
            Z = p.z;
        }

        public Vec3Int()
        {
            X = Y = Z = 0;
        }

        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public IEnumerable<Vec3Int> OrthoNeighbours
        {
            get
            {
                yield return new Vec3Int(X + 1, Y, Z);
                yield return new Vec3Int(X - 1, Y, Z);
                yield return new Vec3Int(X, Y + 1, Z);
                yield return new Vec3Int(X, Y - 1, Z);
                yield return new Vec3Int(X, Y, Z + 1);
                yield return new Vec3Int(X, Y, Z - 1);
            }
        }

        public static Vec3Int operator +(Vec3Int lhs, Vec3Int rhs)
        {
            return new Vec3Int(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        }

        public static Vec3Int operator -(Vec3Int lhs, Vec3Int rhs)
        {
            return new Vec3Int(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
        }

        public static Vec3Int operator *(Vec3Int lhs, float rhs)
        {
            return new Vec3Int((int)(lhs.X * rhs), (int)(lhs.Y * rhs), (int)(lhs.Z * rhs));
        }

        public float Size2()
        {
            return X * X + Y * Y + Z * Z;
        }

        public bool IsBefore(Vec3Int other)
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

        public Vector3 ToVector3Int()
        {
            return new Vector3Int(X, Y, Z);
        }

        #region IEquatable
        public bool Equals(Vec3Int other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }
        #endregion

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() * 17 + Z.GetHashCode() * 19;
        }

        internal Vec3 ToVec3()
        {
            return new Vec3(X, Y, Z);
        }
    }
}
