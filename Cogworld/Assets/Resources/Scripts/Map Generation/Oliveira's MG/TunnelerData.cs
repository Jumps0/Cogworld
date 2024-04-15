using UnityEngine;
using DungeonResources;

[System.Serializable]
public class TunnelerData {
    public Vector2Int location;
    public Direction direction, desiredDirection;
    public int age, maxAge, generation, stepLength, tunnelWidth, straightDoubleSpawnProb, turnDoubleSpawnProb, changeDirProb, makeRoomRightProb, makeRoomLeftProb, joinPref;
    public TunnelerData() {
        location = Vector2Int.zero;
        direction = Direction.XX;
        desiredDirection = Direction.XX;
    }
}
