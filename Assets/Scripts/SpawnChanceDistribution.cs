using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnChanceDistribution
{

   float[] probabilityArray;
 

    public SpawnChanceDistribution(float[] probabilityArray)
    {
      
            this.probabilityArray = probabilityArray;
      
    }
    public int sampleDistribution()
    {
        float uniformValue = Random.Range(0f, 1f);
        for(int i = 0; i < probabilityArray.Length; i++)
        {
           
                if (partitionFunction(i-1) <= uniformValue  && uniformValue <= partitionFunction(i))
                {
                    return i;
                }
          
        }
        return 0;
    }
    public float partitionFunction(int i) {
        float value = 0;
        for(int j=0; j<=i; j++)
        {
            value+= probabilityArray[j];
        }
        return value;
    }

}
