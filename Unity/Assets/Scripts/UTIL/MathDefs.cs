using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class MathDefs  {
	
	private static System.Random rand = new System.Random();

    public static void SetSeed(int seed) {
        rand = new System.Random(seed);
    }

    //generate an alphanumeric code of length = cLength
    public static string RandomCode(int cLength) {

        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] sc = new char[cLength];

        for (int i = 0; i < cLength; i++) {
            sc[i] = chars[rand.Next(chars.Length)];
        }

        return new String(sc);
    }


    public static float[,] MatrixMult(this float[,] a, float[,] b) {
        if (a.GetLength(1) == b.GetLength(0)) {
            float[,] c = new float[a.GetLength(0), b.GetLength(1)];
            for (int i = 0; i < c.GetLength(0); i++) {
                for (int j = 0; j < c.GetLength(1); j++) {
                    c[i, j] = 0;
                    for (int k = 0; k < a.GetLength(1); k++) // OR k<b.GetLength(0)
                        c[i, j] = c[i, j] + a[i, k] * b[k, j];
                }
            }

            return c;
        }
        else {
            Console.WriteLine("\n Number of columns in First Matrix should be equal to Number of rows in Second Matrix.");
            Console.WriteLine("\n Please re-enter correct dimensions.");
            return null;
        }

       
    }

    public static float[,] Transpose(this float[,] a) {
        float[,] aT = new float[a.GetLength(1), a.GetLength(0)];
         for (int i = 0; i < a.GetLength(0); i++) {
             for (int j = 0; j < a.GetLength(1); j++) {
                 aT[j, i] = a[i, j];
             }
             
         }

        return aT;
    }
    public static Vector3 QuadraticCurve(Vector2 p1, Vector2 p2, Vector2 p3 ) {
        float a = ((p2.y - p1.y) * (p1.x - p3.x) + (p3.y - p1.y) * (p2.x - p1.x)) / ((p1.x - p3.x) * (p2.x * p2.x - p1.x * p1.x) + (p2.x - p1.x) * (p3.x * p3.x - p1.x * p1.x));
        float b = ((p2.y - p1.y) - a * (p2.x*p2.x - p1.x*p1.x))/(p2.x - p1.x);
        float c = p1.y - a * p1.x * p1.x - b * p1.x;

        return new Vector3(a, b, c);
    }
  
    public static float Constrain(this float value, float min, float max) {
        if (value < min)
            value = min;
        if (value > max)
            value = max;
        float result = value;
        
        return result;
    }

    public static float Constrain(this float value, float min) {
        if (value < min)
            value = min;
        float result = value;

        return result;
    }


    public static float Truncate(this float value, int digits) {
        double mult = Math.Pow(10.0, digits);
        double result = Math.Truncate(mult * value) / mult;
        return (float)result;
    }
    /// <summary>
    /// Gaussian Distribution with Box-Muller transform
    /// </summary>
    /// 
    public static float GaussianDist(float mean, float std) 
	{
		
		double u1 = rand.NextDouble(); //uniform(0,1) random doubles
		double u2 = rand.NextDouble();
		float randStdNormal = (float)Math.Sqrt(-2.0 * Math.Log(u1)) * (float)Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
		float randNormal = mean + std * randStdNormal; //random normal(mean,stdDev^2)						
		return randNormal;
			
	}
		
    public static float StdDev(List<float>data, float mean) {
        float std = 0f;

        if (data.Count == 0)
            return 0f;

        for(int i = 0; i < data.Count; i++){
            std += ((data[i] - mean) * (data[i] - mean));
        }
        
        std /= (data.Count - 1);

        return (float) Math.Sqrt(std);
    }
    public static int GetRandomNumber(int max)
    {		
		return rand.Next (max);		
	}

    public static float GetRandomNumber(float max) {
        return (float)rand.NextDouble() * max;
    }
    public static int GetRandomNumber(int min, int max) {
        return rand.Next(min, max);
    }
    public static float GetRandomNumber(float min, float max) {
        return min + (float)rand.NextDouble() * (max - min);
    }
    

	public static float GetLength(float[] v)
	{
	    float[] vec = {v[0], v[1], v[2], 0};
	
		vec[0] *= vec[0];
		vec[1] *= vec[1];
		vec[2] *= vec[2];
	
		return (float) Math.Sqrt(vec[0] + vec[1] + vec[2]);
	}
	
	/// <summary>
	/// Normalize all elements of the array arr to [min, max]
	/// </summary>
	public static void NormalizeElements(float[] arr, float min, float max)
	{
		int i;
		float arrMin, arrMax;
		float diff;
		
		arrMin = arrMax = arr[0];		
		for(i=0; i<arr.Length; i++) {		
			if(arr[i] < arrMin)
				arrMin = arr[i];
			if(arr[i] > arrMax)
				arrMax = arr[i];
		}
	
		
		diff = (arrMax - arrMin);
		
		if(arrMax <= max && arrMin >= min) //if they are already in the range [min max] do not change the array
			return;
		
		if(arrMax == arrMin) {
			if(arrMax == min)
				return; //all = min = max
			else if(arrMax > min && arrMax < max ) //[0 1] region
				return;
			else if(arrMax > max) { //clamp to 1
				for(i=0; i<arr.Length; i++)	
					arr[i] = max;
			}
			else if(arrMax < min) {  //clamp to 0
				for(i=0; i<arr.Length; i++)	
					arr[i] = min;
			}
			
		}
				
				
		
		for(i=0; i<arr.Length; i++)		
			arr[i] = (arr[i] - arrMin) / diff;
	}
	
	/// Returns the array length
	public static float  Length(float [] arr) {
		float len = 0;
		
		for(int i = 0 ; i < arr.Length; i++)
			len += arr[i] * arr[i];
		
		
		return Mathf.Sqrt (len);
	}

    //Projects point p onto the line segment a-b
    //Adapted from http://www.alecjacobson.com/weblog/?p=1486
    public static Vector3 ProjectPointLine (Vector3 p ,Vector3 a, Vector3 b) {
        Vector3 q;
        Vector3 ab = b - a;
        float abMag = Vector3.Magnitude(ab) * Vector3.Magnitude(ab);

        if (abMag == 0)
            q = a;
        else {       
            Vector3 ap = p - a;
            // from http://stackoverflow.com/questions/849211/
            // Consider the line extending the segment, parameterized as A + t (B - A)
            // We find projection of point p onto the line. 
            // It falls where t = [(p-A) . (B-A)] / |B-A|^2
            float t = Vector3.Dot(ap, ab / abMag);
            if (t < 0.0f) // "Before" A on the line, just return A            
                q = a;
            else if (t > 1.0f) {// "After" B on the line, just return B
                q = b;
            }
            else //projection lines "inbetween" A and B on the line
                q = a + t * ab;

           

        }

        

        return q;
    }


    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }
}
