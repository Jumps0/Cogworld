using DungeonResources;
using UnityEngine;

public class TunnelerCTR : Builder {

    private Vector2Int desiredDirection;
    private int stepLength;
    private int tunnelWidth;
    private int straightDoubleSpawnProb;
    private int turnDoubleSpawnProb;
    private int changeDirProb;

    private int makeRoomsLeftProb;
    private int makeRoomsRightProb;
    private int joinPreference;
    public int TurnDoubleSpawnProb {get => turnDoubleSpawnProb;}
    public int StraightDoubleSpawnProb {get => straightDoubleSpawnProb;}
    public int ChangeDirProb {get => changeDirProb;}
    public int MakeRoomsLeftProb {get => makeRoomsLeftProb;}
    public int MakeRoomsRightProb {get => makeRoomsRightProb;}
    public TunnelerCTR(ref DungeonGeneratorCTR map, Vector2Int location, Vector2Int forward, int age, int maxAge, int generation, 
    Vector2Int desiredDir, int sL, int tW, int sDSP, int tDSP, int cDP, int mRRP, int mRLP, int jP) : base(ref map, location, forward, age, maxAge, generation) {
        this.desiredDirection = desiredDir;
        this.stepLength = sL;
        this.tunnelWidth = tW;
        this.straightDoubleSpawnProb = sDSP;
        this.turnDoubleSpawnProb = tDSP;
        this.changeDirProb = cDP;
        this.makeRoomsRightProb = mRRP;
        this.makeRoomsLeftProb = mRLP;
        this.joinPreference = jP;
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

        Debug.Assert((position.x >= 0 && position.y >= 0) && (position.x < dX && position.y < dY));

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

        while(frontFree == -1) {
            checkDistance++;
            for (int i = -leftFree; i <= rightFree; i++) {
                test = position + i * right + checkDistance * heading;
                if ((test.x < 0) || (test.y < 0) || (test.x >= dX) || (test.y >= dY)) {
                    frontFree = checkDistance - 1;
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
        if (generation != map.ActiveGeneration) {
            Debug.Assert(generation > map.ActiveGeneration);
            return true;
        }
        age++;
        if (age >= maxAge)
            return false;
        else if (age < 0) 
            return true;


        Debug.Assert(tunnelWidth >= 0);
        int leftFree = tunnelWidth + 1;
        int rightFree = tunnelWidth + 1;
        int frontFree = FrontFree(location, forward, ref leftFree, ref rightFree);
        if (frontFree == 0)
            return false;

        Vector2Int right = Vector2Int.zero;
        Vector2Int left = Vector2Int.zero;
        Vector2Int test = Vector2Int.zero;

        if (forward.x == 0) 
            right = new Vector2Int(forward.y, 0);
        else if (forward.y == 0)
            right = new Vector2Int(0, -forward.x);
        else 
            Debug.Assert(false);

        left = -right;


        RoomSize sizeSideways = RoomSize.NULL, sizeBranching = RoomSize.NULL;

        int probMS, probSS, probMB, probSB;

        probMS = map.GetRoomSizeProbSideways(tunnelWidth, RoomSize.MEDIUM);
        probSS = map.GetRoomSizeProbSideways(tunnelWidth, RoomSize.SMALL);

        probMB = map.GetRoomSizeProbBranching(tunnelWidth, RoomSize.MEDIUM);
        probSB = map.GetRoomSizeProbBranching(tunnelWidth, RoomSize.SMALL);

        int diceRoll = DungeonGeneratorCTR.random.Next(100);
        if (diceRoll < probSS)
            sizeSideways = RoomSize.SMALL;
        else if (diceRoll < (probSS + probMS))
            sizeSideways = RoomSize.MEDIUM;
        else
            sizeSideways = RoomSize.LARGE;

        if (diceRoll < probSB)
            sizeBranching = RoomSize.SMALL;
        else if (diceRoll < (probMB + probSB))
            sizeBranching = RoomSize.MEDIUM;
        else
            sizeBranching = RoomSize.LARGE;
        
        diceRoll = DungeonGeneratorCTR.random.Next(101);
        int roomieGeneration = generation;
        int summedProbs = 0;
        for (int ind = 0; ind <= 10; ind++) {
            summedProbs = summedProbs + map.GetBabyDelayProbsForGenerationR(ind);
            if (diceRoll < summedProbs) {
                roomieGeneration = generation + ind;
                break;
            }
        }

        if (frontFree < (2*stepLength) || (maxAge-1) == age) {
            bool G_CLOSEDAhead = false;
            bool OPENAhead = false;
            bool ROOMAhead = false;
            
            SquareData data;
            int count = 0;
            
            for (int i = -tunnelWidth; i <= tunnelWidth; i++) {
                test = location + (frontFree+1)*forward + i*right;
                data = map.GetMapData(test);

                if (data == SquareData.OPEN || data == SquareData.G_OPEN || data == SquareData.IT_OPEN || data == SquareData.IA_OPEN) {
                    OPENAhead = true;
                    count++;
                } else if (data == SquareData.G_CLOSED || data == SquareData.NJ_G_CLOSED) {
                    G_CLOSEDAhead = true;
                    count = 0;
                } else if (data == SquareData.IR_OPEN) {
                    ROOMAhead = true;
                    count = 0;
                } else
                    count = 0;
            }

            if (((DungeonGeneratorCTR.random.Next(101) <= joinPreference) && ((age < maxAge - 1) || (frontFree <= map.TunnelJoinDistance))) || (frontFree < 5)) {
                //Case full width open ahead - join
                if ((2*tunnelWidth + 1) == count) {
                    BuildTunnel(frontFree, tunnelWidth, true);
                    return false;
                }
                // Its open ahead, but not for full width
                if (OPENAhead) {
                    test = location + (frontFree+1)*forward;
                    data = map.GetMapData(test);
                    if (data == SquareData.OPEN || data == SquareData.G_OPEN || data == SquareData.IT_OPEN || data == SquareData.IA_OPEN) {
                        if (!BuildTunnel(frontFree, 0, true))
                            Debug.Log("OpenAhead failed to join, frontFree = " + frontFree);
                        return false;
                    }

                    int offset = 0;
                    for(int i = 1; i <= tunnelWidth; i++) {
                        test = location + (frontFree+1)*forward + i*right;
                        data = map.GetMapData(test);
                        if (data == SquareData.OPEN || data == SquareData.G_OPEN || data == SquareData.IT_OPEN || data == SquareData.IA_OPEN) {
                            offset = i;
                            break;
                        }
                        test = location + (frontFree+1)*forward - i*right;
                        data = map.GetMapData(test);
                        if (data == SquareData.OPEN || data == SquareData.G_OPEN || data == SquareData.IT_OPEN || data == SquareData.IA_OPEN) {
                            offset = -i;
                            break;
                        }
                    }
                    Debug.Assert(offset != 0);
                    Tunnel tunnel = new Tunnel();
                    tunnel.Width = 1;
                    tunnel.Length = frontFree;
                    tunnel.Center = location + forward*(frontFree/2);
                    tunnel.Start = location + forward;
                    tunnel.End = location + forward*frontFree;
                    for (int i = 1; i <= frontFree; i++) {
                        test = location + i*forward + offset*right;
                        map.SetMapData(test, SquareData.IT_OPEN);
                        tunnel.AddTile(test, Vector2Int.zero, StructureCTR.STType.FLOOR);
                    }
                    map.AddTunnel(tunnel);
                    return false;
                }

                if (ROOMAhead && tunnelWidth == 0) {
                    if (frontFree > 1) {
                        test = location + (frontFree+1)*forward;
                        data = map.GetMapData(test);
                        Debug.Assert(data == SquareData.IR_OPEN);
                        BuildTunnel(frontFree-1, 0, true);
                        if (forward.x == 0) {
                            map.SetMapData(location + frontFree*forward, SquareData.V_DOOR);
                            RoomCTR r = map.FindRoom(location + (frontFree+1)*forward);
                            r.AddTile(location + frontFree*forward, forward, StructureCTR.STType.DOOR);
                            r.AddTile(location+frontFree*forward, forward, StructureCTR.STType.FLOOR);
                        } else {
                            Debug.Assert(forward.y == 0);
                            map.SetMapData(location + (frontFree)*forward, SquareData.H_DOOR);
                            RoomCTR r = map.FindRoom(location + (frontFree+1)*forward);
                            r.AddTile(location + frontFree*forward, forward, StructureCTR.STType.DOOR);
                            r.AddTile(location+frontFree*forward, forward, StructureCTR.STType.FLOOR);
                        }
                        return false;
                    }
                }
                
                if (G_CLOSEDAhead) {
                    if (tunnelWidth == 0) {
                        int jP = DungeonGeneratorCTR.random.Next(11) * 10;
                        if (leftFree >= rightFree) {
                            if ((joinPreference != 100) || (makeRoomsLeftProb != 20) || (makeRoomsRightProb != 20) || (changeDirProb != 30) || (straightDoubleSpawnProb != 0) || (turnDoubleSpawnProb != 0) || (tunnelWidth != 0))
                                map.CreateTunneler(location, -right, 0, maxAge, generation+1, -right, 3, 0, 0, 0, 30, 20, 20, jP);
                        } else {
                            if ((joinPreference != 100) || (makeRoomsLeftProb != 20) || (makeRoomsRightProb != 20) || (changeDirProb != 30) || (straightDoubleSpawnProb != 0) || (turnDoubleSpawnProb != 0) || (tunnelWidth != 0))
                                map.CreateTunneler(location, right, 0, maxAge, generation+1, right, 3, 0, 0, 0, 30, 20, 20, jP);
                        }
                        return false;
                    }
                }

                if (!OPENAhead && !G_CLOSEDAhead) {
                    bool weHaveSpecialCase = CheckRow(frontFree+1, tunnelWidth, right, true, false);
                    if (weHaveSpecialCase) {
                        Debug.Assert(BuildTunnel(frontFree, tunnelWidth, true));
                        Tunnel tunnel = new Tunnel();
                        tunnel.Width = tunnelWidth*2;
                        tunnel.Start = location + forward*(frontFree+1);
                        for (int i = -tunnelWidth; i <= tunnelWidth; i++) {
                            map.SetMapData(location + (frontFree+1)*forward + i*right, SquareData.IT_OPEN);
                            tunnel.AddTile(location + (frontFree+1)*forward + i*right, Vector2Int.zero, StructureCTR.STType.FLOOR);
                        }
                        int fwd = frontFree + 2;
                        bool contactInNextRow = true;
                        bool rowAfterIsOk = true;
                        while (contactInNextRow && rowAfterIsOk) {
                            contactInNextRow = CheckRow(fwd, tunnelWidth, right, false, false);
                            if (!contactInNextRow)
                                break;
                                                        
                            rowAfterIsOk = CheckRow(fwd+1, tunnelWidth, right, false, true);

                            bool allOpen = true;
                            for (int i = -tunnelWidth-1; i <= tunnelWidth+1; i++) {
                                test = location + (fwd+1)*forward + i*right;
                                data = map.GetMapData(test);
                                if (data != SquareData.IT_OPEN && data != SquareData.IA_OPEN)
                                    allOpen = false;
                            }
                            if (allOpen)
                                rowAfterIsOk = true;

                            if (contactInNextRow && rowAfterIsOk) {
                                for (int i = -tunnelWidth; i <= tunnelWidth; i++)  {
                                    map.SetMapData(location + fwd*forward + i*right, SquareData.IT_OPEN);
                                    tunnel.AddTile(location + fwd*forward + i*right, Vector2Int.zero, StructureCTR.STType.FLOOR);
                                }
                            }
                            fwd++; 
                            tunnel.Length = fwd;
                            tunnel.Center = location + forward*(fwd/2);
                            tunnel.End = location + forward*fwd;
                        }
                        map.AddTunnel(tunnel);
                        return false;
                    }   
                    if (tunnelWidth == 0) {
                        if (map.GetMapData(location + (frontFree+1)*forward) == SquareData.CLOSED) {
                            if (map.GetMapData(location + (frontFree+1)*forward + right) == SquareData.IR_OPEN) {
                                forward = -right;
                                if (forward == -desiredDirection)
                                    forward = desiredDirection;
                                return true;
                            } else if (map.GetMapData(location + (frontFree+1)*forward - right) == SquareData.IR_OPEN) {
                                forward = right;
                                if (forward == -desiredDirection)
                                    forward = desiredDirection;
                                return true;
                            }
                        }
                    }
                } // OPEN && G_CLOSED AHEAD
            } // End Join tunnel
            
            if (map.WantsMoreRoomsD(sizeBranching)) {
                int dW = 2*tunnelWidth;
                if (dW < 1)
                    dW = 1;
                map.CreateRoomie(location, forward, 0, 2, generation, dW, sizeBranching, 0);
            }
            int joinPref = DungeonGeneratorCTR.random.Next(11) * 10;

            if (joinPreference != 100 || makeRoomsLeftProb != map.LastChanceRoomsLeftProb || makeRoomsRightProb != map.LastChanceRoomsRightProb || changeDirProb != map.LastChanceChangeDirProb || straightDoubleSpawnProb != map.LastChanceStraightSpawnProb || turnDoubleSpawnProb != map.LastChanceTurnSpawnProb || tunnelWidth != 0) {
                int lF = tunnelWidth + 1;
                int rF = tunnelWidth + 1;
                
                lF = tunnelWidth + 1;
                rF = tunnelWidth + 1;
                int fFR = FrontFree(location + tunnelWidth*right, right, ref lF, ref rF);

                lF = tunnelWidth + 1;
                rF = tunnelWidth + 1;
                int fFL = FrontFree(location - tunnelWidth*right, left, ref lF, ref rF);

                lF = tunnelWidth + 1;
                rF = tunnelWidth + 1;
                int fFB = FrontFree(location, -forward, ref lF, ref rF);

                if (tunnelWidth == 0) {
                    if ((makeRoomsLeftProb == map.LastChanceRoomsLeftProb) && (makeRoomsRightProb == map.LastChanceRoomsRightProb) && (changeDirProb == map.LastChanceChangeDirProb) && (straightDoubleSpawnProb == map.LastChanceStraightSpawnProb) && (turnDoubleSpawnProb == map.LastChanceTurnSpawnProb)) {
                        if (frontFree >= fFR && frontFree >= fFL && frontFree >= fFB)
                            map.CreateTunneler(location, forward, 0, maxAge, generation+1, forward, 3, 0, map.LastChanceStraightSpawnProb, map.LastChanceTurnSpawnProb, map.LastChanceChangeDirProb, map.LastChanceRoomsRightProb, map.LastChanceRoomsLeftProb, joinPref);
                        else if (fFB >= fFR && fFB >= fFL)
                            map.CreateTunneler(location, -forward, 0, maxAge, generation+map.LastChanceGenDelay, -forward, 3, 0, map.LastChanceStraightSpawnProb, map.LastChanceTurnSpawnProb, map.LastChanceChangeDirProb, map.LastChanceRoomsRightProb, map.LastChanceRoomsLeftProb, joinPref);
                        else if ((fFR >= fFL) || ((fFR == fFL) && (DungeonGeneratorCTR.random.Next(100) < 50)))
                            map.CreateTunneler(location, right, 0, maxAge, generation+map.LastChanceGenDelay, right, 3, 0, map.LastChanceStraightSpawnProb, map.LastChanceTurnSpawnProb, map.LastChanceChangeDirProb, map.LastChanceRoomsRightProb, map.LastChanceRoomsLeftProb, joinPref);
                        else
                            map.CreateTunneler(location, left, 0, maxAge, generation+map.LastChanceGenDelay, left, 3, 0, map.LastChanceStraightSpawnProb, map.LastChanceTurnSpawnProb, map.LastChanceChangeDirProb, map.LastChanceRoomsRightProb, map.LastChanceRoomsLeftProb, joinPref);
                    } else
                        map.CreateTunneler(location, forward, 0, maxAge, generation+map.LastChanceGenDelay, forward, 3, 0, map.LastChanceStraightSpawnProb, map.LastChanceTurnSpawnProb, map.LastChanceChangeDirProb, map.LastChanceRoomsRightProb, map.LastChanceRoomsLeftProb, joinPref);
                } else {
                    if (G_CLOSEDAhead) {
                        map.CreateTunneler(location + tunnelWidth*right, right, 0, maxAge, generation+map.LastChanceGenDelay, right, 3, 0, map.LastChanceStraightSpawnProb, map.LastChanceTurnSpawnProb, map.LastChanceChangeDirProb, map.LastChanceRoomsRightProb, map.LastChanceRoomsLeftProb, joinPref);
                        map.CreateTunneler(location - tunnelWidth*right, left, 0, maxAge, generation+map.LastChanceGenDelay, left, 3, 0, map.LastChanceStraightSpawnProb, map.LastChanceTurnSpawnProb, map.LastChanceChangeDirProb, map.LastChanceRoomsRightProb, map.LastChanceRoomsLeftProb, joinPref);
                    } else if (ROOMAhead) {
                        if ((fFR >= fFL) || (fFR == fFL) && DungeonGeneratorCTR.random.Next(100) < 50) {
                            map.CreateTunneler(location + tunnelWidth*right, right, 0, maxAge, generation+map.LastChanceGenDelay, right, 3, 0, map.LastChanceStraightSpawnProb, map.LastChanceTurnSpawnProb, map.LastChanceChangeDirProb, map.LastChanceRoomsRightProb, map.LastChanceRoomsLeftProb, joinPref);
                            map.CreateTunneler(location - tunnelWidth*right, forward, 0, maxAge, generation+map.LastChanceGenDelay, forward, 3, 0, map.LastChanceStraightSpawnProb, map.LastChanceTurnSpawnProb, map.LastChanceChangeDirProb, map.LastChanceRoomsRightProb, map.LastChanceRoomsLeftProb, joinPref);
                        } else {
                            map.CreateTunneler(location + tunnelWidth*right, forward, 0, maxAge, generation+map.LastChanceGenDelay, forward, 3, 0, map.LastChanceStraightSpawnProb, map.LastChanceTurnSpawnProb, map.LastChanceChangeDirProb, map.LastChanceRoomsRightProb, map.LastChanceRoomsLeftProb, joinPref);
                            map.CreateTunneler(location - tunnelWidth*right, left, 0, maxAge, generation+map.LastChanceGenDelay, left, 3, 0, map.LastChanceStraightSpawnProb, map.LastChanceTurnSpawnProb, map.LastChanceChangeDirProb, map.LastChanceRoomsRightProb, map.LastChanceRoomsLeftProb, joinPref);
                        }
                    } else {
                        map.CreateTunneler(location + tunnelWidth*right, forward, 0, maxAge, generation+map.LastChanceGenDelay, forward, 3, 0, map.LastChanceStraightSpawnProb, map.LastChanceTurnSpawnProb, map.LastChanceChangeDirProb, map.LastChanceRoomsRightProb, map.LastChanceRoomsLeftProb, joinPref);
                        map.CreateTunneler(location - tunnelWidth*right, forward, 0, maxAge, generation+map.LastChanceGenDelay, forward, 3, 0, map.LastChanceStraightSpawnProb, map.LastChanceTurnSpawnProb, map.LastChanceChangeDirProb, map.LastChanceRoomsRightProb, map.LastChanceRoomsLeftProb, joinPref);
                    } 
                }
            }
            return false;
        } // End ran out of room
        Debug.Assert(frontFree >= 2*stepLength);
        Debug.Assert(stepLength > 0);

        BuildTunnel(stepLength, tunnelWidth);

        if (DungeonGeneratorCTR.random.Next(100) < makeRoomsRightProb) {
            Vector2Int spawnPoint = location + (stepLength/2 + 1)*forward + tunnelWidth*right;
            int dW = stepLength/2 - 1;
            if (dW < 1)
                dW = 1;
            map.CreateRoomie(spawnPoint, right, -1, 2, roomieGeneration, dW, sizeSideways, 0);
        }
        if (DungeonGeneratorCTR.random.Next(100) < MakeRoomsLeftProb) {
            Vector2Int spawnPoint = location + (stepLength/2 + 1)*forward + tunnelWidth*left;
            int dW = stepLength/2 - 1;
            if (dW < 1)
                dW = 1;
            map.CreateRoomie(spawnPoint, left, -1, 2, roomieGeneration, dW, sizeSideways, 0);
        }
        location = location + stepLength*forward;

        bool smallAnteRoomPossible = false;
        bool largeAnteRoomPossible = false;

        Debug.Assert(tunnelWidth >= 0);
        leftFree = tunnelWidth + 2;
        rightFree = tunnelWidth + 2;
        Debug.Assert(map.GetMapData(location) == SquareData.IT_OPEN);
        map.SetMapData(location, SquareData.CLOSED);
        for (int m = 1; m <= tunnelWidth; m++) {
            Debug.Assert(map.GetMapData(location + m*right) == SquareData.IT_OPEN);
            Debug.Assert(map.GetMapData(location - m*right) == SquareData.IT_OPEN);
            map.SetMapData(location + m*right, SquareData.CLOSED);
            map.SetMapData(location - m*right, SquareData.CLOSED);
        }
        frontFree = FrontFree(location - forward, forward, ref leftFree, ref rightFree);
        if (frontFree >= 2*tunnelWidth+5)
            smallAnteRoomPossible = true;
        map.SetMapData(location, SquareData.IT_OPEN);
        for (int m = 1; m <= tunnelWidth; m++) {
            map.SetMapData(location + m*right, SquareData.IT_OPEN);
            map.SetMapData(location - m*right, SquareData.IT_OPEN);
        }
        leftFree = tunnelWidth + 3;
        rightFree = tunnelWidth + 3;
        Debug.Assert(map.GetMapData(location) == SquareData.IT_OPEN);
        map.SetMapData(location, SquareData.CLOSED);
        for (int m = 1; m <= tunnelWidth; m++) {
            Debug.Assert(map.GetMapData(location + m*right) == SquareData.IT_OPEN);
            Debug.Assert(map.GetMapData(location - m*right) == SquareData.IT_OPEN);
            map.SetMapData(location + m*right, SquareData.CLOSED);
            map.SetMapData(location - m*right, SquareData.CLOSED);
        }
        frontFree = FrontFree(location - forward, forward, ref leftFree, ref rightFree);
        if (frontFree >= 2*tunnelWidth+7)
            largeAnteRoomPossible = true;
        map.SetMapData(location, SquareData.IT_OPEN);
        for (int m = 1; m <= tunnelWidth; m++) {
            map.SetMapData(location + m*right, SquareData.IT_OPEN);
            map.SetMapData(location - m*right, SquareData.IT_OPEN);
        }

        bool sizeUpTunnel = false;
        bool sizeDownTunnel = false;

        diceRoll = DungeonGeneratorCTR.random.Next(101);
        int sizeUpProb = map.GetSizeUpProb(generation);
        int sizeDownProb = sizeUpProb + map.GetSizeDownProb(generation);
        if (diceRoll < sizeUpProb)
            sizeUpTunnel = true;
        else if (diceRoll < sizeDownProb)
            sizeDownTunnel = true;

        if (sizeUpTunnel && !largeAnteRoomPossible) 
            return true;

        bool changeDir = false;
        if (DungeonGeneratorCTR.random.Next(100) < changeDirProb)
            changeDir = true;

        bool doSpawn = false;
        if (changeDir && (DungeonGeneratorCTR.random.Next(101) < turnDoubleSpawnProb))
            doSpawn = true;
        if (!changeDir && (DungeonGeneratorCTR.random.Next(101) < straightDoubleSpawnProb))
            doSpawn = true;
        if (!changeDir && !doSpawn)
            return true;

        bool doSpawnRoom = false;
        if (doSpawn && (DungeonGeneratorCTR.random.Next(101) > map.Patiente))
            doSpawnRoom = true;
        diceRoll = DungeonGeneratorCTR.random.Next(101);
        int babyGeneration = generation + 1;
        summedProbs = 0;
        if (doSpawn) {
            if (!sizeUpTunnel) {
                for (int ind = 0; ind <= 10; ind++) {
                    summedProbs = summedProbs + map.GetBabyDelayProbsForGenerationT(ind);
                    if (diceRoll < summedProbs) {
                        babyGeneration = generation + ind;
                        break;
                    }
                }
            } else {
                babyGeneration = generation + map.SizeUpGenDelay;
            }
        }

        int sDSP = map.Mutate(straightDoubleSpawnProb);
        int tDSP = map.Mutate(turnDoubleSpawnProb);
        int cDP = map.Mutate(changeDirProb);
        int mRRP = map.Mutate(makeRoomsRightProb);
        int mRLP = map.Mutate(makeRoomsLeftProb);
        int joinPr = map.Mutate(joinPreference);

        bool TEST = false;
        Vector2Int SpawnPointForward = Vector2Int.zero;
        Vector2Int SpawnPointRight = Vector2Int.zero;
        Vector2Int SpawnPointLeft = Vector2Int.zero;

        bool usedRight = false;
        bool usedLeft = false;
        bool builtAnteRoom = false;
        if (sizeUpTunnel) {
            if (DungeonGeneratorCTR.random.Next(100) < map.GetAnteRoomProb(tunnelWidth) || doSpawn) {
                TEST = BuildAnteRoom(2 * tunnelWidth + 5, tunnelWidth + 2, right);
                SpawnPointForward = location + (2*tunnelWidth+5) * forward;
                SpawnPointRight = location + (tunnelWidth+3) * forward + (tunnelWidth+2) * right;
                SpawnPointLeft = location + (tunnelWidth+3) * forward + (tunnelWidth+2) * left;
                Debug.Assert(TEST);
                builtAnteRoom = true;
            } else {
                SpawnPointForward = location;
                SpawnPointRight = location - tunnelWidth * forward + tunnelWidth * right;
                SpawnPointLeft = location - tunnelWidth * forward + tunnelWidth * left;
                if ((map.GetMapData(SpawnPointRight) != SquareData.IT_OPEN) || (map.GetMapData(SpawnPointLeft) != SquareData.IT_OPEN)) 
                    return true;
            }
        } else {
            if (DungeonGeneratorCTR.random.Next(100) < map.GetAnteRoomProb(tunnelWidth) && smallAnteRoomPossible) {
                TEST = BuildAnteRoom(2 * tunnelWidth + 3, tunnelWidth + 1, right);
                SpawnPointForward = location + (2*tunnelWidth+3) * forward;
                SpawnPointRight = location + (tunnelWidth+2) * forward + (tunnelWidth+1) * right;
                SpawnPointLeft = location + (tunnelWidth+2) * forward + (tunnelWidth+1) * left;
                Debug.Assert(TEST);
                builtAnteRoom = true;
            } else {
                SpawnPointForward = location;
                SpawnPointRight = location - tunnelWidth * forward + tunnelWidth * right;
                SpawnPointLeft = location - tunnelWidth * forward + tunnelWidth * left;
                if ((map.GetMapData(SpawnPointRight) != SquareData.IT_OPEN) || (map.GetMapData(SpawnPointLeft) != SquareData.IT_OPEN))
                    return true;
            }
        }

        Vector2Int oldForward = new Vector2Int(forward.x, forward.y);
        if (changeDir) {
            int lF = tunnelWidth + 1;
            int rF = tunnelWidth + 1;
            int fFR = FrontFree(SpawnPointRight, right, ref lF, ref rF);
            lF = tunnelWidth + 1;
            rF = tunnelWidth + 1;
            int fFL = FrontFree(SpawnPointLeft, left, ref lF, ref rF);

            if ((desiredDirection == Vector2Int.zero) || desiredDirection == forward) {
                if (!sizeUpTunnel || !doSpawn) {
                    if ((fFR > fFL) || (fFR == fFL && DungeonGeneratorCTR.random.Next(2) == 0)) {
                        if (fFR > 0) {
                            location = SpawnPointRight;
                            forward = right;
                            usedRight = true;
                        } 
                    } else if (fFL > 0) {
                        location = SpawnPointLeft;
                        forward = left;
                        usedLeft = true;
                    }
                } else {
                        Debug.Assert(doSpawn);
                        if ((fFR < fFL) || (fFR == fFL && DungeonGeneratorCTR.random.Next(2) == 0)) {
                            if (fFR > 0) {
                            location = SpawnPointRight;
                            forward = right;
                            usedRight = true;
                            } 
                        } else if (fFL > 0) {
                            location = SpawnPointLeft;
                            forward = left;
                            usedLeft = true;
                        }
                    }
            } else {
                if (desiredDirection.x == 0 || desiredDirection.y == 0) {
                    forward = desiredDirection;
                    if (forward == right) {
                        if (fFR > 0) {
                            usedRight = true;
                            location = SpawnPointRight;
                        }
                    } else if (fFL > 0) {
                        Debug.Assert(forward == left);
                        location = SpawnPointLeft;
                        usedLeft = true;
                    } 
                } else {
                    Debug.Assert(desiredDirection.x != 0 && desiredDirection.y != 0);
                    forward = desiredDirection - forward;
                    if (forward == right) {
                        if (fFR > 0) {
                            usedRight = true;
                            location = SpawnPointRight;
                        }
                    } else if (fFL > 0) {
                        Debug.Assert(forward == left);
                        location = SpawnPointLeft;
                        usedLeft = true;
                    }
                }
            }
            if (doSpawn) {
                Vector2Int SpawnPoint = Vector2Int.zero;
                Vector2Int SpawnDir = Vector2Int.zero;
                if (usedLeft) {
                    SpawnPoint = SpawnPointRight;
                    SpawnDir = right;
                } else if (usedRight) {
                    SpawnPoint = SpawnPointLeft;
                    SpawnDir = left;
                } else {
                    GoStraight(SpawnPointForward, SpawnPointRight, SpawnPointLeft, right, doSpawnRoom, builtAnteRoom, sizeUpTunnel, sizeDownTunnel, roomieGeneration, babyGeneration, sDSP, tDSP, cDP, mRRP, mRLP, joinPr, sizeBranching, sizeSideways);
                    return true;
                }

                diceRoll = DungeonGeneratorCTR.random.Next(100);
                if (doSpawnRoom && (diceRoll < 50)) {
                    int dW = 2*tunnelWidth;
                    if (dW < 1)
                        dW = 1;
                    int rG = roomieGeneration;
                    if (builtAnteRoom)
                        rG = generation + (roomieGeneration - generation)/map.GenSpeedUpOnAnteRoom;
                    map.CreateRoomie(SpawnPoint, SpawnDir, 0, 2, rG, dW, sizeBranching, 0);
                } else {
                    int tW = tunnelWidth;
                    int sL = stepLength;
                    if (sizeUpTunnel) {
                        tW++;
                        sL = sL + 2;
                    } else if (sizeDownTunnel) {
                        tW--;
                        if (tW < 0)
                            tW = 0;
                        sL = sL - 2;
                        if (sL < 3)
                            sL = 3;
                    }
                    map.CreateTunneler(SpawnPoint, SpawnDir, 0, map.GetMaxAgeT(babyGeneration), babyGeneration, SpawnDir, sL, tW, sDSP, tDSP, cDP, mRRP, mRLP, joinPr);
                }

                if (doSpawnRoom && diceRoll >= 50) {
                    int dW = 2*tunnelWidth;
                    if (dW < 1)
                        dW = 1;
                    int rG = roomieGeneration;
                    if (builtAnteRoom)
                        rG = generation + (roomieGeneration - generation)/map.GenSpeedUpOnAnteRoom;
                    map.CreateRoomie(SpawnPointForward, oldForward, 0, 2, rG, dW, sizeBranching, 0);
                } else {
                    int tW = tunnelWidth;
                    int sL = stepLength;
                    if (sizeUpTunnel) {
                        tW++;
                        sL = sL + 2;
                    } else if (sizeDownTunnel) {
                        tW--;
                        if (tW < 0)
                            tW = 0;
                        sL = sL - 2;
                        if (sL < 3)
                            sL = 3;
                    }
                    map.CreateTunneler(SpawnPointForward, oldForward, 0, map.GetMaxAgeT(babyGeneration), babyGeneration, oldForward, sL, tW, sDSP, tDSP, cDP, mRRP, mRLP, joinPr);
                }
            }
        } else 
            GoStraight(SpawnPointForward, SpawnPointRight, SpawnPointLeft, right, doSpawnRoom, builtAnteRoom, sizeUpTunnel, sizeDownTunnel, roomieGeneration, babyGeneration, sDSP, tDSP, cDP, mRRP, mRLP, joinPr, sizeBranching, sizeSideways);
        return true;
    }

    private void GoStraight(Vector2Int SpawnPointForward, Vector2Int SpawnPointRight, Vector2Int SpawnPointLeft, Vector2Int right, bool doSpawnRoom, bool builtAnteRoom, bool sizeUpTunnel, bool sizeDownTunnel, int rG, int bG, int sDSP, int tDSP, int cDP, int mRRP, int mRLP, int jP, RoomSize szB, RoomSize szS) {
        location = SpawnPointForward;
        int diceRoll = DungeonGeneratorCTR.random.Next(100);
        if (doSpawnRoom && diceRoll < 50) {
            int dW = 2*tunnelWidth;
            if (dW < 1)
                dW = 1;
            int roomieGen = rG;
            if (builtAnteRoom)
                roomieGen = generation + (rG - generation)/map.GenSpeedUpOnAnteRoom;
            map.CreateRoomie(SpawnPointRight, right, 0, 2, roomieGen, dW, szB, 0);
        } else {
            int tW = tunnelWidth;
            int sL = stepLength;
            if (sizeUpTunnel) {
                tW++;
                sL = sL + 2;
            } else if (sizeDownTunnel) {
                tW--;
                if (tW < 0)
                    tW = 0;
                sL = sL - 2;
                if (sL < 3)
                    sL = 3;
            }
            map.CreateTunneler(SpawnPointRight, right, 0, map.GetMaxAgeT(bG), bG, right, sL, tW, sDSP, tDSP, cDP, mRRP, mRLP, jP);
        }

        if (doSpawnRoom && diceRoll >= 50) {
            int dW = 2*tunnelWidth;
            if (dW < 1)
                dW = 1;
            int roomieGen = rG;
            if (builtAnteRoom)
                roomieGen = generation + (rG - generation)/map.GenSpeedUpOnAnteRoom;
            map.CreateRoomie(SpawnPointLeft, -right, 0, 2, roomieGen, dW, szB, 0);
        } else {
            int tW = tunnelWidth;
            int sL = stepLength;
            if (sizeUpTunnel) {
                tW++;
                sL = sL + 2;
            } else if (sizeDownTunnel) {
                tW--;
                if (tW < 0)
                    tW = 0;
                sL = sL - 2;
                if (sL < 3)
                    sL = 3;
            }
            map.CreateTunneler(SpawnPointLeft, -right, 0, map.GetMaxAgeT(bG), bG, -right, sL, tW, sDSP, tDSP, cDP, mRRP, mRLP, jP);
        }
    }

    private bool CheckRow(int row, int tunnelWidth, Vector2Int right, bool rowAfter, bool extraCondition) {
        Vector2Int test = Vector2Int.zero;
        SquareData data = SquareData.NULL;
        for (int i = -tunnelWidth; i <= tunnelWidth; i++) {
            test = location + (row)*forward + i*right;
            data = map.GetMapData(test);
            if (data != SquareData.CLOSED)
                    return false;
        }
            Vector2Int testR = location + (row)*forward + (tunnelWidth+1)*right;
            Vector2Int testL = location + (row)*forward - (tunnelWidth+1)*right;
            SquareData dataR = map.GetMapData(testR);
            SquareData dataL = map.GetMapData(testL);

            if (extraCondition)
                if (!(dataR == SquareData.CLOSED || dataL == SquareData.CLOSED))
                    return false;
                    
            if (!(dataR == SquareData.OPEN || dataR == SquareData.G_OPEN || dataR == SquareData.IA_OPEN || dataR == SquareData.IT_OPEN ||
                  dataL == SquareData.OPEN || dataL == SquareData.G_OPEN || dataL == SquareData.IA_OPEN || dataL == SquareData.IT_OPEN))
                return false;
            if (dataR == SquareData.IR_OPEN || dataL == SquareData.IR_OPEN)
                return false;

            if (rowAfter) {
                for (int i = -tunnelWidth-1; i <= tunnelWidth+1; i++) {
                    test = location + (row+1)*forward + i*right;
                    data = map.GetMapData(test);
                    if (data == SquareData.IR_OPEN)
                        return false;
                } 
            }        
        return true;
    }

    public bool BuildAnteRoom(int length, int width, Vector2Int right) {
        if (length < 3 || width < 1) {
            Debug.Log("AnteRoom must be at least 3x3 length = " + length);
            return false;
        }

        int lF = width + 1;
        int rF = width + 1;
        int frontFree = FrontFree(location, forward, ref lF, ref rF);

        if (frontFree <= length)
            return false;

        Vector2Int current = Vector2Int.zero;

        AnteRoom anteRoom = new AnteRoom();
        anteRoom.Width = width;
        anteRoom.Length = length;
        anteRoom.Center = location + forward*length/2;
        anteRoom.Start = location + forward + right*(-width);
        anteRoom.End = location + forward*length + right*width;
        int fwd = -1, side = -1;
        for (fwd = 1; fwd <= length; fwd++) {
            for (side = -width; side <= width; side++) {
                current = location + fwd*forward + side*right;
                map.SetMapData(current, SquareData.IA_OPEN);
                anteRoom.AddTile(current, Vector2Int.zero, StructureCTR.STType.FLOOR);
            }
        }

        if ((width >= 3) && (length >= 7) && map.ColumnsInTunnels) {
            fwd = 2;
            side = -width + 1;
            current = location + fwd*forward + side*right;
            map.SetMapData(current, SquareData.COLUMN);
            map.SetLightAt(current);
            anteRoom.AddTile(current, right, StructureCTR.STType.COLUMN);
            
            side = width - 1;
            current = location + fwd*forward + side*right;
            map.SetMapData(current, SquareData.COLUMN);
            anteRoom.AddTile(current, -right, StructureCTR.STType.COLUMN);

            fwd = length-1;
            side = -width + 1;
            current = location + fwd*forward + side*right;
            map.SetMapData(current, SquareData.COLUMN);
            anteRoom.AddTile(current, right, StructureCTR.STType.COLUMN);

            side = width - 1;
            current = location + fwd*forward + side*right;
            map.SetMapData(current, SquareData.COLUMN);
            anteRoom.AddTile(current, -right, StructureCTR.STType.COLUMN);
        }
        map.AddAnteRoom(anteRoom);
        return true;
    }

    public bool BuildAnteRoom(Vector2Int lB, Vector2Int rT) {
        int xL = rT.x - lB.x;
        int xLength, xIncr;
        if (xL >= 0) {
            xLength = xL;
            xIncr = 1;
        } else {
            xLength = -xL;
            xIncr = -1;
        }

        int yL = rT.y - lB.y;
        int yLength, yIncr;
        if (yL >= 0) {
            yLength = yL;
            yIncr = 1;
        } else {
            yLength = -yL;
            yIncr = -1;
        }

        if (xLength < 3 || yLength < 3)
            return false;
        
        Vector2Int xDir = new Vector2Int(1, 0);
        Vector2Int yDir = new Vector2Int(0, 1);
        Vector2Int current = Vector2Int.zero;
        SquareData dataAtTest = SquareData.NULL;
        int x, y;
        for (x = 0; x <= xLength; x++) {
            for (y = 0; y <= yLength; y++) {
                current = location + ((x*xIncr) * xDir) + ((y*yIncr) * yDir);
                dataAtTest = map.GetMapData(current);
                if ((dataAtTest != SquareData.CLOSED) && (dataAtTest != SquareData.NJ_CLOSED))
                    return false;
            }
        }
        AnteRoom anteRoom = new AnteRoom();
        anteRoom.Width = xLength;
        anteRoom.Length = yLength;
        anteRoom.Center = location + (((xLength/2)*xIncr) * xDir) + (((yLength/2)*yIncr) * yDir);
        anteRoom.Start = location + xDir + yDir;
        anteRoom.End = location + ((xLength*xIncr) * xDir) + ((yLength*yDir) * yDir);
        for (x = 0; x <= xLength; x++) {
            for (y = 0; y <= yLength; y++) {
                current = location + ((x*xIncr) * xDir) + ((y*yIncr) * yDir);
                map.SetMapData(current, SquareData.IA_OPEN);
                anteRoom.AddTile(current, Vector2Int.zero, StructureCTR.STType.FLOOR);
            }
        }


        if (xLength >= 5 && yLength >= 5 && map.ColumnsInTunnels) {
            x = xLength - 1;
            y = 1;
            current = location + ((x*xIncr) * xDir) + ((y*yIncr) * yDir);
            map.SetMapData(current, SquareData.COLUMN);
            anteRoom.AddTile(current, Vector2Int.up, StructureCTR.STType.COLUMN);

            x = 1;
            y = yLength - 1;
            current = location + ((x*xIncr) * xDir) + ((y*yIncr) * yDir);
            map.SetMapData(current, SquareData.COLUMN);
            anteRoom.AddTile(current, Vector2Int.up, StructureCTR.STType.COLUMN);
        }
        map.AddAnteRoom(anteRoom);
        return true;
    }

    public bool BuildTunnel(int length, int width, bool joining = false) {
        if (length < 1 || width < 0) {
            Debug.Log("Trying to build zero size tunnel with length = " + length + " width = " + width);
            return false;
        }

        int lF = width + 1;
        int rF = width + 1;
        int frontFree = FrontFree(location, forward, ref lF, ref rF);

        if (frontFree < length)
            return false;

        Vector2Int right = Vector2Int.zero;
        Vector2Int current = Vector2Int.zero;

        if (forward.x == 0)
            right = new Vector2Int(forward.y, 0);
        else if (forward.y == 0)
            right = new Vector2Int(0, -forward.x);
        else
            Debug.Assert(false);

        Tunnel tunnel = new Tunnel();
        tunnel.Width = width+1;
        tunnel.Length = length;
        tunnel.Center = location + forward*(length/2);
        tunnel.Start = location + forward + right*-width;
        tunnel.End = location + forward*length + right*width;

        int fwd, side;
        for (fwd = 1; fwd <= length; fwd++) {
            for (side = -width; side <= width; side++) {
                current = location + fwd*forward + side*right;
                map.SetMapData(current, SquareData.IT_OPEN);
                tunnel.AddTile(current, Vector2Int.zero, StructureCTR.STType.FLOOR);
            }
        }

        if (width >= 3 && length >= 7 && map.ColumnsInTunnels) {
            int numCols = (length-1)/6;
            Debug.Assert(numCols>0);
            for (int i = 0; i < numCols; i++) {
                fwd = 2 + i*3;
                side = -width + 1;
                current = location + fwd*forward + side*right;
                map.SetMapData(current, SquareData.COLUMN);
                tunnel.AddTile(current, right, StructureCTR.STType.COLUMN);
                

                side = width - 1;
                current = location + fwd*forward + side*right;
                map.SetMapData(current, SquareData.COLUMN);
                tunnel.AddTile(current, -right, StructureCTR.STType.COLUMN);
                

                fwd = (length-1) - i*3;
                side = -width + 1;
                current = location + fwd*forward + side*right;
                map.SetMapData(current, SquareData.COLUMN);
                tunnel.AddTile(current, right, StructureCTR.STType.COLUMN);

                side = width - 1;
                current = location + fwd*forward + side*right;
                map.SetMapData(current, SquareData.COLUMN);
                tunnel.AddTile(current, -right, StructureCTR.STType.COLUMN);
            }
        } 
        map.AddTunnel(tunnel);
        return true;
    }
}
