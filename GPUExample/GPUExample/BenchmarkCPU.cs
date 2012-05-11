using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPUExample
{


    class BenchmarkCPU
    {
        private static readonly float PI = Convert.ToSingle(Math.PI);
        private static readonly float EARTHRADIUS = 3958.75f;

        public float[] CalculateGreaterCircleDistance(float[] lat1, float[] long1, float[] lat2, float[] long2)
        {
            float[] distance = new float[lat1.Length];
            for (int i = 0; i < lat1.Length; i++)
            {
                distance[i] = Convert.ToSingle(Math.Sqrt(
                    Math.Pow(Math.Sin(Radians(lat2[i] - lat1[i]) / 2.0f), 2.0f) +
                    Math.Cos(Radians(lat1[i])) * Math.Cos(Radians(lat2[i])) * Math.Pow(Math.Sin(Radians(long2[i] - long1[i]) / 2.0f), 2.0f)
                    ));
            }

            return distance;
        }

        private float Radians(float degrees)
        {
            return ((PI * degrees) / 180.0f);
        }

    }
}
