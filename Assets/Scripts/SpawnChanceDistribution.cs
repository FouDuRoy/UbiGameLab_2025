using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class SpawnChanceDistribution
{

    float[] probabilityArray;
    List<int> distributionArray = new List<int>();

    public SpawnChanceDistribution(float[] probabilityArray)
    {
        float somme = 0;
        foreach (var item in probabilityArray)
        {
            somme += item;
        }
        if (somme == 1)
        {
            this.probabilityArray = probabilityArray;
        }
        else
        {
            throw new System.Exception("Somme des probabilités ne donne pas 1");
        }

        for (int i = 0; i < probabilityArray.Length; i++)
        {
            int numberOfValues = (int)(100 * probabilityArray[i]);
            for (int j = 0; j < numberOfValues; j++)
            {
                distributionArray.Add(i);
            }
        }
    }
    public int sampleDistribution()
    {
        return distributionArray[Random.Range(0, distributionArray.Count)];
    }

}
