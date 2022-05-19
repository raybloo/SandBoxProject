using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterCollisionManager : MonoBehaviour
{
    //public float radius = 3f; // compute collisiont with the colliders located inside a sphere of this radius
    //public float height
    
    public float tolerance = 0.001f;
    
    public bool collided = false;
    public bool bumped = false;
    //public float collisionAngle = 0f;
    public Vector3 bumpVector = Vector3.zero;

    public CapsuleCollider thisCollider;
    private Collider[] neighbours;
    private Vector3 start;
    private Vector3 end;

    //Displacement
    
    public readonly float maxStepDistance = 1f; // discrete collision step length
    public readonly int maxTestCount = 30; // maximum amount of vector partition if it exceeds the maxStepDistance
    public readonly int maxNeighbours = 6; // maximum amount of neighbours used to compute collision
    public readonly int maxCollisionCount = 8; // maximum amount of collision taken into account (used in MoveS)
    public readonly int maxRollbackDepth = 5; // maximum amount of step used to capture the smallest distance (used in MoveRB)

    private Vector3 currentMovement = Vector3.zero;
    private Vector3 probedMovement = Vector3.zero;
    private Vector3 correction = Vector3.zero;
    private Vector3[] dePenetrationVectors;
    private int dePenVectorCount;
    private Vector3 otherPosition = Vector3.zero;
    private Quaternion otherRotation = Quaternion.identity;

    //Globals to avoid reinstancing
    private Collider collider;
    private Vector3 combinedDepen;
    private Vector3 combinedBump;

    //Rotation
    public int maxStepAngle = 15;
    private Quaternion currentRotation = Quaternion.identity;
    private Quaternion probedRotation = Quaternion.identity;

    //Debug
    public GameObject sphere1;
    public GameObject sphere2;

    public GameObject VectShow;
    public GameObject VectShowRed;

    public struct Plane
    {
        public float a;
        public float b;
        public float c;
        public float d;

        public Plane(in Vector3 normal, in Vector3 point) {
            a = normal.x;
            b = normal.y;
            c = normal.z;
            d = -(a * point.x) - (b * point.y) - (c * point.z);
        }

        public Plane(in Vector3 vec1, in Vector3 vec2, in Vector3 point) {
            Vector3 normal = Vector3.Cross(vec1, vec2);
            a = normal.x;
            b = normal.y;
            c = normal.z;
            d = -(a * point.x) - (b * point.y) - (c * point.z);
        }

        public Vector3 Intersect(in Plane alpha, in Plane beta) { // this kind of solves a linear equation
            float[,] matrix = { { this.a, this.b, this.c, 1f, 0f, 0f }, { alpha.a, alpha.b, alpha.c, 0f, 1f, 0f }, { beta.a, beta.b, beta.c, 0f, 0f, 1f } };
            /*Debug.Log("( " + matrix[0, 0] + " , " + matrix[0, 1] + " , " + matrix[0, 2] + " )\n" +
                "( " + matrix[1, 0] + " , " + matrix[1, 1] + " , " + matrix[1, 2] + " )\n" +
                "( " + matrix[2, 0] + " , " + matrix[2, 1] + " , " + matrix[2, 2] + " )"); */
            for (int k = 0; k < 3; k++) {
                if (matrix[k, k] == 0f) {
                    for (int j = k + 1; j < 3; j++) { //if pivot is 0, add another row
                        if(matrix[j, k] != 0f) {
                            for (int i = k; i < 6; i++) {
                                matrix[k, i] = matrix[k, i] + matrix[j, i];
                            }
                            break;
                        } else if (j == 2) {
                            Debug.LogError("Non invertible matrix");
                            return new Vector3(float.NaN, float.NaN, float.NaN);
                        }     
                    }
                }
                for (int i = k + 1; i < 6; i++) {
                    matrix[k, i] = matrix[k, i] / matrix[k, k]; // pivot line
                    for (int j = 0; j < 3; j++) {
                        if(j != k) { // all lines exept pivot line
                            if (matrix[j, k] != 0f) {
                                matrix[j, i] = matrix[j, i] - (matrix[j, k] * matrix[k, i]);
                            }
                        }
                    }
                }
                //not necessary for this case
                /*for (int j = 0; j < 3; j++) {
                    matrix[j, k] = 0f;
                }
                matrix[k, k] = 1f;*/
            }
            /*Debug.Log("( " + matrix[0, 3] + " , " + matrix[0, 4] + " , " + matrix[0, 5] + " )\n" +
                "( " + matrix[1, 3] + " , " + matrix[1, 4] + " , " + matrix[1, 5] + " )\n" +
                "( " + matrix[2, 3] + " , " + matrix[2, 4] + " , " + matrix[2, 5] + " )");*/
            return new Vector3((matrix[0, 3] * -this.d) + (matrix[0, 4] * -alpha.d) + (matrix[0, 5] * -beta.d),
                (matrix[1, 3] * -this.d) + (matrix[1, 4] * -alpha.d) + (matrix[1, 5] * -beta.d), 
                (matrix[2, 3] * -this.d) + (matrix[2, 4] * -alpha.d) + (matrix[2, 5] * -beta.d));
        }
    }

    void Start() {
        Physics.autoSimulation = false;
        if(!thisCollider) {
            thisCollider = GetComponent<CapsuleCollider>();
        }
        neighbours = new Collider[maxNeighbours];
        dePenetrationVectors = new Vector3[maxCollisionCount];
        dePenVectorCount = 0;
        Plane alpha = new Plane(Vector3.up, Vector3.zero);
        Plane beta = new Plane(Vector3.forward, new Vector3(31f, 35f, 18f));
        Plane gamma = new Plane(new Vector3(1f,3f,0f), new Vector3(0f,3f,0f));

        //Debug.Log(alpha.Intersect(beta, gamma));

        //probedRotations = new Quaternion[180/maxStepAngle];
    }

    //Move full combining depen vectors
    public bool MoveFC(in Vector3 movement, out Vector3 corrected) {
        collided = false;
        if(bumped) {
            bumped = false;
            bumpVector.x = 0f;
            bumpVector.y = 0f;
            bumpVector.z = 0f;
        }

        if (!thisCollider) {
            corrected = movement;
            return true; // nothing to do without a Collider attached
        }

        float magnitude = movement.magnitude;
        float tests = maxTestCount;
        for (float i = 1f; i < maxTestCount; i++) {
            if (!(magnitude > i * maxStepDistance)) {
                tests = i;
                break;
            }
        }

        //Debug.Log("tests " + tests);

        probedMovement = movement * (1f / tests);
        currentMovement = Vector3.zero;
        for (float i = 0f; i < tests; i++) {
            currentMovement += probedMovement;
            if(!StepMoveFC(ref currentMovement)) {
                corrected = currentMovement;
                return false;
            }
        }

        corrected = currentMovement;
        return true;
    }

    public bool StepMoveFC(ref Vector3 movement) {
        if (!thisCollider)
            return true; // nothing to do without a Collider attached
        //Debug.Log("collision start");
        float radius = thisCollider.radius;
        float height = thisCollider.height - (2 * radius);

        start = thisCollider.transform.rotation * (thisCollider.center + (Vector3.down * height / 2)) + thisCollider.transform.position;
        end = thisCollider.transform.rotation * (thisCollider.center + (Vector3.up * height / 2)) + thisCollider.transform.position;

        int count;
        correction = Vector3.zero;
        dePenVectorCount = 0;

        for (int collisionCount = 0; collisionCount < maxCollisionCount; collisionCount++) {

            //Intersect collider with neighbours
            count = Physics.OverlapCapsuleNonAlloc(start + movement + correction, end + movement + correction, radius - tolerance, neighbours);
            //sphere1.transform.position = start + movement + correction;
            //sphere2.transform.position = end + movement + correction;
            count = count > maxNeighbours ? maxNeighbours : count;

            if (count > 0) {
                //Iter over all intersecting neighbours
                combinedDepen = Vector3.zero;
                combinedBump = Vector3.zero;

                for (int i = 0; i < count; ++i) {
                    collider = neighbours[i];
                    if (collider != thisCollider && !collider.isTrigger) {
                        // Compute depen vector
                        otherPosition = collider.gameObject.transform.position;
                        otherRotation = collider.gameObject.transform.rotation;
                        bool overlapped = Physics.ComputePenetration(
                            thisCollider, thisCollider.transform.position + movement + correction, thisCollider.transform.rotation,
                            collider, otherPosition, otherRotation,
                            out Vector3 direction, out float distance
                        );

                        if (overlapped) {  
                            if (combinedDepen == Vector3.zero) {
                                combinedDepen = direction * distance;
                            } else {
                                CombineDepenVectors(ref combinedDepen, direction * distance);
                            }

                            if(collider.CompareTag("Obstacle")) {
                                combinedBump += direction;
                                bumped = true;
                            }
                            
                            if(float.IsNaN(combinedDepen.x) || float.IsNaN(combinedDepen.y) || float.IsNaN(combinedDepen.z)) {
                                //Debug.LogWarning("Uncombinable collision vectors");
                                return false;
                            }
                            //Debug.Log("Collision with " + collider.gameObject.name);
                            //Instantiate(VectShow, thisCollider.transform.position, Quaternion.FromToRotation(Vector3.up, direction * distance));
                            //Instantiate(VectShowRed, thisCollider.transform.position, Quaternion.FromToRotation(Vector3.up, combinedDepen));
                            //Debug.LogWarning(combinedDepen);
                        }
                    }
                }
                //if (combinedDepen != Vector3.zero) {
                //    CombineDepenVectors(ref correction, combinedDepen);
                //}
                correction += combinedDepen;
                bumpVector += combinedBump;
                if (combinedDepen == Vector3.zero) {
                    movement += correction; // No more colliding objects, corrected movement applied
                    return true;
                }

            } else {
                movement += correction; // No more colliding objects, corrected movement applied
                return true;
            }
        }

        //Collision has failed, either movement is ignored or collision is ignored
        count = Physics.OverlapCapsuleNonAlloc(start, end, radius - tolerance, neighbours);
        if (count > 0) {
            var collider = neighbours[0];
            for (int i = 1; i < count; ++i) {
                if (collider == thisCollider || collider.isTrigger) {
                    collider = neighbours[i];
                } else {
                    break;
                }
            }
            if (collider == thisCollider || collider.isTrigger) {
                movement = Vector3.zero; // Stay in place
                return false;
            } else {
                collided = false;
                return false; // Move disregarding collision
            }
        } else {
            movement = Vector3.zero; // Stay in place
            return false;
        }
    }

    public bool MoveS(in Vector3 movement, out Vector3 corrected) {
        
        collided = false;
        if (!thisCollider) {
            corrected = movement;
            return true; // nothing to do without a Collider attached
        }

        float magnitude = movement.magnitude;
        float tests = maxTestCount;
        for (float i = 1f; i < maxTestCount; i++) {
            if (!(magnitude > i * maxStepDistance)) {
                tests = i;
                break;
            }
        }

        probedMovement = movement * (1f / tests);
        currentMovement = Vector3.zero;
        for (float i = 0f; i < tests; i++) {
            currentMovement += probedMovement;
            if(!StepMoveS(ref currentMovement)) {
                corrected = currentMovement;
                return false;
            }
        }
        corrected = currentMovement;
        //Debug.Log("moveS correction" + corrected + " tests :" + tests);
        return true;
    }
    
    //Check the movement and correction slides along colliders
    public bool StepMoveS(ref Vector3 movement) {

        collided = false; 
        if (!thisCollider)
            return true; // nothing to do without a Collider attached
        //Debug.Log("collision start");
        float radius = thisCollider.radius;
        float height = thisCollider.height - (2 * radius);

        start = thisCollider.transform.rotation * (thisCollider.center + (Vector3.down * height / 2)) + thisCollider.transform.position;
        end = thisCollider.transform.rotation * (thisCollider.center + (Vector3.up * height / 2)) + thisCollider.transform.position;

        int count;
        correction = Vector3.zero;
        dePenVectorCount = 0;

        for (int collisionCount = 0; collisionCount < maxCollisionCount; collisionCount++) {

            //Intersect collider with neighbours
            count = Physics.OverlapCapsuleNonAlloc(start + movement + correction, end + movement + correction, radius - tolerance, neighbours); 

            if(count > 0) {

                //Choose an intersecting neighbour (not this collider, not a trigger)
                var collider = neighbours[0];
                for (int i = 1; i < count; ++i) {
                    if (collider == thisCollider || collider.isTrigger) {
                        collider = neighbours[i];
                    } else {
                        break;
                    }
                }

                if (collider != thisCollider && !collider.isTrigger) {
                    // Compute depen vector
                    otherPosition = collider.gameObject.transform.position;
                    otherRotation = collider.gameObject.transform.rotation;
                    bool overlapped = Physics.ComputePenetration(
                        thisCollider, thisCollider.transform.position + movement + correction, thisCollider.transform.rotation,
                        collider, otherPosition, otherRotation,
                        out Vector3 direction, out float distance
                    );

                    // Combine depen vectors
                    if (overlapped) {
                        collided = true;

                        // Valid correction
                        if (AddDepenDirection(in direction)) {
                            //Debug.Log("Distance Added: "+distance);
                            correction += direction * distance;
                        }

                        // Invalid correction, decide whether to disregard collision or stay in place
                        else {
                            count = Physics.OverlapCapsuleNonAlloc(start, end, radius - tolerance, neighbours);
                            if (count > 0) {
                                collider = neighbours[0];
                                for (int i = 1; i < count; ++i) {
                                    if (collider == thisCollider || collider.isTrigger) {
                                        collider = neighbours[i];
                                    } else {
                                        break;
                                    }
                                }
                                if (collider == thisCollider || collider.isTrigger) {
                                    movement = Vector3.zero; // Stay in place
                                    return false;
                                } else {
                                    collided = false;
                                    //hardCollided = false;
                                    return false; // Move disregarding collision
                                }
                            } else {
                                movement = Vector3.zero; // Stay in place
                                return false;
                            }
                        }
                    }
                } else {
                    //hardCollided = Vector3.Angle(movement, correction) > hardCollisionAngle;
                    movement += correction; // No more colliding objects, corrected movement applied
                    return true;
                }
            } else {
                //hardCollided = Vector3.Angle(movement, correction) > hardCollisionAngle;
                movement += correction; // No more colliding objects, corrected movement applied
                return true;
            }
        }

        
        /*Debug.Log("Collision fail print(" + correction.x + ", " + correction.y + ", " + correction.z + ") count: "+dePenVectorCount);
        foreach (Vector3 vectr in dePenetrationVectors) {
            Debug.Log("Depens: ("+vectr.x+", "+vectr.y+", "+vectr.z+")");
        }*/

        // Scaling attempt
        if (correction != Vector3.zero) {
            //Debug.Log("Scaling attempt");
            for (int k = 0; k < 4; k++) {
                correction *= 1.2f;
                count = Physics.OverlapCapsuleNonAlloc(start + movement + correction, end + movement + correction, radius - tolerance, neighbours);

                if (count > 0) {
                    //Choose an intersecting neighbour (not this collider, not a trigger)
                    var collider = neighbours[0];
                    for (int i = 1; i < count; ++i) {
                        if (collider == thisCollider || collider.isTrigger) {
                            collider = neighbours[i];
                        } else {
                            break;
                        }
                    }

                    if (collider == thisCollider || collider.isTrigger) {
                        movement += correction; // No more colliding objects, corrected movement applied
                        return true;
                    }
                }
                else {
                    movement += correction; // No more colliding objects, corrected movement applied
                    return true;
                }
            }
        }

        //Instantiate(VectShowRed, thisCollider.transform.position, Quaternion.FromToRotation(Vector3.up, correction));
        //Debug.Log("Trajectory collision failed");


        //Collision failed, decide to move or not
        count = Physics.OverlapCapsuleNonAlloc(start, end, radius - tolerance, neighbours);
        if(count > 0) {
            var collider = neighbours[0];
            for (int i = 1; i < count; ++i) {
                if (collider == thisCollider || collider.isTrigger) {
                    collider = neighbours[i];
                } else {
                    break;
                }
            }
            if (collider == thisCollider || collider.isTrigger) {
                movement = Vector3.zero; // Stay in place
                return false;
            }
            else {
                collided = false;
                return false; // Move disregarding collision
            }
        }
        else {
            movement = Vector3.zero; // Stay in place
            return false;
        }
    }

    public Quaternion Rotate(in Quaternion rotation) {
        collided = false;
        if (!thisCollider) {
            return rotation; // Nothing to do without a Collider attached
        }

        float angle = Quaternion.Angle(Quaternion.identity, rotation);
        float tests = 180 / maxStepAngle;
        for(int i = 1; i < 180 / maxStepAngle; i++) {
            if (!(angle > i * maxStepAngle)) {
                tests = i;
                break;
            }
        }

        currentRotation = thisCollider.transform.localRotation;
        for(float i = 0; i < tests; i++) {
            probedRotation = Quaternion.Lerp(thisCollider.transform.localRotation, rotation, (i + 1f)/tests);
            if(!CheckRotation(probedRotation)) {
 
                collided = true;
                return currentRotation;
            }
            currentRotation = probedRotation;
        }
        return rotation;
    }
    
    public bool CheckRotation(in Quaternion rotation) {
        if (!thisCollider)
            return true; // Nothing to do without a Collider attached

        float radius = thisCollider.radius;
        float height = thisCollider.height - (2 * radius);

        start = rotation * (thisCollider.center + (Vector3.down * height / 2)) + thisCollider.transform.position;
        end = rotation * (thisCollider.center + (Vector3.up * height / 2)) + thisCollider.transform.position;

        //sphere1.transform.position = start;
        //sphere2.transform.position = end;
        int count = Physics.OverlapCapsuleNonAlloc(start, end, radius - tolerance, neighbours);
        if (count > 0) {
            var collider = neighbours[0];
            for (int i = 1; i < count; ++i) {
                if (collider == thisCollider || collider.isTrigger) {
                    collider = neighbours[i];
                } else {
                    break;
                }
            }
            if (collider == thisCollider || collider.isTrigger) {
                return true; // No colliding objects, rotation valid
            } else {
                return false; 
            }
        } else {
            return true; // No colliding objects, rotation valid
        }
    }

    private bool AddDepenDirection(in Vector3 vec) {
        for (int i = 0; i < dePenVectorCount; i++) {
            if (Vector3.Dot(dePenetrationVectors[i], vec) < -0.1) {
                dePenetrationVectors[dePenVectorCount] = vec;
                dePenVectorCount++;
                Debug.Log("Invalid Correction");
                Instantiate(VectShow, thisCollider.transform.position, Quaternion.FromToRotation(Vector3.up, vec));
                //Debug.Log("Invalid Correction print(" + vec.x + ", " + vec.y + ", " + vec.z + ") count: "+dePenVectorCount);
                //foreach (Vector3 vectr in dePenetrationVectors) {
                //    Debug.Log("Invalid Correction: ("+vectr.x+", "+vectr.y+", "+vectr.z+")");
                //}
                return false;
            }
        }
        dePenetrationVectors[dePenVectorCount] = vec;
        dePenVectorCount++;
        return true;
    }

    private void CombineDepenVectors(ref Vector3 a, in Vector3 b) {
        Vector3 diffab = a - b;
        float conditionA = Vector3.Dot(a, diffab);
        if (conditionA < 0) {
            a = b;
            return;
        }
        float conditionB = Vector3.Dot(b, diffab);
        if (conditionB < 0) {
            return;
        }
        Plane orthogonal2A = new Plane(a, a);
        Plane orthogonal2B = new Plane(b, b);
        Plane abPlane = new Plane(a, b, Vector3.zero);

        a = abPlane.Intersect(orthogonal2A, orthogonal2B);
    }

    private Vector3 CombineDepenDirection() {
        Vector3 res = Vector3.zero;


        return res;
    }

    //Check movement and corrects the movement only along the direction of the movement
    public void MoveRB(in Vector3 movement, out Vector3 correction) {

        collided = false;
        if (!thisCollider) {
            correction = movement;
            return; // Nothing to do without a Collider attached
        }


        float radius = thisCollider.radius;
        float height = thisCollider.height - (2 * radius);
        start = thisCollider.transform.rotation * (thisCollider.center + (Vector3.down * height / 2)) + thisCollider.transform.position;
        end = thisCollider.transform.rotation * (thisCollider.center + (Vector3.up * height / 2)) + thisCollider.transform.position;

        float magnitude = movement.magnitude;
        float tests = maxTestCount;
        for (float i = 1f; i < maxTestCount; i++) {
            if (!(magnitude > i * maxStepDistance)) {
                tests = i;
                break;
            }
        }

        float res = 1.0f;
        float upperbound;
        float lowerbound;
        float current;
        int count;
        for (float i = 0f; i < tests && !collided; i++) {
            upperbound = (i + 1) / tests;
            lowerbound = i / tests;
            current = upperbound;
            for (int j = 0; j < maxRollbackDepth && lowerbound != upperbound; j++) {
                probedMovement = movement * current;
                count = Physics.OverlapCapsuleNonAlloc(start + probedMovement, end + probedMovement, radius - tolerance, neighbours);
                if(count > 0) {
                    var collider = neighbours[0];
                    for (int k = 1; k < count; ++k) {
                        if (collider == thisCollider || collider.isTrigger) {
                            collider = neighbours[k];
                        } else {
                            break;
                        }
                    }
                    if (collider == thisCollider || collider.isTrigger) {
                        // no collision
                        lowerbound = current;
                    } else {
                        // collision
                        upperbound = current;
                        collided = true;
                    }

                } else { // no collision
                    lowerbound = current;
                }
                current = ((upperbound - lowerbound) / 2f) + lowerbound;
            }
            res = lowerbound;
        }
        correction = movement * res;
    }
}
