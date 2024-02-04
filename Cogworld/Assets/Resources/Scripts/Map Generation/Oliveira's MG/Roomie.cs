using Resources;
using UnityEngine;

public class Roomie : Builder {
    private int defaultWidth;
    private int category;
    private RoomSize roomSize;

    public Roomie(ref DungeonGeneratorCTR map, Vector2Int location, Vector2Int forward, int age, int maxAge, int generation,
                  int dW, RoomSize s, int cat) : base(ref map, location, forward, age, maxAge, generation) {
        defaultWidth = dW;
        roomSize = s;
        category = cat;
    }

    private void CheckSides(Vector2Int position, Vector2Int heading, Vector2Int rightDir, ref int side, int frontFree, int sizeX, int sizeY) {
        Vector2Int testDir = Vector2Int.zero;
        SquareData dataAtTest = SquareData.NULL;
        int checkDistance = side;
        bool done = false;
        while (!done) {
            checkDistance++;
            for (int i = 1; i <= frontFree; i++) {
                testDir = position + checkDistance * rightDir + i * heading;
                if ((testDir.x < 0) || (testDir.y < 0) || (testDir.x >= sizeX) || (testDir.y >= sizeY)) {
                    side = checkDistance-1;
                    done = true;
                    break;
                } else
                    dataAtTest = map.GetMapData(testDir.x, testDir.y);
                if ((dataAtTest != SquareData.CLOSED) && (dataAtTest != SquareData.NJ_CLOSED)) {
                        side = checkDistance-1;
                        done = true;
                        break;
                }
            }
        }
    }

    public int FrontFree(Vector2Int position, Vector2Int heading, ref int leftFree, ref int rightFree) {
        Debug.Assert(leftFree >= 1 && rightFree >= 1);

        int dX = map.SizeX;
        int dY = map.SizeY;

        Debug.Assert((position.x >= 0 && position.y >= 0) && (position.x < dX && position.y <= dY));

        Debug.Assert(((heading.x == 0) && (heading.y == -1 || heading.y == 1)) 
                  || ((heading.y == 0) && (heading.x == -1 || heading.x == 1)));

        int frontFree = -1;

        Vector2Int right = Vector2Int.zero;
        Vector2Int left = Vector2Int.zero;
        Vector2Int test = Vector2Int.zero;

        if (heading.x == 0) {
            right = new Vector2Int(heading.y, 0);
        } else if (heading.y == 0) {
            right = new Vector2Int(0, -heading.x);
        }

        left = -right;

        int checkDistance = 0;
        SquareData dataAtTest = SquareData.NULL;

        while (frontFree == -1) {
            checkDistance++;
            for (int i = -leftFree; i <= rightFree; ++i) {
                test = position + i * right + checkDistance * heading;
                if (test.x < 0 || test.y < 0 || test.x >= dX || test.y >= dY) {
                    frontFree = checkDistance-1;
                    break;
                } else 
                    dataAtTest = map.GetMapData(test);
                    if (dataAtTest != SquareData.CLOSED && dataAtTest != SquareData.NJ_CLOSED) {
                        frontFree = checkDistance-1;
                        break;
                    } 
                }
            }
            Debug.Assert(frontFree >= 0);

            if (frontFree > 0) {
                CheckSides(position, heading, left, ref leftFree, frontFree, dX, dY);
                CheckSides(position, heading, right, ref rightFree, frontFree, dX, dY);
            }
        return frontFree;
    } 

