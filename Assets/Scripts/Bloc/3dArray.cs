using System;

public class <T> 3dArray{
	T[][][] grid = new T[101][101][101];
	float distance;
	Vector3 origin = new Vector3 (51, 51, 51);
	public Class1(float distance)
	{
	this.distance = distance;
	}

public T GiveObjectAtPosition(Vector3 Position)
{
	int x = (int)Position.x/distance+51;
    int y = (int)Position.x / distance+51;
    int z = (int)Position.x / distance+51;
	return grid[x, y, z];
}

public void RemoveObjectAtPosition(Vector3 Position)
{
    int x = (int)Position.x / distance + 51;
    int y = (int)Position.x / distance + 51;
    int z = (int)Position.x / distance + 51;
	grid[x,y,z] = null;
}

public void AddObjectAtPosition(Vector3 Position, T obj)
{
    int x = (int)Position.x / distance + 51;
    int y = (int)Position.x / distance + 51;
    int z = (int)Position.x / distance + 51;
    grid[x, y, z] = obj;
}

public Vector3 GetPositionOfObject(T obj)
{

}
}
