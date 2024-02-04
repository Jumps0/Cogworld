using UnityEngine;
using Resources;
public class WallCrawler : Builder {
    private Vector2Int desiredDir;           
                                    
    private int stepLength;                
    private int opening;                   
                                   
    private int corridorWidth;             
    private int straightSingleSpawnProb;   
    private int straightDoubleSpawnProb;   
    private int turnSingleSpawnProb;       
    private int turnDoubleSpawnProb;       
    private int changeDirProb;    

    public WallCrawler(ref DungeonGeneratorCTR _map, Vector2Int _location, Vector2Int _forward, int _age, int _maxAge, int _generation,
                        Vector2Int dir, int sL, int op, int cW, int sSS, int sDS, int tSS, int tDS, int cDP) : base(ref _map, _location, _forward, _age, _maxAge, _generation) {
        desiredDir = dir;
        stepLength = sL;
        opening = op;
        corridorWidth = cW;
        straightSingleSpawnProb = sSS;
        straightDoubleSpawnProb = sDS;
        turnSingleSpawnProb = tSS;
        turnDoubleSpawnProb = tDS;
        changeDirProb = cDP;
    }

    private void CheckSides(Vector2Int position, Vector2Int heading, Vector2Int rightDir, ref int side, int frontFree, int sizeX, int sizeY, int factor) {
        Vector2Int testDir = Vector2Int.zero;
        SquareData dataAtTest = SquareData.NULL;
        int checkDistance = side;
        bool done = false;
        while (!done) {
            checkDistance++;
            for (int i = 1; i <= frontFree; i++) {
                testDir = position + (checkDistance*factor) * rightDir + i * heading;
                if ((testDir.x < 0) || (testDir.y < 0) || (testDir.x >= sizeX) || (testDir.y >= sizeY)) {
                    side = checkDistance-1;
                    done = true;
                    break;
                } else
                    dataAtTest = map.GetMapData(testDir.x, testDir.y);
                if (map.CrawlersInTunnels && map.CrawlersInAnterooms) {
                    if ((dataAtTest != SquareData.OPEN) && (dataAtTest != SquareData.NJ_OPEN) && (dataAtTest != SquareData.G_OPEN) && (dataAtTest != SquareData.IT_OPEN) && (dataAtTest != SquareData.IA_OPEN) && (dataAtTest != SquareData.NJ_G_OPEN)) {
                        side = checkDistance-1;
                        done = true;
                        break;
                    }
                } else if (map.CrawlersInTunnels) {
                    if ((dataAtTest != SquareData.OPEN) && (dataAtTest != SquareData.NJ_OPEN) && (dataAtTest != SquareData.G_OPEN) && (dataAtTest != SquareData.IT_OPEN) && (dataAtTest != SquareData.NJ_G_OPEN)) {
                        side = checkDistance-1;
                        done = true;
                        break;
                    }
                } else if ((dataAtTest != SquareData.OPEN) && (dataAtTest != SquareData.NJ_OPEN) && (dataAtTest != SquareData.G_OPEN) && (dataAtTest != SquareData.NJ_G_OPEN)) {
                        side = checkDistance-1;
                        done = true;
                        break;
                }
            }
        }
    }

