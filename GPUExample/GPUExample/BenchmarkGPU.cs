using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.ParallelArrays;
using PA = Microsoft.ParallelArrays.ParallelArrays;
using FPA = Microsoft.ParallelArrays.FloatParallelArray;

namespace GPUExample
{
    class BenchmarkGPU
    {
        private static readonly float PI = Convert.ToSingle(Math.PI);
        private static readonly float EARTHRADIUS = 3958.75f;
        private static DX9Target eval = new DX9Target();

        public float[] CalculateGreaterCircleDistance(float[] lat1, float[] long1, float[] lat2, float[] long2)
        {
            FPA fpLat1 = new FPA(lat1);
            FPA fpLong1 = new FPA(long1);
            FPA fpLat2 = new FPA(lat2);
            FPA fpLong2 = new FPA(long2);
            FPA fpResult = new FPA(0, fpLat1.Shape);

            fpResult += PA.Sqrt(PA.Add(
                PA.Pow2(PA.Sin(PA.Divide(PA.Divide(PA.Multiply(PI, PA.Subtract(fpLat2, fpLat1)), 180.0f), 2.0f))),
                PA.Multiply(
                    PA.Multiply(
                        PA.Cos(PA.Divide(PA.Multiply(PI, fpLat1), 180.0f)),
                        PA.Cos(PA.Divide(PA.Multiply(PI, fpLat2), 180.0f))
                        ),
                    PA.Pow2(PA.Sin(PA.Divide(PA.Divide(PA.Multiply(PI, PA.Subtract(fpLong2, fpLong1)), 180.0f), 2.0f)))
                    )
                ));

            float[] distance = new float[lat1.Length];
            eval.ToArray(fpResult, out distance);
            return distance;
        }
    }
}