    public override bool StepAhead() {
        //Debug.Log("Beginning RoomCTR");
        if (!map.WantsMoreRoomsD(roomSize)) {
            //Debug.Log("RoomCTR Denied");
            return false;

        }
        
        if (generation != map.ActiveGeneration) {
            Debug.Assert(generation > map.ActiveGeneration);
            //Debug.Log("Current Generation = " + map.ActiveGeneration + "Roomie Generation = " + generation);
            return true;
        }
        age++;

        if (age >= maxAge)
            return false;
        else if (age < 0)
            return true;

        Vector2Int right = Vector2Int.zero;

        if (forward.x == 0) 
            right = new Vector2Int(forward.y, 0);
        else if (forward.y == 0)
            right = new Vector2Int(0, -forward.x);
        else
            Debug.Assert(false);

        int dW = defaultWidth;
        double aR = map.RoomAspectRatio;
        int minSize = map.GetMinRoomSize(roomSize);
        int maxSize = map.GetMaxRoomSize(roomSize);
        int leftFree, rightFree, frontFree;

        do {
            leftFree = dW + 1;
            rightFree = dW + 1;
            frontFree = FrontFree(location, forward, ref leftFree, ref rightFree);

            if (frontFree < 4)
                break;
            int length = frontFree-2;
            double l = (double)length;
            int width = leftFree + rightFree - 1;
            double w = (double)width;

            if (w/l < aR) {
                length = (int)(w/aR);
                l = (double)length;
                if (w/l < aR)
                    Debug.Log("length = " + length + ", width = " + width + ", but width/length should be >= " + aR);
            }

            if (l/w < aR) {
                width = (int)(l/aR);
                w = (double)width;
                if (l/w < aR)
                    Debug.Log("length = " + length + ", width = " + width + ", but width/length should be >= " + aR);
            }

            if (w/l < aR) {
                Debug.Log("Room AspectRatio is too big, consider lowering it");
                Debug.Assert(false);
            }

            while(length*width > maxSize) {
                if (length > width)
                    length--;
                else if (width > length)
                    width--;
                else if (DungeonGeneratorCTR.random.Next(100) < 50)
                    length--;
                else
                    width--;
            }

            Debug.Assert(length*width <= maxSize);
            if (length*width >= minSize) {
                RoomCTR newRoom = new RoomCTR();
                newRoom.Width = width;
                newRoom.Length = length;
                
                if (leftFree <= rightFree) {
                    if ((2*leftFree-1) > width) {
                        newRoom.Center = location + forward*(length+1-(length/2));
                        newRoom.Start = location + forward*2 + right*width/2;
                        newRoom.End = location + forward*(length+1) + right*(width/2-width+1);

                        newRoom.AddTile(newRoom.Start, -right, StructureCTR.STType.CORNER);
                        newRoom.AddTile(location+forward*2+right*(width/2-width+1), forward, StructureCTR.STType.CORNER);
                        newRoom.AddTile(newRoom.End, right, StructureCTR.STType.CORNER);
                        newRoom.AddTile(location+forward*(length+1)+right*(width/2), -forward, StructureCTR.STType.CORNER);

                        for (int fwd = 1; fwd <= length; fwd++) {
                            for (int sd = width/2; sd >= width/2-width+1; sd--) {
                                Vector2Int roomSquare = location + (fwd+1)*forward + sd*right;
                                map.SetMapData(roomSquare, SquareData.IR_OPEN);
                                newRoom.AddTile(roomSquare, Vector2Int.zero, StructureCTR.STType.FLOOR);
                            }
                        }
                    } else {
                        newRoom.Center = location + forward*(length+1-(length/2)) + right*(-leftFree+1+width/2);
                        newRoom.Start = location + forward*2 + right*(-leftFree+1);
                        newRoom.End = location + forward*(length+1) + right*(-leftFree+width);

                        newRoom.AddTile(newRoom.Start, forward, StructureCTR.STType.CORNER);
                        newRoom.AddTile(location+forward*2+right*(-leftFree+width), -right, StructureCTR.STType.CORNER);
                        newRoom.AddTile(newRoom.End, -forward, StructureCTR.STType.CORNER);
                        newRoom.AddTile(location+forward*(length+1)+right*(-leftFree+1), right, StructureCTR.STType.CORNER);

                        for (int fwd = 1; fwd <= length; fwd++) {
                            for (int sd = -leftFree+1; sd <= -leftFree+width; sd++) {
                                Vector2Int roomSquare = location + (fwd+1)*forward + sd*right;
                                map.SetMapData(roomSquare, SquareData.IR_OPEN);
                                newRoom.AddTile(roomSquare, Vector2Int.zero, StructureCTR.STType.FLOOR);
                            }
                        }
                    }
                    if (forward.x == 0) {
                        map.SetMapData(location+forward, SquareData.V_DOOR);
                        newRoom.AddTile(location+forward, forward, StructureCTR.STType.DOOR);
                        newRoom.AddTile(location+forward, forward, StructureCTR.STType.FLOOR);
                    } else {
                        Debug.Assert(forward.y == 0);
                        map.SetMapData(location+forward, SquareData.H_DOOR);
                        newRoom.AddTile(location+forward, forward, StructureCTR.STType.DOOR);
                        newRoom.AddTile(location+forward, forward, StructureCTR.STType.FLOOR);  
                    }
                } else {
                    if ((2*rightFree-1) > width) {
                        newRoom.Center = location + forward*(length+1-(length/2));
                        newRoom.Start = location + forward*2 + right*(-width/2);
                        newRoom.End = location + forward*(length+1) + right*(-width/2+width-1);

                        newRoom.AddTile(newRoom.Start, forward, StructureCTR.STType.CORNER);
                        newRoom.AddTile(location+forward*2+right*(-width/2+width-1), -right, StructureCTR.STType.CORNER);
                        newRoom.AddTile(newRoom.End, -forward, StructureCTR.STType.CORNER);
                        newRoom.AddTile(location+forward*(length+1)+right*(-width/2), right, StructureCTR.STType.CORNER);

                        for (int fwd = 1; fwd <= length; fwd++) {
                            for (int sd = -width/2; sd <= -width/2+width-1; sd++) {
                                Vector2Int roomSquare = location + (fwd+1)*forward + sd*right;
                                map.SetMapData(roomSquare, SquareData.IR_OPEN);
                                newRoom.AddTile(roomSquare, Vector2Int.zero, StructureCTR.STType.FLOOR);
                             }
                        }
                    } else {
                        newRoom.Center = location + forward*(length+1-(length/2)) + right*(rightFree-1-width/2);
                        newRoom.Start = location + forward*2 + right*(rightFree-1);
                        newRoom.End = location + forward*(length+1) + right*(rightFree-width);

                        newRoom.AddTile(newRoom.Start, -right, StructureCTR.STType.CORNER);
                        newRoom.AddTile(location+forward*2+right*(rightFree-width), forward, StructureCTR.STType.CORNER);
                        newRoom.AddTile(newRoom.End, right, StructureCTR.STType.CORNER);
                        newRoom.AddTile(location+forward*(length+1)+right*(rightFree-1), -forward, StructureCTR.STType.CORNER);

                        for (int fwd = 1; fwd <= length; fwd++) {
                            for (int sd = rightFree-1; sd >= rightFree-width; sd--) {
                                Vector2Int roomSquare = location + (fwd+1)*forward + sd*right;
                                map.SetMapData(roomSquare, SquareData.IR_OPEN);
                                newRoom.AddTile(roomSquare, Vector2Int.zero, StructureCTR.STType.FLOOR);
                            }
                        }
                    }
                    if (forward.x == 0) {
                        map.SetMapData(location+forward, SquareData.V_DOOR);
                        newRoom.AddTile(location+forward, forward, StructureCTR.STType.DOOR);
                        newRoom.AddTile(location+forward, forward, StructureCTR.STType.FLOOR);
                    } else {
                        Debug.Assert(forward.y == 0);
                        map.SetMapData(location+forward, SquareData.H_DOOR);
                        newRoom.AddTile(location+forward, forward, StructureCTR.STType.DOOR);
                        newRoom.AddTile(location+forward, forward, StructureCTR.STType.FLOOR);
                    }
                }
                map.BuiltRoomD(roomSize);
                newRoom.InDungeon = true;
                map.AddRoom(newRoom);
                return false;
            } else 
                dW++;
        } while (/*(double)frontFree-2 >= (2*(double)dW+1 * aR)*/false);
        return false;
    }
}