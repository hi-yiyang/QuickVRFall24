using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{
    public static class QuickMathf
    {

        #region CONSTANTS

        public const float DEFAULT_FLOAT_PRECISION = 0.0001f;

        #endregion

        public static bool EqualsTo(float a, float b, float fPrecision = DEFAULT_FLOAT_PRECISION)
        {
            return Mathf.Abs(a - b) < fPrecision;
        }

        public static bool GreaterThan(float a, float b, float fPrecision = DEFAULT_FLOAT_PRECISION)
        {
            return a > b && !EqualsTo(a, b, fPrecision);
        }

        public static bool GreaterThanOrEqualTo(float a, float b, float fPrecision = DEFAULT_FLOAT_PRECISION)
        {
            return a > b || EqualsTo(a, b, fPrecision);
        }

        public static bool LessThan(float a, float b, float fPrecision = DEFAULT_FLOAT_PRECISION) 
        {
            return a < b && !EqualsTo(a, b, fPrecision);
        }

        public static bool LessThanOrEqualTo(float a, float b, float fPrecision = DEFAULT_FLOAT_PRECISION)
        {
            return a < b || EqualsTo(a, b, fPrecision);
        }

    }

}


