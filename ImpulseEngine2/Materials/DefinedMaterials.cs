using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpulseEngine2.Materials
{
    public static class DefinedMaterials
    {
        public static Material Rock => new Material(.0006F, .1F, .5F, .4F);
        public static Material Wood => new Material(.0003F, .2F, .2F, .15F);
        public static Material Metal => new Material(.0012F, .05F, .1F, .05F);
        public static Material Static => new Material(0F, .4F, .2F, .15F);
        public static Material Rubber => new Material(.0002F, .8F, .65F, .55F);
    }
}
