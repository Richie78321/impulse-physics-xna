using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpulseEngine2.Materials
{
    public struct Material
    {
        public readonly float Density;
        public readonly float Bounce;
        public readonly float StaticFriction;
        public readonly float DynamicFriction;

        public Material(float Density, float Bounce, float StaticFriction, float DynamicFriction)
        {
            this.Density = Density;
            this.Bounce = Bounce;
            this.StaticFriction = StaticFriction;
            this.DynamicFriction = DynamicFriction;
        }
    }
}
