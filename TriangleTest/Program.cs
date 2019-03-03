using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using ImpulseEngine2;

namespace TriangleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Vector2[] vectors = new Vector2[3];
            for (int i = 0; i < 3; i++) vectors[i] = PromptVector();
            Triangle triangle = new Triangle(vectors[0], vectors[1], vectors[2]);
            //Console.WriteLine(triangle.GetInertia(1));

            Console.ReadLine();
        }

        private static Vector2 PromptVector()
        {
            Console.WriteLine("Vector?");
            return new Vector2(float.Parse(Console.ReadLine()), float.Parse(Console.ReadLine()));
        }
    }
}