    public int FrontFree(Vector2Int position, Vector2Int heading, ref int leftFree, ref int rightFree) {
        Debug.Assert((leftFree >= 1) && (rightFree >= 1));      
        int sizeX = map.SizeX;
        int sizeY = map.SizeY;
        Debug.Assert((position.x >= 0) && (position.y >= 0) && (position.x < sizeX) && (position.y < sizeY));      
        int frontFree = -1;
        Vector2Int rightDir = Vector2Int.zero;
        Vector2Int testDir;
        if (heading.x == 0)
            rightDir = new Vector2Int(heading.y, 0);
        else if (heading.y == 0)
            rightDir = new Vector2Int(0, -heading.x);
        else
            Debug.Assert(false);
        int checkDistance = 0;
        SquareData dataAtTest = SquareData.NULL;
        while (frontFree == -1) {
            checkDistance++;
            for (int i = -leftFree; i <= rightFree; ++i) {
                testDir = position + i * rightDir + checkDistance * heading;
                if ((testDir.x < 0) || (testDir.y < 0) || (testDir.x >= sizeX) || (testDir.y >= sizeY)) {
                    frontFree = checkDistance - 1;
                    break;
                } else 
                    dataAtTest = map.GetMapData(testDir.x, testDir.y);
                if (map.CrawlersInTunnels && map.CrawlersInAnterooms) {
                    if (((i == 0) &&  (dataAtTest != SquareData.OPEN) && (dataAtTest != SquareData.NJ_OPEN) && (dataAtTest != SquareData.IT_OPEN) && (dataAtTest != SquareData.IA_OPEN))   //we are blocked
		                           || ((dataAtTest != SquareData.OPEN) && (dataAtTest != SquareData.NJ_OPEN) && (dataAtTest != SquareData.IT_OPEN) && (dataAtTest != SquareData.IA_OPEN) && (dataAtTest != SquareData.G_OPEN) && (dataAtTest != SquareData.NJ_G_OPEN))) {
                        frontFree = checkDistance-1;
                        break;
                    }
                } else if (map.CrawlersInTunnels) {
                    if (((i == 0) &&  (dataAtTest != SquareData.OPEN) && (dataAtTest != SquareData.NJ_OPEN) && (dataAtTest != SquareData.IT_OPEN))   //we are blocked
		                          || ((dataAtTest != SquareData.OPEN) && (dataAtTest != SquareData.NJ_OPEN) && (dataAtTest != SquareData.IT_OPEN) && (dataAtTest != SquareData.G_OPEN) && (dataAtTest != SquareData.NJ_G_OPEN))) {
                        frontFree = checkDistance-1;
                        break;
                    }
                } else {
                    if (((i == 0) &&  (dataAtTest != SquareData.OPEN) && (dataAtTest != SquareData.NJ_OPEN))   //we are blocked
		                          || ((dataAtTest != SquareData.OPEN) && (dataAtTest != SquareData.NJ_OPEN) && (dataAtTest != SquareData.G_OPEN) && (dataAtTest != SquareData.NJ_G_OPEN))) {
                        frontFree = checkDistance-1;
                        break;
                    }
                }
            }
        }
        Debug.Assert(frontFree >= 0);

        if (frontFree > 0) {
           CheckSides(position, heading, rightDir, ref leftFree, frontFree, sizeX, sizeY, -1);
           CheckSides(position, heading, rightDir, ref rightFree, frontFree, sizeX, sizeY, 1);
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

        int leftFree = corridorWidth;
        int rightFree = corridorWidth;
        int frontFree = FrontFree(location, forward, ref leftFree, ref rightFree);
        
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

        if (opening == 0 && frontFree < map.JoinDistance) {
            if (Join(frontFree))
                return false;

        }

        int tilesLaid = stepLength;

        if (frontFree > corridorWidth) {
            if (frontFree - corridorWidth < stepLength)
                tilesLaid = frontFree - corridorWidth;
            for (int i = 1; i <= tilesLaid; i++) {
                test = location + i * forward;
                if (opening == 1)  {
                    map.SetMapData(test, SquareData.CLOSED);
                } else {
                    Debug.Assert(opening == 0);
                    map.SetMapData(test, SquareData.NJ_CLOSED);
                }                         
            }
            location = test;

            int diceRoll = DungeonGeneratorCTR.random.Next(101);
            int babyGeneration = generation + 1;
            int summedProbs = 0;

            for (int ind = 0; ind <= 10; ind++) {
                summedProbs = summedProbs + map.GetBabyDelayProbsForGenerationC(ind);
                if (diceRoll < summedProbs) {
                    babyGeneration = generation + ind;
                    break;
                }
            }
            
            int sSSP = map.Mutate(straightSingleSpawnProb);
            int sDSP = map.Mutate(straightDoubleSpawnProb);
            int tSSP = map.Mutate(turnSingleSpawnProb);
            int tDSP = map.Mutate(turnDoubleSpawnProb);
            int cDP = map.Mutate(changeDirProb);

            if (DungeonGeneratorCTR.random.Next(10) < changeDirProb) {
                Vector2Int oldForward = new Vector2Int(forward.x, forward.y);
                if ((desiredDir == Vector2Int.zero) || (desiredDir == forward)) {
                    int rand = DungeonGeneratorCTR.random.Next(4);
                    if (rand == 0)
                        forward = right;
                    else if (rand == 1)
                        forward = left;
                    else {
                        if ((rightFree > leftFree) || (rightFree == leftFree) && (DungeonGeneratorCTR.random.Next(2) == 0))
                            forward = right;
                        else
                            forward = left;
                    }      
                } else {
                    if ((desiredDir.x == 0) || (desiredDir.y == 0))
                        forward = desiredDir;
                    else {
                        Debug.Assert(desiredDir != Vector2Int.zero);
                        forward = desiredDir - forward;
                    }   
                }

                if (DungeonGeneratorCTR.random.Next(101) < turnDoubleSpawnProb) {
                    Vector2Int newBornDir = new Vector2Int(-forward.x, -forward.y);
                    
                    if (DungeonGeneratorCTR.random.Next(101) < map.NoHeadProb)
                        newBornDir = Vector2Int.zero;
                    map.CreateCrawler(location, -forward, 0, map.GetMaxAgeC(babyGeneration), babyGeneration, newBornDir, map.GetStepLength(babyGeneration), 1, map.GetCorrWidth(babyGeneration), sSSP, sDSP, tSSP, tDSP, cDP);
                    
                    newBornDir = oldForward;
                    if (DungeonGeneratorCTR.random.Next(101) < map.NoHeadProb)
                        newBornDir = Vector2Int.zero;
                    map.CreateCrawler(location, oldForward, 0, map.GetMaxAgeC(babyGeneration), babyGeneration, newBornDir, map.GetStepLength(babyGeneration), 1, map.GetCorrWidth(babyGeneration), sSSP, sDSP, tSSP, tDSP, cDP);
                } else if (DungeonGeneratorCTR.random.Next(101) < turnSingleSpawnProb) {
                    Vector2Int newBornDir = new Vector2Int(-forward.x, -forward.y);
                    
                    if (DungeonGeneratorCTR.random.Next(101) < map.NoHeadProb)
                        newBornDir = Vector2Int.zero;
                    map.CreateCrawler(location, -forward, 0, map.GetMaxAgeC(babyGeneration), babyGeneration, newBornDir, map.GetStepLength(babyGeneration), 1, map.GetCorrWidth(babyGeneration), sSSP, sDSP, tSSP, tDSP, cDP);
                } 
            } else {
                if (DungeonGeneratorCTR.random.Next(101) < straightDoubleSpawnProb) {
                    Vector2Int newBornDir = new Vector2Int(right.x, right.y);

                    if (DungeonGeneratorCTR.random.Next(101) < map.NoHeadProb)
                        newBornDir = Vector2Int.zero;
                    map.CreateCrawler(location, right, 0, map.GetMaxAgeC(babyGeneration), babyGeneration, newBornDir, map.GetStepLength(babyGeneration), 1, map.GetCorrWidth(babyGeneration), sSSP, sDSP, tSSP, tDSP, cDP);

                    newBornDir = left;
                    if (DungeonGeneratorCTR.random.Next(101) < map.NoHeadProb)
                        newBornDir = Vector2Int.zero;
                    map.CreateCrawler(location, left, 0, map.GetMaxAgeC(babyGeneration), babyGeneration, newBornDir, map.GetStepLength(babyGeneration), 1, map.GetCorrWidth(babyGeneration), sSSP, sDSP, tSSP, tDSP, cDP);
                } else if (DungeonGeneratorCTR.random.Next(101) < straightSingleSpawnProb) {
                    if ((leftFree > rightFree) || (leftFree == rightFree && DungeonGeneratorCTR.random.Next(2) == 0)) 
                        test = left;
                    else
                        test = right;
                    if (DungeonGeneratorCTR.random.Next(3) == 0)
                        test = -test;
                    Vector2Int newBornDir = new Vector2Int(test.x, test.y);

                    if (DungeonGeneratorCTR.random.Next(101) < map.NoHeadProb)
                        newBornDir = Vector2Int.zero;
                    map.CreateCrawler(location, test, 0, map.GetMaxAgeC(babyGeneration), babyGeneration, newBornDir, map.GetStepLength(babyGeneration), 1, map.GetCorrWidth(babyGeneration), sSSP, sDSP, tSSP, tDSP, cDP);
                }
            } 
        } else {
            int cW1 = corridorWidth;
            int cW2 = corridorWidth;

            if (forward == desiredDir || desiredDir == Vector2Int.zero) {
                int rFree = -1;
                int lFree = -1;
                rFree = FrontFree(location, right, ref cW1, ref cW2);
                cW1 = corridorWidth;
                cW2 = corridorWidth;
                lFree = FrontFree(location, left, ref cW1, ref cW2);

                if((rFree <= corridorWidth) && (lFree <= corridorWidth)) {
                    return false;
                } else if ((rFree > 2*corridorWidth+1) && (lFree >= 2*corridorWidth+1)) {
                    if (DungeonGeneratorCTR.random.Next(2) == 0)
                        forward = right;
                    else
                        forward = left;
                } else if (rFree > lFree)
                    forward = right;
                  else if (lFree > rFree)
                    forward = left;
                  else if (DungeonGeneratorCTR.random.Next(2) == 0)
                    forward = right;
                  else
                    forward = left;
            } else {
                if (desiredDir.x == 0 || desiredDir.y == 0) {
                    cW1 = corridorWidth;
                    cW2 = corridorWidth;
                    int dirFree = FrontFree (location, desiredDir, ref cW1, ref cW2);
                    if (dirFree > corridorWidth)
                        forward = desiredDir;
                    else 
                        return false;
                } else {
                    Debug.Assert(desiredDir != Vector2Int.zero);
                    test = desiredDir - forward;
                    cW1 = corridorWidth;
                    cW2 = corridorWidth;
                    int testFree = FrontFree(location, test, ref cW1, ref cW2);
                    if (testFree > corridorWidth)
                        forward = test;
                    else 
                        return false;
                }
            }
        } 
        return true;
    }

     private bool Join(int frontFree) {
        Vector2Int right = Vector2Int.zero;
        Vector2Int test = Vector2Int.zero;

        if (forward.x == 0) 
             right = new Vector2Int(forward.y, 0);
        else if (forward.y == 0)
             right = new Vector2Int(0, -forward.x);
        else
            Debug.Assert(false);

        test = location + (frontFree+1) * forward;
        if (test.x < 0 || test.y < 0 || test.x > map.SizeX || test.y > map.SizeY)
            return false;

        SquareData squareType = map.GetMapData(test);
        if (squareType == SquareData.CLOSED || squareType == SquareData.G_CLOSED) {
            for (int i = 1; i <= frontFree; i++) {
                test = location + i * forward;
                if (test.x < 0 || test.y < 0 || test.x > map.SizeX || test.y > map.SizeY)
                    return false;
                map.SetMapData(test, SquareData.NJ_CLOSED);
            }
            return true;
        } else if (squareType == SquareData.NJ_CLOSED || squareType == SquareData.NJ_G_CLOSED)
            return false;

        Vector2Int wall = Vector2Int.zero;
        int sideStep = 0;

        for (int i = 0; i <= corridorWidth; ++i) {

            test = location + (i*right) + (frontFree+1)*forward;
            if (test.x < 0 || test.y < 0 || test.x > map.SizeX || test.y > map.SizeY)
                return false;
            squareType = map.GetMapData(test);
            if (squareType == SquareData.CLOSED || squareType == SquareData.G_CLOSED || squareType == SquareData.NJ_CLOSED || squareType == SquareData.NJ_G_CLOSED) {
                wall = test;
                sideStep = i;
                break;
            }

            test = location - (i*right) + (frontFree+1)*forward;
            if (test.x < 0 || test.y < 0 || test.x > map.SizeX || test.y > map.SizeY)
                return false;
            squareType = map.GetMapData(test);
            if (squareType == SquareData.CLOSED || squareType == SquareData.G_CLOSED || squareType == SquareData.NJ_CLOSED || squareType == SquareData.NJ_G_CLOSED) {
                wall = test;
                sideStep = -i;
                break;
            }    
        }
        /*
        if (wall.x != 0 || wall.y != 0) 
            return false;
        if (sideStep != 0)
            return false;*/
        if (squareType == SquareData.NJ_CLOSED || squareType == SquareData.NJ_G_CLOSED)
            return false;
        
        int absSidestep = Mathf.Abs(sideStep);
        int factorSidestep;

        if (sideStep < 0) {
            test = right;
            factorSidestep = -1;
        }
        else {
            test = -right;
            factorSidestep = 1;
        }

        int leftF = 1;
        int rightF = 1;
        int free = FrontFree(wall, test, ref leftF, ref rightF);

        if (free < absSidestep + 1)
            return false;

        for (int i = 1; i <= frontFree+1; ++i) {
            test = location + i*forward;
            if (test.x < 0 || test.y < 0 || test.x > map.SizeX || test.y > map.SizeY)
                return false;
            map.SetMapData(test, SquareData.NJ_CLOSED);
        }

        for (int i = 1; i < absSidestep; ++i) {
            test = location + (i*factorSidestep)*right + (frontFree+1)*forward;
            if (test.x < 0 || test.y < 0 || test.x > map.SizeX || test.y > map.SizeY)
                return false;
            map.SetMapData(test, SquareData.NJ_CLOSED);
        }        
        return true;
    }
    
}
